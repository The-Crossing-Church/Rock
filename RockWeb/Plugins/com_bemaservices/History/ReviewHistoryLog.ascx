<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReviewHistoryLog.ascx.cs" Inherits="RockWeb.com_bemaservices.Core.ReviewHistoryLog" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-file-text-o"></i>
                    <asp:Literal ID="lHeading" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlDateAdded" runat="server" LabelType="Info" />
                </div>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server">
                        <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" Required="false" EntityTypeName="Rock.Model.History" />
                        <Rock:PersonPicker ID="ppWhoFilter" runat="server" Label="Edited By" />
                        <Rock:RockTextBox ID="tbSummary" runat="server" Label="Summary Contains" />
                        <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                        <Rock:DefinedValuesPicker ID="dvfReviewStatus" runat="server" Label="Review Status" />
                        <Rock:PersonPicker ID="ppPersonEdited" runat="server" Label="Person Edited" />
                        <Rock:GroupPicker ID="gpExcludeGroup" runat="server" Label="Group To Exclude" />
                    </Rock:GridFilter>
                    <div class="row" style="padding: 12px;">
                    <div class="col-md-12">
                        <div class="actions">
                            <asp:LinkButton ID="LinkButton1" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Mark Selected As Pending" CssClass="btn btn-primary" OnClick="btnAllPending_Click" />
                            <asp:LinkButton ID="LinkButton2" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Mark Selected As Corrected" CssClass="btn btn-primary" OnClick="btnAllCorrected_Click" />
                            <asp:LinkButton ID="LinkButton3" runat="server" AccessKey="r" ToolTip="Alt+r" Text="Mark Selected As Reviewed" CssClass="btn btn-primary" OnClick="btnAllReviewed_Click" />
                         </div>
                    </div>
                    </div>
                    <Rock:Grid ID="gHistory" runat="server" AllowSorting="true" RowItemText="Change" SelectedKeys="Entity" >
                        <Columns>
                            <Rock:SelectField ></Rock:SelectField>
                            <Rock:RockBoundField DataField="Category.Name" SortExpression="Category.Name" HeaderText="Category" />
                            <asp:HyperLinkField DataTextField="Entity" DataNavigateUrlFields="EntityId" SortExpression="Entity" DataNavigateUrlFormatString="~/Person/{0}" HeaderText="Person Edited" />
                            <Rock:ListDelimitedField DataField="HistoryList" Delimiter="<br />" HeaderText="Did" HtmlEncode="false" SortExpression="Verb,ValueName" />
                            <Rock:RockBoundField DataField="FormattedCaption" HeaderText="Review Status" SortExpression="Caption" HtmlEncode="false" />
                            <asp:HyperLinkField DataTextField="CreatedByPersonName" DataNavigateUrlFields="CreatedByPersonId" SortExpression="CreatedByPersonName" DataNavigateUrlFormatString="~/Person/{0}" HeaderText="Edited By" />
                            <Rock:DateTimeField DataField="CreatedDateTime" SortExpression="CreatedDateTime" HeaderText="When" FormatAsElapsedTime="true" />
                            <Rock:LinkButtonField Text="<i class='fa fa-question'></i>" CssClass="btn btn-default btn-sm" HeaderText="Pending" OnClick="btnPending_Click" ItemStyle-HorizontalAlign="Center"></Rock:LinkButtonField>
                            <Rock:LinkButtonField Text="<i class='fa fa-check'></i>" CssClass="btn btn-default btn-sm" HeaderText="Corrected" OnClick="btnCorrected_Click" ItemStyle-HorizontalAlign="Center"></Rock:LinkButtonField>
                            <Rock:LinkButtonField Text="<i class='fa fa-search'></i>" CssClass="btn btn-default btn-sm" HeaderText="Reviewed" OnClick="btnReviewed_Click" ItemStyle-HorizontalAlign="Center"></Rock:LinkButtonField>
                        </Columns>
                    </Rock:Grid>
                </div>
                
                <div class="row">
                    <div class="col-md-12">
                        <div class="actions">
                            <asp:LinkButton ID="btnPending" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Mark Selected As Pending" CssClass="btn btn-primary" OnClick="btnAllPending_Click" />
                            <asp:LinkButton ID="btnCorrected" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Mark Selected As Corrected" CssClass="btn btn-primary" OnClick="btnAllCorrected_Click" />
                            <asp:LinkButton ID="btnReviewed" runat="server" AccessKey="r" ToolTip="Alt+r" Text="Mark Selected As Reviewed" CssClass="btn btn-primary" OnClick="btnAllReviewed_Click" />
                         </div>
                    </div>
                </div>

                <Rock:ModalAlert ID="maWarning" runat="server" />

            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
