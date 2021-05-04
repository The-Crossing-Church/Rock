<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StickyNavBar.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Cms.StickyNavBar" %>
<div class="sticky-nav" runat="server" id="divStickyNavBar">
    <div class="item" id="itemOne" runat="server" Visible="false">
        <a id="btnStickyOne" class="btn btn-primary" runat="server" Visible="false"></a>
        <span id="txtStickyOne" runat="server" Visible="false"></span>
    </div>
    <div class="item" id="itemTwo" runat="server" Visible="false">
        <a id="btnStickyTwo" class="btn btn-primary" runat="server" Visible="false"></a>
        <span id="txtStickyTwo" runat="server" Visible="false"></span>
    </div>
</div>
<style>
    .sticky-nav {
        min-height: 40px;
        position: fixed;
        width: 100%;
        left: 0;
        right: 0;
        z-index: 100;
        padding: 8px 16px;
        display: flex;
        justify-content: center;
    }
    .sticky-nav.bottom {
        bottom: 0; 
    }
    .sticky-nav.top {
        top: 145px;
    }
    @media only screen and (max-width: 768px) {
    .sticky-nav.top {
            top: 132px;;
        }
    }
    .item {
        display: flex;
        align-items: center;
        padding: 0px 4px;
    }
</style>
<script>
    $(document).ready(() => {
        let e = document.getElementsByClassName('top')
        if (e.length > 0) {
            $('main.container').attr('style', 'padding-top: 200px !important;')
        }
    })
</script>