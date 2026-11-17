import { defineComponent, PropType } from "vue"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockForm from "@Obsidian/Controls/rockForm.obs"
import Validator from "./validator"
import Toggle from "./toggle"
import rules from "../Rules/rules"

export default defineComponent({
  name: "EventForm.Components.ChildcareCatering",
  components: {
    "rck-field": RockField,
    "rck-form": RockForm,
    "tcc-validator": Validator,
    "tcc-switch": Toggle,
  },
  props: {
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
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.NeedsWebCalendar"
        :label="e.attributes.NeedsWebCalendar.name"
        v-if="!readonlySections.includes('Calendar')"
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
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.NeedsPublicity"
        :label="e.attributes.NeedsPublicity.name"
        v-if="!readonlySections.includes('Publicity')"
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
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.NeedsProductionAccommodations"
        :label="e.attributes.NeedsProductionAccommodations.name"
        v-if="!readonlySections.includes('Production')"
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
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.NeedsWorship"
        :label="e.attributes.NeedsWorship.name"
        v-if="!readonlySections.includes('Worship')"
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
