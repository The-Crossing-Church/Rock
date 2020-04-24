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
            Template.RegisterFilter(typeof(RockFilters));
        }
    }

    public class RockFilters
    {
        /// <summary>
        /// Generate QR Code for URL.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="page">The page number.</param>
        /// <param name="query">The query string, = will be added to end before the input.</param>
        /// <returns></returns>
        public static string QRCodeFromURL(object input, string page, string query)
        {
            if (input == null || page == null || query == null)
            {
                return null;
            }
            QREncoder Encoder = new QREncoder();
            string host = ConfigurationManager.AppSettings["RockBaseUrl"];
            string url = host + "/page/" + page + "?" + query + "=" + input.ToString();
            var qrCode = Encoder.Encode(ErrorCorrection.M, url);
            return JsonConvert.SerializeObject(qrCode);
        }

        /// <summary>
        /// Generate QR Code from Person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input to turn into a qr.</param>
        /// <returns></returns>
        public static string QRCodeFromString(object input)
        {
            if (input == null)
            {
                return null;
            }
            QREncoder Encoder = new QREncoder();
            string url = input.ToString();
            var qrCode = Encoder.Encode(ErrorCorrection.M, url);
            return JsonConvert.SerializeObject(qrCode);
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