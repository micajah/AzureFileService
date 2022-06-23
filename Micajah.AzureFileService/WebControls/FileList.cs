using Micajah.AzureFileService.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Micajah.AzureFileService.WebControls
{
    public partial class FileList : Control, INamingContainer
    {
        #region Constants

        private const string FileIdColumnName = "FileId";
        private const string NameColumnName = "Name";
        private const string UrlColumnName = "Url";
        private const string LengthInKBColumnName = "LengthInKB";
        private const string DeleteCommandName = "Delete";
        private const string OnDeletingClientScript = "return flDel();";
        private const string DeletingClientScript = "function flDel() {{ return window.confirm(\"{0}\"); }}\r\n";
        private const string AdapterClientScript = "Opentip.adapters = {}; Opentip.adapter = null; firstAdapter = true; Opentip.addAdapter(new Adapter);";
        private const string ToolTipBigHtml = "<div class=\"flToolTip s600x500\"><span></span><a class=\"flFileName\" target=\"_blank\" rel=\"noopener\" href=\"{0}\"><img alt=\"{1}\" src=\"{2}\"></a></div>";
        private const string ToolTipSmallHtml = "<div class=\"flToolTip s250\"><a class=\"flFileName\" href=\"{0}\" target=\"_blank\" rel=\"noopener\">{1}</a><span class=\"flFileInfo\">{2}, {3:N0} KB</span>{4}</div>";
        private const string DeleteLinkHtml = "<a class=\"flRemove\" href=\"{0}\" title=\"{1}\"{2}>{3}</a>";

        #endregion

        #region Members

        private GridView Grid;
        private DataList List;
        private HyperLink ViewAllAtOnceLink;
        private Panel CaptionPanel;

        private DateTime m_UpdatedDate = DateTime.MinValue;
        private TimeZoneInfo m_TimeZone;
        private FileManager m_FileManager;

        #endregion

        #region Events

        /// <summary>
        /// Occurs after a file is deleted.
        /// </summary>
        public event CommandEventHandler FileDeleted;

        #endregion

        #region Private Properties

        private string[] FileExtensionsFilterInternal
        {
            get
            {
                List<string> extensions = new List<string>(FileExtensionsFilter);

                extensions.AddRange(MimeType.GetExtensions(FileExtensionsFilter));

                var result = extensions.Distinct().ToArray();

                return result;
            }
        }

        private string ViewAllAtOnceLinkNavigateUrl
        {
            get
            {
                string str = string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}|{3}|{4}", this.ContainerName, this.ContainerPublicAccess, this.ObjectType, this.ObjectId, this.NegateFileExtensionsFilter);

                return ResourceVirtualPathProvider.VirtualPathToAbsolute(ResourceProvider.FileListPageVirtualPath)
                    + "?d=" + HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(str));
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the file extensions for displaying or not, if the NegateFileExtensionsFilter is true.
        /// </summary>
        [Category("Appearance")]
        [Description("The the file extensions for displaying.")]
        [DefaultValue(typeof(string[]), "")]
        [TypeConverter(typeof(StringArrayConverter))]
        public string[] FileExtensionsFilter
        {
            get
            {
                object obj = this.ViewState["FileExtensionsFilter"];
                return (obj == null) ? new string[0] : (string[])obj;
            }
            set { this.ViewState["FileExtensionsFilter"] = value; }
        }

        /// <summary>
        /// Gets or sets the culture in which the date and time will be formatted.
        /// </summary>
        [Category("Appearance")]
        [Description("The culture in which the date and time will be formatted.")]
        [DefaultValue(typeof(CultureInfo), "en-US")]
        public CultureInfo Culture
        {
            get
            {
                object obj = this.ViewState["Culture"];
                return (obj == null) ? CultureInfo.CurrentCulture : (CultureInfo)obj;
            }
            set { this.ViewState["Culture"] = value; }
        }

        /// <summary>
        /// Gets or sets the string that specifies the display format for the tool tip of the date column.
        /// </summary>
        [Category("Appearance")]
        [Description("The string that specifies the display format for the tool tip of the date column.")]
        [DefaultValue("{0:MMM d, yyyy H:mm}")]
        public string DateTimeToolTipFormatString
        {
            get
            {
                object obj = this.ViewState["DateTimeToolTipFormatString"];
                return (obj == null) ? "{0:MMM d, yyyy H:mm}" : (string)obj;
            }
            set { this.ViewState["DateTimeToolTipFormatString"] = value; }
        }

        /// <summary>
        /// Gets or sets the string that specifies the display format for the date column.
        /// </summary>
        [Category("Appearance")]
        [Description("The string that specifies the display format for the date column.")]
        [DefaultValue("{0:d-MMM-yyyy}")]
        public string DateTimeFormatString
        {
            get
            {
                object obj = this.ViewState["DateTimeFormatString"];
                return (obj == null) ? "{0:d-MMM-yyyy}" : (string)obj;
            }
            set { this.ViewState["DateTimeFormatString"] = value; }
        }

        /// <summary>
        /// Gets or sets the text of the delete button.
        /// </summary>
        [Category("Appearance")]
        [Description("The text of the delete button.")]
        [DefaultValue("delete")] // Must be sync with Resources.FileList_DeleteText.
        public string DeleteButtonText
        {
            get
            {
                object obj = this.ViewState["DeleteButtonText"];
                return (obj == null) ? Resources.FileList_DeleteText : (string)obj;
            }
            set { this.ViewState["DeleteButtonText"] = value; }
        }

        /// <summary>
        /// Gets or set time zone identifier.
        /// </summary>
        [Category("Appearance")]
        [Description("The time zone identifier.")]
        [DefaultValue("Eastern Standard Time")]
        public string TimeZoneId
        {
            get
            {
                string str = (string)ViewState["TimeZoneId"];
                return string.IsNullOrEmpty(str) ? "Eastern Standard Time" : str;
            }
            set { ViewState["TimeZoneId"] = value; }
        }

        /// <summary>
        /// Gets or set the time zone.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(typeof(TimeZoneInfo), "Eastern Standard Time")]
        public TimeZoneInfo TimeZone
        {
            get
            {
                if (m_TimeZone == null)
                    m_TimeZone = TimeZoneInfo.FindSystemTimeZoneById(this.TimeZoneId);
                return m_TimeZone;
            }
            set
            {
                m_TimeZone = value;
                if (value == null)
                    this.TimeZoneId = null;
                else
                    this.TimeZoneId = value.Id;
            }
        }

        /// <summary>
        /// Gets or set value indicating that the deleting is enabled or disabled.
        /// </summary>
        [Category("Behavior")]
        [Description("Whether the deleting is enabled.")]
        [DefaultValue(true)]
        public bool EnableDeleting
        {
            get
            {
                object obj = ViewState["EnableDeleting"];
                return (obj == null) ? true : (bool)obj;
            }
            set { ViewState["EnableDeleting"] = value; }
        }

        /// <summary>
        /// Gets or set value indicating that the deleting requires confirmation.
        /// </summary>
        [Category("Behavior")]
        [Description("Whether the deleting requires confirmation.")]
        [DefaultValue(true)]
        public bool EnableDeletingConfirmation
        {
            get
            {
                object obj = ViewState["EnableDeletingConfirmation"];
                return (obj == null) ? true : (bool)obj;
            }
            set { ViewState["EnableDeletingConfirmation"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the tool tip for a file displays the thumbnail or the file itself.
        /// </summary>
        [Category("Appearance")]
        [Description("Whether the tool tip for a file displays the thumbnail or the file itself.")]
        [DefaultValue(true)]
        public bool EnableThumbnails
        {
            get
            {
                object obj = this.ViewState["EnableThumbnails"];
                return (obj == null) ? true : (bool)obj;
            }
            set { this.ViewState["EnableThumbnails"] = value; }
        }

        /// <summary>
        /// Gets the files count in the control.
        /// </summary>
        [Browsable(false)]
        public int FilesCount
        {
            get
            {
                object obj = ViewState["FilesCount"];
                return (obj == null) ? 0 : (int)obj;
            }
            private set { ViewState["FilesCount"] = value; }
        }

        /// <summary>
        /// Gets or sets the size of pictures in icons column.
        /// </summary>
        [Category("Appearance")]
        [Description("The size of pictures in icons column.")]
        [DefaultValue(IconSize.Smaller)]
        public IconSize IconSize
        {
            get
            {
                object obj = this.ViewState["IconSize"];
                return (obj == null) ? IconSize.Smaller : (IconSize)obj;
            }
            set { this.ViewState["IconSize"] = value; }
        }

        /// <summary>
        /// Gets a value indicating that the control is empty.
        /// </summary>
        [Browsable(false)]
        public bool IsEmpty
        {
            get { return this.FilesCount == 0; }
        }

        /// <summary>
        /// Gets or set a value indicating that the FileExtensionsFilter be negated.
        /// </summary>
        [Category("Behavior")]
        [Description("Whether the FileExtensionsFilter be negated.")]
        [DefaultValue(false)]
        public bool NegateFileExtensionsFilter
        {
            get
            {
                object obj = ViewState["NegateFileExtensionsFilter"];
                return (obj == null) ? false : (bool)obj;
            }
            set { ViewState["NegateFileExtensionsFilter"] = value; }
        }

        /// <summary>
        /// Gets or sets the rendering mode for the control.
        /// </summary>
        [Category("Appearance")]
        [Description("The rendering mode.")]
        [DefaultValue(FileListRenderingMode.Grid)]
        public FileListRenderingMode RenderingMode
        {
            get
            {
                object obj = this.ViewState["RenderingMode"];
                return (obj == null) ? FileListRenderingMode.Grid : (FileListRenderingMode)obj;
            }
            set { this.ViewState["RenderingMode"] = value; }
        }

        /// <summary>
        /// Gets or sets the number of columns to display in the thumbnails list.
        /// </summary>
        [Category("Appearance")]
        [Description("The number of columns to display in the thumbnails list.")]
        [DefaultValue(4)]
        public int RepeatColumns
        {
            get
            {
                object obj = this.ViewState["RepeatColumns"];
                return (obj == null) ? 4 : (int)obj;
            }
            set { this.ViewState["RepeatColumns"] = value; }
        }

        /// <summary>
        /// Gets or sets whether the thumbnails list displays vertically or horizontally.
        /// </summary>
        [Category("Appearance")]
        [Description("Whether the thumbnails list displays vertically or horizontally.")]
        [DefaultValue(RepeatDirection.Horizontal)]
        public RepeatDirection RepeatDirection
        {
            get
            {
                object obj = this.ViewState["RepeatDirection"];
                return (obj == null) ? RepeatDirection.Horizontal : (RepeatDirection)obj;
            }
            set { this.ViewState["RepeatDirection"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the tool tip for the file is displayed.
        /// </summary>
        [Category("Appearance")]
        [Description("Whether the tool tip for the file is displayed.")]
        [DefaultValue(true)]
        public bool ShowFileToolTip
        {
            get
            {
                object obj = this.ViewState["ShowFileToolTip"];
                return (obj == null) ? true : (bool)obj;
            }
            set { this.ViewState["ShowFileToolTip"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the icons column is displayed in control.
        /// </summary>
        [Category("Appearance")]
        [Description("Whether the icons column is displayed in control.")]
        [DefaultValue(false)]
        public bool ShowIcons
        {
            get
            {
                object obj = this.ViewState["ShowIcons"];
                return ((obj == null) ? false : (bool)obj) && (this.IconSize != IconSize.NotSet);
            }
            set { this.ViewState["ShowIcons"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the view at once hyperlink is displayed in control.
        /// </summary>
        [Category("Appearance")]
        [Description("Whether the view at once hyperlink is displayed in control.")]
        [DefaultValue(true)]
        public bool ShowViewAllAtOnceLink
        {
            get
            {
                object obj = this.ViewState["ShowViewAllAtOnceLink"];
                return (obj == null) ? true : (bool)obj;
            }
            set { this.ViewState["ShowViewAllAtOnceLink"] = value; }
        }

        /// <summary>
        /// Gets or sets the width of the control.
        /// </summary>
        [Category("Layout")]
        [Description("The width of the control.")]
        [DefaultValue(typeof(Unit), "")]
        public Unit Width
        {
            get
            {
                object obj = this.ViewState["Width"];
                return (obj == null) ? Unit.Empty : (Unit)obj;
            }
            set { this.ViewState["Width"] = value; }
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
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                this.ViewState["ContainerPublicAccess"] = value;

                if (m_FileManager != null)
                {
                    m_FileManager.ContainerPublicAccess = value;
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
                    m_FileManager = new FileManager(this.ContainerName, this.ContainerPublicAccess, this.ObjectType, this.ObjectId);
                }
                return m_FileManager;
            }
        }

        #endregion

        #region Private Methods

        private static string GetFileTypeIconCssClass(MimeTypeGroups mimeTypeGroups)
        {
            string cssClass;

            if ((mimeTypeGroups & MimeTypeGroups.Archive) == MimeTypeGroups.Archive)
            {
                cssClass = "bi bi-file-earmark-zip";
            }
            else if ((mimeTypeGroups & MimeTypeGroups.Audio) == MimeTypeGroups.Audio)
            {
                cssClass = "bi bi-file-earmark-music";
            }
            else if ((mimeTypeGroups & MimeTypeGroups.Document) == MimeTypeGroups.Document)
            {
                cssClass = "bi bi-file-earmark-richtext";
            }
            else if ((mimeTypeGroups & MimeTypeGroups.Image) == MimeTypeGroups.Image)
            {
                cssClass = "bi bi-file-earmark-image";
            }
            else if ((mimeTypeGroups & MimeTypeGroups.Message) == MimeTypeGroups.Message)
            {
                cssClass = "bi bi-file-earmark-post";
            }
            else if ((mimeTypeGroups & MimeTypeGroups.Text) == MimeTypeGroups.Text)
            {
                cssClass = "bi bi-file-earmark-text";
            }
            else if ((mimeTypeGroups & MimeTypeGroups.Video) == MimeTypeGroups.Video)
            {
                cssClass = "bi bi-file-earmark-play";
            }
            else
            {
                cssClass = "bi bi-file-earmark";
            }

            return cssClass;
        }

        private void ApplyStyle()
        {
            if (Grid != null)
            {
                if (!this.Width.IsEmpty)
                {
                    Grid.Width = this.Width;
                    Grid.Columns[this.ShowIcons ? 1 : 0].ItemStyle.Width = Unit.Percentage(100);
                }

                Grid.CellPadding = -1;
                Grid.CellSpacing = 0;
                if (this.ShowIcons)
                {
                    if (this.IconSize != IconSize.Smaller)
                        Grid.CellPadding = 0;
                    else
                        Grid.CellSpacing = -1;
                }
                else
                    Grid.CellSpacing = -1;
            }
        }

        private void CreateGridView()
        {
            Grid = new GridView();
            Grid.ID = "Grid";
            Grid.CssClass = "flGrid";
            Grid.DataKeyNames = new string[] { FileIdColumnName };
            Grid.AutoGenerateColumns = false;
            Grid.GridLines = GridLines.None;
            Grid.ShowHeader = false;
            Grid.RowDataBound += new GridViewRowEventHandler(Grid_RowDataBound);
            Grid.RowDeleting += new GridViewDeleteEventHandler(Grid_RowDeleting);

            HyperLinkField linkField = new HyperLinkField();
            linkField.DataNavigateUrlFields = new string[] { UrlColumnName };
            linkField.DataTextField = NameColumnName;
            linkField.HeaderText = Resources.FileList_FileNameText;
            linkField.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
            linkField.Target = "_blank";

            if (this.ShowIcons)
            {
                BoundField iconField = new BoundField();
                Grid.Columns.Add(iconField);
            }
            else
            {
                linkField.ItemStyle.CssClass = "flFirst";
            }

            Grid.Columns.Add(linkField);

            BoundField lengthField = new BoundField();
            lengthField.DataField = LengthInKBColumnName;
            lengthField.DataFormatString = "{0:N0} KB";
            lengthField.HeaderStyle.Wrap = false;
            lengthField.HeaderText = Resources.FileList_SizeText;
            lengthField.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
            lengthField.ItemStyle.Wrap = false;
            Grid.Columns.Add(lengthField);

            if (this.EnableDeleting)
            {
                ButtonField deleteField = new ButtonField();
                deleteField.CausesValidation = false;
                deleteField.CommandName = DataList.DeleteCommandName;
                deleteField.Text = this.DeleteButtonText;
                deleteField.ControlStyle.CssClass = "flRemove";
                deleteField.ItemStyle.Wrap = false;
                Grid.Columns.Add(deleteField);
            }
        }

        private void CreateDataList()
        {
            List = new DataList();
            List.ID = "Grid";
            List.CellSpacing = 0;
            List.CellPadding = 0;
            List.DataKeyField = FileIdColumnName;
            List.DeleteCommand += new DataListCommandEventHandler(DataList_DeleteCommand);

            switch (this.RenderingMode)
            {
                case FileListRenderingMode.Thumbnails:
                    List.CssClass = "flThumbs";
                    List.ItemTemplate = new ThumbnailsListItemTemplate(ListItemType.Item, this);
                    List.RepeatColumns = this.RepeatColumns;
                    List.RepeatDirection = this.RepeatDirection;
                    break;
                case FileListRenderingMode.List:
                    List.CssClass = "flFiles";
                    List.ItemTemplate = new FilesListItemTemplate(ListItemType.Item, this);
                    break;
            }
        }

        private void DeleteFile(string fileId)
        {
            this.FileManager.DeleteFile(fileId);

            if (this.FileDeleted != null)
            {
                string name = FileManager.GetNameFromFileId(fileId);

                File file = new File()
                {
                    FileId = fileId,
                    Name = name
                };

                this.FileDeleted(this, new CommandEventArgs(DeleteCommandName, file));
            }
        }

        private void EnsureGrid()
        {
            if ((Grid == null) && (List == null))
            {
                if ((this.RenderingMode == FileListRenderingMode.Thumbnails) || (this.RenderingMode == FileListRenderingMode.List))
                    this.CreateDataList();
                else
                    this.CreateGridView();
            }
        }

        private void EnsureGridCaptionPanel()
        {
            CaptionPanel = new Panel();
            CaptionPanel.ID = "CaptionPanel";

            if (this.ShowViewAllAtOnceLink)
            {
                ViewAllAtOnceLink = new HyperLink();
                ViewAllAtOnceLink.ID = "ViewAllAtOnceLink";
                ViewAllAtOnceLink.CssClass = "flCptCtrl flLink";
                ViewAllAtOnceLink.Text = Resources.FileList_ViewAllAtOnceLink_Text;
                ViewAllAtOnceLink.Target = "_blank";
                ViewAllAtOnceLink.NavigateUrl = "#";

                CaptionPanel.Controls.Add(ViewAllAtOnceLink);
            }
        }

        private void GridDataBind()
        {
            this.FilesCount = 0;

            m_UpdatedDate = DateTime.MinValue;

            Collection<File> files = this.FileManager.GetFiles(new FileSearchOptions()
            {
                ExtensionsFilter = this.FileExtensionsFilterInternal,
                NegateExtensionsFilter = this.NegateFileExtensionsFilter
            });

            if (Grid != null)
            {
                Grid.DataSource = files;
                Grid.DataBind();
            }
            else if (List != null)
            {
                List.DataSource = files;
                List.DataBind();
            }

            this.FilesCount = files.Count;
        }

        private void DataList_DeleteCommand(object source, DataListCommandEventArgs e)
        {
            DeleteFile((string)List.DataKeys[e.Item.ItemIndex]);
        }

        private void Grid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            if (e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }

            int count = e.Row.Cells.Count;
            if (count == 0)
            {
                return;
            }

            File file = (File)e.Row.DataItem;

            var fileMimeTypeGroups = MimeType.GetGroups(file.Name, true);

            TableCell fileNameCell;
            string dateCssClass = "flDate";

            if (this.ShowIcons)
            {
                fileNameCell = e.Row.Cells[1];

                dateCssClass += " flIcons";

                e.Row.Cells[0].CssClass = "flFirst " + GetFileTypeIconCssClass(fileMimeTypeGroups);

                string fontSize = string.Format(CultureInfo.InvariantCulture, "{0}px", (int)this.IconSize);
                e.Row.Cells[0].Font.Size = FontUnit.Parse(fontSize, CultureInfo.InvariantCulture);
                e.Row.Cells[0].Style["line-height"] = fontSize;
            }
            else
            {
                fileNameCell = e.Row.Cells[0];
            }

            HyperLink link = fileNameCell.Controls[0] as HyperLink;
            if (link == null)
            {
                link = fileNameCell.Controls[1] as HyperLink;
            }

            if (link != null)
            {
                link.Attributes["rel"] = "noopener";

                if (this.ShowFileToolTip)
                {
                    link.CssClass = "flFileName";

                    if ((fileMimeTypeGroups & MimeTypeGroups.Image) == MimeTypeGroups.Image)
                    {
                        string thumbUrl = this.EnableThumbnails ? this.FileManager.GetThumbnailUrl(file.FileId, 600, 500, 1, true) : file.Url;
                        string content = string.Format(CultureInfo.InvariantCulture, ToolTipBigHtml, file.Url, file.Name, thumbUrl);

                        link.Attributes["data-ot"] = content;
                    }
                }
            }

            DateTime updatedTime = TimeZoneInfo.ConvertTimeFromUtc(file.LastModified, this.TimeZone);
            DateTime updatedDate = file.LastModified.Date;

            if (m_UpdatedDate != updatedDate)
            {
                if (m_UpdatedDate != DateTime.MinValue)
                {
                    e.Row.CssClass += " flPt";
                }

                using (HtmlGenericControl panel = new HtmlGenericControl("div"))
                {
                    panel.InnerHtml = string.Format(this.Culture, this.DateTimeFormatString, updatedTime);
                    panel.Attributes["class"] = dateCssClass;

                    fileNameCell.Controls.AddAt(0, panel);
                }
            }
            m_UpdatedDate = updatedDate;

            if (this.EnableDeleting && this.EnableDeletingConfirmation)
            {
                TableCell deleteCell = e.Row.Cells[count - 1];

                if (deleteCell.Controls.Count > 0)
                {
                    WebControl control = deleteCell.Controls[0] as WebControl;
                    if (control != null)
                    {
                        control.Attributes.Add("onclick", OnDeletingClientScript);
                        control.ToolTip = Resources.FileList_DeleteText;
                    }
                }
            }
        }

        private void Grid_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            if (e != null)
            {
                e.Cancel = true;

                DeleteFile(e.Keys[0].ToString());
            }
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            this.EnsureGridCaptionPanel();
            this.EnsureGrid();

            if (this.RenderingMode != FileListRenderingMode.Grid)
            {
                if (CaptionPanel.HasControls())
                {
                    this.Controls.Add(CaptionPanel);
                }
            }

            if (Grid != null)
            {
                this.Controls.Add(Grid);
            }
            else if (List != null)
            {
                this.Controls.Add(List);
            }

            if (this.RenderingMode == FileListRenderingMode.Grid)
            {
                if (CaptionPanel.HasControls())
                {
                    CaptionPanel.CssClass = "flCpt";

                    this.Controls.Add(CaptionPanel);
                }
            }
        }

        /// <summary>
        /// Raises the System.Web.UI.Control.PreRender event and registers the style sheet of the control.
        /// </summary>
        /// <param name="e">An System.EventArgs object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.ApplyStyle();
            this.GridDataBind();

            Page p = this.Page;
            Type t = p.GetType();

            if (this.ShowFileToolTip)
            {
                this.RegisterStyleSheet(ResourceProvider.OpentipStyleSheet);

                ScriptManager.RegisterClientScriptInclude(p, t, "Scripts.opentip.js", ResourceProvider.GetResourceUrl(ResourceProvider.OpentipScript, true));
                ScriptManager.RegisterClientScriptInclude(p, t, "Scripts.FileList.js", ResourceProvider.GetResourceUrl(ResourceProvider.FileListScript, true));
            }

            this.RegisterStyleSheet(ResourceProvider.FileListStyleSheet);
            this.RegisterStyleSheet(ResourceProvider.BootstrapIconsStyleSheet);

            string deletingScript = string.Format(CultureInfo.CurrentCulture, DeletingClientScript, Resources.FileList_DeletingConfirmationText);
            ScriptManager.RegisterClientScriptBlock(p, t, "Scripts.FileList.Deleting", deletingScript, true);

            if (this.ShowFileToolTip)
            {
                ScriptManager sm = ScriptManager.GetCurrent(p);
                if (sm != null)
                {
                    if (sm.IsInAsyncPostBack)
                    {
                        ScriptManager.RegisterStartupScript(p, t, "Scripts.FileList.Adapter", AdapterClientScript, true);
                    }
                }
            }
        }

        /// <summary>
        /// Binds a data source to the invoked server control and all its child controls.
        /// </summary>
        public override void DataBind()
        {
            this.EnsureChildControls();
            this.GridDataBind();
        }

        /// <summary>
        /// Renders the control.
        /// </summary>
        /// <param name="writer">A System.Web.UI.HtmlTextWriter that contains the output stream to render on the client.</param>
        public override void RenderControl(HtmlTextWriter writer)
        {
            if (writer == null) return;

            if (this.DesignMode)
                writer.Write(string.Format(CultureInfo.InvariantCulture, "[{0} \"{1}\"]", this.GetType().Name, this.ID));
            else
            {
                if (ViewAllAtOnceLink != null)
                {
                    ViewAllAtOnceLink.NavigateUrl = this.ViewAllAtOnceLinkNavigateUrl;
                    ViewAllAtOnceLink.Visible = !this.IsEmpty;
                }

                base.RenderControl(writer);

                if (this.IsEmpty)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "flGrid");
                    if (Grid != null)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Id, Grid.ClientID);
                    }
                    else if (List != null)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Id, List.ClientID);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Table);
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(Resources.FileList_EmptyDataText);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
            }
        }

        #endregion

        #region Public Methods

        public void InitializeFromRequest()
        {
            string d = this.Page.Request.QueryString["d"];
            byte[] bytes = HttpServerUtility.UrlTokenDecode(d);
            if (bytes != null)
            {
                string str = Encoding.UTF8.GetString(bytes);
                string[] values = str.Split('|');

                this.ContainerName = values[0];
                this.ContainerPublicAccess = Convert.ToBoolean(values[1], CultureInfo.InvariantCulture);
                this.ObjectType = values[2];
                this.ObjectId = values[3];
                this.NegateFileExtensionsFilter = Convert.ToBoolean(values[4], CultureInfo.InvariantCulture);
            }
        }

        #endregion
    }
}
