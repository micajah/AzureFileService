<%@ Page Title="File Upload" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Micajah.AzureFileService.Web.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <mafs:FileUpload ID="FileUpload1" runat="server" EnableDropToPage="true"
        ContainerName="micajahazurefileservice" TemporaryContainerName="micajahazurefileservicetemp" ObjectType="ticket" ObjectId="12345" />
    <br />
    <asp:Button ID="AcceptButton" runat="server" Text="Accept Changes" OnClick="AcceptButton_Click" />&nbsp;
    <asp:Button ID="RejectButton" runat="server" Text="Reject Changes" OnClick="RejectButton_Click" />&nbsp;&nbsp;&nbsp;
    <asp:Button ID="SubmitButton" runat="server" Text="Just Do Postback" />
    <div id="DropDisabledPanel" class="panel dz-drop-disabled">
        If you drop the files there - it will be rejected
    </div>
</asp:Content>
