<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RedirectorRuleList.ascx.cs" Inherits="RockWeb.Plugins.tech_triumph.WebAgility.RedirectorRuleList" %>

<asp:UpdatePanel ID="upnlContent" runat="server" >
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-route"></i> Redirector Rules</h1>
            </div>
            <div class="panel-body">
                
                <Rock:ModalAlert id="mdGridWarning" runat="server" />
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:RockTextBox ID="tbFilterSource" runat="server" Label="Source Contains" />
                        <Rock:RockCheckBox ID="cbFilterActive" runat="server" Label="Is Active" />
                    </Rock:GridFilter>
                    
                    <Rock:Grid ID="gRedirectorRules" runat="server" AllowSorting="true" OnRowSelected="gRedirectorRules_RowSelected">
                        <Columns>
                            <Rock:ReorderField/>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <Rock:RockBoundField DataField="SourceOptions.SourceUrl" HeaderText="Source Pattern" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                            <Rock:DeleteField OnClick="gRedirectorRules_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
