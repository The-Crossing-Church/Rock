<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ComparativeGivingReport.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.ComparativeGivingReport" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-default">
            <div class="panel-heading"><h4 class="panel-title"><i class="fa fa-filter"></i>&nbsp;Filters</h4></div>
            <div class="panel-body">
                <div class="row">
                    <div class="col col-xs-12 col-md-4">
                        <Rock:DatePicker ID="pkrStart" Label="Start Date" runat="server" />
                    </div>
                    <div class="col col-xs-12 col-md-4">
                        <Rock:DatePicker ID="pkrEnd" Label="End Date" runat="server" />
                    </div>
                    <div class="col col-xs-12 col-md-4">
                        <Rock:AccountPicker ID="pkrAcct" Label="Fund" runat="server" />
                    </div>
                </div>
                <div class="row">
                    <div class="col col-xs-12">
                        <Rock:BootstrapButton ID="btnExport" Text="Export to Excel" runat="server" CssClass="btn btn-primary pull-right" OnClick="btnExport_Click" />
                        <Rock:BootstrapButton ID="btnGenerate" Text="Generate Grid" runat="server" CssClass="btn btn-primary pull-right" OnClick="btnGenerate_Click" />
                    </div>
                </div>
            </div>
        </div>
        <div class="alert alert-info" runat="server" id="alertInfo" visible="false"></div>
        <Rock:Grid ID="grdGiving" runat="server" AllowSorting="true">
            <Columns>
                <Rock:RockBoundField HeaderText="Household" DataField="HouseholdName" SortExpression="HouseholdName" ExcelExportBehavior="AlwaysInclude" />
                <Rock:RockBoundField HeaderText="Address" DataField="Address" SortExpression="Address" ExcelExportBehavior="AlwaysInclude" />
                <Rock:RockTemplateField HeaderText="Amount Given" SortExpression="AmountGiven" ExcelExportBehavior="NeverInclude">
                    <ItemTemplate>
                        $<asp:Label ID="lblAmtGiven" runat="server"
                            Text='<%# Bind("AmountGiven") %>'></asp:Label>
                    </ItemTemplate>
                </Rock:RockTemplateField>
                <Rock:RockBoundField HeaderText="Amount Given" DataField="AmountGiven" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                <Rock:RockBoundField HeaderText="Number of Gifts" DataField="NumberOfGifts" SortExpression="NumberOfGifts" ExcelExportBehavior="AlwaysInclude" />
                <Rock:RockTemplateField HeaderText="Average Gift Amount" SortExpression="AverageGiftAmount" ExcelExportBehavior="NeverInclude">
                    <ItemTemplate>
                        $<asp:Label ID="lblAvgGift" runat="server"
                            Text='<%# Bind("AverageGiftAmount") %>'></asp:Label>
                    </ItemTemplate>
                </Rock:RockTemplateField>
                <Rock:RockBoundField HeaderText="Average Gift Amount" DataField="AverageGiftAmount" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                <Rock:RockTemplateField HeaderText="Previous Amount Given" SortExpression="PreviousAmountGiven" ExcelExportBehavior="NeverInclude">
                    <ItemTemplate>
                        $<asp:Label ID="lblPreAmtGiven" runat="server"
                            Text='<%# Bind("PreviousAmountGiven") %>'></asp:Label>
                    </ItemTemplate>
                </Rock:RockTemplateField>
                <Rock:RockBoundField HeaderText="Previous Amount Given" DataField="PreviousAmountGiven" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                <Rock:RockBoundField HeaderText="Previous Number of Gifts" DataField="PreviousNumberOfGifts" SortExpression="PreviousNumberOfGifts" ExcelExportBehavior="AlwaysInclude" />
                <Rock:RockTemplateField HeaderText="Previous Average Gift Amount" SortExpression="PreviousAverageGiftAmount" ExcelExportBehavior="NeverInclude">
                    <ItemTemplate>
                        $<asp:Label ID="lblPreAvgGift" runat="server"
                            Text='<%# Bind("PreviousAverageGiftAmount") %>'></asp:Label>
                    </ItemTemplate>
                </Rock:RockTemplateField>
                <Rock:RockBoundField HeaderText="Previous Average Gift Amount" DataField="PreviousAverageGiftAmount" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                <Rock:RockTemplateField HeaderText="Change" SortExpression="AmountChange" ExcelExportBehavior="NeverInclude">
                    <ItemTemplate>
                        <asp:Label ID="lblChg" runat="server"
                            Text='<%# Bind("AmountChange") %>' class="change-amount"></asp:Label>
                    </ItemTemplate>
                </Rock:RockTemplateField>
                <Rock:RockBoundField HeaderText="Change" DataField="AmountChange" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                <Rock:DateField HeaderText="First Gift" DataField="FirstGiftEver" SortExpression="FirstGiftEver" ExcelExportBehavior="AlwaysInclude" />
                <Rock:DateField HeaderText="Most Recent Gift" DataField="MostRecentGift" SortExpression="MostRecentGift" ExcelExportBehavior="AlwaysInclude" />
                <Rock:RockBoundField HeaderText="Most Recent Fund" DataField="MostRecentFund" SortExpression="MostRecentFund" ExcelExportBehavior="AlwaysInclude" />
                <Rock:RockBoundField HeaderText="Most Recent Fund Gift Amount" DataField="MostRecentFundAmount" SortExpression="MostRecentFundAmount" ExcelExportBehavior="AlwaysInclude" />
                <Rock:RockBoundField HeaderText="Giving Zone" DataField="GivingZone" SortExpression="GivingZone" ExcelExportBehavior="AlwaysInclude" />
                <Rock:RockBoundField HeaderText="Type of Donor" DataField="DonorType" SortExpression="DonorType" ExcelExportBehavior="AlwaysInclude" />
                <%--<Rock:RockBoundField HeaderText="Type of Giver" DataField="GiverType" SortExpression="GiverType" ExcelExportBehavior="AlwaysInclude" />--%>
                <Rock:RockBoundField HeaderText="Average Days Between Gifts" DataField="Average" SortExpression="Average" ExcelExportBehavior="AlwaysInclude" />
                <Rock:RockBoundField HeaderText="Standard Deviation From Average" DataField="StdDev" SortExpression="StdDev" ExcelExportBehavior="AlwaysInclude" />
                <Rock:RockBoundField HeaderText="Source" DataField="Source" SortExpression="Source" ExcelExportBehavior="AlwaysInclude" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
<script>
    $(document).ready(function () {
        colorizeChange();
    })
    function colorizeChange() {
        $('.change-amount:contains("-")').addClass('negative-change');
    }
</script>
<style>
    .change-amount {
        color: #57cc57;
    }

    .negative-change {
        color: red;
    }

    .change-amount::before {
        content: '$'
    }
</style>
