gPreview<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationListSegments.ascx.cs" Inherits="RockWeb.Plugins.com_9embers.Communication.CommunicationListSegments" %>
<style>
    .cust-label {
        margin-top: 30px;
    }
</style>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalDialog runat="server" ID="mdPreview" Title="Preview">
            <Content>
                <Rock:Grid runat="server" ID="gPreview" ExportSource="ColumnOutput" OnRowDataBound="gPreview_RowDataBound">
                    <Columns>
                        <Rock:RockBoundField HeaderText="Person" DataField="FullName" />
                        <Rock:RockBoundField HeaderText="Nick Name" DataField="NickName" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                        <Rock:RockBoundField HeaderText="Last Name" DataField="LastName" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                        <Rock:RockBoundField HeaderText="Email" DataField="Email" />
                        <Rock:RockLiteralField ID="lCellPhone" HeaderText="Cell Phone"  />
                    </Columns>
                </Rock:Grid>
            </Content>
        </Rock:ModalDialog>

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
                    <div class="col-md-4 col-sm-6">
                        <Rock:RockDropDownList runat="server"
                            Help="Select if you want to send the communication to only the Student/Child of the communication list, their parents, or both. Parents are not filtered through the parameters below, but will ONLY be added when their child passes the filters. When a 'Student/Child' option is selected it will only pull the age appropriate minors for that particular ministry (ie Crossing Students Segments will only pull 6th - 12th graders and Crossing Kids will only pull infants - 5th graders)"
                            ID="ddlSendTo" Label="Send To" Visible="false">
                            <asp:ListItem Text="Student/Child" Value="0" />
                            <asp:ListItem Text="Parents" Value="1" />
                            <asp:ListItem Text="Student/Child and Parents" Value="2" />
                        </Rock:RockDropDownList>
                    </div>
                </div>
                <Rock:RockCheckBoxList runat="server" ID="cblSegments" Label="Communication Segments" Visible="false"
                    DataTextField="Name" DataValueField="Id" />
                <Rock:DynamicControlsPanel runat="server" ID="dcpContainer" />
                <asp:Panel runat="server" ID="pnlRegistration" Visible="false">
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
                </asp:Panel>

                <asp:Panel runat="server" ID="pnlPrevCommunication" CssClass="row margin-t-md" Visible="false">
                    <div class="col-md-12">
                        <Rock:RockDropDownList runat="server" ID="ddlPrevCommunication" Label="Copy Communication Details From" DataTextField="Name" DataValueField="Id" ></Rock:RockDropDownList>
                   </div>
                </asp:Panel>

                <br />

                <Rock:BootstrapButton runat="server" ID="btnPreview" CssClass="btn btn-default" Text="Preview" OnClick="btnPreview_Click" Visible="false" />
                <Rock:BootstrapButton runat="server" ID="btnGenerate" CssClass="btn btn-primary" Text="Generate Communication" OnClick="btnGenerate_Click" Visible="false" />
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>