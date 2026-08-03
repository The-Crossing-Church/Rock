using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Model;

namespace Rock.Blocks.Plugins.EventForm
{
    public class EventFormShared
    {
        private int EventContentChannelId { get; set; }
        private int EventDetailsContentChannelId { get; set; }
        private int EventChangesContentChannelId { get; set; }
        private int EventDetailsChangesContentChannelId { get; set; }
        private RockContext _context { get; set; }
        private string RoomSetUpKey { get; set; }
        private string DiscountCodeKey { get; set; }
        private string OpsInventoryKey { get; set; }


        public void InitializeEventFormHelper( int eventCCId, int eventDetailCCId, int eventChangesCCId, int eventDetailChangesCCId, string roomSetUpKey, string discountCodeKey, string opsInvKey )
        {
            EventContentChannelId = eventCCId;
            EventDetailsContentChannelId = eventDetailCCId;
            EventChangesContentChannelId = eventChangesCCId;
            EventDetailsChangesContentChannelId = eventDetailChangesCCId;
            _context = new RockContext();
            RoomSetUpKey = roomSetUpKey;
            DiscountCodeKey = discountCodeKey;
            OpsInventoryKey = opsInvKey;
        }

        /// <summary>
        /// Generate the current state of the request for email/printing
        /// </summary>
        /// <param name="item">Event Request</param>
        /// <param name="events">Event Request Details</param>
        /// <returns>Formatted text for current state of request</returns>
        public string GetRequestDetails( ContentChannelItem item, List<ContentChannelItem> events )
        {
            ContentChannelItem itemChanges = null;
            ContentChannelItemAssociation itemChangesAssoc = item.ParentItems.FirstOrDefault( ci => ci.ContentChannelItem.ContentChannelId == EventContentChannelId );
            if ( item.ContentChannelId == EventChangesContentChannelId && itemChangesAssoc != null )
            {
                itemChanges = item;
                item = itemChangesAssoc.ContentChannelItem;
                events = item.ChildItems.Where( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( ci => ci.ChildContentChannelItem ).ToList();
                events.LoadAttributes();
                item.LoadAttributes();
                //itemChanges.LoadAttributes();
            }
            string message = "";
            message += RenderValue( "Ministry", item.AttributeValues["Ministry"].ValueFormatted, itemChanges != null ? itemChanges.AttributeValues["Ministry"].ValueFormatted : "" );
            string changeTitle = itemChanges != null ? itemChanges.Title : "";
            if ( item.AttributeValues["RequestType"].Value == "Room" )
            {
                message += RenderValue( "Meeting Listing on Calendar", item.Title, itemChanges != null ? changeTitle : "" );
            }
            else
            {
                message += RenderValue( "Event Name on Calendar", item.Title, itemChanges != null ? changeTitle : "" );

            }
            message += RenderValue( "Ministry Contact", item.AttributeValues["Contact"].ValueFormatted, itemChanges != null ? itemChanges.AttributeValues["Contact"].ValueFormatted : "" );
            message += "<br/>";

            for ( int i = 0; i < events.Count(); i++ )
            {
                ContentChannelItem eventChanges = null;
                ContentChannelItemAssociation eventChangesAssoc = events[i].ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsChangesContentChannelId );
                if ( eventChangesAssoc != null )
                {
                    eventChanges = eventChangesAssoc.ChildContentChannelItem;
                    eventChanges.LoadAttributes();
                }
                message += "<div style='font-size: 18px; margin-bottom: 16px;'><strong style='color: #6485b3;'>Date Information</strong><br/>";
                if ( events.Count() == 1 )
                {
                    message += RenderValue( "Event Dates", String.Join( ", ", item.AttributeValues["EventDates"].Value.Split( ',' ).Select( e => DateTime.Parse( e.Trim() ).ToString( "MM/dd/yyyy" ) ) ), itemChanges != null ? String.Join( ", ", itemChanges.AttributeValues["EventDates"].Value.Split( ',' ).Select( e => DateTime.Parse( e.Trim() ).ToString( "MM/dd/yyyy" ) ) ) : "" );

                }
                else
                {
                    message += RenderValue( "Event Date", DateTime.Parse( events[i].AttributeValues["EventDate"].Value ).ToString( "MM/dd/yyyy" ), eventChanges != null ? DateTime.Parse( eventChanges.AttributeValues["EventDate"].Value ).ToString( "MM/dd/yyyy" ) : "" );
                }
                if ( !String.IsNullOrEmpty( events[i].AttributeValues["StartTime"].Value ) || ( eventChanges != null && !String.IsNullOrEmpty( eventChanges.AttributeValues["StartTime"].Value ) ) )
                {
                    message += RenderValue( "Start Time", events[i].AttributeValues["StartTime"].ValueFormatted, eventChanges != null ? eventChanges.AttributeValues["StartTime"].ValueFormatted : "" );
                }
                if ( !String.IsNullOrEmpty( events[i].AttributeValues["EndTime"].Value ) || ( eventChanges != null && !String.IsNullOrEmpty( eventChanges.AttributeValues["EndTime"].Value ) ) )
                {
                    message += RenderValue( "End Time", events[i].AttributeValues["EndTime"].ValueFormatted, eventChanges != null ? eventChanges.AttributeValues["EndTime"].ValueFormatted : "" );
                }
                if ( !String.IsNullOrEmpty( events[i].AttributeValues["StartBuffer"].Value ) || ( eventChanges != null && !String.IsNullOrEmpty( eventChanges.AttributeValues["StartBuffer"].Value ) ) )
                {
                    message += RenderValue( "Start Time Set-up Buffer", events[i].AttributeValues["StartBuffer"].ValueFormatted, eventChanges != null ? eventChanges.AttributeValues["StartBuffer"].ValueFormatted : "" );
                }
                if ( !String.IsNullOrEmpty( events[i].AttributeValues["EndBuffer"].Value ) || ( eventChanges != null && !String.IsNullOrEmpty( eventChanges.AttributeValues["EndBuffer"].Value ) ) )
                {
                    message += RenderValue( "End Time Tear-down Buffer", events[i].AttributeValues["EndBuffer"].ValueFormatted, eventChanges != null ? eventChanges.AttributeValues["EndBuffer"].ValueFormatted : "" );
                }
                message += "</div>";

                if ( ( item.AttributeValues["NeedsSpace"].Value == "True" && itemChanges == null ) || ( itemChanges != null && itemChanges.AttributeValues["NeedsSpace"].Value == "True" ) )
                {
                    message += GetCategoryDetails( "Event Space", "Space", events[i], eventChanges );
                }
                if ( ( item.AttributeValues["NeedsCatering"].Value == "True" && itemChanges == null ) || ( itemChanges != null && itemChanges.AttributeValues["NeedsCatering"].Value == "True" ) )
                {
                    message += GetCategoryDetails( "Event Catering", "Catering", events[i], eventChanges );
                }
                if ( ( item.AttributeValues["NeedsOpsAccommodations"].Value == "True" && itemChanges == null ) || ( itemChanges != null && itemChanges.AttributeValues["NeedsOpsAccommodations"].Value == "True" ) )
                {
                    string exclusion = "";
                    if ( ( item.AttributeValues["NeedsCatering"].Value == "True" && itemChanges == null ) || ( itemChanges != null && itemChanges.AttributeValues["NeedsCatering"].Value == "True" ) )
                    {
                        exclusion = "Event Catering";
                    }
                    message += GetCategoryDetails( "Event Ops Requests", "Ops Accommodations", events[i], eventChanges, exclusion );
                }
                if ( ( item.AttributeValues["NeedsChildCare"].Value == "True" && itemChanges == null ) || ( itemChanges != null && itemChanges.AttributeValues["NeedsChildCare"].Value == "True" ) )
                {
                    message += GetCategoryDetails( "Event Childcare", "Childcare", events[i], eventChanges, "Event Childcare Catering" );
                }
                if ( ( item.AttributeValues["NeedsChildCareCatering"].Value == "True" && itemChanges == null ) || ( itemChanges != null && itemChanges.AttributeValues["NeedsChildCareCatering"].Value == "True" ) )
                {
                    message += GetCategoryDetails( "Event Childcare Catering", "Childcare Catering", events[i], eventChanges );
                }
                if ( ( item.AttributeValues["NeedsRegistration"].Value == "True" && itemChanges == null ) || ( itemChanges != null && itemChanges.AttributeValues["NeedsRegistration"].Value == "True" ) )
                {
                    message += GetCategoryDetails( "Event Registration", "Registration", events[i], eventChanges );
                }
                if ( ( item.AttributeValues["NeedsOnline"].Value == "True" && itemChanges == null ) || ( itemChanges != null && itemChanges.AttributeValues["NeedsOnline"].Value == "True" ) )
                {
                    message += GetCategoryDetails( "Event Online", "Zoom", events[i], eventChanges );
                }
                message += "<br/>";
            }

            if ( ( item.AttributeValues["NeedsWebCalendar"].Value == "True" && itemChanges == null ) || ( itemChanges != null && itemChanges.AttributeValues["NeedsWebCalendar"].Value == "True" ) )
            {
                if ( !String.IsNullOrEmpty( item.AttributeValues["WebCalendarDescription"].Value ) || !String.IsNullOrEmpty( item.AttributeValues["WebCalendarGoLive"].Value ) )
                {
                    message += "<div style='font-size: 18px; margin-bottom: 16px;'><strong style='color: #6485b3;'>Web Calendar Information</strong><br/>";
                    message += RenderValue( item.Attributes["WebCalendarGoLive"].Name, item.AttributeValues["WebCalendarGoLive"].ValueFormatted, itemChanges != null ? itemChanges.AttributeValues["WebCalendarGoLive"].ValueFormatted : "" );
                    message += RenderValue( item.Attributes["WebCalendarDescription"].Name, item.AttributeValues["WebCalendarDescription"].Value, itemChanges != null ? itemChanges.AttributeValues["WebCalendarDescription"].Value : "" );
                    message += "</div>";
                }
            }
            if ( ( item.AttributeValues["NeedsPublicity"].Value == "True" && itemChanges == null ) || ( itemChanges != null && itemChanges.AttributeValues["NeedsPublicity"].Value == "True" ) )
            {
                message += GetCategoryDetails( "Event Publicity", "Publicity", item, itemChanges );
            }
            if ( ( item.AttributeValues["NeedsProductionAccommodations"].Value == "True" && itemChanges == null ) || ( itemChanges != null && itemChanges.AttributeValues["NeedsProductionAccommodations"].Value == "True" ) )
            {
                message += GetCategoryDetails( "Event Production", "Production Accommodations", item, itemChanges );
            }
            if ( !String.IsNullOrEmpty( item.AttributeValues["Notes"].Value ) || ( itemChanges != null && !String.IsNullOrEmpty( itemChanges.AttributeValues["Notes"].Value ) ) )
            {
                message += "<br/><strong style='color: #6485b3;'>Additional Notes</strong><br/>";
                message += RenderValue( "Notes", item.AttributeValues["Notes"].Value, itemChanges != null ? itemChanges.AttributeValues["Notes"].Value : "", fieldTypeGuid: item.Attributes["Notes"].FieldType.Guid );
            }

            return message;
        }

