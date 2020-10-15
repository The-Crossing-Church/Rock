<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LeaderInfoDetailedView.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Veritas.LeaderInfoDetailedView" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="card">
            <div runat="server" id="divHeader" ></div>
            <div class="row">
                <div class="col col-xs-12">
                    <a runat="server" id="aCreateNew" class="btn btn-primary pull-right">Add Meeting</a>
                </div>
            </div>
        </div>
        <br />
        <div runat="server" id="divInfo"></div>
    </ContentTemplate>
</asp:UpdatePanel>
<style>
.card {
    box-shadow: 4px 4px 8px rgba(0,0,0,.2);
    border-radius: 4px;
    overflow: hidden;
    padding: 16px;
}
.floating-label {
    font-size: 12px;
    text-transform: uppercase;
    padding-top: 8px;
}
.card-staff {
    color: #717171;
}
.card-date {
    font-size: 18px;
}
a, a:hover, a:visited {
    color: black; 
}
hr {
    margin: 8px 0px; 
}
</style>
