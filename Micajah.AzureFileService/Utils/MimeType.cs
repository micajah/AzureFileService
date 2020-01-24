using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;

namespace Micajah.AzureFileService
{
    /// <summary>
    /// The MIME (Multipurpose Internet Mail Extensions) types of a file.
    /// </summary>
    public static class MimeType
    {
        #region Constants

        /// <summary>
        /// text/calendar
        /// </summary>
        public const string Calendar = "text/calendar";

        /// <summary>
        /// application/x-shockwave-flash
        /// </summary>
        public const string Flash = "application/x-shockwave-flash";

        /// <summary>
        /// image/x-icon
        /// </summary>
        public const string Icon = "image/x-icon";

        /// <summary>
        /// image/vnd.microsoft.icon
        /// </summary>
        private const string Icon2 = "image/vnd.microsoft.icon";

        /// <summary>
        /// image/jpeg
        /// </summary>
        public const string Jpeg = "image/jpeg";

        /// <summary>
        /// text/html
        /// </summary>
        public const string Html = "text/html";

        /// <summary>
        /// Default MIME type application/octet-stream.
        /// </summary>
        public const string Default = "application/octet-stream";

        /// <summary>
        /// application/pdf
        /// </summary>
        public const string Pdf = "application/pdf";

        /// <summary>
        /// .swf
        /// </summary>
        public const string SwfExtension = ".swf";

        private const string AudioPrefix = "audio/";
        private const string ImagePrefix = "image/";
        private const string VideoPrefix = "video/";

        #endregion

        #region Members

        private static NameValueCollection s_ArchiveMapping;
        private static NameValueCollection s_AudioMapping;
        private static NameValueCollection s_ImageMapping;
        private static NameValueCollection s_MessageMapping;
        private static NameValueCollection s_MicrosoftOfficeMapping;
        private static NameValueCollection s_TextMapping;
        private static NameValueCollection s_VideoMapping;
        private static NameValueCollection s_VariousMapping;

        #endregion

        #region Public Properties

        /// <summary>
        /// The MIME mapping for all file extensions.
        /// </summary>
        public static NameValueCollection Mapping
        {
            get
            {
                NameValueCollection mapping = new NameValueCollection
                {
                    ArchiveMapping,
                    AudioMapping,
                    DocumentMapping,
                    ImageMapping,
                    MessageMapping,
                    TextMapping,
                    VideoMapping,
                    VariousMapping
                };

                return mapping;
            }
        }

        /// <summary>
        /// The MIME mapping for archive file extensions.
        /// </summary>
        public static NameValueCollection ArchiveMapping
        {
            get
            {
                if (s_ArchiveMapping == null)
                {
                    s_ArchiveMapping = new NameValueCollection
                    {
                        { ".7z", "application/x-7z-compressed" },
                        { ".gtar", "application/x-gtar" },
                        { ".gz", "application/x-gzip" },
                        { ".rar", "application/x-rar-compressed" },
                        { ".tar", "application/x-tar" },
                        { ".zip", "application/zip" },
                        { ".zip", "application/x-zip" },
                        { ".zip", "application/x-zip-compressed" }
                    };
                }

                return s_ArchiveMapping;
            }
        }

        /// <summary>
        /// The MIME mapping for audio file extensions.
        /// </summary>
        public static NameValueCollection AudioMapping
        {
            get
            {
                if (s_AudioMapping == null)
                {
                    s_AudioMapping = new NameValueCollection
                    {
                        { ".aif", "audio/aiff" },
                        { ".aif", "audio/x-aiff" },
                        { ".aifc", "audio/aiff" },
                        { ".aifc", "audio/x-aiff" },
                        { ".aiff", "audio/aiff" },
                        { ".aiff", "audio/x-aiff" },
                        { ".au", "audio/basic" },
                        { ".mid", "audio/midi" },
                        { ".midi", "audio/midi" },
                        { ".mp3", "audio/mpeg" },
                        { ".ogg", "audio/ogg" },
                        { ".ra", "audio/x-realaudio" },
                        { ".ram", "audio/x-pn-realaudio" },
                        { ".rpm", "audio/x-pn-realaudio-plugin" },
                        { ".snd", "audio/basic" },
                        { ".tsi", "audio/tsp-audio" },
                        { ".wav", "audio/wav" },
                        { ".wav", "audio/x-wav" }
                    };
                }

                return s_AudioMapping;
            }
        }

