using Micajah.AzureFileService.Properties;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;

namespace Micajah.AzureFileService.Handlers
{
    public sealed class ResourceHandler : IHttpHandler
    {
        #region Members

        internal const string ResourceHandlerVirtualPath = "~/mafs.axd";
        internal const string ManifestResourceNamePrefix = "Micajah.AzureFileService.Resources.Micajah.AzureFileService";

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
            return ((createApplicationAbsoluteUrl ? VirtualPathUtility.ToAbsolute(ResourceHandlerVirtualPath) : ResourceHandlerVirtualPath) + "?d={0}");
        }

        internal static string GetWebResourceUrl(string resourceName, bool createApplicationAbsoluteUrl)
        {
            return string.Format(CultureInfo.InvariantCulture, GetWebResourceUrlFormat(createApplicationAbsoluteUrl), GetWebResourceName(resourceName));
        }

        internal static bool IsWebResourceUrl(string virtualPath)
        {
            return (string.Compare(VirtualPathUtility.ToAppRelative(virtualPath), ResourceHandlerVirtualPath, StringComparison.OrdinalIgnoreCase) == 0);
        }

        #endregion

        #region Public Methods

        public void ProcessRequest(HttpContext context)
        {
            if (context == null)
                return;

            byte[] bytes = null;

            if (IsWebResourceUrl(context.Request.FilePath) && (context.Request.QueryString["d"] != null))
            {
                byte[] decodedResourceName = null;
                try
                {
                    decodedResourceName = HttpServerUtility.UrlTokenDecode(context.Request.QueryString["d"]);
                }
                catch (FormatException) { }

                if (decodedResourceName != null)
                {
                    string resourceName = Encoding.UTF8.GetString(decodedResourceName).Split('|')[0];
                    if (!string.IsNullOrEmpty(resourceName))
                    {
                        bytes = GetManifestResourceBytes(ManifestResourceNamePrefix + "." + resourceName);

                        if (bytes != null)
                        {
                            context.Response.Clear();
                            context.Response.ContentType = MimeMapping.GetMimeMapping(resourceName);
                            if (bytes.Length > 0) context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
            }

            if (bytes == null)
                throw new HttpException(404, Resources.ResourceHandler_InvalidRequest);
        }

        #endregion
    }
}
