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
using System.Collections.Generic;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.SystemGuid;
using Rock.ViewModels.Utility;

namespace com._9embers.FieldTypes
{
    /// <summary>
    /// Field Type to select multiple groups
    /// Stored as comma seaparated list of Group.Guid
    /// </summary>
    [FieldTypeGuid( "97C299A0-E170-4B1E-81D7-3280E209F4E6" )]
    public class GroupsFieldType : UniversalItemTreePickerFieldType
    {
        protected override bool IsMultipleSelection => true;

        protected override List<Rock.ViewModels.Utility.ListItemBag> GetItemBags( IEnumerable<string> values, Dictionary<string, string> privateConfigurationValues )
        {
            using ( var rockContext = new RockContext() )
            {
                return new GroupService( rockContext ).Queryable()
                    .Where( g => values.Contains( g.Guid.ToString() ) )
                    .Select( g => new ListItemBag
                    {
                        Value = g.Guid.ToString(),
                        Text = g.Name
                    } )
                    .ToList();
            }
        }

        protected override string GetRootRestUrl( Dictionary<string, string> privateConfigurationValues )
        {
            return "/api/v2/plugins/com.9embers/fieldtypes/grouptypes/tree";
        }
    }
}