        /// <summary>
        /// The MIME mapping for Microsoft Office and PDF document file extensions.
        /// </summary>
        public static NameValueCollection DocumentMapping
        {
            get
            {
                var mapping = MicrosoftOfficeMapping;

                mapping.Add(".pdf", Pdf);

                return mapping;
            }
        }

        /// <summary>
        /// The MIME mapping for Microsoft Office document file extensions.
        /// </summary>
        public static NameValueCollection MicrosoftOfficeMapping
        {
            get
            {
                if (s_MicrosoftOfficeMapping == null)
                {
                    s_MicrosoftOfficeMapping = new NameValueCollection
                    {
                        // Word
                        { ".doc", "application/msword" },
                        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                        { ".docm", "application/vnd.ms-word.document.macroenabled.12" },
                        { ".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template" },
                        { ".dotm", "application/vnd.ms-word.template.macroenabled.12" },

                        // Excel
                        { ".xls", "application/vnd.ms-excel" },
                        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                        { ".xlsm", "application/vnd.ms-excel.sheet.macroenabled.12" },
                        { ".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template" },
                        { ".xltm", "application/vnd.ms-excel.template.macroenabled.12" },
                        { ".xlsb", "application/vnd.ms-excel.sheet.binary.macroenabled.12" },
                        { ".xlam", "application/vnd.ms-excel.addin.macroenabled.12" },

                        // Power Point
                        { ".ppt", "application/vnd.ms-powerpoint" },
                        { ".ppt", "application/mspowerpoint" },
                        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                        { ".pptm", "application/vnd.ms-powerpoint.presentation.macroenabled.12" },
                        { ".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow" },
                        { ".ppsm", "application/vnd.ms-powerpoint.slideshow.macroenabled.12" },
                        { ".potx", "application/vnd.openxmlformats-officedocument.presentationml.template" },
                        { ".potm", "application/vnd.ms-powerpoint.template.macroenabled.12" },
                        { ".ppam", "application/vnd.ms-powerpoint.addin.macroenabled.12" },
                        { ".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide" },
                        { ".sldm", "application/vnd.ms-powerpoint.slide.macroenabled.12" },

                        // Other
                        { ".mpp", "application/vnd.ms-project" },
                        { ".rtf", "text/rtf" },
                        { ".rtf", "application/rtf" },
                        { ".onetoc", "application/onenote" },
                        { ".thmx", "application/vnd.ms-officetheme" }
                    };
                }

                return s_MicrosoftOfficeMapping;
            }
        }

