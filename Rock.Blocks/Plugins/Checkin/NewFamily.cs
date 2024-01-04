using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Entities;
using Rock.Web.Cache;

namespace Rock.Blocks.Plugins.Checkin
{
    /// <summary>
    /// Registration Entry.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "New Family Form" )]
    [Category( "Obsidian > Plugin > Check-in" )]
    [Description( "New Family Registration" )]
    [IconCssClass( "fas fa-house-user" )]

    #region Block Attributes
    [CustomDropdownListField(
        name: "Show Title",
        Key = AttributeKey.ShowTitle,
        ListSource = "ALL^Show for everyone,ADULT^Show for adults,CHILD^Show for children,HIDDEN^Hide field",
        DefaultValue = "HIDDEN",
        Category = "Basic Info",
        Order = 1
    )]
    [CustomDropdownListField(
        name: "Show NickName",
        Key = AttributeKey.ShowNickName,
        ListSource = "ALL^Show for everyone,ADULT^Show for adults,CHILD^Show for children,HIDDEN^Hide field",
        DefaultValue = "HIDDEN",
        Category = "Basic Info",
        Order = 2
    )]
    [CustomDropdownListField(
        name: "Show Middle Name",
        Key = AttributeKey.ShowMiddleName,
        ListSource = "ALL^Show for everyone,ADULT^Show for adults,CHILD^Show for children,HIDDEN^Hide field",
        DefaultValue = "HIDDEN",
        Category = "Basic Info",
        Order = 3
    )]
    [CustomDropdownListField(
        name: "Show Suffix",
        Key = AttributeKey.ShowSuffix,
        ListSource = "ALL^Show for everyone,ADULT^Show for adults,CHILD^Show for children,HIDDEN^Hide field",
        DefaultValue = "HIDDEN",
        Category = "Basic Info",
        Order = 4
    )]
    [DefinedValueField(
        name: "Default Connection Stauts",
        Key = AttributeKey.DefaultConnectionStatus,
        DefinedTypeGuid = SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
        Category = "Basic Info",
        Order = 5
    )]
    [CustomDropdownListField(
        name: "Require Connection Status",
        Key = AttributeKey.RequireConnectionStatus,
        ListSource = "ALL^Require for everyone,ADULT^Require for adults,CHILD^Require for children,NONE^Not required,HIDDEN^Hide field",
        DefaultValue = "HIDDEN",
        Category = "Basic Info",
        Order = 6
    )]
    [CustomDropdownListField(
        name: "Require Gender",
        Key = AttributeKey.RequireGender,
        ListSource = "ALL^Require for everyone,ADULT^Require for adults,CHILD^Require for children,NONE^Not required,HIDDEN^Hide field",
        DefaultValue = "CHILD",
        Category = "Basic Info",
        Order = 7
    )]
    [CustomDropdownListField(
        name: "Require Birthdate",
        Key = AttributeKey.RequireBirthDate,
        ListSource = "ALL^Require for everyone,ADULT^Require for adults,CHILD^Require for children,NONE^Not required,HIDDEN^Hide field",
        DefaultValue = "CHILD",
        Category = "Basic Info",
        Order = 8
    )]
    [CustomDropdownListField(
        name: "Require Grade or Ability Level",
        Key = AttributeKey.RequireGradeOrAbility,
        ListSource = "ALL^Require for everyone,ADULT^Require for adults,CHILD^Require for children,NONE^Not required,HIDDEN^Hide field",
        DefaultValue = "CHILD",
        Category = "Basic Info",
        Order = 9
    )]
    [CustomDropdownListField(
        name: "Show Marital Status Field",
        Key = AttributeKey.ShowMaritalStatus,
        ListSource = "ALL^Show for everyone,ADULT^Show for adults,CHILD^Show for children,HIDDEN^Hide field",
        DefaultValue = "ADULT",
        Category = "Basic Info",
        Order = 10
    )]
    [DefinedValueField(
        "Adult Default Marital Status",
        Key = AttributeKey.AdultDefaultMaritalStatus,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS,
        IsRequired = false,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED,
        Category = "Basic Info",
        Order = 11
    )]
    [DefinedValueField(
        "Child Default Marital Status",
        Key = AttributeKey.ChildDefaultMaritalStatus,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS,
        IsRequired = false,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE,
        Category = "Basic Info",
        Order = 12
    )]
    [DefinedTypeField(
        "Ability Level Defined Type",
        Key = AttributeKey.AbilityLevelDefinedType,
        IsRequired = false,
        Category = "Basic Info",
        Order = 13
    )]
    [AttributeField(
        "Ability Level Attribute",
        Key = AttributeKey.AbilityLevelAttribute,
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        IsRequired = false,
        Category = "Basic Info",
        Order = 14
    )]
    [DefinedTypeField(
        "Grade Defined Type",
        Key = AttributeKey.GradeDefinedType,
        IsRequired = false,
        Category = "Basic Info",
        Order = 15
    )]
    [BooleanField(
        "Show Email",
        Key = AttributeKey.ShowEmail,
        TrueText = "Yes",
        FalseText = "No",
        DefaultBooleanValue = false,
        Category = "Contact Info",
        Order = 1
    )]
    [BooleanField(
        "Show Email Opt Out",
        Key = AttributeKey.ShowEmailOptOut,
        TrueText = "Yes",
        FalseText = "No",
        DefaultBooleanValue = false,
        Category = "Contact Info",
        Order = 2
    )]
    [BooleanField(
        "Show Cell Phone Number",
        Key = AttributeKey.ShowCell,
        TrueText = "Yes",
        FalseText = "No",
        DefaultBooleanValue = true,
        Category = "Contact Info",
        Order = 3
    )]
    [BooleanField(
        "Show SMS Enabled Option",
        Key = AttributeKey.ShowSMSEnabled,
        TrueText = "Yes",
        FalseText = "No",
        DefaultBooleanValue = true,
        Category = "Contact Info",
        Order = 4
    )]
    [DefinedValueField(
        "Phone Number Type",
        Key = AttributeKey.MobileDefinedValue,
        DefinedTypeGuid = SystemGuid.DefinedType.PERSON_PHONE_TYPE,
        Category = "Contact Info",
        Order = 5
    )]
    [BooleanField(
        "Show Address",
        Key = AttributeKey.ShowAddress,
        TrueText = "Yes",
        FalseText = "No",
        DefaultBooleanValue = false,
        Category = "Contact Info",
        Order = 6
    )]
    [GroupField(
        "CK Desk STOP Group",
        Key = AttributeKey.CKDeskStopGroup,
        IsRequired = false,
        Category = "Contact Info",
        Order = 7
    )]
    [AttributeCategoryField(
        "Adult Attribute Categories",
        Key = AttributeKey.AdultAttributeCategories,
        AllowMultiple = true,
        EntityTypeName = "Person",
        EntityType = typeof( Person ),
        IsRequired = false,
        Category = "Additional Attributes",
        Order = 1
    )]
    [AttributeCategoryField(
        "Child Attribute Categories",
        Key = AttributeKey.ChildAttributeCategories,
        AllowMultiple = true,
        EntityTypeName = "Person",
        EntityType = typeof( Person ),
        IsRequired = false,
        Category = "Additional Attributes",
        Order = 2
    )]
    [GroupTypeField(
        "Check-in Group Type",
        Key = AttributeKey.CheckinGroupType,
        IsRequired = true,
        Category = "Check-in Group",
        Order = 1
    )]
    [AttributeField(
        "Start DOB Attribute",
        Key = AttributeKey.GroupAttrStartDOB,
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = "29",
        IsRequired = true,
        Category = "Check-in Group",
        Order = 2
    )]
    [AttributeField(
        "End DOB Attribute",
        Key = AttributeKey.GroupAttrEndDOB,
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = "29",
        IsRequired = true,
        Category = "Check-in Group",
        Order = 3
    )]
    [AttributeField(
        "Ability Level Attribute",
        Key = AttributeKey.GroupAttrAbility,
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = "186",
        IsRequired = true,
        Category = "Check-in Group",
        Order = 4
    )]
    [AttributeField(
        "Grade Attribute",
        Key = AttributeKey.GroupAttrGrade,
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        EntityTypeQualifierColumn = "GroupTypeId",
        EntityTypeQualifierValue = "186",
        IsRequired = true,
        Category = "Check-in Group",
        Order = 5
    )]
    [GroupField(
        "A Override Group",
        Key = AttributeKey.OverrideA,
        Category = "Check-in Group",
        Order = 6
    )]
    [GroupField(
        "B Override Group",
        Key = AttributeKey.OverrideB,
        Category = "Check-in Group",
        Order = 7
    )]
    [GroupField(
        "Multi-Age Group",
        Key = AttributeKey.MultiAge,
        Category = "Check-in Group",
        Order = 8
    )]
    [WorkflowTypeField(
        "Person Workflow(s)",
        Key = AttributeKey.PersonWorkflows,
        Description = "The workflow(s) to launch for every person added.",
        AllowMultiple = true,
        IsRequired = false,
        Category = "Workflows",
        Order = 1
    )]
    [WorkflowTypeField(
        "Adult Workflow(s)",
        Key = AttributeKey.AdultWorkflows,
        Description = "When Family group type, the workflow(s) to launch for every adult added.",
        AllowMultiple = true,
        IsRequired = false,
        Category = "Workflows",
        Order = 2
    )]
    [WorkflowTypeField(
        "Child Workflow(s)",
        Key = AttributeKey.ChildWorkflows,
        Description = "The workflow(s) to launch for every child added.",
        AllowMultiple = true,
        IsRequired = false,
        Category = "Workflows",
        Order = 3
    )]
    [WorkflowTypeField(
        "Group Workflow(s)",
        Key = AttributeKey.GroupWorkflows,
        Description = "The workflow(s) to launch for the group (family) that is added.",
        AllowMultiple = true,
        IsRequired = false,
        Category = "Workflows",
        Order = 4
    )]

