using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

using Rock;
using Rock.Migrations;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System.Data;
using CSScriptLibrary;

namespace RockWeb.Plugins.rocks_pillars.Workflow
{
    /// <summary>
    /// Block used to generate the SQL for creating a workflow type or all the workflow types in a particular category
    /// </summary>
    [DisplayName( "Export As SQL" )]
    [Category( "Pillars > Workflow" )]
    [Description( "Block used to generate SQL for creating a workflow type or all the workflow types in a particular category" )]
    [CodeEditorField( "Code Generation Script", "The SQL script used to generate the migration code for the current workflow type or category.", 
        CodeEditorMode.Sql, CodeEditorTheme.Rock, 400, true, @"
DECLARE @TargetId int = NULL;
DECLARE @TargetCategoryId int = NULL;

-- Check for use with single WF
{% if WorkflowTypeId and WorkflowTypeId != '' %}
	SET @TargetId = {{ WorkflowTypeId }};
{% endif %}

-- Check for use with WF category
{% if CategoryId and CategoryId != '' %}
	SET @TargetCategoryId = {{ CategoryId }};
{% endif %}

IF @TargetId IS NOT NULL OR @TargetCategoryId IS NOT NULL
BEGIN 

	DECLARE @CodeTable TABLE (
	    Id int identity(1,1) not null,
	    CodeText nvarchar(max)
    )

	insert into @CodeTable
	values 
		('            #region FieldTypes'),
		('')
    
	-- field Types
	insert into @CodeTable
	SELECT
		'            exportMigration.Helper.UpdateFieldType(""'+    
		ft.Name+ '"",""'+ 
		ISNULL(ft.Description,'')+ '"",""'+ 
		ft.Assembly+ '"",""'+ 
		ft.Class+ '"",""'+ 
		CONVERT(nvarchar(50), ft.Guid)+ '"");'
	from [FieldType] [ft]
	where (ft.IsSystem = 0)

	insert into @CodeTable
	values 
		(''),
		('            #endregion'),
		('')

	insert into @CodeTable
	values 
		('            #region EntityTypes'),
		('')

	-- entitiy types
	insert into @CodeTable
	values 
		('            exportMigration.Helper.UpdateEntityType(""Rock.Model.Workflow"", ""3540E9A7-FE30-43A9-8B0A-A372B63DFC93"", true, true);' ),
		('            exportMigration.Helper.UpdateEntityType(""Rock.Model.WorkflowActivity"", ""2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F"", true, true);' ),
		('            exportMigration.Helper.UpdateEntityType(""Rock.Model.WorkflowActionType"", ""23E3273A-B137-48A3-9AFF-C8DC832DDCA6"", true, true);' )
	-- Action entity types
	insert into @CodeTable
	SELECT DISTINCT
		'            exportMigration.Helper.UpdateEntityType(""'+
		[et].[name]+ '"",""'+   
		CONVERT(nvarchar(50), [et].[Guid])+ '"",'+     
		(CASE [et].[IsEntity] WHEN 1 THEN 'true' ELSE 'false' END) + ','+
		(CASE [et].[IsSecured] WHEN 1 THEN 'true' ELSE 'false' END) + ');'
	from [WorkflowActionType] [a]
	inner join [WorkflowActivityType] [at] on [a].[ActivityTypeId] = [at].[id]
	inner join [WorkflowType] [wt] on [at].[WorkflowTypeId] = [wt].[id]
	inner join [EntityType] [et] on [et].[Id] = [a].[EntityTypeId]
	WHERE ( @TargetId IS NOT NULL AND wt.[Id] = @TargetId )
	OR ( @TargetCategoryId IS NOT NULL AND wt.[CategoryId] = @TargetCategoryId )

	-- Action entity type attributes
	insert into @CodeTable
	SELECT DISTINCT
		'            exportMigration.Helper.UpdateWorkflowActionEntityAttribute(""'+ 
		CONVERT(nvarchar(50), [aet].[Guid])+ '"",""'+   
		CONVERT(nvarchar(50), [ft].[Guid])+ '"",""'+     
		[a].[Name]+ '"",""'+  
		[a].[Key]+ '"",""'+ 
		ISNULL(REPLACE([a].[Description],'""','\""'),'')+ '"",'+ 
		CONVERT(varchar, [a].[Order])+ ',@""'+ 
		ISNULL([a].[DefaultValue],'')+ '"",""'+
		CONVERT(nvarchar(50), [a].[Guid])+ '"");' +
		' // ' + aet.Name + ':'+ a.Name
	from [Attribute] [a] 
	inner join [EntityType] [et] on [et].[Id] = [a].[EntityTypeId] and [et].Name = 'Rock.Model.WorkflowActionType'
	inner join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
	inner join [EntityType] [aet] on CONVERT(varchar, [aet].[id]) = [a].[EntityTypeQualifierValue]
	where [a].[EntityTypeQualifierColumn] = 'EntityTypeId'
	and [aet].[id] in (
		select distinct [at].[EntityTypeId]
		from [WorkflowType] [wt]
		inner join [WorkflowActivityType] [act] on [act].[WorkflowTypeId] = [wt].[id]
		inner join [WorkflowActionType] [at] 
			on [at].[ActivityTypeId] = [act].[id]
			and (
				( @TargetId IS NULL AND @TargetCategoryId IS NULL )
				OR ( @TargetId IS NOT NULL AND wt.[Id] = @TargetId )
				OR ( @TargetCategoryId IS NOT NULL AND wt.[CategoryId] = @TargetCategoryId )
			)
	)

	insert into @CodeTable
	values 
		(''),
		('            #endregion'),
		(''),
		('            #region Categories'),
		('')

	-- categories
	insert into @CodeTable
	SELECT 
		'            exportMigration.Helper.UpdateCategory(""' +
		CONVERT( nvarchar(50), [e].[Guid]) + '"",""'+ 
		[c].[Name] +  '"",""'+
		[c].[IconCssClass] +  '"",""'+
		ISNULL(REPLACE([c].[Description],'""','\""'),'')+ '"",""'+ 
		CONVERT( nvarchar(50), [c].[Guid])+ '"",'+
		CONVERT( nvarchar, [c].[Order] )+ ');' +
		' // ' + c.Name 
	FROM [Category] [c] 
	inner join [EntityType] [e] on [e].[Id] = [c].[EntityTypeId]
	where [c].[Id] in (
		select [CategoryId] 
		from [WorkflowType]
		WHERE ( @TargetId IS NULL AND @TargetCategoryId IS NULL )
		OR ( @TargetId IS NOT NULL AND [Id] = @TargetId )
		OR ( @TargetCategoryId IS NOT NULL AND [CategoryId] = @TargetCategoryId )
	)
	order by [c].[Order]

	insert into @CodeTable
	values 
		(''),
		('            #endregion'),
		('')

	DECLARE @WorkflowTypeName varchar(100)
	DECLARE @WorkflowTypeId int

	DECLARE wfCursor INSENSITIVE CURSOR FOR
	SELECT [Id], [Name]
	FROM [WorkflowType]
	WHERE ( @TargetId IS NOT NULL AND [Id] = @TargetId )
	OR ( @TargetCategoryId IS NOT NULL AND [CategoryId] = @TargetCategoryId )
	ORDER BY [Order]

	OPEN wfCursor
	FETCH NEXT FROM wfCursor
	INTO @WorkflowTypeId, @WorkflowTypeName

	WHILE (@@FETCH_STATUS <> -1)
	BEGIN

		IF (@@FETCH_STATUS = 0)
		BEGIN

			insert into @CodeTable
			values 
				('            #region ' + @WorkflowTypeName),
				('')

			-- Workflow Type
			insert into @CodeTable
			SELECT 
				'            exportMigration.Helper.UpdateWorkflowType('+ 
				(CASE [wt].[IsSystem] WHEN 1 THEN 'true' ELSE 'false' END) + ','+
				(CASE [wt].[IsActive] WHEN 1 THEN 'true' ELSE 'false' END) + ',""'+
				[wt].[Name]+ '"",""'+  
				ISNULL(REPLACE([wt].[Description],'""','\""'),'')+ '"",""'+ 
				CONVERT(nvarchar(50), [c].[Guid])+ '"",""'+     
				[wt].[WorkTerm]+ '"",""'+
				ISNULL([wt].[IconCssClass],'')+ '"",'+ 
				CONVERT(varchar, ISNULL([wt].[ProcessingIntervalSeconds],0))+ ','+
				(CASE [wt].[IsPersisted] WHEN 1 THEN 'true' ELSE 'false' END) + ','+
				CONVERT(varchar, [wt].[LoggingLevel])+ ',""'+
				CONVERT(nvarchar(50), [wt].[Guid])+ '"",'+
				CONVERT(varchar, ISNULL([wt].[Order],0))+ ');'+
				' // ' + wt.Name
			from [WorkflowType] [wt]
			inner join [Category] [c] on [c].[Id] = [wt].[CategoryId] 
			where [wt].[id] = @WorkflowTypeId 


			-- Workflow Type Attributes
			insert into @CodeTable
			SELECT 
				'            exportMigration.Helper.UpdateWorkflowTypeAttribute(""'+ 
				CONVERT(nvarchar(50), wt.Guid)+ '"",""'+   
				CONVERT(nvarchar(50), ft.Guid)+ '"",""'+     
				a.Name+ '"",""'+  
				a.[Key]+ '"",@""'+ 
				ISNULL(a.Description,'')+ '"",'+ 
				CONVERT(varchar, a.[Order])+ ',@""'+ 
				REPLACE(ISNULL(a.DefaultValue,''), '""', '""""') + '"",""'+
				CONVERT(nvarchar(50), a.Guid)+ '"", '+
				(CASE a.IsGridColumn WHEN 1 THEN 'true' ELSE 'false' END) + ');' +
				' // ' + wt.Name + ':'+ a.Name
			from [WorkflowType] [wt]
			inner join [Attribute] [a] on cast([a].[EntityTypeQualifierValue] as int) = [wt].[Id] 
			inner join [EntityType] [et] on [et].[Id] = [a].[EntityTypeId] and [et].Name = 'Rock.Model.Workflow'
			inner join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
			where EntityTypeQualifierColumn = 'WorkflowTypeId'
			and [wt].[id] = @WorkflowTypeId 
			order by [a].[Order]

			-- Workflow Type Attribute Qualifiers
			insert into @CodeTable
			SELECT 
				'            exportMigration.Helper.AddAttributeQualifier(""'+ 
				CONVERT(nvarchar(50), a.Guid)+ '"",""'+   
				CASE WHEN [dt].[guid] IS NOT NULL THEN 'definedtypeguid' ELSE [aq].[Key] END + '"",@""'+ 
				CASE WHEN [dt].[guid] IS NOT NULL THEN CAST([dt].[guid] AS varchar(50) ) ELSE ISNULL(REPLACE([aq].[Value],'""','""""'),'') END + '"",""'+
				CONVERT(nvarchar(50), [aq].[Guid])+ '"");' +
				' // ' + [wt].[Name] + ':'+ [a].[Name]+ ':'+ [aq].[Key]
			from [WorkflowType] [wt]
			inner join [Attribute] [a] on cast([a].[EntityTypeQualifierValue] as int) = [wt].[Id] 
			inner join [FieldType] [ft] on [ft].[id] = [a].[FieldTypeId]
			inner join [AttributeQualifier] [aq] on [aq].[AttributeId] = [a].[Id]
			inner join [EntityType] [et] on [et].[Id] = [a].[EntityTypeId] and [et].Name = 'Rock.Model.Workflow'
			left outer join [DefinedType] [dt] 
				on [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'
				and [aq].[key] = 'definedtype' 
				and cast([dt].[id] as varchar(5) ) = [aq].[Value]
			where [a].[EntityTypeQualifierColumn] = 'WorkflowTypeId'
			and [wt].[id] = @WorkflowTypeId 
			order by [a].[Order], [aq].[Key]

			-- Workflow Activity Type
			insert into @CodeTable
			SELECT 
				'            exportMigration.Helper.UpdateWorkflowActivityType(""'+ 
				CONVERT(nvarchar(50), [wt].[Guid])+ '"",'+     
				(CASE [at].[IsActive] WHEN 1 THEN 'true' ELSE 'false' END) + ',""'+
				[at].[Name]+ '"",""'+  
				ISNULL(REPLACE([at].[Description],'""','\""'),'')+ '"",'+ 
				(CASE [at].IsActivatedWithWorkflow WHEN 1 THEN 'true' ELSE 'false' END) + ','+
				CONVERT(varchar, [at].[Order])+ ',""'+
				CONVERT(nvarchar(50), [at].[Guid])+ '"");' +
				' // ' + wt.Name + ':'+ at.Name
			from [WorkflowActivityType] [at]
			inner join [WorkflowType] [wt] on [wt].[id] = [at].[WorkflowTypeId]
			where [wt].[id] = @WorkflowTypeId 
			order by [at].[order]

			-- Workflow Activity Type Attributes
			insert into @CodeTable
			SELECT 
				'            exportMigration.Helper.UpdateWorkflowActivityTypeAttribute(""'+ 
				CONVERT(nvarchar(50), at.Guid)+ '"",""'+   
				CONVERT(nvarchar(50), ft.Guid)+ '"",""'+     
				a.Name+ '"",""'+  
				a.[Key]+ '"",""'+ 
				ISNULL(a.Description,'')+ '"",'+ 
				CONVERT(varchar, a.[Order])+ ',@""'+ 
				ISNULL(a.DefaultValue,'')+ '"",""'+
				CONVERT(nvarchar(50), a.Guid)+ '"");' +
				' // ' + wt.Name + ':'+ at.Name + ':'+ a.Name
			from [WorkflowType] [wt]
			inner join [WorkflowActivityType] [at] on [at].[WorkflowTypeId] = [wt].[id]
			inner join [Attribute] [a] on cast([a].[EntityTypeQualifierValue] as int) = [at].[Id] 
			inner join [EntityType] [et] on [et].[Id] = [a].[EntityTypeId] and [et].Name = 'Rock.Model.WorkflowActivity'
			inner join [FieldType] [ft] on [ft].[Id] = [a].[FieldTypeId]
			where [a].[EntityTypeQualifierColumn] = 'ActivityTypeId'
			and [wt].[id] = @WorkflowTypeId 
			order by [at].[order], [a].[order]

			-- Workflow Activity Type Attribute Qualifiers
			insert into @CodeTable
			SELECT 
				'            exportMigration.Helper.AddAttributeQualifier(""'+ 
				CONVERT(nvarchar(50), a.Guid)+ '"",""'+   
				[aq].[Key]+ '"",@""'+ 
				ISNULL(REPLACE([aq].[Value],'""','""""'),'')+ '"",""'+
				CONVERT(nvarchar(50), [aq].[Guid])+ '"");' +
				' // ' + [wt].[Name] + ':'+ [a].[Name]+ ':'+ [aq].[Key]
			from [WorkflowType] [wt]
			inner join [WorkflowActivityType] [at] on [at].[WorkflowTypeId] = [wt].[id]
			inner join [Attribute] [a] on cast([a].[EntityTypeQualifierValue] as int) = [at].[Id] 
			inner join [AttributeQualifier] [aq] on [aq].[AttributeId] = [a].[Id]
			inner join [EntityType] [et] on [et].[Id] = [a].[EntityTypeId] and [et].Name = 'Rock.Model.WorkflowActivity'
			where [a].[EntityTypeQualifierColumn] = 'ActivityTypeId'
			and [wt].[id] = @WorkflowTypeId 
			order by [at].[order], [a].[order], [aq].[key]

			-- Action Forms
			insert into @CodeTable
			SELECT 
				'            exportMigration.Helper.UpdateWorkflowActionForm(@""'+ 
				REPLACE(ISNULL([f].[Header],''), '""', '""""')+ '"",@""'+ 
				REPLACE(ISNULL([f].[Footer],''), '""', '""""')+ '"",""'+ 
				ISNULL([f].[Actions],'')+ '"",""'+ 
				(CASE WHEN [se].[Guid] IS NULL THEN '' ELSE CONVERT(nvarchar(50), [se].[Guid]) END) + '"",'+
				(CASE [f].[IncludeActionsInNotification] WHEN 1 THEN 'true' ELSE 'false' END) + ',""'+
				ISNULL(CONVERT(nvarchar(50), [f].[ActionAttributeGuid]),'')+ '"",""'+ 
				CONVERT(nvarchar(50), [f].[Guid])+ '"");' +
				' // ' + wt.Name + ':'+ at.Name + ':'+ a.Name
			from [WorkflowActionForm] [f]
			inner join [WorkflowActionType] [a] on [a].[WorkflowFormId] = [f].[id]
			inner join [WorkflowActivityType] [at] on [at].[id] = [a].[ActivityTypeId]
			inner join [WorkflowType] [wt] on [wt].[id] = [at].[WorkflowTypeId]
			left outer join [SystemEmail] [se] on [se].[id] = [f].[NotificationSystemEmailId]
			where [wt].[id] = @WorkflowTypeId 
			order by [at].[Order], [a].[Order]

			-- Action Form Attributes
			insert into @CodeTable
			SELECT 
				'            exportMigration.Helper.UpdateWorkflowActionFormAttribute(""'+ 
				CONVERT(nvarchar(50), [f].[Guid])+ '"",""' +
				CONVERT(nvarchar(50), [a].[Guid])+ '"",' +
				CONVERT(varchar, [fa].[Order])+ ',' +
				(CASE [fa].[IsVisible] WHEN 1 THEN 'true' ELSE 'false' END) + ','+
				(CASE [fa].[IsReadOnly] WHEN 1 THEN 'true' ELSE 'false' END) + ','+
				(CASE [fa].[IsRequired] WHEN 1 THEN 'true' ELSE 'false' END) + ','+
				(CASE [fa].[HideLabel] WHEN 1 THEN 'true' ELSE 'false' END) + ',@""'+
				REPLACE(ISNULL([fa].[PreHtml],''), '""', '""""') + '"",@""'+
				REPLACE(ISNULL([fa].[PostHtml],''), '""', '""""') +'"",""'+
				CONVERT(nvarchar(50), [fa].[Guid])+ '"");' +
				' // '+ wt.Name+ ':'+ act.Name+ ':'+ at.Name+ ':'+ a.Name
			from [WorkflowActionFormAttribute] [fa]
			inner join [WorkflowActionForm] [f] on [f].[id] = [fa].[WorkflowActionFormId]
			inner join [Attribute] [a] on [a].[id] = [fa].[AttributeId]
			inner join [WorkflowActionType] [at] on [at].[WorkflowFormId] = [f].[id]
			inner join [WorkflowActivityType] [act] on [act].[id] = [at].[ActivityTypeId]
			inner join [WorkflowType] [wt] on [wt].[id] = [act].[WorkflowTypeId]
			where [wt].[id] = @WorkflowTypeId 
			order by [act].[Order], [at].[Order],[a].[Order]

			-- Workflow Action Type
			insert into @CodeTable
			SELECT 
				'            exportMigration.Helper.UpdateWorkflowActionType(""'+ 
				CONVERT(nvarchar(50), [at].[Guid])+ '"",""'+     
				[a].[Name]+ '"",'+  
				CONVERT(varchar, [a].[Order])+ ',""'+
				CONVERT(nvarchar(50), [et].[Guid])+ '"",'+     
				(CASE [a].[IsActionCompletedOnSuccess] WHEN 1 THEN 'true' ELSE 'false' END) + ','+
				(CASE [a].[IsActivityCompletedOnSuccess] WHEN 1 THEN 'true' ELSE 'false' END) + ',""'+
				(CASE WHEN [f].[Guid] IS NULL THEN '' ELSE CONVERT(nvarchar(50), [f].[Guid]) END) + '"",""'+
				ISNULL(CONVERT(nvarchar(50), [a].[CriteriaAttributeGuid]),'')+ '"",'+ 
				CONVERT(varchar, [a].[CriteriaComparisonType])+ ',""'+ 
				ISNULL([a].[CriteriaValue],'')+ '"",""'+ 
				CONVERT(nvarchar(50), [a].[Guid])+ '"");' +
				' // '+ wt.Name+ ':'+ at.Name+ ':'+ a.Name
			from [WorkflowActionType] [a]
			inner join [WorkflowActivityType] [at] on [at].[id] = [a].[ActivityTypeId]
			inner join [WorkflowType] [wt] on [wt].[id] = [at].[WorkflowTypeId]
			inner join [EntityType] [et] on [et].[id] = [a].[EntityTypeId]
			left outer join [WorkflowActionForm] [f] on [f].[id] = [a].[WorkflowFormId]
			where [wt].[id] = @WorkflowTypeId 
			order by [at].[Order], [a].[order]

			-- Workflow Action Type attributes values 
			insert into @CodeTable
			SELECT 
				CASE WHEN [FT].[Guid] = 'E4EAB7B2-0B76-429B-AFE4-AD86D7428C70' THEN
				'            exportMigration.Helper.AddActionTypePersonAttributeValue(""' ELSE
				'            exportMigration.Helper.AddActionTypeAttributeValue(""' END+
				CONVERT(nvarchar(50), at.Guid)+ '"",""'+ 
				CONVERT(nvarchar(50), a.Guid)+ '"",@""'+ 
				REPLACE(ISNULL(av.Value,''), '""', '""""') + '"");'+
				' // '+ wt.Name+ ':'+ act.Name+ ':'+ at.Name+ ':'+ a.Name
			from [AttributeValue] [av]
			inner join [WorkflowActionType] [at] on [at].[Id] = [av].[EntityId]
			inner join [Attribute] [a] on [a].[id] = [av].[AttributeId] AND [a].EntityTypeQualifierValue = CONVERT(nvarchar, [at].EntityTypeId)
			inner join [FieldType] [ft] on [ft].[id] = [a].[FieldTypeId] 
			inner join [EntityType] [et] on [et].[id] = [a].[EntityTypeId] and [et].[Name] = 'Rock.Model.WorkflowActionType'
			inner join [WorkflowActivityType] [act] on [act].[Id] = [at].[ActivityTypeId]
			inner join [WorkflowType] [wt] on [wt].[Id] = [act].[WorkflowTypeId] and [wt].[id] = @WorkflowTypeId 
			order by [act].[Order], [at].[Order], [a].[Order]

			insert into @CodeTable
			values 
				(''),
				('            #endregion'),
				('')

			FETCH NEXT FROM wfCursor
			INTO @WorkflowTypeId, @WorkflowTypeName

		END

	END
	
	CLOSE wfCursor
	DEALLOCATE wfCursor

	insert into @CodeTable
	values
		('            #region DefinedValue AttributeType qualifier helper'),
		(''),
		('            exportMigration.Sql( @""
			UPDATE [aq] SET [key] = ''definedtype'', [Value] = CAST( [dt].[Id] as varchar(5) )
			FROM [AttributeQualifier] [aq]
			INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]
			INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
			INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]
			WHERE [ft].[class] = ''Rock.Field.Types.DefinedValueFieldType''
			AND [aq].[key] = ''definedtypeguid''
		"" );'),
		(''),
		('            #endregion')

	select CodeText from @CodeTable
	order by Id

END
", "", 0 )]
    public partial class ExportAsSql : Rock.Web.UI.RockBlock, Rock.Web.UI.ISecondaryBlock
    {
        private int? _workflowTypeId = null;
        private int? _categoryId = null;
        private bool _visible = true;

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( btnGenerate );

            _workflowTypeId = PageParameter( "WorkflowTypeId" ).AsIntegerOrNull();
            _categoryId = PageParameter( "CategoryId" ).AsIntegerOrNull();
            SetVisible( _visible );

            if ( !Page.IsPostBack )
            {
                if ( _categoryId.HasValue )
                {
                    lTitle.Text = "Share These Workflows";
                    lAlert.Text = "You can share all of the workflow types in this category by clicking the 'Create Script' button below. This will generate and download a SQL script file that you can then use in another Rock environment, or share with someone else to create a copy of this category and all the workflow types in this category.";
                }
                if ( _workflowTypeId.HasValue )
                {
                    lTitle.Text = "Share This Workflow";
                    lAlert.Text = "You can share this workflow type by clicking the 'Create Script' button below. This will generate and download a SQL script file that you can then use in another Rock environment, or share with someone else to create a copy of this workflow type.";
                }
            }
        }

        public void SetVisible( bool visible )
        {
            _visible = visible;
            pnlView.Visible = visible && ( ( _workflowTypeId.HasValue && _workflowTypeId.Value != 0 ) || ( _categoryId.HasValue && _categoryId.Value != 0 ) );
        }

        protected void btnGenerate_Click( object sender, EventArgs e )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            if ( _workflowTypeId.HasValue )
            {
                mergeFields.Add( "WorkflowTypeId", _workflowTypeId.Value.ToString() );
            }
            if ( _categoryId.HasValue )
            {
                mergeFields.Add( "CategoryId", _categoryId.Value.ToString() );
            }

            string codeGenSql = GetAttributeValue( "CodeGenerationScript" ).ResolveMergeFields( mergeFields );

            StringBuilder code = new StringBuilder();

            if ( codeGenSql.IsNotNullOrWhiteSpace() )
            {
                var reader = DbService.GetDataReader( codeGenSql, CommandType.Text, null );
                while ( reader.Read() )
                {
                    code.AppendLine( reader["CodeText"].ToString() );
                }

                var exportMigration = new Pillars.ExportMigration();

                string codeText = string.Format( @"
using RockWeb.Pillars;
public class Script
{{
    public void Execute( ExportMigration exportMigration )
    {{
        {0}
    }}
}}", code.ToString() );
                dynamic csCode = CSScript.Evaluator.LoadCode( codeText );
                csCode.Execute( exportMigration );

                string fileName = "Workflow";

                if ( _categoryId.HasValue )
                {
                    var cat = CategoryCache.Get( _categoryId.Value );
                    fileName = cat != null ? cat.Name.MakeValidFileName() : string.Empty;
                }

                if ( _workflowTypeId.HasValue )
                {
                    var wft = WorkflowTypeCache.Get( _workflowTypeId.Value );
                    fileName = wft != null ? wft.Name.MakeValidFileName() : string.Empty;
                }

                Page.EnableViewState = false;
                Page.Response.Clear();
                Page.Response.ContentType = "application/sql";
                Page.Response.AppendHeader( "Content-Disposition", string.Format( "attachment; filename=\"{0}_{1}.sql\"", fileName, RockDateTime.Now.ToString( "yyyyMMddHHmm" ) ) );
                Page.Response.Write( exportMigration.GeneratedSql );
                Page.Response.Flush();
                Page.Response.End();
            }
        }
    }
}