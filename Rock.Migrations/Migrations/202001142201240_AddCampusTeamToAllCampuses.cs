// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Rock.Migrations.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class AddCampusTeamToAllCampuses : Rock.Migrations.RockMigration
    {
        private static class Guids
        {
            public const string GROUP_TYPE_CAMPUS_TEAM = SystemGuid.GroupType.GROUPTYPE_CAMPUS_TEAM;
            public const string GROUP_TYPE_ROLE_CAMPUS_PASTOR = "F8C6289B-0E68-4121-A595-A51369404EBA";
            public const string GROUP_TYPE_ROLE_CAMPUS_ADMINISTRATOR = "07F857ED-C0D7-47B4-AB6C-9AFDFAE2ADD9";
        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add 'Campus Team' GroupType
            RockMigrationHelper.AddGroupType( "Campus Team", "Used to track groups that serve a given Campus.", "Group", "Member", false, false, false, null, 0, null, 0, null, Guids.GROUP_TYPE_CAMPUS_TEAM, true );

            // Add default Roles to 'Campus Team' GroupType
            RockMigrationHelper.AddGroupTypeRole( Guids.GROUP_TYPE_CAMPUS_TEAM, "Campus Pastor", "Pastor of a Campus", 0, 1, null, Guids.GROUP_TYPE_ROLE_CAMPUS_PASTOR, true, true, false );
            RockMigrationHelper.AddGroupTypeRole( Guids.GROUP_TYPE_CAMPUS_TEAM, "Campus Administrator", "Administrator of a Campus", 1, null, null, Guids.GROUP_TYPE_ROLE_CAMPUS_ADMINISTRATOR, true, false, true );

            // Add 'Group Member Detail' Page to 'Campus Detail' Page
            RockMigrationHelper.AddPage( true, "BDD7B906-4D42-43C0-8DBB-B89A566734D8", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Member Detail", "", "EB135AE0-5BAC-458B-AD5B-47460C2BFD31", "fa fa-users" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "EB135AE0-5BAC-458B-AD5B-47460C2BFD31", "Campus/{CampusId}/GroupMember/{GroupMemberId}", "9660B9FB-C90F-4AFE-9D58-C0EC271C1377" ); // for Page:Group Member Detail

            // ----- Add 'Group Member List' Block to 'Campus Detail' Page;
            
            // Add Block to Page: Campus Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "BDD7B906-4D42-43C0-8DBB-B89A566734D8".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "88B7EFA9-7419-4D05-9F88-38B936E61EDD".AsGuid(), "Group Member List", "Main", @"", @"", 1, "318B80EE-7349-4BF4-82F2-64FC38A5AB0B" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '176FFC6F-6B55-4319-A781-A2F7F1F85F24'" );  // Page: Campus Detail,  Zone: Main,  Block: Campus Detail
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '318B80EE-7349-4BF4-82F2-64FC38A5AB0B'" );  // Page: Campus Detail,  Zone: Main,  Block: Group Member List

//            // Attrib for BlockType: Content:Content
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9191415-F37F-426F-B032-356E7551FA74", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content", "Content", "Content", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "194ADD33-BD9D-4640-868C-80EB75602AC8" );
//            // Attrib for BlockType: Content:Enabled Lava Commands
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9191415-F37F-426F-B032-356E7551FA74", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "AFBA2BBD-94CD-468D-B658-2E92D5B3D3FE" );
//            // Attrib for BlockType: Content:Dynamic Content
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9191415-F37F-426F-B032-356E7551FA74", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Dynamic Content", "DynamicContent", "Dynamic Content", @"If enabled then the client will download fresh content from the server every period of Cache Duration, otherwise the content will remain static.", 0, @"False", "5887F89F-E61C-4BA7-832D-A18BF3909DFF" );
//            // Attrib for BlockType: Content:Cache Duration
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9191415-F37F-426F-B032-356E7551FA74", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "C28EE36A-FFC0-4C1D-89D5-8FF647E9A064" );
//            // Attrib for BlockType: Content:Lava Render Location
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9191415-F37F-426F-B032-356E7551FA74", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Lava Render Location", "LavaRenderLocation", "Lava Render Location", @"Specifies where to render the Lava", 2, @"On Server", "3F42817F-0C9F-4809-A845-854FC73AA903" );
//            // Attrib for BlockType: Content:Callback Logic
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F9191415-F37F-426F-B032-356E7551FA74", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Callback Logic", "CallbackLogic", "Callback Logic", @"If you provided any callback commands in your Content then you can specify the Lava logic for handling those commands here. <span class='tip tip-laval'></span>", 0, @"", "1E437D65-39DA-499C-8517-FD8C5F13B2F4" );
//            // Attrib for BlockType: Content Channel Item List:Content Channel
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9467B5F2-3489-4D19-AE85-C8F250DE8DF2", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"The content channel to retrieve the items for.", 1, @"", "F3638C5B-90ED-4FFB-8C43-BD73E22427BB" );
//            // Attrib for BlockType: Content Channel Item List:Page Size
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9467B5F2-3489-4D19-AE85-C8F250DE8DF2", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 2, @"50", "4FB6B697-7629-4A2C-80AB-2AEAC226A24E" );
//            // Attrib for BlockType: Content Channel Item List:Include Following
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9467B5F2-3489-4D19-AE85-C8F250DE8DF2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Following", "IncludeFollowing", "Include Following", @"Determines if following data should be sent along with the results.", 3, @"False", "682284FF-7F51-4027-B42A-34BC0198A499" );
//            // Attrib for BlockType: Content Channel Item List:Detail Page
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9467B5F2-3489-4D19-AE85-C8F250DE8DF2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 4, @"", "50093051-42E6-40BF-B84F-AEFF48870B6C" );
//            // Attrib for BlockType: Content Channel Item List:Field Settings
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9467B5F2-3489-4D19-AE85-C8F250DE8DF2", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Field Settings", "FieldSettings", "Field Settings", @"JSON object of the configured fields to show.", 5, @"", "50B09932-DF1B-4E00-8558-83027D66D933" );
//            // Attrib for BlockType: Content Channel Item List:Filter Id
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9467B5F2-3489-4D19-AE85-C8F250DE8DF2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filter Id", "FilterId", "Filter Id", @"The data filter that is used to filter items", 6, @"0", "9EA2EB6C-EDB2-47D7-8BE7-125D26C30936" );
//            // Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9467B5F2-3489-4D19-AE85-C8F250DE8DF2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Query Parameter Filtering", "QueryParameterFiltering", "Query Parameter Filtering", @"Determines if block should evaluate the query string parameters for additional filter criteria.", 7, @"False", "64C2D75F-F895-4A8B-B494-65CF73816902" );
//            // Attrib for BlockType: Content Channel Item List:Show Children of Parent
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9467B5F2-3489-4D19-AE85-C8F250DE8DF2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Children of Parent", "ShowChildrenOfParent", "Show Children of Parent", @"If enabled the block will look for a passed ParentItemId parameter and if found filter for children of this parent item.", 8, @"False", "39DA3BA5-7FB7-4415-8B71-E54A446D3C2C" );
//            // Attrib for BlockType: Content Channel Item List:Check Item Security
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9467B5F2-3489-4D19-AE85-C8F250DE8DF2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Check Item Security", "CheckItemSecurity", "Check Item Security", @"Determines if the security of each item should be checked. Recommend not checking security of each item unless required.", 9, @"False", "A9893331-30AD-45A3-ADD2-F161A3455AE8" );
//            // Attrib for BlockType: Content Channel Item List:Order
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9467B5F2-3489-4D19-AE85-C8F250DE8DF2", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Order", "Order", "Order", @"The specifics of how items should be ordered. This value is set through configuration and should not be modified here.", 10, @"", "7312E700-5BD9-4FED-9C3D-8956DF4CA66B" );
//            // Attrib for BlockType: Content Channel Item List:List Data Template
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9467B5F2-3489-4D19-AE85-C8F250DE8DF2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
//    <Label Text=""{Binding Content}"" />
//</StackLayout>", "52C99DEE-2D9E-4F87-8A59-B0861F90060B" );
//            // Attrib for BlockType: Content Channel Item List:Cache Duration
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9467B5F2-3489-4D19-AE85-C8F250DE8DF2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "B725C0B0-8938-459A-BB5D-EA09A239BADE" );
//            // Attrib for BlockType: Content Channel Item View:Content Template
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29119D4B-A93B-4CBC-9A20-DA926904F039", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content Template", "ContentTemplate", "Content Template", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "B80C9D5F-9F30-42C0-900B-2C93A73FD34A" );
//            // Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29119D4B-A93B-4CBC-9A20-DA926904F039", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "CB22F9DD-D505-41F8-BC27-2EDB77FD5CE3" );
//            // Attrib for BlockType: Content Channel Item View:Content Channel
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29119D4B-A93B-4CBC-9A20-DA926904F039", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"Limits content channel items to a specific channel.", 2, @"", "DA5CE7B1-26BA-4ABB-AC2B-CBD7FEC2F9BF" );
//            // Attrib for BlockType: Content Channel Item View:Log Interactions
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29119D4B-A93B-4CBC-9A20-DA926904F039", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Log Interactions", "LogInteractions", "Log Interactions", @"If enabled then an interaction will be saved when the user views the content channel item.", 3, @"False", "A90E56B3-091A-40BB-8893-D2B8BA1F04D2" );
//            // Attrib for BlockType: Lava Item List:Page Size
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C2E23A0-EA2B-4980-A69E-8517DEFD619A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 0, @"50", "748DF3D9-EA43-47FA-967A-0F47B94FC914" );
//            // Attrib for BlockType: Lava Item List:Detail Page
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C2E23A0-EA2B-4980-A69E-8517DEFD619A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 1, @"", "DFE85BE5-0823-43F9-BE3A-590FF4BB74FB" );
//            // Attrib for BlockType: Lava Item List:List Template
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C2E23A0-EA2B-4980-A69E-8517DEFD619A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Template", "ListTemplate", "List Template", @"The Lava used to generate the JSON object structure for the item list.", 2, @"[
//  {
//    ""Id"": 1,
//    ""Title"": ""First Item""
//  },
//  {
//    ""Id"": 2,
//    ""Title"": ""Second Item""
//  }
//]", "A4E68B05-F8B4-4574-932A-DD7219B29005" );
//            // Attrib for BlockType: Lava Item List:List Data Template
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C2E23A0-EA2B-4980-A69E-8517DEFD619A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
//    <Label Text=""{Binding Title}"" />
//</StackLayout>", "4B0FA3B9-7C50-499C-85B0-96A672EC9AD2" );
//            // Attrib for BlockType: Login:Registration Page
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C61E572D-BAA8-436C-B421-95F063C26FDD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page that will be used to register the user.", 0, @"", "A0843CEB-DBD9-4C64-AEDB-EC103290D451" );
//            // Attrib for BlockType: Login:Forgot Password Url
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C61E572D-BAA8-436C-B421-95F063C26FDD", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "Forgot Password Url", "ForgotPasswordUrl", "Forgot Password Url", @"The URL to link the user to when they have forgotton their password.", 1, @"", "FA1F9248-D787-433D-B5ED-22AADCDAAF5A" );
//            // Attrib for BlockType: Profile Details:Connection Status
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8267EF1D-AFBD-44E8-8D97-359761D1C059", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "236D44C5-3DAA-4792-A750-F78754713DA9" );
//            // Attrib for BlockType: Profile Details:Record Status
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8267EF1D-AFBD-44E8-8D97-359761D1C059", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "838AFF3A-CC53-44CD-8E1D-686B9129F799" );
//            // Attrib for BlockType: Profile Details:Birthdate Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8267EF1D-AFBD-44E8-8D97-359761D1C059", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "F148105C-6F15-42D4-B3E8-8926E988F338" );
//            // Attrib for BlockType: Profile Details:BirthDate Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8267EF1D-AFBD-44E8-8D97-359761D1C059", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "68B03429-0CAA-4DA5-904D-B6365EB7EF76" );
//            // Attrib for BlockType: Profile Details:Campus Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8267EF1D-AFBD-44E8-8D97-359761D1C059", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "1A8D9294-AA52-4FBE-AC7C-757F050433DC" );
//            // Attrib for BlockType: Profile Details:Campus Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8267EF1D-AFBD-44E8-8D97-359761D1C059", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "8411487A-C68B-4781-8CF4-30AEC8A03408" );
//            // Attrib for BlockType: Profile Details:Email Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8267EF1D-AFBD-44E8-8D97-359761D1C059", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "89B4CBFD-E95D-46A3-BA49-B50894C67C2C" );
//            // Attrib for BlockType: Profile Details:Email Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8267EF1D-AFBD-44E8-8D97-359761D1C059", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "ED4D9F8D-9D5D-4D76-AB23-BB5FF60B0A6F" );
//            // Attrib for BlockType: Profile Details:Mobile Phone Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8267EF1D-AFBD-44E8-8D97-359761D1C059", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "879BAF1A-1E75-4FA2-B76E-FF0A583C6F7F" );
//            // Attrib for BlockType: Profile Details:Mobile Phone Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8267EF1D-AFBD-44E8-8D97-359761D1C059", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "CF1AADA5-A8C8-40EF-98BF-A919F89EB5A8" );
//            // Attrib for BlockType: Profile Details:Address Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8267EF1D-AFBD-44E8-8D97-359761D1C059", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Show", "AddressShow", "Address Show", @"Determines whether the address field will be available for input.", 8, @"True", "724AADCA-8FEF-41B2-A4D3-4CE596AE58C0" );
//            // Attrib for BlockType: Profile Details:Address Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8267EF1D-AFBD-44E8-8D97-359761D1C059", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Required", "AddressRequired", "Address Required", @"Requires that a address value be entered before allowing the user to register.", 9, @"True", "9039BEB3-B50C-4D9A-AB21-514120BAA397" );
//            // Attrib for BlockType: Register:Connection Status
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41655361-B703-43E6-AB8D-7DEB74EC1132", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "FD21F8FC-287B-4C01-95B9-FD5BE30FF3AB" );
//            // Attrib for BlockType: Register:Record Status
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41655361-B703-43E6-AB8D-7DEB74EC1132", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "8ECD8656-AD01-42FF-A31B-723A2DF0BF0E" );
//            // Attrib for BlockType: Register:Birthdate Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41655361-B703-43E6-AB8D-7DEB74EC1132", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "D0A53168-D333-4262-98BB-CC59BAB0C732" );
//            // Attrib for BlockType: Register:BirthDate Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41655361-B703-43E6-AB8D-7DEB74EC1132", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "48EAF240-62EC-4D33-A814-8A571A778EFF" );
//            // Attrib for BlockType: Register:Campus Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41655361-B703-43E6-AB8D-7DEB74EC1132", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "77436FC9-F46E-43E6-8CD6-94B9B64ECED7" );
//            // Attrib for BlockType: Register:Campus Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41655361-B703-43E6-AB8D-7DEB74EC1132", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "5B8627C8-DB3B-4025-AAFB-5F517E107A1A" );
//            // Attrib for BlockType: Register:Email Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41655361-B703-43E6-AB8D-7DEB74EC1132", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "25AC69F4-722C-4768-A120-FCA30CB60FC5" );
//            // Attrib for BlockType: Register:Email Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41655361-B703-43E6-AB8D-7DEB74EC1132", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "B78DB556-A13E-49DC-84B7-CD8B2A2D2277" );
//            // Attrib for BlockType: Register:Mobile Phone Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41655361-B703-43E6-AB8D-7DEB74EC1132", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "59952950-5765-404C-B98D-4FEF4CA9586C" );
//            // Attrib for BlockType: Register:Mobile Phone Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41655361-B703-43E6-AB8D-7DEB74EC1132", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "12F0DCD9-12AF-426F-BB29-FCB0BF135686" );
//            // Attrib for BlockType: Workflow Entry:Workflow Type
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6785A5A4-AF4D-4070-A86E-DFD81E0B99A6", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "Workflow Type", @"The type of workflow to launch when viewing this.", 0, @"", "FCDB0796-5FA2-4ADC-853E-452BF87C8470" );
//            // Attrib for BlockType: Workflow Entry:Completion Action
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6785A5A4-AF4D-4070-A86E-DFD81E0B99A6", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform when there is nothing left for the user to do.", 1, @"0", "9BC9EB27-5AED-4486-B63C-D689716A78A7" );
//            // Attrib for BlockType: Workflow Entry:Completion Xaml
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6785A5A4-AF4D-4070-A86E-DFD81E0B99A6", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the Completion Action is set to Show Completion Xaml. <span class='tip tip-lava'></span>", 2, @"", "E6EF339B-52A6-4A18-B983-FED94F1DA297" );
//            // Attrib for BlockType: Workflow Entry:Enabled Lava Commands
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6785A5A4-AF4D-4070-A86E-DFD81E0B99A6", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 3, @"", "E00CC393-6B46-4D62-8A2B-B1D4C73E4CD1" );
//            // Attrib for BlockType: Workflow Entry:Redirect To Page
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6785A5A4-AF4D-4070-A86E-DFD81E0B99A6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect To Page", "RedirectToPage", "Redirect To Page", @"The page the user will be redirected to if the Completion Action is set to Redirect to Page.", 4, @"", "FAEE8B9C-22DD-4A16-986E-6B92F31121EA" );

            // Attrib Value for Block:Group Member List, Attribute:core.CustomGridEnableStickyHeaders Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "318B80EE-7349-4BF4-82F2-64FC38A5AB0B", "7F31B5A8-96F6-4D30-9BB6-3EB2DBE26234", @"False" );
            // Attrib Value for Block:Group Member List, Attribute:Show Note Column Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "318B80EE-7349-4BF4-82F2-64FC38A5AB0B", "5F54C068-1418-44FA-B215-FBF70072F6A5", @"False" );
            // Attrib Value for Block:Group Member List, Attribute:Show Date Added Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "318B80EE-7349-4BF4-82F2-64FC38A5AB0B", "F281090E-A05D-4F81-AD80-A3599FB8E2CD", @"False" );
            // Attrib Value for Block:Group Member List, Attribute:Show Campus Filter Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "318B80EE-7349-4BF4-82F2-64FC38A5AB0B", "65B9EA6C-D904-4105-8B51-CCA784DDAAFA", @"True" );
            // Attrib Value for Block:Group Member List, Attribute:Show First/Last Attendance Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "318B80EE-7349-4BF4-82F2-64FC38A5AB0B", "65834FB0-0AB0-4F73-BE1B-9D2F9FFD2664", @"False" );
            // Attrib Value for Block:Group Member List, Attribute:Detail Page Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "318B80EE-7349-4BF4-82F2-64FC38A5AB0B", "E4CCB79C-479F-4BEE-8156-969B2CE05973", @"eb135ae0-5bac-458b-ad5b-47460c2bfd31,9660b9fb-c90f-4afe-9d58-c0ec271c1377" );

            // ----- Add 'Group Member Detail' Block to 'Group Member Detail' Page;

            // Add Block to Page: Group Member Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "EB135AE0-5BAC-458B-AD5B-47460C2BFD31".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "AAE2E5C3-9279-4AB0-9682-F4D19519D678".AsGuid(), "Group Member Detail", "Main", @"", @"", 0, "96361229-3CF1-4713-84B5-E913AECDC804" );

