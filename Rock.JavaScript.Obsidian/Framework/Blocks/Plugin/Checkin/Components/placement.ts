import { defineComponent, PropType } from "vue"
import { Attribute, Person } from "../../../../ViewModels"
import { DateTime } from "luxon"
import DropDownList from "../../../../Elements/dropDownList"
import FormField from "../../../../Elements/rockFormField"

export default defineComponent({
    name: "Checkin.Components.Placement",
    components: {
      "rck-ddl": DropDownList,
      "rck-field": FormField
    },
    props: {
      person: {
        type: Object as PropType<Person>,
        required: false
      },
      groups: Array,
      abilityAttribute: Object,
      gradeAttribute: Object,
      startDOBAttribute: Object,
      endDOBAttribute: Object,
      graduationYear: Number
    },
    setup() {

    },
    data() {
      return {
        placementGroup: {} as any,
        defaultGroup: {} as any
      }
    },
    computed: {
      groupOptions() {
        if(this.groups && this.groups.length > 0 && this.graduationYear && this.startDOBAttribute && this.endDOBAttribute && this.gradeAttribute && this.abilityAttribute) {
          if(this.person) {
            if(this.person.birthDay && this.person.birthMonth && this.person.birthYear) {
              let abilityLevel = ""
              if(this.person.attributeValues && this.person.attributeValues['AbilityLevel']) {
                abilityLevel = JSON.parse(this.person.attributeValues['AbilityLevel']).value
              }
              let birthDate = DateTime.fromFormat(`${this.person.birthYear}-${this.person.birthMonth}-${this.person.birthDay}`, "yyyy-MM-dd")
              if(this.person.graduationYear || abilityLevel) {
                let correctGroup = []
                if(this.person.graduationYear) {
                  // Elementary
                  correctGroup = this.groups.filter((g: any) => {
                    let startDOB = g.attributeValues[this.startDOBAttribute?.key]
                    if(startDOB) {
                      if(startDOB.includes('CURRENT')) {
                        startDOB = DateTime.now
                      } else {
                        startDOB = DateTime.fromISO(startDOB)
                      }
                      if(birthDate < startDOB) {
                        return false
                      }
                    }
                    let endDOB = g.attributeValues[this.endDOBAttribute?.key]
                    if(endDOB) {
                      if(endDOB.includes('CURRENT')) {
                        endDOB = DateTime.now
                      } else {
                        endDOB = DateTime.fromISO(endDOB)
                      }
                      if(birthDate >= endDOB) {
                        return false
                      }
                    }
                    let offset = g.attributeValues[this.gradeAttribute?.key]
                    let diff = (this.person?.graduationYear ? this.person.graduationYear : 0) - (this.graduationYear ? this.graduationYear : 0)
                    return offset == diff.toString()
                  }).map((g: any) => { return { text: g.name, value: g.guid, attributeValues: g.attributeValues } })
                } else {
                  //LO/PS
                  correctGroup = this.groups.filter((g: any) => {
                    let startDOB = g.attributeValues[this.startDOBAttribute?.key]
                    if(startDOB) {
                      if(startDOB.includes('CURRENT')) {
                        startDOB = DateTime.now
                      } else {
                        startDOB = DateTime.fromISO(startDOB)
                      }
                      if(birthDate < startDOB) {
                        return false
                      }
                    }
                    let endDOB = g.attributeValues[this.endDOBAttribute?.key]
                    if(endDOB) {
                      if(endDOB.includes('CURRENT')) {
                        endDOB = DateTime.now
                      } else {
                        endDOB = DateTime.fromISO(endDOB)
                        endDOB = DateTime.fromObject({year: endDOB.year, month: endDOB.month, day: endDOB.day, hour: 23, minute: 59, second: 59})
                      }
                      if(birthDate > endDOB) {
                        return false
                      }
                    }
                    let ability = JSON.parse(g.attributeValues[this.abilityAttribute?.key]).value
                    return ability == abilityLevel
                  }).map((g: any) => { return { text: g.name, value: g.guid, attributeValues: g.attributeValues } })
                } 
                if(correctGroup && correctGroup.length > 0) {
                  this.placementGroup = correctGroup[0].value
                  this.defaultGroup = correctGroup[0]
                  return this.groups.filter((g: any) => {
                    return g.name.substring(0, g.name.length -1) == this.defaultGroup.text.substring(0, this.defaultGroup.text.length -1)
                  }).map((g: any) => { return { text: g.name, value: g.guid, attributeValues: g.attributeValues } })
                }
              }
            }
          }
        }
        return []
      },
      isValid() {
        if(this.groups && this.groups.length > 0 && this.startDOBAttribute && this.endDOBAttribute && this.gradeAttribute && this.abilityAttribute) {
          if(this.person) {
            if(this.person.birthDay && this.person.birthMonth) {
              let abilityLevel = ""
              if(this.person.attributeValues && this.person.attributeValues['AbilityLevel']) {
                abilityLevel = JSON.parse(this.person.attributeValues['AbilityLevel']).value
              }
              if(this.person.graduationYear || abilityLevel) {
                return true
              }
              return 'Fill out grade or ability level information to assign this person to a group'
            }
            return 'Fill out birthdate information to assign this person to a group'
          }
        }
        return 'Please check the block configuration to be able to assign this person to a group'
      }
    },
    methods: {
      
    },
    watch: {
      placementGroup(val) {
        if(this.person) {
          let p = this.person as any
          p.selectedGroup = val
        }
      },
      defaultGroup(val) {
        if(this.person) {
          let p = this.person as any
          p.correctGroup = val.value
        }
      }
    },
    mounted() {
      
    },
    template: `
<div class="row mt-4" v-if="isValid != true">
  <div class="col col-xs-12">
    <div class="alert alert-danger">
      {{isValid}}
    </div>
  </div>
</div>
<div class="row mt-4" v-else>
  <div class="col col-xs-12 col-md-6">
    <rck-ddl
      :options="groupOptions"
      v-model="placementGroup"
      :showBlankItem="false"
      :rules="['required']"
      label="Check-in Group"
    ></rck-ddl>
    <div class="alert alert-warning" v-if="groupOptions && groupOptions.length > 0 && placementGroup != defaultGroup.value">
      Selecting this group will also place the child in an override group. Based on their birthday they should be in {{defaultGroup.text}}.
    </div>
    <div class="alert alert-danger" v-if="isValid && (!groupOptions || groupOptions.length == 0)">
      Please double check the child's date of birth and ability level/grade. There are not any valid options for this child based on the current information.
    </div>
  </div>
</div>
`
});
