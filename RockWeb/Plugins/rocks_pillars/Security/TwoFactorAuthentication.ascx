<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TwoFactorAuthentication.ascx.cs" Inherits="RockWeb.Plugins.rocks_pillars.Security.TwoFactorAuthentication" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />
        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

        <asp:Panel ID="pnlSelect" runat="server" Visible="true" CssClass="login" >

            <Rock:RockRadioButtonList ID="rblMediums" runat="server" Label="Send Code To" Required="true" />

            <Rock:BootstrapButton ID="btnSelect" runat="server" Text="Send Code" CssClass="btn btn-primary" OnClick="btnSelect_Click" DataLoadingText="Sending Code..." />
            <asp:Button ID="btnCancelSelect" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancelSelect_Click" CausesValidation="false" />
        
        </asp:Panel>

        <asp:Panel ID="pnlEnterCode" runat="server" Visible="false" CssClass="login" >
        
            <Rock:RockTextBox ID="tbCode" runat="server" Label="Code" Required="true" />

            <Rock:BootstrapButton ID="btnSubmit" runat="server" Text="Log In" CssClass="btn btn-primary" OnClick="btnLogin_Click" DataLoadingText="Logging In..." />
            <asp:Button ID="btnCancelCode" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancelLogin_Click" CausesValidation="false" />
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