        /// <summary>
        /// Generate Category Heading and Display Attributes in Category
        /// </summary>
        /// <param name="category">Name of Category</param>
        /// <param name="sectionTitle">Title of Section</param>
        /// <param name="item">Event Request</param>
        /// <param name="itemChanges">Changes Requested to Event Request</param>
        /// <returns></returns>
        private string GetCategoryDetails( string category, string sectionTitle, ContentChannelItem item, ContentChannelItem itemChanges, string excludeCategory = "" )
        {
            string message = "";
            var attrs = item.Attributes.Where( a => a.Value.Categories.Select( c => c.Name ).Contains( category ) && ( excludeCategory == "" || !a.Value.Categories.Select( c => c.Name ).Contains( excludeCategory ) ) ).OrderBy( a => a.Value.Order ).Select( a => a.Key ).ToList();
            if ( attrs.Count() > 0 )
            {
                message += "<div style='font-size: 18px; margin-bottom: 16px;'><strong style='color: #6485b3;'>" + sectionTitle + " Information</strong><br/>";
            }
            for ( int k = 0; k < attrs.Count(); k++ )
            {
                message += RenderValue( item.Attributes[attrs[k]].Name, item.AttributeValues[attrs[k]].ValueFormatted, itemChanges != null ? itemChanges.AttributeValues[attrs[k]].ValueFormatted : "", attrs[k], item.Attributes[attrs[k]].FieldType.Guid );
            }
            if ( attrs.Count() > 0 )
            {
                message += "</div>";
            }
            return message;
        }

