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

namespace com_thecrossingchurch.LavaFilters {
    /// <summary>
    /// Custom startup class used to register custom filters
    /// </summary>
    public class Startup : IRockStartup {
        public int StartupOrder => 0;
        /// <summary>
        /// Called when rock applicatoin starts up.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void OnStartup() {
            Template.RegisterFilter(typeof(RockFilters));
        }
    }

    public class RockFilters {
        /// <summary>
        /// Generate QR Code for URL.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="page">The page number.</param>
        /// <param name="query">The query string, = will be added to end before the input.</param>
        /// <returns></returns>
        public static string QRCodeFromURL(object input, string page, string query) {
            if (input == null || page == null || query == null) {
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
        /// <param name="input">The input to turn into a qr.</param>
        /// <returns></returns>
        public static string QRCodeFromString(object input) {
            if (input == null) {
                return null;
            }
            QREncoder Encoder = new QREncoder();
            string url = input.ToString();
            var qrCode = Encoder.Encode(ErrorCorrection.M, url);
            return JsonConvert.SerializeObject(qrCode);
        }

        /// <summary>
        /// Generate QR Code from Person.
        /// </summary>
        /// <param name="input">The input to turn into a qr.</param>
        /// <returns></returns>
        public static string QRCodeAsImage(object input, int? size = 3) {
            if (input == null) {
                return null;
            }
            QREncoder Encoder = new QREncoder();
            string url = input.ToString();
            var qrCode = Encoder.Encode(ErrorCorrection.M, url);

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
            int x = qrCode.GetLength(0);
            int y = qrCode.GetLength(1);
            for (int i = 0; i < qrCode.GetLength(0); i++) {
                html += "<tr>";
                for (int k = 0; k < qrCode.GetLength(1); k++) {
                    if (qrCode[i, k]) {
                        html += "<td class='qr qr-fill'></td>";
                    } else {
                        html += "<td class='qr'></td>";
                    }
                }
                html += "</tr>";
            }
            html += "</table>";
            Bitmap m_Bitmap = new Bitmap(qrCode.GetLength(0) * size.Value, qrCode.GetLength(1) * size.Value);
            PointF point = new PointF(0, 0);
            SizeF maxSize = new System.Drawing.SizeF(500, 500);
            TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.Render(Graphics.FromImage(m_Bitmap), html, point, maxSize);
            string dataUrl = "";
            using (MemoryStream ms = new MemoryStream()) {
                m_Bitmap.Save(ms, ImageFormat.Png);
                byte[] byteArr = ms.ToArray();
                string b64Txt = Convert.ToBase64String(byteArr);
                dataUrl = "data:image/png;base64," + b64Txt;
            }
            Console.WriteLine("x");
            return dataUrl;
        }

        /// <summary>
        /// Array Pop Functionality.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static dynamic Pop(object input) {
            var type = input.GetType();
            if (input.GetType() == typeof(string)) {
                List<string> list = input.ToString().Split(',').ToList();
                list.RemoveAt(0);
                return string.Join(",", list);
            } else if (type.FullName.Contains("Collection")) {
                Type listType = typeof(List<>).MakeGenericType(new[] { type });
                IList list = (IList)Activator.CreateInstance(listType);
                list = (IList)input;
                list.RemoveAt(0);
                return list;
            } else {
                throw new Exception("Invalid Input: input must be of type string or Collection");
            }
        }

        /// <summary>
        /// Gives correct pronoun for list of personids
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static dynamic Pronoun(object input) {
            var type = input.GetType();
            if (input.GetType() == typeof(string)) {
                List<string> list = input.ToString().Split(',').ToList();
                if (list.Count() > 1) {
                    return new { Subject = "they", Object = "them", Posessive = "their" };
                }
                Person p = new PersonService(new RockContext()).Get(Int32.Parse(list[0]));
                if (p.Gender == Gender.Female) {
                    return new { Subject = "she", Object = "her", Posessive = "her" };
                } else if (p.Gender == Gender.Male) {
                    return new { Subject = "he", Object = "him", Posessive = "his" };
                }
                return new { Subject = "they", Object = "them", Posessive = "their" };
            } else {
                throw new Exception("Invalid Input: input must be of type string");
            }
        }

        /// <summary>
        /// Return true if the date is within the provided range, false if outside of range
        /// </summary>
        /// <param name="input">The input date.</param>
        /// <param name="start">The range start date.</param>
        /// <param name="end">The range end date.</param>
        /// <returns></returns>
        public static bool DateIsBetween(object input, object start, object end) {
            var type = input.GetType();
            DateTime? target = null;
            DateTime? rangeStart = null;
            DateTime? rangeEnd = null;
            if (input.GetType() == typeof(string)) {
                target = DateTime.Parse(input.ToString());
            } else if (type.FullName.Contains("Date")) {
                target = (DateTime)input;
            } else {
                throw new Exception("Invalid Input: input must be of type string or date");
            }
            if (start.GetType() == typeof(string)) {
                rangeStart = DateTime.Parse(start.ToString());
            } else if (start.GetType().FullName.Contains("Date")) {
                rangeStart = (DateTime)start;
            } else {
                throw new Exception("Invalid Input: start of range must be of type string or date");
            }
            if (end.GetType() == typeof(string)) {
                rangeEnd = DateTime.Parse(end.ToString());
            } else if (end.GetType().FullName.Contains("Date")) {
                rangeEnd = (DateTime)end;
            } else {
                throw new Exception("Invalid Input: end of range must be of type string or date");
            }
            if (target.HasValue && rangeEnd.HasValue && rangeStart.HasValue) {
                if (DateTime.Compare(target.Value, rangeStart.Value) >= 0 && DateTime.Compare(target.Value, rangeEnd.Value) <= 0) {
                    return true;
                }
                return false;
            }
            throw new Exception("Unable to parse input, start, and end");
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