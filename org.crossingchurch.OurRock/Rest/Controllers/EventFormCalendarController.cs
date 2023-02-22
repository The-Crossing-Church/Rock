using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web.Http;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Rest;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.iCalendar.Serializers;
using Rock;
using System.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;

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
        public IHttpActionResult GetMainBuilding( string token )
        {
            if ( String.IsNullOrWhiteSpace( token ) )
            {
                return BadRequest( "403 Unauthorized" );
            }
            try
            {
                if ( ValidateToken( token ) )
                {
                    List<string> mainBuildingLocations = GlobalAttributesCache.Get().GetValue( "EventFormMainBuildingLocationTypes" ).Split( ',' ).Select( value => value.Trim() ).ToList();

                    return Ok( GenerateData( mainBuildingLocations, "Main Building" ) );
                }
                return BadRequest( "403 Unauthorized" );
            }
            catch ( Exception ex )
            {
                return BadRequest( ex.Message );
            }
        }

        private string GenerateData( List<string> locations, string calName )
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
            var rooms = av_svc.Queryable().Where( av => av.AttributeId == roomsAttr.Id ).ToList().Join( eventDetails,
                er => er.EntityId,
                ed => ed.Id,
                ( er, ed ) => er
            );

            Calendar c = new Calendar();
            c.Name = calName;
            for ( var i = 0; i < eventList.Count(); i++ )
            {
                var details = eventDetails.Where( ed => eventList[i].ChildItems.Select( ci => ci.ChildContentChannelItemId ).Contains( ed.Id ) ).ToList();
                details.LoadAttributes();
                List<DateTime> dates = eventList[i].GetAttributeValue( "EventDates" ).Split( ',' ).Select( d => DateTime.Parse( d.Trim() ) ).ToList();
                for ( var h = 0; h < details.Count(); h++ )
                {
                    for ( var k = 0; k < dates.Count(); k++ )
                    {
                        Event e = new Event();
                        e.Name = eventList[i].Title;
                        string startTime = details[h].GetAttributeValue( "StartTime" );
                        string endTime = details[h].GetAttributeValue( "EndTime" );
                        dates[k] = new DateTime( dates[k].Year, dates[k].Month, dates[k].Day, Int32.Parse( startTime.Split( ':' )[0] ), Int32.Parse( startTime.Split( ':' )[1] ), 0 );
                        e.Start = new CalDateTime( dates[k] );
                        e.End = new CalDateTime( dates[k].Year, dates[k].Month, dates[k].Day, Int32.Parse( endTime.Split( ':' )[0] ), Int32.Parse( endTime.Split( ':' )[1] ), 0 );
                        var roomGuids = details[h].GetAttributeValue( "Rooms" ).Split( ',' ).ToList();
                        e.Location = String.Join( ", ", roomList.Where( dv => roomGuids.Contains( dv.Guid.ToString() ) ).Select( dv => dv.Value ) );
                        c.Events.Add( e );
                    }
                }
            }
            var iCalSerializer = new CalendarSerializer();
            string result = iCalSerializer.SerializeToString( c );
            return result;
        }

        private bool ValidateToken( string token )
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
                return true;
            }
            throw new UnauthorizedAccessException( "Only staff can view this calendar" );
        }
    }
}
