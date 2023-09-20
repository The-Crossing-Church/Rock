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

namespace RockWeb.Plugins.com_thecrossingchurch.Reporting.DR
{
    [DisplayName( "Current Sermon" )]
    [Category( "com_thecrossingchurch > Reporting > DR" )]
    [Description( "Report to let you see the watch stats on the current Sermon" )]
    [ContentChannelField( "Content Channel", key: AttributeKey.ContentChannel, required: true, order: 0 )]
    [InteractionChannelField( "Interaction Channel", key: AttributeKey.InteractionChannel, required: true, order: 1 )]
    [DayOfWeekField( "Day of Week", "Day of the week we should check for a new item in the given channels", true, key: AttributeKey.DayOfWeek, order: 2 )]
    [DateField( "Testing Date", "For testing, select a date in the past to use instead of current time", false, "", "", 3, AttributeKey.TestDate )]
    public partial class CurrentSermon : Rock.Web.UI.RockBlock
    {
        #region Keys
        private static class AttributeKey
        {
            public const string ContentChannel = "ContentChannel";
            public const string InteractionChannel = "InteractionChannel";
            public const string DayOfWeek = "DayOfWeek";
            public const string TestDate = "TestDate";
        }
        #endregion

        #region Variables
        private RockContext context { get; set; }
        private Guid? channelGuid { get; set; }
        private ContentChannel channel { get; set; }
        private Guid? interactionChannelGuid { get; set; }
        private InteractionChannel interactionChannel { get; set; }
        private int dayOfWeek { get; set; }
        private DateTime dt { get; set; }
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
            channelGuid = GetAttributeValue( AttributeKey.ContentChannel ).AsGuidOrNull();
            interactionChannelGuid = GetAttributeValue( AttributeKey.InteractionChannel ).AsGuidOrNull();
            channel = new ContentChannelService( context ).Get( channelGuid.Value );
            interactionChannel = new InteractionChannelService( context ).Get( interactionChannelGuid.Value );
            dayOfWeek = GetAttributeValue( AttributeKey.DayOfWeek ).AsInteger();
            string testdate = GetAttributeValue( AttributeKey.TestDate );
            dt = DateTime.Now;
            if ( !String.IsNullOrEmpty( testdate ) )
            {
                dt = DateTime.Parse( testdate );
            }
            if ( dt.DayOfWeek != ( DayOfWeek ) dayOfWeek )
            {
                dt = dt.AddDays( 0 - ( int ) dt.DayOfWeek );
            }
            hdrSermon.InnerText = GetSermonName();

            if ( !Page.IsPostBack )
            {
                if ( Request.Params["isViewsRequest"].AsBoolean() )
                {
                    Response.Write( BuildViewsChart() );
                    Response.End();
                }
                if ( Request.Params["isViewersRequest"].AsBoolean() )
                {
                    Response.Write( BuildViewersFunnel() );
                    Response.End();
                }
            }
        }
        #endregion

