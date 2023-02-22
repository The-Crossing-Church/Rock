using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotLiquid;
using Rock;
using Genesis.QRCodeLib;
using System.IO;
using Rock.Utility;
using Newtonsoft.Json;
using System.Configuration;
using Rock.Data;
using System.Collections.ObjectModel;
using TheArtOfDev.HtmlRenderer;
using System.Collections;
using Rock.Model;
using System.Drawing;
using System.Drawing.Imaging;
using Rock.Lava;
using System.Globalization;
using Rock.Attribute;
using Rock.Web.Cache;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace com_thecrossingchurch.LavaFilters
{
    /// <summary>
    /// Custom startup class used to register custom filters
    /// </summary>
    public class Startup : IRockStartup
    {
        public int StartupOrder => 0;
        /// <summary>
        /// Called when rock applicatoin starts up.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void OnStartup()
        {
            Template.RegisterFilter( typeof( RockFilters ) );
        }
    }

    public class RockFilters
    {
        /// <summary>
        /// Generate QR Code for URL.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="page">The page number.</param>
        /// <param name="query">The query string, = will be added to end before the input.</param>
        /// <returns></returns>
        public static string QRCodeFromURL( object input, string page, string query )
        {
            if ( input == null || page == null || query == null )
            {
                return null;
            }
            QREncoder Encoder = new QREncoder();
            string host = ConfigurationManager.AppSettings["RockBaseUrl"];
            string url = host + "/page/" + page + "?" + query + "=" + input.ToString();
            var qrCode = Encoder.Encode( ErrorCorrection.M, url );
            return JsonConvert.SerializeObject( qrCode );
        }

        /// <summary>
        /// Generate QR Code from Person.
        /// </summary>
        /// <param name="input">The input to turn into a qr.</param>
        /// <returns></returns>
        public static string QRCodeFromString( object input )
        {
            if ( input == null )
            {
                return null;
            }
            QREncoder Encoder = new QREncoder();
            string url = input.ToString();
            var qrCode = Encoder.Encode( ErrorCorrection.M, url );
            return JsonConvert.SerializeObject( qrCode );
        }

        /// <summary>
        /// Generate QR Code from Person.
        /// </summary>
        /// <param name="input">The input to turn into a qr.</param>
        /// <returns></returns>
        public static string QRCodeAsImage( object input, int? size = 3 )
        {
            if ( input == null )
            {
                return null;
            }
            QREncoder Encoder = new QREncoder();
            string url = input.ToString();
            var qrCode = Encoder.Encode( ErrorCorrection.M, url );

            string html = @"
            <style>
                .qr {
                    width: " + size + @"px;
                    height: " + size + @"px;
                }
                .qr-fill {
                    background-color: black;
                }
                table {
                    border-spacing: 0px;
                }
            </style>
            <table>";
            int x = qrCode.GetLength( 0 );
            int y = qrCode.GetLength( 1 );
            for ( int i = 0; i < qrCode.GetLength( 0 ); i++ )
            {
                html += "<tr>";
                for ( int k = 0; k < qrCode.GetLength( 1 ); k++ )
                {
                    if ( qrCode[i, k] )
                    {
                        html += "<td class='qr qr-fill'></td>";
                    }
                    else
                    {
                        html += "<td class='qr'></td>";
                    }
                }
                html += "</tr>";
            }
            html += "</table>";
            Bitmap m_Bitmap = new Bitmap( qrCode.GetLength( 0 ) * size.Value, qrCode.GetLength( 1 ) * size.Value );
            PointF point = new PointF( 0, 0 );
            SizeF maxSize = new System.Drawing.SizeF( 500, 500 );
            TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.Render( Graphics.FromImage( m_Bitmap ), html, point, maxSize );
            string dataUrl = "";
            using ( MemoryStream ms = new MemoryStream() )
            {
                m_Bitmap.Save( ms, ImageFormat.Png );
                byte[] byteArr = ms.ToArray();
                string b64Txt = Convert.ToBase64String( byteArr );
                dataUrl = "data:image/png;base64," + b64Txt;
            }
            Console.WriteLine( "x" );
            return dataUrl;
        }

        /// <summary>
        /// Array Pop Functionality.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static dynamic Pop( object input )
        {
            var type = input.GetType();
            if ( input.GetType() == typeof( string ) )
            {
                List<string> list = input.ToString().Split( ',' ).ToList();
                list.RemoveAt( 0 );
                return string.Join( ",", list );
            }
            else if ( type.FullName.Contains( "Collection" ) )
            {
                Type listType = typeof( List<> ).MakeGenericType( new[] { type } );
                IList list = ( IList ) Activator.CreateInstance( listType );
                list = ( IList ) input;
                list.RemoveAt( 0 );
                return list;
            }
            else
            {
                throw new Exception( "Invalid Input: input must be of type string or Collection" );
            }
        }

        /// <summary>
        /// Gives correct pronoun for list of personids
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static dynamic Pronoun( object input )
        {
            var type = input.GetType();
            if ( input.GetType() == typeof( string ) )
            {
                List<string> list = input.ToString().Split( ',' ).ToList();
                if ( list.Count() > 1 )
                {
                    return new { Subject = "they", Object = "them", Posessive = "their" };
                }
                Person p = new PersonService( new RockContext() ).Get( Int32.Parse( list[0] ) );
                if ( p.Gender == Gender.Female )
                {
                    return new { Subject = "she", Object = "her", Posessive = "her" };
                }
                else if ( p.Gender == Gender.Male )
                {
                    return new { Subject = "he", Object = "him", Posessive = "his" };
                }
                return new { Subject = "they", Object = "them", Posessive = "their" };
            }
            else
            {
                throw new Exception( "Invalid Input: input must be of type string" );
            }
        }

        /// <summary>
        /// Return true if the date is within the provided range, false if outside of range
        /// </summary>
        /// <param name="input">The input date.</param>
        /// <param name="start">The range start date.</param>
        /// <param name="end">The range end date.</param>
        /// <returns></returns>
        public static bool DateIsBetween( object input, object start, object end, string format = null )
        {
            DateTime? target = ParseInputForIsDateBetween( input, "target", format );
            DateTime? rangeStart = ParseInputForIsDateBetween( start, "start", format );
            DateTime? rangeEnd = ParseInputForIsDateBetween( end, "end", format );

            if ( target.HasValue && rangeEnd.HasValue && rangeStart.HasValue )
            {
                if ( DateTime.Compare( target.Value, rangeStart.Value ) >= 0 && DateTime.Compare( target.Value, rangeEnd.Value ) <= 0 )
                {
                    return true;
                }
                return false;
            }
            throw new Exception( "Unable to parse input, start, and end" );
        }

        /// <summary>
        /// This filter parses the user input for the lava filter for the IsDateBetween method. Should not be modified without testing with that filter.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="parameter">Which parameter from lava filter, used to set time of day in some cases.</param>
        /// <param name="format">Optional format string for parsing string to datetime.</param>
        /// <returns></returns>
        private static DateTime ParseInputForIsDateBetween( object input, string parameter, string format = null )
        {
            DateTime result;
            if ( input.GetType() == typeof( string ) )
            {
                if ( format != null )
                {
                    var isValid = DateTime.TryParseExact( input.ToString(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result );
                    if ( !isValid )
                    {
                        throw new Exception( "Unable to parse " + parameter + " input as date with format: \"" + format + "\"." );
                    }
                }
                else
                {
                    var isValid = DateTime.TryParse( input.ToString(), out result );
                    if ( isValid )
                    {
                        if ( parameter == "end" )
                        {
                            result = new DateTime( result.Year, result.Month, result.Day, 23, 59, 59 );
                        }
                        else if ( parameter == "start" )
                        {
                            result = new DateTime( result.Year, result.Month, result.Day, 0, 0, 0 );
                        }
                    }
                    else
                    {
                        throw new Exception( "Unable to parse " + parameter + " input as date." );
                    }
                }
            }
            else if ( input.GetType() == typeof( DateTimeOffset ) )
            {
                var offset = ( DateTimeOffset ) input;
                result = offset.DateTime;
            }
            else if ( input.GetType() == typeof( DateTime ) )
            {
                result = ( DateTime ) input;
            }
            else
            {
                throw new Exception( "Invalid Input: " + parameter + " input must be of type string or date" );
            }
            return result;
        }

        /// <summary>
        /// Takes a collection and groups it by the specified attribute value.
        /// </summary>
        /// <param name="input">A collection of objects to be grouped.</param>
        /// <param name="attribute">The attribute key of the attribute value to use when grouping the objects.</param>
        /// <returns>A dictionary of group keys and value collections.</returns>
        /// <example><![CDATA[
        /// {% assign members = group.Members | GroupByAttribute:'Position' %}
        /// <ul>
        /// {% for member in members %}
        ///     {% assign parts = member | PropertyToKeyValue %}
        ///     <li>{{ parts.Key }}</li>
        ///     <ul>
        ///         {% for m in parts.Value %}
        ///             <li>{{ m.Person.FullName }}</li>
        ///         {% endfor %}
        ///     </ul>
        /// {% endfor %}
        /// </ul>
        /// ]]></example>
        public static object GroupByAttribute( object input, string attribute, int? limit = null )
        {
            IEnumerable<object> obj = input is IEnumerable<object> ? input as IEnumerable<object> : new List<object>( new[] { input } );

            IEnumerable<IHasAttributes> e = obj.Cast<IHasAttributes>();

            if ( !e.Any() )
            {
                return new Dictionary<string, List<IModel>>();
            }

            if ( string.IsNullOrWhiteSpace( attribute ) )
            {
                throw new Exception( "Must provide an attribute to group by." );
            }

            //Load Attribute for the Entities
            var first = e.First();
            first.LoadAttributes();
            var attrId = first.Attributes[attribute].Id;
            var values = new AttributeValueService( new RockContext() ).Queryable().Where( av => av.AttributeId == attrId );
            var data = e.Join( values,
                    entity => entity.Id,
                    v => v.EntityId,
                    ( entity, v ) =>
                    {
                        entity.AttributeValues[attribute] = new AttributeValueCache( v );
                        return entity;
                    }
                );

            var grouping = data.AsQueryable().GroupBy( x => x.GetAttributeValue( attribute ) );
            Dictionary<string, object> groupedList;
            if ( limit.HasValue )
            {
                groupedList = grouping.ToDictionary( g => g.Key != null ? g.Key.ToString() : string.Empty, g => ( object ) g.Take( limit.Value ).ToList() );
            }
            else
            {
                groupedList = grouping.ToDictionary( g => g.Key != null ? g.Key.ToString() : string.Empty, g => ( object ) g.ToList() );
            }

            return groupedList;
        }

        /// <summary>
        /// Generate a token specifically to use with the calendar api endpoint
        /// </summary>
        /// <param name="input">The Person the token is for</param>
        /// <param name="expiration">An optional parameter to determine when the token should expire</param>
        /// <returns></returns>
        public static String GenerateCalendarToken( object input, bool? regenerate = true, DateTime? expiration = null )
        {
            string id = "";
            id = input.GetPropertyValue( "Id" ).ToString();
            Person p = new PersonService( new RockContext() ).Get( id );
            string tk = "";
            if ( !regenerate.Value )
            {
                if ( p != null )
                {
                    p.LoadAttributes();
                    tk = p.GetAttributeValue( "PersonalCalendarToken" );
                }
                else
                {
                    throw new Exception( "Input must be a person" );
                }
            }
            else
            {
                //LOL - Microsoft has 15 years to fix this issue.
                DateTime tokenExp = new DateTime( 2038, 1, 18, 0, 0, 0 );
                if ( expiration != null )
                {
                    tokenExp = expiration.Value;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var secret = Encoding.ASCII.GetBytes( GlobalAttributesCache.Get().GetValue( "EventFormCalendarSecret" ) );
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity( new[] { new Claim( "id", id ) } ),
                    Expires = tokenExp,
                    SigningCredentials = new SigningCredentials( new SymmetricSecurityKey( secret ), SecurityAlgorithms.HmacSha256Signature )
                };
                var token = tokenHandler.CreateToken( tokenDescriptor );
                tk = tokenHandler.WriteToken( token );
                if ( p != null )
                {
                    p.SetAttributeValue( "PersonalCalendarToken", tk );
                    p.SaveAttributeValue( "PersonalCalendarToken" );
                }
                else
                {
                    throw new Exception( "Input must be a person" );
                }
            }
            return tk;
        }
    }
}

//Lava to display QR Code
//{% assign id = Person.Id %}
//{% assign qrCode = id | QRCodeFromURL:'405','registrationId' %}
//{% assign qrObj = qrCode | FromJSON %}
//<style>
//.qr {
//    width: 5px;
//    height: 5px;
//}
//.qr-fill {
//    background-color: black;
//}
//table {
//    border-spacing: 0px;
//}
//</style>
//<table>
//{% for r in qrObj %}
//    <tr>
//        {% assign rowStr = r | ToJSON %}
//        {% assign row = rowStr | FromJSON %}
//        {% for c in row %}
//            {% if c == 'true' %}
//                <td class='qr qr-fill'></td>
//            {% else %}
//                <td class='qr'></td>
//            {% endif %}
//        {% endfor %}
//    </tr>
//{% endfor %}
//</table>