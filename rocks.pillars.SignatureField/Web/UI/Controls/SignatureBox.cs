// <copyright>
// Copyright Pillars Inc.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace rocks.pillars.Web.UI.Controls
{
    /// <summary>
    /// Signature Control
    /// </summary>
    [ToolboxData( "<{0}:SignatureBox runat=server></{0}:SignatureBox>" )]
    public class SignatureBox : CompositeControl, IRockControl
    {

        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }
            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }
            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return CustomValidator != null ? CustomValidator.ErrorMessage : string.Empty;
            }
            set
            {
                if ( CustomValidator != null )
                {
                    CustomValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || CustomValidator == null || CustomValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        /// <summary>
        /// Gets or sets the custom validator.
        /// </summary>
        /// <value>
        /// The custom validator.
        /// </value>
        public CustomValidator CustomValidator { get; set; }

        /// <summary>
        /// Gets or sets the group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server.
        /// </summary>
        /// <returns>The group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server. The default value is an empty string ("").</returns>
        public string ValidationGroup
        {
            get
            {
                EnsureChildControls();
                return CustomValidator.ValidationGroup;
            }

            set
            {
                EnsureChildControls();
                CustomValidator.ValidationGroup = value;
            }
        }

        #endregion

        #region Controls

        /// <summary>
        /// Gets or sets the hf signature.
        /// </summary>
        /// <value>
        /// The hf signature.
        /// </value>
        public HiddenFieldWithClass _hfSignature;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the height (default of 200, minimum of 50)
        /// </summary>
        /// <value>
        /// The height of the control.
        /// </value>
        [
        Bindable( false ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The height in pixels of the control" )
        ]
        public string EditorHeight
        {
            get
            {
                var height = ViewState["EditorHeight"] as string;
                var heightPixels = ( height ?? string.Empty ).AsIntegerOrNull() ?? 0;

                if ( heightPixels <= 0 )
                {
                    // if height is not specified or is zero or less, default it to 200
                    height = "200";
                }
                else if ( heightPixels < 50 )
                {
                    // ensure a minimum height of 50 pixels
                    height = "50";
                }

                return height;
            }

            set
            {
                ViewState["EditorHeight"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value
        {
            get
            {
                EnsureChildControls();
                return _hfSignature.Value;
            }

            set
            {
                EnsureChildControls();
                _hfSignature.Value = value;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> class.
        /// </summary>
        /// <returns></returns>
        public SignatureBox() : base()
        {
            CustomValidator = new CustomValidator();
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            if ( this.Visible && !ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack )
            {
                RockPage.AddScriptLink( Page, ResolveUrl( "~/Scripts/rocks_pillars/signature_pad.min.js" ), false );
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _hfSignature = new HiddenFieldWithClass();
            _hfSignature.ID = this.ID + "_hfSignature";
            Controls.Add( _hfSignature );

            if ( CustomValidator != null )
            {
                CustomValidator.ID = this.ID + "_cv";
                CustomValidator.Display = ValidatorDisplay.Dynamic;
                CustomValidator.CssClass = "validation-error help-inline";
                CustomValidator.Enabled = Required;
                CustomValidator.ClientValidationFunction = $"validateSignature_{ClientID}";
                CustomValidator.ServerValidate += CustomValidator_ServerValidate;
                Controls.Add( CustomValidator );
            }
        }

        /// <summary>
        /// Handles the ServerValidate event of the CustomValidator control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        private void CustomValidator_ServerValidate( object source, ServerValidateEventArgs args )
        {
            args.IsValid =!Required || _hfSignature.Value.IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        public virtual void RenderBaseControl( HtmlTextWriter writer )
        {
            RegisterJavascript();

            // write custom css for the code editor
            string styleTag = $@"
<style>
    .signature-wrapper_{ClientID} {{
        position: relative;
        width: 100%;
        min-height: {this.EditorHeight}px;
        -moz-user-select: none;
        -webkit-user-select: none;
        -ms-user-select: none;
        user-select: none;
        border: solid black 1px;
    }}

    .has-error .signature-wrapper_{ClientID} {{
        border: solid #b94a48 1px;
    }}

    .signature-pad_{ClientID} {{
        position: absolute;
        left: 0;
        top: 0;
        width:100%;
        height:100%;
        background-color: white;
    }}
</style>
";
            writer.Write( styleTag );

            _hfSignature.CssClass = $"js-signature-file_{ClientID}";
            _hfSignature.RenderControl( writer );

            writer.AddAttribute( "class", $"signature-wrapper_{ClientID}" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( "id", $"signature-pad_{ClientID}" );
            writer.AddAttribute( "class", $"signature-pad_{ClientID}" );
            writer.RenderBeginTag( "canvas" );
            writer.RenderEndTag();  // canvas
            writer.RenderEndTag();  // div

            writer.AddAttribute( "class", "clearfix" );
            writer.RenderBeginTag( "div" );
            writer.AddAttribute( "href", "#" );
            writer.AddAttribute( "class", $"btn btn-link btn-info btn-sm js-signature-clear_{ClientID} pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.A );
            writer.Write( "Clear" );
            writer.RenderEndTag();  // a
            writer.RenderEndTag();  // div

            if ( CustomValidator != null )
            {
                if ( Required )
                {
                    CustomValidator.Display = ValidatorDisplay.Dynamic;
                    CustomValidator.CssClass = "validation-error help-inline";
                    CustomValidator.Enabled = true;
                    CustomValidator.ClientValidationFunction = $"validateSignature_{ClientID}";

                    if ( string.IsNullOrWhiteSpace( CustomValidator.ErrorMessage ) )
                    {
                        CustomValidator.ErrorMessage = Label + " is Required.";
                    }

                    CustomValidator.RenderControl( writer );
                }
                else
                {
                    CustomValidator.Enabled = false;
                }
            }
        }


        /// <summary>
        /// Registers the javascript.
        /// </summary>
        private void RegisterJavascript()
        {
            // UniqueCommon Script
            string script = $@"

    var signaturePad_{ClientID} = null;

    debugger;

    // Adjust canvas coordinate space taking into account pixel ratio,
    // to make it look crisp on mobile devices.
    // This also causes canvas to be cleared.
    function resizeCanvas_{ClientID}() {{

        // When zoomed out to less than 100%, for some very strange reason,
        // some browsers report devicePixelRatio as less than 1
        // and only part of the canvas is cleared then.
        var ratio = Math.max(window.devicePixelRatio || 1, 1);
        $('canvas.signature-pad_{ClientID}').each(function () {{
            var canvas = $(this)[0];
            canvas.width = canvas.offsetWidth * ratio;
            canvas.height = canvas.offsetHeight * ratio;
            canvas.getContext(""2d"").scale(ratio, ratio);
        }});
    }}

	function validateSignature_{ClientID}(source, arguments) {{
        var valid = !signaturePad_{ClientID}.isEmpty();
        if ( valid ) {{
            $('#signature-pad_{ClientID}').closest('.required').removeClass('has-error');
        }} else {{
            $('#signature-pad_{ClientID}').closest('.required').addClass('has-error');
        }}
		arguments.IsValid = valid;
	}}

    $(function () {{
        $(window).on('resize',
            function () {{
                resizeCanvas_{ClientID}();
            }});
    }});

    Sys.Application.add_load(function () {{

        setTimeout(function () {{

            resizeCanvas_{ClientID}();

            $('canvas.signature-pad_{ClientID}').each(function () {{
                var canvas = $(this)[0];
                signaturePad_{ClientID} = new SignaturePad(canvas,
                {{
                    backgroundColor:'rgb(255, 255, 255)', // necessary for saving image as JPEG; can be removed if only saving as PNG or SVG
                    onEnd: function() {{
                        $('.js-signature-file_{ClientID}').val(signaturePad_{ClientID}.toDataURL(""image/svg+xml""));
                    }}
                }});
                var imgData = $('.js-signature-file_{ClientID}').val();
                if ( imgData && imgData != '' ) {{
                    signaturePad_{ClientID}.fromDataURL(imgData);
                }}
            }});

        }}, 0);

        $('a.js-signature-clear_{ClientID}').click( function () {{
            signaturePad_{ClientID}.clear();
            $('.js-signature-file_{ClientID}').val('');
            return false;
        }});

    }});
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "signature-script-" + this.ClientID, script, true );

            // add script on demand only when there will be a signature box rendered
            if ( ScriptManager.GetCurrent( this.Page ).IsInAsyncPostBack )
            {
                ScriptManager.RegisterClientScriptInclude( this.Page, this.Page.GetType(), "signature-pad", ( (RockPage)this.Page ).ResolveRockUrl( "~/Scripts/rocks_pillars/signature_pad.min.js", false ) );
            }

        }
    }
}
