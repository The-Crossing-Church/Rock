using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Attribute;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Attribute = Rock.Model.Attribute;
using System.Diagnostics;

namespace RockWeb.Plugins.com_thecrossingchurch.Reporting.DR
{
    [DisplayName( "Behavior Trends" )]
    [Category( "com_thecrossingchurch > Reporting > DR" )]
    [Description( "Report to let you see which sermons and podcasts have been watched together by multiple people" )]
    [InteractionChannelField( "Watch Interaction Channel", key: AttributeKey.WatchInteractionChannel, required: true, order: 1 )]
    [AttributeField( Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM, name: "Watch Series Attribute", key: AttributeKey.WatchSeriesAttr, required: true, allowMultiple: false, entityTypeQualifierColumn: "ContentChannelId", entityTypeQualifierValue: "56", order: 3 )]
    [InteractionChannelField( "Listen Interaction Channel", key: AttributeKey.ListenInteractionChannel, required: true, order: 4 )]
    [AttributeField( Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM, name: "Listen Series Attribute", key: AttributeKey.ListenSeriesAttr, required: true, allowMultiple: false, entityTypeQualifierColumn: "ContentChannelId", entityTypeQualifierValue: "55", order: 6 )]
    [IntegerField( "Days Back", "The number of days to go back to look for data", false, 90, "", 7, AttributeKey.DaysBack )]
    public partial class BehaviorTrends : Rock.Web.UI.RockBlock
    {
        #region Keys
        private static class AttributeKey
        {
            public const string WatchInteractionChannel = "WatchInteracnnel";
            public const string WatchSeriesAttr = "WatchSeriesAttr";
            public const string ListenInteractionChannel = "ListenInteractionChannel";
            public const string ListenSeriesAttr = "ListenSeriesAttr";
            public const string DaysBack = "DaysBack";
        }
        #endregion

        #region Variables
        private RockContext context { get; set; }
        #endregion

