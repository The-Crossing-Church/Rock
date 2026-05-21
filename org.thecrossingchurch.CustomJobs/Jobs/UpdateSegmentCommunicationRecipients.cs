using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace org.thecrossingchurch.CustomJobs.Jobs
{
    /// <summary>
    /// Job that keeps the recipient list of scheduled communications created from Segments block up to date.
    ///
    /// The <c>com_9embers</c> "Communication List Segments" block builds a communication's recipient list from a
    /// set of criteria (communication list group, segments, person property/attribute filters, registration
    /// include/exclude, and an optional send-to-parents option) and stores the full selection as JSON in a
    /// communication attribute (<c>SegmentSelectionState</c> by default). When an admin schedules that
    /// communication for a future date, the recipient list is frozen at the moment it was generated, so anyone
    /// who matches the criteria afterwards is never included.
    ///
    /// This job finds every approved communication with a future send date that has not yet passed and that
    /// carries the selection-state attribute, re-evaluates the stored criteria, and adds any newly-matching
    /// people to the recipient list. It is add-only: it never removes recipients that are already on the list.
    ///
    /// NOTE: The recipient query below intentionally mirrors <c>GetCommunicationQry</c> in
    /// <c>RockWeb/Plugins/com_9embers/Communication/CommunicationListSegments.ascx.cs</c>. If that block's
    /// filtering logic changes, update this job to match so the two stay in sync.
    /// </summary>
    [DisplayName( "Update Segment Communication Recipients" )]
    [Description( "Re-evaluates the recipient criteria stored by the Communication List Segments block on approved communications scheduled to send in the future, and adds any newly-matching people to the recipient list." )]

    [TextField(
        "Selection State Attribute Key",
        Description = "The key of the Communication attribute that the Communication List Segments block stores the recipient selection criteria (JSON) in.",
        IsRequired = true,
        DefaultValue = DefaultSelectionStateAttributeKey,
        Key = AttributeKey.SelectionStateAttributeKey,
        Order = 0 )]

    [AttributeField(
        "Parents Group Attribute",
        Description = "The Communication List group attribute that identifies the parent group. This is only needed to re-evaluate communications that were built to send to parents, and should be set to the same attribute configured on the Communication List Segments block. Leave blank if communications are never sent to parents.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST,
        IsRequired = false,
        Key = AttributeKey.ParentsGroupAttribute,
        Order = 1 )]

    public class UpdateSegmentCommunicationRecipients : RockJob
    {
        private const string DefaultSelectionStateAttributeKey = "SegmentSelectionState";

        /// <summary>
        /// The prefix the block uses when building the control id key for a stored filter value.
        /// The block stores filter values keyed by <c>string.Format( "{0}_{1}", dcpContainer.ID, entityField.UniqueName )</c>
        /// and <c>dcpContainer</c> is the id of the DynamicControlsPanel in the block markup.
        /// </summary>
        private const string FilterControlIdPrefix = "dcpContainer_";

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string SelectionStateAttributeKey = "SelectionStateAttributeKey";
            public const string ParentsGroupAttribute = "ParentsGroupAttribute";
        }

        #endregion Attribute Keys

        /// <inheritdoc />
        public override void Execute()
        {
            var errors = new List<string>();
            int communicationsUpdated = 0;
            int recipientsAdded = 0;

            string selectionStateKey = GetAttributeValue( AttributeKey.SelectionStateAttributeKey );
            if ( selectionStateKey.IsNullOrWhiteSpace() )
            {
                selectionStateKey = DefaultSelectionStateAttributeKey;
            }

            Guid? parentsGroupAttributeGuid = GetAttributeValue( AttributeKey.ParentsGroupAttribute ).AsGuidOrNull();

            var now = RockDateTime.Now;

            // Find approved communications that are scheduled to go out in the future and have not been sent yet.
            List<int> candidateCommunicationIds;
            using ( var rockContext = new RockContext() )
            {
                candidateCommunicationIds = new CommunicationService( rockContext ).Queryable()
                    .Where( c =>
                        c.Status == CommunicationStatus.Approved &&
                        c.FutureSendDateTime.HasValue &&
                        c.FutureSendDateTime.Value > now &&
                        !c.SendDateTime.HasValue )
                    .Select( c => c.Id )
                    .ToList();
            }

            foreach ( var communicationId in candidateCommunicationIds )
            {
                // Use a fresh context per communication so a failure on one doesn't poison the rest.
                using ( var rockContext = new RockContext() )
                {
                    try
                    {
                        var communication = new CommunicationService( rockContext ).Get( communicationId );
                        if ( communication == null )
                        {
                            continue;
                        }

                        communication.LoadAttributes( rockContext );
                        var selectionStateJson = communication.GetAttributeValue( selectionStateKey );
                        if ( selectionStateJson.IsNullOrWhiteSpace() )
                        {
                            // Not generated by the Communication List Segments block (no stored criteria), skip it.
                            continue;
                        }

                        SelectionState selectionState;
                        try
                        {
                            selectionState = JsonConvert.DeserializeObject<SelectionState>( selectionStateJson );
                        }
                        catch ( Exception ex )
                        {
                            errors.Add( $"Communication {communicationId}: unable to parse the stored selection state. {ex.Message}" );
                            continue;
                        }

                        if ( selectionState == null || !selectionState.CommunicationListId.HasValue )
                        {
                            continue;
                        }

                        var recipientQry = BuildRecipientQuery( rockContext, selectionState, parentsGroupAttributeGuid, out string skipReason );
                        if ( recipientQry == null )
                        {
                            if ( skipReason.IsNotNullOrWhiteSpace() )
                            {
                                errors.Add( $"Communication {communicationId}: {skipReason}" );
                            }
                            continue;
                        }

                        var matchingPersons = recipientQry.ToList();
                        int added = AddNewRecipients( rockContext, communication, matchingPersons );
                        if ( added > 0 )
                        {
                            rockContext.SaveChanges();
                            communicationsUpdated++;
                            recipientsAdded += added;
                        }
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex );
                        errors.Add( $"Communication {communicationId}: {ex.Message}" );
                    }
                }
            }

            var status = $"Added {recipientsAdded} recipient{( recipientsAdded == 1 ? "" : "s" )} across {communicationsUpdated} communication{( communicationsUpdated == 1 ? "" : "s" )} ({candidateCommunicationIds.Count} scheduled communication{( candidateCommunicationIds.Count == 1 ? "" : "s" )} evaluated).";
            if ( errors.Any() )
            {
                status += $"\n\n{errors.Count} error{( errors.Count == 1 ? "" : "s" )}:\n{string.Join( "\n", errors )}";
                UpdateLastStatusMessage( status );
                throw new RockJobWarningException( status );
            }

            UpdateLastStatusMessage( status );
        }

        #region Recipient Evaluation

        /// <summary>
        /// Rebuilds the recipient query for a communication from its stored selection state. Mirrors the block's
        /// <c>GetCommunicationQry</c>, reading values from the <see cref="SelectionState"/> instead of UI controls.
        /// </summary>
        /// <returns>The query of matching people, or <c>null</c> if it could not be built (see <paramref name="skipReason"/>).</returns>
        private IQueryable<Person> BuildRecipientQuery( RockContext rockContext, SelectionState selectionState, Guid? parentsGroupAttributeGuid, out string skipReason )
        {
            skipReason = null;

            var group = new GroupService( rockContext ).Get( selectionState.CommunicationListId.Value );
            if ( group == null )
            {
                skipReason = "the communication list group no longer exists.";
                return null;
            }

            var personService = new PersonService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );

            // Base set: active, non-archived members of the communication list group.
            var qry = groupMemberService.Queryable()
                .Where( gm =>
                    gm.GroupId == group.Id &&
                    gm.GroupMemberStatus == GroupMemberStatus.Active &&
                    gm.IsArchived == false )
                .Select( gm => gm.Person );

            // Segments (data views).
            var segmentQry = GetSegmentQry( rockContext, personService, selectionState );
            if ( segmentQry != null )
            {
                qry = qry.Where( p => segmentQry.Select( s => s.Id ).Contains( p.Id ) );
            }

            // Person property / attribute filters.
            var filterQry = GetFilterQry( personService, selectionState );
            qry = qry.Where( p => filterQry.Select( s => s.Id ).Contains( p.Id ) );

            // Registration include / exclude.
            var registrationQry = GetRegistrationQry( rockContext, selectionState );
            if ( registrationQry != null )
            {
                if ( selectionState.RegistrationIncludeExclude == 1 ) // Include
                {
                    qry = qry.Where( p => registrationQry.Select( s => s.Id ).Contains( p.Id ) );
                }
                else
                {
                    qry = qry.Where( p => !registrationQry.Select( s => s.Id ).Contains( p.Id ) );
                }
            }

            // Send to parents.
            if ( ( selectionState.SendTo ?? 0 ) > 0 )
            {
                if ( !parentsGroupAttributeGuid.HasValue )
                {
                    skipReason = "it was built to send to parents, but the 'Parents Group Attribute' job setting is not configured.";
                    return null;
                }

                int adultRoleId = GroupTypeCache.GetFamilyGroupType().Roles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Select( a => a.Id ).FirstOrDefault();
                int childRoleId = GroupTypeCache.GetFamilyGroupType().Roles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Select( a => a.Id ).FirstOrDefault();

                var parentQry = personService.Queryable()
                    .Where( p => p.Members.Where( a => a.GroupRoleId == adultRoleId )
                        .Any( a => a.Group.Members
                            .Any( c => c.GroupRoleId == childRoleId && qry.Select( p2 => p2.Id ).Contains( c.PersonId ) ) ) );

                var parentGroup = GetParentGroup( group, parentsGroupAttributeGuid.Value, rockContext );
                if ( parentGroup != null )
                {
                    var inactiveParentIds = parentGroup.Members
                        .Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Inactive )
                        .Select( gm => gm.PersonId );
                    parentQry = parentQry.Where( p => !inactiveParentIds.Contains( p.Id ) );
                }

                switch ( selectionState.SendTo ?? 0 )
                {
                    case 1: // Parents only
                        return parentQry;
                    case 2: // Members and parents
                        return personService.Queryable().Where( p => qry.Contains( p ) || parentQry.Contains( p ) );
                }
            }

            return qry;
        }

        private IQueryable<Person> GetSegmentQry( RockContext rockContext, PersonService personService, SelectionState selectionState )
        {
            var segmentIds = ToIntList( selectionState.SegmentIds );
            if ( !segmentIds.Any() )
            {
                return null;
            }

            var personEntityType = EntityTypeCache.Get( typeof( Person ) );
            var dataviews = new DataViewService( rockContext )
                .GetByIds( segmentIds )
                .Where( dv => dv.EntityTypeId == personEntityType.Id )
                .ToList();

            if ( !dataviews.Any() )
            {
                return null;
            }

            ParameterExpression parameterExpression = personService.ParameterExpression;
            Expression expression = dataviews[0].GetExpression( personService, parameterExpression );
            foreach ( var dataview in dataviews.Skip( 1 ) )
            {
                expression = Expression.OrElse( expression, dataview.GetExpression( personService, parameterExpression ) );
            }

            return GetPersonQueryFromExpression( personService, parameterExpression, expression );
        }

        private IQueryable<Person> GetFilterQry( PersonService personService, SelectionState selectionState )
        {
            var propertyValues = selectionState.PropertyValues ?? new Dictionary<string, List<string>>();
            if ( !propertyValues.Any() )
            {
                return personService.Queryable();
            }

            var entityFields = EntityHelper.GetEntityFields( typeof( Person ) );
            ParameterExpression paramExpression = personService.ParameterExpression;
            var expressions = new List<Expression>();

            foreach ( var kvp in propertyValues )
            {
                var uniqueName = kvp.Key.StartsWith( FilterControlIdPrefix )
                    ? kvp.Key.Substring( FilterControlIdPrefix.Length )
                    : kvp.Key;

                var entityField = entityFields.FirstOrDefault( ef => ef.UniqueName == uniqueName );
                if ( entityField == null )
                {
                    // The property/attribute is no longer available to filter on; can't reconstruct it, so skip.
                    continue;
                }

                var filterValues = kvp.Value;
                if ( filterValues == null || !filterValues.Any() )
                {
                    continue;
                }

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

            if ( !expressions.Any() )
            {
                return personService.Queryable();
            }

            var expression = expressions[0];
            foreach ( var ex in expressions.Skip( 1 ) )
            {
                expression = Expression.AndAlso( expression, ex );
            }

            return GetPersonQueryFromExpression( personService, paramExpression, expression ) ?? personService.Queryable();
        }

        private IQueryable<Person> GetRegistrationQry( RockContext rockContext, SelectionState selectionState )
        {
            var instanceIds = ToIntList( selectionState.RegistrationInstanceIds );
            if ( !instanceIds.Any() )
            {
                return null;
            }

            return new RegistrationService( rockContext ).Queryable()
                .Where( r => instanceIds.Contains( r.RegistrationInstanceId ) )
                .SelectMany( r => r.Registrants.Where( rr => rr.PersonAlias != null ) )
                .Select( rr => rr.PersonAlias.Person );
        }

        private Group GetParentGroup( Group group, Guid parentsGroupAttributeGuid, RockContext rockContext )
        {
            var parentsAttribute = AttributeCache.Get( parentsGroupAttributeGuid );
            if ( parentsAttribute == null )
            {
                return null;
            }

            group.LoadAttributes( rockContext );
            var parentGroupGuids = group.GetAttributeValue( parentsAttribute.Key ).SplitDelimitedValues();
            if ( parentGroupGuids.Length < 2 )
            {
                return null;
            }

            return new GroupService( rockContext ).Get( parentGroupGuids[1].AsGuid() );
        }

        /// <summary>
        /// Runs a built filter <see cref="Expression"/> through <c>PersonService.Get</c>. Uses reflection to match
        /// the block, which calls the <c>Get( ParameterExpression, Expression, SortProperty )</c> overload this way.
        /// </summary>
        private IQueryable<Person> GetPersonQueryFromExpression( PersonService personService, ParameterExpression parameterExpression, Expression whereExpression )
        {
            MethodInfo getMethod = personService.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( SortProperty ) } );
            var sortProperty = new SortProperty { Direction = SortDirection.Ascending, Property = "Id" };
            var getResult = getMethod.Invoke( personService, new object[] { parameterExpression, whereExpression, sortProperty } );
            return getResult as IQueryable<Person>;
        }

        /// <summary>
        /// Mirrors the block's <c>FixDelimination</c>: a single value that looks like a JSON array is flattened into
        /// a comma-delimited string so the field type's filter expression can parse it.
        /// </summary>
        private List<string> FixDelimination( List<string> values )
        {
            if ( values.Count == 1 && values[0] != null && values[0].Contains( "[" ) )
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

        #endregion Recipient Evaluation

        /// <summary>
        /// Adds a recipient for every matching person that isn't already a recipient. Add-only: existing recipients
        /// are never removed.
        /// </summary>
        /// <returns>The number of recipients added.</returns>
        private int AddNewRecipients( RockContext rockContext, Rock.Model.Communication communication, List<Person> matchingPersons )
        {
            if ( !matchingPersons.Any() )
            {
                return 0;
            }

            var existingPersonIds = new HashSet<int>(
                new CommunicationRecipientService( rockContext ).Queryable()
                    .Where( cr => cr.CommunicationId == communication.Id && cr.PersonAlias != null )
                    .Select( cr => cr.PersonAlias.PersonId )
                    .ToList() );

            var emailMediumEntityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
            var smsMediumEntityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() );
            var recipientService = new CommunicationRecipientService( rockContext );

            int added = 0;
            foreach ( var person in matchingPersons )
            {
                if ( !person.PrimaryAliasId.HasValue || existingPersonIds.Contains( person.Id ) )
                {
                    continue;
                }

                var newRecipient = new CommunicationRecipient
                {
                    CommunicationId = communication.Id,
                    PersonAliasId = person.PrimaryAliasId.Value,
                    MediumEntityTypeId = communication.CommunicationType == CommunicationType.SMS
                        ? smsMediumEntityType.Id
                        : emailMediumEntityType.Id
                };

                recipientService.Add( newRecipient );
                existingPersonIds.Add( person.Id );
                added++;
            }

            return added;
        }

        private static List<int> ToIntList( List<string> values )
        {
            if ( values == null )
            {
                return new List<int>();
            }

            return values
                .Select( v => v.AsIntegerOrNull() )
                .Where( v => v.HasValue )
                .Select( v => v.Value )
                .ToList();
        }

        #region Support Classes

        /// <summary>
        /// Mirror of the private <c>SelectionState</c> class serialized by the Communication List Segments block.
        /// Property names must match the block's class so the stored JSON deserializes correctly.
        /// </summary>
        private class SelectionState
        {
            public int? ExistingCommunicationId { get; set; }
            public int? CommunicationListId { get; set; }
            public int? SendTo { get; set; }
            public List<string> SegmentIds { get; set; }
            public Dictionary<string, List<string>> PropertyValues { get; set; }
            public int? RegistrationIncludeExclude { get; set; }
            public int? RegistrationTemplateId { get; set; }
            public List<string> RegistrationInstanceIds { get; set; }
            public bool IncludeInactiveRegistrations { get; set; }
            public int? PrevCommunicationId { get; set; }
        }

        #endregion Support Classes
    }
}
