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
            <mafs:FileUpload ID="FileUpload1" runat="server" ContainerName="micajahazurefileservice" TemporaryContainerName="micajahazurefileservicetemp" ObjectType="ticket" ObjectId="12345" Accept="image/*" />
            <br />
            <asp:Button ID="AcceptButton" runat="server" Text="Accept Changes" OnClick="AcceptButton_Click" />
            &nbsp;&nbsp;&nbsp;
            <asp:Button ID="RejectButton" runat="server" Text="Reject Changes" OnClick="RejectButton_Click" />
        </div>
        Text below the file upload control.
    </form>
</body>
</html>
