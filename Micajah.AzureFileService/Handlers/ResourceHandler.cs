using Micajah.AzureFileService.Properties;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
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

        #region Private Methods

        private static byte[] GetManifestResourceBytes(string resourceName)
        {
            byte[] bytes = null;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    int count = (int)stream.Length;

                    bytes = new byte[] { };
                    Array.Resize<byte>(ref bytes, count);

                    BinaryReader reader = new BinaryReader(stream);
                    reader.Read(bytes, 0, count);
                }
            }
            return bytes;
        }

        #endregion

        #region Internal Methods

        internal static string GetWebResourceName(string resourceName)
        {
            return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0}|{1}", resourceName, Assembly.GetExecutingAssembly().GetName().Version)));
        }

        internal static string GetWebResourceUrlFormat(bool createApplicationAbsoluteUrl)
        {
            return ((createApplicationAbsoluteUrl ? VirtualPathUtility.ToAbsolute(VirtualPath) : VirtualPath) + "?d={0}");
        }

        internal static string GetWebResourceUrl(string resourceName, bool createApplicationAbsoluteUrl)
        {
            return string.Format(CultureInfo.InvariantCulture, GetWebResourceUrlFormat(createApplicationAbsoluteUrl), GetWebResourceName(resourceName));
        }

        #endregion

        #region Public Methods

        public void ProcessRequest(HttpContext context)
        {
            byte[] bytes = null;

            if (context != null)
            {
                if (context.Request.QueryString["d"] != null)
                {
                    byte[] decodedResourceName = HttpServerUtility.UrlTokenDecode(context.Request.QueryString["d"]);

                    if (decodedResourceName != null)
                    {
                        string resourceName = Encoding.UTF8.GetString(decodedResourceName).Split('|')[0];
                        if (!string.IsNullOrEmpty(resourceName))
                        {
                            bytes = GetManifestResourceBytes(ResourceVirtualPathProvider.ManifestResourceNamePrefix + "." + resourceName);

                            if (bytes != null)
                            {
                                context.Response.Clear();
                                context.Response.ContentType = MimeMapping.GetMimeMapping(resourceName);
                                if (bytes.Length > 0)
                                {
                                    context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                                }
                            }
                        }
                    }
                }
            }

            if (bytes == null)
            {
                throw new HttpException(404, Resources.ResourceHandler_InvalidRequest);
            }
        }

        #endregion
    }
}
