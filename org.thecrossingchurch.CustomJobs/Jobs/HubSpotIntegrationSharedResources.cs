using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rock;
using Rock.Model;

namespace org.crossingchurch.HubspotIntegration.Jobs
{
    internal class HubSpotIntegrationSharedResources
    {
    }

    public interface IAdditionalProperties
    {
        Dictionary<string, object> GetAdditionalProperties( Dictionary<string, string> configurationValues, List<int> rockIds, IQueryable<PersonAlias> aliasQuery );
        List<HubspotPropertyUpdate> ProcessAdditionalProperties( Dictionary<string, string> configurationValues, Dictionary<string, object> data, List<HubspotProperty> attrs, List<HubspotProperty> props, List<PersonAlias> aliases, List<AttributeValue> personAttributeValues, List<GroupMember> familyGroupMemberships, List<GroupMember> personGroupMemberships, List<ChildrenMap> childKnownRelationships, TransactionMap givingTransactions, HSContactResult contact, Person person );
    }

    #region Patching Helpers
    public class ChildrenMap
    {
        public Person Parent { get; set; }
        public List<Person> Children { get; set; }
    }
    public class TransactionMap
    {
        public Person Person { get; set; }
        public List<FinancialTransaction> ValidTransactions { get; set; }
    }
    #endregion

    [DebuggerDisplay( "Label: {label}, FieldType: {fieldType}" )]
    public class HubspotProperty
    {
        public string name { get; set; }
        public string label { get; set; }
        public string fieldType { get; set; }
        public string groupName { get; set; }
    }

    public class HubspotPropertyUpdate
    {
        public string property { get; set; }
        public string value { get; set; }
    }

    [DebuggerDisplay( "Id: {id}, Email: {email}" )]
    public class HSContactResult
    {
        public string id { get; set; }
        public Dictionary<string, string> properties { get; set; }
        public string archived { get; set; }
        public virtual int rock_id
        {
            get
            {
                int id = 0;
                if ( properties != null && properties["rock_id"] != null )
                {
                    Int32.TryParse( properties["rock_id"], out id );
                }
                return id;
            }
        }
        public virtual string firstname
        {
            get
            {
                string _firstname = String.Empty;
                if ( properties != null )
                {
                    properties.TryGetValue( "firstname", out _firstname );
                }
                return _firstname;
            }
        }
        public virtual string lastname
        {
            get
            {
                string _lastname = String.Empty;
                if ( properties != null )
                {
                    properties.TryGetValue( "lastname", out _lastname );
                }
                return _lastname;
            }
        }
        public virtual string email
        {
            get
            {
                string _email = String.Empty;
                if ( properties != null )
                {
                    properties.TryGetValue( "email", out _email );
                }
                //For easier matching to Rock
                return !String.IsNullOrEmpty( _email ) ? _email.ToLower() : "";
            }
        }
        public virtual string phone
        {
            get
            {
                string _phone = String.Empty;
                if ( properties != null )
                {
                    properties.TryGetValue( "phone", out _phone );
                }
                //Hubspot can format phone numbers, so we need to strip the formatting so we can match on the end of the number in Rock to account for country codes
                return !String.IsNullOrEmpty( _phone ) ? _phone.Replace( " ", "" ).Replace( "(", "" ).Replace( ")", "" ).Replace( "-", "" ).Replace( "+", "" ) : "";
            }
        }
    }
    public class HSResultPaging
    {
        public HSResultPagingNext next { get; set; }
    }
    public class HSResultPagingNext
    {
        public string after { get; set; }
        public string link { get; set; }
    }
    public class HSContactQueryResult
    {
        public int total { get; set; }
        public List<HSContactResult> results { get; set; }
        public HSResultPaging paging { get; set; }
    }
    public class HSPropertyQueryResult
    {
        public List<HubspotProperty> results { get; set; }
    }

    public class PotentialMatch
    {
        public HSContactResult hubspotContact { get; set; }
        public Person rockPerson { get; set; }
    }
}
