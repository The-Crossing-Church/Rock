import { defineComponent, PropType } from "vue"
import { ContentChannelItem } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField"
import RockForm from "../../../../Controls/rockForm"
import Validator from "./validator"
import rules from "../Rules/rules"


export default defineComponent({
    name: "EventForm.Components.Online",
    components: {
      "rck-field": RockField,
      "rck-form": RockForm,
      "tcc-validator": Validator,
    },
    props: {
      e: {
          type: Object as PropType<ContentChannelItem>,
          required: false
      },
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
    <div class="col col-xs-12">
      <tcc-validator :rules="[rules.required(e.attributeValues.EventURL, e.attributes.EventURL.name)]" ref="validators_url">
        <rck-field
          v-model="e.attributeValues.EventURL"
          :attribute="e.attributes.EventURL"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
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
      ></rck-field>
    </div>
  </div>
</rck-form>
`
});
