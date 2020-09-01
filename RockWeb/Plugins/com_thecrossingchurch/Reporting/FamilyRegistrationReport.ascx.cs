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
using Nest;
using System.Linq.Expressions;
using System.Reflection;

namespace RockWeb.Plugins.com_thecrossingchurch.Reporting
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Family Registration Report" )]
    [Category( "com_thecrossingchurch > Reporting" )]
    [Description( "Comparing Event Registrations against Data Views to get list of families not signed up" )]

    public partial class FamilyRegistrationReport : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext context { get; set; }
        private RegistrationInstance registrationInstance { get; set; }
        private Rock.Model.DataView dataView { get; set; }
        private bool byFamily { get; set; }
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
            grdReport.GridRebind += grdReport_GridRebind; 
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            context = new RockContext();
            if ( pkrEvent.RegistrationInstanceId.HasValue )
            {
                registrationInstance = new RegistrationInstanceService( context ).Get( pkrEvent.RegistrationInstanceId.Value );
            }
            if ( pkrDataView.SelectedValue != null && pkrDataView.SelectedValue != "0" )
            {
                dataView = new DataViewService( context ).Get( Int32.Parse(pkrDataView.SelectedValue) );
            }
            if( ckbxFamily.Checked )
            {
                byFamily = true;
            }
            else
            {
                byFamily = false; 
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Function to bind data on grid 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Function to clear filters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnClear_Click( object sender, EventArgs e )
        {
            pkrEvent.RegistrationInstanceId = null;
            pkrEvent.RegistrationTemplateId = null;
            pkrDataView.SetValue( null );
            ckbxFamily.Checked = false; 
        }

        /// <summary>
        /// Adds attendance entry to metrics.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void grdReport_GridRebind( object sender, EventArgs e )
        {
            BindGrid(); 
        }

        #endregion

        #region Methods
        /// <summary>
        /// Generate data of all non-registered families based on source and event and send back adults in non registered families
        /// </summary>
        /// <returns></returns>
        protected List<Person> GenerateFamilyData()
        {
            List<Group> registrantFamilies = registrationInstance.Registrations.Select( r => r.PersonAlias.Person.PrimaryFamily ).ToList();
            List<string> errs = new List<string>();
            var query = dataView.GetQuery( null, null, out errs );
            List<Group> sourceFamilies = query.Select( e => (Person)e ).Select( p => p.PrimaryFamily ).ToList();
            List<Group> notRegisteredFamilies = sourceFamilies.Where( g => registrantFamilies.All( e => e.Id != g.Id ) ).ToList();
            return notRegisteredFamilies.SelectMany( g => g.Members.Where( gm => gm.Person.AgeClassification == AgeClassification.Adult ).Select(gm => gm.Person) ).Distinct().ToList(); 
        }

        /// <summary>
        /// Generate data of all non-registered individuals based on source and event and return the non-registered individuals 
        /// </summary>
        /// <returns></returns>
        protected List<Person> GenerateData()
        {
            List<Person> registrants = registrationInstance.Registrations.SelectMany( r => r.Registrants.Select(rr => rr.Person) ).ToList();
            List<string> errs = new List<string>();
            var query = dataView.GetQuery( null, null, out errs );
            List<Person> source = query.Select( e => (Person)e ).ToList();
            return source.Where( g => registrants.All( e => e.Id != g.Id ) ).Distinct().ToList();
        }

        protected void BindGrid()
        {
            if( registrationInstance != null && dataView != null )
            {
                List<Person> data = new List<Person>();
                if ( byFamily )
                {
                    data = GenerateFamilyData();
                }
                else
                {
                    data = GenerateData();
                }
                SortProperty sortProp = grdReport.SortProperty;
                if ( sortProp != null )
                {
                    data = data.AsQueryable().Sort( sortProp ).ToList();
                }
                else
                {
                    data = data.AsQueryable().OrderBy( p => p.LastName ).ThenBy( p => p.FirstName ).ToList();
                }
                //grdReport.SetLinqDataSource( data.AsQueryable().AsNoTracking() );
                var selectedKeys = grdReport.SelectedKeys;
                var selectField = grdReport.Columns.OfType<SelectField>().FirstOrDefault();
                grdReport.RowItemText = "Person";
                grdReport.DataKeyNames = new string[1] { "Id" };
                grdReport.CommunicationRecipientPersonIdFields.Add( "Id" );
                grdReport.DataSource = data;
                grdReport.DataBind();
            }
        }

        #endregion

    }

}