<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Upload.aspx.cs" Inherits="Micajah.AzureFileService.Web.Upload" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <iframe id="fileUploader" src="<%= UploadUrl %>" style="width: 100%; height: 600px; border: none 0; overflow: hidden;" scrolling="no"></iframe>
        <br />
        <asp:PlaceHolder ID="FilesList" runat="server"></asp:PlaceHolder>
    </form>
</body>
</html>
