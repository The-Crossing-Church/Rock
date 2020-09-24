<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ComparativeGivingReport.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.ComparativeGivingReport" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:Grid ID="grdGiving" runat="server" AllowSorting="true">
            <Columns>
                <Rock:RockTemplateField HeaderText="Donor" SortExpression="Donor.LastName, Donor.NickName" ExcelExportBehavior="NeverInclude">
                    <ItemTemplate>
                        <asp:Label ID="lblLastName" runat="server"
                            Text='<%# Bind("Donor.LastName") %>'></asp:Label>, 
                        <asp:Label ID="lblNickName" runat="server"
                            Text='<%# Bind("Donor.NickName") %>'></asp:Label>
                    </ItemTemplate>
                </Rock:RockTemplateField>
                <Rock:RockBoundField HeaderText="First Name" DataField="Donor.NickName" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                <Rock:RockBoundField HeaderText="Last Name" DataField="Donor.LastName" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                <Rock:RockTemplateField HeaderText="Amount Given" SortExpression="AmountGiven" ExcelExportBehavior="AlwaysInclude">
                    <ItemTemplate>
                        $<asp:Label ID="lblAmtGiven" runat="server"
                            Text='<%# Bind("AmountGiven") %>'></asp:Label>
                    </ItemTemplate>
                </Rock:RockTemplateField>
                <Rock:RockBoundField HeaderText="Number of Gifts" DataField="NumberOfGifts" SortExpression="NumberOfGifts" ExcelExportBehavior="AlwaysInclude" />
                <Rock:RockTemplateField HeaderText="Average Gift Amount" SortExpression="AverageGiftAmount" ExcelExportBehavior="AlwaysInclude">
                    <ItemTemplate>
                        $<asp:Label ID="lblAvgGift" runat="server"
                            Text='<%# Bind("AverageGiftAmount") %>'></asp:Label>
                    </ItemTemplate>
                </Rock:RockTemplateField>
                <Rock:RockBoundField HeaderText="Previous Number of Gifts" DataField="PreviousNumberOfGifts" SortExpression="PreviousNumberOfGifts" ExcelExportBehavior="AlwaysInclude" />
                <Rock:RockTemplateField HeaderText="Previous Average Gift Amount" SortExpression="PreviousAverageGiftAmount" ExcelExportBehavior="AlwaysInclude">
                    <ItemTemplate>
                        $<asp:Label ID="lblPreAvgGift" runat="server"
                            Text='<%# Bind("PreviousAverageGiftAmount") %>'></asp:Label>
                    </ItemTemplate>
                </Rock:RockTemplateField>
                <Rock:RockTemplateField HeaderText="Change" SortExpression="AmountChange" ExcelExportBehavior="AlwaysInclude">
                    <ItemTemplate>
                        <asp:Label ID="lblChg" runat="server"
                            Text='<%# Bind("AmountChange") %>' class="change-amount"></asp:Label>
                    </ItemTemplate>
                </Rock:RockTemplateField>
                <Rock:DateField HeaderText="First Gift" DataField="FirstGiftEver" SortExpression="FirstGiftEver" ExcelExportBehavior="AlwaysInclude" />
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
