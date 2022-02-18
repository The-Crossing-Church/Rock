<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingTxnMatching.ascx.cs" Inherits="RockWeb.Plugins.rocks_pillars.Finance.FundraisingTxnMatching" %>
<style>
    .padding-group-member {
        padding: 12px !important;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-check-square-o"></i>
                    Assign Transactions
                </h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlFilter" runat="server">
                    <div class="row">
                        <div class="col-md-3">
                            <Rock:RockDropDownList ID="ddlAccount" runat="server" Label="Account"  />
                        </div>
                        <div class="col-md-4">
                            <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockDropDownList ID="ddlAssigned" runat="server" Label="Assigned" >
                                <asp:ListItem Text="All" Value=""></asp:ListItem>
                                <asp:ListItem Text="Only Unassiged" Value="1"></asp:ListItem>
                                <asp:ListItem Text="Only Assigned" Value="2"></asp:ListItem>
                            </Rock:RockDropDownList>
                        </div>
                        <div class="col-md-2">
                            <Rock:RockCheckBox ID="cbClosedBatches" runat="server" Label="Closed Batches" Text="Yes" 
                                Help="Should transactions from closed batches be included in the list?" />
                        </div>
                    </div>
                    <div class="actions margin-b-md">
                        <asp:LinkButton ID="lbFilter" runat="server" Text="Filter" CssClass="btn btn-sm btn-default pull-right" OnClick="lbFilter_Click" />
                    </div>
                </asp:Panel>

                <div class="grid grid-panel">

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                    <Rock:Grid ID="gTransactions" runat="server" EmptyDataText="No Transactions Found" 
                        RowItemText="Transaction" AllowSorting="true"  >
                        <Columns>
                            <Rock:RockBoundField DataField="PersonName" HeaderText="Person" SortExpression="PersonName" />
                            <Rock:RockBoundField DataField="SoftCreditFrom" HeaderText="Soft Credit From" SortExpression="SoftCreditFrom" />
                            <Rock:DateField DataField="TransactionDateTime" HeaderText="Date" SortExpression="TransactionDateTime" />                
                            <Rock:RockBoundField DataField="BatchName" HeaderText="Batch" SortExpression="BatchName" />                
                            <Rock:RockBoundField DataField="Account" HeaderText="Account" SortExpression="Account" />                
                            <Rock:CurrencyField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
                            <Rock:RockTemplateFieldUnselected HeaderText="Group Member" ItemStyle-CssClass="padding-group-member">
                                <ItemTemplate>
                                    <Rock:RockDropDownList ID="ddlGroupMember" runat="server" />
                                </ItemTemplate>
                            </Rock:RockTemplateFieldUnselected>
                            <Rock:RockTemplateFieldUnselected ItemStyle-CssClass="badge-legend">
                                <ItemTemplate>
                                    <span 
                                        class='badge badge-<%# (bool)Eval("Assigned") ? "success" : "warning" %> badge-circle' 
                                        data-toggle="tooltip" 
                                        data-original-title='<%# (bool)Eval("Assigned") ? "Assigned" : "Not Assigned" %>'
                                    ><span class="sr-only"><%# (bool)Eval("Assigned") ? "Assigned" : "Not Assigned" %></span></span>
                                </ItemTemplate>
                            </Rock:RockTemplateFieldUnselected>
                        </Columns>
                    </Rock:Grid>

                    <div class="margin-all-md">
                        <Rock:NotificationBox ID="nbSaveSuccess" runat="server" NotificationBoxType="Success" Text="Changes Saved" Visible="false" />
                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" Visible="false" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        </div>
                    </div>

                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
