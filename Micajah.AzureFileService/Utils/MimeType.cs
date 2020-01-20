using System;
using System.Collections.Generic;
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

        private static ReadOnlyCollection<string> s_ArchiveExtensions;
        private static ReadOnlyCollection<string> s_AudioExtensions;
        private static ReadOnlyCollection<string> s_DocumentExtensions;
        private static ReadOnlyCollection<string> s_ImageExtensions;
        private static ReadOnlyCollection<string> s_MessageExtensions;
        private static ReadOnlyCollection<string> s_TextExtensions;
        private static ReadOnlyCollection<string> s_VideoExtensions;

        #endregion

        #region Public Properties

        /// <summary>
        /// The collection of the archive files extensions.
        /// </summary>
        public static ReadOnlyCollection<string> ArchiveExtensions
        {
            get
            {
                if (s_ArchiveExtensions == null)
                {
                    s_ArchiveExtensions = new ReadOnlyCollection<string>(new string[] {
                        ".7z", ".gtar", ".gz", ".rar", ".tar", ".zip"
                    });
                }

                return s_ArchiveExtensions;
            }
        }

        /// <summary>
        /// The collection of the audio files extensions.
        /// </summary>
        public static ReadOnlyCollection<string> AudioExtensions
        {
            get
            {
                if (s_AudioExtensions == null)
                {
                    s_AudioExtensions = new ReadOnlyCollection<string>(new string[] {
                        ".aif", ".aifc", ".aiff", ".au", ".mid", ".midi", ".mp3", ".ogg", ".ra", ".ram", ".rpm", ".snd", ".tsi", ".wav"
                    });
                }

                return s_AudioExtensions;
            }
        }

        /// <summary>
        /// The collection of the text files extensions.
        /// </summary>
        public static ReadOnlyCollection<string> DocumentExtensions
        {
            get
            {
                if (s_DocumentExtensions == null)
                {
                    s_DocumentExtensions = new ReadOnlyCollection<string>(new string[] {
                        ".pdf", ".doc", ".docx", ".dotx", ".dotm", ".mpp", ".ppt", ".ppt", ".pptx", ".pptm", ".ppsx", ".ppsm", ".potx", ".potm", ".ppam", ".sldx", ".sldm", ".thmx", ".onetoc", ".xls", ".xlsx", ".xlsm", ".xltx", ".xltm", ".xlsb", ".xlam"
                    });
                }

                return s_DocumentExtensions;
            }
        }

        /// <summary>
        /// The collection of the image files extensions.
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
        /// The collection of the message files extensions.
        /// </summary>
        public static ReadOnlyCollection<string> MessageExtensions
        {
            get
            {
                if (s_MessageExtensions == null)
                {
                    s_MessageExtensions = new ReadOnlyCollection<string>(new string[] {
                        ".eml", ".msg"
                    });
                }

                return s_MessageExtensions;
            }
        }

        /// <summary>
        /// The collection of the text files extensions.
        /// </summary>
        public static ReadOnlyCollection<string> TextExtensions
        {
            get
            {
                if (s_TextExtensions == null)
                {
                    s_TextExtensions = new ReadOnlyCollection<string>(new string[] {
                        ".csv", ".txt", ".rtf", ".tsv", ".xml"
                    });
                }

                return s_TextExtensions;
            }
        }

        /// <summary>
        /// The collection of the video files extensions.
        /// </summary>
        public static ReadOnlyCollection<string> VideoExtensions
        {
            get
            {
                if (s_VideoExtensions == null)
                {
                    s_VideoExtensions = new ReadOnlyCollection<string>(new string[] {
                        ".asf", ".avi", ".fli", ".flv", ".mov", ".movie", ".mp4", ".mpe", ".mpeg", ".mpg", SwfExtension, ".viv", ".vivo", ".vob", ".wmv"
                    });
                }

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
                case "AUDIO/MIDI":
                    return ".mid";
                case "AUDIO/MPEG":
                    return ".mp3";
                case "AUDIO/ogg":
                    return ".ogg";
                case "AUDIO/X-REALAUDIO":
                    return ".ra";
                case "AUDIO/X-PN-REALAUDIO":
                    return ".ram";
                case "AUDIO/X-PN-REALAUDIO-PLUGIN":
                    return ".rpm";
                case "AUDIO/BASIC":
                    return ".snd";
                case "AUDIO/TSP-AUDIO":
                    return ".tsi";
                case "AUDIO/X-WAV":
                    return ".wav";
            }
            return null;
        }

        private static string GetArchiveExtension(string mimeType)
        {
            switch (mimeType)
            {
                case "APPLICATION/X-7Z-COMPRESSED":
                    return ".7z";
                case "APPLICATION/X-GTAR":
                    return ".gtar";
                case "APPLICATION/X-GZIP":
                    return ".gz";
                case "APPLICATION/X-TAR":
                    return ".tar";
                case "APPLICATION/X-ZIP-COMPRESSED":
                    return ".zip";
                case "APPLICATION/X-ZIP":
                    return ".zip";
                case "APPLICATION/ZIP":
                    return ".zip";
                case "APPLICATION/X-RAR-COMPRESSED":
                    return ".rar";
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
                    return SwfExtension;
                case "APPLICATION/X-TCL":
                    return ".tcl";
                case "APPLICATION/X-TEX":
                    return ".tex";
                case "APPLICATION/X-TEXINFO":
                    return ".texi";
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
            }
            return null;
        }

        private static string GetMessageExtension(string mimeType)
        {
            switch (mimeType)
            {
                case "MESSAGE/RFC822":
                    return ".eml";
                case "APPLICATION/VND.MS-OUTLOOK":
                    return ".msg";
            }
            return null;
        }

        private static string GetMicrosoftWordExtension(string mimeType)
        {
            switch (mimeType)
            {
                case "APPLICATION/MSWORD":
                    return ".doc";
                case "APPLICATION/VND.OPENXMLFORMATS-OFFICEDOCUMENT.WORDPROCESSINGML.DOCUMENT":
                    return ".docx";
                case "APPLICATION/VND.MS-WORD.DOCUMENT.MACROENABLED.12":
                    return ".docm";
                case "APPLICATION/VND.OPENXMLFORMATS-OFFICEDOCUMENT.WORDPROCESSINGML.TEMPLATE":
                    return ".dotx";
                case "APPLICATION/VND.MS-WORD.TEMPLATE.MACROENABLED.12":
                    return ".dotm";
            }
            return null;
        }

        private static string GetMicrosoftExcelExtension(string mimeType)
        {
            switch (mimeType)
            {
                case "APPLICATION/VND.MS-EXCEL":
                    return ".xls";
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
            }
            return null;
        }

        private static string GetMicrosoftPowerPointExtension(string mimeType)
        {
            switch (mimeType)
            {
                case "APPLICATION/MSPOWERPOINT":
                    return ".ppt";
                case "APPLICATION/VND.MS-POWERPOINT":
                    return ".ppt";
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
            }
            return null;
        }

        private static string GetMicrosoftOfficeExtension(string mimeType)
        {
            string extension = GetMicrosoftWordExtension(mimeType) ?? GetMicrosoftExcelExtension(mimeType) ?? GetMicrosoftPowerPointExtension(mimeType);

            if (extension == null)
            {
                switch (mimeType)
                {
                    case "APPLICATION/VND.MS-OFFICETHEME":
                        return ".thmx";
                    case "APPLICATION/VND.MS-PROJECT":
                        return ".mpp";
                    case "APPLICATION/ONENOTE":
                        return ".onetoc";
                }
            }

            return extension;
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
                case "VIDEO/X-MS-VOB":
                    return ".vob";
                case "VIDEO/X-MS-ASF":
                    return ".asf";
                case "VIDEO/X-MS-WMV":
                    return ".wmv";
                case "VIDEO/X-FLV":
                    return ".flv";
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
        /// Determines whether the specified MIME type is Microsoft Office document.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is Microsoft Office document; otherwise, false.</returns>
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
        /// Determines whether the specified MIME type is PDF or Microsoft Office document.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is PDF or Microsoft Office document; otherwise, false.</returns>
        public static bool IsDocument(string mimeType)
        {
            return IsPdf(mimeType) || IsMicrosoftOffice(mimeType);
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
        /// Determines whether the specified MIME type is archive.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is archive; otherwise, false.</returns>
        public static bool IsArchive(string mimeType)
        {
            if (!string.IsNullOrEmpty(mimeType))
            {
                string extension = GetArchiveExtension(mimeType.ToUpperInvariant());

                return !string.IsNullOrEmpty(extension);
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified MIME type is audio.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is audio; otherwise, false.</returns>
        public static bool IsAudio(string mimeType)
        {
            if (!string.IsNullOrEmpty(mimeType))
                return mimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase);
            return false;
        }

        /// <summary>
        /// Determines whether the specified MIME type is message.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is message; otherwise, false.</returns>
        public static bool IsMessage(string mimeType)
        {
            if (!string.IsNullOrEmpty(mimeType))
            {
                string extension = GetMessageExtension(mimeType.ToUpperInvariant());

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
            string extension = null;

            if (!string.IsNullOrEmpty(mimeType))
            {
                mimeType = mimeType.ToUpperInvariant();

                extension = GetApplicationExtension(mimeType) ?? GetArchiveExtension(mimeType) ?? GetMicrosoftOfficeExtension(mimeType) ?? GetImageExtension(mimeType) ?? GetAudioExtension(mimeType) ??
                    GetVideoExtension(mimeType) ?? GetTextExtension(mimeType) ?? GetMessageExtension(mimeType) ?? GetMarkupExtension(mimeType);

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

            return extension ?? string.Empty;
        }

        /// <summary>
        /// Returns the file extensions by specified MIME type names (archive, audio, document, image, text, video).
        /// </summary>
        /// <param name="mimeTypeNames">The array of strings that contains the MIME type names.</param>
        /// <returns>The file extensions for the MIME type names (archive, audio, document, image, text, video).</returns>
        public static string[] GetFileExtensions(string[] mimeTypeNames)
        {
            List<string> extensions = new List<string>();

            if (mimeTypeNames != null)
            {
                foreach (string name in mimeTypeNames)
                {
                    switch (name.ToUpperInvariant())
                    {
                        case "ARCHIVE":
                            extensions.AddRange(ArchiveExtensions);
                            break;
                        case "AUDIO":
                            extensions.AddRange(AudioExtensions);
                            break;
                        case "DOCUMENT":
                            extensions.AddRange(DocumentExtensions);
                            break;
                        case "IMAGE":
                            extensions.AddRange(ImageExtensions);
                            break;
                        case "MESSAGE":
                            extensions.AddRange(MessageExtensions);
                            break;
                        case "TEXT":
                            extensions.AddRange(TextExtensions);
                            break;
                        case "VIDEO":
                            extensions.AddRange(VideoExtensions);
                            break;
                    }
                }
            }

            var result = extensions.Distinct().ToArray();

            return result;
        }

        #endregion
    }
}
