// <copyright>
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
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Rock.ViewModels.Controls;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Workflow;
using Rock.Web.Cache;

namespace Rock.Security
{
    /// <summary>
    /// CROSSING (not needed after v18.4): validates free-text values entered through workflow
    /// entry forms to prevent Lava and/or HTML injection. A user-entered value is
    /// stored raw in a workflow/activity attribute and can later be re-resolved as
    /// Lava by downstream actions (which can run the <c>sql</c> command), so entry
    /// forms must not accept Lava (or HTML) unless the specific field is configured
    /// to allow it.
    ///
    /// This is a backport of the shape of core's <c>StringValueValidator</c>
    /// (added upstream after 18.1) narrowed to what the workflow entry blocks need.
    /// When this instance is upgraded to a Rock version that ships the core
    /// framework, this file and its call sites should be removed in favor of it.
    /// </summary>
    public static class WorkflowFormInputValidator
    {
        #region Rules

        /// <summary>
        /// Individual content checks that can be enforced against a value.
        /// </summary>
        [Flags]
        private enum Rule
        {
            None = 0,

            /// <summary>Lava output expressions ( <c>{{ ... }}</c> ).</summary>
            LavaOutput = 0x01,

            /// <summary>Lava tags ( <c>{% ... %}</c> ) and shortcodes ( <c>{[ ... ]}</c> ).</summary>
            LavaCommands = 0x02,

            /// <summary>A <c>&lt;script</c> tag opening.</summary>
            ScriptTags = 0x04,

            /// <summary>The <c>javascript:</c> URL scheme in an attribute/url context.</summary>
            JavascriptProtocol = 0x08,

            /// <summary>HTML event-handler attributes ( <c>onclick=</c>, <c>onerror=</c>, ... ).</summary>
            EventHandlers = 0x10,

            /// <summary>Any HTML tag start ( <c>&lt;</c> followed by a letter, <c>!</c>, or <c>/</c> ).</summary>
            AnyHtmlTags = 0x20,

            /// <summary>ASCII control characters (excluding tab, CR, and LF).</summary>
            ControlCharacters = 0x40,
        }

        #endregion

        #region Fields

        private const RegexOptions DefaultOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;

        private const RegexOptions IgnoreCase = DefaultOptions | RegexOptions.IgnoreCase;

        private static readonly Regex ScriptTagPattern = new Regex( @"<script\b", IgnoreCase );

        private static readonly Regex JavascriptProtocolPattern = new Regex( @"[=""'(]\s*javascript\s*:", IgnoreCase );

        // Explicit list of HTML event-handler attribute names (from the HTML
        // Living Standard) so prose like "online =" is not falsely flagged.
        private static readonly Regex EventHandlerPattern = new Regex(
            @"\bon(?:" +
            @"abort|auxclick|beforeinput|beforematch|beforetoggle|blur|" +
            @"cancel|canplay|canplaythrough|change|click|close|" +
            @"contextlost|contextmenu|contextrestored|copy|cuechange|cut|" +
            @"dblclick|drag|dragend|dragenter|dragleave|dragover|" +
            @"dragstart|drop|durationchange|emptied|ended|error|" +
            @"focus|formdata|input|invalid|keydown|keypress|keyup|" +
            @"load|loadeddata|loadedmetadata|loadstart|mousedown|" +
            @"mouseenter|mouseleave|mousemove|mouseout|mouseover|" +
            @"mouseup|paste|pause|play|playing|progress|ratechange|" +
            @"reset|resize|scroll|securitypolicyviolation|seeked|" +
            @"seeking|select|slotchange|stalled|submit|suspend|" +
            @"timeupdate|toggle|volumechange|waiting|wheel|" +
            @"afterprint|beforeprint|beforeunload|hashchange|" +
            @"languagechange|message|messageerror|offline|online|" +
            @"pagehide|pageshow|popstate|rejectionhandled|storage|" +
            @"unhandledrejection|unload" +
            @")\s*=",
            IgnoreCase );

