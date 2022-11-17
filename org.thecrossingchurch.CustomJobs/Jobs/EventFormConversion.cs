// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System.Text.RegularExpressions;
using System;
using Rock.Communication;
using Rock;
using Rock.Web.Cache;
using Newtonsoft.Json;
using System.IO;

namespace org.crossingchurch.CrossingStudentsSteps.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [IntegerField( "Event Request Id", "", false )]
    [ContentChannelField( "Event CC", category: "Content Channels", order: 0 )]
    [ContentChannelField( "Event Pending Changes CC", category: "Content Channels", order: 1 )]
    [ContentChannelField( "Event Details CC", category: "Content Channels", order: 2 )]
    [ContentChannelField( "Event Details Pending Changes CC", category: "Content Channels", order: 3 )]
    [ContentChannelField( "Comments CC", category: "Content Channels", order: 4 )]
    [DefinedTypeField( "Ministry", category: "Lists", order: 0 )]
    [DefinedTypeField( "Budget Line", category: "Lists", order: 1 )]
    [DefinedTypeField( "Locations", category: "Lists", order: 2 )]
    [DefinedTypeField( "Drinks", category: "Lists", order: 3 )]
    [IntegerField( "Matrix Id", "", false )]
    [PersonField( "Andrew", category: "Users", order: 0 )]
    [PersonField( "Christian", category: "Users", order: 1 )]
    [PersonField( "Kaelyn", category: "Users", order: 2 )]
    [IntegerField( "Skip", defaultValue: 391 )]
    [IntegerField( "Event Date Attribute Id", "Attribute Id of the Event Date Attribute on the Event Details CC" )]
    [DisallowConcurrentExecution]
    public class EventFormConversion : IJob
    {
        ///Variables
        private RockContext _context { get; set; }
        private ContentChannelItemService _cci_svc { get; set; }
        private ContentChannelService _cc_svc { get; set; }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public EventFormConversion()
        {
        }

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            _context = new RockContext();
            _cci_svc = new ContentChannelItemService( _context );
            _cc_svc = new ContentChannelService( _context );
            var _ccia_svc = new ContentChannelItemAssociationService( _context );

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            int? eventid = null;
            if ( !String.IsNullOrEmpty( dataMap.GetString( "EventRequestId" ) ) )
            {
                eventid = Int32.Parse( dataMap.GetString( "EventRequestId" ) );
            }
            Guid eventGuid = Guid.Parse( dataMap.GetString( "EventCC" ) );
            Guid eventDetailsGuid = Guid.Parse( dataMap.GetString( "EventDetailsCC" ) );
            Guid eventChangesGuid = Guid.Parse( dataMap.GetString( "EventPendingChangesCC" ) );
            Guid eventDetailsChangesGuid = Guid.Parse( dataMap.GetString( "EventDetailsPendingChangesCC" ) );
            Guid commentsGuid = Guid.Parse( dataMap.GetString( "CommentsCC" ) );
            int? eventDateAttrId = dataMap.GetString( "EventDateAttributeId" ).AsIntegerOrNull();

            //Defined Types
            Guid ministryListGuid = Guid.Parse( dataMap.GetString( "Ministry" ) );
            int MinistryDefinedTypeId = new DefinedTypeService( _context ).Get( ministryListGuid ).Id;
            List<DefinedValue> ministries = new DefinedValueService( _context ).Queryable().Where( dv => dv.DefinedTypeId == MinistryDefinedTypeId ).OrderBy( dv => dv.Order ).ToList();
            ministries.LoadAttributes();
            Guid budgetListGuid = Guid.Parse( dataMap.GetString( "BudgetLine" ) );
            int BudgetDefinedTypeId = new DefinedTypeService( _context ).Get( budgetListGuid ).Id;
            List<DefinedValue> budgets = new DefinedValueService( _context ).Queryable().Where( dv => dv.DefinedTypeId == BudgetDefinedTypeId ).OrderBy( dv => dv.Order ).ToList();
            budgets.LoadAttributes();
            Guid locationListGuid = Guid.Parse( dataMap.GetString( "Locations" ) );
            int LocationDefinedTypeId = new DefinedTypeService( _context ).Get( locationListGuid ).Id;
            List<DefinedValue> locations = new DefinedValueService( _context ).Queryable().Where( dv => dv.DefinedTypeId == LocationDefinedTypeId ).OrderBy( dv => dv.Order ).ToList();
            locations.LoadAttributes();
            Guid drinkListGuid = Guid.Parse( dataMap.GetString( "Drinks" ) );
            int DrinkDefinedTypeId = new DefinedTypeService( _context ).Get( drinkListGuid ).Id;
            List<DefinedValue> drinks = new DefinedValueService( _context ).Queryable().Where( dv => dv.DefinedTypeId == DrinkDefinedTypeId ).OrderBy( dv => dv.Order ).ToList();

            //For Comments
            Guid andrewGuid = Guid.Parse( dataMap.GetString( "Andrew" ) );
            Guid christianGuid = Guid.Parse( dataMap.GetString( "Christian" ) );
            Guid kaelynGuid = Guid.Parse( dataMap.GetString( "Kaelyn" ) );
            PersonAliasService per_svc = new PersonAliasService( _context );
            Person andrew = per_svc.Get( andrewGuid ).Person;
            Person christian = per_svc.Get( christianGuid ).Person;
            Person kaelyn = per_svc.Get( kaelynGuid ).Person;

            int matrixId = Int32.Parse( dataMap.GetString( "MatrixId" ) );

            ContentChannel eventCC = _cc_svc.Get( eventGuid );
            ContentChannel eventDetailsCC = _cc_svc.Get( eventDetailsGuid );
            ContentChannel eventChangesCC = _cc_svc.Get( eventChangesGuid );
            ContentChannel eventDetailsChangesCC = _cc_svc.Get( eventDetailsChangesGuid );
            ContentChannel commentsCC = _cc_svc.Get( commentsGuid );

            AttributeValueService _av_svc = new AttributeValueService( _context );

            List<ContentChannelItem> items = new List<ContentChannelItem>();
            if ( eventid.HasValue )
            {
                items.Add( _cci_svc.Get( eventid.Value ) );
            }
            else
            {
                int skip = dataMap.GetIntegerFromString( "Skip" );
                items = _cci_svc.Queryable().Where( cci => cci.ContentChannelId == eventCC.Id ).OrderByDescending( "Id" ).Skip( skip ).ToList();
            }

            for ( int i = 0; i < items.Count(); i++ )
            {
                var item = items[i];
                item.LoadAttributes();
                //item.ChildItems.Select( ci => ci.ChildContentChannelItem ).LoadAttributes();
                var itemChanges = item.ChildItems.Select( ci => ci.ChildContentChannelItem ).FirstOrDefault( ci => ci.ContentChannelId == eventChangesCC.Id );
                EventRequest req = JsonConvert.DeserializeObject<EventRequest>( item.GetAttributeValue( "RequestJSON" ) );
                var changesJSON = item.GetAttributeValue( "ProposedChangesJSON" );
                EventRequest changes = null;
                if ( !String.IsNullOrEmpty( changesJSON ) )
                {
                    changes = JsonConvert.DeserializeObject<EventRequest>( item.GetAttributeValue( "ProposedChangesJSON" ) );
                    if ( itemChanges == null )
                    {
                        itemChanges = new ContentChannelItem() { ContentChannelId = eventChangesCC.Id, ContentChannelTypeId = eventChangesCC.ContentChannelTypeId };
                        itemChanges.Title = item.Title + " Changes";
                        itemChanges.CreatedByPersonAliasId = item.CreatedByPersonAliasId;
                        itemChanges.ModifiedByPersonAliasId = item.ModifiedByPersonAliasId;
                        itemChanges.CreatedDateTime = item.CreatedDateTime;
                        itemChanges.ModifiedDateTime = item.ModifiedDateTime;
                        itemChanges.LoadAttributes();
                    }
                }

                //Comments
                string commentsJSON = item.GetAttributeValue( "Comments" );
                if ( commentsJSON != "[]" )
                {
                    List<Comment> comments = JsonConvert.DeserializeObject<List<Comment>>( commentsJSON );
                    var existingComments = item.ChildItems.Where( ci => ci.ChildContentChannelItem.ContentChannelId == commentsCC.Id ).Select( ci => ci.ChildContentChannelItem ).ToList();
                    for ( int k = 0; k < comments.Count(); k++ )
                    {
                        var exists = existingComments.FirstOrDefault( c => c.Content == comments[k].Message );
                        if ( exists == null )
                        {
                            ContentChannelItem comment = new ContentChannelItem() { ContentChannelId = commentsCC.Id, ContentChannelTypeId = commentsCC.ContentChannelTypeId };
                            comment.Title = "Comment From " + comments[k].CreatedBy;
                            comment.Content = comments[k].Message;
                            comment.CreatedDateTime = comments[k].CreatedOn;
                            comment.ModifiedDateTime = comments[k].CreatedOn;
                            if ( comments[k].CreatedBy == item.CreatedByPersonAlias.Person.FullName )
                            {
                                comment.CreatedByPersonAliasId = item.CreatedByPersonAliasId;
                                comment.ModifiedByPersonAliasId = item.CreatedByPersonAliasId;
                            }
                            else
                            {
                                if ( comments[k].CreatedBy == andrew.FullName )
                                {
                                    comment.CreatedByPersonAliasId = andrew.PrimaryAliasId;
                                    comment.ModifiedByPersonAliasId = andrew.PrimaryAliasId;
                                }
                                else if ( comments[k].CreatedBy == christian.FullName )
                                {
                                    comment.CreatedByPersonAliasId = christian.PrimaryAliasId;
                                    comment.ModifiedByPersonAliasId = christian.PrimaryAliasId;
                                }
                                else if ( comments[k].CreatedBy == kaelyn.FullName )
                                {
                                    comment.CreatedByPersonAliasId = kaelyn.PrimaryAliasId;
                                    comment.ModifiedByPersonAliasId = kaelyn.PrimaryAliasId;
                                }
                            }
                            _cci_svc.Add( comment );
                            _context.SaveChanges();

                            //Add Child Item Comment to Event Request
                            var comment_exists = _ccia_svc.Queryable().Where( ccia => ccia.ContentChannelItemId == item.Id && ccia.ChildContentChannelItemId == comment.Id );
                            if ( !comment_exists.Any() )
                            {
                                var order = _ccia_svc.Queryable().AsNoTracking()
                                    .Where( a => a.ContentChannelItemId == item.Id )
                                    .Select( a => ( int? ) a.Order )
                                    .DefaultIfEmpty()
                                    .Max();
                                var assoc = new ContentChannelItemAssociation();
                                assoc.ContentChannelItemId = item.Id;
                                assoc.ChildContentChannelItemId = comment.Id;
                                assoc.Order = order.HasValue ? order.Value + 1 : 0;
                                _ccia_svc.Add( assoc );
                            }
                        }
                    }
                }

                //Set Attributes on Item
                item = SetRequestAttributes( item, req, ministries );
                //Set Attributes on Proposed Changes Item
                if ( itemChanges != null )
                {
                    itemChanges = SetRequestAttributes( itemChanges, changes, ministries );
                }
                //Event Details
                for ( int k = 0; k < req.Events.Count(); k++ )
                {
                    ContentChannelItem detail = new ContentChannelItem() { ContentChannelId = eventDetailsCC.Id, ContentChannelTypeId = eventDetailsCC.ContentChannelTypeId };
                    if ( item.ChildItems.Where( ci => ci.ChildContentChannelItem.ContentChannelId == eventDetailsCC.Id ).Count() > 0 )
                    {
                        if ( !String.IsNullOrEmpty( req.Events[k].EventDate ) )
                        {
                            var temp = item.ChildItems.Where( ci => ci.ChildContentChannelItem.ContentChannelId == eventDetailsCC.Id ).ToList().FirstOrDefault( c =>
                            {
                                if ( c.ChildContentChannelItem != null )
                                {
                                    var eDate = _av_svc.Queryable().FirstOrDefault( av => av.AttributeId == eventDateAttrId.Value && av.EntityId == c.ChildContentChannelItemId );
                                    if ( eDate.Value == req.Events[k].EventDate )
                                    {
                                        return true;
                                    }
                                }
                                return false;
                            } );
                            if ( temp != null )
                            {
                                detail = temp.ChildContentChannelItem;
                            }
                        }
                        else
                        {
                            detail = item.ChildItems.Where( ci => ci.ChildContentChannelItem.ContentChannelId == eventDetailsCC.Id ).First().ChildContentChannelItem;
                        }
                    }
                    ContentChannelItem detailChanges = null;
                    if ( !String.IsNullOrEmpty( changesJSON ) )
                    {
                        if ( detail.ChildItems.Count() > 0 )
                        {
                            detailChanges = detail.ChildItems.Select( ci => ci.ChildContentChannelItem ).First();
                        }
                        else
                        {
                            detailChanges = new ContentChannelItem() { ContentChannelId = eventDetailsChangesCC.Id, ContentChannelTypeId = eventDetailsChangesCC.ContentChannelTypeId };
                        }
                        detailChanges.LoadAttributes();
                    }
                    detail.LoadAttributes();
                    detail = SetDetailAttributes( detail, item, k, req, locations, budgets, drinks );
                    if ( detailChanges != null )
                    {
                        detailChanges = SetDetailAttributes( detailChanges, itemChanges, k, changes, locations, budgets, drinks );
                    }

                    if ( detail.Id < 1 )
                    {
                        _cci_svc.Add( detail );
                    }
                    if ( detailChanges != null && detailChanges.Id < 1 )
                    {
                        _cci_svc.Add( detailChanges );
                    }
                    _context.SaveChanges();
                    detail.SaveAttributeValues( _context );
                    if ( detailChanges != null )
                    {
                        detailChanges.SaveAttributeValues( _context );
                        //Add Child Item Event Detail Changes to Event Details
                        var chng_exists = _ccia_svc.Queryable().Where( ccia => ccia.ContentChannelItemId == detail.Id && ccia.ChildContentChannelItemId == detailChanges.Id );
                        if ( !chng_exists.Any() )
                        {
                            var order = _ccia_svc.Queryable().AsNoTracking()
                                .Where( a => a.ContentChannelItemId == detail.Id )
                                .Select( a => ( int? ) a.Order )
                                .DefaultIfEmpty()
                                .Max();
                            var assoc = new ContentChannelItemAssociation();
                            assoc.ContentChannelItemId = detail.Id;
                            assoc.ChildContentChannelItemId = detailChanges.Id;
                            assoc.Order = order.HasValue ? order.Value + 1 : 0;
                            _ccia_svc.Add( assoc );
                        }
                    }

                    //Add Child Item Event Detail to Event Request
                    var exists = _ccia_svc.Queryable().Where( ccia => ccia.ContentChannelItemId == item.Id && ccia.ChildContentChannelItemId == detail.Id );
                    if ( !exists.Any() )
                    {
                        var order = _ccia_svc.Queryable().AsNoTracking()
                            .Where( a => a.ContentChannelItemId == item.Id )
                            .Select( a => ( int? ) a.Order )
                            .DefaultIfEmpty()
                            .Max();
                        var assoc = new ContentChannelItemAssociation();
                        assoc.ContentChannelItemId = item.Id;
                        assoc.ChildContentChannelItemId = detail.Id;
                        assoc.Order = order.HasValue ? order.Value + 1 : 0;
                        _ccia_svc.Add( assoc );
                    }
                    _context.SaveChanges();
                }

                if ( itemChanges != null && itemChanges.Id < 1 )
                {
                    _cci_svc.Add( itemChanges );
                    _context.SaveChanges();
                    //Add Child Item Event Changes to Event Request
                    var assoc_exists = _ccia_svc.Queryable().Where( ccia => ccia.ContentChannelItemId == item.Id && ccia.ChildContentChannelItemId == itemChanges.Id );
                    if ( !assoc_exists.Any() )
                    {
                        var order = _ccia_svc.Queryable().AsNoTracking()
                            .Where( a => a.ContentChannelItemId == item.Id )
                            .Select( a => ( int? ) a.Order )
                            .DefaultIfEmpty()
                            .Max();
                        var assoc = new ContentChannelItemAssociation();
                        assoc.ContentChannelItemId = item.Id;
                        assoc.ChildContentChannelItemId = itemChanges.Id;
                        assoc.Order = order.HasValue ? order.Value + 1 : 0;
                        _ccia_svc.Add( assoc );
                    }
                }
                _context.SaveChanges();
                item.SaveAttributeValues( _context );
                if ( itemChanges != null )
                {
                    itemChanges.SaveAttributeValues( _context );
                }
            }
        }

        private ContentChannelItem SetRequestAttributes( ContentChannelItem item, EventRequest req, List<DefinedValue> ministries )
        {
            //Set Base Event Attributes
            UpdateAttribute( item, "NeedsSpace", req.needsSpace.ToString() );
            UpdateAttribute( item, "NeedsOnline", req.needsOnline.ToString() );
            UpdateAttribute( item, "NeedsPublicity", req.needsPub.ToString() );
            UpdateAttribute( item, "NeedsRegistration", req.needsReg.ToString() );
            UpdateAttribute( item, "NeedsChildCare", req.needsChildCare.ToString() );
            UpdateAttribute( item, "NeedsCatering", req.needsCatering.ToString() );
            UpdateAttribute( item, "NeedsOpsAccommodations", req.needsAccom.ToString() );
            UpdateAttribute( item, "IsSame", req.IsSame.ToString() );
            string EventDates = item.GetAttributeValue( "EventDates" );
            if ( String.IsNullOrEmpty( EventDates ) )
            {
                //Set Event Dates on proposed changes item
                UpdateAttribute( item, "EventDates", String.Join( ",", req.EventDates ) );
            }

            var ministry = ministries.FirstOrDefault( m => m.Id.ToString() == req.Ministry );
            if ( ministry != null )
            {
                UpdateAttribute( item, "Ministry", ministry.Guid.ToString() );
            }
            UpdateAttribute( item, "Contact", req.Contact );
            UpdateAttribute( item, "Notes", req.Notes );

            //Publicity
            UpdateAttribute( item, "WhyAttend", req.WhyAttendSixtyFive );
            UpdateAttribute( item, "TargetAudience", req.TargetAudience.Split( ' ' )[0] );
            UpdateAttribute( item, "EventisSticky", req.EventIsSticky.ToString() );
            UpdateAttribute( item, "PublicityStartDate", req.PublicityStartDate.ToString() );
            UpdateAttribute( item, "PublicityEndDate", req.PublicityEndDate.ToString() );
            if ( req.PublicityStrategies != null )
            {
                List<string> pubStrategies = new List<string>();
                if ( req.PublicityStrategies.Contains( "Social Media/Google Ads" ) )
                {
                    pubStrategies.Add( "Ads" );
                }
                if ( req.PublicityStrategies.Contains( "Mobile Worship Folder" ) )
                {
                    pubStrategies.Add( "MWF" );
                }
                if ( req.PublicityStrategies.Contains( "Announcement" ) )
                {
                    pubStrategies.Add( "Announcement" );
                }
                UpdateAttribute( item, "PublicityStrategies", pubStrategies.Count() > 0 ? String.Join( ",", pubStrategies ) : "" );
            }

            //Web Cal
            UpdateAttribute( item, "NeedsWebCalendar", req.Events[0].ShowOnCalendar.ToString() );
            UpdateAttribute( item, "WebCalendarDescription", req.Events[0].PublicityBlurb );

            //Production
            if ( req.Events[0].TechNeeds != null && ( req.Events[0].TechNeeds.Contains( "Worship Team" ) || req.Events[0].TechNeeds.Contains( "Stage Set-Up" ) || req.Events[0].TechNeeds.Contains( "Basic Live Stream ($)" ) || req.Events[0].TechNeeds.Contains( "Advanced Live Stream ($)" ) || req.Events[0].TechNeeds.Contains( "Special Lighting" ) ) )
            {
                UpdateAttribute( item, "NeedsProductionAccommodations", "true" );
                UpdateAttribute( item, "ProductionTech", req.Events[0].TechNeeds != null ? String.Join( ",", req.Events[0].TechNeeds ) : "" );
                UpdateAttribute( item, "ProductionSetup", req.Events[0].SetUp );
            }
            else
            {
                UpdateAttribute( item, "NeedsProductionAccommodations", "false" );
            }

            return item;
        }

        private ContentChannelItem SetDetailAttributes( ContentChannelItem detail, ContentChannelItem item, int k, EventRequest req, List<DefinedValue> locations, List<DefinedValue> budgets, List<DefinedValue> drinks )
        {
            UpdateAttribute( detail, "EventDate", req.Events[k].EventDate );
            detail.Title = item.Title + ( req.Events[k].EventDate != "" ? ": " + req.Events[k].EventDate.ToString() : "" );
            detail.CreatedByPersonAliasId = item.CreatedByPersonAliasId;
            detail.CreatedDateTime = item.CreatedDateTime;
            detail.ModifiedByPersonAliasId = item.ModifiedByPersonAliasId;
            detail.ModifiedDateTime = item.ModifiedDateTime;
            if ( req.IsValid )
            {
                UpdateAttribute( detail, "EventIsValid", "True" );
            }
            UpdateAttribute( detail, "StartTime", !String.IsNullOrEmpty( req.Events[k].StartTime ) && !req.Events[k].StartTime.Contains( "null" ) ? DateTime.Parse( req.Events[k].StartTime ).ToString( "HH:mm:ss" ) : "" );
            UpdateAttribute( detail, "EndTime", !String.IsNullOrEmpty( req.Events[k].EndTime ) && !req.Events[k].EndTime.Contains( "null" ) ? DateTime.Parse( req.Events[k].EndTime ).ToString( "HH:mm:ss" ) : "" );
            UpdateAttribute( detail, "StartBuffer", req.Events[k].MinsStartBuffer.ToString() );
            UpdateAttribute( detail, "EndBuffer", req.Events[k].MinsEndBuffer.ToString() );

            //Space Info
            UpdateAttribute( detail, "ExpectedAttendance", req.Events[k].ExpectedAttendance.ToString() );
            List<DefinedValue> rooms = new List<DefinedValue>();
            if ( req.Events[k].Rooms != null )
            {
                rooms = locations.Where( l => req.Events[k].Rooms.Contains( l.Id.ToString() ) ).ToList();
                if ( req.Events[k].Rooms != null && req.Events[k].Rooms.Count() > 0 )
                {
                    if ( rooms.Any() )
                    {
                        UpdateAttribute( detail, "Rooms", String.Join( ",", rooms.Select( r => r.Guid.ToString() ) ) );
                    }
                }
            }
            UpdateAttribute( detail, "InfrastructureSpace", req.Events[k].InfrastructureSpace );
            //Room Set Up
            //AttributeMatrix matrix = new AttributeMatrix() { AttributeMatrixTemplateId = matrixId };
            //AttributeMatrixService ams = new AttributeMatrixService( _context );
            //ams.Add( matrix );
            //_context.SaveChanges();

            List<TableSetUp> setUp = new List<TableSetUp>();
            for ( int j = 0; j < rooms.Count(); j++ )
            {
                if ( req.Events[k].NumTablesRound.HasValue )
                {
                    TableSetUp roundSetUp = new TableSetUp() { Room = rooms[j].Guid.ToString(), TypeofTable = "Round" };
                    roundSetUp.NumberofTables = req.Events[k].NumTablesRound.HasValue ? req.Events[k].NumTablesRound.Value : 0;
                    roundSetUp.NumberofChairs = req.Events[k].NumChairsRound.HasValue ? req.Events[k].NumChairsRound.Value : 0;
                    setUp.Add( roundSetUp );
                }
                if ( req.Events[k].NumTablesRect.HasValue )
                {
                    TableSetUp rectSetUp = new TableSetUp() { Room = rooms[j].Guid.ToString(), TypeofTable = "Rectangular" };
                    rectSetUp.NumberofTables = req.Events[k].NumTablesRect.HasValue ? req.Events[k].NumTablesRect.Value : 0;
                    rectSetUp.NumberofChairs = req.Events[k].NumChairsRect.HasValue ? req.Events[k].NumChairsRect.Value : 0;
                    setUp.Add( rectSetUp );
                }
            }
            UpdateAttribute( detail, "RoomSetUp", JsonConvert.SerializeObject( setUp ) );

            //For Matrix - Not yet available
            //for ( int j = 0; j < rooms.Count(); j++ )
            //{
            //    if ( req.Events[k].TableType.Contains( "Round" ) )
            //    {
            //        AttributeMatrixItem matrixItem = new AttributeMatrixItem();
            //        matrixItem.AttributeMatrix = matrix;
            //        matrixItem.LoadAttributes();

            //        matrixItem.SetAttributeValue( "Room", rooms[j].Guid.ToString() );
            //        matrixItem.SetAttributeValue( "TypeofTable", "Round" );
            //        matrixItem.SetAttributeValue( "NumberofTables", req.Events[k].NumTablesRound.ToString() );
            //        matrixItem.SetAttributeValue( "NumberofChairs", req.Events[k].NumChairsRound.ToString() );

            //        AttributeMatrixItemService amis = new AttributeMatrixItemService( _context );
            //        amis.Add( matrixItem );
            //        _context.SaveChanges();
            //        matrixItem.SaveAttributeValues();

            //    }
            //    if ( req.Events[k].TableType.Contains( "Rectangular" ) )
            //    {
            //        AttributeMatrixItem matrixItem = new AttributeMatrixItem();
            //        matrixItem.AttributeMatrix = matrix;
            //        matrixItem.LoadAttributes();

            //        matrixItem.SetAttributeValue( "Room", rooms[j].Guid.ToString() );
            //        matrixItem.SetAttributeValue( "TypeofTable", "Rectangular" );
            //        matrixItem.SetAttributeValue( "NumberofTables", req.Events[k].NumTablesRect.ToString() );
            //        matrixItem.SetAttributeValue( "NumberofChairs", req.Events[k].NumChairsRect.ToString() );

            //        AttributeMatrixItemService amis = new AttributeMatrixItemService( _context );
            //        amis.Add( matrixItem );
            //        _context.SaveChanges();
            //        matrixItem.SaveAttributeValues();
            //    }
            //}

            //UpdateAttribute( detail, "RoomSetup", matrix.Guid.ToString() );

            //Online
            UpdateAttribute( detail, "EventURL", req.Events[k].EventURL );
            UpdateAttribute( detail, "Password", req.Events[k].ZoomPassword );

            //Registration
            UpdateAttribute( detail, "RegistrationStartDate", req.Events[k].RegistrationDate.HasValue ? req.Events[k].RegistrationDate.Value.ToString( "yyyy-MM-dd HH:mm:ss" ) : "" );
            UpdateAttribute( detail, "RegistrationEndDate", req.Events[k].RegistrationEndDate.HasValue ? req.Events[k].RegistrationEndDate.Value.ToString( "yyyy-MM-dd HH:mm:ss" ) : "" );
            UpdateAttribute( detail, "RegistrationEndTime", !String.IsNullOrEmpty( req.Events[k].RegistrationEndTime ) && !req.Events[k].RegistrationEndTime.Contains( "null" ) ? DateTime.Parse( req.Events[k].RegistrationEndTime ).ToString( "HH:mm:ss" ) : "" );
            UpdateAttribute( detail, "RegistrationFeeType", req.Events[k].FeeType != null ? String.Join( ",", req.Events[k].FeeType ) : "" );
            var regBudget = budgets.FirstOrDefault( b => b.Id.ToString() == req.Events[k].FeeBudgetLine );
            if ( regBudget != null )
            {
                UpdateAttribute( detail, "RegistrationFeeBudgetLine", regBudget.Guid.ToString() );
            }
            UpdateAttribute( detail, "IndividualRegistrationFee", req.Events[k].Fee );
            UpdateAttribute( detail, "CoupleRegistrationFee", req.Events[k].CoupleFee );
            UpdateAttribute( detail, "OnlineRegistrationFee", req.Events[k].OnlineFee );
            UpdateAttribute( detail, "RegistrationConfirmationEmailSender", req.Events[k].Sender );
            UpdateAttribute( detail, "RegistrationConfirmationEmailFromAddress", req.Events[k].SenderEmail );
            UpdateAttribute( detail, "RegistrationConfirmationEmailAdditionalDetails", req.Events[k].AdditionalDetails );
            UpdateAttribute( detail, "NeedsReminderEmail", req.Events[k].NeedsReminderEmail.ToString() );
            UpdateAttribute( detail, "RegistrationReminderEmailAdditionalDetails", req.Events[k].ReminderAdditionalDetails );
            UpdateAttribute( detail, "NeedsCheckin", req.Events[k].Checkin.ToString() );
            UpdateAttribute( detail, "NeedsDatabaseSupportTeam", req.Events[k].SupportTeam.ToString() );

            //Catering
            UpdateAttribute( detail, "PreferredVendor", req.Events[k].Vendor );
            UpdateAttribute( detail, "PreferredMenu", req.Events[k].Menu );
            var budget = budgets.FirstOrDefault( b => b.Id.ToString() == req.Events[k].BudgetLine );
            if ( budget != null )
            {
                UpdateAttribute( detail, "FoodBudgetLine", budget.Guid.ToString() );
            }
            UpdateAttribute( detail, "NeedsDelivery", req.Events[k].FoodDelivery.ToString() );
            UpdateAttribute( detail, "FoodTime", !String.IsNullOrEmpty( req.Events[k].FoodTime ) && !req.Events[k].FoodTime.Contains( "null" ) ? DateTime.Parse( req.Events[k].FoodTime ).ToString( "HH:mm:ss" ) : "" );
            UpdateAttribute( detail, "FoodSetupLocation", req.Events[k].FoodDropOff );
            if ( req.Events[k].Drinks != null )
            {
                var selectedDrinks = drinks.Where( d => req.Events[k].Drinks.Contains( d.Value ) ).ToList();
                if ( req.Events[k].Drinks.Contains( "Water" ) )
                {
                    var water = drinks.FirstOrDefault( d => d.Value == "Bottled Water" );
                    selectedDrinks.Add( water );
                }
                UpdateAttribute( detail, "Drinks", String.Join( ",", selectedDrinks.Select( d => d.Guid.ToString() ) ) );
            }
            //UpdateAttribute( detail, "Drinks", req.Events[k].Drinks != null ? String.Join( ",", req.Events[k].Drinks ) : "" );
            UpdateAttribute( detail, "DrinkTime", !String.IsNullOrEmpty( req.Events[k].DrinkTime ) && !req.Events[k].DrinkTime.Contains( "null" ) ? DateTime.Parse( req.Events[k].DrinkTime ).ToString( "HH:mm:ss" ) : "" );
            UpdateAttribute( detail, "SetupFoodandDrinkTogether", ( req.Events[k].FoodDropOff == req.Events[k].DrinkDropOff ).ToString() );
            UpdateAttribute( detail, "DrinkSetupLocation", req.Events[k].DrinkDropOff );

            UpdateAttribute( detail, "ChildcareVendor", req.Events[k].CCVendor );
            budget = budgets.FirstOrDefault( b => b.Id.ToString() == req.Events[k].CCBudgetLine );
            if ( budget != null )
            {
                UpdateAttribute( detail, "ChildcareCateringBudgetLine", budget.Guid.ToString() );
            }
            UpdateAttribute( detail, "ChildcarePreferredMenu", req.Events[k].CCMenu );
            UpdateAttribute( detail, "ChildcareFoodTime", !String.IsNullOrEmpty( req.Events[k].CCFoodTime ) && !req.Events[k].CCFoodTime.Contains( "null" ) ? DateTime.Parse( req.Events[k].CCFoodTime ).ToString( "HH:mm:ss" ) : "" );

            //Childcare
            UpdateAttribute( detail, "ChildcareStartTime", !String.IsNullOrEmpty( req.Events[k].CCStartTime ) && !req.Events[k].CCStartTime.Contains( "null" ) ? DateTime.Parse( req.Events[k].CCStartTime ).ToString( "HH:mm:ss" ) : "" );
            UpdateAttribute( detail, "ChildcareEndTime", !String.IsNullOrEmpty( req.Events[k].CCEndTime ) && !req.Events[k].CCEndTime.Contains( "null" ) ? DateTime.Parse( req.Events[k].CCEndTime ).ToString( "HH:mm:ss" ) : "" );
            UpdateAttribute( detail, "ChildcareOptions", req.Events[k].ChildCareOptions != null ? String.Join( ",", req.Events[k].ChildCareOptions ) : "" );
            UpdateAttribute( detail, "EstimatedNumberofKids", req.Events[k].EstimatedKids.ToString() );

            //Special Accomm
            if ( !item.GetAttributeValue( "NeedsProductionAccommodations" ).AsBoolean() )
            {
                UpdateAttribute( detail, "RoomTech", req.Events[k].TechNeeds != null ? String.Join( ",", req.Events[k].TechNeeds ) : "" );
            }
            UpdateAttribute( detail, "TechNeeds", req.Events[k].TechDescription );
            UpdateAttribute( detail, "NeedsDoorsUnlocked", req.Events[k].NeedsDoorsUnlocked.ToString() );
            if ( req.Events[k].Doors != null && req.Events[k].Doors.Count() > 0 )
            {
                var doors = locations.Where( l => req.Events[k].Doors.Contains( l.Id.ToString() ) );
                if ( doors.Any() )
                {
                    UpdateAttribute( detail, "Doors", String.Join( ",", doors.Select( r => r.Guid.ToString() ) ) );
                }
            }
            UpdateAttribute( detail, "Setup", req.Events[k].SetUp );
            UpdateAttribute( detail, "NeedsMedical", req.Events[k].NeedsMedical.ToString() );
            UpdateAttribute( detail, "NeedsSecurity", req.Events[k].NeedsSecurity.ToString() );

            return detail;
        }

        private void UpdateAttribute( ContentChannelItem item, string attr, string value )
        {
            try
            {
                item.SetAttributeValue( attr, value );
            }
            catch ( Exception e )
            {
                Console.WriteLine( e.Message );
            }
        }

        #region Classes

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
            public SetupImage SetUpImage { get; set; }
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

        public class SetupImage
        {
            public string name { get; set; }
            public string type { get; set; }
            public string data { get; set; }
        }

        public class TableSetUp
        {
            public string Room { get; set; }
            public string TypeofTable { get; set; }
            public int NumberofTables { get; set; }
            public int NumberofChairs { get; set; }
        }
        #endregion
    }
}
