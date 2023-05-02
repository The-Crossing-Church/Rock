import { defineComponent, PropType } from "vue"
import { ContentChannelItem } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField"
import RockForm from "../../../../Controls/rockForm"
import Validator from "./validator"


export default defineComponent({
    name: "EventForm.Components.ProductionTech",
    components: {
      "rck-field": RockField,
      "rck-form": RockForm,
      "tcc-validator": Validator,
    },
    props: {
      request: {
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
          rules: {
            required: (value: any, key: string) => {
              if(typeof value === 'string') {
                if(value.includes("{")) {
                  let obj = JSON.parse(value)
                  return obj.value != '' || `${key} is required`
                } 
              } 
              return !!value || `${key} is required`
            },
          },
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
      <tcc-validator :rules="[rules.required(request.attributeValues.ProductionTech, request.attributes.ProductionTech.name)]" ref="validators_prodTech">
        <rck-field
          v-model="request.attributeValues.ProductionTech"
          :attribute="request.attributes.ProductionTech"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <tcc-validator :rules="[rules.required(request.attributeValues.ProductionSetup, request.attributes.ProductionSetup.name)]" ref="validators_prodSetup">
        <rck-field
          v-model="request.attributeValues.ProductionSetup"
          :attribute="request.attributes.ProductionSetup"
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