        /// <summary>
        /// The MIME mapping for image file extensions.
        /// </summary>
        public static NameValueCollection ImageMapping
        {
            get
            {
                if (s_ImageMapping == null)
                {
                    s_ImageMapping = new NameValueCollection
                    {
                        { ".art", "image/x-jg" },
                        { ".bmp", "image/bmp" },
                        { ".cod", "image/cis-cod" },
                        { ".cmx", "image/x-cmx" },
                        { ".gif", "image/gif" },
                        { ".ico", Icon },
                        { ".ico", Icon2 },
                        { ".ief", "image/ief" },
                        { ".jfif", "image/pjpeg" },
                        { ".jpg", Jpeg },
                        { ".jpe", Jpeg },
                        { ".jpeg", Jpeg },
                        { ".mac", "image/x-macpaint" },
                        { ".pbm", "image/x-portable-bitmap" },
                        { ".pgm", "image/x-portable-graymap" },
                        { ".pic", "image/pict" },
                        { ".png", "image/png" },
                        { ".pnm", "image/x-portable-anymap" },
                        { ".pnt", "image/x-macpaint" },
                        { ".pntg", "image/x-macpaint" },
                        { ".ppm", "image/x-portable-pixmap" },
                        { ".qti", "image/x-quicktime" },
                        { ".ras", "image/cmu-raster" },
                        { ".rgb", "image/x-rgb" },
                        { ".rf", "image/vnd.rn-realflash" },
                        { ".tif", "image/tiff" },
                        { ".tiff", "image/tiff" },
                        { ".wbmp", "image/vnd.wap.wbmp" },
                        { ".wdp", "image/vnd.ms-photo" },
                        { ".xbm", "image/x-xbitmap" },
                        { ".xpm", "image/x-xpixmap" },
                        { ".xwd", "image/x-xwindowdump" }
                    };
                }
                return s_ImageMapping;
            }
        }

        /// <summary>
        /// The MIME mapping for message file extensions.
        /// </summary>
        public static NameValueCollection MessageMapping
        {
            get
            {
                if (s_MessageMapping == null)
                {
                    s_MessageMapping = new NameValueCollection
                    {
                        { ".eml", "message/rfc822" },
                        { ".msg", "application/vnd.ms-outlook" }
                    };
                }

                return s_MessageMapping;
            }
        }

        /// <summary>
        /// The MIME mapping for text file extensions.
        /// </summary>
        public static NameValueCollection TextMapping
        {
            get
            {
                if (s_TextMapping == null)
                {
                    s_TextMapping = new NameValueCollection
                    {
                        { ".csv", "text/csv" },
                        { ".ics", Calendar },
                        { ".ical", Calendar },
                        { ".ifb", Calendar },
                        { ".rtx", "text/richtext" },
                        { ".tsv", "text/tab-separated-values" },
                        { ".txt", "text/plain" },
                        { ".xml", "text/xml" }
                    };
                }

                return s_TextMapping;
            }
        }

        /// <summary>
        /// The MIME mapping for video file extensions.
        /// </summary>
        public static NameValueCollection VideoMapping
        {
            get
            {
                if (s_VideoMapping == null)
                {
                    s_VideoMapping = new NameValueCollection
                    {
                        { ".asf", "video/x-ms-asf" },
                        { ".avi", "video/x-msvideo" },
                        { ".fli", "video/x-fli" },
                        { ".flv", "video/x-flv" },
                        { ".mov", "video/quicktime" },
                        { ".movie", "video/x-sgi-movie" },
                        { ".mp4", "video/mp4" },
                        { ".mpe", "video/mpeg" },
                        { ".mpeg", "video/mpeg" },
                        { ".mpg", "video/mpeg" },
                        { ".qt", "video/quicktime" },
                        { ".viv", "video/vnd.vivo" },
                        { ".vivo", "video/vnd.vivo" },
                        { ".vob", "video/x-ms-vob" },
                        { ".wmv", "video/x-ms-wmv" },
                        { SwfExtension, "application/x-shockwave-flash" }
                    };
                }

                return s_VideoMapping;
            }
        }

