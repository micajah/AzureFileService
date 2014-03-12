<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SimpleUpload.ascx.cs" Inherits="Micajah.AzureFileService.Web.Controls.SimpleUpload" %>

<mafs:FileUpload ID="FileUpload1" runat="server" ContainerName="micajahazurefileservice" ObjectType="ticket" ObjectId="12345" />
<br />
<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <asp:Panel ID="FilePanel" runat="server">
            <mafs:FileList ID="FileList1" runat="server" ShowViewAllAtOnceLink="false" ShowIcons="true" DeleteButtonText="X" Width="100%"
                ContainerName="micajahazurefileservice" ObjectType="ticket" ObjectId="12345">
            </mafs:FileList>
            <br />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
