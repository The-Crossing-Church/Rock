<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LeaderInfoDashboard.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Veritas.LeaderInfoDashboard" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">Filters</h3>
            </div>
            <div class="panel-body">
                <Rock:GroupPicker ID="pkrGroup" Label="Small Group" runat="server" OnSelectItem="pkrGroup_SelectItem" />
                <div id="divLeaders" runat="server"></div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<style>

</style>
