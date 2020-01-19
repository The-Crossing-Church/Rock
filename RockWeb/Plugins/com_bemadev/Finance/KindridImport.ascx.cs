using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using CsvHelper;
using System.IO;
using System.Text;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Web;
using System.Globalization;

namespace RockWeb.Plugins.com_bemaservices.Finance
{
    [DisplayName( "Kindrid Import" )]
    [Category( "BEMA Services > Finance" )]
    [Description( "Allows you to upload a CSV from Kindrid and import all the Financial Transactions" )]
    [LinkedPage( "Batch Detail", "Page that displays the contents of a batch", true, Rock.SystemGuid.Page.FINANCIAL_BATCH_DETAIL )]
    [AccountField( "Default Account", "Default Financial Account that will be used when a match is not found or account is not specified.", true, Rock.SystemGuid.FinancialAccount.GENERAL_FUND, "Configuration", 0 )]
    [FinancialGatewayField( "Financial Gateway", "Financial Gateway all Kindrid transaction need to be tied to.", false, "", "Configuration",1 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE, "Transaction Type", "Transaction Type Kindrind Transactions will be imported with.", true, false, Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION, "Configuration", 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, "Currency Type", "Currency Type transaction will be imported with", true, false, Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_UNKNOWN, "Configuration", 3 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Transaction Source", "Source transaction will be imported with", true, false, Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_MOBILE_APPLICATION, "Configuration", 4 )]
    [BooleanField( "CSV Has Header Row", "Does the CSV file being uploaded have a header row with column names?", false, "Configuration", 5)]
    public partial class KindridImport : RockBlock
    {
        private List<KindridTransaction> kindridTransactions = new List<KindridTransaction>();
        private int _financialGatewayId;
        private int _financialTransactionTypeId;
        private int _financialTransactionSourceId;
        private int _currencyTypeId;
        public int _defaultAccountId;
        private decimal _totalAmount;
        private int _totalTransactions;
        public int _batchId;
        private int _matchedImports;
        private int _nonmatchedImports;
        private int _notImported;
        private List<KindridTransaction> _notImportedList = new List<KindridTransaction>();

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            LoadBlockAttributes();
        }

        protected void LoadBlockAttributes()
        {
            RockContext rockContext = new RockContext();

            // Getting Financial Gateway Attribute
            if ( GetAttributeValue( "FinancialGateway" ).AsGuidOrNull() != null )
            {
                FinancialGatewayService financialGatewayService = new FinancialGatewayService( rockContext );
                financialGatewayService.Queryable();
                _financialGatewayId = financialGatewayService.Get( GetAttributeValue( "FinancialGateway" ).AsGuid() ).Id;
            }
            else
            {
                pEntry.Visible = false;
                pConfirmation.Visible = false;
                lMessages.Text = @"<div class='alert alert-warning'>Financial Gateway <strong>MUST</strong> be configured via Block Attributes.</div>";
            }

            // Getting Currency Type Attribute
            if ( GetAttributeValue( "CurrencyType" ).AsGuidOrNull() != null )
            {
                _currencyTypeId = DefinedValueCache.Get( GetAttributeValue( "CurrencyType" ).ToString() ).Id;
            }
            else
            {
                pEntry.Visible = false;
                pConfirmation.Visible = false;
                lMessages.Text = @"<div class='alert alert-warning'>Currency Type <strong>MUST</strong> be configured via Block Attributess.</div>";
            }

            // Getting Default Account Attribute
            if ( GetAttributeValue( "DefaultAccount" ).AsGuidOrNull() != null )
            {
                FinancialAccountService financialAccountService = new FinancialAccountService( rockContext );
                _defaultAccountId = financialAccountService.Get( GetAttributeValue( "DefaultAccount" ).AsGuid() ).Id;
            }
            else
            {
                pEntry.Visible = false;
                pConfirmation.Visible = false;
                lMessages.Text = @"<div class='alert alert-warning'>Default Account <strong>MUST</strong> be configured via Block Attributess.</div>";
            }

            // Getting Transaction Type Attribute
            if ( GetAttributeValue( "TransactionType" ).AsGuidOrNull() != null )
            {
                _financialTransactionTypeId = DefinedValueCache.Get( GetAttributeValue( "TransactionType" ).AsGuid() ).Id;
            }

            // Getting Transaction Source Attribute
            if ( GetAttributeValue( "TransactionSource" ).AsGuidOrNull() != null )
            {
                _financialTransactionSourceId = DefinedValueCache.Get( GetAttributeValue( "TransactionSource" ).AsGuid() ).Id;
            }
        }

        /// <summary>
        /// Handles file uploads of CSV files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void FileUpload_FileUploaded( object sender, Rock.Web.UI.Controls.FileUploaderEventArgs e )
        {
            // Reload Block Attributes
            LoadBlockAttributes();

            BinaryFile binaryFile = LoadFileFromService();

            if ( binaryFile != null )
            {
                if ( !binaryFile.FileName.EndsWith( "csv" ) )
                {
                    pEntry.Visible = false;
                    pConfirmation.Visible = false;

                    // File is not a CSV
                    lMessages.Text = @"<div class='alert alert-warning'>Uploaded file must be a CSV.</div>";
                }
            }
        }

        /// <summary>
        /// Load the file from the Rock file service
        /// </summary>
        private BinaryFile LoadFileFromService()
        {
            RockContext rockContext = new RockContext();

            var binaryFileService = new BinaryFileService( rockContext );
            BinaryFile binaryFile = null;

            if ( FileUpload.BinaryFileId.HasValue )
            {
                binaryFile = binaryFileService.Get( FileUpload.BinaryFileId.Value );
            }

            if ( binaryFile != null )
            {
                if ( binaryFile.BinaryFileTypeId.HasValue )
                {
                    binaryFile.BinaryFileType = new BinaryFileTypeService( rockContext ).Get( binaryFile.BinaryFileTypeId.Value );

                    ConvertCSVIntoList( binaryFile, rockContext );
                }
            }
            else
            {
                lMessages.Text = @"<div class='alert alert-warning'>File appears to be blank.</div>";
            }

            return binaryFile;
        }

        private void ConvertCSVIntoList( BinaryFile binaryFile, RockContext rockContext )
        {
            var csvContents = binaryFile.ContentStream;
            TextReader textReader = new StreamReader( csvContents );

            using ( var csv = new CsvReader( textReader ) )
            {
                // Configuring Model
                csv.Configuration.RegisterClassMap<KindridTransactionsMap>();
                csv.Configuration.WillThrowOnMissingField = false;

		        if ( GetAttributeValue("CSVHasHeaderRow").AsBoolean() )
		        {
                            csv.Configuration.HasHeaderRecord = true;
		        }
		        else
		        {
                            csv.Configuration.HasHeaderRecord = false;
		        }

                // adding conversion options.  These options will allow converter to process dollar amounts 
                //  with commas ($2,000.00) without failing.
                TypeConverterOptions options = new TypeConverterOptions();
                options.NumberStyle = NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
                TypeConverterOptionsFactory.AddOptions( typeof( decimal ), options );

                try
                {
                    kindridTransactions.AddRange( csv.GetRecords<KindridTransaction>().OrderByDescending( t => t.Date ).ToList() );

                    // Validating all transactions
                    kindridTransactions = ValidateTransactions( kindridTransactions, rockContext );

                    rTransactions.DataSource = kindridTransactions;
                    rTransactions.DataBind();

                    // Hiding Panels
                    pEntry.Visible = false;
                    pConfirmation.Visible = true;
                }
                catch ( Exception ex )
                {
                    lMessages.Text = @"<div class='alert alert-warning'>There was a problem parsing the CSV. Please validate the contents and try again.<br/>" + ex.Message + "</div>";
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( ex, context2 );
                }
            }
        }

        /// <summary>
        /// Checks to see if transactions were previously imported or match existing people
        /// </summary>
        /// <param name="transactions">List of Kindrid Transactions</param>
        /// <param name="rockContext">A rockContext to use for querying the DB</param>
        private List<KindridTransaction> ValidateTransactions( List<KindridTransaction> transactions, RockContext rockContext )
        {
            FinancialTransactionService financialTransactionService = new FinancialTransactionService( rockContext );
            PersonService personService = new PersonService( rockContext );
            financialTransactionService.Queryable();
            personService.Queryable();

            _totalTransactions = transactions.Count;

            for ( var i = 0; i < transactions.Count; i++ )
            {
                // The id column of the csv file contains a "tab" at the end and will need to be removed
                char tab = '\u0009'; // tab character
                transactions[i].Id = transactions[i].Id.Replace( tab.ToString(), "" );

                // Checking to see if transaction was already imported
                if ( financialTransactionService.GetByTransactionCode( _financialGatewayId, transactions[i].Id.Trim() ) != null )
                {
                    transactions[i].PreviouslyImported = true;
                    transactions[i].CurrentStatus = "Previously Imported";
                }
                else
                {
                    transactions[i].PreviouslyImported = false;
                    transactions[i].CurrentStatus = "Person Not Matched";

                    // Check to see if person has a transaction with matching Donor Id
                    string donorId = "Kindred_" + transactions[i].DonorId;
                    int? personAliasId = financialTransactionService.Queryable()
                                                                    .Where( t => t.ForeignKey == donorId && t.AuthorizedPersonAliasId != null )
                                                                    .OrderByDescending( t => t.CreatedDateTime )
                                                                    .Select( t => t.AuthorizedPersonAliasId )
                                                                    .FirstOrDefault();
                        
                    if ( personAliasId.HasValue )
                    {
                        transactions[i].RockPersonAliasId = personAliasId.Value;
                        transactions[i].CurrentStatus = "Person Matched (Donor Id)";
                    }

                    // Checking to see if person with same name exists
                    if ( transactions[i].CurrentStatus != "Person Matched (Donor Id)" )
                    {
                        List<PersonName> personNames = ParseName( transactions[i].Name );
                        foreach ( var person in personNames )
                        {
                            var personRecords = personService.GetByFirstLastName( person.FirstName, person.LastName, false, false ).ToList();
                            if ( personRecords.Any() )
                            {
                                // Checking for Address Matches
                                var addressMatches = personRecords.Where( x => x.GetHomeLocation( rockContext ) != null &&
                                                                             x.GetHomeLocation( rockContext ).Street1 == transactions[i].DonorAddress ).ToList();

                                if ( addressMatches.Count == 1 )
                                {
                                    transactions[i].RockPersonAliasId = addressMatches.First().PrimaryAliasId.Value;
                                    transactions[i].CurrentStatus = "Person Matched (Name & Address)";
                                }
                            }
                        }
                    }
                }

                // Set Account/Name if not present
                if ( transactions[i].FundCode != null && transactions[i].FundCode != "" )
                {
                    var account = new FinancialAccountService ( rockContext ).Get ( Convert.ToInt32 ( transactions[i].FundCode ) );
                    if ( account != null )
                    {
                        transactions[i].AccountName = account.Name;
                    }
                }
                if ( transactions[i].FundCode == null || transactions[i].FundCode == "" || transactions[i].AccountName == null || transactions[i].AccountName == "" )
                {
                    transactions[i].FundCode = _defaultAccountId.ToString ();
                    transactions[i].AccountName = new FinancialAccountService ( rockContext ).Get (_defaultAccountId ).Name;
                }
            }

            return transactions;
        }


        /// <summary>
        /// Parses Name string from kindrid file and converts into list on name(s)
        /// </summary>
        /// <param name="nameString">The string of names</param>
        private List<PersonName> ParseName( string nameString )
        {
            List<PersonName> personNames = new List<PersonName>();

            // Does personName contain two people
            if ( nameString.Contains( "&" ) || nameString.Contains( " and " ) )
            {
                string[] parts = null;

                if ( nameString.Contains( "&" ) )
                {
                    parts = nameString.Split( new string[] { " & " }, StringSplitOptions.None );
                }
                else if ( nameString.Contains( " and " ) )
                {
                    parts = nameString.Split( new string[] { " and " }, StringSplitOptions.None );
                }
                else
                {
                    throw new Exception( "Donor Name is formatted in an unapproved format." );
                }

                foreach ( var part in parts )
                {
                    PersonName personName = new PersonName();

                    // Finding first space
                    var space = part.IndexOf( ' ' );
                    if ( space > 0 )
                    {
                        personName.FirstName = part.Substring( 0, space );

                        // Finding last space
                        space = part.LastIndexOf( ' ' );
                        personName.LastName = part.Substring( space + 1, part.Count() - space - 1 );

                        // Pushing into list
                        personNames.Add( personName );
                    }
                }

                return personNames;
            }
            else
            {
                PersonName personName = new PersonName();

                // Finding first space
                var space = nameString.IndexOf( ' ' );
                personName.FirstName = nameString.Substring( 0, space );

                // Finding last space
                space = nameString.LastIndexOf( ' ' );
                personName.LastName = nameString.Substring( space + 1, nameString.Count() - space - 1 );

                // Pushing into list
                personNames.Add( personName );

                return personNames;
            }

        }

        /// <summary>
        /// Handles the Click event of the btnImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnImport_Click( object sender, EventArgs e )
        {
            // Resetting Total
            _totalAmount = 0.0M;
            _matchedImports = 0;
            _nonmatchedImports = 0;
            _notImported = 0;

            // Creating context and services
            using ( var rockContext = new RockContext() )
            {
                // Re-Loading transactions from Birnary Service
                BinaryFile binaryFile = LoadFileFromService();
                LoadBlockAttributes();

                var importableTransactions = kindridTransactions.Where( x => x.PreviouslyImported == false ).Count();
                if ( importableTransactions > 0 )
                {
                    FinancialBatchService financialBatchService = new FinancialBatchService( rockContext );

                    // Creating Batch
                    FinancialBatch financialBatch = new FinancialBatch();
                    financialBatch.BatchStartDateTime = DateTime.Today;
                    financialBatch.BatchEndDateTime = DateTime.Today.AddHours( 23 ).AddMinutes( 59 );
                    financialBatch.ControlAmount = 0;
                    financialBatch.CreatedByPersonAliasId = CurrentPersonAliasId;
                    financialBatch.CreatedDateTime = DateTime.Now;
                    financialBatch.Name = string.Format( "Kindrid Giving - {0}", DateTime.Now.ToString( "MM/dd/yyyy HH:mm" ) );

                    // Saving Batch
                    financialBatchService.Add( financialBatch );
                    rockContext.SaveChanges();

                    _batchId = financialBatch.Id;

                    // Looping through all transactions
                    foreach ( var transaction in kindridTransactions )
                    {
                        ProcessStatus status = CreateTransaction( transaction, _batchId );

                        switch ( status )
                        {
                            case ProcessStatus.Matched:
                                _matchedImports++;
                                break;
                            case ProcessStatus.Unmatched:
                                _nonmatchedImports++;
                                break;
                            case ProcessStatus.NotImported:
                                _notImported++;
                                _notImportedList.Add ( transaction );
                                break;
                        }
                    }

                    // Getting Batch
                    var batch = financialBatchService.Get( _batchId );

                    // Updating batch Control Amount
                    batch.ControlAmount = _totalAmount;
                    rockContext.SaveChanges();

                    // Displaying results
                    SetResultsData();

                }
                else
                {
                    lMessages.Text = string.Format("<div class='row'><div class='alert alert-warning'>No Transactions to Import.<br/><strong>{0}</strong>/<strong>{0}</strong> transaction already imported</div></div>", kindridTransactions.Count );
                }

            }

        }

        /// <summary>
        /// Handles the creation of Financial Transactions
        /// </summary>
        /// <param name="transaction">The kindrid transaction object</param>
        /// <param name="batchId">The batch ID the transaction needs to be tied to</param>
        private ProcessStatus CreateTransaction( KindridTransaction transaction, int batchId )
        {
            ProcessStatus status;

            using ( var rockContext = new RockContext() )
            {
                if ( !transaction.PreviouslyImported )
                {
                    // Updating total counter
                    _totalAmount += transaction.Amount;

                    // Creating Transaction
                    var newTransaction = new FinancialTransaction
                    {
                        TransactionCode = transaction.Id,
                        TransactionDateTime = transaction.Date,
                        FinancialGatewayId = _financialGatewayId,
                        TransactionTypeValueId = _financialTransactionTypeId,
                        BatchId = batchId,
                        SourceTypeValueId = _financialTransactionSourceId,
                        ForeignKey = "Kindred_" + transaction.DonorId
                    };

                    // Creeating Payment Detail
                    var paymentDetail = new FinancialPaymentDetail
                    {
                        CreatedDateTime = transaction.Date,
                        ModifiedDateTime = transaction.Date,
                        CurrencyTypeValueId = _currencyTypeId
                    };

                    newTransaction.FinancialPaymentDetail = paymentDetail;

                    // Creating TransactionDetail
                    var newTransactionDetail = new FinancialTransactionDetail
                    {
                        Amount = transaction.Amount,
                        AccountId = Convert.ToInt32(transaction.FundCode),
                        CreatedDateTime = transaction.Date
                    };

                    // Adding Transaction Detail onto Transaction
                    newTransaction.TransactionDetails.Add( newTransactionDetail );

                    if ( transaction.RockPersonAliasId != null )
                    {
                        newTransaction.AuthorizedPersonAliasId = transaction.RockPersonAliasId;
                        status = ProcessStatus.Matched;
                    }
                    else
                    {
                        status = ProcessStatus.Unmatched;
                    }

                    newTransaction.Summary = string.Format( "Name: {0}\nAddress: {1}\nEmail: {2}\nPhone Number: {3}\nDonorID: {4}",
                                                   transaction.Name,
                                                   string.Format( "{0} {1}, {2} {3}",
                                                       transaction.DonorAddress,
                                                       transaction.DonorCity,
                                                       transaction.DonorState,
                                                       transaction.DonorZip
                                                   ),
                                                   transaction.DonorEmail,
                                                   PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), transaction.PhoneNumber ),
                                                   transaction.DonorId
                                               );

                    // Creating Transaction in DB
                    rockContext.FinancialTransactions.Add( newTransaction );
                    rockContext.SaveChanges();

                    return status;
                }
                else
                {
                    return ProcessStatus.NotImported;
                }
            }
        }

