using System;

namespace Micajah.AzureFileService.Web
{
    public partial class SimpleUpload : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            SimpleUpload1.FileUpload.AcceptChanges();
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            SimpleUpload1.FileUpload.RejectChanges();
        }
    }
}