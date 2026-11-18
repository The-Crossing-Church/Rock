using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Rock.Blocks.Plugins.ViewModels;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Blocks.Plugins.EventForm
{
    public class EventFormShared
    {
        #region Properties
        private int EventContentChannelId { get; set; }
        private int EventDetailsContentChannelId { get; set; }
        private int EventChangesContentChannelId { get; set; }
        private int EventDetailsChangesContentChannelId { get; set; }
        private RockContext _context { get; set; }
        private string RoomSetUpKey { get; set; }
        private string DiscountCodeKey { get; set; }
        private string AdditionalRegQuestionsKey { get; set; }
        private string OpsInventoryKey { get; set; }

        /// <summary>
        /// Map of the data contained in each event request and how it relates to each service provided by another team
        /// </summary>
        public List<SectionMap> SectionMappings
        {
            get
            {
                var map = new List<SectionMap>();
                map.Add( new SectionMap() { category = "Event", section = "Time" } );
                map.Add( new SectionMap() { attr = "NeedsSpace", occ_attr = "NeedsSpace", category = "Event Space", section = "Space", type = "Room" } );
                map.Add( new SectionMap() { attr = "NeedsOnline", occ_attr = "NeedsOnline", category = "Event Online", section = "Online", type = "Online Event" } );
                map.Add( new SectionMap()
                {
                    attr = "NeedsCatering",
                    occ_attr = "NeedsCatering",
                    category = "Event Catering",
                    section = "Catering",
                    type = "Catering",
                    status = "CateringStatus"
                } );
                map.Add( new SectionMap()
                {
                    attr = "NeedsChildCare",
                    occ_attr = "NeedsChildCare",
                    category = "Event Childcare",
                    section = "Childcare",
                    type = "Childcare",
                    status = "ChildcareStatus"
                } );
                map.Add( new SectionMap()
                {
                    attr = "NeedsChildCareCatering",
                    occ_attr = "NeedsChildCareCatering",
                    category = "Event Childcare Catering",
                    section = "Childcare Catering",
                    type = "Childcare Catering",
                    status = "ChildcareCateringStatus"
                } );
                map.Add( new SectionMap()
                {
                    attr = "NeedsChildCare",
                    occ_attr = "NeedsChildCare",
                    category = "Event Childcare Registration",
                    section = "Childcare Registration",
                    type = "Childcare",
                    status = "ChildcareRegistrationStatus"
                } );
                map.Add( new SectionMap()
                {
                    attr = "NeedsOpsAccommodations",
                    occ_attr = "NeedsOpsAccommodations",
                    category = "Event Ops Requests",
                    section = "Ops",
                    type = "Extra Resources",
                    status = "DailyOPSStatus"
                } );
                map.Add( new SectionMap()
                {
                    attr = "NeedsRegistration",
                    occ_attr = "EventNeedsSeparateLink",
                    category = "Event Registration",
                    section = "Registration",
                    type = "Registration",
                    status = "DatabaseStatus"
                } );
                map.Add( new SectionMap()
                {
                    attr = "NeedsCheckin",
                    occ_attr = "NeedsAttendanceOccurrence",
                    category = "Event Check-in",
                    section = "Check-in",
                    type = "Check-in",
                    status = "DatabaseStatus"
                } );
                map.Add( new SectionMap()
                {
                    attr = "NeedsPublicity",
                    occ_attr = "NeedsPublicity",
                    category = "Event Publicity",
                    section = "Publicity",
                    type = "Publicity",
                    status = "PublicityStatus"
                } );
                map.Add( new SectionMap()
                {
                    attr = "NeedsProductionAccommodations",
                    occ_attr = "NeedsProductionAccommodations",
                    category = "Event Production",
                    section = "Production",
                    type = "Production",
                    status = "ProductionStatus"
                } );
                map.Add( new SectionMap()
                {
                    attr = "NeedsWorship",
                    occ_attr = "NeedsWorship",
                    category = "Event Worship",
                    section = "Worship",
                    type = "Worship",
                    status = "WorshipStatus"
                } );
                map.Add( new SectionMap()
                {
                    attr = "NeedsWebCalendar",
                    occ_attr = "NeedsWebCalendar",
                    category = "Event Calendar",
                    section = "Calendar",
                    type = "Web Calendar",
                    status = "PublicityStatus"
                } );
                return map;
            }
        }
        #endregion

        public void InitializeEventFormHelper( int eventCCId, int eventDetailCCId, int eventChangesCCId, int eventDetailChangesCCId, string roomSetUpKey, string discountCodeKey, string opsInvKey, string addRegQuestionsKey )
        {
            EventContentChannelId = eventCCId;
            EventDetailsContentChannelId = eventDetailCCId;
            EventChangesContentChannelId = eventChangesCCId;
            EventDetailsChangesContentChannelId = eventDetailChangesCCId;
            _context = new RockContext();
            RoomSetUpKey = roomSetUpKey;
            DiscountCodeKey = discountCodeKey;
            AdditionalRegQuestionsKey = addRegQuestionsKey;
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
                else
                {
                    message += GetCategoryDetails( "Event Offsite", "Offsite", events[i], eventChanges );
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
            if ( ( item.AttributeValues["NeedsWorship"].Value == "True" && itemChanges == null ) || ( itemChanges != null && itemChanges.AttributeValues["NeedsWorship"].Value == "True" ) )
            {
                message += GetCategoryDetails( "Event Worship", "Worship Information", item, itemChanges );
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
                    message = "<strong>" + title + ":</strong>";
                    message = "<strong>" + title + ":</strong>";
                    message += "<div>";
                    message += "  <div style='display: inline-block; width: 48%; color: #cc3f0c !important;'>";
                    message += "    <ul style='color: #cc3f0c !important;'>";
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
                    message += "    </ul>";
                    message += "  </div>";
                    message += "  <div style='display: inline-block; width: 48%; color: #347689 !important;'>";
                    message += "    <ul style='color: #347689 !important;'>";
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
                    message += "    </ul>";
                    message += "  </div>";
                    message += "</div>";
                }
                else if ( key == OpsInventoryKey )
                {
                    List<OpsInventorySetUp> originalSetUp = JsonConvert.DeserializeObject<List<OpsInventorySetUp>>( original );
                    List<OpsInventorySetUp> currentSetUp = JsonConvert.DeserializeObject<List<OpsInventorySetUp>>( current );
                    message = "<strong>" + title + ":</strong>";
                    message += "<div>";
                    message += "  <div style='display: inline-block; width: 48%; color: #cc3f0c !important;'>";
                    message += "    <ul style='color: #cc3f0c !important;'>";
                    if ( originalSetUp != null )
                    {
                        for ( int i = 0; i < originalSetUp.Count(); i++ )
                        {
                            if ( !String.IsNullOrEmpty( originalSetUp[i].InventoryItem ) )
                            {
                                var item = new DefinedValueService( _context ).Get( Guid.Parse( originalSetUp[i].InventoryItem ) );
                                message += $"<li>{originalSetUp[i].QuantityNeeded} {item.Value} {( originalSetUp[i].QuantityNeeded > 1 && !item.Value.Trim().EndsWith( "s" ) ? "s" : "" )}";
                                if ( !String.IsNullOrEmpty( originalSetUp[i].InventoryDelivery ) )
                                {
                                    string[] deliveryInfo = originalSetUp[i].InventoryDelivery.Split( '\n' );
                                    message += "<ul>";
                                    for ( int k = 0; k < deliveryInfo.Length; k++ )
                                    {
                                        message += $"<li>{deliveryInfo[k]}</li>";
                                    }
                                    message += "</ul>";
                                }
                                message += "</li>";
                            }
                        }
                    }
                    else
                    {
                        message += "<li>Empty</li>";
                    }
                    message += "    </ul>";
                    message += "  </div>";
                    message += "  <div style='display: inline-block; width: 48%; color: #347689 !important;'>";
                    message += "    <ul style='color: #347689 !important;'>";
                    if ( currentSetUp != null )
                    {
                        for ( int i = 0; i < currentSetUp.Count(); i++ )
                        {
                            if ( !String.IsNullOrEmpty( currentSetUp[i].InventoryItem ) )
                            {
                                var item = new DefinedValueService( _context ).Get( Guid.Parse( currentSetUp[i].InventoryItem ) );
                                message += $"<li>{currentSetUp[i].QuantityNeeded} {item.Value} {( currentSetUp[i].QuantityNeeded > 1 && !item.Value.Trim().EndsWith( "s" ) ? "s" : "" )}";
                                if ( !String.IsNullOrEmpty( currentSetUp[i].InventoryDelivery ) )
                                {
                                    string[] deliveryInfo = currentSetUp[i].InventoryDelivery.Split( '\n' );
                                    message += "<ul>";
                                    for ( int k = 0; k < deliveryInfo.Length; k++ )
                                    {
                                        message += $"<li>{deliveryInfo[k]}</li>";
                                    }
                                    message += "</ul>";
                                }
                                message += "</li>";
                            }
                        }
                    }
                    else
                    {
                        message += "<li>Empty</li>";
                    }
                    message += "    </ul>";
                    message += "  </div>";
                    message += "</div>";
                }
                else if ( key == DiscountCodeKey )
                {
                    List<DiscountCodeSetUp> originalSetUp = JsonConvert.DeserializeObject<List<DiscountCodeSetUp>>( original );
                    List<DiscountCodeSetUp> currentSetUp = JsonConvert.DeserializeObject<List<DiscountCodeSetUp>>( current );
                    message = "<strong>" + title + ":</strong>";

                    message += "<div>";
                    message += "  <div style='display: inline-block; width: 48%; color: #cc3f0c !important;'>";
                    message += "    <ul style='color: #cc3f0c !important;'>";
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
                    message += "    </ul>";
                    message += "  </div>";
                    message += "  <div style='display: inline-block; width: 48%; color: #347689 !important;'>";
                    message += "    <ul style='color: #347689 !important;'>";
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
                    message += "    </ul>";
                    message += "  </div>";
                    message += "</div>";
                }
                else if ( key == AdditionalRegQuestionsKey )
                {
                    List<RegistrationQuestionConfig> originalSetUp = JsonConvert.DeserializeObject<List<RegistrationQuestionConfig>>( original );
                    List<RegistrationQuestionConfig> currentSetUp = JsonConvert.DeserializeObject<List<RegistrationQuestionConfig>>( current );
                    message = "<strong>" + title + ":</strong>";

                    message += "<div>";
                    message += "  <div style='display: inline-block; width: 48%; color: #cc3f0c !important;'>";
                    message += "    <ul style='color: #cc3f0c !important;'>";
                    if ( originalSetUp != null )
                    {
                        for ( int i = 0; i < originalSetUp.Count(); i++ )
                        {
                            message += $"<li>{originalSetUp[i].QuestionName} ({originalSetUp[i].QuestionFieldTypeName})</li>";
                        }
                    }
                    else
                    {
                        message += "<li>Empty</li>";
                    }
                    message += "    </ul>";
                    message += "  </div>";
                    message += "  <div style='display: inline-block; width: 48%; color: #347689 !important;'>";
                    message += "    <ul style='color: #347689 !important;'>";
                    if ( currentSetUp != null )
                    {
                        for ( int i = 0; i < currentSetUp.Count(); i++ )
                        {
                            message += $"<li>{currentSetUp[i].QuestionName} ({currentSetUp[i].QuestionFieldTypeName})</li>";
                        }
                    }
                    else
                    {
                        message += "<li>Empty</li>";
                    }
                    message += "    </ul>";
                    message += "  </div>";
                    message += "</div>";
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
                                message += $"<li>{originalSetUp[i].QuantityNeeded} {item.Value} {( originalSetUp[i].QuantityNeeded > 1 && !item.Value.Trim().EndsWith( "s" ) ? "s" : "" )}";
                                if ( !String.IsNullOrEmpty( originalSetUp[i].InventoryDelivery ) )
                                {
                                    string[] deliveryInfo = originalSetUp[i].InventoryDelivery.Split( '\n' );
                                    message += "<ul>";
                                    for ( int k = 0; k < deliveryInfo.Length; k++ )
                                    {
                                        message += $"<li>{deliveryInfo[k]}</li>";
                                    }
                                    message += "</ul>";
                                }
                                message += "</li>";
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
                else if ( key == AdditionalRegQuestionsKey )
                {
                    List<RegistrationQuestionConfig> originalSetUp = JsonConvert.DeserializeObject<List<RegistrationQuestionConfig>>( original );
                    message = "<strong>" + title + ":</strong> <ul>";
                    if ( originalSetUp != null )
                    {
                        for ( int i = 0; i < originalSetUp.Count(); i++ )
                        {
                            message += $"<li>{originalSetUp[i].QuestionName} ({originalSetUp[i].QuestionFieldTypeName})</li>";
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

        public RequestAuthorization CheckRequestPermissions( ContentChannelItem request, Person p, bool isEventAdmin, bool isRoomAdmin, Guid? sharedRequestGroupTypeGuid, string sharedWithAttributeKey )
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
                                    List<Guid?> limitedToMinistryGuid = sharingMembership[k].GetAttributeValue( "Ministry" ).Split( ',' ).AsGuidOrNullList().Where( g => g.HasValue ).ToList();
                                    if ( limitedToMinistryGuid.Any() )
                                    {
                                        Guid? requestMinistry = request.GetAttributeValue( "Ministry" ).AsGuidOrNull();
                                        if ( !requestMinistry.HasValue )
                                        {
                                            request.LoadAttributes();
                                            requestMinistry = request.GetAttributeValue( "Ministry" ).AsGuidOrNull();
                                        }
                                        if ( limitedToMinistryGuid.Contains( requestMinistry.Value ) )
                                        {
                                            auth.CanView = true;
                                            auth.CanEdit = membershipHasEdit;
                                        }
                                    }
                                    else
                                    {
                                        auth.CanView = true;
                                        auth.CanEdit = membershipHasEdit;
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
                    if ( !auth.CanEdit && !auth.CanView )
                    {
                        //If this request wasn't shared via group sharing, check it it was shared directly
                        var sharedWith = request.GetAttributeValue( sharedWithAttributeKey );

                        if ( !String.IsNullOrEmpty( sharedWith ) && sharedWith.Split( ',' ).Contains( p.Id.ToString() ) )
                        {
                            auth.CanView = true;
                            auth.CanEdit = true;
                        }
                    }
                }
            }

            return auth;
        }

        /// <summary>
        /// Method to get the Ministry Event Admins for a given Event Request
        /// </summary>
        /// <param name="item">The Event Request</param>
        /// <param name="sharedWithAttrKey">Sharing Attribute Key</param>
        /// <param name="sharedRequestGroupTypeGuid">Sharing Group Type</param>
        /// <returns></returns>
        public List<Person> GetRequestUsers( ContentChannelItem item, string sharedWithAttrKey, Guid? sharedRequestGroupTypeGuid )
        {
            List<Person> users = new List<Person>();
            RockContext context = new RockContext();
            PersonService p_svc = new PersonService( context );
            var sharedWithAttr = item.GetAttributeValue( sharedWithAttrKey );
            Guid? requestMinistry = item.GetAttributeValue( "Ministry" ).AsGuidOrNull();
            if ( sharedRequestGroupTypeGuid.HasValue )
            {
                //Shared requests are configured, find any for the current user.
                var SharedRequestGT = new GroupTypeService( context ).Get( sharedRequestGroupTypeGuid.Value );
                var groups = new GroupService( context ).Queryable().Where( g => g.GroupTypeId == SharedRequestGT.Id );
                var groupMembers = new GroupMemberService( context ).Queryable().Where( gm => gm.PersonId == item.CreatedByPersonId && gm.GroupRole.Name == "Request Creator" );
                groups = from g in groups
                         join gm in groupMembers on g.Id equals gm.GroupId
                         select g;
                var grpList = groups.ToList();
                for ( int k = 0; k < grpList.Count(); k++ )
                {
                    var additionalEditors = grpList[k].Members.Where( gm => gm.GroupRole.Name == "Can Edit" ).ToList();
                    additionalEditors.LoadAttributes();
                    for ( int j = 0; j < additionalEditors.Count(); j++ )
                    {
                        List<Guid?> limitedToMinistryGuid = additionalEditors[j].GetAttributeValue( "Ministry" ).Split( ',' ).AsGuidOrNullList().Where( g => g.HasValue ).ToList();
                        if ( limitedToMinistryGuid.Any() )
                        {
                            if ( limitedToMinistryGuid.Contains( requestMinistry.Value ) )
                            {
                                users.Add( additionalEditors[j].Person );
                            }
                        }
                        else
                        {
                            users.Add( additionalEditors[j].Person );
                        }
                    }
                }
            }
            List<int?> sharedRequests = new List<int?>();
            if ( !String.IsNullOrEmpty( sharedWithAttr ) )
            {
                List<int> ids = sharedWithAttr.Split( ',' ).Select( i =>
                {
                    if ( !String.IsNullOrEmpty( i ) )
                    {
                        int id;
                        if ( Int32.TryParse( i, out id ) )
                        {
                            return id;
                        }
                    }
                    return 0;
                } ).ToList();
                for ( int k = 0; k < ids.Count(); k++ )
                {
                    if ( ids[k] > 0 )
                    {
                        users.Add( p_svc.Get( ids[k] ) );
                    }
                }
            }
            users.Add( item.CreatedByPersonAlias.Person );
            users = users.Distinct().ToList();
            return users;
        }

        /// <summary>
        /// Methos to get the OPS Event Directors
        /// </summary>
        /// <param name="eventAdminRole">The block attribute value for the security role</param>
        /// <returns></returns>
        public List<Person> GetAdminUsers( string eventAdminRole )
        {
            List<Person> users = new List<Person>();
            RockContext context = new RockContext();
            Guid securityRoleGuid = Guid.Empty;
            if ( Guid.TryParse( eventAdminRole, out securityRoleGuid ) )
            {
                Rock.Model.Group securityRole = new GroupService( context ).Get( securityRoleGuid );
                users.AddRange( securityRole.Members.Select( gm => gm.Person ) );
            }
            users = users.Distinct().ToList();
            return users;
        }

        /// <summary>
        /// Method to build the communication and send it
        /// </summary>
        /// <param name="subject">Subject of Email</param>
        /// <param name="message">Content of Email</param>
        /// <param name="users">Recipients</param>
        public void SendNotification( string subject, string message, List<Person> users )
        {
            RockEmailMessage email = new RockEmailMessage();
            users = users.Distinct().ToList();
            for ( int i = 0; i < users.Count(); i++ )
            {
                RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( users[i], new Dictionary<string, object>() );
                email.AddRecipient( recipient );
            }
            email.Subject = subject;
            email.Message = message;
            email.FromEmail = "system@thecrossingchurch.com";
            email.FromName = "The Crossing System";
            email.CreateCommunicationRecord = true;
            var output = email.Send();
        }
    }
    #region Helper Classes
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
        public string InventoryDelivery { get; set; }
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
    public class RegistrationQuestionConfig
    {
        public string QuestionName { get; set; }
        public string QuestionFieldType { get; set; }
        public string QuestionFieldTypeName { get; set; }
        public string QuestionConfigurationValues { get; set; }
    }
    public class RequestAuthorization
    {
        public int RequestId { get; set; }
        public bool CanEdit { get; set; }
        public bool CanView { get; set; }
    }
    public class Filters
    {
        public string title { get; set; }
        public string ministry { get; set; }
        public List<string> statuses { get; set; }
        public List<string> resources { get; set; }
        public DateRangeParts eventDates { get; set; }
        public DateRangeParts eventModified { get; set; }
        public string submitter { get; set; }
    }
    public class DateRangeParts
    {
        public string lowerValue { get; set; }
        public string upperValue { get; set; }
    }
    public class RoleMap
    {
        public string PageAttributeKey { get; set; }
        public string RoleAttributeKey { get; set; }
        public string Tag { get; set; }
    }
    public class SectionMap
    {
        /// <summary>
        /// The Event Request Attribute that indicates the service is being requested
        /// </summary>
        public string attr { get; set; }
        /// <summary>
        /// The Event Request Details Attribute that indicates the service is being requested on that occurrence 
        /// </summary>
        public string occ_attr { get; set; }
        /// <summary>
        /// The Attribute Category assigned to information for that service
        /// </summary>
        public string category { get; set; }
        /// <summary>
        /// The friendly name of the section
        /// </summary>
        public string section { get; set; }
        /// <summary>
        /// The Event Request Attribute that indicates the current status (Needs Confirmation/Approval, Confirmed/Approved, or Denied) of that service
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// The Event Request Attribute Values that indicates what resources were requested (Room, Online, Catering, etc) 
        /// </summary>
        public string type { get; set; }
    }
    public class Comment
    {
        public ContentChannelItemBag comment { get; set; }
        public string createdBy { get; set; }
    }
    #endregion
}
