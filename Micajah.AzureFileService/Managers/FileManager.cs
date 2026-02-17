using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Micajah.AzureFileService.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace Micajah.AzureFileService
{
    public class FileManager
    {
        #region Members

        private static BlobServiceClient s_ServiceClient;
        private static BlobContainerClient s_TemporaryContainer;

        private string m_ContainerName;
        private BlobSasBuilder m_ReadPermissions;
        private BlobContainerClient m_Container;

        #endregion

        #region Constructors

        public FileManager(string containerName, string objectType, string objectId)
        {
            ObjectType = objectType;
            ObjectId = objectId;

            m_ContainerName = containerName;
        }

        public FileManager(string containerName, bool containerPublicAccess, string objectType, string objectId) :
            this(containerName, objectType, objectId)
        {
            ContainerPublicAccess = containerPublicAccess;
        }

        #endregion

        #region Private Properties

        private string BlobPath => GetBlobPath(ObjectType, ObjectId);

        private string BlobNameFormat => GetBlobNameFormat(ObjectType, ObjectId);

        private BlobContainerClient Container
        {
            get
            {
                if (m_Container == null)
                {
                    m_Container = ServiceClient.GetBlobContainerClient(ContainerName);
                }
                return m_Container;
            }
        }

        private static BlobContainerClient TemporaryContainer
        {
            get
            {
                if (s_TemporaryContainer == null)
                {
                    s_TemporaryContainer = ServiceClient.GetBlobContainerClient(Settings.TemporaryContainerName);

                    s_TemporaryContainer.CreateIfNotExists();
                }
                return s_TemporaryContainer;
            }
        }

        private BlobSasBuilder ReadPermissions
        {
            get
            {
                if (m_ReadPermissions == null)
                {
                    m_ReadPermissions = CreateReadPermissions();
                }
                return m_ReadPermissions;
            }
        }

        #endregion

        #region Internal Properties

        internal static BlobServiceClient ServiceClient
        {
            get
            {
                if (s_ServiceClient == null)
                {
                    s_ServiceClient = new BlobServiceClient(Settings.StorageConnectionString);
                }
                return s_ServiceClient;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the base URI for the Blob service client at the primary location.
        /// </summary>
        public static string ServiceClientBaseUrl => ServiceClient.Uri.ToString();

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
            int result;

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

        private static BlobSasBuilder CreateReadPermissions()
        {
            return CreateReadPermissions(Settings.SharedAccessExpiryTime);
        }

        private static BlobSasBuilder CreateReadPermissions(int sharedAccessExpiryTime)
        {
            BlobSasBuilder policy = new BlobSasBuilder(BlobContainerSasPermissions.Read, DateTime.UtcNow.AddMinutes(sharedAccessExpiryTime));

            return policy;
        }

        private static BlobSasBuilder CreateWriteDeletePermissions()
        {
            BlobSasBuilder policy = new BlobSasBuilder(BlobContainerSasPermissions.Write | BlobContainerSasPermissions.Delete, DateTime.UtcNow.AddMinutes(Settings.SharedAccessExpiryTime));

            return policy;
        }

        private void CopyTemporaryFiles(string directoryName, Dictionary<string, string> metadata, bool deleteTemporaryFiles)
        {
            var temporaryBlobItems = GetBlockBlobs(TemporaryContainer, directoryName + "/");

            foreach (var tempBlobItem in temporaryBlobItems)
            {
                string fileName = GetNameFromFileId(tempBlobItem.Name);
                string blobName = GetFileId(fileName);

                var blob = Container.GetBlobClient(blobName);
                var tempBlob = TemporaryContainer.GetBlobClient(tempBlobItem.Name);

                if (MimeType.IsInGroups(tempBlobItem.Properties.ContentType, MimeTypeGroups.Image))
                {
                    Stream source = null;

                    try
                    {
                        var downloadResult = tempBlob.DownloadContent().Value;

                        source = downloadResult.Content.ToStream();

                        byte[] bytes = RotateFlipImageByOrientation(tempBlobItem.Properties.ContentType, source);

                        if (bytes == null)
                        {
                            bytes = downloadResult.Content.ToArray();
                        }

                        if (bytes != null)
                        {
                            var uploadOptions = new BlobUploadOptions
                            {
                                HttpHeaders = new BlobHttpHeaders
                                {
                                    ContentType = tempBlobItem.Properties.ContentType,
                                    CacheControl = Settings.ClientCacheControl
                                },
                                Metadata = metadata
                            };

                            blob.Upload(new BinaryData(bytes), uploadOptions);
                        }
                    }
                    finally
                    {
                        source?.Dispose();
                    }

                    DeleteThumbnails(blobName);
                }
                else
                {
                    if (tempBlob.Exists())
                    {
                        blob.StartCopyFromUri(tempBlob.Uri, new BlobCopyFromUriOptions { Metadata = metadata });
                    }
                }

                if (deleteTemporaryFiles)
                {
                    tempBlob.DeleteIfExists();
                }
            }
        }

        private void CopyFiles(string objectId, bool delete)
        {
            string newBlobNameFormat = GetBlobNameFormat(ObjectType, objectId);

            var blobItems = GetBlockBlobs(Container, BlobPath);

            foreach (var blobItem in blobItems)
            {
                var blob = Container.GetBlobClient(blobItem.Name);

                if (blob.Exists())
                {
                    string fileName = GetNameFromFileId(blobItem.Name);
                    string newBlobName = string.Format(CultureInfo.InvariantCulture, newBlobNameFormat, fileName);

                    var newBlob = Container.GetBlobClient(newBlobName);

                    newBlob.StartCopyFromUri(blob.Uri);

                    if (delete)
                    {
                        DeleteFile(blobItem.Name);
                    }
                }
            }
        }

        private void DeleteThumbnails(string fileId)
        {
            string fileName = GetNameFromFileId(fileId);
            string prefix = fileId.Replace(fileName, string.Empty);
            fileName = "/" + fileName;

            var blobItems = GetBlockBlobs(Container, prefix, true);

            foreach (var blobItem in blobItems)
            {
                if (blobItem.Name != fileId && blobItem.Name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    var blob = Container.GetBlobClient(blobItem.Name);

                    blob.DeleteIfExists();
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
            string fileId = string.Format(CultureInfo.InvariantCulture, BlobNameFormat, fileName);

            return fileId;
        }

        private static void FillFileInfo(File file, BlobClient blob, BlobSasBuilder readPermissions)
        {
            file.FileId = blob.Name;
            file.FullName = blob.Name;

            string[] parts = blob.Name.Split('/');
            int length = parts.Length;

            file.Name = parts[length - 1];
            file.ObjectType = parts[0];
            file.ObjectId = parts[1];

            string sasUrl = null;
            if (readPermissions != null)
            {
                sasUrl = blob.GenerateSasUri(readPermissions).ToString();
            }

            file.Url = sasUrl ?? blob.Uri.ToString();

            if (!string.IsNullOrWhiteSpace(Settings.FileSecondaryUrl))
            {
                file.SecondaryUrl = sasUrl == null
                    ? $"{Settings.FileSecondaryUrl}{blob.Uri.AbsolutePath}"
                    : $"{Settings.FileSecondaryUrl}{blob.Uri.AbsolutePath}?{sasUrl.Split('?')[1]}";
            }
        }

        private static File GetFileInfo(BlobClient blob, BlobSasBuilder readPermissions)
        {
            File file = null;

            if (blob.Exists())
            {
                var props = blob.GetProperties().Value;

                file = GetFileInfo(blob, readPermissions, props);
            }

            return file;
        }

        private static File GetFileInfo(BlobClient blob, BlobSasBuilder readPermissions, BlobProperties blobProperties)
        {
            File file = new File
            {
                Length = blobProperties.ContentLength,
                ContentType = blobProperties.ContentType,
                LastModified = blobProperties.LastModified.DateTime
            };

            FillFileInfo(file, blob, readPermissions);

            return file;
        }

        private static File GetFileInfo(BlobClient blob, BlobSasBuilder readPermissions, BlobItemProperties blobItemProperties)
        {
            File file = new File
            {
                Length = blobItemProperties.ContentLength.GetValueOrDefault(),
                ContentType = blobItemProperties.ContentType,
                LastModified = blobItemProperties.LastModified.GetValueOrDefault().DateTime
            };

            FillFileInfo(file, blob, readPermissions);

            return file;
        }

        private Collection<File> GetFiles(FileSearchOptions searchOptions, BlobSasBuilder readPermissions)
        {
            List<File> files = new List<File>();

            List<string> extensionsList = searchOptions.ExtensionsFilter == null ? new List<string>() : new List<string>(searchOptions.ExtensionsFilter);
            bool extensionsIsNotEmpty = extensionsList.Count > 0;

            bool filterByMetadata = searchOptions.MetadataFilter?.Any() ?? false;

            var blobItems = GetBlockBlobs(Container, BlobPath, searchOptions.AllFiles, filterByMetadata);

            foreach (var blobItem in blobItems)
            {
                bool add = true;

                if (extensionsIsNotEmpty)
                {
                    string extension = Path.GetExtension(blobItem.Name).ToLowerInvariant();
                    bool match = extensionsList.Contains(extension);

                    add = (((!searchOptions.NegateExtensionsFilter) && match) || (searchOptions.NegateExtensionsFilter && (!match)));
                }

                if (filterByMetadata)
                {
                    foreach (var filter in searchOptions.MetadataFilter)
                    {
                        blobItem.Metadata.TryGetValue(filter.Key, out string value);

                        if (bool.TryParse(filter.Value, out _))
                        {
                            if (value == null)
                            {
                                value = bool.FalseString;
                            }
                        }

                        if (value != filter.Value)
                        {
                            add = false;
                            break;
                        }
                    }
                }

                if (add)
                {
                    var blob = Container.GetBlobClient(blobItem.Name);

                    File file = GetFileInfo(blob, readPermissions, blobItem.Properties);

                    files.Add(file);
                }
            }

            files.Sort(CompareFilesByLastModifiedAndName);

            return new Collection<File>(files);
        }

        private static BlobHttpHeaders CreateBlobHttpHeaders(string contentType, string fileName)
        {
            if (MimeType.IsDefaultOrEmpty(contentType))
            {
                contentType = MimeType.GetMimeType(fileName);
            }

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType,
                CacheControl = Settings.ClientCacheControl
            };

            if (contentType.In(MimeType.Html))
            {
                blobHttpHeaders.ContentDisposition = "attachment";
            }

            return blobHttpHeaders;
        }

        private static byte[] RotateFlipImageByOrientation(string contentType, Stream source)
        {
            MemoryStream output = null;
            Image image = null;

            try
            {
                if (source != null && source.Length > 0)
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
            catch { }
            finally
            {
                output?.Dispose();
                image?.Dispose();
            }

            return null;
        }

        private static void UploadBlobFromStream(BlobClient blob, string fileName, string contentType, Stream source)
        {
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = CreateBlobHttpHeaders(contentType, fileName)
            };

            if (MimeType.IsInGroups(contentType, MimeTypeGroups.Image))
            {
                byte[] bytes = RotateFlipImageByOrientation(contentType, source);

                if (bytes != null)
                {
                    blob.Upload(new BinaryData(bytes), uploadOptions);

                    return;
                }
            }

            source.Position = 0;

            blob.Upload(source, uploadOptions);
        }

        #endregion

        #region Internal Methods

        internal static IEnumerable<BlobItem> GetBlockBlobs(BlobContainerClient containerClient, string prefix, bool useFlatBlobListing = false, bool includeMetadata = false)
        {
            try
            {
                List<BlobItem> list = new List<BlobItem>();

                if (useFlatBlobListing || string.IsNullOrWhiteSpace(prefix))
                {
                    var blobPages = containerClient.GetBlobs(includeMetadata ? BlobTraits.Metadata : BlobTraits.None, BlobStates.None,
                        prefix, CancellationToken.None).AsPages();

                    foreach (var blobPage in blobPages)
                    {
                        var blobItems = blobPage.Values.Where(x => x.Properties.BlobType == BlobType.Block);

                        list.AddRange(blobItems);
                    }
                }
                else
                {
                    var blobPages = containerClient.GetBlobsByHierarchy(includeMetadata ? BlobTraits.Metadata : BlobTraits.None, BlobStates.None,
                        "/", prefix, CancellationToken.None).AsPages();

                    foreach (var blobPage in blobPages)
                    {
                        foreach (var blobItem in blobPage.Values)
                        {
                            if (blobItem.Blob?.Properties.BlobType == BlobType.Block)
                            {
                                list.Add(blobItem.Blob);
                            }
                        }
                    }
                }

                return list;
            }
            catch (RequestFailedException ex)
            {
                if (ex.ErrorCode == BlobErrorCode.ContainerNotFound)
                {
                    throw new RequestFailedException(string.Format(CultureInfo.InvariantCulture, Resources.FileManager_ContainerNotFound, containerClient?.Name), ex);
                }

                throw;
            }
        }

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

            string thumbBlobName = $"{ObjectType}/{ObjectId}/{width}x{height}x{align}/{fileName}";

            var thumbBlob = Container.GetBlobClient(thumbBlobName);

            if (thumbBlob.Exists())
            {
                var thumbFileInfo = GetFileInfo(thumbBlob, ReadPermissions);

                if (thumbFileInfo.ContentType == MimeType.Png)
                {
                    var downloadResult = thumbBlob.DownloadContent().Value;

                    bytes = downloadResult.Content.ToArray();
                }
            }

            if (bytes == null)
            {
                var imageBlob = Container.GetBlobClient(fileId);

                if (imageBlob.Exists())
                {
                    Stream imageStream = null;
                    MemoryStream thumbStream = null;

                    try
                    {
                        var downloadResult = imageBlob.DownloadContent().Value;
                        imageStream = downloadResult.Content.ToStream();

                        thumbStream = new MemoryStream();
                        Thumbnail.Create(imageStream, fileName, width, height, align, thumbStream);

                        var uploadOptions = new BlobUploadOptions
                        {
                            HttpHeaders = new BlobHttpHeaders
                            {
                                ContentType = MimeType.Png,
                                CacheControl = Settings.ClientCacheControl
                            }
                        };

                        thumbBlob.Upload(thumbStream, uploadOptions);

                        thumbStream.Position = 0;

                        bytes = thumbStream.ToArray();
                    }
                    finally
                    {
                        imageStream?.Dispose();
                        thumbStream?.Dispose();
                    }
                }
            }

            return bytes;
        }

        internal static string GetTemporaryFilesUrlFormat(string directoryName)
        {
            var writeDeletePermissions = CreateWriteDeletePermissions();

            string sasUrl = TemporaryContainer.GenerateSasUri(writeDeletePermissions).ToString();

            string urlFormat = sasUrl.Replace("?", $"/{directoryName}/{{0}}?");

            return urlFormat;
        }

        #endregion

        #region Public Methods

        public File GetFileInfo(string fileId)
        {
            var blob = Container.GetBlobClient(fileId);

            return GetFileInfo(blob, ContainerPublicAccess ? null : ReadPermissions);
        }

        public File GetFileInfo(string fileId, int sharedAccessExpiryTime)
        {
            var blob = Container.GetBlobClient(fileId);

            return GetFileInfo(blob, sharedAccessExpiryTime > 0 ? CreateReadPermissions(sharedAccessExpiryTime) : null);
        }

        public File GetFileInfoByName(string fileName)
        {
            string fileId = GetFileId(fileName);

            return GetFileInfo(fileId);
        }

        public File GetFileInfoByName(string fileName, int sharedAccessExpiryTime)
        {
            string fileId = GetFileId(fileName);

            return GetFileInfo(fileId, sharedAccessExpiryTime);
        }

        public byte[] GetFile(string fileId)
        {
            byte[] bytes = null;

            var blob = Container.GetBlobClient(fileId);

            if (blob.Exists())
            {
                var downloadResult = blob.DownloadContent().Value;

                bytes = downloadResult.Content.ToArray();
            }

            return bytes;
        }

        public byte[] GetFileByName(string fileName)
        {
            string fileId = GetFileId(fileName);

            return GetFile(fileId);
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
            return GetFiles(searchOptions, ReadPermissions);
        }

        public Collection<File> GetFiles(int readAccessExpiryTime)
        {
            return GetFiles(new FileSearchOptions(), readAccessExpiryTime);
        }

        public Collection<File> GetFiles(FileSearchOptions searchOptions, int readAccessExpiryTime)
        {
            return GetFiles(searchOptions, CreateReadPermissions(readAccessExpiryTime));
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
                    DeleteThumbnails(fileId);
                }

                var blob = Container.GetBlobClient(fileId);

                blob.DeleteIfExists();
            }
        }

        public void DeleteFiles()
        {
            var blobItems = GetBlockBlobs(Container, BlobPath);

            foreach (var blobItem in blobItems)
            {
                DeleteFile(blobItem.Name);
            }
        }

        public bool DownloadToFile(string fileId, string path)
        {
            var blob = Container.GetBlobClient(fileId);

            if (blob.Exists())
            {
                blob.DownloadTo(path);

                return true;
            }

            return false;
        }

        public byte[] GetThumbnail(string fileId, int width, int height, int align)
        {
            string fileName = GetNameFromFileId(fileId);

            return GetThumbnail(fileId, fileName, width, height, align);
        }

        public string GetThumbnailUrl(string fileId, int width, int height, int align, bool createApplicationAbsoluteUrl)
        {
            string url = createApplicationAbsoluteUrl
                ? VirtualPathUtility.ToAbsolute(FileHandler.VirtualPath)
                : FileHandler.VirtualPath;
            string d = HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes($"{fileId}|{width}|{height}|{align}|{ContainerName}|v1"));

            return $"{url}?d={d}";
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
            MoveTemporaryFiles(directoryName, null);
        }

        public void MoveTemporaryFiles(string directoryName, Dictionary<string, string> metadata)
        {
            CopyTemporaryFiles(directoryName, metadata, true);
        }

        public void CopyTemporaryFiles(string directoryName)
        {
            CopyTemporaryFiles(directoryName, null);
        }

        public void CopyTemporaryFiles(string directoryName, Dictionary<string, string> metadata)
        {
            CopyTemporaryFiles(directoryName, metadata, false);
        }

        public void RenameFile(string fileId, string fileName)
        {
            if (!string.IsNullOrEmpty(fileId))
            {
                var blob = Container.GetBlobClient(fileId);

                if (blob.Exists())
                {
                    string fileName2 = EscapeInvalidChars(fileName);
                    string newBlobName = GetFileId(fileName2);

                    var newBlob = Container.GetBlobClient(newBlobName);

                    newBlob.StartCopyFromUri(blob.Uri);

                    DeleteFile(blob.Name);
                }
            }
        }

        public string UploadFile(string fileName, string contentType, byte[] buffer)
        {
            if (buffer != null && buffer.Length > 0)
            {
                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    return UploadFile(fileName, contentType, stream);
                }
            }

            return null;
        }

        public string UploadFile(string fileName, string contentType, Stream source)
        {
            if (source != null)
            {
                string fileName2 = EscapeInvalidChars(fileName);

                string fileId = GetFileId(fileName2);

                var blob = Container.GetBlobClient(fileId);

                UploadBlobFromStream(blob, fileName, contentType, source);

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
                            throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, Resources.FileManager_InvalidContentType, contentType));
                        }
                    }

                    return UploadFile(fileName, contentType, buffer);
                }
            }

            return null;
        }

        public static File GetTemporaryFileInfo(string fileId)
        {
            var blob = TemporaryContainer.GetBlobClient(fileId);

            return GetFileInfo(blob, CreateReadPermissions());
        }

        public static Collection<File> GetTemporaryFiles(string directoryName)
        {
            List<File> files = new List<File>();

            var readPermissions = CreateReadPermissions();

            var blobItems = GetBlockBlobs(TemporaryContainer, directoryName + "/");

            foreach (var blobItem in blobItems)
            {
                var blob = TemporaryContainer.GetBlobClient(blobItem.Name);

                File file = GetFileInfo(blob, readPermissions, blobItem.Properties);

                files.Add(file);
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
            var blobItems = GetBlockBlobs(TemporaryContainer, directoryName + "/");

            foreach (var blobItem in blobItems)
            {
                var blob = TemporaryContainer.GetBlobClient(blobItem.Name);

                blob.DeleteIfExists();
            }
        }

        public static string UploadTemporaryFile(string fileName, string contentType, byte[] buffer, string directoryName)
        {
            if (buffer != null && buffer.Length > 0)
            {
                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    return UploadTemporaryFile(fileName, contentType, stream, directoryName);
                }
            }

            return null;
        }

        public static string UploadTemporaryFile(string fileName, string contentType, Stream source, string directoryName)
        {
            if (source != null)
            {
                string fileName2 = EscapeInvalidChars(fileName);

                string fileId = $"{directoryName}/{fileName2}";

                var blob = TemporaryContainer.GetBlobClient(fileId);

                UploadBlobFromStream(blob, fileName, contentType, source);

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
