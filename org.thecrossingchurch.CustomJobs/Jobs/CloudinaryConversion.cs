using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Media.Imaging;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using tech.triumph.WebAgility;

namespace org.thecrossingchurch.CustomJobs.Jobs
{
    [ContentChannelsField( "Channels to Process", "Which channels that we should look for text fields that contianed image urls to convert to a new file type", true, Key = AttributeKey.Channels )]
    public class CloudinaryConversion : RockJob
    {
        private class AttributeKey
        {
            public const string Channels = "Channels";
        }

        private RockContext rockContext;
        private BinaryFileService bf_svc;
        private BinaryFileTypeService bft_svc;
        private ContentChannelItemService cci_svc;
        private AttributeValueService av_svc;
        public override void Execute()
        {
            List<Guid?> channels = GetAttributeValue( AttributeKey.Channels ).Split( ',' ).AsGuidOrNullList();
            rockContext = new RockContext();
            bf_svc = new BinaryFileService( rockContext );
            bft_svc = new BinaryFileTypeService( rockContext );
            cci_svc = new ContentChannelItemService( rockContext );
            av_svc = new AttributeValueService( rockContext );
            ContentChannelService cc_svc = new ContentChannelService( rockContext );
            //Loop through each channel
            for ( int i = 0; i < channels.Count; i++ )
            {
                if ( channels[i].HasValue )
                {
                    //Look for attributes of binary file type and the coresponding text field
                    ContentChannel channel = cc_svc.Get( channels[i].Value );
                    ContentChannelItem item = channel.Items.FirstOrDefault();
                    if ( item != null )
                    {
                        item.LoadAttributes();
                        var binaryAttrs = item.Attributes.Where( a => a.Value.FieldTypeId == 37 ).ToList(); //We will create the binary file, so we will use the existing binary file attribute 
                        if ( binaryAttrs.Any() )
                        {
                            Dictionary<string, string> attributeMap = new Dictionary<string, string>();
                            foreach ( var attribute in binaryAttrs )
                            {
                                //map to the legacy text field for purpose of merge, the binary field will need to have key of Binary_MappedAttributeKey
                                var textAttr = item.Attributes.Where( a => a.Key != attribute.Key && attribute.Key.Contains( a.Key ) ).ToList();
                                if ( textAttr != null )
                                {
                                    attributeMap.Add( attribute.Key, textAttr[0].Key );
                                }
                            }
                            if ( attributeMap.Count > 0 )
                            {
                                //Process each channel item for conversion
                                foreach ( var channelItem in channel.Items )
                                {
                                    ProcessItem( attributeMap, channelItem );
                                }
                            }
                        }
                    }
                }
            }
        }
        private void ProcessItem( Dictionary<string, string> attributeMap, ContentChannelItem item )
        {
            item.LoadAttributes();

            //Create a new binary file item from the text path
            //save it as the attribute value for the binary file attribute 
            foreach ( var map in attributeMap )
            {
                try
                {
                    var currentValue = item.GetAttributeValue( map.Value );
                    var updateValue = item.GetAttributeValue( map.Key );
                    if ( !String.IsNullOrEmpty( currentValue ) && String.IsNullOrEmpty( updateValue ) )
                    { //Only run if we have an attribute value we need to convert
                        //Check other content channel items in this channel for the same image
                        //If we have already converted this file once to Cloudinary we should use its value instead of making a new one
                        var existing = cci_svc.Queryable( "AttributeValues" ).Where( cci => cci.ContentChannelId == item.ContentChannelId && cci.Id != item.Id );
                        var currentTextAttr = item.Attributes.FirstOrDefault( a => a.Key == map.Value );
                        var currentBinaryAttr = item.Attributes.FirstOrDefault( a => a.Key == map.Key );
                        var existingWithAttribute = av_svc.Queryable().Where( av => av.AttributeId == currentTextAttr.Value.Id && av.Value == currentValue )
                            .Join( existing,
                                av => av.EntityId,
                                cci => cci.Id,
                                ( av, cci ) => cci
                            ).Join( av_svc.Queryable().Where( av => av.AttributeId == currentBinaryAttr.Value.Id && !String.IsNullOrEmpty( av.Value ) ),
                                cci => cci.Id,
                                av => av.EntityId,
                                ( cci, av ) => cci
                            ).ToList();

                        BinaryFile binaryFile = null;
                        if ( existingWithAttribute.Count() == 0 )
                        {
                            //Create Binary File
                            binaryFile = new BinaryFile();
                            var binaryFileTypeGuid = item.Attributes[map.Key].ConfigurationValues["binaryFileType"].AsGuidOrNull();
                            if ( binaryFileTypeGuid != null )
                            {
                                var fileType = bft_svc.Get( binaryFileTypeGuid.Value );
                                binaryFile.BinaryFileTypeId = fileType != null ? fileType.Id : 0;
                            }
                            if ( binaryFile.BinaryFileTypeId > 0 )
                            {
                                binaryFile.LoadAttributes();
                                using ( var webClient = new WebClient() )
                                {
                                    byte[] imageBytes = webClient.DownloadData( item.AttributeValues[map.Value].Value );
                                    binaryFile.ContentStream = new MemoryStream( imageBytes );
                                    binaryFile.FileSize = imageBytes.Length;
                                    binaryFile.FileName = item.AttributeValues[map.Value].Value.Split( '/' ).Last();
                                    binaryFile.MimeType = "image/" + item.AttributeValues[map.Value].Value.Replace( ".jpg", ".jpeg" ).Split( '.' ).Last();
                                }

                                try
                                {
                                    binaryFile.SetAttributeValue( "AltText", binaryFile.FileName.Split('.').First() );
                                }
                                catch
                                {
                                    // do nothing if we can't set the alt text 
                                }

                                bf_svc.Add( binaryFile );
                                rockContext.SaveChanges();
                                binaryFile.SaveAttributeValues();
                            }
                        }
                        else
                        {
                            Guid binaryFileGuid = existingWithAttribute.First().GetAttributeValue( map.Key ).AsGuid();
                            binaryFile = bf_svc.Get( binaryFileGuid );
                        }

                        item.SetAttributeValue( map.Key, binaryFile.Guid.ToString() );
                        item.SaveAttributeValues();
                    }
                }
                catch ( Exception ex )
                {
                    ex.Source = "Cloudinary Conversion: " + ex.Source;
                    ex.Data.Add( "Item", item.Id );
                    ex.Data.Add( "Binary File Attribute", map.Key );
                    ex.Data.Add( "Legacy Attribute", map.Value );
                    //If we are unable to log a file, log it to Rock Exceptions
                    ExceptionLogService.LogException( ex );
                }
            }
        }
    }
}
