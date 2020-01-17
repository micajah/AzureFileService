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
        /// application/pdf
        /// </summary>
        public const string Pdf = "application/pdf";

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
                {
                    s_ImageExtensions = new ReadOnlyCollection<string>(new string[] {
                        ".art", ".bmp", ".cod", ".cmx", ".gif", ".ico", ".ief", ".jfif", ".jpg", ".mac",
                        ".pbm", ".pgm", ".pic", ".png", ".pnm", ".pnt", ".ppm", ".qti", ".ras", ".rgb", ".rf",
                        ".tif", ".tiff", ".wbmp", ".wdp", ".xbm", ".xpm", ".xwd"
                    });
                }
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

        #region Private Methods

        private static string GetAudioExtension(string mimeType)
        {
            switch (mimeType)
            {
                case "AUDIO/X-AIFF":
                    return ".aif";
                case "AUDIO/BASIC":
                    return ".snd";
                case "AUDIO/MIDI":
                    return ".mid";
                case "AUDIO/MPEG":
                    return ".MP3";
                case "AUDIO/X-REALAUDIO":
                    return ".ra";
                case "AUDIO/X-PN-REALAUDIO":
                    return ".ram";
                case "AUDIO/X-PN-REALAUDIO-PLUGIN":
                    return ".rpm";
                case "AUDIO/TSP-AUDIO":
                    return ".tsi";
                case "AUDIO/X-WAV":
                    return ".wav";
            }
            return null;
        }

        private static string GetApplicationExtension(string mimeType)
        {
            switch (mimeType)
            {
                case "APPLICATION/POSTSCRIPT":
                    return ".ps";
                case "APPLICATION/X-BCPIO":
                    return ".bcpio";
                case "APPLICATION/OCTET-STREAM":
                    return ".bin";
                case "APPLICATION/CLARISCAD":
                    return ".ccad";
                case "APPLICATION/X-NETCDF":
                    return ".cdf";
                case "APPLICATION/X-CPIO":
                    return ".cpio";
                case "APPLICATION/MAC-COMPACTPRO":
                    return ".cpt";
                case "APPLICATION/X-CSH":
                    return ".csh";
                case "APPLICATION/X-DIRECTOR":
                    return ".dir";
                case "APPLICATION/DRAFTING":
                    return ".drw";
                case "APPLICATION/X-DVI":
                    return ".dvi";
                case "APPLICATION/ACAD":
                    return ".dwg";
                case "APPLICATION/DXF":
                    return ".dxf";
                case "APPLICATION/ANDREW-INSET":
                    return ".ez";
                case "APPLICATION/X-GTAR":
                    return ".gtar";
                case "APPLICATION/X-GZIP":
                    return ".gz";
                case "APPLICATION/X-HDF":
                    return ".hdf";
                case "APPLICATION/MAC-BINHEX40":
                    return ".hqx";
                case "APPLICATION/X-IPSCRIPT":
                    return ".ips";
                case "APPLICATION/X-IPIX":
                    return ".ipx";
                case "APPLICATION/X-JAVASCRIPT":
                    return ".js";
                case "APPLICATION/X-LATEX":
                    return ".latex";
                case "APPLICATION/X-LISP":
                    return ".lsp";
                case "APPLICATION/X-TROFF-MAN":
                    return ".man";
                case "APPLICATION/X-TROFF-ME":
                    return ".me";
                case "APPLICATION/VND.MIF":
                    return ".mif";
                case "APPLICATION/X-TROFF-MS":
                    return ".ms";
                case "APPLICATION/ODA":
                    return ".oda";
                case "APPLICATION/PDF":
                    return ".pdf";
                case "APPLICATION/X-CHESS-PGN":
                    return ".pgn";
                case "APPLICATION/X-FREELANCE":
                    return ".pre";
                case "APPLICATION/PRO_ENG":
                    return ".prt";
                case "APPLICATION/X-TROFF":
                    return ".roff";
                case "APPLICATION/X-LOTUSSCREENCAM":
                    return ".scm";
                case "APPLICATION/SET":
                    return ".set";
                case "APPLICATION/X-SH":
                    return ".sh";
                case "APPLICATION/X-SILVERLIGHT":
                    return ".xap";
                case "APPLICATION/X-SHAR":
                    return ".shar";
                case "APPLICATION/X-STUFFIT":
                    return ".sit";
                case "APPLICATION/X-KOAN":
                    return ".skd";
                case "APPLICATION/SMIL":
                    return ".smi";
                case "APPLICATION/SOLIDS":
                    return ".sol";
                case "APPLICATION/X-FUTURESPLASH":
                    return ".spl";
                case "APPLICATION/X-WAIS-SOURCE":
                    return ".src";
                case "APPLICATION/STEP":
                    return ".stp";
                case "APPLICATION/SLA":
                    return ".stl";
                case "APPLICATION/X-SV4CPIO":
                    return ".sv4cpio";
                case "APPLICATION/X-SV4CRC":
                    return ".sv4crc";
                case "APPLICATION/X-SHOCKWAVE-FLASH":
                    return ".swf";
                case "APPLICATION/X-TAR":
                    return ".tar";
                case "APPLICATION/X-TCL":
                    return ".tcl";
                case "APPLICATION/X-TEX":
                    return ".tex";
                case "APPLICATION/X-TEXINFO":
                    return ".texi";
                case "APPLICATION/X-ZIP-COMPRESSED":
                    return ".zip";
                case "APPLICATION/X-ZIP":
                    return ".zip";
                case "APPLICATION/DSPTYPE":
                    return ".tsp";
                case "APPLICATION/I-DEAS":
                    return ".unv";
                case "APPLICATION/X-USTAR":
                    return ".ustar";
                case "APPLICATION/X-CDLINK":
                    return ".vcd";
                case "APPLICATION/VDA":
                    return ".vda";
                case "APPLICATION/ZIP":
                    return ".zip";
            }
            return GetMicrosoftOfficeExtension(mimeType);
        }

        private static string GetMicrosoftOfficeExtension(string mimeType)
        {
            switch (mimeType)
            {
                case "APPLICATION/MSWORD":
                    return ".doc";
                case "APPLICATION/MSPOWERPOINT":
                    return ".ppt";
                case "APPLICATION/VND.MS-POWERPOINT":
                    return ".ppt";
                case "APPLICATION/VND.MS-EXCEL":
                    return ".xls";
            }
            return GetMicrosoftOffice2007Extension(mimeType);
        }

        private static string GetMicrosoftOffice2007Extension(string mimeType)
        {
            switch (mimeType)
            {
                case "APPLICATION/VND.OPENXMLFORMATS-OFFICEDOCUMENT.WORDPROCESSINGML.DOCUMENT":
                    return ".docx";
                case "APPLICATION/VND.MS-WORD.DOCUMENT.MACROENABLED.12":
                    return ".docm";
                case "APPLICATION/VND.OPENXMLFORMATS-OFFICEDOCUMENT.WORDPROCESSINGML.TEMPLATE":
                    return ".dotx";
                case "APPLICATION/VND.MS-WORD.TEMPLATE.MACROENABLED.12":
                    return ".dotm";
                case "APPLICATION/VND.OPENXMLFORMATS-OFFICEDOCUMENT.SPREADSHEETML.SHEET":
                    return ".xlsx";
                case "APPLICATION/VND.MS-EXCEL.SHEET.MACROENABLED.12":
                    return ".xlsm";
                case "APPLICATION/VND.OPENXMLFORMATS-OFFICEDOCUMENT.SPREADSHEETML.TEMPLATE":
                    return ".xltx";
                case "APPLICATION/VND.MS-EXCEL.TEMPLATE.MACROENABLED.12":
                    return ".xltm";
                case "APPLICATION/VND.MS-EXCEL.SHEET.BINARY.MACROENABLED.12":
                    return ".xlsb";
                case "APPLICATION/VND.MS-EXCEL.ADDIN.MACROENABLED.12":
                    return ".xlam";
                case "APPLICATION/VND.OPENXMLFORMATS-OFFICEDOCUMENT.PRESENTATIONML.PRESENTATION":
                    return ".pptx";
                case "APPLICATION/VND.MS-POWERPOINT.PRESENTATION.MACROENABLED.12":
                    return ".pptm";
                case "APPLICATION/VND.OPENXMLFORMATS-OFFICEDOCUMENT.PRESENTATIONML.SLIDESHOW":
                    return ".ppsx";
                case "APPLICATION/VND.MS-POWERPOINT.SLIDESHOW.MACROENABLED.12":
                    return ".ppsm";
                case "APPLICATION/VND.OPENXMLFORMATS-OFFICEDOCUMENT.PRESENTATIONML.TEMPLATE":
                    return ".potx";
                case "APPLICATION/VND.MS-POWERPOINT.TEMPLATE.MACROENABLED.12":
                    return ".potm";
                case "APPLICATION/VND.MS-POWERPOINT.ADDIN.MACROENABLED.12":
                    return ".ppam";
                case "APPLICATION/VND.OPENXMLFORMATS-OFFICEDOCUMENT.PRESENTATIONML.SLIDE":
                    return ".sldx";
                case "APPLICATION/VND.MS-POWERPOINT.SLIDE.MACROENABLED.12":
                    return ".sldm";
                case "APPLICATION/VND.MS-OFFICETHEME":
                    return ".thmx";
                case "APPLICATION/ONENOTE":
                    return ".onetoc";
            }
            return null;
        }

        private static string GetImageExtension(string mimeType)
        {
            switch (mimeType)
            {
                case "IMAGE/BMP":
                    return ".bmp";
                case "IMAGE/CIS-COD":
                    return ".cod";
                case "IMAGE/GIF":
                    return ".gif";
                case "IMAGE/IEF":
                    return ".ief";
                case "IMAGE/JPEG":
                    return ".jpg";
                case "IMAGE/PJPEG":
                    return ".jfif";
                case "IMAGE/X-CMX":
                    return ".cmx";
                case "IMAGE/X-JG":
                    return ".art";
                case "IMAGE/X-ICON":
                    return ".ico";
                case "IMAGE/X-MACPAINT":
                    return ".mac";
                case "IMAGE/X-PORTABLE-BITMAP":
                    return ".pbm";
                case "IMAGE/X-PORTABLE-GRAYMAP":
                    return ".pgm";
                case "IMAGE/PICT":
                    return ".pic";
                case "IMAGE/PNG":
                    return ".png";
                case "IMAGE/X-PORTABLE-ANYMAP":
                    return ".pnm";
                case "IMAGE/X-PORTABLE-PIXMAP":
                    return ".ppm";
                case "IMAGE/X-QUICKTIME":
                    return ".qti";
                case "IMAGE/CMU-RASTER":
                    return ".ras";
                case "IMAGE/VND.RN-REALFLASH":
                    return ".rf";
                case "IMAGE/X-RGB":
                    return ".rgb";
                case "IMAGE/TIFF":
                    return ".tif";
                case "IMAGE/VND.WAP.WBMP":
                    return ".wbmp";
                case "IMAGE/VND.MS-PHOTO":
                    return ".wdp";
                case "IMAGE/X-XBITMAP":
                    return ".xbm";
                case "IMAGE/X-XPIXMAP":
                    return ".xpm";
                case "IMAGE/X-XWINDOWDUMP":
                    return ".xwd";
            }
            return null;
        }

        private static string GetMarkupExtension(string mimeType)
        {
            switch (mimeType)
            {
                case "TEXT/CSS":
                    return ".css";
                case "TEXT/X-SETEXT":
                    return ".etx";
                case "TEXT/HTML":
                    return ".htm";
                case "TEXT/SGML":
                    return ".sgm";
            }
            return null;
        }

        private static string GetTextExtension(string mimeType)
        {
            switch (mimeType)
            {
                case "TEXT/CSV":
                    return ".csv";
                case "TEXT/PLAIN":
                    return ".txt";
                case "TEXT/RTF":
                    return ".rtf";
                case "TEXT/RICHTEXT":
                    return ".rtx";
                case "TEXT/TAB-SEPARATED-VALUES":
                    return ".tsv";
                case "TEXT/XML":
                    return ".xml";
            }
            return null;
        }

        private static string GetVideoExtension(string mimeType)
        {
            switch (mimeType)
            {
                case "VIDEO/X-MSVIDEO":
                    return ".avi";
                case "VIDEO/X-FLI":
                    return ".fli";
                case "VIDEO/QUICKTIME":
                    return ".qt";
                case "VIDEO/X-SGI-MOVIE":
                    return ".movie";
                case "VIDEO/MP4":
                    return ".mp4";
                case "VIDEO/MPEG":
                    return ".mpg";
                case "VIDEO/VND.VIVO":
                    return ".viv";
                case "VIDEO/X-MS-ASF":
                    return ".asf";
                case "VIDEO/X-MS-WMV":
                    return ".wmv";
            }
            return null;
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
        /// Determines whether the specified MIME type is PDF.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is PDF; otherwise, false.</returns>
        public static bool IsPdf(string mimeType)
        {
            return (string.Compare(mimeType, Pdf, StringComparison.OrdinalIgnoreCase) == 0);
        }

        /// <summary>
        /// Determines whether the specified MIME type is Microsoft Office.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is Microsoft Office; otherwise, false.</returns>
        public static bool IsMicrosoftOffice(string mimeType)
        {
            if (!string.IsNullOrEmpty(mimeType))
            {
                string extension = GetMicrosoftOfficeExtension(mimeType.ToUpperInvariant());

                return !string.IsNullOrEmpty(extension);
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified MIME type is text.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is text; otherwise, false.</returns>
        public static bool IsText(string mimeType)
        {
            if (!string.IsNullOrEmpty(mimeType))
            {
                string extension = GetTextExtension(mimeType.ToUpperInvariant());

                return !string.IsNullOrEmpty(extension);
            }

            return false;
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

        /// <summary>
        /// Returns a file extension by specified MIME type.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type.</param>
        /// <returns>The file extension for the MIME type or empty string, if the MIME type is not found.</returns>
        public static string GetFileExtension(string mimeType)
        {
            if (mimeType == null) return null;

            mimeType = mimeType.ToUpperInvariant();
            string extension = GetApplicationExtension(mimeType);
            if (extension == null)
            {
                extension = GetImageExtension(mimeType);
                if (extension == null)
                {
                    extension = GetAudioExtension(mimeType);
                    if (extension == null)
                    {
                        extension = GetVideoExtension(mimeType);
                        if (extension == null)
                        {
                            extension = GetMarkupExtension(mimeType);
                            if (extension == null)
                            {
                                extension = GetTextExtension(mimeType);
                                if (extension == null)
                                {
                                    switch (mimeType)
                                    {
                                        case "CHEMICAL/X-PDB":
                                            return ".pdb";
                                        case "MODEL/IGES":
                                            return ".igs";
                                        case "MODEL/MESH":
                                            return ".msh";
                                        case "MODEL/VRML":
                                            return ".vrml";
                                        case "WWW/MIME":
                                            return ".mime";
                                        case "X-CONFERENCE/X-COOLTALK":
                                            return ".ice";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ((extension == null) ? string.Empty : extension);
        }

        #endregion
    }
}
