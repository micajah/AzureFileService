using Micajah.AzureFileService.Properties;
using System;
using System.Globalization;
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

            private Panel PicturePanel;
            private Image Picture;
            private Label IconLabel;
            private Panel LinksPanel;
            private HyperLink PictureLink;
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

            private void PicturePanel_DataBinding(object sender, EventArgs e)
            {
                Panel panel = (Panel)sender;

                File file = (File)DataBinder.GetDataItem(panel.NamingContainer);

                var fileMimeTypeGroups = MimeType.GetGroups(file.Name, true);
                bool isImage = (fileMimeTypeGroups & MimeTypeGroups.Image) == MimeTypeGroups.Image;

                if (isImage)
                {
                    Picture = new Image();

                    Picture.ImageUrl = m_FileList.FileManager.GetThumbnailUrl(file.FileId, 0, 0, 0, true);

                    panel.Controls.Add(Picture);
                }
                else
                {
                    IconLabel = new Label();

                    IconLabel.CssClass = GetFileTypeIconCssClass(fileMimeTypeGroups);

                    string fontSize = string.Format(CultureInfo.InvariantCulture, "{0}px", (int)IconSize.Bigger);
                    IconLabel.Font.Size = FontUnit.Parse(fontSize, CultureInfo.InvariantCulture);
                    IconLabel.Style["line-height"] = fontSize;

                    panel.Controls.Add(IconLabel);
                }
            }

            private void PictureLink_DataBinding(object sender, EventArgs e)
            {
                HyperLink link = (HyperLink)sender;

                File file = (File)DataBinder.GetDataItem(link.NamingContainer);

                var fileMimeTypeGroups = MimeType.GetGroups(file.Name, true);
                bool isImage = (fileMimeTypeGroups & MimeTypeGroups.Image) == MimeTypeGroups.Image;

                link.Text = file.Name;
                link.NavigateUrl = file.Url;

                if (file.Extension.In(MimeType.SwfExtension) || isImage)
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

                PicturePanel = new Panel();
                PicturePanel.DataBinding += PicturePanel_DataBinding;
                container.Controls.Add(PicturePanel);

                LinksPanel = new Panel();
                LinksPanel.CssClass = "flLinks";

                PictureLink = new HyperLink();
                PictureLink.DataBinding += new EventHandler(PictureLink_DataBinding);
                PictureLink.CssClass = "flFileName";
                LinksPanel.Controls.Add(PictureLink);

                if (m_FileList.EnableDeleting)
                {
                    LinksPanel.Controls.Add(new LiteralControl(SeparatorHtml));

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
                    LinksPanel.Controls.Add(DeleteLink);
                }

                container.Controls.Add(LinksPanel);
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);

                if (PicturePanel != null)
                {
                    PicturePanel.Dispose();
                }

                if (Picture != null)
                {
                    Picture.Dispose();
                }

                if (IconLabel != null)
                {
                    IconLabel.Dispose();
                }

                if (LinksPanel != null)
                {
                    LinksPanel.Dispose();
                }

                if (PictureLink != null)
                {
                    PictureLink.Dispose();
                }

                if (DeleteLink != null)
                {
                    DeleteLink.Dispose();
                }
            }

            #endregion
        }
    }
}
