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

            gPreview.GridRebind += GPreview_GridRebind;
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
                SetRegistrationOptions();
                BindRegistrationInstanceDropdown();
                BindPreviousCommunications();

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
            ddlSendTo.Visible = SendingToParentsEnabled( groupId );

            ShowCommunicationFields();
        }

        protected void btnGenerate_Click( object sender, EventArgs e )
        {
            GenerateEmailList();
        }
        protected void btnPreview_Click( object sender, EventArgs e )
        {
            ShowPreview();
        }

        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        private void GPreview_GridRebind( object sender, GridRebindEventArgs e )
        {
            ShowPreview();
        }

        protected void rpRegistrationTemplates_SelectItem( object sender, EventArgs e )
        {
            BindRegistrationInstanceDropdown();
        }

        protected void cbIncludeInactive_CheckedChanged( object sender, EventArgs e )
        {
            BindRegistrationInstanceDropdown();
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

        private void SetRegistrationOptions()
        {
            if ( GetAttributeValue( AttributeKey.ShowRegistrationTemplate ).AsBoolean() )
            {
                pnlRegistrationTemplates.Visible = true;

                pnlIncludeExclude.AddCssClass( "col-sm-6" ).AddCssClass( "col-md-4" ).AddCssClass( "col-lg-3" );
                pnlRegistrationTemplates.AddCssClass( "col-sm-6" ).AddCssClass( "col-md-3" ).AddCssClass( "col-lg-3" );
                pnlRegistrationInstances.AddCssClass( "col-sm-6" ).AddCssClass( "col-md-3" ).AddCssClass( "col-lg-4" );
                pnlIncludeInactive.AddCssClass( "col-sm-6" ).AddCssClass( "col-md-2" ).AddCssClass( "col-lg-2" );
            }
            else
            {
                pnlIncludeExclude.AddCssClass( "col-sm-4" ).AddCssClass( "col-md-4" ).AddCssClass( "col-lg-3" );
                pnlRegistrationInstances.AddCssClass( "col-sm-5" ).AddCssClass( "col-md-6" ).AddCssClass( "col-lg-7" );
                pnlIncludeInactive.AddCssClass( "col-sm-3" ).AddCssClass( "col-md-2" ).AddCssClass( "col-lg-2" );
            }


        }

        private void BindRegistrationInstanceDropdown()
        {
            var now = RockDateTime.Now;
            RockContext rockContext = new RockContext();
            RegistrationInstanceService registrationInstanceService = new RegistrationInstanceService( rockContext );

            var registrationInstancesQry = registrationInstanceService.Queryable();

            if ( GetAttributeValue( AttributeKey.ShowRegistrationTemplate ).AsBoolean() )
            {
                var templateId = rpRegistrationTemplates.SelectedValue.AsIntegerOrNull();
                registrationInstancesQry = registrationInstancesQry.Where( ri => templateId.HasValue && templateId.Value == ri.RegistrationTemplateId );
            }

            if ( !cbIncludeInactive.Checked )
            {
                registrationInstancesQry = registrationInstancesQry.Where( ri => ri.IsActive && ri.RegistrationTemplate.IsActive && ri.StartDateTime <= now && ri.EndDateTime >= now );
            }

            var registrationInstances = registrationInstancesQry.OrderBy( r => r.Name ).ToList();

            cblRegistrationInstances.DataSource = registrationInstances;
            cblRegistrationInstances.DataBind();
        }

        private void BindPreviousCommunications()
        {
            RockContext rockContext = new RockContext();
            CommunicationService commService = new CommunicationService( rockContext );

            ddlPrevCommunication.Items.Clear();
            
            if ( CurrentPersonId.HasValue )
            {
                ddlPrevCommunication.DataSource = commService
                    .Queryable().AsNoTracking()
                    .Where( c =>
                        c.CreatedByPersonAlias.PersonId == CurrentPersonId.Value &&
                        c.Status != CommunicationStatus.Transient &&
                        ( ( c.Name != null && c.Name != "" ) || ( c.Subject != null && c.Subject != "" ) )
                    )
                    .OrderByDescending( c => c.CreatedDateTime )
                    .Take( 20 )
                    .ToList()
                    .Select( c => new
                    {
                        c.Id,
                        Name = $"{c.Name ?? c.Subject} ({c.CreatedDateTime.ToElapsedString(false,true)})"
                    } )
                    .ToList();
                ddlPrevCommunication.DataBind();
                ddlPrevCommunication.Items.Insert( 0, new ListItem() );
            }
        }

        private bool SendingToParentsEnabled( int? groupId )
        {
            RockContext rockContext = new RockContext();
            GroupService groupService = new GroupService( rockContext );
            var group = groupService.Get( groupId ?? 0 );
            if ( group == null )
            {
                return false;
            }

            var canSendToParentsAttributeGuid = GetAttributeValue( AttributeKey.CanSendToParentsAttribute ).AsGuid();
            var canSendToParentsAttribute = AttributeCache.Get( canSendToParentsAttributeGuid );

            if ( canSendToParentsAttribute == null )
            {
                return false;
            }

            group.LoadAttributes();
            return group.GetAttributeValue( canSendToParentsAttribute.Key ).AsBoolean();

        }

        private void UpdateFilters( int groupId )
        {
            Group group = new GroupService( new RockContext() ).Get( groupId );
            group.LoadAttributes();

            if ( group == null )
            {
                return;
            }

            var communicationSegmentGuids = group.GetAttributeValue( "CommunicationSegments" ).SplitDelimitedValues().AsGuidList();
            var dataViews = new DataViewService( new RockContext() ).GetByGuids( communicationSegmentGuids ).ToList();

            if ( dataViews.Any() )
            {
                cblSegments.Visible = true;
                cblSegments.DataSource = dataViews.OrderBy( d => d.Name ).ToList();
                cblSegments.DataBind();
            }
            else
            {
                cblSegments.Items.Clear();
                cblSegments.Visible = false;
            }


            dcpContainer.Controls.Clear();

            List<PropertyFields> propertyFields = GetPropertyFields( group );

            foreach ( var propertyField in propertyFields )
            {
                string controlId = string.Format( "{0}_{1}", dcpContainer.ID, propertyField.EntityField.UniqueName );

                var control = propertyField.EntityField.FieldType.Field.FilterControl( propertyField.EntityField.FieldConfig, controlId, false, FilterMode.AdvancedFilter );
                if ( control != null )
                {
                    dcpContainer.Controls.Add( new Label { ID = controlId + "_Label", Text = propertyField.Property, CssClass = "control-label", AssociatedControlID = control.ID } );
                    dcpContainer.Controls.Add( control );
                }
            }

            List<AttributeFields> attributeFields = GetAttributeFields( group );

            foreach ( var attributeField in attributeFields )
            {
                FilterMode filterMode = FilterMode.AdvancedFilter;
                if ( attributeField.EntityField.FieldType.Guid == Rock.SystemGuid.FieldType.MULTI_SELECT.AsGuid() )
                {
                    filterMode = FilterMode.SimpleFilter;
                }

                string controlId = string.Format( "{0}_{1}", dcpContainer.ID, attributeField.EntityField.UniqueName );

                var control = attributeField.EntityField.FieldType.Field.FilterControl( attributeField.EntityField.FieldConfig, controlId, false, filterMode );
                if ( control != null )
                {
                    dcpContainer.Controls.Add( new Label { ID = controlId + "_Label", Text = attributeField.Attribute.Name, CssClass = "control-label cust-label", AssociatedControlID = control.ID } );
                    dcpContainer.Controls.Add( control );
                }
            }
        }

        private List<PropertyFields> GetPropertyFields( Group group )
        {
            var propertyFields = new List<PropertyFields>();

            var filterPropertyGuid = GetAttributeValue( AttributeKey.PropertyFilterAttribute ).AsGuid();
            var filterAttribute = AttributeCache.Get( filterPropertyGuid );

            if ( filterAttribute == null )
            {
                return propertyFields;
            }

            group.LoadAttributes();
            var filterPropertyNames = group.GetAttributeValue( filterAttribute.Key ).SplitDelimitedValues();

            var entityFields = EntityHelper.GetEntityFields( typeof( Person ) );

            foreach ( var propertyName in filterPropertyNames )
            {
                var entityField = entityFields.FirstOrDefault( ef => ef.Name == propertyName );
                if ( entityField != null )
                {
                    propertyFields.Add( new PropertyFields { Property = propertyName, EntityField = entityField } );
                }
            }
            return propertyFields;
        }

        private List<AttributeFields> GetAttributeFields( Group group )
        {
            var attributeFields = new List<AttributeFields>();

            var filterAttributeGuid = GetAttributeValue( AttributeKey.AttributeFilterAttribute ).AsGuid();
            var filterAttribute = AttributeCache.Get( filterAttributeGuid );

            if ( filterAttribute == null )
            {
                return attributeFields;
            }

            group.LoadAttributes();
            var filterAttributeKeys = group.GetAttributeValue( filterAttribute.Key ).SplitDelimitedValues();

            var filterAttributes = filterAttributeKeys.Select( k => AttributeCache.Get( k ) ).ToList();

            var entityFields = EntityHelper.GetEntityFields( typeof( Person ) );

            foreach ( var attribute in filterAttributes )
            {
                var entityField = entityFields.FirstOrDefault( ef => ef.AttributeGuid == attribute.Guid );
                if ( entityField != null )
                {
                    attributeFields.Add( new AttributeFields { Attribute = attribute, EntityField = entityField } );
                }
            }
            return attributeFields;
        }

        private void ShowPreview()
        {
            _cellPhoneField = gPreview.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lCellPhone" ).FirstOrDefault();

            gPreview.SetLinqDataSource( GetCommunicationQry().OrderBy( p => p.FirstName ).ThenBy( p => p.LastName ) );
            gPreview.DataBind();
            mdPreview.Show();
        }

        private void SetSelection( SelectionState selectionState )
        {
            if ( selectionState != null && selectionState.CommunicationId.HasValue )
            {
                rpRegistrationTemplates.SetValue( selectionState.RegistrationTemplateId );
                cbIncludeInactive.Checked = selectionState.IncludeInactiveRegistrations;
                BindRegistrationInstanceDropdown();

                int commGroupId = selectionState.CommunicationId.Value;
                ddlCommunicationList.SelectedValue = commGroupId.ToString();

                ddlSendTo.Visible = SendingToParentsEnabled( commGroupId );
                if ( selectionState.SendTo.HasValue )
                {
                    ddlSendTo.SelectedValue = selectionState.SendTo.Value.ToString();
                }

                ShowCommunicationFields();

                cblSegments.SetValues( selectionState.SegmentIds );
                cblRegistrationInstances.SetValues( selectionState.RegistrationInstanceIds );

                RockContext rockContext = new RockContext();
                var group = new GroupService( rockContext ).Get( ddlCommunicationList.SelectedValueAsId() ?? 0 );
                if ( group != null )
                {
                    SetFilterValues( group, selectionState );
                }

                ddlPrevCommunication.SetValue( selectionState.PrevCommunicationId );

            }
        }

        private void ShowCommunicationFields()
        {
            var groupId = ddlCommunicationList.SelectedValueAsId();
            ViewState["ListId"] = groupId;
            btnGenerate.Visible = groupId.HasValue;
            btnPreview.Visible = groupId.HasValue;
            pnlRegistration.Visible = groupId.HasValue;
            pnlPrevCommunication.Visible = groupId.HasValue;
            cblSegments.Visible = groupId.HasValue;
            dcpContainer.Controls.Clear();
            SaveViewState();
            UpdateFilters( groupId ?? 0 );
        }

        private void SetFilterValues( Group group, SelectionState selectionState )
        {
            List<PropertyFields> propertyFields = GetPropertyFields( group );
            List<AttributeFields> attributeFields = GetAttributeFields( group );

            List<EntityField> entityFields = propertyFields.Select( p => p.EntityField ).ToList();
            entityFields.AddRange( attributeFields.Select( a => a.EntityField ) );

            foreach ( var entityField in entityFields )
            {
                string controlId = string.Format( "{0}_{1}", dcpContainer.ID, entityField.UniqueName );
                var control = dcpContainer.FindControl( controlId );

                if ( selectionState.PropertyValues.ContainsKey( controlId ) )
                {
                    entityField.FieldType.Field.SetFilterValues( control, entityField.FieldConfig, selectionState.PropertyValues[controlId] );
                }
            }
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

            var prevCommId = ddlPrevCommunication.SelectedValueAsId();
            if ( prevCommId.HasValue)
            {
                var prevCommunication = communicationService.Get( prevCommId.Value );
                if ( prevCommunication != null )
                {
                    communication.Name = prevCommunication.Name;
                    communication.CommunicationType = prevCommunication.CommunicationType;
                    communication.FutureSendDateTime = prevCommunication.FutureSendDateTime;
                    communication.CommunicationTemplateId = prevCommunication.CommunicationTemplateId;

                    communication.FromName = prevCommunication.FromName;
                    communication.FromEmail = prevCommunication.FromEmail;
                    communication.Subject = prevCommunication.Subject;
                    foreach( var prevAttachment in prevCommunication.Attachments )
                    {
                        var commAttachment = new CommunicationAttachment
                        {
                            BinaryFileId = prevAttachment.BinaryFileId,
                            CommunicationType = prevAttachment.CommunicationType,
                        };
                        communication.Attachments.Add( commAttachment );
                    }
                    communication.Message = prevCommunication.Message;

                    communication.SMSFromDefinedValueId = prevCommunication.SMSFromDefinedValueId;
                    communication.SMSMessage = prevCommunication.SMSMessage;
                }
            }

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
                CommunicationId = group.Id,
                SendTo = ddlSendTo.SelectedValueAsId(),
                PrevCommunicationId = ddlPrevCommunication.SelectedValueAsId()
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
            qry = qry.Where( p => filterQry.Select( s => s.Id ).Contains( p.Id ) );

            IQueryable<Person> registrationQry = GetRegistrationQry( personService );
            if ( registrationQry != null )
            {
                var registrants = registrationQry.ToList();
                if ( ddlIncludeExclude.SelectedValue == "1" ) //Include
                {
                    qry = qry.Where( p => registrationQry.Select( s => s.Id ).Contains( p.Id ) );
                }
                else
                {
                    qry = qry.Where( p => !registrationQry.Select( s => s.Id ).Contains( p.Id ) );
                }
            }

            int adultRoleId = GroupTypeCache.GetFamilyGroupType().Roles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Select( a => a.Id ).FirstOrDefault();
            int childRoleId = GroupTypeCache.GetFamilyGroupType().Roles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Select( a => a.Id ).FirstOrDefault();

            if ( SendingToParentsEnabled( group.Id ) )
            {

                var parentQry = personService.Queryable()
                    .Where( p => p.Members.Where( a => a.GroupRoleId == adultRoleId )
                        .Any( a => a.Group.Members
                        .Any( c => c.GroupRoleId == childRoleId && qry.Select( p2 => p2.Id ).Contains( c.PersonId ) ) ) );

                Group parentGroup = GetParentGroup( group, rockContext );

                if ( parentGroup != null )
                {
                    var inactiveParentIds = parentGroup.Members
                        .Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Inactive )
                        .Select( gm => gm.PersonId );
                    parentQry = parentQry.Where( p => !inactiveParentIds.Contains( p.Id ) );
                }

                switch ( ddlSendTo.SelectedValueAsId() ?? 0 )
                {
                    case 1: //Parents
                        return parentQry;
                    case 2:
                        return personService.Queryable().Where( p => qry.Contains( p ) || parentQry.Contains( p ) );
                }
            }

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

        private IQueryable<Person> GetRegistrationQry( PersonService personService )
        {
            var rockContext = personService.Context as RockContext;
            var instanceIds = cblRegistrationInstances.SelectedValuesAsInt;

            if ( !instanceIds.Any() )
            {
                return null;
            }

            _selectionState.RegistrationTemplateId = rpRegistrationTemplates.SelectedValue.AsIntegerOrNull();
            _selectionState.RegistrationInstanceIds = cblRegistrationInstances.SelectedValues;
            _selectionState.IncludeInactiveRegistrations = cbIncludeInactive.Checked;

            RegistrationService registrationService = new RegistrationService( rockContext );
            return registrationService.Queryable().Where( r => instanceIds.Contains( r.RegistrationInstanceId ) )
                .SelectMany( r => r.Registrants.Where( rr => rr.PersonAlias != null ) )
                .Select( rr => rr.PersonAlias.Person );
        }

        private IQueryable<Person> GetSegmentQry( PersonService personService, Group group )
        {
            if ( cblSegments.Visible )
            {
                _selectionState.SegmentIds = cblSegments.SelectedValues;

                var personEntityType = EntityTypeCache.Get( typeof( Person ) );

                DataViewService dataViewService = new DataViewService( new RockContext() );
                var dataviews = dataViewService
                    .GetByIds( cblSegments.SelectedValuesAsInt )
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

        private IQueryable<Person> GetFilterQry( PersonService personService, Group group )
        {
            List<PropertyFields> propertyFields = GetPropertyFields( group );
            List<AttributeFields> attributeFields = GetAttributeFields( group );

            List<EntityField> entityFields = propertyFields.Select( p => p.EntityField ).ToList();
            entityFields.AddRange( attributeFields.Select( a => a.EntityField ) );

            List<Expression> expressions = new List<Expression>();
            ParameterExpression paramExpression = personService.ParameterExpression;

            foreach ( var entityField in entityFields )
            {
                string controlId = string.Format( "{0}_{1}", dcpContainer.ID, entityField.UniqueName );
                var control = dcpContainer.FindControl( controlId );

                var filterValues = entityField.FieldType.Field.GetFilterValues( control, entityField.FieldConfig, FilterMode.AdvancedFilter );
                if ( !filterValues.Any() )
                {
                    continue;
                }

                //If you leave a filter blank it will still run
                //So create a comparison control to see if the current values == default values
                var compairsonControl = entityField.FieldType.Field.FilterControl( entityField.FieldConfig, "", false, FilterMode.AdvancedFilter );
                var comparisonValues = entityField.FieldType.Field.GetFilterValues( compairsonControl, entityField.FieldConfig, FilterMode.AdvancedFilter );

                bool isSame = true;
                if ( comparisonValues.Count == filterValues.Count )
                {
                    for ( var i = 0; i < comparisonValues.Count; i++ )
                    {
                        if ( comparisonValues[i] != filterValues[i] )
                        {
                            isSame = false;
                        }
                    }
                }
                else
                {
                    isSame = false;
                }

                if ( !isSame )
                {
                    _selectionState.PropertyValues.Add( controlId, filterValues );

                    if ( entityField.FieldKind == FieldKind.Property )
                    {
                        expressions.Add(
                            entityField.FieldType.Field.PropertyFilterExpression(
                                entityField.FieldConfig,
                                FixDelimination( filterValues.ToList() ),
                                paramExpression,
                                entityField.Name,
                                entityField.PropertyType ) );
                    }
                    else
                    {
                        expressions.Add(
                            Rock.Utility.ExpressionHelper.GetAttributeExpression(
                                personService,
                                paramExpression,
                                entityField,
                                FixDelimination( filterValues.ToList() ) ) );
                    }
                }
            }

            if ( expressions.Any() )
            {
                var expression = expressions[0];

                foreach ( var ex in expressions.Skip( 1 ) )
                {
                    expression = Expression.AndAlso( expression, ex );
                }

                MethodInfo getMethod = personService.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( SortProperty ) } );


                var sortProperty = new SortProperty { Direction = SortDirection.Ascending, Property = "Id" };

                var getResult = getMethod.Invoke( personService, new object[] { paramExpression, expression, sortProperty } );
                return ( getResult as IQueryable<Person> ) ?? personService.Queryable();
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
            public int? SendTo { get; set; }
            public List<string> SegmentIds { get; set; }
            public List<string> RegistrationInstanceIds { get; set; }
            public Dictionary<string, List<string>> PropertyValues { get; set; }
            public int? RegistrationTemplateId { get; set; }
            public bool IncludeInactiveRegistrations { get; set; }
            public int? PrevCommunicationId { get; set; }
            public SelectionState()
            {
                SegmentIds = new List<string>();
                RegistrationInstanceIds = new List<string>();
                PropertyValues = new Dictionary<string, List<string>>();
                IncludeInactiveRegistrations = false;
            }
        }
        #endregion




        protected void gPreview_RowDataBound( object sender, GridViewRowEventArgs e )
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