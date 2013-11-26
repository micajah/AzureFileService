using System.Web.Configuration;

namespace Micajah.AzureFileService.Properties
{
    internal sealed partial class Settings
    {
        #region Members

        private static int? s_MaxRequestLengthInKB;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the maximum request size in bytes.
        /// </summary>
        public static int MaxRequestLength
        {
            get { return (MaxRequestLengthInKB * 1024); }
        }

        /// <summary>
        /// Gets the maximum request size in kilobytes.
        /// </summary>
        public static int MaxRequestLengthInKB
        {
            get
            {
                if (!s_MaxRequestLengthInKB.HasValue)
                {
                    HttpRuntimeSection sect = (WebConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection);
                    s_MaxRequestLengthInKB = ((sect == null) ? 4096 : sect.MaxRequestLength);
                }
                return s_MaxRequestLengthInKB.Value;
            }
        }

        /// <summary>
        /// Gets the maximum request size in megabytes.
        /// </summary>
        public static int MaxRequestLengthInMB
        {
            get { return (MaxRequestLengthInKB / 1024); }
        }

        #endregion
    }
}
