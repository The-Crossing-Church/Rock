using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Web.Cache;

using Attribute = Rock.Model.Attribute;

namespace org.thecrossingchurch.CustomJobs.Jobs
{
    [AttributeField(
        "Person Attributes",
        Key = AttributeKey.PersonAttributes,
        AllowMultiple = true,
        Category = "Attributes",
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        Order = 1,
        IsRequired = false
    )]
    [AttributeField(
        "Family Attributes",
        Key = AttributeKey.GroupAttributes,
        AllowMultiple = true,
        Category = "Attributes",
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = "10",
        Order = 2,
        IsRequired = false
    )]

    [GroupTypeField( "CK Sunday Morning Group Type",
        Key = AttributeKey.CKGroupType,
        Description = "The group type for sunday morning services check-in for CK",
        Category = "Check-in Configuration Data",
        Order = 1,
        IsRequired = true
    )]
    [GroupTypeField( "CS Sunday Morning Group Type",
        Key = AttributeKey.CSGroupType,
        Description = "The group type for sunday morning services check-in for CS",
        Category = "Check-in Configuration Data",
        Order = 2,
        IsRequired = true
    )]
    [GroupTypesField( "Sunday Morning Services Group Types",
        Key = AttributeKey.SundayServiceGroupTypes,
        Description = "The group types for sunday morning services",
        Category = "Check-in Configuration Data",
        Order = 3,
        IsRequired = true
    )]
    [SchedulesField( "First Service Schedules",
        Key = AttributeKey.FirstServiceSchedules,
        Description = "The schedules for first service",
        Category = "Check-in Configuration Data",
        Order = 4,
        IsRequired = true
    )]
    [SchedulesField( "Second Service Schedules",
        Key = AttributeKey.SecondServiceSchedules,
        Description = "The schedules for second service",
        Category = "Check-in Configuration Data",
        Order = 5,
        IsRequired = true
    )]
    [SchedulesField( "Third Service Schedules",
        Key = AttributeKey.ThirdServiceSchedules,
        Description = "The schedules for third service",
        Category = "Check-in Configuration Data",
        Order = 6,
        IsRequired = true
    )]
    [DefinedValueField( "Group Type Purpose for Small Groups",
        Key = AttributeKey.SmallGroupPurpose,
        Description = "The group type purpose to use to determine if a group is a small group",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE,
        Category = "Check-in Configuration Data",
        Order = 7,
        IsRequired = true
    )]
    [DefinedValueField( "Group Type Purpose for Serving Teams",
        Key = AttributeKey.ServingTeamGroupPurpose,
        Description = "The group type purpose to use to determine if a group is a serving team",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE,
        Category = "Check-in Configuration Data",
        Order = 8,
        IsRequired = true
    )]
    [DateField( "Earliest Date to Process Check-in Data",
        Key = AttributeKey.EarliestDate,
        Description = "The earliest date that a check-in occurrence date is to be considered value",
        DefaultValue = "2024-01-01 00:00:00",
        Category = "Check-in Configuration Data",
        Order = 9,
        IsRequired = true
    )]

    [LocationField( "Campus",
        Key = AttributeKey.CampusLocation,
        Description = "The location to use when calculating distance from campus.",
        Category = "Other Configuration Data",
        Order = 1,
        IsRequired = true
    )]

    [GroupField( "Filter Group",
        Key = AttributeKey.FilterGroup,
        Description = "Filter active person records down to membership in this group before processing records.",
        Category = "Person Filters",
        Order = 1,
        IsRequired = false
    )]
    [DataViewField( "Filter Dataview",
        Key = AttributeKey.FilterDataview,
        Description = "Filter active person records down to membership in this dataview before processing records.",
        EntityTypeName = "Rock.Model.Person",
        Category = "Person Filters",
        Order = 2,
        IsRequired = false
    )]
    [DataViewField( "Filter Dataview",
        Key = AttributeKey.GroupFilterDataview,
        Description = "Filter family groups down to membership in this dataview before processing families.",
        EntityTypeName = "Rock.Model.Group",
        Category = "Group Filters",
        Order = 1,
        IsRequired = false
    )]
    public class PopulateEngagementAttributes : RockJob
    {
        #region Attribute Keys
        private class AttributeKey
        {
            public const string FilterGroup = "FilterGroup";
            public const string FilterDataview = "FilterDataview";
            public const string CKGroupType = "CKGroupType";
            public const string CSGroupType = "CSGroupType";
            public const string SundayServiceGroupTypes = "SundayServiceGroupTypes";
            public const string FirstServiceSchedules = "FirstServiceSchedules";
            public const string SecondServiceSchedules = "SecondServiceSchedules";
            public const string ThirdServiceSchedules = "ThirdServiceSchedules";
            public const string SmallGroupPurpose = "SmallGroupPurpose";
            public const string ServingTeamGroupPurpose = "ServingTeamGroupPurpose";
            public const string EarliestDate = "EarliestDate";
            public const string GroupFilterDataview = "GroupFilterDataview";
            public const string PersonAttributes = "PersonAttributes";
            public const string GroupAttributes = "GroupAttributes";
            public const string CampusLocation = "CampusLocation";
        }

        private class EngagementPersonAttributeKeys
        {
            public const string IsServing = "tcc_engagement_is_serving";
            public const string IsLeader = "tcc_engagement_is_leader";
            public const string InSmallGroup = "tcc_engagement_in_small_group";
            public const string ServingSchedule = "tcc_engagement_usual_schedules";
            public const string ServingFrequency = "tcc_engagement_serving_frequency";
        }

