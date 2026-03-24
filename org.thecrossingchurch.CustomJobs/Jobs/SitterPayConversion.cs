using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Jobs;
using System.ComponentModel;
using RestSharp.Extensions;

namespace org.thecrossingchurch.CustomJobs.Jobs
{
    /// <summary>
    /// Job to supply hubspot contacts that already have rock_ids with other info.
    /// </summary>
    [DisplayName( "Sitter Pay Conversion" )]
    [Description( "Convert Sitter Pay Fields." )]

    [DefinedTypeField( "Designation Defined Type", Key = AttributeKey.DesignationDefinedtype, Order = 1 )]
    [GroupTypesField( "Group Types", Key = AttributeKey.GroupTypes, Order = 2 )]
    [TextField( "Group Designation Key", Key = AttributeKey.GroupDesignationAttrKey, Order = 3 )]
    [IntegerField( "Matrix Template Id", Key = AttributeKey.ConfigMatrixId, Order = 4 )]
    [ContentChannelTypeField( "Content Channel Type", Key = AttributeKey.ContentChannelType, Order = 5 )]
    [TextField( "Matrix Attribute Key", Key = AttributeKey.MatrixAttributeKey, DefaultValue = "SitterPayConfiguration", Order = 6 )]
    public class SitterPayConversion : RockJob
    {
        private class AttributeKey
        {
            public const string DesignationDefinedtype = "DesignationDefinedtype";
            public const string GroupTypes = "GroupTypes";
            public const string GroupDesignationAttrKey = "GroupDesignationAttrKey";
            public const string ConfigMatrixId = "ConfigMatrixId";
            public const string ContentChannelType = "ContentChannelType";
            public const string MatrixAttributeKey = "MatrixAttributeKey";
        }
        public override void Execute()
        {
            Guid? definedTypeGuid = GetAttributeValue( AttributeKey.DesignationDefinedtype ).AsGuidOrNull();
            List<Guid> groupTypeGuids = GetAttributeValue( AttributeKey.GroupTypes ).Split( ',' ).AsGuidList();
            string designationAttrKey = GetAttributeValue( AttributeKey.GroupDesignationAttrKey );
            int? matrixTemplateId = GetAttributeValue( AttributeKey.ConfigMatrixId ).AsIntegerOrNull();
            Guid? contentChannelTypeGuid = GetAttributeValue( AttributeKey.ContentChannelType ).AsGuidOrNull();
            string matrixAttrKey = GetAttributeValue( AttributeKey.MatrixAttributeKey );

            if ( !( definedTypeGuid.HasValue && groupTypeGuids.Count() > 0 && !String.IsNullOrEmpty( designationAttrKey ) && !String.IsNullOrEmpty( matrixAttrKey ) && matrixTemplateId.HasValue && contentChannelTypeGuid.HasValue ) )
            {
                // throw config error
                string jobStatus = "Configuration Error";
                this.UpdateLastStatusMessage( jobStatus );
                throw new RockJobWarningException( jobStatus );
            }
            using ( RockContext context = new RockContext() )
            {
                GroupTypeService gt_svc = new GroupTypeService( context );
                GroupService grp_svc = new GroupService( context );
                AttributeService attr_svc = new AttributeService( context );
                ContentChannelItemService cci_svc = new ContentChannelItemService( context );
                AttributeMatrixService am_svc = new AttributeMatrixService( context );
                AttributeMatrixItemService ami_svc = new AttributeMatrixItemService( context );

                DefinedType designationDT = new DefinedTypeService( context ).Get( definedTypeGuid.Value );
                List<GroupType> groupTypes = new List<GroupType>();
                foreach ( Guid guid in groupTypeGuids )
                {
                    GroupType gt = gt_svc.Get( guid );
                    groupTypes.Add( gt );
                }
                List<int> groupTypeIds = groupTypes.Select( gt => gt.Id ).ToList();
                var groups = grp_svc.Queryable().Where( g => groupTypeIds.Contains( g.GroupTypeId ) ).ToList();

                AttributeMatrixTemplate template = new AttributeMatrixTemplateService( context ).Get( matrixTemplateId.Value );
                template.LoadAttributes();

                ContentChannelType channelType = new ContentChannelTypeService( context ).Get( contentChannelTypeGuid.Value );

                if ( !( designationDT != null && groupTypes.Count() > 0 && template != null && channelType != null ) )
                {
                    // throw data error
                    string jobStatus = "Error pulling data from configuration, check configuration values";
                    this.UpdateLastStatusMessage( jobStatus );
                    throw new RockJobWarningException( jobStatus );
                }

                var items = cci_svc.Queryable().Where( cci => cci.ContentChannelTypeId == channelType.Id ).ToList();

                try
                {
                    designationDT.DefinedValues.LoadAttributes();
                    DefinedValue teamLead = designationDT.DefinedValues.FirstOrDefault( dv => dv.GetAttributeValue( "IsLeader" ) == "True" );
                    DefinedValue juniorSitter = designationDT.DefinedValues.FirstOrDefault( dv => dv.Value.Contains( "Junior" ) );
                    DefinedValue earlySitter = designationDT.DefinedValues.FirstOrDefault( dv => dv.Value.Contains( "Early" ) );
                    DefinedValue sitter = designationDT.DefinedValues.FirstOrDefault( dv => dv.Value == "Sitter" );
                    DefinedValue exemptVolunteer = designationDT.DefinedValues.FirstOrDefault( dv => dv.GetAttributeValue( "IsExempt" ) == "True" && dv.Value.Contains( "Volunteer" ) );
                    DefinedValue exemptStaff = designationDT.DefinedValues.FirstOrDefault( dv => dv.GetAttributeValue( "IsExempt" ) == "True" && dv.Value.Contains( "Intern" ) );
                    //Save new Designation in each group
                    for ( int i = 0; i < groups.Count(); i++ )
                    {
                        Group group = groups[i];
                        group.LoadAttributes();
                        if ( group.GetAttributeValue( "IsLeaderGroup" ) == "True" )
                        {
                            group.SetAttributeValue( designationAttrKey, teamLead.Guid.ToString() );
                            group.SaveAttributeValue( designationAttrKey );
                        }
                        else if ( group.GetAttributeValue( "IsJuniorGroup" ) == "True" )
                        {
                            group.SetAttributeValue( designationAttrKey, juniorSitter.Guid.ToString() );
                            group.SaveAttributeValue( designationAttrKey );
                        }
                        else if ( group.GetAttributeValue( "IsExemptGroup" ) == "True" )
                        {
                            if ( group.Name.Contains( "Intern" ) || group.Name.Contains( "Staff" ) || group.Name.Contains( "Employee" ) )
                            {
                                group.SetAttributeValue( designationAttrKey, exemptStaff.Guid.ToString() );
                                group.SaveAttributeValue( designationAttrKey );

                            }
                            else
                            {
                                group.SetAttributeValue( designationAttrKey, exemptVolunteer.Guid.ToString() );
                                group.SaveAttributeValue( designationAttrKey );
                            }
                        }
                        else if ( group.GetAttributeValue( "IsEarlyGroup" ) == "True" )
                        {
                            group.SetAttributeValue( designationAttrKey, earlySitter.Guid.ToString() );
                            group.SaveAttributeValue( designationAttrKey );
                        }
                        else
                        {
                            group.SetAttributeValue( designationAttrKey, sitter.Guid.ToString() );
                            group.SaveAttributeValue( designationAttrKey );
                        }
                    }

                    for ( int i = 0; i < items.Count(); i++ )
                    {
                        ContentChannelItem item = items[i];
                        item.LoadAttributes();
                        Guid? matrixGuid = item.GetAttributeValue( matrixAttrKey ).AsGuidOrNull();
                        AttributeMatrix matrix;

                        if ( matrixGuid.HasValue )
                        {
                            matrix = am_svc.Get( matrixGuid.Value );
                            matrix.AttributeMatrixItems.LoadAttributes();
                        }
                        else
                        {
                            //Create matrix item
                            matrix = new AttributeMatrix() { AttributeMatrixTemplateId = matrixTemplateId.Value };
                            am_svc.Add( matrix );
                            context.SaveChanges();
                        }

                        if ( matrix == null )
                        {
                            //Log error and continue
                            ExceptionLogService.LogException( new Exception( "Sitter Pay Conversion Error: Unable to create or load matrix." ) );
                            continue;
                        }

                        if ( item.GetAttributeValue( "SitterPay" ).AsInteger() > 0 )
                        {
                            //Add Sitter Pay item
                            AddMatrixItem( context, matrix, item, ami_svc, matrixTemplateId.Value, sitter, "SitterPay" );
                        }
                        if ( item.GetAttributeValue( "TeamLeaderPay" ).AsInteger() > 0 )
                        {
                            //Add Team Lead Pay item
                            AddMatrixItem( context, matrix, item, ami_svc, matrixTemplateId.Value, teamLead, "TeamLeaderPay" );
                        }
                        if ( item.GetAttributeValue( "JuniorLeaderPay" ).AsInteger() > 0 )
                        {
                            //Add Junior Sitter Pay item
                            AddMatrixItem( context, matrix, item, ami_svc, matrixTemplateId.Value, juniorSitter, "JuniorLeaderPay" );
                        }
                        if ( item.GetAttributeValue( "EarlySitterPay" ).AsInteger() > 0 )
                        {
                            //Add Early Sitter Pay item
                            AddMatrixItem( context, matrix, item, ami_svc, matrixTemplateId.Value, earlySitter, "EarlySitterPay" );
                        }

                        item.SetAttributeValue( matrixAttrKey, matrix.Guid.ToString() );
                        item.SaveAttributeValue( matrixAttrKey );
                    }
                }
                catch ( Exception ex )
                {
                    this.UpdateLastStatusMessage( ex.Message );
                    throw new RockJobWarningException( ex.Message );
                }
            }
        }

        private void AddMatrixItem( RockContext context, AttributeMatrix matrix, ContentChannelItem item, AttributeMatrixItemService ami_svc, int matrixTemplateId, DefinedValue designation, string payAttributeKey )
        {
            AttributeMatrixItem matrixItem = matrix.AttributeMatrixItems != null ? matrix.AttributeMatrixItems.FirstOrDefault( mi => mi.GetAttributeValue( "Role" ) == designation.Guid.ToString() ) : null;
            bool isNewItem = false;
            if ( matrixItem == null )
            {
                matrixItem = new AttributeMatrixItem() { AttributeMatrixId = matrix.Id, AttributeMatrixTemplateId = matrixTemplateId };
                matrixItem.LoadAttributes();
                isNewItem = true;
            }
            matrixItem.SetAttributeValue( "Role", designation.Guid.ToString() );
            matrixItem.SetAttributeValue( "Rate", item.GetAttributeValue( payAttributeKey ) );
            if ( isNewItem )
            {
                ami_svc.Add( matrixItem );
                context.SaveChanges();
            }
            matrixItem.SaveAttributeValues();
        }
    }
}
