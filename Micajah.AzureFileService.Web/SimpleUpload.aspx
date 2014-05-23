<%@ Page Title="Simple Upload" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SimpleUpload.aspx.cs" Inherits="Micajah.AzureFileService.Web.SimpleUpload" %>

<%@ Register Src="~/Controls/SimpleUpload.ascx" TagPrefix="uc1" TagName="SimpleUpload" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <p>
        The User Control with combination of FileUpload and FileList controls
    </p>
    <div style="width: 650px;">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <uc1:SimpleUpload ID="SimpleUpload1" runat="server" />
                <asp:Button ID="Button1" runat="server" Text="Update" OnClick="Button1_Click" />
                or
            <asp:LinkButton ID="Button2" runat="server" Text="Cancel" OnClick="Button2_Click" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
