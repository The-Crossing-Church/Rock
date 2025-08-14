
using System.Collections.Generic;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Field;
using Rock.Model;
using Rock.Utility;
using Rock.Web.UI.Controls;

namespace com._9embers.FieldTypes
{
    /// <summary>
    /// Field Type to select multiple groups
    /// Stored as comma seaparated list of Group.Guid
    /// </summary>
    [RockPlatformSupport( RockPlatform.WebForms, RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M1.52,10.6l3-1.27a.86.86,0,0,1,1,.24l1.21,1.48a9.59,9.59,0,0,0,4.36-4.36L9.58,5.48a.85.85,0,0,1-.25-1l1.27-3a.87.87,0,0,1,1-.5l2.76.64a.85.85,0,0,1,.66.83A12.52,12.52,0,0,1,2.49,15a.85.85,0,0,1-.83-.66L1,11.58A.87.87,0,0,1,1.52,10.6Z""/></svg>" )]
    public class PhoneNumberWithCountryCodeFieldType : Rock.Field.FieldType
    {

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
            string formattedValue = string.Empty;

            if ( value.IsNotNullOrWhiteSpace() )
            {
                var valueArray = value.Split( '|' );
                if ( valueArray.Length == 2 )
                {
                    formattedValue = PhoneNumber.FormattedNumber( valueArray[0], valueArray[1], true );
                }
            }

            return base.FormatValue( parentControl, formattedValue, configurationValues, condensed );
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
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new PhoneNumberBox { ID = id };
        }

        /// <summary>
        /// Reads new values entered by the user for the field (as id)
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control is PhoneNumberBox phoneNumberBox )
            {
                return $"{phoneNumberBox.CountryCode}|{PhoneNumber.CleanNumber(phoneNumberBox.Number)}";
            }

            return null;
        }

        /// <summary>
        /// Sets the value (as id)
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value.IsNotNullOrWhiteSpace() )
            {
                if ( control is PhoneNumberBox phoneNumberBox )
                {
                    var valueArray = value.Split( '|' );
                    if ( valueArray.Length == 2 )
                    {
                        phoneNumberBox.CountryCode = valueArray[0];
                        phoneNumberBox.Number = PhoneNumber.FormattedNumber( valueArray[0], valueArray[1] );
                    }
                }
            }
        }

        #endregion

    }
}
