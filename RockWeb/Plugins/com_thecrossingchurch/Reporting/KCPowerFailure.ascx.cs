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
using OpenHtmlToPdf;
using System.IO.Compression;

namespace RockWeb.Plugins.com_thecrossingchurch.Reporting
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Kids Club Power Failure Report" )]
    [Category( "com_thecrossingchurch > KC Power Failure" )]
    [Description( "Custom Power Failure Report for Crossing Kids" )]

    public partial class KCPowerFailure : Rock.Web.UI.RockBlock //, ICustomGridColumns
    {
        #region Variables

        // Variables that get set with filter 
        private List<int> GroupIds;
        private List<Roster> rosters;
        private string html;
        private DateTime rptDate;

        #endregion

        #region Base Control Methods

        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent( this.Page );
            scriptManager.RegisterPostBackControl( this.btnExportReports );
            //scriptManager.RegisterPostBackControl(this.btnExport);
            //scriptManager.RegisterPostBackControl(this.btnPDF);
            //scriptManager.RegisterPostBackControl(this.btnTags);
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
            GroupIds = Request.QueryString["GroupIds"].Split( ',' ).Select( int.Parse ).ToList();
            GenerateData();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnExportReports_Click( object sender, EventArgs e )
        {
            rptDate = tagDate.SelectedDate.Value;
            byte[] files;
            using ( var memoryStream = new MemoryStream() )
            {
                using ( var archive = new ZipArchive( memoryStream, ZipArchiveMode.Create, true ) )
                {
                    var excelFile = archive.CreateEntry( "PowerFailureRosters.xlsx" );
                    using ( var streamWriter = excelFile.Open() )
                    {
                        var excel = GenerateExcel();
                        new MemoryStream( excel.GetAsByteArray() ).CopyTo( streamWriter );
                    }

                    var pdfFile = archive.CreateEntry( "PowerFailureRosters.pdf" );
                    using ( var streamWriter = pdfFile.Open() )
                    {
                        new MemoryStream( GeneratePDF() ).CopyTo( streamWriter );
                    }


                    var pdfTags = archive.CreateEntry( "PowerFailureTags.pdf" );
                    using ( var streamWriter = pdfTags.Open() )
                    {
                        new MemoryStream( GenerateTags() ).CopyTo( streamWriter );
                    }
                }
                files = memoryStream.ToArray();
            }
            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/zip";
            Response.AddHeader( "content-disposition", "attachment;filename=PowerFailureReports.zip" );
            Response.Cache.SetCacheability( HttpCacheability.Public );
            Response.Charset = "";
            Response.BinaryWrite( files );
            Response.Flush();
            Response.End();
        }

        /// <summary>
        /// Handles the Click event of the btnExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnExport_Click( object sender, EventArgs e )
        {
            var excel = GenerateExcel();
            byte[] byteArray;
            using ( MemoryStream ms = new MemoryStream() )
            {
                excel.SaveAs( ms );
                byteArray = ms.ToArray();
            }
            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader( "content-disposition", "attachment;filename=PowerFailureRoster.xlsx" );
            Response.Cache.SetCacheability( HttpCacheability.Public );
            Response.Charset = "";
            Response.BinaryWrite( byteArray );
            Response.Flush();
            Response.End();
        }

        /// <summary>
        /// Handles the Click event of the btnPDF control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnExportPDF_Click( object sender, EventArgs e )
        {
            var pdf = GeneratePDF();
            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader( "content-disposition", "attachment;filename=PowerFailureRoster.pdf" );
            Response.Cache.SetCacheability( HttpCacheability.Public );
            Response.Charset = "";
            Response.BinaryWrite( pdf );
            Response.Flush();
            Response.End();

        }

        /// <summary>
        /// Handles the Click event of the btnTags control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnTags_Click( object sender, EventArgs e )
        {
            var pdf = GenerateTags();
            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader( "content-disposition", "attachment;filename=PowerFailureTags.pdf" );
            Response.Cache.SetCacheability( HttpCacheability.Public );
            Response.Charset = "";
            Response.BinaryWrite( pdf );
            Response.Flush();
            Response.End();
        }

        #endregion

        #region Methods
        /// <summary>
        /// Binds the schedules.
        /// </summary>
        private void GenerateData()
        {
            rosters = new List<Roster>();
            var _context = new RockContext();
            var content = new HtmlGenericControl( "div" );
            for ( var i = 0; i < GroupIds.Count(); i++ )
            {
                var group = new GroupService( _context ).Get( GroupIds[i] );
                var members = new GroupMemberService( _context ).Queryable().Where( gm => gm.GroupId == group.Id && gm.GroupRole.Name == "Child" ).OrderBy( gm => gm.Person.NickName ).ThenBy( gm => gm.Person.LastName ).ToList();
                var classroster = new Roster()
                {
                    ClassName = group.Name,
                    GroupId = group.Id,
                    RosterData = new List<RosterRow>()
                };
                var classdiv = new HtmlGenericControl( "div" );
                classdiv.AddCssClass( "class-container" );
                var classHeader = new HtmlGenericControl( "div" );
                classHeader.AddCssClass( "class-name" );
                classHeader.InnerText = group.Name;
                classdiv.Controls.Add( classHeader );
                var classData = new HtmlGenericControl( "table" );

                //Header Row
                var hrow = new HtmlGenericControl( "tr" );
                hrow.AddCssClass( "header-row bg-secondary" );

                var hcheckin = new HtmlGenericControl( "th" );
                hcheckin.InnerText = "Initial In";
                var hcheckout = new HtmlGenericControl( "th" );
                hcheckout.InnerText = "Initial Out";
                var hsecurity = new HtmlGenericControl( "th" );
                hsecurity.InnerText = "Attendance Code";
                var hname = new HtmlGenericControl( "th" );
                hname.InnerText = "Name";
                hname.AddCssClass( "child-col" );
                var hbday = new HtmlGenericControl( "th" );
                hbday.InnerText = "Birthday";
                var hgender = new HtmlGenericControl( "th" );
                hgender.InnerText = "Gender";
                var hallergyMed = new HtmlGenericControl( "th" );
                hallergyMed.InnerText = "Allergy/Medical";
                var hparents = new HtmlGenericControl( "th" );
                hparents.InnerText = "Parent Names";
                hparents.AddCssClass( "parent-col" );
                var hphones = new HtmlGenericControl( "th" );
                hphones.InnerText = "Parent Phone Numbers";
                hphones.AddCssClass( "phone-col" );

                hrow.Controls.Add( hcheckin );
                hrow.Controls.Add( hcheckout );
                hrow.Controls.Add( hsecurity );
                hrow.Controls.Add( hname );
                hrow.Controls.Add( hbday );
                hrow.Controls.Add( hgender );
                hrow.Controls.Add( hallergyMed );
                hrow.Controls.Add( hparents );
                hrow.Controls.Add( hphones );

                classData.Controls.Add( hrow );

                for ( var j = 0; j < members.Count(); j++ )
                {
                    var row = new HtmlGenericControl( "tr" );
                    if ( j % 2 > 0 )
                    {
                        row.AddCssClass( "bg-secondary" );
                    }
                    var checkin = new HtmlGenericControl( "td" );
                    var checkout = new HtmlGenericControl( "td" );
                    var security = new HtmlGenericControl( "td" );
                    var person = new PersonService( _context ).Get( members[j].PersonId );
                    person.LoadAttributes();
                    var name = new HtmlGenericControl( "td" );
                    name.InnerText = person.NickName + " " + person.LastName;
                    var bday = new HtmlGenericControl( "td" );
                    bday.InnerText = person.BirthDate.Value.ToString( "MM/dd/yyyy" );
                    var gender = new HtmlGenericControl( "td" );
                    gender.InnerText = person.Gender.ToString();
                    var allergyMed = new HtmlGenericControl( "td" );
                    if ( !String.IsNullOrEmpty( person.AttributeValues["Allergy"].Value ) )
                    {
                        allergyMed.InnerText = person.AttributeValues["Allergy"].Value;
                    }
                    if ( !String.IsNullOrEmpty( person.AttributeValues["MedicalSituation"].Value ) )
                    {
                        allergyMed.InnerText += ( String.IsNullOrEmpty( allergyMed.InnerText ) ? "" : " | " ) + person.AttributeValues["MedicalSituation"].Value;
                    }
                    if ( !String.IsNullOrEmpty( person.AttributeValues["MedicalInformation-Arena"].Value ) )
                    {
                        allergyMed.InnerText += ( String.IsNullOrEmpty( allergyMed.InnerText ) ? "" : " | " ) + person.AttributeValues["MedicalInformation-Arena"].Value;
                    }
                    var parents = new HtmlGenericControl( "td" );
                    var parentNames = person.GetFamilyMembers().Where( fm => fm.GroupRoleId == 3 ).ToList();
                    if ( parentNames.Count() != 0 )
                    {
                        parents.InnerText = parentNames[0].Person.NickName;
                    }
                    if ( parentNames.Count() > 1 )
                    {
                        parents.InnerText += " and " + parentNames[1].Person.NickName + " " + person.LastName;
                    }
                    else
                    {
                        parents.InnerText += " " + person.LastName;
                    }

                    var phones = new HtmlGenericControl( "td" );
                    var parentPhones = person.GetFamilyMembers().Where( fm => fm.GroupRoleId == 3 ).ToList();
                    if ( parentPhones.Count() > 0 )
                    {
                        phones.InnerText = GetPhoneNumber( parentPhones[0].Person );
                    }
                    if ( parentPhones.Count() > 1 )
                    {
                        phones.InnerText += ", " + GetPhoneNumber( parentPhones[1].Person );
                    }

                    //Add to Dataset
                    var svc = new AttendanceCodeService( _context );
                    var rosterRow = new RosterRow()
                    {
                        PersonId = person.Id,
                        Name = name.InnerText,
                        NickName = person.NickName,
                        LastName = person.LastName,
                        Birthday = bday.InnerText,
                        Gender = gender.InnerText,
                        AllergyMedical = allergyMed.InnerText,
                        Allergy = person.AttributeValues["Allergy"].Value,
                        Medical = person.AttributeValues["MedicalSituation"].Value,
                        MedicalInfo = person.AttributeValues["MedicalInformation-Arena"].Value,
                        ParentNames = parents.InnerText,
                        ParentPhones = phones.InnerText,
                        Securitycode = AttendanceCodeService.GetNew( 6, 0, 0, true ).Code
                    };
                    security.InnerText = rosterRow.Securitycode;
                    classroster.RosterData.Add( rosterRow );

                    row.Controls.Add( checkin );
                    row.Controls.Add( checkout );
                    row.Controls.Add( security );
                    row.Controls.Add( name );
                    row.Controls.Add( bday );
                    row.Controls.Add( gender );
                    row.Controls.Add( allergyMed );
                    row.Controls.Add( parents );
                    row.Controls.Add( phones );

                    classData.Controls.Add( row );
                }
                rosters.Add( classroster );
                classdiv.Controls.Add( classData );
                content.Controls.Add( classdiv );
            }
            phContent.Controls.Add( content );
            phContent.Visible = true;

            //Save html as variable
            html = "";
            foreach ( HtmlGenericControl c in phContent.Controls )
            {
                System.IO.TextWriter tw = new System.IO.StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter( tw );
                c.RenderControl( htw );
                html += tw.ToString();
            }
        }

        private string GetPhoneNumber( Person person )
        {
            if ( person.PhoneNumbers.Count() == 0 )
            {
                return "";
            }
            var mobile = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValue.Value == "Mobile" );
            if ( mobile != null )
            {
                return mobile.NumberFormatted;
            }
            mobile = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValue.Value == "Home" );
            if ( mobile != null )
            {
                return mobile.NumberFormatted;
            }
            return person.PhoneNumbers.First().NumberFormatted;

        }

        public ExcelPackage GenerateExcel()
        {
            ExcelPackage excel = new ExcelPackage();
            excel.Workbook.Properties.Title = "Kids Club Power Failure";
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
            for ( var i = 0; i < rosters.Count(); i++ )
            {
                ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add( rosters[i].ClassName );
                worksheet.PrinterSettings.LeftMargin = .5m;
                worksheet.PrinterSettings.RightMargin = .5m;
                worksheet.PrinterSettings.TopMargin = .5m;
                worksheet.PrinterSettings.BottomMargin = .5m;

                //Header Row
                var headers = new List<string> { "Initial In", "Initial Out", "Attendance Code", "Name", "Birthday", "Gender", "Allergy/Medical", "Parent Names", "Parent Phones" };
                var h = 1;
                foreach ( var header in headers )
                {
                    worksheet.Cells[1, h].Value = header;
                    h++;
                }
                for ( var j = 0; j < rosters[i].RosterData.Count(); j++ )
                {
                    worksheet.Cells[j + 2, 3].Value = rosters[i].RosterData[j].Securitycode;
                    worksheet.Cells[j + 2, 4].Value = rosters[i].RosterData[j].Name;
                    worksheet.Cells[j + 2, 5].Value = rosters[i].RosterData[j].Birthday;
                    worksheet.Cells[j + 2, 6].Value = rosters[i].RosterData[j].Gender;
                    worksheet.Cells[j + 2, 7].Value = rosters[i].RosterData[j].AllergyMedical;
                    worksheet.Cells[j + 2, 8].Value = rosters[i].RosterData[j].ParentNames;
                    worksheet.Cells[j + 2, 9].Value = rosters[i].RosterData[j].ParentPhones;
                }
            }

            return excel;
        }

        public byte[] GeneratePDF()
        {
            var content = "<!DOCTYPE html>" +
                            "<html>" +
                            "<head>" +
                                "<meta charset='UTF-8'>" +
                                "<title>Power Failure Rosters</title>" +
                                "<style>" +
                                    ".class-container { width:100%; font-family: sans-serif; font-size:14px; } " +
                                    ".class-container table td, .class-container table th { min-width: 75px; text-align: left; padding: 2px; } " +
                                    //".class-container table td, .class-container table th { min-width: 75px; text-align: left; border-right: 1px solid black; border-bottom: 1px solid black; padding: 4px; } " +
                                    ".class-container table tr { border-left: 1px solid black; } " +
                                    //".header-row { border-top: 1px solid black; font-weight: bold; } " +
                                    "table, tr { width:100%; } " +
                                    //"table { border: 1px solid black; } " +
                                    ".class-name { font-weight: bold; font-size: 32px; page-break-before: always; } " +
                                    ".bg-secondary { background-color: #F1F1F1; } " +
                                    ".phone-col { min-width: 215px !important; } " +
                                    ".parent-col { min-width: 200px !important; } " +
                                    ".child-col { min-width: 150px !important; } " +
                                "</style>" +
                            "</head>" +
                            "<body>" +
                            html +
                            "</body>" +
                            "</html>";
            var size = new PaperSize( Length.Inches( 11 ), Length.Inches( 8.5 ) );
            var pdf = Pdf.From( content ).OfSize( size ).WithResolution( 1080 ).WithMargins( 0.50.Centimeters() ).Content();
            return pdf;
        }

        public byte[] GenerateTags()
        {
            var tags = "";
            for ( var i = 0; i < rosters.Count(); i++ )
            {
                var counter = 1;
                var page = "";
                for ( var j = 0; j < rosters[i].RosterData.Count(); j++ )
                {
                    if ( counter == 1 )
                    {
                        page = "<div class='page'><table>";
                    }
                    if ( counter == 4 )
                    {
                        page += "<tr style='padding-top:16px;'>";
                    }
                    else if ( counter == 5 )
                    {
                        page += "<tr style='padding-top:24px;'>";
                    }
                    else
                    {
                        page += "<tr>";
                    }
                    page += "<td class='tag first-tag'>";
                    //Child Tag
                    page += "<div style='padding: 8px;'>" +
                                "<div class='inline' style='font-size: 26pt;'>" +
                                    rosters[i].RosterData[j].NickName +
                                "</div>" +
                                "<div class='inline right'>" +
                                    "<div>" +
                                        "Kids Club" +
                                    "</div>" +
                                    "<div>" +
                                        rptDate.ToString( "MM/dd/y" ) +
                                    "</div>" +
                                "</div>" +
                            "</div>" +
                            "<div style='padding: 0px 8px;'>" +
                                "<div class='inline' style='font-size: 16pt;'>" +
                                    rosters[i].RosterData[j].LastName +
                                "</div>" +
                                "<div class='inline right' style='font-size: 20pt;'>" +
                                    rosters[i].RosterData[j].Securitycode +
                                "</div>" +
                            "</div>" +
                            "<div style='padding: 8px;' style='font-size: 18pt;'>" +
                                rosters[i].ClassName +
                            "</div><br/><br/><br/>";
                    if ( !String.IsNullOrWhiteSpace( rosters[i].RosterData[j].AllergyMedical ) )
                    {
                        page += "<div class='med' style='padding: 8px;'>" +
                                    ( !String.IsNullOrWhiteSpace( rosters[i].RosterData[j].Medical ) ? ( rosters[i].RosterData[j].Medical + " | " ) : "" ) +
                                    rosters[i].RosterData[j].Allergy +
                                "</div>";
                    }
                    page += "</td><td class='vertical-spacer'></td><td class='tag'><div>";
                    //Parent Receipt
                    page += "<div style='padding: 8px 16px; float: left; text-align: center;' class='inline'>" +
                                "<div style='font-size: 20pt;'>Receipt</div>" +
                                "<div class='sec' style='font-size: 20pt;'>" +
                                    rosters[i].RosterData[j].Securitycode +
                                "</div>" +
                                "<div style='font-size: 22pt;'>" +
                                    rosters[i].RosterData[j].NickName +
                                "</div>" +
                                "<div style='font-size: 18pt;'>" +
                                    rptDate.ToString( "MM/dd/y" ) +
                                "</div>" +
                                "<div style='font-size: 18pt;'>" +
                                    "Kids Club" +
                                "</div>" +
                            "</div>" +
                            "<div style='padding: 8px 16px; text-align: center;' class='inline right'>" +
                                "<div style='font-size: 20pt;'>Receipt</div>" +
                                "<div class='sec' style='font-size: 20pt;'>" +
                                    rosters[i].RosterData[j].Securitycode +
                                "</div>" +
                                "<div style='font-size: 22pt;'>" +
                                    rosters[i].RosterData[j].NickName +
                                "</div>" +
                                "<div style='font-size: 18pt;'>" +
                                    rptDate.ToString( "MM/dd/y" ) +
                                "</div>" +
                                "<div style='font-size: 18pt;'>" +
                                    "Kids Club" +
                                "</div>" +
                            "</div>";
                    page += "</div></td></tr>";
                    //page += "<tr class='horizontal-spacer'><td></td><td class='vertical-spacer'></td><td></td></tr>";
                    if ( counter == 5 || j == ( rosters[i].RosterData.Count() - 1 ) )
                    {
                        page += "</table></div>";
                        tags += page;
                        counter = 0;
                    }
                    counter++;
                }
            }
            var content = "<!DOCTYPE html>" +
                            "<html>" +
                            "<head>" +
                                "<meta charset='UTF-8'>" +
                                "<title>Power Failure Tags</title>" +
                                "<style>" +
                                  //"body { widht: 8.5in; height: 11in; }" +
                                  ".page { page-break-before: always; font-family: sans-serif; }" +
                                  ".row { margin-bottom: 0.125in; }" +
                                  ".tag { height: 2in; width: 4in; position: relative; }" +
                                  ".vertical-spacer { width: 0.14in; }" +
                                  ".med { position: absolute; bottom: 0px; background-color: black; color: white; padding: 8px; font-weight: bold; }" +
                                  ".sec { background-color: black; color: white; padding: 8px; font-weight: bold; }" +
                                  ".inline { display: inline-block; }" +
                                  ".right { float: right; }" +
                                  "td:not(.first-tag) { text-align: center; } " +
                                //".row { margin-bottom: 0.25in; }" +
                                //".tag { display: inline-block; height: 4.66in; width: 6.75in; padding: 8px; border: 1px solid grey; }" +
                                //".first-tag { margin-right: .375in; }" +
                                "</style>" +
                            "</head>" +
                            "<body>" +
                            tags +
                            "</body>" +
                            "</html>";
            var size = new PaperSize( Length.Inches( 8.5 ), Length.Inches( 11 ) );
            var pdf = Pdf.From( content )
                        .OfSize( size )
                        .WithObjectSetting( "web.enableIntelligentShrinking", "false" )
                        .WithGlobalSetting( "margin.top", ".5in" )
                        .WithGlobalSetting( "margin.bottom", ".5in" )
                        .WithGlobalSetting( "margin.left", ".18in" )
                        .WithGlobalSetting( "margin.right", ".18in" )
                        .Portrait()
                        .Content();
            return pdf;
        }
        #endregion
    }

    public class RosterRow
    {
        public int PersonId { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string LastName { get; set; }
        public string Birthday { get; set; }
        public string Gender { get; set; }
        public string AllergyMedical { get; set; }
        public string Allergy { get; set; }
        public string Medical { get; set; }
        public string MedicalInfo { get; set; }
        public string ParentNames { get; set; }
        public string ParentPhones { get; set; }
        public string Securitycode { get; set; }
    }

    public class Roster
    {
        public string ClassName { get; set; }
        public int GroupId { get; set; }
        public List<RosterRow> RosterData { get; set; }
    }
}