        private static readonly Regex AnyHtmlTagPattern = new Regex( @"<[a-zA-Z!/]", DefaultOptions );

        private static readonly Regex ControlCharacterPattern = new Regex( "[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", DefaultOptions );

        // Field types whose whole purpose is to hold rich content (HTML / Lava /
        // code). Placing one of these on a form is itself an explicit opt-in to
        // that content, so they are not restricted.
        private static readonly HashSet<string> UnrestrictedFieldTypeGuids = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
        {
            SystemGuid.FieldType.HTML,
            SystemGuid.FieldType.CODE_EDITOR,
            SystemGuid.FieldType.LAVA,
            SystemGuid.FieldType.MARKDOWN,
            SystemGuid.FieldType.STRUCTURE_CONTENT_EDITOR,
        };

        #endregion

        #region Methods

        /// <summary>
        /// Validates a single user-entered value against the content policy for
        /// the attribute's field type and configuration.
        /// </summary>
        /// <param name="attribute">The attribute whose value is being set.</param>
        /// <param name="value">The user-entered value.</param>
        /// <returns>
        /// A human-readable reason the value is not allowed (for example
        /// "may not contain Lava commands"), or <c>null</c> if the value is allowed.
        /// </returns>
        public static string Validate( AttributeCache attribute, string value )
        {
            if ( attribute == null || string.IsNullOrEmpty( value ) )
            {
                return null;
            }

            return Validate( value, GetRules( attribute ) );
        }

        /// <summary>
        /// Validates a raw free-text value that is not backed by an attribute field
        /// type (for example a Person Entry name/email/address field, or a value
        /// seeded from a query-string parameter). Rejects any Lava or HTML, matching
        /// the plain-text policy applied to form fields that have not opted in.
        /// </summary>
        /// <param name="value">The user-supplied value.</param>
        /// <returns>A human-readable reason the value is not allowed, or <c>null</c> if it is allowed.</returns>
        public static string ValidatePlainText( string value )
        {
            if ( string.IsNullOrEmpty( value ) )
            {
                return null;
            }

            return Validate( value, Rule.LavaOutput | Rule.LavaCommands | Rule.AnyHtmlTags | Rule.ControlCharacters );
        }

        /// <summary>
        /// Validates the free-text values of a Person Entry submission (the person,
        /// the spouse, and the address). None of these fields are ever allowed to
        /// contain Lava or HTML.
        /// </summary>
        /// <param name="values">The person entry values from the client.</param>
        /// <returns>
        /// A ready-to-display message naming the first disallowed field, or
        /// <c>null</c> if all values are allowed.
        /// </returns>
        public static string ValidatePersonEntry( PersonEntryValuesBag values )
        {
            if ( values == null )
            {
                return null;
            }

            return ValidatePersonBag( values.Person, string.Empty )
                ?? ValidatePersonBag( values.Spouse, "Spouse " )
                ?? ValidateAddressBag( values.Address );
        }

        private static string ValidatePersonBag( PersonBasicEditorBag person, string labelPrefix )
        {
            if ( person == null )
            {
                return null;
            }

            return ComposeFieldViolation( labelPrefix + "First Name", person.FirstName )
                ?? ComposeFieldViolation( labelPrefix + "Nickname", person.NickName )
                ?? ComposeFieldViolation( labelPrefix + "Last Name", person.LastName )
                ?? ComposeFieldViolation( labelPrefix + "Email", person.Email );
        }

        private static string ValidateAddressBag( AddressControlBag address )
        {
            if ( address == null )
            {
                return null;
            }

            return ComposeFieldViolation( "Address Line 1", address.Street1 )
                ?? ComposeFieldViolation( "Address Line 2", address.Street2 )
                ?? ComposeFieldViolation( "City", address.City )
                ?? ComposeFieldViolation( "State", address.State )
                ?? ComposeFieldViolation( "Postal Code", address.PostalCode )
                ?? ComposeFieldViolation( "Country", address.Country );
        }

