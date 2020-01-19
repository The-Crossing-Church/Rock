<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceFilter.ascx.cs" Inherits="RockWeb.Plugins.rocks_pillars.Core.RegistrationInstanceFilter" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-filter"></i>
                    Filter
                </h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbQueryError" runat="server" NotificationBoxType="Danger" Title="Query Error" Visible="false" />
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div class="row">
                    <div class="col-sm-4">
                        <Rock:RockDropDownList ID="ddlActive" runat="server" Label="Instance Status" AutoPostBack="true" OnSelectedIndexChanged="ddlActive_SelectedIndexChanged">
                            <asp:ListItem Text="Active Only" Value="active" />
                            <asp:ListItem Text="Active And Inactive" Value="all" />
                        </Rock:RockDropDownList>
                    </div>
                    <div class="col-sm-4">
                        <Rock:RockDropDownList ID="ddlInstance" runat="server" Label="Event Instance" AutoPostBack="true" OnSelectedIndexChanged="ddlInstance_SelectedIndexChanged" />
                        <Rock:RockListBox ID="lbInstances" runat="server" Label="Event Instance(s)" Visible="false" />
                    </div> 
                    <div class="col-sm-4">
                        <Rock:RockDropDownList ID="ddlAttribute" runat="server" Label="Attribute" Visible="false" />
                        <Rock:RockListBox ID="lbAttributes" runat="server" Label="Attribute(s)" Visible="false" />
                    </div>

                </div>

                <div class="pull-right">
                    <Rock:BootstrapButton ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" />
                </div>

            </div>
       
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
