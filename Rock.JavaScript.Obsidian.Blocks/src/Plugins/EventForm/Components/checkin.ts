import { defineComponent, PropType } from "vue"
import { DefinedValueBag } from "../../ViewModels/definedValueBag"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import { AttributeBag } from "../../ViewModels/attributeBag"
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag"
import { DateTime } from "luxon"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockForm from "@Obsidian/Controls/rockForm.obs"
import Validator from "./validator"
import TimePicker from "./timePicker"
import Toggle from "./toggle"
import DatePicker from "./datePicker"
import rules from "../Rules/rules"


export default defineComponent({
  name: "EventForm.Components.Registration",
  components: {
    "rck-field": RockField,
    "rck-form": RockForm,
    "tcc-validator": Validator,
    "tcc-time": TimePicker,
    "tcc-switch": Toggle,
    "tcc-date-pkr": DatePicker
  },
  props: {
    e: {
      type: Object as PropType<ContentChannelItemBag>,
      required: false
    },
    request: {
      type: Object as PropType<ContentChannelItemBag>,
      required: false
    },
    original: {
      type: Object as PropType<ContentChannelItemBag>,
      required: false
    },
    ministries: Array as PropType<DefinedValueBag[]>,
    discountAttrs: Array as PropType<AttributeBag[]>,
    regAttrs: Array as PropType<AttributeBag[]>,
    locations: Array,
    showValidation: Boolean,
    refName: String,
    readonly: Boolean,
    isMultiOccurrence: Boolean
  },
  setup() {

  },
  data() {
    return {
      rules: rules,
      errors: [] as Record<string, string>[],
      needsAdditionalRegQuestions: 'False'
    };
  },
  computed: {
    lastDate() {
      if(this.e?.attributeValues?.EventDate) {
        return this.e.attributeValues?.EventDate
      } else {
        let dates = this.request?.attributeValues?.EventDates.split(",").map((d: string) => d.trim() )
        if(dates && dates.length > 0) {
          return dates[dates.length - 1]
        }
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
    },
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
    }
  },
  watch: {
    'e.attributeValues.ExpectedAttendance'(val, original) {
      if(this.e?.attributeValues) {
        if(val && val >= 180 && this.e.attributeValues.NeedsCheckin == 'True') {
          this.e.attributeValues.NeedsDatabaseSupportTeam = 'True'
        }
        if(!val || val <= 100) {
          this.e.attributeValues.NeedsDatabaseSupportTeam = 'False'
        }
      }
    },
    'e.attributeValues.NeedsCheckin'(val, original) {
      if(this.e?.attributeValues) {
        let att = parseInt(this.e.attributeValues.ExpectedAttendance)
        if(this.e.attributeValues.ExpectedAttendance && att >= 180 && val == 'True') {
          this.e.attributeValues.NeedsDatabaseSupportTeam = 'True'
        }
        if(val == 'False') {
          this.e.attributeValues.NeedsDatabaseSupportTeam = 'False'
          this.e.attributeValues.NeedsLabels = 'False'
        }
        if(val == 'True') {
          this.e.attributeValues.NeedsAttendanceOccurrence = 'True'
        }
      }
    },
    'e.attributeValues.NeedsDatabaseSupportTeam'(val, original) {
      if(this.e?.attributeValues) {
        let att = parseInt(this.e.attributeValues.ExpectedAttendance)
        if(val == 'False' && att >= 180 && this.e.attributeValues.NeedsCheckin == 'True') {
          this.e.attributeValues.NeedsDatabaseSupportTeam = 'True'
        }
        if(this.e.attributeValues.NeedsDatabaseSupportTeam == 'True') {
          this.e.attributeValues.NeedsCheckin = 'True'
        }
      }
    },
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
    if(this.e?.attributeValues?.StartTime) {
      let dt = {} as DateTime
      if(this.e?.attributeValues?.EventDate) {
        dt = DateTime.fromFormat(`${this.e?.attributeValues?.EventDate} ${this.e?.attributeValues?.StartTime}`, "yyyy-MM-dd HH:mm:ss")
      } else if(this.request?.attributeValues?.EventDates) {
        let dates = this.request?.attributeValues?.EventDates.split(",").map(d => {
          return DateTime.fromFormat(d.trim(), "yyyy-MM-dd")
        }).sort()
        dt = DateTime.fromFormat(`${dates[0].toFormat("yyyy-MM-dd")} ${this.e?.attributeValues?.StartTime}`, "yyyy-MM-dd HH:mm:ss")
      } else {
        dt = DateTime.fromFormat(`${DateTime.now().toFormat("yyyy-MM-dd")} ${this.e?.attributeValues?.StartTime}`, "yyyy-MM-dd HH:mm:ss")
      }
      let defaultTime = dt.minus({days: 1})
      //If Childcare close one week before event
      if(this.request?.attributeValues?.NeedsChildCare == 'True') {
        defaultTime = dt.minus({weeks: 1})
      }
      if(this.e.attributeValues.RegistrationEndDate == '') {
        this.e.attributeValues.RegistrationEndDate = defaultTime.toFormat("yyyy-MM-dd")
      }
      if(this.e.attributeValues.RegistrationEndTime == '') {
        this.e.attributeValues.RegistrationEndTime = defaultTime.toFormat("HH:mm:ss")
      }
    }
    if(this.e?.attributeValues?.AdditionalRegistrationQuestions) {
      this.needsAdditionalRegQuestions = 'True'
    }
  },
  template: `
<rck-form ref="form" @validationChanged="validationChange">
  <div class="row mb-2">
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.NeedsAttendanceOccurrence"
        :label="e.attributes.NeedsAttendanceOccurrence.name"
        v-if="!readonly"
        id="boolNeedsAttendanceOccurrence"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="e.attributeValues.NeedsAttendanceOccurrence"
        :attribute="e.attributes.NeedsAttendanceOccurrence"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="boolNeedsAttendanceOccurrence"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="request.attributeValues.NeedsSpace == 'True' && e.attributeValues.NeedsAttendanceOccurrence == 'True'">
      <tcc-switch
        v-model="e.attributeValues.NeedsCheckin"
        :label="e.attributes.NeedsCheckin.name"
        v-if="!readonly"
        id="boolNeedsCheckin"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="e.attributeValues.NeedsCheckin"
        :attribute="e.attributes.NeedsCheckin"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="boolNeedsCheckin"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.NeedsCheckin == 'True'">
      <tcc-switch
        v-model="e.attributeValues.NeedsLabels"
        :label="e.attributes.NeedsLabels.name"
        v-if="!readonly"
        id="boolNeedsLabels"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="e.attributeValues.NeedsLabels"
        :attribute="e.attributes.NeedsLabels"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="boolNeedsLabels"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.NeedsCheckin == 'True' && e.attributeValues.ExpectedAttendance > 100">
      <tcc-switch
        v-model="e.attributeValues.NeedsDatabaseSupportTeam"
        :label="e.attributes.NeedsDatabaseSupportTeam.name"
        v-if="!readonly"
        id="boolNeedsDatabaseSupportTeam"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="e.attributeValues.NeedsDatabaseSupportTeam"
        :attribute="e.attributes.NeedsDatabaseSupportTeam"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="boolNeedsDatabaseSupportTeam"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.NeedsDatabaseSupportTeam == 'True'">
      <tcc-validator :name="e.attributes.DatabaseSupportStartTime.key" :rules="[rules.required(e.attributeValues.DatabaseSupportStartTime, e.attributes.DatabaseSupportStartTime.name), rules.timeCannotBeAfterEvent(e.attributeValues.DatabaseSupportStartTime, e.attributeValues.StartTime, e.attributes.DatabaseSupportStartTime.name)]" ref="validators_db_start_time" v-if="!readonly">
        <tcc-time 
          :label="e.attributes.DatabaseSupportStartTime.name"
          v-model="e.attributeValues.DatabaseSupportStartTime"
          id="timeDatabaseSupportStartTime"
        ></tcc-time>
      </tcc-validator>
      <rck-field
        v-else
        v-model="e.attributeValues.DatabaseSupportStartTime"
        :attribute="e.attributes.DatabaseSupportStartTime"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="timeDatabaseSupportStartTime"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.NeedsAttendanceOccurrence == 'True'">
      <tcc-validator :name="e.attributes.CheckinGroupName.key" :rules="[rules.charLimit(e.attributeValues.CheckinGroupName, 22, 'Check-in group names', true, null), rules.nonASCII(e.attributeValues.CheckinGroupName, 'Check-in group names', true, null)]" ref="validators_checkin_group" v-if="!readonly">
        <rck-field
          v-model="e.attributeValues.CheckinGroupName"
          :attribute="e.attributes.CheckinGroupName"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="listCheckinGroupName"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
</rck-form>
`
});
