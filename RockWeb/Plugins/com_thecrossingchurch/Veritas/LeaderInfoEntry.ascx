<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LeaderInfoEntry.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Veritas.LeaderInfoEntry" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title" id="pnlTitle" runat="server"></h3>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col col-xs-12 col-md-4">
                        <Rock:DatePicker ID="MeetingDate" runat="server" Label="Meeting Date" />
                    </div>
                </div>
                <div class="row">
                    <div class="col col-xs-12 col-md-6">
                        <h4>Attendance</h4>
                        <Rock:Grid ID="grdAttendance" runat="server" AllowSorting="true" AllowPaging="True" PersonIdField="Id" EmptyDataText="No Results" ShowActionRow="false" >
                            <Columns>
                                <Rock:SelectField ItemStyle-Width="48px" />
                                <Rock:RockBoundField DataField="Id" Visible="false" />
                                <Rock:RockTemplateField HeaderText="Name" SortExpression="NickName LastName" >
                                    <ItemTemplate>
                                        <asp:Label ID="lblNickName" runat="server"
                                            Text='<%# Bind("NickName") %>'></asp:Label> 
                                        <asp:Label ID="lblLastName" runat="server"
                                            Text='<%# Bind("LastName") %>'></asp:Label>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                            </Columns>
                        </Rock:Grid>
                    </div>
                    <div class="col col-xs-12 col-md-6">
                        <h4>One on Ones</h4>
                        <Rock:Grid ID="grdOneOnOne" runat="server" AllowSorting="true" AllowPaging="True" PersonIdField="Id" EmptyDataText="No Results" ShowActionRow="false" >
                            <Columns>
                                <Rock:SelectField ItemStyle-Width="48px" />
                                <Rock:RockBoundField DataField="Id" Visible="false" />
                                <Rock:RockTemplateField HeaderText="Name" SortExpression="NickName LastName" >
                                    <ItemTemplate>
                                        <asp:Label ID="lblNickName" runat="server"
                                            Text='<%# Bind("NickName") %>'></asp:Label> 
                                        <asp:Label ID="lblLastName" runat="server"
                                            Text='<%# Bind("LastName") %>'></asp:Label>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
                <div class="row">
                    <div class="col col-xs-12">
                        <h4>Leader Check-in</h4>
                        <div class="row">
                            <div class="col col-xs-6 col-md-4 js-form-options">
                                <Rock:RockCheckBox ID="chkAttendingSG" runat="server" Text="Attending their Small group?" />
                            </div>
                            <div class="col col-xs-6 col-md-8">
                                <Rock:RockTextBox ID="txtAttendingSG" runat="server" Placeholder="Additional Details" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-6 col-md-4 js-form-options">
                                <Rock:RockCheckBox ID="chkSpiritualGrowth" runat="server" Text="Spiritual Growth?" />
                            </div>
                            <div class="col col-xs-6 col-md-8">
                                <Rock:RockTextBox ID="txtSpiritualGrowth" runat="server" Placeholder="Additional Details" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-6 col-md-4 js-form-options">
                                <Rock:RockCheckBox ID="chkMeetingCO" runat="server" Text="Consistent meetings with CO?" />
                            </div>
                            <div class="col col-xs-6 col-md-8">
                                <Rock:RockTextBox ID="txtMeetingCO" runat="server" Placeholder="Additional Details" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-6 col-md-4 js-form-options">
                                <Rock:RockCheckBox ID="chkPreping" runat="server" Text="Prepping lessons intentionally?" />
                            </div>
                            <div class="col col-xs-6 col-md-8">
                                <Rock:RockTextBox ID="txtPreping" runat="server" Placeholder="Additional Details" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-6 col-md-4 js-form-options">
                                <Rock:RockCheckBox ID="chkLeaderMeetings" runat="server" Text="Attending Leader Meetings?" />
                            </div>
                            <div class="col col-xs-6 col-md-8">
                                <Rock:RockTextBox ID="txtLeaderMeetings" runat="server" Placeholder="Additional Details" />
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col col-xs-12">
                        <h4>Follow-up</h4>
                        <div class="row">
                            <div class="col col-xs-6 col-md-4 js-form-options">
                                <Rock:RockCheckBox ID="chkResources" runat="server" Text="Do they need resources?" />
                            </div>
                            <div class="col col-xs-6 col-md-8">
                                <Rock:RockTextBox ID="txtResources" runat="server" Placeholder="Additional Details" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-6 col-md-4 js-form-options">
                                <Rock:RockCheckBox ID="chkPrayer" runat="server" Text="Things to pray for?" />
                            </div>
                            <div class="col col-xs-6 col-md-8">
                                <Rock:RockTextBox ID="txtPrayer" runat="server" Placeholder="Additional Details" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-6 col-md-4 js-form-options">
                                <Rock:RockCheckBox ID="chkBooks" runat="server" Text="Books reccomendations?" />
                            </div>
                            <div class="col col-xs-6 col-md-8">
                                <Rock:RockTextBox ID="txtBooks" runat="server" Placeholder="Additional Details" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-6 col-md-4 js-form-options">
                                <Rock:RockCheckBox ID="chkQuestions" runat="server" Text="Theological Questions?" />
                            </div>
                            <div class="col col-xs-6 col-md-8">
                                <Rock:RockTextBox ID="txtQuestions" runat="server" Placeholder="Additional Details" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col col-xs-6 col-md-4 js-form-options">
                                <Rock:RockCheckBox ID="chkCirriculum" runat="server" Text="Curriculum?" />
                            </div>
                            <div class="col col-xs-6 col-md-8">
                                <Rock:RockTextBox ID="txtCirriculum" runat="server" Placeholder="Additional Details" />
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col col-xs-12">
                        <h4>General Feelings</h4>
                        <div class="row">
                            <div class="col col-xs-12">
                                <Rock:RockCheckBoxList ID="chkListGeneralFeelings" runat="server" />
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col col-xs-12">
                        <h4>Additional Notes</h4>
                        <div class="row">
                            <div class="col col-xs-12">
                                <Rock:RockTextBox runat="server" id="txtNotes" TextMode="MultiLine"/>
                            </div>
                        </div>
                    </div>
                </div>
                <br />
                <div class="row">
                    <div class="col col-xs-12">
                        <Rock:BootstrapButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary pull-right" OnClick="btnSave_Click" />
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<style>

</style>