    #endregion Block Attributes

    public class NewFamily : RockObsidianBlockType
    {
        #region Keys

        /// <summary>
        /// Attribute Key
        /// </summary>
        private static class AttributeKey
        {
            public const string ShowTitle = "ShowTitle";
            public const string ShowNickName = "ShowNickName";
            public const string ShowMiddleName = "ShowMiddleName";
            public const string ShowSuffix = "ShowSuffix";
            public const string DefaultConnectionStatus = "DefaultConnectionStatus";
            public const string RequireConnectionStatus = "RequireConnectionStatus";
            public const string RequireGender = "RequireGender";
            public const string RequireBirthDate = "RequireBirthDate";
            public const string RequireGradeOrAbility = "RequireGradeOrAbility";
            public const string ShowMaritalStatus = "ShowMaritalStatus";
            public const string AdultDefaultMaritalStatus = "AdultDefaultMaritalStatus";
            public const string ChildDefaultMaritalStatus = "ChildDefaultMaritalStatus";
            public const string AbilityLevelDefinedType = "AbilityLevelDefinedType";
            public const string AbilityLevelAttribute = "AbilityLevelAttribute";
            public const string GradeDefinedType = "GradeDefinedType";
            public const string ShowEmail = "ShowEmail";
            public const string ShowEmailOptOut = "ShowEmailOptOut";
            public const string ShowCell = "ShowCell";
            public const string ShowSMSEnabled = "ShowSMSEnabled";
            public const string MobileDefinedValue = "MobileDefinedValue";
            public const string ShowAddress = "ShowAddress";
            public const string CKDeskStopGroup = "CKDeskStopGroup";
            public const string AdultAttributeCategories = "AdultAttributeCategories";
            public const string ChildAttributeCategories = "ChildAttributeCategories";
            public const string CheckinGroupType = "CheckinGroupType";
            public const string GroupAttrStartDOB = "GroupAttrStartDOB";
            public const string GroupAttrEndDOB = "GroupAttrEndDOB";
            public const string GroupAttrAbility = "GroupAttrAbility";
            public const string GroupAttrGrade = "GroupAttrGrade";
            public const string OverrideA = "OverrideA";
            public const string OverrideB = "OverrideB";
            public const string MultiAge = "MultiAge";
            public const string PersonWorkflows = "PersonWorkflows";
            public const string AdultWorkflows = "AdultWorkflows";
            public const string ChildWorkflows = "ChildWorkflows";
            public const string GroupWorkflows = "GroupWorkflows";
        }