        private class EngagementFamilyAttributeKeys
        {
            public const string NumberCKCheckins = "tcc_engagement_number_ck_checkins";
            public const string NumberCSCheckins = "tcc_engagement_number_cs_checkins";
            public const string NumberSundaysAttended = "tcc_engagement_number_of_sunday_att";
            public const string NumberFirstServiceCheckins = "tcc_engagement_number_first_checkins";
            public const string NumberSecondServiceCheckins = "tcc_engagement_number_second_checkins";
            public const string NumberThirdServiceCheckins = "tcc_engagement_number_third_checkins";
            public const string NumberFirstServiceServingCheckins = "tcc_engagement_number_first_serving_checkins";
            public const string NumberSecondServiceServingCheckins = "tcc_engagement_number_second_serving_checkins";
            public const string NumberThirdServiceServingCheckins = "tcc_engagement_number_third_serving_checkins";
            public const string NumberOfKidsLO = "tcc_engagement_number_kids_lo";
            public const string NumberOfKidsPS = "tcc_engagement_number_kids_ps";
            public const string NumberOfKidsEC = "tcc_engagement_number_kids_ec";
            public const string NumberOfKidsELM = "tcc_engagement_number_kids_el";
            public const string NumberOfKidsMS = "tcc_engagement_number_kids_ms";
            public const string NumberOfKidsHS = "tcc_engagement_number_kids_hs";
            public const string DistanceFromCampus = "tcc_engagement_distance_from_campus";
        }
        #endregion Attribute Keys

