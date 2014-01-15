using Micajah.AzureFileService.Properties;
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

        protected System.Web.UI.WebControls.FileUpload FileFromMyComputer;

        private BlobClient m_Client;

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
                    value = Settings.TemporaryContainerName;
                }
                return value;
            }
            set { this.ViewState["TemporaryContainerName"] = value; }
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
                return ((obj == null) ? Settings.StorageConnectionString : (string)obj);
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

        private BlobClient Client
        {
            get
            {
                if (m_Client == null)
                {
                    m_Client = new BlobClient()
                    {
                        ContainerName = this.ContainerName,
                        ObjectId = this.ObjectId,
                        ObjectType = this.ObjectType,
                        StorageConnectionString = this.StorageConnectionString,
                        TemporaryContainerName = this.TemporaryContainerName
                    };
                }
                return m_Client;
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

        private string UploadUri
        {
            get
            {
                string sas = this.Client.TemporaryContainer.GetSharedAccessSignature(BlobClient.WriteAccessPolicy);

                return string.Format(CultureInfo.InvariantCulture, "{0}/{1}{{0}}{2}", this.Client.TemporaryContainer.Uri.AbsoluteUri, this.TemporaryBlobPath, sas);
            }
        }

        private string ClientScript
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(CultureInfo.InvariantCulture, "Dropzone.options.{0} = false;\r\nvar {1} = new Dropzone(\"#{2}\",{{method:\"Put\",createImageThumbnails:false,paramName:\"{3}\",url:\"{4}\""
                    , this.ClientID.Replace("_", string.Empty)
                    , (char.ToLowerInvariant(this.ClientID[0]) + this.ClientID.Substring(1)).Replace("_", string.Empty)
                    , this.ClientID
                    , FileFromMyComputer.UniqueID
                    , this.UploadUri);
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
                sb.AppendFormat(CultureInfo.InvariantCulture, ",cacheControl:\"{0}\",dictDefaultMessage:\"{1}\"}});\r\n", Settings.ClientCacheControl, Resources.FileUpload_DefaultMessage);
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

                            this.Client.UploadToTemporaryContainer(blobName, file.ContentType, file.InputStream);
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
            HtmlGenericControl div = null;

            try
            {
                div = new HtmlGenericControl("div");
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
            finally
            {
                if (div != null)
                {
                    div.Dispose();
                }
            }
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

            Helper.RegisterControlStyleSheet(this.Page, "Styles.dropzone.css");

            ScriptManager.RegisterClientScriptInclude(this.Page, this.Page.GetType(), "Scripts.dropzone.js", ResourceHandler.GetWebResourceUrl("Scripts.dropzone.js", true));

            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), this.ClientID, this.ClientScript, true);
        }

        /// <summary>
        /// Renders the control.
        /// </summary>
        /// <param name="writer">A System.Web.UI.HtmlTextWriter that contains the output stream to render on the client.</param>
        public override void RenderControl(HtmlTextWriter writer)
        {
            if (writer == null) return;

            if (this.DesignMode)
            {
                writer.Write(string.Format(CultureInfo.InvariantCulture, "[{0} \"{1}\"]", this.GetType().Name, this.ID));
            }
            else
            {
                base.RenderControl(writer);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Commits all the changes since the last time AcceptChanges was called.
        /// </summary>
        public void AcceptChanges()
        {
            this.Client.MoveFromTemporaryContainerToContainer(this.TemporaryBlobPath);

            this.TemporaryBlobPath = null;
        }

        /// <summary>
        /// Rolls back all changes that have been made to the control since it was loaded, or the last time AcceptChanges was called.
        /// </summary>
        public void RejectChanges()
        {
            this.Client.DeleteFromTemporaryContainer(this.TemporaryBlobPath);

            this.TemporaryBlobPath = null;
        }

        #endregion
    }
}
