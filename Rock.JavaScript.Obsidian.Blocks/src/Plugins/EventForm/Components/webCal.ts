import { defineComponent, PropType } from "vue"
import { DefinedValueBag } from "../../ViewModels/definedValueBag"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag"
import Modal from "@Obsidian/Controls/modal.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockForm from "@Obsidian/Controls/rockForm.obs"
import Toggle from "./toggle"
import Validator from "./validator"
import DatePicker from "./datePicker"
import { DateTime } from "luxon"
import rules from "../Rules/rules"


export default defineComponent({
  name: "EventForm.Components.WebCalendar",
  components: {
    "rck-modal": Modal,
    "rck-btn": RockButton,
    "rck-field": RockField,
    "rck-form": RockForm,
    "tcc-date-pkr": DatePicker,
    "tcc-validator": Validator,
    "tcc-switch": Toggle,
  },
  props: {
    request: {
      type: Object as PropType<ContentChannelItemBag>,
      required: false
    },
    original: {
      type: Object as PropType<ContentChannelItemBag>,
      required: false
    },
    ministries: Array as PropType<DefinedValueBag[]>,
    showValidation: Boolean,
    refName: String,
    readonly: Boolean
  },
  setup() {

  },
  data() {
    return {
      rules: rules,
      errors: [] as Record<string, string>[],
      unlockModal: false,
      newStartDate: ""
    };
  },
  computed: {
    lastDate() {
      let dates = this.request?.attributeValues?.EventDates.split(",").map((d: string) => d.trim())
      if(dates && dates.length > 0) {
        return dates[dates.length - 1]
      }
      return ""
    },
    earliestDate() {
      let submissionDate = DateTime.now()
      if(this.request?.attributeValues) {
        let isFuneralRequest = false
        let val = this.request.attributeValues.Ministry
        let ministry = {} as DefinedValueBag | undefined
        if(val != '') {
          let min = JSON.parse(val) as ListItemBag
          ministry = this.ministries?.filter((dv: any) => {
            return dv.guid == min.value
          })[0]
        }
        if(ministry?.value?.toLowerCase().includes("funeral")) {
          isFuneralRequest = true
        }
        //Funeral Request, can submit for today
        if(isFuneralRequest) {
          return submissionDate.toFormat("yyyy-MM-dd")
        }
        //New request, must be min two weeks from today
        if(this.request.idKey || this.request.attributeValues.RequestStatus == 'Draft') {
          return submissionDate.plus({days: 14}).toFormat("yyyy-MM-dd")
        }
        //if newly requesting registration, two weeks from today, else two weeks from submission date
        //Existing request, no pending changes
        if(this.original?.attributeValues?.NeedsRegistration == 'True') {
          let val = this.request.startDateTime as string
          submissionDate = DateTime.fromISO(val)
        }
        return submissionDate.plus({days: 14}).toFormat("yyyy-MM-dd")
      }
    }
  },
  methods: {
    validate() {
      let formRef = this.$refs as any
      for(let r in formRef) {
        if(formRef[r].className?.includes("validator")) {
          formRef[r].validate()
        }
      }
    },
    validationChange(errs: Record<string, string>[]) {
      this.errors = errs
    },
    unlock() {
      if(this.request?.attributeValues) {
        this.request.attributeValues.WebCalendarGoLive = this.newStartDate
        this.unlockModal = false
        this.$emit("validate")
      }
    }
  },
  watch: {
    errors: {
      handler(val) {
        this.$emit("validation-change", { ref: this.refName, errors: val})
      },
      deep: true
    }
  },
  mounted() {
    if(this.showValidation) {
      this.validate()
    }
  },
  template: `
<rck-form ref="form" @validationChanged="validationChange">
  <div class="row row-equal-height">
    <div class="col col-xs-12 col-md-6 d-flex">
      <tcc-validator :name="request.attributes.WebCalendarGoLive.key" :rules="[rules.required(request.attributeValues.WebCalendarGoLive, request.attributes.WebCalendarGoLive.name), rules.dateCannotBeAfterEvent(request.attributeValues.WebCalendarGoLive, lastDate, request.attributes.WebCalendarGoLive.name)]" ref="validators_start" v-if="!readonly">
        <tcc-date-pkr
          :label="request.attributes.WebCalendarGoLive.name"
          v-model="request.attributeValues.WebCalendarGoLive"
          :min="earliestDate"
          :max="lastDate"
          id="dateWebCalendarGoLive"
        ></tcc-date-pkr>
      </tcc-validator>
      <rck-field
        v-else
        v-model="request.attributeValues.WebCalendarGoLive"
        :attribute="request.attributes.WebCalendarGoLive"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="dateWebCalendarGoLive"
      ></rck-field>
      <rck-btn 
        v-if="readonly" 
        btnType="primary" 
        class="ml-2 btn-circle"
        @click="newStartDate = request.attributeValues.WebCalendarGoLive; unlockModal = true;"
      >
        <i class="ti ti-lock-open"></i>
      </rck-btn>
    </div>
    <div class="col col-xs-12 col-md-6 pt-3">
      <tcc-switch
        v-model="request.attributeValues.DisplayOnPublicCalendar"
        :label="request.attributes.DisplayOnPublicCalendar.name"
        v-if="!readonly"
        id="boolDisplayOnPublicCalendar"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="request.attributeValues.DisplayOnPublicCalendar"
        :attribute="request.attributes.DisplayOnPublicCalendar"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="boolDisplayOnPublicCalendar"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6 pt-3">
      <tcc-switch
        v-model="request.attributeValues.NeedsCalendarShortLink"
        :label="request.attributes.NeedsCalendarShortLink.name"
        v-if="!readonly"
        id="boolNeedsCalendarShortLink"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="request.attributeValues.NeedsCalendarShortLink"
        :attribute="request.attributes.NeedsCalendarShortLink"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="boolNeedsCalendarShortLink"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <tcc-validator :name="request.attributes.WebCalendarDescription.key" :rules="[rules.required(request.attributeValues.WebCalendarDescription, request.attributes.WebCalendarDescription.name)]" ref="validators_webcal">
        <rck-field
          v-model="request.attributeValues.WebCalendarDescription"
          :attribute="request.attributes.WebCalendarDescription"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="txtWebCalendarDescription"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <br/>
  
  <rck-modal 
    v-model="unlockModal" 
    width="80%" 
    :clickBackdropToClose="true" 
    :isCloseButtonHidden="true" 
    cancelText="" 
    modalWrapperClasses="modal-no-header"
  >
    <h4>Unlock Web Calendar Information</h4>
    <p>
      To edit the calendar information for this request you will have to pick a new go live date farther in the future. If you wish to keep your go live date you cannot make any changes to this section.
    </p>
    <tcc-validator :name="request.attributes.WebCalendarGoLive.key" :rules="[rules.required(request.attributeValues.WebCalendarGoLive, request.attributes.WebCalendarGoLive.name), rules.dateCannotBeAfterEvent(request.attributeValues.WebCalendarGoLive, lastDate, request.attributes.WebCalendarGoLive.name)]" ref="validators_start">
      <tcc-date-pkr
        :label="request.attributes.WebCalendarGoLive.name"
        v-model="newStartDate"
        :min="earliestDate"
        :max="lastDate"
        id="dateWebCalendarGoLive"
      ></tcc-date-pkr>
    </tcc-validator>
    <template #customButtons>
      <rck-btn btnType="primary" @click="unlock">Unlock</rck-btn>
      <rck-btn btnType="grey" @click="unlockModal = false;">Cancel</rck-btn>
    </template>
  </rck-modal>
</rck-form>
`
});
