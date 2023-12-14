using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_9embers.Communication
{
    [DisplayName( "Comunication List Segments" )]
    [Category( "com_9embers > Communication" )]
    [Description( "Creates a communication based on a communication list and allows for additional filtering by person attributes." )]

    #region Block Attributes

    [AttributeField( "Attribute Filter Attribute",
        Description = "Attribute used to filter recipients by person attributes.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST,
        Order = 0,
        Key = AttributeKey.AttributeFilterAttribute
        )]

    [AttributeField( "Property Filter Attribute",
        Description = "Attribute used to filter recipients by person properties.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST,
        Order = 1,
        Key = AttributeKey.PropertyFilterAttribute
        )]

    [AttributeField( "Can Send To Parents Attribute",
        Description = "Attribute used to determine if sending to parents is allowed.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST,
        Order = 2,
        Key = AttributeKey.CanSendToParentsAttribute
        )]

    [AttributeField( "Hide In Segments Attribute",
        Description = "Attribute used to determine if group should an option in the communication list dropdown.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST,
        Order = 3,
        Key = AttributeKey.HideInSegmentsAttribute
        )]

    [AttributeField( "Parents Group Attribute",
        Description = "Attribute used to manage if a parent can recieve an email.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST,
        Order = 4,
        Key = AttributeKey.ParentsGroupAttribute
        )]

    [BooleanField( "Show Registration Template Filter",
        "If you have a lot of registration instances, you can optionally display a template filter to narrow down list of instances.",
        false,
        Order = 5,
        Key = AttributeKey.ShowRegistrationTemplate
        )]

    #endregion Block Attributes

    public partial class CommunicationListSegments : Rock.Web.UI.RockBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string AttributeFilterAttribute = "AttributeFilterAttribute";
            public const string PropertyFilterAttribute = "PropertyFilterAttribute";
            public const string CanSendToParentsAttribute = "CanSendToParentsAttribute";
            public const string HideInSegmentsAttribute = "HideInSegmentsAttribute";
            public const string ParentsGroupAttribute = "ParentsGroupAttribute";
            public const string ShowRegistrationTemplate = "ShowRegistrationTemplate";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
        }

        #endregion PageParameterKeys

        #region Fields

        private SelectionState _selectionState;

        private RockLiteralField _cellPhoneField = null;

        #endregion

        #region Properties


        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gMovement.GridRebind += GMovement_GridRebind;

        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            if ( ViewState["ListId"] != null )
            {
                UpdateFilters( ( int ) ViewState["ListId"] );
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindCommunicationListDropdown();

                if ( PageParameter( "Restore" ).AsBoolean() )
                {
                    var selectionState = Session["CommunicationListSegmentsSelection"] as SelectionState;
                    if ( selectionState != null )
                    {
                        SetSelection( selectionState );
                    }
                }
            }
        }

        #endregion

        #region Events
        protected void ddlCommunicationList_SelectedIndexChanged( object sender, EventArgs e )
        {
            var groupId = ddlCommunicationList.SelectedValueAsId();

            ShowCommunicationFields();
            gMovement.GridRebind += GMovement_GridRebind;
        }

        // Use as an exampe of event listener for drp and cbl
        protected void btnGenerate_Click( object sender, EventArgs e )
        {
            GenerateEmailList();
        }

        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        // Rebind the grid
        private void GMovement_GridRebind( object sender, GridRebindEventArgs e )
        {
            ShowGrid();
        }

        #endregion

        #region Methods

        private void BindCommunicationListDropdown()
        {
            if ( CurrentPerson == null )
            {
                return;
            }

            RockContext rockContext = new RockContext();
            GroupService groupService = new GroupService( rockContext );

            var communicationListGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid();

            var groups = groupService.Queryable()
                .Where( g => g.IsActive && !g.IsArchived
                        && g.GroupType.Guid == communicationListGroupTypeGuid )
                .ToList();

            groups = groups
                .Where( g => g.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                .OrderBy( g => g.Name )
                .ToList();

            var hideInListAttribute = AttributeCache.Get( GetAttributeValue( AttributeKey.HideInSegmentsAttribute ) );

            if ( hideInListAttribute != null )
            {
                groups.ForEach( g => g.LoadAttributes() );
                groups = groups.Where( g => g.GetAttributeValue( hideInListAttribute.Key ).AsBoolean() != true ).ToList();
            }

            ddlCommunicationList.DataSource = groups;
            ddlCommunicationList.DataBind();

            ddlCommunicationList.Items.Insert( 0, "" );
        }

        private void UpdateFilters( int groupId )
        {
            RockContext rockContext = new RockContext();
            Group group = new GroupService( new RockContext() ).Get( groupId );
            group.LoadAttributes();

            if ( group == null )
            {
                return;
            }

            var communicationSegmentGuids = group.GetAttributeValue( "CommunicationSegments" ).SplitDelimitedValues().AsGuidList();
            var dataViews = new DataViewService( rockContext ).GetByGuids( communicationSegmentGuids ).ToList();
            
            var totals = new List<string>();

            if ( dataViews.Any() )
            {                
                //Add all the dataviews into the check box list
                cblSegments.Visible = true;
                cblSegments.DataSource = dataViews.OrderBy( d => d.Name ).ToList();
                cblSegments.DataBind();

                foreach ( ListItem item in cblSegments.Items )
                {
                    int dataViewId = item.Value.AsInteger();
                    var dataviewService = new DataViewService( rockContext );
                    var dataviewQry = dataviewService.Queryable().Where(d => d.Id == dataViewId );
                    var dataviewList = dataviewQry.ToList();
                    String total = "";
                    foreach (DataView d in dataviewList )
                    {
                        var qry = d.GetQuery( new DataViewGetQueryArgs
                        {
                            DatabaseTimeoutSeconds = 20
                        } );

                        if ( qry != null )
                        {
                            total = d.Name + ' ' + qry.Count().ToString();
                        }
                    }
                    
                    item.Text = total;
                }

            }
            else
            {
                cblSegments.Items.Clear();
                cblSegments.Visible = false;
            }

            // Now Calculate the Segments Summary
            SetActivitySummaryList( dataViews, rockContext );

            // Reind the Grid
            //gMovement.GridRebind += GMovement_GridRebind;
            ShowGrid();

        }

        // This is for the CBL Segment Summary
        private void SetActivitySummaryList( List<DataView> dataViews, RockContext rockContext)
        {
            //Add all the dataviews into the check box list
            cblSegmentSummary.Visible = true;
            cblSegmentSummary.DataSource = dataViews.OrderBy( d => d.Name ).ToList();
            cblSegmentSummary.DataBind();
            

            // loop through the check box items to get the dataview each one represents. 
            foreach ( ListItem item in cblSegmentSummary.Items )
            {
                int dataViewId = item.Value.AsInteger();
                var dataviewService = new DataViewService( rockContext );
                var dataviewQry = dataviewService.Queryable().Where( d => d.Id == dataViewId );
                var dataviewList = dataviewQry.ToList();
                String total = "";
                foreach ( DataView d in dataviewList )
                {
                    var qry = d.GetQuery( new DataViewGetQueryArgs
                    {
                        DatabaseTimeoutSeconds = 20
                    } );

                    if ( qry != null )
                    {
                        total = d.Name + ' ' + qry.Count().ToString();
                    }
                }

                item.Text = total;
            }
        }

        private void ShowGrid()
        {
            _cellPhoneField = gMovement.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lCellPhone" ).FirstOrDefault();
            gMovement.SetLinqDataSource( GetCommunicationQry().OrderBy( p => p.FirstName ).ThenBy( p => p.LastName ) );
            gMovement.DataBind();
            pnlGrid.Visible = true;
        }


        private void SetSelection( SelectionState selectionState )
        {
            if ( selectionState != null && selectionState.CommunicationId.HasValue )
            { 
                int commGroupId = selectionState.CommunicationId.Value;
                ddlCommunicationList.SelectedValue = commGroupId.ToString();

                ShowCommunicationFields();

                cblSegmentSummary.SetValues( selectionState.SegmentIds );

                RockContext rockContext = new RockContext();
                var group = new GroupService( rockContext ).Get( ddlCommunicationList.SelectedValueAsId() ?? 0 );

            }
        }

        private void ShowCommunicationFields()
        {
            var groupId = ddlCommunicationList.SelectedValueAsId();
            ViewState["ListId"] = groupId;           
            cblSegments.Visible = groupId.HasValue;
            pnlSummary.Visible = groupId.HasValue;
            cblSegmentSummary.Visible = groupId.HasValue;
            pnlGrid.Visible = groupId.HasValue;
            SaveViewState();
            UpdateFilters( groupId ?? 0 );
        }

        private void GenerateEmailList()
        {
            RockContext rockContext = new RockContext();
            CommunicationService communicationService = new CommunicationService( rockContext );

            var communication = new Rock.Model.Communication
            {
                IsBulkCommunication = true,
                Status = CommunicationStatus.Transient,
                SenderPersonAliasId = CurrentPersonAliasId
            };

            var recipientPersons = GetCommunicationQry().ToList();

            foreach ( var person in recipientPersons )
            {
                communication.Recipients.Add( new CommunicationRecipient() { PersonAliasId = person.PrimaryAliasId.Value } );
            }

            communicationService.Add( communication );
            rockContext.SaveChanges();

            Session["CommunicationListSegmentsSelection"] = _selectionState;

            Response.Redirect( $"/Communication/{communication.Id}?Segments=true" );
        }

        private IQueryable<Person> GetCommunicationQry()
        {
            RockContext rockContext = new RockContext();
            var group = new GroupService( rockContext ).Get( ddlCommunicationList.SelectedValueAsId() ?? 0 );
            if ( group == null )
            {
                return null;
            }

            _selectionState = new SelectionState
            {
                CommunicationId = group.Id //,
                //DateRangeDates = drpSummaryDate.DateRange

            };

            var personService = new PersonService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );

            var qry = groupMemberService.Queryable()
                .Where( gm =>
                    gm.GroupId == group.Id &&
                    gm.GroupMemberStatus == GroupMemberStatus.Active &&
                    gm.IsArchived == false
                )
                .Select( gm => gm.Person );

            IQueryable<Person> segmentQry = GetSegmentQry( personService, group );

            if ( segmentQry != null )
            {
                qry = qry.Where( p => segmentQry.Select( s => s.Id ).Contains( p.Id ) );
            }

            IQueryable<Person> filterQry = GetFilterQry( personService, group );
            if ( filterQry != null)

            qry = qry.Where( p => filterQry.Select( s => s.Id ).Contains( p.Id ) );

            int adultRoleId = GroupTypeCache.GetFamilyGroupType().Roles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Select( a => a.Id ).FirstOrDefault();
            int childRoleId = GroupTypeCache.GetFamilyGroupType().Roles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Select( a => a.Id ).FirstOrDefault();

            return qry;
        }

        private Group GetParentGroup( Group group, RockContext rockContext )
        {
            var parentsAttribute = AttributeCache.Get( GetAttributeValue( AttributeKey.ParentsGroupAttribute ) );
            if ( parentsAttribute == null )
            {
                return null;
            }

            group.LoadAttributes();
            var parentGroupGuids = group.GetAttributeValue( parentsAttribute.Key ).SplitDelimitedValues();

            if (parentGroupGuids.Length < 2 )
            {
                return null;
            }

            return new GroupService( rockContext ).Get( parentGroupGuids[1].AsGuid() );
        }


        private IQueryable<Person> GetSegmentQry( PersonService personService, Group group )
        {
            if ( cblSegmentSummary.Visible )
            {
                _selectionState.SegmentIds = cblSegmentSummary.SelectedValues;
                

                var personEntityType = EntityTypeCache.Get( typeof( Person ) );

                DataViewService dataViewService = new DataViewService( new RockContext() );
                var dataviews = dataViewService
                    .GetByIds( cblSegmentSummary.SelectedValuesAsInt )
                    .Where( dv => dv.EntityTypeId == personEntityType.Id )
                    .ToList();


                if ( dataviews.Any() )
                {
                    ParameterExpression parameterExpression = personService.ParameterExpression;
                    Expression expression = dataviews[0].GetExpression( personService, parameterExpression );

                    foreach ( var dataview in dataviews.Skip( 1 ).ToList() )
                    {

                        expression = Expression.OrElse( expression, dataview.GetExpression( personService, parameterExpression ) );
                    }
                    MethodInfo getMethod = personService.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( SortProperty ) } );

                    var sortProperty = new SortProperty { Direction = SortDirection.Ascending, Property = "Id" };

                    var getResult = getMethod.Invoke( personService, new object[] { parameterExpression, expression, sortProperty } );
                    return getResult as IQueryable<Person>;

                }
            }
            return null;
        }

        // This should become the date query
        private IQueryable<Person> GetFilterQry( PersonService personService, Group group )
        {
            if ( pnlSummary.Visible )
            {
                _selectionState.DateRangeDates = drpSummaryDate.DateRange;
            }      

            return personService.Queryable();
        }

        protected internal List<string> FixDelimination( List<string> values )
        {
            if ( values.Count() == 1 && values[0].Contains( "[" ) )
            {
                try
                {
                    var jsonValues = JsonConvert.DeserializeObject<List<string>>( values[0] );
                    values[0] = jsonValues.AsDelimited( "," );
                }
                catch { }
            }

            return values;
        }

        #endregion

        #region Classes

        class AttributeFields
        {
            public AttributeCache Attribute { get; set; }
            public EntityField EntityField { get; set; }
        }

        class PropertyFields
        {
            public string Property { get; set; }
            public EntityField EntityField { get; set; }
        }

        [Serializable]
        class SelectionState
        {
            public int? CommunicationId { get; set; }
            public DateRange DateRangeDates { get; set; }
            public List<string> SegmentIds { get; set; }          
         
            public SelectionState()
            {
                DateRangeDates = new DateRange(); 
                SegmentIds = new List<string>();
            }
        }
        #endregion

        protected void gMovement_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            Person person = e.Row.DataItem as Person;
            if ( person == null )
            {
                return;
            }

            var lCellPhone = e.Row.FindControl( _cellPhoneField.ID ) as Literal;
            if ( lCellPhone == null )
            {
                return;
            }

            var pn = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            if ( pn != null )
            {
                lCellPhone.Text = pn.NumberFormatted;
            }

        }
    }
}