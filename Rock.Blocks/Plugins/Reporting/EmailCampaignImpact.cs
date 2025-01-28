using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.ViewModels.Utility;
using Rock.Model;
using Rock.Security;

namespace Rock.Blocks.Plugins.Reporting
{
    [DisplayName( "Email Campaign Impact" )]
    [Category( "Obsidian > Plugin > Reporting" )]
    [Description( "A report with stats on a set of emails to guage the impact of the campaign" )]
    [IconCssClass( "fa fa-envelope-open-text" )]

    #region Block Attributes
    [CustomDropdownListField(
        "Communication Status",
        Key = AttributeKey.CommunicationStatus,
        Description = "List of returned communications will be filtered to ones with the selected statuses",
        IsRequired = true,
        Category = "Filter Configuration",
        Order = 1,
        ListSource = "0^Transient,1^Draft,2^Pending Approval,3^Approved,4^Denied",
        DefaultValue = "3",
        IsEnhanced = true,
        FieldTypeAssembly = "Rock",
        FieldTypeClass = "Rock.Field.Types.SelectMultiFieldType"
    )]
    [IntegerField(
        "Minimum Recipient Count",
        Key = AttributeKey.MinimumRecipients,
        Description = "List of returned communications will be filtered to ones that have at least the number of recipients set here",
        IsRequired = true,
        DefaultIntegerValue = 2,
        Category = "Filter Configuration",
        Order = 2
    )]
    [KeyValueListField( "Transaction Bands",
        Key = AttributeKey.TransactionBands,
        Description = "Minimum (Exclusive) and Maximum (Inclusive) values for each grouping of financial transactions",
        IsRequired = false,
        Category = "Filter Configuration",
        Order = 3
    )]
    #endregion Block Attributes

    public class EmailCampaignImpact : RockObsidianBlockType
    {
        #region Keys

        /// <summary>
        /// Attribute Key
        /// </summary>
        private static class AttributeKey
        {
            public const string CommunicationStatus = "CommunicationStatus";
            public const string MinimumRecipients = "MinimumRecipients";
            public const string TransactionBands = "TransactionBands";
        }

        #endregion

        public override object GetObsidianBlockInitialization()
        {
            EmailCampaignImpactViewModel viewModel = new EmailCampaignImpactViewModel();
            viewModel.communications = GetCommunicationOptions();
            viewModel.accounts = GetAuthorizedAccounts();
            return viewModel;
        }

