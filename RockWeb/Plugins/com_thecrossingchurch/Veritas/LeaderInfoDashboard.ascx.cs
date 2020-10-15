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
using Newtonsoft.Json;
using CSScriptLibrary;
using System.Data.Entity.Migrations;

namespace RockWeb.Plugins.com_thecrossingchurch.Veritas
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Veritas Leader Info Dashboard" )]
    [Category( "com_thecrossingchurch > Veritas" )]
    [Description( "Custom block for viewing leader info" )]

    [IntegerField( "ContentChannelId", "The id of the content channel.", true, 0, "", 0 )]
    [IntegerField( "Veritas Small Group Parent Group Id", "The id of the Veritas Small Group, Parent Group.", true, 0, "", 0 )]
    [IntegerField( "Detailed View Page Id", "The id of the leader overview page.", true, 0, "", 0 )]

    public partial class LeaderInfoDashboard : Rock.Web.UI.RockBlock
    {
        #region Variables
        public int? ParentGroupId { get; set; }
        public int? ContentChannelId { get; set; }
        public int? DetailedViewPageId { get; set; }
        public RockContext context { get; set; }
        public GroupService groupSvc { get; set; }
        public GroupMemberService groupMemSvc { get; set; }
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
            ParentGroupId = GetAttributeValue( "VeritasSmallGroupParentGroupId" ).AsInteger();
            ContentChannelId = GetAttributeValue( "ContentChannelId" ).AsInteger();
            DetailedViewPageId = GetAttributeValue( "DetailedViewPageId" ).AsInteger();
            pkrGroup.RootGroupId = ParentGroupId.Value;
            if ( !Page.IsPostBack )
            {

            }
        }

        #endregion

        #region Events
        /// <summary>
        /// Pull Data for Small Group Leaders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void pkrGroup_SelectItem( object sender, EventArgs e )
        {
            Group selected = groupSvc.Get( Int32.Parse( pkrGroup.SelectedValue ) );
            List<GroupMember> leaders = selected.Members.Where( gm => gm.GroupRole.IsLeader ).ToList();
            var label = new HtmlGenericControl( "label" );
            label.InnerText = "Leaders";
            label.AddCssClass( "control-label" );
            divLeaders.Controls.Add( label );
            for ( int i = 0; i < leaders.Count(); i++ )
            {
                var a = new HtmlGenericControl( "div" );
                a.InnerHtml = "<a href='/page/" + DetailedViewPageId + "?LeaderId=" + leaders[i].Person.Id + "&GroupId=" + selected.Id + "'>" + leaders[i].Person.FullName + "</a>";
                divLeaders.Controls.Add(a);
            }
        }

        #endregion

        #region Methods

        #endregion

    }
}