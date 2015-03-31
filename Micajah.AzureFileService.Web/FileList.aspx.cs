using Micajah.AzureFileService.WebControls;
using System;
using System.Globalization;

namespace Micajah.AzureFileService.Web
{
    public partial class FileList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                FileList3.FileExtensionsFilter = (string.IsNullOrWhiteSpace(FilterTextBox.Text) ? null : FilterTextBox.Text.Split(','));
                FileList3.NegateFileExtensionsFilter = NegateCheckBox.Checked;

                foreach (string name in Enum.GetNames(typeof(IconSize)))
                {
                    IconSizeList.Items.Add(name);
                }
                IconSizeList.SelectedValue = IconSize.Smaller.ToString();

                VideoLink.Attributes["onclick"] = string.Format(CultureInfo.InvariantCulture, "document.getElementById('{0}').value='video';", FilterTextBox.ClientID);
                ImageLink.Attributes["onclick"] = string.Format(CultureInfo.InvariantCulture, "document.getElementById('{0}').value='image';", FilterTextBox.ClientID);
                ResetLink.Attributes["onclick"] = string.Format(CultureInfo.InvariantCulture, "document.getElementById('{0}').value='';", FilterTextBox.ClientID);
            }
        }

        protected override void OnPreRenderComplete(EventArgs e)
        {
            base.OnPreRenderComplete(e);

            long size = ContainerManager.GetContainerLength(FileList2.ContainerName);
            long sizeInKB = size / 1024;
            long sizeInMB = sizeInKB / 1024;

            if (sizeInMB > 1)
            {
                ContainerLengthLabel.Text = string.Format(CultureInfo.CurrentCulture, "{0:N0} MB", sizeInMB);
            }
            else
            {
                ContainerLengthLabel.Text = string.Format(CultureInfo.CurrentCulture, "{0:N0} KB", sizeInKB);
            }
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            FileList3.FileExtensionsFilter = (string.IsNullOrWhiteSpace(FilterTextBox.Text) ? null : FilterTextBox.Text.Split(','));
            FileList3.NegateFileExtensionsFilter = NegateCheckBox.Checked;
            FileList3.DataBind();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            FileList2.IconSize = (IconSize)Enum.Parse(typeof(IconSize), IconSizeList.SelectedValue);
            IconSizeList.SelectedValue = FileList2.IconSize.ToString();
            FileList2.EnableThumbnails = ThumbnailsCheckBox.Checked;
            FileList2.ShowIcons = IconsCheckBox.Checked;
            FileList2.DataBind();
        }
    }
}