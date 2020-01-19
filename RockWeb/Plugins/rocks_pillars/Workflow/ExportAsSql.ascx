
<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExportAsSql.ascx.cs" Inherits="RockWeb.Plugins.rocks_pillars.Workflow.ExportAsSql" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-share-alt"></i> <asp:Literal id="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <div class="alert alert-info">
                    <p>
                        <asp:Literal ID="lAlert" runat="server" />
                    </p>
                    <p><strong>Disclaimer:</strong> Any reference to custom workflow actions, attributes, or other entities, will not be set in the new workflow type created by the script.</p>
                    <asp:Button ID="btnGenerate" runat="server" Text="Create Script" CssClass="btn btn-default margin-t-md" OnClick="btnGenerate_Click"/>
                </div>
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
