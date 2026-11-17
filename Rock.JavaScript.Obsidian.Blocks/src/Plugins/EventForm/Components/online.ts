import { defineComponent, PropType } from "vue"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockForm from "@Obsidian/Controls/rockForm.obs"
import Validator from "./validator"
import Toggle from "./toggle"
import rules from "../Rules/rules"


export default defineComponent({
  name: "EventForm.Components.Online",
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
    readonly: Boolean,
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
        this.e.attributeValues.NeedsOnline = 'True'
      }
    },
  },
  mounted() {
    if(this.showValidation) {
      this.validate()
    }
  },
  template: `
<div class="row" v-if="isMultiOccurrence">
  <div class="col col-xs-12 col-md-6">
    <tcc-switch
      v-model="e.attributeValues.NeedsOnline"
      :label="e.attributes.NeedsOnline.name"
      v-if="!readonly"
      id="boolNeedsOnline"
    ></tcc-switch>
    <rck-field
      v-else
      v-model="e.attributeValues.NeedsOnline"
      :attribute="e.attributes.NeedsOnline"
      :is-edit-mode="false"
      :showEmptyValue="true"
      id="boolNeedsOnline"
    ></rck-field>
  </div>
</div>
<rck-form ref="form" @validationChanged="validationChange" v-if="e.attributeValues.NeedsOnline == 'True' || !isMultiOccurrence">
  <div class="row">
    <div class="col col-xs-12">
      <tcc-validator :name="e.attributes.EventURL.key" :rules="[rules.required(e.attributeValues.EventURL, e.attributes.EventURL.name)]" ref="validators_url">
        <rck-field
          v-model="e.attributeValues.EventURL"
          :attribute="e.attributes.EventURL"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="txtEventURL"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.Password"
        :attribute="e.attributes.Password"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
        id="txtPassword"
      ></rck-field>
    </div>
  </div>
</rck-form>
`
});
