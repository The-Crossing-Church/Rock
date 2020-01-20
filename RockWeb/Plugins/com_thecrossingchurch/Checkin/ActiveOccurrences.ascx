<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ActiveOccurrences.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Checkin.ActiveOccurrences" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar-check"></i> <asp:Literal ID="lOccurrenceCount" runat="server" /> Active Occurrences</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">

                    <Rock:ModalAlert ID="maWarningDialog" runat="server" />

                    <Rock:GridFilter ID="gfOccurrenceFilter" runat="server" OnApplyFilterClick="gfOccurrenceFilter_ApplyFilterClick" 
                        OnDisplayFilterValue="gfOccurrenceFilter_DisplayFilterValue" OnClearFilterClick="gfOccurrenceFilter_ClearFilterClick">
                        <Rock:RockDropDownList ID="ddlAttendanceArea" runat="server" Label="Attendance Area" />
                        <Rock:RockCheckBox ID="cbOnlyWithAttendees" runat="server" Label="Only Occurrences with Attendees" Text="Yes"  />
                        <Rock:DateTimePicker ID="dtpActiveDate" runat="server" Label="Active At" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gOccurrences" runat="server" AllowSorting="true" RowItemText="Occurrence" ExportSource="ColumnOutput" >
                        <Columns>
                            <Rock:RockBoundField DataField="GroupName" HeaderText="Class Name" SortExpression="GroupName" />
                            <Rock:RockBoundField DataField="Attendees" HeaderText="Attendees" SortExpression="Attendees" />
                            <Rock:RockBoundField DataField="ScheduleName" HeaderText="Schedule" SortExpression="ScheduleName" />
                            <Rock:TimeField DataField="CheckInStart" HeaderText="Check-In Begins" SortExpression="CheckInStart"/>
                            <Rock:TimeField DataField="Start" HeaderText="Start" SortExpression="Start"/>
                            <Rock:TimeField DataField="End" HeaderText="End" SortExpression="End"/>
                            <Rock:RockBoundField DataField="Threshold" HeaderText="Thresholds (soft/hard)" />
                            <asp:HyperLinkField DataTextField="Location" HeaderText="Location" SortExpression="Location" DataNavigateUrlFields="GroupTypeId,GroupId,LocationId" Target="_blank" />
                        </Columns>
                    </Rock:Grid>

                </div>
            </div>
        </asp:Panel>

        <div style="display:none">
            <asp:LinkButton ID="lbRefresh" runat="server" OnClick="lbRefresh_Click" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
