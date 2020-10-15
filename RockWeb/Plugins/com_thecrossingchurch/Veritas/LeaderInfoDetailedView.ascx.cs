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

namespace RockWeb.Plugins.com_thecrossingchurch.Veritas
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Veritas Leader Info Detailed View" )]
    [Category( "com_thecrossingchurch > Veritas" )]
    [Description( "Custom block for viewing leader data" )]

    [IntegerField( "ContentChannelId", "The id of the content channel.", true, 0, "", 0 )]
    [IntegerField( "Veritas Small Group TypeId", "The id of the Veritas Small Group, Group Type.", true, 0, "", 0 )]
    [IntegerField( "Entry Page Id", "The id of the Leader Info Entry Page.", true, 0, "", 0 )]

    public partial class LeaderInfoDetailedView : Rock.Web.UI.RockBlock
    {
        #region Variables
        public int? LeaderId { get; set; }
        public int? GroupId { get; set; }
        public Person Leader { get; set; }
        public int? GroupTypeId { get; set; }
        public int? ContentChannelId { get; set; }
        public int? PageId { get; set; }
        public List<Person> members { get; set; }
        public RockContext context { get; set; }
        public GroupService groupSvc { get; set; }
        public GroupMemberService groupMemSvc { get; set; }
        public ContentChannelItemService cciSvc { get; set; }
        private static class PageParameterKey
        {
            public const string LeaderId = "LeaderId";
            public const string GroupId = "GroupId";
        }
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
            context = new RockContext();
            groupSvc = new GroupService( context );
            groupMemSvc = new GroupMemberService( context );
            cciSvc = new ContentChannelItemService( context );
            GroupTypeId = GetAttributeValue( "VeritasSmallGroupTypeId" ).AsInteger();
            ContentChannelId = GetAttributeValue( "ContentChannelId" ).AsInteger();
            PageId = GetAttributeValue( "EntryPageId" ).AsInteger();
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.LeaderId ) ) && !String.IsNullOrEmpty( PageParameter( PageParameterKey.GroupId ) ) ) 
            {
                LeaderId = Int32.Parse( PageParameter( PageParameterKey.LeaderId ) );
                GroupId = Int32.Parse( PageParameter( PageParameterKey.GroupId ) );
                Leader = new PersonService( context ).Get( LeaderId.Value );
                LoadItems();
            }
            if ( !Page.IsPostBack )
            {

            }
        }

        #endregion

        #region Events


        #endregion

        #region Methods

        private void LoadItems()
        {
            var items = cciSvc.Queryable().Where( cci => cci.ContentChannelId == ContentChannelId.Value ).ToList();
            items.LoadAttributes(); 
            var leadersItems = items.Where( i => i.AttributeValues.Any(av => av.Key == "Leader" && av.Value.Value == Leader.Guid.ToString() )  ).OrderByDescending(i => DateTime.Parse(i.AttributeValues.FirstOrDefault(av => av.Key == "MeetingDate").Value.Value)).ToList();
            var members = groupMemSvc.Queryable().Where( gm => gm.GroupId == GroupId && gm.GroupRole.IsLeader == false ).ToList().Count();
            var hOne = new HtmlGenericContainer( "h2" );
            hOne.InnerText = Leader.FullName + " " + groupSvc.Get( GroupId.Value ).Name;
            divHeader.Controls.Add( hOne );
            aCreateNew.HRef = "/page/" + PageId + "?LeaderId=" + LeaderId + "&GroupId=" + GroupId;
            var row = new HtmlGenericControl( "div" );
            row.AddCssClass( "row" );
            for(var i=0; i<leadersItems.Count(); i++ )
            {
                var col = new HtmlGenericControl( "div" );
                col.AddCssClass( "col col-xs-12 col-sm-6 col-md-3" );
                var anchor = new HtmlGenericControl( "a" );
                string href = "/page/" + PageId + "?LeaderId=" + LeaderId + "&Id=" + leadersItems[i].Id + "&GroupId=" + GroupId;
                anchor.Attributes.Add( "href", href );
                var card = new HtmlGenericControl( "div" );
                card.AddCssClass( "card" );
                var dt = new HtmlGenericControl( "div" );
                dt.AddCssClass( "card-date" );
                dt.InnerText = DateTime.Parse( leadersItems[i].GetAttributeValue( "MeetingDate" ) ).ToString( "MM/dd/yyyy" );
                card.Controls.Add( dt );
                var hr = new HtmlGenericContainer( "hr" );
                card.Controls.Add( hr );
                var staff = new HtmlGenericControl( "div" );
                staff.AddCssClass( "card-staff" );
                staff.InnerText = "Met with " + leadersItems[i].CreatedByPersonName;
                card.Controls.Add( staff );
                var attending = leadersItems[i].GetAttributeValue( "Attendance" ).Split( ',' ).ToList().Count();
                var att = new HtmlGenericControl( "div" );
                att.InnerText = "Attendance: " + attending + "/" + members;
                card.Controls.Add( att );
                var oneOnOne = leadersItems[i].GetAttributeValue( "OneonOnes" ).Split( ',' ).ToList().Count();
                var one = new HtmlGenericControl( "div" );
                one.InnerText = "One on Ones: " + oneOnOne + "/" + members;
                card.Controls.Add( one );
                var temp = leadersItems[i].GetAttributeValue( "Thingtoprayfor" ); 
                if (leadersItems[i].GetAttributeValue( "Thingtoprayfor" ) == "True" )
                {
                    var prayer = new HtmlGenericControl( "div" );
                    var label = new HtmlGenericControl( "div" );
                    label.AddCssClass( "floating-label" );
                    label.InnerText = "Prayer Requests";
                    var req = new HtmlGenericControl( "div" );
                    req.InnerText = leadersItems[i].GetAttributeValue( "ThingtoprayforDetails" );
                    prayer.Controls.Add( label );
                    prayer.Controls.Add( req );
                    card.Controls.Add( prayer );
                }
                if(leadersItems[i].GetAttributeValue( "Dotheyneedresources" ) == "True" )
                {
                    var resources = new HtmlGenericControl( "div" );
                    var label = new HtmlGenericControl( "div" );
                    label.AddCssClass( "floating-label" );
                    label.InnerText = "Requested Resources";
                    var req = new HtmlGenericControl( "div" );
                    req.InnerText = leadersItems[i].GetAttributeValue( "DotheyneedresourcesDetails" );
                    resources.Controls.Add( label );
                    resources.Controls.Add( req );
                    card.Controls.Add( resources );
                }
                if(leadersItems[i].GetAttributeValue( "TheologicalQuestions" ) == "True" )
                {
                    var resources = new HtmlGenericControl( "div" );
                    var label = new HtmlGenericControl( "div" );
                    label.AddCssClass( "floating-label" );
                    label.InnerText = "Theological Questions";
                    var req = new HtmlGenericControl( "div" );
                    req.InnerText = leadersItems[i].GetAttributeValue( "TheologicalQuestionsDetails" );
                    resources.Controls.Add( label );
                    resources.Controls.Add( req );
                    card.Controls.Add( resources );
                }
                anchor.Controls.Add( card );
                col.Controls.Add( anchor );
                row.Controls.Add( col );
            }
            divInfo.Controls.Add( row );
        }

        #endregion
    }
}