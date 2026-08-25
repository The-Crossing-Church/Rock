import { defineComponent, PropType } from "vue"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockForm from "@Obsidian/Controls/rockForm.obs"
import Validator from "./validator"
import Toggle from "./toggle"
import rules from "../Rules/rules"


export default defineComponent({
  name: "EventForm.Components.ProductionTech",
  components: {
    "rck-field": RockField,
    "rck-form": RockForm,
    "tcc-validator": Validator,
    "tcc-switch": Toggle,
  },
  props: {
    request: {
      type: Object as PropType<ContentChannelItemBag>,
      required: false
    },
    events: Array as PropType<ContentChannelItemBag[]>,
    isFuneralRequest: Boolean,
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
    }
  },
  computed: {
    isGymRequested() {
      if(this.events && this.events.length > 0) {
        let rooms = this.events.map((e: any) => {
          if(e.attributeValues?.Rooms) {
            let selectedRooms = JSON.parse(e.attributeValues.Rooms)
            if(selectedRooms) {
              return selectedRooms.text.split(',')
            }
          }
        })
        if(rooms) {
          rooms = rooms.flat().filter((r: string) => { return r.includes('Gym') })
          if(rooms && rooms.length > 0) {
            return true
          }
        }
      }
      return false
    },
    isAuditoriumRequested() {
      if(this.events && this.events.length > 0) {
        let rooms = this.events.map((e: any) => {
          if(e.attributeValues?.Rooms) {
            let selectedRooms = JSON.parse(e.attributeValues.Rooms)
            if(selectedRooms) {
              return selectedRooms.text.split(',')
            }
          }
        })
        if(rooms) {
          rooms = rooms.flat().filter((r: string) => { return r.includes('Auditorium') })
          if(rooms && rooms.length > 0) {
            return true
          }
        }
      }
      return false
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
  <div class="text-accent mb-4">
    <i>
      Starting January 1st, 2027 limited production support will be provided for most events. 
    </i> <br/>
    Any ministry may request the use of production inventory for their event, but production staff will not be present at the event and only provide remote support as needed. <br/>
    Ministries wishing to have an event with full production support must submit their proposal to execs for vote.
  </div>

  <div class="row">
    <div class="col col-xs-12">
      <tcc-switch
        v-model="request.attributeValues.IsExecApproved"
        :label="request.attributes.IsExecApproved.name"
        v-if="!readonly"
        id="boolIsExecApproved"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="request.attributeValues.IsExecApproved"
        :attribute="request.attributes.IsExecApproved"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="boolIsExecApproved"
      ></rck-field>
    </div>
  </div>

  <template v-if="request.attributeValues.IsExecApproved == 'True' || isFuneralRequest">
    <div class="row">
      <div class="col col-xs-12 col-md-6">
        <tcc-switch
          v-model="request.attributeValues.HasMedia"
          :label="request.attributes.HasMedia.name"
          v-if="!readonly"
          id="boolHasMedia"
        ></tcc-switch>
        <rck-field
          v-else
          v-model="request.attributeValues.HasMedia"
          :attribute="request.attributes.HasMedia"
          :is-edit-mode="false"
          :showEmptyValue="true"
          id="boolHasMedia"
        ></rck-field>
      </div>
    </div>
    <div class="row">
      <div class="col col-xs-12">
        <tcc-validator :name="request.attributes.ProductionSetup.key" :rules="[rules.required(request.attributeValues.ProductionSetup, request.attributes.ProductionSetup.name)]" ref="validators_prodSetup">
          <rck-field
            v-model="request.attributeValues.ProductionSetup"
            :attribute="request.attributes.ProductionSetup"
            :is-edit-mode="!readonly"
            :showEmptyValue="true"
            id="txtProductionSetup"
          ></rck-field>
        </tcc-validator>
      </div>
      <div class="col col-xs-12">
        <tcc-validator :name="request.attributes.ProductionLightingNeeds.key" :rules="[rules.required(request.attributeValues.ProductionLightingNeeds, request.attributes.ProductionLightingNeeds.name)]" ref="validators_prodLights">
          <rck-field
            v-model="request.attributeValues.ProductionLightingNeeds"
            :attribute="request.attributes.ProductionLightingNeeds"
            :is-edit-mode="!readonly"
            :showEmptyValue="true"
            id="txtProductionLightingNeeds"
          ></rck-field>
        </tcc-validator>
      </div>
    </div>
  </template>
  
  <div class="row">
    <div v-if="isGymRequested" class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="request.attributeValues.GymLightPresets"
        :label="request.attributes.GymLightPresets.name"
        v-if="!readonly"
        id="boolGymLightPresets"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="request.attributeValues.GymLightPresets"
        :attribute="request.attributes.GymLightPresets"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="boolGymLightPresets"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="request.attributeValues.ProductionInventory"
        :attribute="request.attributes.ProductionInventory"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
        id="ddlProductionInventory"
      ></rck-field>
    </div>
  </div>
  <br/>
</rck-form>
`
});
