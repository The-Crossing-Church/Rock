// <copyright>
// Copyright Pillars Inc.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Web.UI;

using Rock;
using Rock.Field;
using Rock.Security;
using Rock.Web.UI.Controls;
using rocks.pillars.Web.UI.Controls;

namespace rocks.pillars.SignatureField.Field.Types
{
    /// <summary>
    /// Signature Field Type
    /// </summary>
    /// <seealso cref="Rock.Field.FieldType" />
    [Serializable]
    class SignatureFieldType : FieldType
    {
        #region Configuration

        private const string EDITOR_HEIGHT = "editorHeight";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( EDITOR_HEIGHT );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var nbHeight = new NumberBox();
            controls.Add( nbHeight );
            nbHeight.NumberType = System.Web.UI.WebControls.ValidationDataType.Integer;
            nbHeight.AutoPostBack = true;
            nbHeight.TextChanged += OnQualifierUpdated;
            nbHeight.Label = "Editor Height";
            nbHeight.Help = "The height of the control in pixels";

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
            configurationValues.Add( EDITOR_HEIGHT, new ConfigurationValue( "Editor Height", "The height of the control in pixels.", "200" ) );

            if ( controls != null && controls.Count >= 1 )
            {
                if ( controls[0] != null && controls[0] is NumberBox )
                {
                    configurationValues[EDITOR_HEIGHT].Value = ( (NumberBox)controls[0] ).Text;
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
            if ( controls != null && controls.Count >= 1 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is NumberBox && configurationValues.ContainsKey( EDITOR_HEIGHT ) )
                {
                    ( (NumberBox)controls[0] ).Text = configurationValues[EDITOR_HEIGHT].Value;
                }
            }
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var editor = new SignatureBox { ID = id };

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( EDITOR_HEIGHT ) )
                {
                    editor.EditorHeight = configurationValues[EDITOR_HEIGHT].Value.ToString();
                }
            }

            return editor;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is SignatureBox )
            {
                return Encryption.EncryptString( ( (SignatureBox)control ).Value );
            }
            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control != null && control is SignatureBox )
            {
                ( (SignatureBox)control ).Value = Encryption.DecryptString( value );
            }
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string signatureUrl = Encryption.DecryptString( value );
            if ( signatureUrl.IsNotNullOrWhiteSpace() )
            {
                return $"<img src='{signatureUrl}' style='width:100%; height:auto'>";
            }
            return string.Empty;
        }

        #endregion
    }
}