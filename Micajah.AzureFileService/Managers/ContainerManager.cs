using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Micajah.AzureFileService
{
    public static class ContainerManager
    {
        #region Members

        private static CloudBlobClient s_ServiceClient;
        private static CloudBlobContainer s_TemporaryContainer;
        private static string s_BlobEndpoint;

        #endregion

        #region Private Properties

        private static CloudBlobClient ServiceClient
        {
            get
            {
                if (s_ServiceClient == null)
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Settings.StorageConnectionString);

                    s_BlobEndpoint = storageAccount.BlobEndpoint.ToString().TrimEnd('/');

                    if (s_BlobEndpoint.StartsWith(Uri.UriSchemeHttps + Uri.SchemeDelimiter, StringComparison.OrdinalIgnoreCase))
                    {
                        ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                        {
                            if (sslPolicyErrors != SslPolicyErrors.None)
                            {
                                if (sender is WebRequest request)
                                {
                                    if (request.RequestUri.ToString().IndexOf(s_BlobEndpoint, System.StringComparison.OrdinalIgnoreCase) > -1)
                                    {
                                        return true;
                                    }
                                }
                            }

                            return sslPolicyErrors == SslPolicyErrors.None;
                        };
                    }

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

        #region Public Properties

        /// <summary>
        /// Gets the base URI for the Blob service client at the primary location.
        /// </summary>
        public static string ServiceClientBaseUrl
        {
            get
            {
                return ServiceClient.BaseUri.AbsoluteUri;
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

        /// <summary>
        /// Creates the container if it does not already exist.
        /// </summary>
        /// <param name="containerName">A string containing the name of the container.</param>
        /// <param name="publicAccess">A value indicating whether the public access to the files is allowed in the container.</param>
        public static void CreateContainer(string containerName, bool publicAccess)
        {
            CreateContainerIfNotExists(containerName, publicAccess);
        }

        /// <summary>
        /// Returns the size of all blobs in the container, in bytes.
        /// </summary>
        /// <param name="containerName">A string containing the name of the container.</param>
        /// <returns>Returns the size of all blobs in the container, in bytes.</returns>
        public static long GetContainerLength(string containerName)
        {
            long length = 0;

            CloudBlobContainer container = GetContainerReference(containerName);

            IEnumerable<IListBlobItem> blobList = container.ListBlobs(null, true);
            foreach (IListBlobItem item in blobList)
            {
                CloudBlockBlob blob = item as CloudBlockBlob;
                if (blob != null)
                {
                    if (blob.BlobType == BlobType.BlockBlob)
                    {
                        length += blob.Properties.Length;
                    }
                }
            }

            return length;
        }

        #endregion
    }
}
