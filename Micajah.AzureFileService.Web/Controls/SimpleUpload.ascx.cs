using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Micajah.AzureFileService.Web.Controls
{
    public partial class SimpleUpload : System.Web.UI.UserControl
    {
        public Micajah.AzureFileService.WebControls.FileUpload FileUpload
        {
            get
            {
                return FileUpload1;
            }
        }

        public Micajah.AzureFileService.WebControls.FileList FileList
        {
            get
            {
                return FileList1;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.Page.PreRenderComplete += Page_PreRenderComplete;
        }

        private void Page_PreRenderComplete(object sender, EventArgs e)
        {
            if (FileList1.IsEmpty)
            {
                FilePanel.Style[HtmlTextWriterStyle.Display] = "none";
            }
            else
            {
                FilePanel.Style.Remove(HtmlTextWriterStyle.Display);
            }
        }
    }
}