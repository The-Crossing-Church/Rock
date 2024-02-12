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
using System.Security.Permissions;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Workflow;

namespace org.crossingchurch.OurRock.Workflow.Action
{

    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Create Nameless Person from Phone Number" )]
    [ActionCategory( "The Crossing Church" )]
    [Description( "Using a phone number, get a person or create a nameless person." )]

    [WorkflowAttribute( "Phone Number",
        Description = "Workflow attribute that contains phone number.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKeys.PhoneNumber,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PhoneNumberFieldType", "Rock.Field.Types.TextFieldType" } )]

    [WorkflowAttribute( "Person",
        Description = "Workflow attribute to set with new nameless person.",
        IsRequired = true,
        Order = 1,
        Key = AttributeKeys.Person,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]

    public class CreateNamelessPersonFromPhoneNumber : ActionComponent
    {
        static class AttributeKeys
        {
            public const string PhoneNumber = "PhoneNumber";
            public const string Person = "Person";
        }

        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var phoneNumber = ( GetAttributeValue( action, AttributeKeys.PhoneNumber, true ) ?? string.Empty );
            phoneNumber = PhoneNumber.DefaultCountryCode() + PhoneNumber.CleanNumber( phoneNumber );

            var person = new PersonService( rockContext ).GetPersonFromMobilePhoneNumber( phoneNumber, true );

            if ( person.IsNameless() )
            {
                action.AddLogEntry( $"Added nameless person: {person.Id}" );
            }
            else
            {
                action.AddLogEntry( $"Phone number matched: {person.FullName}" );
            }

            SetWorkflowAttributeValue( action, AttributeKeys.Person, person.PrimaryAlias.Guid );

            return true;
        }

    }
}