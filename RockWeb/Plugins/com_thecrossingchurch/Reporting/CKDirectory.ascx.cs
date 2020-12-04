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
using OpenHtmlToPdf;
using DocumentFormat.OpenXml.Drawing;
using System.IO.Compression;

namespace RockWeb.Plugins.com_thecrossingchurch.Reporting
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "CK Directory Report" )]
    [Category( "com_thecrossingchurch > CK Directory" )]
    [Description( "Directory Crossing Kids" )]
    [IntegerField( "Time Frame", "The number of weeks back from the current day to include as recent attendance.", true, 0, "", 0 )]

    public partial class CKDirectory : Rock.Web.UI.RockBlock //, ICustomGridColumns
    {
        #region Variables

        // Variables that get set with filter 
        private List<int> Groups { get; set; }
        private string FileName { get; set; }
        //Local Variables
        private List<ReportData> Data { get; set; }
        private int weeksPrior { get; set; }

        #endregion

        #region Base Control Methods

        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent( this.Page );
            scriptManager.RegisterPostBackControl( this.btnExportExcel );
            scriptManager.RegisterPostBackControl( this.btnExportPDF );
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
            weeksPrior = GetAttributeValue( "TimeFrame" ).AsInteger();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnExportExcel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnExportExcel_Click( object sender, EventArgs e )
        {
            Groups = GroupIds.SelectedValue.Split( ',' ).Select( i => Int32.Parse( i ) ).ToList();
            FileName = ReportName.Text;
            Data = new List<ReportData>();
            GenerateData();
            var excel = GenerateExcel();
            byte[] byteArray;
            using ( MemoryStream ms = new MemoryStream() )
            {
                excel.SaveAs( ms );
                byteArray = ms.ToArray();
            }
            this.Page.EnableViewState = false;
            Response.Clear();
            //Response.Buffer = true;
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader( "content-disposition", "attachment;filename=" + FileName + ".xlsx" );
            //Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Charset = "";
            Response.BinaryWrite( byteArray );
            Response.Flush();
            Response.End();
            this.btnExportExcel.Enabled = true;
        }

        /// <summary>
        /// Handles the Click event of the btnExportPDF control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnExportPDF_Click( object sender, EventArgs e )
        {
            Groups = GroupIds.SelectedValue.Split( ',' ).Select( i => Int32.Parse( i ) ).ToList();
            FileName = ReportName.Text;
            Data = new List<ReportData>();
            GenerateData();
            var pdfs = GeneratePDF();
            byte[] files;
            using ( var memoryStream = new MemoryStream() )
            {
                using ( var archive = new ZipArchive( memoryStream, ZipArchiveMode.Create, true ) )
                {
                    for ( var i = 0; i < pdfs.Count(); i++ )
                    {
                        var fname = Data[i].ParentGroup.Name.Replace( "/", "-" ).Replace( "8:20 ", "" ).Replace( "9:45 ", "" ).Replace( "11:15 ", "" );
                        var pdfFile = archive.CreateEntry( fname + ".pdf" );
                        using ( var streamWriter = pdfFile.Open() )
                        {
                            new MemoryStream( pdfs[i] ).CopyTo( streamWriter );
                        }
                    }
                }
                files = memoryStream.ToArray();
            }

            this.Page.EnableViewState = false;
            Response.Clear();
            //Response.Buffer = true;
            Response.ContentType = "application/zip";
            Response.AddHeader( "content-disposition", "attachment;filename=" + FileName + ".zip" );
            //Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Charset = "";
            Response.BinaryWrite( files );
            Response.Flush();
            Response.End();
            this.btnExportPDF.Enabled = true;
        }
        #endregion

        #region Methods

        public void GenerateData()
        {
            RockContext context = new RockContext();
            for ( var i = 0; i < Groups.Count(); i++ )
            {
                List<Person> members = new List<Person>();
                Group group = new GroupService( context ).Get( Groups[i] );
                List<int> childGroups = GetChildGroups( Groups[i] );
                if ( group.Members.Count() > 0 )
                {
                    members.AddRange( group.Members.Select( gm => gm.Person ) );
                }
                for ( var k = 0; k < childGroups.Count(); k++ )
                {
                    Group g = new GroupService( context ).Get( childGroups[k] );
                    if ( g.Members.Count() > 0 )
                    {
                        members.AddRange( g.Members.Select( gm => gm.Person ) );
                    }
                }
                ReportData rd = new ReportData
                {
                    ParentGroup = group,
                    Members = members.Distinct().ToList()
                };
                Data.Add( rd );
            }
        }

        public List<int> GetChildGroups( int groupid )
        {
            RockContext context = new RockContext();
            List<int> grps = new List<int>();
            List<int> children = new GroupService( context ).Queryable().Where( g => g.ParentGroupId == groupid ).Select( g => g.Id ).ToList();
            if ( children.Count() > 0 )
            {
                grps.AddRange( children );
                for ( var i = 0; i < children.Count(); i++ )
                {
                    grps.AddRange( GetChildGroups( children[i] ) );
                }
            }
            return grps;
        }

        public ExcelPackage GenerateExcel()
        {
            ExcelPackage excel = new ExcelPackage();
            excel.Workbook.Properties.Title = FileName;
            // add author info
            Rock.Model.UserLogin userLogin = Rock.Model.UserLoginService.GetCurrentUser();
            if ( userLogin != null )
            {
                excel.Workbook.Properties.Author = userLogin.Person.FullName;
            }
            else
            {
                excel.Workbook.Properties.Author = "Rock";
            }
            for ( var i = 0; i < Data.Count(); i++ )
            {
                ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add( Data[i].ParentGroup.Name );
                worksheet.PrinterSettings.LeftMargin = .5m;
                worksheet.PrinterSettings.RightMargin = .5m;
                worksheet.PrinterSettings.TopMargin = .5m;
                worksheet.PrinterSettings.BottomMargin = .5m;

                //Header Row
                var headers = new List<string> { "Child's Name", "Birthdate", "Gender", "Address", "Parents' Names", "Parents' Phones", "Parents' Emails" };
                var h = 1;
                foreach ( var header in headers )
                {
                    worksheet.Cells[1, h].Value = header;
                    h++;
                }
                //Child Data
                int row = 0;
                var sorted = Data[i].Members.OrderBy( p => p.FullName ).ToList();
                for ( var k = 0; k < sorted.Count(); k++ )
                {
                    if ( HasAttendedRecently( sorted[k] ) )
                    {
                        //Generate Parental Data
                        List<Person> parents = sorted[k].GetFamilyMembers().Where( fm => fm.GroupRoleId == 3 ).Select( gm => gm.Person ).ToList();
                        string parentNames = string.Join( ", ", parents.Select( p => p.FullName ) );
                        string parentPhones = string.Join( ", ", parents.Where( p => p.PhoneNumbers.Count() > 0 ).Select( p => p.PhoneNumbers.Any( pn => pn.NumberTypeValue.Value == "Mobile" ) == true ? p.PhoneNumbers.FirstOrDefault( pn => pn.NumberTypeValue.Value == "Mobile" ).NumberFormatted : p.PhoneNumbers.FirstOrDefault().NumberFormatted ) );
                        string parentEmails = string.Join( ", ", parents.Select( p => p.Email ) );
                        //Add Data to Cells
                        worksheet.Cells[row + 2, 1].Value = sorted[k].FullName;
                        worksheet.Cells[row + 2, 2].Value = sorted[k].BirthDate.HasValue ? sorted[k].BirthDate.Value.ToString( "MM/dd/yyyy" ) : "";
                        worksheet.Cells[row + 2, 3].Value = sorted[k].Gender;
                        worksheet.Cells[row + 2, 4].Value = sorted[k].GetHomeLocation() != null ? sorted[k].GetHomeLocation().FormattedAddress : "";
                        worksheet.Cells[row + 2, 5].Value = parentNames;
                        worksheet.Cells[row + 2, 6].Value = parentPhones;
                        worksheet.Cells[row + 2, 7].Value = parentEmails;

                        //Advance Row Count
                        row++;
                    }
                }
            }
            return excel;
        }

        private bool HasAttendedRecently( Person child )
        {
            RockContext context = new RockContext();
            Attendance att = new AttendanceService( context ).Queryable().Where( a => a.PersonAliasId == child.PrimaryAliasId && a.Occurrence != null && a.Occurrence.OccurrenceDate != null ).OrderByDescending( a => a.Occurrence.OccurrenceDate ).FirstOrDefault();
            if ( att != null )
            {
                DateTime checkDt = DateTime.Now.AddDays( -7 * weeksPrior ); //Last x number of weeks to include as recent attendance 
                if ( DateTime.Compare( checkDt, att.Occurrence.OccurrenceDate ) <= 0 )
                {
                    return true;
                }
            }
            return false;
        }

        public List<byte[]> GeneratePDF()
        {
            List<byte[]> pdfs = new List<byte[]>();
            for ( var i = 0; i < Data.Count(); i++ )
            {
                var content =
                    "<!DOCTYPE html PUBLIC>" +
                    "<html>" +
                        "<head>" +
                            "<style>" +
                                "body {" +
                                    "@page {" +
                                        "size: letter;" +
                                        "@top-left {" +
                                            //"content: element(header);" +
                                            "content: '" + Data[i].ParentGroup.Name + "';" +
                                        "}" +
                                    "}" +
                                "}" +
                                "table {" +
                                    "-fs-table-paginate: paginate;" +
                                    "font-family: Arial;" +
                                    "font-size: 14px !important;" +
                                    "width: 100%;" +
                                "}" +
                                ".bg-alt {" +
                                    "background-color: #F1F1F1;" +
                                "}" +
                                "th {" +
                                    "text-align: left;" +
                                    "font-weight: bold;" +
                                    "font-size: 18px;" +
                                "}" +
                                "table, th, td {" +
                                    "border: 0px;" +
                                "}" +
                                "td {" +
                                    "padding: 4px;" +
                                "}" +
                                "div.header {" +
                                    "display: block;" +
                                    "position: running(header);" +
                                "}" +
                            "</style>" +
                        "</head>" +
                        "<body>" +
                            //"<div class='header'>" + Data[i].ParentGroup.Name + "</div>" +
                            "<table>" +
                                "<thead>" +
                                    "<tr><th>" + Data[i].ParentGroup.Name + "</th></tr>" +
                                    "<tr>" +
                                        "<th>Child's Name</th>" +
                                        "<th>Birthdate</th>" +
                                        "<th>Gender</th>" +
                                        "<th>Address</th>" +
                                        "<th>Parents' Names</th>" +
                                        "<th>Parents' Phones</th>" +
                                        "<th>Parents' Emails</th>" +
                                    "</tr>" +
                                "</thead>" +
                                "<tbody>";

                var sorted = Data[i].Members.OrderBy( p => p.FullName ).ToList();
                var row = 0;
                for ( var k = 0; k < sorted.Count(); k++ )
                {
                    if ( HasAttendedRecently( sorted[k] ) )
                    {
                        //Generate Parental Data
                        List<Person> parents = sorted[k].GetFamilyMembers().Where( fm => fm.GroupRoleId == 3 ).Select( gm => gm.Person ).ToList();
                        string parentNames = string.Join( ", ", parents.Select( p => p.FullName ) );
                        string parentPhones = string.Join( ", ", parents.Where( p => p.PhoneNumbers.Count() > 0 ).Select( p => p.PhoneNumbers.Any( pn => pn.NumberTypeValue.Value == "Mobile" ) == true ? p.PhoneNumbers.FirstOrDefault( pn => pn.NumberTypeValue.Value == "Mobile" ).NumberFormatted : p.PhoneNumbers.FirstOrDefault().NumberFormatted ) );
                        string parentEmails = string.Join( ", ", parents.Select( p => p.Email ) );
                        content +=
                            ( row % 2 == 0 ? "<tr>" : "<tr class='bg-alt'>" ) +
                                "<td>" + sorted[k].FullName + "</td>" +
                                "<td>" + ( sorted[k].BirthDate.HasValue ? sorted[k].BirthDate.Value.ToString( "MM/dd/yyyy" ) : "" ) + "</td>" +
                                "<td>" + sorted[k].Gender + "</td>" +
                                "<td>" + ( sorted[k].GetHomeLocation() != null ? sorted[k].GetHomeLocation().FormattedAddress : " " ) + "</td>" +
                                "<td>" + parentNames + "</td>" +
                                "<td>" + parentPhones + "</td>" +
                                "<td>" + parentEmails + "</td>" +
                            "</tr>";
                        row++;
                    }

                }

                content +=
                                "</tbody>" +
                            "</table>" +
                        "</body>" +
                    "</html>";
                var size = new PaperSize( Length.Inches( 11 ), Length.Inches( 8.5 ) );
                var pdf = Pdf.From( content ).OfSize( size ).WithResolution( 1080 ).WithMargins( 0.25.Inches() ).Content();
                pdfs.Add( pdf );
            }
            return pdfs;
        }

        #endregion
    }

    public class ReportData
    {
        public Rock.Model.Group ParentGroup { get; set; }
        public List<Person> Members { get; set; }
    }
}