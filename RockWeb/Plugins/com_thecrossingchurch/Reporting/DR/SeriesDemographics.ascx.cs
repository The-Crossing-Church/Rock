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
using DocumentFormat.OpenXml.Vml.Presentation;
using Microsoft.Graph;

namespace RockWeb.Plugins.com_thecrossingchurch.Reporting.DR
{
    [DisplayName( "Series Demographics" )]
    [Category( "com_thecrossingchurch > Reporting > DR" )]
    [Description( "Demographic Data for Selected Series" )]
    [ContentChannelField( "Watch Content Channel", key: AttributeKey.WatchContentChannel, required: true, order: 0 )]
    [InteractionChannelField( "Watch Interaction Channel", key: AttributeKey.WatchInteractionChannel, required: true, order: 1 )]
    [ContentChannelField( "Listen Content Channel", key: AttributeKey.ListenContentChannel, required: true, order: 2 )]
    [InteractionChannelField( "Listen Interaction Channel", key: AttributeKey.ListenInteractionChannel, required: true, order: 3 )]
    public partial class SeriesDemographics : Rock.Web.UI.RockBlock
    {
        #region Keys
        private static class AttributeKey
        {
            public const string WatchContentChannel = "WatchContentChannel";
            public const string WatchInteractionChannel = "WatchInteractionChannel";
            public const string ListenContentChannel = "ListenContentChannel";
            public const string ListenInteractionChannel = "ListenInteractionChannel";
        }
        #endregion

        #region Variables
        private RockContext context { get; set; }
        private Guid? watchChannelGuid { get; set; }
        private ContentChannel watchChannel { get; set; }
        private Guid? watchInteractionChannelGuid { get; set; }
        private InteractionChannel watchInteractionChannel { get; set; }
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
            watchChannelGuid = GetAttributeValue( AttributeKey.WatchContentChannel ).AsGuidOrNull();
            watchInteractionChannelGuid = GetAttributeValue( AttributeKey.WatchInteractionChannel ).AsGuidOrNull();
            watchChannel = new ContentChannelService( context ).Get( watchChannelGuid.Value );
            watchInteractionChannel = new InteractionChannelService( context ).Get( watchInteractionChannelGuid.Value );

            if (!Page.IsPostBack)
            {
                if (Request.Params["isViewersRequest"].AsBoolean())
                {
                    Response.Write( BuildViewersFunnel() );
                    Response.End();
                }
            }
        }
        #endregion

        #region Methods
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
            ", new SqlParameter( "@ChannelId", watchInteractionChannel.Id ), new SqlParameter( "@ContentChannelId", watchChannel.Id ) );
            return JsonConvert.SerializeObject( query );
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