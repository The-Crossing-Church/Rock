import { defineComponent, PropType } from "vue"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import { DateTime } from "luxon"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockForm from "@Obsidian/Controls/rockForm.obs"
import Validator from "./validator"
import Toggle from "./toggle"
import rules from "../Rules/rules"
import helper from "../../EventDashboard/Helpers/helper"

export default defineComponent({
  name: "EventForm.Components.OccurrenceServices",
  components: {
    "rck-field": RockField,
    "rck-form": RockForm,
    "tcc-validator": Validator,
    "tcc-switch": Toggle,
  },
  props: {
    request: Object,
    e: {
      type: Object as PropType<ContentChannelItemBag>,
      required: false
    },
    showValidation: Boolean,
    refName: String,
    readonlySections: Array,
    isMultiOccurrence: Boolean
  },
  setup() {

  },
  data() {
    return {
      rules: rules,
      helper: helper,
      errors: [] as Record<string, string>[]
    };
  },
  computed: {
    
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
    isSectionValid(attr: string) {
      let section = this.helper.sectionInfo.filter(si => si.attr == attr) as any
      if(section && section.length > 0) {
        section = section[0]
      }
      let eventDate = helper.dateFromString(this.e?.attributeValues?.EventDate)
      let compareDate = DateTime.now()
      if(this.request?.attributeValues?.RequestStatus != 'Draft' && this.request?.startDateTime) {
        compareDate = helper.dateFromString(this.request.startDateTime)
      }
      compareDate = compareDate.startOf("day").plus({ days: section.days })
      let isValid = compareDate <= eventDate
      if(this.request?.attributeValues?.RequestStatus == 'Draft' && !isValid && this.e?.attributeValues) {
        this.e.attributeValues[attr] = 'False'
      }
      return isValid
    }
  },
  watch: {
    errors: {
      handler(val) {
        this.$emit("validation-change", { ref: this.refName, errors: val})
      },
      deep: true
    }, 
    isMultiOccurrence(val) {
      if(val == false && this.e?.attributeValues) {
        this.e.attributeValues.NeedsChildCareCatering = 'True'
      }
    },
  },
  mounted() {
    if(this.showValidation) {
      this.validate()
    }
  },
  template: `
<rck-form ref="form" @validationChanged="validationChange" v-if="isMultiOccurrence">
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-if="request?.attributeValues?.NeedsWebCalendar == 'True'">
      <tcc-switch
        v-model="e.attributeValues.NeedsWebCalendar"
        :label="e.attributes.NeedsWebCalendar.name"
        v-if="isSectionValid('NeedsWebCalendar') || e.attributeValues.NeedsWebCalendar == 'True'"
        id="boolNeedsWebCalendar"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="e.attributeValues.NeedsWebCalendar"
        :attribute="e.attributes.NeedsWebCalendar"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="boolNeedsWebCalendar"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="request?.attributeValues?.NeedsPublicity == 'True'">
      <tcc-switch
        v-model="e.attributeValues.NeedsPublicity"
        :label="e.attributes.NeedsPublicity.name"
        v-if="isSectionValid('NeedsPublicity') || e.attributeValues.NeedsPublicity == 'True'"
        id="boolNeedsPublicity"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="e.attributeValues.NeedsPublicity"
        :attribute="e.attributes.NeedsPublicity"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="boolNeedsPublicity"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-if="request?.attributeValues?.NeedsProductionAccommodations == 'True'">
      <tcc-switch
        v-model="e.attributeValues.NeedsProductionAccommodations"
        :label="e.attributes.NeedsProductionAccommodations.name"
        v-if="isSectionValid('NeedsProductionAccommodations') || e.attributeValues.NeedsProductionAccommodations == 'True'"
        id="boolNeedsProductionAccommodations"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="e.attributeValues.NeedsProductionAccommodations"
        :attribute="e.attributes.NeedsProductionAccommodations"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="boolNeedsProductionAccommodations"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="request?.attributeValues?.NeedsWorship == 'True'">
      <tcc-switch
        v-model="e.attributeValues.NeedsWorship"
        :label="e.attributes.NeedsWorship.name"
        v-if="isSectionValid('NeedsWorship') || e.attributeValues.NeedsWorship == 'True'"
        id="boolNeedsWorship"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="e.attributeValues.NeedsWorship"
        :attribute="e.attributes.NeedsWorship"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="boolNeedsWorship"
      ></rck-field>
    </div>
  </div>
</rck-form>
`
});
