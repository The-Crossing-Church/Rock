<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SimpleSMSEntry.ascx.cs" Inherits="RockWeb.Plugins.rocks_pillars.Communication.SimpleSMSEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarnings" runat="server" NotificationBoxType="Warning" Visible="false" />
        
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">    

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-comments"></i> 
                    Send SMS Message
                </h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlCompose" runat="server">

                    <asp:ValidationSummary ID="ValidationSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <div class="row">
                        <div class="col-md-6">
                            <div class="row">
                                <div class="col-sm-7">
                                    <Rock:PersonPicker ID="ppAddPerson" runat="server" Label="Add New Recipient" OnSelectPerson="ppAddPerson_SelectPerson" />
                                    <Rock:NotificationBox ID="nbInvalidPerson" runat="server" NotificationBoxType="Danger" Visible="false" Dismissable="true" />
                                </div>
                                <div class="col-sm-5">
                                    <Rock:RockControlWrapper ID="rcwRecipients" runat="server" Label="Selected Recipients">
                                        <ul class="list-unstyled">
                                            <asp:Repeater ID="rptRecipients" runat="server" OnItemCommand="rptRecipients_ItemCommand" >
                                                <ItemTemplate>
                                                    <li><%# Eval("PersonName") %> <asp:LinkButton ID="lbRemoveRecipient" runat="server" CommandArgument='<%# Eval("PersonId") %>' CausesValidation="false"><i class="fa fa-times"></i></asp:LinkButton></li>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </ul>
                                    </Rock:RockControlWrapper>
                                    <asp:CustomValidator ID="valRecipients" runat="server" OnServerValidate="valRecipients_ServerValidate" Display="None" ErrorMessage="At least one recipient is required." />
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbMessage" runat="server" Rows="5" Label="Message to Send" TextMode="MultiLine" Required="true" />
                        </div>
                    </div>

                    <div class="actions">
                        <Rock:BootstrapButton ID="btnNext" runat="server" CssClass="btn btn-primary pull-right" OnClick="btnNext_NextClick" Text="Next" />
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlConfirm" runat="server" Visible="false">

                    <Rock:NotificationBox ID="nbConfirm" runat="server" NotificationBoxType="Info" />

                    <div class="actions">
                        <Rock:BootstrapButton ID="btnBack" runat="server" CssClass="btn btn-default" OnClick="btnBack_BackClick" Text="Back" />
                        <Rock:BootstrapButton ID="btnSend" runat="server" CssClass="btn btn-primary pull-right" OnClick="btnSend_SendClick" Text="Send" />
                    </div>

                </asp:Panel>

                <Rock:NotificationBox ID="nbMessageSent" runat="server" NotificationBoxType="Success" Visible="false" />

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>