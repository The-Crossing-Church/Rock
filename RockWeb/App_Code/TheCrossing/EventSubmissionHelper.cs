using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace RockWeb.TheCrossing
{
    /// <summary>
    /// Summary description for EventSubmissionHelper
    /// </summary>
    public class EventSubmissionHelper
    {
        #region variables
        public List<DefinedValue> Rooms { get; set; }
        public string RoomsJSON { get; set; }
        public List<DefinedValue> Doors { get; set; }
        public string DoorsJSON { get; set; }
        public List<DefinedValue> Ministries { get; set; }
        public string MinistriesJSON { get; set; }
        public List<DefinedValue> BudgetLines { get; set; }
        public string BudgetLinesJSON { get; set; }
        public int EventContentChannelId { get; set; }
        public int EventContentChannelTypeId { get; set; }
        public int EventDetailsContentChannelId { get; set; }
        public int EventDetailsContentChannelTypeId { get; set; }
        public int CommentsContentChannelId { get; set; }
        public int CommentsContentChannelTypeId { get; set; }
        public string BaseURL { get; set; }
        #endregion

        public EventSubmissionHelper( Guid? RoomDefinedTypeGuid, Guid? MinistryDefinedTypeGuid, Guid? BudgetDefinedTypeGuid, Guid? EventContentChannelGuid, Guid? EventDetailsContentChannelGuid )
        {
            RockContext context = new RockContext();

            if ( RoomDefinedTypeGuid.HasValue )
            {
                int RoomDefinedTypeId = new DefinedTypeService( context ).Get( RoomDefinedTypeGuid.Value ).Id;
                Rooms = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == RoomDefinedTypeId ).OrderBy( dv => dv.Order ).ToList();
                Rooms.LoadAttributes();
                Doors = Rooms.Where( dv => dv.AttributeValues.FirstOrDefault( av => av.Key == "IsDoor" ).Value.Value.AsBoolean() == true ).ToList();
                Rooms = Rooms.Where( dv => dv.AttributeValues.FirstOrDefault( av => av.Key == "IsDoor" ).Value.Value.AsBoolean() == false ).ToList();
                RoomsJSON = JsonConvert.SerializeObject( Rooms.Select( dv => new { Id = dv.Id, Value = dv.Value, Type = dv.AttributeValues.FirstOrDefault( av => av.Key == "Type" ).Value.Value, Capacity = dv.AttributeValues.FirstOrDefault( av => av.Key == "Capacity" ).Value.Value.AsInteger(), IsActive = dv.IsActive, SetUp = dv.AttributeValues.FirstOrDefault( av => av.Key == "StandardSetUp" ).Value.Value } ) );
                DoorsJSON = JsonConvert.SerializeObject( Doors.Select( dv => new { Id = dv.Id, Value = dv.Value, Type = dv.AttributeValues.FirstOrDefault( av => av.Key == "Type" ).Value.Value, IsActive = dv.IsActive } ) );
            }

            if ( MinistryDefinedTypeGuid.HasValue )
            {
                int MinistryDefinedTypeId = new DefinedTypeService( context ).Get( MinistryDefinedTypeGuid.Value ).Id;
                Ministries = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == MinistryDefinedTypeId ).OrderBy( dv => dv.Order ).ToList();
                Ministries.LoadAttributes();
                MinistriesJSON = JsonConvert.SerializeObject( Ministries.Select( dv => new { Id = dv.Id, Value = dv.Value, IsPersonal = dv.AttributeValues.FirstOrDefault( av => av.Key == "IsPersonalRequest" ).Value.Value.AsBoolean(), IsActive = dv.IsActive } ) );
            }

            if ( BudgetDefinedTypeGuid.HasValue )
            {
                int BudgetDefinedTypeId = new DefinedTypeService( context ).Get( BudgetDefinedTypeGuid.Value ).Id;
                BudgetLines = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == BudgetDefinedTypeId ).OrderBy( dv => dv.Order ).ToList();
                BudgetLinesJSON = JsonConvert.SerializeObject( BudgetLines.Select( dv => new { Id = dv.Id, Value = dv.Value, IsActive = dv.IsActive } ) );
            }

            if ( EventContentChannelGuid.HasValue )
            {
                ContentChannel channel = new ContentChannelService( context ).Get( EventContentChannelGuid.Value );
                EventContentChannelId = channel.Id;
                EventContentChannelTypeId = channel.ContentChannelTypeId;
            }

            if ( EventDetailsContentChannelGuid.HasValue )
            {
                ContentChannel channel = new ContentChannelService( context ).Get( EventDetailsContentChannelGuid.Value );
                EventDetailsContentChannelId = channel.Id;
                EventDetailsContentChannelTypeId = channel.ContentChannelTypeId;
            }

            Rock.Model.Attribute attr = new AttributeService( context ).Queryable().FirstOrDefault( a => a.Key == "InternalApplicationRoot" );
            if ( attr != null )
            {
                BaseURL = new AttributeValueService( context ).Queryable().FirstOrDefault( av => av.AttributeId == attr.Id ).Value;
                if ( !BaseURL.EndsWith( "/" ) )
                {
                    BaseURL += "/";
                }
            }
        }

        public string GetRequestResources( EventRequest request )
        {
            List<string> rt = new List<string>();
            if ( request.needsSpace )
            {
                rt.Add( "Room" );
            }
            if ( request.needsOnline )
            {
                rt.Add( "Online Event" );
            }
            if ( request.needsPub )
            {
                rt.Add( "Publicity" );
            }
            if ( request.needsCatering )
            {
                rt.Add( "Catering" );
            }
            if ( request.needsChildCare )
            {
                rt.Add( "Childcare" );
            }
            if ( request.needsReg )
            {
                rt.Add( "Registration" );
            }
            if ( request.needsAccom )
            {
                rt.Add( "Extra Resources" );
            }
            return String.Join( ",", rt );
        }
        public class EventRequest
        {
            public bool needsSpace { get; set; }
            public bool needsOnline { get; set; }
            public bool needsPub { get; set; }
            public bool needsReg { get; set; }
            public bool needsCatering { get; set; }
            public bool needsChildCare { get; set; }
            public bool needsAccom { get; set; }
            public bool IsSame { get; set; }
            public string Name { get; set; }
            public string Ministry { get; set; }
            public string Contact { get; set; }
            public List<string> EventDates { get; set; }
            public List<EventDetails> Events { get; set; }
            public string WhyAttendSixtyFive { get; set; }
            public string TargetAudience { get; set; }
            public bool EventIsSticky { get; set; }
            public DateTime? PublicityStartDate { get; set; }
            public DateTime? PublicityEndDate { get; set; }
            public List<string> PublicityStrategies { get; set; }
            public string WhyAttendNinety { get; set; }
            public List<string> GoogleKeys { get; set; }
            public string WhyAttendTen { get; set; }
            public string VisualIdeas { get; set; }
            public List<StoryItem> Stories { get; set; }
            public string WhyAttendTwenty { get; set; }
            public string Notes { get; set; }
            public bool HasConflicts { get; set; }
            public bool IsValid { get; set; }
            public List<string> ValidSections { get; set; }
        }
        public class StoryItem
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Description { get; set; }
        }
        public class EventDetails
        {
            public string EventDate { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public int? MinsStartBuffer { get; set; }
            public int? MinsEndBuffer { get; set; }
            public int? ExpectedAttendance { get; set; }
            public List<string> Rooms { get; set; }
            public string InfrastructureSpace { get; set; }
            public List<string> TableType { get; set; }
            public int? NumTablesRound { get; set; }
            public int? NumTablesRect { get; set; }
            public int? NumChairsRound { get; set; }
            public int? NumChairsRect { get; set; }
            public bool? NeedsTableCloths { get; set; }
            public bool? Checkin { get; set; }
            public bool? SupportTeam { get; set; }
            public string EventURL { get; set; }
            public string ZoomPassword { get; set; }
            public DateTime? RegistrationDate { get; set; }
            public DateTime? RegistrationEndDate { get; set; }
            public string RegistrationEndTime { get; set; }
            public List<string> FeeType { get; set; }
            public string FeeBudgetLine { get; set; }
            public string Fee { get; set; }
            public string CoupleFee { get; set; }
            public string OnlineFee { get; set; }
            public string Sender { get; set; }
            public string SenderEmail { get; set; }
            public string ThankYou { get; set; }
            public string TimeLocation { get; set; }
            public string AdditionalDetails { get; set; }
            public bool NeedsReminderEmail { get; set; }
            public string ReminderSender { get; set; }
            public string ReminderSenderEmail { get; set; }
            public string ReminderTimeLocation { get; set; }
            public string ReminderAdditionalDetails { get; set; }
            public string Vendor { get; set; }
            public string Menu { get; set; }
            public bool FoodDelivery { get; set; }
            public string FoodTime { get; set; }
            public string BudgetLine { get; set; }
            public string FoodDropOff { get; set; }
            public string CCVendor { get; set; }
            public string CCMenu { get; set; }
            public string CCFoodTime { get; set; }
            public string CCBudgetLine { get; set; }
            public List<string> ChildCareOptions { get; set; }
            public int? EstimatedKids { get; set; }
            public string CCStartTime { get; set; }
            public string CCEndTime { get; set; }
            public List<string> Drinks { get; set; }
            public string DrinkDropOff { get; set; }
            public string DrinkTime { get; set; }
            public List<string> TechNeeds { get; set; }
            public bool ShowOnCalendar { get; set; }
            public string PublicityBlurb { get; set; }
            public string TechDescription { get; set; }
            public string SetUp { get; set; }
            public bool? NeedsDoorsUnlocked { get; set; }
            public List<string> Doors { get; set; }
            public bool? NeedsMedical { get; set; }
            public bool? NeedsSecurity { get; set; }
        }
        public class Comment
        {
            public string CreatedBy { get; set; }
            public DateTime? CreatedOn { get; set; }
            public string Message { get; set; }
        }

        public class PartialApprovalChange
        {
            public string label { get; set; }
            public string field { get; set; }
            public bool isApproved { get; set; }
            public int? idx { get; set; }
        }
    }
}