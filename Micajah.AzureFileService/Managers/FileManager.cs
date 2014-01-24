﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;

namespace Micajah.AzureFileService
{
    public class FileManager
    {
        #region Members

        private string m_ContainerName;
        private string m_TemporaryContainerName;
        private string m_StorageConnectionString;
        private string m_ContainerSas;
        private SharedAccessBlobPolicy m_ReadAccessPolicy;
        private SharedAccessBlobPolicy m_WriteAccessPolicy;
        private SharedAccessBlobPolicy m_ReadWriteAccessPolicy;
        private CloudBlobClient m_ServiceClient;
        private CloudBlobContainer m_Container;
        private CloudBlobContainer m_TemporaryContainer;

        #endregion

        #region Constructors

        internal FileManager(string fileId, string containerUriWithSas)
        {
            if ((!string.IsNullOrEmpty(fileId)) && (!string.IsNullOrEmpty(containerUriWithSas)))
            {
                string[] parts = fileId.Split('/');
                int length = parts.Length;
                this.ObjectId = parts[length - 2];
                this.ObjectType = parts[length - 3];

                parts = containerUriWithSas.Split('?');
                Uri uri = new Uri(parts[0]);
                m_ContainerSas = parts[1];
                StorageCredentials credentials = new StorageCredentials(m_ContainerSas);
                m_Container = new CloudBlobContainer(uri, credentials);
                m_ContainerName = m_Container.Name;
            }
        }

        public FileManager()
        {

        }

        public FileManager(string containerName, string objectId, string objectType, string storageConnectionString)
        {
            this.ObjectId = objectId;
            this.ObjectType = objectType;

            m_ContainerName = containerName;
            m_StorageConnectionString = storageConnectionString;
        }

        public FileManager(string containerName, string objectId, string objectType, string storageConnectionString, string temporaryContainerName) :
            this(containerName, objectId, objectType, storageConnectionString)
        {
            m_TemporaryContainerName = temporaryContainerName;
        }

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
        public string ContainerName
        {
            get
            {
                return m_ContainerName;
            }
            set
            {
                m_ContainerName = value;
                m_Container = null;
                m_ContainerSas = null;
            }
        }

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
        public string StorageConnectionString
        {
            get
            {
                return m_StorageConnectionString;
            }
            set
            {
                m_StorageConnectionString = value;
                m_ServiceClient = null;
                m_Container = null;
                m_ContainerSas = null;
            }
        }

        /// <summary>
        /// Gets or sets the name of the temporary container the files are uploaded to.
        /// </summary>
        public string TemporaryContainerName
        {
            get
            {
                return m_TemporaryContainerName;
            }
            set
            {
                m_TemporaryContainerName = value;
                m_TemporaryContainer = null;
            }
        }

        #endregion

        #region Internal Properties

        internal SharedAccessBlobPolicy ReadAccessPolicy
        {
            get
            {
                if (m_ReadAccessPolicy == null)
                {
                    m_ReadAccessPolicy = new SharedAccessBlobPolicy
                    {
                        Permissions = SharedAccessBlobPermissions.Read,
                        SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(Settings.SharedAccessExpiryTime)
                    };
                }
                return m_ReadAccessPolicy;
            }
        }

        internal SharedAccessBlobPolicy WriteAccessPolicy
        {
            get
            {
                if (m_WriteAccessPolicy == null)
                {
                    m_WriteAccessPolicy = new SharedAccessBlobPolicy
                    {
                        Permissions = SharedAccessBlobPermissions.Write,
                        SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(Settings.SharedAccessExpiryTime)
                    };
                }
                return m_WriteAccessPolicy;
            }
        }

        internal SharedAccessBlobPolicy ReadWriteAccessPolicy
        {
            get
            {
                if (m_ReadWriteAccessPolicy == null)
                {
                    m_ReadWriteAccessPolicy = new SharedAccessBlobPolicy
                    {
                        Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write,
                        SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(Settings.SharedAccessExpiryTime)
                    };
                }
                return m_ReadWriteAccessPolicy;
            }
        }

