using System;
using Rock.Attribute;

namespace com._9embers.FieldTypes
{
    /// <summary>
    /// Field Attribute for setting a phone number with country code
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class PhoneNumberWithCountryCodeFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public PhoneNumberWithCountryCodeFieldAttribute( string name, string description = "", bool required = true, double defaultValue = double.MinValue, string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue == double.MinValue ? "" : defaultValue.ToString(), category, order, key, typeof( PhoneNumberWithCountryCodeFieldType ).FullName )
        {
        }
    }
}
