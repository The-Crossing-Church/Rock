<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CKMoveUpReport.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.CKMoveUpReport" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />
        <div class="panel panel-block">
            <div class="panel-heading">
                <h4 class="panel-title">Report Settings</h4>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col col-xs-12 col-md-4">
                        <Rock:DateRangePicker runat="server" ID="pkrBirthdate" Label="Birthdate Range" Help="Only people with birth dates in this range will appear in the report" />
                    </div>
                    <div class="col col-xs-12 col-md-4">
                        <Rock:DateRangePicker runat="server" ID="pkrAttendance" Label="Attendance Range" Help="Only attendance that falls within this range will be used in the report" />
                    </div>
                    <div class="col col-xs-12 col-md-4">
                        <Rock:SchedulePicker runat="server" ID="pkrSchedule" Label="Schedules" Help="Only the selected schedules will be used in the report" AllowMultiSelect="true" />
                    </div>
                </div>
                <div class="row">
                    <div class="col col-xs-6">
                        <div style="height: 90px;">
                            <canvas id="chtService"></canvas>
                        </div>
                    </div>
                    <div class="col col-xs-6">
                        <Rock:BootstrapButton CssClass="pull-right btn btn-primary" runat="server" OnClick="RunReport_Click">Run Report</Rock:BootstrapButton>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col col-xs-12">
                <Rock:Grid ID="grdKids" runat="server" AllowSorting="false">
                    <Columns>
                        <Rock:RockBoundField HeaderText="Name" DataField="Name" SortExpression="Name" ExcelExportBehavior="AlwaysInclude" />
                        <Rock:RockBoundField HeaderText="Days Attended" DataField="DaysAttended" SortExpression="DaysAttended" ExcelExportBehavior="AlwaysInclude" />
                        <Rock:RockBoundField HeaderText="Services Attended" DataField="ServicesAttended" SortExpression="ServicesAttended" ExcelExportBehavior="AlwaysInclude" />
                    </Columns>
                </Rock:Grid>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
<script>
    function buildChart(data) {
        console.log('build the chart!')
        console.log(data)
        let ctx = document.getElementById('chtService');
        let cData = {
            labels: data.map(d => d.Schedule),
            datasets: [
                {
                    label: "Percent Attending",
                    data: data.map(d => d.Data),
                    backgroundColor: [
                        "#A2C3CC", "#5F9BAC", "#1B3142", "#054557"
                    ]
                }
            ]
        }
        new Chart(ctx, {
            type: 'doughnut',
            data: cData,
            options: {
                plugins: {
                    legend: {
                        position: 'right'
                    }
                },
                aspectRatio: 2
            }
        });
    }
</script>
