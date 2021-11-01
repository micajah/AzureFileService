using Azure.Storage.Blobs.Models;

namespace Micajah.AzureFileService
{
    public static class ContainerManager
    {
        #region Public Methods

        /// <summary>
        /// Creates the container if it does not already exist.
        /// </summary>
        /// <param name="containerName">A string containing the name of the container.</param>
        /// <param name="publicAccess">A value indicating whether the public access to the files is allowed in the container.</param>
        public static void CreateContainer(string containerName, bool publicAccess)
        {
            var containerClient = FileManager.ServiceClient.GetBlobContainerClient(containerName);

            containerClient.CreateIfNotExists(publicAccess ? PublicAccessType.Blob : PublicAccessType.None);
        }

        /// <summary>
        /// Returns the size of all blobs in the container, in bytes.
        /// </summary>
        /// <param name="containerName">A string containing the name of the container.</param>
        /// <returns>Returns the size of all blobs in the container, in bytes.</returns>
        public static long CalculateContainerSize(string containerName)
        {
            long length = 0;

            var containerClient = FileManager.ServiceClient.GetBlobContainerClient(containerName);

            var blobItems = FileManager.GetBlockBlobs(containerClient, null, true);

            foreach (var blobItem in blobItems)
            {
                length += blobItem.Properties.ContentLength.GetValueOrDefault();
            }

            return length;
        }

        #endregion
    }
}
