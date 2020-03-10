<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ProtectMyMinistryV2Settings.ascx.cs" Inherits="RockWeb.Plugins.com_protectmyministry.BackgroundCheck.ProtectMyMinistryV2Settings" %>
<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlRestKeys" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-shield"></i> Protect My Ministry 2.0</h1>
                <div class="pull-right">
                    <asp:LinkButton ID="btnDefault" runat="server" CssClass="btn btn-default btn-xs" 
						OnClick="btnDefault_Click">Enable As Default Background Check Provider</asp:LinkButton>
                </div>

            </div>

            <div class="panel-body">

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbNotification" runat="server" Title="Please correct the following:" NotificationBoxType="Danger" Visible="false" />

                <div id="pnlNew" runat="server" class="row">
                    <div class="col-md-6 text-center">
                        <asp:Image ID="imgPromotion" runat="server" CssClass="img-responsive" />
                        <div class="actions margin-t-lg">
                            <asp:HyperLink ID="hlClient" runat="server" Text="Get started using Protect My Ministry" CssClass="btn btn-primary btn-block margin-b-lg" />
                        </div>
                    </div>
                    <div class="col-md-1">
                    </div>
                    <div class="col-md-5">
                        <Rock:RockTextBox ID="tbUserNameNew" runat="server" Label="Username" Required="true" />
                        <Rock:RockTextBox ID="tbPasswordNew" runat="server" Label="Password" Required="true" TextMode="Password" />
                        <div class="actions">
                            <asp:LinkButton ID="lbSaveNew" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSaveNew_Click" />
                        </div>
                    </div>
                </div>

                <div id="pnlViewDetails" runat="server" visible="false">

                    <Rock:RockLiteral ID="lPackages" runat="server" Label="Enabled Background Check Types" />

                    <Rock:NotificationBox ID="nbSSLWarning" runat="server" CssClass="clearfix" NotificationBoxType="Danger">
                        <i class="fa fa-2x fa-exclamation-triangle pull-left margin-v-sm margin-r-md"></i>
                        Your current configuration will not allow Protect My Ministry to return results to your server. You must 
                        ensure your server is configured for SSL and use an <code>https://URL</code> to protect the data during 
                        transmission. Please  send this URL to the Protect My Ministry team prior to placing your first order 
                        so they can import your security certificate. 
                    </Rock:NotificationBox>

                    <Rock:NotificationBox ID="nbPackageListError" runat="server" NotificationBoxType="Danger" Visible="false"/>

                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbEdit_Click" />
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server" visible="false">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:UrlLinkBox ID="urlWebHook" runat="server" Label="Result Webhook" Required="true"
                                Help="The URL that Protect My Ministry should use when sending background check results back to your server." />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox id="cbActive" runat="server" Label="Active" />
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>
                </div>

            </div>

        </div>

        <asp:Panel ID="pnlTabs" runat="server" Visible="false">

            <ul class="nav nav-pills margin-b-md">
                <li id="liPackages" runat="server" class="active">
                    <asp:LinkButton ID="lbPackages" runat="server" Text="Background Check Types" OnClick="lbTab_Click" />
                </li>
                <li id="liUsers" runat="server">
                    <asp:LinkButton ID="lbUsers" runat="server" Text="User Accounts" OnClick="lbTab_Click" />
                </li>
            </ul>

            <asp:Panel ID="pnlPackages" runat="server" CssClass="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-archive"></i> Background Check Types</h1>
                </div>
                <div class="panel-body">
                    <div class="alert alert-info">
                        Below are the background Check types that have been configured for this account at Protect My Ministry. For each type, select the person attributes that should be updated
                        when a request of that type is completed. 
                    </div>
                    <Rock:ModalAlert ID="mdGridTypesWarningValues" runat="server" />
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gTypes" runat="server" AllowPaging="true" DisplayType="Full" RowItemText="Type" OnRowSelected="gTypes_RowSelected" AllowSorting="False" >
                            <Columns>
                                <Rock:ReorderField/>
                                <Rock:RockBoundField DataField="Value" HeaderText="Name"/>
                                <Rock:RockBoundField DataField="Description" HeaderText="Included Packages"/>
                                <Rock:RockBoundField DataField="PersonAttributes" HeaderText="Person Attributes" />
                                <Rock:BoolField DataField="IsActive" HeaderText="Active"/>
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlUsers" runat="server" Visible="false" CssClass="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-users"></i> User Accounts</h1>
                </div>
                <div class="panel-body">
                    <div class="alert alert-info">
                        When submitting background checks to Protect My Ministry the user account that the request is submitted under can either be the Admin username above or any additional user listed 
                        here. These usernames and passwords will be provided by Protect My Ministry. 
                    </div>
                    <Rock:ModalAlert ID="mdGridUsersWarningValues" runat="server" />
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gUsers" runat="server" AllowPaging="true" DisplayType="Full" RowItemText="Additional User" OnRowSelected="gUsers_RowSelected" AllowSorting="False" >
                            <Columns>
                                <Rock:RockBoundField DataField="Value" HeaderText="User"/>
                                <Rock:RockBoundField DataField="Username" HeaderText="Username"/>
                                <Rock:BoolField DataField="IsActive" HeaderText="Active"/>
                                <Rock:DeleteField OnClick="gUsers_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </asp:Panel>
            
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgPackage" runat="server" Title="Background Check Type" ValidationGroup="Package" 
            OnSaveClick="dlgPackage_SaveClick" OnCancelScript="clearActiveDialog();">
            <Content>

                <asp:HiddenField ID="hlPackageDefinedValueId" runat="server" />
                <asp:ValidationSummary ID="valSummaryPackage" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Package" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lPackageDescription" runat="server" Label="Included Packages"  />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbPackageIsActive" runat="server" Label="Active" ValidationGroup="Package" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlDateAttribute" runat="server" Label="Date Attribute" Help="The person attribute to update with date that this type of background check was completed." DataTextField="Text" DataValueField="Value" ValidationGroup="Package" Required="true" />
                        <Rock:RockDropDownList ID="ddlResultAttribute" runat="server" Label="Result Attribute" Help="The person attribute to update with the result of background check (pass/fail)." DataTextField="Text" DataValueField="Value" ValidationGroup="Package" Required="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlDocumentAttribute" runat="server" Label="Document Attribute" Help="The person attribute to update with result of background check." DataTextField="Text" DataValueField="Value" ValidationGroup="Package" Required="true" />
                        <Rock:RockDropDownList ID="ddlCheckedAttribute" runat="server" Label="Checked Attribute" Help="The person attribute to update with an indication that background check was completed." DataTextField="Text" DataValueField="Value" ValidationGroup="Package" Required="true" />
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgUser" runat="server" Title="User Account" ValidationGroup="User" 
            OnSaveClick="dlgUser_SaveClick" OnCancelScript="clearActiveDialog();">
            <Content>

                <asp:HiddenField ID="hlUserDefinedValueId" runat="server" />
                <asp:ValidationSummary ID="ValidationSummaryUser" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="User" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbUserTitle" runat="server" Label="Title" Required="true" ValidationGroup="User" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbUserIsActive" runat="server" Label="Active" ValidationGroup="User" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockTextBox ID="tbUserDescription" runat="server" Label="Description" ValidationGroup="User" TextMode="MultiLine" Rows="2" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbUserUsername" runat="server" Label="Username" Required="true" ValidationGroup="User" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbUserPassword" runat="server" Label="Password" Required="true" ValidationGroup="User" TextMode="Password" />
                    </div>
                </div>

            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>