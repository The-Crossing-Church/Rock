<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReprintLabels.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Checkin.ReprintLabels" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="row">
            <div class="col-md-12">
                <asp:Literal ID="lWarning" runat="server" />
            </div>
        </div>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-print"></i> Choose a Printer</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gPrinters" runat="server" DataKeyNames="Id">
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Printer Name" />
                            <Rock:RockBoundField DataField="IPAddress" HeaderText="IP Address / Hostname" />
                            <Rock:LinkButtonField HeaderText="Print" Text='<i class="fa fa-print"></i>' CssClass="btn btn-default btn-sm" OnClick="gPrint_Click" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="left" HeaderStyle-Width="75" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlPeople" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> People Checked In</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gCheckInGroup" runat="server" DisplayType="Full" AllowSorting="true" CssClass="js-grid-group-members" >
                        <Columns>
                            <Rock:SelectField></Rock:SelectField>
                            <Rock:RockBoundField DataField="FullName" HeaderText="Name" SortExpression="Person.LastName,Person.NickName" HtmlEncode="false" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlLabels" CssClass="panel panel-block" runat="server">            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-files-o"></i> Labels to Print</h1>
            </div>            
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:Grid ID="gLabels" runat="server" DisplayType="Full" >
                        <Columns>
                            <Rock:SelectField></Rock:SelectField>
                            <Rock:RockBoundField DataField="FileName" HeaderText="Name" HtmlEncode="false" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
