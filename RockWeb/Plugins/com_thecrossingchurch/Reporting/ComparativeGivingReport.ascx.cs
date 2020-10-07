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
//using Microsoft.Ajax.Utilities;
using System.Collections.ObjectModel;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Nest;
using Amazon.S3.Model;
using DocumentFormat.OpenXml.Vml.Spreadsheet;

namespace RockWeb.Plugins.com_thecrossingchurch.Reporting
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Comparative Giving Report" )]
    [Category( "com_thecrossingchurch > Reporting" )]
    [Description( "Comparative Giving Report to view data across years" )]

    [IntegerField( "ContentChannelId", "The id of the content channel.", true, 0, "", 0 )]

    public partial class ComparativeGivingReport : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext context { get; set; }
        private DateTime? start { get; set; }
        private DateTime? end { get; set; }
        private DateTime? lystart { get; set; }
        private DateTime? lyend { get; set; }
        private bool hasChanged { get; set; }
        private FinancialAccount fund { get; set; }
        private GroupService groupSvc { get; set; }
        private PersonService personSvc { get; set; }
        private FinancialTransactionService ftSvc { get; set; }
        private static class PageParameterKey
        {
            public const string Start = "Start";
            public const string End = "End";
            public const string Fund = "Fund";
        }
        #endregion

        #region Base Control Methods

        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent( this.Page );
            ScriptManager.RegisterStartupScript( Page, this.GetType(), "AKey", "colorizeChange();", true );
        }

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            grdGiving.GridRebind += grdGiving_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            context = new RockContext();
            //Get Variable Data from Query Params
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Start ) ) )
            {
                start = DateTime.Parse( PageParameter( PageParameterKey.Start ) );
                lystart = new DateTime( start.Value.Year - 1, start.Value.Month, start.Value.Day );
            }
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.End ) ) )
            {
                end = DateTime.Parse( PageParameter( PageParameterKey.End ) );
                lyend = new DateTime( end.Value.Year - 1, end.Value.Month, end.Value.Day );
            }
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Fund ) ) )
            {
                fund = new FinancialAccountService( context ).Get( Guid.Parse( PageParameter( PageParameterKey.Fund ) ) );
            }
            //Generate Giving Data if filters have data 
            personSvc = new PersonService( context );
            groupSvc = new GroupService( context );
            ftSvc = new FinancialTransactionService( context );
            if ( !Page.IsPostBack )
            {
                if ( start.HasValue && end.HasValue && fund != null )
                {
                    BindGrid();
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Bind Grid
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void grdGiving_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        protected List<DonorData> GenerateData()
        {
            //Current Date Range Transactions
            //For People
            var transactions = ftSvc.Queryable().Where( ft => DateTime.Compare( start.Value, ft.TransactionDateTime.Value ) <= 0 && DateTime.Compare( end.Value, ft.TransactionDateTime.Value ) >= 0 && ft.TransactionTypeValueId == 53 && ft.AuthorizedPersonAlias.Person.RecordTypeValueId == 1 );
            //For Businesses
            var busTransactions = ftSvc.Queryable().Where( ft => DateTime.Compare( start.Value, ft.TransactionDateTime.Value ) <= 0 && DateTime.Compare( end.Value, ft.TransactionDateTime.Value ) >= 0 && ft.TransactionTypeValueId == 53 && ft.AuthorizedPersonAlias.Person.RecordTypeValueId == 2 );
            //Previous Year Transactions
            //For People
            var lytransactions = ftSvc.Queryable().Where( ft => DateTime.Compare( lystart.Value, ft.TransactionDateTime.Value ) <= 0 && DateTime.Compare( lyend.Value, ft.TransactionDateTime.Value ) >= 0 && ft.TransactionTypeValueId == 53 && ft.AuthorizedPersonAlias.Person.RecordTypeValueId == 1 );
            //For Businesses
            var busLyTransactions = ftSvc.Queryable().Where( ft => DateTime.Compare( lystart.Value, ft.TransactionDateTime.Value ) <= 0 && DateTime.Compare( lyend.Value, ft.TransactionDateTime.Value ) >= 0 && ft.TransactionTypeValueId == 53 && ft.AuthorizedPersonAlias.Person.RecordTypeValueId == 2 );

            //Join the Person Data
            var groupedTransactions = transactions.GroupBy( t => t.AuthorizedPersonAlias.Person.PrimaryFamilyId.Value ).OrderBy(e => e.Key).ToList();
            var groupedLyTransactions = lytransactions.GroupBy( t => t.AuthorizedPersonAlias.Person.PrimaryFamilyId.Value ).ToList();
            var leftJoin =
                            from gt in groupedTransactions
                            join glyt in groupedLyTransactions on gt.Key equals glyt.Key into temp
                            from glyt in temp.DefaultIfEmpty()
                            select new JoinDataObj() { KeyId = gt.Key, CurrentTransactions = gt, PreviousTransactions = glyt, Type = "Family" };
            var rightJoin =
                            from glyt in groupedLyTransactions
                            join gt in groupedTransactions on glyt.Key equals gt.Key into temp
                            from gt in temp.DefaultIfEmpty()
                            select new JoinDataObj() { KeyId = glyt.Key, CurrentTransactions = gt, PreviousTransactions = glyt, Type = "Family" };
            var perJoinedData = leftJoin.Union( rightJoin ).DistinctBy(e => e.KeyId).ToList();
            //Join the Business Data
            var busGroupedTransactions = busTransactions.GroupBy( t => t.AuthorizedPersonAlias.Person.Id ).ToList();
            var busGroupedLyTransactions = busLyTransactions.GroupBy( t => t.AuthorizedPersonAlias.Person.Id ).ToList();
            var busLeftJoin =
                            from gt in busGroupedTransactions
                            join glyt in busGroupedLyTransactions on gt.Key equals glyt.Key into temp
                            from glyt in temp.DefaultIfEmpty()
                            select new JoinDataObj() { KeyId = gt.Key, CurrentTransactions = gt, PreviousTransactions = glyt, Type = "Business" };
            var busRightJoin =
                            from glyt in busGroupedLyTransactions
                            join gt in busGroupedTransactions on glyt.Key equals gt.Key into temp
                            from gt in temp.DefaultIfEmpty()
                            select new JoinDataObj() { KeyId = glyt.Key, CurrentTransactions = gt, PreviousTransactions = glyt, Type = "Business" };
            var busJoinedData = busLeftJoin.Union( busRightJoin ).DistinctBy( e => e.KeyId ).ToList();
            //Combine Datasets
            var joinedData = perJoinedData.Union( busJoinedData ).ToList();
            List<DonorData> donorData = new List<DonorData>();
            for ( var i = 0; i < joinedData.Count(); i++ )
            {
                //Create DonorData
                string householdName = "";
                Rock.Model.Group household = null;
                Person p = null;
                List<int> adults = new List<int>(); //Just family now since there are kids who give
                //For Family
                if ( joinedData[i].Type == "Family" )
                {
                    household = groupSvc.Get( joinedData[i].KeyId.Value );
                    adults = household.Members.Select( gm => gm.PersonId ).ToList();
                    p = personSvc.Get( adults.First() );
                    householdName = household.Name.Replace( " Family", "" );
                    householdName += " (" + String.Join( " & ", household.Members.Where( gm => adults.Contains( gm.PersonId ) ).Select( a => a.Person.NickName ).ToList() ) + ")";
                }
                //For business
                else
                {
                    p = personSvc.Get( joinedData[i].KeyId.Value );
                    adults.Add( p.Id );
                    householdName = p.LastName;
                }
                //Remove leading space that is somehow being added to certain records because reasons 
                if ( householdName.Substring( 0, 1 ) == " " )
                {
                    householdName = householdName.Substring( 1 ); 
                }
                p.LoadAttributes();
                DonorData d = new DonorData()
                {
                    FamilyId = joinedData[i].KeyId.Value,
                    Donor = p,
                    Household = household,
                    HouseholdName = householdName
                };

                d.AmountGiven = joinedData[i].CurrentTransactions != null ? joinedData[i].CurrentTransactions.Sum( ft => ft.TransactionDetails.Sum( ftd => ftd.Amount ) ) : 0;
                d.NumberOfGifts = joinedData[i].CurrentTransactions != null ? joinedData[i].CurrentTransactions.Count() : 0;
                d.AverageGiftAmount = d.NumberOfGifts != 0 ? Math.Round( ( d.AmountGiven / d.NumberOfGifts ), 2 ) : 0;
                var prevAmountGiven = joinedData[i].PreviousTransactions != null ? joinedData[i].PreviousTransactions.Sum( ft => ft.TransactionDetails.Sum( ftd => ftd.Amount ) ) : 0;
                d.PreviousNumberOfGifts = joinedData[i].PreviousTransactions != null ? joinedData[i].PreviousTransactions.Count() : 0;
                d.PreviousAverageGiftAmount = d.PreviousNumberOfGifts.Value != 0 ? Math.Round( ( prevAmountGiven / d.PreviousNumberOfGifts.Value ), 2 ) : 0;
                d.AmountChange = d.AmountGiven - prevAmountGiven;
                d.Source = joinedData[i].CurrentTransactions != null ? string.Join( ",", joinedData[i].CurrentTransactions.Select( ft => ft.SourceTypeValue.Value ).Distinct() ) : "";
                d.GivingZone = p.AttributeValues["GivingZone"].ValueFormatted;
                d.Average = p.AttributeValues["AvgDaysBetweenGifts"].ValueFormatted;
                d.StdDev = p.AttributeValues["StdDevFromAvg"].ValueFormatted;
                var allTransactions = ftSvc.Queryable().Where( ft => adults.Contains( ft.AuthorizedPersonAlias.PersonId ) ).OrderBy( ft => ft.TransactionDateTime );
                //If Source is empty for Current Transactions, Try Previous Transactions
                if ( String.IsNullOrEmpty( d.Source ) )
                {
                    d.Source = joinedData[i].PreviousTransactions != null ? string.Join( ",", joinedData[i].PreviousTransactions.Select( ft => ft.SourceTypeValue.Value ).Distinct() ) : string.Join( ",", allTransactions.Select( ft => ft.SourceTypeValue.Value ).Distinct() );
                }
                d.FirstGiftEver = allTransactions.First().TransactionDateTime;
                //Determine Type of Donor
                if ( DateTime.Compare( d.FirstGiftEver.Value, start.Value ) < 0 )
                {
                    //They have donated before this timeframe 
                    d.DonorType = "Existing";
                    var currentTransactions = allTransactions.Where( ft => DateTime.Compare( start.Value, ft.TransactionDateTime.Value ) <= 0 && DateTime.Compare( end.Value, ft.TransactionDateTime.Value ) >= 0 );
                    if ( currentTransactions.Count() == 0 )
                    {
                        //They have donated previously but not in this time frame 
                        d.DonorType = "Lapse";
                    }
                }
                else
                {
                    //This timeframe is the first time they have donated
                    d.DonorType = "New";
                }
                donorData.Add( d );
            }
            return donorData;
        }

        private void BindGrid()
        {
            var donorData = GenerateData();
            if ( grdGiving.SortProperty != null )
            {
                donorData = donorData.AsQueryable().Sort( grdGiving.SortProperty ).ToList();
            }
            else
            {
                donorData = donorData.AsQueryable().OrderBy( d => d.HouseholdName ).ToList();
            }
            grdGiving.DataSource = donorData;
            grdGiving.DataBind();
        }

        #endregion

        public class DonorData
        {
            public int FamilyId { get; set; }
            public int PersonId { get; set; }
            public Person Donor { get; set; }
            public Rock.Model.Group Household { get; set; }
            public string HouseholdName { get; set; }
            public decimal AmountGiven { get; set; }
            public int NumberOfGifts { get; set; }
            public decimal AverageGiftAmount { get; set; }
            public int? PreviousNumberOfGifts { get; set; }
            public decimal? PreviousAverageGiftAmount { get; set; }
            public decimal AmountChange { get; set; }
            public DateTime? FirstGiftEver { get; set; }
            public string Source { get; set; }
            public string GivingZone { get; set; }
            public string DonorType { get; set; }
            public string GiverType { get; set; }
            public string Average { get; set; }
            public string StdDev { get; set; }
        }
        private class JoinDataObj
        {
            public int? KeyId { get; set; }
            public IGrouping<int, FinancialTransaction> PreviousTransactions { get; set; }
            public IGrouping<int, FinancialTransaction> CurrentTransactions { get; set; }
            public string Type { get; set; }
        }
    }

}