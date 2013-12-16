﻿<%@ Page Title="File Upload" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Micajah.AzureFileService.Web.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <mafs:FileUpload ID="FileUpload1" runat="server" ContainerName="micajahazurefileservice" TemporaryContainerName="micajahazurefileservicetemp" ObjectType="ticket" ObjectId="12345" Accept="image/*" />
    <br />
    <asp:Button ID="AcceptButton" runat="server" Text="Accept Changes" OnClick="AcceptButton_Click" />&nbsp;
    <asp:Button ID="RejectButton" runat="server" Text="Reject Changes" OnClick="RejectButton_Click" />
</asp:Content>