        /// <summary>
        /// Page Parameter
        /// </summary>
        private static class PageParameterKey
        {
            public const string ExistingPersonId = "Id";
            public const string ExistingPersonAlias = "Guid";
        }

        #endregion Keys

        #region Obsidian Block Type Overrides

        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetObsidianBlockInitialization()
        {
            SetProperties();
            DefinedValueService dv_svc = new DefinedValueService( context );
            DefinedTypeService dt_svc = new DefinedTypeService( context );
            AttributeService attr_svc = new AttributeService( context );
            PersonService per_svc = new PersonService( context );
            PersonAliasService alias_svc = new PersonAliasService( context );
            GroupService grp_svc = new GroupService( context );
            GroupTypeService gt_svc = new GroupTypeService( context );

            NewFamilyBlockViewModel viewModel = new NewFamilyBlockViewModel();

            viewModel.ShowTitle = GetAttributeValue( AttributeKey.ShowTitle );
            viewModel.TitleDefinedType = dt_svc.Get( Guid.Parse( SystemGuid.DefinedType.PERSON_TITLE ) );
            viewModel.ShowNickName = GetAttributeValue( AttributeKey.ShowNickName );
            viewModel.ShowMiddleName = GetAttributeValue( AttributeKey.ShowMiddleName );
            viewModel.ShowSuffix = GetAttributeValue( AttributeKey.ShowSuffix );
            viewModel.SuffixDefinedType = dt_svc.Get( Guid.Parse( SystemGuid.DefinedType.PERSON_SUFFIX ) );
            viewModel.ConnectionStatusDefinedType = dt_svc.Get( Guid.Parse( SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) );
            viewModel.DefaultConnectionStatusGuid = GetAttributeValue( AttributeKey.DefaultConnectionStatus ).AsGuid();
            viewModel.DefaultConnectionStatus = dv_svc.Get( viewModel.DefaultConnectionStatusGuid );
            viewModel.RequireConnectionStatus = GetAttributeValue( AttributeKey.RequireConnectionStatus );
            viewModel.RequireGender = GetAttributeValue( AttributeKey.RequireGender );
            viewModel.RequireBirthDate = GetAttributeValue( AttributeKey.RequireBirthDate );
            viewModel.RequireGradeOrAbility = GetAttributeValue( AttributeKey.RequireGradeOrAbility );
            viewModel.ShowMaritalStatus = GetAttributeValue( AttributeKey.ShowMaritalStatus );
            viewModel.MaritalStatusDefinedType = dt_svc.Get( SystemGuid.DefinedType.PERSON_MARITAL_STATUS );
            viewModel.AdultDefaultMaritalStatusGuid = GetAttributeValue( AttributeKey.AdultDefaultMaritalStatus ).AsGuid();
            viewModel.ChildDefaultMaritalStatusGuid = GetAttributeValue( AttributeKey.ChildDefaultMaritalStatus ).AsGuid();
            viewModel.DefaultAdultMaritalStatus = dv_svc.Get( viewModel.AdultDefaultMaritalStatusGuid );
            viewModel.DefaultChildMaritalStatus = dv_svc.Get( viewModel.ChildDefaultMaritalStatusGuid );
            viewModel.ShowEmail = GetAttributeValue( AttributeKey.ShowEmail ).AsBoolean();
            viewModel.ShowEmailOptOut = GetAttributeValue( AttributeKey.ShowEmailOptOut ).AsBoolean();
            viewModel.ShowCell = GetAttributeValue( AttributeKey.ShowCell ).AsBoolean();
            viewModel.ShowSMSEnabled = GetAttributeValue( AttributeKey.ShowSMSEnabled ).AsBoolean();
            viewModel.PhoneType = dv_svc.Get( GetAttributeValue( AttributeKey.MobileDefinedValue ).AsGuid() );
            viewModel.ShowAddress = GetAttributeValue( AttributeKey.ShowAddress ).AsBoolean();
            viewModel.ExistingPersonPhoneCantBeMessaged = false;
            viewModel.AdultAttributeCategories = adultAttributeCategories;
            viewModel.AdultAttributes = adultAttributes;
            viewModel.ChildAttributeCategories = childAttributeCategories;
            viewModel.ChildAttributes = childAttributes.Select( a => a.ToViewModel( null, true ) ).ToList();
            viewModel.AbilityLevelDefinedType = dt_svc.Get( GetAttributeValue( AttributeKey.AbilityLevelDefinedType ).AsGuid() );
            viewModel.AbilityLevelAttribute = attr_svc.Get( GetAttributeValue( AttributeKey.AbilityLevelAttribute ).AsGuid() );
            viewModel.GradeDefinedType = dt_svc.Get( GetAttributeValue( AttributeKey.GradeDefinedType ).AsGuid() );
            DateTime? gradeTransition = GlobalAttributesCache.Get().GetValue( "GradeTransitionDate" ).MonthDayStringAsDateTime();
            viewModel.GraduationYear = RockDateTime.Now.Year;
            if (RockDateTime.Now >= gradeTransition)
            {
                viewModel.GraduationYear++;
            }
            Guid? ckDeskStopGroupGuid = GetAttributeValue( AttributeKey.CKDeskStopGroup ).AsGuidOrNull();
            Rock.Model.Group ckDeskStopGroup = null;
            if (ckDeskStopGroupGuid != null)
            {
                ckDeskStopGroup = new GroupService( context ).Get( ckDeskStopGroupGuid.Value );
            }
            Guid? checkinGroupTypeGuid = GetAttributeValue( AttributeKey.CheckinGroupType ).AsGuidOrNull();
            if (checkinGroupTypeGuid.HasValue)
            {
                GroupType checkinGroupType = gt_svc.Get( checkinGroupTypeGuid.Value );
                var groups = grp_svc.GetByGroupTypeId( checkinGroupType.Id ).ToList();
                groups.LoadAttributes();
                viewModel.Groups = groups.Select( g => g.ToViewModel( null, true ) ).ToList();
            }
            viewModel.GroupStartDOBAttribute = attr_svc.Get( GetAttributeValue( AttributeKey.GroupAttrStartDOB ).AsGuid() ).ToViewModel();
            viewModel.GroupEndDOBAttribute = attr_svc.Get( GetAttributeValue( AttributeKey.GroupAttrEndDOB ).AsGuid() ).ToViewModel();
            viewModel.GroupAbilityAttribute = attr_svc.Get( GetAttributeValue( AttributeKey.GroupAttrAbility ).AsGuid() ).ToViewModel();
            viewModel.GroupGradeAttribute = groupGradeAttribute.ToViewModel();
            ExistingPersonId = PageParameter( PageParameterKey.ExistingPersonId ).AsIntegerOrNull();
            if (ExistingPersonId.HasValue)
            {
                Person p = per_svc.Get( ExistingPersonId.Value );
                viewModel.ExistingPerson = p.ToViewModel( null, true );
                if (mobileNumber != null)
                {
                    viewModel.ExistingPersonPhoneNumber = p.PhoneNumbers.FirstOrDefault( pn => pn.NumberTypeValueId == mobileNumber.Id ).ToViewModel( null, true );
                }
                if (ckDeskStopGroup != null)
                {
                    var exists = ckDeskStopGroup.Members.FirstOrDefault( gm => gm.PersonId == p.Id );
                    if (exists != null)
                    {
                        viewModel.ExistingPersonPhoneCantBeMessaged = true;
                    }
                }
            }
            else
            {
                Guid? existingPersonGuid = PageParameter( PageParameterKey.ExistingPersonAlias ).AsGuidOrNull();
                if (existingPersonGuid.HasValue)
                {
                    PersonAlias pa = alias_svc.Get( existingPersonGuid.Value );
                    if(pa != null)
                    {
                        Person p = pa.Person; // per_svc.Get( pa.PersonId );
                        viewModel.ExistingPerson = p.ToViewModel( null, true );
                        if (mobileNumber != null)
                        {
                            viewModel.ExistingPersonPhoneNumber = p.PhoneNumbers.FirstOrDefault( pn => pn.NumberTypeValueId == mobileNumber.Id ).ToViewModel( null, true );
                        }
                        if (ckDeskStopGroup != null)
                        {
                            var exists = ckDeskStopGroup.Members.FirstOrDefault( gm => gm.PersonId == p.Id );
                            if (exists != null)
                            {
                                viewModel.ExistingPersonPhoneCantBeMessaged = true;
                            }
                        }
                    }
                }
            }
            viewModel.EmptyPerson = new Person().ToViewModel( null, true );
            if (mobileNumber != null)
            {
                var phoneNumber = new PhoneNumber()
                {
                    NumberTypeValueId = mobileNumber.Id,
                    IsMessagingEnabled = true
                };
                viewModel.EmptyPersonPhoneNumber = phoneNumber.ToViewModel( null, true );
            }

            return viewModel;
        }

