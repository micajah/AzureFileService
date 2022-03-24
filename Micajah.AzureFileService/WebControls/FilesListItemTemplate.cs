using Micajah.AzureFileService.Properties;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Micajah.AzureFileService.WebControls
{
    public partial class FileList
    {
        private class FilesListItemTemplate : ITemplate, IDisposable
        {
            #region Constants

            private const string SeparatorHtml = "&nbsp;&nbsp;&nbsp;&nbsp;";

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

            public FilesListItemTemplate(ListItemType itemType, FileList fileList)
            {
                m_ItemType = itemType;
                m_FileList = fileList;
            }

            #endregion

            #region Private Methods

            private void Image_DataBinding(object sender, EventArgs e)
            {
                Image img = sender as Image;
                File file = (File)DataBinder.GetDataItem(img.NamingContainer);

                string url = GetNonImageFileTypeIconUrl(file.Name, IconSize.Bigger);

                if (url == null)
                {
                    string contentType = MimeType.GetMimeType(file.Extension);

                    url = MimeType.IsHeif(contentType) ? m_FileList.FileManager.GetThumbnailUrl(file.FileId, 0, 0, 0, true) : file.Url;
                }

                img.ImageUrl = url;
            }

            private void HyperLink_DataBinding(object sender, EventArgs e)
            {
                HyperLink link = (HyperLink)sender;
                File file = (File)DataBinder.GetDataItem(link.NamingContainer);

                string extension = file.Extension;

                link.Text = file.Name;
                link.NavigateUrl = file.Url;
                if ((string.Compare(extension, MimeType.SwfExtension, StringComparison.OrdinalIgnoreCase) == 0) || MimeType.IsInGroups(extension, MimeTypeGroups.Image, true))
                {
                    link.Target = "_blank";
                    link.Attributes["rel"] = "noopener";
                }
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

                PicturePanel = new Panel();
                PictureLink = new HyperLink();
                PictureLink.DataBinding += new EventHandler(HyperLink_DataBinding);
                PictureLink.CssClass = "flFileName";
                PicturePanel.Controls.Add(PictureLink);

                if (m_FileList.EnableDeleting)
                {
                    PicturePanel.Controls.Add(new LiteralControl(SeparatorHtml));
                    DeleteLink = new LinkButton();
                    DeleteLink.ID = "DeleteLink";
                    DeleteLink.CommandName = DataList.DeleteCommandName;
                    DeleteLink.CausesValidation = false;
                    DeleteLink.CssClass = "flRemove";
                    DeleteLink.Text = m_FileList.DeleteButtonText;
                    if (m_FileList.EnableDeletingConfirmation)
                    {
                        DeleteLink.OnClientClick = OnDeletingClientScript;
                        DeleteLink.ToolTip = Resources.FileList_DeleteText;
                    }
                    PicturePanel.Controls.Add(DeleteLink);
                }

                container.Controls.Add(Picture);
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
