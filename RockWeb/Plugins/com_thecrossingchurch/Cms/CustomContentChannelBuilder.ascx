<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CustomContentChannelBuilder.ascx.cs" Inherits="RockWeb.Plugins.com_thecrossingchurch.Cms.CustomContentChannelBuilder" %>
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/grapesjs/0.16.22/css/grapes.min.css">
<script src="https://cdnjs.cloudflare.com/ajax/libs/grapesjs/0.16.22/grapes.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/grapesjs-preset-webpage@0.1.11/dist/grapesjs-preset-webpage.min.js"></script>
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
                    <div class="col col-xs-6">
                        <Rock:DateTimePicker Label="Start Date" runat="server" ID="dtStart" />
                    </div>
                    <div class="col col-xs-6">
                        <Rock:DateTimePicker Label="End Date" runat="server" ID="dtEnd" />
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
        </div>
    </ContentTemplate>
</asp:UpdatePanel>

<script  type="text/javascript">
    console.log('update builder')
    let el = $("[id$='_gjs']")[0].id
    var editor = grapesjs.init({
        height: '90vh',
        container: `#${el}`,
        plugins: ['gjs-preset-webpage'],
        pluginsOpts: {
            'gjs-preset-webpage': {
                blocks: ['link', 'text', 'image', 'video', 'quote']
            }
        },
        canvas: {
            styles: ['https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css']
        },
        baseCss: `
            * {
              box-sizing: border-box;
            }
            html, body, [data-gjs-type=wrapper] {
              min-height: 100%;
            }
            body {
              margin: 0;
              height: 100%;
              background-color: #fff
            }
            [data-gjs-type=wrapper] {
              overflow: auto;
              overflow-x: hidden;
            }
            * ::-webkit-scrollbar-track {
              background: rgba(0, 0, 0, 0.1)
            }
            * ::-webkit-scrollbar-thumb {
              background: rgba(255, 255, 255, 0.2)
            }
            * ::-webkit-scrollbar {
              width: 10px
            }
            .row {
                margin: 0px;
            }
            .row, .col {
                padding: 8px;
            }
            .col {
                min-height: 75px;
            }
        `
    });
    //Show outlines
    editor.runCommand('core:component-outline', {})
    //Add Blocks! 
    var blockManager = editor.BlockManager;
    blockManager.getAll().reset();
    blockManager.add('col-one', {
        label: 'Single Column',
        category: 'Basic',
        content: `<div data-gjs-type="default" class="row" draggable="true">
                    <div data-gjs-type="column" class="col col-xs-12" draggable="true"></div>
                </div>`,
        attributes: {
            class: 'fa fa-stop'
        }
    });
    blockManager.add('col-two', {
        label: '50/50 Column',
        category: 'Basic',
        content: `<div data-gjs-type="default" class="row" draggable="true">
                    <div data-gjs-type="default" class="col col-xs-12 col-md-6" draggable="true"></div>
                    <div data-gjs-type="default" class="col col-xs-12 col-md-6" draggable="true"></div>
                </div>`,
        attributes: {
            class: 'fa fa-pause'
        }
    });
    blockManager.add('card', {
        label: 'Simple Card',
        category: 'Basic',
        content: `<div class="card">
                    <img data-gjs-type="image" class="card-media" draggable="true" />
                    <div class='card-body-wrapper'>
                        <h3 data-gjs-type="text" class="card-title" draggable="true" contenteditable="true">Title</h3>
                        <div data-gjs-type="text" class="card-body" draggable="true" contenteditable="true">Body Text</div>
                        <div data-gjs-type="default" class="row" draggable="true">
                            <div data-gjs-type="default" class="col col-xs-12 card-actions" draggable="true"></div>
                        </div>
                    </div>
                </div>
                <style>
                    .card {
                        box-shadow: 4px 4px 8px rgba(0,0,0,.2);
                        border-radius: 4px;
                        overflow: hidden;
                    }
                    .card-media {
                        width: 100%;
                    }
                    .card-body-wrapper {
                        padding: 8px;
                    }
                </style>`,
        attributes: {
            class: 'fa fa-clone'
        }
    });
    blockManager.add('card-button', {
        label: 'Card Action',
        category: 'Basic',
        content: `<a class="btn btn-primary">
                    Link Text
                  </a>`,
        attributes: {
            class: 'fa fa-mouse'
        }
    });
    function saveTemplate() {
        let hfHtml = $("[id$='_hfHtml']")[0].id;
        let hfCss = $("[id$='_hfCss']")[0].id;
        let hfComp = $("[id$='_hfComponents']")[0].id;
        let hfStyle = $("[id$='_hfStyle']")[0].id;
        try {
            let c = editor.getHtml().replaceAll(`>`, `&gt;`).replaceAll(`<`, `&lt;`).replaceAll(`&#039;`, `'`)
            document.getElementById(hfHtml).value = `${c}`;
            document.getElementById(hfCss).value = `${editor.getCss()}`;
            document.getElementById(hfComp).value = JSON.stringify(editor.getComponents());
            document.getElementById(hfStyle).value = JSON.stringify(editor.getStyle());
        } catch (e) {
            console.log(e);
        }
    }
    $(document).ready(() => {
        //Load Components and Styles or clear editor
        let hfComp = $("[id$='_hfComponents']")[0].id
        let hfStyle = $("[id$='_hfStyle']")[0].id
        editor.DomComponents.clear();
        editor.CssComposer.clear();
        editor.UndoManager.clear();
        if (document.getElementById(hfComp).value) {
            let content = JSON.parse(document.getElementById(hfComp).value)
            editor.setComponents(content)
        }
        if (document.getElementById(hfStyle).value) {
            let style = JSON.parse(document.getElementById(hfStyle).value)
            editor.setStyle(style);
        }
        //Watch for changes to the Id hidden field
        let hfId = $("[id$='_hfId']")[0].id
        document.getElementById(hfId).addEventListener('change', () => {
            console.log('changed')
        })
        document.getElementById(hfId).addEventListener('input', () => {
            console.log('input')
        })
    });
</script>

<style>
    .gjs-block.fa {
        font-size: 4em; 
    }
    .gjs-block-label {
        font-size: 1rem; 
    }
    .row {
        padding: 8px;
    }
    .col {
        min-height: 30px; 
    }
</style>
