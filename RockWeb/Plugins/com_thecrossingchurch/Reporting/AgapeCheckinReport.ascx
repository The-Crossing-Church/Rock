<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AgapeCheckinReport.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.AgapeCheckinReport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />
        <div class="panel panel-block">
            <div class="panel-heading" data-toggle="collapse" data-target="#filters" aria-expanded="false" aria-controls="filters">
                <h4 class="panel-title">Report Settings</h4>
            </div>
            <div class="panel-body collapse" id="filters">
                <div class="row">
                    <div class="col col-xs-12">
                        <Rock:DatePicker runat="server" ID="reportDate" Label="Report Date" OnValueChanged="reportDate_ValueChanged"></Rock:DatePicker>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col col-xs-12 col-md-6">
                <Rock:Grid ID="grdAgapeKids" runat="server" AllowSorting="false">
                    <Columns>
                        <Rock:RockBoundField HeaderText="Name" DataField="Name" SortExpression="Name" ExcelExportBehavior="AlwaysInclude" />
                        <Rock:RockBoundField HeaderText="Checkin" DataField="Checkin" SortExpression="Checkin" ExcelExportBehavior="AlwaysInclude" />
                    </Columns>
                </Rock:Grid>
            </div>
            <div class="col col-xs-12 col-md-6">
                <Rock:Grid ID="grdBuddies" runat="server" AllowSorting="false">
                    <Columns>
                        <Rock:RockBoundField HeaderText="Name" DataField="Name" SortExpression="Name" ExcelExportBehavior="AlwaysInclude" />
                        <Rock:RockBoundField HeaderText="Checkin" DataField="Checkin" SortExpression="Checkin" ExcelExportBehavior="AlwaysInclude" />
                    </Columns>
                </Rock:Grid>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
