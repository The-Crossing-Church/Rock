using System;
using System.Collections.Generic;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;

namespace org.crossingchurch.HubspotIntegration.Jobs
{
    public class AdditionalDatabaseQueries : IAdditionalProperties
    {
        public Dictionary<string, object> GetAdditionalProperties( RockJob rockJob )
        {
            Dictionary<string, object> customProps = new Dictionary<string, object>();
            var accountGuid = rockJob.GetAttributeValue( "TMBTAccount" ).AsGuidOrNull();
            RockContext context = new RockContext();
            if ( accountGuid.HasValue )
            {
                FinancialAccount account = new FinancialAccountService( context ).Get( accountGuid.Value );
                customProps.Add( "TMBTAccount", account );
            }
            return customProps;
        }
    }
}
