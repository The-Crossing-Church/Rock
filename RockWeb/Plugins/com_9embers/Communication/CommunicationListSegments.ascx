<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationListSegments.ascx.cs" Inherits="RockWeb.Plugins.com_9embers.Communication.CommunicationListSegments" %>
<style>
    .cust-label {
        margin-top: 30px;
    }
</style>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalDialog runat="server" ID="mdPreview" Title="Preview">
            <Content>
                <Rock:Grid runat="server" ID="gPreview">
                    <Columns>
                        <Rock:RockBoundField HeaderText="Person" DataField="FullName" />
                        <Rock:RockBoundField HeaderText="Email" DataField="Email" />
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
                            Help="Select if you want to send the email to only the members of the communication list, their parents, or both. Parents are not filtered through the parameters below, but will only be added when their child passes the filters."
                            ID="ddlSendTo" Label="Send To" Visible="false">
                            <asp:ListItem Text="List Members" Value="0" />
                            <asp:ListItem Text="Parents" Value="1" />
                            <asp:ListItem Text="Members and Parents" Value="2" />
                        </Rock:RockDropDownList>
                    </div>
                </div>
                <Rock:RockCheckBoxList runat="server" ID="cblSegments" Label="Communication Segments" Visible="false"
                    DataTextField="Name" DataValueField="Id" />
                <Rock:DynamicControlsPanel runat="server" ID="dcpContainer" />
                <asp:Panel runat="server" ID="pnlRegistration" Visible="false">
                    <br />
                    <div class="row">
                        <div class="col-sm-4 col-md-4 col-lg-3">
                            <Rock:RockDropDownList runat="server" ID="ddlIncludeExclude" Label="Registration">
                                <asp:ListItem Text="Has NOT registered for" Value="-1" />
                                <asp:ListItem Text="Has registered for" Value="1" />
                            </Rock:RockDropDownList>
                        </div>
                        <div class="col-sm-5 col-md-6 col-lg-7">
                            <Rock:RockListBox runat="server" ID="cblRegistrationInstances" Label="Instance" DataValueField="Id" DataTextField="Name" />
                        </div>
                        <div class="col-sm-3 col-md-2 col-lg-2">
                            <Rock:RockCheckBox runat="server" ID="cbIncludeInactive" Label="Include Inactive" OnCheckedChanged="cbIncludeInactive_CheckedChanged" AutoPostBack="true" />
                        </div>
                    </div>
                </asp:Panel>
                <br />

                <Rock:BootstrapButton runat="server" ID="btnPreview" CssClass="btn btn-default" Text="Preview" OnClick="btnPreview_Click" Visible="false" />
                <Rock:BootstrapButton runat="server" ID="btnGenerate" CssClass="btn btn-primary" Text="Generate Communication" OnClick="btnGenerate_Click" Visible="false" />
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>