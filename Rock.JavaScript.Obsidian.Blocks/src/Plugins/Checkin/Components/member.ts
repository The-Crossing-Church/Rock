import { defineComponent, PropType } from "vue"
import { DefinedValueBag } from "@Obsidian/ViewModels/Entities/definedValueBag"
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag"
import { DateTime } from "luxon"
import BasicInfo from "./basicInfo"
import ContactInfo from "./contactInfo"
import AdditionalAttributes from "./additionalAttributes"
import Placement from "./placement"
import RockForm from "@Obsidian/Controls/rockForm"

export default defineComponent({
    name: "Checkin.Components.Member",
    setup() {
      function createDebounce() {
        let timeout = null as any;
        return function (fnc: Function, delayMs: number) {
          clearTimeout(timeout)
          timeout = setTimeout(() => {
            fnc()
          }, delayMs || 500)
        }
      }
      return {
        debounce: createDebounce()
      }
    },
    components: {
      "rck-form": RockForm,
      "basic-info": BasicInfo,
      "contact-info": ContactInfo,
      "group-info": Placement,
      "add-attrs": AdditionalAttributes
    },
    props: {
      showTitle: Boolean,
      showNickName: Boolean,
      showMiddleName: Boolean,
      showSuffix: Boolean,
      defaultConnectionStatus: {
        type: Object as PropType<DefinedValueBag>,
        required: false
      },
      connectionStatusDefinedType: Object,
      showConnectionStatus: Boolean,
      requireConnectionStatus: Boolean,
      titleDefinedType: Object,
      suffixDefinedType: Object,
      defaultMaritalStatus: {
        type: Object as PropType<DefinedValueBag>,
        required: false
      },
      maritalStatusDefinedType: Object,
      showEmail: Boolean,
      showEmailOptOut: Boolean,
      showCell: Boolean,
      showSMS: Boolean,
      phoneType: {
        type: Object as PropType<DefinedValueBag>,
        required: false
      },
      showMaritalStatus: Boolean,
      showBirthDate: Boolean,
      requireBirthDate: Boolean,
      showGender: Boolean,
      requireGender: Boolean,
      showGradeOrAbility: Boolean,
      requireGradeOrAbility: Boolean,
      abilityAttribute: Object,
      abilityDefinedType: Object,
      gradeDefinedType: Object,
      graduationYear: Number,
      canRemove: Boolean,
      attributes: {
        type: [] as any[],
        required: false
      },
      groups: Array,
      groupStartDOBAttribute: Object,
      groupEndDOBAttribute: Object,
      groupAbilityAttribute: Object,
      groupGradeAttribute: Object,
      person: {
        type: Object as PropType<PersonBag>,
        required: false
      },
      showValidation: Boolean,
      findExisting: Function
    },
    data() {
      return {
        potentialMatch: null
      };
    },
    computed: {
      displayName() {
        if(this.person?.firstName) {
          return this.person.firstName
        } else if(this.person?.ageClassification == 1) {
          return "Parent"
        } else if(this.person?.ageClassification == 2) {
          return "Child"
        }
        return "PersonBag"
      }
    },
    methods: {
      remove() {
        this.$emit('remove-person', this.person)
      },
      checkForDuplicates() {
        if(this.person && !(this.person.idKey) && this.findExisting) {
          let p = this.person as any
          let number = ""
          if(p.phoneNumbers && p.phoneNumbers[0].numberFormatted) {
            number = p.phoneNumbers[0].numberFormatted.replaceAll("(", "").replaceAll(")", "").replaceAll("-", "").replaceAll("+", "").replaceAll(" ", "")
          }
          if(this.person.firstName && this.person.lastName && ((this.person.birthDay && this.person.birthMonth && this.person.birthYear && this.person.gender) || number)) {
            this.findExisting(this.person, number)
            .then((res: any) => {
              if(res.isSuccess) {
                if(res.data.hasMatch && res.data.person) {
                  this.potentialMatch = res.data.person
                } else {
                  this.potentialMatch = null
                }
              } else {
                this.potentialMatch = null
              }
            }).catch((err: any) => {
              console.log(err)
              this.potentialMatch = null
            })
          }
        }
      },
      formatDate(date: string) {
        let dt = DateTime.fromFormat(date, 'yyyy-MM-dd')
        if(!dt.isValid) {
          dt = DateTime.fromISO(date)
        }
        return dt.toFormat("MM/dd/yyyy")
      },
      generateUrl(id: string) {
        let url = window.location.href
        if(url.includes('?')) {
          url = url.split('?')[0]
        }
        return url + "?Id=" + id
      }
    },
    watch: {

    },
    mounted() {
      if(this.person) {
        if(!this.person?.maritalStatusValueId && this.defaultMaritalStatus && this.showMaritalStatus) {
          this.person.maritalStatusValueId = parseInt(this.defaultMaritalStatus.idKey as string)
        }
        if(!this.person.connectionStatusValueId && this.defaultConnectionStatus) {
          this.person.connectionStatusValueId = parseInt(this.defaultConnectionStatus.idKey as string)
        }
      }
    },
    template: `
<rck-form ref="form" v-model:submit="showValidation">
  <basic-info 
    :showTitle="showTitle" 
    :titleDefinedType="titleDefinedType"
    :showNickName="showNickName"
    :showMiddleName="showMiddleName"
    :showSuffix="showSuffix"
    :suffixDefinedType="suffixDefinedType"
    :defaultConnectionStatus="defaultConnectionStatus"
    :connectionStatusDefinedType="connectionStatusDefinedType"
    :showConnectionStatus="showConnectionStatus"
    :requireConnectionStatus="requireConnectionStatus"
    :maritalStatusDefinedType="maritalStatusDefinedType"
    :defaultMaritalStatus="defaultMaritalStatus"
    :showMaritalStatus="showMaritalStatus"
    :showBirthDate="showBirthDate"
    :requireBirthDate="requireBirthDate"
    :showGender="showGender"
    :requireGender="requireGender"
    :showGradeOrAbility="showGradeOrAbility"
    :requireGradeOrAbility="requireGradeOrAbility"
    :abilityAttribute="abilityAttribute"
    :abilityDefinedType="abilityDefinedType"
    :gradeDefinedType="gradeDefinedType"
    :graduationYear="graduationYear"
    :person="person"
    v-on:checkForDuplicates="debounce(checkForDuplicates, 1000)"
  ></basic-info>
  <contact-info
    v-if="showEmail || showCell"
    :showEmail="showEmail"
    :showEmailOptOut="showEmailOptOut"
    :showCell="showCell"
    :showSMS="showSMS"
    :phoneType="phoneType"
    :person="person"
    v-on:checkForDuplicates="debounce(checkForDuplicates, 1000)"
  ></contact-info>
  <add-attrs 
    v-if="attributes && attributes.length > 0"
    :person="person"
    :attributes="attributes"
  ></add-attrs>
  <group-info
    v-if="person.ageClassification == 2"
    :person="person"
    :groups="groups"
    :startDOBAttribute="groupStartDOBAttribute"
    :endDOBAttribute="groupEndDOBAttribute"
    :abilityAttribute="groupAbilityAttribute"
    :gradeAttribute="groupGradeAttribute"
    :graduationYear="graduationYear"
  ></group-info>
  <div class="alert alert-warning mt-3" v-if="potentialMatch" role="alert">
      <a target="_blank" :href="'/person/' + potentialMatch.id">
        Potential Match: {{potentialMatch.fullName}} {{formatDate(potentialMatch.birthDate)}} ({{potentialMatch.age}})
      </a>
      <a :href="generateUrl(potentialMatch.id)" class="ml-3" v-if="potentialMatch.ageClassification == 1">
        <i class="fas fa-user-plus"></i> Add Child to {{potentialMatch.lastName}} Family
      </a>
  </div>
  <div class="w-100 mt-3" v-if="canRemove" style="height: 26px;">
    <a class="pull-right btn btn-xs btn-default" @click="remove">
      <i class="fa fa-trash"></i>
      Remove {{displayName}}
    </a>
  </div>
</rck-form>
`
});
