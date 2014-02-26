using System;
using System.Globalization;
using System.Web.Configuration;

namespace Micajah.AzureFileService
{
    internal static partial class Settings
    {
        #region Constants

        private const string SharedAccessExpiryTimeKey = "mafs:SharedAccessExpiryTime";
        private const string ClientCacheExpiryTimeKey = "mafs:ClientCacheExpiryTime";
        private const string StorageConnectionStringKey = "mafs:StorageConnectionString";
        private const string TemporaryContainerNameKey = "mafs:TemporaryContainerName";
        private const string MaxFileSizeKey = "mafs:MaxFileSize";

        private const int DefaultSharedAccessExpiryTime = 60; // 1 hour
        private const int DefaultClientCacheExpiryTime = 144000; // 100 days
        private const string DefaultTemporaryContanerName = "micajahazurefileservicetemp";
        private const string ClientCacheControlFormat = "public, max-age={0}";

        internal const int DefaultMaxFileSize = 52428800; // 50 MB

        #endregion

        #region Internal Properties

        internal static int SharedAccessExpiryTime
        {
            get
            {
                int minutes = DefaultSharedAccessExpiryTime;
                string str = WebConfigurationManager.AppSettings[SharedAccessExpiryTimeKey];
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
                string str = WebConfigurationManager.AppSettings[ClientCacheExpiryTimeKey];
                if (!int.TryParse(str, out minutes))
                {
                    minutes = DefaultClientCacheExpiryTime;
                }
                return minutes;
            }
        }

        internal static string StorageConnectionString
        {
            get { return WebConfigurationManager.AppSettings[StorageConnectionStringKey]; }
        }

        internal static string TemporaryContainerName
        {
            get
            {
                string value = WebConfigurationManager.AppSettings[TemporaryContainerNameKey];
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = DefaultTemporaryContanerName;
                }
                return value;
            }
        }

        internal static string ClientCacheControl
        {
            get
            {
                TimeSpan t = new TimeSpan(0, ClientCacheExpiryTime, 0);
                return string.Format(CultureInfo.InvariantCulture, ClientCacheControlFormat, t.TotalSeconds);
            }
        }

        internal static int MaxFileSize
        {
            get
            {
                int maxFileSize = DefaultMaxFileSize;
                string str = WebConfigurationManager.AppSettings[MaxFileSizeKey];
                if (!int.TryParse(str, out maxFileSize))
                {
                    maxFileSize = DefaultMaxFileSize;
                }
                return maxFileSize;
            }
        }

        #endregion
    }
}
