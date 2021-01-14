<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CustomContentChannelBuilder.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Cms.CustomContentChannelBuilder" %>
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/grapesjs/0.16.22/css/grapes.min.css">
<script src="https://cdnjs.cloudflare.com/ajax/libs/grapesjs/0.16.22/grapes.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/grapesjs-preset-webpage@0.1.11/dist/grapesjs-preset-webpage.min.js"></script>
<script src="https://unpkg.com/grapesjs-parser-postcss"></script>
<link href="https://fonts.googleapis.com/css2?family=Material+Icons" rel="stylesheet">
<div id="gjs" runat="server" style="height: 90vh !important; overflow: hidden; width: 100%;"></div>
<br />
<asp:UpdatePanel runat="server" ID="pnlUpdate">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading rollover-container">
                <h4 class="panel-title"><i class="fa fa-rebel"></i>&nbsp;Additional Information</h4>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col col-xs-6">
                        <div class="form-group rock-drop-down-list">
	                        <label class="control-label" >Content Channel</label>
                            <div class="control-wrapper">
                                <asp:DropDownList CssClass="form-control" ID="pkrCC" runat="server" OnSelectedIndexChanged="pkrCustom_SelectedIndexChanged" AutoPostBack="true" >
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                    <div class="col col-xs-6">
                        <Rock:RockTextBox Label="Title" runat="server" ID="txtTitle" />
                    </div>
                </div>
                <div class="row">
                    <div class="col col-xs-4">
                        <Rock:DateTimePicker Label="Start Date" runat="server" ID="dtStart" />
                    </div>
                    <div class="col col-xs-4">
                        <Rock:DateTimePicker Label="End Date" runat="server" ID="dtEnd" />
                    </div>
                    <div class="col col-xs-4">
                        <Rock:NumberBox Label="Priority" runat="server" ID="nbPriority" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-xs-12">
                        <Rock:DynamicPlaceholder ID="phAttributes" runat="server" />
                    </div>
                </div>
                <div class="pull-right">
                    <asp:LinkButton runat="server" ID="btnCreate" Text="Create Item" CssClass="btn btn-primary"  OnClientClick="saveTemplate();" OnClick="btnCreate_Click" />
		        </div>
	        </div>
            <Rock:HiddenFieldWithClass runat="server" ID="hfHtml" />
            <Rock:HiddenFieldWithClass runat="server" ID="hfCss" />
            <Rock:HiddenFieldWithClass runat="server" ID="hfComponents" />
            <Rock:HiddenFieldWithClass runat="server" ID="hfStyle" />
            <Rock:HiddenFieldWithClass runat="server" ID="hfId" />
            <Rock:HiddenFieldWithClass runat="server" ID="hfStyleSheets" />
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

<script src="/Scripts/com_thecrossingchurch/CustomContentGrapesComponents.js"  type="text/javascript"></script>

<style>
    .gjs-block.fa {
        font-size: 4em;
        font-weight: 400;
    }
    .gjs-block-label {
        font-size: .8rem; 
    }
    .row {
        padding: 8px;
    }
    .col {
        min-height: 30px; 
    }
    .material-icons.stop:before {
        content: "stop";
    }
    .material-icons.pause:before {
        content: "pause";
    }
    .material-icons.view_array:before {
        content: "view_array";
    }
    .material-icons.view_column:before {
        content: "view_column";
    }
    .material-icons.arrow_spacer:before {
        content: "unfold_more";
    }
    .material-icons {
        font-size: 4em;
        line-height: 2em;
        padding: 11px;
    }
</style>