        #endregion Obsidian Block Type Overrides

        #region Properties

        private int? ExistingPersonId { get; set; }
        private DefinedValue mobileNumber { get; set; }
        private RockContext context { get; set; }
        private List<Guid> childAttributeCategories { get; set; }
        private List<Guid> adultAttributeCategories { get; set; }
        private List<Rock.Model.Attribute> childAttributes { get; set; }
        private Rock.Model.Attribute abilityAttribute { get; set; }
        private Rock.Model.Attribute groupGradeAttribute { get; set; }
        private List<Rock.Model.Attribute> adultAttributes { get; set; }

        #endregion

        #region Block Actions
        [BlockAction]
        public BlockActionResult ProcessFamily( List<PersonBag> parents, List<PersonBag> children, List<PhoneNumberBag> phonenumbers, List<GroupPlacement> placements )
        {
            try
            {
                SetProperties();
                Rock.Model.Group family = null;
                var groupType = GroupTypeCache.GetFamilyGroupType();
                GroupService grp_svc = new GroupService( context );
                DefinedValueService dv_svc = new DefinedValueService( context );
                PhoneNumberService phn_svc = new PhoneNumberService( context );
                Rock.Model.Group overrideA = grp_svc.Get( GetAttributeValue( AttributeKey.OverrideA ).AsGuid() );
                Rock.Model.Group overrideB = grp_svc.Get( GetAttributeValue( AttributeKey.OverrideB ).AsGuid() );
                Rock.Model.Group multiAge = grp_svc.Get( GetAttributeValue( AttributeKey.MultiAge ).AsGuid() );
                List<Person> people = new List<Person>();
                List<GroupMember> members = new List<GroupMember>();
                List<GroupBag> groups = new List<GroupBag>();
                DateTime? gradeTransition = GlobalAttributesCache.Get().GetValue( "GradeTransitionDate" ).MonthDayStringAsDateTime();
                int GraduationYear = RockDateTime.Now.Year;
                if (RockDateTime.Now >= gradeTransition)
                {
                    GraduationYear++;
                }
                for (int i = 0; i < parents.Count(); i++)
                {
                    Person p = FromViewModel( parents[i] );
                    if (p.Id > 0)
                    {
                        family = p.PrimaryFamily;
                        break;
                    }
                    else
                    {
                        context.People.Add( p );
                        context.SaveChanges();
                        people.Add( p );
                        for (int k = 0; k < adultAttributes.Count(); k++)
                        {
                            p.SaveAttributeValue( adultAttributes[k].Key, context );
                        }
                        if (family == null)
                        {
                            family = new Rock.Model.Group() { GroupTypeId = groupType.Id, Name = p.LastName };
                            context.Groups.Add( family );
                            context.SaveChanges();
                        }
                        if (!String.IsNullOrEmpty( phonenumbers[i].NumberFormatted ))
                        {
                            phonenumbers[i].PersonId = p.Id;
                            phonenumbers[i].Number = phonenumbers[i].NumberFormatted.Replace( "(", "" ).Replace( ")", "" ).Replace( "-", "" ).Replace( " ", "" );
                            PhoneNumber number = PhoneFromViewModel( phonenumbers[i] );
                            context.PhoneNumbers.Add( number );
                        }
                        var role = groupType.Roles.FirstOrDefault( gr => gr.Guid.ToString().ToUpper() == SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT );
                        AddFamilyMember( family, p, role );
                    }
                }
                childAttributes.Add( abilityAttribute );
                for (int i = 0; i < children.Count(); i++)
                {
                    Rock.Model.Group selected = grp_svc.Get( placements[i].SelectedGroup );
                    selected.LoadAttributes();
                    Person p = FromViewModel( children[i] );
                    var selectedGradeValue = selected.AttributeValues[groupGradeAttribute.Key];
                    if (selectedGradeValue != null && !String.IsNullOrEmpty( selectedGradeValue.Value ))
                    {
                        p.GraduationYear = GraduationYear + selectedGradeValue.Value.AsInteger();
                    }
                    context.People.Add( p );
                    context.SaveChanges();
                    people.Add( p );
                    for (int k = 0; k < childAttributes.Count(); k++)
                    {
                        p.SaveAttributeValue( childAttributes[k].Key, context );
                    }
                    var role = groupType.Roles.FirstOrDefault( gr => gr.Guid.ToString().ToUpper() == SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD );
                    AddFamilyMember( family, p, role );
                    //Group Placement
                    GroupMember checkinGroup = new GroupMember { PersonId = p.Id, GroupId = selected.Id, GroupRoleId = selected.GroupType.DefaultGroupRole.Id };
                    context.GroupMembers.Add( checkinGroup );
                    groups.Add( selected.ToViewModel() );
                    members.Add( checkinGroup );
                    if (placements[i].SelectedGroup != placements[i].CorrectGroup)
                    {
                        //Add to Override Group
                        if (selected.Name.EndsWith( "A" ))
                        {
                            GroupMember overrideGroup = new GroupMember
                            {
                                PersonId = p.Id,
                                GroupId = overrideA.Id,
                                GroupRoleId = overrideA.GroupType.DefaultGroupRole.Id
                            };
                            context.GroupMembers.Add( overrideGroup );
                            members.Add( overrideGroup );
                            groups.Add( overrideA.ToViewModel() );
                        }
                        else
                        {
                            GroupMember overrideGroup = new GroupMember
                            {
                                PersonId = p.Id,
                                GroupId = overrideB.Id,
                                GroupRoleId = overrideB.GroupType.DefaultGroupRole.Id
                            };
                            context.GroupMembers.Add( overrideGroup );
                            members.Add( overrideGroup );
                            groups.Add( overrideB.ToViewModel() );
                        }
                    }
                    if (multiAge != null && !String.IsNullOrEmpty( selectedGradeValue.Value ) && selectedGradeValue.Value.AsInteger() < 13)
                    {
                        //Add Elementary Kids to Multi-Age Group
                        GroupMember multiAgeGroup = new GroupMember
                        {
                            PersonId = p.Id,
                            GroupId = multiAge.Id,
                            GroupRoleId = multiAge.GroupType.DefaultGroupRole.Id
                        };
                        context.GroupMembers.Add( multiAgeGroup );
                        members.Add( multiAgeGroup );
                        groups.Add( multiAge.ToViewModel() );
                    }
                }
                context.SaveChanges();
                return ActionOk( new { people, members, groups } );
            }
            catch (Exception e)
            {
                ExceptionLogService.LogException( e );
                return ActionInternalServerError( e.Message );
            }
        }

