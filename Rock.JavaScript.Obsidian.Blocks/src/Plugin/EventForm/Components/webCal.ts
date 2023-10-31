import { defineComponent, PropType } from "vue"
import { ContentChannelItem, DefinedValue, ListItem, Attribute } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField"
import RockForm from "../../../../Controls/rockForm"
import Validator from "./validator"
import DatePicker from "./datePicker"
import { DateTime } from "luxon"
import rules from "../Rules/rules"


export default defineComponent({
    name: "EventForm.Components.WebCalendar",
    components: {
      "rck-field": RockField,
      "rck-form": RockForm,
      "tcc-date-pkr": DatePicker,
      "tcc-validator": Validator,
    },
    props: {
      request: {
          type: Object as PropType<ContentChannelItem>,
          required: false
      },
      original: {
        type: Object as PropType<ContentChannelItem>,
        required: false
      },
      ministries: Array as PropType<DefinedValue[]>,
      showValidation: Boolean,
      refName: String,
      readonly: Boolean
    },
    setup() {

    },
    data() {
        return {
          rules: rules,
          errors: [] as Record<string, string>[]
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
          let ministry = {} as DefinedValue | undefined
          if(val != '') {
            let min = JSON.parse(val) as ListItem
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
          if(this.request.id == 0 || this.request.attributeValues.RequestStatus == 'Draft') {
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
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(request.attributeValues.WebCalendarGoLive, request.attributes.WebCalendarGoLive.name), rules.dateCannotBeAfterEvent(request.attributeValues.WebCalendarGoLive, lastDate, request.attributes.WebCalendarGoLive.name)]" ref="validators_start" v-if="!readonly">
        <tcc-date-pkr
          :label="request.attributes.WebCalendarGoLive.name"
          v-model="request.attributeValues.WebCalendarGoLive"
          :min="earliestDate"
          :max="lastDate"
        ></tcc-date-pkr>
      </tcc-validator>
      <rck-field
        v-else
        v-model="request.attributeValues.WebCalendarGoLive"
        :attribute="request.attributes.WebCalendarGoLive"
        :is-edit-mode="false"
        :showEmptyValue="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <tcc-validator :rules="[rules.required(request.attributeValues.WebCalendarDescription, request.attributes.WebCalendarDescription.name)]" ref="validators_webcal">
        <rck-field
          v-model="request.attributeValues.WebCalendarDescription"
          :attribute="request.attributes.WebCalendarDescription"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <br/>
</rck-form>
`
});
