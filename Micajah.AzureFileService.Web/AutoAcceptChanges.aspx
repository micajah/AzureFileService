<%@ Page Title="Auto Upload" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AutoAcceptChanges.aspx.cs" Inherits="Micajah.AzureFileService.Web.AutoAcceptChanges" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <p>Drop files to the area below for attaching to the files list</p>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <mafs:FileUpload ID="FileUpload2" runat="server" ContainerName="micajahazurefileservice" ObjectType="ticket" ObjectId="12345"
                AutoAcceptChanges="true" EnablePreview="false" DropElement="document.getElementById('FileUpload2DropPanel')" />
            <div id="FileUpload2DropPanel" style="width: 500px;">
                <mafs:FileList ID="FileList2" runat="server" ContainerName="micajahazurefileservice" ObjectType="ticket" ObjectId="12345"
                    DateTimeToolTipFormatString="{0:g} (UTC-5)" ShowIcons="false" ShowFileToolTip="false" RenderingMode="Grid" Width="100%">
                </mafs:FileList>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <script type="text/javascript" src="Scripts/Site.js"></script>
</asp:Content>
