using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Attribute;
using Rock.Jobs;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using System.Collections.Concurrent;

namespace org.thecrossingchurch.CustomJobs.Jobs
{
    /// <summary>
    /// Job to do some load testing on our Rock database.
    /// </summary>
    [DisplayName( "Stress Test" )]
    [Description( "This job will hit the database very hard with reads. The purpose of this is for load testing, this should never be active or regularly run on our Rock instance." )]

    [IntegerField( "Number of Threads", "The number of threads this job should run, minimum is 1.", true, 10, key: AttributeKey.NumThreads )]
    [IntegerField( "Number of Records", "The number of records to process per thread.", true, 100, key: AttributeKey.NumRecords )]
    internal class StressTest : RockJob
    {
        private class AttributeKey
        {
            public const string NumThreads = "NumThreads";
            public const string NumRecords = "NumRecords";
        }
        public override void Execute()
        {
            int numThreads = GetAttributeValue( AttributeKey.NumThreads ).AsInteger();
            if ( numThreads <= 0 )
            {
                numThreads = 1;
            }
            int numRecords = GetAttributeValue( AttributeKey.NumRecords ).AsInteger();
            if ( numRecords <= 0 )
            {
                numRecords = 100;
            }

            ConcurrentBag<Task> taskBag = new ConcurrentBag<Task>();
            for ( int i = 0; i < numThreads; i++ )
            {
                var people = new PersonService( new RockContext() ).Queryable().OrderBy( p => p.Id ).Skip( i * numRecords ).Take( numRecords );
                Task task = new Task( () =>
                {
                    ProcessRecords( people.ToList() );
                } );
                taskBag.Add( task );
                task.Start();
            }
            Task.WaitAll( taskBag.ToArray() );
        }

        private void ProcessRecords( List<Person> people )
        {
            using ( var rockContext = new RockContext() )
            {
                for ( int i = 0; i < people.Count; i++ )
                {
                    var person = new PersonService( rockContext ).Get( people[i].Id );
                    person.LoadAttributes();
                    var address = person.GetHomeLocation();
                    var registrations = new RegistrationService( rockContext ).Queryable().Where( r => r.PersonAlias.PersonId == person.Id ).ToList();
                    var groups = new GroupMemberService( rockContext ).Queryable().Where( gm => gm.PersonId == person.Id ).ToList();
                    foreach ( var attr in person.Attributes )
                    {
                        if ( attr.Value.FieldTypeId == 114 )
                        {
                            var value = person.AttributeValues[attr.Key];
                            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                            mergeFields.Add( "Entity", person );
                            var renderedLavaValue = value.Value.ResolveMergeFields( mergeFields ).Trim();
                        }
                    }
                    var groupsByType = new GroupMemberService( rockContext ).Queryable().Where( gm => gm.PersonId == person.Id && gm.Group.GroupType.GroupTypePurposeValueId > 0 ).Select( gm => gm.Group ).GroupBy( g => g.GroupTypeId );
                    var financials = new FinancialTransactionService( rockContext ).Queryable().Where( ft => ft.AuthorizedPersonAliasId.HasValue && person.Aliases.Select( pa => pa.Id ).Contains( ft.AuthorizedPersonAliasId.Value ) && ft.TransactionDetails.Any( ftd => ftd.AccountId == 12 ) );

                }
            }
        }
    }
}
