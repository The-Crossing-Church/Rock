using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using Rock;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;

namespace org.crossingchurch.HubspotIntegration.Jobs
{
    public class AdditionalDatabaseQueries : IAdditionalProperties
    {
        public Dictionary<string, object> GetAdditionalProperties( Dictionary<string, string> configurationValues, List<int> rockIds, IQueryable<PersonAlias> aliasQuery )
        {
            using ( RockContext context = new RockContext() )
            {
                Dictionary<string, object> customProps = new Dictionary<string, object>();
                string accountId;
                if ( configurationValues.TryGetValue( "TMBTAccount", out accountId ) )
                {
                    if ( !String.IsNullOrEmpty( accountId ) )
                    {
                        FinancialAccount account = new FinancialAccountService( context ).Get( accountId.AsInteger() );
                        if ( account != null )
                        {
                            customProps.Add( "TMBTAccount", account );
                        }
                    }
                }
                return customProps;
            }
        }
        public List<HubspotPropertyUpdate> ProcessAdditionalProperties( Dictionary<string, string> configurationValues, Dictionary<string, object> data, List<HubspotProperty> attrs, List<HubspotProperty> props, List<PersonAlias> aliases, List<AttributeValue> personAttributeValues, List<GroupMember> familyGroupMemberships, List<GroupMember> personGroupMemberships, List<ChildrenMap> childKnownRelationships, TransactionMap givingTransactions, HSContactResult contact, Person person )
        {
            List<HubspotPropertyUpdate> properties = new List<HubspotPropertyUpdate>();

            //Special Property for Parents
            if ( person.AgeClassification == AgeClassification.Adult )
            {
                try
                {
                    properties.AddRange( GetChildrensAgeGroups( familyGroupMemberships, childKnownRelationships, person, props ) );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new Exception( $"HubSpot Sync Error: Custom Action, unable to add children's age group. HubSpot ID: {contact.id}, Rock ID: {person.Id}", ex ) );
                }
            }

            //Set Serving Teams and Small Groups
            var memberships = personGroupMemberships.Where( gm => gm.PersonId == person.Id ).ToList();
            if ( memberships.Count() > 0 )
            {
                try
                {
                    properties.AddRange( GetGroupMembershipData( memberships, props, person ) );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new Exception( $"HubSpot Sync Error: Custom Action, unable to add serving teams and small groups. HubSpot ID: {contact.id}, Rock ID: {person.Id}", ex ) );
                }
            }

            if ( givingTransactions != null && givingTransactions.ValidTransactions.Count > 0 )
            {
                FinancialAccount account = null;
                if ( data.ContainsKey( "TMBTAccount" ) )
                {
                    account = ( FinancialAccount ) data["TMBTAccount"];
                }
                try
                {
                    properties.AddRange( GetGivingProps( props, givingTransactions, person, account ) );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new Exception( $"HubSpot Sync Error: Custom Action, unable to add giving data. HubSpot ID: {contact.id}, Rock ID: {person.Id}", ex ) );
                }

