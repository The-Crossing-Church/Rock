using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;

namespace org.thecrossingchurch.CustomJobs.Jobs
{
    /// <summary>
    /// Job to send connection request status reminders.
    /// </summary>
    [DisplayName( "Connection Request Status Reminders" )]
    [Description( "This job sends applicable reminders for connection requests stuck in a status as they are configured in the selected content channel." )]
    [ContentChannelField( 
        Name = "Reminder Content Channel", 
        Description = "The content channel that contains all the reminders", 
        IsRequired = true, 
        Key = AttributeKey.ContentChannel 
    )]
    [AttributeField(
        Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM, 
        Name = "Reminder Time Attribute", 
        Description = "The attribute that holds the time the reminder should be sent", 
        IsRequired = true,
        Key = AttributeKey.ReminderTimeAttr
    )]
    public class ConnectionRequestReminders : RockJob
    {

        #region Attribute Keys
        private class AttributeKey
        {
            public const string ContentChannel = "ContentChannel";
            public const string ReminderTimeAttr = "ReminderTimeAttr";
        }
        #endregion Attribute Keys
        public override void Execute()
        {
            //Guid? ContentChannelGuid = GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull();
            //if ( !ContentChannelGuid.HasValue )
            //{
            //    throw new Exception( "Unable to Load Reminder Content Channel." );
            //}
            RockContext context = new RockContext();
            //ContentChannel channel = new ContentChannelService( context ).Get( ContentChannelGuid.Value );
            //if ( channel == null )
            //{
            //    throw new Exception( "Unable to Load Reminder Content Channel." );
            //}
            //Guid? reminderTimeAttrGuid = GetAttributeValue(AttributeKey.ReminderTimeAttr).AsGuidOrNull();
            //AttributeService attr_svc = new AttributeService( context );
            //if(!reminderTimeAttrGuid.HasValue) {
            //    throw new Exception("Unable to Load Reminder Time Attribute.");
            //}
            //Attribute reminderTimeAttr = attr_svc.Get(reminderTimeAttrGuid.Value);
            //GetActiveReminders( channel, reminderTimeAttr );

            IQueryable<IISLog> requests = context.Database.SqlQuery<IISLog>( "SELECT id, [cs-uri-stem] as 'path', [cs-uri-query] as 'query' FROM ___iis_api_logs ORDER BY id;" ).AsQueryable();

            int numIterations = ( int ) Math.Ceiling( ( decimal ) requests.Count() / 25000 );
            for ( int i = 0; i <= numIterations; i++ )
            {
                IQueryable<IISLog> filter = requests.Skip( 25000 * i );
                if ( i < numIterations )
                {
                    filter = filter.Take( 25000 );
                }
                List<IISLog> logs = filter.ToList();
                for ( int k = 0; k < logs.Count; k++ )
                {
                    string cleanPath = "";
                    if ( !String.IsNullOrEmpty( logs[k].path ) )
                    {
                        var pathFragments = logs[k].path.Split( '/' );
                        for ( int j = 0; j < pathFragments.Length; j++ )
                        {
                            if ( pathFragments[j] != "" )
                            {
                                int id;
                                bool canParse = int.TryParse( pathFragments[j], out id );
                                if ( !Regex.Match( pathFragments[j], "[a-zA-Z]" ).Success && canParse )
                                {
                                    cleanPath += "/{Id}";
                                }
                                else
                                {
                                    cleanPath += "/" + pathFragments[j];
                                }
                            }
                        }
                    }

                    string cleanQuery = "";
                    if ( !String.IsNullOrEmpty( logs[k].query ) )
                    {
                        var queryFragments = logs[k].query.Split( '&' );
                        for ( int j = 0; j < queryFragments.Length; j++ )
                        {
                            if ( !String.IsNullOrEmpty( queryFragments[j] ) )
                            {
                                var fragmentParts = queryFragments[j].Split( '=' );

                                if ( j > 0 )
                                {
                                    cleanQuery += "&";
                                }
                                if ( fragmentParts.Length > 1 )
                                {
                                    int id;
                                    bool canParse = int.TryParse( fragmentParts[1], out id );
                                    if ( !Regex.Match( fragmentParts[1], "[a-zA-Z]" ).Success && canParse )
                                    {
                                        cleanQuery += fragmentParts[0] + "={Id}";
                                    }
                                    else
                                    {
                                        cleanQuery += queryFragments[j];
                                    }
                                }
                                else
                                {
                                    cleanQuery += queryFragments[j];
                                }
                            }
                        }
                    }
                    SqlParameter[] sqlParameters = new SqlParameter[] {
                        new SqlParameter( "@cleanPath", cleanPath ),
                        new SqlParameter( "@cleanQuery", cleanQuery ),
                        new SqlParameter( "@id", logs[k].id )
                    };
                    context.Database.ExecuteSqlCommand( "UPDATE ___iis_api_logs SET clean_path = @cleanPath, clean_query = @cleanQuery WHERE id = @id;", sqlParameters );
                }
            }
        }

        private void GetActiveReminders( ContentChannel channel, Attribute reminderTimeAttr )
        {
            RockContext context = new RockContext();
            AttributeService attr_svc = new AttributeService( context );
            AttributeValueService av_svc = new AttributeValueService(context);
            IQueryable<ContentChannelItem> reminders = new ContentChannelItemService( context ).Queryable().Where( cci => cci.ContentChannelId == channel.Id && cci.StartDateTime <= RockDateTime.Now && ( !cci.ExpireDateTime.HasValue || cci.ExpireDateTime.Value > RockDateTime.Now ) );
            // ServiceJob.LastRunDateTime
            av_svc.Queryable().Where(av => );
            var sendtime = reminders.First().GetAttributeValue( "ReminderTime" );
            var x = 7;
        }
        private class IISLog
        {
            public int id { get; set; }
            public string path { get; set; }
            public string query { get; set; }
        }
    }
}
