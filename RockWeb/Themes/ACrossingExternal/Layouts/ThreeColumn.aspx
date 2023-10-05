<%@ Page Language="C#" MasterPageFile="Site.Master" AutoEventWireup="true" Inherits="Rock.Web.UI.RockPage" %>
<%@ Register TagPrefix="tcc" Namespace="org.crossingchurch.OurRock.Web.UI.Controls" Assembly="org.crossingchurch.OurRock" %>

<asp:Content ID="ctMain" ContentPlaceHolderID="main" runat="server">

    <main class="container">
                
        <!-- Start Content Area -->
        
        <!-- Page Title -->
        <Rock:PageIcon ID="PageIcon" runat="server" /> <tcc:PageTitle ID="PageTitle" runat="server" Tag="h1" CssClasses="pagetitle py-custom-2x" />
        <Rock:PageBreadCrumbs ID="PageBreadCrumbs" runat="server" />
                    
        <!-- Ajax Error -->
        <div class="alert alert-danger ajax-error" style="display:none">
            <p><strong>Error</strong></p>
            <span class="ajax-error-message"></span>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Feature" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <Rock:Zone Name="Sidebar 1" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="Main" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="Sidebar 2" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:Zone Name="Section A" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-md-4">
                <Rock:Zone Name="Section B" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="Section C" runat="server" />
            </div>
            <div class="col-md-4">
                <Rock:Zone Name="Section D" runat="server" />
            </div>
        </div>

        <!-- End Content Area -->

    </main>

</asp:Content>
