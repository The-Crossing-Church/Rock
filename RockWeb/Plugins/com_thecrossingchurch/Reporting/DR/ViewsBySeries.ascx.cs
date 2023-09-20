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
    [DisplayName( "Views By Series and Tags" )]
    [Category( "com_thecrossingchurch > Reporting > DR" )]
    [Description( "Report to let you see which sermon and podcast series have been watched most, and which topics are most popular" )]
    [InteractionChannelField( "Watch Interaction Channel", key: AttributeKey.WatchInteractionChannel, required: true, order: 1 )]
    [AttributeField( Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM, name: "Watch Series Attribute", key: AttributeKey.WatchSeriesAttr, required: true, allowMultiple: false, entityTypeQualifierColumn: "ContentChannelId", entityTypeQualifierValue: "56", order: 3 )]
    [InteractionChannelField( "Listen Interaction Channel", key: AttributeKey.ListenInteractionChannel, required: true, order: 4 )]
    [AttributeField( Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM, name: "Listen Series Attribute", key: AttributeKey.ListenSeriesAttr, required: true, allowMultiple: false, entityTypeQualifierColumn: "ContentChannelId", entityTypeQualifierValue: "55", order: 6 )]
    [IntegerField( "Days Back", "The number of days to go back to look for data", false, 90, "", 7, AttributeKey.DaysBack )]
    public partial class ViewsBySeries : Rock.Web.UI.RockBlock
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
            if (watchInteractionChannelGuid.HasValue && watchSeriesAttrGuid.HasValue && listenInteractionChannelGuid.HasValue && listenSeriesAttrGuid.HasValue)
            {
                InteractionChannel watchInteractionChannel = new InteractionChannelService( context ).Get( watchInteractionChannelGuid.Value );
                Attribute watchSeriesAttr = new AttributeService( context ).Get( watchSeriesAttrGuid.Value );
                InteractionChannel listenInteractionChannel = new InteractionChannelService( context ).Get( listenInteractionChannelGuid.Value );
                Attribute listenSeriesAttr = new AttributeService( context ).Get( listenSeriesAttrGuid.Value );

                if (!Page.IsPostBack)
                {
                    if (Request.Params["isSeriesViewRequest"].AsBoolean())
                    {
                        List<SeriesViewResult> results = new List<SeriesViewResult>();
                        if (Request.Params["Filter"] != "listen")
                        {
                            var r = BuildSeriesViewGrid( watchInteractionChannel.Id, watchSeriesAttr.Id );
                            results.AddRange( r );
                        }
                        if (Request.Params["Filter"] != "watch")
                        {
                            var r = BuildSeriesViewGrid( listenInteractionChannel.Id, listenSeriesAttr.Id );
                            results.AddRange( r );
                        }
                        Response.Write( JsonConvert.SerializeObject( results ) );
                        Response.End();
                    }
                    if (Request.Params["isTagViewRequest"].AsBoolean())
                    {
                        List<TagsViewResult> results = new List<TagsViewResult>();
                        if (Request.Params["Filter"] != "listen")
                        {
                            var r = BuildTagsViewGrid( watchInteractionChannel.Id );
                            results.AddRange( r );
                        }
                        if (Request.Params["Filter"] != "watch")
                        {
                            var r = BuildTagsViewGrid( listenInteractionChannel.Id );
                            results.AddRange( r );
                        }
                        Response.Write( JsonConvert.SerializeObject( results ) );
                        Response.End();
                    }
                }
            }
        }
        #endregion

        #region Methods
        private List<SeriesViewResult> BuildSeriesViewGrid( int channelId, int seriesAttrId )
        {
            int? daysBack = GetAttributeValue( AttributeKey.DaysBack ).AsIntegerOrNull();
            DateTime dateCheck = DateTime.Now;
            if (daysBack.HasValue && daysBack.Value > 0)
            {
                dateCheck = dateCheck.AddDays( -1 * daysBack.Value );
            }
            else
            {
                dateCheck = new DateTime( 2020, 01, 01, 0, 0, 0 );
            }
            var query = context.Database.SqlQuery<SeriesViewResult>( $@"
                SELECT Series, COUNT(*) AS 'UniqueViews', SUM(Views) AS 'TotalViews'
                FROM (
                         SELECT Value AS 'Series', IpAddress, COUNT(*) AS 'Views'
                         FROM AttributeValue
                                  INNER JOIN (
                             SELECT EntityId, IpAddress
                             FROM InteractionDeviceType
                                      INNER JOIN (
                                 SELECT EntityId, DeviceTypeId, IpAddress
                                 FROM InteractionSession
                                          INNER JOIN (
                                     SELECT ic.EntityId, InteractionSessionId
                                     FROM Interaction
                                              INNER JOIN (
                                         SELECT Id, EntityId
                                         FROM InteractionComponent
                                         WHERE InteractionChannelId = @ChannelId
                                     ) AS ic ON ic.Id = Interaction.InteractionComponentId
                                     WHERE InteractionDateTime > @DateCheck
                                 ) AS i ON InteractionSessionId = InteractionSession.Id
                             ) AS session ON DeviceTypeId = InteractionDeviceType.Id
                             WHERE Application NOT LIKE '%bot%'
                         ) AS idt ON idt.EntityId = AttributeValue.EntityId
                         WHERE AttributeId = @SeriesAttrId
                         GROUP BY Value, IpAddress
                     ) AS av
                GROUP BY Series
                ORDER BY TotalViews DESC
            ", new SqlParameter( "@ChannelId", channelId ), new SqlParameter( "@SeriesAttrId", seriesAttrId ), new SqlParameter( "@DateCheck", dateCheck ) );
            return query.ToList();
        }
        private List<TagsViewResult> BuildTagsViewGrid( int channelId )
        {
            int? daysBack = GetAttributeValue( AttributeKey.DaysBack ).AsIntegerOrNull();
            DateTime dateCheck = DateTime.Now;
            if (daysBack.HasValue && daysBack.Value > 0)
            {
                dateCheck = dateCheck.AddDays( -1 * daysBack.Value );
            }
            else
            {
                dateCheck = new DateTime( 2020, 01, 01, 0, 0, 0 );
            }
            var query = context.Database.SqlQuery<TagsViewResult>( $@"
                SELECT tags.Name AS 'Tag', Title AS 'Sermon', TotalViews
                FROM (
                         SELECT Name, MAX(Views) AS 'Highest', SUM(Views) AS 'TotalViews'
                         FROM (
                                  SELECT Name, Title, COUNT(*) AS 'Views'
                                  FROM Tag
                                           INNER JOIN (
                                      SELECT InteractionId, Title, TagId
                                      FROM TaggedItem
                                               INNER JOIN (
                                          SELECT idt.Id AS 'InteractionId', Title, Guid
                                          FROM ContentChannelItem
                                                   INNER JOIN (
                                              SELECT EntityId, session.Name, session.Id
                                              FROM InteractionDeviceType
                                                       INNER JOIN (
                                                  SELECT EntityId, Name, i.Id, DeviceTypeId
                                                  FROM InteractionSession
                                                           INNER JOIN (
                                                      SELECT ic.EntityId, Name, Interaction.Id, InteractionSessionId
                                                      FROM Interaction
                                                               INNER JOIN (
                                                          SELECT Id, Name, EntityId
                                                          FROM InteractionComponent
                                                          WHERE InteractionChannelId = @ChannelId
                                                      ) AS ic ON InteractionComponentId = ic.Id
                                                      WHERE InteractionDateTime > @DateCheck
                                                  ) AS i ON InteractionSessionId = InteractionSession.Id
                                              ) AS session ON DeviceTypeId = InteractionDeviceType.Id
                                              WHERE Application NOT LIKE '%bot%'
                                          ) AS idt ON EntityId = ContentChannelItem.Id
                                      ) AS cci ON cci.Guid = EntityGuid
                                      WHERE EntityTypeId = 208
                                  ) AS ti ON TagId = Tag.Id
                                  GROUP BY Title, Name
                              ) AS tag
                         GROUP BY Name
                     ) AS tags
                         INNER JOIN (
                    SELECT Name, Title, COUNT(*) AS 'Views'
                    FROM Tag
                             INNER JOIN (
                        SELECT InteractionId, Title, TagId
                        FROM TaggedItem
                                 INNER JOIN (
                            SELECT idt.Id AS 'InteractionId', Title, Guid
                            FROM ContentChannelItem
                                     INNER JOIN (
                                SELECT EntityId, session.Name, session.Id
                                FROM InteractionDeviceType
                                         INNER JOIN (
                                    SELECT EntityId, Name, i.Id, DeviceTypeId
                                    FROM InteractionSession
                                             INNER JOIN (
                                        SELECT ic.EntityId, Name, Interaction.Id, InteractionSessionId
                                        FROM Interaction
                                                 INNER JOIN (
                                            SELECT Id, Name, EntityId
                                            FROM InteractionComponent
                                            WHERE InteractionChannelId = @ChannelId
                                        ) AS ic ON InteractionComponentId = ic.Id
                                    ) AS i ON InteractionSessionId = InteractionSession.Id
                                ) AS session ON DeviceTypeId = InteractionDeviceType.Id
                                WHERE Application NOT LIKE '%bot%'
                            ) AS idt ON EntityId = ContentChannelItem.Id
                        ) AS cci ON cci.Guid = EntityGuid
                        WHERE EntityTypeId = 208
                    ) AS ti ON TagId = Tag.Id
                    GROUP BY Title, Name
                ) AS titles ON Views = Highest AND tags.Name = titles.Name
                ORDER BY Tag ASC, TotalViews DESC
            ", new SqlParameter( "@ChannelId", channelId ), new SqlParameter( "@DateCheck", dateCheck ) );
            return query.ToList();
        }
        #endregion

        private class SeriesViewResult
        {
            public string Series { get; set; }
            public int UniqueViews { get; set; }
            public int TotalViews { get; set; }
        }
        private class TagsViewResult
        {
            public string Tag { get; set; }
            public string Sermon { get; set; }
            public int TotalViews { get; set; }
        }
    }
}