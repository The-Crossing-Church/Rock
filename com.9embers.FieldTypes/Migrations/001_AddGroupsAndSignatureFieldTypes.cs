//
// Copyright (C) 9 Embers - All Rights Reserved
//
using Rock;
using Rock.Plugin;

namespace com._9embers.AmazonStorageProvider.Migrations
{
    [MigrationNumber( 1, "1.13.0" )]
    public class AddGroupsAndSignatureFieldTypes : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.UpdateFieldType( "Groups (9 Embers)", "Group Picker field that allows selecting multiple groups", "com.9embers.FieldTypes", "com._9embers.FieldTypes.GroupsFieldType", "c802d4d9-034f-45ba-b9b7-26e09b77b103" );
            RockMigrationHelper.UpdateFieldType( "Signature (9 Embers)", "Signature Field", "com.9embers.FieldTypes", "com._9embers.FieldTypes.SignatureFieldType", "8d582fc8-47eb-4ebd-a82a-5a5d5acf4d66" );
            RockMigrationHelper.UpdateFieldType( "Static HTML (9 Embers)", "Static HTML", "com.9embers.FieldTypes", "com._9embers.FieldTypes.StaticHtml", "557cc15f-5345-4dec-b7fe-6645c6b90f02" );

            // Rename Groups Field Type assembly (initially had wrong name)
            Sql( @"
    DECLARE @oId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Assembly] = 'com._9embers.FieldTypes' AND [Class] = 'com._9embers.FieldTypes.GroupsFieldType' )
    DECLARE @nId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = 'c802d4d9-034f-45ba-b9b7-26e09b77b103' )
    UPDATE [Attribute] SET [FieldTypeId] = @nId WHERE [FieldTypeId] = @oId
    DELETE [FieldType] WHERE [Id] = @oId
" );

            // Rename Pillars SignatureFieldType to 9 Embers 
            Sql( @"
    DECLARE @pId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Class] = 'rocks.pillars.SignatureField.Field.Types.SignatureFieldType' )
    DECLARE @eId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '8d582fc8-47eb-4ebd-a82a-5a5d5acf4d66' )
    UPDATE [Attribute] SET [FieldTypeId] = @eId WHERE [FieldTypeId] = @pId
    DELETE [FieldType] WHERE [Id] = @pId
" );

            // Rename Pillars Static HTML field type to 9 Embers 
            Sql( @"
    DECLARE @pId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Class] = 'rocks.pillars.FieldTypes.StaticHTML' )
    DECLARE @eId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '557cc15f-5345-4dec-b7fe-6645c6b90f02' )
    UPDATE [Attribute] SET [FieldTypeId] = @eId WHERE [FieldTypeId] = @pId
    DELETE [FieldType] WHERE [Id] = @pId
" );
    }

    public override void Down()
        {
        }
    }
}
