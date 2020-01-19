<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceFilter.ascx.cs" Inherits="RockWeb.Plugins.rocks_pillars.Core.AttendanceFilter" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">Attendance Filter</h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <asp:Panel ID="pnlArea" runat="server" Visible="false" CssClass="row">
                    <div class="col-sm-4">
                        <Rock:ButtonDropDownList ID="ddlArea" runat="server" Label="Area" />
                    </div>
                </asp:Panel>
                <div class="row">
                    <div class="col-sm-4">
                        <Rock:DatePicker ID="dpDate" runat="server" Label="Date" AutoPostBack="true" OnTextChanged="dpDate_TextChanged" Required="true"  />
                        <Rock:SlidingDateRangePicker ID="sdrpDates" runat="server" Label="Date Range" OnSelectedDateRangeChanged="sdrpDates_SelectedDateRangeChanged" Required="true" Visible="false"  EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" />
                    </div>
                    <div class="col-sm-4">
                        <asp:Panel ID="pnlSchedules" runat="server" CssClass="form-group">
                            <label class="control-label">Service Times(s)</label> &nbsp; <asp:LinkButton ID="lbRefreshSchedules" runat="server" OnClick="sdrpDates_SelectedDateRangeChanged" CausesValidation="false"><i class="fa fa-sm fa-sync"></i></asp:LinkButton>
                            <div class="control-wrapper">
								<asp:Literal ID="lSchedules" runat="server" />
								<Rock:RockListBox ID="lbSchedules" runat="server" SelectionMode="Multiple" Visible="true" RequiredErrorMessage="Service Time(s) is Required"
									DataValueField="Id" DataTextField="Name" cssClass="input-width-md" />
                            </div>
                        </asp:Panel>
                    </div>
                    <div class="col-sm-4">
                        <asp:Panel runat="server" id="pnlCheckInTime" cssClass="row" Visible="false">
                            <div class="col-sm-6">
                                <Rock:TimePicker ID="tpCheckInTimeStart" runat="server" Label="Start Check-In Time" />
                            </div>
                            <div class="col-sm-6">
                                <Rock:TimePicker ID="tpCheckInTimeEnd" runat="server" Label="End Check-In Time" />
                            </div>
                        </asp:Panel>
                        <Rock:RockDropDownList ID="ddlEarlyCheckin" runat="server" Visible="false" />
                    </div>
                </div>

                <div class="pull-right">
                    <Rock:BootstrapButton ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" />
                </div>

            </div>
       
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
