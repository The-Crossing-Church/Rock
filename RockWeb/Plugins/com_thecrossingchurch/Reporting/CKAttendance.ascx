<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CKAttendance.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.CKAttendance" %>
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
                <div class="col-md-3">
                    <Rock:DatePicker ID="stDate" runat="server" Label="Start Date" Required="true"  />
                </div>
                <div class="col-md-3">
                    <Rock:DatePicker ID="endDate" runat="server" Label="End Date" Required="true"  />
                </div>
                <div class="col-md-6">
                    <asp:Panel ID="pnlSchedules" runat="server" CssClass="form-group">
                        <label class="control-label">Service Times(s)</label> &nbsp; <asp:LinkButton ID="lbRefreshSchedules" runat="server" OnClick="sdrpDates_SelectedDateRangeChanged" CausesValidation="false"><i class="fa fa-sm fa-sync"></i></asp:LinkButton>
                        <div class="control-wrapper">
							<asp:Literal ID="lSchedules" runat="server" />
							<Rock:RockListBox ID="lbSchedules" runat="server" SelectionMode="Multiple" Visible="true" RequiredErrorMessage="Service Time(s) is Required"
								DataValueField="Id" DataTextField="Name" cssClass="input-width-md" />
                        </div>
                    </asp:Panel>
                </div>
            </div>
            <div class="row">
                <div class="pull-right">
                    <Rock:BootstrapButton ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClientClick="notes();" OnClick="btnFilter_Click" />
                </div>
            </div>
        </div>
        <br />
        <div class="custom-container" id="DataContainer" runat="server">
            <asp:PlaceHolder ID="phContent" runat="server" Visible="false" />
        </div>
        <div class="row">
            <div class="pull-right">
                <Rock:BootstrapButton ID="btnExport" runat="server" Text="Export" CssClass="btn btn-primary" OnClick="btnExport_Click" />
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<style>
    .custom-seperator {
        width: fit-content;
        border-top: 1px solid #E0E0E0;
        padding-top: 16px;
        margin-top: 16px;
    }
    .custom-container {
        overflow-x: scroll;
        overflow-y: hidden;
    }
    .custom-row {
        display: flex;
        flex-wrap: nowrap;
        width: fit-content;
    }
    div.custom-row:not(.service-time){
        padding-left:8px;
    }
    .custom-col {
        min-width: 150px;
        text-align: center;
    }
    .name-col {
        text-align: left;
        position: absolute;
        background-color: #FAFAFA;
    }
    .service-time {
        font-weight: bold;
        height: 18px; 
    }
    .bg-secondary {
        background-color: #F1F1F1;
    }
    .over-threshold {
        background-color: #9a0000;
        color: white;
    }
    .first-custom-col {
        margin-left: 150px;
    }
</style>
