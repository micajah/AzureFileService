using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;

namespace Micajah.AzureFileService
{
    public class BlobClient
    {
        #region Members

        private static SharedAccessBlobPolicy s_ReadAccessPolicy;
        private static SharedAccessBlobPolicy s_WriteAccessPolicy;

        private CloudBlobClient m_BlobClient;
        private CloudBlobContainer m_Container;
        private CloudBlobContainer m_TemporaryContainer;

        #endregion

        #region Private Properties

        private string BlobPath
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}/{1}/", this.ObjectType, this.ObjectId);
            }
        }

        private string BlobNameFormat
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}{{0}}", this.BlobPath);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the name of the container where the files are stored.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the type of the object which the files are associated with.
        /// </summary>
        public string ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the object which the files are associated with.
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the connection string to the storage.
        /// </summary>
        public string StorageConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the temporary container the files are uploaded to.
        /// </summary>
        public string TemporaryContainerName { get; set; }

        #endregion

        #region Internal Properties

        internal static SharedAccessBlobPolicy ReadAccessPolicy
        {
            get
            {
                if (s_ReadAccessPolicy == null)
                {
                    s_ReadAccessPolicy = new SharedAccessBlobPolicy
                    {
                        Permissions = SharedAccessBlobPermissions.Read,
                        SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(Settings.SharedAccessExpiryTime)
                    };
                }
                return s_ReadAccessPolicy;
            }
        }

        internal static SharedAccessBlobPolicy WriteAccessPolicy
        {
            get
            {
                if (s_WriteAccessPolicy == null)
                {
                    s_WriteAccessPolicy = new SharedAccessBlobPolicy
                    {
                        Permissions = SharedAccessBlobPermissions.Write,
                        SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(Settings.SharedAccessExpiryTime)
                    };
                }
                return s_WriteAccessPolicy;
            }
        }

        internal CloudBlobClient Client
        {
            get
            {
                if (m_BlobClient == null)
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.StorageConnectionString);
                    m_BlobClient = storageAccount.CreateCloudBlobClient();
                }
                return m_BlobClient;
            }
        }

        internal CloudBlobContainer Container
        {
            get
            {
                if (m_Container == null)
                {
                    m_Container = this.Client.GetContainerReference(this.ContainerName);
                    m_Container.CreateIfNotExists();
                }
                return m_Container;
            }
        }

        internal CloudBlobContainer TemporaryContainer
        {
            get
            {
                if (m_TemporaryContainer == null)
                {
                    m_TemporaryContainer = this.Client.GetContainerReference(this.TemporaryContainerName);
                    m_TemporaryContainer.CreateIfNotExists();
                }
                return m_TemporaryContainer;
            }
        }

        #endregion

        #region Internal Methods

        internal void FillFromContainer(DataTable table)
        {
            IEnumerable<IListBlobItem> blobList = this.Container.ListBlobs(this.BlobPath);
            foreach (IListBlobItem item in blobList)
            {
                CloudBlockBlob blob = item as CloudBlockBlob;
                if (blob != null)
                {
                    if (blob.BlobType == BlobType.BlockBlob)
                    {
                        string[] parts = blob.Name.Split('/');
                        string fileName = parts[parts.Length - 1];
                        string sas = blob.GetSharedAccessSignature(ReadAccessPolicy);
                        string uri = string.Format(CultureInfo.InvariantCulture, "{0}{1}", blob.Uri, sas);

                        table.Rows.Add(
                            blob.Name,
                            blob.Properties.ContentType,
                            fileName,
                            uri,
                            blob.Properties.Length,
                            blob.Properties.LastModified.Value.DateTime,
                            blob.Properties.LastModified.Value.Date
                        );
                    }
                }
            }
        }

        internal void UploadToTemporaryContainer(string blobName, string contentType, Stream source)
        {
            CloudBlockBlob blob = this.TemporaryContainer.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = contentType;
            blob.Properties.CacheControl = Settings.ClientCacheControl;
            blob.UploadFromStream(source);
        }

        internal void DeleteFromTemporaryContainer(string prefix)
        {
            IEnumerable<IListBlobItem> temporaryBlobList = this.TemporaryContainer.ListBlobs(prefix);
            foreach (IListBlobItem item in temporaryBlobList)
            {
                CloudBlockBlob tempBlob = item as CloudBlockBlob;
                tempBlob.Delete();
            }
        }

        internal void MoveFromTemporaryContainerToContainer(string prefix)
        {
            string blobNameFormat = this.BlobNameFormat;

            IEnumerable<IListBlobItem> temporaryBlobList = this.TemporaryContainer.ListBlobs(prefix);
            foreach (IListBlobItem item in temporaryBlobList)
            {
                CloudBlockBlob tempBlob = item as CloudBlockBlob;
                if (tempBlob.BlobType == BlobType.BlockBlob)
                {
                    string blobName = tempBlob.Name;
                    string[] parts = tempBlob.Name.Split('/');
                    int length = parts.Length;
                    if (length > 0)
                    {
                        blobName = string.Format(CultureInfo.InvariantCulture, blobNameFormat, parts[length - 1]);
                    }

                    CloudBlockBlob blob = this.Container.GetBlockBlobReference(blobName);
                    blob.StartCopyFromBlob(tempBlob);

                    tempBlob.Delete();
                }
            }
        }

        #endregion

        #region Public Methods

        public byte[] GetThumbnail(string fileName, int width, int height, int align)
        {
            byte[] bytes = null;

            string thumbBlobName = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}x{3}x{4}/{5}", this.ObjectType, this.ObjectId, width, height, align, fileName);
            CloudBlockBlob thumbBlob = this.Container.GetBlockBlobReference(thumbBlobName);

            if (thumbBlob.Exists())
            {
                long length = thumbBlob.Properties.Length;
                bytes = new byte[length];
                thumbBlob.DownloadToByteArray(bytes, 0);
            }
            else
            {
                string blobName = string.Format(CultureInfo.InvariantCulture, this.BlobNameFormat, fileName);
                CloudBlockBlob blob = this.Container.GetBlockBlobReference(blobName);

                if (blob.Exists())
                {
                    MemoryStream imageStream = null;
                    Stream thumbStream = null;

                    try
                    {
                        imageStream = new MemoryStream();
                        blob.DownloadToStream(imageStream);
                        imageStream.Position = 0;

                        thumbStream = new MemoryStream();
                        Thumbnail.Create(imageStream, width, height, align, thumbStream);

                        thumbBlob.Properties.ContentType = MimeType.Jpeg;
                        thumbBlob.Properties.CacheControl = Settings.ClientCacheControl;
                        thumbBlob.UploadFromStream(thumbStream);

                        thumbStream.Position = 0;

                        long length = thumbStream.Length;
                        BinaryReader reader = new BinaryReader(thumbStream);
                        bytes = reader.ReadBytes((int)length);
                    }
                    finally
                    {
                        if (imageStream != null)
                        {
                            imageStream.Dispose();
                        }

                        if (thumbStream != null)
                        {
                            thumbStream.Dispose();
                        }
                    }
                }
            }

            return bytes;
        }

        public void Delete(string blobName)
        {
            CloudBlockBlob blob = this.Container.GetBlockBlobReference(blobName);

            blob.Delete();
        }

        #endregion
    }
}
