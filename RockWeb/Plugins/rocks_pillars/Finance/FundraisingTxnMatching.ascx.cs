// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

/// <summary>
/// 
/// </summary>
namespace RockWeb.Plugins.rocks_pillars.Finance
{
    [DisplayName( "Fundraising Transaction Matching" )]
    [Category( "Pillars > Finance" )]
    [Description( "Used to assign fund raising transaction details to a specific group member. Optionally this can be limited to group members that a person has pledged to support." )]
    [LinkedPage("Transaction Detail Page")]
    [ContextAware( typeof( Person ))]
    public partial class FundraisingTxnMatching : RockBlock, ICustomGridColumns
    {
        const string FUNDRAISING_GROUP_ACCOUNT = "7C6FF01B-F68E-4A83-A96D-85071A92AAF1";

        private Person _person = null;
        private List<FinancialTransactionDetail> _transactionDetailList;
        private Dictionary<Guid, List<FundRaisingGroup>> _accountGroupMembers;
        private Dictionary<int, List<FundRaisingGroup>> _personFundRaisingGroupMembers;

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _person = this.ContextEntity<Person>();

            gTransactions.DataKeyNames = new string[] { "Id" };
            gTransactions.Actions.ShowAdd = false;
            gTransactions.GridRebind += GTransactions_GridRebind;
            gTransactions.RowDataBound += GTransactions_RowDataBound;
            gTransactions.RowSelected += GTransactions_RowSelected;
            gTransactions.IsDeleteEnabled = false;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                LoadDropDowns();

