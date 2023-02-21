using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;

namespace Micajah.AzureFileService
{
    /// <summary>
    /// Maps file extensions to content MIME types.
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
        /// image/heic
        /// </summary>
        public const string Heif = "image/heif";

        /// <summary>
        /// Default MIME type application/octet-stream.
        /// </summary>
        public const string Default = "application/octet-stream";

        /// <summary>
        /// application/pdf
        /// </summary>
        public const string Pdf = "application/pdf";

        /// <summary>
        /// image/webp
        /// </summary>
        public const string Webp = "image/webp";

        /// <summary>
        /// .swf
        /// </summary>
        public const string SwfExtension = ".swf";

        private const string AudioPrefix = "audio/";
        private const string ImagePrefix = "image/";
        private const string VideoPrefix = "video/";

        #endregion

        #region Members

        private static NameValueCollection s_Mapping;
        private static NameValueCollection s_ArchiveMapping;
        private static NameValueCollection s_AudioMapping;
        private static NameValueCollection s_ImageMapping;
        private static NameValueCollection s_MessageMapping;
        private static NameValueCollection s_MicrosoftOfficeMapping;
        private static NameValueCollection s_TextMapping;
        private static NameValueCollection s_VideoMapping;
        private static NameValueCollection s_VariousMapping;

        private static string[] s_JpegExtensions;

        #endregion

        #region Private Properties

        /// <summary>
        /// The MIME mapping for all file extensions.
        /// </summary>
        private static NameValueCollection Mapping
        {
            get
            {
                if (s_Mapping == null)
                {
                    s_Mapping = new NameValueCollection
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
                }

                return s_Mapping;
            }
        }

        /// <summary>
        /// The MIME mapping for archive file extensions.
        /// </summary>
        private static NameValueCollection ArchiveMapping
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
        private static NameValueCollection AudioMapping
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
        private static NameValueCollection DocumentMapping
        {
            get
            {
                var mapping = new NameValueCollection
                {
                    MicrosoftOfficeMapping,

                    { ".pdf", Pdf },

                    // Illustrator
                    { ".ai", "application/postscript" },
                    { ".ai", Pdf },

                    // Photoshop
                    { ".psd", "image/vnd.adobe.photoshop" },
                    { ".psd", "image/x-photoshop" },
                    { ".psd", "image/psd" },
                    { ".psd", "application/x-photoshop" },
                    { ".psd", "application/photoshop" },
                    { ".psd", "application/psd" }
                };

                return mapping;
            }
        }

        /// <summary>
        /// The MIME mapping for Microsoft Office document file extensions.
        /// </summary>
        private static NameValueCollection MicrosoftOfficeMapping
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
        private static NameValueCollection ImageMapping
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
                        { ".heif", Heif },
                        { ".heifs", Heif },
                        { ".heic", Heif },
                        { ".heics", Heif },
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
                        { ".svg", "image/svg+xml" },
                        { ".tif", "image/tiff" },
                        { ".tiff", "image/tiff" },
                        { ".wbmp", "image/vnd.wap.wbmp" },
                        { ".wdp", "image/vnd.ms-photo" },
                        { ".webp", Webp },
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
        private static NameValueCollection MessageMapping
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
        private static NameValueCollection TextMapping
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
                        { ".iif", "text/tab-separated-values" },
                        { ".sql", "application/sql" },
                        { ".rtx", "text/richtext" },
                        { ".tsv", "text/tab-separated-values" },
                        { ".txt", "text/plain" },

                        { ".vcf", "text/vcard" },
                        { ".vcf", "text/x-vcard" },

                        { ".xml", "text/xml" },

                        { ".yaml", "text/yaml" },
                        { ".yaml", "text/x-yaml" },
                        { ".yaml", "application/yaml" },
                        { ".yaml", "application/x-yaml" },

