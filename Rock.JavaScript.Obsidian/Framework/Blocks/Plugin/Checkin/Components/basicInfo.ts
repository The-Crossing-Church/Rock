import { defineComponent, PropType } from "vue"
import { DefinedValue, DefinedType, Person } from "../../../../ViewModels"
import { DateTime } from "luxon"
import RockText from "../../../../Elements/textBox"
import RockDDL from "../../../../Elements/dropDownList"
import RockLabel from "../../../../Elements/rockLabel"
import RockField from "../../../../Controls/rockField"
import GenderDDL from "../../../../Elements/genderDropDownList"
import DatePicker from "../../../../Elements/datePicker"
import rules from "../Rules/rules"

export default defineComponent({
    name: "Checkin.Components.BasicInfo",
    components: {
      "rck-txt": RockText,
      "rck-ddl": RockDDL,
      "rck-lbl": RockLabel,
      "rck-field": RockField,
      "ddl-gender": GenderDDL,
      "rck-date": DatePicker
    },
    props: {
      showTitle: Boolean,
      showNickName: Boolean,
      showMiddleName: Boolean,
      showSuffix: Boolean,
      defaultConnectionStatus: {
        type: Object as PropType<DefinedValue>,
        required: false
      },
      connectionStatusDefinedType: Object,
      showConnectionStatus: Boolean,
      requireConnectionStatus: Boolean,
      titleDefinedType: Object,
      suffixDefinedType: Object,
      defaultMaritalStatus: {
        type: Object as PropType<DefinedValue>,
        required: false
      },
      maritalStatusDefinedType: Object,
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
      person: {
        type: Object as PropType<Person>,
        required: false
      }
    },
    setup() {

    },
    data() {
      return {
        gradeAbilityOptions: [] as any[],
        birthDate: "",
        abilityLevel: "" as any,
        genderOptions: [ { text: 'Male', value: 1 }, { text: 'Female', value: 2 } ],
        rules: rules,
      };
    },
    computed: {
      titleOptions() {
        if(this.titleDefinedType?.definedValues) {
          return this.titleDefinedType.definedValues.map((dv: any) => { return { value: dv.id, text: dv.value }; })
        } else {
          return []
        }
      },
      suffixOptions() {
        if(this.suffixDefinedType?.definedValues) {
          return this.suffixDefinedType.definedValues.map((dv: any) => { return { value: dv.id, text: dv.value }; })
        } else {
          return []
        }
      },
      connectionStatusOptions() {
        if(this.connectionStatusDefinedType?.definedValues) {
          return this.connectionStatusDefinedType.definedValues.map((dv: any) => { return { value: dv.id, text: dv.value }; })
        } else {
          return []
        }
      },
      maritalStatusOptions() {
        if(this.maritalStatusDefinedType?.definedValues) {
          return this.maritalStatusDefinedType.definedValues.map((dv: any) => { return { value: dv.id, text: dv.value }; })
        } else {
          return []
        }
      },
      nameRowColumnWidths() {
        let takenSpace = 0
        if(this.showTitle) {
          takenSpace += 2
        }
        if(this.showSuffix) {
          takenSpace += 2
        }
        let neededSpace = 6
        if(this.showMiddleName) {
          neededSpace += 3
        }
        if(this.showNickName) {
          neededSpace += 3
        }
        let availableSpace = 12 - takenSpace
        let neededFields = neededSpace / 3
        if(availableSpace <= neededSpace) {
          return { small: 2, normal: 3}
        } else {
          let size = Math.round(availableSpace / neededFields)
          if(size < 3) {
            size = 3
          }
          return { small: 2, normal: size }
        }
      },
      detailsColumnWidths() {
        let items = 0
        if(this.showBirthDate) {
          items++
        }
        if(this.showGender) {
          items++
        }
        if(this.showGradeOrAbility) {
          items++
        }
        return 12/items
      },
      age() {
        if(this.person?.birthYear && this.person?.birthMonth && this.person?.birthDay){
          let today = DateTime.now()
          let bday = DateTime.fromObject({ year: this.person.birthYear, month: this.person.birthMonth, day: this.person.birthDay})
          let diff = today.diff(bday, 'years').toObject()
          if(diff.years && diff.years >= 1) {
            return Math.floor(diff.years) + (Math.floor(diff.years) == 1 ? " Year Old" : " Years Old")
          } 
          diff = today.diff(bday,'months').toObject()
          if(diff.months && diff.months >= 1) {
            return Math.floor(diff.months) + (Math.floor(diff.months) == 1 ? " Month Old" : " Months Old")
          }
          diff = today.diff(bday,'days').toObject()
          if(diff.days && diff.days >= 1) {
            return Math.floor(diff.days) + " Days Old"
          }
        }
        return ""
      }
    },
    methods: {
      checkForDuplicates() {
        this.$emit('checkForDuplicates')
      }
    },
    watch: {
      birthDate(val) {
        if(this.person && val) {
          this.person.birthYear = val.split('-')[0]
          this.person.birthMonth = val.split('-')[1]
          this.person.birthDay = val.split('-')[2]
          if(this.person.birthYear && this.person.birthMonth && this.person.birthDay) {
            this.checkForDuplicates()
          }
        }
      },
      abilityLevel(val) {
        if(this.person && this.abilityAttribute && this.person?.attributeValues) {
          this.person.attributeValues[this.abilityAttribute.key] = JSON.stringify({ text: "", value: "", description: ""})
          this.person.graduationYear = null
          let optionType = val.split('_')[0]
          let value = val.split('_')[1]
          if(optionType == "go") {
            let gradeOffset = parseInt(value)
            if(this.graduationYear) {
              this.person.graduationYear =  this.graduationYear + gradeOffset
            }
          } else {
            this.person.attributeValues[this.abilityAttribute.key] = value
          }
        }
      }
    },
    mounted() {
      if(this.person) {
        if(this.person.birthYear && this.person.birthMonth && this.person.birthDay) {
          this.birthDate = this.person.birthYear + '-' + this.person.birthMonth + '-' + this.person.birthDay
        }
      }
      if(this.abilityDefinedType?.definedValues) {
        let options = this.abilityDefinedType.definedValues.filter((value: any) => { return value.isActive }).sort((a: any, b: any) => { return a.order - b.order }).map((value: any) => {
          let val = { text: value.value, value: value.guid, description: value.description }
          return { text: value.value, value: "dv_" + JSON.stringify(val) }
        })
        this.gradeAbilityOptions.push(...options)
      }
      if(this.gradeDefinedType) {
        let options = this.gradeDefinedType.definedValues.filter((value: any) => { return !value.description.includes("Preschool") && value.order < 8 }).sort((a: any, b: any) => { return a.order - b.order }).map((value: any) => {
          return { text: value.description, value: "go_" + value.value }
        })
        this.gradeAbilityOptions.push(...options)
      }
      let els = document.querySelectorAll(".readonly")
      els.forEach((el: any) => {
        el.setAttribute("readonly", "")
        el.setAttribute("disabled", "")
      })
    },
    template: `
<div class="row">
  <div :class="'col col-xs-12 col-md-' + nameRowColumnWidths.small" v-if="showTitle">
    <rck-ddl
      v-model="person.titleValueId"
      :options="titleOptions"
      :class="(person.id && person.id > 0) ? 'readonly' : ''"
      :label="titleDefinedType.name"
    ></rck-ddl>
  </div>
  <div :class="'col col-xs-12 col-md-' + nameRowColumnWidths.normal">
    <rck-txt
      v-model="person.firstName"
      :inputClasses="(person.id && person.id > 0) ? 'readonly' : ''"
      v-on:blur="checkForDuplicates"
      name="firstname"
      ref="fname"
      label="First Name"
      :rules="['required']"
    ></rck-txt>
  </div>
  <div :class="'col col-xs-12 col-md-' + nameRowColumnWidths.normal" v-if="showNickName">
    <rck-txt
      v-model="person.nickName"
      :inputClasses="(person.id && person.id > 0) ? 'readonly' : ''"
      label="Nick Name"
    ></rck-txt>
  </div>
  <div :class="'col col-xs-12 col-md-' + nameRowColumnWidths.normal" v-if="showMiddleName">
    <rck-txt
      v-model="person.middleName"
      :inputClasses="(person.id && person.id > 0) ? 'readonly' : ''"
      label="Middle Name"
    ></rck-txt>
  </div>
  <div :class="'col col-xs-12 col-md-' + nameRowColumnWidths.normal">
    <rck-txt
      v-model="person.lastName"
      :inputClasses="(person.id && person.id > 0) ? 'readonly' : ''"
      v-on:blur="checkForDuplicates"
      name="lastname"
      ref="lname"
      label="Last Name"
      :rules="['required']"
    ></rck-txt>
  </div>
  <div :class="'col col-xs-12 col-md-' + nameRowColumnWidths.small" v-if="showSuffix">
    <rck-ddl
      v-model="person.suffixValueId"
      :class="(person.id && person.id > 0) ? 'readonly' : ''"
      :options="suffixOptions"
      :label="suffixDefinedType.name"
    ></rck-ddl>
  </div>
</div>
<div class="row" v-if="showBirthDate || showGender || showGradeOrAbility">
  <div :class="'col col-xs-12 col-md-' + detailsColumnWidths" v-if="showGender">
    <rck-ddl
      v-model="person.gender"
      :class="(person.id && person.id > 0) ? 'readonly' : ''"
      :options="genderOptions"
      label="Gender"
      :rules="['required']"
      v-on:change="checkForDuplicates"
    ></rck-ddl>
  </div>
  <div :class="'col col-xs-12 col-md-' + detailsColumnWidths" v-if="showBirthDate">
    <div class="row row-eq-height">
      <div class="col col-xs-6">
        <rck-txt
          v-if="person.id && person.id > 0"
          inputClasses="readonly"
          v-model="birthDate"
          label="Birthdate"
        ></rck-txt>
        <rck-date
          v-else
          v-model="birthDate"
          label="Birthdate"
          :rules="['required']"
        ></rck-date>
      </div>
      <div class="col col-xs-6 pt-3">
        <br/>
        {{age}}
      </div>
    </div>
  </div>
  <div :class="'col col-xs-12 col-md-' + detailsColumnWidths" v-if="showGradeOrAbility">
    <rck-ddl
      v-model="abilityLevel"
      :class="(person.id && person.id > 0) ? 'readonly' : ''"
      :options="gradeAbilityOptions"
      label="Ability Level/Grade"
      :rules="['required']"
    ></rck-ddl>
  </div>
</div>
<div class="row">
  <div class="col col-xs-12 col-md-6" v-if="showConnectionStatus">
    <rck-ddl
      v-model="person.connectionStatusValueId"
      :class="(person.id && person.id > 0) ? 'readonly' : ''"
      :options="connectionStatusOptions"
      :label="connectionStatusDefinedType.name"
      :rules="['required']"
    ></rck-ddl>
  </div>
  <div class="col col-xs-12 col-md-6" v-if="showMaritalStatus">
    <rck-ddl
      v-model="person.maritalStatusValueId"
      :class="(person.id && person.id > 0) ? 'readonly' : ''"
      :options="maritalStatusOptions"
      :label="maritalStatusDefinedType.name"
      :rules="['required']"
    ></rck-ddl>
  </div>
</div>
`
});
