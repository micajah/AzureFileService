<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Micajah.AzureFileService.Web.Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>

    <form id="form1" runat="server">
        Top navigation.
        <div>
            Text above the file upload control.
            <mafs:FileUpload ID="FileUpload1" runat="server"
                ContainerName="15acca54-8bc3-4519-b609-5c11a16a63ec"
                ObjectType="ticket" ObjectId="12345" Accept="image/*" />
            <br />
            <asp:Button ID="Button1" runat="server" Text="Submit" />
        </div>
        Text below the file upload control.
    </form>
</body>
</html>
