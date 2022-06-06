using Micajah.AzureFileService.Properties;
using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Micajah.AzureFileService.WebControls
{
    public partial class FileList
    {
        private class ThumbnailsListItemTemplate : ITemplate, IDisposable
        {
            #region

            private const int ThumbnailWidth = 128;
            private const int ThumbnailHeight = 128;
            private const int ThumbnailPadding = 4; // 2 borders x 1px + 2 paddings x 1px 

            #endregion

            #region Members

            private readonly ListItemType m_ItemType;
            private readonly FileList m_FileList;

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

            private void PictureLink_DataBinding(object sender, EventArgs e)
            {
                HyperLink link = (HyperLink)sender;

                File file = (File)DataBinder.GetDataItem(link.NamingContainer);

                var fileMimeTypeGroups = MimeType.GetGroups(file.Name, true);
                bool isImage = (fileMimeTypeGroups & MimeTypeGroups.Image) == MimeTypeGroups.Image;

                link.NavigateUrl = file.Url;

                if (file.Extension.In(MimeType.SwfExtension) || isImage)
                {
                    link.Target = "_blank";
                    link.Attributes["rel"] = "noopener";
                }

                if (isImage)
                {
                    Picture = new Image();
                    Picture.Width = Unit.Pixel(ThumbnailWidth);
                    Picture.Height = Unit.Pixel(ThumbnailHeight);

                    Picture.ImageUrl = m_FileList.FileManager.GetThumbnailUrl(file.FileId, ThumbnailWidth, ThumbnailHeight, 1, false);

                    link.Controls.Add(Picture);
                }
                else
                {
                    link.CssClass = GetFileTypeIconCssClass(fileMimeTypeGroups);

                    string fontSize = string.Format(CultureInfo.InvariantCulture, "{0}px", ThumbnailHeight);
                    link.Font.Size = FontUnit.Parse(fontSize, CultureInfo.InvariantCulture);
                    link.Style["line-height"] = fontSize;
                }
            }

            private void PicturePanel_DataBinding(object sender, EventArgs e)
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

                PictureLink = new HyperLink();
                PictureLink.DataBinding += new EventHandler(PictureLink_DataBinding);

                int width = ThumbnailWidth + ThumbnailPadding;
                int height = ThumbnailHeight + ThumbnailPadding;

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
                    PicturePanel.DataBinding += new EventHandler(PicturePanel_DataBinding);
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
