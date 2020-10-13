<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CustomContentBlock.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Cms.CustomContentBlock" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div id="content" runat="server"></div>
    </ContentTemplate>
</asp:UpdatePanel>
<style>
    .row.equal {
        display: flex;
        display: -webkit-flex; 
        flex-wrap: wrap; 
    }
    .card {
        height: 100%;
    }
    .card-actions {
        text-align: center; 
    }
    .col {
        padding-top: 6px;
        padding-bottom: 6px;
    }
</style>
