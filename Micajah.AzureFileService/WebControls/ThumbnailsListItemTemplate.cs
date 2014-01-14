using Micajah.AzureFileService.Properties;
using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Micajah.AzureFileService.WebControls
{
    public partial class FileList
    {
        private class ThumbnailsListItemTemplate : ITemplate, IDisposable
        {
            #region Constants

            #endregion

            #region Members

            private ListItemType m_ItemType;
            private FileList m_FileList;

            private LinkButton DeleteLink;
            private Image Picture;
            private Panel PicturePanel;
            private HyperLink PictureLink;

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

                string fileName = (string)drv[FileNameColumnName];

                string uri = (m_FileList.ShowVideoOnly ? ResourceHandler.GetWebResourceUrl("Images.Video.gif", true) : GetNonImageFileTypeIconUrl(fileName, IconSize.Bigger));
                if (uri == null)
                {
                    uri = FileHandler.GetThumbnailUrl(fileName, (int)IconSize.Bigger, (int)IconSize.Bigger, 1, m_FileList.PropertyTableId, false);
                }
                img.ImageUrl = uri;
            }

            private void HyperLink_DataBinding(object sender, EventArgs e)
            {
                HyperLink link = (HyperLink)sender;
                DataRowView drv = (DataRowView)DataBinder.GetDataItem(link.NamingContainer);

                string fileName = (string)drv[FileNameColumnName];
                string extension = Path.GetExtension(fileName);

                link.NavigateUrl = (string)drv[UriColumnName];
                if ((string.Compare(extension, MimeType.SwfExtension, StringComparison.OrdinalIgnoreCase) == 0) || MimeType.IsImageType(MimeMapping.GetMimeMapping(extension)))
                {
                    link.Target = "_blank";
                }
            }

            private void Panel_DataBinding(object sender, EventArgs e)
            {
                Panel panel = (Panel)sender;
                DataRowView drv = (DataRowView)DataBinder.GetDataItem(panel.NamingContainer);

                string fileName = (string)drv[FileNameColumnName];
                string uri = (string)drv[UriColumnName];
                long lengthInKB = (long)drv[LengthInKBColumnName];
                DateTime lastModified = (DateTime)drv[LastModifiedColumnName];

                string date = string.Format(m_FileList.Culture, m_FileList.DateTimeToolTipFormatString, TimeZoneInfo.ConvertTimeFromUtc(lastModified, m_FileList.TimeZone));

                string content = string.Format(m_FileList.Culture, ToolTipSmallHtml,
                    uri, fileName, date, lengthInKB, m_FileList.Page.ClientScript.GetPostBackClientHyperlink(DeleteLink, string.Empty), Resources.FileList_DeleteText,
                    m_FileList.EnableDeletingConfirmation ? string.Format(m_FileList.Culture, " onclick='{0}'", OnDeletingClientScript) : string.Empty);

                panel.Attributes["data-ot"] = content;
            }

            #endregion

            #region Public Methods

            public void InstantiateIn(Control container)
            {
                if (container == null)
                {
                    return;
                }
                if ((m_ItemType != ListItemType.Item) && (m_ItemType != ListItemType.AlternatingItem))
                {
                    return;
                }

                Picture = new Image();
                Picture.DataBinding += new EventHandler(Image_DataBinding);
                Picture.Width = Picture.Height = Unit.Pixel(m_FileList.ShowVideoOnly ? 148 : 128);

                PictureLink = new HyperLink();
                PictureLink.DataBinding += new EventHandler(HyperLink_DataBinding);
                PictureLink.Controls.Add(Picture);

                PicturePanel = new Panel();
                PicturePanel.ID = "ThumbPanel";
                PicturePanel.Width = PicturePanel.Height = Unit.Pixel(m_FileList.ShowVideoOnly ? 148 : 128);
                PicturePanel.Style[HtmlTextWriterStyle.BackgroundColor] = "White";
                PicturePanel.DataBinding += new EventHandler(Panel_DataBinding);
                PicturePanel.Controls.Add(PictureLink);

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
                        DeleteLink.OnClientClick = OnDeletingClientScript;
                    }
                    PicturePanel.Controls.Add(DeleteLink);
                }

                container.Controls.Add(PicturePanel);
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);

                if (DeleteLink != null)
                {
                    DeleteLink.Dispose();
                }

                if (Picture != null)
                {
                    Picture.Dispose();
                }

                if (PictureLink != null)
                {
                    PictureLink.Dispose();
                }

                if (PicturePanel != null)
                {
                    PicturePanel.Dispose();
                }
            }

            #endregion
        }
    }
}
