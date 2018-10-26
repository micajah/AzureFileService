using Micajah.AzureFileService.Properties;
using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Micajah.AzureFileService.WebControls
{
    public partial class FileList
    {
        private class ThumbnailsListItemTemplate : ITemplate, IDisposable
        {
            #region

            private const int ImageThumbnailWidth = 128;
            private const int ImageThumbnailHeight = 128;
            private const int VideoThumbnailWidth = 148;
            private const int VideoThumbnailHeight = 148;
            private const int ThumbnailPadding = 4; // 2 borders x 1px + 2 paddings x 1px 

            #endregion

            #region Members

            private ListItemType m_ItemType;
            private FileList m_FileList;
            private int m_PictureWidth;
            private int m_PictureHeight;

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

                m_PictureWidth = ImageThumbnailWidth;
                m_PictureHeight = ImageThumbnailHeight;

                if (m_FileList.ShowVideoOnly)
                {
                    m_PictureWidth = VideoThumbnailWidth;
                    m_PictureHeight = VideoThumbnailWidth;
                }
            }

            #endregion

            #region Private Methods

            private void Image_DataBinding(object sender, EventArgs e)
            {
                Image img = (sender as Image);
                File file = (File)DataBinder.GetDataItem(img.NamingContainer);

                string url = (m_FileList.ShowVideoOnly ? ResourceHandler.GetWebResourceUrl("Images.Video.gif", true) : GetNonImageFileTypeIconUrl(file.Name, IconSize.Bigger));
                if (url == null)
                {
                    url = m_FileList.FileManager.GetThumbnailUrl(file.FileId, m_PictureWidth, m_PictureHeight, 1, false);
                }

                img.ImageUrl = url;
            }

            private void HyperLink_DataBinding(object sender, EventArgs e)
            {
                HyperLink link = (HyperLink)sender;
                File file = (File)DataBinder.GetDataItem(link.NamingContainer);

                string extension = file.Extension;

                link.NavigateUrl = file.Url;
                if ((string.Compare(extension, MimeType.SwfExtension, StringComparison.OrdinalIgnoreCase) == 0) || MimeType.IsImageType(MimeMapping.GetMimeMapping(extension)))
                {
                    link.Target = "_blank";
                    link.Attributes["rel"] = "noopener";
                }
            }

            private void Panel_DataBinding(object sender, EventArgs e)
            {
                Panel panel = (Panel)sender;
                File file = (File)DataBinder.GetDataItem(panel.NamingContainer);

                string date = string.Format(m_FileList.Culture, m_FileList.DateTimeToolTipFormatString, TimeZoneInfo.ConvertTimeFromUtc(file.LastModified, m_FileList.TimeZone));

                string delete = string.Empty;
                if (m_FileList.EnableDeleting)
                {
                    string deletingConfirmation = string.Empty;
                    if (m_FileList.EnableDeletingConfirmation)
                    {
                        deletingConfirmation = string.Format(CultureInfo.InvariantCulture, " onclick='{0}'", OnDeletingClientScript);
                    }

                    string postBackClientHyperlink = m_FileList.Page.ClientScript.GetPostBackClientHyperlink(DeleteLink, string.Empty);

                    delete = string.Format(CultureInfo.InvariantCulture, DeleteLinkHtml, postBackClientHyperlink, Resources.FileList_DeleteText, deletingConfirmation, m_FileList.DeleteButtonText);
                }

                string content = string.Format(m_FileList.Culture, ToolTipSmallHtml, file.Url, file.Name, date, file.LengthInKB, delete);

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
                Picture.Width = Unit.Pixel(m_PictureWidth);
                Picture.Height = Unit.Pixel(m_PictureHeight);

                PictureLink = new HyperLink();
                PictureLink.DataBinding += new EventHandler(HyperLink_DataBinding);
                PictureLink.Controls.Add(Picture);

                int width = m_PictureWidth + ThumbnailPadding;
                int height = m_PictureHeight + ThumbnailPadding;

                if (m_FileList.EnableDeleting && (!m_FileList.ShowFileToolTip))
                {
                    height += 19;
                }

                PicturePanel = new Panel();
                PicturePanel.ID = "ThumbPanel";
                PicturePanel.Width = Unit.Pixel(width);
                PicturePanel.Height = Unit.Pixel(height);
                PicturePanel.Style[HtmlTextWriterStyle.BackgroundColor] = "White";

                if (m_FileList.ShowFileToolTip)
                {
                    PicturePanel.DataBinding += new EventHandler(Panel_DataBinding);
                }

                PicturePanel.Controls.Add(PictureLink);

                if (m_FileList.EnableDeleting)
                {
                    DeleteLink = new LinkButton();
                    DeleteLink.ID = "DeleteLink";
                    DeleteLink.CommandName = DataList.DeleteCommandName;
                    DeleteLink.CausesValidation = false;
                    DeleteLink.CssClass = "flRemove";
                    DeleteLink.Text = m_FileList.DeleteButtonText;

                    if (m_FileList.ShowFileToolTip)
                    {
                        DeleteLink.Style[HtmlTextWriterStyle.Display] = "none";
                    }

                    if (m_FileList.EnableDeletingConfirmation)
                    {
                        DeleteLink.OnClientClick = OnDeletingClientScript;
                        DeleteLink.ToolTip = Resources.FileList_DeleteText;
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
