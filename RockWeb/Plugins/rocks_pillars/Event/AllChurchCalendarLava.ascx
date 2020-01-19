<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AllChurchCalendarLava.ascx.cs" Inherits="RockWeb.Plugins.rocks_pillars.Event.AllChurchCalendarLava" %>

<style>

.month {
    font-size: 3.5rem;
    font-weight: 500;
    margin-bottom: 4px;
}

.days {
    background: black;
    opacity: 0.75;
    margin-bottom: -10px;
}

.row > h5{
	text-align:  center;
	float: left;
	width: 14.28571428571429%;
	color: white;
}

.row > .calendar-day {
	font-family: 'Roboto', sans-serif;
	width: 14.28571428571429%;
	border: 1px solid rgb(235, 235, 235);
	border-right-width: 0px;
	border-top-width: 0px;
	min-height: 190px;
    background: white;
	transition: 0.4s;
	color: black;
}

.row > .calendar-day:hover {
    background: grey;
    color: white;
}

.out-month {
    opacity: 0.5;
}

.calendar-day > time {
	position: absolute;
	display: block;
	top: 0px;
	right: 0px;
	font-size: 16px;
	font-weight: 300;
	width: 100%;
	padding: 5px 10px 0px 3px;
	text-align: left;
}

.martop15 {
    margin-top: 15px;
}

.event-month {
    position: relative;
    font-size: 12px;
    margin: 5px;
    padding: 2px;
    border-radius: 5px;
}

.event-month .tooltiptext {
    visibility: hidden;
    width: 120px;
    background-color: black;
    color: #fff;
    text-align: center;
    border-radius: 6px;
    padding: 5px 0;
  
    /* Position the tooltip */
    position: absolute;
    z-index: 1;
    top: 100%;
    left: 40%;
    margin-left: -60px;
}

.event-month:hover .tooltiptext {
    visibility: visible;
}

.day-view > h5{
	text-align:  center;
	float: left;
	width: 100%;
	color: white;
}


</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="cblCampus" />
        <asp:AsyncPostBackTrigger ControlID="cblCategory" />
    </Triggers>
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

        <asp:Panel id="pnlDetails" runat="server" CssClass="row"> 

            <asp:Panel ID="pnlFilters" CssClass="col-md-3 col-xl-2 hidden-print" runat="server">

                <asp:Panel ID="pnlCalendar" CssClass="calendar" runat="server">
                    <asp:Calendar ID="calEventCalendar" runat="server" DayNameFormat="FirstLetter" SelectionMode="Day" BorderStyle="None"
                        TitleStyle-BackColor="#ffffff" NextPrevStyle-ForeColor="#333333" FirstDayOfWeek="Sunday" Width="100%" CssClass="calendar-month" OnSelectionChanged="calEventCalendar_SelectionChanged" OnDayRender="calEventCalendar_DayRender" OnVisibleMonthChanged="calEventCalendar_VisibleMonthChanged">
                        <DayStyle CssClass="calendar-day" />
                        <TodayDayStyle CssClass="calendar-today" />
                        <SelectedDayStyle CssClass="calendar-selected" BackColor="Transparent" />
                        <OtherMonthDayStyle CssClass="calendar-last-month" />
                        <DayHeaderStyle CssClass="calendar-day-header" />
                        <NextPrevStyle CssClass="calendar-next-prev" />
                        <TitleStyle CssClass="calendar-title" />
                    </asp:Calendar>
                </asp:Panel>

                <% if ( CampusPanelOpen || CampusPanelClosed )
                  { %>
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <h4 class="panel-title">
                                <a role="button" data-toggle="collapse" href="#collapseOne">Campuses
                                </a>
                            </h4>
                        </div>
                        <div id="collapseOne" class='<%= CampusPanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
                            <div class="panel-body">
                <% } %>

                                <%-- Note: RockControlWrapper/Div/CheckboxList is being used instead of just a RockCheckBoxList, because autopostback does not currently work for RockControlCheckbox--%>
                                <Rock:RockControlWrapper ID="rcwCampus" runat="server" Label="Filter by Campus">
                                    <div class="controls">
                                        <asp:CheckBoxList ID="cblCampus" RepeatDirection="Vertical" runat="server" DataTextField="Name" DataValueField="Id"
                                            OnSelectedIndexChanged="cblCampus_SelectedIndexChanged" AutoPostBack="true" />
                                    </div>
                                </Rock:RockControlWrapper>

                <% if ( CampusPanelOpen || CampusPanelClosed )
                    { %>
                            </div>
                        </div>
                    </div>
                <% } %>

                <% if ( CategoryPanelOpen || CategoryPanelClosed )
                   { %>
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <h4 class="panel-title">
                                <a role="button" data-toggle="collapse" href="#collapseTwo">Ministries
                                </a>
                            </h4>
                        </div>
                        <div id="collapseTwo" class='<%= CategoryPanelOpen ? "panel-collapse collapse in" : "panel-collapse collapse out" %>'>
                            <div class="panel-body">
                <% } %>

                                <Rock:RockControlWrapper ID="rcwCategory" runat="server" Label="Filter by Category">
                                    <div class="controls">
                                        <asp:CheckBoxList ID="cblCategory" RepeatDirection="Vertical" runat="server" DataTextField="Value" DataValueField="Id" OnSelectedIndexChanged="cblCategory_SelectedIndexChanged" AutoPostBack="true" />
                                    </div>
                                </Rock:RockControlWrapper>

                <% if ( CategoryPanelOpen || CategoryPanelClosed )
                    { %>
                            </div>
                        </div>
                    </div>
                <% } %>

                <%--<Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Select Range" /><asp:LinkButton ID="lbDateRangeRefresh" runat="server" CssClass="btn btn-default btn-sm" Text="Refresh" OnClick="lbDateRangeRefresh_Click" />--%>

            </asp:Panel>

            <asp:Panel ID="pnlList" CssClass="col-md-9 col-xl-10" runat="server">

                <Rock:BootstrapButton ID="btnPrev" runat="server" CssClass="btn btn-default" OnClick="btnPrev_Click"><i class="fa fa-caret-left"></i></Rock:BootstrapButton>

                <div class="btn-group hidden-print" role="group">
                    
                    <Rock:BootstrapButton ID="btnDay" runat="server" CssClass="btn btn-default" Text="Day" OnClick="btnViewMode_Click" />
                    <Rock:BootstrapButton ID="btnWeek" runat="server" CssClass="btn btn-default" Text="Week" OnClick="btnViewMode_Click" />
                    <Rock:BootstrapButton ID="btnMonth" runat="server" CssClass="btn btn-default" Text="Month" OnClick="btnViewMode_Click" />
                    
                </div>

                <Rock:BootstrapButton ID="btnNext" runat="server" CssClass="btn btn-default" OnClick="btnNext_Click"><i class="fa fa-caret-right"></i></Rock:BootstrapButton>

                <Rock:BootstrapButton ID="btnAddEvent" runat="server" CssClass="btn btn-default" OnClick="btnAddEvent_Click" ToolTip="Add Event"><i class="fa fa-plus"></i></Rock:BootstrapButton>

                <asp:Literal ID="lOutput" runat="server"></asp:Literal>
                <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>

            </asp:Panel>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
