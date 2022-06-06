using System;
using System.Text;
using System.Web;

namespace Micajah.AzureFileService
{
    public sealed class ResourceHandler : IHttpHandler
    {
        #region Constants

        internal const string VirtualPath = "~/mafsr.axd";

        #endregion

        #region Public Properties

        public bool IsReusable
        {
            get { return true; }
        }

        #endregion

        #region Internal Methods

        internal static void ConfigureResponse(HttpContext context, string fileName, string contentType, DateTime? expires)
        {
            context.Response.Clear();
            context.Response.ClearHeaders();

            context.Response.Charset = Encoding.UTF8.WebName;
            context.Response.ContentEncoding = Encoding.UTF8;

            string userAgent = context.Request.UserAgent ?? string.Empty;

            string contentDisposition;
            if (context.Request.Browser.IsBrowser("IE") || userAgent.Contains("Chrome"))
                contentDisposition = "filename=\"" + fileName.ToHex() + "\";";
            else if (userAgent.Contains("Safari"))
                contentDisposition = "filename=\"" + fileName + "\";";
            else
                contentDisposition = "filename*=utf-8''" + HttpUtility.UrlPathEncode(fileName) + ";";

            context.Response.AddHeader("Content-Disposition", contentDisposition);

            context.Response.ContentType = contentType;

            if (expires.HasValue)
            {
                context.Response.Cache.SetCacheability(HttpCacheability.Public);
                context.Response.Cache.SetExpires(expires.Value);
            }
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

            byte[] content = null;
            string contentType = null;
            string fileName = null;
            bool cacheable = true;

            ResourceProvider.GetResource(d, ref content, ref contentType, ref fileName, ref cacheable);

            if (content != null)
            {
                DateTime? expires = null;
                if (cacheable)
                {
                    expires = DateTime.UtcNow.AddYears(1);
                }

                ConfigureResponse(context, fileName, contentType, expires);

                context.Response.BinaryWrite(content);
            }
        }

        #endregion
    }
}
