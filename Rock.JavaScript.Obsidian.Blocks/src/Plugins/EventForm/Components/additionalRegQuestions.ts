import { defineComponent, PropType, readonly } from "vue"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import { AttributeBag } from "../../ViewModels/attributeBag"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockForm from "@Obsidian/Controls/rockForm.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import Modal from "@Obsidian/Controls/modal.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"
import DropDownList from "@Obsidian/Controls/dropDownList.obs"
import FieldTypeEditor from "@Obsidian/Controls/fieldTypeEditor.obs"

type RegistrationQuestion = {
  QuestionName: string,
  QuestionFieldType: string,
  QuestionConfigurationValues: string
}

export default defineComponent({
  name: "EventForm.Components.RegistrationQuestions",
  components: {
    "rck-field": RockField,
    "rck-form": RockForm,
    "rck-lbl": RockLabel,
    "rck-modal": Modal,
    "rck-btn": RockButton,
    "rck-ddl": DropDownList,
    "rck-field-type": FieldTypeEditor
  },
  props: {
    e: {
      type: Object as PropType<ContentChannelItemBag>,
      required: false
    },
    attrs: Array as PropType<AttributeBag[]>,
    readonly: Boolean
  },
  setup() {

  },
  data() {
    return {
      regQuestions: [] as RegistrationQuestion[],
      newQuestion: {} as any,
      fieldTypeConfig: {} as any,
      fieldTypeValue: {} as any,
      attributeGuid: "",
      modal: false,
      message: "",
      selectedIdx: -1
    };
  },
  computed: {
    nameAttr() {
      if(this.attrs) {
        let attr = this.attrs.filter((a: any) => { return a.key == "QuestionName" })
        if(attr && attr.length > 0) {
          return attr[0]
        }
      }
      return null
    },
    fieldTypeAttr() {
      if(this.attrs) {
        let attr = this.attrs.filter((a: any) => { return a.key == "QuestionFieldType" })
        if(attr && attr.length > 0) {
          if(attr[0].qualifierValues) {
            let vals = attr[0].qualifierValues.values?.split(',') as any[]
            if(vals && vals.length > 0) {
              vals = vals.map(v => { 
                let val = v.replace('\n', '').trim()
                if(val.includes('^')) {
                  val = val.split('^')
                  return { value: val[0], text: val[1] } 
                }
                return { value: val, text: val } 
              })
              attr[0].configurationValues = { 
                values: JSON.stringify(vals)
              }
            }
          }
          return attr[0]
        }
      }
      return null
    },
    wrapperClass() {
      let className = "attribute-configuration"
      if(this.newQuestion.QuestionFieldType == "T-Shirt Size" || this.newQuestion.QuestionFieldType == "School" || 
        this.newQuestion.QuestionFieldType == "Grade" || this.newQuestion.QuestionFieldType =="College Year") {
          className += " defined-value-config"
      }

      return className
    }
  },
  methods: {
    edit(idx: number) {
      this.newQuestion = JSON.parse(JSON.stringify(this.regQuestions[idx]))
      this.fieldTypeConfig = JSON.parse(this.newQuestion.QuestionConfigurationValues)
      this.selectedIdx = idx
      this.modal = true
    },
    save() {
      if(this.newQuestion.QuestionName && this.newQuestion.QuestionFieldType) {
        this.newQuestion.QuestionConfigurationValues = JSON.stringify(this.fieldTypeConfig)
        // Save field type name for easier dashboard display
        if(this.fieldTypeAttr?.configurationValues?.values) {
          let fieldTypes = JSON.parse(this.fieldTypeAttr?.configurationValues?.values)
          let ft = fieldTypes.filter(ft => ft.value == this.newQuestion.QuestionFieldType)
          if(ft && ft.length > 0) {
            this.newQuestion.QuestionFieldTypeName = ft[0].text
          }
        }
        if(this.selectedIdx >= 0) {
          this.regQuestions[this.selectedIdx] = this.newQuestion
        } else {
          this.regQuestions.push(this.newQuestion as RegistrationQuestion)
        }
        this.modal = false
        this.message = ""
      } else {
        let msg = "Registration questions require a name and field type. "
        this.message = msg
      }
    },
    removeQuestion() {
      if(this.selectedIdx >= 0) {
        this.regQuestions.splice(this.selectedIdx, 1)
      }
      this.modal = false
    },
    getFieldType(question) {
      if(this.fieldTypeAttr) {
        if(this.fieldTypeAttr.configurationValues?.values) {
          let ddl = JSON.parse(this.fieldTypeAttr.configurationValues.values)
          if(ddl && ddl.length > 0) {
            let ft = ddl.filter(li => li.value == question.QuestionFieldType)
            if(ft && ft.length > 0) {
              return ft[0].text
            }
          }
        }
      }
      return ""
    }
  },
  watch: {
    regQuestions: {
      handler(val) {
        if(this.e?.attributeValues) {
          this.e.attributeValues.AdditionalRegistrationQuestions = JSON.stringify(val)
        }
      },
      deep: true
    },
    modal(val) {
      if(!val) {
        this.selectedIdx = -1
      }
    },
    'newQuestion.QuestionFieldType'(val, original) {
      this.fieldTypeConfig = { fieldTypeGuid: "" }
      setTimeout(() => {
        let config = { fieldTypeGuid: "", defaultValue: "", configurationValues: {} } as any
        if(val == "T-Shirt Size" || val == "School" || val == "Grade" || val =="College Year") {
          config.fieldTypeGuid = "59D5A94C-94A0-4630-B80A-BB25697D74C7"
          if(val == "T-Shirt Size") {
            config.configurationValues.definedtype = "8196d1f1-3974-415c-828f-20b4c3b39c7a"
          }
          if(val == "School") {
            config.configurationValues.definedtype = "576ff1e2-6225-4565-a16d-230e26167a3d"
          }
          if(val == "Grade") {
            config.configurationValues.definedtype = "24e5a79f-1e62-467a-ad5d-0d10a2328b4d"
            config.configurationValues.displaydescription = "True"
          }
          if(val == "College Year") {
            config.configurationValues.definedtype = "576ff1e2-6225-4565-a16d-230e26167a3d"
          }
        } else {
          config.fieldTypeGuid = val
        }
        if(this.newQuestion.QuestionConfigurationValues) {
          config = JSON.parse(this.newQuestion.QuestionConfigurationValues)
        }

        this.fieldTypeConfig = config
      }, 500)
    }
  },
  mounted() {
    if(this.e?.attributeValues) {
      if(this.e?.attributeValues.AdditionalRegistrationQuestions) {
        this.regQuestions = JSON.parse(this.e.attributeValues.AdditionalRegistrationQuestions)
      }
    }
  },
  template: `
<rck-lbl class="mt-2">Additional Registration Questions</rck-lbl>
<div class="setup-table mb-2">
  <div class="row">
    <div class="col col-xs-11">
      <template v-if="regQuestions.length > 0">
        <ul>
          <li v-for="(rq, idx) in regQuestions" :key="idx">
            <b>{{rq.QuestionName}}:</b> {{getFieldType(rq)}} <i class="fa fa-pencil text-accent hover" @click="edit(idx)"></i>
          </li>
        </ul>
      </template>
      <template v-else>
        Click the add button to add a question to your event registration.
      </template>
    </div>
    <div class="col col-xs-1">
      <rck-btn class="pull-right btn-circle" btnType="accent" @click="newQuestion = { QuestionName: '', QuestionFieldType: '', QuestionConfigurationValues: '' }; modal = true;" v-if="!readonly" id="btnNewQuestion">
        <i class="fa fa-plus"></i>
      </rck-btn>
    </div>
  </div>
</div>
<rck-modal v-model="modal" style="min-width: 50%;"  :isCloseButtonHidden="true" cancelText="" :clickBackdropToClose="true" modalWrapperClasses="modal-no-header">
  <div class="alert alert-danger mt-2" v-if="message != ''">
    {{message}}
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="newQuestion.QuestionName"
        :attribute="nameAttr"
        :is-edit-mode="true"
        id="txtQuestionName"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="newQuestion.QuestionFieldType"
        :attribute="fieldTypeAttr"
        :is-edit-mode="true"
        id="txtQuestionFieldType"
      ></rck-field>
    </div>
  </div>
  <div :class="wrapperClass">
    <rck-field-type
      v-if="fieldTypeConfig.fieldTypeGuid"
      v-model="fieldTypeConfig" 
      :isFieldTypeReadOnly="true" 
      :attributeGuid="attributeGuid"
    ></rck-field-type>
  </div>
  <template #customButtons>
    <rck-btn btnType="red" @click="removeQuestion" v-if="selectedIdx >= 0" id="btnDeleteQuestion">Delete</rck-btn>
    <rck-btn btnType="primary" @click="save" id="btnSaveQuestion">Save</rck-btn>
  </template>
</rck-modal>
<v-style>
.setup-table {
  border-radius: 6px;
  border: 1px solid #dfe0e1;
  padding: 8px;
}
.setup-row {
  display: flex;
  align-items: center;
}
.setup-row:not(:last-child) {
  border-bottom: 1px solid #F0F0F0;
}
.spacer {
  flex-grow: 1!important;
}
/* Hide Defined Value Selector, Show Inactive, and Allow Adding New Values */
.attribute-configuration.defined-value-config > div > div:not(.form-group):not(:last-child) > div:first-child,
.attribute-configuration.defined-value-config > div > div:not(.form-group):not(:last-child) > div:nth-child(5),
.attribute-configuration.defined-value-config > div > div:not(.form-group):not(:last-child) > div:nth-child(6) {
  display: none;
}
</v-style>
`
});
