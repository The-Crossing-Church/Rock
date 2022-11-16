<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationListSegments.ascx.cs" Inherits="RockWeb.Plugins.com_9embers.Communication.CommunicationListSegments" %>
<style>
    .cust-label{
        margin-top:30px;
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
                <Rock:RockDropDownList runat="server" ID="ddlCommunicationList" Label="Communication List"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlCommunicationList_SelectedIndexChanged"
                    DataTextField="Name" DataValueField="Id" />
                <Rock:RockCheckBoxList runat="server" ID="cblSegments" Label="Communication Segments" Visible="false"
                    DataTextField="Name" DataValueField="Id" />
                <Rock:DynamicControlsPanel runat="server" ID="dcpContainer" />
                <asp:Panel runat="server" ID="pnlRegistration" Visible="false">
                    <br />
                    <label class="control-label">Registration</label>
                    <div class="row">
                        <div class="col-sm-4 col-md-4 col-lg-3">
                            <Rock:RockDropDownList runat="server" ID="ddlIncludeExclude">
                                <asp:ListItem Text="Has NOT registered for" Value="-1" />
                                <asp:ListItem Text="Has registered for" Value="1" />
                            </Rock:RockDropDownList>
                        </div>
                        <div class="col-sm-8 col-md-8 col-lg-9">
                            <Rock:RockListBox runat="server" ID="cblRegistrationInstances" DataValueField="Id" DataTextField="Name" />
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
