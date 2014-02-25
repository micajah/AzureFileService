using Micajah.AzureFileService.Properties;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Micajah.AzureFileService.WebControls
{
    /// <summary>
    /// The control for single- and multi-file uploads.
    /// </summary>
    [ToolboxData("<{0}:FileUpload runat=server></{0}:FileUpload>")]
    public class FileUpload : WebControl, INamingContainer
    {
        #region Members

        protected Panel FallbackPanel;
        protected System.Web.UI.WebControls.FileUpload FileFromMyComputer;
        protected CustomValidator Validator;

        private FileManager m_FileManager;

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
        /// Gets or set a value indicating whether the error message is displayed in the control.
        /// </summary>
        [Category("Appearance")]
        [Description("Whether the error message is displayed in the control.")]
        [DefaultValue(true)]
        public bool ShowErrorMessage
        {
            get
            {
                object obj = ViewState["ShowErrorMessage"];
                return ((obj == null) ? true : (bool)obj);
            }
            set { ViewState["ShowErrorMessage"] = value; }
        }

        /// <summary>
        /// Gets the message that describes the error, if it is occured.
        /// </summary>
        [Browsable(false)]
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Gets a value indicating that an error occurred.
        /// </summary>
        [Browsable(false)]
        public bool ErrorOccurred
        {
            get
            {
                return (!string.IsNullOrEmpty(this.ErrorMessage));
            }
        }

        /// <summary>
        /// Gets the names of the successfully uploaded files.
        /// </summary>
        [Browsable(false)]
        public ReadOnlyCollection<string> UploadedFileNames
        {
            get
            {
                return new ReadOnlyCollection<string>(this.FileManager.GetTemporaryFileNames(this.TemporaryDirectoryName));
            }
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
            set
            {
                this.ViewState["TemporaryContainerName"] = value;

                if (m_FileManager != null)
                {
                    m_FileManager.TemporaryContainerName = value;
                }
            }
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
            set
            {
                this.ViewState["ContainerName"] = value;

                if (m_FileManager != null)
                {
                    m_FileManager.ContainerName = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the public access to the files is allowed in the container.
        /// </summary>
        [Category("Behavior")]
        [Description("Whether the public access to the files is allowed in the container.")]
        [DefaultValue(false)]
        public bool ContainerPublicAccess
        {
            get
            {
                object obj = this.ViewState["ContainerPublicAccess"];
                return ((obj == null) ? false : (bool)obj);
            }
            set
            {
                this.ViewState["ContainerPublicAccess"] = value;

                if (m_FileManager != null)
                {
                    m_FileManager.ContainerPublicAccess = value;
                    m_FileManager.ContainerName = this.ContainerName;
                }
            }
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
            set
            {
                this.ViewState["ObjectType"] = value;

                if (m_FileManager != null)
                {
                    m_FileManager.ObjectType = value;
                }
            }
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
            set
            {
                this.ViewState["ObjectId"] = value;

                if (m_FileManager != null)
                {
                    m_FileManager.ObjectId = value;
                }
            }
        }

        [Browsable(false)]
        public FileManager FileManager
        {
            get
            {
                if (m_FileManager == null)
                {
                    m_FileManager = new FileManager(this.ContainerName, this.ContainerPublicAccess, this.ObjectType, this.ObjectId, this.TemporaryContainerName);
                }
                return m_FileManager;
            }
        }

        #endregion

        #region Overriden Properties

        protected override HtmlTextWriterTag TagKey
        {
            get { return HtmlTextWriterTag.Div; }
        }

        #endregion

        #region Private Properties

        private int MaxFileSizeInMB
        {
            get
            {
                return this.MaxFileSize / 1024 / 1024;
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
                    , this.FileManager.GetTemporaryFilesUploadUrlFormat(this.TemporaryDirectoryName));
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

        private string TemporaryDirectoryName
        {
            get
            {
                string value = (string)this.ViewState["TemporaryDirectoryName"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = Guid.NewGuid().ToString("N");
                    this.TemporaryDirectoryName = value;
                }
                return value;
            }
            set { this.ViewState["TemporaryDirectoryName"] = value; }
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
                            string fileName = Path.GetFileName(file.FileName);

                            this.FileManager.UploadTemporaryFile(fileName, file.ContentType, file.InputStream, this.TemporaryDirectoryName);
                        }
                        else
                            this.ErrorMessage = Resources.FileUpload_InvalidFileSize;
                    }
                    else
                        this.ErrorMessage = Resources.FileUpload_InvalidMimeType;
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

        private void Validator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (this.ErrorOccurred)
            {
                args.IsValid = false;
                Validator.ErrorMessage = (this.ShowErrorMessage ? this.ErrorMessage : string.Empty);
            }
            else
            {
                args.IsValid = true;
                Validator.ErrorMessage = string.Empty;
            }
        }

        #endregion

        #region Overriden Methods

        protected override void CreateChildControls()
        {
            FallbackPanel = new Panel();
            FallbackPanel.ID = "FallbackPanel";
            FallbackPanel.CssClass = "fallback";
            this.Controls.Add(FallbackPanel);

            FileFromMyComputer = new System.Web.UI.WebControls.FileUpload();
            if (!string.IsNullOrWhiteSpace(this.Accept))
            {
                FileFromMyComputer.Attributes["accept"] = this.Accept;
            }
            FileFromMyComputer.ID = "FileFromMyComputer";
            FallbackPanel.Controls.Add(FileFromMyComputer);

            Validator = new CustomValidator();
            Validator.ID = "Validator";
            Validator.Display = ValidatorDisplay.Dynamic;
            Validator.CssClass = "dz-error-message";
            Validator.ForeColor = Color.Empty;
            Validator.ServerValidate += new ServerValidateEventHandler(Validator_ServerValidate);
            FallbackPanel.Controls.Add(Validator);
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

            this.RegisterStyleSheet("Styles.dropzone.css");

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
            this.FileManager.MoveTemporaryFiles(this.TemporaryDirectoryName);

            this.TemporaryDirectoryName = null;
        }

        /// <summary>
        /// Rolls back all changes that have been made to the control since it was loaded, or the last time AcceptChanges was called.
        /// </summary>
        public void RejectChanges()
        {
            this.FileManager.DeleteTemporaryFiles(this.TemporaryDirectoryName);

            this.TemporaryDirectoryName = null;
        }

        #endregion
    }
}
