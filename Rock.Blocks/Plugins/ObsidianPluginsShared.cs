using System.Linq;
using Rock.Blocks.Plugins.ViewModels;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Blocks.Plugins.EventForm
{
    public class ObsidianPluginsShared
    {
        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="ContentChannelItem"/> that represents the entity.</returns>
        public ContentChannelItemBag GetCommonContentChannelItemEntityBag( ContentChannelItem entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new ContentChannelItemBag
            {
                IdKey = entity.IdKey,
                ApprovedByPersonAliasId = entity.ApprovedByPersonAliasId,
                ApprovedDateTime = entity.ApprovedDateTime,
                Content = entity.Content,
                ContentChannelId = entity.ContentChannelId,
                ContentChannelTypeId = entity.ContentChannelTypeId,
                ExpireDateTime = entity.ExpireDateTime,
                ItemGlobalKey = entity.ItemGlobalKey,
                Order = entity.Order,
                Permalink = entity.Permalink,
                Priority = entity.Priority,
                StartDateTime = entity.StartDateTime,
                Status = ( int ) entity.Status,
                StructuredContent = entity.StructuredContent,
                Title = entity.Title,
                CreatedDateTime = entity.CreatedDateTime,
                ModifiedDateTime = entity.ModifiedDateTime,
                CreatedByPersonAliasId = entity.CreatedByPersonAliasId,
                ModifiedByPersonAliasId = entity.ModifiedByPersonAliasId
            };
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="Person"/> that represents the entity.</returns>
        public PersonBag GetCommonPersonEntityBag( Person entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new PersonBag
            {
                IdKey = entity.IdKey,
                AccountProtectionProfile = ( int ) entity.AccountProtectionProfile,
                AgeClassification = ( int ) entity.AgeClassification,
                AnniversaryDate = entity.AnniversaryDate,
                BirthDateKey = entity.BirthDateKey,
                BirthDay = entity.BirthDay,
                BirthMonth = entity.BirthMonth,
                BirthYear = entity.BirthYear,
                CommunicationPreference = ( int ) entity.CommunicationPreference,
                ConnectionStatusValueId = entity.ConnectionStatusValueId,
                ContributionFinancialAccountId = entity.ContributionFinancialAccountId,
                DeceasedDate = entity.DeceasedDate,
                Email = entity.Email,
                EmailNote = entity.EmailNote,
                EmailPreference = ( int ) entity.EmailPreference,
                EthnicityValueId = entity.EthnicityValueId,
                FirstName = entity.FirstName,
                Gender = ( int ) entity.Gender,
                GivingGroupId = entity.GivingGroupId,
                GivingLeaderId = entity.GivingLeaderId,
                GraduationYear = entity.GraduationYear,
                InactiveReasonNote = entity.InactiveReasonNote,
                IsDeceased = entity.IsDeceased,
                IsEmailActive = entity.IsEmailActive,
                IsLockedAsChild = entity.IsLockedAsChild,
                IsSystem = entity.IsSystem,
                LastName = entity.LastName,
                MaritalStatusValueId = entity.MaritalStatusValueId,
                MiddleName = entity.MiddleName,
                NickName = entity.NickName,
                PhotoId = entity.PhotoId,
                PreferredLanguageValueId = entity.PreferredLanguageValueId,
                PrimaryCampusId = entity.PrimaryCampusId,
                PrimaryFamilyId = entity.PrimaryFamilyId,
                RaceValueId = entity.RaceValueId,
                RecordStatusLastModifiedDateTime = entity.RecordStatusLastModifiedDateTime,
                RecordStatusReasonValueId = entity.RecordStatusReasonValueId,
                RecordStatusValueId = entity.RecordStatusValueId,
                RecordTypeValueId = entity.RecordTypeValueId,
                ReminderCount = entity.ReminderCount,
                ReviewReasonNote = entity.ReviewReasonNote,
                ReviewReasonValueId = entity.ReviewReasonValueId,
                SuffixValueId = entity.SuffixValueId,
                SystemNote = entity.SystemNote,
                TitleValueId = entity.TitleValueId,
                TopSignalColor = entity.TopSignalColor,
                TopSignalIconCssClass = entity.TopSignalIconCssClass,
                TopSignalId = entity.TopSignalId,
                ViewedCount = entity.ViewedCount,
                CreatedDateTime = entity.CreatedDateTime,
                ModifiedDateTime = entity.ModifiedDateTime,
                CreatedByPersonAliasId = entity.CreatedByPersonAliasId,
                ModifiedByPersonAliasId = entity.ModifiedByPersonAliasId
            };
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="Model.Attribute"/> that represents the entity.</returns>
        public AttributeBag GetCommonAttributeEntityBag( Model.Attribute entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var attributeCache = AttributeCache.Get( entity.Id );

            return new AttributeBag
            {
                IdKey = entity.IdKey,
                AbbreviatedName = entity.AbbreviatedName,
                AttributeColor = entity.AttributeColor,
                DefaultPersistedCondensedHtmlValue = entity.DefaultPersistedCondensedHtmlValue,
                DefaultPersistedCondensedTextValue = entity.DefaultPersistedCondensedTextValue,
                DefaultPersistedHtmlValue = entity.DefaultPersistedHtmlValue,
                DefaultPersistedTextValue = entity.DefaultPersistedTextValue,
                DefaultValue = entity.DefaultValue,
                Description = entity.Description,
                EnableHistory = entity.EnableHistory,
                EntityTypeId = entity.EntityTypeId,
                EntityTypeQualifierColumn = entity.EntityTypeQualifierColumn,
                EntityTypeQualifierValue = entity.EntityTypeQualifierValue,
                FieldTypeId = entity.FieldTypeId,
                FieldTypeGuid = entity.FieldType.Guid,
                IsActive = entity.IsActive,
                IsMultiValue = entity.IsMultiValue,
                IsPublic = entity.IsPublic,
                IsRequired = entity.IsRequired,
                IsSystem = entity.IsSystem,
                Key = entity.Key,
                Name = entity.Name,
                Order = entity.Order,
                PostHtml = entity.PostHtml,
                PreHtml = entity.PreHtml,
                CreatedDateTime = entity.CreatedDateTime,
                ModifiedDateTime = entity.ModifiedDateTime,
                CreatedByPersonAliasId = entity.CreatedByPersonAliasId,
                ModifiedByPersonAliasId = entity.ModifiedByPersonAliasId,
                QualifierValues = attributeCache.QualifierValues
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Value
                )
            };
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="Model.Group"/> that represents the entity.</returns>
        public GroupBag GetCommonGroupEntityBag( Model.Group entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new GroupBag
            {
                IdKey = entity.IdKey,
                AllowGuests = entity.AllowGuests,
                ArchivedByPersonAliasId = entity.ArchivedByPersonAliasId,
                ArchivedDateTime = entity.ArchivedDateTime,
                AttendanceRecordRequiredForCheckIn = ( int ) entity.AttendanceRecordRequiredForCheckIn,
                CampusId = entity.CampusId,
                ConfirmationAdditionalDetails = entity.ConfirmationAdditionalDetails,
                Description = entity.Description,
                DisableScheduleToolboxAccess = entity.DisableScheduleToolboxAccess,
                DisableScheduling = entity.DisableScheduling,
                ElevatedSecurityLevel = ( int ) entity.ElevatedSecurityLevel,
                GroupCapacity = entity.GroupCapacity,
                GroupSalutation = entity.GroupSalutation,
                GroupSalutationFull = entity.GroupSalutationFull,
                GroupTypeId = entity.GroupTypeId,
                InactiveDateTime = entity.InactiveDateTime,
                InactiveReasonNote = entity.InactiveReasonNote,
                InactiveReasonValueId = entity.InactiveReasonValueId,
                IsActive = entity.IsActive,
                IsArchived = entity.IsArchived,
                IsPublic = entity.IsPublic,
                IsSecurityRole = entity.IsSecurityRole,
                IsSystem = entity.IsSystem,
                Name = entity.Name,
                Order = entity.Order,
                ParentGroupId = entity.ParentGroupId,
                ReminderAdditionalDetails = entity.ReminderAdditionalDetails,
                ReminderOffsetDays = entity.ReminderOffsetDays,
                ReminderSystemCommunicationId = entity.ReminderSystemCommunicationId,
                RequiredSignatureDocumentTemplateId = entity.RequiredSignatureDocumentTemplateId,
                RSVPReminderOffsetDays = entity.RSVPReminderOffsetDays,
                RSVPReminderSystemCommunicationId = entity.RSVPReminderSystemCommunicationId,
                ScheduleCancellationPersonAliasId = entity.ScheduleCancellationPersonAliasId,
                ScheduleConfirmationLogic = ( int? ) entity.ScheduleConfirmationLogic,
                ScheduleId = entity.ScheduleId,
                SchedulingMustMeetRequirements = entity.SchedulingMustMeetRequirements,
                StatusValueId = entity.StatusValueId,
                CreatedDateTime = entity.CreatedDateTime,
                ModifiedDateTime = entity.ModifiedDateTime,
                CreatedByPersonAliasId = entity.CreatedByPersonAliasId,
                ModifiedByPersonAliasId = entity.ModifiedByPersonAliasId
            };
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="PhoneNumber"/> that represents the entity.</returns>
        public PhoneNumberBag GetCommonPhoneNumberEntityBag( PhoneNumber entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new PhoneNumberBag
            {
                IdKey = entity.IdKey,
                CountryCode = entity.CountryCode,
                Description = entity.Description,
                Extension = entity.Extension,
                IsMessagingEnabled = entity.IsMessagingEnabled,
                IsSystem = entity.IsSystem,
                IsUnlisted = entity.IsUnlisted,
                Number = entity.Number,
                NumberFormatted = entity.NumberFormatted,
                NumberTypeValueId = entity.NumberTypeValueId,
                PersonId = entity.PersonId,
                CreatedDateTime = entity.CreatedDateTime,
                ModifiedDateTime = entity.ModifiedDateTime,
                CreatedByPersonAliasId = entity.CreatedByPersonAliasId,
                ModifiedByPersonAliasId = entity.ModifiedByPersonAliasId
            };
        }
    }
}