        private string RenderValue( string title, string original, string current, string key = "", Guid? fieldTypeGuid = null )
        {
            string message = "";
            if ( !String.IsNullOrEmpty( current ) && original != current )
            {
                if ( key == RoomSetUpKey )
                {
                    List<TableSetUp> originalSetUp = JsonConvert.DeserializeObject<List<TableSetUp>>( original );
                    List<TableSetUp> currentSetUp = JsonConvert.DeserializeObject<List<TableSetUp>>( current );
                    message = "<strong>" + title + ":</strong> <ul style='color: #cc3f0c !important;'>";
                    if ( originalSetUp != null )
                    {
                        for ( int i = 0; i < originalSetUp.Count(); i++ )
                        {
                            if ( !String.IsNullOrEmpty( originalSetUp[i].Room ) )
                            {
                                var room = new DefinedValueService( _context ).Get( Guid.Parse( originalSetUp[i].Room ) );
                                message += $"<li>{room.Value}: {originalSetUp[i].NumberofTables} {( originalSetUp[i].NeedsTablecloths == "True" ? "Clothed " : String.Empty )}{originalSetUp[i].TypeofTable} tables with {originalSetUp[i].NumberofChairs} each.</li>";
                            }
                        }
                    }
                    else
                    {
                        message += "<li>Empty</li>";
                    }
                    message += "</ul> <ul style='color: #347689 !important;'>";
                    if ( currentSetUp != null )
                    {
                        for ( int i = 0; i < currentSetUp.Count(); i++ )
                        {
                            if ( !String.IsNullOrEmpty( currentSetUp[i].Room ) )
                            {
                                var room = new DefinedValueService( _context ).Get( Guid.Parse( currentSetUp[i].Room ) );
                                message += $"<li>{room}: {currentSetUp[i].NumberofTables} {( currentSetUp[i].NeedsTablecloths == "True" ? "Clothed " : String.Empty )}{currentSetUp[i].TypeofTable} tables with {currentSetUp[i].NumberofChairs} each.</li>";
                            }
                        }
                    }
                    else
                    {
                        message += "<li>Empty</li>";
                    }
                    message += "</ul>";
                }
                else if ( key == OpsInventoryKey )
                {
                    List<OpsInventorySetUp> originalSetUp = JsonConvert.DeserializeObject<List<OpsInventorySetUp>>( original );
                    List<OpsInventorySetUp> currentSetUp = JsonConvert.DeserializeObject<List<OpsInventorySetUp>>( current );
                    message = "<strong>" + title + ":</strong> <ul style='color: #cc3f0c !important;'>";
                    if ( originalSetUp != null )
                    {
                        for ( int i = 0; i < originalSetUp.Count(); i++ )
                        {
                            if ( !String.IsNullOrEmpty( originalSetUp[i].InventoryItem ) )
                            {
                                var item = new DefinedValueService( _context ).Get( Guid.Parse( originalSetUp[i].InventoryItem ) );
                                message += $"<li>{originalSetUp[i].QuantityNeeded} {item.Value} {( originalSetUp[i].QuantityNeeded > 1 && !item.Value.Trim().EndsWith( "s" ) ? "s" : "" )}</li>";
                            }
                        }
                    }
                    else
                    {
                        message += "<li>Empty</li>";
                    }
                    message += "</ul> <ul style='color: #347689 !important;'>";
                    if ( currentSetUp != null )
                    {
                        for ( int i = 0; i < currentSetUp.Count(); i++ )
                        {
                            if ( !String.IsNullOrEmpty( currentSetUp[i].InventoryItem ) )
                            {
                                var item = new DefinedValueService( _context ).Get( Guid.Parse( currentSetUp[i].InventoryItem ) );
                                message += $"<li>{currentSetUp[i].QuantityNeeded} {item.Value} {( currentSetUp[i].QuantityNeeded > 1 && !item.Value.Trim().EndsWith( "s" ) ? "s" : "" )}</li>";
                            }
                        }
                    }
                    else
                    {
                        message += "<li>Empty</li>";
                    }
                    message += "</ul>";

                }
                else if ( key == DiscountCodeKey )
                {
                    List<DiscountCodeSetUp> originalSetUp = JsonConvert.DeserializeObject<List<DiscountCodeSetUp>>( original );
                    List<DiscountCodeSetUp> currentSetUp = JsonConvert.DeserializeObject<List<DiscountCodeSetUp>>( current );
                    message = "<strong>" + title + ":</strong> <ul style='color: #cc3f0c !important;'>";
                    if ( originalSetUp != null )
                    {
                        for ( int i = 0; i < originalSetUp.Count(); i++ )
                        {
                            string dates = "";
                            if ( !String.IsNullOrEmpty( originalSetUp[i].EffectiveDateRange ) )
                            {
                                dates = String.Join( " - ", originalSetUp[i].EffectiveDateRange.Split( ',' ).Select( d => DateTime.Parse( d ).ToString( "MM/dd/yy" ) ) );
                            }
                            if ( originalSetUp[i].CodeType == "$" )
                            {
                                message += $"<li>{originalSetUp[i].Code}: {originalSetUp[i].CodeType}{originalSetUp[i].Amount}, Auto-Apply: {originalSetUp[i].AutoApply}, Date Range: {dates}, Max Usage: {originalSetUp[i].MaxUses}</li>";
                            }
                            else
                            {
                                message += $"<li>{originalSetUp[i].Code}: {originalSetUp[i].Amount}{originalSetUp[i].CodeType}, Auto-Apply: {originalSetUp[i].AutoApply}, Date Range: {dates}, Max Usage: {originalSetUp[i].MaxUses}</li>";
                            }
                        }
                    }
                    else
                    {
                        message += "<li>Empty</li>";
                    }
                    message += "</ul> <ul style='color: #347689 !important;'>";
                    if ( currentSetUp != null )
                    {
                        for ( int i = 0; i < currentSetUp.Count(); i++ )
                        {
                            string dates = "";
                            if ( !String.IsNullOrEmpty( currentSetUp[i].EffectiveDateRange ) )
                            {
                                dates = String.Join( " - ", currentSetUp[i].EffectiveDateRange.Split( ',' ).Select( d => DateTime.Parse( d ).ToString( "MM/dd/yy" ) ) );
                            }
                            if ( currentSetUp[i].CodeType == "$" )
                            {
                                message += $"<li>{currentSetUp[i].Code}: {currentSetUp[i].CodeType}{currentSetUp[i].Amount}, Auto-Apply: {currentSetUp[i].AutoApply}, Date Range: {dates}, Max Usage: {currentSetUp[i].MaxUses}</li>";
                            }
                            else
                            {
                                message += $"<li>{currentSetUp[i].Code}: {currentSetUp[i].Amount}{currentSetUp[i].CodeType}, Auto-Apply: {currentSetUp[i].AutoApply}, Date Range: {dates}, Max Usage: {currentSetUp[i].MaxUses}</li>";
                            }
                        }
                    }
                    else
                    {
                        message += "<li>Empty</li>";
                    }
                    message += "</ul>";

                }
                else if ( fieldTypeGuid.HasValue && fieldTypeGuid.Value == SystemGuid.FieldType.MEMO.AsGuid() )
                {
                    message = "<strong>" + title + ":</strong>";
                    message += "<div>";
                    message += "  <div style='display: inline-block; width: 48%; color: #cc3f0c !important;'>";
                    message += original.Replace( "\n", "<br/>" );
                    message += "  </div>";
                    message += "  <div style='display: inline-block; width: 48%; color: #347689 !important;'>";
                    message += current.Replace( "\n", "<br/>" );
                    message += "  </div>";
                    message += "</div>";
                }
                else
                {
                    message = "<strong>" + title + ":</strong> <span style='color: #cc3f0c !important;'>" + original + "</span> <span style='color: #347689 !important;'>" + current + "</span><br/>";
                }
            }
            else
            {
                if ( key == RoomSetUpKey )
                {
                    List<TableSetUp> originalSetUp = JsonConvert.DeserializeObject<List<TableSetUp>>( original );
                    message = "<strong>" + title + ":</strong> <ul>";
                    if ( originalSetUp != null )
                    {
                        for ( int i = 0; i < originalSetUp.Count(); i++ )
                        {
                            if ( !String.IsNullOrEmpty( originalSetUp[i].Room ) )
                            {
                                var room = new DefinedValueService( _context ).Get( Guid.Parse( originalSetUp[i].Room ) );
                                message += $"<li>{room.Value}: {originalSetUp[i].NumberofTables} {( originalSetUp[i].NeedsTablecloths == "True" ? "Clothed " : String.Empty )}{originalSetUp[i].TypeofTable} tables with {originalSetUp[i].NumberofChairs} each.</li>";
                            }
                        }
                    }
                    message += "</ul>";
                }
                else if ( key == OpsInventoryKey )
                {
                    List<OpsInventorySetUp> originalSetUp = JsonConvert.DeserializeObject<List<OpsInventorySetUp>>( original );
                    message = "<strong>" + title + ":</strong> <ul>";
                    if ( originalSetUp != null )
                    {
                        for ( int i = 0; i < originalSetUp.Count(); i++ )
                        {
                            if ( !String.IsNullOrEmpty( originalSetUp[i].InventoryItem ) )
                            {
                                var item = new DefinedValueService( _context ).Get( Guid.Parse( originalSetUp[i].InventoryItem ) );
                                message += $"<li>{originalSetUp[i].QuantityNeeded} {item.Value} {( originalSetUp[i].QuantityNeeded > 1 && !item.Value.Trim().EndsWith( "s" ) ? "s" : "" )}</li>";
                            }
                        }
                    }
                    message += "</ul>";
                }
                else if ( key == DiscountCodeKey )
                {
                    List<DiscountCodeSetUp> originalSetUp = JsonConvert.DeserializeObject<List<DiscountCodeSetUp>>( original );
                    message = "<strong>" + title + ":</strong> <ul>";
                    if ( originalSetUp != null )
                    {
                        for ( int i = 0; i < originalSetUp.Count(); i++ )
                        {
                            string dates = "";
                            if ( !String.IsNullOrEmpty( originalSetUp[i].EffectiveDateRange ) )
                            {
                                dates = String.Join( " - ", originalSetUp[i].EffectiveDateRange.Split( ',' ).Select( d => DateTime.Parse( d ).ToString( "MM/dd/yy" ) ) );
                            }
                            if ( originalSetUp[i].CodeType == "$" )
                            {
                                message += $"<li>{originalSetUp[i].Code}: {originalSetUp[i].CodeType}{originalSetUp[i].Amount}, Auto-Apply: {originalSetUp[i].AutoApply}, Date Range: {dates}, Max Usage: {originalSetUp[i].MaxUses}</li>";
                            }
                            else
                            {
                                message += $"<li>{originalSetUp[i].Code}: {originalSetUp[i].Amount}{originalSetUp[i].CodeType}, Auto-Apply: {originalSetUp[i].AutoApply}, Date Range: {dates}, Max Usage: {originalSetUp[i].MaxUses}</li>";
                            }
                        }
                    }
                    message += "</ul>";
                }
                else if ( fieldTypeGuid.HasValue && fieldTypeGuid.Value == SystemGuid.FieldType.MEMO.AsGuid() )
                {
                    message = "<strong>" + title + ":</strong> <br/>" + original.Replace( "\n", "<br/>" ) + "<br/>";
                }
                else
                {
                    message = "<strong>" + title + ":</strong> " + original + "<br/>";
                }
            }
            return message;
        }

