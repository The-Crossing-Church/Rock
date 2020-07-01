<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceMetricEntry.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.AttendanceMetricEntry" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="row">
            <div class="col col-xs-0 col-md-3"></div>
            <div class="col col-xs-6 col-md-3 pad-8">
                <Rock:BootstrapButton runat="server" OnClick="OpenPanel" ID="btnSunday">
                    <div class="tile panel panel-default">
                        <h3>Sunday Morning Attendance</h3><br /> 
                        <i class="fa fa-sun fa-3x"></i>
                    </div>
                </Rock:BootstrapButton>
            </div>
            <div class="col col-xs-6 col-md-3 pad-8">
                <Rock:BootstrapButton runat="server" OnClick="OpenPanel" ID="btnSpecial">
                    <div class="tile panel panel-default">
                        <h3>Special Event Attendance</h3><br />
                        <i class="fa fa-calendar fa-3x"></i>
                    </div>
                </Rock:BootstrapButton>
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
        <div class="row">
            <div class="col col-xs-0 col-md-2"></div>
            <div class="col col-xs-12 col-md-8">
                <div class="well">
                    <asp:PlaceHolder runat="server" ID="Entryform" Visible="false"></asp:PlaceHolder>
                    <div class="row">
                        <div class="col col-xs-12">
                            <Rock:BootstrapButton Text="Save" ID="btnSave" OnClick="btnSave_Click" runat="server" CssClass="btn btn-primary pull-right"></Rock:BootstrapButton>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col col-xs-0 col-md-2"></div>
        </div>
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