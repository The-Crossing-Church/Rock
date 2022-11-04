import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField";
import Validator from "./validator";


export default defineComponent({
    name: "EventForm.Components.ProductionTech",
    components: {
      "rck-field": RockField,
      "tcc-validator": Validator,
    },
    props: {
      request: {
          type: Object as PropType<ContentChannelItem>,
          required: false
      },
      showValidation: Boolean
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
          }
        };
    },
    computed: {
      errors() {
        let formRef = this.$refs as any
        let errs = [] as string[]
        for(let r in formRef) {
          if(formRef[r].className?.includes("validator")) {
            errs.push(...formRef[r].errors)
          }
        }
        return errs
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
      }
    },
    watch: {
      
    },
    mounted() {
      if(this.showValidation) {
        this.validate()
      }
    },
    template: `
<div>
  <div class="row">
    <div class="col col-xs-12">
      <tcc-validator :rules="[rules.required(request.attributeValues.ProductionTech, request.attributes.ProductionTech.name)]" ref="validators_prodTech">
        <rck-field
          v-model="request.attributeValues.ProductionTech"
          :attribute="request.attributes.ProductionTech"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <br/>
</div>
`
});