        /// <summary>
        /// The MIME mapping for various file extensions.
        /// </summary>
        public static NameValueCollection VariousMapping
        {
            get
            {
                if (s_VariousMapping == null)
                {
                    s_VariousMapping = new NameValueCollection
                    {
                        { ".ps", "application/postscript" },
                        { ".bcpio", "application/x-bcpio" },
                        { ".bin", Default },
                        { ".ccad", "application/clariscad" },
                        { ".cdf", "application/x-netcdf" },
                        { ".cpio", "application/x-cpio" },
                        { ".cpt", "application/mac-compactpro" },
                        { ".csh", "application/x-csh" },
                        { ".dir", "application/x-director" },
                        { ".drw", "application/drafting" },
                        { ".dvi", "application/x-dvi" },
                        { ".dwg", "application/acad" },
                        { ".dxf", "application/dxf" },
                        { ".ez", "application/andrew-inset" },
                        { ".hdf", "application/x-hdf" },
                        { ".hqx", "application/mac-binhex40" },
                        { ".ips", "application/x-ipscript" },
                        { ".ipx", "application/x-ipix" },
                        { ".js", "application/javascript" },
                        { ".js", "application/x-javascript" },
                        { ".js", "text/javascript" },
                        { ".latex", "application/x-latex" },
                        { ".lsp", "application/x-lisp" },
                        { ".man", "application/x-troff-man" },
                        { ".me", "application/x-troff-me" },
                        { ".mif", "application/vnd.mif" },
                        { ".ms", "application/x-troff-ms" },
                        { ".oda", "application/oda" },
                        { ".pgn", "application/x-chess-pgn" },
                        { ".pre", "application/x-freelance" },
                        { ".prt", "application/pro_eng" },
                        { ".roff", "application/x-troff" },
                        { ".scm", "application/x-lotusscreencam" },
                        { ".set", "application/set" },
                        { ".sh", "application/x-sh" },
                        { ".xap", "application/x-silverlight" },
                        { ".shar", "application/x-shar" },
                        { ".sit", "application/x-stuffit" },
                        { ".skd", "application/x-koan" },
                        { ".smi", "application/smil" },
                        { ".sol", "application/solids" },
                        { ".spl", "application/x-futuresplash" },
                        { ".src", "application/x-wais-source" },
                        { ".stp", "application/step" },
                        { ".stl", "application/sla" },
                        { ".sv4cpio", "application/x-sv4cpio" },
                        { ".sv4crc", "application/x-sv4crc" },
                        { ".tcl", "application/x-tcl" },
                        { ".tex", "application/x-tex" },
                        { ".texi", "application/x-texinfo" },
                        { ".tsp", "application/dsptype" },
                        { ".unv", "application/i-deas" },
                        { ".ustar", "application/x-ustar" },
                        { ".vcd", "application/x-cdlink" },
                        { ".vda", "application/vda" },
                        { ".css", "text/css" },
                        { ".etx", "text/x-setext" },
                        { ".htm", "text/html" },
                        { ".sgm", "text/sgml" },
                        { ".pdb", "chemical/x-pdb" },
                        { ".igs", "model/iges" },
                        { ".msh", "model/mesh" },
                        { ".vrml", "model/vrml" },
                        { ".mime", "www/mime" },
                        { ".ice", "x-conference/x-cooltalk" }
                    };
                }

                return s_VariousMapping;
            }
        }

        #endregion

        #region Private Methods

        private static string GetExtension(string mimeType, NameValueCollection mapping)
        {
            string extension = null;

            if (!string.IsNullOrEmpty(mimeType))
            {
                extension = mapping.AllKeys.FirstOrDefault(key => mapping[key].Split(',').Contains(mimeType));
            }

            return extension;
        }

        private static string GetMimeType(string fileName, NameValueCollection mapping)
        {
            string extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();

            string mimeType = mapping[extension];

            if (mimeType != null)
            {
                mimeType = mimeType.Split(',')[0];
            }
            else
            {
                mimeType = MimeMapping.GetMimeMapping(fileName);
            }

            return mimeType;
        }

