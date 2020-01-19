<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReprintLabelsLocal.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Checkin.ReprintLabelsLocal" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="row">
            <div class="col-md-12">
                <asp:Literal ID="lWarning" runat="server" />
            </div>
        </div>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-print"></i> Local Print</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <asp:Button ID="bOk" runat="server" Text='Ok' CssClass="btn btn-default btn-sm" OnClick="bOk_Click" />
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