        /// <summary>
        /// Handles the updating of results screen
        /// </summary>
        private void SetResultsData()
        {
            string matchedWidth;
            string nonmatchedWidth;
            string existingWidth;

            // Building widths for progress bars
            if ( _matchedImports > 0 )
                matchedWidth = Math.Truncate( ( _matchedImports / ( decimal ) _totalTransactions ) * 100 ) + "%";
            else
                matchedWidth = "0%";
            if ( _nonmatchedImports > 0 )
                nonmatchedWidth = Math.Truncate( ( _nonmatchedImports / ( decimal ) _totalTransactions ) * 100 ) + "%";
            else
                nonmatchedWidth = "0%";
            if ( _notImported > 0 )
                existingWidth = Math.Truncate( ( _notImported / ( decimal ) _totalTransactions ) * 100 ) + "%";
            else
                existingWidth = "0%";

            // Updating Progress Bar Widths
            progMatching.Style.Add( "width", matchedWidth );
            progMatching.InnerText = matchedWidth;
            progNotMatching.Style.Add( "width", nonmatchedWidth );
            progNotMatching.InnerText = nonmatchedWidth;
            progExisting.Style.Add( "width", existingWidth );
            progExisting.InnerText = existingWidth;

            // Updating Ratios
            lMatchingRatio.InnerText = string.Format( "{0}/{1}", _matchedImports, _totalTransactions );
            lNonMatchingRatio.InnerText = string.Format( "{0}/{1}", _nonmatchedImports, _totalTransactions );
            lNotImportedRatio.InnerText = string.Format( "{0}/{1}", _notImported, _totalTransactions );

            // show transaction not imported         
            rNotImported.DataSource = _notImportedList;
            rNotImported.DataBind ();

            // Updating Text
            if ( GetAttributeValue( "BatchDetail" ).AsGuidOrNull() != null )
            {
                var pageId = PageCache.Get( GetAttributeValue( "BatchDetail" ).AsGuid() ).Id;
                lStatus.InnerHtml = string.Format( "<p>Below you will find the import statistics.");
                btnViewBatch.HRef = string.Format( "/page/{0}?batchId={1}", pageId.ToString(), _batchId );
                btnViewBatch.Visible = true;
            }
            else
            {
                btnViewBatch.Visible = false;
                lStatus.InnerHtml = "<p>Below you will find the import statistics.";
            }

            // Displaying Summary Panel
            pResults.Visible = true;
            pConfirmation.Visible = false;
        }

