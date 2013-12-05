using Micajah.AzureFileService.Handlers;
using Micajah.AzureFileService.Properties;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Micajah.AzureFileService.WebControls
{
    /// <summary>
    /// The control for single- and multi-file uploads.
    /// </summary>
    [ToolboxData("<{0}:FileUpload runat=server></{0}:FileUpload>")]
    public class FileUpload : WebControl, IUploadControl, INamingContainer
    {
        #region Members

        private const string FileServiceContanerName = "fileservice";
        private const string UploadPageName = "upload.html";

        protected HtmlIframe IFrame;
        protected System.Web.UI.WebControls.FileUpload FileFromMyComputer;

        private CloudBlobClient m_CloudClient;
        private CloudBlobContainer m_Container;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a comma-separated list of MIME encodings used to constrain the file types the user can select.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue("")]
        public string Accept
        {
            get { return (string)this.ViewState["Accept"]; }
            set { this.ViewState["Accept"] = value; }
        }

        /// <summary>
        /// Gets or sets the maximum files count that can be selected by user in the control.
        /// The default value is 0 that indicates the maximum files count is not set.
        /// </summary>
        [Description("The maximum files count that can be selected by user in the control.")]
        [Category("Behavior")]
        [DefaultValue(0)]
        public int MaxFilesCount
        {
            get
            {
                object obj = this.ViewState["MaxFilesCount"];
                return ((obj == null) ? 0 : (int)obj);
            }
            set { this.ViewState["MaxFilesCount"] = value; }
        }

        /// <summary>
        /// Gets or sets the maximum file size allowed for uploading in bytes. Set to 0 for unlimited size.
        /// </summary>
        [Description("Gets or sets the maximum file size allowed for uploading in bytes. Set to 0 for unlimited size.")]
        [Category("Behavior")]
        [DefaultValue(0)]
        public int MaxFileSize
        {
            get
            {
                object obj = this.ViewState["MaxFileSize"];
                return ((obj == null) ? 0 : (int)obj);
            }
            set { this.ViewState["MaxFileSize"] = value; }
        }

        /// <summary>
        /// Gets or sets the unique identifier of the organization.
        /// </summary>
        [Category("Data")]
        [Description("The unique identifier of the organization.")]
        [DefaultValue(typeof(Guid), "00000000-0000-0000-0000-000000000000")]
        public Guid OrganizationId
        {
            get
            {
                object obj = this.ViewState["OrganizationId"];
                return ((obj == null) ? Guid.Empty : (Guid)obj);
            }
            set { this.ViewState["OrganizationId"] = value; }
        }

        /// <summary>
        /// Gets or sets the unique identifier of the instance.
        /// </summary>
        [Category("Data")]
        [Description("The unique identifier of the instance.")]
        [DefaultValue(typeof(Guid), "00000000-0000-0000-0000-000000000000")]
        public Guid InstanceId
        {
            get
            {
                object obj = this.ViewState["InstanceId"];
                return ((obj == null) ? Guid.Empty : (Guid)obj);
            }
            set { this.ViewState["InstanceId"] = value; }
        }

        /// <summary>
        /// Gets or sets the type of the object which the files are associated with.
        /// </summary>
        [Category("Data")]
        [Description("The type of the object which the files are associated with.")]
        [DefaultValue("")]
        public string LocalObjectType
        {
            get { return (string)this.ViewState["LocalObjectType"]; }
            set { this.ViewState["LocalObjectType"] = value; }
        }

        /// <summary>
        /// Gets or sets the unique identifier of the object which the files are associated with.
        /// </summary>
        [Category("Data")]
        [Description("The unique identifier of the object which the files are associated with.")]
        [DefaultValue("")]
        public string LocalObjectId
        {
            get { return (string)this.ViewState["LocalObjectId"]; }
            set { this.ViewState["LocalObjectId"] = value; }
        }

        /// <summary>
        /// Gets or sets the connection string to the storage.
        /// </summary>
        [Category("Data")]
        [Description("The connection string to the storage.")]
        [DefaultValue("")]
        public string StorageConnectionString
        {
            get
            {
                object obj = this.ViewState["StorageConnectionString"];
                return ((obj == null) ? Settings.Default.StorageConnectionString : (string)obj);
            }
            set { this.ViewState["StorageConnectionString"] = value; }
        }

        #endregion

        #region Overriden Properties

        protected override HtmlTextWriterTag TagKey
        {
            get { return HtmlTextWriterTag.Div; }
        }

        #endregion

        #region Private Properties

        private CloudBlobClient Client
        {
            get
            {
                if (m_CloudClient == null)
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.StorageConnectionString);
                    m_CloudClient = storageAccount.CreateCloudBlobClient();
                }
                return m_CloudClient;
            }
        }

        private CloudBlobContainer Container
        {
            get
            {
                if (m_Container == null)
                {
                    m_Container = this.Client.GetContainerReference(this.InstanceId.ToString("N"));
                    m_Container.CreateIfNotExists();
                }
                return m_Container;
            }
        }

        private string IFrameUri
        {
            get
            {
                string sas = this.Container.GetSharedAccessSignature(new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Write,
                    SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(Settings.Default.SharedAccessExpiryTime)
                });

                string uploadUri = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}/{{0}}{3}", this.Container.Uri.AbsoluteUri, this.LocalObjectType, this.LocalObjectId, sas);

                StringBuilder sb = new StringBuilder("method:\"PUT\",createImageThumbnails:false");
                sb.AppendFormat(CultureInfo.InvariantCulture, ",url:\"{0}\"", uploadUri);
                if (!string.IsNullOrWhiteSpace(this.Accept))
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, ",acceptedFiles:\"{0}\"", this.Accept);
                }
                if (this.MaxFilesCount > 0)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, ",maxFiles:{0}", this.MaxFilesCount);
                }
                if (this.MaxFileSize > 0)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, ",maxFilesize:{0}", this.MaxFileSize / 1024 / 1024);
                }
                sb.AppendFormat(CultureInfo.InvariantCulture, ",dictDefaultMessage:\"{0}\"", Resources.FileUpload_DefaultMessage);

                byte[] settingsBytes = Encoding.UTF8.GetBytes(sb.ToString());
                string settingsBase64String = Convert.ToBase64String(settingsBytes);
                settingsBase64String = HttpUtility.UrlEncode(settingsBase64String);

                return this.UploadPageUri + "?d=" + settingsBase64String;
            }
        }

        private string UploadPageUri
        {
            get
            {
                CloudBlobContainer container = this.Client.GetContainerReference(FileServiceContanerName);
                CloudBlockBlob blob = container.GetBlockBlobReference(UploadPageName);

                return blob.Uri.AbsoluteUri;
            }
        }

        #endregion

        #region Private Methods

        private void UploadFile()
        {
            if (!string.IsNullOrEmpty(FileFromMyComputer.FileName))
            {
                if (FileFromMyComputer.HasFile)
                {
                    HttpPostedFile file = FileFromMyComputer.PostedFile;
                    if (this.ValidateMimeType(file.ContentType))
                    {
                        long fileSize = file.InputStream.Length;
                        if ((this.MaxFileSize == 0) || (fileSize <= this.MaxFileSize))
                        {
                            string blobName = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}", this.LocalObjectType, this.LocalObjectId, Path.GetFileName(file.FileName));

                            CloudBlockBlob blob = this.Container.GetBlockBlobReference(blobName);
                            blob.UploadFromStream(file.InputStream);
                        }
                        // TODO: Error handling on server and client side.
                        //else
                        //    m_ErrorMessage = Resources.ImageUpload_InvalidFileSize;
                    }
                    //else
                    //    m_ErrorMessage = Resources.ImageUpload_InvalidMimeType;
                }
            }
        }

        private bool ValidateMimeType(string mimeType)
        {
            if (string.IsNullOrWhiteSpace(this.Accept))
                return true;

            foreach (string item in this.Accept.Split(','))
            {
                if (string.Compare(item, mimeType, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
                else if (mimeType.IndexOf(item.Replace("*", string.Empty), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Overriden Methods

        protected override void CreateChildControls()
        {
            IFrame = new HtmlIframe();
            IFrame.ID = "IFrame";
            IFrame.Attributes["style"] = "width: 100%; height: 370px; border: none 0; overflow: hidden; display: none;";
            IFrame.Attributes["scrolling"] = "no";
            this.Controls.Add(IFrame);

            FileFromMyComputer = new System.Web.UI.WebControls.FileUpload();
            FileFromMyComputer.Style.Add(HtmlTextWriterStyle.Display, "none");
            if (!string.IsNullOrWhiteSpace(this.Accept))
            {
                FileFromMyComputer.Attributes["accept"] = this.Accept;
            }
            FileFromMyComputer.ID = "FileFromMyComputer";
            this.Controls.Add(FileFromMyComputer);

            if (this.Page.IsPostBack)
                this.UploadFile();
        }

        /// <summary>
        /// Raises the System.Web.UI.Control.PreRender event and registers the client scripts and style sheets for the control.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager.RegisterClientScriptInclude(this.Page, this.Page.GetType(), "Scripts.FileUpload.js", ResourceHandler.GetWebResourceUrl("Scripts.FileUpload.js", true));

            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), this.ClientID
                , string.Format(CultureInfo.InvariantCulture, "var {0} = new FileUpload(\"{1}\",{{url:\"{2}\"}});\r\n"
                    , char.ToLowerInvariant(this.ClientID[0]) + this.ClientID.Substring(1), this.ClientID, this.IFrameUri)
                , true);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Commits all the changes since the last time AcceptChanges was called.
        /// </summary>
        public void AcceptChanges()
        {
            // TODO: Move the files from temporary folder.
        }

        /// <summary>
        /// Rolls back all changes that have been made to the control since it was loaded, or the last time AcceptChanges was called.
        /// </summary>
        public void RejectChanges()
        {
            // TODO: Delete the files?
        }

        #endregion
    }
}
