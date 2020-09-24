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
using Nest;

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
        private FinancialAccount fund { get; set; }
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
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            context = new RockContext();
            //GroupTypeId = GetAttributeValue( "VeritasSmallGroupTypeId" ).AsInteger();
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
            if ( start.HasValue && end.HasValue && fund != null )
            {
                GenerateData();
            }
            if ( !Page.IsPostBack )
            {

            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Adds attendance entry to metrics.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddAttendance_Click( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        protected void GenerateData()
        {
            //Current Date Range Transactions
            var transactions = new FinancialTransactionService( context ).Queryable().Where( ft => DateTime.Compare( start.Value, ft.TransactionDateTime.Value ) <= 0 && DateTime.Compare( end.Value, ft.TransactionDateTime.Value ) >= 0 );
            //Previous Year Transactions
            var lytransactions = new FinancialTransactionService( context ).Queryable().Where( ft => DateTime.Compare( lystart.Value, ft.TransactionDateTime.Value ) <= 0 && DateTime.Compare( lyend.Value, ft.TransactionDateTime.Value ) >= 0 );
            var groupedTransactions = transactions.GroupBy( t => t.AuthorizedPersonAlias.PersonId ).ToList();
            var groupedLyTransactions = lytransactions.GroupBy( t => t.AuthorizedPersonAlias.PersonId ).ToList();
            var leftJoin =
                            from gt in groupedTransactions
                            join glyt in groupedLyTransactions on gt.Key equals glyt.Key into temp
                            from glyt in temp.DefaultIfEmpty()
                            select new { PersonId = gt.Key, CurrentTransactions = gt, PreviousTransactions = glyt };
            var rightJoin =
                            from glyt in groupedLyTransactions
                            join gt in groupedTransactions on glyt.Key equals gt.Key into temp
                            from gt in temp.DefaultIfEmpty()
                            select new { PersonId = glyt.Key, CurrentTransactions = gt, PreviousTransactions = glyt };
            var joinedData = leftJoin.Union( rightJoin ).ToList();
            List<DonorData> donorData = new List<DonorData>();
            for ( var i = 0; i < joinedData.Count(); i++ )
            {
                //Create DonorData
                Person p = new PersonService( context ).Get( joinedData[i].PersonId );
                p.LoadAttributes();
                DonorData d = new DonorData()
                {
                    PersonId = joinedData[i].PersonId,
                    Donor = p
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
                var allTransactions = new FinancialTransactionService( context ).Queryable().Where( ft => ft.AuthorizedPersonAlias.PersonId == p.Id ).OrderBy( ft => ft.TransactionDateTime );
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
            if ( grdGiving.SortProperty != null )
            {
                donorData = donorData.AsQueryable().Sort( grdGiving.SortProperty ).ToList();
            }
            else
            {
                donorData = donorData.AsQueryable().OrderBy( d => d.Donor.LastName ).ThenBy( d => d.Donor.NickName ).ToList();
            }
            grdGiving.DataSource = donorData;
            grdGiving.DataBind();
        }

        #endregion

        public class DonorData
        {
            public int PersonId { get; set; }
            public Person Donor { get; set; }
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
    }

}