                        { ".csr", "application/pkcs10" },
                        { ".pem", "application/pkcs10" },
                        { ".p10", "application/pkcs10" }
                    };
                }

                return s_TextMapping;
            }
        }

        /// <summary>
        /// The MIME mapping for video file extensions.
        /// </summary>
        private static NameValueCollection VideoMapping
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
        private static NameValueCollection VariousMapping
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

        #region Internal Properties

        internal static string[] JpegExtensions
        {
            get
            {
                if (s_JpegExtensions == null)
                {
                    s_JpegExtensions = GetExtensions(Jpeg, ImageMapping);
                }

                return s_JpegExtensions;
            }
        }

        #endregion

        #region Private Methods

        private static string GetExtension(string mimeType, NameValueCollection mapping)
        {
            string extension = null;

            if (!string.IsNullOrEmpty(mimeType))
            {
                mimeType = mimeType.ToLowerInvariant();

                extension = mapping.AllKeys.FirstOrDefault(key => mapping[key].Split(',').Contains(mimeType));
            }

            return extension ?? string.Empty;
        }

        private static string[] GetExtensions(string mimeType, NameValueCollection mapping)
        {
            string[] extensions = null;

            if (!string.IsNullOrEmpty(mimeType))
            {
                mimeType = mimeType.ToLowerInvariant();

                extensions = mapping.AllKeys.Where(key => mapping[key].Split(',').Contains(mimeType)).ToArray();
            }

            return extensions;
        }

        private static string GetMimeType(string extension, NameValueCollection mapping)
        {
            string mimeType = null;

            if (!string.IsNullOrEmpty(extension))
            {
                string ext = System.IO.Path.GetExtension(extension).ToLowerInvariant();

                mimeType = mapping[ext];
            }

            if (mimeType != null)
            {
                mimeType = mimeType.Split(',')[0];
            }
            else
            {
                mimeType = MimeMapping.GetMimeMapping(extension);
            }

            return mimeType;
        }

        private static NameValueCollection GetMapping(MimeTypeGroups mimeTypeGroups)
        {
            NameValueCollection mapping = new NameValueCollection();

            if ((mimeTypeGroups & MimeTypeGroups.Archive) == MimeTypeGroups.Archive)
            {
                mapping.Add(ArchiveMapping);
            }

            if ((mimeTypeGroups & MimeTypeGroups.Audio) == MimeTypeGroups.Audio)
            {
                mapping.Add(AudioMapping);
            }

            if ((mimeTypeGroups & MimeTypeGroups.Document) == MimeTypeGroups.Document)
            {
                mapping.Add(DocumentMapping);
            }
            else if ((mimeTypeGroups & MimeTypeGroups.MicrosoftOffice) == MimeTypeGroups.MicrosoftOffice)
            {
                mapping.Add(MicrosoftOfficeMapping);
            }

            if ((mimeTypeGroups & MimeTypeGroups.Image) == MimeTypeGroups.Image)
            {
                mapping.Add(ImageMapping);
            }

            if ((mimeTypeGroups & MimeTypeGroups.Message) == MimeTypeGroups.Message)
            {
                mapping.Add(MessageMapping);
            }

            if ((mimeTypeGroups & MimeTypeGroups.Text) == MimeTypeGroups.Text)
            {
                mapping.Add(TextMapping);
            }

            if ((mimeTypeGroups & MimeTypeGroups.Video) == MimeTypeGroups.Video)
            {
                mapping.Add(VideoMapping);
            }

            return mapping;
        }

        private static bool IsMapped(string mimeType, NameValueCollection mapping)
        {
            string extension = GetExtension(mimeType, mapping);

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
            return GetExtension(mimeType, Mapping);
        }

        /// <summary>
        /// Returns the array of file extensions by specified MIME type.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type.</param>
        /// <returns>The array of file extensions for the MIME type or null, if the MIME type is not found.</returns>
        public static string[] GetExtensions(string mimeType)
        {
            return GetExtensions(mimeType, Mapping);
        }

        /// <summary>
        /// Returns the file extensions for specified MIME type groups (see MimeTypeGroups enum).
        /// </summary>
        /// <param name="mimeTypeGroups">The array of strings that contains the MIME type groups.</param>
        /// <returns>The file extensions for the MIME groups.</returns>
        public static string[] GetExtensions(string[] mimeTypeGroups)
        {
            MimeTypeGroups groups = MimeTypeGroups.None;

            if (mimeTypeGroups != null)
            {
                foreach (string s in mimeTypeGroups)
                {
                    if (Enum.TryParse(s, true, out MimeTypeGroups group))
                    {
                        groups |= group;
                    }
                }
            }

            return GetExtensions(groups);
        }

        /// <summary>
        /// Returns the file extensions for specified MIME type groups.
        /// </summary>
        /// <param name="mimeTypeGroups">The MIME type groups.</param>
        /// <returns>The file extensions for the MIME type groups.</returns>
        public static string[] GetExtensions(MimeTypeGroups mimeTypeGroups)
        {
            var mapping = GetMapping(mimeTypeGroups);

            List<string> extensions = new List<string>(mapping.AllKeys);

            var result = extensions.Distinct().ToArray();

            return result;
        }

        /// <summary>
        /// Returns the MIME type for the specified file extension or name.
        /// </summary>
        /// <param name="extension">The file extension or name that is used to determine the MIME type.</param>
        /// <returns>The MIME type for the specified file extension or name.</returns>
        public static string GetMimeType(string extension)
        {
            return GetMimeType(extension, Mapping);
        }

        /// <summary>
        /// Determines whether the specified MIME type is default (application/octet-stream) or empty.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is default or empty; otherwise, false.</returns>
        public static bool IsDefaultOrEmpty(string mimeType)
        {
            return string.IsNullOrWhiteSpace(mimeType) || string.Compare(mimeType, Default, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Determines whether the specified MIME type is icon image.
        /// </summary>
        /// <param name="mimeType">The string that contains the MIME type to check.</param>
        /// <returns>true, if the specified MIME type is icon image; otherwise, false.</returns>
        public static bool IsIcon(string mimeType)
        {
            return mimeType.In(Icon, Icon2);
        }

        /// <summary>
        /// Determines whether the specified MIME type is mapped to one of specified MIME type groups.
        /// </summary>
        /// <param name="mimeType">The MIME type to check.</param>
        /// <param name="mimeTypeGroups">The MIME type groups.</param>
        /// <returns>true, if the MIME type is mapped to one of MIME type groups; otherwise, false.</returns>
        public static bool IsInGroups(string mimeType, MimeTypeGroups mimeTypeGroups)
        {
            return IsInGroups(mimeType, mimeTypeGroups, false);
        }

        /// <summary>
        /// Determines whether the specified MIME type or file extension or file name is mapped to one of specified MIME type groups.
        /// </summary>
        /// <param name="mimeType">The MIME type to check.</param>
        /// <param name="mimeTypeGroups">The MIME type groups.</param>
        /// <param name="isExtension">The specified MIME type is file extension or name.</param>
        /// <returns>true, if the MIME type or file extension or file name is mapped to one of MIME type groups; otherwise, false.</returns>
        public static bool IsInGroups(string mimeType, MimeTypeGroups mimeTypeGroups, bool isExtension)
        {
            NameValueCollection mapping;

            if (isExtension)
            {
                mapping = GetMapping(mimeTypeGroups);

                mimeType = GetMimeType(mimeType, mapping);
            }
            else
            {
                if (!string.IsNullOrEmpty(mimeType))
                {
                    if ((mimeTypeGroups & MimeTypeGroups.Audio) == MimeTypeGroups.Audio)
                    {
                        if (mimeType.StartsWith(AudioPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }

                    if ((mimeTypeGroups & MimeTypeGroups.Image) == MimeTypeGroups.Image)
                    {
                        if (mimeType.StartsWith(ImagePrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }

                    if ((mimeTypeGroups & MimeTypeGroups.Video) == MimeTypeGroups.Video)
                    {
                        if (mimeType.StartsWith(VideoPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }

                mapping = GetMapping(mimeTypeGroups);
            }

            return IsMapped(mimeType, mapping);
        }

        public static MimeTypeGroups GetGroups(string mimeType, bool isExtension)
        {
            if (!string.IsNullOrEmpty(mimeType))
            {
                if (isExtension)
                {
                    string ext = System.IO.Path.GetExtension(mimeType).ToLowerInvariant();

                    if (ArchiveMapping[ext] != null)
                    {
                        return MimeTypeGroups.Archive;
                    }

                    if (AudioMapping[ext] != null)
                    {
                        return MimeTypeGroups.Audio;
                    }

                    if (DocumentMapping[ext] != null)
                    {
                        var mimeTypeGroup = MimeTypeGroups.Document;

                        if (MicrosoftOfficeMapping[ext] != null)
                        {
                            mimeTypeGroup |= MimeTypeGroups.MicrosoftOffice;
                        }

                        return mimeTypeGroup;
                    }

                    if (ImageMapping[ext] != null)
                    {
                        return MimeTypeGroups.Image;
                    }

                    if (MessageMapping[ext] != null)
                    {
                        return MimeTypeGroups.Message;
                    }

                    if (TextMapping[ext] != null)
                    {
                        return MimeTypeGroups.Text;
                    }

                    if (VideoMapping[ext] != null)
                    {
                        return MimeTypeGroups.Video;
                    }
                }
                else
                {
                    if (IsMapped(mimeType, ArchiveMapping))
                    {
                        return MimeTypeGroups.Archive;
                    }

                    if (IsMapped(mimeType, AudioMapping))
                    {
                        return MimeTypeGroups.Audio;
                    }

                    if (IsMapped(mimeType, DocumentMapping))
                    {
                        var mimeTypeGroup = MimeTypeGroups.Document;

                        if (IsMapped(mimeType, MicrosoftOfficeMapping))
                        {
                            mimeTypeGroup |= MimeTypeGroups.MicrosoftOffice;
                        }

                        return mimeTypeGroup;
                    }

                    if (IsMapped(mimeType, ImageMapping))
                    {
                        return MimeTypeGroups.Image;
                    }

                    if (IsMapped(mimeType, MessageMapping))
                    {
                        return MimeTypeGroups.Message;
                    }

                    if (IsMapped(mimeType, TextMapping))
                    {
                        return MimeTypeGroups.Text;
                    }

                    if (IsMapped(mimeType, VideoMapping))
                    {
                        return MimeTypeGroups.Video;
                    }
                }
            }

            return MimeTypeGroups.None;
        }

        #endregion
    }
}