        private enum ProcessStatus
        {
            Matched = 0,
            Unmatched = 1,
            NotImported = 2
        }

    }

    public class KindridTransaction
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string DonorAddress { get; set; }
        public string DonorCity { get; set; }
        public string DonorState { get; set; }
        public string DonorZip { get; set; }
        public string DonorId { get; set; }
        public string DonorEmail { get; set; }
        public decimal Amount { get; set; }
        public string PhoneNumber { get; set; }
        public string FundCode { get; set; }
        public string AccountName { get; set; }
        public bool PreviouslyImported { get; set; }
        public int? RockPersonAliasId { get; set; }
        public string CurrentStatus { get; set; }
    }

    public sealed class KindridTransactionsMap : CsvClassMap<KindridTransaction>
    {
        public KindridTransactionsMap()
        {
            Map( m => m.Id ).Index( 0 );
            Map( m => m.Date ).Index( 1 );
            Map( m => m.Name ).Index( 2 );
            Map( m => m.DonorAddress ).Index( 3 );
            Map( m => m.DonorCity ).Index( 4 );
            Map( m => m.DonorState ).Index( 5 );
            Map( m => m.DonorZip ).Index( 6 );
            Map( m => m.DonorId ).Index( 7 );
            Map( m => m.DonorEmail ).Index( 8 );
            Map( m => m.Amount ).Index( 9 );
            Map( m => m.PhoneNumber ).Index( 10 );
            Map( m => m.FundCode ).Index( 11 );
        }
    }

    public class PersonName
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}