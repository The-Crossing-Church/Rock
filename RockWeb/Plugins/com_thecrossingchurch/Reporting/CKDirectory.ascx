<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CKDirectory.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.CKDirectory" %>
<script>
function notes() {
    $('.add-note').click(function(e) {
        e.preventDefault();
        window.open($(this).attr('href'), 'fbShareWindow', 'height=450, width=550, top=' + ($(window).height() / 2 - 275) + ', left=' + ($(window).width() / 2 - 225) + ', toolbar=0, location=0, menubar=0, directories=0, scrollbars=0');
        return false;
    });
}
</script>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="well">
            <div class="row">
                <div class="col col-xs-12 col-sm-3">
                    <Rock:GroupPicker ID="GroupIds" runat="server" RootGroupId="115942" AllowMultiSelect="true" Label="Groups"/>
                </div>
                <div class="col col-xs-12 col-sm-3">
                    <Rock:RockTextBox ID="ReportName" runat="server" Label="Report Name"></Rock:RockTextBox>
                </div>
            </div>
            <div class="row">
                <div class="pull-right">
                    <Rock:BootstrapButton ID="btnExportExcel" runat="server" Text="Export Excel" CssClass="btn btn-primary" OnClick="btnExportExcel_Click" />
                    <Rock:BootstrapButton ID="btnExportPDF" runat="server" Text="Export PDF" CssClass="btn btn-primary" OnClick="btnExportPDF_Click" />
                </div>
            </div>
        </div>
        <div class="custom-container" id="DataContainer" runat="server">
            <asp:PlaceHolder ID="phContent" runat="server" Visible="false" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<style>
    .btn {
        margin-right: 16px;
    }
</style>
