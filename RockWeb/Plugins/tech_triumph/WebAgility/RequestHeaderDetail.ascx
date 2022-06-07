<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RequestHeaderDetail.ascx.cs" Inherits="RockWeb.Plugins.tech_triumph.WebAgility.RequestHeaderDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server" >
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-sign-out-alt"></i> Request Header Rules</h1>
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

                <!-- Match Criteria -->
                <div class="well">
                    <h4>Match Criteria</h4>
                    <div class="row">
                        <div class="col-md-8">
                            <Rock:RockTextBox ID="tbSourceUrl" runat="server" Label="Source URL Pattern" Help="The incoming URL pattern to match on." Required="true" />
                        </div>
                        <div class="col-md-4">
                            <Rock:RockDropDownList ID="ddlSourceComparisonType" runat="server" Label="Source Comparison Type" />
                        </div>
                    </div>
                    <Rock:RockCheckBox ID="cbIsCaseSensitive" runat="server" Label="Case Sensitive Compare" Help="Should the comparison be case sensitive?" />
                </div>

                <div class="well">
                    <h4>Header Configuration</h4>

                    <!-- Header Configuration -->
                    <div class="row">
                        <div class="col-md-8">
                            <Rock:RockTextBox ID="tbHeaderName" runat="server" Label="Header Name" Help="The name portion of the header to include in the HTTP request." />
                        </div>
                        <div class="col-md-4">
                            <Rock:RockTextBox ID="tbHeaderValue" runat="server" Label="Header Value" Help="The value portion of the header to include in the HTTP request." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-8">
                            <Rock:RockCheckBox ID="cbOverwriteExistingValue" runat="server" Label="Overwrite Existing Value" Help="Should this rule overwrite an existing header value if it exists?" />
                        </div>
                        <div class="col-md-4">
                        </div>
                    </div>
                </div>

                <asp:LinkButton ID="lbSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="lbSave_Click" />
                <asp:LinkButton ID="lbCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbCancel_Click" />
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
