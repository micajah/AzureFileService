using Micajah.AzureFileService.Properties;
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Micajah.AzureFileService.WebControls
{
    public partial class FileList
    {
        private class ThumbnailsListItemTemplate : ITemplate, IDisposable
        {
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
                File file = (File)DataBinder.GetDataItem(img.NamingContainer);

                string url = (m_FileList.ShowVideoOnly ? ResourceHandler.GetWebResourceUrl("Images.Video.gif", true) : GetNonImageFileTypeIconUrl(file.Name, IconSize.Bigger));
                if (url == null)
                {
                    url = m_FileList.FileManager.GetThumbnailUrl(file.FileId, (int)IconSize.Bigger, (int)IconSize.Bigger, 1, false);
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
                }
            }

            private void Panel_DataBinding(object sender, EventArgs e)
            {
                Panel panel = (Panel)sender;
                File file = (File)DataBinder.GetDataItem(panel.NamingContainer);

                string date = string.Format(m_FileList.Culture, m_FileList.DateTimeToolTipFormatString, TimeZoneInfo.ConvertTimeFromUtc(file.LastModified, m_FileList.TimeZone));

                string content = string.Format(m_FileList.Culture, ToolTipSmallHtml,
                    file.Url, file.Name, date, file.LengthInKB, m_FileList.Page.ClientScript.GetPostBackClientHyperlink(DeleteLink, string.Empty), Resources.FileList_DeleteText,
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