//            // Attrib for BlockType: Content:Content
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E70D584A-5479-482E-B1D3-4D03DD79DB43", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content", "Content", "Content", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "86AA46A8-BC05-4351-B179-C3E25BA5FBDB" );
//            // Attrib for BlockType: Content:Enabled Lava Commands
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E70D584A-5479-482E-B1D3-4D03DD79DB43", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "7DB18992-3B16-4A9F-9A38-8EFD5C561AD4" );
//            // Attrib for BlockType: Content:Dynamic Content
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E70D584A-5479-482E-B1D3-4D03DD79DB43", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Dynamic Content", "DynamicContent", "Dynamic Content", @"If enabled then the client will download fresh content from the server every period of Cache Duration, otherwise the content will remain static.", 0, @"False", "BE0E4C27-12F2-4D56-9FF4-7CD1F44ED419" );
//            // Attrib for BlockType: Content:Cache Duration
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E70D584A-5479-482E-B1D3-4D03DD79DB43", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "DDD5875C-0BE8-452C-8840-F41387E15BD8" );
//            // Attrib for BlockType: Content:Lava Render Location
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E70D584A-5479-482E-B1D3-4D03DD79DB43", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Lava Render Location", "LavaRenderLocation", "Lava Render Location", @"Specifies where to render the Lava", 2, @"On Server", "195BCC27-B3D2-4B94-9712-446BC027DCD3" );
//            // Attrib for BlockType: Content:Callback Logic
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E70D584A-5479-482E-B1D3-4D03DD79DB43", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Callback Logic", "CallbackLogic", "Callback Logic", @"If you provided any callback commands in your Content then you can specify the Lava logic for handling those commands here. <span class='tip tip-laval'></span>", 0, @"", "0ED11D3A-85FD-4505-A428-04BC62482DBC" );
//            // Attrib for BlockType: Content Channel Item List:Content Channel
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3164ECA4-C79A-4743-9B17-03D6B3CB1053", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"The content channel to retrieve the items for.", 1, @"", "6DCBE858-2B9A-45D5-BBDB-D24E0ABA0057" );
//            // Attrib for BlockType: Content Channel Item List:Page Size
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3164ECA4-C79A-4743-9B17-03D6B3CB1053", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 2, @"50", "A1F5BD92-CF3D-424C-AE5E-87C7BEC1855F" );
//            // Attrib for BlockType: Content Channel Item List:Include Following
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3164ECA4-C79A-4743-9B17-03D6B3CB1053", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Following", "IncludeFollowing", "Include Following", @"Determines if following data should be sent along with the results.", 3, @"False", "C1480455-387D-4F01-94E5-DA1D47F8FCDF" );
//            // Attrib for BlockType: Content Channel Item List:Detail Page
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3164ECA4-C79A-4743-9B17-03D6B3CB1053", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 4, @"", "0D77E5F9-B706-4645-B129-CA93BFE6C392" );
//            // Attrib for BlockType: Content Channel Item List:Field Settings
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3164ECA4-C79A-4743-9B17-03D6B3CB1053", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Field Settings", "FieldSettings", "Field Settings", @"JSON object of the configured fields to show.", 5, @"", "0254CAE5-207C-406B-BCB0-6A5F6A0128C9" );
//            // Attrib for BlockType: Content Channel Item List:Filter Id
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3164ECA4-C79A-4743-9B17-03D6B3CB1053", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filter Id", "FilterId", "Filter Id", @"The data filter that is used to filter items", 6, @"0", "5309A4E3-953E-4B74-822B-6179CC7B692A" );
//            // Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3164ECA4-C79A-4743-9B17-03D6B3CB1053", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Query Parameter Filtering", "QueryParameterFiltering", "Query Parameter Filtering", @"Determines if block should evaluate the query string parameters for additional filter criteria.", 7, @"False", "1FF19852-C9B9-4C35-BCB9-4B104E1220F6" );
//            // Attrib for BlockType: Content Channel Item List:Show Children of Parent
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3164ECA4-C79A-4743-9B17-03D6B3CB1053", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Children of Parent", "ShowChildrenOfParent", "Show Children of Parent", @"If enabled the block will look for a passed ParentItemId parameter and if found filter for children of this parent item.", 8, @"False", "B882A7AC-3BAB-4F1B-B20D-1EFC0EEAB4D1" );
//            // Attrib for BlockType: Content Channel Item List:Check Item Security
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3164ECA4-C79A-4743-9B17-03D6B3CB1053", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Check Item Security", "CheckItemSecurity", "Check Item Security", @"Determines if the security of each item should be checked. Recommend not checking security of each item unless required.", 9, @"False", "8914BA86-E477-4B9F-9632-93C72BDB2338" );
//            // Attrib for BlockType: Content Channel Item List:Order
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3164ECA4-C79A-4743-9B17-03D6B3CB1053", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Order", "Order", "Order", @"The specifics of how items should be ordered. This value is set through configuration and should not be modified here.", 10, @"", "58289A07-3D71-4C51-9EDB-3E70D13C1908" );
//            // Attrib for BlockType: Content Channel Item List:List Data Template
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3164ECA4-C79A-4743-9B17-03D6B3CB1053", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
//    <Label Text=""{Binding Content}"" />
//</StackLayout>", "3BEF56A0-0E4F-42B1-B33B-1A28928E93F4" );
//            // Attrib for BlockType: Content Channel Item List:Cache Duration
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3164ECA4-C79A-4743-9B17-03D6B3CB1053", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "Cache Duration", @"The number of seconds the data should be cached on the client before it is requested from the server again. A value of 0 means always reload.", 1, @"86400", "E687A972-35CA-4512-ADA3-B201F3D5EFA3" );
//            // Attrib for BlockType: Content Channel Item View:Content Template
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88FF703E-9B33-4049-86A2-0FB4282A2128", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Content Template", "ContentTemplate", "Content Template", @"The XAML to use when rendering the block. <span class='tip tip-lava'></span>", 0, @"", "FEFC7AE3-590C-4B85-A284-ACDDF676D13D" );
//            // Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88FF703E-9B33-4049-86A2-0FB4282A2128", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block, only affects Lava rendered on the server.", 1, @"", "218DC0FC-77DF-4597-A16A-1E74200D24C8" );
//            // Attrib for BlockType: Content Channel Item View:Content Channel
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88FF703E-9B33-4049-86A2-0FB4282A2128", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "Content Channel", @"Limits content channel items to a specific channel.", 2, @"", "A4E2F22C-1B48-4982-AD28-DDB7B2257FA8" );
//            // Attrib for BlockType: Content Channel Item View:Log Interactions
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88FF703E-9B33-4049-86A2-0FB4282A2128", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Log Interactions", "LogInteractions", "Log Interactions", @"If enabled then an interaction will be saved when the user views the content channel item.", 3, @"False", "8B858484-042A-4BBF-A8EB-9A187C92331C" );
//            // Attrib for BlockType: Lava Item List:Page Size
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "87653CB9-B97E-4CD6-B423-8952C65A8F61", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Page Size", "PageSize", "Page Size", @"The number of items to send per page.", 0, @"50", "9CB5ED90-765C-431C-B3A2-D80E2C658AD8" );
//            // Attrib for BlockType: Lava Item List:Detail Page
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "87653CB9-B97E-4CD6-B423-8952C65A8F61", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to redirect to when selecting an item.", 1, @"", "4CF14666-94D8-453C-B941-943BC7BD8459" );
//            // Attrib for BlockType: Lava Item List:List Template
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "87653CB9-B97E-4CD6-B423-8952C65A8F61", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Template", "ListTemplate", "List Template", @"The Lava used to generate the JSON object structure for the item list.", 2, @"[
//  {
//    ""Id"": 1,
//    ""Title"": ""First Item""
//  },
//  {
//    ""Id"": 2,
//    ""Title"": ""Second Item""
//  }
//]", "CFC98FB2-BE52-48B8-B59B-C8B8FB3533D5" );
//            // Attrib for BlockType: Lava Item List:List Data Template
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "87653CB9-B97E-4CD6-B423-8952C65A8F61", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Data Template", "ListDataTemplate", "List Data Template", @"The XAML for the lists data template.", 0, @"<StackLayout HeightRequest=""50"" WidthRequest=""200"" Orientation=""Horizontal"" Padding=""0,5,0,5"">
//    <Label Text=""{Binding Title}"" />
//</StackLayout>", "F835AF4D-BD2F-41FB-9B0E-D2E8055381E1" );
//            // Attrib for BlockType: Login:Registration Page
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "34E8DA76-B510-4761-8007-F2B103007693", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page that will be used to register the user.", 0, @"", "28427F89-48DA-4202-9547-CDF6B554ED78" );
//            // Attrib for BlockType: Login:Forgot Password Url
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "34E8DA76-B510-4761-8007-F2B103007693", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "Forgot Password Url", "ForgotPasswordUrl", "Forgot Password Url", @"The URL to link the user to when they have forgotton their password.", 1, @"", "C21E2C7A-37AD-41F0-8BC7-8D5C226AC5EA" );
//            // Attrib for BlockType: Profile Details:Connection Status
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "326771C7-DD22-4E88-B727-74EC84133424", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "85EDD699-DD2F-4122-AA6B-6B9A5C19098E" );
//            // Attrib for BlockType: Profile Details:Record Status
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "326771C7-DD22-4E88-B727-74EC84133424", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "F3EDD57C-6CF9-4862-B908-55931DE748DC" );
//            // Attrib for BlockType: Profile Details:Birthdate Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "326771C7-DD22-4E88-B727-74EC84133424", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "61A82CA0-CDA3-4DA8-9F9C-C237918B0041" );
//            // Attrib for BlockType: Profile Details:BirthDate Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "326771C7-DD22-4E88-B727-74EC84133424", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "A55B5F75-68A7-48DF-89D4-E351379ABDAF" );
//            // Attrib for BlockType: Profile Details:Campus Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "326771C7-DD22-4E88-B727-74EC84133424", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "B91ADA6F-B1CE-491F-AB50-0201F47B7211" );
//            // Attrib for BlockType: Profile Details:Campus Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "326771C7-DD22-4E88-B727-74EC84133424", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "456BDBAB-8427-4D10-946C-C36449F71755" );
//            // Attrib for BlockType: Profile Details:Email Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "326771C7-DD22-4E88-B727-74EC84133424", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "32BF88F5-B9A6-4E09-B27B-80C38CFDADDF" );
//            // Attrib for BlockType: Profile Details:Email Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "326771C7-DD22-4E88-B727-74EC84133424", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "5D6CAF8A-25DE-40DF-83C8-F530BEAE68BE" );
//            // Attrib for BlockType: Profile Details:Mobile Phone Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "326771C7-DD22-4E88-B727-74EC84133424", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "6618D251-AF6B-4664-90AA-FA4D06AFE36F" );
//            // Attrib for BlockType: Profile Details:Mobile Phone Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "326771C7-DD22-4E88-B727-74EC84133424", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "7C5D273C-5D7D-43BB-8843-8D2BEF6291E2" );
//            // Attrib for BlockType: Profile Details:Address Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "326771C7-DD22-4E88-B727-74EC84133424", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Show", "AddressShow", "Address Show", @"Determines whether the address field will be available for input.", 8, @"True", "E1D8FFD0-0F77-42DE-A055-FCA1B227DEFB" );
//            // Attrib for BlockType: Profile Details:Address Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "326771C7-DD22-4E88-B727-74EC84133424", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Required", "AddressRequired", "Address Required", @"Requires that a address value be entered before allowing the user to register.", 9, @"True", "B2B37992-B2F6-4812-8F45-13FE994E4B46" );
//            // Attrib for BlockType: Register:Connection Status
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ACDF252C-3692-4EDD-B4D9-3975D9AADB79", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default = 'Web Prospect'.)", 11, @"368DD475-242C-49C4-A42C-7278BE690CC2", "91B1C2AD-0B09-4156-83AC-A2049059044E" );
//            // Attrib for BlockType: Register:Record Status
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ACDF252C-3692-4EDD-B4D9-3975D9AADB79", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default = 'Pending'.)", 12, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "7A8BF804-FD1D-494A-8EF8-6F6B3EDB6CB6" );
//            // Attrib for BlockType: Register:Birthdate Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ACDF252C-3692-4EDD-B4D9-3975D9AADB79", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birthdate Show", "BirthDateShow", "Birthdate Show", @"Determines whether the birthdate field will be available for input.", 0, @"True", "CBD0AAD4-9420-44BF-94CC-CD10719E8F09" );
//            // Attrib for BlockType: Register:BirthDate Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ACDF252C-3692-4EDD-B4D9-3975D9AADB79", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "BirthDate Required", "BirthDateRequired", "BirthDate Required", @"Requires that a birthdate value be entered before allowing the user to register.", 1, @"True", "312898DF-2D8F-405B-8728-956A90191D96" );
//            // Attrib for BlockType: Register:Campus Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ACDF252C-3692-4EDD-B4D9-3975D9AADB79", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Show", "CampusShow", "Campus Show", @"Determines whether the campus field will be available for input.", 2, @"True", "DCC54247-AF3F-4FBE-B992-F7E5933D5F58" );
//            // Attrib for BlockType: Register:Campus Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ACDF252C-3692-4EDD-B4D9-3975D9AADB79", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Campus Required", "CampusRequired", "Campus Required", @"Requires that a campus value be entered before allowing the user to register.", 3, @"True", "070A2F4D-0E73-4159-8062-D76785BF62EC" );
//            // Attrib for BlockType: Register:Email Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ACDF252C-3692-4EDD-B4D9-3975D9AADB79", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Show", "EmailShow", "Email Show", @"Determines whether the email field will be available for input.", 4, @"True", "5D0B3AE2-774C-4146-970C-B380800CCA31" );
//            // Attrib for BlockType: Register:Email Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ACDF252C-3692-4EDD-B4D9-3975D9AADB79", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Email Required", "EmailRequired", "Email Required", @"Requires that a email value be entered before allowing the user to register.", 5, @"True", "22FD39AA-F893-4CB2-B5C3-BBA47AFA6480" );
//            // Attrib for BlockType: Register:Mobile Phone Show
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ACDF252C-3692-4EDD-B4D9-3975D9AADB79", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Show", "MobilePhoneShow", "Mobile Phone Show", @"Determines whether the mobile phone field will be available for input.", 6, @"True", "FCDA5253-CFE7-4569-A940-7E5036754E38" );
//            // Attrib for BlockType: Register:Mobile Phone Required
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ACDF252C-3692-4EDD-B4D9-3975D9AADB79", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Mobile Phone Required", "MobilePhoneRequired", "Mobile Phone Required", @"Requires that a mobile phone value be entered before allowing the user to register.", 7, @"True", "D5A850D6-6347-4DF3-BCFC-0A0C59DB9C10" );
//            // Attrib for BlockType: Workflow Entry:Workflow Type
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E029A00E-943A-4EEF-A671-61D13354AC8A", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "Workflow Type", @"The type of workflow to launch when viewing this.", 0, @"", "7F262C6C-1F66-4062-9FBE-8D51769BACA7" );
//            // Attrib for BlockType: Workflow Entry:Completion Action
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E029A00E-943A-4EEF-A671-61D13354AC8A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform when there is nothing left for the user to do.", 1, @"0", "08956A4B-5D97-42C6-9262-F405E279137C" );
//            // Attrib for BlockType: Workflow Entry:Completion Xaml
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E029A00E-943A-4EEF-A671-61D13354AC8A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the Completion Action is set to Show Completion Xaml. <span class='tip tip-lava'></span>", 2, @"", "CE73E125-4DC2-4433-B85F-D7CACF92D2EA" );
//            // Attrib for BlockType: Workflow Entry:Enabled Lava Commands
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E029A00E-943A-4EEF-A671-61D13354AC8A", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block.", 3, @"", "577EE62D-BA20-49D3-B365-E0EE0D668865" );
//            // Attrib for BlockType: Workflow Entry:Redirect To Page
//            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E029A00E-943A-4EEF-A671-61D13354AC8A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect To Page", "RedirectToPage", "Redirect To Page", @"The page the user will be redirected to if the Completion Action is set to Redirect to Page.", 4, @"", "5E0A89AF-A66B-4E91-B628-F36830722122" );

            // Seed all existing Campuses with a new 'TeamGroup' Group association
            Sql( RockMigrationSQL._202001142201240_AddCampusTeamToAllCampuses_Up );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete any Campus > Group assiations that were seeded as a part of the Up() method
            Sql( RockMigrationSQL._202001142201240_AddCampusTeamToAllCampuses_Down );

            // ----- Remove 'Group Member Detail' Block from 'Group Member Detail' Page;

            //// Attrib for BlockType: Workflow Entry:Redirect To Page
            //RockMigrationHelper.DeleteAttribute( "5E0A89AF-A66B-4E91-B628-F36830722122" );
            //// Attrib for BlockType: Workflow Entry:Enabled Lava Commands
            //RockMigrationHelper.DeleteAttribute( "577EE62D-BA20-49D3-B365-E0EE0D668865" );
            //// Attrib for BlockType: Workflow Entry:Completion Xaml
            //RockMigrationHelper.DeleteAttribute( "CE73E125-4DC2-4433-B85F-D7CACF92D2EA" );
            //// Attrib for BlockType: Workflow Entry:Completion Action
            //RockMigrationHelper.DeleteAttribute( "08956A4B-5D97-42C6-9262-F405E279137C" );
            //// Attrib for BlockType: Workflow Entry:Workflow Type
            //RockMigrationHelper.DeleteAttribute( "7F262C6C-1F66-4062-9FBE-8D51769BACA7" );
            //// Attrib for BlockType: Register:Mobile Phone Required
            //RockMigrationHelper.DeleteAttribute( "D5A850D6-6347-4DF3-BCFC-0A0C59DB9C10" );
            //// Attrib for BlockType: Register:Mobile Phone Show
            //RockMigrationHelper.DeleteAttribute( "FCDA5253-CFE7-4569-A940-7E5036754E38" );
            //// Attrib for BlockType: Register:Email Required
            //RockMigrationHelper.DeleteAttribute( "22FD39AA-F893-4CB2-B5C3-BBA47AFA6480" );
            //// Attrib for BlockType: Register:Email Show
            //RockMigrationHelper.DeleteAttribute( "5D0B3AE2-774C-4146-970C-B380800CCA31" );
            //// Attrib for BlockType: Register:Campus Required
            //RockMigrationHelper.DeleteAttribute( "070A2F4D-0E73-4159-8062-D76785BF62EC" );
            //// Attrib for BlockType: Register:Campus Show
            //RockMigrationHelper.DeleteAttribute( "DCC54247-AF3F-4FBE-B992-F7E5933D5F58" );
            //// Attrib for BlockType: Register:BirthDate Required
            //RockMigrationHelper.DeleteAttribute( "312898DF-2D8F-405B-8728-956A90191D96" );
            //// Attrib for BlockType: Register:Birthdate Show
            //RockMigrationHelper.DeleteAttribute( "CBD0AAD4-9420-44BF-94CC-CD10719E8F09" );
            //// Attrib for BlockType: Register:Record Status
            //RockMigrationHelper.DeleteAttribute( "7A8BF804-FD1D-494A-8EF8-6F6B3EDB6CB6" );
            //// Attrib for BlockType: Register:Connection Status
            //RockMigrationHelper.DeleteAttribute( "91B1C2AD-0B09-4156-83AC-A2049059044E" );
            //// Attrib for BlockType: Profile Details:Address Required
            //RockMigrationHelper.DeleteAttribute( "B2B37992-B2F6-4812-8F45-13FE994E4B46" );
            //// Attrib for BlockType: Profile Details:Address Show
            //RockMigrationHelper.DeleteAttribute( "E1D8FFD0-0F77-42DE-A055-FCA1B227DEFB" );
            //// Attrib for BlockType: Profile Details:Mobile Phone Required
            //RockMigrationHelper.DeleteAttribute( "7C5D273C-5D7D-43BB-8843-8D2BEF6291E2" );
            //// Attrib for BlockType: Profile Details:Mobile Phone Show
            //RockMigrationHelper.DeleteAttribute( "6618D251-AF6B-4664-90AA-FA4D06AFE36F" );
            //// Attrib for BlockType: Profile Details:Email Required
            //RockMigrationHelper.DeleteAttribute( "5D6CAF8A-25DE-40DF-83C8-F530BEAE68BE" );
            //// Attrib for BlockType: Profile Details:Email Show
            //RockMigrationHelper.DeleteAttribute( "32BF88F5-B9A6-4E09-B27B-80C38CFDADDF" );
            //// Attrib for BlockType: Profile Details:Campus Required
            //RockMigrationHelper.DeleteAttribute( "456BDBAB-8427-4D10-946C-C36449F71755" );
            //// Attrib for BlockType: Profile Details:Campus Show
            //RockMigrationHelper.DeleteAttribute( "B91ADA6F-B1CE-491F-AB50-0201F47B7211" );
            //// Attrib for BlockType: Profile Details:BirthDate Required
            //RockMigrationHelper.DeleteAttribute( "A55B5F75-68A7-48DF-89D4-E351379ABDAF" );
            //// Attrib for BlockType: Profile Details:Birthdate Show
            //RockMigrationHelper.DeleteAttribute( "61A82CA0-CDA3-4DA8-9F9C-C237918B0041" );
            //// Attrib for BlockType: Profile Details:Record Status
            //RockMigrationHelper.DeleteAttribute( "F3EDD57C-6CF9-4862-B908-55931DE748DC" );
            //// Attrib for BlockType: Profile Details:Connection Status
            //RockMigrationHelper.DeleteAttribute( "85EDD699-DD2F-4122-AA6B-6B9A5C19098E" );
            //// Attrib for BlockType: Login:Forgot Password Url
            //RockMigrationHelper.DeleteAttribute( "C21E2C7A-37AD-41F0-8BC7-8D5C226AC5EA" );
            //// Attrib for BlockType: Login:Registration Page
            //RockMigrationHelper.DeleteAttribute( "28427F89-48DA-4202-9547-CDF6B554ED78" );
            //// Attrib for BlockType: Lava Item List:List Data Template
            //RockMigrationHelper.DeleteAttribute( "F835AF4D-BD2F-41FB-9B0E-D2E8055381E1" );
            //// Attrib for BlockType: Lava Item List:List Template
            //RockMigrationHelper.DeleteAttribute( "CFC98FB2-BE52-48B8-B59B-C8B8FB3533D5" );
            //// Attrib for BlockType: Lava Item List:Detail Page
            //RockMigrationHelper.DeleteAttribute( "4CF14666-94D8-453C-B941-943BC7BD8459" );
            //// Attrib for BlockType: Lava Item List:Page Size
            //RockMigrationHelper.DeleteAttribute( "9CB5ED90-765C-431C-B3A2-D80E2C658AD8" );
            //// Attrib for BlockType: Content Channel Item View:Log Interactions
            //RockMigrationHelper.DeleteAttribute( "8B858484-042A-4BBF-A8EB-9A187C92331C" );
            //// Attrib for BlockType: Content Channel Item View:Content Channel
            //RockMigrationHelper.DeleteAttribute( "A4E2F22C-1B48-4982-AD28-DDB7B2257FA8" );
            //// Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
            //RockMigrationHelper.DeleteAttribute( "218DC0FC-77DF-4597-A16A-1E74200D24C8" );
            //// Attrib for BlockType: Content Channel Item View:Content Template
            //RockMigrationHelper.DeleteAttribute( "FEFC7AE3-590C-4B85-A284-ACDDF676D13D" );
            //// Attrib for BlockType: Content Channel Item List:Cache Duration
            //RockMigrationHelper.DeleteAttribute( "E687A972-35CA-4512-ADA3-B201F3D5EFA3" );
            //// Attrib for BlockType: Content Channel Item List:List Data Template
            //RockMigrationHelper.DeleteAttribute( "3BEF56A0-0E4F-42B1-B33B-1A28928E93F4" );
            //// Attrib for BlockType: Content Channel Item List:Order
            //RockMigrationHelper.DeleteAttribute( "58289A07-3D71-4C51-9EDB-3E70D13C1908" );
            //// Attrib for BlockType: Content Channel Item List:Check Item Security
            //RockMigrationHelper.DeleteAttribute( "8914BA86-E477-4B9F-9632-93C72BDB2338" );
            //// Attrib for BlockType: Content Channel Item List:Show Children of Parent
            //RockMigrationHelper.DeleteAttribute( "B882A7AC-3BAB-4F1B-B20D-1EFC0EEAB4D1" );
            //// Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
            //RockMigrationHelper.DeleteAttribute( "1FF19852-C9B9-4C35-BCB9-4B104E1220F6" );
            //// Attrib for BlockType: Content Channel Item List:Filter Id
            //RockMigrationHelper.DeleteAttribute( "5309A4E3-953E-4B74-822B-6179CC7B692A" );
            //// Attrib for BlockType: Content Channel Item List:Field Settings
            //RockMigrationHelper.DeleteAttribute( "0254CAE5-207C-406B-BCB0-6A5F6A0128C9" );
            //// Attrib for BlockType: Content Channel Item List:Detail Page
            //RockMigrationHelper.DeleteAttribute( "0D77E5F9-B706-4645-B129-CA93BFE6C392" );
            //// Attrib for BlockType: Content Channel Item List:Include Following
            //RockMigrationHelper.DeleteAttribute( "C1480455-387D-4F01-94E5-DA1D47F8FCDF" );
            //// Attrib for BlockType: Content Channel Item List:Page Size
            //RockMigrationHelper.DeleteAttribute( "A1F5BD92-CF3D-424C-AE5E-87C7BEC1855F" );
            //// Attrib for BlockType: Content Channel Item List:Content Channel
            //RockMigrationHelper.DeleteAttribute( "6DCBE858-2B9A-45D5-BBDB-D24E0ABA0057" );
            //// Attrib for BlockType: Content:Callback Logic
            //RockMigrationHelper.DeleteAttribute( "0ED11D3A-85FD-4505-A428-04BC62482DBC" );
            //// Attrib for BlockType: Content:Lava Render Location
            //RockMigrationHelper.DeleteAttribute( "195BCC27-B3D2-4B94-9712-446BC027DCD3" );
            //// Attrib for BlockType: Content:Cache Duration
            //RockMigrationHelper.DeleteAttribute( "DDD5875C-0BE8-452C-8840-F41387E15BD8" );
            //// Attrib for BlockType: Content:Dynamic Content
            //RockMigrationHelper.DeleteAttribute( "BE0E4C27-12F2-4D56-9FF4-7CD1F44ED419" );
            //// Attrib for BlockType: Content:Enabled Lava Commands
            //RockMigrationHelper.DeleteAttribute( "7DB18992-3B16-4A9F-9A38-8EFD5C561AD4" );
            //// Attrib for BlockType: Content:Content
            //RockMigrationHelper.DeleteAttribute( "86AA46A8-BC05-4351-B179-C3E25BA5FBDB" );

            // Remove Block: Group Member Detail, from Page: Group Member Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "96361229-3CF1-4713-84B5-E913AECDC804" );

            //RockMigrationHelper.DeleteBlockType( "E029A00E-943A-4EEF-A671-61D13354AC8A" ); // Workflow Entry
            //RockMigrationHelper.DeleteBlockType( "ACDF252C-3692-4EDD-B4D9-3975D9AADB79" ); // Register
            //RockMigrationHelper.DeleteBlockType( "326771C7-DD22-4E88-B727-74EC84133424" ); // Profile Details
            //RockMigrationHelper.DeleteBlockType( "34E8DA76-B510-4761-8007-F2B103007693" ); // Login
            //RockMigrationHelper.DeleteBlockType( "87653CB9-B97E-4CD6-B423-8952C65A8F61" ); // Lava Item List
            //RockMigrationHelper.DeleteBlockType( "88FF703E-9B33-4049-86A2-0FB4282A2128" ); // Content Channel Item View
            //RockMigrationHelper.DeleteBlockType( "3164ECA4-C79A-4743-9B17-03D6B3CB1053" ); // Content Channel Item List
            //RockMigrationHelper.DeleteBlockType( "E70D584A-5479-482E-B1D3-4D03DD79DB43" ); // Content

            // ----- Remove 'Group Member List' Block from 'Campus Detail' Page;

            //// Attrib for BlockType: Workflow Entry:Redirect To Page
            //RockMigrationHelper.DeleteAttribute( "FAEE8B9C-22DD-4A16-986E-6B92F31121EA" );
            //// Attrib for BlockType: Workflow Entry:Enabled Lava Commands
            //RockMigrationHelper.DeleteAttribute( "E00CC393-6B46-4D62-8A2B-B1D4C73E4CD1" );
            //// Attrib for BlockType: Workflow Entry:Completion Xaml
            //RockMigrationHelper.DeleteAttribute( "E6EF339B-52A6-4A18-B983-FED94F1DA297" );
            //// Attrib for BlockType: Workflow Entry:Completion Action
            //RockMigrationHelper.DeleteAttribute( "9BC9EB27-5AED-4486-B63C-D689716A78A7" );
            //// Attrib for BlockType: Workflow Entry:Workflow Type
            //RockMigrationHelper.DeleteAttribute( "FCDB0796-5FA2-4ADC-853E-452BF87C8470" );
            //// Attrib for BlockType: Register:Mobile Phone Required
            //RockMigrationHelper.DeleteAttribute( "12F0DCD9-12AF-426F-BB29-FCB0BF135686" );
            //// Attrib for BlockType: Register:Mobile Phone Show
            //RockMigrationHelper.DeleteAttribute( "59952950-5765-404C-B98D-4FEF4CA9586C" );
            //// Attrib for BlockType: Register:Email Required
            //RockMigrationHelper.DeleteAttribute( "B78DB556-A13E-49DC-84B7-CD8B2A2D2277" );
            //// Attrib for BlockType: Register:Email Show
            //RockMigrationHelper.DeleteAttribute( "25AC69F4-722C-4768-A120-FCA30CB60FC5" );
            //// Attrib for BlockType: Register:Campus Required
            //RockMigrationHelper.DeleteAttribute( "5B8627C8-DB3B-4025-AAFB-5F517E107A1A" );
            //// Attrib for BlockType: Register:Campus Show
            //RockMigrationHelper.DeleteAttribute( "77436FC9-F46E-43E6-8CD6-94B9B64ECED7" );
            //// Attrib for BlockType: Register:BirthDate Required
            //RockMigrationHelper.DeleteAttribute( "48EAF240-62EC-4D33-A814-8A571A778EFF" );
            //// Attrib for BlockType: Register:Birthdate Show
            //RockMigrationHelper.DeleteAttribute( "D0A53168-D333-4262-98BB-CC59BAB0C732" );
            //// Attrib for BlockType: Register:Record Status
            //RockMigrationHelper.DeleteAttribute( "8ECD8656-AD01-42FF-A31B-723A2DF0BF0E" );
            //// Attrib for BlockType: Register:Connection Status
            //RockMigrationHelper.DeleteAttribute( "FD21F8FC-287B-4C01-95B9-FD5BE30FF3AB" );
            //// Attrib for BlockType: Profile Details:Address Required
            //RockMigrationHelper.DeleteAttribute( "9039BEB3-B50C-4D9A-AB21-514120BAA397" );
            //// Attrib for BlockType: Profile Details:Address Show
            //RockMigrationHelper.DeleteAttribute( "724AADCA-8FEF-41B2-A4D3-4CE596AE58C0" );
            //// Attrib for BlockType: Profile Details:Mobile Phone Required
            //RockMigrationHelper.DeleteAttribute( "CF1AADA5-A8C8-40EF-98BF-A919F89EB5A8" );
            //// Attrib for BlockType: Profile Details:Mobile Phone Show
            //RockMigrationHelper.DeleteAttribute( "879BAF1A-1E75-4FA2-B76E-FF0A583C6F7F" );
            //// Attrib for BlockType: Profile Details:Email Required
            //RockMigrationHelper.DeleteAttribute( "ED4D9F8D-9D5D-4D76-AB23-BB5FF60B0A6F" );
            //// Attrib for BlockType: Profile Details:Email Show
            //RockMigrationHelper.DeleteAttribute( "89B4CBFD-E95D-46A3-BA49-B50894C67C2C" );
            //// Attrib for BlockType: Profile Details:Campus Required
            //RockMigrationHelper.DeleteAttribute( "8411487A-C68B-4781-8CF4-30AEC8A03408" );
            //// Attrib for BlockType: Profile Details:Campus Show
            //RockMigrationHelper.DeleteAttribute( "1A8D9294-AA52-4FBE-AC7C-757F050433DC" );
            //// Attrib for BlockType: Profile Details:BirthDate Required
            //RockMigrationHelper.DeleteAttribute( "68B03429-0CAA-4DA5-904D-B6365EB7EF76" );
            //// Attrib for BlockType: Profile Details:Birthdate Show
            //RockMigrationHelper.DeleteAttribute( "F148105C-6F15-42D4-B3E8-8926E988F338" );
            //// Attrib for BlockType: Profile Details:Record Status
            //RockMigrationHelper.DeleteAttribute( "838AFF3A-CC53-44CD-8E1D-686B9129F799" );
            //// Attrib for BlockType: Profile Details:Connection Status
            //RockMigrationHelper.DeleteAttribute( "236D44C5-3DAA-4792-A750-F78754713DA9" );
            //// Attrib for BlockType: Login:Forgot Password Url
            //RockMigrationHelper.DeleteAttribute( "FA1F9248-D787-433D-B5ED-22AADCDAAF5A" );
            //// Attrib for BlockType: Login:Registration Page
            //RockMigrationHelper.DeleteAttribute( "A0843CEB-DBD9-4C64-AEDB-EC103290D451" );
            //// Attrib for BlockType: Lava Item List:List Data Template
            //RockMigrationHelper.DeleteAttribute( "4B0FA3B9-7C50-499C-85B0-96A672EC9AD2" );
            //// Attrib for BlockType: Lava Item List:List Template
            //RockMigrationHelper.DeleteAttribute( "A4E68B05-F8B4-4574-932A-DD7219B29005" );
            //// Attrib for BlockType: Lava Item List:Detail Page
            //RockMigrationHelper.DeleteAttribute( "DFE85BE5-0823-43F9-BE3A-590FF4BB74FB" );
            //// Attrib for BlockType: Lava Item List:Page Size
            //RockMigrationHelper.DeleteAttribute( "748DF3D9-EA43-47FA-967A-0F47B94FC914" );
            //// Attrib for BlockType: Content Channel Item View:Log Interactions
            //RockMigrationHelper.DeleteAttribute( "A90E56B3-091A-40BB-8893-D2B8BA1F04D2" );
            //// Attrib for BlockType: Content Channel Item View:Content Channel
            //RockMigrationHelper.DeleteAttribute( "DA5CE7B1-26BA-4ABB-AC2B-CBD7FEC2F9BF" );
            //// Attrib for BlockType: Content Channel Item View:Enabled Lava Commands
            //RockMigrationHelper.DeleteAttribute( "CB22F9DD-D505-41F8-BC27-2EDB77FD5CE3" );
            //// Attrib for BlockType: Content Channel Item View:Content Template
            //RockMigrationHelper.DeleteAttribute( "B80C9D5F-9F30-42C0-900B-2C93A73FD34A" );
            //// Attrib for BlockType: Content Channel Item List:Cache Duration
            //RockMigrationHelper.DeleteAttribute( "B725C0B0-8938-459A-BB5D-EA09A239BADE" );
            //// Attrib for BlockType: Content Channel Item List:List Data Template
            //RockMigrationHelper.DeleteAttribute( "52C99DEE-2D9E-4F87-8A59-B0861F90060B" );
            //// Attrib for BlockType: Content Channel Item List:Order
            //RockMigrationHelper.DeleteAttribute( "7312E700-5BD9-4FED-9C3D-8956DF4CA66B" );
            //// Attrib for BlockType: Content Channel Item List:Check Item Security
            //RockMigrationHelper.DeleteAttribute( "A9893331-30AD-45A3-ADD2-F161A3455AE8" );
            //// Attrib for BlockType: Content Channel Item List:Show Children of Parent
            //RockMigrationHelper.DeleteAttribute( "39DA3BA5-7FB7-4415-8B71-E54A446D3C2C" );
            //// Attrib for BlockType: Content Channel Item List:Query Parameter Filtering
            //RockMigrationHelper.DeleteAttribute( "64C2D75F-F895-4A8B-B494-65CF73816902" );
            //// Attrib for BlockType: Content Channel Item List:Filter Id
            //RockMigrationHelper.DeleteAttribute( "9EA2EB6C-EDB2-47D7-8BE7-125D26C30936" );
            //// Attrib for BlockType: Content Channel Item List:Field Settings
            //RockMigrationHelper.DeleteAttribute( "50B09932-DF1B-4E00-8558-83027D66D933" );
            //// Attrib for BlockType: Content Channel Item List:Detail Page
            //RockMigrationHelper.DeleteAttribute( "50093051-42E6-40BF-B84F-AEFF48870B6C" );
            //// Attrib for BlockType: Content Channel Item List:Include Following
            //RockMigrationHelper.DeleteAttribute( "682284FF-7F51-4027-B42A-34BC0198A499" );
            //// Attrib for BlockType: Content Channel Item List:Page Size
            //RockMigrationHelper.DeleteAttribute( "4FB6B697-7629-4A2C-80AB-2AEAC226A24E" );
            //// Attrib for BlockType: Content Channel Item List:Content Channel
            //RockMigrationHelper.DeleteAttribute( "F3638C5B-90ED-4FFB-8C43-BD73E22427BB" );
            //// Attrib for BlockType: Content:Callback Logic
            //RockMigrationHelper.DeleteAttribute( "1E437D65-39DA-499C-8517-FD8C5F13B2F4" );
            //// Attrib for BlockType: Content:Lava Render Location
            //RockMigrationHelper.DeleteAttribute( "3F42817F-0C9F-4809-A845-854FC73AA903" );
            //// Attrib for BlockType: Content:Cache Duration
            //RockMigrationHelper.DeleteAttribute( "C28EE36A-FFC0-4C1D-89D5-8FF647E9A064" );
            //// Attrib for BlockType: Content:Dynamic Content
            //RockMigrationHelper.DeleteAttribute( "5887F89F-E61C-4BA7-832D-A18BF3909DFF" );
            //// Attrib for BlockType: Content:Enabled Lava Commands
            //RockMigrationHelper.DeleteAttribute( "AFBA2BBD-94CD-468D-B658-2E92D5B3D3FE" );
            //// Attrib for BlockType: Content:Content
            //RockMigrationHelper.DeleteAttribute( "194ADD33-BD9D-4640-868C-80EB75602AC8" );

            // Attrib Value for Block:Group Member List, Attribute:core.CustomGridEnableStickyHeaders Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "318B80EE-7349-4BF4-82F2-64FC38A5AB0B", "7F31B5A8-96F6-4D30-9BB6-3EB2DBE26234" );
            // Attrib Value for Block:Group Member List, Attribute:Show Note Column Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "318B80EE-7349-4BF4-82F2-64FC38A5AB0B", "5F54C068-1418-44FA-B215-FBF70072F6A5" );
            // Attrib Value for Block:Group Member List, Attribute:Show Date Added Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "318B80EE-7349-4BF4-82F2-64FC38A5AB0B", "F281090E-A05D-4F81-AD80-A3599FB8E2CD" );
            // Attrib Value for Block:Group Member List, Attribute:Show Campus Filter Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "318B80EE-7349-4BF4-82F2-64FC38A5AB0B", "65B9EA6C-D904-4105-8B51-CCA784DDAAFA" );
            // Attrib Value for Block:Group Member List, Attribute:Show First/Last Attendance Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "318B80EE-7349-4BF4-82F2-64FC38A5AB0B", "65834FB0-0AB0-4F73-BE1B-9D2F9FFD2664" );
            // Attrib Value for Block:Group Member List, Attribute:Detail Page Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlockAttributeValue( "318B80EE-7349-4BF4-82F2-64FC38A5AB0B", "E4CCB79C-479F-4BEE-8156-969B2CE05973" );

            // Remove Block: Group Member List, from Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "318B80EE-7349-4BF4-82F2-64FC38A5AB0B" );            

            //RockMigrationHelper.DeleteBlockType( "6785A5A4-AF4D-4070-A86E-DFD81E0B99A6" ); // Workflow Entry
            //RockMigrationHelper.DeleteBlockType( "41655361-B703-43E6-AB8D-7DEB74EC1132" ); // Register
            //RockMigrationHelper.DeleteBlockType( "8267EF1D-AFBD-44E8-8D97-359761D1C059" ); // Profile Details
            //RockMigrationHelper.DeleteBlockType( "C61E572D-BAA8-436C-B421-95F063C26FDD" ); // Login
            //RockMigrationHelper.DeleteBlockType( "3C2E23A0-EA2B-4980-A69E-8517DEFD619A" ); // Lava Item List
            //RockMigrationHelper.DeleteBlockType( "29119D4B-A93B-4CBC-9A20-DA926904F039" ); // Content Channel Item View
            //RockMigrationHelper.DeleteBlockType( "9467B5F2-3489-4D19-AE85-C8F250DE8DF2" ); // Content Channel Item List
            //RockMigrationHelper.DeleteBlockType( "F9191415-F37F-426F-B032-356E7551FA74" ); // Content

            // Remove 'Group Member Detail' Page from 'Campus Detail' Page
            RockMigrationHelper.DeletePageRoute( "9660B9FB-C90F-4AFE-9D58-C0EC271C1377" );
            RockMigrationHelper.DeletePage( "EB135AE0-5BAC-458B-AD5B-47460C2BFD31" ); //  Page: Group Member Detail, Layout: Full Width, Site: Rock RMS

            // Remove default Roles from 'Campus Team' GroupType
            RockMigrationHelper.DeleteGroupTypeRole( Guids.GROUP_TYPE_ROLE_CAMPUS_PASTOR );
            RockMigrationHelper.DeleteGroupTypeRole( Guids.GROUP_TYPE_ROLE_CAMPUS_ADMINISTRATOR );

            // Remove 'Campus Team' GroupType
            RockMigrationHelper.DeleteGroupType( Guids.GROUP_TYPE_CAMPUS_TEAM );
        }
    }
}