        #region Base Control Methods
        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent( this.Page );
        }

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            context = new RockContext();
            Guid? watchInteractionChannelGuid = GetAttributeValue( AttributeKey.WatchInteractionChannel ).AsGuidOrNull();
            Guid? watchSeriesAttrGuid = GetAttributeValue( AttributeKey.WatchSeriesAttr ).AsGuidOrNull();
            Guid? listenInteractionChannelGuid = GetAttributeValue( AttributeKey.ListenInteractionChannel ).AsGuidOrNull();
            Guid? listenSeriesAttrGuid = GetAttributeValue( AttributeKey.ListenSeriesAttr ).AsGuidOrNull();
            if ( watchInteractionChannelGuid.HasValue && watchSeriesAttrGuid.HasValue && listenInteractionChannelGuid.HasValue && listenSeriesAttrGuid.HasValue )
            {
                InteractionChannel watchInteractionChannel = new InteractionChannelService( context ).Get( watchInteractionChannelGuid.Value );
                Attribute watchSeriesAttr = new AttributeService( context ).Get( watchSeriesAttrGuid.Value );
                InteractionChannel listenInteractionChannel = new InteractionChannelService( context ).Get( listenInteractionChannelGuid.Value );
                Attribute listenSeriesAttr = new AttributeService( context ).Get( listenSeriesAttrGuid.Value );

                if ( !Page.IsPostBack )
                {
                    if ( Request.Params["isBehaviorTrendRequest"].AsBoolean() )
                    {
                        List<IGrouping<string, BehaviorResult>> ipResults = new List<IGrouping<string, BehaviorResult>>();
                        List<IpData> ipData = new List<IpData>();
                        List<BehaviorResult> results = new List<BehaviorResult>();
                        Stopwatch w = new Stopwatch();
                        w.Start();
                        if ( Request.Params["Filter"] != "listen" )
                        {
                            var r = BuildViewsGrid( watchInteractionChannel.Id, watchSeriesAttr.Id );
                            ipResults.AddRange( r.GroupBy( i => i.IpAddress ) );
                            results.AddRange( r );
                        }
                        if ( Request.Params["Filter"] != "watch" )
                        {
                            var r = BuildViewsGrid( listenInteractionChannel.Id, listenSeriesAttr.Id );
                            ipResults.AddRange( r.GroupBy( i => i.IpAddress ) );
                            results.AddRange( r );
                        }
                        var afterBuild = w.Elapsed.TotalSeconds;

                        for ( var i = 0; i < ipResults.Count(); i++ )
                        {
                            List<int> sermons = ipResults[i].Select( d => d.EntityId ).Distinct().OrderBy( d => d ).ToList();
                            var sermonCombinations = Combination( sermons, 2 ).Select( d => d.ToList() );
                            List<string> series = ipResults[i].Select( d => d.Series ).Distinct().OrderBy( d => d ).ToList();
                            var seriesCombinations = Combination( series, 2 ).Select( d => d.ToList() );
                            ipData.Add( new IpData() { IpAddress = ipResults[i].Key, Data = ipResults[i].ToList(), Series = seriesCombinations, Sermons = sermonCombinations } );
                        }
                        var sermonData = ipData.Where( d => d.Sermons.Any() ).SelectMany( d => d.Sermons ).OrderBy( d => d.First() );
                        var sermonCounts = sermonData.GroupBy( d => d.First().ToString() + ";" + d.Last().ToString() ).Where( d => d.Count() >= 30 ).Select( d => new { Combo = d.First(), Count = d.Count() } ).ToList();
                        for ( var i = 0; i < sermonCounts.Count(); i++ )
                        {

                        }
                        var afterSerialize = w.Elapsed.TotalSeconds;
                        Response.Write( JsonConvert.SerializeObject( new { IP = ipResults, Series = results } ) );
                        Response.End();
                    }
                }
            }
        }
        #endregion

        #region Methods
        private List<BehaviorResult> BuildViewsGrid( int channelId, int seriesAttrId )
        {
            int? daysBack = GetAttributeValue( AttributeKey.DaysBack ).AsIntegerOrNull();
            DateTime dateCheck = DateTime.Now;
            if ( daysBack.HasValue && daysBack.Value > 0 )
            {
                dateCheck = dateCheck.AddDays( -1 * daysBack.Value );
            }
            else
            {
                dateCheck = new DateTime( 2020, 01, 01, 0, 0, 0 );
            }
            var query = context.Database.SqlQuery<BehaviorResult>( $@"
                SELECT DISTINCT idt.EntityId, Value AS 'Series', Name AS 'Title', IpAddress
                FROM AttributeValue
                         INNER JOIN (
                    SELECT EntityId, session.Name, InteractionDateTime, IpAddress, InteractionSessionId
                    FROM InteractionDeviceType
                             INNER JOIN (
                        SELECT EntityId, Name, InteractionDateTime, IpAddress, InteractionSessionId, DeviceTypeId
                        FROM InteractionSession
                                 INNER JOIN (
                            SELECT ic.EntityId, Name, InteractionSessionId, InteractionDateTime
                            FROM Interaction
                                     INNER JOIN (
                                SELECT Id, EntityId, Name
                                FROM InteractionComponent
                                WHERE InteractionChannelId = @ChannelId
                            ) AS ic ON InteractionComponentId = ic.Id
                            WHERE InteractionDateTime > @DateCheck
                        ) AS i ON InteractionSessionId = InteractionSession.Id
                    ) AS session ON DeviceTypeId = InteractionDeviceType.Id
                    WHERE Application NOT LIKE '%bot%'
                ) AS idt ON idt.EntityId = AttributeValue.EntityId
                WHERE AttributeId = @SeriesAttrId
            ", new SqlParameter( "@ChannelId", channelId ), new SqlParameter( "@SeriesAttrId", seriesAttrId ), new SqlParameter( "@DateCheck", dateCheck ) );
            return query.ToList();
        }

        static IEnumerable<IEnumerable<T>> Combination<T>( IEnumerable<T> list, int length ) where T : IComparable
        {
            if ( length == 1 ) return list.Select( t => new T[] { t } );
            return Combination( list, length - 1 )
                .SelectMany( t => list.Where( o => o.CompareTo( t.Last() ) > 0 ),
                    ( t1, t2 ) => t1.Concat( new T[] { t2 } ) );
        }
        #endregion

        private class BehaviorResult
        {
            public string Series { get; set; }
            public string Title { get; set; }
            public string IpAddress { get; set; }
            public int EntityId { get; set; }
        }

        private class IpData
        {
            public string IpAddress { get; set; }
            public IEnumerable<List<int>> Sermons { get; set; }
            public IEnumerable<List<string>> Series { get; set; }
            public List<BehaviorResult> Data { get; set; }

        }

        private class CombinationResult
        {
            public int First { get; set; }
            public int Second { get; set; }
        }

        private class SermonBehaviroData
        {
            public int SermonId { get; set; }
            public string Sermon { get; set; }
            public string Series { get; set; }
            //public List<> RelatedSermons { get; set; }
        }
    }
}