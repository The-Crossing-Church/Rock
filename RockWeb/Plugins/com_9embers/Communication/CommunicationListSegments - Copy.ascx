gPreview<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationListSegments - Copy.ascx.cs" Inherits="RockWeb.Plugins.com_9embers.Communication.CommunicationListSegments" %>
<style>
    .cust-label {
        margin-top: 30px;
    }
</style>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>        

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Create New Communication
                </h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-8 col-sm-6">
                        <Rock:RockDropDownList runat="server" ID="ddlCommunicationList" Label="Communication List"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlCommunicationList_SelectedIndexChanged"
                            DataTextField="Name" DataValueField="Id" />
                    </div>
                </div>

                <!-- This should just be a normal list eventually-->
                <Rock:RockCheckBoxList runat="server" ID="cblSegments" Label="Communication Segment Totals" Visible="false"
                    DataTextField="Name" DataValueField="Id" />

                <!-- ACTIVITY SUMMARY -->   
                <asp:Panel runat="server" ID="pnlSummary" Label="Activity Summary" Visible="false">
                    <br />
                    <div class="row">
                        <asp:Panel ID="pnlDate" runat="server">
                            <!-- Date Range should go here -->
                            <Rock:DateRangePicker runat="server" ID="drpSummaryDate" Label="Date Range: " />
                        </asp:Panel>
                    </div>
                </asp:Panel>

                <!-- Segment Movements and Totals-->
                <Rock:RockCheckBoxList runat="server" ID="cblSegmentSummary" Label="Segment Movements" Visible="false"
                    DataTextField="Name" DataValueField ="Id" />

               <!-- report grid -->
                <asp:Panel runat="server" ID="pnlGrid" Visible="false">
                    <content>
                    <!-- Report should go here -->
                    <Rock:Grid runat="server" ID="gMovement" ExportSource="ColumnOutput" OnRowDataBound="gMovement_RowDataBound">
                    <Columns>
                        <Rock:RockBoundField HeaderText="Person" DataField="FullName" />
                        <Rock:RockBoundField HeaderText="Nick Name" DataField="NickName" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                        <Rock:RockBoundField HeaderText="Last Name" DataField="LastName" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                        <Rock:RockBoundField HeaderText="Email" DataField="Email" />
                        <Rock:RockLiteralField ID="lCellPhone" HeaderText="Cell Phone"  />
                    </Columns>
                    </Rock:Grid>
                    </content>
                </asp:Panel>


                <%--<asp:Panel runat="server" ID="pnlRegistration" Visible="false">
                    <br />
                    <div class="row">
                        <asp:Panel ID="pnlIncludeExclude" runat="server" >
                            <Rock:RockDropDownList runat="server" ID="ddlIncludeExclude" Label="Registration">
                                <asp:ListItem Text="Has NOT registered for" Value="-1" />
                                <asp:ListItem Text="Has registered for" Value="1" />
                            </Rock:RockDropDownList>
                        </asp:Panel>
                        <asp:Panel ID="pnlRegistrationTemplates" runat="server" Visible="false" >
                            <Rock:RegistrationTemplatePicker ID="rpRegistrationTemplates" runat="server" Label="Template" AllowMultiSelect="false" OnSelectItem="rpRegistrationTemplates_SelectItem" />
                        </asp:Panel>
                        <asp:Panel ID="pnlRegistrationInstances" runat="server" >
                            <Rock:RockListBox runat="server" ID="cblRegistrationInstances" Label="Instance" DataValueField="Id" DataTextField="Name" />
                        </asp:Panel>
                        <asp:Panel ID="pnlIncludeInactive" runat="server" >
                            <Rock:RockCheckBox runat="server" ID="cbIncludeInactive" Label="Include Inactive" OnCheckedChanged="cbIncludeInactive_CheckedChanged" AutoPostBack="true" />
                        </asp:Panel>
                    </div>
                </asp:Panel>--%>

                <%--<asp:Panel runat="server" ID="pnlPrevCommunication" CssClass="row margin-t-md" Visible="false">
                    <div class="col-md-12">
                        <Rock:RockDropDownList runat="server" ID="ddlPrevCommunication" Label="Copy Communication Details From" DataTextField="Name" DataValueField="Id" ></Rock:RockDropDownList>
                   </div>
                </asp:Panel>--%>

                <%--<br />--%>

                <%--<Rock:BootstrapButton runat="server" ID="btnPreview" CssClass="btn btn-default" Text="Preview" OnClick="btnPreview_Click" Visible="false" />
                <Rock:BootstrapButton runat="server" ID="btnGenerate" CssClass="btn btn-primary" Text="Generate Communication" OnClick="btnGenerate_Click" Visible="false" />--%>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>