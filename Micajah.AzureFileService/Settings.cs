using System.Web.Configuration;

namespace Micajah.AzureFileService
{
    internal static partial class Settings
    {
        #region Constants

        private const int DefaultSharedAccessExpiryTime = 60;
        private const int DefaultClientCacheExpiryTime = 720;
        private const string DefaultTemporaryContanerName = "temp";

        #endregion

        #region Internal Properties

        internal static int SharedAccessExpiryTime
        {
            get
            {
                int minutes = DefaultSharedAccessExpiryTime;
                string str = WebConfigurationManager.AppSettings["mafs:SharedAccessExpiryTime"];
                if (!int.TryParse(str, out minutes))
                {
                    minutes = DefaultSharedAccessExpiryTime;
                }
                return minutes;
            }
        }

        internal static int ClientCacheExpiryTime
        {
            get
            {
                int minutes = DefaultClientCacheExpiryTime;
                string str = WebConfigurationManager.AppSettings["mafs:ClientCacheExpiryTime"];
                if (!int.TryParse(str, out minutes))
                {
                    minutes = DefaultClientCacheExpiryTime;
                }
                return minutes;
            }
        }

        internal static string StorageConnectionString
        {
            get { return WebConfigurationManager.AppSettings["mafs:StorageConnectionString"]; }
        }

        internal static string TemporaryContainerName
        {
            get
            {
                string value = WebConfigurationManager.AppSettings["mafs:TemporaryContainerName"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = DefaultTemporaryContanerName;
                }
                return value;
            }
        }

        #endregion
    }
}
