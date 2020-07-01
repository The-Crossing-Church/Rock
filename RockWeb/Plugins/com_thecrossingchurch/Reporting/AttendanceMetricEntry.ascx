<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceMetricEntry.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.AttendanceMetricEntry" %>
<script>
    function openPanel(panel) {
        if (panel == 'Sunday') {
            $('#SpecialEvent').css('display', 'none')
            $('#SpecialEvent').css('visibility', 'hidden')
            $('#SundayMorning').css('visibility', 'visible')
            $('#SundayMorning').css('display', 'block')
        } else {
            $('#SundayMorning').css('display', 'none')
            $('#SundayMorning').css('visibility', 'hidden')
            $('#SpecialEvent').css('visibility', 'visible')
            $('#SpecialEvent').css('display', 'block')
        }
    }
</script>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="row">
            <div class="col col-xs-0 col-md-3"></div>
            <div class="col col-xs-6 col-md-3 pad-8">
                <div class="tile panel panel-default" onclick="openPanel('Sunday')" ID="btnSunday">
                    <h3>Sunday Morning Attendance</h3><br />
                    <i class="fa fa-sun fa-3x"></i>
                </div>
            </div>
            <div class="col col-xs-6 col-md-3 pad-8">
                <div class="tile panel panel-default" onclick="openPanel('Special')" ID="btnSpecial">
                    <h3>Special Event Attendance</h3><br />
                    <i class="fa fa-calendar fa-3x"></i>
                </div>
            </div>
            <div class="col col-xs-0 col-md-3">
            </div>
        </div>
        <ContentTemplate ID="ErrorMsg" runat="server">
            <div class="row">
                <div class="col col-xs-0 col-md-2"></div>
                <div class="col col-xs-0 col-md-8">
                    <div class="alert alert-danger">
                        Please fill out all the information.
                    </div>
                </div>
                <div class="col col-xs-0 col-md-2"></div>
            </div>
        </ContentTemplate>
        <ContentTemplate ID="SundayMorning" style="display: none;" class="sunday-morning">
            <div class="row">
                <div class="col col-xs-0 col-md-2"></div>
                <div class="col col-xs-0 col-md-8">
                    <div class="well">
                        <h4>Sunday Morning Attendance</h4><br />
                        <div class="row">
                            <div class="col col-xs-6">
                                <Rock:DatePicker Required="true" runat="server" ID="OccurrenceDate" Label="Date"/>
                            </div>
                            <div class="col col-xs-6">
                                <Rock:RockDropDownList Required="true" runat="server" ID="Time" Label="Service" style="max-width:225px;" />
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
                            <Rock:BootstrapButton ID="btnAddAttendance" runat="server" OnClick="btnAddAttendance_Click" Text="Save" CssClass="pull-right btn btn-primary"/>
                        </div>
                    </div>
                </div>
                <div class="col col-xs-0 col-md-2"></div>
            </div>
        </ContentTemplate>
        <ContentTemplate ID="SpecialEvent" style="display: none;" class="special-events">
            <div class="row">
                <div class="col col-xs-0 col-md-2"></div>
                <div class="col col-xs-0 col-md-8">
                    <div class="well">
                        <h4>Special Event Attendance</h4><br />
                        <div class="row">
                            <div class="col col-xs-6">
                                <Rock:DatePicker Required="true" runat="server" ID="seDate" Label="Date"/>
                            </div>
                            <div class="col col-xs-6">
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
                                <Rock:RockTextBox Required="true" runat="server" ID="seAttendance" Label="Attendance" style="max-width:225px;"/>
                            </div>
                            <div class="col col-xs-6">
                                <Rock:LocationItemPicker Required="true" Label="Location" ID="seLocation" runat="server" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-12">
                                <Rock:RockTextBox runat="server" ID="seNotes" Label="Notes" TextMode="MultiLine" />
                            </div>
                        </div>
                        <div class="row">
                            <Rock:BootstrapButton ID="btnSEAddAttendance" runat="server" OnClick="btnAddAttendance_Click" Text="Save" CssClass="pull-right btn btn-primary"/>
                        </div>
                    </div>
                </div>
                <div class="col col-xs-0 col-md-2"></div>
            </div>
        </ContentTemplate>
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
    .required.has-error .control-wrapper .picker.picker-select a.picker-label {
        border: 1px solid #e55235; 
    }
    .required.has-error .control-wrapper .picker.picker-select a.picker-label:focus {
        box-shadow: 0 0 0 3px rgba(229, 82, 53, 0.25);
    }
</style>