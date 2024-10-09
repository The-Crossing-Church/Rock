using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Rest;
using Ical.Net;
using Ical.Net.DataTypes;
using Rock;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;
using Rock.Address;
using System.IO;
using System.Net;

namespace org.crossingchurch.OurRock.Rest.Controllers
{
    public partial class EventFormCalendarController : ApiControllerBase
    {
        /// <summary>
        /// Endpoint to add the main buiding calendar to an external app
        /// </summary>
        /// <param name="token">The token to authenticate the user</param>
        /// <returns></returns>
        [HttpGet]
        [System.Web.Http.Route( "api/EventForm/GetMainBuilding" )]
        public HttpStatusCode GetMainBuilding( string token )
        {
            var x = System.Web.HttpContext.Current;
            if ( String.IsNullOrWhiteSpace( token ) )
            {
                throw new UnauthorizedAccessException( "403 Unauthorized" );
            }
            try
            {
                int userid;
                if ( ValidateToken( token, out userid ) )
                {
                    List<string> mainBuildingLocations = GlobalAttributesCache.Get().GetValue( "EventFormMainBuildingLocationTypes" ).Split( ',' ).Select( value => value.Trim() ).ToList();
                    x.Response.Clear();
                    x.Response.ClearHeaders();
                    x.Response.ClearContent();
                    x.Response.BufferOutput = true;
                    x.Response.ContentType = "text/calendar";
                    x.Response.Write( GenerateData( mainBuildingLocations, "Main Building" ) );
                    return HttpStatusCode.OK;
                }
                else
                {
                    throw new UnauthorizedAccessException( "403 Unauthorized" );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                x.Response.Write( $"API Error: {ex.Message}\r\n{ex.StackTrace}" );
                return HttpStatusCode.BadRequest;
            }
        }

        /// <summary>
        /// Endpoint to add the student center calendar to an external app
        /// </summary>
        /// <param name="token">The token to authenticate the user</param>
        /// <returns></returns>
        [HttpGet]
        [System.Web.Http.Route( "api/EventForm/GetStudentCenter" )]
        public HttpStatusCode GetStudentCenter( string token )
        {
            var x = System.Web.HttpContext.Current;
            if ( String.IsNullOrWhiteSpace( token ) )
            {
                throw new UnauthorizedAccessException( "403 Unauthorized" );
            }
            try
            {
                int userid;
                if ( ValidateToken( token, out userid ) )
                {
                    List<string> studentCenterLocations = GlobalAttributesCache.Get().GetValue( "EventFormStudentCenterLocationTypes" ).Split( ',' ).Select( value => value.Trim() ).ToList();
                    x.Response.Clear();
                    x.Response.ClearHeaders();
                    x.Response.ClearContent();
                    x.Response.BufferOutput = true;
                    x.Response.ContentType = "text/calendar";
                    x.Response.Write( GenerateData( studentCenterLocations, "Student Center" ) );
                    return HttpStatusCode.OK;
                }
                else
                {
                    throw new UnauthorizedAccessException( "403 Unauthorized" );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                x.Response.Write( $"API Error: {ex.Message}\r\n{ex.StackTrace}" );
                return HttpStatusCode.BadRequest;
            }
        }

        /// <summary>
        /// Endpoint to add the student center calendar to an external app
        /// </summary>
        /// <param name="token">The token to authenticate the user</param>
        /// <returns></returns>
        [HttpGet]
        [System.Web.Http.Route( "api/EventForm/GetGym" )]
        public HttpStatusCode GetGym( string token )
        {
            var x = System.Web.HttpContext.Current;
            if ( String.IsNullOrWhiteSpace( token ) )
            {
                throw new UnauthorizedAccessException( "403 Unauthorized" );
            }
            try
            {
                int userid;
                if ( ValidateToken( token, out userid ) )
                {
                    List<string> studentCenterLocations = GlobalAttributesCache.Get().GetValue( "EventFormGymLocationTypes" ).Split( ',' ).Select( value => value.Trim() ).ToList();
                    x.Response.Clear();
                    x.Response.ClearHeaders();
                    x.Response.ClearContent();
                    x.Response.BufferOutput = true;
                    x.Response.ContentType = "text/calendar";
                    x.Response.Write( GenerateData( studentCenterLocations, "Gym" ) );
                    return HttpStatusCode.OK;
                }
                else
                {
                    throw new UnauthorizedAccessException( "403 Unauthorized" );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                x.Response.Write( $"API Error: {ex.Message}\r\n{ex.StackTrace}" );
                return HttpStatusCode.BadRequest;
            }
        }

        /// <summary>
        /// Endpoint to add the outdoor calendar to an external app
        /// </summary>
        /// <param name="token">The token to authenticate the user</param>
        /// <returns></returns>
        [HttpGet]
        [System.Web.Http.Route( "api/EventForm/GetOutdoor" )]
        public HttpStatusCode GetOutdoor( string token )
        {
            var x = System.Web.HttpContext.Current;
            if ( String.IsNullOrWhiteSpace( token ) )
            {
                throw new UnauthorizedAccessException( "403 Unauthorized" );
            }
            try
            {
                int userid;
                if ( ValidateToken( token, out userid ) )
                {
                    List<string> outdoorLocations = GlobalAttributesCache.Get().GetValue( "EventFormOutdoorLocationTypes" ).Split( ',' ).Select( value => value.Trim() ).ToList();
                    x.Response.Clear();
                    x.Response.ClearHeaders();
                    x.Response.ClearContent();
                    x.Response.BufferOutput = true;
                    x.Response.ContentType = "text/calendar";
                    x.Response.Write( GenerateData( outdoorLocations, "Outdoor Spaces" ) );
                    return HttpStatusCode.OK;
                }
                else
                {
                    throw new UnauthorizedAccessException( "403 Unauthorized" );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                x.Response.Write( $"API Error: {ex.Message}\r\n{ex.StackTrace}" );
                return HttpStatusCode.BadRequest;
            }
        }

        private string GenerateData( List<string> locations, string calName, bool filterEvents = false, int userid = 0 )
        {
            Guid? definedTypeGuid = GlobalAttributesCache.Get().GetValue( "EventFormLocations" ).AsGuidOrNull();
            Guid? eventCCTGuid = GlobalAttributesCache.Get().GetValue( "EventRequestContentChannelType" ).AsGuidOrNull();
            Guid? detailsCCTGuid = GlobalAttributesCache.Get().GetValue( "EventRequestDetailsContentChannelType" ).AsGuidOrNull();
            Guid? eventCCGuid = GlobalAttributesCache.Get().GetValue( "EventRequestContentChannel" ).AsGuidOrNull();
            Guid? detailsCCGuid = GlobalAttributesCache.Get().GetValue( "EventRequestDetailsContentChannel" ).AsGuidOrNull();
            RockContext context = new RockContext();
            AttributeValueService av_svc = new AttributeValueService( context );
            AttributeService attr_svc = new AttributeService( context );
            DefinedTypeService dt_svc = new DefinedTypeService( context );
            DefinedValueService dv_svc = new DefinedValueService( context );
            ContentChannelTypeService cct_svc = new ContentChannelTypeService( context );
            ContentChannelService cc_svc = new ContentChannelService( context );
            ContentChannelItemService cci_svc = new ContentChannelItemService( context );
            ContentChannelItemAssociationService ccia_svc = new ContentChannelItemAssociationService( context );
            int definedTypeId = dt_svc.Get( definedTypeGuid.Value ).Id;
            int eventCCTId = cct_svc.Get( eventCCTGuid.Value ).Id;
            int detailsCCTId = cct_svc.Get( detailsCCTGuid.Value ).Id;
            int eventCCId = cc_svc.Get( eventCCGuid.Value ).Id;
            int detailsCCId = cc_svc.Get( detailsCCGuid.Value ).Id;

            IQueryable<ContentChannelItem> events = cci_svc.Queryable().Where( cci => cci.ContentChannelId == eventCCId );
            var statusAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierValue == eventCCTId.ToString() && a.Key == "RequestStatus" );
            IQueryable<AttributeValue> eventStatuses = av_svc.Queryable().Where( av => av.AttributeId == statusAttr.Id && av.Value != "Draft" && av.Value != "Submitted" && av.Value != "Denied" && !av.Value.Contains( "Cancelled" ) ).Join( events,
                es => es.EntityId,
                e => e.Id,
                ( es, e ) => es
            );

            if ( filterEvents )
            {
                Person p = new PersonService( context ).Get( userid );
                //Filter to User's events and events that have been shared with them
                //events = events.Where( cci =>
                //{
                //    if ( cci.CreatedByPersonAliasId == p.PrimaryAliasId )
                //    {
                //        return true;
                //    }

                //} );
            }

            DateTime today = RockDateTime.Now.SundayDate();
            DateTime twoWeeksBack = today.AddDays( -14 );
            DateTime sixMonthsOut = today.AddMonths( 6 );
            var eventDatesAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierValue == eventCCTId.ToString() && a.Key == "EventDates" );
            List<AttributeValue> eventDates = av_svc.Queryable().Where( av => av.AttributeId == eventDatesAttr.Id ).Join( eventStatuses,
                ed => ed.EntityId,
                es => es.EntityId,
                ( ed, es ) => ed
            ).ToList().Where( av =>
                {
                    List<DateTime> dates = av.Value.Split( ',' ).Select( v => DateTime.Parse( v.Trim() ) ).ToList();
                    var hasDateInRange = false;
                    for ( int i = 0; i < dates.Count(); i++ )
                    {
                        if ( twoWeeksBack <= dates[i] && dates[i] <= sixMonthsOut )
                        {
                            hasDateInRange = true;
                        }
                    }
                    return hasDateInRange;
                }
            ).ToList();

            List<ContentChannelItem> eventDetails = events.ToList().Join( eventDates,
                    e => e.Id,
                    ed => ed.EntityId,
                    ( e, ed ) => e
                ).SelectMany( e => e.ChildItems.Select( ci => ci.ChildContentChannelItem ) ).Where( e => e.ContentChannelId == detailsCCId ).ToList();
            List<ContentChannelItem> eventList = events.ToList().Where( e => eventDetails.Select( ed => ed.ParentItems.First().ContentChannelItemId ).Contains( e.Id ) ).ToList();
            eventList.LoadAttributes();

            var roomsAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierValue == detailsCCTId.ToString() && a.Key == "Rooms" );
            List<DefinedValue> roomList = dv_svc.Queryable().Where( dv => dv.DefinedTypeId == definedTypeId ).ToList();
            roomList.LoadAttributes();
            roomList = roomList.Where( dv => locations.Contains( dv.GetAttributeValue( "Type" ) ) ).ToList();
            List<string> locationRoomGuids = roomList.Select( dv => dv.Guid.ToString() ).ToList();
            eventDetails = av_svc.Queryable().Where( av => av.AttributeId == roomsAttr.Id ).ToList().Where( av =>
             {
                 List<string> guids = av.Value.Split( ',' ).ToList();
                 if ( guids.Intersect( locationRoomGuids ).Count() > 0 )
                 {
                     return true;
                 }
                 return false;
             } ).Join( eventDetails,
                er => er.EntityId,
                ed => ed.Id,
                ( er, ed ) => ed
            ).ToList();

            Calendar c = new Calendar();
            c.AddProperty( "X-PUBLISHED-TTL", "PT1H" );
            c.AddProperty( "REFRESH-INTERVAL;VALUE=DURATION", "PT1H" );
            c.AddProperty( "X-WR-CALNAME", calName );
            var vtz = VTimeZone.FromLocalTimeZone();
            c.AddTimeZone( vtz );
            var timeZoneId = vtz.TzId;

            for ( var i = 0; i < eventList.Count(); i++ )
            {
                var details = eventDetails.Where( ed => eventList[i].ChildItems.Select( ci => ci.ChildContentChannelItemId ).Contains( ed.Id ) ).ToList();
                details.LoadAttributes();
                List<DateTime> dates = eventList[i].GetAttributeValue( "EventDates" ).Split( ',' ).Select( d => DateTime.Parse( d.Trim() ) ).ToList();
                string contact = eventList[i].GetAttributeValue( "Contact" );
                for ( var h = 0; h < details.Count(); h++ )
                {
                    var roomGuids = details[h].GetAttributeValue( "Rooms" ).Split( ',' ).ToList();
                    string eventLocation = String.Join( ", ", roomList.Where( dv => roomGuids.Contains( dv.Guid.ToString() ) ).Select( dv => dv.Value ) );
                    string startTime = details[h].GetAttributeValue( "StartTime" );
                    string endTime = details[h].GetAttributeValue( "EndTime" );
                    string startBuffer = details[h].GetAttributeValue( "StartBuffer" );
                    string endBuffer = details[h].GetAttributeValue( "EndBuffer" );
                    if ( !String.IsNullOrEmpty( startTime ) && !String.IsNullOrEmpty( endTime ) )
                    {
                        if ( eventList[i].GetAttributeValue( "IsSame" ) == "False" )
                        {
                            DateTime eventDate;
                            //Getting errors about being unable ot parse a datetime, at this time cannot find an event causing the issues but we'll wrap it just to handle that error
                            if ( DateTime.TryParse( details[h].GetAttributeValue( "EventDate" ), out eventDate ) )
                            {
                                CalendarEvent e = GenerateEvent( eventDate, startTime, endTime, startBuffer, endBuffer, eventLocation, eventList[i].Title, contact, timeZoneId );
                                c.Events.Add( e );
                            }
                        }
                        else
                        {
                            for ( var k = 0; k < dates.Count(); k++ )
                            {
                                dates[k] = new DateTime( dates[k].Year, dates[k].Month, dates[k].Day, Int32.Parse( startTime.Split( ':' )[0] ), Int32.Parse( startTime.Split( ':' )[1] ), 0 );
                                CalendarEvent e = GenerateEvent( dates[k], startTime, endTime, startBuffer, endBuffer, eventLocation, eventList[i].Title, contact, timeZoneId );
                                c.Events.Add( e );

                            }
                        }
                    }
                }
            }
            var iCalSerializer = new CalendarSerializer();
            string result = iCalSerializer.SerializeToString( c );
            return result;
        }

        private CalendarEvent GenerateEvent( DateTime eventDate, string startTime, string endTime, string startBuffer, string endBuffer, string location, string summary, string contact, string timeZoneId )
        {
            CalendarEvent e = new CalendarEvent();
            e.Summary = summary;
            eventDate = new DateTime( eventDate.Year, eventDate.Month, eventDate.Day, Int32.Parse( startTime.Split( ':' )[0] ), Int32.Parse( startTime.Split( ':' )[1] ), 0 );
            DateTime eventEnd = new DateTime( eventDate.Year, eventDate.Month, eventDate.Day, Int32.Parse( endTime.Split( ':' )[0] ), Int32.Parse( endTime.Split( ':' )[1] ), 0 );
            e.Start = new CalDateTime( eventDate, timeZoneId );
            e.End = new CalDateTime( eventEnd, timeZoneId );
            if ( !String.IsNullOrEmpty( startBuffer ) )
            {
                int buffer;
                if ( Int32.TryParse( startBuffer, out buffer ) )
                {
                    e.Start = e.Start.AddMinutes( buffer * -1 );
                }
            }
            if ( !String.IsNullOrEmpty( endBuffer ) )
            {
                int buffer;
                if ( Int32.TryParse( endBuffer, out buffer ) )
                {
                    e.End = e.End.AddMinutes( buffer );
                }
            }
            if ( !String.IsNullOrEmpty( startBuffer ) || !String.IsNullOrEmpty( endBuffer ) )
            {
                e.Description = summary + " officially runs " + eventDate.ToString( "hh:mm tt" ) + " to " + eventEnd.ToString( "hh:mm tt" ) + " and is reserved longer for set up and tear down.";
            }
            e.Location = location;
            return e;
        }

        private bool ValidateToken( string token, out int userid )
        {
            RockContext context = new RockContext();
            var secret = Encoding.ASCII.GetBytes( GlobalAttributesCache.Get().GetValue( "EventFormCalendarSecret" ) );
            Guid? grpGuid = GlobalAttributesCache.Get().GetValue( "EventFormGroup" ).AsGuidOrNull();
            Group g = new GroupService( context ).Get( grpGuid.Value );
            if ( g == null )
            {
                throw new Exception( "Unable to retreive group information for validation." );
            }
            if ( token == null )
            {
                userid = -1;
                return false;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tk = tokenHandler.ReadJwtToken( token );
            tokenHandler.ValidateToken( token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey( secret ),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken );

            var jwtToken = ( JwtSecurityToken ) validatedToken;
            var userId = int.Parse( jwtToken.Claims.First( x => x.Type == "id" ).Value );

            var member = new GroupMemberService( new RockContext() ).Queryable().Where( gm => gm.GroupId == g.Id && gm.IsArchived == false && gm.GroupMemberStatus == GroupMemberStatus.Active ).FirstOrDefault( gm => gm.PersonId == userId );

            if ( member != null )
            {
                member.Person.LoadAttributes();
                var validToken = member.Person.GetAttributeValue( "PersonalCalendarToken" );
                if ( validToken != token )
                {
                    throw new UnauthorizedAccessException( "Invalid Token" );
                }
                userid = userId;
                return true;
            }
            throw new UnauthorizedAccessException( "Only staff can view this calendar" );
        }
    }
}
