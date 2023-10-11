import { defineComponent, provide, PropType } from "vue"
import { useConfigurationValues, useInvokeBlockAction } from "../../../Util/block"
import { Person, PhoneNumber } from "../../../ViewModels"
import { NewFamilyBlockViewModel } from "./newFamilyBlockViewModel"
import { useStore } from "../../../Store/index"
import { Button } from "ant-design-vue"
import Panel from "../../../Controls/panel"
import Modal from "../../../Controls/modal"
import PersonPicker from "../../../Controls/personPicker"
import Member from "./Components/member"
const store = useStore()

export default defineComponent({
  name: "Checkin.NewFamily",
  components: {
    "rck-panel": Panel,
    "rck-modal": Modal,
    "pkr-person": PersonPicker,
    "row-member": Member,
    "a-btn": Button
  },
  setup() {
    const viewModel = useConfigurationValues<NewFamilyBlockViewModel | null>();
    const invokeBlockAction = useInvokeBlockAction();
    
    /** A method to process new family members */
    const save: (parents: Person[], children: Person[], phonenumbers: PhoneNumber[], placements: any[]) => Promise<any> = async (parents, children, phonenumbers, placements) => {
        const response = await invokeBlockAction<{ expirationDateTime: string }>("ProcessFamily", {
            parents: parents, children: children, phonenumbers: phonenumbers, placements: placements
        });
        if (response) {
          return response
        }
    };
    provide("save", save);
      
    /** A method to check for existing person */
    const findExisting: (person: Person, phonenumber:String) => Promise<any> = async (person, phonenumber) => {
        const response = await invokeBlockAction<{ expirationDateTime: string }>("CheckForExisting", {
          viewModel: person, mobileNumber: phonenumber
        });
        if (response) {
          return response
        }
    };
    provide("findExisting", findExisting);

    return {
      viewModel,
      save,
      findExisting
    }
  },
  data() {
    return {
      parents: [] as Person[],
      children: [] as Person[],
      existingAdult: {} as any,
      modal: false,
      matchesFoundAlert: false,
      showValidation: false,
      showResults: false,
      message: "",
      alertClass: "alert alert-success",
      loading: false
    }
  },
  computed: {

  },
  methods: {
    addPerson(list: Person[], ageClassification: number) {
      let newPerson = JSON.parse(JSON.stringify(this.viewModel?.emptyPerson))
      if(this.viewModel?.emptyPerson && this.viewModel.emptyPerson.attributes) {
        newPerson.attributes = JSON.parse(JSON.stringify(this.viewModel.emptyPerson.attributes))
      }
      newPerson.phoneNumbers = [ JSON.parse(JSON.stringify(this.viewModel?.emptyPersonPhoneNumber)) ]
      newPerson.ageClassification = ageClassification
      list.push(newPerson)
    },
    setPerson() {
      this.modal = false
      let url = window.location.origin + window.location.pathname + "?Guid=" + this.existingAdult?.value
      window.location.assign(url)
    },
    removeParent(person: Person) {
      if(this.parents.length > 1) {
        let idx = this.parents.indexOf(person)
        this.parents.splice(idx, 1)
      }
    },
    removeChild(person: Person) {
      if(this.children.length > 1) {
        let idx = this.children.indexOf(person)
        this.children.splice(idx, 1)
      }
    },
    processForm() {
      this.loading = true
      this.showValidation = false
      this.showValidation = true
      let members = this.$refs.member as any[]
      let hasErrors = false
      members.forEach((ref: any) => {
        let form = ref.$refs.form as any
        if(Object.keys(form.errors).length > 0) {
          hasErrors = true
        }
      })
      if(!hasErrors) {
        let el = document.getElementById('updateProgress')
        if(el) {
          el.style.display = 'block'
        }
        let numbers = [] as any[]
        this.parents.forEach((p: any) => {
          numbers.push(...p.phoneNumbers)
        })
        let placements = [] as any[]
        this.children.forEach((p: any) => {
          if(p.selectedGroup && p.correctGroup) {
            placements.push({selectedGroup: p.selectedGroup, correctGroup: p.correctGroup})
          }
        })
        this.save(this.parents, this.children, numbers, placements)
        .then((res) => {
          this.showValidation = false
          console.log(res)
          if(res.isSuccess) {
            //Family Processed
            this.message = ""
            for(let i = 0; i < res.data.people.length; i++) {
              let person = res.data.people[i]
              let membership = res.data.members.filter((gm: any) => { return gm.personId == person.id })
              if(membership && membership.length > 0) {
                this.message += person.nickName + " was added to:<br/>"
                for(let j = 0; j < membership.length; j++) {
                  let group = res.data.groups.filter((g: any) => { return g.id == membership[j].groupId })
                  if(group) {
                    this.message += group[0].name + "<br/>"
                  }
                }
                this.message += "<br/>"
              }
            }
            this.message += `<a href="/Person/${res.data.people[0].id}" class="btn btn-primary my-2">View Profile<a/>`
            this.alertClass = "alert alert-success"
            this.showResults = true
          } else {
            console.log(res.errorMessage)
            this.message = res.errorMessage
            this.alertClass = "alert alert-danger"
            this.showResults = true
          }
          window.scrollTo(0, 0)
        }).catch((err) => {
          this.showValidation = false
        }).finally(() => {
          this.loading = false
          if(el) {
            el.style.display = 'none'
          }
        })
      } else {
        this.loading = false
      }
    },
    openForm() {
      window.location.href = window.location.href.split("?")[0]
    }
  },
  watch: {

  },
  mounted() {
    let p = JSON.parse(JSON.stringify(this.viewModel?.emptyPerson))
    if(this.viewModel?.existingPerson && this.viewModel.existingPerson.id > 0) {
      p = this.viewModel.existingPerson
      if(this.viewModel.existingPersonPhoneNumber) {
        p.phoneNumbers = [ this.viewModel.existingPersonPhoneNumber ]
      } else {
        p.phoneNumbers = [ JSON.parse(JSON.stringify(this.viewModel?.emptyPersonPhoneNumber)) ]
      }
      p.phoneNumberCantBeMessaged = this.viewModel.existingPersonPhoneCantBeMessaged
    } else {
      p.ageClassification = 1
      p.phoneNumbers = [ JSON.parse(JSON.stringify(this.viewModel?.emptyPersonPhoneNumber)) ]
    }
    this.parents.push(p)

    let c = JSON.parse(JSON.stringify(this.viewModel?.emptyPerson))
    c.ageClassification = 2
    this.children.push(c)
  },
  template: `
<div class="container" v-if="showResults">
  <div :class="alertClass" v-html="message"></div>
  <a class="pull-right btn btn-primary mt-2" @click="openForm">New Family Form</a>
</div>
<div class="container" v-else>
  <div class="alert alert-warning" role="alert" v-if="matchesFoundAlert">
    Existing people were found in the database, please review
  </div>
  <rck-panel
    title="Parents"
  >
    <div style="height: 26px;">
      <a class="pull-right btn btn-xs btn-default" @click="modal = true">
        Select Existing Parent
      </a>
    </div>
    <template v-for="(p, idx) in parents">
      <row-member 
        ref="member"
        :showTitle="(viewModel.showTitle == 'ALL' || viewModel.showTitle == 'ADULT') ? true : false" 
        :titleDefinedType="viewModel.titleDefinedType"
        :showNickName="(viewModel.showNickName == 'ALL' || viewModel.showNickName == 'ADULT') ? true : false"
        :showMiddleName="(viewModel.showMiddleName == 'ALL' || viewModel.showMiddleName == 'ADULT') ? true : false"
        :showSuffix="(viewModel.showSuffix == 'ALL' || viewModel.showSuffix == 'ADULT') ? true : false"
        :suffixDefinedType="viewModel.suffixDefinedType"
        :defaultConnectionStatus="viewModel.defaultConnectionStatus"
        :connectionStatusDefinedType="viewModel.connectionStatusDefinedType"
        :showConnectionStatus="(viewModel.requireConnectionStatus == 'CHILD' || viewModel.requireConnectionStatus == 'HIDDEN') ? false : true"
        :requireConnectionStatus="(viewModel.requireConnectionStatus == 'ALL' || viewModel.requireConnectionStatus == 'ADULT') ? true : false"
        :maritalStatusDefinedType="viewModel.maritalStatusDefinedType"
        :defaultMaritalStatus="viewModel.defaultAdultMaritalStatus"
        :showMaritalStatus="(viewModel.showMaritalStatus == 'ALL' || viewModel.showMaritalStatus == 'ADULT') ? true : false"
        :showBirthDate="(viewModel.requireBirthDate == 'CHILD' || viewModel.requireBirthDate == 'HIDDEN') ? false : true"
        :requireBirthDate="(viewModel.requireBirthDate == 'ALL' || viewModel.requireBirthDate == 'ADULT') ? true : false"
        :showGender="(viewModel.requireGender == 'CHILD' || viewModel.requireGender == 'HIDDEN') ? false : true"
        :requireGender="(viewModel.requireGender == 'ALL' || viewModel.requireGender == 'ADULT') ? true : false"
        :showGradeOrAbility="(viewModel.requireGradeOrAbility == 'CHILD' || viewModel.requireGradeOrAbility == 'HIDDEN') ? false : true"
        :requireGradeOrAbility="(viewModel.requireGradeOrAbility == 'ALL' || viewModel.requireGradeOrAbility == 'ADULT') ? true : false"
        :abilityAttribute="viewModel.abilityLevelAttribute"
        :abilityDefinedType="viewModel.abilityLevelDefinedType"
        :gradeDefinedType="viewModel.gradeDefinedType"
        :graduationYear="viewModel.graduationYear"
        :showEmail="viewModel.showEmail"
        :showEmailOptOut="viewModel.showEmailOptOut"
        :showCell="viewModel.showCell"
        :showSMS="viewModel.showSMSEnabled"
        :phoneType="viewModel.phoneType"
        :attributes="viewModel.adultAttributes"
        :canRemove="parents.length > 1"
        :person="p"
        :showValidation="showValidation"
        :findExisting="findExisting"
        v-on:removePerson="removeParent"
      ></row-member>
      <hr v-if="idx < (parents.length - 1)" />
    </template>
    <div class="w-100 mt-3" v-if="!viewModel.existingPerson">
      <a class="pull-right btn btn-xs btn-default" @click="addPerson(parents, 1)" v-if="parents.length < 2">
        <i class="fa fa-user"></i>  
        Add Parent
      </a>
    </div>
  </rck-panel>
  <rck-panel
    title="Children"
  >
    <template v-for="(c, idx) in children">
      <row-member 
        ref="member"
        :showTitle="(viewModel.showTitle == 'ALL' || viewModel.showTitle == 'CHILD') ? true : false" 
        :titleDefinedType="viewModel.titleDefinedType"
        :showNickName="(viewModel.showNickName == 'ALL' || viewModel.showNickName == 'CHILD') ? true : false"
        :showMiddleName="(viewModel.showMiddleName == 'ALL' || viewModel.showMiddleName == 'CHILD') ? true : false"
        :showSuffix="(viewModel.showSuffix == 'ALL' || viewModel.showSuffix == 'CHILD') ? true : false"
        :suffixDefinedType="viewModel.suffixDefinedType"
        :defaultConnectionStatus="viewModel.defaultConnectionStatus"
        :connectionStatusDefinedType="viewModel.connectionStatusDefinedType"
        :showConnectionStatus="(viewModel.requireConnectionStatus == 'ADULT' || viewModel.requireConnectionStatus == 'HIDDEN') ? false : true"
        :requireConnectionStatus="(viewModel.requireConnectionStatus == 'ALL' || viewModel.requireConnectionStatus == 'CHILD') ? true : false"
        :maritalStatusDefinedType="viewModel.maritalStatusDefinedType"
        :defaultMaritalStatus="viewModel.defaultChildMaritalStatus"
        :showMaritalStatus="(viewModel.showMaritalStatus == 'ALL' || viewModel.showMaritalStatus == 'CHILD') ? true : false"
        :showBirthDate="(viewModel.requireBirthDate == 'ADULT' || viewModel.requireBirthDate == 'HIDDEN') ? false : true"
        :requireBirthDate="(viewModel.requireBirthDate == 'ALL' || viewModel.requireBirthDate == 'CHILD') ? true : false"
        :showGender="(viewModel.requireGender == 'ADULT' || viewModel.requireGender == 'HIDDEN') ? false : true"
        :requireGender="(viewModel.requireGender == 'ALL' || viewModel.requireGender == 'CHILD') ? true : false"
        :showGradeOrAbility="(viewModel.requireGradeOrAbility == 'ADULT' || viewModel.requireGradeOrAbility == 'HIDDEN') ? false : true"
        :requireGradeOrAbility="(viewModel.requireGradeOrAbility == 'ALL' || viewModel.requireGradeOrAbility == 'CHILD') ? true : false"
        :abilityAttribute="viewModel.abilityLevelAttribute"
        :abilityDefinedType="viewModel.abilityLevelDefinedType"
        :gradeDefinedType="viewModel.gradeDefinedType"
        :graduationYear="viewModel.graduationYear"
        :showEmail="false"
        :showEmailOptOut="false"
        :showCell="false"
        :showSMS="false"
        :attributes="viewModel.childAttributes"
        :groups="viewModel.groups"
        :groupStartDOBAttribute="viewModel.groupStartDOBAttribute"
        :groupEndDOBAttribute="viewModel.groupEndDOBAttribute"
        :groupAbilityAttribute="viewModel.groupAbilityAttribute"
        :groupGradeAttribute="viewModel.groupGradeAttribute"
        :canRemove="children.length > 1"
        :person="c"
        :showValidation="showValidation"
        :findExisting="findExisting"
        v-on:removePerson="removeChild"
      ></row-member>
      <hr v-if="idx < (children.length - 1)" />
    </template>
    <div class="w-100 mt-3" style="height: 26px;">
      <a class="pull-right btn btn-xs btn-default" @click="addPerson(children, 2)">
        <i class="fa fa-user"></i>  
        Add Child
      </a>
    </div>
    <div class="w-100 mt-3">
      <a-btn class="pull-right btn btn-primary" :loading="loading" @click="processForm">
        Process Family
      </a-btn>
    </div>
  </rck-panel>
  <rck-modal v-model="modal">
    <div>
      If results don't automatically load hit "enter" to load search results.
    </div>
    <div class="row mt-3">
      <div class="col col-xs-12 col-md-4">
        <pkr-person
          v-model="existingAdult"
        ></pkr-person>
      </div>
    </div>
    <template #customButtons>
      <a class="btn btn-primary" @click="setPerson">Select</a>
    </template>
  </rck-modal>
</div>
<v-style>
  label, .control-label {
    color: rgba(0,0,0,.6);
    line-height: 18px;
    letter-spacing: normal;
    font-size: 14px;
  }
  .picker.picker-select.person-picker > div {
    left: auto !important;
    top: auto !important;
    margin-top: 35px;
    width: 400px !important;
    height: 400px !important;
  }
  .picker.picker-select.person-picker div .panel {
    width: 100%;
    height: 100%;
  }
  .picker.picker-select.person-picker div .panel .panel-body div div {
    flex-direction: column;
  }
</v-style>
`
})