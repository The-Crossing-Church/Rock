<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceMetricEntry.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.AttendanceMetricEntry" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="row">
            <div class="col col-xs-0 col-md-3"></div>
            <div class="col col-xs-6 col-md-3 pad-8">
                <Rock:BootstrapButton runat="server" OnClick="OpenPanel" ID="btnSunday" CausesValidation="false">
                    <div class="tile panel panel-default" ID="SundayPnl" runat="server">
                        <h3>Sunday Morning Attendance</h3><br />
                        <i class="fa fa-sun fa-3x"></i>
                    </div>
                </Rock:BootstrapButton>
            </div>
            <div class="col col-xs-6 col-md-3 pad-8">
                <Rock:BootstrapButton runat="server" OnClick="OpenPanel" ID="btnSpecial" CausesValidation="false">
                    <div class="tile panel panel-default" ID="SpecialPnl" runat="server">
                        <h3>Special Event Attendance</h3><br />
                        <i class="fa fa-calendar fa-3x"></i>
                    </div>
                </Rock:BootstrapButton>
            </div>
            <div class="col col-xs-0 col-md-3">
            </div>
        </div>
        <div class="row" runat="server" ID="EntryForm" Visible="false">
            <div class="col col-xs-0 col-md-2"></div>
            <div class="col col-xs-0 col-md-8">
                <div class="well">
                    <asp:Placeholder runat="server" ID="headingPlaceholder"></asp:Placeholder><br />
                    <div class="row">
                        <div class="col col-xs-6">
                            <Rock:DatePicker Required="true" runat="server" ID="OccurrenceDate" Label="Date"/>
                        </div>
                        <div class="col col-xs-6">
                            <Rock:RockDropDownList Required="true" runat="server" ID="Time" Label="Service" style="max-width:225px;" />
                            <Rock:TimePicker Required="true" runat="server" ID="seTime" Label="Time" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col col-xs-6">
                            <Rock:RockDropDownList Required="true" runat="server" ID="ServiceType" Label="Service Type" style="max-width:225px;" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col col-xs-6">
                            <Rock:RockTextBox Required="true" runat="server" ID="Attendance" Label="Attendance" style="max-width:225px;"/>
                        </div>
                        <div class="col col-xs-6">
                            <Rock:LocationItemPicker Required="true" Label="Location" ID="Location" runat="server" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col col-xs-12">
                            <Rock:RockTextBox runat="server" ID="Notes" Label="Notes" TextMode="MultiLine" />
                        </div>
                    </div>
                    <div class="row">
                        <Button ID="btnConfirmRemoveAttendance"  onclick="deleteMetric()" class="pull-left btn btn-danger"  CausesValidation="false">Delete</Button>
                        <Rock:BootstrapButton ID="btnAddAttendance" runat="server" OnClick="btnAddAttendance_Click" Text="Save" CssClass="pull-right btn btn-primary"/>
                    </div>
                </div>
            </div>
            <div class="col col-xs-0 col-md-2"></div>
        </div>
        <div style="display:none">
            <Rock:BootstrapButton ID="btnRemoveAttendance" runat="server" OnClick="btnRemoveAttendance_Click" Text="Delete" CssClass="btn btn-danger" CausesValidation="false"></Rock:BootstrapButton>
        </div>
            <script>
                function deleteMetric() {
                // delete prompt
                var confirmMessage = 'Are you sure you want to delete this attendance entry?';
                Rock.dialogs.confirm(confirmMessage, function (result) {
                    if (result) {
                        var btn = $('[id$="btnRemoveAttendance"]');
                        btn[0].click();
                    }
                });
            }
            </script>
    </ContentTemplate>
</asp:UpdatePanel>
<style>
    .pad-8 {
        padding: 8px;
    }
    .tile {
        text-align: center;
        padding: 8px;
        border-radius: 4px !important;
        box-shadow: 0 2px 8px 0 rgba(0,0,0,0.2) !important;
    }
    .tile:hover {
        cursor: pointer;
    }
    .tile.pressed {
        box-shadow: inset 0 2px 8px 0 rgba(0,0,0,0.2) !important;
        background-color: #f3f3f3;
    }
    .required.has-error .control-wrapper .picker.picker-select a.picker-label {
        border: 1px solid #e55235; 
    }
    .required.has-error .control-wrapper .picker.picker-select a.picker-label:focus {
        box-shadow: 0 0 0 3px rgba(229, 82, 53, 0.25);
    }
</style>