        private static bool IsKnown(string mimeType, NameValueCollection mapping)
        {
            string extension = GetExtension(mimeType.ToLowerInvariant(), mapping);

            return !string.IsNullOrEmpty(extension);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns image format associated to the specified MIME type.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type.</param>
        /// <returns>An image format, if it is found; otherwise null reference.</returns>
        public static ImageFormat GetImageFormat(string mimeType)
        {
            ImageCodecInfo[] imageCodecs = ImageCodecInfo.GetImageEncoders();

            ImageCodecInfo imageCodec = imageCodecs.FirstOrDefault(codec => codec.MimeType == mimeType);

            return imageCodec != null ? new ImageFormat(imageCodec.FormatID) : null;
        }

        /// <summary>
        /// Returns the file extension by specified MIME type.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type.</param>
        /// <returns>The file extension for the MIME type or empty string, if the MIME type is not found.</returns>
        public static string GetExtension(string mimeType)
        {
            string extension = null;

            if (!string.IsNullOrEmpty(mimeType))
            {
                extension = GetExtension(mimeType.ToLowerInvariant(), Mapping);
            }

            return extension ?? string.Empty;
        }

        /// <summary>
        /// Returns the file extensions by specified MIME type names (archive, audio, document, image, text, video).
        /// </summary>
        /// <param name="mimeTypeNames">The array of strings that contains the MIME type names.</param>
        /// <returns>The file extensions for the MIME type names (archive, audio, document, image, text, video).</returns>
        public static string[] GetExtensions(string[] mimeTypeNames)
        {
            List<string> extensions = new List<string>();

            if (mimeTypeNames != null)
            {
                foreach (string name in mimeTypeNames)
                {
                    switch (name.ToLowerInvariant())
                    {
                        case "archive":
                            extensions.AddRange(ArchiveMapping.AllKeys);
                            break;
                        case "audio":
                            extensions.AddRange(AudioMapping.AllKeys);
                            break;
                        case "document":
                            extensions.AddRange(DocumentMapping.AllKeys);
                            break;
                        case "image":
                            extensions.AddRange(ImageMapping.AllKeys);
                            break;
                        case "message":
                            extensions.AddRange(MessageMapping.AllKeys);
                            break;
                        case "text":
                            extensions.AddRange(TextMapping.AllKeys);
                            break;
                        case "video":
                            extensions.AddRange(VideoMapping.AllKeys);
                            break;
                    }
                }
            }

            var result = extensions.Distinct().ToArray();

            return result;
        }

        /// <summary>
        /// Returns the MIME type for the specified file name.
        /// </summary>
        /// <param name="fileName">The file name that is used to determine the MIME type.</param>
        /// <returns>The MIME type for the specified file name.</returns>
        public static string GetMimeType(string fileName)
        {
            return GetMimeType(fileName, Mapping);
        }

        /// <summary>
        /// Determines whether the specified MIME type is empty or default (application/octet-stream).
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is empty or default (application/octet-stream); otherwise, false.</returns>
        public static bool IsEmptyOrDefault(string mimeType)
        {
            return string.IsNullOrEmpty(mimeType) || string.Compare(mimeType, Default, StringComparison.OrdinalIgnoreCase) == 0;
        }

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
        /// Determines whether the specified MIME type is audio.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is audio; otherwise, false.</returns>
        public static bool IsAudio(string mimeType)
        {
            if (!string.IsNullOrEmpty(mimeType))
                return mimeType.StartsWith(AudioPrefix, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        /// <summary>
        /// Determines whether the specified MIME type is image type.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is image type; otherwise, false.</returns>
        public static bool IsImage(string mimeType)
        {
            if (!string.IsNullOrEmpty(mimeType))
                return mimeType.StartsWith(ImagePrefix, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        /// <summary>
        /// Determines whether the specified MIME type is video type.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is video type; otherwise, false.</returns>
        public static bool IsVideo(string mimeType)
        {
            if (!string.IsNullOrEmpty(mimeType))
                return mimeType.StartsWith(VideoPrefix, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        /// <summary>
        /// Determines whether the specified MIME type is archive.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is archive; otherwise, false.</returns>
        public static bool IsArchive(string mimeType)
        {
            return IsKnown(mimeType, ArchiveMapping);
        }

        /// <summary>
        /// Determines whether the specified MIME type is Microsoft Office or PDF document.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is Microsoft Office or PDF document; otherwise, false.</returns>
        public static bool IsDocument(string mimeType)
        {
            return IsKnown(mimeType, DocumentMapping);
        }

        /// <summary>
        /// Determines whether the specified MIME type is Microsoft Office document.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is Microsoft Office document; otherwise, false.</returns>
        public static bool IsMicrosoftOffice(string mimeType)
        {
            return IsKnown(mimeType, MicrosoftOfficeMapping);
        }

        /// <summary>
        /// Determines whether the specified MIME type is message.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is message; otherwise, false.</returns>
        public static bool IsMessage(string mimeType)
        {
            return IsKnown(mimeType, MessageMapping);
        }

        /// <summary>
        /// Determines whether the specified MIME type is text.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is text; otherwise, false.</returns>
        public static bool IsText(string mimeType)
        {
            return IsKnown(mimeType, TextMapping);
        }

        /// <summary>
        /// Determines whether the specified file is archive.
        /// </summary>
        /// <param name="fileName">The string that contains the file name to check.</param>
        /// <returns>true, if the specified file is archive; otherwise, false.</returns>
        public static bool IsArchiveFile(string fileName)
        {
            return IsArchive(GetMimeType(fileName, ArchiveMapping));
        }

        /// <summary>
        /// Determines whether the specified file is audio.
        /// </summary>
        /// <param name="fileName">The string that contains the file name to check.</param>
        /// <returns>true, if the specified file is audio; otherwise, false.</returns>
        public static bool IsAudioFile(string fileName)
        {
            return IsAudio(GetMimeType(fileName, AudioMapping));
        }

        /// <summary>
        /// Determines whether the specified file is Microsoft Office or PDF document.
        /// </summary>
        /// <param name="fileName">The string that contains the file name to check.</param>
        /// <returns>true, if the specified file is Microsoft Office or PDF; otherwise, false.</returns>
        public static bool IsDocumentFile(string fileName)
        {
            return IsDocument(GetMimeType(fileName, DocumentMapping));
        }

        /// <summary>
        /// Determines whether the specified file is Microsoft Office document.
        /// </summary>
        /// <param name="fileName">The string that contains the file name to check.</param>
        /// <returns>true, if the specified file is Microsoft Office; otherwise, false.</returns>
        public static bool IsMicrosoftOfficeFile(string fileName)
        {
            return IsMicrosoftOffice(GetMimeType(fileName, MicrosoftOfficeMapping));
        }

        /// <summary>
        /// Determines whether the specified file is image.
        /// </summary>
        /// <param name="fileName">The string that contains the file name to check.</param>
        /// <returns>true, if the specified file is image; otherwise, false.</returns>
        public static bool IsImageFile(string fileName)
        {
            return IsImage(GetMimeType(fileName, ImageMapping));
        }

        /// <summary>
        /// Determines whether the specified file is message.
        /// </summary>
        /// <param name="fileName">The string that contains the file name to check.</param>
        /// <returns>true, if the specified file is message; otherwise, false.</returns>
        public static bool IsMessageFile(string fileName)
        {
            return IsMessage(GetMimeType(fileName, MessageMapping));
        }

        /// <summary>
        /// Determines whether the specified file is text.
        /// </summary>
        /// <param name="fileName">The string that contains the file name to check.</param>
        /// <returns>true, if the specified file is text; otherwise, false.</returns>
        public static bool IsTextFile(string fileName)
        {
            return IsText(GetMimeType(fileName, TextMapping));
        }

        /// <summary>
        /// Determines whether the specified file is video.
        /// </summary>
        /// <param name="fileName">The string that contains the file name to check.</param>
        /// <returns>true, if the specified file is video; otherwise, false.</returns>
        public static bool IsVideoFile(string fileName)
        {
            return IsVideo(GetMimeType(fileName, VideoMapping));
        }

        #endregion
    }
}
