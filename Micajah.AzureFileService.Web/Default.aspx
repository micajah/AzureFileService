<%@ Page Title="File Upload" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Micajah.AzureFileService.Web.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <p>Upload a file using drag and drop for whole page</p>
    <mafs:FileUpload ID="FileUpload1" runat="server" DropElement="document.body"
        ContainerName="micajahazurefileservice" ObjectType="ticket" ObjectId="12345" />
    <br />
    <asp:Button ID="AcceptButton" runat="server" Text="Accept Changes" OnClick="AcceptButton_Click" UseSubmitBehavior="false" />&nbsp;
    <asp:Button ID="RejectButton" runat="server" Text="Reject Changes" OnClick="RejectButton_Click" UseSubmitBehavior="false" />&nbsp;
    <asp:Button ID="SubmitButton" runat="server" Text="Just Do Postback" Style="margin-left: 45px;" UseSubmitBehavior="false" />
    <div id="DropDisabledPanel" class="panel dz-drop-disabled">
        If you drop the files here - it will be rejected
    </div>
</asp:Content>
