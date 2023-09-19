using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace org.thecrossingchurch.CustomJobs.Jobs
{
    [GroupField( "One Time Event Group", "", true, order: 1, category: "Groups" )]
    [GroupField( "Studies Event Group", "", true, order: 2, category: "Groups" )]
    [GroupField( "Signed up for MG Group", "", true, order: 3, category: "Groups" )]
    [GroupField( "Disengaged Group", "", true, order: 4, category: "Groups" )]
    [GroupField( "Exclude Group", "", false, order: 6, category: "Groups" )]
    [GroupTypeField( "Group Type", "", true, order: 7, category: "Groups" )]
    [DateRangeField( "Spring", "", true, order: 1, category: "Timeframes" )]
    [DateRangeField( "Summer", "", true, order: 2, category: "Timeframes" )]
    [DateRangeField( "Fall", "", true, order: 3, category: "Timeframes" )]
    [CategoryField( "One Time Event Category", "", false, "Rock.Model.RegistrationTemplate", required: true, order: 1, category: "Events" )]
    [CategoryField( "Studies Event Category", "", false, "Rock.Model.RegistrationTemplate", required: true, order: 2, category: "Events" )]
    [RegistrationTemplateField( "Sign Up Template", "", true, order: 3, category: "Events" )]
    public class MensGroupSync : IJob
    {
        private Group _excludeGroup { get; set; }

        public MensGroupSync() { }

        public virtual void Execute( IJobExecutionContext context )
        {
            RockContext _context = new RockContext();
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            DateRange currentSpring = GetCurrentRange( dataMap.GetString( "Spring" ) );
            DateRange currentSummer = GetCurrentRange( dataMap.GetString( "Summer" ) );
            DateRange currentFall = GetCurrentRange( dataMap.GetString( "Fall" ) );
            DateTime today = RockDateTime.Now;
            DateTime startDate = RockDateTime.Now;
            if (currentSpring.Contains( today ))
            {
                startDate = currentSpring.Start.Value.AddYears( -1 );
            }
            else if (currentSummer.Contains( today ))
            {
                startDate = currentSummer.Start.Value.AddYears( -1 );
            }
            else
            {
                startDate = currentFall.Start.Value.AddYears( -1 );
            }

            Group oteGroup, studiesGroup, signupGroup, disengagedGroup;
            GroupType groupType;
            Category oteCategory, studiesCategory;
            RegistrationTemplate signUpTemplate;
            Guid? oteGroupGuid = dataMap.GetString( "OneTimeEventGroup" ).AsGuidOrNull();
            Guid? studiesGroupGuid = dataMap.GetString( "StudiesEventGroup" ).AsGuidOrNull();
            Guid? signupGroupGuid = dataMap.GetString( "SignedupforMGGroup" ).AsGuidOrNull();
            Guid? disengagedGroupGuid = dataMap.GetString( "DisengagedGroup" ).AsGuidOrNull();
            Guid? groupTypeGuid = dataMap.GetString( "GroupType" ).AsGuidOrNull();
            Guid? excludeGroupGuid = dataMap.GetString( "ExcludeGroup" ).AsGuidOrNull();
            Guid? oteCategoryGuid = dataMap.GetString( "OneTimeEventCategory" ).AsGuidOrNull();
            Guid? studiesCategoryGuid = dataMap.GetString( "StudiesEventCategory" ).AsGuidOrNull();
            Guid? signUpRegistrationTemplateGuid = dataMap.GetString( "SignUpTemplate" ).AsGuidOrNull();

            if (oteGroupGuid.HasValue && studiesGroupGuid.HasValue && signupGroupGuid.HasValue && disengagedGroupGuid.HasValue && groupTypeGuid.HasValue && oteCategoryGuid.HasValue && studiesCategoryGuid.HasValue && signUpRegistrationTemplateGuid.HasValue)
            {
                GroupService grp_svc = new GroupService( _context );
                GroupTypeService gt_svc = new GroupTypeService( _context );
                CategoryService cat_svc = new CategoryService( _context );
                RegistrationTemplateService reg_svc = new RegistrationTemplateService( _context );
                oteGroup = grp_svc.Get( oteGroupGuid.Value );
                studiesGroup = grp_svc.Get( studiesGroupGuid.Value );
                signupGroup = grp_svc.Get( signupGroupGuid.Value );
                disengagedGroup = grp_svc.Get( disengagedGroupGuid.Value );
                groupType = gt_svc.Get( groupTypeGuid.Value );
                oteCategory = cat_svc.Get( oteCategoryGuid.Value );
                studiesCategory = cat_svc.Get( studiesCategoryGuid.Value );
                signUpTemplate = reg_svc.Get( signUpRegistrationTemplateGuid.Value );
                if (excludeGroupGuid.HasValue)
                {
                    _excludeGroup = grp_svc.Get( excludeGroupGuid.Value );
                }
                UpdateSignUpGroup( signupGroup, startDate, signUpTemplate );
                List<Group> excludeGroups = grp_svc.GetByGroupTypeId( groupType.Id ).ToList();
                if (_excludeGroup != null)
                {
                    excludeGroups.Add( _excludeGroup );
                }
                excludeGroups.Add( signupGroup );
                UpdateDisengagedGroup( disengagedGroup, startDate, groupType, excludeGroups );
                UpdateEventGroups( studiesGroup, studiesCategory, startDate, excludeGroups );
                excludeGroups.Add( studiesGroup );
                UpdateEventGroups( oteGroup, oteCategory, startDate, excludeGroups );
            }
        }

        private DateRange GetCurrentRange( string values )
        {
            DateRange currentRange = DateRange.FromDelimitedValues( values );
            currentRange.Start = new DateTime( RockDateTime.Now.Year, currentRange.Start.Value.Month, currentRange.Start.Value.Day );
            currentRange.End = new DateTime( RockDateTime.Now.Year, currentRange.End.Value.Month, currentRange.End.Value.Day );
            return currentRange;
        }

        private void UpdateSignUpGroup( Group group, DateTime startDate, RegistrationTemplate template )
        {
            using (RockContext context = new RockContext())
            {
                RegistrationService reg_svc = new RegistrationService( context );
                RegistrationInstanceService ri_svc = new RegistrationInstanceService( context );
                RegistrationTemplatePlacementService rtp_svc = new RegistrationTemplatePlacementService( context );
                RelatedEntityService re_svc = new RelatedEntityService( context );
                GroupMemberService gm_svc = new GroupMemberService( context );
                List<int> excludeIds = new List<int>();
                if (_excludeGroup != null)
                {
                    excludeIds = _excludeGroup.Members.Select( gm => gm.Person.PrimaryAliasId.Value ).ToList();
                }
                RegistrationTemplatePlacement placement = rtp_svc.Queryable().FirstOrDefault( rtp => rtp.RegistrationTemplateId == template.Id );
                var registrationInstances = ri_svc.Queryable().Where( ri => ri.RegistrationTemplateId == template.Id );
                List<int> regIds = registrationInstances.Select( ri => ri.Id ).ToList();
                int groupEntityTypeId = EntityTypeCache.Get( Guid.Parse( Rock.SystemGuid.EntityType.GROUP ) ).Id;
                int registrationTemplatePlacementEntityTypeId = placement != null ? placement.TypeId : 0;
                int registrationInstanceEntityTypeId = registrationInstances.Count() > 0 ? registrationInstances.First().TypeId : 0;

                //Find Placement Groups and members of them
                var relatedEntities = re_svc.Queryable().Where( re => (re.PurposeKey == RelatedEntityPurposeKey.RegistrationTemplateGroupPlacementTemplate || re.PurposeKey == RelatedEntityPurposeKey.RegistrationInstanceGroupPlacement) && ((registrationTemplatePlacementEntityTypeId > 0 && re.SourceEntityTypeId == registrationTemplatePlacementEntityTypeId && re.SourceEntityId == placement.Id) || (registrationInstanceEntityTypeId > 0 && re.SourceEntityTypeId == registrationInstanceEntityTypeId && regIds.Contains( re.SourceEntityId ))) && re.TargetEntityTypeId == groupEntityTypeId ).Select( re => re.TargetEntityId ).Distinct();

                var groupMembers = gm_svc.Queryable().Join( relatedEntities,
                    gm => gm.GroupId,
                    re => re,
                    ( gm, re ) => gm
                ).Where( gm => gm.GroupMemberStatus != GroupMemberStatus.Inactive && gm.IsArchived == false ).Select( gm => gm.Person ).Distinct();

                //Registrants of event registration that have been modified recently and not yet placed
                var validRegistrations = reg_svc.Queryable()
                    .Join( registrationInstances,
                        r => r.RegistrationInstanceId,
                        ri => ri.Id,
                        ( r, ri ) => r
                    )
                    .Where( r => startDate <= r.ModifiedDateTime )
                    .SelectMany( r => r.Registrants ).Where( rr => rr.PersonAliasId.HasValue && !excludeIds.Contains( rr.PersonAliasId.Value ) ).Select( rr => rr.PersonAlias.Person ).Where( p => p.RecordStatusValueId == 3 && p.Gender == Gender.Male ).Distinct();

                var unplacedRegistrants =
                                            from registrant in validRegistrations
                                            join member in groupMembers on registrant.Id equals member.Id into data
                                            from groupMember in data.DefaultIfEmpty()
                                            select new
                                            {
                                                registrant,
                                                groupMember
                                            };
                unplacedRegistrants = unplacedRegistrants.Where( r => r.groupMember == null );
                UpdateGroupMembership( group, unplacedRegistrants.Select( r => r.registrant ) );
            }
        }

        private void UpdateEventGroups( Group group, Category category, DateTime startDate, List<Group> excludeGroups )
        {
            using (RockContext context = new RockContext())
            {
                RegistrationTemplateService rt_svc = new RegistrationTemplateService( context );
                GroupMemberService gm_svc = new GroupMemberService( context );
                var peopleRecentlyRegistered = rt_svc.Queryable().Where( rt => rt.CategoryId == category.Id ).SelectMany( rt => rt.Instances ).SelectMany( ri => ri.Registrations ).Where( r => startDate <= r.ModifiedDateTime ).SelectMany( r => r.Registrants ).Select( rr => rr.PersonAlias.Person );
                var groupIds = excludeGroups.Select( g => g.Id ).ToList();
                var membersOfExcludeGroups = gm_svc.Queryable().Where( gm => groupIds.Contains( gm.GroupId ) && gm.GroupMemberStatus != GroupMemberStatus.Inactive && !gm.IsArchived ).Select( gm => gm.Person ).Where( p => p.RecordStatusValueId == 3 ).Distinct();
                var registrantsJoinedGroupMembers = from p in peopleRecentlyRegistered
                                                    join gm in membersOfExcludeGroups on p.Id equals gm.Id into pgm
                                                    from joinData in pgm.DefaultIfEmpty()
                                                    select new
                                                    {
                                                        p,
                                                        gm = joinData
                                                    };
                var validRegistrants = registrantsJoinedGroupMembers.Where( rjgm => rjgm.gm == null ).Select( rjgm => rjgm.p ).Where( p => p.RecordStatusValueId == 3 && p.Gender == Gender.Male ).Distinct();
                UpdateGroupMembership( group, validRegistrants );
            }
        }

        private void UpdateDisengagedGroup( Group group, DateTime startDate, GroupType groupType, List<Group> excludeGroups )
        {
            using (RockContext context = new RockContext())
            {
                GroupMemberService gm_svc = new GroupMemberService( context );
                GroupService grp_svc = new GroupService( context );
                var groupIds = grp_svc.GetByGroupTypeId( groupType.Id ).Select( g => g.Id ).ToList();
                var previousMembers = gm_svc.Queryable().Where( gm => groupIds.Contains( gm.GroupId ) && startDate <= gm.CreatedDateTime ).Select( gm => gm.Person ).Distinct();
                var excludeGroupIds = excludeGroups.Select( g => g.Id ).ToList();
                var membersOfExcludeGroups = gm_svc.Queryable().Where( gm => excludeGroupIds.Contains( gm.GroupId ) && gm.GroupMemberStatus != GroupMemberStatus.Inactive && !gm.IsArchived ).Select( gm => gm.Person ).Where( p => p.RecordStatusValueId == 3 ).Distinct();
                var joinedGroupMembers = from p in previousMembers
                                         join gm in membersOfExcludeGroups on p.Id equals gm.Id into pgm
                                         from joinData in pgm.DefaultIfEmpty()
                                         select new
                                         {
                                             p,
                                             gm = joinData
                                         };
                var validPeople = joinedGroupMembers.Where( jgm => jgm.gm == null ).Select( jgm => jgm.p ).Where( p => p.RecordStatusValueId == 3 && p.Gender == Gender.Male ).Distinct();
                UpdateGroupMembership( group, validPeople );
            }
        }
        private void UpdateGroupMembership( Group group, IQueryable<Person> people )
        {
            using (RockContext context = new RockContext())
            {
                GroupService grp_svc = new GroupService( context );
                GroupMemberService gm_svc = new GroupMemberService( context );
                group = grp_svc.Get( group.Id );
                var personIds = people.Select( p => p.Id ).ToList();
                var deleteMembers = group.Members.Where( gm => !personIds.Contains( gm.PersonId ) );
                gm_svc.DeleteRange( deleteMembers );
                List<int> groupMemberIds = group.Members.Select( gm => gm.PersonId ).ToList();
                List<GroupMember> addMembers = people.Where( p => !groupMemberIds.Contains( p.Id ) ).ToList().Select( p =>
                {
                    return new GroupMember() { PersonId = p.Id, GroupId = group.Id, GroupRoleId = group.GroupType.DefaultGroupRoleId.Value, CreatedDateTime = RockDateTime.Now, ModifiedDateTime = RockDateTime.Now };
                } ).ToList();
                gm_svc.AddRange( addMembers );
                context.SaveChanges();
            }
        }
    }
}
