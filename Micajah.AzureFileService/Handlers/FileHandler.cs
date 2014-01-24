using System;
using System.Globalization;
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
                contentDisposition = "filename=\"" + fileName.ToHex() + "\";";
            else if (context.Request.UserAgent.Contains("Safari"))
                contentDisposition = "filename=\"" + fileName + "\";";
            else
                contentDisposition = "filename*=utf-8''" + HttpUtility.UrlPathEncode(fileName) + ";";

            context.Response.AddHeader("Content-Disposition", contentDisposition);
        }

        // TODO: Display HTML with object tag for Flash files.
        //private static string GetObjectTag(int width, int height, string fileUrl, string fileName)
        //{
        //    string widthAttribute = string.Empty;
        //    if (width > 0) widthAttribute = " width=\"" + width + "\"";
        //    string heightAttribute = string.Empty;
        //    if (height > 0) heightAttribute = " height=\"" + height + "\"";
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendFormat(CultureInfo.InvariantCulture, HtmlObjectFormat, widthAttribute, heightAttribute, fileUrl, fileName);
        //    return sb.ToString();
        //}

        #endregion

        #region Public Methods

        public void ProcessRequest(HttpContext context)
        {
            if (context != null)
            {
                if (context.Request.QueryString["d"] != null)
                {
                    byte[] decodedVars = HttpServerUtility.UrlTokenDecode(context.Request.QueryString["d"]);
                    if (decodedVars != null)
                    {
                        string[] parts = Encoding.UTF8.GetString(decodedVars).Split('|');

                        string fileId = parts[0];
                        string fileName = FileManager.GetNameFromFileId(fileId);
                        int width = Convert.ToInt32(parts[1], CultureInfo.InvariantCulture);
                        int height = Convert.ToInt32(parts[2], CultureInfo.InvariantCulture);
                        int align = Convert.ToInt32(parts[3], CultureInfo.InvariantCulture);
                        string containerUriWithSas = parts[4];

                        FileManager manager = new FileManager(fileId, containerUriWithSas);
                        byte[] bytes = manager.GetThumbnail(fileId, fileName, width, height, align);
                        if (bytes != null)
                        {
                            ConfigureResponse(context, fileName, MimeType.Jpeg);
                            context.Response.BinaryWrite(bytes);
                        }
                    }
                }
            }
        }

        #endregion
    }
}