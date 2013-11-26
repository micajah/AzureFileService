using Micajah.AzureFileService.Handlers;
using Micajah.AzureFileService.Properties;
using System;
using System.ComponentModel;
using System.Globalization;
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
    public class FileUpload : WebControl, IUploadControl
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets a comma-separated list of MIME encodings used to constrain the file types the user can select.
        /// </summary>
        [Category("Behavior")]
        [DefaultValue("")]
        public string Accept { get; set; }

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

        /// <summary>
        /// Raises the System.Web.UI.Control.PreRender event and registers the client scripts and style sheets for the control.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.CssClass = "dropzone";

            RegisterControlStyleSheet(this.Page, "Styles.dropzone.css");

            ScriptManager.RegisterClientScriptInclude(this.Page, this.Page.GetType(), "Scripts.dropzone.js", ResourceHandler.GetWebResourceUrl("Scripts.dropzone.js", true));

            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), this.ClientID
                , string.Format(CultureInfo.InvariantCulture, "var {0} = new Dropzone(\"#{1}\",{{url:\"{2}\",headers:{{\"OrganizationId\":\"{3}\",\"InstanceId\":\"{4}\",\"LocalObjectType\":\"{5}\",\"LocalObjectId\":\"{6}\"}},{7}dictDefaultMessage:\"{8}\"}});\r\n"
                    , char.ToLowerInvariant(this.ClientID[0]) + this.ClientID.Substring(1), this.ClientID
                    , VirtualPathUtility.ToAbsolute(ResourceHandler.ResourceHandlerVirtualPath)
                    , this.OrganizationId, this.InstanceId, this.LocalObjectType, this.LocalObjectId
                    , (string.IsNullOrWhiteSpace(this.Accept) ? string.Empty : string.Format(CultureInfo.InvariantCulture, "acceptedFiles:\"{0}\",", this.Accept))
                    , Resources.FileUpload_DefaultMessage
                )
                , true);
        }

        #endregion
    }
}