        #region Internal Methods
        private List<ListItemBag> GetCommunicationOptions()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                List<ListItemBag> items = new List<ListItemBag>();
                Person p = RequestContext.CurrentPerson;
                CommunicationService comm_svc = new CommunicationService( rockContext );
                List<int> filterStatuses = GetAttributeValues( AttributeKey.CommunicationStatus ).Select( s => Int32.Parse( s ) ).ToList();
                int minRecipients = GetAttributeValue( AttributeKey.MinimumRecipients ).AsInteger();
                items = comm_svc.Queryable()
                    .Where( c => filterStatuses.Contains( ( int ) c.Status ) && c.Recipients.Count() >= minRecipients && c.SendDateTime <= RockDateTime.Now && c.CommunicationType == CommunicationType.Email )
                    .WherePersonAuthorizedToView( rockContext, p )
                    .OrderByDescending( c => c.SendDateTime )
                    .ToList()
                    .Select( c => new ListItemBag() { Text = c.Subject + " (" + c.SendDateTime.Value.ToString( "M/d/yy" ) + ")", Value = c.Id.ToString() } ).ToList();
                return items;
            }
        }

        private List<TreeItemBag> GetAuthorizedAccounts()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                FinancialAccountService fa_svc = new FinancialAccountService( rockContext );
                Person p = RequestContext.CurrentPerson;
                List<FinancialAccount> accounts = fa_svc.Queryable().ToList().Where( fa => fa.IsAuthorized( Authorization.VIEW, p ) ).ToList();
                return accounts.Where( fa => !fa.ParentAccountId.HasValue ).Select( fa =>
                {
                    return BuildItems( fa, accounts );
                } ).ToList();
            }
        }
        private TreeItemBag BuildItems( FinancialAccount account, List<FinancialAccount> allAccounts )
        {
            var item = new TreeItemBag()
            {
                Value = account.Guid.ToString(),
                Text = account.Name,
                IsActive = account.IsActive,
                HasChildren = allAccounts.Any( cfa => cfa.ParentAccountId == account.Id )
            };
            if ( item.HasChildren )
            {
                item.Children = allAccounts.Where( fa => fa.ParentAccountId == account.Id ).Select( fa => { return BuildItems( fa, allAccounts ); } ).ToList();
            }
            return item;

        }
        private int FindBand( List<Band> bands, decimal amount )
        {
            for ( int i = 0; i < bands.Count; i++ )
            {
                if ( amount > bands[i].LowerLimit && amount <= bands[i].UpperLimit )
                {
                    return i + 1;
                }
            }
            throw new Exception( "Unable to Match Band" );
        }
        #endregion

        #region Block Actions
        [BlockAction]
        public BlockActionResult GenerateReport( List<int> commIds, Guid? groupGuid, List<Guid> accountGuids, List<Guid> transactionTypeGuids, DateTime? transactionEndDate )
        {
            try
            {
                using ( RockContext rockContext = new RockContext() )
                {
                    PersonService person_svc = new PersonService( rockContext );
                    PersonAliasService pa_svc = new PersonAliasService( rockContext );
                    CommunicationService comm_svc = new CommunicationService( rockContext );
                    CommunicationRecipientService cr_svc = new CommunicationRecipientService( rockContext );
                    GroupService group_svc = new GroupService( rockContext );
                    GroupMemberService gm_svc = new GroupMemberService( rockContext );
                    InteractionChannelService ichannel_svc = new InteractionChannelService( rockContext );
                    InteractionComponentService icomp_svc = new InteractionComponentService( rockContext );
                    InteractionService int_svc = new InteractionService( rockContext );
                    FinancialTransactionService ft_svc = new FinancialTransactionService( rockContext );
                    FinancialTransactionDetailService ftd_svc = new FinancialTransactionDetailService( rockContext ); ;
                    FinancialAccountService fa_svc = new FinancialAccountService( rockContext );
                    FinancialScheduledTransactionService fst_svc = new FinancialScheduledTransactionService( rockContext );
                    DefinedValueService dv_svc = new DefinedValueService( rockContext );

                    IQueryable<Model.Communication> commsQry = comm_svc.GetByIds( commIds ).OrderBy( c => c.SendDateTime );
                    List<Model.Communication> comms = commsQry.ToList();
                    InteractionChannel commChannel = ichannel_svc.Get( SystemGuid.InteractionChannel.COMMUNICATION );
                    IQueryable<InteractionComponent> interaction_components = icomp_svc.GetByChannelId( commChannel.Id ).Where( ic => commIds.Contains( ic.EntityId.Value ) );
                    List<int> icIds = interaction_components.Select( ic => ic.Id ).ToList();
                    IQueryable<Interaction> interactionsQry = int_svc.Queryable().Where( i => icIds.Contains( i.InteractionComponentId ) && i.Operation == "Click" );
                    IQueryable<CommunicationRecipient> recipientsOry = cr_svc.Queryable().Where( cr => commIds.Contains( cr.CommunicationId ) );

                    Result result = new Result()
                    {
                        comms = commsQry.Select( c => new CommunicationSummary()
                        {
                            Id = c.Id,
                            Subject = c.Subject,
                            SendDateTime = c.SendDateTime
                        } ).ToList(),
                        includeFinancial = false
                    };

                    var clicks = interactionsQry.Join( pa_svc.Queryable(),
                        i => i.PersonAliasId,
                        pa => pa.Id,
                        ( i, pa ) => new { interaction = i, alias = pa }
                    ).GroupBy( g => new { g.alias.PersonId, g.interaction.InteractionComponentId } )
                    .Select( g => new
                    {
                        g.Key.PersonId,
                        g.Key.InteractionComponentId,
                        EmailFirstClick = g.Min( i => i.interaction.InteractionDateTime ),
                        EmailLastClick = g.Max( i => i.interaction.InteractionDateTime ),
                        Count = g.Count()
                    } );

                    var emailData = from recipient in recipientsOry
                                    join click in clicks on recipient.PersonAlias.PersonId equals click.PersonId into clickJoin
                                    from clickData in clickJoin.DefaultIfEmpty()
                                    select new { recipient.PersonAlias.PersonId, recipient.OpenedDateTime, recipient.CommunicationId, clickData.InteractionComponentId, clickData.EmailFirstClick, clickData.EmailLastClick, clickData.Count };

                    List<FamilyGivingSummary> transactionsByFamily = new List<FamilyGivingSummary>();

                    var bands = GetAttributeValues( AttributeKey.TransactionBands ).Select( b => b.Split( '^' ) ).Select( b =>
                    {
                        decimal lower;
                        decimal upper;
                        if ( !Decimal.TryParse( b[0], out lower ) )
                        {
                            lower = 0;
                        }
                        if ( !Decimal.TryParse( b[1], out upper ) )
                        {
                            upper = Decimal.MaxValue;
                        }
                        return new Band() { LowerLimit = lower, UpperLimit = upper };
                    } ).ToList();

                    if ( ( accountGuids.Count > 0 || transactionTypeGuids.Count > 0 ) && bands.Count > 0 )
                    {
                        result.includeFinancial = true;

                        var familyGroupMembersQry = rockContext.Database.SqlQuery<AdditionalFamilyRelations>( $@"
                            SELECT GroupId AS PrimaryFamilyId, PersonId
                            FROM GroupMember
                            WHERE GroupTypeId = 10
                                AND IsArchived = 0
                                AND GroupMemberStatus = 1
                            UNION
                            SELECT DISTINCT PrimaryFamilyId, AdditionalId AS PersonId
                            FROM (
                                    SELECT Person.Id, PrimaryFamilyId, RelationId AS AdditionalId
                                    FROM _tcc_GivingRelevantKnownRelationships
                                            INNER JOIN Person ON OwnerId = Id
                                    UNION
                                    SELECT Person.Id, PrimaryFamilyId, OwnerId AS AdditionalId
                                    FROM _tcc_GivingRelevantKnownRelationships
                                            INNER JOIN Person ON RelationId = Id
                                    ) AS KnownRelations
                            WHERE PrimaryFamilyId IS NOT NULL
                        " ).ToList();

                        DateTime startDate = comms[0].SendDateTime.Value;
                        DateTime endDate = transactionEndDate.HasValue ? transactionEndDate.Value.EndOfDay() : RockDateTime.Now;
                        var accounts = fa_svc.Queryable().Where( fa => accountGuids.Contains( fa.Guid ) );
                        List<int> accountIds = accounts.Select( fa => fa.Id ).ToList();
                        var transactionTypes = dv_svc.Queryable().Where( dv => transactionTypeGuids.Contains( dv.Guid ) ).ToList();
                        List<int> transactionTypeIds = transactionTypes.Select( dv => dv.Id ).ToList();

                        var transactionsInRange = from ft in ft_svc.Queryable()
                                                  join pa in pa_svc.Queryable() on ft.AuthorizedPersonAliasId equals pa.Id
                                                  join ftd in ftd_svc.Queryable() on ft.Id equals ftd.TransactionId
                                                  join fa in fa_svc.Queryable() on ftd.AccountId equals fa.Id
                                                  join fst in fst_svc.Queryable() on ft.ScheduledTransactionId equals fst.Id into fstJoin
                                                  from fstData in fstJoin.DefaultIfEmpty()
                                                  select new
                                                  {
                                                      TransactionId = ft.Id,
                                                      ft.CreatedDateTime,
                                                      ft.TransactionDateTime,
                                                      ft.TransactionTypeValueId,
                                                      ft.AuthorizedPersonAliasId,
                                                      pa.PersonId,
                                                      ftd.AccountId,
                                                      fa.Name,
                                                      ftd.Amount,
                                                      ft.ScheduledTransactionId,
                                                      ScheduleCreatedOn = fstData.CreatedDateTime
                                                  };

                        transactionsInRange = transactionsInRange.Where( ft =>
                            ft.CreatedDateTime >= startDate &&
                            ft.CreatedDateTime <= endDate &&
                            ( !ft.ScheduledTransactionId.HasValue || ft.ScheduleCreatedOn >= startDate ) &&
                            ( accountIds.Count == 0 || accountIds.Contains( ft.AccountId ) ) &&
                            ( transactionTypeIds.Count == 0 || transactionTypeIds.Contains( ft.TransactionTypeValueId ) )
                        );

                        var transactionSummary = transactionsInRange.GroupBy( ft => new { ft.PersonId, ft.TransactionId, ft.TransactionDateTime } )
                            .Select( g => new
                            {
                                g.Key.PersonId,
                                g.Key.TransactionId,
                                g.Key.TransactionDateTime,
                                Amount = g.Sum( ft => ft.Amount ),
                                Accounts = g.Select( ft => ft.Name )
                            } ).ToList();

                        transactionsByFamily = transactionSummary.Join( familyGroupMembersQry,
                                ft => ft.PersonId,
                                gm => gm.PersonId,
                                ( ft, gm ) => new { gm.PrimaryFamilyId, ft.TransactionId, ft.Accounts, ft.Amount, ft.TransactionDateTime }
                            ).GroupBy( g => g.PrimaryFamilyId )
                            .Select( g => new FamilyGivingSummary()
                            {
                                PrimaryFamilyId = g.Key,
                                FirstTransactionDate = g.OrderBy( t => t.TransactionDateTime ).FirstOrDefault().TransactionDateTime,
                                FirstTransactionAccount = g.OrderBy( t => t.TransactionDateTime ).FirstOrDefault().Accounts.Distinct(),
                                FirstTransactionAmount = g.OrderBy( t => t.TransactionDateTime ).FirstOrDefault().Amount,
                                AllAccounts = g.SelectMany( t => t.Accounts ).Distinct(),
                                TotalAmount = g.Sum( ft => ft.Amount ),
                                NumberOfGifts = g.Count()
                            } ).ToList();
                    }

                    var data = emailData.GroupBy( d => d.PersonId )
                        .Select( grp => new ReportFields()
                        {
                            PersonId = grp.Key,
                            EmailQry = grp.Select( g => new EmailData()
                            {
                                CommunicationId = g.CommunicationId,
                                OpenedOn = g.OpenedDateTime,
                                FirstClick = g.EmailFirstClick,
                                LastClick = g.EmailLastClick,
                                TotalClicks = g.Count
                            } ).AsQueryable()
                        }
                    ).ToList();

                    var personQry = from cr in recipientsOry
                                    join pa in pa_svc.Queryable() on cr.PersonAliasId equals pa.Id
                                    join p in person_svc.Queryable() on pa.PersonId equals p.Id
                                    join cs in dv_svc.Queryable() on p.ConnectionStatusValueId equals cs.Id into csJoin
                                    from csData in csJoin.DefaultIfEmpty()
                                    join ms in dv_svc.Queryable() on p.MaritalStatusValueId equals ms.Id into msJoin
                                    from msData in msJoin.DefaultIfEmpty()
                                    select new { PersonId = p.Id, p.PrimaryFamilyId, p.FirstName, p.LastName, p.Age, p.Gender, MaritalStatus = msData.Value, ConnectionStatus = csData.Value, p.CommunicationPreference, p.EmailPreference, p.IsEmailActive };

                    var personData = personQry.Distinct().ToList();
                    List<int> sortedCommIds = comms.Select( c => c.Id ).ToList();

                    for ( int i = 0; i < data.Count(); i++ )
                    {
                        var person = personData.FirstOrDefault( p => p.PersonId == data[i].PersonId );
                        if ( person != null )
                        {
                            data[i].FirstName = person.FirstName;
                            data[i].LastName = person.LastName;
                            data[i].Age = person.Age;
                            data[i].Gender = person.Gender.ToString();
                            data[i].MaritalStatus = person.MaritalStatus;
                            data[i].ConnectionStatus = person.ConnectionStatus;
                            data[i].CommunicationPreference = person.CommunicationPreference.ToString();
                            data[i].EmailPreference = person.EmailPreference.ToString();
                            data[i].EmailIsActive = person.IsEmailActive;
                            data[i].PrimaryFamilyId = person.PrimaryFamilyId;
                        }
                        var transaction = transactionsByFamily.FirstOrDefault( ft => ft.PrimaryFamilyId == data[i].PrimaryFamilyId );
                        if ( transaction != null )
                        {
                            data[i].FirstTransactionDate = transaction.FirstTransactionDate;
                            data[i].FirstTransactionAccount = String.Join( ", ", transaction.FirstTransactionAccount );
                            data[i].FirstTransactionAmount = transaction.FirstTransactionAmount;
                            data[i].FirstTransactionBand = FindBand( bands, transaction.FirstTransactionAmount );
                            data[i].TotalGiven = transaction.TotalAmount;
                            data[i].TotalBand = FindBand( bands, transaction.TotalAmount );
                            data[i].NumberOfGifts = transaction.NumberOfGifts;
                        }
                    }

                    result.data = data;


                    if ( groupGuid.HasValue )
                    {
                        Model.Group grp = group_svc.Get( groupGuid.Value );
                        var people = gm_svc.Queryable().Where( gm => gm.GroupId == grp.Id ).Select( gm => gm.PersonId ).ToList();
                        result.data = result.data.Where( d => people.Contains( d.PersonId ) ).ToList();
                    }

                    return ActionOk( result );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                return ActionBadRequest( ex.Message );
            }
        }
        #endregion

        #region Helper Classes 
        private class EmailCampaignImpactViewModel
        {
            public List<ListItemBag> communications { get; set; }
            public List<TreeItemBag> accounts { get; set; }
        }

        private class CommunicationSummary
        {
            public int Id { get; set; }
            public string Subject { get; set; }
            public DateTime? SendDateTime { get; set; }
        }
        private class EmailData
        {
            public int? CommunicationId { get; set; }
            public DateTime? OpenedOn { get; set; }
            public DateTime? FirstClick { get; set; }
            public DateTime? LastClick { get; set; }
            public int? TotalClicks { get; set; }
        }

        private class FamilyGivingSummary
        {

            public int? PrimaryFamilyId { get; set; }
            public DateTime? FirstTransactionDate { get; set; }
            public IEnumerable<string> FirstTransactionAccount { get; set; }
            public Decimal FirstTransactionAmount { get; set; }
            public IEnumerable<string> AllAccounts { get; set; }
            public Decimal TotalAmount { get; set; }
            public int NumberOfGifts { get; set; }
        }

        private class ReportFields
        {
            public int PersonId { get; set; }
            public int? PrimaryFamilyId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int? Age { get; set; }
            public string Gender { get; set; }
            public string MaritalStatus { get; set; }
            public string ConnectionStatus { get; set; }
            public string CommunicationPreference { get; set; }
            public string EmailPreference { get; set; }
            public bool EmailIsActive { get; set; }
            public IQueryable<EmailData> EmailQry { get; set; }
            public DateTime? FirstTransactionDate { get; set; }
            public Decimal FirstTransactionAmount { get; set; }
            public int? FirstTransactionBand { get; set; }
            public string FirstTransactionAccount { get; set; }
            public Decimal TotalGiven { get; set; }
            public int? TotalBand { get; set; }
            public int? NumberOfGifts { get; set; }
        }

        private class Result
        {
            public List<ReportFields> data { get; set; }
            public bool includeFinancial { get; set; }
            public List<CommunicationSummary> comms { get; set; }
        }

        private class AdditionalFamilyRelations
        {
            public int? PrimaryFamilyId { get; set; }
            public int PersonId { get; set; }
        }

        private class Band
        {
            public decimal LowerLimit { get; set; }
            public decimal UpperLimit { get; set; }
        }
        #endregion
    }
}