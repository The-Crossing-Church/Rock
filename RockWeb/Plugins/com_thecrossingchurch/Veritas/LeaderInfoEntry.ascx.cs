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
using Newtonsoft.Json;
using CSScriptLibrary;
using System.Data.Entity.Migrations;

namespace RockWeb.Plugins.com_thecrossingchurch.Veritas
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Veritas Leader Info Entry" )]
    [Category( "com_thecrossingchurch > Veritas" )]
    [Description( "Custom block for entering leader info for the dashboard" )]

    [IntegerField( "ContentChannelId", "The id of the content channel.", true, 0, "", 0 )]
    [IntegerField( "Veritas Small Group TypeId", "The id of the Veritas Small Group, Group Type.", true, 0, "", 0 )]

    public partial class LeaderInfoEntry : Rock.Web.UI.RockBlock
    {
        #region Variables
        public int? Id { get; set; }
        public ContentChannelItem ccItem { get; set; }
        public int? LeaderId { get; set; }
        public int? GroupId { get; set; }
        public Person Leader { get; set; }
        public int? GroupTypeId { get; set; }
        public int? ContentChannelId { get; set; }
        public List<Person> members { get; set; }
        public RockContext context { get; set; }
        public GroupService groupSvc { get; set; }
        public GroupMemberService groupMemSvc { get; set; }
        private static class PageParameterKey
        {
            public const string Id = "Id";
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
            grdAttendance.GridRebind += grdAttendance_Rebind;
            grdOneOnOne.GridRebind += grdOneOnOne_Rebind;
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
            GroupTypeId = GetAttributeValue( "VeritasSmallGroupTypeId" ).AsInteger();
            ContentChannelId = GetAttributeValue( "ContentChannelId" ).AsInteger();
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Id ) ) )
            {
                Id = Int32.Parse( PageParameter( PageParameterKey.Id ) );
                ccItem = new ContentChannelItemService( context ).Get( Id.Value );
                ccItem.LoadAttributes();
            }
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.LeaderId ) ) )
            {
                LeaderId = Int32.Parse( PageParameter( PageParameterKey.LeaderId ) );
                Leader = new PersonService( context ).Get( LeaderId.Value );
                pnlTitle.InnerHtml = "Meeting with " + Leader.FullName;
            }
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.GroupId ) ) )
            {
                GroupId = Int32.Parse( PageParameter( PageParameterKey.GroupId ) );
            }
            if ( !Page.IsPostBack )
            {
                if ( LeaderId.HasValue )
                {
                    GroupMember leaderGrp; 
                    if ( GroupId.HasValue )
                    {
                        leaderGrp = groupMemSvc.Queryable().FirstOrDefault( gm => gm.PersonId == LeaderId && gm.GroupId == GroupId.Value );
                    }
                    else
                    {
                        leaderGrp = groupMemSvc.Queryable().FirstOrDefault( gm => gm.PersonId == LeaderId && gm.GroupRole.IsLeader == true && gm.Group.IsActive == true && gm.Group.GroupTypeId == GroupTypeId );
                    }
                    if ( leaderGrp != null )
                    {
                        members = groupMemSvc.Queryable().Where( gm => gm.GroupId == leaderGrp.GroupId && gm.GroupRole.IsLeader == false ).Select( gm => gm.Person ).ToList();
                    }
                }
                GenerateAttendance();
                GenerateOneOnOnes();
                GenerateGeneralFeelings();
                if ( Id.HasValue )
                {
                    LoadData();
                }
            }
        }

        #endregion

        #region Events

        protected void grdAttendance_Rebind( object sender, EventArgs e )
        {
            GenerateAttendance();
        }
        protected void grdOneOnOne_Rebind( object sender, EventArgs e )
        {
            GenerateOneOnOnes();
        }

        /// <summary>
        /// Adds attendance entry to metrics.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            ContentChannelItem item = new ContentChannelItem();
            //set to actual item if exists
            if ( Id.HasValue )
            {
                item = ccItem;
            }
            else
            {
                item.ContentChannelId = ContentChannelId.Value;
                item.LoadAttributes();
                item.CreatedByPersonAliasId = CurrentPersonAliasId;
                item.CreatedDateTime = RockDateTime.Now;
            }
            item.Title = "Meeting with " + Leader.FullName + " (" + MeetingDate.SelectedDate.Value.ToString("MM/dd/yyyy") + ")";
            item.ModifiedDateTime = item.CreatedDateTime;
            item.ModifiedByPersonAliasId = CurrentPersonAliasId;
            //Set Basic Info
            item.SetAttributeValue( "Leader", Leader.Guid );
            item.SetAttributeValue( "MeetingDate", MeetingDate.SelectedDate );
            //Set Attendance
            var selectedAttendance = grdAttendance.SelectedKeys;
            item.SetAttributeValue( "Attendance", string.Join( ",", selectedAttendance ) );
            var selectedOneOnOnes = grdOneOnOne.SelectedKeys;
            item.SetAttributeValue( "OneonOnes", string.Join( ",", selectedOneOnOnes ) );
            //Set the other questions and their detial notes
            item.SetAttributeValue( "AttendingtheirSmallGroup", chkAttendingSG.Checked.ToString() );
            item.SetAttributeValue( "AttendingTheirSmallGroupDetails", txtAttendingSG.Text );
            item.SetAttributeValue( "SpiritualGrowth", chkSpiritualGrowth.Checked.ToString() );
            item.SetAttributeValue( "SpiritualGrowthDetails", txtSpiritualGrowth.Text );
            item.SetAttributeValue( "ConsistentmeetingswithCO", chkMeetingCO.Checked.ToString() );
            item.SetAttributeValue( "ConsistentmeetingswithCODetails", txtMeetingCO.Text );
            item.SetAttributeValue( "Preppinglessonsintentionally", chkPreping.Checked.ToString() );
            item.SetAttributeValue( "PreppinglessonsintentionallyDetails", txtPreping.Text );
            item.SetAttributeValue( "AttendingLeaderMeetings", chkLeaderMeetings.Checked.ToString() );
            item.SetAttributeValue( "AttendingLeaderMeetingsDetails", txtLeaderMeetings.Text );
            item.SetAttributeValue( "Dotheyneedresources", chkResources.Checked.ToString() );
            item.SetAttributeValue( "DotheyneedresourcesDetails", txtResources.Text );
            item.SetAttributeValue( "Thingtoprayfor", chkPrayer.Checked.ToString() );
            item.SetAttributeValue( "ThingtoprayforDetails", txtPrayer.Text );
            item.SetAttributeValue( "Booksrecommendations", chkBooks.Checked.ToString() );
            item.SetAttributeValue( "BooksrecommendationsDetails", txtBooks.Text );
            item.SetAttributeValue( "TheologicalQuestions", chkQuestions.Checked.ToString() );
            item.SetAttributeValue( "TheologicalQuestionsDetails", txtQuestions.Text );
            item.SetAttributeValue( "Curriculum", chkCirriculum.Checked.ToString() );
            item.SetAttributeValue( "CurriculumDetails", txtCirriculum.Text );
            item.SetAttributeValue( "Notes", txtNotes.Text );
            item.SetAttributeValue( "GeneralFeelings", String.Join( ",", chkListGeneralFeelings.SelectedValues ) );

            //Save everything
            context.ContentChannelItems.AddOrUpdate( item );
            context.SaveChanges();
            item.SaveAttributeValues( context );
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add( "LeaderId", LeaderId.ToString() );
            query.Add( "GroupId", GroupId.ToString() );
            NavigateToParentPage( query );
        }

        #endregion

        #region Methods

        private void GenerateAttendance()
        {
            if ( LeaderId.HasValue )
            {
                grdAttendance.RowItemText = "Person";
                grdAttendance.DataKeyNames = new string[1] { "Id" };
                grdAttendance.CommunicationRecipientPersonIdFields.Add( "Id" );
                grdAttendance.DataSource = members;
                //Pre-Select if we are editing
                if ( Id.HasValue )
                {
                    var rawAtt = ccItem.GetAttributeValue( "Attendance" ).Split( ',' ).Select( e => Int32.Parse( e ) ).ToList();
                    var selectedAttendance = grdAttendance.SelectedKeys;
                    for ( var i = 0; i < rawAtt.Count(); i++ )
                    {
                        selectedAttendance.Add( rawAtt[i] );
                    }
                    grdAttendance.Columns.OfType<SelectField>().FirstOrDefault().SetValue( "Id", selectedAttendance );
                }
                grdAttendance.DataBind();
            }
        }

        private void GenerateOneOnOnes()
        {
            if ( LeaderId.HasValue )
            {
                grdOneOnOne.RowItemText = "Person";
                grdOneOnOne.DataKeyNames = new string[1] { "Id" };
                grdOneOnOne.CommunicationRecipientPersonIdFields.Add( "Id" );
                grdOneOnOne.DataSource = members;
                //Pre-Select if we are editing
                if ( Id.HasValue )
                {
                    var rawOOO = ccItem.GetAttributeValue( "OneonOnes" ).Split( ',' ).Select( e => Int32.Parse( e ) ).ToList();
                    var selectedOneOnOnes = grdOneOnOne.SelectedKeys;
                    for ( var i = 0; i < rawOOO.Count(); i++ )
                    {
                        selectedOneOnOnes.Add( rawOOO[i] );
                    }
                    grdOneOnOne.Columns.OfType<SelectField>().FirstOrDefault().SetValue( "Id", selectedOneOnOnes );
                }
                grdOneOnOne.DataBind();
            }
        }

        private void GenerateGeneralFeelings()
        {
            ContentChannelItem item = new ContentChannelItem();
            //set to actual item if exists
            if ( Id.HasValue )
            {
                item = ccItem;
            }
            else
            {
                item.ContentChannelId = ContentChannelId.Value;
                item.LoadAttributes();
            }
            var feels = item.Attributes.FirstOrDefault( av => av.Key == "GeneralFeelings" );
            var feelsAttr = new AttributeService( context ).Get( feels.Value.Id );
            feelsAttr.LoadAttributes();
            var values = JsonConvert.DeserializeObject<KeyValuePair<string, string>[]>( feelsAttr.AttributeQualifiers.FirstOrDefault( aq => aq.Key == "listItems" ).Value );
            chkListGeneralFeelings.DataSource = values.Select( kv => kv.Value );
            chkListGeneralFeelings.DataBind();
            if ( ccItem != null && !String.IsNullOrEmpty( ccItem.GetAttributeValue( "GeneralFeelings" ) ) )
            {
                var vals = ccItem.GetAttributeValue( "GeneralFeelings" ).Split( ',' );
                foreach ( ListItem itm in chkListGeneralFeelings.Items )
                {
                    itm.Selected = vals.Contains( itm.Value );
                }
            }
        }

        private void LoadData()
        {
            MeetingDate.SelectedDate = DateTime.Parse( ccItem.GetAttributeValue( "MeetingDate" ) );
            chkAttendingSG.Checked = Boolean.Parse( ccItem.GetAttributeValue( "AttendingtheirSmallGroup" ) );
            txtAttendingSG.Text = ccItem.GetAttributeValue( "AttendingTheirSmallGroupDetails" );
            chkSpiritualGrowth.Checked = Boolean.Parse( ccItem.GetAttributeValue( "SpiritualGrowth" ) );
            txtSpiritualGrowth.Text = ccItem.GetAttributeValue( "SpiritualGrowthDetails" );
            chkMeetingCO.Checked = Boolean.Parse( ccItem.GetAttributeValue( "ConsistentmeetingswithCO" ) );
            txtMeetingCO.Text = ccItem.GetAttributeValue( "ConsistentmeetingswithCODetails" );
            chkPreping.Checked = Boolean.Parse( ccItem.GetAttributeValue( "Preppinglessonsintentionally" ) );
            txtPreping.Text = ccItem.GetAttributeValue( "PreppinglessonsintentionallyDetails" );
            chkLeaderMeetings.Checked = Boolean.Parse( ccItem.GetAttributeValue( "AttendingLeaderMeetings" ) );
            txtLeaderMeetings.Text = ccItem.GetAttributeValue( "AttendingLeaderMeetingsDetails" );
            chkResources.Checked = Boolean.Parse( ccItem.GetAttributeValue( "Dotheyneedresources" ) );
            txtResources.Text = ccItem.GetAttributeValue( "DotheyneedresourcesDetails" );
            chkPrayer.Checked = Boolean.Parse( ccItem.GetAttributeValue( "Thingtoprayfor" ) );
            txtPrayer.Text = ccItem.GetAttributeValue( "ThingtoprayforDetails" );
            chkBooks.Checked = Boolean.Parse( ccItem.GetAttributeValue( "Booksrecommendations" ) );
            txtBooks.Text = ccItem.GetAttributeValue( "BooksrecommendationsDetails" );
            chkQuestions.Checked = Boolean.Parse( ccItem.GetAttributeValue( "TheologicalQuestions" ) );
            txtQuestions.Text = ccItem.GetAttributeValue( "TheologicalQuestionsDetails" );
            chkCirriculum.Checked = Boolean.Parse( ccItem.GetAttributeValue( "Curriculum" ) );
            txtCirriculum.Text = ccItem.GetAttributeValue( "CurriculumDetails" );
            txtNotes.Text = ccItem.GetAttributeValue( "Notes" );
        }

        #endregion
    }
}