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
        private SharedAccessBlobPolicy m_ReadAccessPolicy;
        private CloudBlobContainer m_Container;

        #endregion

        #region Constructors

        public FileManager(string containerName, string objectType, string objectId)
        {
            this.ObjectType = objectType;
            this.ObjectId = objectId;

            m_ContainerName = containerName;
        }

        public FileManager(string containerName, bool containerPublicAccess, string objectType, string objectId) :
            this(containerName, objectType, objectId)
        {
            this.ContainerPublicAccess = containerPublicAccess;
        }

        #endregion

        #region Private Properties

        private string BlobPath
        {
            get
            {
                return GetBlobPath(this.ObjectType, this.ObjectId);
            }
        }

        private string BlobNameFormat
        {
            get
            {
                return GetBlobNameFormat(this.ObjectType, this.ObjectId);
            }
        }

        private CloudBlobContainer Container
        {
            get
            {
                if (m_Container == null)
                {
                    m_Container = ContainerManager.GetContainerReference(this.ContainerName);
                }
                return m_Container;
            }
        }

        private SharedAccessBlobPolicy ReadAccessPolicy
        {
            get
            {
                if (m_ReadAccessPolicy == null)
                {
                    m_ReadAccessPolicy = CreateReadAccessPolicy();
                }
                return m_ReadAccessPolicy;
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
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the public access to the files is allowed in the container.
        /// </summary>
        public bool ContainerPublicAccess { get; set; }

        /// <summary>
        /// Gets or sets the type of the object which the files are associated with.
        /// </summary>
        public string ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the object which the files are associated with.
        /// </summary>
        public string ObjectId { get; set; }

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

        private static SharedAccessBlobPolicy CreateReadAccessPolicy()
        {
            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(Settings.SharedAccessExpiryTime)
            };

            return policy;
        }

        private static SharedAccessBlobPolicy CreateWriteDeleteAccessPolicy()
        {
            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Delete,
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(Settings.SharedAccessExpiryTime)
            };

            return policy;
        }

        private void DeleteThumbnails(string fileId)
        {
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

        private static string GetBlobPath(string objectType, string objectId)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}/", objectType, objectId);
        }

        private static string GetBlobNameFormat(string objectType, string objectId)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{{0}}", GetBlobPath(objectType, objectId));
        }

        private File GetFileInfo(CloudBlockBlob blob)
        {
            return GetFileInfo(blob, (this.ContainerPublicAccess ? null : this.ReadAccessPolicy));
        }

        private static File GetFileInfo(CloudBlockBlob blob, SharedAccessBlobPolicy readAccessPolicy)
        {
            BlobProperties props = blob.Properties;

            string fileName = GetNameFromFileId(blob.Name);

            string url = blob.Uri.ToString();
            if (readAccessPolicy != null)
            {
                string sas = blob.GetSharedAccessSignature(readAccessPolicy);
                url = string.Format(CultureInfo.InvariantCulture, "{0}{1}", url, sas);
            }

            return new File()
            {
                FileId = blob.Name,
                Name = fileName,
                Length = props.Length,
                ContentType = props.ContentType,
                FullName = blob.Name,
                Url = url,
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

        private static CloudBlockBlob GetTemporaryFileReference(string fileName, string contentType, string directoryName)
        {
            string fileId = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", directoryName, fileName);

            CloudBlockBlob blob = ContainerManager.TemporaryContainer.GetBlockBlobReference(fileId);
            blob.Properties.ContentType = contentType;
            blob.Properties.CacheControl = Settings.ClientCacheControl;

            return blob;
        }

        private static bool IsImageBlob(string fileId)
        {
            return MimeType.IsImageType(MimeMapping.GetMimeMapping(fileId));
        }

        #endregion

        #region Internal Methods

        internal static string GetNameFromFileId(string fileId)
        {
            string[] parts = fileId.Split('/');
            int length = parts.Length;

            string fileName = parts[length - 1];

            return fileName;
        }

        internal static byte[] GetThumbnail(string d, out string fileName)
        {
            fileName = null;

            byte[] bytes = HttpServerUtility.UrlTokenDecode(d);
            if (bytes != null)
            {
                string str = Encoding.UTF8.GetString(bytes);
                string[] values = str.Split('|');

                if (values.Length > 4)
                {
                    string fileId = values[0];
                    int width = Convert.ToInt32(values[1], CultureInfo.InvariantCulture);
                    int height = Convert.ToInt32(values[2], CultureInfo.InvariantCulture);
                    int align = Convert.ToInt32(values[3], CultureInfo.InvariantCulture);
                    string containerName = values[4];

                    values = fileId.Split('/');
                    int length = values.Length;

                    string objectType = values[length - 3];
                    string objectId = values[length - 2];
                    fileName = values[length - 1];

                    FileManager manager = new FileManager(containerName, objectType, objectId);

                    return manager.GetThumbnail(fileId, fileName, width, height, align);
                }
            }

            return null;
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

        internal static string GetTemporaryFilesUrlFormat(string directoryName)
        {
            SharedAccessBlobPolicy writeDeleteAccessPolicy = CreateWriteDeleteAccessPolicy();

            string sas = ContainerManager.TemporaryContainer.GetSharedAccessSignature(writeDeleteAccessPolicy);

            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{{0}}{2}", ContainerManager.TemporaryContainer.Uri.AbsoluteUri, directoryName, sas);
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

        public static byte[] GetFileByUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                string[] values = url.Split('?')[0].Split('/');
                int length = values.Length;

                string containerName = values[length - 4];
                string objectType = values[length - 3];
                string objectId = values[length - 2];
                string fileName = values[length - 1];

                FileManager manager = new FileManager(containerName, objectType, objectId);

                string fileId = string.Format(CultureInfo.InvariantCulture, manager.BlobNameFormat, fileName);

                return manager.GetFile(fileId);
            }

            return null;
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

        public string[] GetFileNames()
        {
            List<string> names = new List<string>();

            Collection<File> files = this.GetFiles();

            foreach (File file in files)
            {
                names.Add(file.Name);
            }

            return names.ToArray();
        }

        public void DeleteFile(string fileId)
        {
            if (!string.IsNullOrEmpty(fileId))
            {
                if (IsImageBlob(fileId))
                {
                    this.DeleteThumbnails(fileId);
                }
                else
                {
                    CloudBlockBlob blob = this.Container.GetBlockBlobReference(fileId);
                    blob.Delete();
                }
            }
        }

        public void DeleteFiles()
        {
            IEnumerable<IListBlobItem> blobList = this.Container.ListBlobs(this.BlobPath);
            foreach (IListBlobItem item in blobList)
            {
                CloudBlockBlob blob = item as CloudBlockBlob;
                if (blob != null)
                {
                    if (blob.BlobType == BlobType.BlockBlob)
                    {
                        this.DeleteFile(blob.Name);
                    }
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
                , HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}|{3}|{4}", fileId, width, height, align, this.ContainerName))));
        }

        public void MoveFiles(string objectId)
        {
            string newBlobNameFormat = GetBlobNameFormat(this.ObjectType, objectId);

            IEnumerable<IListBlobItem> blobList = this.Container.ListBlobs(this.BlobPath);
            foreach (IListBlobItem item in blobList)
            {
                CloudBlockBlob blob = item as CloudBlockBlob;
                if (blob != null)
                {
                    if (blob.BlobType == BlobType.BlockBlob)
                    {
                        string fileName = GetNameFromFileId(blob.Name);
                        string newBlobName = string.Format(CultureInfo.InvariantCulture, newBlobNameFormat, fileName);

                        CloudBlockBlob newBlob = this.Container.GetBlockBlobReference(newBlobName);
                        newBlob.StartCopyFromBlob(blob);

                        this.DeleteFile(blob.Name);
                    }
                }
            }
        }

        public void MoveTemporaryFiles(string directoryName)
        {
            directoryName += "/";
            string blobNameFormat = this.BlobNameFormat;

            IEnumerable<IListBlobItem> temporaryBlobList = ContainerManager.TemporaryContainer.ListBlobs(directoryName);
            foreach (IListBlobItem item in temporaryBlobList)
            {
                CloudBlockBlob tempBlob = item as CloudBlockBlob;
                if (tempBlob != null)
                {
                    if (tempBlob.BlobType == BlobType.BlockBlob)
                    {
                        string fileName = GetNameFromFileId(tempBlob.Name);
                        string blobName = string.Format(CultureInfo.InvariantCulture, blobNameFormat, fileName);

                        if (IsImageBlob(blobName))
                        {
                            this.DeleteThumbnails(blobName);
                        }

                        CloudBlockBlob blob = this.Container.GetBlockBlobReference(blobName);
                        blob.StartCopyFromBlob(tempBlob);

                        tempBlob.Delete();
                    }
                }
            }
        }

        public void RenameFile(string fileId, string fileName)
        {
            if (!string.IsNullOrEmpty(fileId))
            {
                CloudBlockBlob blob = this.Container.GetBlockBlobReference(fileId);
                if (blob != null)
                {
                    if (blob.BlobType == BlobType.BlockBlob)
                    {
                        string blobNameFormat = this.BlobNameFormat;
                        string newBlobName = string.Format(CultureInfo.InvariantCulture, blobNameFormat, fileName);

                        CloudBlockBlob newBlob = this.Container.GetBlockBlobReference(newBlobName);
                        newBlob.StartCopyFromBlob(blob);

                        this.DeleteFile(blob.Name);
                    }
                }
            }
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

        public static File GetTemporaryFileInfo(string fileId)
        {
            CloudBlockBlob blob = ContainerManager.TemporaryContainer.GetBlockBlobReference(fileId);
            if (blob.Exists())
            {
                SharedAccessBlobPolicy readAccessPolicy = CreateReadAccessPolicy();

                return GetFileInfo(blob, readAccessPolicy);
            }

            return null;
        }

        public static Collection<File> GetTemporaryFiles(string directoryName)
        {
            directoryName += "/";

            List<File> files = new List<File>();

            SharedAccessBlobPolicy readAccessPolicy = CreateReadAccessPolicy();

            IEnumerable<IListBlobItem> blobList = ContainerManager.TemporaryContainer.ListBlobs(directoryName);
            foreach (IListBlobItem item in blobList)
            {
                CloudBlockBlob blob = item as CloudBlockBlob;
                if (blob != null)
                {
                    if (blob.BlobType == BlobType.BlockBlob)
                    {
                        File file = GetFileInfo(blob, readAccessPolicy);
                        files.Add(file);
                    }
                }
            }

            files.Sort(CompareFilesByLastModifiedAndName);

            return new Collection<File>(files);
        }

        public static string[] GetTemporaryFileNames(string directoryName)
        {
            List<string> names = new List<string>();

            Collection<File> files = GetTemporaryFiles(directoryName);

            foreach (File file in files)
            {
                string fileName = file.Name;
                names.Add(fileName);
            }

            return names.ToArray();
        }

        public static void DeleteTemporaryFiles(string directoryName)
        {
            directoryName += "/";

            IEnumerable<IListBlobItem> blobList = ContainerManager.TemporaryContainer.ListBlobs(directoryName);
            foreach (IListBlobItem item in blobList)
            {
                CloudBlockBlob blob = item as CloudBlockBlob;
                if (blob != null)
                {
                    blob.Delete();
                }
            }
        }

        public static string UploadTemporaryFile(string fileName, string contentType, byte[] buffer, string directoryName)
        {
            if (buffer != null)
            {
                CloudBlockBlob blob = GetTemporaryFileReference(fileName, contentType, directoryName);

                int count = buffer.Length;
                blob.UploadFromByteArray(buffer, 0, count);

                return blob.Name;
            }

            return null;
        }

        public static string UploadTemporaryFile(string fileName, string contentType, Stream source, string directoryName)
        {
            CloudBlockBlob blob = GetTemporaryFileReference(fileName, contentType, directoryName);

            blob.UploadFromStream(source);

            return blob.Name;
        }

        #endregion
    }
}
