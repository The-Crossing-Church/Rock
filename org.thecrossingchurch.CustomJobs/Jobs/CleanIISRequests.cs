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
    [DisplayName( "Clean Request URLs" )]
    [Description( "This job processes IIS request logs and cleans the urls that have unique identifiers." )]
    public class CleanIISRequests : RockJob
    {

        #region Attribute Keys
        private class AttributeKey
        {

        }
        #endregion Attribute Keys
        public override void Execute()
        {
            //BasicScrub();
            v2Scrub();
        }
        private void v2Scrub()
        {
            RockContext context = new RockContext();
            PageService page_svc = new PageService( context );
            BlockService block_svc = new BlockService( context );

            IQueryable<IISLog> requests = context.Database.SqlQuery<IISLog>( "SELECT id, [cs-uri-stem] as 'path', [cs-uri-query] as 'query' FROM ___iis_api_logs WHERE clean_path LIKE '/api/v2/BlockActions/%' ORDER BY id;" ).AsQueryable();
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
                    var pathFragments = logs[k].path.Split( '/' );
                    for ( int j = 0; j < pathFragments.Length; j++ )
                    {
                        if ( pathFragments[j] != "" )
                        {
                            if ( j < 4 || j > 5 )
                            {
                                cleanPath += "/" + pathFragments[j];
                            }
                            else if ( j == 4 )
                            {
                                // Page Guid
                                Page p = page_svc.Get( pathFragments[j] );
                                if ( p != null )
                                {
                                    cleanPath += "/" + p.InternalName + " (" + p.Id + ")";
                                }
                                else
                                {
                                    cleanPath += "/" + pathFragments[j];
                                }
                            }
                            else
                            {
                                // Block Guid
                                Block b = block_svc.Get( pathFragments[j] );
                                if ( b != null )
                                {
                                    cleanPath += "/" + b.Name + " (" + b.Id + ")";
                                }
                                else
                                {
                                    cleanPath += "/" + pathFragments[j];
                                }
                            }
                        }
                    }

                    SqlParameter[] sqlParameters = new SqlParameter[] {
                        new SqlParameter( "@cleanPath", cleanPath ),
                        new SqlParameter( "@id", logs[k].id )
                    };
                    context.Database.ExecuteSqlCommand( "UPDATE ___iis_api_logs SET clean_path = @cleanPath WHERE id = @id;", sqlParameters );
                }
            }
        }
        private void BasicScrub()
        {
            RockContext context = new RockContext();

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
                                if ( logs[k].path.Contains( "/api/People/GetByEmail/" ) )
                                {
                                    if ( j < pathFragments.Length - 1 )
                                    {
                                        cleanPath += "/" + pathFragments[j];
                                    }
                                    else
                                    {
                                        cleanPath += "/{Email}";
                                    }
                                }
                                else
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
        private class IISLog
        {
            public int id { get; set; }
            public string path { get; set; }
            public string query { get; set; }
        }
    }
}
