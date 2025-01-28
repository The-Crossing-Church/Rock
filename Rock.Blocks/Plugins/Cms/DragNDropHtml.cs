using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using RestSharp;
using Newtonsoft.Json;
using Rock.Web.UI.Controls;
using System.ComponentModel;

namespace Rock.Blocks.Plugins.Cms
{
    [DisplayName( "HTML Drag 'n Drop" )]
    [Category( "Obsidian > Plugin > CMS" )]
    [Description( "A block for HTML editing within a GUI" )]
    [IconCssClass( "fas fa-code" )]
    #region BlockAttributes
    [LavaCommandsField( "Enabled Lava Commands", "", false, key: AttributeKey.LavaCommands )]
    #endregion
    internal class DragNDropHtml : RockObsidianBlockType
    {
        #region Keys
        private static class AttributeKey
        {
            public const string LavaCommands = "LavaCommands";
        }
        #endregion

        #region Properties
        #endregion

        #region Obsidian Block Type Overrides
        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetObsidianBlockInitialization()
        {
            DragNDropHtmlViewModel viewModel = new DragNDropHtmlViewModel();
            return viewModel;
        }
        #endregion

        #region Block Actions

        #endregion

        #region Helpers
        private class DragNDropHtmlViewModel
        {

        }
        #endregion
    }
}
