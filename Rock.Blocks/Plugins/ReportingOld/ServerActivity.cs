using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using RestSharp;
using Newtonsoft.Json;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Plugins.Reporting
{
    #region BlockAttributes
    [IntegerField( "Days Back", key: AttributeKey.DaysBack, defaultValue: 30 )]
    #endregion
    internal class ServerActivity : RockObsidianBlockType
    {
        #region Keys
        private static class AttributeKey
        {
            public const string DaysBack = "DaysBack";
        }
        #endregion

        #region Properties
        private int endOfMorningInterval = 10;
        private int endOfMiddayInterval = 18;
        #endregion

        #region Obsidian Block Type Overrides
        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetObsidianBlockInitialization()
        {
            ServerActivityViewModel viewModel = new ServerActivityViewModel();
            int daysBack = GetAttributeValue( AttributeKey.DaysBack ).AsInteger();
            DateTime today = DateTime.Now.EndOfDay();
            DateTime start = today.AddDays( daysBack * -1 ).StartOfDay();
            for ( int i = daysBack; i >= 0; i-- )
            {
                DateTime current = today.AddDays( i * -1 );
                viewModel.Labels.Add( current.ToString( "M/d" ) + " AM" );
                viewModel.Labels.Add( current.ToString( "M/d" ) + " Mid" );
                viewModel.Labels.Add( current.ToString( "M/d" ) + " PM" );
            }
            viewModel.DataViewData = BuildPersistedDataViewData( viewModel.Labels, start );
            return viewModel;
        }
        #endregion

        #region Block Actions

        #endregion

        #region Helpers
        private List<int> BuildPersistedDataViewData( List<string> labels, DateTime start )
        {
            List<DataView> dataviews = new DataViewService( new Data.RockContext() ).Queryable().Where( dv => dv.PersistedScheduleIntervalMinutes.HasValue && dv.PersistedLastRefreshDateTime > start ).ToList();
            List<int> data = labels.Select( l => 0 ).ToList();

            for ( int i = 0; i < dataviews.Count(); i++ )
            {
                DateTime currentRunTime = dataviews[i].PersistedLastRefreshDateTime.Value;
                while ( currentRunTime > start && currentRunTime >= dataviews[i].CreatedDateTime )
                {
                    int hour = currentRunTime.Hour;
                    string timeframe = currentRunTime.ToString( "M/d" );
                    if ( hour < endOfMorningInterval )
                    {
                        timeframe += " AM";
                    }
                    else if ( hour < endOfMiddayInterval )
                    {
                        timeframe += " Mid";
                    }
                    else
                    {
                        timeframe += " PM";
                    }
                    int idx = labels.IndexOf( timeframe );
                    data[idx]++;
                    currentRunTime = currentRunTime.AddMinutes( dataviews[i].PersistedScheduleIntervalMinutes.Value * -1 );
                }
            }
            return data;
        }
        private class ServerActivityViewModel
        {
            public List<string> Labels { get; set; }
            public List<int> DataViewData { get; set; }
            public List<int> JobData { get; set; }
            public List<int> InteractionData { get; set; }
            public List<int> CheckinData { get; set; }
            public List<int> WorkflowData { get; set; }
        }
        #endregion
    }
}
