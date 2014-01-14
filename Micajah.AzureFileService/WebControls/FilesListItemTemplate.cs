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
        private class FilesListItemTemplate : ITemplate, IDisposable
        {
            #region Constants

            private const string SeparatorHtml = "&nbsp;&nbsp;&nbsp;&nbsp;";

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

                link.Text = fileName;
                link.NavigateUrl = (string)drv[UriColumnName];
                if ((string.Compare(extension, MimeType.SwfExtension, StringComparison.OrdinalIgnoreCase) == 0) || MimeType.IsImageType(MimeMapping.GetMimeMapping(extension)))
                {
                    link.Target = "_blank";
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
                    DeleteLink.Text = Resources.FileList_DeleteText;
                    if (m_FileList.EnableDeletingConfirmation) DeleteLink.OnClientClick = OnDeletingClientScript;
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
