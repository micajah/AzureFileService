using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace Micajah.AzureFileService
{
    /// <summary>
    /// Displays the file content.
    /// </summary>
    public class FileHandler : IHttpHandler, IRequiresSessionState
    {
        #region Constants

        private const string HtmlObjectFormat = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <title>{3}</title>
</head>
<body>
    <object classid=""clsid:D27CDB6E-AE6D-11cf-96B8-444553540000"" codebase=""https://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=5,0,0,0""{0}{1}>
        <param name=""movie"" value=""{2}&Flash=1"">
        <param name=""quality"" value=""high"">
        <embed src=""{2}&Flash=1"" quality=""high""{0}{1} type=""application/x-shockwave-flash"" pluginspage=""https://www.macromedia.com/shockwave/download/index.cgi?P1_Prod_Version=ShockwaveFlash"">
    </object>
</body>
</html>";

        internal const string VirtualPath = "~/mafsf.axd";

        #endregion

        #region Public Properties

        public bool IsReusable
        {
            get { return true; }
        }

        #endregion

        #region Private Methods

        private static void ConfigureResponse(HttpContext context, string fileName, string contentType)
        {
            context.Response.Clear();
            context.Response.ClearHeaders();
            context.Response.Charset = Encoding.UTF8.WebName;
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(Settings.ClientCacheExpiryTime).ToLocalTime());
            context.Response.ContentType = contentType;

            string contentDisposition = string.Empty;
            if (context.Request.Browser.IsBrowser("IE") || context.Request.UserAgent.Contains("Chrome"))
                contentDisposition = "filename=\"" + Helper.ToHexString(fileName) + "\";";
            else if (context.Request.UserAgent.Contains("Safari"))
                contentDisposition = "filename=\"" + fileName + "\";";
            else
                contentDisposition = "filename*=utf-8''" + HttpUtility.UrlPathEncode(fileName) + ";";

            context.Response.AddHeader("Content-Disposition", contentDisposition);
        }

        private static string GetObjectTag(int width, int height, string fileUrl, string fileName)
        {
            string widthAttribute = string.Empty;
            if (width > 0) widthAttribute = " width=\"" + width + "\"";
            string heightAttribute = string.Empty;
            if (height > 0) heightAttribute = " height=\"" + height + "\"";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, HtmlObjectFormat, widthAttribute, heightAttribute, fileUrl, fileName);
            return sb.ToString();
        }

        #endregion

        #region Internal Methods

        internal static string GetThumbnailUrl(string fileName, int width, int height, int align, string propertyTableId, bool createApplicationAbsoluteUrl)
        {
            return string.Format(CultureInfo.InvariantCulture
                , ((createApplicationAbsoluteUrl ? VirtualPathUtility.ToAbsolute(VirtualPath) : VirtualPath) + "?d={0}")
                , HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}|{3}|{4}|{5}"
                    , fileName, width, height, align, propertyTableId, Assembly.GetExecutingAssembly().GetName().Version))));
        }

        #endregion

        #region Public Methods

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.QueryString["d"] != null)
            {
                byte[] decodedVars = HttpServerUtility.UrlTokenDecode(context.Request.QueryString["d"]);

                if (decodedVars != null)
                {
                    string[] parts = Encoding.UTF8.GetString(decodedVars).Split('|');
                    string propertyTableId = parts[4];

                    Hashtable properties = context.Session[propertyTableId] as Hashtable;
                    if (properties != null)
                    {
                        string fileName = parts[0];
                        int width = Convert.ToInt32(parts[1], CultureInfo.InvariantCulture);
                        int height = Convert.ToInt32(parts[2], CultureInfo.InvariantCulture);
                        int align = Convert.ToInt32(parts[3], CultureInfo.InvariantCulture);

                        string containerName = (string)properties["ContainerName"];
                        string objectId = (string)properties["ObjectId"];
                        string objectType = (string)properties["ObjectType"];
                        string storageConnectionString = (string)properties["StorageConnectionString"];

                        // TODO: Separate the logic (from there and controls also) to a BL class.
                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

                        CloudBlobClient client = storageAccount.CreateCloudBlobClient();

                        CloudBlobContainer container = client.GetContainerReference(containerName);
                        container.CreateIfNotExists();

                        string thumbBlobName = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}x{3}x{4}/{5}", objectType, objectId, width, height, align, fileName);
                        CloudBlockBlob thumbBlob = container.GetBlockBlobReference(thumbBlobName);

                        if (thumbBlob.Exists())
                        {
                            ConfigureResponse(context, fileName, thumbBlob.Properties.ContentType);

                            thumbBlob.DownloadToStream(context.Response.OutputStream);
                        }
                        else
                        {
                            string blobName = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}", objectType, objectId, fileName);
                            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

                            if (blob.Exists())
                            {
                                MemoryStream stream = new MemoryStream();
                                blob.DownloadToStream(stream);
                                stream.Position = 0;

                                Stream thumb = Thumbnail.Create(stream, width, height, align);

                                thumbBlob.Properties.ContentType = MimeType.Jpeg;
                                thumbBlob.Properties.CacheControl = Settings.ClientCacheControl;
                                thumbBlob.UploadFromStream(thumb);

                                ConfigureResponse(context, fileName, thumbBlob.Properties.ContentType);

                                long length = thumb.Length;
                                BinaryReader reader = new BinaryReader(thumb);
                                byte[] bytes = reader.ReadBytes((int)length);
                                context.Response.BinaryWrite(bytes);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}