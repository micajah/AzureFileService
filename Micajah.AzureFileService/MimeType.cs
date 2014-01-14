using System;
using System.Collections.ObjectModel;

namespace Micajah.AzureFileService
{
    /// <summary>
    /// The MIME (Multipurpose Internet Mail Extensions) types of a file.
    /// </summary>
    public static class MimeType
    {
        #region Constants

        public const string Flash = "application/x-shockwave-flash";
        public const string Jpeg = "image/jpeg";
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

        #endregion
    }
}
