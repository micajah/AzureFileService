using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Globalization;
using System.IO;

namespace Micajah.AzureFileService
{
    public static class Client
    {
        internal static byte[] GetThumbnail(string fileName, int width, int height, int align, string containerName, string objectId, string objectType, string storageConnectionString)
        {
            byte[] bytes = null;

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = client.GetContainerReference(containerName);
            container.CreateIfNotExists();

            string thumbBlobName = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}x{3}x{4}/{5}", objectType, objectId, width, height, align, fileName);
            CloudBlockBlob thumbBlob = container.GetBlockBlobReference(thumbBlobName);

            if (thumbBlob.Exists())
            {
                long length = thumbBlob.Properties.Length;
                bytes = new byte[length];
                thumbBlob.DownloadToByteArray(bytes, 0);
            }
            else
            {
                string blobName = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}", objectType, objectId, fileName);
                CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

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
    }
}
