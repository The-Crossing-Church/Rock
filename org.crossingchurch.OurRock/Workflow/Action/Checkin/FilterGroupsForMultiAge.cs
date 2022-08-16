using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace org.crossingchurch.OurRock.Workflow.Action.CheckIn
{
    /// <summary>
    /// Removes (or excludes) the groups for each selected family member as needed for multi-age.
    /// </summary>
    [ActionCategory( "The Crossing Church" )]
    [Description( "Removes (or excludes) the groups for each selected family member that are not specific to their grade." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Filter Groups For Multi-Age" )]

    [BooleanField( "Remove", "Select 'Yes' if groups should be removed.  Select 'No' if they should just be marked as excluded.", true )]
    [TextField( "MultiAge Group Name", "The name of the group to match for multi-age", false, "Multi-Age" )]
    public class FilterGroupsForMultiAge : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState == null )
            {
                return false;
            }

            var family = checkInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                var remove = GetAttributeValue( action, "Remove" ).AsBoolean();
                var groupName = GetAttributeValue( action, "MultiAgeGroupName" );

                foreach ( var person in family.People )
                {
                    foreach ( var groupType in person.GroupTypes.ToList() )
                    {
                        foreach ( var group in groupType.Groups.ToList() )
                        {
                            foreach ( var location in group.Locations.ToList() )
                            {
                                foreach ( var schedule in location.Schedules.ToList() )
                                {
                                    bool isMultiAge = false;
                                    bool removeMultiAge = false;
                                    var possibleGroups = person.GroupTypes.SelectMany( gt => gt.Groups.Select( g => g.Group.Name ) ).ToList();

                                    if ( person.Person.AgeClassification == AgeClassification.Child && possibleGroups.Contains( groupName ) )
                                    {
                                        var allSchedules = person.SelectedSchedules.OrderBy( s => s.StartTime ).ToList();
                                        if(allSchedules.Count() > 0)
                                        {
                                            if ( schedule.Schedule.Id != allSchedules[0].Schedule.Id )
                                            {
                                                isMultiAge = true;
                                            }
                                            else
                                            {
                                                removeMultiAge = true;
                                            }
                                        }
                                    }

                                    if ( isMultiAge && groupName != group.Group.Name )
                                    {
                                        if ( remove )
                                        {
                                            location.Schedules.Remove( schedule );
                                        }
                                        else
                                        {
                                            schedule.ExcludedByFilter = true;
                                        }

                                        continue;
                                    }
                                    // Remove multi-age from the first schedule selection
                                    // Example, we have three services, multi-age is available at the second and third
                                    // If child is attending second and third services, multi-age should not be an option for second service
                                    if ( removeMultiAge && groupName == group.Group.Name )
                                    {
                                        if ( remove )
                                        {
                                            location.Schedules.Remove( schedule );
                                        }
                                        else
                                        {
                                            schedule.ExcludedByFilter = true;
                                        }

                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}