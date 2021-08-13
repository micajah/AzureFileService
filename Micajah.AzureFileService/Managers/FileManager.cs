using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
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
            return CreateReadAccessPolicy(Settings.SharedAccessExpiryTime);
        }

        private static SharedAccessBlobPolicy CreateReadAccessPolicy(int sharedAccessExpiryTime)
        {
            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(sharedAccessExpiryTime)
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

        private void CopyTemporaryFiles(string directoryName, bool deleteTemporaryFiles)
        {
            directoryName += "/";

            IEnumerable<IListBlobItem> temporaryBlobList = ListBlobs(ContainerManager.TemporaryContainer, directoryName);
            foreach (IListBlobItem item in temporaryBlobList)
            {
                CloudBlockBlob tempBlob = item as CloudBlockBlob;
                if (tempBlob != null)
                {
                    if (tempBlob.BlobType == BlobType.BlockBlob)
                    {
                        string fileName = GetNameFromFileId(tempBlob.Name);
                        string blobName = GetFileId(fileName);

                        if (MimeType.IsInGroups(tempBlob.Properties.ContentType, MimeTypeGroups.Image))
                        {
                            RotateFlipImageByOrientation(tempBlob);

                            if (deleteTemporaryFiles)
                            {
                                this.DeleteThumbnails(blobName);
                            }
                        }

                        CloudBlockBlob blob = this.Container.GetBlockBlobReference(blobName);
                        blob.StartCopy(tempBlob);

                        if (deleteTemporaryFiles)
                        {
                            tempBlob.Delete();
                        }
                    }
                }
            }
        }

        private void CopyFiles(string objectId, bool delete)
        {
            string newBlobNameFormat = GetBlobNameFormat(this.ObjectType, objectId);

            IEnumerable<IListBlobItem> blobList = ListBlobs(this.Container, this.BlobPath);
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
                        newBlob.StartCopy(blob);

                        if (delete)
                        {
                            this.DeleteFile(blob.Name);
                        }
                    }
                }
            }
        }

        private void DeleteThumbnails(string fileId)
        {
            string fileName = GetNameFromFileId(fileId);
            string prefix = fileId.Replace(fileName, string.Empty);
            fileName = "/" + fileName;

            IEnumerable<IListBlobItem> thumbnailBlobList = ListBlobs(this.Container, prefix, true);
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
            if (string.IsNullOrEmpty(objectId))
            {
                return string.IsNullOrEmpty(objectType) ? null : $"{objectType}/";
            }

            return $"{objectType}/{objectId}/";
        }

        private static string GetBlobNameFormat(string objectType, string objectId)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{{0}}", GetBlobPath(objectType, objectId));
        }

        private string GetFileId(string fileName)
        {
            string fileId = string.Format(CultureInfo.InvariantCulture, this.BlobNameFormat, fileName);

            return fileId;
        }

        private File GetFileInfo(CloudBlockBlob blob)
        {
            return GetFileInfo(blob, (this.ContainerPublicAccess ? null : this.ReadAccessPolicy));
        }

        private static File GetFileInfo(CloudBlockBlob blob, SharedAccessBlobPolicy readAccessPolicy)
        {
            BlobProperties props = blob.Properties;

            string[] parts = blob.Name.Split('/');
            int length = parts.Length;

            string fileName = parts[length - 1];
            string objectType = parts[0];
            string objectId = parts[1];

            string sas = string.Empty;
            if (readAccessPolicy != null)
            {
                sas = blob.GetSharedAccessSignature(readAccessPolicy);
            }

            string url = string.Format(CultureInfo.InvariantCulture, "{0}{1}", blob.Uri, sas);

            string secondaryUrl = null;
            if (!string.IsNullOrWhiteSpace(Settings.FileSecondaryUrl))
            {
                secondaryUrl = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", Settings.FileSecondaryUrl, blob.Uri.AbsolutePath, sas);
            }

            return new File()
            {
                FileId = blob.Name,
                Name = fileName,
                Length = props.Length,
                ContentType = props.ContentType,
                FullName = blob.Name,
                Url = url,
                SecondaryUrl = secondaryUrl,
                LastModified = props.LastModified.Value.DateTime,
                ObjectType = objectType,
                ObjectId = objectId
            };
        }

        private Collection<File> GetFiles(FileSearchOptions searchOptions, SharedAccessBlobPolicy readAccessPolicy)
        {
            List<File> files = new List<File>();

            List<string> extensionsList = searchOptions.ExtensionsFilter == null ? new List<string>() : new List<string>(searchOptions.ExtensionsFilter);
            bool extensionsIsNotEmpty = extensionsList.Count > 0;

            IEnumerable<IListBlobItem> blobList = ListBlobs(this.Container, this.BlobPath, searchOptions.AllFiles);
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

                            add = (((!searchOptions.NegateExtensionsFilter) && match) || (searchOptions.NegateExtensionsFilter && (!match)));
                        }

                        if (add)
                        {
                            File file = GetFileInfo(blob, readAccessPolicy);
                            files.Add(file);
                        }
                    }
                }
            }

            files.Sort(CompareFilesByLastModifiedAndName);

            return new Collection<File>(files);
        }

        private CloudBlockBlob GetFileReference(string fileName, string contentType)
        {
            fileName = EscapeInvalidChars(fileName);

            string fileId = this.GetFileId(fileName);

            // Fixes content type.
            if (MimeType.IsDefaultOrEmpty(contentType))
            {
                contentType = MimeType.GetMimeType(fileName);
            }

            CloudBlockBlob blob = this.Container.GetBlockBlobReference(fileId);

            blob.Properties.CacheControl = Settings.ClientCacheControl;
            blob.Properties.ContentType = contentType;

            if (MimeType.IsHtml(contentType))
            {
                blob.Properties.ContentDisposition = "attachment";
            }

            return blob;
        }

        private static CloudBlockBlob GetTemporaryFileReference(string fileName, string contentType, string directoryName)
        {
            fileName = EscapeInvalidChars(fileName);

            string fileId = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", directoryName, fileName);

            // Fixes content type.
            if (MimeType.IsDefaultOrEmpty(contentType))
            {
                contentType = MimeType.GetMimeType(fileName);
            }

            CloudBlockBlob blob = ContainerManager.TemporaryContainer.GetBlockBlobReference(fileId);

            blob.Properties.ContentType = contentType;
            blob.Properties.CacheControl = Settings.ClientCacheControl;

            if (MimeType.IsHtml(contentType))
            {
                blob.Properties.ContentDisposition = "attachment";
            }

            return blob;
        }

        private static IEnumerable<IListBlobItem> ListBlobs(CloudBlobContainer container, string prefix, bool useFlatBlobListing = false)
        {
            try
            {
                return container.ListBlobs(prefix, useFlatBlobListing);
            }
            catch (StorageException ex)
            {
                RequestResult requestInfo = ex.RequestInformation;
                if (requestInfo != null)
                {
                    StorageExtendedErrorInformation errorInfo = requestInfo.ExtendedErrorInformation;
                    if (errorInfo != null)
                    {
                        if (errorInfo.ErrorCode.StartsWith("Container", StringComparison.Ordinal))
                        {
                            string errorMessage = string.Format(CultureInfo.InvariantCulture, "{0} The container name is \"{1}\".", requestInfo.HttpStatusMessage, container.Name);

                            throw new StorageException(errorMessage, ex);
                        }
                    }
                }

                throw;
            }
        }

        private static void RotateFlipImageByOrientation(CloudBlockBlob blob)
        {
            MemoryStream source = null;

            try
            {
                if (blob.Properties.Length > 0)
                {
                    source = new MemoryStream();
                    blob.DownloadToStream(source);

                    byte[] bytes = RotateFlipImageByOrientation(blob.Properties.ContentType, source);

                    if (bytes != null)
                    {
                        int count = bytes.Length;
                        blob.UploadFromByteArray(bytes, 0, count);
                    }
                }
            }
            finally
            {
                if (source != null)
                {
                    source.Dispose();
                }
            }
        }

        private static byte[] RotateFlipImageByOrientation(string contentType, byte[] buffer)
        {
            MemoryStream source = null;

            try
            {
                if (buffer.Length > 0)
                {
                    source = new MemoryStream(buffer);
                }

                return RotateFlipImageByOrientation(contentType, source);
            }
            finally
            {
                if (source != null)
                {
                    source.Dispose();
                }
            }
        }

        private static byte[] RotateFlipImageByOrientation(string contentType, Stream source)
        {
            MemoryStream output = null;
            Image image = null;

            try
            {
                if (source != null)
                {
                    if (source.Length > 0)
                    {
                        source.Position = 0;

                        image = Image.FromStream(source);

                        if (image.RotateFlipByOrientation())
                        {
                            ImageFormat imageFormat = MimeType.GetImageFormat(contentType) ?? ImageFormat.Jpeg;

                            output = new MemoryStream();
                            image.Save(output, imageFormat);
                            output.Position = 0;

                            return output.ToArray();
                        }
                    }
                }
            }
            catch (Exception) { }
            finally
            {
                if (output != null)
                {
                    output.Dispose();
                }

                if (image != null)
                {
                    image.Dispose();
                }
            }

            return null;
        }

        private static void UploadBlobFromByteArray(CloudBlockBlob blob, string contentType, byte[] buffer)
        {
            byte[] bytes = null;

            if (MimeType.IsInGroups(contentType, MimeTypeGroups.Image))
            {
                bytes = RotateFlipImageByOrientation(contentType, buffer);
            }

            if (bytes == null)
            {
                bytes = buffer;
            }

            int count = bytes.Length;
            blob.UploadFromByteArray(bytes, 0, count);
        }

        private static void UploadBlobFromStream(CloudBlockBlob blob, string contentType, Stream source)
        {
            if (MimeType.IsInGroups(contentType, MimeTypeGroups.Image))
            {
                byte[] bytes = RotateFlipImageByOrientation(contentType, source);

                if (bytes != null)
                {
                    int count = bytes.Length;
                    blob.UploadFromByteArray(bytes, 0, count);

                    return;
                }
            }

            source.Position = 0;
            blob.UploadFromStream(source);
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

                if (blob.Exists() && blob.Properties.Length > 0)
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

        public File GetFileInfo(string fileId, int sharedAccessExpiryTime)
        {
            CloudBlockBlob blob = this.Container.GetBlockBlobReference(fileId);
            if (blob.Exists())
            {
                SharedAccessBlobPolicy readAccessPolicy = CreateReadAccessPolicy(sharedAccessExpiryTime);

                return GetFileInfo(blob, readAccessPolicy);
            }

            return null;
        }

        public File GetFileInfoByName(string fileName)
        {
            string fileId = this.GetFileId(fileName);

            return this.GetFileInfo(fileId);
        }

        public File GetFileInfoByName(string fileName, int sharedAccessExpiryTime)
        {
            string fileId = this.GetFileId(fileName);

            return this.GetFileInfo(fileId, sharedAccessExpiryTime);
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

        public byte[] GetFileByName(string fileName)
        {
            string fileId = this.GetFileId(fileName);

            return this.GetFile(fileId);
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

                return manager.GetFileByName(fileName);
            }

            return null;
        }

        public Collection<File> GetFiles()
        {
            return GetFiles(new FileSearchOptions());
        }

        public Collection<File> GetFiles(FileSearchOptions searchOptions)
        {
            return GetFiles(searchOptions, ReadAccessPolicy);
        }

        public Collection<File> GetFiles(int readAccessExpiryTime)
        {
            return GetFiles(new FileSearchOptions(), readAccessExpiryTime);
        }

        public Collection<File> GetFiles(FileSearchOptions searchOptions, int readAccessExpiryTime)
        {
            SharedAccessBlobPolicy readAccessPolicy = CreateReadAccessPolicy(readAccessExpiryTime);

            return GetFiles(searchOptions, readAccessPolicy);
        }

        public string[] GetFileNames()
        {
            return GetFileNames(new FileSearchOptions());
        }

        public string[] GetFileNames(FileSearchOptions searchOptions)
        {
            List<string> names = new List<string>();

            Collection<File> files = GetFiles(searchOptions);

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
                if (MimeType.IsInGroups(fileId, MimeTypeGroups.Image, true))
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
            IEnumerable<IListBlobItem> blobList = ListBlobs(this.Container, this.BlobPath);
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

        public bool DownloadToFile(string fileId, string path)
        {
            CloudBlockBlob blob = this.Container.GetBlockBlobReference(fileId);
            if (blob.Exists())
            {
                blob.DownloadToFile(path, FileMode.Create);

                return true;
            }

            return false;
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

        public void CopyFiles(string objectId)
        {
            CopyFiles(objectId, false);
        }

        public void MoveFiles(string objectId)
        {
            CopyFiles(objectId, true);
        }

        public void MoveTemporaryFiles(string directoryName)
        {
            CopyTemporaryFiles(directoryName, true);
        }

        public void CopyTemporaryFiles(string directoryName)
        {
            CopyTemporaryFiles(directoryName, false);
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
                        fileName = EscapeInvalidChars(fileName);

                        string newBlobName = GetFileId(fileName);

                        CloudBlockBlob newBlob = this.Container.GetBlockBlobReference(newBlobName);
                        newBlob.StartCopy(blob);

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

                UploadBlobFromByteArray(blob, contentType, buffer);

                return blob.Name;
            }

            return null;
        }

        public string UploadFile(string fileName, string contentType, Stream source)
        {
            if (source != null)
            {
                CloudBlockBlob blob = this.GetFileReference(fileName, contentType);

                UploadBlobFromStream(blob, contentType, source);

                return blob.Name;
            }

            return null;
        }

        public string UploadFileFromUrl(string fileUrl)
        {
            return UploadFileFromUrl(fileUrl, null);
        }

        public string UploadFileFromUrl(string fileUrl, FileContentTypeValidator validator)
        {
            if (!string.IsNullOrEmpty(fileUrl))
            {
                string fileName = null;
                string contentType = null;
                byte[] buffer = null;

                if (fileUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) // data:[<media type[;charset=utf-8]>][;base64],<data>
                {
                    string[] parts = fileUrl.Split(',');
                    string[] parts2 = parts[0].Split(':')[1].Split(';');

                    if (Array.IndexOf<string>(parts2, "base64") > -1)
                    {
                        buffer = Convert.FromBase64String(parts[1]);
                    }
                    else if (parts.Length > 1)
                    {
                        string data = parts[1];

                        if (!string.IsNullOrEmpty(data))
                        {
                            string charset = Array.Find<string>(parts2, x => x.StartsWith("charset=", StringComparison.OrdinalIgnoreCase));
                            if (!string.IsNullOrEmpty(charset))
                            {
                                charset = charset.Split('=')[1];
                                if (!string.IsNullOrEmpty(charset))
                                {
                                    try
                                    {
                                        Encoding encoding = Encoding.GetEncoding(charset);

                                        data = HttpUtility.UrlDecode(data, encoding);
                                    }
                                    catch (ArgumentException) { }
                                }
                            }
                        }

                        buffer = Encoding.UTF8.GetBytes(data);
                    }

                    contentType = parts2[0];
                    fileName = Guid.NewGuid().ToString("N").Substring(0, 12) + MimeType.GetExtension(contentType);
                }
                else if (fileUrl.StartsWith(Uri.UriSchemeHttp + Uri.SchemeDelimiter, StringComparison.OrdinalIgnoreCase) ||
                    fileUrl.StartsWith(Uri.UriSchemeHttps + Uri.SchemeDelimiter, StringComparison.OrdinalIgnoreCase))
                {
                    fileName = Path.GetFileName(fileUrl.Split('?')[0]);
                    contentType = MimeType.GetMimeType(fileName);

                    string responseContentType = null;

                    using (WebClient webClient = new WebClient())
                    {
                        buffer = webClient.DownloadData(fileUrl);

                        if (webClient.ResponseHeaders != null)
                        {
                            responseContentType = webClient.ResponseHeaders["Content-Type"];
                        }
                    }

                    if (!string.IsNullOrEmpty(responseContentType) && string.Compare(responseContentType, contentType, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        fileName = Path.GetFileNameWithoutExtension(fileName) + MimeType.GetExtension(responseContentType);
                        contentType = responseContentType;
                    }
                }

                if (buffer != null)
                {
                    if (validator != null)
                    {
                        if (!validator.Invoke(contentType))
                        {
                            throw new System.IO.InvalidDataException(string.Format(CultureInfo.InvariantCulture, "Invalid content type \"{0}\".", contentType));
                        }
                    }

                    return UploadFile(fileName, contentType, buffer);
                }
            }

            return null;
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

            IEnumerable<IListBlobItem> blobList = ListBlobs(ContainerManager.TemporaryContainer, directoryName);
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

            IEnumerable<IListBlobItem> blobList = ListBlobs(ContainerManager.TemporaryContainer, directoryName);
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

                UploadBlobFromByteArray(blob, contentType, buffer);

                return blob.Name;
            }

            return null;
        }

        public static string UploadTemporaryFile(string fileName, string contentType, Stream source, string directoryName)
        {
            if (source != null)
            {
                CloudBlockBlob blob = GetTemporaryFileReference(fileName, contentType, directoryName);

                UploadBlobFromStream(blob, contentType, source);

                return blob.Name;
            }

            return null;
        }

        public static string EscapeInvalidChars(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                string name = string.Join(string.Empty, fileName.Split(Path.GetInvalidFileNameChars())).Replace("\ufffd", string.Empty);

                string nameWithoutExtension = Path.GetFileNameWithoutExtension(name);

                if (nameWithoutExtension.Trim().Length == 0)
                {
                    string extension = Path.GetExtension(name);

                    name = DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture) + extension;
                }

                return name;
            }

            return fileName;
        }

        #endregion
    }

    public delegate bool FileContentTypeValidator(string contentType);
}
