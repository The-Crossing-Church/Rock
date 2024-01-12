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
using System.Threading.Tasks;
using Rock.Communication;

namespace RockWeb.Plugins.com_thecrossingchurch.Reporting
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Comparative Giving Report" )]
    [Category( "com_thecrossingchurch > Reporting" )]
    [Description( "Comparative Giving Report to view data across years" )]
    [PersonField( "Person", "Who will be notified when the report is complete", true )]

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
        private AttributeValueService avSvc { get; set; }
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
            scriptManager.RegisterPostBackControl( this.btnExport );
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
            //Generate Giving Data if filters have data 
            personSvc = new PersonService( context );
            groupSvc = new GroupService( context );
            ftSvc = new FinancialTransactionService( context );
            avSvc = new AttributeValueService( context );
            if ( pkrAcct.SelectedValue == "0" )
            {
                pkrAcct.SetValue( 12 );
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
            try
            {
                //Current Date Range Transactions
                //For People
                var transactions = ftSvc.Queryable().Where( ft => DateTime.Compare( start.Value, ft.TransactionDateTime.Value ) <= 0 && DateTime.Compare( end.Value, ft.TransactionDateTime.Value ) >= 0 && ft.TransactionTypeValueId == 53 && ft.AuthorizedPersonAlias.Person.RecordTypeValueId == 1 && ft.TransactionDetails.Any( ftd => ftd.AccountId == fund.Id ) ).Select( ft => new FinancialTransactionLight { Id = ft.Id, AuthorizedPersonAlias = ft.AuthorizedPersonAlias, TransactionDetails = ft.TransactionDetails, TransactionDateTime = ft.TransactionDateTime, SourceTypeValue = ft.SourceTypeValue } ).ToList();
                //For Businesses
                var busTransactions = ftSvc.Queryable().Where( ft => DateTime.Compare( start.Value, ft.TransactionDateTime.Value ) <= 0 && DateTime.Compare( end.Value, ft.TransactionDateTime.Value ) >= 0 && ft.TransactionTypeValueId == 53 && ft.AuthorizedPersonAlias.Person.RecordTypeValueId == 2 && ft.TransactionDetails.Any( ftd => ftd.AccountId == fund.Id ) ).Select( ft => new FinancialTransactionLight { Id = ft.Id, AuthorizedPersonAlias = ft.AuthorizedPersonAlias, TransactionDetails = ft.TransactionDetails, TransactionDateTime = ft.TransactionDateTime, SourceTypeValue = ft.SourceTypeValue } ).ToList();
                //Previous Year Transactions
                //For People
                var lytransactions = ftSvc.Queryable().Where( ft => DateTime.Compare( lystart.Value, ft.TransactionDateTime.Value ) <= 0 && DateTime.Compare( lyend.Value, ft.TransactionDateTime.Value ) >= 0 && ft.TransactionTypeValueId == 53 && ft.AuthorizedPersonAlias.Person.RecordTypeValueId == 1 && ft.TransactionDetails.Any( ftd => ftd.AccountId == fund.Id ) ).Select( ft => new FinancialTransactionLight { Id = ft.Id, AuthorizedPersonAlias = ft.AuthorizedPersonAlias, TransactionDetails = ft.TransactionDetails, TransactionDateTime = ft.TransactionDateTime, SourceTypeValue = ft.SourceTypeValue } ).ToList();
                //For Businesses
                var busLyTransactions = ftSvc.Queryable().Where( ft => DateTime.Compare( lystart.Value, ft.TransactionDateTime.Value ) <= 0 && DateTime.Compare( lyend.Value, ft.TransactionDateTime.Value ) >= 0 && ft.TransactionTypeValueId == 53 && ft.AuthorizedPersonAlias.Person.RecordTypeValueId == 2 && ft.TransactionDetails.Any( ftd => ftd.AccountId == fund.Id ) ).Select( ft => new FinancialTransactionLight { Id = ft.Id, AuthorizedPersonAlias = ft.AuthorizedPersonAlias, TransactionDetails = ft.TransactionDetails, TransactionDateTime = ft.TransactionDateTime, SourceTypeValue = ft.SourceTypeValue } ).ToList();

                //Join the Person Data
                var groupedTransactions = transactions.GroupBy( t => t.AuthorizedPersonAlias.Person.PrimaryFamilyId.Value ).OrderBy( e => e.Key ).ToList();
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
                var perJoinedData = leftJoin.Union( rightJoin ).DistinctBy( e => e.KeyId ).ToList();
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
                for (var i = 0; i < joinedData.Count(); i++)
                {
                    //Create DonorData
                    string householdName = "";
                    Rock.Model.Group household = null;
                    Person p = null;
                    List<int> adults = new List<int>(); //Just family now since there are kids who give
                                                        //For Family
                    if (joinedData[i].Type == "Family")
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
                    if (householdName.Substring( 0, 1 ) == " ")
                    {
                        householdName = householdName.Substring( 1 );
                    }
                    //p.LoadAttributes();
                    Location home = p.GetHomeLocation();
                    DonorData d = new DonorData()
                    {
                        FamilyId = joinedData[i].KeyId.Value,
                        Donor = p,
                        Household = household,
                        HouseholdName = householdName,
                        Address = home != null ? home.Street1 + ", " + home.City + ", " + home.State : ""
                    };

                    d.AmountGiven = joinedData[i].CurrentTransactions != null ? joinedData[i].CurrentTransactions.Sum( ft => ft.TransactionDetails.Where( ftd => ftd.AccountId == fund.Id ).Sum( ftd => ftd.Amount ) ) : 0;
                    d.NumberOfGifts = joinedData[i].CurrentTransactions != null ? joinedData[i].CurrentTransactions.Count() : 0;
                    d.AverageGiftAmount = d.NumberOfGifts != 0 ? Math.Round( (d.AmountGiven / d.NumberOfGifts), 2 ) : 0;
                    d.PreviousAmountGiven = joinedData[i].PreviousTransactions != null ? joinedData[i].PreviousTransactions.Sum( ft => ft.TransactionDetails.Where( ftd => ftd.AccountId == fund.Id ).Sum( ftd => ftd.Amount ) ) : 0;
                    d.PreviousNumberOfGifts = joinedData[i].PreviousTransactions != null ? joinedData[i].PreviousTransactions.Count() : 0;
                    d.PreviousAverageGiftAmount = d.PreviousNumberOfGifts.Value != 0 ? Math.Round( (d.PreviousAmountGiven / d.PreviousNumberOfGifts.Value), 2 ) : 0;
                    d.AmountChange = d.AmountGiven - d.PreviousAmountGiven;
                    d.Source = joinedData[i].CurrentTransactions != null ? string.Join( ",", joinedData[i].CurrentTransactions.Select( ft => ft.SourceTypeValue.Value ).Distinct() ) : "";
                    var allTransactions = ftSvc.Queryable().Where( ft => adults.Contains( ft.AuthorizedPersonAlias.PersonId ) && ft.TransactionTypeValueId == 53 ).Select( ft => new { Id = ft.Id, TransactionDateTime = ft.TransactionDateTime, SourceTypeValue = ft.SourceTypeValue, TransactionDetails = ft.TransactionDetails } ).OrderBy( ft => ft.TransactionDateTime ).ToList();
                    // Calculate Giving Zone Based on previous year or current year
                    var startOfYear = new DateTime( lystart.Value.Year, 1, 1, 0, 0, 0 );
                    var endOfYear = new DateTime( lystart.Value.Year, 12, 31, 23, 59, 59 );
                    var givingZoneTransactions = allTransactions.Where( ft => DateTime.Compare( startOfYear, ft.TransactionDateTime.Value ) <= 0 && DateTime.Compare( endOfYear, ft.TransactionDateTime.Value ) >= 0 && ft.TransactionDetails.Any( ftd => ftd.AccountId == fund.Id ) ).ToList();
                    if (givingZoneTransactions.Count() == 0)
                    {
                        startOfYear = new DateTime( start.Value.Year, 1, 1, 0, 0, 0 );
                        endOfYear = new DateTime( start.Value.Year, 12, 31, 23, 59, 59 );
                        givingZoneTransactions = allTransactions.Where( ft => DateTime.Compare( startOfYear, ft.TransactionDateTime.Value ) <= 0 && DateTime.Compare( endOfYear, ft.TransactionDateTime.Value ) >= 0 && ft.TransactionDetails.Any( ftd => ftd.AccountId == fund.Id ) ).ToList();
                    }
                    decimal givingZoneAmt = 0;
                    if (givingZoneTransactions.Count() > 0)
                    {
                        givingZoneAmt = givingZoneTransactions.Sum( ft => ft.TransactionDetails.Where( ftd => ftd.AccountId == fund.Id ).Sum( ftd => ftd.Amount ) );

                    }
                    //var givingZoneAmt = prevAmountGiven > 0 ? prevAmountGiven : d.AmountGiven;
                    if (givingZoneAmt < 1)
                    {
                        d.GivingZone = "Unknown";
                    }
                    else if (givingZoneAmt < 600)
                    {
                        d.GivingZone = "Zone 1";
                    }
                    else if (givingZoneAmt < 1200)
                    {
                        d.GivingZone = "Zone 2";
                    }
                    else if (givingZoneAmt < 2400)
                    {
                        d.GivingZone = "Zone 3";
                    }
                    else if (givingZoneAmt < 6000)
                    {
                        d.GivingZone = "Zone 4";
                    }
                    else if (givingZoneAmt < 12000)
                    {
                        d.GivingZone = "Zone 5";
                    }
                    else if (givingZoneAmt < 20000)
                    {
                        d.GivingZone = "Zone 6";
                    }
                    else if (givingZoneAmt < 50000)
                    {
                        d.GivingZone = "Zone 7";
                    }
                    else if (givingZoneAmt < 100000)
                    {
                        d.GivingZone = "Zone 8";
                    }
                    else if (givingZoneAmt < 1000000)
                    {
                        d.GivingZone = "Zone 9";
                    }
                    else
                    {
                        d.GivingZone = "Zone 10";
                    }

                    //d.GivingZone = p.AttributeValues["GivingZone"].ValueFormatted;
                    //d.Average = p.AttributeValues["AvgDaysBetweenGifts"].ValueFormatted;
                    //d.StdDev = p.AttributeValues["StdDevFromAvg"].ValueFormatted;
                    var personEntity = EntityTypeCache.Get( "Rock.Model.Person" );
                    var vals = avSvc.Queryable().Where( av => av.Attribute.EntityTypeId == personEntity.Id && av.EntityId == p.Id ).ToList();
                    if (vals.FirstOrDefault( av => av.Attribute.Key == "AvgDaysBetweenGifts" ) != null)
                    {
                        d.Average = vals.FirstOrDefault( av => av.Attribute.Key == "AvgDaysBetweenGifts" ).ValueFormatted;
                    }
                    if (vals.FirstOrDefault( av => av.Attribute.Key == "StdDevFromAvg" ) != null)
                    {
                        d.StdDev = vals.FirstOrDefault( av => av.Attribute.Key == "StdDevFromAvg" ).ValueFormatted;
                    }
                    //If Source is empty for Current Transactions, Try Previous Transactions
                    if (String.IsNullOrEmpty( d.Source ))
                    {
                        d.Source = joinedData[i].PreviousTransactions != null ? string.Join( ",", joinedData[i].PreviousTransactions.Select( ft => ft.SourceTypeValue.Value ).Distinct() ) : string.Join( ",", allTransactions.Select( ft => ft.SourceTypeValue.Value ).Distinct() );
                    }
                    d.FirstGiftEver = allTransactions.First().TransactionDateTime;
                    var firstFundGift = allTransactions.Where( ft => ft.TransactionDetails.Any( ftd => ftd.AccountId == fund.Id ) ).OrderBy( ft => ft.TransactionDateTime ).First().TransactionDateTime;
                    var giftsBeforeStart = allTransactions.Where( ft => ft.TransactionDetails.Any( ftd => ftd.AccountId == fund.Id ) && DateTime.Compare( ft.TransactionDateTime.Value, start.Value ) < 0 ).OrderByDescending( ft => ft.TransactionDateTime );
                    DateTime? mostRecentFundGiftBeforeStart = null;
                    if (giftsBeforeStart.Count() > 0)
                    {
                        mostRecentFundGiftBeforeStart = giftsBeforeStart.First().TransactionDateTime;
                    }
                    d.MostRecentGift = mostRecentFundGiftBeforeStart;
                    //Determine Type of Donor
                    if (DateTime.Compare( firstFundGift.Value, start.Value ) < 0 && mostRecentFundGiftBeforeStart.HasValue && mostRecentFundGiftBeforeStart.Value.Year >= (start.Value.Year - 8))
                    {
                        //They have donated before this timeframe
                        if (mostRecentFundGiftBeforeStart.Value.Year > (start.Value.Year - 3))
                        {
                            d.DonorType = "Existing";
                        }
                        else
                        {
                            d.DonorType = "Re-Engaged";
                        }
                        var currentTransactions = allTransactions.Where( ft => DateTime.Compare( start.Value, ft.TransactionDateTime.Value ) <= 0 && DateTime.Compare( end.Value, ft.TransactionDateTime.Value ) >= 0 && ft.TransactionDetails.Any( ftd => ftd.AccountId == fund.Id ) ).ToList();
                        if (currentTransactions.Count() == 0)
                        {
                            //They have donated previously but not in this time frame 
                            d.DonorType = "Lapse";
                        }
                    }
                    else
                    {
                        //This timeframe is the first time they have donated, or it has been more than 8 years since their last gift
                        d.DonorType = "New";
                    }
                    //Figure out what most recent gift was if it was not to the Selected Fund
                    if (mostRecentFundGiftBeforeStart == null)
                    {
                        var mostRecent = allTransactions.Where( ft => DateTime.Compare( ft.TransactionDateTime.Value, start.Value ) < 0 ).OrderByDescending( ft => ft.TransactionDateTime );
                        if (mostRecent.Count() > 0)
                        {
                            d.MostRecentGiftAnyFund = mostRecent.First().TransactionDateTime;
                            d.MostRecentFund = String.Join( ",", mostRecent.First().TransactionDetails.Select( td => td.Account.Name ) );
                            d.MostRecentFundAmount = mostRecent.First().TransactionDetails.Sum( td => td.Amount );
                        }
                    }
                    else
                    {
                        d.MostRecentGiftAnyFund = mostRecentFundGiftBeforeStart;
                        d.MostRecentFund = fund.Name;
                        d.MostRecentFundAmount = giftsBeforeStart.First().TransactionDetails.Where( td => td.AccountId == fund.Id ).Sum( td => td.Amount );
                    }
                    if (d.PreviousAmountGiven > 0 || d.AmountGiven > 0)
                    {
                        donorData.Add( d );
                    }
                }
                return donorData;
            } catch(Exception ex)
            {
                var x = 7;
                return new List<DonorData>();
            }
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
            public string Address { get; set; }
            public decimal AmountGiven { get; set; }
            public int NumberOfGifts { get; set; }
            public decimal AverageGiftAmount { get; set; }
            public int? PreviousNumberOfGifts { get; set; }
            public decimal? PreviousAverageGiftAmount { get; set; }
            public decimal PreviousAmountGiven { get; set; }
            public decimal AmountChange { get; set; }
            public DateTime? FirstGiftEver { get; set; }
            public DateTime? MostRecentGift { get; set; }
            public DateTime? MostRecentGiftAnyFund { get; set; }
            public string MostRecentFund { get; set; }
            public decimal MostRecentFundAmount { get; set; }
            public string Source { get; set; }
            public string GivingZone { get; set; }
            public string DonorType { get; set; }
            public string GiverType { get; set; }
            public string Average { get; set; }
            public string StdDev { get; set; }
        }
        private class FinancialTransactionLight
        {
            public int Id { get; set; }
            public PersonAlias AuthorizedPersonAlias { get; set; }
            public ICollection<FinancialTransactionDetail> TransactionDetails { get; set; }
            public DateTime? TransactionDateTime { get; set; }
            public DefinedValue SourceTypeValue { get; set; }
        }
        private class JoinDataObj
        {
            public int? KeyId { get; set; }
            public IGrouping<int, FinancialTransactionLight> PreviousTransactions { get; set; }
            public IGrouping<int, FinancialTransactionLight> CurrentTransactions { get; set; }
            public string Type { get; set; }
        }

        protected void btnExport_Click( object sender, EventArgs e )
        {
            start = pkrStart.SelectedDate;
            end = pkrEnd.SelectedDate;
            end = new DateTime( end.Value.Year, end.Value.Month, end.Value.Day, 23, 59, 59 );
            lystart = new DateTime( start.Value.Year - 1, start.Value.Month, start.Value.Day );
            lyend = new DateTime( end.Value.Year - 1, end.Value.Month, end.Value.Day, 23, 59, 59 );
            fund = new FinancialAccountService( context ).Get( Int32.Parse( pkrAcct.SelectedValue ) );
            if ( start.HasValue && end.HasValue && fund != null )
            {
                alertInfo.InnerText = "Your file is being prepared, you will recieve an email when it is ready.";
                alertInfo.Visible = true;

                Task.Run( () =>
                {
                    ExcelPackage excel = new ExcelPackage();
                    excel.Workbook.Properties.Title = "Comparative Giving Report";
                    // add author info
                    Rock.Model.UserLogin userLogin = Rock.Model.UserLoginService.GetCurrentUser();
                    if ( userLogin != null )
                    {
                        excel.Workbook.Properties.Author = userLogin.Person.FullName;
                    }
                    else
                    {
                        excel.Workbook.Properties.Author = "Rock";
                    }
                    ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add( "Report" );
                    worksheet.PrinterSettings.LeftMargin = .5m;
                    worksheet.PrinterSettings.RightMargin = .5m;
                    worksheet.PrinterSettings.TopMargin = .5m;
                    worksheet.PrinterSettings.BottomMargin = .5m;

                    var headers = new List<string> { "Household", "Address", "Amount Given", "Number of Gifts", "Average Gift Amount", "Previous Amount Given", "Previous Number of Gifts", "Previous Average Gift Amount", "Change", "First Gift", "Most Recent Gift", "Most Recent Fund", "Most Recent Fund Gift Amount", "Giving Zone", "Type of Donor", "Average Days Between Gifts", "Standard Deviation From Average", "Source" };
                    var h = 1;
                    var row = 2;
                    foreach ( var header in headers )
                    {
                        worksheet.Cells[1, h].Value = header;
                        h++;
                    }

                    var data = GenerateData();

                    for ( var i = 0; i < data.Count(); i++ )
                    {
                        worksheet.Cells[row, 1].Value = data[i].HouseholdName;
                        worksheet.Cells[row, 2].Value = data[i].Address;
                        worksheet.Cells[row, 3].Value = data[i].AmountGiven;
                        worksheet.Cells[row, 4].Value = data[i].NumberOfGifts;
                        worksheet.Cells[row, 5].Value = data[i].AverageGiftAmount;
                        worksheet.Cells[row, 6].Value = data[i].PreviousAmountGiven;
                        worksheet.Cells[row, 7].Value = data[i].PreviousNumberOfGifts;
                        worksheet.Cells[row, 8].Value = data[i].PreviousAverageGiftAmount;
                        worksheet.Cells[row, 9].Value = data[i].AmountChange;
                        worksheet.Cells[row, 10].Value = data[i].FirstGiftEver.Value.ToString( "MM/dd/yyyy" );
                        worksheet.Cells[row, 11].Value = data[i].MostRecentGift.HasValue ? data[i].MostRecentGift.Value.ToString( "MM/dd/yyyy" ) : "";
                        worksheet.Cells[row, 12].Value = data[i].MostRecentFund;
                        worksheet.Cells[row, 13].Value = data[i].MostRecentFundAmount;
                        worksheet.Cells[row, 14].Value = data[i].GivingZone;
                        worksheet.Cells[row, 15].Value = data[i].DonorType;
                        worksheet.Cells[row, 16].Value = data[i].Average;
                        worksheet.Cells[row, 17].Value = data[i].StdDev;
                        worksheet.Cells[row, 18].Value = data[i].Source;
                        row++;
                    }
                    byte[] sheetbytes = excel.GetAsByteArray();
                    BinaryFile file = new BinaryFile()
                    {
                        FileName = "Comparative_Giving_Report.xlsx",
                        MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        BinaryFileTypeId = 14,
                        IsTemporary = true
                    };
                    file.FileSize = sheetbytes.Length;
                    file.ContentStream = new MemoryStream( sheetbytes );
                    new BinaryFileService( context ).Add( file );
                    context.SaveChanges();
                    //string path = AppDomain.CurrentDomain.BaseDirectory + "\\Content\\Finance\\Comparative_Giving_Report.xlsx";
                    //System.IO.File.WriteAllBytes( path, sheetbytes );

                    //Notify file is ready
                    Guid? guid = GetAttributeValue( "Person" ).AsGuidOrNull();
                    if ( guid.HasValue )
                    {
                        Person p = new PersonAliasService( context ).Get( guid.Value ).Person;
                        RockEmailMessage email = new RockEmailMessage();
                        RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( p, new Dictionary<string, object>() );
                        email.AddRecipient( recipient );
                        email.Subject = "Comparative Giving Report is Ready";
                        email.Message = "Here is your comparative giving report for " + start.Value.ToString("d") + " to " + end.Value.ToString("d") + ".";
                        email.FromEmail = "system@thecrossingchurch.com";
                        email.FromName = "The Crossing System";
                        email.CreateCommunicationRecord = true;
                        email.Attachments.Add(file);
                        List<string> errorMessages = new List<string>();
                        var output = email.Send(out errorMessages);
                        if(!output)
                        {
                            Exception ex = new Exception( "Error in CGR:\n" + String.Join( "\n", errorMessages ) );
                            ExceptionLogService.LogException( ex, HttpContext.Current );
                        }
                    }
                } );
            }
        }

        protected void btnGenerate_Click( object sender, EventArgs e )
        {
            start = pkrStart.SelectedDate;
            end = pkrEnd.SelectedDate;
            lystart = new DateTime( start.Value.Year - 1, start.Value.Month, start.Value.Day );
            lyend = new DateTime( end.Value.Year - 1, end.Value.Month, end.Value.Day );
            fund = new FinancialAccountService( context ).Get( Int32.Parse( pkrAcct.SelectedValue ) );
            if ( start.HasValue && end.HasValue && fund != null )
            {
                BindGrid();
            }
        }
    }

}