                if ( account != null )
                {
                    //Get the TMBT Giving Props!
                    try
                    {
                        properties.AddRange( GetTMBTGivingProps( props, givingTransactions, person, account ) );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new Exception( $"HubSpot Sync Error: Custom Action, unable to add TMBT giving data. HubSpot ID: {contact.id}, Rock ID: {person.Id}", ex ) );
                    }
                }
            }

            //ZipCode
            var homeAddress = person.GetHomeLocation();
            if ( homeAddress != null )
            {
                var zipcode_prop = props.FirstOrDefault( p => p.label == "Rock Custom ZipCode" );
                if ( zipcode_prop != null )
                {
                    try
                    {
                        properties.Add( new HubspotPropertyUpdate() { property = zipcode_prop.name, value = homeAddress.PostalCode } );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new Exception( $"HubSpot Sync Error: Custom Action, unable to add home address zipcode. HubSpot ID: {contact.id}, Rock ID: {person.Id}", ex ) );
                    }
                }
            }

            return properties;
        }

        /// <summary>
        /// Finds the information we want about the age categories of a person's children
        /// </summary>
        /// <returns></returns>
        private List<HubspotPropertyUpdate> GetChildrensAgeGroups( List<GroupMember> familyGroupMemberships, List<ChildrenMap> childKnownRelationships, Person person, List<HubspotProperty> props )
        {
            List<HubspotPropertyUpdate> properties = new List<HubspotPropertyUpdate>();
            int sixYearsAgo = DateTime.Now.Year - 6;
            var child_ages_prop = props.FirstOrDefault( p => p.label == "Rock Custom Children's Age Groups" );

            var family = familyGroupMemberships.Where( p => p.GroupId == person.PrimaryFamilyId ).ToList();
            //Direct Family Members
            var children = family.Where( gm => gm.Person != null && gm.PersonId != person.Id && gm.Person.AgeClassification == AgeClassification.Child ).Select( gm => gm.Person ).ToList();
            var agegroups = "";
            //Known Relationships
            children.AddRange(
                childKnownRelationships.Where( kr => kr.Parent.Id == person.Id ).SelectMany( kr => kr.Children ).ToList()
            );
            agegroups = String.Join( ",", children.Distinct().Select( p =>
            {
                if ( p.GradeOffset.HasValue )
                {
                    if ( p.GradeOffset.Value > 12 )
                    {
                        return "EarlyChildhood";
                    }
                    else if ( p.GradeOffset.Value > 6 )
                    {
                        return "Elementary";
                    }
                    else if ( p.GradeOffset.Value > 3 )
                    {
                        return "Middle";
                    }
                    else if ( p.GradeOffset.Value >= 0 && p.GradeOffset.Value <= 3 )
                    {
                        return "SeniorHigh";
                    }
                    return "Adult";
                }
                else
                {
                    if ( p.AgeClassification == AgeClassification.Adult )
                    {
                        return "Adult";
                    }
                    if ( p.BirthYear >= sixYearsAgo )
                    {
                        return "EarlyChildhood";
                    }
                }
                return "Unknown";
            } ).Where( a => a != "Unknown" ).Distinct().ToList()
            ); //In theory we shouldn't have unknown data, but we can't use it so ignore it

            if ( agegroups.Length > 0 && child_ages_prop != null )
            {
                properties.Add( new HubspotPropertyUpdate() { property = child_ages_prop.name, value = agegroups } );
            }

            return properties;
        }

        /// <summary>
        /// Information about whether the current person is in a Small Group or on a Serving Team
        /// </summary>
        /// <returns></returns>
        private List<HubspotPropertyUpdate> GetGroupMembershipData( List<GroupMember> memberships, List<HubspotProperty> props, Person person )
        {
            List<HubspotPropertyUpdate> properties = new List<HubspotPropertyUpdate>();
            //Special properties for a person's group membership 
            //Currently in adult small group, currently in a 20s small group, currently in veritas small group, currently serving, currently in connections, membership list
            var today = DateTime.UtcNow;
            var term = "fall";
            if ( DateTime.Compare( today, new DateTime( today.Year, 5, 15 ) ) <= 0 )
            {
                term = "winter";
            }
            else if ( DateTime.Compare( today, new DateTime( today.Year, 8, 15 ) ) <= 0 )
            {
                term = "summer";
            }
            //All current memberships for this year
            memberships = memberships.Where( m => m.Group != null && m.GroupMemberStatus == GroupMemberStatus.Active && m.Group.IsActive && ( m.Group.Name.Contains( today.ToString( "yyyy" ) ) ||
                 m.Group.Name.Contains( $"{today.AddYears( -1 ).ToString( "yyyy" )}-{today.ToString( "yy" )}" ) ) ).ToList();
            //Where the group name has Fall/Winter/Summer or Purpose == Serving Area
            var current_serving = memberships.Where( m => m.Group.Name.ToLower().Contains( term ) || ( m.Group.GroupType.GroupTypePurposeValue != null && m.Group.GroupType.GroupTypePurposeValue.Value == "Serving Area" ) ).ToList();
            //All current groups with the words Small Group, SG or Purpose == Small Group
            var current_sg = memberships.Where( m => m.Group.Name.ToLower().Contains( "small group" ) || m.Group.Name.ToLower().Contains( "sg" ) || ( m.Group.GroupType.GroupTypePurposeValue != null && m.Group.GroupType.GroupTypePurposeValue.Value == "Small Group" ) ).ToList();

            var serving_prop = props.FirstOrDefault( p => p.label == "Rock Custom Currently Serving" );
            var sg_props = props.Where( p => p.label.Contains( "Small Group" ) ).ToList();

            //set the serving prop
            if ( serving_prop != null )
            {
                if ( current_serving.Count() > 0 )
                {
                    properties.Add( new HubspotPropertyUpdate() { property = serving_prop.name, value = "true" } );
                }
                else
                {
                    properties.Add( new HubspotPropertyUpdate() { property = serving_prop.name, value = "false" } );
                }
            }
            //figure out if they attend small group
            if ( current_sg.Count() > 0 && person.AgeClassification != AgeClassification.Child )
            {
                if ( current_sg.Count() > 1 && current_sg.Where( sg => !sg.GroupRole.IsLeader ).Count() > 0 )
                { //See if we can get this to one small group hopefully
                    current_sg = current_sg.Where( sg => !sg.GroupRole.IsLeader ).ToList();
                }
                foreach ( var sg in current_sg )
                {
                    var small_group = sg_props.FirstOrDefault( p => p.label == "Rock Custom Currently in Adult Small Group" );
                    if ( sg.Group.ParentGroup.Name.ToLower().Contains( "veritas" ) )
                    {
                        small_group = sg_props.FirstOrDefault( p => p.label == "Rock Custom Currently in Veritas Small Group" );
                    }
                    else if ( sg.Group.ParentGroup.Name.ToLower().Contains( "twenties" ) )
                    {
                        small_group = sg_props.FirstOrDefault( p => p.label == "Rock Custom Currently in Twenties Small Group" );
                    }

                    var exists = properties.FirstOrDefault( p => p.property == small_group.name );
                    if ( exists == null )
                    {
                        properties.Add( new HubspotPropertyUpdate() { property = small_group.name, value = "true" } );
                    }
                }
            }
            //Make the other values false so we keep the list up to date
            foreach ( var sg_prop in sg_props )
            {
                var exists = properties.FirstOrDefault( p => p.property == sg_prop.name );
                if ( exists == null )
                {
                    properties.Add( new HubspotPropertyUpdate() { property = sg_prop.name, value = "false" } );
                }
            }

            return properties;
        }

        private List<HubspotPropertyUpdate> GetGivingProps( List<HubspotProperty> props, TransactionMap transactionMap, Person person, FinancialAccount tmbtAccount )
        {
            List<HubspotPropertyUpdate> properties = new List<HubspotPropertyUpdate>();

            //Sort Valid Transactions
            var validTransactions = transactionMap.ValidTransactions.Where( ft => tmbtAccount == null || ft.TransactionDetails.Any( ftd => ftd.AccountId != tmbtAccount.Id ) )
                .Select( ft =>
                {
                    ft.TransactionDetails = ft.TransactionDetails.Where( ftd => tmbtAccount == null || ftd.AccountId != tmbtAccount.Id ).ToList();
                    return ft;
                } ).Where( ft => ft.TransactionDetails.Count() > 0 ).OrderBy( ft => ft.TransactionDateTime );

            if ( validTransactions.Count() > 0 )
            {
                //Hubspot Giving Properties
                var first_contribution_date_prop = props.FirstOrDefault( p => p.label == "Rock Custom FirstContributionDate" );
                var last_contribution_date_prop = props.FirstOrDefault( p => p.label == "Rock Custom LastContributionDate" );
                string firstDate = ConvertDate( validTransactions.First().TransactionDateTime );
                if ( !String.IsNullOrEmpty( firstDate ) && first_contribution_date_prop != null )
                {
                    properties.Add( new HubspotPropertyUpdate() { property = first_contribution_date_prop.name, value = firstDate } );
                }
                var first_contribution_fund_prop = props.FirstOrDefault( p => p.label == "Rock Custom FirstContributionFund" );
                if ( first_contribution_fund_prop != null )
                {
                    properties.Add( new HubspotPropertyUpdate() { property = first_contribution_fund_prop.name, value = validTransactions.First().TransactionDetails.First().Account.Name } );
                }

                //Get Last Transaction
                validTransactions = validTransactions.OrderByDescending( ft => ft.TransactionDateTime );
                string lastDate = ConvertDate( validTransactions.First().TransactionDateTime );
                if ( !String.IsNullOrEmpty( lastDate ) && last_contribution_date_prop != null )
                {
                    properties.Add( new HubspotPropertyUpdate() { property = last_contribution_date_prop.name, value = lastDate } );
                }
                var last_contribution_fund_prop = props.FirstOrDefault( p => p.label == "Rock Custom LastContributionFund" );
                if ( last_contribution_fund_prop != null )
                {
                    properties.Add( new HubspotPropertyUpdate() { property = last_contribution_fund_prop.name, value = validTransactions.First().TransactionDetails.First().Account.Name } );
                }
            }

            return properties;
        }

        private List<HubspotPropertyUpdate> GetTMBTGivingProps( List<HubspotProperty> props, TransactionMap transactionMap, Person person, FinancialAccount account )
        {
            List<HubspotPropertyUpdate> properties = new List<HubspotPropertyUpdate>();

            var tmbtTransactions = transactionMap.ValidTransactions.Where( ft => ft.TransactionDetails.Any( ftd => ftd.AccountId == account.Id ) )
                .Select( ft =>
                {
                    ft.TransactionDetails = ft.TransactionDetails.Where( ftd => ftd.AccountId == account.Id ).ToList();
                    return ft;
                } ).OrderBy( ft => ft.TransactionDateTime );

            if ( tmbtTransactions.Count() > 0 )
            {
                //Total Amount
                var total = tmbtTransactions.Sum( ft => ft.TransactionDetails.Sum( ftd => ftd.Amount ) );
                var total_contribution_amount_prop = props.FirstOrDefault( p => p.label == "Rock Custom Total TMBT Contribution Amount" );
                if ( total_contribution_amount_prop != null )
                {
                    properties.Add( new HubspotPropertyUpdate() { property = total_contribution_amount_prop.name, value = total.ToString() } );
                }
                var first_contribution_amt_prop = props.FirstOrDefault( p => p.label == "Rock Custom First TMBT Contribution Amount" );
                if ( first_contribution_amt_prop != null )
                {
                    properties.Add( new HubspotPropertyUpdate() { property = first_contribution_amt_prop.name, value = tmbtTransactions.First().TotalAmount.ToString() } );
                }
                var first_contribution_date_prop = props.FirstOrDefault( p => p.label == "Rock Custom First TMBT Contribution Date" );
                string firstDate = ConvertDate( tmbtTransactions.First().TransactionDateTime );
                if ( !String.IsNullOrEmpty( firstDate ) && first_contribution_date_prop != null )
                {
                    properties.Add( new HubspotPropertyUpdate() { property = first_contribution_date_prop.name, value = firstDate } );
                }

                string frquency = String.Join( ", ", tmbtTransactions.Select( ft => ft.ScheduledTransactionId.HasValue ? ft.ScheduledTransaction.TransactionFrequencyValue.Description : "One Time" ).Distinct() );
                var frequency_prop = props.FirstOrDefault( p => p.label == "Rock Custom TMBT Contribution Frequency" );
                if ( frequency_prop != null )
                {
                    properties.Add( new HubspotPropertyUpdate() { property = frequency_prop.name, value = frquency } );
                }

                tmbtTransactions = tmbtTransactions.OrderByDescending( ft => ft.TransactionDateTime );

                var last_contribution_amt_prop = props.FirstOrDefault( p => p.label == "Rock Custom Last TMBT Contribution Amount" );
                if ( last_contribution_amt_prop != null )
                {
                    properties.Add( new HubspotPropertyUpdate() { property = last_contribution_amt_prop.name, value = tmbtTransactions.First().TotalAmount.ToString() } );
                }
                var last_contribution_date_prop = props.FirstOrDefault( p => p.label == "Rock Custom Last TMBT Contribution Date" );
                string lastDate = ConvertDate( tmbtTransactions.First().TransactionDateTime );
                if ( !String.IsNullOrEmpty( lastDate ) && last_contribution_date_prop != null )
                {
                    properties.Add( new HubspotPropertyUpdate() { property = last_contribution_date_prop.name, value = lastDate } );
                }
            }
            return properties;
        }
        private string ConvertDate( DateTime? date )
        {
            if ( date.HasValue )
            {
                DateTime today = RockDateTime.Now;
                if ( today.Year - date.Value.Year < 1000 && today.Year - date.Value.Year > -1000 )
                {
                    date = new DateTime( date.Value.Year, date.Value.Month, date.Value.Day, 0, 0, 0 );
                    var d = date.Value.Subtract( new DateTime( 1970, 1, 1 ) ).TotalSeconds * 1000;
                    return d.ToString();
                }
            }
            return "";
        }
    }
}
