<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RequestHeaderList.ascx.cs" Inherits="RockWeb.Plugins.tech_triumph.WebAgility.RequestHeaderList" %>

<asp:UpdatePanel ID="upnlContent" runat="server" >
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-sign-out-alt"></i> Request Header Rules</h1>
            </div>
            <div class="panel-body">
                
                <Rock:ModalAlert id="mdGridWarning" runat="server" />
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:RockTextBox ID="tbFilterSource" runat="server" Label="Source Contains" />
                        <Rock:RockCheckBox ID="cbFilterActive" runat="server" Label="Is Active" />
                    </Rock:GridFilter>
                    
                    <Rock:Grid ID="gRequestHeaderRules" runat="server" AllowSorting="true" OnRowSelected="gRequestHeaderRules_RowSelected">
                        <Columns>
                            <Rock:ReorderField/>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <Rock:RockBoundField DataField="SourceOptions.SourceUrl" HeaderText="Source Pattern" />
                            <Rock:RockBoundField DataField="HeaderConfiguration.HeaderName" HeaderText="Header Name" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                            <Rock:DeleteField OnClick="gRequestHeaderRules_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
