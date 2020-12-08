<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Reprint.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.CheckIn.Reprint" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <!-- Reprint label functionality -->
        <Rock:NotificationBox ID="nbReprintMessage" runat="server" Visible="false"></Rock:NotificationBox>
        <Rock:ModalAlert ID="maNoLabelsFound" runat="server"></Rock:ModalAlert>
    </ContentTemplate>
</asp:UpdatePanel>