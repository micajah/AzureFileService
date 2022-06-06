using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;

namespace Micajah.AzureFileService
{
    /// <summary>
    /// The class provides the methods to work with resources.
    /// </summary>
    public static class ResourceProvider
    {
        #region Constants

        internal const string FileListPageVirtualPath = ResourceVirtualPathProvider.VirtualRootShortPath + "FileList.aspx";

        internal const string BootstrapIconsStyleSheet = "Styles.bootstrap-icons.css";
        internal const string DropzoneStyleSheet = "Styles.dropzone.css";
        internal const string FileListStyleSheet = "Styles.FileList.css";
        internal const string OpentipStyleSheet = "Styles.opentip.css";

        internal const string DropzoneScript = "Scripts.dropzone.js";
        internal const string FileListScript = "Scripts.FileList.js";
        internal const string OpentipScript = "Scripts.opentip.js";

        #endregion

        #region Private Methods

        private static void GetEmbeddedResource(string resourceName, ref byte[] content, ref string contentType, ref string name, ref bool cacheable)
        {
            if (string.IsNullOrEmpty(resourceName)) return;

            string[] parts = resourceName.Split('?');
            resourceName = parts[0];

            name = GetResourceFileName(resourceName);
            contentType = MimeType.GetMimeType(resourceName);
            cacheable = true;

            resourceName = ResourceVirtualPathProvider.ManifestResourceNamePrefix + "." + resourceName;

            if (resourceName.EndsWith(BootstrapIconsStyleSheet, StringComparison.OrdinalIgnoreCase))
            {
                content = Encoding.UTF8.GetBytes(ProcessBootstrapIconsStyleSheet(GetManifestResourceString(resourceName)));
            }
            else
            {
                content = GetManifestResourceBytes(resourceName);
            }
        }

        private static string GetResourceName(string resourceName)
        {
            return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0}|{1}", resourceName, Assembly.GetExecutingAssembly().GetName().Version)));
        }

        private static string GetResourceUrlFormat(bool createApplicationAbsoluteUrl)
        {
            return (createApplicationAbsoluteUrl ? VirtualPathUtility.ToAbsolute(ResourceHandler.VirtualPath) : ResourceHandler.VirtualPath) + "?d={0}";
        }

        private static byte[] GetManifestResourceBytes(string resourceName)
        {
            byte[] bytes = null;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    int length = (int)stream.Length;
                    bytes = new byte[length];
                    stream.Read(bytes, 0, length);
                }
            }
            return bytes;
        }

        private static string GetManifestResourceString(string resourceName)
        {
            string content = null;
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    content = streamReader.ReadToEnd();
                }
            }
            return content;
        }

        private static string ProcessStyleSheet(string styleSheetContent, string[] keyNames, string resourceNameFormat)
        {
            if ((!string.IsNullOrEmpty(styleSheetContent)) && (keyNames != null))
            {
                StringBuilder sb = new StringBuilder(styleSheetContent);

                foreach (string keyName in keyNames)
                {
                    sb.Replace("$" + keyName + "$", GetResourceUrl(string.Format(CultureInfo.InvariantCulture, resourceNameFormat, keyName), true));
                }

                return sb.ToString();
            }
            return styleSheetContent;
        }

        private static string ProcessBootstrapIconsStyleSheet(string styleSheetContent)
        {
            return ProcessStyleSheet(styleSheetContent
                , new string[] { "bootstrap-icons.woff2", "bootstrap-icons.woff" }
                , "Fonts.{0}");
        }

        #endregion

        #region Internal Methods

        internal static string GetResourceFileName(string resourceName)
        {
            string[] parts = resourceName.Split('.');
            if (parts.Length < 2)
                return null;
            return string.Join(".", new string[] { parts[parts.Length - 2], parts[parts.Length - 1] });
        }

        internal static string GetResourceUrl(string resourceName, bool createApplicationAbsoluteUrl)
        {
            return string.Format(CultureInfo.InvariantCulture, GetResourceUrlFormat(createApplicationAbsoluteUrl), GetResourceName(resourceName));
        }

        internal static void GetResource(string resourceName, ref byte[] content, ref string contentType, ref string name, ref bool cacheable)
        {
            byte[] decodedResourceNameBytes = null;

            try
            {
                decodedResourceNameBytes = HttpServerUtility.UrlTokenDecode(resourceName);
            }
            catch (FormatException) { }

            if (decodedResourceNameBytes != null)
            {
                string[] parts = Encoding.UTF8.GetString(decodedResourceNameBytes).Split('|');
                string decodedResourceName = parts[0];

                GetEmbeddedResource(decodedResourceName, ref content, ref contentType, ref name, ref cacheable);
            }
        }

        #endregion
    }
}
