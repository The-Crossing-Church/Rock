<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RedirectorRuleDetail.ascx.cs" Inherits="RockWeb.Plugins.tech_triumph.WebAgility.RedirectorRuleDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server" >
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <script type="text/javascript">
                function pageLoad() {
                    var showAdvancedSettings = ($('#<%=hfRedirectorShowAdvancedSettings.ClientID %>').val() == 'true');
                    if (!showAdvancedSettings) {
                        $('.js-redirector-advanced-settings').hide();
                    }

                    $('.js-redirector-show-advanced-settings').off('click').on('click', function ()
                    {
                        var isVisible = !$('.js-redirector-advanced-settings').is(':visible');
                        $('#<%=hfRedirectorShowAdvancedSettings.ClientID %>').val(isVisible);
                        $('.js-redirector-show-advanced-settings').text(isVisible ? 'Hide Advanced Settings' : 'Show Advanced Settings');
                        $('.js-redirector-advanced-settings').slideToggle();
                        return false;
                    });
                }
            </script>
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-route"></i> Redirector Rules</h1>
            </div>
            <div class="panel-body">
                <asp:HiddenField ID="hfRuleId" runat="server" />
                
                <Rock:NotificationBox ID="nbWarnings" runat="server" NotificationBoxType="Warning" />

                <div class="row">
                    <div class="col-md-8">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Name" Required="true" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockCheckBox id="cbIsActive" runat="server" Label="Is Active" />
                    </div>
                </div>

                <!-- Source Options -->
                <div class="well">
                    <h4>Match Criteria</h4>
                    <div class="row">
                        <div class="col-md-8">
                            <Rock:RockTextBox ID="tbSourceUrl" runat="server" Label="Source URL Pattern" Help="The incoming URL to match on." Required="true" />
                        </div>
                        <div class="col-md-4">
                            <Rock:RockDropDownList ID="ddlSourceComparisonType" runat="server" Label="Source Comparison Type" AutoPostBack="true" OnSelectedIndexChanged="ddlSourceComparisonType_SelectedIndexChanged" />
                        </div>
                    </div>
                
                    <!-- Match Options -->
                    <div class="row">
                        <div class="col-md-4">
                            <Rock:RockDropDownList ID="ddlMatchComponents" runat="server" Label="Match Components" Help="What criteria should be used in determining a match." AutoPostBack="true" OnSelectedIndexChanged="ddlMatchCriteria_SelectedIndexChanged" />
                        </div>
                        <div class="col-md-8">
                            <Rock:RockRadioButtonList ID="rblMatchIsLoggedIn" runat="server" RepeatDirection="Horizontal" Label="Login Status">
                                <asp:ListItem Text="Is Logged In" Value="LoggedIn" Selected="True" />
                                <asp:ListItem Text="Is Not Logged In" Value="NotLoggedIn" />
                            </Rock:RockRadioButtonList>
                            <Rock:RockTextBox ID="tbReferrer" runat="server" Label="Referred Pattern" Help="If the source URL match and the request's referrer string contains the provided value it will be considered a match." />
                            <Rock:RockTextBox ID="tbUserAgent" runat="server" Label="User Agent Pattern" Help="If the source URL match and the request's user agent string contains the provided value it will be considered a match." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:RockCheckBox ID="cbIsCaseSensitive" runat="server" Label="Case Sensitive Compare" Help="Should the comparison be case sensitive?" />
                        </div>
                        <div class="col-md-8">
                            <div class="pull-right">
                                <asp:HiddenField ID="hfRedirectorShowAdvancedSettings" runat="server" Value="false" />
                                <a href="#" class="btn btn-xs btn-link js-redirector-show-advanced-settings">Show Advanced Settings</a>
                            </div>
                        </div>
                    </div>

                    <div class="row js-redirector-advanced-settings">
                        <div class="col-md-5">
                            <Rock:ListItems ID="liIpAddressFilters" runat="server" Label="IP Address Filters" ValuePrompt="Filter Expression" Help="Filter expression can be in one of the following formats: 192.168.10.0/24<br> 192.168.0.0/255.255.255.0<br> 192.168.10.10-20<br> 192.168.0.10 - 192.168.10.20<br> fe80::/10. " />
                        </div>
                    </div>

                    <div class="row js-redirector-advanced-settings">
                        <div class="col-md-5">
                            <Rock:RockTextBox ID="tbCookieKey" runat="server" Label="Cookie Key" Help="The key of the cookie to match on. A key is required for cookie filtering to be considered." />
                        </div>
                        <div class="col-md-2">
                            <Rock:RockDropDownList ID="ddlCookieMatchLogic" runat="server" Label="Match Logic" Help="The logic to use to filter with." />
                        </div>
                        <div class="col-md-5">
                            <Rock:RockTextBox ID="tbCookieValue" runat="server" Label="Cookie Value" Help="The optional value to use for the filter." />
                        </div>
                    </div>
                </div>

                <div class="well">
                    <h4>Action</h4>
                    <!-- Action Options -->
                    <div class="row">
                        <div class="col-md-4">
                            <Rock:RockDropDownList ID="ddlActionType" runat="server" Label="Action Type" Help="Determines what should be done when a match is found." AutoPostBack="true" OnSelectedIndexChanged="ddlActionType_SelectedIndexChanged" />
                        </div>
                        <div class="col-md-8">
                            <Rock:RockRadioButtonList ID="rdlErrorTypes" runat="server" Label="Error To Return" Help="The type of error to return." RepeatDirection="Horizontal" />
                            <Rock:RockRadioButtonList ID="rdlRedirectHttpCode" runat="server" Label="Redirect HTTP Code" Help="The HTTP code to use when redirecting." RepeatDirection="Horizontal" />
                        </div>
                    </div>

                    <!-- Target -->
                    <Rock:RockTextBox ID="tbTarget" runat="server" Label="Target URL Pattern" Help="The redirect target URL to use when a match is found." />
                
                    <asp:LinkButton ID="lbRegexTester" runat="server" Text="Regex Tester" CssClass="btn btn-xs btn-default" OnClick="lbRegexTester_Click" />
                </div>

                <asp:LinkButton ID="lbSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="lbSave_Click" />
                <asp:LinkButton ID="lbCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbCancel_Click" />
            </div>
        
        </asp:Panel>

        <Rock:ModalDialog ID="mdRegexTester" runat="server" Title="Regex Tester" SaveButtonText="Apply" OnSaveClick="mdRegexTester_SaveClick">
            <Content>
                <Rock:RockTextBox ID="tbTestUrl" runat="server" Label="Test Incoming URL" Help="Put a sample URL that you would like to test with." />

                <hr />

                <Rock:RockTextBox ID="tbSourceMatch" runat="server" Label="Source URL Pattern" Help="The source URL pattern for the rule." />
                <Rock:RockTextBox ID="tbTargetMatch" runat="server" Label="Target URL Pattern" Help="The target URL pattern for the rule." />

                <asp:LinkButton ID="lbTestRule" runat="server" CssClass="btn btn-xs btn-primary" Text="Test" OnClick="lbTestRule_Click" />
                
                <hr />
                <strong>Results</strong>
                <asp:Literal ID="lTestResults" runat="server" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
