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
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using HubSpot.NET.Core;
using Newtonsoft.Json;
using System.Net;
using HubSpot.NET.Api.Contact.Dto;
using System.IO;
using System.Reflection;

namespace org.crossingchurch.HubspotIntegration.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [TextField("Hubspot API Key", "API Key for Hubspot", true, "", "", 0)]
    [DisallowConcurrentExecution]
    public class HubspotIntegration : IJob
    {

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public HubspotIntegration()
        {
        }

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            string key = dataMap.GetString("HubspotAPIKey");

            var current_id = 0;


            //New instance of api 
            HubSpotApi api = new HubSpotApi(key);

            //Get custom contact properties from Hubspot 
            WebRequest request = WebRequest.Create($"https://api.hubapi.com/properties/v1/contacts/properties?hapikey={key}");
            var response = request.GetResponse();
            var props = new List<HubspotProperty>();
            using ( Stream stream = response.GetResponseStream() )
            {
                using ( StreamReader reader = new StreamReader(stream) )
                {
                    var jsonResponse = reader.ReadToEnd();
                    props = JsonConvert.DeserializeObject<List<HubspotProperty>>(jsonResponse);
                }
            }
            //Only care about the ones that are custom and could be valid Rock fields
            props = props.Where(p => p.createdUserId != null).ToList();

            //Get List of all contacts from Hubspot
            List<ContactHubSpotModel> contacts = new List<ContactHubSpotModel>();
            long offset = 0;
            var hasmore = true;
            while ( hasmore )
            {
                var list = api.Contact.List<ContactHubSpotModel>(new ListRequestOptions
                {
                    PropertiesToInclude = new List<string> { "firstname", "lastname", "email", "phone" },
                    Limit = 100,
                    Offset = offset
                });
                hasmore = list.MoreResultsAvailable;
                offset = list.ContinuationOffset;
                contacts.AddRange(list.Contacts);
            }
            //Contacts with emails only 
            var contacts_with_email = contacts.Where(c => c.Email != null).ToList();

            //Foreach contact with an email, look for a 1:1 match in Rock by email and schedule it's update 
            for ( var i = 0; i < contacts_with_email.Count(); i++ )
            {
                var query = new PersonService.PersonMatchQuery(contacts_with_email[i].FirstName, contacts_with_email[i].LastName, contacts_with_email[i].Email, contacts_with_email[i].Phone); 
                var person = new PersonService(new RockContext()).FindPerson(query, false);
                //For Testing
                //if ( contacts_with_email[i].Email == "jim@thecrossingchurch.com" || contacts_with_email[i].Email == "jimbeatyjr@gmail.com" )
                //{
                //    query = new PersonService.PersonMatchQuery(contacts_with_email[i].FirstName, contacts_with_email[i].LastName, "jimbeaty@safety.netz", contacts_with_email[i].Phone);
                //    person = new PersonService(new RockContext()).FindPerson(query, false);
                //}
                //if ( contacts_with_email[i].Email == "cody.melton@thecrossingchurch.com")
                //{
                //    query = new PersonService.PersonMatchQuery(contacts_with_email[i].FirstName, contacts_with_email[i].LastName, "codymelton@safety.netz", contacts_with_email[i].Phone);
                //    person = new PersonService(new RockContext()).FindPerson(query, false);
                //}
                //Schedule HubSpot update if 1:1 match
                if ( person != null )
                {
                    try
                    {
                        current_id = person.Id;
                        //Get the Attributes for that Person 
                        var attrs = new AttributeValueService(new RockContext()).GetByEntityId(person.Id).ToList();
                        //Build the POST request and schedule in the db 10 at a time 
                        var url = $"https://api.hubapi.com/contacts/v1/contact/vid/{contacts_with_email[i].Id}/profile?hapikey={key}";
                        var properties = new List<HubspotPropertyUpdate>();
                        //Add each Rock prop to the list with the Hubspot name
                        for ( var j = 0; j < attrs.Count(); j++ )
                        {
                            var current_prop = props.FirstOrDefault(p => p.label == attrs[j].Attribute.Name);
                            //If the attribute is in our list of props from Hubspot
                            if ( current_prop != null )
                            {
                                if ( attrs[j].Attribute.FieldType.Name == "Date" || attrs[j].Attribute.FieldType.Name == "Date Time" )
                                {
                                    //Get Epoc miliseconds 
                                    DateTime tryDate;
                                    if ( DateTime.TryParse(attrs[j].Value, out tryDate) )
                                    {
                                        var d = tryDate.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000;
                                        properties.Add(new HubspotPropertyUpdate() { property = current_prop.name, value = d.ToString() });
                                    }
                                }
                                else
                                {
                                    properties.Add(new HubspotPropertyUpdate() { property = current_prop.name, value = attrs[j].Value });
                                }
                            }
                        }
                        //All properties begining with "Rock " are properties on the Person entity itself 
                        var person_props = props.Where(p => p.label.Contains("Rock ")).ToList();
                        foreach ( PropertyInfo propInfo in person.GetType().GetProperties() )
                        {
                            var current_prop = props.FirstOrDefault(p => p.label == "Rock " + propInfo.Name);
                            if ( current_prop != null )
                            {
                                if ( propInfo.PropertyType.FullName == "Rock.Model.DefinedValue" )
                                {
                                    DefinedValue dv = JsonConvert.DeserializeObject<DefinedValue>(JsonConvert.SerializeObject(propInfo.GetValue(person)));
                                    properties.Add(new HubspotPropertyUpdate() { property = current_prop.name, value = dv.Value });
                                }
                                else if ( propInfo.PropertyType.FullName == "Date" || propInfo.PropertyType.FullName == "Date Time" )
                                {
                                    //Get Epoc miliseconds 
                                    DateTime tryDate;
                                    if ( DateTime.TryParse(propInfo.GetValue(person).ToString(), out tryDate) )
                                    {
                                        var d = tryDate.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000;
                                        properties.Add(new HubspotPropertyUpdate() { property = current_prop.name, value = d.ToString() });
                                    }
                                }
                                else
                                {
                                    properties.Add(new HubspotPropertyUpdate() { property = current_prop.name, value = propInfo.GetValue(person).ToString() });
                                }
                            }
                        }

                        //Special Property for Parents
                        if ( person.PrimaryFamily.Members.FirstOrDefault(gm => gm.PersonId == person.Id).GroupRole.Name == "Adult" )
                        {
                            //Direct Family Members
                            var child_ages_prop = props.FirstOrDefault(p => p.label == "Children's Age Groups");
                            var children = person.PrimaryFamily.Members.Where(m => m.GroupRole.Name == "Child").ToList();
                            var agegroups = "";
                            //Known Relationships
                            int pid = person.Id;
                            var krGroups = new GroupMemberService(new RockContext()).Queryable().Where(gm => gm.PersonId == pid && gm.GroupRoleId == 5).Select(gm => gm.GroupId).ToList();
                            var childRelationships = new List<int> { 4, 15, 17 };
                            var krMembers = new GroupMemberService(new RockContext()).Queryable().Where(gm => krGroups.Contains(gm.GroupId) && childRelationships.Contains(gm.GroupRoleId)).ToList();
                            children.AddRange(krMembers);
                            for ( var j = 0; j < children.Count(); j++ )
                            {
                                if ( children[j].Person.GradeOffset > 6 )
                                {
                                    //Child is in K-5
                                    if ( !agegroups.Contains("Elementary") )
                                    {
                                        agegroups += "Elementary,";
                                    }
                                }
                                else if ( children[j].Person.GradeOffset > 3 )
                                {
                                    //Child is in 6-8
                                    if ( !agegroups.Contains("Middle") )
                                    {
                                        agegroups += "Middle,";
                                    }
                                }
                                else if ( children[j].Person.GradeOffset <= 3 )
                                {
                                    //Child is in 9-12
                                    if ( !agegroups.Contains("SeniorHigh") )
                                    {
                                        agegroups += "SeniorHigh,";
                                    }
                                }
                                else
                                {
                                    //Check if child is infant-toddler or adult
                                    var bornCheck = DateTime.Now;
                                    if ( children[j].Person.BirthYear >= ( bornCheck.Year - 5 ) )
                                    {
                                        if ( !agegroups.Contains("EarlyChildhood") )
                                        {
                                            agegroups += "EarlyChildhood,";
                                        }
                                    }
                                    else
                                    {
                                        if ( !agegroups.Contains("Adult") )
                                        {
                                            agegroups += "Adult,";
                                        }
                                    }
                                }
                            }
                            properties.Add(new HubspotPropertyUpdate() { property = child_ages_prop.name, value = agegroups.Substring(0, agegroups.Length - 1) });
                        }

                        if ( person.Members != null && person.Members.Count() > 0 )
                        {
                            //Special properties for a person's group membership 
                            //Currently in adult small group, currently in a 20s small group, currently in veritas small group, currently serving, currently in connections, membership list
                            var today = DateTime.UtcNow;
                            var term = "fall";
                            if ( DateTime.Compare(today, new DateTime(today.Year, 5, 15)) <= 0 )
                            {
                                term = "winter";
                            }
                            else if ( DateTime.Compare(today, new DateTime(today.Year, 8, 15)) <= 0 )
                            {
                                term = "summer";
                            }
                            //All current memberships for this year
                            var memberships = person.Members.Where(m => ( m.Group.Name.Contains(today.ToString("yyyy")) ||
                                m.Group.Name.Contains($"{today.AddYears(-1).ToString("yyyy")}-{today.ToString("yy")}") )).ToList();
                            //Where the group name has Fall/Winter/Summer
                            var current_serving = memberships.Where(m => m.Group.Name.ToLower().Contains(term)).ToList();
                            //All current groups with the words Small Group
                            var current_sg = memberships.Where(m => m.Group.Name.ToLower().Contains("small group")).ToList();

                            var serving_prop = props.FirstOrDefault(p => p.label == "Currently Serving");
                            var sg_props = props.Where(p => p.label.Contains("Small Group")).ToList();
                            //set the serving prop
                            if ( current_serving.Count() > 0 )
                            {
                                properties.Add(new HubspotPropertyUpdate() { property = serving_prop.name, value = "true" });
                            }
                            else
                            {
                                properties.Add(new HubspotPropertyUpdate() { property = serving_prop.name, value = "false" });
                            }
                            //figure out if they attend small group
                            if ( current_sg.Count() > 0 )
                            {
                                if ( current_sg.Count() > 1 && current_sg.Where(sg => !sg.GroupRole.IsLeader).Count() > 0 )
                                { //See if we can get this to one small group hopefully
                                    current_sg = current_sg.Where(sg => !sg.GroupRole.IsLeader).ToList();
                                }
                                foreach ( var sg in current_sg )
                                {
                                    var small_group = sg_props.FirstOrDefault(p => p.label == "Currently in Adult Small Group");
                                    if ( sg.Group.ParentGroup.Name.ToLower().Contains("veritas") )
                                    {
                                        small_group = sg_props.FirstOrDefault(p => p.label == "Currently in Veritas Small Group");
                                    }
                                    else if ( sg.Group.ParentGroup.Name.ToLower().Contains("twenties") )
                                    {
                                        small_group = sg_props.FirstOrDefault(p => p.label == "Currently in Twenties Small Group");
                                    }

                                    var exists = properties.FirstOrDefault(p => p.property == small_group.name);
                                    if ( exists == null )
                                    {
                                        properties.Add(new HubspotPropertyUpdate() { property = small_group.name, value = "true" });
                                    }
                                }
                            }
                            //Make the other values false so we keep the list up to date
                            foreach ( var sg_prop in sg_props )
                            {
                                var exists = properties.FirstOrDefault(p => p.property == sg_prop.name);
                                if ( exists == null )
                                {
                                    properties.Add(new HubspotPropertyUpdate() { property = sg_prop.name, value = "false" });
                                }
                            }
                            //List of all groups person is a member of (fingers crossed it's not too long) 
                            var group_prop = props.FirstOrDefault(p => p.label == "Group Membership");
                            var grps = "";
                            for ( var idx = 0; idx < person.Members.Count(); idx++ )
                            {
                                grps += $"{person.Members.ToList()[idx].Group.Name}, ";
                            }
                            grps = grps.Substring(0, grps.Length - 2);
                            properties.Add(new HubspotPropertyUpdate() { property = group_prop.name, value = grps });
                            //Figure out if the person is currently involved in connections 
                            //Prod ID for Parent Group: 136298
                            var conn_prop = props.FirstOrDefault(p => p.label == "Currently in Connections");
                            var groups = new GroupService(new RockContext()).Queryable().AsNoTracking().Where(g => g.ParentGroupId == 103379).ToList(); //test with sg minitry group 
                            var inConnections = FindGroup(groups, person, false);
                            if ( inConnections )
                            {
                                properties.Add(new HubspotPropertyUpdate() { property = conn_prop.name, value = "true" });
                            }
                            else
                            {
                                properties.Add(new HubspotPropertyUpdate() { property = conn_prop.name, value = "false" });
                            }
                        }

                        //Update the Hubspot Contact
                        try
                        {
                            var webrequest = WebRequest.Create(url);
                            webrequest.Method = "POST";
                            webrequest.ContentType = "application/json";
                            using ( Stream requestStream = webrequest.GetRequestStream() )
                            {
                                var json = $"{{\"properties\": {JsonConvert.SerializeObject(properties)} }}";
                                byte[] bytes = Encoding.ASCII.GetBytes(json);
                                requestStream.Write(bytes, 0, bytes.Length);
                            }
                            using ( WebResponse webResponse = webrequest.GetResponse() )
                            {
                                using ( Stream responseStream = webResponse.GetResponseStream() )
                                {
                                    using ( StreamReader reader = new StreamReader(responseStream) )
                                    {
                                        var jsonResponse = reader.ReadToEnd();
                                        Console.WriteLine(jsonResponse);
                                    }
                                }
                            }

                        }
                        catch ( WebException ex )
                        {
                            using ( Stream responseStream = ex.Response.GetResponseStream() )
                            {
                                using ( StreamReader reader = new StreamReader(responseStream) )
                                {
                                    var jsonResponse = reader.ReadToEnd();
                                    Console.WriteLine($"Hubspot: {jsonResponse}");
                                    System.IO.File.AppendAllText("hubspot_error.txt", $"___________________________\nCurennt Id: {current_id}\nMessage: {jsonResponse}\n");
                                }
                            }
                        }
                        catch ( Exception e )
                        {
                            Console.WriteLine($"Other: {e.Message}");
                            System.IO.File.AppendAllText("hubspot_error.txt", $"___________________________\nCurennt Id: {current_id}\nMessage: {e.Message}\nStack: {e.StackTrace}\n");
                        }

                    }
                    catch ( Exception err )
                    {
                        System.IO.File.AppendAllText("hubspot_error.txt", $"___________________________\nCurennt Id: {current_id}\nMessage: {err.Message}\nStack: {err.StackTrace}\n");
                    }
                }
            }
        }

        private bool FindGroup( List<Group> groups, Person per, bool subOfYear )
        {
            var today = DateTime.UtcNow;
            var inConnections = false;
            for ( var i = 0; i < groups.Count(); i++ )
            {
                if ( groups[i].Name.Contains(today.ToString("yyyy")) || groups[i].Name.Contains($"{today.AddYears(-1).ToString("yyyy")}-{today.ToString("yy")}") || subOfYear )
                {
                    if ( groups[i].Members.Count() > 0 )
                    {
                        var exists = groups[i].Members.FirstOrDefault(p => p.PersonId == per.Id);
                        if ( exists != null )
                        {
                            return true;
                        }
                    }
                    else
                    {
                        inConnections = FindGroup(groups[i].Groups.ToList(), per, true);
                    }
                }
                else
                {
                    inConnections = FindGroup(groups[i].Groups.ToList(), per, false);
                }
            }
            return inConnections;
        }

    }

    public class HubspotProperty
    {
        public string name { get; set; }
        public string label { get; set; }
        public bool? deleted { get; set; }
        public int? createdUserId { get; set; }
    }

    public class HubspotPropertyUpdate
    {
        public string property { get; set; }
        public string value { get; set; }
    }
}
