using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;


using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;
using Rock.CheckIn;
using System.Data.Entity;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Text;
using System.Web;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Microsoft.Ajax.Utilities;
using System.Collections.ObjectModel;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using CSScriptLibrary;
using System.Data.Entity.Migrations;
using Z.EntityFramework.Plus;
using Newtonsoft.Json;

namespace RockWeb.Plugins.com_thecrossingchurch.Cms
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Sticky Nav Bar" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Adds a sticky navigation bar to a specific page" )]
    [TextField( "Item One Text", required: false, order: 0 )]
    [TextField( "Item One URL", "If filled out, this item will become a button dircting users to a new page", required: false, order: 1 )]
    [TextField( "Item Two Text", required: false, order: 2 )]
    [TextField( "Item Two URL", "If filled out, this item will become a button dircting users to a new page", required: false, order: 3 )]
    [CustomDropdownListField( "Justification", Description = "Where buttons will appear in the bar", DefaultValue = "Center", ListSource = "Left,Center,Right", Order = 4 )]
    [CustomDropdownListField( "Location", Description = "Where bar will stick on the screen", DefaultValue = "Bottom", ListSource = "Top,Bottom", Order = 5 )]
    [TextField( "Background Color", Description = "A Hex or RGB value for color", DefaultValue = "#A2C3CC", Order = 6 )]
    public partial class StickyNavBar : Rock.Web.UI.RockBlock
    {
        #region Variables
        private string text_one { get; set; }
        private string url_one { get; set; }
        private string text_two { get; set; }
        private string url_two { get; set; }
        private string justification { get; set; }
        private string location { get; set; }
        private string color { get; set; }
        #endregion

        #region Base Control Methods

        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent( this.Page );
        }

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            text_one = GetAttributeValue( "ItemOneText" );
            url_one = GetAttributeValue( "ItemOneURL" );
            text_two = GetAttributeValue( "ItemTwoText" );
            url_two = GetAttributeValue( "ItemTwoURL" );
            justification = GetAttributeValue( "Justification" );
            location = GetAttributeValue( "Location" );
            color = GetAttributeValue( "BackgroundColor" );

            divStickyNavBar.Style.Add( "background-color", color );
            if ( !String.IsNullOrEmpty( text_one ) )
            {
                //Item one should be visible
                if ( !String.IsNullOrEmpty( url_one ) )
                {
                    //We are rendering a button
                    btnStickyOne.InnerHtml = text_one;
                    btnStickyOne.HRef = url_one;
                    btnStickyOne.Visible = true;
                    txtStickyOne.Visible = false;
                }
                else
                {
                    //We are rendering text
                    txtStickyOne.InnerHtml = text_one;
                    txtStickyOne.Visible = true;
                    btnStickyOne.Visible = false;
                }
                itemOne.Visible = true;
            }
            if ( !String.IsNullOrEmpty( text_two ) )
            {
                //Item two should be visible
                if ( !String.IsNullOrEmpty( url_two ) )
                {
                    //We are rendering a button
                    btnStickyTwo.InnerHtml = text_two;
                    btnStickyTwo.HRef = url_two;
                    btnStickyTwo.Visible = true;
                    txtStickyTwo.Visible = false;
                }
                else
                {
                    //We are rendering text
                    txtStickyTwo.InnerHtml = text_two;
                    txtStickyTwo.Visible = true;
                    btnStickyTwo.Visible = false;
                }
                itemTwo.Visible = true;
            }
            if ( justification == "Left" )
            {
                divStickyNavBar.Style.Add( "justify-content", "flex-start" );
            }
            if ( justification == "Right" )
            {
                divStickyNavBar.Style.Add( "justify-content", "flex-end" );
            }
            if ( location == "Bottom" )
            {
                divStickyNavBar.AddCssClass( "bottom" );
            }
            else
            {
                divStickyNavBar.AddCssClass( "top" );
            }
        }

        #endregion

        #region Methods

        #endregion

    }
}