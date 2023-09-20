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

namespace RockWeb.Plugins.com_thecrossingchurch.Reporting.DR
{
    [DisplayName( "Watched/Listened Today" )]
    [Category( "com_thecrossingchurch > Reporting > DR" )]
    [Description( "Report to let you see which sermons and podcasts have been watched today" )]
    [InteractionChannelField( "Watch Interaction Channel", key: AttributeKey.WatchInteractionChannel, required: true, order: 1 )]
    [AttributeField( Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM, name: "Watch Series Attribute", key: AttributeKey.WatchSeriesAttr, required: true, allowMultiple: false, entityTypeQualifierColumn: "ContentChannelId", entityTypeQualifierValue: "56", order: 3 )]
    [InteractionChannelField( "Listen Interaction Channel", key: AttributeKey.ListenInteractionChannel, required: true, order: 4 )]
    [AttributeField( Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM, name: "Listen Series Attribute", key: AttributeKey.ListenSeriesAttr, required: true, allowMultiple: false, entityTypeQualifierColumn: "ContentChannelId", entityTypeQualifierValue: "55", order: 6 )]
    public partial class WatchedToday : Rock.Web.UI.RockBlock
    {
        #region Keys
        private static class AttributeKey
        {
            public const string WatchInteractionChannel = "WatchInteracnnel";
            public const string WatchSeriesAttr = "WatchSeriesAttr";
            public const string ListenInteractionChannel = "ListenInteractionChannel";
            public const string ListenSeriesAttr = "ListenSeriesAttr";
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
                    if ( Request.Params["isWatchedTodayRequest"].AsBoolean() )
                    {
                        List<IGrouping<string, ViewsResult>> results = new List<IGrouping<string, ViewsResult>>();
                        if ( Request.Params["Filter"] != "listen" )
                        {
                            results.AddRange( BuildViewsGrid( watchInteractionChannel.Id, watchSeriesAttr.Id ).GroupBy( i => i.Series ) );
                        }
                        if ( Request.Params["Filter"] != "watch" )
                        {
                            results.AddRange( BuildViewsGrid( listenInteractionChannel.Id, listenSeriesAttr.Id ).GroupBy( i => i.Series ) );
                        }
                        Response.Write( JsonConvert.SerializeObject( results ) );
                        Response.End();
                    }
                }
            }
        }
        #endregion

        #region Methods
        private List<ViewsResult> BuildViewsGrid( int channelId, int seriesAttrId )
        {
            var query = context.Database.SqlQuery<ViewsResult>( $@"
                SELECT Value AS 'Series', Name, COUNT(*) AS Views
                FROM AttributeValue
                         INNER JOIN (
                    SELECT EntityId, session.Name
                    FROM InteractionDeviceType
                             INNER JOIN (
                        SELECT EntityId, Name, DeviceTypeId
                        FROM InteractionSession
                                 INNER JOIN (
                            SELECT ic.EntityId, Name, InteractionSessionId
                            FROM Interaction
                                     INNER JOIN (
                                SELECT Id, Name, EntityId
                                FROM InteractionComponent
                                WHERE InteractionChannelId = @ChannelId
                            ) AS ic ON InteractionComponentId = ic.Id
                            WHERE CAST(InteractionDateTime AS DATE) = CAST(GETDATE() AS Date)
                        ) AS i ON InteractionSessionId = InteractionSession.Id
                    ) AS session ON DeviceTypeId = InteractionDeviceType.Id
                    WHERE Application NOT LIKE '%bot%'
                ) AS idt ON idt.EntityId = AttributeValue.EntityId
                WHERE AttributeId = @SeriesAttrId
                GROUP BY Value, Name
                ORDER BY Views DESC
            ", new SqlParameter( "@ChannelId", channelId ), new SqlParameter( "@SeriesAttrId", seriesAttrId ) );
            return query.ToList();
        }
        #endregion

        private class ViewsResult
        {
            public string Series { get; set; }
            public string Name { get; set; }
            public int Views { get; set; }
        }
    }
}