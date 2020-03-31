<%@ Control Language="C#" AutoEventWireup="true" CodeFile="KCPowerFailure.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.KCPowerFailure" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="row">
            <div class="col pull-right">
                <div style="display:inline-block; padding-right: 8px;" class="report-date">
                    <Rock:DatePicker ID="tagDate" runat="server" Label="Report Date" Required="true"  class="report-date"/>
                </div>
                <div style="display:inline-block;">
                    <Rock:BootstrapButton ID="btnExportReports" runat="server" Text="Export Reports" CssClass="btn btn-primary" OnClick="btnExportReports_Click" style="display: inline-block; vertical-align: bottom;" />
                </div>
                <%--<Rock:BootstrapButton ID="btnExport" runat="server" Text="Export Excel" CssClass="btn btn-primary" OnClick="btnExport_Click" />
                <Rock:BootstrapButton ID="btnPDF" runat="server" Text="Export PDF" CssClass="btn btn-primary" OnClick="btnExportPDF_Click" />
                <Rock:BootstrapButton ID="btnTags" runat="server" Text="Print Tags" CssClass="btn btn-primary" OnClick="btnTags_Click" />--%>
            </div>
        </div>
        <div class="custom-container" id="DataContainer" runat="server">
            <asp:PlaceHolder ID="phContent" runat="server" Visible="false" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<style>
    .class-container table td, .class-container table th {
        min-width: 150px;
        text-align: left;
        border-right: 1px solid black;
        border-bottom: 1px solid black;
        padding: 4px;
    }
    .class-container table tr {
        border-left: 1px solid black;
    }
    .header-row {
        border-top: 1px solid black;
        font-weight: bold;
    }
    .class-name {
        font-weight: bold;
        font-size: 22px;
    }
    .bg-secondary {
        background-color: #F1F1F1;
    }
    .report-date {
        display: inline-block;
        vertical-align: bottom;
    }
    .report-date div {
        margin: 0px;
    }
</style>
