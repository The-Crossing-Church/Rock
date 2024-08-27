using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rock.Jobs;

namespace org.crossingchurch.HubspotIntegration.Jobs
{
    internal class HubSpotIntegrationSharedResources
    {
    }

    public interface IAdditionalProperties
    {
        Dictionary<string, object> GetAdditionalProperties( RockJob rockJob );
    }

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

    public class HSContactProperties
    {
        public string createdate { get; set; }
        public string lastmodifieddate { get; set; }
        public string email { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string has_potential_rock_match { get; set; }
        public string hs_all_assigned_business_unit_ids { get; set; }
        private string _phone { get; set; }
        public string phone
        {
            get
            {
                return !String.IsNullOrEmpty( _phone ) ? _phone.Replace( " ", "" ).Replace( "(", "" ).Replace( ")", "" ).Replace( "-", "" ).Replace( "+", "" ) : "";
            }
            set
            {
                _phone = value;
            }
        }
        public string rock_id { get; set; }
        public string which_best_describes_your_involvement_with_the_crossing_ { get; set; }
    }

    [DebuggerDisplay( "Id: {id}, Email: {properties.email}" )]
    public class HSContactResult
    {
        public string id { get; set; }
        public HSContactProperties properties { get; set; }
        public string archived { get; set; }
        public virtual int rock_id
        {
            get
            {
                int id = 0;
                if ( properties != null && properties.rock_id != null )
                {
                    Int32.TryParse( properties.rock_id, out id );
                }
                return id;
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
}
