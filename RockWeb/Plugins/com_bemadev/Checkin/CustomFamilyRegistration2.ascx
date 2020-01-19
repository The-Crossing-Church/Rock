<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CustomFamilyRegistration2.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Checkin.CustomFamilyRegistration" %>
<%@ Register TagPrefix="bema" Namespace="com.bemadev.Checkin.Web.UI.Controls" Assembly="com.bemadev.Checkin" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>


        <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-check"></i>&nbsp;Family Registration</h1>
                    <div class="panel-labels"> 
                        <span class="label label-campus"><asp:Literal ID="lCampus" runat="server" /></span>
                    </div>
                </div>
                <div class="panel-body">
                    <asp:Panel ID="pnlFamily" runat="server">
                        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                        <div class="row">
                            <div class="col-md-6">
                                <div class="panel panel-default">
                                    <div class="panel-body">
                                        <div class="btn-group margin-b-md">
                                            <label>Please Select Registration Type</label><br />
                                            <asp:LinkButton ID="btnModeNew" runat="server" CssClass="btn btn-default active" Text="New" data-val="0" OnClick="btnModeNew_Click" CausesValidation="false" />
                                            <asp:LinkButton ID="btnModeExisting" runat="server" CssClass="btn btn-default" Text="Existing" data-val="1" OnClick="btnModeExisting_Click" CausesValidation="false" />
                                        </div>
                                        <br />
                                        <Rock:PersonPicker ID="ppExistingFamilyMember" runat="server" Label="Existing Family Member" ToolTip="Please select ONE Family Member from the list to Find the family." Required="true" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <bema:NewFamilyMembers ID="nfmMembers" runat="server" OnAddGroupMemberClick="nfmMembers_AddFamilyMemberClick" />
                            </div>
                            <div class="col-md-6">
                                <Rock:AddressControl ID="acAddress" runat="server" Label="Family Address" />
                            </div>
                            <div class="col-md-6">
                                <Rock:AddressControl ID="acGuestAddress" runat="server" Label="Guest Address" Visible="false" />
                            </div>
                        
                            <div class="col-md-12">
                                <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="pull-right btn btn-primary" OnClick="lbSave_Click" />
                            </div>
                        </div>

                    </asp:Panel>

                    <asp:Panel ID="pnlMessages" runat="server" Visible="false">
                        
                        <div class="row">
                            <div class="col-md-12">
                                <asp:Literal ID="lMessages" runat="server" />
                                <asp:LinkButton ID="lbAddNewFamily" runat="server" Text="Add New Family" CssClass="pull-right btn btn-primary" OnClick="lbAddNewFamily_Click" />
                            </div>
                        </div>
                    </asp:Panel>
                </div>

    </ContentTemplate>
</asp:UpdatePanel>
