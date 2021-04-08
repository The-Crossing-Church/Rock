﻿using System;
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
using System.Drawing;
using System.Drawing.Imaging;

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
        public static string QRCodeAsImage( object input )
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
                    width: 3px;
                    height: 3px;
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
                    if(qrCode[i,k])
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
            Bitmap m_Bitmap = new Bitmap( qrCode.GetLength(0)*3, qrCode.GetLength(1)*3 );
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