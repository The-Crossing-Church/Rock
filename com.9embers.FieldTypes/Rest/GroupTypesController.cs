using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest;
using Rock.SystemGuid;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;

namespace com._9embers.FieldTypes.Rest
{
    public class GroupTypesController : ApiControllerBase
    {
        [HttpPost]
        [System.Web.Http.Route( "api/v2/plugins/com.9embers/fieldtypes/grouptypes/tree")]
        [RestActionGuid( "0A6C2C94-8C82-40CC-87E9-FC73F2FBCAC3" )]
        public IHttpActionResult PostTreeItems( [FromBody] UniversalItemTreePickerOptionsBag options )
        {
            using (var rockContext = new RockContext())
            {
                var groupService = new GroupService( rockContext );
                var expandGuids = GetExpandGuids( groupService, options.ExpandToValues?.AsGuidList() );
                var groups = LoadGroups( groupService, options.ParentValue.AsGuidOrNull(), expandGuids );
                return Ok( groups );
            }
        }

        private List<Guid> GetExpandGuids( GroupService groupService, List<Guid> expandToGuids )
        {
            var expandGuids = new List<Guid>();
            if (expandToGuids == null)
            {
                return expandGuids;
            }

            foreach (var guid in expandToGuids)
            {
                var group = groupService.Get( guid ).ParentGroup;

                while (group != null)
                {
                    if ( !expandGuids.Contains( group.Guid ) )
                    {
                        expandGuids.Add( group.Guid );
                    }
                    group = group.ParentGroup;
                }
            }

            return expandGuids;
        }

        private List<TreeItemBag> LoadGroups( GroupService groupService, Guid? parentGuid, List<Guid> expandGuids )
        {
            var groupQry = groupService.Queryable()
                .Where( g => g.Name != null && g.Name != string.Empty )
                .Where( g => (parentGuid.HasValue && g.ParentGroup.Guid == parentGuid.Value) ||
                    (!parentGuid.HasValue && !g.ParentGroupId.HasValue && g.GroupType.ShowInNavigation == true) );

            var items = new List<TreeItemBag>();

            foreach( var group in groupQry )
            {
                var item = new TreeItemBag
                {
                    Value = group.Guid.ToString(),
                    Text = group.Name,
                    IsFolder = true,
                    IsActive = group.IsActive,
                    HasChildren = group.Groups.Any()
                };

                if ( expandGuids.Contains( group.Guid ) ) 
                {
                    item.Children = LoadGroups( groupService, group.Guid, expandGuids );
                }

                items.Add( item );
            }

            return items;
        }
    }
}
