using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Web.Cache;
using Attribute = Rock.Model.Attribute;

namespace org.thecrossingchurch.CustomJobs.Jobs
{
    /// <summary>
    /// Job to send connection request status reminders.
    /// </summary>
    [DisplayName( "Connection Request Status Reminders" )]
    [Description( "This job sends applicable reminders for connection requests stuck in a status as they are configured in the selected content channel." )]

    #region General Configuration Attributes
    [ContentChannelField(
        Name = "Reminder Content Channel",
        Description = "The content channel that contains all the reminders",
        IsRequired = true,
        Key = AttributeKey.ContentChannel,
        Order = 1
    )]
    [AttributeField(
        "Reminder Time Attribute",
        Description = "The attribute that holds the time the reminder should be sent",
        IsRequired = true,
        Key = AttributeKey.ReminderTimeAttr,
        EntityTypeGuid = Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM,
        AllowMultiple = false,
        Order = 2
    )]
    [AttributeField(
        "Days in Status Attribute",
        Description = "The attribute that holds the number of days a connection request or workflow has been stuck in the status before sending a reminder",
        IsRequired = true,
        Key = AttributeKey.DaysInStatusAttr,
        EntityTypeGuid = Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM,
        AllowMultiple = false,
        Order = 3
    )]
    [AttributeField(
        "Repeat Interval Attribute",
        Description = "The attribute that holds the number of days a reminder should repeat if the connection or workflow is still in the same state",
        IsRequired = true,
        Key = AttributeKey.RepeatIntervalAttr,
        EntityTypeGuid = Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM,
        AllowMultiple = false,
        Order = 4
    )]
    [AttributeField(
        "Max Reminders Attribute",
        Description = "The attribute that holds the maximum number of reminders that should be sent",
        IsRequired = true,
        Key = AttributeKey.MaxRemindersAttr,
        EntityTypeGuid = Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM,
        AllowMultiple = false,
        Order = 5
    )]
    [AttributeField(
        "Connection Activity Reminder Attribute",
        Description = "The attribute that links the reminder connection activity to the reminder that triggered it",
        IsRequired = true,
        Key = AttributeKey.ActivityReminderAttr,
        EntityTypeGuid = Rock.SystemGuid.EntityType.CONNECTION_REQUEST_ACTIVITY,
        AllowMultiple = false,
        Order = 6
    )]
    #endregion

    #region Connection Configuration Attributes
    [AttributeField(
        "Connection Request Status Attribute",
        Description = "The attribute that holds the status a connection request must be in to get reminder",
        IsRequired = true,
        Key = AttributeKey.ConnectionRequestStatusAttr,
        EntityTypeGuid = Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM,
        AllowMultiple = false,
        Category = "Connection Request Reminders",
        Order = 1
    )]
    [AttributeField(
        "Connection Request Indicator Activity Type Attribute",
        Description = "The attribute that holds the trigger activity used to calculate how long a connection request has been in the desired status",
        IsRequired = true,
        Key = AttributeKey.ConnectionRequestActivityAttr,
        EntityTypeGuid = Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM,
        AllowMultiple = false,
        Category = "Connection Request Reminders",
        Order = 2
    )]
    [AttributeField(
        "Connection Request Reminder Sent Activity Type Attribute",
        Description = "The attribute that holds the activity used to indicate a reminder has been sent",
        IsRequired = true,
        Key = AttributeKey.ConnectionRequestReminderActivityAttr,
        EntityTypeGuid = Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM,
        AllowMultiple = false,
        Category = "Connection Request Reminders",
        Order = 3
    )]
    #endregion

    #region Communication Configuration Attributes
    [AttributeField(
        "Communication Template Attribute",
        Description = "The attribute that holds the communication template for the reminder",
        IsRequired = true,
        Key = AttributeKey.CommunicationTemplateAttr,
        EntityTypeGuid = Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM,
        AllowMultiple = false,
        Category = "Reminder Communication",
        Order = 1
    )]
    [AttributeField(
        "Communication Medium Attribute",
        Description = "The attribute that holds the desired communication medium for the reminder",
        IsRequired = true,
        Key = AttributeKey.CommunicationMediumAttr,
        EntityTypeGuid = Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM,
        AllowMultiple = false,
        Category = "Reminder Communication",
        Order = 1
    )]
    [AttributeField(
        "Recipient Option Attribute",
        Description = "The attribute that holds information about the recipient of the reminder",
        IsRequired = true,
        Key = AttributeKey.RecipientOptionAttr,
        EntityTypeGuid = Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM,
        AllowMultiple = false,
        Category = "Reminder Communication",
        Order = 3
    )]
    [AttributeField(
        "Recipient Attribute Key Attribute",
        Description = "If the reminder should be sent to the value of an attribute on the connection or workflow, this is the attribute that stores the key for that attribute on the connection or workflow",
        IsRequired = true,
        Key = AttributeKey.RecipientAttributeKeyAttr,
        EntityTypeGuid = Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM,
        AllowMultiple = false,
        Category = "Reminder Communication",
        Order = 4
    )]
    [AttributeField(
        "Static Recipient Attribute",
        Description = "If the reminder should always be sent to a specific person and whatever their current contact information is, this is the attribute that stores their person record",
        IsRequired = true,
        Key = AttributeKey.StaticRecipientAttr,
        EntityTypeGuid = Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM,
        AllowMultiple = false,
        Category = "Reminder Communication",
        Order = 5
    )]
    [AttributeField(
        "Static Email Attribute",
        Description = "If the reminder should always be sent to a specific email, this is the attribute that stores the email",
        IsRequired = true,
        Key = AttributeKey.StaticEmailAttr,
        EntityTypeGuid = Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM,
        AllowMultiple = false,
        Category = "Reminder Communication",
        Order = 6
    )]
    #endregion

