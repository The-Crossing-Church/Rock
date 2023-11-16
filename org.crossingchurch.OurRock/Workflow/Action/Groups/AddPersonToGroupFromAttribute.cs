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

namespace org.crossingchurch.OurRock.Workflow.Action
{
    /// <summary>
    /// Sets an attribute's value to the selected person 
    /// </summary>
    [ActionCategory( "The Crossing Church" )]
    [Description( "Adds person to a group from an attribute value." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Group Member Add From Attribute" )]

    [WorkflowAttribute( "Person", "Workflow attribute that contains the person to add to the group.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowAttribute( "Group", "Workflow attribute that contains the group to add to the person to.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.GroupFieldType" } )]
    [WorkflowAttribute( "Group Role", "Workflow attribute that contains the group role of the group member to add.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.GroupRoleFieldType" } )]
    [WorkflowAttribute( "Group Member Status", "Workflow attribute that contains the group member status of the group member to add.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.IntegerFieldType" } )]
    [BooleanField( "Update Existing", "If the selected person already belongs to the selected group, should their current role and status be updated to reflect the configured values above.", true, "", 3 )]
    public class AddPersonToGroupFromAttribute : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            // Determine which group to add the person to
            Group group = null;
            Person person = null;
            GroupTypeRole groupRole = null;
            int? groupStatus = null;
            var groupMemberService = new GroupMemberService( rockContext );

            var personGuid = ( GetAttributeValue( action, "Person", true ) ?? string.Empty ).AsGuidOrNull();
            var groupGuid = ( GetAttributeValue( action, "Group", true ) ?? string.Empty ).AsGuidOrNull();
            var groupRoleGuid = ( GetAttributeValue( action, "GroupRole", true ) ?? string.Empty ).AsGuidOrNull();
            groupStatus = ( GetAttributeValue( action, "GroupMemberStatus", true ) ?? string.Empty ).AsIntegerOrNull();

            if ( groupGuid.HasValue && personGuid.HasValue && groupRoleGuid.HasValue && groupStatus.HasValue )
            {
                group = new GroupService( rockContext ).Get( groupGuid.Value );
                person = new PersonAliasService( rockContext ).Get( personGuid.Value ).Person;
                groupRole = new GroupTypeRoleService( rockContext ).Get( groupRoleGuid.Value );
                GroupMember groupMember = groupMemberService.GetByGroupIdAndPersonIdAndPreferredGroupRoleId( group.Id, person.Id, groupRole.Id );
                bool isNew = false;
                if ( groupMember == null )
                {
                    groupMember = new GroupMember();
                    groupMember.PersonId = person.Id;
                    groupMember.GroupId = group.Id;
                    groupMember.GroupRoleId = groupRole.Id;
                    groupMember.GroupMemberStatus = ( GroupMemberStatus ) groupStatus;
                    isNew = true;
                }
                else
                {
                    if ( GetAttributeValue( action, "UpdateExisting" ).AsBoolean() )
                    {
                        groupMember.GroupRoleId = groupRole.Id;
                        groupMember.GroupMemberStatus = ( GroupMemberStatus ) groupStatus;
                    }
                    action.AddLogEntry( $"{person.FullName} was already a member of the selected group.", true );
                }

                if ( groupMember.IsValidGroupMember( rockContext ) )
                {
                    if ( isNew )
                    {
                        groupMemberService.Add( groupMember );
                    }
                    rockContext.SaveChanges();
                }
                else
                {
                    // if the group member couldn't be added (for example, one of the group membership rules didn't pass), add the validation messages to the errormessages
                    errorMessages.AddRange( groupMember.ValidationResults.Select( a => a.ErrorMessage ) );
                }
            }
            else
            {
                errorMessages.Add( "Provide All Information." );
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}