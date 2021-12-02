<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReprintLables.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.CheckIn.Manager.ReprintLables" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="row margin-b-sm">
            <div class="col-xs-12">
                <Rock:BootstrapButton runat="server" ID="btnReprintDeskA" CssClass="btn btn-info" OnClick="mdReprintLabelsCustom_Click">Reprint Desk A</Rock:BootstrapButton>
                <Rock:BootstrapButton runat="server" ID="btnReprintDeskB" CssClass="btn btn-success" OnClick="mdReprintLabelsCustom_Click">Reprint Desk B</Rock:BootstrapButton>
                <Rock:BootstrapButton runat="server" ID="btnReprintFoyer3" CssClass="btn btn-warning" OnClick="mdReprintLabelsCustom_Click">Reprint Foyer 3</Rock:BootstrapButton>
            </div>
        </div>

        <Rock:NotificationBox ID="nbReprintMessage" runat="server" Visible="false"></Rock:NotificationBox>
        <asp:HiddenField ID="hfCurrentAttendanceIds" runat="server" />
        <asp:HiddenField ID="hfPersonId" runat="server" />

    </ContentTemplate>
</asp:UpdatePanel>