    public class ConnectionRequestReminders : RockJob
    {

        #region Attribute Keys
        private class AttributeKey
        {
            public const string ContentChannel = "ContentChannel";
            public const string ReminderTimeAttr = "ReminderTimeAttr";
            public const string DaysInStatusAttr = "DaysInStatusAttr";
            public const string RepeatIntervalAttr = "RepeatIntervalAttr";
            public const string MaxRemindersAttr = "MaxRemindersAttr";
            public const string ActivityReminderAttr = "ActivityReminderAttr";
            public const string ConnectionRequestStatusAttr = "ConnectionRequestStatusAttr";
            public const string ConnectionRequestActivityAttr = "ConnectionRequestActivityAttr";
            public const string ConnectionRequestReminderActivityAttr = "ConnectionRequestReminderActivityAttr";
            public const string CommunicationTemplateAttr = "CommunicationTemplateAttr";
            public const string CommunicationMediumAttr = "CommunicationMediumAttr";
            public const string RecipientOptionAttr = "RecipientOptionAttr";
            public const string RecipientAttributeKeyAttr = "RecipientAttributeKeyAttr";
            public const string StaticRecipientAttr = "StaticRecipientAttr";
            public const string StaticEmailAttr = "StaticEmailAttr";
        }
        #endregion Attribute Keys

        #region Global Variables
        private List<string> _jobErrors = new List<string>();
        private int connectionRemindersProcessed;
        private RockContext _context;
        private ContentChannel _channel;
        private Attribute _reminderTimeAttr;
        private Attribute _daysInStatusAttr;
        private Attribute _repeatIntervalAttr;
        private Attribute _maxReminderAttr;
        private Attribute _activityReminderAttr;
        private Attribute _connectionRequestStatusAttr;
        private Attribute _connectionRequestActivityAttr;
        private Attribute _connectionRequestReminderActivityAttr;
        private Attribute _communicationTemplateAttr;
        private Attribute _communicationMediumAttr;
        private Attribute _recipientOptionAttr;
        private Attribute _recipientAttrKeyAttr;
        private Attribute _staticRecipientAttr;
        private Attribute _staticEmailAttr;
        #endregion

