﻿// <copyright>
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

import { defineComponent, PropType } from 'vue';
import RockField from '../../../Controls/RockField';
import Alert from '../../../Elements/Alert';
import { Guid } from '../../../Util/Guid';
import Attribute from '../../../ViewModels/CodeGenerated/AttributeViewModel';
import { ComparisonType, FilterExpressionType, RegistrationEntryBlockFormFieldRuleViewModel, RegistrationEntryBlockFormFieldViewModel } from './RegistrationEntryBlockViewModel';

export default defineComponent( {
    name: 'Event.RegistrationEntry.RegistrantAttributeField',
    components: {
        Alert,
        RockField
    },
    props: {
        field: {
            type: Object as PropType<RegistrationEntryBlockFormFieldViewModel>,
            required: true
        },
        fieldValues: {
            type: Object as PropType<Record<Guid, unknown>>,
            required: true
        }
    },
    data()
    {
        return {
            fieldControlComponent: null as unknown,
            fieldControlComponentProps: {}
        };
    },
    methods: {
        isRuleMet( rule: RegistrationEntryBlockFormFieldRuleViewModel )
        {
            const value = this.fieldValues[ rule.ComparedToRegistrationTemplateFormFieldGuid ] || '';

            if ( typeof value !== 'string' )
            {
                return false;
            }

            const strVal = value.toLowerCase().trim();
            const comparison = rule.ComparedToValue.toLowerCase().trim();

            if ( !strVal )
            {
                return false;
            }

            switch ( rule.ComparisonType )
            {
                case ComparisonType.EqualTo:
                    return strVal === comparison;
                case ComparisonType.NotEqualTo:
                    return strVal !== comparison;
                case ComparisonType.Contains:
                    return strVal.includes( comparison );
                case ComparisonType.DoesNotContain:
                    return !strVal.includes( comparison );
            }

            return false;
        }
    },
    computed: {
        isVisible(): boolean
        {
            switch ( this.field.VisibilityRuleType )
            {
                case FilterExpressionType.GroupAll:
                    return this.field.VisibilityRules.every( vr => this.isRuleMet( vr ) );
                case FilterExpressionType.GroupAllFalse:
                    return this.field.VisibilityRules.every( vr => !this.isRuleMet( vr ) );
                case FilterExpressionType.GroupAny:
                    return this.field.VisibilityRules.some( vr => this.isRuleMet( vr ) );
                case FilterExpressionType.GroupAnyFalse:
                    return this.field.VisibilityRules.some( vr => !this.isRuleMet( vr ) );
            }

            return true;
        },
        attribute(): Attribute | null
        {
            return this.field.Attribute || null;
        },
        fieldProps(): Record<string, unknown>
        {
            if ( !this.attribute )
            {
                return {};
            }

            return {
                fieldTypeGuid: this.attribute.FieldTypeGuid,
                isEditMode: true,
                label: this.attribute.Name,
                help: this.attribute.Description,
                rules: this.field.IsRequired ? 'required' : '',
                configurationValues: this.attribute.QualifierValues
            };
        }
    },
    watch: {
        field: {
            immediate: true,
            handler()
            {
                if ( !( this.field.Guid in this.fieldValues ) )
                {
                    this.fieldValues[ this.field.Guid ] = this.attribute?.DefaultValue ||  '';
                }
            }
        }
    },
    template: `
<template v-if="isVisible">
    <RockField v-if="attribute" v-bind="fieldProps" v-model="this.fieldValues[this.field.Guid]" />
    <Alert v-else alertType="danger">Could not resolve attribute field</Alert>
</template>`
} );