        /// <summary>
        /// Runs the plain-text policy against a single labeled value and, on a
        /// violation, returns a ready-to-display message; otherwise <c>null</c>.
        /// </summary>
        public static string ComposeFieldViolation( string fieldLabel, string value )
        {
            var reason = ValidatePlainText( value );

            return reason != null
                ? $"The value entered for \"{fieldLabel}\" {reason}. Please remove it and try again."
                : null;
        }

        /// <summary>
        /// Resolves the rules to enforce for the given attribute based on its
        /// field type and the "Allow HTML" / "Allow Lava" configuration.
        /// </summary>
        private static Rule GetRules( AttributeCache attribute )
        {
            var fieldTypeGuid = attribute.FieldType?.Guid.ToString();

            if ( fieldTypeGuid != null && UnrestrictedFieldTypeGuids.Contains( fieldTypeGuid ) )
            {
                return Rule.None;
            }

            var config = attribute.ConfigurationValues;
            var allowHtml = config != null && config.GetValueOrNull( "allowhtml" ).AsBoolean();
            var allowLava = config != null && config.GetValueOrNull( "allowlava" ).AsBoolean();

            var rules = Rule.None;

            if ( !allowLava )
            {
                rules |= Rule.LavaOutput | Rule.LavaCommands;
            }

            if ( allowHtml )
            {
                // Basic HTML is allowed, but never scripts / JS.
                rules |= Rule.ScriptTags | Rule.JavascriptProtocol | Rule.EventHandlers;
            }
            else
            {
                // No markup of any kind.
                rules |= Rule.AnyHtmlTags | Rule.ControlCharacters;
            }

            return rules;
        }

        /// <summary>
        /// Checks a value against the supplied rule set, returning the reason for
        /// the first violation or <c>null</c> if the value is allowed.
        /// </summary>
        private static string Validate( string value, Rule rules )
        {
            if ( rules == Rule.None )
            {
                return null;
            }

            if ( rules.HasFlag( Rule.LavaCommands ) && ContainsLavaCommands( value ) )
            {
                return "may not contain Lava commands";
            }

            if ( rules.HasFlag( Rule.LavaOutput ) && value.IndexOf( "{{", StringComparison.Ordinal ) >= 0 )
            {
                return "may not contain Lava";
            }

            if ( rules.HasFlag( Rule.ScriptTags ) && ScriptTagPattern.IsMatch( value ) )
            {
                return "may not contain script tags";
            }

            if ( rules.HasFlag( Rule.JavascriptProtocol ) && JavascriptProtocolPattern.IsMatch( value ) )
            {
                return "may not contain JavaScript actions";
            }

            if ( rules.HasFlag( Rule.EventHandlers ) && EventHandlerPattern.IsMatch( value ) )
            {
                return "may not contain JavaScript event handler attributes";
            }

            if ( rules.HasFlag( Rule.AnyHtmlTags ) && AnyHtmlTagPattern.IsMatch( value ) )
            {
                return "may not contain HTML";
            }

            if ( rules.HasFlag( Rule.ControlCharacters ) && ControlCharacterPattern.IsMatch( value ) )
            {
                return "may not contain control characters";
            }

            return null;
        }

        /// <summary>
        /// Determines whether the value contains a Lava tag ( <c>{%</c> ) or a Lava
        /// shortcode ( <c>{[</c> ). This scans character-by-character so it cannot be
        /// evaded by placing a <c>}</c> inside the tag (unlike a naive regex).
        /// </summary>
        private static bool ContainsLavaCommands( string value )
        {
            var length = value.Length - 1;

            for ( int i = 0; i < length; i++ )
            {
                if ( value[i] == '{' )
                {
                    var next = value[i + 1];

                    if ( next == '%' || next == '[' )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}
