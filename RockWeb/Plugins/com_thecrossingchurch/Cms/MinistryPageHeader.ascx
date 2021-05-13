<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MinistryPageHeader.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Cms.MinistryPageHeader" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

        <asp:Panel id="pnlDetails" runat="server" CssClass="row">

            <asp:Panel ID="pnlList" CssClass="col-xs-12" runat="server">

                <asp:Literal ID="lOutput" runat="server"></asp:Literal>

            </asp:Panel>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>