        #region Methods
        public string BuildViewsChart()
        {
            var query = context.Database.SqlQuery<ViewsResult>( $@"
                SELECT Date, COUNT( *) AS 'Unique', SUM([Total Views] ) AS 'Total'
                FROM(
                        SELECT Date, Identifier, SUM([Total Views] ) AS 'Total Views'
                        FROM(
                                SELECT Date, Identifier, [Total Views]
                                FROM InteractionDeviceType
                                        INNER JOIN(
                                    SELECT CAST( InteractionDateTime AS DATE ) AS 'Date',
                                            ( CASE
                                                WHEN PersonAliasId IS NOT NULL THEN CAST( PersonAliasId AS VARCHAR )
                                                ELSE IpAddress END )          AS 'Identifier',
                                            1                                 AS 'Total Views',
                                            DeviceTypeId
                                    FROM InteractionSession
                                            INNER JOIN(
                                        SELECT Id, InteractionDateTime, PersonAliasId, InteractionSessionId
                                        FROM Interaction
                                                INNER JOIN(
                                            SELECT InteractionComponent.Id AS 'ComponentId', Name
                                            FROM InteractionComponent
                                                    INNER JOIN(
                                                SELECT Id
                                                FROM ContentChannelItem
                                                WHERE ContentChannelId = @ContentChannelId
                                                    AND CAST( StartDateTime AS DATE ) = @SundayDate
                                            ) AS cci ON EntityId = cci.Id
                                            WHERE InteractionChannelId = @ChannelId
                                        ) AS ic ON ComponentId = InteractionComponentId
                                    ) AS i ON InteractionSessionId = InteractionSession.Id
                                ) AS session ON DeviceTypeId = InteractionDeviceType.Id
                                WHERE Application NOT LIKE '%bot%'
                            ) AS interactionSession
                        GROUP BY Date, Identifier
                    ) AS views
                GROUP BY Date
            ", new SqlParameter( "@ChannelId", interactionChannel.Id ), new SqlParameter( "@ContentChannelId", channel.Id ), new SqlParameter( "@SundayDate", dt.ToString( "yyyy-MM-dd" ) ) );
            return JsonConvert.SerializeObject( query );
        }
        public string BuildViewersFunnel()
        {
            var query = context.Database.SqlQuery<ViewersResult>( $@"
                SELECT Range, COUNT(*) AS 'ViewersInRange'
                FROM (
                        SELECT DISTINCT IpAddress
                        FROM InteractionSession
                                INNER JOIN (
                            SELECT Id, PersonAliasId, InteractionSessionId
                            FROM Interaction
                                    INNER JOIN (
                                SELECT InteractionComponent.Id AS 'ComponentId', Name
                                FROM InteractionComponent
                                        INNER JOIN (
                                    SELECT Id
                                    FROM ContentChannelItem
                                    WHERE ContentChannelId = @ContentChannelId
                                    AND CAST(StartDateTime AS DATE) = @SundayDate
                                ) AS cci ON EntityId = cci.Id
                                WHERE InteractionChannelId = @ChannelId
                            ) AS ic ON ComponentId = InteractionComponentId
                        ) AS i ON InteractionSessionId = InteractionSession.Id
                    ) AS currentSermonViewers
                        INNER JOIN (
                    SELECT IpAddress,
                        (CASE
                                WHEN FirstInteraction > DATEADD(DAY, -14, GETDATE()) THEN 0
                                WHEN Views < 4 THEN 1
                                WHEN Views < 9 THEN 2
                                WHEN Views < 13 THEN 3
                                ELSE 4
                            END) AS 'Range'
                    FROM (
                            SELECT channelInteractions.IpAddress, Views, FirstInteraction
                            FROM (SELECT IpAddress, MIN(CreatedDateTime) AS 'FirstInteraction'
                                FROM InteractionSession
                                GROUP BY IpAddress) AS firstTime
                                    INNER JOIN (
                                SELECT IpAddress, COUNT(*) AS 'Views'
                                FROM InteractionDeviceType
                                        INNER JOIN (
                                    SELECT IpAddress, DeviceTypeId, InteractionDateTime
                                    FROM InteractionSession
                                            INNER JOIN (
                                        SELECT InteractionSessionId, Interaction.Id, InteractionDateTime
                                        FROM Interaction
                                                INNER JOIN (
                                            SELECT Id FROM InteractionComponent WHERE InteractionChannelId = @ChannelId
                                        ) AS ic ON InteractionComponentId = ic.Id
                                    ) AS i ON InteractionSessionId = InteractionSession.Id
                                ) AS session ON DeviceTypeId = InteractionDeviceType.Id
                                WHERE Application NOT LIKE '%bot%'
                                GROUP BY IpAddress
                            ) AS channelInteractions ON channelInteractions.IpAddress = firstTime.IpAddress
                        ) AS interactionSession
                ) AS allSermonViewers ON allSermonViewers.IpAddress = currentSermonViewers.IpAddress
                GROUP BY Range
            ", new SqlParameter( "@ChannelId", interactionChannel.Id ), new SqlParameter( "@ContentChannelId", channel.Id ), new SqlParameter( "@SundayDate", dt.ToString( "yyyy-MM-dd" ) ) );
            return JsonConvert.SerializeObject( query );
        }
        public string GetSermonName()
        {
            var query = context.Database.SqlQuery<SermonResult>( $@"
                SELECT CONCAT(FORMAT(StartDateTime, 'dddd, MMMM d'), ' - ', Title) AS 'Title' FROM ContentChannelItem
                WHERE ContentChannelId = @ContentChannelId AND CAST(StartDateTime AS DATE) = @SundayDate
            ", new SqlParameter( "@ChannelId", interactionChannel.Id ), new SqlParameter( "@ContentChannelId", channel.Id ), new SqlParameter( "@SundayDate", dt.ToString( "yyyy-MM-dd" ) ) );
            return query.First().Title;
        }
        #endregion

        private class ViewsResult
        {
            public DateTime Date { get; set; }
            public int Unique { get; set; }
            public int Total { get; set; }
        }
        private class ViewersResult
        {
            public int Range { get; set; }
            public int ViewersInRange { get; set; }
        }

        private class SermonResult
        {
            public string Title { get; set; }
        }
    }
}