        private ConcurrentBag<Task> _personUpdates { get; set; }
        private ConcurrentBag<Task> _groupUpdates { get; set; }
        private int _activePersonRecordStatusId { get; set; }
        private int _personRecordTypeId { get; set; }
        private int _homeLocationTypeId { get; set; }
        public override void Execute()
        {
            List<Guid> personAttrGuids = GetAttributeValue( AttributeKey.PersonAttributes ).Split( ',' ).AsGuidList();
            List<Guid> familyAttrGuids = GetAttributeValue( AttributeKey.GroupAttributes ).Split( ',' ).AsGuidList();
            Guid? filterGroupGuid = GetAttributeValue( AttributeKey.FilterGroup ).AsGuidOrNull();
            Guid? filterDataviewGuid = GetAttributeValue( AttributeKey.FilterDataview ).AsGuidOrNull();
            Guid? filterGroupDataviewGuid = GetAttributeValue( AttributeKey.GroupFilterDataview ).AsGuidOrNull();
            _personUpdates = new ConcurrentBag<Task>();
            _groupUpdates = new ConcurrentBag<Task>();

            RockContext context = new RockContext();
            AttributeService attr_svc = new AttributeService( context );
            var personAttrs = attr_svc.Queryable().Where( attr => personAttrGuids.Contains( attr.Guid ) );
            var familyAttrs = attr_svc.Queryable().Where( attr => familyAttrGuids.Contains( attr.Guid ) );

            DefinedValueService dv_svc = new DefinedValueService( context );
            _activePersonRecordStatusId = dv_svc.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE ).Id;
            _personRecordTypeId = dv_svc.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON ).Id;
            _homeLocationTypeId = dv_svc.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME ).Id;
            Guid? smallGroupPurposeGuid = GetAttributeValue( AttributeKey.SmallGroupPurpose ).AsGuidOrNull();
            int smallGroupPurposeId = smallGroupPurposeGuid.HasValue ? dv_svc.Get( smallGroupPurposeGuid.Value ).Id : 0;
            Guid? servingTeamPurposeGuid = GetAttributeValue( AttributeKey.ServingTeamGroupPurpose ).AsGuidOrNull();
            int servingTeamPurposeId = servingTeamPurposeGuid.HasValue ? dv_svc.Get( servingTeamPurposeGuid.Value ).Id : 0;

            GroupTypeService gt_svc = new GroupTypeService( context );
            Guid? ckGroupTypeGuid = GetAttributeValue( AttributeKey.CKGroupType ).AsGuidOrNull();
            Guid? csGroupTypeGuid = GetAttributeValue( AttributeKey.CSGroupType ).AsGuidOrNull();
            List<Guid> sundayServicesGroupType = GetAttributeValue( AttributeKey.SundayServiceGroupTypes ).Split( ',' ).AsGuidList();
            var ckCheckinGroupType = gt_svc.Get( ckGroupTypeGuid.Value );
            var csCheckinGroupType = gt_svc.Get( csGroupTypeGuid.Value );
            var sundayGroupTypes = gt_svc.Queryable().Where( gt => sundayServicesGroupType.Contains( gt.Guid ) ).Select( gt => gt.Id ).ToList();
            var servingTeamGroupTypes = gt_svc.Queryable().Where( gt => gt.GroupTypePurposeValueId == servingTeamPurposeId ).Select( gt => gt.Id ).ToList();

            ScheduleService schd_svc = new ScheduleService( context );
            DateTime earliestDate = GetAttributeValue( AttributeKey.EarliestDate ).AsDateTime().Value;
            List<Guid?> firstServiceGuids = GetAttributeValue( AttributeKey.FirstServiceSchedules ).Split( ',' ).AsGuidOrNullList().Where( g => g.HasValue ).ToList();
            List<Guid?> secondServiceGuids = GetAttributeValue( AttributeKey.SecondServiceSchedules ).Split( ',' ).AsGuidOrNullList().Where( g => g.HasValue ).ToList();
            List<Guid?> thirdServiceGuids = GetAttributeValue( AttributeKey.ThirdServiceSchedules ).Split( ',' ).AsGuidOrNullList().Where( g => g.HasValue ).ToList();
            List<int> firstServiceScheduleIds = schd_svc.Queryable().Where( s => firstServiceGuids.Contains( s.Guid ) ).Select( s => s.Id ).ToList();
            List<int> secondServiceScheduleIds = schd_svc.Queryable().Where( s => secondServiceGuids.Contains( s.Guid ) ).Select( s => s.Id ).ToList();
            List<int> thirdServiceScheduleIds = schd_svc.Queryable().Where( s => thirdServiceGuids.Contains( s.Guid ) ).Select( s => s.Id ).ToList();
            List<int> allScheduleIds = firstServiceScheduleIds.Union( secondServiceScheduleIds ).Union( thirdServiceScheduleIds ).ToList();


            var people = GetPeopleForProcessing( context, filterGroupGuid, filterDataviewGuid );

            foreach ( var personAttr in personAttrs )
            {
                switch ( personAttr.Key )
                {
                    case EngagementPersonAttributeKeys.IsServing:
                        ProcessPersonInGroupOfPurpose( context, people, personAttr, servingTeamPurposeId );
                        break;
                    case EngagementPersonAttributeKeys.IsLeader:
                        ProcessPersonHasLeadershipRole( context, people, personAttr );
                        break;
                    case EngagementPersonAttributeKeys.InSmallGroup:
                        ProcessPersonInGroupOfPurpose( context, people, personAttr, smallGroupPurposeId );
                        break;
                    case EngagementPersonAttributeKeys.ServingSchedule:
                        ProcessPersonServingSchedules( context, people, personAttr, servingTeamPurposeId, earliestDate, null, allScheduleIds );
                        break;
                    case EngagementPersonAttributeKeys.ServingFrequency:
                        ProcessPersonServingFrequency( context, people, personAttr, servingTeamPurposeId, earliestDate, null, allScheduleIds );
                        break;
                }
            }

            Task.WaitAll( _personUpdates.ToArray() );

            var groups = GetGroupsForProcessing( context, filterGroupDataviewGuid );
            int currentGradYear = GlobalAttributesCache.Get().CurrentGraduationYear;

            foreach ( var familyAttr in familyAttrs )
            {
                switch ( familyAttr.Key )
                {
                    case EngagementFamilyAttributeKeys.NumberOfKidsLO:
                        ProcessNumberOfPeopleInFamily( context, groups, familyAttr, 0, 3, null, null );
                        break;
                    case EngagementFamilyAttributeKeys.NumberOfKidsPS:
                        ProcessNumberOfPeopleInFamily( context, groups, familyAttr, 4, 7, null, currentGradYear + 13 );
                        break;
                    case EngagementFamilyAttributeKeys.NumberOfKidsEC:
                        ProcessNumberOfPeopleInFamily( context, groups, familyAttr, 0, 7, null, currentGradYear + 13 );
                        break;
                    case EngagementFamilyAttributeKeys.NumberOfKidsELM:
                        ProcessNumberOfPeopleInFamily( context, groups, familyAttr, 0, 18, currentGradYear + 7, currentGradYear + 12 );
                        break;
                    case EngagementFamilyAttributeKeys.NumberOfKidsMS:
                        ProcessNumberOfPeopleInFamily( context, groups, familyAttr, null, null, currentGradYear + 4, currentGradYear + 6 );
                        break;
                    case EngagementFamilyAttributeKeys.NumberOfKidsHS:
                        ProcessNumberOfPeopleInFamily( context, groups, familyAttr, null, null, currentGradYear, currentGradYear + 3 ); ;
                        break;
                    case EngagementFamilyAttributeKeys.NumberCKCheckins:
                        List<int> ckGroupTypeId = new List<int>() { ckCheckinGroupType.Id };
                        ProcessNumberOfCheckings( context, groups, familyAttr, ckGroupTypeId, earliestDate, null, allScheduleIds, false );
                        break;
                    case EngagementFamilyAttributeKeys.NumberCSCheckins:
                        List<int> csGroupTypeId = new List<int>() { csCheckinGroupType.Id };
                        ProcessNumberOfCheckings( context, groups, familyAttr, csGroupTypeId, earliestDate, null, allScheduleIds, false );
                        break;
                    case EngagementFamilyAttributeKeys.NumberSundaysAttended:
                        ProcessNumberOfCheckings( context, groups, familyAttr, sundayGroupTypes, earliestDate, null, allScheduleIds, true );
                        break;
                    case EngagementFamilyAttributeKeys.NumberFirstServiceCheckins:
                        ProcessNumberOfCheckings( context, groups, familyAttr, sundayGroupTypes, earliestDate, null, firstServiceScheduleIds, false );
                        break;
                    case EngagementFamilyAttributeKeys.NumberSecondServiceCheckins:
                        ProcessNumberOfCheckings( context, groups, familyAttr, sundayGroupTypes, earliestDate, null, secondServiceScheduleIds, false );
                        break;
                    case EngagementFamilyAttributeKeys.NumberThirdServiceCheckins:
                        ProcessNumberOfCheckings( context, groups, familyAttr, sundayGroupTypes, earliestDate, null, thirdServiceScheduleIds, false );
                        break;
                    case EngagementFamilyAttributeKeys.NumberFirstServiceServingCheckins:
                        ProcessNumberOfCheckings( context, groups, familyAttr, servingTeamGroupTypes, earliestDate, null, firstServiceScheduleIds, false );
                        break;
                    case EngagementFamilyAttributeKeys.NumberSecondServiceServingCheckins:
                        ProcessNumberOfCheckings( context, groups, familyAttr, servingTeamGroupTypes, earliestDate, null, secondServiceScheduleIds, false );
                        break;
                    case EngagementFamilyAttributeKeys.NumberThirdServiceServingCheckins:
                        ProcessNumberOfCheckings( context, groups, familyAttr, servingTeamGroupTypes, earliestDate, null, thirdServiceScheduleIds, false );
                        break;
                    case EngagementFamilyAttributeKeys.DistanceFromCampus:
                        ProcessDistanceFromCampus( context, groups, familyAttr );
                        break;
                }
            }

            Task.WaitAll( _groupUpdates.ToArray() );
        }

        /// <summary>
        /// Method to get the list of records available for processing
        /// </summary>
        /// <param name="context">The DB Context to use</param>
        /// <param name="filterGroupGuid">The guid of the group used to filter the list of records available for processing</param>
        /// <param name="filterDataviewGuid">The guid of the dataview used to filter the list of records available for processing</param>
        /// <returns>The list of people that should be processed</returns>
        private IQueryable<Person> GetPeopleForProcessing( RockContext context, Guid? filterGroupGuid, Guid? filterDataviewGuid )
        {
            IQueryable<Person> people;
            PersonService p_svc = new PersonService( context );
            GroupService grp_svc = new GroupService( context );
            GroupMemberService gm_svc = new GroupMemberService( context );
            DataViewService dataview_svc = new DataViewService( context );

            people = p_svc.Queryable().Where( p => p.RecordTypeValueId == _personRecordTypeId && p.RecordStatusValueId == _activePersonRecordStatusId );

            if ( filterGroupGuid.HasValue )
            {
                Group filterGroup = grp_svc.Get( filterGroupGuid.Value );
                if ( filterGroup != null )
                {
                    IQueryable<GroupMember> memberships = gm_svc.Queryable().Where( gm => !gm.IsArchived && gm.GroupMemberStatus == GroupMemberStatus.Active && gm.GroupId == filterGroup.Id );
                    people = people.Join( memberships,
                        p => p.Id,
                        gm => gm.PersonId,
                        ( p, gm ) => p
                    );
                }
            }
            if ( filterDataviewGuid.HasValue )
            {
                DataView filterDataview = dataview_svc.Get( filterDataviewGuid.Value );
                if ( filterDataview != null )
                {
                    var dataViewGetQueryArgs = new DataViewGetQueryArgs
                    {
                        DbContext = context
                    };
                    var qry = filterDataview.GetQuery( dataViewGetQueryArgs );
                    people = people.Join( qry,
                        p => p.Id,
                        q => q.Id,
                        ( p, q ) => p
                    );
                }
            }

            return people;
        }

        /// <summary>
        /// Update person attributes based on the person's membership in a group with a specific group type purpose.
        /// </summary>
        /// <param name="context">The DB Context to use</param>
        /// <param name="people">The list of people to check membership for</param>
        /// <param name="attr">The person attribute to be updated</param>
        /// <param name="groupTypePurposeId">The id of the group type purpose that indicates the person should have a true value in the attribute</param>
        private void ProcessPersonInGroupOfPurpose( RockContext context, IQueryable<Person> people, Attribute attr, int groupTypePurposeId )
        {
            AttributeValueService av_svc = new AttributeValueService( context );
            GroupService grp_svc = new GroupService( context );
            GroupMemberService gm_svc = new GroupMemberService( context );
            DefinedValueService dv_svc = new DefinedValueService( context );

            var attr_vals = av_svc.Queryable().Where( av => av.AttributeId == attr.Id );

            var groups = grp_svc.Queryable().Where( g => g.IsActive && !g.IsArchived && g.GroupType.GroupTypePurposeValueId == groupTypePurposeId );
            var groupMemberships = gm_svc.Queryable().Where( gm => !gm.IsArchived && gm.GroupMemberStatus == GroupMemberStatus.Active );
            var peopleInGroups = groupMemberships.Join( groups,
                gm => gm.GroupId,
                g => g.Id,
                ( gm, g ) => gm.Person
            ).Distinct();

            var current_data = from p in people
                               join av in attr_vals on p.Id equals av.EntityId into pav_join
                               from pav in pav_join.DefaultIfEmpty()
                               join pig in peopleInGroups on p.Id equals pig.Id into pig_join
                               from gm in pig_join.DefaultIfEmpty()
                               select new PersonDataResult { p = p, current = pav == null ? false : pav.ValueAsBoolean.Value, actual = gm != null ? true : false };
            var needsUpdate = current_data.Where( cd => cd.current != cd.actual ).ToList();
            foreach ( var p in needsUpdate )
            {
                var update = p;
                var task = new Task( () =>
                {
                    update.p.LoadAttributes();
                    update.p.SetAttributeValue( attr.Key, update.actual.ToString() );
                    update.p.SaveAttributeValue( attr.Key );
                } );
                _personUpdates.Add( task );
                task.Start();
            }
        }

        /// <summary>
        /// Update person attributes based on the person having an active role in a group that is a leadership role.
        /// </summary>
        /// <param name="context">The DB Context to use</param>
        /// <param name="people">The list of people to check membership for</param>
        /// <param name="attr">The person attribute to be updated</param>
        private void ProcessPersonHasLeadershipRole( RockContext context, IQueryable<Person> people, Attribute attr )
        {
            AttributeValueService av_svc = new AttributeValueService( context );
            GroupService grp_svc = new GroupService( context );
            GroupMemberService gm_svc = new GroupMemberService( context );
            GroupTypeRoleService gtr_svc = new GroupTypeRoleService( context );
            DefinedValueService dv_svc = new DefinedValueService( context );

            var attr_vals = av_svc.Queryable().Where( av => av.AttributeId == attr.Id );

            var roles = gtr_svc.Queryable().Where( gtr => gtr.IsLeader );
            var groups = grp_svc.Queryable().Where( g => g.IsActive && !g.IsArchived );
            var groupMemberships = gm_svc.Queryable().Where( gm => !gm.IsArchived && gm.GroupMemberStatus == GroupMemberStatus.Active );
            var peopleInGroups = groupMemberships.Join( roles,
                gm => gm.GroupRoleId,
                r => r.Id,
                ( gm, r ) => gm
            ).Join( groups,
                gm => gm.GroupId,
                g => g.Id,
                ( gm, g ) => gm.Person
            ).Distinct();

            var current_data = from p in people
                               join av in attr_vals on p.Id equals av.EntityId into pav_join
                               from pav in pav_join.DefaultIfEmpty()
                               join pig in peopleInGroups on p.Id equals pig.Id into pig_join
                               from gm in pig_join.DefaultIfEmpty()
                               select new PersonDataResult { p = p, current = pav == null ? false : pav.ValueAsBoolean.Value, actual = gm != null ? true : false };
            var needsUpdate = current_data.Where( cd => cd.current != cd.actual ).ToList();
            foreach ( var p in needsUpdate )
            {
                var update = p;
                var task = new Task( () =>
                {
                    update.p.LoadAttributes();
                    update.p.SetAttributeValue( attr.Key, update.actual.ToString() );
                    update.p.SaveAttributeValue( attr.Key );
                } );
                _personUpdates.Add( task );
                task.Start();
            }
        }

        /// <summary>
        /// Method to find which schedules a person ususally serves during
        /// </summary>
        /// <param name="context">The DB Context to use</param>
        /// <param name="people">The list of people to find schedules for</param>
        /// <param name="attr">The person attribute to be updated</param>
        /// <param name="groupTypePurposeId">The id of the group type purpose that indicates a serving team</param>
        /// <param name="lowerDateRange">The earliest date to look at check-in data for</param>
        /// <param name="upperDateRange">The latest date to look at check-in data for</param>
        /// <param name="filterToScheduleId">The schedules to include in processing</param>
        private void ProcessPersonServingSchedules( RockContext context, IQueryable<Person> people, Attribute attr, int groupTypePurposeId, DateTime? lowerDateRange, DateTime? upperDateRange, List<int> filterToScheduleId )
        {
            AttributeValueService av_svc = new AttributeValueService( context );
            AttendanceService att_svc = new AttendanceService( context );
            AttendanceOccurrenceService ao_svc = new AttendanceOccurrenceService( context );
            GroupService grp_svc = new GroupService( context );

            var attr_vals = av_svc.Queryable().Where( av => av.AttributeId == attr.Id );
            var groups = grp_svc.Queryable().Where( g => g.IsActive && !g.IsArchived && g.GroupType.GroupTypePurposeValueId == groupTypePurposeId );
            var occurrences = ao_svc.Queryable().Where( ao =>
                ( !lowerDateRange.HasValue || ao.OccurrenceDate >= lowerDateRange ) &&
                ( !upperDateRange.HasValue || ao.OccurrenceDate <= upperDateRange ) &&
                ( !ao.DidNotOccur.HasValue || !ao.DidNotOccur.Value ) &&
                ( filterToScheduleId.Count == 0 || ( ao.ScheduleId.HasValue && filterToScheduleId.Contains( ao.ScheduleId.Value ) ) )
            );

            var servingOccurrences = occurrences.Join( groups,
                ao => ao.GroupId,
                g => g.Id,
                ( ao, g ) => ao
            );

            var servingAtt = att_svc.Queryable().Where( a =>
                a.DidAttend.HasValue && a.DidAttend.Value
            ).Join( servingOccurrences,
                a => a.OccurrenceId,
                ao => ao.Id,
                ( a, ao ) => new { att = a, occurrence = ao }
            );
            var personSchedules = servingAtt.GroupBy( grp =>
                new
                {
                    grp.att.PersonAlias.PersonId,
                    ScheduleGuid = grp.occurrence.Schedule.Guid
                }
            ).Select( grp =>
                new
                {
                    grp.Key.PersonId,
                    grp.Key.ScheduleGuid,
                    Num = grp.Count()
                }
            ).GroupBy( grp => grp.PersonId )
            .Select( grp =>
                new
                {
                    PersonId = grp.Key,
                    TotalCount = grp.Sum( sc => sc.Num ),
                    NumSchedules = grp.Count(),
                    ScheduleCounts = grp.Select( s =>
                        new
                        {
                            ScheduleGuid = s.ScheduleGuid.ToString(),
                            s.Num,
                            Percent = Math.Round( 100 * ( ( double ) s.Num / grp.Sum( sc => sc.Num ) ), 2 )
                        }
                    ).Where( s =>
                        s.Percent >= ( 100 / grp.Count() )
                    ).OrderByDescending( a => a.Percent ).Select( s => s.ScheduleGuid ).FirstOrDefault()
                }
            );

            var current_data = from p in people
                               join av in attr_vals on p.Id equals av.EntityId into pav_join
                               from pav in pav_join.DefaultIfEmpty()
                               join ps in personSchedules on p.Id equals ps.PersonId into ps_join
                               from sched in ps_join.DefaultIfEmpty()
                               select new { p = p, current = pav != null ? pav.Value : "", actual = sched != null ? sched.ScheduleCounts : null };
            var needsUpdate = current_data.Where( cd => cd.current != cd.actual ).ToList();
            foreach ( var p in needsUpdate )
            {
                var update = p;
                var task = new Task( () =>
                {
                    update.p.LoadAttributes();
                    update.p.SetAttributeValue( attr.Key, update.actual );
                    update.p.SaveAttributeValue( attr.Key );
                } );
                _personUpdates.Add( task );
                task.Start();
            }
        }
        /// <summary>
        /// Method to find which schedules a person ususally serves during
        /// </summary>
        /// <param name="context">The DB Context to use</param>
        /// <param name="people">The list of people to find schedules for</param>
        /// <param name="attr">The person attribute to be updated</param>
        /// <param name="groupTypePurposeId">The id of the group type purpose that indicates a serving team</param>
        /// <param name="lowerDateRange">The earliest date to look at check-in data for</param>
        /// <param name="upperDateRange">The latest date to look at check-in data for</param>
        /// <param name="filterToScheduleId">The schedules to include in processing</param>
        private void ProcessPersonServingFrequency( RockContext context, IQueryable<Person> people, Attribute attr, int groupTypePurposeId, DateTime? lowerDateRange, DateTime? upperDateRange, List<int> filterToScheduleId )
        {
            AttributeValueService av_svc = new AttributeValueService( context );
            AttendanceService att_svc = new AttendanceService( context );
            AttendanceOccurrenceService ao_svc = new AttendanceOccurrenceService( context );
            GroupService grp_svc = new GroupService( context );
            DateTime today = DateTime.Now;
            int year = today.Year;

            var attr_vals = av_svc.Queryable().Where( av => av.AttributeId == attr.Id );
            var groups = grp_svc.Queryable().Where( g => g.IsActive && !g.IsArchived && g.GroupType.GroupTypePurposeValueId == groupTypePurposeId );
            var occurrences = ao_svc.Queryable().Where( ao =>
                ( !lowerDateRange.HasValue || ao.OccurrenceDate >= lowerDateRange ) &&
                ( !upperDateRange.HasValue || ao.OccurrenceDate <= upperDateRange ) &&
                ( !ao.DidNotOccur.HasValue || !ao.DidNotOccur.Value ) &&
                ( filterToScheduleId.Count == 0 || ( ao.ScheduleId.HasValue && filterToScheduleId.Contains( ao.ScheduleId.Value ) ) )
            );

            var servingOccurrences = occurrences.Join( groups,
                ao => ao.GroupId,
                g => g.Id,
                ( ao, g ) => ao
            );

            var servingAtt = att_svc.Queryable().Where( a =>
                a.DidAttend.HasValue && a.DidAttend.Value
            ).Join( servingOccurrences,
                a => a.OccurrenceId,
                ao => ao.Id,
                ( a, ao ) => new { a.PersonAlias.PersonId, ao.OccurrenceDate }
            ).Distinct();

            var personDates = servingAtt.GroupBy( att =>
                new
                {
                    att.PersonId,
                    att.OccurrenceDate.Year,
                    att.OccurrenceDate.Month
                }
            ).Select( att =>
                new
                {
                    att.Key.PersonId,
                    att.Key.Year,
                    att.Key.Month,
                    FirstForMonth = att.Min( d => d.OccurrenceDate ),
                    LastForMonth = att.Max( d => d.OccurrenceDate ),
                    DaysServedInMonth = att.Count()
                }
            ).GroupBy( att =>
                new
                {
                    att.PersonId,
                    att.Year
                }
            ).Select( att =>
                new
                {
                    att.Key.PersonId,
                    att.Key.Year,
                    FirstForYear = att.Min( d => d.FirstForMonth ),
                    LastForYear = att.Max( d => d.LastForMonth ),
                    MonthsServedInYear = att.Count(),
                    AverageDaysServed = att.Average( d => d.DaysServedInMonth ),
                    TotalDaysServed = att.Sum( d => d.DaysServedInMonth )
                }
            ).Select( att =>
                new
                {
                    att.PersonId,
                    att.Year,
                    att.FirstForYear,
                    att.LastForYear,
                    att.MonthsServedInYear,
                    att.AverageDaysServed,
                    att.TotalDaysServed,
                    PossibleMonths = DbFunctions.DiffMonths( att.FirstForYear, att.LastForYear )
                }
            ).Select( att =>
                new
                {
                    att.PersonId,
                    att.Year,
                    att.FirstForYear,
                    att.LastForYear,
                    att.MonthsServedInYear,
                    att.AverageDaysServed,
                    att.TotalDaysServed,
                    att.PossibleMonths,
                    Frequency = (
                        att.MonthsServedInYear >= ( att.PossibleMonths - 2 ) && att.AverageDaysServed > 2 ? "Weekly" :
                        att.MonthsServedInYear >= ( att.PossibleMonths - 2 ) && att.AverageDaysServed > 1 ? "Bi-Monthly" :
                        att.MonthsServedInYear >= ( att.PossibleMonths - 2 ) ? "Monthly" :
                        att.MonthsServedInYear >= ( ( att.PossibleMonths + 1 ) / 2 ) ? "Semi-Monthly" :
                        att.MonthsServedInYear >= ( ( att.PossibleMonths + 1 ) / 4 ) ? "Quarterly" :
                        att.MonthsServedInYear >= ( ( att.PossibleMonths + 1 ) / 6 ) ? "Semi-Annually" :
                        "Sporadic"
                    )
                }
            ).GroupBy( att => att.PersonId )
            .Select( att =>
                new
                {
                    PersonId = att.Key,
                    Frequency = DbFunctions.DiffDays( att.Max( a => a.LastForYear ), today ) >= 90 ? "Sporadic" : att.OrderByDescending( a => a.Year ).Select( a => a.Frequency ).FirstOrDefault()
                }
            );

            var current_data = from p in people
                               join av in attr_vals on p.Id equals av.EntityId into pav_join
                               from pav in pav_join.DefaultIfEmpty()
                               join pd in personDates on p.Id equals pd.PersonId into pd_join
                               from freq in pd_join.DefaultIfEmpty()
                               select new { p = p, current = pav == null ? "" : pav.Value, actual = freq.Frequency };
            var needsUpdate = current_data.Where( cd => cd.current != cd.actual ).ToList();
            foreach ( var p in needsUpdate )
            {
                var update = p;
                var task = new Task( () =>
                {
                    update.p.LoadAttributes();
                    update.p.SetAttributeValue( attr.Key, update.actual );
                    update.p.SaveAttributeValue( attr.Key );
                } );
                _personUpdates.Add( task );
                task.Start();
            }
        }

        private class PersonDataResult
        {
            public Person p { get; set; }
            public bool current { get; set; }
            public bool actual { get; set; }
        }
        /// <summary>
        /// Method to get the list of families available for processing
        /// </summary>
        /// <param name="context">The DB Context to use</param>
        /// <param name="filterDataviewGuid">The guid of the dataview used to filter the list of families available for processing</param>
        /// <returns></returns>
        private IQueryable<Group> GetGroupsForProcessing( RockContext context, Guid? filterDataviewGuid )
        {
            IQueryable<Group> groups;
            GroupService grp_svc = new GroupService( context );
            GroupTypeService gt_svc = new GroupTypeService( context );
            DataViewService dataview_svc = new DataViewService( context );

            var familyGroupType = gt_svc.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );

            groups = grp_svc.Queryable().Where( g => g.IsActive && g.GroupTypeId == familyGroupType.Id && !g.IsArchived );

            if ( filterDataviewGuid.HasValue )
            {
                DataView filterDataview = dataview_svc.Get( filterDataviewGuid.Value );
                if ( filterDataview != null )
                {
                    var dataViewGetQueryArgs = new DataViewGetQueryArgs
                    {
                        DbContext = context
                    };
                    var qry = filterDataview.GetQuery( dataViewGetQueryArgs );
                    groups = groups.Join( qry,
                        p => p.Id,
                        q => q.Id,
                        ( p, q ) => p
                    );
                }
            }

            return groups;
        }

        /// <summary>
        /// Method to calculate the number of people in the familiy by age or grade
        /// </summary>
        /// <param name="context">The DB Context to use</param>
        /// <param name="groups">The list of groups to check membership for</param>
        /// <param name="attr">The family attribute to be updated</param>
        /// <param name="lowerAgeRange">The youngest a person can be to add to the count</param>
        /// <param name="uppderAgeRange">The oldest a person can be to add to the count</param>
        /// <param name="lowerGradYear">The lowest graduation year a person can have to be added to the count</param>
        /// <param name="upperGradYear">The highest graduation year a person can have to be added to the count</param>
        private void ProcessNumberOfPeopleInFamily( RockContext context, IQueryable<Group> groups, Attribute attr, int? lowerAgeRange, int? uppderAgeRange, int? lowerGradYear, int? upperGradYear )
        {
            AttributeValueService av_svc = new AttributeValueService( context );
            GroupMemberService gm_svc = new GroupMemberService( context );
            PersonService p_svc = new PersonService( context );

            var attr_vals = av_svc.Queryable().Where( av => av.AttributeId == attr.Id );

            IQueryable<Person> people = p_svc.Queryable().Where( p =>
                p.RecordTypeValueId == _personRecordTypeId &&
                p.RecordStatusValueId == _activePersonRecordStatusId &&
                ( !lowerAgeRange.HasValue || p.Age >= lowerAgeRange.Value ) &&
                ( !uppderAgeRange.HasValue || p.Age <= uppderAgeRange.Value ) &&
                ( !lowerGradYear.HasValue || ( p.GraduationYear.HasValue && p.GraduationYear.Value >= lowerGradYear.Value ) ) &&
                ( !upperGradYear.HasValue || ( !p.GraduationYear.HasValue || p.GraduationYear.Value <= upperGradYear.Value ) )
            );

            var membershipsInRange = gm_svc.Queryable().Where( gm => !gm.IsArchived && gm.GroupMemberStatus == GroupMemberStatus.Active ).Join( people,
                gm => gm.PersonId,
                p => p.Id,
                ( gm, p ) => gm
            );

            var grpResult = groups.Join( membershipsInRange,
                g => g.Id,
                gm => gm.GroupId,
                ( g, gm ) => new { g, gm }
            ).GroupBy( grp => grp.g.Id ).Select( grp => new { grp.Key, actual = grp.Count() } );


            var current_data = from g in groups
                               join r in grpResult on g.Id equals r.Key into grp_join
                               from grp in grp_join.DefaultIfEmpty()
                               join av in attr_vals on g.Id equals av.EntityId into gav_join
                               from gav in gav_join.DefaultIfEmpty()
                               select new GroupDataResult { g = g, current = gav == null ? 0 : ( int ) gav.ValueAsNumeric, actual = grp == null ? 0 : grp.actual };
            var needsUpdate = current_data.Where( cd => cd.current != cd.actual ).ToList();

            foreach ( var grp in needsUpdate )
            {
                var update = grp;
                var task = new Task( () =>
                {
                    update.g.LoadAttributes();
                    update.g.SetAttributeValue( attr.Key, update.actual.ToString() );
                    update.g.SaveAttributeValue( attr.Key );
                } );
                _groupUpdates.Add( task );
                task.Start();
            }
        }

        /// <summary>
        /// Method to calculate number of check-ins for a family
        /// </summary>
        /// <param name="context">The DB Context to use</param>
        /// <param name="groups">The list of groups to check membership for</param>
        /// <param name="attr">The family attribute to be updated</param>
        /// <param name="groupTypes">The group types that should be included</param>
        /// <param name="lowerDateRange">The earliest date to look at check-in data for</param>
        /// <param name="upperDateRange">The latest date to look at check-in data for</param>
        /// <param name="filterToScheduleId">The schedules to include in processing</param>
        /// <param name="groupByDate">Should the attendance records be grouped by c=occurrence date</param>
        private void ProcessNumberOfCheckings( RockContext context, IQueryable<Group> groups, Attribute attr, List<int> groupTypes, DateTime? lowerDateRange, DateTime? upperDateRange, List<int> filterToScheduleId, bool groupByDate = false )
        {
            AttributeValueService av_svc = new AttributeValueService( context );
            AttendanceOccurrenceService ao_avc = new AttendanceOccurrenceService( context );
            AttendanceService att_svc = new AttendanceService( context );
            GroupService grp_svc = new GroupService( context );

            var attr_vals = av_svc.Queryable().Where( av => av.AttributeId == attr.Id );
            var attendanceGroups = grp_svc.Queryable();
            if ( groupTypes != null && groupTypes.Count() > 0 )
            {
                attendanceGroups = attendanceGroups.Where( g => groupTypes.Contains( g.GroupTypeId ) );
            }

            var attendance = ao_avc.Queryable().Where( ao =>
                ( !lowerDateRange.HasValue || ao.OccurrenceDate >= lowerDateRange.Value ) &&
                ( !upperDateRange.HasValue || ao.OccurrenceDate <= upperDateRange.Value ) &&
                ( !ao.DidNotOccur.HasValue || !ao.DidNotOccur.Value ) &&
                ( filterToScheduleId.Count == 0 || ( ao.ScheduleId.HasValue && filterToScheduleId.Contains( ao.ScheduleId.Value ) ) )
            ).Join( attendanceGroups,
                ao => ao.GroupId,
                g => g.Id,
                ( ao, g ) => ao
            ).Join( att_svc.Queryable().Where( a => a.DidAttend.HasValue && a.DidAttend.Value ),
                ao => ao.Id,
                a => a.OccurrenceId,
                ( ao, a ) => new { occurrence = ao, attendance = a }
            ).GroupBy( a => a.attendance.SearchResultGroupId )
            .Select( grp => new { Id = grp.Key, Checkins = groupByDate ? grp.Select( e => e.occurrence ).GroupBy( ao => ao.OccurrenceDate ).Count() : grp.Count() } );

            var y = attendance.ToList();

            var current_data = from g in groups
                               join r in attendance on g.Id equals r.Id into grp_join
                               from grp in grp_join.DefaultIfEmpty()
                               join av in attr_vals on g.Id equals av.EntityId into gav_join
                               from gav in gav_join.DefaultIfEmpty()
                               select new GroupDataResult { g = g, current = gav == null ? 0 : ( int ) gav.ValueAsNumeric, actual = grp == null ? 0 : grp.Checkins };
            var x = current_data.ToList();
            var needsUpdate = current_data.Where( cd => cd.current != cd.actual ).ToList();

            foreach ( var grp in needsUpdate )
            {
                var update = grp;
                var task = new Task( () =>
                {
                    update.g.LoadAttributes();
                    update.g.SetAttributeValue( attr.Key, update.actual.ToString() );
                    update.g.SaveAttributeValue( attr.Key );
                } );
                _groupUpdates.Add( task );
                task.Start();
            }
        }

        /// <summary>
        /// Method to calculate number of miles a family lives from campus
        /// </summary>
        /// <param name="context">The DB Context to use</param>
        /// <param name="groups">The list of groups to check membership for</param>
        /// <param name="attr">The family attribute to be updated</param>
        private void ProcessDistanceFromCampus( RockContext context, IQueryable<Group> groups, Attribute attr )
        {
            LocationService loc_svc = new LocationService( context );
            AttributeValueService av_svc = new AttributeValueService( context );

            var attr_vals = av_svc.Queryable().Where( av => av.AttributeId == attr.Id );

            Guid? campusGuid = GetAttributeValue( AttributeKey.CampusLocation ).AsGuidOrNull();
            if ( campusGuid != null )
            {
                var mainCampus = loc_svc.Get( campusGuid.Value );
                GroupLocationService gl_svc = new GroupLocationService( context );
                var locations = gl_svc.Queryable().Where( gl => gl.GroupLocationTypeValueId == _homeLocationTypeId && gl.IsMappedLocation );

                var familiesWithMappedHomeAddr = groups.Join( locations,
                    g => g.Id,
                    gl => gl.GroupId,
                    ( g, gl ) => new { family = g, distance = gl != null && gl.Location != null && gl.Location.GeoPoint != null ? gl.Location.GeoPoint.Distance( mainCampus.GeoPoint ) : 0 }
                );

                var current_data = from g in groups
                                   join gl in locations on g.Id equals gl.GroupId into grp_join
                                   from grp in grp_join.DefaultIfEmpty()
                                   join av in attr_vals on g.Id equals av.EntityId into gav_join
                                   from gav in gav_join.DefaultIfEmpty()
                                   select new GroupDataResult { g = g, current = gav == null ? 0 : ( int ) gav.ValueAsNumeric, actual = grp != null && grp.Location != null && grp.Location.GeoPoint != null ? ( int ) ( grp.Location.GeoPoint.Distance( mainCampus.GeoPoint ) / Location.MetersPerMile ) : 0 };
                var needsUpdate = current_data.Where( cd => cd.current != cd.actual ).ToList();

                foreach ( var grp in needsUpdate )
                {
                    var update = grp;
                    var task = new Task( () =>
                    {
                        update.g.LoadAttributes();
                        update.g.SetAttributeValue( attr.Key, update.actual.ToString() );
                        update.g.SaveAttributeValue( attr.Key );
                    } );
                    _groupUpdates.Add( task );
                    task.Start();
                }
            }

        }

        private class GroupDataResult
        {
            public Group g { get; set; }
            public int current { get; set; }
            public int actual { get; set; }
        }
    }
}
