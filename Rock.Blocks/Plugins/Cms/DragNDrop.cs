using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Blocks.Plugins.Cms
{
    internal class DragNDrop : RockObsidianBlockType
    {
        #region Obsidian Block Type Overrides
        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetObsidianBlockInitialization()
        {
            DragNDropViewModel viewModel = new DragNDropViewModel();
            return viewModel;
        }
        #endregion
        #region Helper Classes
        private class DragNDropViewModel
        {
            public string JSON { get; set; }
        }
        #endregion
    }
}