        [BlockAction]
        public BlockActionResult CheckForExisting( PersonBag viewModel, string mobileNumber )
        {
            try
            {
                SetProperties();
                var newPerson = FromViewModel( viewModel );
                PersonService per_svc = new PersonService( context );
                var personQuery = new PersonService.PersonMatchQuery( newPerson.FirstName, newPerson.LastName, newPerson.Email, mobileNumber, newPerson.Gender, newPerson.BirthMonth, newPerson.BirthDay, newPerson.BirthYear );
                var person = per_svc.FindPerson( personQuery, false );
                if (person != null)
                {
                    return ActionOk( new { hasMatch = true, person } );
                }
                return ActionOk( new { hasMatch = false } );
            }
            catch (Exception e)
            {
                ExceptionLogService.LogException( e );
                return ActionInternalServerError( e.Message );
            }
        }

        #endregion Block Actions

        #region Helpers

        private Person FromViewModel( PersonBag viewModel )
        {
            Person p = new Person()
            {
                TitleValueId = viewModel.TitleValueId,
                FirstName = viewModel.FirstName,
                NickName = viewModel.NickName,
                MiddleName = viewModel.MiddleName,
                LastName = viewModel.LastName,
                SuffixValueId = viewModel.SuffixValueId,
                BirthDay = viewModel.BirthDay,
                BirthMonth = viewModel.BirthMonth,
                BirthYear = viewModel.BirthYear,
                Gender = (Gender) viewModel.Gender,
                GraduationYear = viewModel.GraduationYear,
                ConnectionStatusValueId = viewModel.ConnectionStatusValueId,
                MaritalStatusValueId = viewModel.MaritalStatusValueId
            };
            if (!String.IsNullOrEmpty( viewModel.IdKey ))
            {
                p = new PersonService( context ).Get( viewModel.IdKey );
            }
            p.LoadAttributes();
            foreach (KeyValuePair<string, string> av in viewModel.AttributeValues)
            {
                p.SetPublicAttributeValue( av.Key, av.Value, p, false );
            }

            return p;
        }

