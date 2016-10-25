using System;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Linq;

namespace Micajah.AzureFileService
{
    /// <summary>
    /// The MIME (Multipurpose Internet Mail Extensions) types of a file.
    /// </summary>
    public static class MimeType
    {
        #region Constants

        /// <summary>
        /// image/vnd.microsoft.icon
        /// </summary>
        private const string Icon2 = "image/vnd.microsoft.icon";

        /// <summary>
        /// image/x-icon
        /// </summary>
        public const string Icon = "image/x-icon";

        /// <summary>
        /// application/x-shockwave-flash
        /// </summary>
        public const string Flash = "application/x-shockwave-flash";

        /// <summary>
        /// image/jpeg
        /// </summary>
        public const string Jpeg = "image/jpeg";

        /// <summary>
        /// text/html
        /// </summary>
        public const string Html = "text/html";

        /// <summary>
        /// .swf
        /// </summary>
        public const string SwfExtension = ".swf";

        #endregion

        #region Members

        private static ReadOnlyCollection<string> s_ImageExtensions;
        private static ReadOnlyCollection<string> s_VideoExtensions;

        #endregion

        #region Public Properties

        /// <summary>
        /// The collection of the image files's extensions.
        /// </summary>
        public static ReadOnlyCollection<string> ImageExtensions
        {
            get
            {
                if (s_ImageExtensions == null)
                    s_ImageExtensions = new ReadOnlyCollection<string>(new string[] { ".bmp", ".gif", ".ief", ".jpg", ".pbm", ".png", ".pnm", ".ppm", ".ras", ".rgb", ".tif", ".tiff", ".xbm", ".xpm", ".xwd" });
                return s_ImageExtensions;
            }
        }

        /// <summary>
        /// The collection of the video files's extensions.
        /// </summary>
        public static ReadOnlyCollection<string> VideoExtensions
        {
            get
            {
                if (s_VideoExtensions == null)
                    s_VideoExtensions = new ReadOnlyCollection<string>(new string[] { ".asf", ".avi", ".fli", ".mov", ".movie", ".mp4", ".mpe", ".mpeg", ".mpg", SwfExtension, ".viv", ".vivo", ".wmv" });
                return s_VideoExtensions;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines whether the specified MIME type is flash.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is flash; otherwise, false.</returns>
        public static bool IsFlash(string mimeType)
        {
            return (string.Compare(mimeType, Flash, StringComparison.OrdinalIgnoreCase) == 0);
        }

        /// <summary>
        /// Determines whether the specified MIME type is HTML.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is HTML; otherwise, false.</returns>
        public static bool IsHtml(string mimeType)
        {
            return (string.Compare(mimeType, Html, StringComparison.OrdinalIgnoreCase) == 0);
        }

        /// <summary>
        /// Determines whether the specified MIME type is image type.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is image type; otherwise, false.</returns>
        public static bool IsImageType(string mimeType)
        {
            if (!string.IsNullOrEmpty(mimeType))
                return mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            return false;
        }

        /// <summary>
        /// Determines whether the specified MIME type is video type.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is video type; otherwise, false.</returns>
        public static bool IsVideoType(string mimeType)
        {
            if (!string.IsNullOrEmpty(mimeType))
                return mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
            return false;
        }

        /// <summary>
        /// Determines whether the specified MIME type is icon image.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is icon image; otherwise, false.</returns>
        public static bool IsIcon(string mimeType)
        {
            return ((string.Compare(mimeType, Icon, StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(mimeType, Icon2, StringComparison.OrdinalIgnoreCase) == 0));
        }

        /// <summary>
        /// Returns image format associated to the specified MIME type.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type.</param>
        /// <returns>An image format, if it is found; otherwise null reference.</returns>
        public static ImageFormat GetImageFormat(string mimeType)
        {
            ImageCodecInfo[] imageCodecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo imageCodec = imageCodecs.First(codec => codec.MimeType == mimeType);
            if (imageCodec != null)
            {
                return new ImageFormat(imageCodec.FormatID);
            }

            return null;
        }

        #endregion
    }
}
