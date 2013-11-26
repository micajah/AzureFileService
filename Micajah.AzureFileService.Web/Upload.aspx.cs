using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Text;
using System.Web.UI;

namespace Micajah.AzureFileService.Web
{
    public partial class Upload : System.Web.UI.Page
    {
        protected string UploadUrl { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnection"));

            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            // /files/OrganizationId/InstanceId/LocalObjectType/LocalObjectId/
            CloudBlobContainer container = client.GetContainerReference("files");
            container.CreateIfNotExists();

            string sas = container.GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Delete | SharedAccessBlobPermissions.List,
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(1440) // Get from config.
            });

            string uploadUri = container.Uri.AbsoluteUri + "/{0}" + sas;
            byte[] uriInBytes = Encoding.UTF8.GetBytes(uploadUri);
            string base64Uri = Convert.ToBase64String(uriInBytes);

            container = client.GetContainerReference("fileservice");
            CloudBlockBlob blob = container.GetBlockBlobReference("upload.html");

            this.UploadUrl = blob.Uri.AbsoluteUri + "?sas=" + base64Uri; // + &opt={all options here}

            //foreach (IListBlobItem item in container.ListBlobs(null, false))
            //{
            //    if (item.GetType() == typeof(CloudBlockBlob))
            //    {
            //        CloudBlockBlob b = (CloudBlockBlob)item;
            //        FilesList.Controls.Add(new LiteralControl(b.Uri.ToString() + " | " + b.Name + " | " + b.Properties.Length.ToString() + " | " + b.Properties.LastModified.ToString() + "<br />"));
            //    }
            //}
        }
    }
}