<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ClosedLocations.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Checkin.ClosedLocations" %>

<Rock:RockUpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <h3 class="margin-b-lg xs-text-center text-center"><asp:Literal ID="lTitle" runat="server" /></h3>

        <Rock:NotificationBox ID="nbAllOpen" runat="server" NotificationBoxType="Success" Text="All rooms are open and have at least 3 open spots." />

        <asp:Repeater ID="rLocations" runat="server">
            <ItemTemplate>
                <div class='clearfix alert alert-<%# (((bool)Eval("IsClosed")) ? "danger" : "warning" ) %>'>
                    <a style="text-decoration:none" href='<%# GetLocationLink( Eval("LocationId") ) %>'>
                        <h4>
                            <%# Eval("LocationName") %>
                            <div class="pull-right">
                                <span class='badge badge-<%# (((bool)Eval("IsClosed")) ? "danger" : "warning" ) %>'><%# Eval("Capacity") %></span>
                            </div>
                        </h4>
                    </a>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <div style="display:none">
            <asp:LinkButton ID="lbRefresh" runat="server" OnClick="lbRefresh_Click" />
        </div>

    </ContentTemplate>
</Rock:RockUpdatePanel>
