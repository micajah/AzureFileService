using System;
using System.IO;
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

        #region Public Methods

        public void ProcessRequest(HttpContext context)
        {
            if (context == null)
            {
                return;
            }

            string d = context.Request.QueryString["d"];

            if (d == null)
            {
                return;
            }

            byte[] content = FileManager.GetThumbnail(d, out string fileName);

            if (content != null)
            {
                string extension = Path.GetExtension(fileName).ToLowerInvariant();

                if (extension != MimeType.PngExtension)
                {
                    fileName += MimeType.PngExtension;
                }

                ResourceHandler.ConfigureResponse(context, fileName, MimeType.Png, DateTime.UtcNow.AddMinutes(Settings.ClientCacheExpiryTime).ToLocalTime());

                context.Response.BinaryWrite(content);
            }
        }

        #endregion
    }
}