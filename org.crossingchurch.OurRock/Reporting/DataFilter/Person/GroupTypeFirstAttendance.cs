
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace org.crossingchurch.OurRock.Reporting.DataFilter.Person
{
    /// <summary>
    ///
    /// </summary>
    [Description( "Filter people on their first attendance to a group type" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Group Type First Attendance Filter" )]
    public class GroupTypeAttendanceFilter : DataFilterComponent
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return "Rock.Model.Person"; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Attendance"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "First Attendance in Group Types";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return "'First Attendance for ' + " +
                "'\\'' + $('.js-group-type', $content).find(':selected').text() + '\\' ' + " +
                " ($('.js-child-group-types', $content).is(':checked') ? '( or child group types) ' : '') + " +
                " $('.js-filter-compare', $content).find(':selected').text() + ' ' + " +
                " ($('.js-current-date-checkbox', $content).is(':checked') ? 'Current Date' : $('.js-current-date-checkbox > input', $content).val() )";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string s = "Group Type First Attendance";

            string[] options = selection.Split( '|' );
            if ( options.Length >= 3 )
            {
                var groupType = GroupTypeCache.Get( options[0].AsGuid() );
                ComparisonType comparisonType = options[1].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );

                var dateFieldType = new Rock.Field.Types.DateFieldType();
                string dateRangeText = dateFieldType.FormatValue( null, options[2], null, false );

                bool includeChildGroups = options.Length > 3 ? options[3].AsBoolean() : false;

                s = string.Format(
                    "First Attendance for '{0}'{1} {2} {3}",
                    groupType != null ? groupType.Name : "?",
                    includeChildGroups ? " (or child group types) " : string.Empty,
                    comparisonType.ConvertToString(),
                    dateRangeText );
            }

            return s;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var gtpGroupType = new GroupTypePicker();
            gtpGroupType.ID = filterControl.ID + "_0";
            gtpGroupType.AddCssClass( "js-group-type" );
            filterControl.Controls.Add( gtpGroupType );

            gtpGroupType.UseGuidAsValue = true;
            gtpGroupType.GroupTypes = new GroupTypeService( new RockContext() ).Queryable().OrderBy( a => a.Name ).ToList();

            var cbChildGroupTypes = new RockCheckBox();
            cbChildGroupTypes.ID = filterControl.ID + "_cbChildGroupTypes";
            cbChildGroupTypes.AddCssClass( "js-child-group-types" );
            cbChildGroupTypes.Text = "Include Child Group Types(s)";
            filterControl.Controls.Add( cbChildGroupTypes );

            var ddlDateCompare = ComparisonHelper.ComparisonControl( ComparisonHelper.DateFilterComparisonTypes );
            ddlDateCompare.Label = "First Attendance";
            ddlDateCompare.ID = filterControl.ID + "_ddlDateCompare";
            ddlDateCompare.AddCssClass( "js-filter-compare" );
            filterControl.Controls.Add( ddlDateCompare );

            var datePicker = new DatePicker();
            datePicker.DisplayCurrentOption = true;
            datePicker.Label = "Date";
            datePicker.ID = filterControl.ID + "_datePicker";
            datePicker.AddCssClass( "js-date-range" );
            filterControl.Controls.Add( datePicker );

            var controls = new Control[4] { gtpGroupType, cbChildGroupTypes, ddlDateCompare, datePicker };

            var defaultDelimitedValues = "CURRENT:0";

            // set the default values in case this is a newly added filter
            SetSelection(
                entityType,
                controls,
                string.Format( "{0}|{1}|{2}|false", gtpGroupType.Items.Count > 0 ? gtpGroupType.Items[0].Value : "0", ComparisonType.LessThan.ConvertToInt().ToString(), defaultDelimitedValues ) );

            return controls;
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            var gtpGroupType = controls[0] as GroupTypePicker;
            var cbChildGroupTypes = controls[1] as RockCheckBox;
            var ddlIntegerCompare = controls[2] as DropDownList;
            var dpDateRange = controls[3] as DatePicker;

            // Row 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            gtpGroupType.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            cbChildGroupTypes.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Row 3
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            ddlIntegerCompare.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            dpDateRange.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var gtpGroupType = controls[0] as GroupTypePicker;
            var cbChildGroupTypes = controls[1] as RockCheckBox;
            var ddlIntegerCompare = controls[2] as DropDownList;
            var datePicker = controls[3] as DatePicker;

            string dateValue = string.Empty;
            if ( datePicker.DisplayCurrentOption && datePicker.IsCurrentDateOffset )
            {
                dateValue = string.Format( "CURRENT:{0}", datePicker.CurrentDateOffsetDays );
            }
            else if ( datePicker.SelectedDate.HasValue )
            {
                dateValue = datePicker.SelectedDate.Value.ToString( "o" );
            }

            // convert the date range from pipe-delimited to comma since we use pipe delimited for the selection values
            return string.Format( "{0}|{1}|{2}|{3}", gtpGroupType.SelectedValue, ddlIntegerCompare.SelectedValue, dateValue, cbChildGroupTypes.Checked.ToTrueFalse() );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var gtpGroupType = controls[0] as GroupTypePicker;
            var cbChildGroupTypes = controls[1] as RockCheckBox;
            var ddlIntegerCompare = controls[2] as DropDownList;
            var datePicker = controls[3] as DatePicker;

            string[] options = selection.Split( '|' );
            if ( options.Length >= 4 )
            {
                gtpGroupType.SelectedValue = options[0];
                ddlIntegerCompare.SelectedValue = options[1];

                string dateValue = options[2];
                if ( datePicker.DisplayCurrentOption && dateValue != null && dateValue.StartsWith( "CURRENT", StringComparison.OrdinalIgnoreCase ) )
                {
                    datePicker.IsCurrentDateOffset = true;
                    var valueParts = dateValue.Split( ':' );
                    if ( valueParts.Length > 1 )
                    {
                        datePicker.CurrentDateOffsetDays = valueParts[1].AsInteger();
                    }
                    else
                    {
                        datePicker.CurrentDateOffsetDays = 0;
                    }
                }
                else
                {
                    datePicker.SelectedDate = dateValue.AsDateTime();
                }

                if ( options.Length >= 4 )
                {
                    cbChildGroupTypes.Checked = options[3].AsBooleanOrNull() ?? false;
                }
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            string[] options = selection.Split( '|' );
            if ( options.Length < 3 )
            {
                return null;
            }

            Guid groupTypeGuid = options[0].AsGuid();
            ComparisonType comparisonType = options[1].ConvertToEnum<ComparisonType>( ComparisonType.GreaterThanOrEqualTo );

            string dateValue = options[2];
            var dateTime = RockDateTime.Today;

            if ( dateValue.StartsWith( "CURRENT", StringComparison.OrdinalIgnoreCase ) )
            {
                var valueParts = dateValue.Split( ':' );
                if ( valueParts.Length > 1 )
                {
                    dateTime = dateTime.AddDays( valueParts[1].AsInteger() );
                }
            }
            else
            {
                dateTime = dateValue.AsDateTime() ?? RockDateTime.Today;
            }

            bool includeChildGroupTypes = options.Length >= 4 && ( options[3].AsBooleanOrNull() ?? false );

            var groupTypeService = new GroupTypeService( new RockContext() );

            var groupType = groupTypeService.Get( groupTypeGuid );
            List<int> groupTypeIds = new List<int>();
            if ( groupType != null )
            {
                groupTypeIds.Add( groupType.Id );

                if ( includeChildGroupTypes )
                {
                    var childGroupTypes = groupTypeService.GetAllAssociatedDescendents( groupType.Id );
                    if ( childGroupTypes.Any() )
                    {
                        groupTypeIds.AddRange( childGroupTypes.Select( a => a.Id ) );

                        // get rid of any duplicates
                        groupTypeIds = groupTypeIds.Distinct().ToList();
                    }
                }
            }

            var rockContext = serviceInstance.Context as RockContext;
            var attendanceQry = new AttendanceService( rockContext ).Queryable().Where( a => a.DidAttend.HasValue && a.DidAttend.Value );

            if ( groupTypeIds.Count == 1 )
            {
                int groupTypeId = groupTypeIds[0];
                attendanceQry = attendanceQry.Where( a => a.Occurrence.Group.GroupTypeId == groupTypeId );
            }
            else if ( groupTypeIds.Count > 1 )
            {
                attendanceQry = attendanceQry.Where( a => groupTypeIds.Contains( a.Occurrence.Group.GroupTypeId ) );
            }
            else
            {
                // no group type selected, so return nothing
                return Expression.Constant( false );
            }

            var firstAttendanceQry = attendanceQry
                .GroupBy( xx => xx.PersonAlias.PersonId )
                .Select( ss => new
                {
                    PersonId = ss.Key,
                    FirstAttendanceDate = ss.Min( a => ( DateTime? ) a.Occurrence.OccurrenceDate )
                } );

            var innerQry = firstAttendanceQry.Select( xx => xx.PersonId ).AsQueryable();
            var personQry = new PersonService( rockContext ).Queryable()
                .Where( p => firstAttendanceQry.Where( xx => xx.PersonId == p.Id ).Any() );

            var qry = personQry
                .Where( p =>
                    firstAttendanceQry
                        .Where( xx => xx.PersonId == p.Id && xx.FirstAttendanceDate.HasValue )
                        .Select( xx => xx.FirstAttendanceDate )
                        .FirstOrDefault() == dateTime
            );

            BinaryExpression compareEqualExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" ) as BinaryExpression;
            BinaryExpression result = FilterExpressionExtractor.AlterComparisonType( comparisonType, compareEqualExpression, null );

            return result;
        }

        #endregion
    }
}