        internal CloudBlobClient ServiceClient
        {
            get
            {
                if (m_ServiceClient == null)
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.StorageConnectionString);
                    m_ServiceClient = storageAccount.CreateCloudBlobClient();
                }
                return m_ServiceClient;
            }
        }

        internal CloudBlobContainer Container
        {
            get
            {
                if (m_Container == null)
                {
                    m_Container = this.ServiceClient.GetContainerReference(this.ContainerName);
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
                    m_TemporaryContainer = this.ServiceClient.GetContainerReference(this.TemporaryContainerName);
                    m_TemporaryContainer.CreateIfNotExists();
                }
                return m_TemporaryContainer;
            }
        }

        internal string ContainerSas
        {
            get
            {
                if (string.IsNullOrEmpty(m_ContainerSas))
                {
                    m_ContainerSas = this.Container.GetSharedAccessSignature(this.ReadWriteAccessPolicy);
                }
                return m_ContainerSas;
            }
        }

        #endregion

        #region Private Methods

        private static int CompareFilesByLastModifiedAndName(File x, File y)
        {
            int result = 0;

            if (x == null)
            {
                result = ((y == null) ? 0 : -1);
            }
            else
            {
                if (y == null)
                    result = 1;
                else
                {
                    result = x.LastModified.CompareTo(y.LastModified);
                    if (result == 0)
                    {
                        result = string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
                    }
                }
            }
            return result;
        }

        private File GetFileInfo(CloudBlockBlob blob)
        {
            BlobProperties props = blob.Properties;
            string fileName = GetNameFromFileId(blob.Name);
            string sas = blob.GetSharedAccessSignature(this.ReadAccessPolicy);
            string uri = string.Format(CultureInfo.InvariantCulture, "{0}{1}", blob.Uri, sas);

            return new File()
            {
                FileId = blob.Name,
                Name = fileName,
                Length = props.Length,
                ContentType = props.ContentType,
                FullName = blob.Name,
                Uri = uri,
                LastModified = props.LastModified.Value.DateTime
            };
        }

        private CloudBlockBlob GetFileReference(string fileName, string contentType)
        {
            string fileId = string.Format(CultureInfo.InvariantCulture, this.BlobNameFormat, fileName);

            CloudBlockBlob blob = this.Container.GetBlockBlobReference(fileId);
            blob.Properties.ContentType = contentType;
            blob.Properties.CacheControl = Settings.ClientCacheControl;

            return blob;
        }

        private CloudBlockBlob GetTemporaryFileReference(string fileName, string contentType, string directoryName)
        {
            string fileId = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", directoryName, fileName);

            CloudBlockBlob blob = this.TemporaryContainer.GetBlockBlobReference(fileId);
            blob.Properties.ContentType = contentType;
            blob.Properties.CacheControl = Settings.ClientCacheControl;

            return blob;
        }

        #endregion

        #region Internal Methods

        internal static string GetNameFromFileId(string fileId)
        {
            string[] parts = fileId.Split('/');
            string fileName = parts[parts.Length - 1];

            return fileName;
        }

        internal byte[] GetThumbnail(string fileId, string fileName, int width, int height, int align)
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
                CloudBlockBlob blob = this.Container.GetBlockBlobReference(fileId);

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

        #endregion

        #region Public Methods

        public File GetFileInfo(string fileId)
        {
            CloudBlockBlob blob = this.Container.GetBlockBlobReference(fileId);
            if (blob.Exists())
            {
                return GetFileInfo(blob);
            }

            return null;
        }

        public byte[] GetFile(string fileId)
        {
            byte[] bytes = null;

            CloudBlockBlob blob = this.Container.GetBlockBlobReference(fileId);
            if (blob.Exists())
            {
                long length = blob.Properties.Length;
                bytes = new byte[length];

                blob.DownloadToByteArray(bytes, 0);
            }

            return bytes;
        }

        public Collection<File> GetFiles()
        {
            List<File> files = new List<File>();

            IEnumerable<IListBlobItem> blobList = this.Container.ListBlobs(this.BlobPath);
            foreach (IListBlobItem item in blobList)
            {
                CloudBlockBlob blob = item as CloudBlockBlob;
                if (blob != null)
                {
                    if (blob.BlobType == BlobType.BlockBlob)
                    {
                        File file = GetFileInfo(blob);
                        files.Add(file);
                    }
                }
            }

            files.Sort(CompareFilesByLastModifiedAndName);

            return new Collection<File>(files);
        }

        public Collection<File> GetFiles(string[] extensions, bool negate)
        {
            List<File> files = new List<File>();

            List<string> extensionsList = null;
            if (extensions == null)
            {
                extensionsList = new List<string>();
            }
            else
            {
                extensionsList = new List<string>(extensions);
            }
            bool extensionsIsNotEmpty = (extensionsList.Count > 0);

            IEnumerable<IListBlobItem> blobList = this.Container.ListBlobs(this.BlobPath);
            foreach (IListBlobItem item in blobList)
            {
                CloudBlockBlob blob = item as CloudBlockBlob;
                if (blob != null)
                {
                    if (blob.BlobType == BlobType.BlockBlob)
                    {
                        bool add = true;
                        if (extensionsIsNotEmpty)
                        {
                            string extension = Path.GetExtension(blob.Name).ToLowerInvariant();
                            bool match = extensionsList.Contains(extension);

                            add = (((!negate) && match) || (negate && (!match)));
                        }

                        if (add)
                        {
                            File file = GetFileInfo(blob);
                            files.Add(file);
                        }
                    }
                }
            }

            files.Sort(CompareFilesByLastModifiedAndName);

            return new Collection<File>(files);
        }

        public void DeleteFile(string fileId)
        {
            if (!string.IsNullOrEmpty(fileId))
            {
                if (MimeType.IsImageType(MimeMapping.GetMimeMapping(fileId)))
                {
                    // Delete all thumbnails of the image.
                    string fileName = GetNameFromFileId(fileId);
                    string prefix = fileId.Replace(fileName, string.Empty);
                    fileName = "/" + fileName;

                    IEnumerable<IListBlobItem> thumbnailBlobList = this.Container.ListBlobs(prefix, true);
                    foreach (IListBlobItem item in thumbnailBlobList)
                    {
                        CloudBlockBlob blob = item as CloudBlockBlob;
                        if (blob != null)
                        {
                            if (blob.Name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
                            {
                                blob.Delete();
                            }
                        }
                    }
                }
                else
                {
                    CloudBlockBlob blob = this.Container.GetBlockBlobReference(fileId);
                    blob.Delete();
                }
            }
        }

        public byte[] GetThumbnail(string fileId, int width, int height, int align)
        {
            string fileName = GetNameFromFileId(fileId);

            return this.GetThumbnail(fileId, fileName, width, height, align);
        }

        public string GetThumbnailUrl(string fileId, int width, int height, int align, bool createApplicationAbsoluteUrl)
        {
            return string.Format(CultureInfo.InvariantCulture
                , ((createApplicationAbsoluteUrl ? VirtualPathUtility.ToAbsolute(FileHandler.VirtualPath) : FileHandler.VirtualPath) + "?d={0}")
                , HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}|{3}|{4}{5}"
                    , fileId, width, height, align, this.Container.Uri, this.ContainerSas))));
        }

        public string UploadFile(string fileName, string contentType, byte[] buffer)
        {
            if (buffer != null)
            {
                CloudBlockBlob blob = this.GetFileReference(fileName, contentType);

                int count = buffer.Length;
                blob.UploadFromByteArray(buffer, 0, count);

                return blob.Name;
            }

            return null;
        }

        public string UploadFile(string fileName, string contentType, Stream source)
        {
            CloudBlockBlob blob = this.GetFileReference(fileName, contentType);

            blob.UploadFromStream(source);

            return blob.Name;
        }

        public string UploadTemporaryFile(string fileName, string contentType, byte[] buffer, string directoryName)
        {
            if (buffer != null)
            {
                CloudBlockBlob blob = this.GetTemporaryFileReference(fileName, contentType, directoryName);

                int count = buffer.Length;
                blob.UploadFromByteArray(buffer, 0, count);

                return blob.Name;
            }

            return null;
        }

        public string UploadTemporaryFile(string fileName, string contentType, Stream source, string directoryName)
        {
            CloudBlockBlob blob = this.GetTemporaryFileReference(fileName, contentType, directoryName);

            blob.UploadFromStream(source);

            return blob.Name;
        }

        public void DeleteTemporaryFiles(string directoryName)
        {
            directoryName += "/";

            IEnumerable<IListBlobItem> blobList = this.TemporaryContainer.ListBlobs(directoryName);
            foreach (IListBlobItem item in blobList)
            {
                CloudBlockBlob blob = item as CloudBlockBlob;
                if (blob != null)
                {
                    blob.Delete();
                }
            }
        }

        public void MoveTemporaryFiles(string directoryName)
        {
            directoryName += "/";
            string blobNameFormat = this.BlobNameFormat;

            IEnumerable<IListBlobItem> temporaryBlobList = this.TemporaryContainer.ListBlobs(directoryName);
            foreach (IListBlobItem item in temporaryBlobList)
            {
                CloudBlockBlob tempBlob = item as CloudBlockBlob;
                if (tempBlob != null)
                {
                    if (tempBlob.BlobType == BlobType.BlockBlob)
                    {
                        string fileName = GetNameFromFileId(tempBlob.Name);
                        string blobName = string.Format(CultureInfo.InvariantCulture, blobNameFormat, fileName);

                        CloudBlockBlob blob = this.Container.GetBlockBlobReference(blobName);
                        blob.StartCopyFromBlob(tempBlob);

                        tempBlob.Delete();
                    }
                }
            }
        }

        #endregion
    }
}