        public override void Execute()
        {
            _context = new RockContext();
            AttributeValueService av_svc = new AttributeValueService( _context );

            bool jobConfiguredProperly = LoadConfiguration();
            if ( !jobConfiguredProperly )
            {
                int errorCt = _jobErrors.Count();
                string message = errorCt + " Configuration Error" + ( errorCt > 1 ? "s" : "" ) + ": \n" + _jobErrors.JoinStrings( "\n" );
                this.UpdateLastStatusMessage( message );
                throw new RockJobWarningException( message );
            }

            var reminders = GetConnectionRequestReminders();
            ProcessReminders( reminders );

            string jobStatus = connectionRemindersProcessed + " Reminder" + ( connectionRemindersProcessed == 1 ? "" : "s" ) + " Sent.";
            if ( _jobErrors.Count > 0 )
            {
                jobStatus += "\n\n" + _jobErrors.Count + " Error" + ( _jobErrors.Count == 1 ? "" : "s" ) + ":\n" + String.Join( "\n", _jobErrors );
                throw new RockJobWarningException( jobStatus );
            }
            this.UpdateLastStatusMessage( jobStatus );
        }
        /// <summary>
        /// Method to load the job configuration and ensure all the necessary data is available to process reminders.
        /// </summary>
        /// <returns>If all necessary information was loaded.</returns>
        private bool LoadConfiguration()
        {
            bool success = true;
            AttributeService attr_svc = new AttributeService( _context );

            Guid? ContentChannelGuid = GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull();
            if ( !ContentChannelGuid.HasValue )
            {
                _jobErrors.Add( "Configure Reminder Content Channel." );
                success = false;
            }
            else
            {
                _channel = new ContentChannelService( _context ).Get( ContentChannelGuid.Value );
                if ( _channel == null )
                {
                    _jobErrors.Add( "Unable to Load Reminder Content Channel." );
                    success = false;
                }
            }

            Guid? reminderTimeAttrGuid = GetAttributeValue( AttributeKey.ReminderTimeAttr ).AsGuidOrNull();
            if ( !reminderTimeAttrGuid.HasValue )
            {
                _jobErrors.Add( "Configure Reminder Time Attribute." );
                success = false;
            }
            else
            {
                _reminderTimeAttr = attr_svc.Get( reminderTimeAttrGuid.Value );
                if ( _reminderTimeAttr == null )
                {
                    _jobErrors.Add( "Unable to Load Reminder Time Attribute." );
                    success = false;
                }
            }

            Guid? daysInStatusAttrGuid = GetAttributeValue( AttributeKey.DaysInStatusAttr ).AsGuidOrNull();
            if ( !daysInStatusAttrGuid.HasValue )
            {
                _jobErrors.Add( "Configure Days in Status Attribute." );
                success = false;
            }
            else
            {
                _daysInStatusAttr = attr_svc.Get( daysInStatusAttrGuid.Value );
                if ( _daysInStatusAttr == null )
                {
                    _jobErrors.Add( "Unable to Load Days in Status Attribute." );
                    success = false;
                }
            }

            Guid? repeatIntervalAttrGuid = GetAttributeValue( AttributeKey.RepeatIntervalAttr ).AsGuidOrNull();
            if ( !repeatIntervalAttrGuid.HasValue )
            {
                _jobErrors.Add( "Configure Repeat Interval Attribute." );
                success = false;
            }
            else
            {
                _repeatIntervalAttr = attr_svc.Get( repeatIntervalAttrGuid.Value );
                if ( _repeatIntervalAttr == null )
                {
                    _jobErrors.Add( "Unable to Load Repeat Interval Attribute." );
                    success = false;
                }
            }

            Guid? maxRemindersAttrGuid = GetAttributeValue( AttributeKey.MaxRemindersAttr ).AsGuidOrNull();
            if ( !maxRemindersAttrGuid.HasValue )
            {
                _jobErrors.Add( "Configure Max Reminders Attribute." );
                success = false;
            }
            else
            {
                _maxReminderAttr = attr_svc.Get( maxRemindersAttrGuid.Value );
                if ( _maxReminderAttr == null )
                {
                    _jobErrors.Add( "Unable to Load Max Reminders Attribute." );
                    success = false;
                }
            }

            Guid? activityReminderAttrGuid = GetAttributeValue( AttributeKey.ActivityReminderAttr ).AsGuidOrNull();
            if ( !activityReminderAttrGuid.HasValue )
            {
                _jobErrors.Add( "Configure Activity Reminder Attribute." );
                success = false;
            }
            else
            {
                _activityReminderAttr = attr_svc.Get( activityReminderAttrGuid.Value );
                if ( _activityReminderAttr == null )
                {
                    _jobErrors.Add( "Unable to Load Activity Reminder Attribute." );
                    success = false;
                }
            }

            Guid? connectionRequestActivityAttrGuid = GetAttributeValue( AttributeKey.ConnectionRequestActivityAttr ).AsGuidOrNull();
            if ( !connectionRequestActivityAttrGuid.HasValue )
            {
                _jobErrors.Add( "Configure Connection Request Indicator Activity Attribute." );
                success = false;
            }
            else
            {
                _connectionRequestActivityAttr = attr_svc.Get( connectionRequestActivityAttrGuid.Value );
                if ( _connectionRequestActivityAttr == null )
                {
                    _jobErrors.Add( "Unable to Load Connection Request Indicator Activity Attribute." );
                    success = false;
                }
            }

            Guid? connectionRequestReminderActivityAttrGuid = GetAttributeValue( AttributeKey.ConnectionRequestReminderActivityAttr ).AsGuidOrNull();
            if ( !connectionRequestReminderActivityAttrGuid.HasValue )
            {
                _jobErrors.Add( "Configure Connection Request Reminder Activity Attribute." );
                success = false;
            }
            else
            {
                _connectionRequestReminderActivityAttr = attr_svc.Get( connectionRequestReminderActivityAttrGuid.Value );
                if ( _connectionRequestReminderActivityAttr == null )
                {
                    _jobErrors.Add( "Unable to Load Connection Request Reminder Activity Attribute." );
                    success = false;
                }
            }

            Guid? connectionRequestStatusAttrGuid = GetAttributeValue( AttributeKey.ConnectionRequestStatusAttr ).AsGuidOrNull();
            if ( !connectionRequestStatusAttrGuid.HasValue )
            {
                _jobErrors.Add( "Configure Connection Request Status Attribute." );
                success = false;
            }
            else
            {
                _connectionRequestStatusAttr = attr_svc.Get( connectionRequestStatusAttrGuid.Value );
                if ( _connectionRequestStatusAttr == null )
                {
                    _jobErrors.Add( "Unable to Load Connection Request Status Attribute." );
                    success = false;
                }
            }

            Guid? communicationTemplateAttrGuid = GetAttributeValue( AttributeKey.CommunicationTemplateAttr ).AsGuidOrNull();
            if ( !communicationTemplateAttrGuid.HasValue )
            {
                _jobErrors.Add( "Configure Communication Template Attribute." );
                success = false;
            }
            else
            {
                _communicationTemplateAttr = attr_svc.Get( communicationTemplateAttrGuid.Value );
                if ( _communicationTemplateAttr == null )
                {
                    _jobErrors.Add( "Unable to Load Communication Template Attribute." );
                    success = false;
                }
            }

            Guid? communicationMediumAttrGuid = GetAttributeValue( AttributeKey.CommunicationMediumAttr ).AsGuidOrNull();
            if ( !communicationMediumAttrGuid.HasValue )
            {
                _jobErrors.Add( "Configure Communication Medium Attribute." );
                success = false;
            }
            else
            {
                _communicationMediumAttr = attr_svc.Get( communicationMediumAttrGuid.Value );
                if ( _communicationMediumAttr == null )
                {
                    _jobErrors.Add( "Unable to Load Communication Medium Attribute." );
                    success = false;
                }
            }

            Guid? recipientOptionAttrGuid = GetAttributeValue( AttributeKey.RecipientOptionAttr ).AsGuidOrNull();
            if ( !recipientOptionAttrGuid.HasValue )
            {
                _jobErrors.Add( "Configure Recipient Option Attribute." );
                success = false;
            }
            else
            {
                _recipientOptionAttr = attr_svc.Get( recipientOptionAttrGuid.Value );
                if ( _recipientOptionAttr == null )
                {
                    _jobErrors.Add( "Unable to Load Recipient Option Attribute." );
                    success = false;
                }
            }

            Guid? recipientAttrKeyAttrGuid = GetAttributeValue( AttributeKey.RecipientAttributeKeyAttr ).AsGuidOrNull();
            if ( !recipientAttrKeyAttrGuid.HasValue )
            {
                _jobErrors.Add( "Configure Recipient Attribute Key Attribute." );
                success = false;
            }
            else
            {
                _recipientAttrKeyAttr = attr_svc.Get( recipientAttrKeyAttrGuid.Value );
                if ( _recipientAttrKeyAttr == null )
                {
                    _jobErrors.Add( "Unable to Load Recipient Attribute Key Attribute." );
                    success = false;
                }
            }

            Guid? staticRecipientAttrGuid = GetAttributeValue( AttributeKey.StaticRecipientAttr ).AsGuidOrNull();
            if ( !staticRecipientAttrGuid.HasValue )
            {
                _jobErrors.Add( "Configure Static Recipient Attribute." );
                success = false;
            }
            else
            {
                _staticRecipientAttr = attr_svc.Get( staticRecipientAttrGuid.Value );
                if ( _staticRecipientAttr == null )
                {
                    _jobErrors.Add( "Unable to Load Static Recipient Attribute." );
                    success = false;
                }
            }

            Guid? staticEmailAttrGuid = GetAttributeValue( AttributeKey.StaticEmailAttr ).AsGuidOrNull();
            if ( !staticEmailAttrGuid.HasValue )
            {
                _jobErrors.Add( "Configure Static Email Attribute." );
                success = false;
            }
            else
            {
                _staticEmailAttr = attr_svc.Get( staticEmailAttrGuid.Value );
                if ( _staticEmailAttr == null )
                {
                    _jobErrors.Add( "Unable to Load Static Email Attribute." );
                    success = false;
                }
            }

            return success;
        }

