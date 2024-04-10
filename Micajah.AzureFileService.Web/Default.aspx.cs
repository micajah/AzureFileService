﻿using System;

namespace Micajah.AzureFileService.Web
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }

            //FileUpload1.Metadata = new System.Collections.Generic.Dictionary<string, string>()
            //{
            //    { "TechOnly", bool.TrueString }
            //};
        }

        protected void AcceptButton_Click(object sender, EventArgs e)
        {
            FileUpload1.AcceptChanges();
        }

        protected void RejectButton_Click(object sender, EventArgs e)
        {
            FileUpload1.RejectChanges();
        }
    }
}