        private PhoneNumber PhoneFromViewModel( PhoneNumberBag viewModel )
        {
            PhoneNumber phoneNumber = new PhoneNumber()
            {
                PersonId = viewModel.PersonId,
                Number = viewModel.Number,
                NumberFormatted = viewModel.NumberFormatted,
                NumberTypeValueId = viewModel.NumberTypeValueId,
                IsMessagingEnabled = viewModel.IsMessagingEnabled,
                IsUnlisted = viewModel.IsUnlisted
            };
            return phoneNumber;
        }

        private void SetProperties()
        {
            context = new RockContext();
            DefinedValueService dv_svc = new DefinedValueService( context );
            AttributeService attr_svc = new AttributeService( context );
            Guid? mobileDefinedValueGuid = GetAttributeValue( AttributeKey.MobileDefinedValue ).AsGuidOrNull();
            if (mobileDefinedValueGuid.HasValue)
            {
                mobileNumber = dv_svc.Get( mobileDefinedValueGuid.Value );
            }
            adultAttributeCategories = GetAttributeValue( AttributeKey.AdultAttributeCategories ).SplitDelimitedValues( false ).AsGuidOrNullList().Where( g => g.HasValue ).Select( g => g.Value ).ToList();
            adultAttributes = attr_svc.Queryable().Where( attr => attr.Categories.Any( c => adultAttributeCategories.Contains( c.Guid ) ) ).ToList();
            childAttributeCategories = GetAttributeValue( AttributeKey.ChildAttributeCategories ).SplitDelimitedValues( false ).AsGuidOrNullList().Where( g => g.HasValue ).Select( g => g.Value ).ToList();
            childAttributes = attr_svc.Queryable().Where( attr => attr.Categories.Any( c => childAttributeCategories.Contains( c.Guid ) ) ).ToList().ToList();
            abilityAttribute = attr_svc.Get( GetAttributeValue( AttributeKey.AbilityLevelAttribute ).AsGuid() );
            groupGradeAttribute = attr_svc.Get( GetAttributeValue( AttributeKey.GroupAttrGrade ).AsGuid() );
        }

