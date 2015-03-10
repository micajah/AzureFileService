using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Micajah.AzureFileService
{
    public static class ContainerManager
    {
        #region Members

        private static CloudBlobClient s_ServiceClient;
        private static CloudBlobContainer s_TemporaryContainer;

        #endregion

        #region Private Properties

        private static CloudBlobClient ServiceClient
        {
            get
            {
                if (s_ServiceClient == null)
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Settings.StorageConnectionString);

                    s_ServiceClient = storageAccount.CreateCloudBlobClient();
                }
                return s_ServiceClient;
            }
        }

        #endregion

        #region Internal Properties

        internal static CloudBlobContainer TemporaryContainer
        {
            get
            {
                if (s_TemporaryContainer == null)
                {
                    s_TemporaryContainer = CreateContainerIfNotExists(Settings.TemporaryContainerName, false);
                }
                return s_TemporaryContainer;
            }
        }

        #endregion

        #region Internal Methods

        internal static CloudBlobContainer CreateContainerIfNotExists(string containerName, bool publicAccess)
        {
            CloudBlobContainer container = ServiceClient.GetContainerReference(containerName);

            container.CreateIfNotExists();

            if (publicAccess)
            {
                BlobContainerPermissions p = new BlobContainerPermissions()
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };

                container.SetPermissions(p);
            }

            return container;
        }

        internal static CloudBlobContainer GetContainerReference(string containerName)
        {
            CloudBlobContainer container = ServiceClient.GetContainerReference(containerName);

            return container;
        }

        #endregion

        #region Public Methods

        public static void CreateContainer(string containerName, bool publicAccess)
        {
            CreateContainerIfNotExists(containerName, publicAccess);
        }

        #endregion
    }
}
