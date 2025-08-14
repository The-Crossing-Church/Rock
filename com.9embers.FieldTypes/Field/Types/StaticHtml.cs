//
// Copyright (C) 9 Embers. - All Rights Reserved
//
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Field;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace com._9embers.FieldTypes
{
    /// <summary>
    /// Field used to save and display a named location (room) picker
    /// </summary>
    public class StaticHtml : FieldType
    {
        #region Configuration

        private const string CONTENT = "content";

        private const string SCRIPT = @"
    Sys.Application.add_load(function () {
        $('.js-9embers-static-html').each( function() {
            var $formGroup = $(this).closest('.form-group');
            $formGroup.find('label').remove();
            $formGroup.find('div').removeClass('control-wrapper form-control-static');
            $formGroup.removeClass('form-group static-control');
        });
    });
";

        /// <summary>
        /// Returns a list of the configuration keys.
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( CONTENT );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var ceContent = new CodeEditor();
            controls.Add( ceContent );
            ceContent.Label = "Content";
            ceContent.EditorTheme = CodeEditorTheme.Rock;
            ceContent.EditorMode = CodeEditorMode.Html;
            ceContent.AutoPostBack = true;
            ceContent.TextChanged += OnQualifierUpdated;

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( CONTENT, new ConfigurationValue( "Content", "The static content to display.", string.Empty ) );

            if ( controls != null )
            {
                CodeEditor ceContent = controls.Count > 0 ? controls[0] as CodeEditor : null;

                if ( ceContent != null )
                {
                    configurationValues[CONTENT].Value = ceContent.Text;
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && configurationValues != null )
            {
                CodeEditor ceContent = controls.Count > 0 ? controls[0] as CodeEditor : null;
                if ( ceContent != null )
                {
                    ceContent.Text = configurationValues.GetValueOrNull( CONTENT );
                }
            }
        }

        #endregion

        #region EntityQualifierConfiguration

        /// <summary>
        /// Gets the configuration values for this field using the EntityTypeQualiferColumn and EntityTypeQualifierValues
        /// </summary>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <returns></returns>
        public Dictionary<string, Rock.Field.ConfigurationValue> GetConfigurationValuesFromEntityQualifier( string entityTypeQualifierColumn, string entityTypeQualifierValue )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( CONTENT, new ConfigurationValue( "Content", "The static content to display.", string.Empty ) );
            return configurationValues;
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Formats the value.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( parentControl != null )
            {
                ScriptManager.RegisterStartupScript( parentControl, parentControl.GetType(), "9embers-static-html", SCRIPT, true );

                if ( configurationValues != null && configurationValues.ContainsKey( CONTENT ) )
                {
                    var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( parentControl.RockBlock()?.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
                    return $"<div class='js-9embers-static-html'>{configurationValues[CONTENT].Value.ResolveMergeFields( mergeObjects )}</div>";
                }
            }

            return string.Empty;
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value ( as Guid )
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var control = new LiteralControl { ID = id };
            if ( configurationValues != null && configurationValues.ContainsKey( CONTENT ) )
            {
                var mergeObjects = Rock.Lava.LavaHelper.GetCommonMergeFields( control?.RockBlock()?.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
                control.Text = $"<div class='js-9embers-static-html'>{configurationValues[CONTENT].Value.ResolveMergeFields( mergeObjects )}</div>";
            }

            control.Init += Control_Init;
            control.PreRender += Control_PreRender;
            return control;
        }

        private void Control_Init( object sender, EventArgs e )
        {
            if ( sender is Control control )
            {
                ScriptManager.RegisterStartupScript( control, control.GetType(), "9embers-static-html", SCRIPT, true );
            }
        }

        private void Control_PreRender( object sender, EventArgs e )
        {
            if ( sender is WebControl control )
            {
                if ( ScriptManager.GetCurrent( control.Page ).IsInAsyncPostBack )
                {
                    ScriptManager.RegisterStartupScript( control, control.GetType(), "9embers-static-html", SCRIPT, true );
                }
            }
        }

        /// <summary>
        /// Reads new values entered by the user for the field ( as Guid )
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return string.Empty;
        }

        /// <summary>
        /// Sets the value. ( as Guid )
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
        }

        #endregion

    }
}
