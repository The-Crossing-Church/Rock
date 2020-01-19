using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;

using com.bemadev.Checkin.Web.UI.Controls;
using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_bemadev.Checkin
{
    [DisplayName("Custom Family Registration")]
    [Category("com_bemadev > Checkin")]
    [Description("Family registration block designed for quicker entry.")]

    [CampusField("Campus", "The campus that the new people will be assigned to.", true, "", "", 0)]
    [AttributeField(Rock.SystemGuid.EntityType.PERSON, "Allergy Person Attribute", "A person attribute used for tracking allergies.", false, false, "", "", 1)]
    [AttributeField(Rock.SystemGuid.EntityType.PERSON, "Legal Notes Attribute", "A person attribute used for tracking legal notes.", false, false, "", "", 1)]
    [DefinedValueField(Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Connection Status", "The default connection status for new people.", true, false, "", "", 2)]
    [DefinedValueField(Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Selectable Connection Statuses", "The connection statuses that can be selected.", false, true, "", "", 2)]
    [BooleanField("Enable Common Last Name", "Auto populates Last Name from First person to each added people", true, order: 3)]
    [BooleanField("Enable Guest Addition", "Allows you to add a guest", true, order: 4)]
    [BooleanField("Child Role As Default", "Make Child the default role in the family instead of using Family Group Type default", true, order:5)]
    [BooleanField("Show Title", "Allow entry of person title", true, order:6)]
    [BooleanField("Show Nick Name", "Allow entry of nick name", true, order:7)]
    [BooleanField("Show Middle name", "Allow entry of middle name", true, order:8)]
    [BooleanField("Show Suffix", "Allow entry of suffix", true, order:9)]
    [BooleanField("Require Birthdate for Child Role", "Is birthdate field required if child role is selected?", true, order:10)]
    [BooleanField("Show Birthdate If Not Required", "Is birtdate shown if optional?", true, order: 11)]
    [BooleanField("Require Gender for Child Role", "Is gender required if child role is selected?", true, order: 11)]
    [BooleanField("Show Gender If Not Required", "Is gender shown if optional?", true, order: 11)]
    [BooleanField("Require Grade for Child Role", "Is grade required for child role", true, order:12)]
    [BooleanField("Show Grade for Child Role", "Is grade shown as an option for child role?", true, order: 11)]
    [BooleanField("Require Gender for Adult Role", "Is gender required if adult role is selected?", true, order:13)]
    [BooleanField("Require Mobile Phone for Adult Role", "Is mobile required or optional for adult role?", true, order:14)]
    [BooleanField("Show Mobile If Not Required", "Allow entry of mobile", true, order: 16)]
    [BooleanField("Require Email for Adult Role", "Is Email required for adult role?", false, order:15)]
    [BooleanField("Show Email If Not Required", "Allow entry of email", true, order:16)]
    [BooleanField("Show Address", "Is address shown?", false, order: 17)]
    [BooleanField("Require Address", "Is address required?", false, order:18)]
    [LinkedPage("Group Selector Page", "The page that allows you to place people in a group", true, order:19)]
    public partial class CustomFamilyRegistration : RockBlock
    {
        #region Fields

        private CampusCache _campus;

        private GroupTypeCache _groupType = null;
        private bool _isFamilyGroupType = true;
        protected string _groupTypeName = string.Empty;

        private string _homeLocationGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME;

        //private bool _confirmMaritalStatus = true;
        private bool _confirmMaritalStatus = false;
        private int _childRoleId = 0;
        private int _adultRoleId = 0;
        private DefinedValueCache _mobilePhone = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid());
        private bool _SMSEnabled = false;

        private string _groupSelectorPage = string.Empty;

        #endregion

        #region Properties

        protected List<NewGroupMember> NewGroupMembers { get; set; }

        protected bool NewMode { get; set; }

        #endregion

        #region Control Methods

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            NewMode = ViewState["NewMode"] as bool? ?? false;

            string json = ViewState["NewGroupMembers"] as string;
            if (string.IsNullOrWhiteSpace(json))
            {
                NewGroupMembers = new List<NewGroupMember>();
            }
            else
            {
                NewGroupMembers = JsonConvert.DeserializeObject<List<NewGroupMember>>(json);
            }

            CreateControls(false);
        }

        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["NewGroupMembers"] = JsonConvert.SerializeObject(NewGroupMembers, Formatting.None, jsonSetting);

            ViewState["NewMode"] = NewMode;

            return base.SaveViewState();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _campus = CampusCache.Get(GetAttributeValue("Campus").AsGuid());

            if (_campus != null)
            {
                lCampus.Text = _campus.Name;
            }

            _groupType = GroupTypeCache.Get(Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY);
            if (_groupType == null)
            {
                _groupType = GroupTypeCache.GetFamilyGroupType();
            }

            _groupTypeName = _groupType.Name;
            _isFamilyGroupType = _groupType.Guid.Equals(Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid());

            nfmMembers.ShowGrade = false;
            nfmMembers.ShowMiddleName = false;
            nfmMembers.ShowSuffix = false;
            nfmMembers.ShowTitle = false;
            //nfmMembers.RequireBirthdate = true;

            _childRoleId = _groupType.Roles
                .Where(r => r.Guid.Equals(Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid()))
                .Select(r => r.Id)
                .FirstOrDefault();

            _adultRoleId = _groupType.Roles
                .Where(r => r.Guid.Equals(Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()))
                .Select(r => r.Id)
                .FirstOrDefault();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var allergyAttribute = GetAttributeValue("AllergyPersonAttribute").AsGuidOrNull();
            var legalNotesAttribute = GetAttributeValue("LegalNotesAttribute").AsGuidOrNull();
            var existingPerson = PageParameter("ExistingPersonId").AsIntegerOrNull();

            if (_campus != null)
            {
                if (!Page.IsPostBack)
                {
                    if (existingPerson.HasValue)
                    {
                        NewMode = false;
                        var personService = new PersonService(new RockContext());
                        var person = personService.Get(existingPerson.Value);
                        ppExistingFamilyMember.SetValue(person);
                        btnModeNew.RemoveCssClass("active");
                        btnModeExisting.AddCssClass("active");
                    }
                    else
                    {
                        NewMode = true;
                    }

                    ppExistingFamilyMember.Visible = !NewMode;
                    ppExistingFamilyMember.Required = !NewMode;
                    
                    NewGroupMembers = new List<NewGroupMember>();
                    AddGroupMember();
                    CreateControls(true);
                }
                else
                {
                    ppExistingFamilyMember.Visible = !NewMode;
                    ppExistingFamilyMember.Required = !NewMode;
                    GetControlData();
                }
            }
            else
            {
                pnlFamily.Visible = false;
                lbAddNewFamily.Visible = false;
                pnlMessages.Visible = true;             
                lMessages.Text = "<div class='alert alert-warning'>Please configure the block settings.</div>";
            }
        }

        #endregion

        #region Methods

        private void CreateControls(bool setSelection)
        {
            var rockContext = new RockContext();

            nfmMembers.ClearRows();

            var groupMemberService = new GroupMemberService(rockContext);

            int defaultRoleId;

            if (GetAttributeValue("ChildRoleAsDefault").AsBoolean())
            {
                defaultRoleId = _childRoleId;
            }
            else
            {
                defaultRoleId = _groupType.DefaultGroupRoleId ?? _groupType.Roles.Select(r => r.Id).FirstOrDefault();
            }


            var attributeRequireAddress = GetAttributeValue("RequireAddress"); //True or False
            var attributeShowAddress = GetAttributeValue("ShowAddress"); //True or False

            var location = new Location();
            acAddress.Required = attributeRequireAddress.AsBoolean();
            acAddress.Visible = attributeShowAddress.AsBoolean();
            if ( acAddress.Visible )
            {
                acAddress.GetValues(location);
            }

            foreach (var member in NewGroupMembers)
            {
                string groupMemberGuidString = member.GroupMember.Person.Guid.ToString().Replace("-", "_");

                // Getting Block Attributes
                var attributeConnectionStatuses = GetAttributeValue("SelectableConnectionStatuses"); // Guid,Guid,Guid
                var attributeEnableGuestAddition = GetAttributeValue("EnableGuestAddition"); //True or False
                var attributeShowTitle = GetAttributeValue("ShowTitle"); //True or False
                var attributeShowNickName = GetAttributeValue("ShowNickName"); //True or False
                var attributeShowMiddleName = GetAttributeValue("ShowMiddleName"); //True or False
                var attributeShowSuffix = GetAttributeValue("ShowSuffix"); //True or False
                var attributeRequireBirthdateforChildRole = GetAttributeValue("RequireBirthdateforChildRole"); //True or False
                var attributeRequireGenderforChildRole = GetAttributeValue("RequireGenderforChildRole"); //True or False
                var attributeShowGradeforChildRole = GetAttributeValue("ShowGradeforChildRole"); //True or False
                var attributeRequireGradeforChildRole = GetAttributeValue("RequireGradeforChildRole"); //True or False
                var attributeShowEmail = GetAttributeValue("ShowEmailIfNotRequired"); //True or False
                var attributeRequireMobileforAdultRole = GetAttributeValue("RequireMobilePhoneforAdultRole"); //True or False
                var attributeRequireEmailforAdultRole = GetAttributeValue("RequireEmailforAdultRole"); //True or False
                var attributeRequireGenderforAdultRole = GetAttributeValue("RequireGenderforAdultRole"); //True or False
                var attributeAllergyPersonAttribute = GetAttributeValue("AllergyPersonAttribute");
                var attributeLegalNotesAttribute = GetAttributeValue("LegalNotesAttribute");
                var attributeShowBirthdate = GetAttributeValue("ShowBirthdateIfNotRequired");
                var attributeShowMobile = GetAttributeValue("ShowMobileIfNotRequired");
                var attributeShowGender = GetAttributeValue("ShowGenderIfNotRequired");

                // Creating new row
                var groupMemberRow = new NewFamilyMembersRow();

                // Passing properties into row
                groupMemberRow.AllowedConnectionStatuses = attributeConnectionStatuses;
                groupMemberRow.ShowGuest = attributeEnableGuestAddition.AsBoolean();

                // Assigning values
                groupMemberRow.GroupTypeId = _groupType.Id;
                nfmMembers.Controls.Add(groupMemberRow);
                groupMemberRow.ID = string.Format("row_{0}", groupMemberGuidString);
                groupMemberRow.RoleUpdated += groupMemberRow_RoleUpdated;
                groupMemberRow.GuestUpdated += groupMemberRow_GuestUpdated;
                groupMemberRow.DeleteClick += groupMemberRow_DeleteClick;
                groupMemberRow.PersonGuid = member.GroupMember.Person.Guid;
                groupMemberRow.RequireBirthdate = (member.GroupMember.GroupRoleId == _childRoleId && attributeRequireBirthdateforChildRole.AsBoolean());
                groupMemberRow.ShowBirthdate = (groupMemberRow.RequireBirthdate || attributeShowBirthdate.AsBoolean());
                groupMemberRow.RequireGender = (( member.GroupMember.GroupRoleId == _childRoleId && attributeRequireGenderforChildRole.AsBoolean() ) ||
                                                ( member.GroupMember.GroupRoleId == _adultRoleId && attributeRequireGenderforAdultRole.AsBoolean() ) );
                groupMemberRow.ShowGender = (groupMemberRow.RequireGender || attributeShowGender.AsBoolean());
                groupMemberRow.RequireGrade = (member.GroupMember.GroupRoleId == _childRoleId && attributeRequireGradeforChildRole.AsBoolean());
                groupMemberRow.ShowGradePicker = ((member.GroupMember.GroupRoleId == _childRoleId) && (attributeShowGradeforChildRole.AsBoolean()));

                groupMemberRow.RoleId = member.GroupMember.GroupRoleId;

                groupMemberRow.ShowTitle = attributeShowTitle.AsBoolean();
                groupMemberRow.ShowNickName = attributeShowNickName.AsBoolean();
                groupMemberRow.ShowMiddleName = attributeShowMiddleName.AsBoolean();
                groupMemberRow.ShowSuffix = attributeShowSuffix.AsBoolean();
                groupMemberRow.ShowEmail = (attributeShowEmail.AsBoolean() || (member.GroupMember.GroupRoleId == _adultRoleId && attributeRequireEmailforAdultRole.AsBoolean()));
                groupMemberRow.RequireEmail = (member.GroupMember.GroupRoleId == _adultRoleId && attributeRequireEmailforAdultRole.AsBoolean());
                groupMemberRow.RequireMobilePhone = (member.GroupMember.GroupRoleId == _adultRoleId && attributeRequireMobileforAdultRole.AsBoolean());
                groupMemberRow.ShowMobile = (groupMemberRow.RequireMobilePhone || attributeShowMobile.AsBoolean());

                groupMemberRow.ShowAllergy = ((member.GroupMember.GroupRoleId == _childRoleId) && (attributeAllergyPersonAttribute.AsGuidOrNull() != null));
                groupMemberRow.ShowLegalNotes = ((member.GroupMember.GroupRoleId == _childRoleId) && (attributeLegalNotesAttribute.AsGuidOrNull() != null));

                groupMemberRow.ValidationGroup = BlockValidationGroup;

                acAddress.Required = attributeRequireAddress.AsBoolean();

                if (_mobilePhone != null)
                {
                    var cellPhoneNumber = member.GroupMember.Person.PhoneNumbers.Where(p => p.NumberTypeValueId == _mobilePhone.Id).FirstOrDefault();
                    if (cellPhoneNumber != null)
                    {
                        groupMemberRow.MobilePhoneNumber = PhoneNumber.FormattedNumber(cellPhoneNumber.CountryCode, cellPhoneNumber.Number);
                    }
                    else
                    {
                        groupMemberRow.MobilePhoneNumber = string.Empty;
                    }
                }

                groupMemberRow.Email = member.GroupMember.Person.Email;

                if (setSelection)
                {
                    if (member.GroupMember.Person != null)
                    {
                        groupMemberRow.TitleValueId = member.GroupMember.Person.TitleValueId;
                        groupMemberRow.FirstName = member.GroupMember.Person.FirstName;
                        groupMemberRow.NickName = member.GroupMember.Person.NickName;
                        groupMemberRow.LastName = member.GroupMember.Person.LastName;
                        groupMemberRow.SuffixValueId = member.GroupMember.Person.SuffixValueId;
                        groupMemberRow.Gender = member.GroupMember.Person.Gender;
                        groupMemberRow.BirthDate = member.GroupMember.Person.BirthDate;
                        groupMemberRow.ConnectionStatusValueId = member.GroupMember.Person.ConnectionStatusValueId;
                        groupMemberRow.GradeOffset = member.GroupMember.Person.GradeOffset;
                        groupMemberRow.IsGuest = member.IsGuest;
                        groupMemberRow.Allergies = member.Allergies;
                        groupMemberRow.LegalNotes = member.LegalNotes;

                        //Chad - I added this!!
                        groupMemberRow.RoleId = member.GroupMember.GroupRoleId;
                    }
                }

            }

            ShowPage();
        }

        private void GetControlData()
        {
            NewGroupMembers = new List<NewGroupMember>();

            int? childMaritalStatusId = null;
            var childMaritalStatus = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE.AsGuid());
            if (childMaritalStatus != null)
            {
                //childMaritalStatus.Id = 144;
                childMaritalStatusId = childMaritalStatus.Id;
            }
            //int? adultMaritalStatusId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid()).Id;
            int? adultMaritalStatusId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE.AsGuid()).Id;

            int recordTypePersonId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()).Id;
            int recordStatusActiveId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid()).Id;

            foreach (NewFamilyMembersRow row in nfmMembers.FamilyMemberRows)
            {
                var groupMember = new GroupMember();
                groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                groupMember.Person = new Person();
                groupMember.Person.Guid = row.PersonGuid.Value;
                groupMember.Person.RecordTypeValueId = recordTypePersonId;
                groupMember.Person.RecordStatusValueId = recordStatusActiveId;

                if (row.RoleId.HasValue)
                {
                    groupMember.GroupRoleId = row.RoleId.Value;

                    if (_isFamilyGroupType)
                    {
                        if (groupMember.GroupRoleId == _childRoleId)
                        {
                            groupMember.Person.MaritalStatusValueId = childMaritalStatusId;
                        }
                        else
                        {
                            groupMember.Person.MaritalStatusValueId = adultMaritalStatusId;
                        }
                    }
                    else
                    {
                        groupMember.Person.MaritalStatusValueId = null;
                    }
                }

                groupMember.Person.TitleValueId = row.TitleValueId;
                groupMember.Person.FirstName = row.FirstName;
                groupMember.Person.NickName = row.NickName;
                groupMember.Person.LastName = row.LastName;
                groupMember.Person.SuffixValueId = row.SuffixValueId;
                groupMember.Person.Gender = row.Gender;

                var birthday = row.BirthDate;
                if (birthday.HasValue)
                {
                    // If setting a future birthdate, subtract a century until birthdate is not greater than today.
                    var today = RockDateTime.Today;
                    while (birthday.Value.CompareTo(today) > 0)
                    {
                        birthday = birthday.Value.AddYears(-100);
                    }
                    groupMember.Person.BirthMonth = birthday.Value.Month;
                    groupMember.Person.BirthDay = birthday.Value.Day;
                    if (birthday.Value.Year != DateTime.MinValue.Year)
                    {
                        groupMember.Person.BirthYear = birthday.Value.Year;
                    }
                    else
                    {
                        groupMember.Person.BirthYear = null;
                    }
                }
                else
                {
                    groupMember.Person.SetBirthDate(null);
                }

                groupMember.Person.ConnectionStatusValueId = row.ConnectionStatusValueId;

                if (_isFamilyGroupType)
                {
                    groupMember.Person.GradeOffset = row.GradeOffset;
                }

                var mobilePhone = row.MobilePhoneNumber;
                if (!string.IsNullOrWhiteSpace(mobilePhone))
                {
                    string mobileNumber = PhoneNumber.CleanNumber(mobilePhone);
                    if (!string.IsNullOrWhiteSpace(mobileNumber))
                    {
                        var mobilePhoneNumber = new PhoneNumber();
                        mobilePhoneNumber.NumberTypeValueId = _mobilePhone.Id;
                        mobilePhoneNumber.Number = mobileNumber;
                        mobilePhoneNumber.NumberFormatted = PhoneNumber.FormattedNumber(mobilePhoneNumber.CountryCode, mobileNumber);
                        mobilePhoneNumber.IsMessagingEnabled = true;
                        groupMember.Person.PhoneNumbers.Add(mobilePhoneNumber);
                    }
                }

                var email = row.Email;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    groupMember.Person.Email = row.Email;
                    groupMember.Person.IsEmailActive = true;
                    groupMember.Person.EmailPreference = EmailPreference.EmailAllowed;
                }

                groupMember.Person.LoadAttributes();

                NewGroupMember member = new NewGroupMember();
                member.GroupMember = groupMember;
                member.IsGuest = row.IsGuest;
                member.Allergies = row.Allergies;
                member.LegalNotes = row.LegalNotes;

                NewGroupMembers.Add(member);
            }
        }

        private void AddGroupMember()
        {
            int recordTypePersonId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()).Id;
            int recordStatusActiveId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid()).Id;
            var ConnectionStatusValue = DefinedValueCache.Get(GetAttributeValue("DefaultConnectionStatus").AsGuid());

            int defaultRoleId;
            if (GetAttributeValue("ChildRoleAsDefault").AsBoolean())
            {
                defaultRoleId = _childRoleId;
            }
            else
            {
                defaultRoleId = _groupType.DefaultGroupRoleId ?? _groupType.Roles.Select(r => r.Id).FirstOrDefault();
            }

            var person = new Person();
            person.Guid = Guid.NewGuid();
            person.RecordTypeValueId = recordTypePersonId;
            person.RecordStatusValueId = recordStatusActiveId;
            person.Gender = Gender.Unknown;
            person.ConnectionStatusValueId = (ConnectionStatusValue != null) ? ConnectionStatusValue.Id : (int?)null;

            var groupMember = new GroupMember();
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;
            groupMember.GroupRoleId = defaultRoleId;
            groupMember.Person = person;

            if (GetAttributeValue("EnableCommonLastName").AsBoolean())
            {
                if (NewGroupMembers.Count > 0)
                {
                    person.LastName = NewGroupMembers.FirstOrDefault().GroupMember.Person.LastName;
                }
            }

            NewGroupMember member = new NewGroupMember();
            member.GroupMember = groupMember;

            NewGroupMembers.Add(member);
        }

        private void ShowPage()
        {
        }

        private string GetLocationKey()
        {
            var location = new Location();
            acAddress.GetValues(location);
            return location.GetFullStreetAddress().Trim();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the RoleUpdated event of the groupMemberRow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void groupMemberRow_RoleUpdated(object sender, EventArgs e)
        {
            NewFamilyMembersRow row = sender as NewFamilyMembersRow;

            // Getting Block Attributes
            var attributeRequireBirthdateforChildRole = GetAttributeValue("RequireBirthdateforChildRole"); //True or False
            var attributeRequireGenderforChildRole = GetAttributeValue("RequireGenderforChildRole"); //True or False
            var attributeRequireGradeforChildRole = GetAttributeValue("RequireGradeforChildRole"); //True or False
            var attributeShowGradeforChildRole = GetAttributeValue("RequireShowforChildRole"); //True or False
            var attributeRequireMobileforAdultRole = GetAttributeValue("RequireMobilePhoneforAdultRole"); //True or False
            var attributeRequireEmailforAdultRole = GetAttributeValue("RequireEmailforAdultRole"); //True or False
            var attributeRequireGenderforAdultRole = GetAttributeValue("RequireGenderforAdultRole"); //True or False
            var attributeAllergyPersonAttribute = GetAttributeValue("AllergyPersonAttribute");
            var attributeLegalNotesAttribute = GetAttributeValue("LegalNotesAttribute");
            var attributeShowBirthdate = GetAttributeValue("ShowBirthdateIfNotRequired");
            var attributeShowMobile = GetAttributeValue("ShowMobileIfNotRequired");
            var attributeShowGender = GetAttributeValue("ShowGenderIfNotRequired");


            // Hiding and requiring vales based on RoleId
            row.RequireBirthdate = (row.RoleId == _childRoleId && attributeRequireBirthdateforChildRole.AsBoolean());
            row.ShowBirthdate = (row.RequireBirthdate || attributeShowBirthdate.AsBoolean());
            row.RequireGender = ((row.RoleId == _childRoleId && attributeRequireGenderforChildRole.AsBoolean()) ||
                                            (row.RoleId == _adultRoleId && attributeRequireGenderforAdultRole.AsBoolean()));
            row.ShowGender = (row.RequireGender || attributeShowGender.AsBoolean());
            row.RequireGrade = (row.RoleId == _childRoleId && attributeRequireGradeforChildRole.AsBoolean());
            row.ShowGradePicker = ((row.RoleId == _childRoleId) && (attributeShowGradeforChildRole.AsBoolean()));

            row.RequireEmail = (row.RoleId == _adultRoleId && attributeRequireEmailforAdultRole.AsBoolean());
            row.ShowEmail = (row.RequireEmail || attributeShowMobile.AsBoolean());
            row.RequireMobilePhone = (row.RoleId == _adultRoleId && attributeRequireMobileforAdultRole.AsBoolean());
            row.ShowMobile = (row.RequireMobilePhone || attributeShowMobile.AsBoolean());

            row.ShowAllergy = ((row.RoleId == _childRoleId) && (attributeAllergyPersonAttribute.AsGuidOrNull() != null));
            row.ShowLegalNotes = ((row.RoleId == _childRoleId) && (attributeLegalNotesAttribute.AsGuidOrNull() != null));

        }

        void groupMemberRow_GuestUpdated(object sender, EventArgs e)
        {
            // check to see if we should display the guest address field
            bool hasGuests = false;

            foreach (var row in nfmMembers.FamilyMemberRows)
            {
                if (row.IsGuest == true)
                {
                    hasGuests = true;
                }
            }

            acGuestAddress.Visible = hasGuests;
        }

        /// <summary>
        /// Handles the DeleteClick event of the groupMemberRow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void groupMemberRow_DeleteClick(object sender, EventArgs e)
        {
            NewFamilyMembersRow row = sender as NewFamilyMembersRow;
            var groupMember = NewGroupMembers.FirstOrDefault(m => m.GroupMember.Person.Guid.Equals(row.PersonGuid));
            if (groupMember != null)
            {
                NewGroupMembers.Remove(groupMember);
            }

            CreateControls(true);

            // check to see if we should display the guest address field
            bool hasGuests = false;

            foreach (var member in nfmMembers.FamilyMemberRows)
            {
                if (member.IsGuest == true)
                {
                    hasGuests = true;
                }
            }

            acGuestAddress.Visible = hasGuests;
        }

        protected void nfmMembers_AddFamilyMemberClick(object sender, EventArgs e)
        {
            AddGroupMember();
            CreateControls(true);
        }

        protected void btnModeNew_Click(object sender, EventArgs e)
        {
            NewMode = true;

            ppExistingFamilyMember.Visible = !NewMode;
            ppExistingFamilyMember.Required = !NewMode;
            btnModeNew.AddCssClass("active");
            btnModeExisting.RemoveCssClass("active");
        }

        protected void btnModeExisting_Click(object sender, EventArgs e)
        {
            NewMode = false;
            ppExistingFamilyMember.Visible = !NewMode;
            ppExistingFamilyMember.Required = !NewMode;
            btnModeNew.RemoveCssClass("active");
            btnModeExisting.AddCssClass("active");
        }


        protected void lbSave_Click(object sender, EventArgs e)
        {
            var messages = new StringBuilder();
            messages.AppendLine("Success!<br />");
            messages.AppendLine("<br />");

            var rockContext = new RockContext();
            var personService = new PersonService(rockContext);
            var memberService = new GroupMemberService(rockContext);
            var allergyAttribute = AttributeCache.Get(GetAttributeValue("AllergyPersonAttribute").AsGuid());
            var legalNotesAttribute = AttributeCache.Get(GetAttributeValue("LegalNotesAttribute").AsGuid());
            var attributeValueService = new AttributeValueService(rockContext);
            var canCheckinRelationshipId = new GroupTypeRoleService(rockContext).Get(Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid()).Id;

            var familyMembers = NewGroupMembers.Where(g => g.IsGuest == false).Select(g => g.GroupMember).ToList();
            var guests = NewGroupMembers.Where(g => g.IsGuest == true).Select(g => g.GroupMember).ToList();

            if (familyMembers.Any() || guests.Any())
            {
                if (NewMode)
                {
                    // save the family members as their own family
                    Group familyGroup = null;
                    familyGroup = GroupService.SaveNewFamily(rockContext, familyMembers, _campus.Id, true);

                    messages.AppendLine(string.Format("New Family added: {0}<br />", familyGroup.Name));
                    foreach (var member in familyMembers)
                    {
                        messages.AppendLine(string.Format("&nbsp;&nbsp;&nbsp;&nbsp;Family Member: {0}<br />", member.Person.FullName));
                    }

                    // save family address
                    var location = new LocationService(rockContext).Get(acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country);
                    GroupService.AddNewGroupAddress(rockContext, familyGroup, _homeLocationGuid, location);

                    // save the guests as their own family
                    if (guests.Any())
                    {
                        Group guestGroup = null;
                        guestGroup = GroupService.SaveNewFamily(rockContext, guests, _campus.Id, true);

                        messages.AppendLine("");
                        messages.AppendLine(string.Format("New Guest Family added: {0}<br />", guestGroup.Name));
                        foreach (var member in guests)
                        {
                            messages.AppendLine(string.Format("&nbsp;&nbsp;&nbsp;&nbsp;Family Member: {0}<br />", member.Person.FullName));
                        }

                        // save guest address
                        var guestLocation = new LocationService(rockContext).Get(acGuestAddress.Street1, acGuestAddress.Street2, acAddress.City, acGuestAddress.State, acGuestAddress.PostalCode, acGuestAddress.Country);
                        GroupService.AddNewGroupAddress(rockContext, guestGroup, _homeLocationGuid, guestLocation);

                        // add can check-in relationship to guests that links to the oldest family member entered
                        var head = familyMembers
                            .OrderBy(f => f.GroupRoleId)
                            .ThenBy(f => f.Person.Gender)
                            .FirstOrDefault();

                        foreach (var guest in guests)
                        {
                            memberService.CreateKnownRelationship(head.Person.Id, guest.Person.Id, canCheckinRelationshipId);
                        }
                    }

                    // save allergy attribute
                    foreach (var member in NewGroupMembers)
                    {
                        if (!string.IsNullOrWhiteSpace(member.Allergies))
                        {
                            var attributeValue = attributeValueService.Queryable().Where(v => v.AttributeId == allergyAttribute.Id && v.EntityId == member.GroupMember.Person.Id).FirstOrDefault();
                            if (attributeValue == null)
                            {
                                attributeValue = new AttributeValue();
                                attributeValue.EntityId = member.GroupMember.Person.Id;
                                attributeValue.AttributeId = allergyAttribute.Id;
                                attributeValueService.Add(attributeValue);
                            }
                            attributeValue.Value = member.Allergies;
                        }

                        // save legal notes attribute
                        if (!string.IsNullOrWhiteSpace(member.LegalNotes))
                        {
                            var attributeValue = attributeValueService.Queryable().Where(v => v.AttributeId == legalNotesAttribute.Id && v.EntityId == member.GroupMember.Person.Id).FirstOrDefault();
                            if (attributeValue == null)
                            {
                                attributeValue = new AttributeValue();
                                attributeValue.EntityId = member.GroupMember.Person.Id;
                                attributeValue.AttributeId = legalNotesAttribute.Id;
                                attributeValueService.Add(attributeValue);
                            }
                            attributeValue.Value = member.LegalNotes;
                        }
                    }

                    rockContext.SaveChanges();
                }
                else // existing mode
                {
                    // add any new family members to the existing family member's family
                    var existingFamilyMember = personService.Get(ppExistingFamilyMember.PersonId.Value);
                    var existingFamily = existingFamilyMember.GetFamily(rockContext);

                    if (familyMembers.Any())
                    {
                        foreach (var member in familyMembers)
                        {
                            existingFamily.Members.Add(member);
                        }
                        rockContext.SaveChanges();

                        // save updated address
                        var location = new LocationService(rockContext).Get(acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country);
                        GroupService.AddNewGroupAddress(rockContext, existingFamily, _homeLocationGuid, location);

                    }

                    // save the guests as their own family
                    if (guests.Any())
                    {
                        Group guestGroup = null;
                        guestGroup = GroupService.SaveNewFamily(rockContext, guests, _campus.Id, true);

                        // save guest address
                        var guestLocation = new LocationService(rockContext).Get(acGuestAddress.Street1, acGuestAddress.Street2, acAddress.City, acGuestAddress.State, acGuestAddress.PostalCode, acGuestAddress.Country);
                        GroupService.AddNewGroupAddress(rockContext, guestGroup, _homeLocationGuid, guestLocation);

                        // add can check-in relationship to guests
                        foreach (var guest in guests)
                        {
                            memberService.CreateKnownRelationship(existingFamilyMember.Id, guest.Person.Id, canCheckinRelationshipId);
                        }
                    }

                    // save allergy attribute
                    foreach (var member in NewGroupMembers)
                    {
                        if (!string.IsNullOrWhiteSpace(member.Allergies))
                        {
                            var attributeValue = attributeValueService.Queryable().Where(v => v.AttributeId == allergyAttribute.Id && v.EntityId == member.GroupMember.Person.Id).FirstOrDefault();
                            if (attributeValue == null)
                            {
                                attributeValue = new AttributeValue();
                                attributeValue.EntityId = member.GroupMember.Person.Id;
                                attributeValue.AttributeId = allergyAttribute.Id;
                                attributeValueService.Add(attributeValue);
                            }
                            attributeValue.Value = member.Allergies;
                        }

                        // save legal notes attribute
                        if (!string.IsNullOrWhiteSpace(member.LegalNotes))
                        {
                            var attributeValue = attributeValueService.Queryable().Where(v => v.AttributeId == legalNotesAttribute.Id && v.EntityId == member.GroupMember.Person.Id).FirstOrDefault();
                            if (attributeValue == null)
                            {
                                attributeValue = new AttributeValue();
                                attributeValue.EntityId = member.GroupMember.Person.Id;
                                attributeValue.AttributeId = legalNotesAttribute.Id;
                                attributeValueService.Add(attributeValue);
                            }
                            attributeValue.Value = member.LegalNotes;
                        }
                    }

                    rockContext.SaveChanges();
                }
            }

            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            List<int> personIds = new List<int>();
            foreach (var member in NewGroupMembers)
            {
                if (member.GroupMember.GroupRoleId == _childRoleId)
                {
                    personIds.Add(member.GroupMember.Person.Id);
                }
            }

            // Checking to see if any children were found
            if (personIds.Count > 0)
            {
                string personIdsString = string.Join(",", personIds);
                queryParams.Add("PersonIds", personIdsString);
                NavigateToLinkedPage("GroupSelectorPage", queryParams);
            }
            else
            {
                string message = string.Format("<div class='alert alert-success'>Success! Click <a href='/Person/{0}'>Here</a> to view the Person record.</div>",
                                                NewGroupMembers[0].GroupMember.Person.Id.ToString()
                                            );

                lMessages.Text = message;
            }

            pnlFamily.Visible = false;
            pnlMessages.Visible = true;
        }

        protected void lbAddNewFamily_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request.Url.AbsolutePath, false);
        }

        #endregion
    }

    public class NewGroupMember
    {
        public bool IsGuest = false;

        public string Allergies = "";

        public string LegalNotes = "";

        public GroupMember GroupMember;
    }
}