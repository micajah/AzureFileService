using System;
using System.IO;

namespace Micajah.AzureFileService
{
    /// <summary>
    /// The information of the file.
    /// </summary>
    [Serializable]
    public class File
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the size, in bytes, of the file.
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// Gets or sets the MIME type of the file.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the full path of the file.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the file's URI.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the time, in coordinated universal time (UTC), when the file was last modified.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the file.
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        ///  Gets or sets the extension of the file.
        /// </summary>
        public string Extension
        {
            get
            {
                return Path.GetExtension(this.Name);
            }
        }

        /// <summary>
        /// Gets the size, in kylobytes, of the file.
        /// </summary>
        public long LengthInKB
        {
            get
            {
                return this.Length / 1024;
            }
        }

        /// <summary>
        /// Gets the size, in megabytes, of the file.
        /// </summary>
        public long LengthInMB
        {
            get
            {
                return this.LengthInKB / 1024;
            }
        }

        #endregion
    }
}
