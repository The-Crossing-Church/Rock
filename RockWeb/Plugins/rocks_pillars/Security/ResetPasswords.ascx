<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResetPasswords.ascx.cs" Inherits="RockWeb.Plugins.rocks_pillars.Security.ResetPasswords" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
                
            <div class="panel-heading clearfix">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-eraser"></i> 
                    Reset Passwords
                </h1>
            </div>

            <div class="panel-body">

                <Rock:DataViewItemPicker ID="dvPeople" runat="server" Label="Person Data View" Required="true" 
                    Help="Any Database login for the people in this data view will have the password reset to the selected password." />
                <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" Required="true"
                    Help="The password to set each Database login to for the people in the selecte data view" />

                <div class="actions">
                    <Rock:BootstrapButton ID="btnReset" runat="server" CssClass="btn btn-primary" Text="Reset Passwords" DataLoadingText="Resetting Passwords..." CausesValidation="true" OnClick="btnReset_Click" />
                </div>
                <br />

                <Rock:NotificationBox ID="nbSuccess" runat="server" NotificationBoxType="Success" Heading="Reset Summary" Visible="false" />
                <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" Visible="false" />

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
