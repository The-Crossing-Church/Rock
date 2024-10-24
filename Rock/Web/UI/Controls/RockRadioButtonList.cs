﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.ComponentModel;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:RockRadioButtonList runat=server></{0}:RockRadioButtonList>" )]
    public class RockRadioButtonList : RadioButtonList, IRockControl
    {
        #region IRockControl implementation (NOTE: uses a different Required property than other IRockControl controls)

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
        /// Gets or sets a value indicating whether to show the none option.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show none option]; otherwise, <c>false</c>.
        /// </value>
        [Bindable( true )]
        [Category( "Behavior" )]
        [DefaultValue( "true" )]
        [Description( "If a selection is not required should a 'None' option should be shown? This setting will not affect the control if it is required." )]
        public bool ShowNoneOption
        {
            get
            {
                return ViewState["ShowNoneOption"] as bool? ?? true;
            }

            set
            {
                ViewState["ShowNoneOption"] = value;
                if ( value && Required == false )
                {
                    this.Items.Insert( 0, new ListItem( "None", string.Empty ) );
                }
                else
                {
                    var li = this.Items.FindByValue( string.Empty );
                    if ( li != null )
                    {
                        this.Items.Remove( li );
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [Bindable( true )]
        [Category( "Behavior" )]
        [DefaultValue( "false" )]
        [Description( "Is the value required?" )]
        public bool Required
        {
            get { return ViewState["Required"] as bool? ?? false; }

            set
            {
                var li = this.Items.FindByValue( string.Empty );

                if ( value )
                {
                    if ( li != null )
                    {
                        this.Items.Remove( li );
                    }
                }
                else
                {
                    if ( li == null && ShowNoneOption == true )
                    {
                        this.Items.Insert( 0, new ListItem( "None", string.Empty ) );
                    }
                }

                ViewState["Required"] = value;
            }
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
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }
            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
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
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
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
        /// Gets or sets the group of controls for which the control that is derived from the <see cref="T:System.Web.UI.WebControls.ListControl" /> class causes validation when it posts back to the server.
        /// </summary>
        /// <returns>The group of controls for which the derived <see cref="T:System.Web.UI.WebControls.ListControl" /> causes validation when it posts back to the server. The default is an empty string ("").</returns>
        public override string ValidationGroup
        {
            get
            {
                return base.ValidationGroup;
            }
            set
            {
                base.ValidationGroup = value;
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ValidationGroup = value;
                }
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RockRadioButtonList"/> class.
        /// </summary>
        public RockRadioButtonList()
            : base()
        {
            RequiredFieldValidator = new RequiredFieldValidator();
            RequiredFieldValidator.ValidationGroup = this.ValidationGroup;
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains event data.</param>
        protected override void OnLoad( System.EventArgs e )
        {
            base.OnLoad( e );

            // If a radiobutton list has no items selected, then there is not anything included in
            // the control state for this control, so on postback a value is not set, and the
            // creation of child controls will not be called.  this presents a problem when using
            // validation since it is during the CreateChildControls that the validator is rewired
            // to the control.  Because of this, always ensure child controls on a postback
            if (Page.IsPostBack)
            {
                EnsureChildControls();
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

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            StringBuilder cssClassBuilder = new StringBuilder( "controls js-rockradiobuttonlist rockradiobuttonlist" );
            if ( this.RepeatDirection == RepeatDirection.Horizontal )
            {
                cssClassBuilder.Append( " rockradiobuttonlist-horizontal" );

                if ( this.RepeatColumns > 0 )
                {
                    cssClassBuilder.Append( string.Format( " in-columns in-columns-{0}", RepeatColumns ) );
                }

            }
            else
            {
                cssClassBuilder.Append( " rockradiobuttonlist-vertical" );
            }

            writer.AddAttribute( "class", cssClassBuilder.ToString() );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            base.RenderControl( writer );

            writer.RenderEndTag();
        }

    }
}