using System;
using System.IO;
using System.Text;
using System.Web;

namespace Micajah.AzureFileService
{
    /// <summary>
    /// Displays the file content.
    /// </summary>
    public class FileHandler : IHttpHandler
    {
        #region Constants

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

            string userAgent = context.Request.UserAgent != null ? context.Request.UserAgent : string.Empty;

            string[] jpegExtensions = MimeType.GetExtensions(MimeType.Jpeg);

            if (jpegExtensions != null)
            {
                string extension = Path.GetExtension(fileName);

                if (Array.IndexOf(jpegExtensions, extension) == -1)
                {
                    fileName += MimeType.GetExtension(MimeType.Jpeg);
                }
            }

            string contentDisposition;
            if (context.Request.Browser.IsBrowser("IE") || userAgent.Contains("Chrome"))
                contentDisposition = "filename=\"" + fileName.ToHex() + "\";";
            else if (userAgent.Contains("Safari"))
                contentDisposition = "filename=\"" + fileName + "\";";
            else
                contentDisposition = "filename*=utf-8''" + HttpUtility.UrlPathEncode(fileName) + ";";

            context.Response.AddHeader("Content-Disposition", contentDisposition);
        }

        #endregion

        #region Public Methods

        public void ProcessRequest(HttpContext context)
        {
            if (context != null)
            {
                if (context.Request.QueryString["d"] != null)
                {
                    string fileName = null;
                    byte[] bytes = FileManager.GetThumbnail(context.Request.QueryString["d"], out fileName);
                    if (bytes != null)
                    {
                        ConfigureResponse(context, fileName, MimeType.Jpeg);
                        context.Response.BinaryWrite(bytes);
                    }
                }
            }
        }

        #endregion
    }
}