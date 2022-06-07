<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RequestViewer.ascx.cs" Inherits="RockWeb.Plugins.tech_triumph.WebAgility.RequestViewer" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-search"></i> 
                    Server-side Request Viewer
                </h1>
            </div>

            <div class="panel-body">

                <h4>Request Viewer</h4>
                <p>Below is the details for the current request.</p>


                <strong>URL:</strong> <%= Request.Url %><br />
                <strong>Method:</strong> <%= Request.HttpMethod %><br />
                <strong>Application Path:</strong> <%= Request.ApplicationPath %><br />
                <strong>Content Encoding:</strong> <%= Request.ContentEncoding %><br />
                <strong>Physical Application Path:</strong> <%= Request.PhysicalApplicationPath %><br />
                <strong>Referrer:</strong> <%= Request.UrlReferrer %><br />
                <strong>User Agent:</strong> <%= Request.UserAgent %><br />
                <strong>Host Name:</strong> <%= Request.UserHostName %><br />
                <strong>Host Address:</strong> <%= Request.UserHostAddress %><br />

                <p>

                <hr />

                <h5>Headers</h5>
                <ul>
                    <asp:Literal ID="lHeaders" runat="server" />
                </ul>

                <hr />

                <h5>Server Variables</h5>
                <ul>
                    <asp:Literal ID="lServerVariables" runat="server" />
                </ul>

                <hr />

                <h5>Cookies</h5>
                <ul>
                    <asp:Literal ID="lCookies" runat="server" />
                </ul>
                

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>