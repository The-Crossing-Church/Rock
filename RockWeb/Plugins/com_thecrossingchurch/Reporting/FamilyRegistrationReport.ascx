<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyRegistrationReport.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Reporting.FamilyRegistrationReport" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-default">
            <div class="panel-heading">
                <h4 class="panel-title"><i class="fa fa-filter"></i> Filters</h4>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col col-xs-12">
                        <Rock:DataViewItemPicker ID="pkrDataView" runat="server" Label="Source" />
                    </div>
                </div>
                <div class="row">
                    <div class="col col-xs-12">
                        <Rock:RegistrationInstancePicker ID="pkrEvent" runat="server" Label="Event" />
                    </div>
                </div>
                <div class="row">
                    <div class="col col-xs-12">
                        <Rock:RockCheckbox ID="ckbxFamily" runat="server" Label="Filter By Family" />
                    </div>
                </div>
                <div class="row">
                    <div class="col col-xs-12">
                        <div class="pull-right">
                            <Rock:BootstrapButton ID="btnClear" runat="server" CssClass="btn btn-default" Text="Reset Filters" OnClick="btnClear_Click" />
                            <Rock:BootstrapButton ID="btnFilter" runat="server" CssClass="btn btn-primary" Text="Filter" OnClick="btnFilter_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="grid ">
            <Rock:GridFilter ID="grdFilReport" runat="server" >

            </Rock:GridFilter>
            <Rock:Grid ID="grdReport" runat="server" AllowSorting="true" AllowPaging="True" PersonIdField="Id" EmptyDataText="No Results" ExportSource="ColumnOutput" ExportFilename="RegistrationReport">
                <Columns>
                    <Rock:SelectField ItemStyle-Width="48px" />
                    <Rock:RockBoundField DataField="Id" Visible="false" ExcelExportBehavior="NeverInclude" />
                    <Rock:RockTemplateField HeaderText="Name" SortExpression="LastName, NickName" ExcelExportBehavior="NeverInclude">
                        <ItemTemplate>
                            <asp:Label ID="lblLastName" runat="server"
                                Text='<%# Bind("LastName") %>'></asp:Label>, 
                            <asp:Label ID="lblNickName" runat="server"
                                Text='<%# Bind("NickName") %>'></asp:Label>
                        </ItemTemplate>
                    </Rock:RockTemplateField>
                    <Rock:RockBoundField HeaderText="First Name" DataField="NickName" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                    <Rock:RockBoundField HeaderText="Last Name" DataField="LastName" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                    <Rock:RockBoundField HeaderText="Email" DataField="Email" SortExpression="Email" ExcelExportBehavior="AlwaysInclude" />
                </Columns>
            </Rock:Grid>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<script>

</script>
<style>

</style>