        private List<ConnectionRequestRmeinder> GetConnectionRequestReminders()
        {
            ContentChannelItemService cci_svc = new ContentChannelItemService( _context );
            AttributeValueService av_svc = new AttributeValueService( _context );
            ConnectionRequestService cr_svc = new ConnectionRequestService( _context );
            ConnectionStatusService cs_svc = new ConnectionStatusService( _context );
            ConnectionActivityTypeService cat_svc = new ConnectionActivityTypeService( _context );
            ConnectionRequestActivityService cra_svc = new ConnectionRequestActivityService( _context );

            DateTime today = DateTime.Now.StartOfDay();
            string today_date_str = today.ToString( "yyyy-MM-dd" );
            DateTime currentTime = DateTime.Now;

            var reminders =
                cci_svc.Queryable().Where( cci =>
                    cci.ContentChannelId == _channel.Id && cci.StartDateTime < today &&
                    ( !cci.ExpireDateTime.HasValue || cci.ExpireDateTime.Value > today )
                );
            var connectionStatusAVs =
                av_svc.Queryable().Where( av =>
                    av.AttributeId == _connectionRequestStatusAttr.Id &&
                    !String.IsNullOrEmpty( av.Value )
                );
            var indicatorActivityAVs =
                av_svc.Queryable().Where( av =>
                    av.AttributeId == _connectionRequestActivityAttr.Id &&
                    !String.IsNullOrEmpty( av.Value )
                );
            var reminderActivityAVs =
                av_svc.Queryable().Where( av =>
                    av.AttributeId == _connectionRequestReminderActivityAttr.Id &&
                    !String.IsNullOrEmpty( av.Value )
                );
            var daysInStatusAVs =
                av_svc.Queryable().Where( av =>
                    av.AttributeId == _daysInStatusAttr.Id &&
                    !String.IsNullOrEmpty( av.Value )
                );
            var reminderTimeAvs =
                av_svc.Queryable().Where( av =>
                    av.AttributeId == _reminderTimeAttr.Id &&
                    !String.IsNullOrEmpty( av.Value )
                );

            var repeatIntervalAVs =
                av_svc.Queryable().Where( av =>
                    av.AttributeId == _repeatIntervalAttr.Id &&
                    !String.IsNullOrEmpty( av.Value )
                );
            var maxRemindersAVs =
                av_svc.Queryable().Where( av =>
                    av.AttributeId == _maxReminderAttr.Id &&
                    !String.IsNullOrEmpty( av.Value )
                );

            var reminderDetails =
                from reminder in reminders
                join cs_av in connectionStatusAVs on reminder.Id equals cs_av.EntityId
                join cs in cs_svc.Queryable() on cs_av.Value.ToUpper() equals cs.Guid.ToString().ToUpper()
                join ia_av in indicatorActivityAVs on reminder.Id equals ia_av.EntityId
                join ia_cat in cat_svc.Queryable() on ia_av.Value.ToUpper() equals ia_cat.Guid.ToString().ToUpper()
                join ra_av in reminderActivityAVs on reminder.Id equals ra_av.EntityId
                join ra_cat in cat_svc.Queryable() on ra_av.Value.ToUpper() equals ra_cat.Guid.ToString().ToUpper()
                join dis_av in daysInStatusAVs on reminder.Id equals dis_av.EntityId
                join rt_av in reminderTimeAvs on reminder.Id equals rt_av.EntityId
                join _ri_av in repeatIntervalAVs on reminder.Id equals _ri_av.EntityId into rij
                from ri_av in rij.DefaultIfEmpty()
                join _mr_av in maxRemindersAVs on reminder.Id equals _mr_av.EntityId into mrj
                from mr_av in mrj.DefaultIfEmpty()
                select new ReminderDetails()
                {
                    ReminderId = reminder.Id,
                    ReminderGuid = reminder.Guid.ToString().ToUpper(),
                    ConnectionStatusId = cs.Id,
                    DaysInStatus = dis_av.ValueAsNumeric,
                    RepeatInterval = ri_av.ValueAsNumeric,
                    MaxReminders = mr_av.ValueAsNumeric,
                    ReminderTime = rt_av.Value,
                    IndicatorActivityTypeId = ia_cat.Id,
                    ReminderActivityTypeId = ra_cat.Id
                };

            var x = reminderDetails.ToList();

            var mostRecentIndicator = cra_svc.Queryable()
                .GroupBy( cra => new { cra.ConnectionRequestId, cra.ConnectionActivityTypeId } )
                .Select( g => new
                {
                    g.Key.ConnectionRequestId,
                    g.Key.ConnectionActivityTypeId,
                    MostRecentIndicatorActivityDateTime = g.Max( cra => cra.CreatedDateTime )
                } );

            var y = mostRecentIndicator.ToList();

            var reminderActivityReminderAV =
                av_svc.Queryable().Where( av =>
                    av.AttributeId == _activityReminderAttr.Id &&
                    !String.IsNullOrEmpty( av.Value )
                );
            var reminderActivities = cra_svc.Queryable()
                .Join( reminderActivityReminderAV,
                    cra => cra.Id,
                    av => av.EntityId,
                    ( cra, av ) => new
                    {
                        cra.ConnectionActivityTypeId,
                        cra.ConnectionRequestId,
                        cra.CreatedDateTime,
                        ReminderGuid = av.Value.ToUpper()
                    }
                );

            var z = reminderActivities.ToList();

            var connectionReminderActivity =
                from reminder in reminderDetails
                join connection in cr_svc.Queryable() on reminder.ConnectionStatusId equals connection.ConnectionStatusId
                join ia in mostRecentIndicator on new { ConnectionActivityTypeId = reminder.IndicatorActivityTypeId, ConnectionRequestId = connection.Id } equals new { ia.ConnectionActivityTypeId, ia.ConnectionRequestId }
                join _ra in reminderActivities on new { ConnectionActivityTypeId = reminder.ReminderActivityTypeId, ConnectionRequestId = connection.Id, reminder.ReminderGuid } equals new { _ra.ConnectionActivityTypeId, _ra.ConnectionRequestId, _ra.ReminderGuid } into raj
                from ra in raj.DefaultIfEmpty()
                select new
                {
                    reminder.ReminderId,
                    reminder.ReminderGuid,
                    reminder.ConnectionStatusId,
                    reminder.DaysInStatus,
                    reminder.RepeatInterval,
                    reminder.MaxReminders,
                    reminder.ReminderTime,
                    reminder.IndicatorActivityTypeId,
                    reminder.ReminderActivityTypeId,
                    ConnectionRequestId = connection.Id,
                    ia.MostRecentIndicatorActivityDateTime,
                    ReminderActivityDateTime = ra.CreatedDateTime
                };

            var a = connectionReminderActivity.ToList();

            var possibleReminders = connectionReminderActivity
                .Where( cra => !cra.ReminderActivityDateTime.HasValue || cra.ReminderActivityDateTime > cra.MostRecentIndicatorActivityDateTime )
                .GroupBy( cr => new
                {
                    cr.ReminderId,
                    cr.ConnectionRequestId,
                    cr.DaysInStatus,
                    cr.RepeatInterval,
                    cr.MaxReminders,
                    cr.ReminderTime,
                    cr.MostRecentIndicatorActivityDateTime
                } ).Select( g => new ConnectionRequestRmeinder()
                {
                    ReminderId = g.Key.ReminderId,
                    ConnectionRequestId = g.Key.ConnectionRequestId,
                    DaysInStatus = g.Key.DaysInStatus,
                    RepeatInterval = g.Key.RepeatInterval,
                    MaxReminders = g.Key.MaxReminders,
                    ReminderTime = g.Key.ReminderTime,
                    MostRecentIndicatorActivityDateTime = g.Key.MostRecentIndicatorActivityDateTime,
                    MostRecentReminderActivityDateTime = g.Max( cr => cr.ReminderActivityDateTime ),
                    DaysRequestHasBeenInStatus = DbFunctions.DiffDays( g.Key.MostRecentIndicatorActivityDateTime, today ),
                    DaysSinceLastReminder = DbFunctions.DiffDays( g.Max( cr => cr.ReminderActivityDateTime ), today ),
                    NumberOfReminders = g.Where( cr => cr.ReminderActivityDateTime.HasValue ).Count(),

                } );

            var b = possibleReminders.ToList();

            var connectionReminders = possibleReminders.Where( cr =>
                cr.DaysRequestHasBeenInStatus >= cr.DaysInStatus &&
                ( !cr.MaxReminders.HasValue || cr.NumberOfReminders < cr.MaxReminders.Value ) &&
                ( !cr.MostRecentReminderActivityDateTime.HasValue || cr.DaysSinceLastReminder >= cr.RepeatInterval )
            ).ToList().Select( cr =>
            {
                cr.TargetSendDateTime = DateTime.ParseExact( today_date_str + " " + cr.ReminderTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture );
                return cr;
            } ).Where( cr =>
                cr.TargetSendDateTime <= currentTime
            ).ToList();

            return connectionReminders;
        }

