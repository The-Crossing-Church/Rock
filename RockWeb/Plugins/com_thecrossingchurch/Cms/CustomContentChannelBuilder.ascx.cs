using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;


using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;
using Rock.CheckIn;
using System.Data.Entity;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Text;
using System.Web;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Microsoft.Ajax.Utilities;
using System.Collections.ObjectModel;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using CSScriptLibrary;
using System.Data.Entity.Migrations;
using Z.EntityFramework.Plus;

namespace RockWeb.Plugins.com_thecrossingchurch.Cms
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Custom Content Channel Builder" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Block to create a cci with GrapesJS" )]

    [IntegerField( "Page Id", "The Id of the Page to Redirect to on Save", true )]
    [MemoField( "Additional Themes", "A comma separated list of local paths to theme.css files (/Themes/MyTheme/Styles/theme.css)", false )]
    [CodeEditorField( "Additional Grapes Components", mode: CodeEditorMode.JavaScript )]
    public partial class CustomContentChannelBuilder : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext _context { get; set; }
        private ContentChannelItemService _cciSvc { get; set; }
        private ContentChannelService _ccSvc { get; set; }
        private int PageId { get; set; }
        private int? Id { get; set; }
        private static class PageParameterKey
        {
            public const string Id = "Id";
            public const string ContentItemId = "contentItemId";
        }
        #endregion

        #region Base Control Methods

        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent( this.Page );
            string grapesScript = "\n<script>\n" + GetAttributeValue( "AdditionalGrapesComponents" ) + "\n</script>\n";
            Page.ClientScript.RegisterStartupScript( this.GetType(), "grapesJS", grapesScript, false );
        }

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Id ) ) )
            {
                Id = Int32.Parse( PageParameter( PageParameterKey.Id ) );
                btnCreate.Text = "Save Item";
            }
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.ContentItemId ) ) )
            {
                Id = Int32.Parse( PageParameter( PageParameterKey.ContentItemId ) );
                btnCreate.Text = "Save Item";
            }
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            _context = new RockContext();
            _cciSvc = new ContentChannelItemService( _context );
            _ccSvc = new ContentChannelService( _context );
            PageId = GetAttributeValue( "PageId" ).AsInteger();
            hfStyleSheets.Value = GetAttributeValue( "AdditionalThemes" );
            if ( !Page.IsPostBack )
            {
                BindData();
                if ( !String.IsNullOrEmpty( PageParameter( "ContentChannelId" ) ) )
                {
                    pkrCC.SelectedValue = PageParameter( "ContentChannelId" );
                }
                if ( Id.HasValue )
                {
                    LoadContent();
                }
            }
            pkrCustom_SelectedIndexChanged( new object(), new EventArgs() );
        }

        #endregion

        #region Methods

        protected void BindData()
        {
            pkrCC.DataSource = _ccSvc.Queryable().OrderBy( cc => cc.Name ).ToList();
            pkrCC.DataTextField = "Name";
            pkrCC.DataValueField = "Id";
            pkrCC.DataBind();
        }

        protected void LoadContent()
        {
            ContentChannelItem item = _cciSvc.Get( Id.Value );
            item.LoadAttributes();
            txtTitle.Text = item.Title;
            dtStart.SelectedDateTime = item.StartDateTime;
            if ( item.ExpireDateTime.HasValue )
            {
                dtEnd.SelectedDateTime = item.ExpireDateTime.Value;
            }
            hfComponents.Value = item.GetAttributeValue( "GrapesJSComponents" );
            hfStyle.Value = item.GetAttributeValue( "GrapesJSStyle" );
            nbPriority.Text = item.Priority.ToString();
            pkrCC.SelectedValue = item.ContentChannelId.ToString();
        }

        #endregion

        #region Events

        protected void btnCreate_Click( object sender, EventArgs e )
        {
            ContentChannel cc = _ccSvc.Get( pkrCC.SelectedValue.AsInteger() );
            DateTime? start = dtStart.SelectedDateTime;
            DateTime? end = dtEnd.SelectedDateTime;
            var hfh = hfHtml.Value.Replace( "&gt;", ">" ).Replace( "&lt;", "<" );
            var hfc = hfCss.Value;
            ContentChannelItem item = new ContentChannelItem();
            if ( Id.HasValue )
            {
                item = _cciSvc.Get( Id.Value );
            }
            item.ContentChannelId = cc.Id;
            item.ContentChannelTypeId = cc.ContentChannelTypeId;
            item.LoadAttributes( _context );
            if ( start.HasValue )
            {
                item.StartDateTime = start.Value;
            }
            else
            {
                //Default to right now because it cannot be null
                item.StartDateTime = RockDateTime.Now;
            }
            if ( end.HasValue )
            {
                item.ExpireDateTime = end.Value;
            }
            item.Content = hfh + "\n<style>\n" + hfc + "\n</style>";
            item.CreatedByPersonAliasId = CurrentPersonAliasId;
            item.CreatedDateTime = RockDateTime.Now;
            item.Title = txtTitle.Text;
            if ( String.IsNullOrEmpty( item.Title ) )
            {
                item.Title = cc.Name + " Item (" + CurrentPerson.FullName + " " + item.CreatedDateTime.Value.ToString( "MM/dd/yy" ) + ")";
            }
            Rock.Attribute.Helper.GetEditValues( phAttributes, item );
            item.SetAttributeValue( "GrapesJSComponents", hfComponents.Value );
            item.SetAttributeValue( "GrapesJSStyle", hfStyle.Value );
            if ( !String.IsNullOrEmpty( nbPriority.Text ) )
            {
                item.Priority = nbPriority.Text.AsInteger();
            }
            else
            {
                item.Priority = 0;
            }
            //Save everything
            _context.ContentChannelItems.AddOrUpdate( item );
            _context.SaveChanges();
            item.SaveAttributeValues( _context );
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add( "ContentChannelId", item.ContentChannelId.ToString() );
            NavigateToParentPage( query );
        }

        protected void pkrCustom_SelectedIndexChanged( object sender, EventArgs e )
        {
            ContentChannelItem item;
            ContentChannel channel;
            List<string> excludeGrapesJS = new List<string>() { "GrapesJS Components", "GrapesJS Style" };
            if ( Id.HasValue )
            {
                item = _cciSvc.Get( Id.Value );
                item.ContentChannelId = pkrCC.SelectedValue.AsInteger();
            }
            else
            {
                item = new ContentChannelItem() { ContentChannelId = pkrCC.SelectedValue.AsInteger() };
            }
            channel = _ccSvc.Get( item.ContentChannelId );
            item.ContentChannelTypeId = channel.ContentChannelTypeId;
            item.LoadAttributes();
            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( item, phAttributes, true, BlockValidationGroup, excludeGrapesJS, false );
        }
        #endregion
    }
}