        public RequestAuthorization CheckRequestPermissions( ContentChannelItem request, Person p, bool isEventAdmin, bool isRoomAdmin, Guid? sharedRequestGroupTypeGuid )
        {
            RequestAuthorization auth = new RequestAuthorization() { RequestId = request.Id, CanEdit = false, CanView = false };

            using ( RockContext context = new RockContext() )
            {
                var ids = p.Aliases.Select( pa => pa.Id ).ToList();
                //Created the request or is an Event/Room Admin
                if ( request.Id == 0 )
                {
                    auth.CanEdit = true;
                    auth.CanView = true;
                }
                else if ( ids.Contains( request.CreatedByPersonAliasId.Value ) || isEventAdmin || isRoomAdmin )
                {
                    auth.CanEdit = true;
                    auth.CanView = true;
                }
                else
                {
                    if ( sharedRequestGroupTypeGuid.HasValue )
                    {
                        var sharedRequestGT = new GroupTypeService( context ).Get( sharedRequestGroupTypeGuid.Value );
                        //Anything but Request Creator roles
                        var sharingMembership = new GroupMemberService( context ).Queryable().Where( gm => gm.GroupTypeId == sharedRequestGT.Id && gm.PersonId == p.Id && !gm.GroupRole.IsLeader ).ToList();
                        for ( int k = 0; k < sharingMembership.Count(); k++ )
                        {
                            GroupMember creator = sharingMembership[k].Group.Members.FirstOrDefault( gm => gm.GroupRole.IsLeader );
                            bool membershipHasEdit = false;
                            if ( sharingMembership[k].GroupRole.Name == "Can Edit" )
                            {
                                membershipHasEdit = true;
                            }
                            if ( creator != null )
                            {
                                //The request in question belongs to someone in a sharing group witht the current person
                                ids = creator.Person.Aliases.Select( pa => pa.Id ).ToList();
                                if ( ids.Contains( request.CreatedByPersonAliasId.Value ) )
                                {
                                    sharingMembership[k].LoadAttributes();
                                    Guid? limitedToMinistryGuid = sharingMembership[k].GetAttributeValue( "Ministry" ).AsGuidOrNull();
                                    if ( limitedToMinistryGuid.HasValue )
                                    {
                                        Guid? requestMinistry = request.GetAttributeValue( "Ministry" ).AsGuidOrNull();
                                        if ( !requestMinistry.HasValue )
                                        {
                                            request.LoadAttributes();
                                            requestMinistry = request.GetAttributeValue( "Ministry" ).AsGuidOrNull();
                                        }
                                        if ( limitedToMinistryGuid.Value == requestMinistry.Value )
                                        {
                                            auth.CanView = true;
                                            auth.CanEdit = membershipHasEdit;
                                        }
                                    }
                                }
                                else
                                {
                                    auth.CanView = true;
                                    auth.CanEdit = membershipHasEdit;
                                }
                            }
                        }
                    }
                }
            }

            return auth;
        }
    }
    public class TableSetUp
    {
        public string Room { get; set; }
        public string TypeofTable { get; set; }
        public int NumberofTables { get; set; }
        public int NumberofChairs { get; set; }
        public string NeedsTablecloths { get; set; }
    }
    public class OpsInventorySetUp
    {
        public string InventoryItem { get; set; }
        public int QuantityNeeded { get; set; }
    }
    public class DiscountCodeSetUp
    {
        public string CodeType { get; set; }
        public string Code { get; set; }
        public int Amount { get; set; }
        public string AutoApply { get; set; }
        public string EffectiveDateRange { get; set; }
        public int? MaxUses { get; set; }
    }
    public class RequestAuthorization
    {
        public int RequestId { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }
    }
}
