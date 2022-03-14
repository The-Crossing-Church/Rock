using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Z.EntityFramework.Plus;
using Newtonsoft.Json;

namespace RockWeb.Plugins.com_thecrossingchurch.Cms
{
    [DisplayName( "Org Chart View" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Display's org chart for selected group" )]
    [TextField( "Primary Overseer Key", "Attribute key for the primary overseer attribute on the groups", true, "PrimaryOverseer", order: 1 )]
    [TextField( "Secondary Overseer Key", "Attribute key for the secondary overseer attribute on the groups", true, "SecondaryOverseer", order: 2 )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Person Attributes", "Attributes to include", required: false, allowMultiple: true, order: 3 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 5 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "", "", order: 6 )]

    public partial class OrgChart : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext _context { get; set; }
        private Group parent { get; set; }
        private string PrimaryOverseerKey { get; set; }
        private string SecondaryOverseerKey { get; set; }
        private List<string> AttributeKeys { get; set; }
        #endregion

        #region Base Control Methods

        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent( this.Page );
        }

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            _context = new RockContext();
            int? group_id = PageParameter( "GroupId" ).AsIntegerOrNull();
            PrimaryOverseerKey = GetAttributeValue( "PrimaryOverseerKey" );
            SecondaryOverseerKey = GetAttributeValue( "SecondaryOverseerKey" );
            AttributeKeys = new List<string>();
            List<Guid?> personAttrGuids = GetAttributeValues( "PersonAttributes" ).AsGuidOrNullList();
            for ( int i = 0; i < personAttrGuids.Count(); i++ )
            {
                if ( personAttrGuids[i].HasValue )
                {
                    string key = new AttributeService( _context ).Get( personAttrGuids[i].Value ).Key;
                    AttributeKeys.Add( key );
                }
            }
            if ( group_id.HasValue && group_id.Value > 0 && !String.IsNullOrEmpty( PrimaryOverseerKey ) && !String.IsNullOrEmpty( SecondaryOverseerKey ) )
            {
                parent = new GroupService( _context ).Get( group_id.Value );

                Data d = BuildDepartment( parent );

                parent.LoadAttributes();
                string pos = parent.GetAttributeValue( PrimaryOverseerKey );
                string sos = parent.GetAttributeValue( SecondaryOverseerKey );
                if ( !String.IsNullOrEmpty( pos ) || !String.IsNullOrEmpty( sos ) )
                {
                    //build the parent data for this and tack it on
                    Data item = new Data()
                    {
                        Id = "g" + parent.ParentGroupId,
                        Name = parent.ParentGroup.Name,
                        Type = "Group",
                        Children = new List<Data>()
                    };
                    List<GroupMember> overseers = parent.ParentGroup.Members.Where( gm => gm.Person.PrimaryAlias.Guid.ToString() == pos || gm.Person.PrimaryAlias.Guid.ToString() == sos ).ToList();
                    for ( int i = 0; i < overseers.Count(); i++ )
                    {
                        Data person = new Data()
                        {
                            Id = "p" + overseers[i].Id,
                            Name = overseers[i].Person.FullName,
                            Type = "Person",
                            Children = new List<Data>(),
                            PhotoUrl = overseers[i].Person.PhotoUrl
                        };
                        overseers[i].LoadAttributes();
                        overseers[i].AttributeValues.Add( "IsLeader", new Rock.Web.Cache.AttributeValueCache() { Value = overseers[i].GroupRole.IsLeader.ToString() } );
                        overseers[i].AttributeValues.Add( "PersonId", new Rock.Web.Cache.AttributeValueCache() { Value = overseers[i].PersonId.ToString() } );
                        overseers[i].AttributeValues.Add( "Email", new Rock.Web.Cache.AttributeValueCache() { Value = overseers[i].Person.Email } );
                        var phone = overseers[i].Person.GetPhoneNumber( Guid.Parse( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );
                        if ( phone != null )
                        {
                            overseers[i].AttributeValues.Add( "Mobile", new Rock.Web.Cache.AttributeValueCache() { Value = phone.NumberFormatted } );
                        }
                        overseers[i].Person.LoadAttributes();
                        for ( int k = 0; k < AttributeKeys.Count(); k++ )
                        {
                            overseers[i].AttributeValues.Add( AttributeKeys[k], new Rock.Web.Cache.AttributeValueCache() { Value = overseers[i].Person.GetAttributeValue( AttributeKeys[k] ) } );
                        }
                        person.JsonData = JsonConvert.SerializeObject( overseers[i].AttributeValues );


                        person.Children.Add( d );
                        item.Children.Add( person );
                    }
                    d = item;
                }

                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "data", JsonConvert.SerializeObject( d ) );

                lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
            }
        }

        #endregion

        #region Methods
        private Data BuildDepartment( Group grp )
        {
            Data item = new Data()
            {
                Id = "g" + grp.Id,
                Name = grp.Name,
                Type = "Group",
                Children = new List<Data>()
            };

            //Build SubDepartments
            List<Group> ChildGroups = new GroupService( _context ).Queryable().Where( g => g.ParentGroupId == grp.Id ).ToList().Where( g =>
            {
                g.LoadAttributes();
                string pos = g.GetAttributeValue( PrimaryOverseerKey );
                return String.IsNullOrEmpty( pos );
            } ).ToList();
            for ( int i = 0; i < ChildGroups.Count(); i++ )
            {
                item.Children.Add( BuildDepartment( ChildGroups[i] ) );
            }

            //Build Staff 
            List<GroupMember> Members = grp.Members.ToList();
            bool hasMultipleRoles = false;
            List<int> Roles = Members.Select( gm => gm.GroupRole.Id ).Distinct().ToList();
            if ( Roles.Count() > 1 )
            {
                hasMultipleRoles = true;
            }
            if ( !hasMultipleRoles )
            {
                for ( int i = 0; i < Members.Count(); i++ )
                {
                    item.Children.Add( BuildPerson( Members[i], grp.Id ) );
                }
            }
            else
            {
                List<GroupMember> Leaders = Members.Where( gm => gm.GroupRole.IsLeader ).ToList();
                for ( int i = 0; i < Leaders.Count(); i++ )
                {
                    item.Children.Add( BuildPerson( Leaders[i], grp.Id ) );
                }
            }

            return item;
        }

        private Data BuildPerson( GroupMember gm, int groupId )
        {
            Data item = new Data()
            {
                Id = "p" + gm.Id,
                Name = gm.Person.FullName,
                Type = "Person",
                Children = new List<Data>(),
                PhotoUrl = gm.Person.PhotoUrl
            };

            gm.LoadAttributes();
            gm.AttributeValues.Add( "IsLeader", new Rock.Web.Cache.AttributeValueCache() { Value = gm.GroupRole.IsLeader.ToString() } );
            gm.AttributeValues.Add( "PersonId", new Rock.Web.Cache.AttributeValueCache() { Value = gm.PersonId.ToString() } );
            gm.AttributeValues.Add( "Email", new Rock.Web.Cache.AttributeValueCache() { Value = gm.Person.Email } );
            var phone = gm.Person.GetPhoneNumber( Guid.Parse( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );
            if ( phone != null )
            {
                gm.AttributeValues.Add( "Mobile", new Rock.Web.Cache.AttributeValueCache() { Value = phone.NumberFormatted } );
            }
            gm.Person.LoadAttributes();
            for ( int k = 0; k < AttributeKeys.Count(); k++ )
            {
                gm.AttributeValues.Add( AttributeKeys[k], new Rock.Web.Cache.AttributeValueCache() { Value = gm.Person.GetAttributeValue( AttributeKeys[k] ) } );
            }
            item.JsonData = JsonConvert.SerializeObject( gm.AttributeValues );

            //Build Direct Reports
            if ( gm.GroupRole.IsLeader )
            {
                List<GroupMember> Reports = new GroupMemberService( _context ).Queryable().Where( cgm => cgm.GroupId == groupId && !cgm.GroupRole.IsLeader ).ToList();
                for ( int i = 0; i < Reports.Count(); i++ )
                {
                    item.Children.Add( BuildPerson( Reports[i], groupId ) );
                }
            }

            //Build SubDepartments
            List<Group> ChildGroups = new GroupService( _context ).Queryable().Where( g => g.ParentGroupId == groupId ).ToList().Where( g =>
            {
                g.LoadAttributes();
                string pos = g.GetAttributeValue( PrimaryOverseerKey );
                string sos = g.GetAttributeValue( SecondaryOverseerKey );
                return pos == gm.Person.PrimaryAlias.Guid.ToString() || sos == gm.Person.PrimaryAlias.Guid.ToString();
            } ).ToList();

            for ( int i = 0; i < ChildGroups.Count(); i++ )
            {
                item.Children.Add( BuildDepartment( ChildGroups[i] ) );
            }

            return item;
        }
        #endregion

        #region Classes
        [DotLiquid.LiquidType( "Id", "Name", "Type", "PhotoUrl", "Children", "JsonData" )]
        private class Data
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string PhotoUrl { get; set; }
            public List<Data> Children { get; set; }
            public string JsonData { get; set; }
        }
        #endregion
    }
}