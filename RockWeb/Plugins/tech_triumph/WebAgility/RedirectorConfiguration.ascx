<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RedirectorConfiguration.ascx.cs" Inherits="RockWeb.Plugins.tech_triumph.WebAgility.RedirectorConfiguration" %>

<asp:UpdatePanel ID="upnlContent" runat="server" >
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

             <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-route"></i> Redirector Configuration</h1>
            </div>
            <div class="panel-body">

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lExclusionList" runat="server" Label="Exclusion Extensions" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lDynamicRobotFile" runat="server" Label="Enable Dynamic Robot Files" />
                    </div>
                </div>
                


                
                <asp:LinkButton ID="lbEdit" runat="server" CssClass="btn btn-primary" Text="Edit" OnClick="lbEdit_Click" />
                <asp:LinkButton ID="lbTest" runat="server" CssClass="btn btn-link" Text="Test" OnClick="lbTest_Click" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlEdit" runat="server" CssClass="panel panel-block" Visible="false">

             <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-route"></i> Redirector Configuration</h1>
            </div>
            <div class="panel-body">

                <div class="row">
                    <div class="col-md-6">
                        <Rock:ValueList ID="vlExclusionExtensions" runat="server" Label="Exclusion Extensions" Help="Links with the following extentions (e.g. gif, jpg, css) will be ignored." />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbDynamicRobotFile" runat="server" Label="Enable Dynamic Robot Files" Help="Enable this setting to create dynamic robot.txt files for each site in Rock. This will ignore any robot.txt files in the root folder." />
                    </div>
                </div>

                
                
                <asp:LinkButton ID="lbSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="lbSave_Click" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlTest" runat="server" CssClass="panel panel-block" Visible="false">

             <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-route"></i> Link Tester</h1>
            </div>
            <div class="panel-body">

                <Rock:UrlLinkBox ID="urlTestLink" runat="server" Label="Link" Help="Add the link here to test." Required="true" />

                <Rock:RockLiteral ID="lTestResults" runat="server" Label="Results" Visible="false" />

                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockRadioButtonList ID="rblLoginStatus" runat="server" Label="Login Status" Help="Only used for rules that check for login status." RepeatDirection="Horizontal" />
                    </div>
                    <div class="col-md-4">
                        <Rock:UrlLinkBox ID="urlReferrer" runat="server" Label="Referrer" Help="Only required if you need to match a rule with a referrer." />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockTextBox ID="txtUserAgent" runat="server" Label="User Agent" Help="Only required if you need to match on User Agent." />
                    </div>
                </div>

                <asp:LinkButton ID="lbTestLink" runat="server" CssClass="btn btn-primary" Text="Test" OnClick="lbTestLink_Click" />
                <asp:LinkButton ID="lbCancelTest" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbCancelTest_Click" />
                
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