        private void AddFamilyMember( Rock.Model.Group family, Person person, GroupTypeRoleCache role )
        {
            GroupMember gm = new GroupMember()
            {
                GroupId = family.Id,
                PersonId = person.Id,
                GroupRoleId = role.Id,
                GroupMemberStatus = GroupMemberStatus.Active
            };
            context.GroupMembers.Add( gm );
        }

        #endregion Helpers

        public class NewFamilyBlockViewModel
        {
            public string ShowTitle { get; set; }
            public DefinedType TitleDefinedType { get; set; }
            public string ShowNickName { get; set; }
            public string ShowMiddleName { get; set; }
            public string ShowSuffix { get; set; }
            public DefinedType SuffixDefinedType { get; set; }
            public Guid DefaultConnectionStatusGuid { get; set; }
            public DefinedType ConnectionStatusDefinedType { get; set; }
            public DefinedValue DefaultConnectionStatus { get; set; }
            public string RequireConnectionStatus { get; set; }
            public string RequireGender { get; set; }
            public string RequireBirthDate { get; set; }
            public string RequireGradeOrAbility { get; set; }
            public string ShowMaritalStatus { get; set; }
            public DefinedType MaritalStatusDefinedType { get; set; }
            public Guid AdultDefaultMaritalStatusGuid { get; set; }
            public DefinedValue DefaultAdultMaritalStatus { get; set; }
            public Guid ChildDefaultMaritalStatusGuid { get; set; }
            public DefinedValue DefaultChildMaritalStatus { get; set; }
            public bool ShowEmail { get; set; }
            public bool ShowEmailOptOut { get; set; }
            public bool ShowCell { get; set; }
            public bool ShowSMSEnabled { get; set; }
            public DefinedValue PhoneType { get; set; }
            public bool ShowAddress { get; set; }
            public List<Guid> AdultAttributeCategories { get; set; }
            public List<Rock.Model.Attribute> AdultAttributes { get; set; }
            public List<Guid> ChildAttributeCategories { get; set; }
            public List<AttributeBag> ChildAttributes { get; set; }
            public DefinedType AbilityLevelDefinedType { get; set; }
            public Rock.Model.Attribute AbilityLevelAttribute { get; set; }
            public int GraduationYear { get; set; }
            public DefinedType GradeDefinedType { get; set; }
            public PersonBag ExistingPerson { get; set; }
            public PhoneNumberBag ExistingPersonPhoneNumber { get; set; }
            public bool ExistingPersonPhoneCantBeMessaged { get; set; }
            public PersonBag EmptyPerson { get; set; }
            public PhoneNumberBag EmptyPersonPhoneNumber { get; set; }
            public List<GroupBag> Groups { get; set; }
            public AttributeBag GroupStartDOBAttribute { get; set; }
            public AttributeBag GroupEndDOBAttribute { get; set; }
            public AttributeBag GroupAbilityAttribute { get; set; }
            public AttributeBag GroupGradeAttribute { get; set; }
        }

        public class GroupPlacement
        {
            public Guid CorrectGroup { get; set; }
            public Guid SelectedGroup { get; set; }
        }
    }
}
