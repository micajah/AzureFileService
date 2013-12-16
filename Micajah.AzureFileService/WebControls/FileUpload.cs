using Micajah.AzureFileService.Handlers;
using Micajah.AzureFileService.Properties;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Configuration;
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

        private const int DefaultSharedAccessExpiryTime = 1440;
        private const string DefaultTemporaryContanerName = "temp";

        protected System.Web.UI.WebControls.FileUpload FileFromMyComputer;

        private CloudBlobClient m_CloudClient;
        private CloudBlobContainer m_Container;
        private CloudBlobContainer m_TemporaryContainer;

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
        /// Gets or sets the name of the container the files are uploaded to.
        /// </summary>
        [Category("Data")]
        [Description("The name of the container the files are uploaded to.")]
        [DefaultValue("")]
        public string ContainerName
        {
            get { return (string)this.ViewState["ContainerName"]; }
            set { this.ViewState["ContainerName"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the temporary container the files are uploaded to.
        /// </summary>
        [Category("Data")]
        [Description("The name of the temporary container the files are uploaded to.")]
        [DefaultValue("")]
        public string TemporaryContainerName
        {
            get
            {
                string value = (string)this.ViewState["TemporaryContainerName"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = WebConfigurationManager.AppSettings["mafs:TemporaryContainerName"];
                }
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = DefaultTemporaryContanerName;
                }
                return value;
            }
            set { this.ViewState["TemporaryContainerName"] = value; }
        }

        /// <summary>
        /// Gets or sets the type of the object which the files are associated with.
        /// </summary>
        [Category("Data")]
        [Description("The type of the object which the files are associated with.")]
        [DefaultValue("")]
        public string ObjectType
        {
            get { return (string)this.ViewState["ObjectType"]; }
            set { this.ViewState["ObjectType"] = value; }
        }

        /// <summary>
        /// Gets or sets the unique identifier of the object which the files are associated with.
        /// </summary>
        [Category("Data")]
        [Description("The unique identifier of the object which the files are associated with.")]
        [DefaultValue("")]
        public string ObjectId
        {
            get { return (string)this.ViewState["ObjectId"]; }
            set { this.ViewState["ObjectId"] = value; }
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
                return ((obj == null) ? WebConfigurationManager.AppSettings["mafs:StorageConnectionString"] : (string)obj);
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
                    m_Container = this.Client.GetContainerReference(this.ContainerName);
                    m_Container.CreateIfNotExists();
                }
                return m_Container;
            }
        }

        private CloudBlobContainer TemporaryContainer
        {
            get
            {
                if (m_TemporaryContainer == null)
                {
                    m_TemporaryContainer = this.Client.GetContainerReference(this.TemporaryContainerName);
                    m_TemporaryContainer.CreateIfNotExists();
                }
                return m_TemporaryContainer;
            }
        }

        private int MaxFileSizeInMB
        {
            get
            {
                return this.MaxFileSize / 1024 / 1024;
            }
        }

        private string TemporaryBlobPath
        {
            get
            {
                string value = (string)this.ViewState["TemporaryBlobPath"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = string.Format(CultureInfo.InvariantCulture, "{0:N}/", Guid.NewGuid());
                    this.TemporaryBlobPath = value;
                }
                return value;
            }
            set { this.ViewState["TemporaryBlobPath"] = value; }
        }

        private static int SharedAccessExpiryTime
        {
            get
            {
                int minutes = DefaultSharedAccessExpiryTime;
                string str = WebConfigurationManager.AppSettings["mafs:SharedAccessExpiryTime"];
                if (!int.TryParse(str, out minutes))
                    minutes = DefaultSharedAccessExpiryTime;
                return minutes;
            }
        }

        private string SharedAccessSignature
        {
            get
            {
                string sas = this.TemporaryContainer.GetSharedAccessSignature(new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Write,
                    SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(SharedAccessExpiryTime)
                });
                return sas;
            }
        }

        private string UploadUri
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}/{1}{{0}}{2}", this.TemporaryContainer.Uri.AbsoluteUri, this.TemporaryBlobPath, this.SharedAccessSignature);
            }
        }

        private string ClientScript
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(CultureInfo.InvariantCulture, "Dropzone.options.{1} = false;\r\nvar {0} = new Dropzone(\"#{1}\",{{method:\"Put\",createImageThumbnails:false,paramName:\"{2}\",url:\"{3}\""
                    , char.ToLowerInvariant(this.ClientID[0]) + this.ClientID.Substring(1), this.ClientID, FileFromMyComputer.UniqueID, this.UploadUri);
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
                    sb.AppendFormat(CultureInfo.InvariantCulture, ",maxFilesize:{0}", this.MaxFileSizeInMB);
                }
                sb.AppendFormat(CultureInfo.InvariantCulture, ",dictDefaultMessage:\"{0}\"}});\r\n", Resources.FileUpload_DefaultMessage);
                return sb.ToString();
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
                            string blobName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", this.TemporaryBlobPath, Path.GetFileName(file.FileName));

                            CloudBlockBlob blob = this.TemporaryContainer.GetBlockBlobReference(blobName);
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

        #region Internal Methods

        /// <summary>
        /// Registers the specified style sheet for the specified control.
        /// </summary>
        /// <param name="ctl">The control to register style sheet for.</param>
        /// <param name="styleSheetWebResourceName">The name of the server-side resource of the style sheet to register.</param>
        internal static void RegisterControlStyleSheet(Page page, string styleSheetWebResourceName)
        {
            Type pageType = page.GetType();
            string webResourceUrl = ResourceHandler.GetWebResourceUrl(styleSheetWebResourceName, true);

            if (!page.ClientScript.IsClientScriptBlockRegistered(pageType, webResourceUrl))
            {
                string script = string.Empty;

                if (page.Header == null)
                    script = string.Format(CultureInfo.InvariantCulture, "<link type=\"text/css\" rel=\"stylesheet\" href=\"{0}\"></link>", webResourceUrl);
                else
                {
                    HtmlLink link = new HtmlLink();
                    link.Href = webResourceUrl;
                    link.Attributes.Add("type", "text/css");
                    link.Attributes.Add("rel", "stylesheet");
                    page.Header.Controls.Add(link);
                }

                page.ClientScript.RegisterClientScriptBlock(pageType, webResourceUrl, script, false);
            }
        }

        #endregion

        #region Overriden Methods

        protected override void CreateChildControls()
        {
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.Attributes["class"] = "fallback";
            this.Controls.Add(div);

            FileFromMyComputer = new System.Web.UI.WebControls.FileUpload();
            if (!string.IsNullOrWhiteSpace(this.Accept))
            {
                FileFromMyComputer.Attributes["accept"] = this.Accept;
            }
            FileFromMyComputer.ID = "FileFromMyComputer";
            div.Controls.Add(FileFromMyComputer);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this.Page.IsPostBack)
            {
                this.EnsureChildControls();
                this.UploadFile();
            }
        }

        /// <summary>
        /// Raises the System.Web.UI.Control.PreRender event and registers the client scripts and style sheets for the control.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            this.CssClass = "dropzone";

            RegisterControlStyleSheet(this.Page, "Styles.dropzone.css");

            ScriptManager.RegisterClientScriptInclude(this.Page, this.Page.GetType(), "Scripts.dropzone.js", ResourceHandler.GetWebResourceUrl("Scripts.dropzone.js", true));

            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), this.ClientID, this.ClientScript, true);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Commits all the changes since the last time AcceptChanges was called.
        /// </summary>
        public void AcceptChanges()
        {
            string blobNameFormat = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{{0}}", this.ObjectType, this.ObjectId);

            IEnumerable<IListBlobItem> temporaryBlobList = this.TemporaryContainer.ListBlobs(this.TemporaryBlobPath);
            foreach (IListBlobItem item in temporaryBlobList)
            {
                CloudBlockBlob tempBlob = item as CloudBlockBlob;
                if (tempBlob.Properties.BlobType == BlobType.BlockBlob)
                {
                    string blobName = tempBlob.Name;
                    string[] parts = tempBlob.Name.Split('/');
                    int length = parts.Length;
                    if (length > 0)
                    {
                        blobName = string.Format(CultureInfo.InvariantCulture, blobNameFormat, parts[length - 1]);
                    }

                    CloudBlockBlob blob = this.Container.GetBlockBlobReference(blobName);
                    blob.StartCopyFromBlob(tempBlob);

                    tempBlob.Delete();
                }
            }

            this.TemporaryBlobPath = null;
        }

        /// <summary>
        /// Rolls back all changes that have been made to the control since it was loaded, or the last time AcceptChanges was called.
        /// </summary>
        public void RejectChanges()
        {
            IEnumerable<IListBlobItem> temporaryBlobList = this.TemporaryContainer.ListBlobs(this.TemporaryBlobPath);
            foreach (IListBlobItem item in temporaryBlobList)
            {
                CloudBlockBlob tempBlob = item as CloudBlockBlob;
                tempBlob.Delete();
            }

            this.TemporaryBlobPath = null;
        }

        #endregion
    }
}