        private void ProcessReminders( List<ConnectionRequestRmeinder> connectionReminders )
        {
            ContentChannelItemService cci_svc = new ContentChannelItemService( _context );
            ConnectionRequestService cr_svc = new ConnectionRequestService( _context );
            ConnectionActivityTypeService cat_svc = new ConnectionActivityTypeService( _context );
            PersonAliasService pa_svc = new PersonAliasService( _context );

            var reminders = connectionReminders.GroupBy( cr => cr.ReminderId ).ToList();

            for ( int i = 0; i < reminders.Count(); i++ )
            {
                ContentChannelItem reminder = cci_svc.Get( reminders[i].Key );
                List<ConnectionRequestRmeinder> connections = reminders[i].ToList();

                reminder.LoadAttributes();
                RecipientOption recipientOption = ( RecipientOption ) reminder.GetAttributeValue( _recipientOptionAttr.Key ).AsInteger();
                ReminderConfiguration reminderConfiguration = GetReminderDetails( reminder, recipientOption );
                Guid? reminderActivityGuid = reminder.GetAttributeValue( _connectionRequestReminderActivityAttr.Key ).AsGuidOrNull();
                ConnectionActivityType reminderActivity = null;
                if ( reminderActivityGuid.HasValue )
                {
                    reminderActivity = cat_svc.Get( reminderActivityGuid.Value );
                }

                for ( int k = 0; k < connections.Count(); k++ )
                {
                    ConnectionRequest connectionRequest = cr_svc.Get( connections[k].ConnectionRequestId );
                    ReminderConfiguration connectionReminderConfiguration = reminderConfiguration;
                    bool reminderSent = false;
                    Dictionary<string, object> mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "Requestor", connectionRequest.PersonAlias.Person );
                    mergeFields.Add( "Connector", connectionRequest.ConnectorPersonAliasId.HasValue ? connectionRequest.ConnectorPersonAlias.Person : null );
                    mergeFields.Add( "ConnectionRequest", connectionRequest );
                    string commName = "Reminder for " + connectionRequest.PersonAlias.Person.FullName + "'s " + connectionRequest.ConnectionOpportunity.Name;
                    if ( recipientOption == RecipientOption.Requestor )
                    {
                        connectionReminderConfiguration.Recipient = connectionRequest.PersonAlias.Person;
                    }
                    else if ( recipientOption == RecipientOption.Connector )
                    {
                        if ( connectionRequest.ConnectorPersonAliasId.HasValue )
                        {
                            connectionReminderConfiguration.Recipient = connectionRequest.ConnectorPersonAlias.Person;
                        }
                        else
                        {
                            _jobErrors.Add( "No Connector on " + commName );
                        }
                    }
                    else if ( recipientOption == RecipientOption.AttributeValue )
                    {
                        connectionRequest.LoadAttributes();
                        string attrKey = reminder.GetAttributeValue( _recipientAttrKeyAttr.Key );
                        string attrValue = connectionRequest.GetAttributeValue( attrKey );
                        if ( EmailAddressFieldValidator.IsValid( attrValue ) )
                        {
                            connectionReminderConfiguration.RecipientEmail = attrValue;
                            if ( connectionReminderConfiguration.CommunicationType == CommunicationType.SMS )
                            {
                                if ( !String.IsNullOrEmpty( connectionReminderConfiguration.CommunicationTemplate.Message ) )
                                {
                                    connectionReminderConfiguration.CommunicationType = CommunicationType.Email;
                                }
                                else
                                {
                                    connectionReminderConfiguration.CommunicationType = null;
                                }
                            }
                        }
                        else
                        {
                            //If the value isn't a valid email address then we'll see if it is a person guid
                            Guid? guid = attrValue.AsGuidOrNull();
                            if ( guid.HasValue )
                            {
                                PersonAlias pa = pa_svc.Get( guid.Value );
                                if ( pa != null && pa.Person != null )
                                {
                                    connectionReminderConfiguration.Recipient = pa.Person;
                                }
                            }
                        }
                    }
                    if ( connectionReminderConfiguration.Recipient != null )
                    {
                        mergeFields.Add( "Person", connectionReminderConfiguration.Recipient );
                    }

                    try
                    {
                        if ( connectionReminderConfiguration.CommunicationType == CommunicationType.Email )
                        {
                            RockEmailMessageRecipient recipient = null;
                            if ( connectionReminderConfiguration.Recipient != null )
                            {
                                recipient = new RockEmailMessageRecipient( connectionReminderConfiguration.Recipient, mergeFields );
                            }
                            else if ( !String.IsNullOrEmpty( connectionReminderConfiguration.RecipientEmail ) )
                            {
                                recipient = RockEmailMessageRecipient.CreateAnonymous( connectionReminderConfiguration.RecipientEmail, null );
                            }
                            if ( recipient == null )
                            {
                                throw new Exception( "Unable to Generate Recipient for " + commName );
                            }
                            reminderSent = ProcessEmailReminder( connectionReminderConfiguration, commName, recipient, mergeFields );
                        }
                        else if ( connectionReminderConfiguration.CommunicationType == CommunicationType.SMS )
                        {
                            RockSMSMessageRecipient recipient = null;
                            if ( connectionReminderConfiguration.Recipient != null )
                            {
                                recipient = new RockSMSMessageRecipient( connectionReminderConfiguration.Recipient, connectionReminderConfiguration.Recipient.PhoneNumbers.GetFirstSmsNumber(), mergeFields );
                            }
                            if ( recipient == null || String.IsNullOrEmpty( recipient.SMSNumber ) )
                            {
                                throw new Exception( "Unable to Generate Recipient for " + commName );
                            }
                            reminderSent = ProcessSMSReminder( connectionReminderConfiguration, commName, recipient, mergeFields );
                        }
                    }
                    catch ( Exception ex )
                    {
                        _jobErrors.Add( ex.Message );
                    }
                    if ( reminderSent )
                    {
                        connectionRemindersProcessed++;
                        if ( reminderActivity != null )
                        {
                            AddReminderActivityToConnection( connectionRequest, reminderActivity, connectionReminderConfiguration, reminder );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method to identify the communication template and type of communication for the reminder
        /// </summary>
        /// <param name="reminder">The content channel item that contains the reminder information</param>
        /// <returns></returns>
        private ReminderConfiguration GetReminderDetails( ContentChannelItem reminder, RecipientOption recipientOption )
        {
            CommunicationTemplateService ct_svc = new CommunicationTemplateService( _context );
            ReminderConfiguration configuration = new ReminderConfiguration();
            CommunicationTemplate communicationTemplate = null;

            Guid? communicationTemplateGuid = reminder.GetAttributeValue( _communicationTemplateAttr.Key ).AsGuidOrNull();
            if ( communicationTemplateGuid.HasValue )
            {
                communicationTemplate = ct_svc.Get( communicationTemplateGuid.Value );
                configuration.CommunicationTemplate = communicationTemplate;

                CommunicationType communicationType = ( CommunicationType ) reminder.GetAttributeValue( _communicationMediumAttr.Key ).AsInteger();

                if ( communicationType == CommunicationType.Email && !String.IsNullOrEmpty( communicationTemplate.Message ) )
                {
                    configuration.CommunicationType = CommunicationType.Email;
                }
                else if ( communicationType == CommunicationType.SMS && !String.IsNullOrEmpty( communicationTemplate.SMSMessage ) )
                {
                    configuration.CommunicationType = CommunicationType.SMS;
                }
                else if ( !String.IsNullOrEmpty( communicationTemplate.Message ) )
                {
                    configuration.CommunicationType = CommunicationType.Email;
                }
                else if ( !String.IsNullOrEmpty( communicationTemplate.SMSMessage ) )
                {
                    configuration.CommunicationType = CommunicationType.SMS;
                }
            }
            if ( recipientOption == RecipientOption.StaticEmail )
            {
                configuration.RecipientEmail = reminder.GetAttributeValue( _staticEmailAttr.Key );
                if ( configuration.CommunicationType == CommunicationType.SMS )
                {
                    //Can't send an SMS to an email
                    if ( !String.IsNullOrEmpty( communicationTemplate.Message ) )
                    {
                        configuration.CommunicationType = CommunicationType.Email;
                    }
                    else
                    {
                        configuration.CommunicationType = null;
                    }
                }
            }
            else if ( recipientOption == RecipientOption.StaticRecipient )
            {
                PersonAliasService pa_svc = new PersonAliasService( _context );
                Guid? recipientGuid = reminder.GetAttributeValue( _staticRecipientAttr.Key ).AsGuidOrNull();
                if ( recipientGuid.HasValue )
                {
                    var recipient = pa_svc.Get( recipientGuid.Value );
                    if ( recipient != null && recipient.Person != null )
                    {
                        configuration.Recipient = recipient.Person;
                    }
                }
            }

            return configuration;
        }

        /// <summary>
        /// Method to Create and Send an Email Reminder
        /// </summary>
        /// <param name="reminderConfiguration">Reminder Details</param>
        /// <param name="communicationName">Name of the Communication</param>
        /// <param name="recipient">Recipient of the Reminder</param>
        /// <param name="mergeFields">Communication Merge Fields</param>
        /// <returns>If the Reminder was sent sucessfully or not</returns>
        private bool ProcessEmailReminder( ReminderConfiguration reminderConfiguration, string communicationName, RockEmailMessageRecipient recipient, Dictionary<string, object> mergeFields )
        {
            var errorMessages = new List<string>();
            try
            {
                RockEmailMessage communication = new RockEmailMessage()
                {
                    FromEmail = reminderConfiguration.CommunicationTemplate.FromEmail,
                    FromName = reminderConfiguration.CommunicationTemplate.FromName,
                    ReplyToEmail = reminderConfiguration.CommunicationTemplate.ReplyToEmail,
                    Message = reminderConfiguration.CommunicationTemplate.Message,
                    Subject = reminderConfiguration.CommunicationTemplate.Subject,
                    CreateCommunicationRecord = true,
                    AdditionalMergeFields = mergeFields
                };
                communication.AddRecipient( recipient );
                communication.Send( out errorMessages );
                if ( errorMessages.Count > 0 )
                {
                    throw new Exception( String.Join( "\n", errorMessages ) );
                }
                return true;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                _jobErrors.Add( "Error Processing: " + communicationName + "\n" + ex.Message );
                return false;
            }
        }
        /// <summary>
        /// Method to Create and Send an SMS Reminder.
        /// </summary>
        /// <param name="reminderConfiguration">Reminder Details</param>
        /// <param name="communicationName">Name of the Communication</param>
        /// <param name="recipient">Recipient of the Reminder</param>
        /// <param name="mergeFields">Communication Merge Fields</param>
        /// <returns>If the Reminder was sent sucessfully or not</returns>
        private bool ProcessSMSReminder( ReminderConfiguration reminderConfiguration, string communicationName, RockSMSMessageRecipient recipient, Dictionary<string, object> mergeFields )
        {
            var errorMessages = new List<string>();
            try
            {
                RockSMSMessage communication = new RockSMSMessage()
                {
                    Message = reminderConfiguration.CommunicationTemplate.SMSMessage,
                    CreateCommunicationRecord = true,
                    AdditionalMergeFields = mergeFields
                };
                if ( reminderConfiguration.CommunicationTemplate.SmsFromSystemPhoneNumberId.HasValue )
                {
                    communication.FromSystemPhoneNumber = SystemPhoneNumberCache.Get( reminderConfiguration.CommunicationTemplate.SmsFromSystemPhoneNumberId.Value );
                }
                communication.AddRecipient( recipient );
                communication.Send( out errorMessages );
                if ( errorMessages.Count > 0 )
                {
                    throw new Exception( String.Join( "\n", errorMessages ) );
                }
                return true;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                _jobErrors.Add( "Error Processing: " + communicationName + "\n" + ex.Message );
                return false;
            }
        }

        /// <summary>
        /// Method to add a new activity to a connection request indicating a reminder was sent.
        /// </summary>
        /// <param name="connectionRequest">The Connection Request that recieved the reminder.</param>
        /// <param name="reminderActivity">The Connection Request Activity Type reserved for reminders.</param>
        /// <param name="reminderConfiguration">Information about who the reminder was sent to.</param>
        /// <param name="reminder">The Content Channel Item with the reminder configuration.</param>
        private void AddReminderActivityToConnection( ConnectionRequest connectionRequest, ConnectionActivityType reminderActivity, ReminderConfiguration reminderConfiguration, ContentChannelItem reminder )
        {
            try
            {
                string note = "Reminder " + reminder.Title + " sent to ";
                ConnectionRequestActivityService cra_svc = new ConnectionRequestActivityService( _context );
                if ( reminderConfiguration.Recipient != null )
                {
                    note += reminderConfiguration.Recipient.FullName + " at ";
                    if ( reminderConfiguration.CommunicationType == CommunicationType.Email )
                    {
                        note += reminderConfiguration.Recipient.Email + ".";
                    }
                    else if ( reminderConfiguration.CommunicationType == CommunicationType.SMS )
                    {
                        note += reminderConfiguration.Recipient.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).NumberFormatted + ".";
                    }
                }
                else
                {
                    note += reminderConfiguration.RecipientEmail + ".";
                }
                var reminderSentActivity = new ConnectionRequestActivity()
                {
                    ConnectionActivityTypeId = reminderActivity.Id,
                    ConnectionRequestId = connectionRequest.Id,
                    ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId,
                    Note = note
                };
                cra_svc.Add( reminderSentActivity );
                _context.SaveChanges();
                reminderSentActivity.LoadAttributes();
                reminderSentActivity.SetAttributeValue( _activityReminderAttr.Key, reminder.Guid );
                reminderSentActivity.SaveAttributeValue( _activityReminderAttr.Key );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                _jobErrors.Add( "Error Adding Activity to : " + connectionRequest.PersonAlias.Person.FullName + "'s " + connectionRequest.ConnectionOpportunity.Name + "\n" + ex.Message );
            }
        }

        private class ConnectionActivityResult
        {
            public int ConnectionRequestId { get; set; }
            public ConnectionRequestActivity Activity { get; set; }
            public int? DaysInStatus { get; set; }
        }

        private class ReminderDetails
        {
            public int ReminderId { get; set; }
            public string ReminderGuid { get; set; }
            public int ConnectionStatusId { get; set; }
            public decimal? DaysInStatus { get; set; }
            public decimal? RepeatInterval { get; set; }
            public decimal? MaxReminders { get; set; }
            public string ReminderTime { get; set; }
            public int IndicatorActivityTypeId { get; set; }
            public int ReminderActivityTypeId { get; set; }
        }
        private class ConnectionRequestRmeinder
        {
            public int ReminderId { get; set; }
            public int ConnectionRequestId { get; set; }
            public decimal? DaysInStatus { get; set; }
            public decimal? RepeatInterval { get; set; }
            public decimal? MaxReminders { get; set; }
            public string ReminderTime { get; set; }
            public DateTime? MostRecentIndicatorActivityDateTime { get; set; }
            public DateTime? MostRecentReminderActivityDateTime { get; set; }
            public DateTime? TargetSendDateTime { get; set; }
            public int? DaysRequestHasBeenInStatus { get; set; }
            public int? DaysSinceLastReminder { get; set; }
            public int? NumberOfReminders { get; set; }
        }
        private class ReminderConfiguration
        {
            public Person Recipient { get; set; }
            public string RecipientEmail { get; set; }
            public CommunicationType? CommunicationType { get; set; }
            public CommunicationTemplate CommunicationTemplate { get; set; }
        }
        private enum RecipientOption
        {
            Requestor = 0,
            Connector = 1,
            AttributeValue = 2,
            StaticEmail = 3,
            StaticRecipient = 4
        }
    }
}