                if ( _person != null )
                {
                    BindData();
                }
            }
        }

        #endregion

        #region Events

        private void GTransactions_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            // ignore any row that is not a data row
            if ( e.Row.RowType != DataControlRowType.DataRow ) return;

            // Get the transaction detail row details and the person selection dropdown
            var rowItem = e.Row.DataItem as RowItem;
            var ddlGroupMember = e.Row.FindControl( "ddlGroupMember" ) as RockDropDownList;
            if ( rowItem == null || ddlGroupMember == null ) return;

            // Get the associated transaction detail item
            var txnDetail = _transactionDetailList.FirstOrDefault( d => d.Id == rowItem.Id );
            if ( txnDetail == null ) return;

            // Create a dictionary for unique group/member options
            var options = new Dictionary<int, string>();

            // Get all of the families person ids
            var familyPersonIds = txnDetail.Transaction.AuthorizedPersonAlias.Person.GetFamilyMembers( true ).Select( m => m.PersonId ).ToList();

            // Get list of group members for the selected account, first check to see if person has pledged to support specific individual(s) 
            // and if so limit to that list, otherwise show list of all group members for current transaction's account
            var groupMembers = _personFundRaisingGroupMembers.Where( b => familyPersonIds.Contains( b.Key ) ).Any() ?
                _personFundRaisingGroupMembers.Where( b => familyPersonIds.Contains( b.Key ) ).Select( b => b.Value ) :
                _accountGroupMembers.Where( b => b.Key == txnDetail.Account.Guid ).Select( b => b.Value );

            // Build list of unique options
            foreach ( var fundRaisingGroups in groupMembers )
            {
                foreach( var group in fundRaisingGroups )
                {
                    foreach( var member in group.Members )
                    {
                        string option = string.Format( "{0} ({1})", member.PersonName, group.GroupName );
                        options.AddOrIgnore( member.GroupMemberId, option );
                    }
                }
            }

            // Build the dropdown list options
            ddlGroupMember.Items.Clear();
            ddlGroupMember.Items.Add( new ListItem() );
            foreach( var option in options.OrderBy( o => o.Value ) )
            {
                ddlGroupMember.Items.Add( new ListItem( option.Value, option.Key.ToString() ) );
            }

            // If there were any options (group members), show the list and set the value
            if ( options.Any() )
            {
                ddlGroupMember.Visible = true;
                if ( rowItem.GroupMemberId.HasValue && options.ContainsKey( rowItem.GroupMemberId.Value ) )
                {
                    ddlGroupMember.SetValue( rowItem.GroupMemberId.Value );
                }
                else
                {
                    if ( options.Count == 1 )
                    {
                        ddlGroupMember.SetValue( options.First().Key );
                    }
                }
            }
            else
            {
                ddlGroupMember.Visible = false;
            }
        }

        private void GTransactions_RowSelected( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var txnDtl = new FinancialTransactionDetailService( rockContext ).Get( e.RowKeyId );
                if ( txnDtl != null )
                {
                    var personId = txnDtl.Transaction.AuthorizedPersonAlias.PersonId;
                    NavigateToLinkedPage( "TransactionDetailPage", new Dictionary<string, string> { { "TransactionId", txnDtl.TransactionId.ToString() } } );
                }
            }
        }

        private void GTransactions_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindData();
        }

        protected void lbFilter_Click( object sender, EventArgs e )
        {
            BindData();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            foreach ( GridViewRow row in gTransactions.Rows )
            {
                int txnDetailId = (int)gTransactions.DataKeys[row.RowIndex].Value;
                var ddlGroupMember = row.FindControl( "ddlGroupMember" ) as RockDropDownList;
                if ( ddlGroupMember != null && ddlGroupMember.Items.Count > 1 )
                {
                    AssignTransactionDetail( ddlGroupMember.SelectedValueAsInt(), txnDetailId );
                }
            }

            nbSaveSuccess.Visible = true;

            BindData();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            var attribute = AttributeCache.Get( FUNDRAISING_GROUP_ACCOUNT.AsGuid() );
            if ( attribute == null ) return;

            using ( var rockContext = new RockContext() )
            {
                // Get a dictionary of fund raising groups and their account guids
                var groupAccountGuids = new AttributeValueService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( v =>
                        v.AttributeId == attribute.Id &&
                        v.EntityId.HasValue &&
                        v.Value != null &&
                        v.Value != "" )
                    .ToDictionary( k => k.EntityId.Value, v => v.Value );

                // Of those, determine the groups that are active and not archived
                var activeGroups = new GroupService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( g =>
                        groupAccountGuids.Keys.Contains( g.Id ) &&
                        g.IsActive == true &&
                        g.IsArchived == false )
                    .Select( g => g.Id )
                    .ToList();
                var accountGuids = groupAccountGuids
                    .Where( g => activeGroups.Contains( g.Key ) )
                    .Select( g => g.Value )
                    .AsGuidList();

                // Get the active accounts associated with any active groups
                var accounts = new FinancialAccountService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a => 
                        accountGuids.Contains( a.Guid ) &&
                        a.IsActive == true )
                    .OrderBy( a => a.Name )
                    .Select( a => new
                    {
                        a.Id,
                        a.Name
                    } )
                    .ToList();

                ddlAccount.Items.Clear();
                ddlAccount.Items.Add( new ListItem() );
                foreach ( var account in accounts )
                {
                    ddlAccount.Items.Add( new ListItem( account.Name, account.Id.ToString() ) );
                }
            }

            pnlFilter.Visible = _person == null;

        }

        /// <summary>
        /// Creates the table controls.
        /// </summary>
        /// <param name="batchId">The batch identifier.</param>
        /// <param name="dataViewId">The data view identifier.</param>
        private void BindData()
        {
            using ( var rockContext = new RockContext() )
            {
                var attributeValueService = new AttributeValueService( rockContext );
                var groupService = new GroupService( rockContext );
                var memberService = new GroupMemberService( rockContext );

                // Determine the account(s) 
                var accountIds = new List<int>();
                if ( _person != null )
                {
                    foreach ( ListItem item in ddlAccount.Items )
                    {
                        var accountId = item.Value.AsIntegerOrNull();
                        if ( accountId.HasValue )
                        {
                            accountIds.Add( accountId.Value );
                        }
                    }
                }
                else
                {
                    var accountId = ddlAccount.SelectedValueAsInt();
                    if ( accountId.HasValue )
                    {
                        accountIds.Add( accountId.Value );
                    }
                }

                // Get any fundraising groups that use the selected account(s) and the group's active members
                _accountGroupMembers = new Dictionary<Guid, List<FundRaisingGroup>>();
                var attribute = AttributeCache.Get( FUNDRAISING_GROUP_ACCOUNT.AsGuid() );
                if ( attribute != null )
                {
                    foreach( Guid accountGuid in new FinancialAccountService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a => accountIds.Contains( a.Id ) )
                        .Select( a => a.Guid ) )
                    {
                        _accountGroupMembers.Add( accountGuid, new List<FundRaisingGroup>() );

                        string attributeValue = accountGuid.ToString();
                        foreach ( var groupId in attributeValueService
                            .Queryable().AsNoTracking()
                            .Where( v =>
                                v.AttributeId == attribute.Id &&
                                v.EntityId.HasValue &&
                                v.Value != null &&
                                v.Value == attributeValue )
                            .Select( v => v.EntityId.Value )
                            .ToList()
                            .Distinct() )
                        {
                            var group = groupService.Get( groupId );
                            if ( group != null && group.IsActive )
                            {
                                var accountGroup = new FundRaisingGroup();
                                accountGroup.GroupId = group.Id;
                                accountGroup.GroupName = group.Name;

                                accountGroup.Members = memberService
                                    .GetByGroupId( group.Id )
                                    .Where( m =>
                                        !m.IsArchived &&
                                        m.GroupMemberStatus == GroupMemberStatus.Active )
                                    .Select( m => new FundRaisingGroupMember
                                    {
                                        GroupMemberId = m.Id,
                                        PersonId = m.PersonId,
                                        PersonName = m.Person.NickName + " " + m.Person.LastName
                                    } )
                                    .ToList();

                                _accountGroupMembers[accountGuid].Add( accountGroup );
                            }
                        }
                    }
                }

                // Check to see if there are any pledges for the selected account(s)
                _personFundRaisingGroupMembers = new Dictionary<int, List<FundRaisingGroup>>();

                var pledgeEntityTypeId = EntityTypeCache.GetId<Rock.Model.FinancialPledge>();
                var personFieldType = FieldTypeCache.Get( Rock.SystemGuid.FieldType.PERSON.AsGuid() );
                if ( pledgeEntityTypeId != null && personFieldType != null )
                {
                    // Get a list of all pledges that have a person attribute value 
                    var pledgePeople = attributeValueService
                        .Queryable().AsNoTracking()
                        .Where( v =>
                            v.Attribute != null &&
                            v.Attribute.EntityTypeId == pledgeEntityTypeId.Value &&
                            v.Attribute.FieldTypeId == personFieldType.Id &&
                            v.EntityId.HasValue &&
                            v.ValueAsPersonId != null )
                        .Select( v => new
                        {
                            PledgeId = v.EntityId.Value,
                            PersonId = v.ValueAsPersonId.Value
                        } )
                        .ToDictionary( k => k.PledgeId, v => v.PersonId );

                    // Loop through each pledge that is active, and for the selected account(s)
                    var today = RockDateTime.Today;
                    foreach( var pledge in new FinancialPledgeService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p =>
                            p.AccountId.HasValue &&
                            accountIds.Contains( p.AccountId.Value ) &&
                            p.StartDate <= today &&
                            p.EndDate >= today )
                        .Select( p => new
                        {
                            AccountGuid = p.Account.Guid,
                            p.Id,
                            p.PersonAlias.PersonId
                        }) )
                    {
                        // Check to see if that pledge is one that had an associated person attribute value
                        if ( _accountGroupMembers.ContainsKey( pledge.AccountGuid ) && pledgePeople.ContainsKey( pledge.Id ) )
                        {
                            // if so, get the person value for that attribute (this is assumed to be a person that they pledged to support)
                            int pledgePersonId = pledgePeople[pledge.Id];

                            // Add the person who made the pledge to the dictionary of pledges
                            _personFundRaisingGroupMembers.AddOrIgnore( pledge.PersonId, new List<FundRaisingGroup>() );

                            // Loop through each of the fund raising group/members that match the pledge's person attribute value and add those group/members to a list
                            foreach ( var gm in _accountGroupMembers[pledge.AccountGuid].Where( g => g.Members.Any( m => m.PersonId == pledgePersonId ) ) )
                            {
                                _personFundRaisingGroupMembers[pledge.PersonId].Add( new FundRaisingGroup
                                {
                                    GroupId = gm.GroupId,
                                    GroupName = gm.GroupName,
                                    Members = gm.Members.Where( m => m.PersonId == pledgePersonId ).Select( m => new FundRaisingGroupMember
                                    {
                                        GroupMemberId = m.GroupMemberId,
                                        PersonId = m.PersonId,
                                        PersonName = m.PersonName
                                    } ).ToList()
                                } );
                            }
                        }
                    }
                }

                // Get all of the transaction details for the selected account(s)
                var txnDetailQry = new FinancialTransactionDetailService( rockContext )
                .Queryable().AsNoTracking()
                .Where( d => 
                    accountIds.Contains( d.AccountId ) &&
                    d.Account != null &&
                    d.Transaction != null &&
                    d.Transaction.Batch != null &&
                    d.Transaction.AuthorizedPersonAlias != null &&
                    d.Transaction.AuthorizedPersonAlias.Person != null );

                // check to see if filter has been set to only include unassigned or assigned transactions and if so add that criteria to the query
                int? assigned = ddlAssigned.SelectedValueAsInt();
                if ( assigned.HasValue && assigned.Value == 1 )
                {
                    txnDetailQry = txnDetailQry.Where( d => !d.EntityId.HasValue );
                }
                if ( assigned.HasValue && assigned.Value == 2 )
                {
                    txnDetailQry = txnDetailQry.Where( d => d.EntityId.HasValue );
                }

                var txnDetailEntityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.FINANCIAL_TRANSACTION_DETAIL.AsGuid() );

                if ( _person != null )
                {
                    if ( txnDetailEntityType != null )
                    {
                        var personAliasGuids = new PersonAliasService( rockContext ).Queryable().AsNoTracking()
                        .Where( pa => pa.Person.GivingLeaderId == _person.GivingLeaderId )
                        .Select( pa => pa.Guid )
                        .ToList()
                        .Select( g => g.ToString() )
                        .ToList();

                        var softCreditTxnDetailIdQry = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                            .Where( v =>
                                v.Attribute.EntityTypeId == txnDetailEntityType.Id &&
                                v.Attribute.Key == "SoftCreditFor" &&
                                personAliasGuids.Contains( v.Value ) )
                            .Select( v => v.EntityId )
                            .ToList();

                        // If viewing transactions for a specific person (on a profile tab), limit transactions to any for their giving id
                        var personIds = _person.GetFamilyMembers( true ).Select( m => m.PersonId ).ToList();
                        txnDetailQry = txnDetailQry.Where( t =>
                            t.Transaction.AuthorizedPersonAlias.Person.GivingLeaderId == _person.GivingLeaderId ||
                            softCreditTxnDetailIdQry.Contains( t.Id ) );
                    }
                }
                else
                {
                    // Otherwise limit transactions based on the selected date range
                    var dateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( drpDates.DelimitedValues );
                    if ( dateRange.Start.HasValue )
                    {
                        txnDetailQry = txnDetailQry.Where( a => a.Transaction.TransactionDateTime >= dateRange.Start.Value );
                    }
                    if ( dateRange.End.HasValue )
                    {
                        txnDetailQry = txnDetailQry.Where( a => a.Transaction.TransactionDateTime < dateRange.End.Value );
                    }

                    if ( !cbClosedBatches.Checked )
                    {
                        txnDetailQry = txnDetailQry.Where( d => d.Transaction.Batch.Status == BatchStatus.Open || d.Transaction.Batch.Status == BatchStatus.Pending );
                    }
                }

                var softCredits = new Dictionary<int, Person>();

                if ( txnDetailEntityType != null )
                {
                    var softCreditTxns = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                        .Where( v =>
                            v.Attribute.EntityTypeId == txnDetailEntityType.Id &&
                            v.Attribute.Key == "SoftCreditFor" &&
                            v.EntityId.HasValue &&
                            txnDetailQry.Select( d => d.Id ).Contains( v.EntityId.Value ) &&
                            v.Value != null &&
                            v.Value != "" )
                        .Select( v => new
                        {
                            TxnId = v.EntityId.Value,
                            v.Value
                        } )
                        .ToList()
                        .ToDictionary( k => k.TxnId, v => v.Value.AsGuid() );

                    var personAliasGuids = softCreditTxns.Select( t => t.Value ).Distinct();
                    var softCreditPersons = new PersonAliasService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( pa => personAliasGuids.Contains( pa.Guid ) )
                        .ToDictionary( k => k.Guid, v => v.Person );

                    foreach ( var txn in softCreditTxns )
                    {
                        if ( softCreditPersons.ContainsKey( txn.Value ) )
                        {
                            softCredits.Add( txn.Key, softCreditPersons[txn.Value] );
                        }
                    }
                }

                
                // Get the list of transaction detail records and create a custom row item for each one
                _transactionDetailList = txnDetailQry.ToList();

                var details = new List<RowItem>();
                foreach ( var d in _transactionDetailList )
                {
                    var person = d.Transaction.AuthorizedPersonAlias.Person;
                    var row = new RowItem
                    {
                        Id = d.Id,
                        TransactionDateTime = d.Transaction.TransactionDateTime,
                        BatchName = d.Transaction.Batch.Name,
                        Amount = d.Amount,
                        Account = d.Account.Name,
                        Assigned = d.EntityId.HasValue,
                        GroupMemberId = d.EntityId
                    };

                    if ( softCredits.ContainsKey( d.Id ) )
                    {
                        row.SoftCreditFrom = person.NickName + " " + person.LastName;
                        person = softCredits[d.Id];
                    }

                    row.PersonName = person.NickName + " " + person.LastName;

                    details.Add( row );
                }

                // Only show the save button if there are transactions to view
                btnSave.Visible = details.Any();

                // Sort the results
                if ( gTransactions.SortProperty != null )
                {
                    gTransactions.DataSource = details.AsQueryable().Sort( gTransactions.SortProperty );
                }
                else
                {
                    gTransactions.DataSource = details.OrderByDescending( d => d.TransactionDateTime ).ToList();
                }
                
                // Bind the results to data grid
                gTransactions.DataBind();
            }
        }

        /// <summary>
        /// Assigns the entity to transaction detail.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="financialTransactionDetailId">The financial transaction detail identifier.</param>
        private void AssignTransactionDetail( int? groupMemberId, int? financialTransactionDetailId )
        {
            int? entityTypeId = EntityTypeCache.GetId<Rock.Model.GroupMember>();
            if ( entityTypeId.HasValue && financialTransactionDetailId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var financialTransactionDetail = new FinancialTransactionDetailService( rockContext ).Get( financialTransactionDetailId.Value );
                    if ( financialTransactionDetail.EntityTypeId != entityTypeId.Value || ( financialTransactionDetail.EntityId ?? 0 ) != ( groupMemberId ?? 0 ) )
                    {
                        financialTransactionDetail.EntityTypeId = entityTypeId.Value;
                        financialTransactionDetail.EntityId = groupMemberId;

                        rockContext.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Loads the group members drop down.
        /// </summary>
        /// <param name="ddlGroup">The DDL group.</param>
        /// <param name="groupId">The group identifier.</param>
        private void LoadGroupMembersDropDown( RockDropDownList ddlGroup )
        {
            var ddlGroupMember = ddlGroup.Parent.ControlsOfTypeRecursive<RockDropDownList>().FirstOrDefault( a => a.ID.StartsWith( "ddlGroupMember_" ) ) as RockDropDownList;
            if ( ddlGroupMember != null )
            {
                int? groupId = ddlGroup.SelectedValue.AsIntegerOrNull();
                ddlGroupMember.Items.Clear();
                ddlGroupMember.Items.Add( new ListItem() );
                if ( groupId.HasValue )
                {
                    var groupMemberListItems = new GroupMemberService( new RockContext() ).Queryable().Where( a => a.GroupId == groupId.Value )
                        .OrderBy( a => a.Person.NickName ).ThenBy( a => a.Person.LastName )
                        .Select( a => new
                        {
                            a.Id,
                            a.Person.SuffixValueId,
                            a.Person.NickName,
                            a.Person.LastName
                        } ).ToList().Select( a => new ListItem( Person.FormatFullName( a.NickName, a.LastName, a.SuffixValueId ), a.Id.ToString() ) );

                    ddlGroupMember.Items.AddRange( groupMemberListItems.ToArray() );
                }

                ddlGroupMember.Visible = groupId.HasValue;
            }
        }

        #endregion

        public class RowItem
        {
            public int Id { get; set; }
            public string PersonName { get; set; }
            public string SoftCreditFrom { get; set; }
            public DateTime? TransactionDateTime { get; set; }
            public string BatchName { get; set; }
            public decimal Amount { get; set; }
            public string Account { get; set; }
            public bool Assigned { get; set; }
            public int? GroupMemberId { get; set; }
        }

        public class FundRaisingGroup
        {
            public int GroupId { get; set; }
            public string GroupName { get; set; }
            public List<FundRaisingGroupMember> Members { get; set; }
        }

        public class FundRaisingGroupMember
        {
            public int GroupMemberId { get; set; }
            public int PersonId { get; set; }
            public string PersonName { get; set; }
        }

    }
}