﻿<%@ Page Title="File List" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FileList.aspx.cs" Inherits="Micajah.AzureFileService.Web.FileList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="size">
        <p title="Including all thumbnails">
            Total files size:
            <asp:Label ID="ContainerLengthLabel" runat="server"></asp:Label>
        </p>
    </div>
    <div class="left" style="width: 40%;">
        <div class="left">
            <p>
                The files list with the deleting function and thumbnails
            </p>
        </div>
        <div class="clear"></div>
        <table>
            <tr>
                <td>Please configure the list:</td>
            </tr>
            <tr>
                <td>Icon size&nbsp;<asp:DropDownList ID="IconSizeList" runat="server">
                </asp:DropDownList>&nbsp;
                <asp:CheckBox ID="ThumbnailsCheckBox" runat="server" Text="Thumbnails" ToolTip="Whether the tool tip for a file displays the thumbnail or the file itself" Checked="true" />&nbsp;
                <asp:CheckBox ID="IconsCheckBox" runat="server" Text="Icons" ToolTip="Whether the icons column is displayed" Checked="true" />&nbsp;
                <asp:Button ID="Button1" runat="server" Text="Submit" OnClick="Button1_Click" />
                </td>
            </tr>
        </table>
        <br />
        <mafs:FileList ID="FileList2" runat="server" ContainerName="micajahazurefileservice" ObjectType="ticket" ObjectId="12345"
            DateTimeToolTipFormatString="{0:g} (UTC-5)" ShowIcons="True" RenderingMode="Grid">
        </mafs:FileList>
    </div>
    <div class="right" style="width: 60%;">
        <p>
            The thumbnails list
        </p>
        <table>
            <tr>
                <td>Please input the files extensions separated by comma to display in this list:
                </td>
            </tr>
            <tr>
                <td>
                    <asp:TextBox ID="FilterTextBox" runat="server" Text="" />&nbsp;
                    <asp:CheckBox ID="NegateCheckBox" runat="server" Text="Negate" ToolTip="Negate filter" />&nbsp;
                    <asp:HyperLink ID="ResetLink" runat="server" Text="Reset" ToolTip="Reset filter" NavigateUrl="javascript:void(0);"></asp:HyperLink>&nbsp;
                    <asp:Button ID="SubmitButton" runat="server" Text="Submit" OnClick="SubmitButton_Click" />
                    <br />
                    <asp:HyperLink ID="ArchiveLink" runat="server" Text="Archive" NavigateUrl="javascript:void(0);"></asp:HyperLink>&nbsp;
                    <asp:HyperLink ID="AudioLink" runat="server" Text="Audio" NavigateUrl="javascript:void(0);"></asp:HyperLink>&nbsp;
                    <asp:HyperLink ID="DocumentLink" runat="server" Text="Document" NavigateUrl="javascript:void(0);"></asp:HyperLink>&nbsp;
                    <asp:HyperLink ID="ImageLink" runat="server" Text="Images" NavigateUrl="javascript:void(0);"></asp:HyperLink>&nbsp;
                    <asp:HyperLink ID="TextLink" runat="server" Text="Text" NavigateUrl="javascript:void(0);"></asp:HyperLink>&nbsp;
                    <asp:HyperLink ID="VideoLink" runat="server" Text="Video" NavigateUrl="javascript:void(0);"></asp:HyperLink>&nbsp;
                </td>
            </tr>
        </table>
        <br />
        <mafs:FileList ID="FileList3" runat="server" ContainerName="micajahazurefileservice" ObjectType="ticket" ObjectId="12345"
            DateTimeToolTipFormatString="{0:g} (UTC-5)" RenderingMode="Thumbnails" RepeatColumns="5" ShowViewAllAtOnceLink="false" />
    </div>
</asp:Content>
