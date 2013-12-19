<%@ Page Title="File List" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FileList.aspx.cs" Inherits="Micajah.AzureFileService.Web.FileList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <p>
        The files list with the deleting function and thumbnails
    </p>
    <table>
        <tr>
            <td>Please select the icon size:
            </td>
            <td>
                <asp:DropDownList ID="IconSizeList" runat="server">
                </asp:DropDownList>
            </td>
            <td>
                <asp:Button ID="Button1" runat="server" Text="Submit" OnClick="Button1_Click" />
            </td>
        </tr>
    </table>
    <br />
    <mafs:FileList ID="FileList2" runat="server" ContainerName="micajahazurefileservice" ObjectType="ticket" ObjectId="12345"
        DateTimeToolTipFormatString="{0:g} (UTC-5)" ShowIcons="True" RenderingMode="Grid">
    </mafs:FileList>
    <%--<p>
        <br />
        <br />
        The thumbnails list
    </p>
    <table>
        <tr>
            <td>Please input the files extensions separated by comma to display in this list:
            </td>
        </tr>
        <tr>
            <td>
                <asp:TextBox ID="FilterTextBox" runat="server" Text="video" />&nbsp;
                <asp:CheckBox ID="NegateCheckBox" runat="server" Text="Negate filter" />&nbsp;
                <asp:Button ID="SubmitButton" runat="server" Text="Submit" OnClick="SubmitButton_Click" />
            </td>
        </tr>
    </table>
    <br />
    <mafs:FileList ID="FileList3" runat="server" LocalObjectType="test object type" LocalObjectId="1"
        DateTimeToolTipFormatString="{0:g} (UTC-5)" RenderingMode="ThumbnailsList" ShowViewAllAtOnceLink="false"
        OrganizationId="f53995a0-2b8c-4c16-8be4-476495220ea6" OrganizationName="test organization"
        DepartmentId="96ca7b84-bdf7-4af2-b78c-29e60f6569fe" DepartmentName="test department" />--%>
</asp:Content>
