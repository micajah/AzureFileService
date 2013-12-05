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

            CloudBlobContainer container = client.GetContainerReference("15acca548bc34519b6095c11a16a63ec");
            container.CreateIfNotExists();

            foreach (IListBlobItem item in container.ListBlobs("ticket/12345/DSC", false))
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob b = (CloudBlockBlob)item;
                    FilesList.Controls.Add(new LiteralControl(b.Uri.ToString() + " | " + b.Name + " | " + b.Properties.Length.ToString() + " | " + b.Properties.LastModified.ToString() + "<br />"));
                }
            }
        }
    }
}