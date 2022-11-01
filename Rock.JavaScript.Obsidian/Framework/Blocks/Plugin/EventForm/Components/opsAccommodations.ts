import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField";
import Validator from "./validator";
import Toggle from "./toggle";


export default defineComponent({
    name: "EventForm.Components.Ops",
    components: {
      "rck-field": RockField,
      "tcc-validator": Validator,
      "tcc-switch": Toggle
    },
    props: {
      e: {
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
  <h4 class="text-accent">Tech Needs</h4>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.RoomTech"
        :attribute="e.attributes.RoomTech"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.TechNeeds"
        :attribute="e.attributes.TechNeeds"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <h4 class="text-accent">Set-Up</h4>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.NeedsDoorsUnlocked"
        :label="e.attributes.NeedsDoorsUnlocked.name"
      ></tcc-switch>
    </div>
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="e.attributeValues.Doors"
        :attribute="e.attributes.Doors"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.Setup"
        :attribute="e.attributes.Setup"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.SetupImage"
        :attribute="e.attributes.SetupImage"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
</div>
`
});
