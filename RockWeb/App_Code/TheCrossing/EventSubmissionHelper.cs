using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace RockWeb.TheCrossing
{
    /// <summary>
    /// Summary description for EventSubmissionHelper
    /// </summary>
    public class EventSubmissionHelper
    {
        #region variables
        public List<DefinedValue> Rooms { get; set; }
        public string RoomsJSON { get; set; }
        public List<DefinedValue> Ministries { get; set; }
        public string MinistriesJSON { get; set; }
        public List<DefinedValue> BudgetLines { get; set; }
        public string BudgetLinesJSON { get; set; }
        public int ContentChannelId { get; set; }
        public string BaseURL { get; set; }
        #endregion

        public EventSubmissionHelper( Guid? RoomDefinedTypeGuid, Guid? MinistryDefinedTypeGuid, Guid? BudgetDefinedTypeGuid, Guid? ContentChannelGuid )
        {
            RockContext context = new RockContext();

            if ( RoomDefinedTypeGuid.HasValue )
            {
                int RoomDefinedTypeId = new DefinedTypeService( context ).Get( RoomDefinedTypeGuid.Value ).Id;
                Rooms = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == RoomDefinedTypeId ).ToList();
                Rooms.LoadAttributes();
                RoomsJSON = JsonConvert.SerializeObject( Rooms.Select( dv => new { Id = dv.Id, Value = dv.Value, Type = dv.AttributeValues.FirstOrDefault( av => av.Key == "Type" ).Value.Value, Capacity = dv.AttributeValues.FirstOrDefault( av => av.Key == "Capacity" ).Value.Value.AsInteger(), IsActive = dv.IsActive } ) );
            }

            if ( MinistryDefinedTypeGuid.HasValue )
            {
                int MinistryDefinedTypeId = new DefinedTypeService( context ).Get( MinistryDefinedTypeGuid.Value ).Id;
                Ministries = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == MinistryDefinedTypeId ).OrderBy( dv => dv.Order ).ToList();
                Ministries.LoadAttributes();
                MinistriesJSON = JsonConvert.SerializeObject( Ministries.Select( dv => new { Id = dv.Id, Value = dv.Value, IsPersonal = dv.AttributeValues.FirstOrDefault( av => av.Key == "IsPersonalRequest" ).Value.Value.AsBoolean(), IsActive = dv.IsActive } ) );
            }

            if ( BudgetDefinedTypeGuid.HasValue )
            {
                int BudgetDefinedTypeId = new DefinedTypeService( context ).Get( BudgetDefinedTypeGuid.Value ).Id;
                BudgetLines = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == BudgetDefinedTypeId ).OrderBy( dv => dv.Order ).ToList();
                BudgetLinesJSON = JsonConvert.SerializeObject( BudgetLines.Select( dv => new { Id = dv.Id, Value = dv.Value, IsActive = dv.IsActive } ) );
            }

            if ( ContentChannelGuid.HasValue )
            {
                ContentChannel channel = new ContentChannelService( context ).Get( ContentChannelGuid.Value );
                ContentChannelId = channel.Id;
            }

            Rock.Model.Attribute attr = new AttributeService( context ).Queryable().FirstOrDefault( a => a.Key == "InternalApplicationRoot" );
            if ( attr != null )
            {
                BaseURL = new AttributeValueService( context ).Queryable().FirstOrDefault( av => av.AttributeId == attr.Id ).Value;
                if ( !BaseURL.EndsWith( "/" ) )
                {
                    BaseURL += "/";
                }
            }
        }
    }
}