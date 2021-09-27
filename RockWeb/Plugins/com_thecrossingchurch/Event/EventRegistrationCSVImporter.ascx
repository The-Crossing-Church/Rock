<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventRegistrationCSVImporter.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Event.EventRegistrationCSVImporter" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />
        <div class="container">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h5><i class="fa fa-cog"></i> Settings</h5>
                </div>
                <div class="panel-body">
                    <asp:Panel ID="pnlCSV" runat="server">
                        <div class="row">
                            <div class="col col-xs-12">
                                <h5>First, select a CSV to use</h5>
                                <p>The first row of your CSV needs to be a header column, the header values will be used to customize your import.</p>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-12 col-md-6">
                                <Rock:FileUploader ID="inputFile" Required="false" Label="Select Your CSV" runat="server" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-12">
                                <Rock:BootstrapButton ID="btnToRISelect" runat="server" CssClass="btn btn-primary pull-right" Text="Next" CommandArgument="1" OnClick="btnChangePnl_Click" />
                            </div>
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnlRI" runat="server" Visible="false">
                        <div class="row">
                            <div class="col col-xs-12">
                                <h5>Next, select the registration instance to add registrations to.</h5>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-12 col-md-6">
                                <Rock:RegistrationInstancePicker Required="true" ID="pkrRegistrationInstance" Label="Select Your Registration" runat="server" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-12">
                                <Rock:BootstrapButton ID="btnBackToCSVSelect" runat="server" CausesValidation="false" CssClass="btn btn-primary pull-left" Text="Back" CommandArgument="0" OnClick="btnChangePnl_Click" />
                                <Rock:BootstrapButton ID="btnToCSVSettings" runat="server" CssClass="btn btn-primary pull-right" Text="Next" CommandArgument="2" OnClick="btnChangePnl_Click" />
                            </div>
                        </div>
                    </asp:Panel>
                    <asp:Panel ID="pnlSettings" runat="server" Visible="false">
                        <div class="row">
                            <div class="col col-xs-12">
                                <h5>Let's define your CSV</h5>
                                <p>Here, you will use the values in your header row to correlate the data in that row to information in Rock.</p>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-12 col-md-6">
                                <Rock:RockTextBox ID="txtRegistrarFName" runat="server" Label="Which column contains the registrar's first name?" Help="The first name of the person filling out the registration." />
                            </div>
                            <div class="col col-xs-12 col-md-6">
                                <Rock:RockTextBox ID="txtRegistrarLName" runat="server" Label="Which column contains the registrar's last name?" Help="The last name of the person filling out the registration." />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-12 col-md-6">
                                <Rock:RockTextBox ID="txtRegistrarGrp" Required="true" runat="server" Label="Which column contains the registrar's email?" Help="The email of the person filling out the registration." />
                            </div>
                            <div class="col col-xs-12 col-md-6">
                                <Rock:RockTextBox ID="txtRegistrarPhone" runat="server" Label="Which column contains the registrar's mobile phone?" Help="The mobile phone number of the person filling out the registration." />
                            </div>
                            <div class="col col-xs-12 col-md-6">
                                <Rock:RockTextBox ID="txtRegistrarRockId" runat="server" Label="Which column contains the registrar's Rock ID?" Help="If your data does not have this information, leave this field blank." />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-12 col-md-6">
                                <Rock:RockTextBox ID="txtRegistrantFName" Required="true" runat="server" Label="Which column contains the registrant's first name?" Help="The first name of the person being registered." />
                            </div>
                            <div class="col col-xs-12 col-md-6">
                                <Rock:RockTextBox ID="txtRegistrantLName" Required="true" runat="server" Label="Which column contains the registrant's last name?" Help="The last name of the person being registered." />
                            </div>
                        </div>
                        <h6>Additional Columns</h6>
                        <p>Add additional columns and select the data they correlate to in Rock. Person properties will be used to find the person in Rock but not updated.</p>
                        <asp:Panel ID="pnlCSVColumns" runat="server"></asp:Panel>
                        <div class="row">
                            <div class="col col-xs-12 actions">
                                <Rock:BootstrapButton ID="btnAddColumn" runat="server" OnClick="btnAddColumn_Click" CssClass="btn btn-action btn-xs btn-square pull-left" ><i class="fa fa-plus-circle"></i></Rock:BootstrapButton>
                            </div>
                        </div>
                        <br />
                        <div class="row">
                            <div class="col col-xs-12">
                                <Rock:BootstrapButton ID="btnBackToRISelect" runat="server" CausesValidation="false" CssClass="btn btn-primary pull-left" Text="Back" CommandArgument="1" OnClick="btnChangePnl_Click" />
                                <Rock:BootstrapButton ID="btnSubmit" runat="server" CssClass="btn btn-primary pull-right" Text="Finish" OnClick="btnSubmit_Click" />
                            </div>
                        </div>
                    </asp:Panel>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
<style>
    .del-btn-wrapper {
        display: flex;
        justify-content: center;
        height: 100%;
        min-height: 59px;
        align-items: flex-end;
    }
</style>