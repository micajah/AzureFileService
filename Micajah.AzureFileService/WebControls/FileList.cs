﻿using Micajah.AzureFileService.Properties;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Micajah.AzureFileService.WebControls
{
    // TODO: Need to display the flash like in FS - add the embedded page.
    public class FileList : Control, INamingContainer, IUploadControl
    {
        #region Constants

        private const string BlobNameColumnName = "BlobName";
        private const string ContentTypeColumnName = "ContentType";
        private const string FileNameColumnName = "FileName";
        private const string UriColumnName = "Uri";
        private const string LengthColumnName = "Length";
        private const string LengthInKBColumnName = "LengthInKB";
        private const string LastModifiedColumnName = "LastModified";
        private const string LastModifiedDateColumnName = "LastModifiedDate";

        #endregion

        #region Classes

        private class FilesListItemTemplate : ITemplate, IDisposable
        {
            #region Members

            private ListItemType m_ItemType;
            private FileList m_FileList;
            private LinkButton DeleteLink;

            #endregion

            #region Constructors

            public FilesListItemTemplate(ListItemType itemType, FileList fileList)
            {
                m_ItemType = itemType;
                m_FileList = fileList;
            }

            #endregion

            #region Private Methods

            private void Image_DataBinding(object sender, EventArgs e)
            {
                Image img = (sender as Image);
                DataRowView drv = (DataRowView)DataBinder.GetDataItem(img.NamingContainer);

                string contentType = (string)drv[ContentTypeColumnName];

                string uri = GetNonImageFileTypeIconUrl((string)drv[FileNameColumnName], IconSize.Bigger);
                if (uri == null)
                {
                    uri = (string)drv[UriColumnName];
                }
                img.ImageUrl = uri;
            }

            private void HyperLink_DataBinding(object sender, EventArgs e)
            {
                HyperLink link = (HyperLink)sender;
                DataRowView drv = (DataRowView)DataBinder.GetDataItem(link.NamingContainer);

                string fileName = (string)drv[FileNameColumnName];
                string extension = Path.GetExtension(fileName);
                string contentType = (string)drv[ContentTypeColumnName];

                link.Text = fileName;
                link.NavigateUrl = (string)drv[UriColumnName];
                if ((string.Compare(extension, ".swf", StringComparison.OrdinalIgnoreCase) == 0) || MimeType.IsImageType(MimeMapping.GetMimeMapping(extension)))
                {
                    link.Target = "_blank";
                }
            }

            #endregion

            #region Public Methods

            public void InstantiateIn(Control container)
            {
                if (container == null) return;
                if ((m_ItemType != ListItemType.Item) && (m_ItemType != ListItemType.AlternatingItem)) return;

                Image img = new Image();
                img.DataBinding += new EventHandler(Image_DataBinding);

                Panel panel = new Panel();
                HyperLink link = new HyperLink();
                link.DataBinding += new EventHandler(HyperLink_DataBinding);
                link.CssClass = "flFileName";
                panel.Controls.Add(link);

                if (m_FileList.EnableDeleting)
                {
                    panel.Controls.Add(new LiteralControl("&nbsp;&nbsp;&nbsp;&nbsp;"));
                    DeleteLink = new LinkButton();
                    DeleteLink.ID = "DeleteLink";
                    DeleteLink.CommandName = DataList.DeleteCommandName;
                    DeleteLink.CausesValidation = false;
                    DeleteLink.CssClass = "flRemove";
                    DeleteLink.Text = Resources.FileList_DeleteText;
                    if (m_FileList.EnableDeletingConfirmation) DeleteLink.OnClientClick = FileList.OnClientDeleting;
                    panel.Controls.Add(DeleteLink);
                }

                container.Controls.Add(img);
                container.Controls.Add(panel);
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);

                if (DeleteLink != null)
                {
                    DeleteLink.Dispose();
                }
            }

            #endregion
        }

        private class ThumbnailsListItemTemplate : ITemplate, IDisposable
        {
            #region Members

            private ListItemType m_ItemType;
            private FileList m_FileList;
            private LinkButton DeleteLink;

            #endregion

            #region Constructors

            public ThumbnailsListItemTemplate(ListItemType itemType, FileList fileList)
            {
                m_ItemType = itemType;
                m_FileList = fileList;
            }

            #endregion

            #region Private Methods

            private void Image_DataBinding(object sender, EventArgs e)
            {
                Image img = (sender as Image);
                DataRowView drv = (DataRowView)DataBinder.GetDataItem(img.NamingContainer);

                string uri = (m_FileList.ShowVideoOnly
                    ? ResourceHandler.GetWebResourceUrl("Images.Video.gif", true)
                    : GetNonImageFileTypeIconUrl((string)drv[FileNameColumnName], IconSize.Bigger));
                if (uri == null)
                {
                    uri = (string)drv[UriColumnName];
                }
                img.ImageUrl = uri;
            }

            private void HyperLink_DataBinding(object sender, EventArgs e)
            {
                HyperLink link = (HyperLink)sender;
                DataRowView drv = (DataRowView)DataBinder.GetDataItem(link.NamingContainer);

                string fileName = (string)drv[FileNameColumnName];
                string extension = Path.GetExtension(fileName);
                string contentType = (string)drv[ContentTypeColumnName];

                link.NavigateUrl = (string)drv[UriColumnName];
                if ((string.Compare(extension, ".swf", StringComparison.OrdinalIgnoreCase) == 0) || MimeType.IsImageType(MimeMapping.GetMimeMapping(extension)))
                {
                    link.Target = "_blank";
                }
            }

            //private void Panel_DataBinding(object sender, EventArgs e)
            //{
            //    Panel panel = (Panel)sender;
            //    DataRowView drv = (DataRowView)DataBinder.GetDataItem(panel.NamingContainer);

            //    string fileName = (string)drv[FileNameColumnName];
            //    string uri = (string)drv[UriColumnName];
            //    long lengthInKB = (long)drv[LengthInKBColumnName];
            //    DateTime lastModified = (DateTime)drv[LastModifiedColumnName];

            //    string date = string.Format(m_FileList.Culture, m_FileList.DateTimeToolTipFormatString, TimeZoneInfo.ConvertTimeFromUtc(lastModified, m_FileList.TimeZone));
            //    string content = string.Format(m_FileList.Culture,
            //        "<div style=\"width: 250px\" class=\"flToolTip\"><a class=\"flFileName\" href=\"{0}\" target=\"_blank\">{1}</a><span class=\"flFileInfo\">{2}, {3:N0} KB</span>DELETE_LINK</div>",
            //        uri, fileName, date, lengthInKB);

            //    panel.Attributes["data-ot"] = content;
            //}

            #endregion

            #region Public Methods

            public void InstantiateIn(Control container)
            {
                if (container == null) return;
                if ((m_ItemType != ListItemType.Item) && (m_ItemType != ListItemType.AlternatingItem)) return;

                Image img = new Image();
                img.DataBinding += new EventHandler(Image_DataBinding);
                img.Width = img.Height = Unit.Pixel(m_FileList.ShowVideoOnly ? 148 : 128);

                HyperLink link = new HyperLink();
                link.DataBinding += new EventHandler(HyperLink_DataBinding);
                link.Controls.Add(img);

                Panel panel = new Panel();
                panel.ID = "ThumbPanel";
                panel.Width = panel.Height = Unit.Pixel(m_FileList.ShowVideoOnly ? 148 : 128);
                panel.Style[HtmlTextWriterStyle.BackgroundColor] = "White";
                //panel.DataBinding += new EventHandler(Panel_DataBinding);
                panel.Attributes["data-ot-style"] = "fileInfo";
                panel.Attributes["data-ot-group"] = m_FileList.ClientID;
                panel.Attributes["data-ot"] = DateTime.UtcNow.Ticks.ToString();
                panel.Controls.Add(link);

                if (m_FileList.EnableDeleting)
                {
                    DeleteLink = new LinkButton();
                    DeleteLink.ID = "DeleteLink";
                    DeleteLink.CommandName = DataList.DeleteCommandName;
                    DeleteLink.CausesValidation = false;
                    DeleteLink.CssClass = "flRemove";
                    DeleteLink.Text = Resources.FileList_DeleteText;
                    DeleteLink.Style[HtmlTextWriterStyle.Display] = "none";
                    if (m_FileList.EnableDeletingConfirmation)
                    {
                        DeleteLink.OnClientClick = FileList.OnClientDeleting;
                    }
                    panel.Controls.Add(DeleteLink);
                }

                container.Controls.Add(panel);
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);

                if (DeleteLink != null)
                {
                    DeleteLink.Dispose();
                }
            }

            #endregion
        }

        #endregion

        #region Members

        private GridView Grid;
        private DataList List;
        private HyperLink ViewAllAtOnceLink;
        private Panel CaptionPanel;

        private DateTime m_UpdatedDate = DateTime.MinValue;
        private TimeZoneInfo m_TimeZone;
        private static ReadOnlyCollection<string> s_KnownFileExtensions;
        private CloudBlobClient m_Client;
        private CloudBlobContainer m_Container;

        #endregion

        #region Events

        /// <summary>
        /// Occurs after a file is deleted.
        /// </summary>
        public event CommandEventHandler FileDeleted;

        #endregion

        #region Private Properties

        private static ReadOnlyCollection<string> KnownFileExtensions
        {
            get
            {
                if (s_KnownFileExtensions == null)
                    s_KnownFileExtensions = new ReadOnlyCollection<string>(new string[] { "generic", "avi", "bmp", "doc", "docx", "gif", "htm", "html", "jpg", "mov", "mp3", "mpg", "ogg", "pdf", "png", "ppt", "pptx", "txt", "xls", "xlsx", "wav", "wma", "wmv", "zip" });
                return s_KnownFileExtensions;
            }
        }

        private List<string> FileExtensionsFilterInternal
        {
            get
            {
                List<string> extensions = new List<string>(this.FileExtensionsFilter);
                if (extensions.Count > 0)
                {
                    switch (extensions[0].ToUpperInvariant())
                    {
                        case "VIDEO":
                            extensions = new List<string>(MimeType.VideoExtensions);
                            extensions.Add(".swf");
                            break;
                        case "IMAGE":
                            extensions = new List<string>(MimeType.ImageExtensions);
                            break;
                    }
                }
                return extensions;
            }
        }

        private string ViewAllAtOnceLinkId
        {
            get
            {
                string value = (string)this.ViewState["ViewAllAtOnceLinkId"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = string.Format(CultureInfo.InvariantCulture, "mafs:ViewAllAtOnceLinkId:{0:N}", Guid.NewGuid());
                    this.ViewAllAtOnceLinkId = value;
                }
                return value;
            }
            set { this.ViewState["ViewAllAtOnceLinkId"] = value; }
        }

        private string ViewAllAtOnceLinkNavigateUrl
        {
            get
            {
                string key = this.ViewAllAtOnceLinkId;

                Hashtable properties = new Hashtable();
                properties["NegateFileExtensionsFilter"] = this.NegateFileExtensionsFilter;
                properties["ContainerName"] = this.ContainerName;
                properties["ObjectId"] = this.ObjectId;
                properties["ObjectType"] = this.ObjectType;
                properties["StorageConnectionString"] = this.StorageConnectionString;
                properties["SharedAccessSignature"] = this.SharedAccessSignature;

                this.Page.Session[key] = properties;

                return ResourceVirtualPathProvider.VirtualPathToAbsolute(ResourceVirtualPathProvider.VirtualRootShortPath + "FileList.aspx")
                    + "?d=" + HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(key));
            }
        }

        private CloudBlobClient Client
        {
            get
            {
                if (m_Client == null)
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.StorageConnectionString);
                    m_Client = storageAccount.CreateCloudBlobClient();
                }
                return m_Client;
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

        private string SharedAccessSignature
        {
            get
            {
                string value = (string)this.ViewState["SharedAccessSignature"];
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = this.Container.GetSharedAccessSignature(new SharedAccessBlobPolicy
                    {
                        Permissions = SharedAccessBlobPermissions.Read,
                        SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(FileUpload.SharedAccessExpiryTime)
                    });

                    this.SharedAccessSignature = value;
                }
                return value;
            }
            set { this.ViewState["SharedAccessSignature"] = value; }
        }

        private string BlobPath
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}/{1}/", this.ObjectType, this.ObjectId);
            }
        }

        private DataTable DataSource
        {
            get
            {
                DataTable table = new DataTable();
                table.Locale = CultureInfo.CurrentCulture;
                table.Columns.Add(BlobNameColumnName, typeof(string));
                table.Columns.Add(ContentTypeColumnName, typeof(string));
                table.Columns.Add(FileNameColumnName, typeof(string));
                table.Columns.Add(UriColumnName, typeof(string));
                table.Columns.Add(LengthColumnName, typeof(long));
                table.Columns.Add(LastModifiedColumnName, typeof(DateTime));
                table.Columns.Add(LastModifiedDateColumnName, typeof(DateTime));
                table.Columns[5].DateTimeMode = DataSetDateTime.Utc;
                table.Columns[6].DateTimeMode = DataSetDateTime.Utc;

                string sas = this.SharedAccessSignature;

                IEnumerable<IListBlobItem> blobList = this.Container.ListBlobs(this.BlobPath);
                foreach (IListBlobItem item in blobList)
                {
                    CloudBlockBlob blob = item as CloudBlockBlob;
                    if (blob.BlobType == BlobType.BlockBlob)
                    {
                        string[] parts = blob.Name.Split('/');
                        string fileName = parts[parts.Length - 1];
                        table.Rows.Add(
                            blob.Name,
                            blob.Properties.ContentType,
                            fileName,
                            blob.Uri.ToString() + sas,
                            blob.Properties.Length,
                            blob.Properties.LastModified.Value.DateTime,
                            blob.Properties.LastModified.Value.Date
                        );
                    }
                }

                table.Columns.Add(LengthInKBColumnName, typeof(long), "Length / 1024");

                DataView view = table.DefaultView;

                if (this.FileExtensionsFilter.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    string format = string.Format(CultureInfo.InvariantCulture, (this.NegateFileExtensionsFilter ? " AND {0} NOT LIKE '%{{0}}'" : " OR {0} LIKE '%{{0}}'"), FileNameColumnName);
                    foreach (string ext in this.FileExtensionsFilterInternal)
                    {
                        sb.AppendFormat(format, ext);
                    }
                    sb.Remove(0, 4);
                    view.RowFilter = sb.ToString();
                }

                view.Sort = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", LastModifiedDateColumnName, FileNameColumnName);

                return view.ToTable();
            }
        }

        //private string ClientScript
        //{
        //    get
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        sb.AppendFormat(CultureInfo.InvariantCulture, "var opentip1 = new Opentip('#{0}');", this.ClientID);
        //        return sb.ToString();
        //    }
        //}

        #endregion

        #region Internal Properties

        internal static string OnClientDeleting
        {
            get { return string.Format(CultureInfo.CurrentCulture, "return window.confirm(\"{0}\");", Resources.FileList_DeletingConfirmationText); }
        }

        internal bool ShowVideoOnly
        {
            get
            {
                string[] extensions = this.FileExtensionsFilter;
                if (extensions.Length > 0)
                {
                    if (string.Compare(extensions[0], "video", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        foreach (string ext in extensions)
                        {
                            string mimeType = MimeMapping.GetMimeMapping(ext);
                            if (!(MimeType.IsVideoType(mimeType) || MimeType.IsFlash(mimeType)))
                                return false;
                        }
                    }
                    return (!this.NegateFileExtensionsFilter);
                }
                return false;
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
                return ((obj == null) ? new string[0] : (string[])obj);
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
                return ((obj == null) ? CultureInfo.CurrentCulture : (CultureInfo)obj);
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
                return ((obj == null) ? "{0:MMM d, yyyy H:mm}" : (string)obj);
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
                return ((obj == null) ? "{0:d-MMM-yyyy}" : (string)obj);
            }
            set { this.ViewState["DateTimeFormatString"] = value; }
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
                return (string.IsNullOrEmpty(str) ? "Eastern Standard Time" : str);
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
                return ((obj == null) ? true : (bool)obj);
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
                return ((obj == null) ? true : (bool)obj);
            }
            set { ViewState["EnableDeletingConfirmation"] = value; }
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
                return ((obj == null) ? 0 : (int)obj);
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
                return ((obj == null) ? IconSize.Smaller : (IconSize)obj);
            }
            set { this.ViewState["IconSize"] = value; }
        }

        /// <summary>
        /// Gets a value indicating that the control is empty.
        /// </summary>
        [Browsable(false)]
        public bool IsEmpty
        {
            get { return (this.FilesCount == 0); }
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
                return ((obj == null) ? false : (bool)obj);
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
                return ((obj == null) ? FileListRenderingMode.Grid : (FileListRenderingMode)obj);
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
                return ((obj == null) ? 4 : (int)obj);
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
                return ((obj == null) ? RepeatDirection.Horizontal : (RepeatDirection)obj);
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
                return ((obj == null) ? true : (bool)obj);
            }
            set { this.ViewState["ShowFileToolTip"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the icons column are displayed in control.
        /// </summary>
        [Category("Appearance")]
        [Description("Whether the icons column are displayed in control.")]
        [DefaultValue(false)]
        public bool ShowIcons
        {
            get
            {
                object obj = this.ViewState["ShowIcons"];
                return ((obj == null) ? false : (bool)obj);
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
                return ((obj == null) ? true : (bool)obj);
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
                return ((obj == null) ? Unit.Empty : (Unit)obj);
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
                return ((obj == null) ? WebConfigurationManager.AppSettings["mafs:StorageConnectionString"] : (string)obj);
            }
            set { this.ViewState["StorageConnectionString"] = value; }
        }

        #endregion

        #region Private Methods

        private static string GetFileTypeIconUrl(string fileName, IconSize iconSize)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant().TrimStart('.');
            string webResourceNameFormatString = string.Format(CultureInfo.InvariantCulture, "Images.Icons{0}x{0}.{{0}}.gif", (int)iconSize);
            return ResourceHandler.GetWebResourceUrl(string.Format(CultureInfo.InvariantCulture, webResourceNameFormatString, (KnownFileExtensions.Contains(extension) ? extension : KnownFileExtensions[0])), true);
        }

        private static string GetNonImageFileTypeIconUrl(string fileName, IconSize iconSize)
        {
            return (MimeType.IsImageType(MimeMapping.GetMimeMapping(fileName)) ? null : GetFileTypeIconUrl(fileName, iconSize));
        }

        private void ApplyStyle()
        {
            if (Grid != null)
            {
                if (!this.Width.IsEmpty)
                {
                    Grid.Width = this.Width;
                    Grid.Columns[(this.ShowIcons ? 2 : 1)].ItemStyle.Width = Unit.Percentage(100);
                }

                Grid.CellPadding = -1;
                Grid.CellSpacing = 0;
                if (this.ShowIcons)
                {
                    if (this.IconSize != IconSize.Smaller)
                        Grid.CellPadding = 0;
                    else
                        Grid.CellSpacing = -1;
                    Grid.Columns[0].ControlStyle.Width = Grid.Columns[0].ControlStyle.Height = Unit.Pixel((int)this.IconSize);
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
            Grid.DataKeyNames = new string[] { BlobNameColumnName };
            Grid.AutoGenerateColumns = false;
            Grid.GridLines = GridLines.None;
            Grid.ShowHeader = false;
            Grid.RowDataBound += new GridViewRowEventHandler(Grid_RowDataBound);
            Grid.RowDeleting += new GridViewDeleteEventHandler(Grid_RowDeleting);

            BoundField updatedTimeField = new BoundField();
            updatedTimeField.DataField = LastModifiedColumnName;
            updatedTimeField.HeaderStyle.Wrap = false;
            updatedTimeField.HeaderText = Resources.FileList_UpdatedWhenText;
            updatedTimeField.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            updatedTimeField.ItemStyle.Wrap = false;

            if (this.ShowIcons)
            {
                ImageField imageField = new ImageField();
                imageField.DataImageUrlField = BlobNameColumnName;
                Grid.Columns.Add(imageField);
            }

            HyperLinkField linkField = new HyperLinkField();
            linkField.DataNavigateUrlFields = new string[] { UriColumnName };
            linkField.DataTextField = FileNameColumnName;
            linkField.HeaderText = Resources.FileList_FileNameText;
            linkField.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
            linkField.ControlStyle.CssClass = "flFileName";
            linkField.Target = "_blank";
            Grid.Columns.Add(linkField);

            BoundField boundField = new BoundField();
            boundField.DataField = LengthInKBColumnName;
            boundField.DataFormatString = "{0:N0} KB";
            boundField.HeaderStyle.Wrap = false;
            boundField.HeaderText = Resources.FileList_SizeText;
            boundField.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
            boundField.ItemStyle.Wrap = false;
            Grid.Columns.Add(boundField);

            if (this.EnableDeleting)
            {
                ButtonField buttonField = new ButtonField();
                buttonField.CausesValidation = false;
                buttonField.CommandName = DataList.DeleteCommandName;
                buttonField.Text = Resources.FileList_DeleteText;
                buttonField.ControlStyle.CssClass = "flRemove";
                buttonField.ItemStyle.Wrap = false;
                Grid.Columns.Add(buttonField);
            }

            Grid.Columns.Add(updatedTimeField);
        }

        private void CreateDataList()
        {
            List = new DataList();
            List.ID = "Grid";
            List.CellSpacing = 0;
            List.CellPadding = 0;
            List.DataKeyField = BlobNameColumnName;
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

        private void DeleteFile(string blobName)
        {
            CloudBlockBlob blob = this.Container.GetBlockBlobReference(blobName);
            if (blob != null)
            {
                blob.Delete();
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
                ViewAllAtOnceLink.CssClass = "flCptCtrl";
                ViewAllAtOnceLink.Text = Resources.FileList_ViewAllAtOnceLink_Text;
                ViewAllAtOnceLink.Target = "_blank";
                ViewAllAtOnceLink.NavigateUrl = "#";

                if (this.RenderingMode == FileListRenderingMode.Grid)
                    ViewAllAtOnceLink.CssClass += " flLink";
            }

            if (ViewAllAtOnceLink != null)
            {
                if (this.RenderingMode == FileListRenderingMode.Grid)
                    CaptionPanel.Controls.Add(ViewAllAtOnceLink);
            }

            if (CaptionPanel.HasControls())
            {
                this.Controls.Add(CaptionPanel);
            }
        }

        private void GridDataBind()
        {
            this.FilesCount = 0;

            DataTable dataSource = this.DataSource;

            if (Grid != null)
            {
                Grid.DataSource = dataSource;
                Grid.DataBind();
            }
            else if (List != null)
            {
                List.DataSource = dataSource;
                List.DataBind();
            }

            this.FilesCount = dataSource.Rows.Count;
        }

        private void DataList_DeleteCommand(object source, DataListCommandEventArgs e)
        {
            DeleteFile((string)(List.DataKeys[e.Item.ItemIndex]));
        }

        private void Grid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e == null) return;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int count = e.Row.Cells.Count;
                if (count > 0)
                {
                    DataRowView drv = (DataRowView)e.Row.DataItem;

                    if (this.ShowIcons)
                    {
                        Image img = e.Row.Cells[0].Controls[0] as Image;
                        if (img != null)
                        {
                            string fileName = (string)drv[FileNameColumnName];
                            img.ImageUrl = GetFileTypeIconUrl(fileName, this.IconSize);
                        }
                    }

                    e.Row.Attributes["onmouseover"] = "this.className += ' flHover';";
                    e.Row.Attributes["onmouseout"] = "this.className = this.className.replace(' flHover', '');";

                    DateTime updatedTime = (DateTime)drv[LastModifiedColumnName];
                    updatedTime = TimeZoneInfo.ConvertTimeFromUtc(updatedTime, this.TimeZone);

                    TableCell cell = e.Row.Cells[((this.ShowIcons ? 4 : 3) + (!this.EnableDeleting ? -1 : 0))];
                    cell.Text = string.Format(this.Culture, this.DateTimeFormatString, updatedTime);

                    TableCell deleteCell = e.Row.Cells[count - 2];

                    DateTime updatedDate = (DateTime)drv[LastModifiedDateColumnName];
                    if (m_UpdatedDate == updatedDate)
                        cell.Text = string.Empty;
                    else
                    {
                        if (m_UpdatedDate != DateTime.MinValue) e.Row.CssClass += " flPt";
                        cell.CssClass = "flDate";
                    }
                    m_UpdatedDate = updatedDate;

                    cell.ToolTip = string.Format(this.Culture, this.DateTimeToolTipFormatString, updatedTime);

                    if (this.EnableDeleting && this.EnableDeletingConfirmation)
                    {
                        if (deleteCell.Controls.Count > 0)
                        {
                            WebControl control = e.Row.Cells[count - 2].Controls[0] as WebControl;
                            if (control != null) control.Attributes.Add("onclick", OnClientDeleting);
                        }
                    }
                }
            }
        }

        protected void Grid_RowDeleting(object sender, GridViewDeleteEventArgs e)
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

            if (this.RenderingMode != FileListRenderingMode.Grid)
                this.EnsureGridCaptionPanel();

            this.EnsureGrid();
            if (Grid != null)
                this.Controls.Add(Grid);
            else if (List != null)
                this.Controls.Add(List);

            if (this.RenderingMode == FileListRenderingMode.Grid)
                this.EnsureGridCaptionPanel();
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

            Helper.RegisterControlStyleSheet(this.Page, "Styles.opentip.css");
            Helper.RegisterControlStyleSheet(this.Page, "Styles.FileList.css");

            ScriptManager.RegisterClientScriptInclude(this.Page, this.Page.GetType(), "Scripts.opentip.js", ResourceHandler.GetWebResourceUrl("Scripts.opentip.js", true));
            ScriptManager.RegisterClientScriptInclude(this.Page, this.Page.GetType(), "Scripts.FileList.js", ResourceHandler.GetWebResourceUrl("Scripts.FileList.js", true));

            //ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), this.ClientID, this.ClientScript, true);
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
                    ViewAllAtOnceLink.Visible = (!this.IsEmpty);
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
    }
}
