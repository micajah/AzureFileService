<%@ Page Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.Page" EnableViewState="false" EnableViewStateMac="false" EnableEventValidation="false" %>

<%@ Register Assembly="Micajah.AzureFileService" Namespace="Micajah.AzureFileService.WebControls" TagPrefix="mafs" %>

<script runat="server" type="text/C#">
    private void Page_Load(object sender, EventArgs e)
    {
        if (!this.Page.IsPostBack)
        {
            byte[] bytes = HttpServerUtility.UrlTokenDecode(Request.QueryString["d"]);
            string key = Encoding.UTF8.GetString(bytes);
            Hashtable table = Session[key] as Hashtable;
            if (table != null)
            {
                Micajah.AzureFileService.Helper.LoadProperties(FileList1, table);
            }
        }
    }
</script>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
</head>
<body style="margin: 4px;">
    <form id="form1" runat="server">
        <mafs:FileList ID="FileList1" runat="server" ShowViewAllAtOnceLink="false" FileExtensionsFilter="image" RenderingMode="List" EnableDeleting="false" />
    </form>
</body>
</html>
