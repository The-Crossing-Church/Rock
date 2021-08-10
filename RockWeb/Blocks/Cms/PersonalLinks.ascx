﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalLinks.ascx.cs" Inherits="RockWeb.Blocks.Cms.PersonalLinks" %>
<%@ Import Namespace="Rock" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        Rock.personalLinks.buildQuickReturn();

        $(".js-rock-bookmark").off("click").on("click", function (e) {
            e.preventDefault();

            // if one of the configuration options is open (AddLink or AddSection), don't hide the links
            var bookMarkConfigurationMode = $(".js-bookmark-configuration").length > 0;

            if (!bookMarkConfigurationMode) {

                // Show/hide the personalLinks
                Rock.personalLinks.showPersonalLinks($('#<%= upnlContent.ClientID %>'), $(this))
            }
        });
    })
</script>

<asp:LinkButton runat="server" ID="lbBookmark" Visible="false" class="rock-bookmark js-rock-bookmark"
    href="#" ><i class="fa fa-bookmark"></i></asp:LinkButton>


<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional" class="popover rock-popover styled-scroll js-personal-link-popover position-fixed d-none" role="tooltip">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server" CssClass="rock-popover">
            <div class="popover-panel">
                <h3 class="popover-title">Personal Links

                    <div class="ml-auto">
                        <div class="dropdown pull-right">
                            <a class="btn btn-xs py-0 px-1 text-muted" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" role="button"><i class="fa fa-ellipsis-v"></i></a>
                            <ul class="dropdown-menu">
                                <li><asp:LinkButton runat="server" ID="lbManageLinks" OnClick="lbManageLinks_Click">Manage Links</asp:LinkButton></li>
                            </ul>
                        </div>
                        <div class="dropdown pull-right">
                            <a class="btn btn-xs py-0 px-1 text-muted" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" role="button"><i class="fa fa-plus"></i></a>
                            <ul class="dropdown-menu">
                                <li><asp:LinkButton runat="server" ID="lbAddLink" OnClick="lbAddLink_Click">Add Link</asp:LinkButton></li>
                                <li><asp:LinkButton runat="server" ID="lbAddSection" OnClick="lbAddSection_Click">Add Section</asp:LinkButton></li>
                            </ul>
                        </div>
                    </div>
                </h3>
                <div class="popover-content">
                    <asp:Repeater ID="rptPersonalLinkSection" runat="server" OnItemDataBound="rptPersonalLinkSection_ItemDataBound">
                        <ItemTemplate>
                            <ul class="list-unstyled">
                                <li><strong><%# (bool)Eval("IsShared") ? "<i class='fa fa-users'></i>" : string.Empty %> <%# ((string)Eval( "Name" )).FixCase() %></strong></li>
                                <asp:Repeater ID="rptLinks" runat="server">
                                    <ItemTemplate>
                                        <li><a href="<%#Eval("Url")%>"><%#((string)Eval( "Name" )).FixCase()%></a></li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
            <div class="popover-panel">
                <h3 class="popover-title">Quick Returns</h3>
                <div id="divQuickReturn" class="popover-content"></div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlAddSection" runat="server" CssClass="popover-panel w-100 js-bookmark-configuration js-bookmark-add-section" Visible="false">
            <h3 class="popover-title">Personal Link Section</h3>
            <div class="panel-body">
                <fieldset>
                    <Rock:DataTextBox ID="tbSectionName" runat="server" SourceTypeName="Rock.Model.PersonalLinkSection, Rock" PropertyName="Name" ValidationGroup="vgAddSection" />
                </fieldset>
                <div class="actions">
                    <asp:LinkButton ID="btnSectionSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary btn-xs js-rebuild-links" OnClick="btnSectionSave_Click" ValidationGroup="vgAddSection" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link btn-xs js-rebuild-links" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlAddLink" runat="server" CssClass="popover-panel w-100 js-bookmark-configuration js-bookmark-add-link" Visible="false">
            <h3 class="popover-title">Personal Link</h3>
            <div class="panel-body">
                <fieldset>
                    <Rock:DataTextBox ID="tbLinkName" runat="server" SourceTypeName="Rock.Model.PersonalLink, Rock" PropertyName="Name" ValidationGroup="vgAddLink" />
                    <Rock:UrlLinkBox ID="urlLink" runat="server" Label="Link URL" ValidationGroup="vgAddLink" Required="true" />
                    <Rock:RockDropDownList ID="ddlSection" runat="server" ValidationGroup="vgAddLink" Label="Section" />
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnLinkSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary btn-xs js-rebuild-links" OnClick="btnLinkSave_Click" ValidationGroup="vgAddLink" />
                    <asp:LinkButton ID="btnLinkCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link btn-xs js-rebuild-links" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>