import { defineComponent, PropType } from "vue"
import { ContentChannelItem } from "../../../../ViewModels"
import { Menu, Dropdown } from "ant-design-vue"
const { MenuItem } = Menu
import RockField from "../../../../Controls/rockField"
import RockForm from "../../../../Controls/rockForm"
import Toggle from "./toggle"
import TimePicker from "./timePicker"
import Validator from "./validator"
import { DateTime } from "luxon"


export default defineComponent({
    name: "EventForm.Components.Catering",
    components: {
      "rck-field": RockField,
      "rck-form": RockForm,
      "tcc-validator": Validator,
      "a-dropdown": Dropdown,
      "a-menu": Menu,
      "a-menu-item": MenuItem,
      "tcc-switch": Toggle,
      "tcc-time": TimePicker,
    },
    props: {
      e: {
          type: Object as PropType<ContentChannelItem>,
          required: false
      },
      showValidation: Boolean,
      refName: String
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
            drinkTimeRequired: (value: string, drinkStr: string, key: string) => {
              let drinks = JSON.parse(drinkStr)
              if(drinks && drinks.value) {
                let selected = drinks.value.split(',')
                if(selected.length > 0) {
                  //Required
                  return !!value || `${key} is required`
                }
              }
              return true
            }
          },
          errors: [] as Record<string, string>[],
          vendorMenu: false
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
      'e.attributeValues.FoodSetupLocation'(val) {
        if(this.e?.attributeValues) {
          if(this.e.attributeValues.SetupFoodandDrinkTogether == "True") {
            this.e.attributeValues.DrinkSetupLocation = val
          }
        }
      },
      'e.attributeValues.SetupFoodandDrinkTogether'(val) {
        if(val == 'True' && this.e?.attributeValues) {
          this.e.attributeValues.DrinkSetupLocation = this.e.attributeValues.FoodSetupLocation
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
        let dt = DateTime.fromFormat(this.e?.attributeValues?.StartTime, "HH:mm:ss")
        let defaultTime = dt.minus({minutes: 30})
        if(this.e.attributeValues.DrinkTime == '') {
          this.e.attributeValues.DrinkTime = defaultTime.toFormat("HH:mm:ss")
        }
        if(this.e.attributeValues.FoodTime == '') {
          this.e.attributeValues.FoodTime = defaultTime.toFormat("HH:mm:ss")
        }
      }
    },
    template: `
<rck-form ref="form" @validationChanged="validationChange">
  <a-dropdown :trigger="['click']" v-model:visible="vendorMenu">
    <div class="hover font-weight-bold">For a list of our preferred vendors - <span class="text-accent">please click here.</span></div>
    <template #overlay>
      <a-menu class="tcc-dropdown">
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Arris’'; vendorMenu = false;">
          Arris’
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'B&B'; vendorMenu = false;">
          B&B
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Chick-fil-A'; vendorMenu = false;">
          Chick-fil-A
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Como Smoke and Fire'; vendorMenu = false;">
          Como Smoke and Fire
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'D-Rowe’s'; vendorMenu = false;">
          D-Rowe’s
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Freddy’s'; vendorMenu = false;">
          Freddy’s
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Honey Baked Ham'; vendorMenu = false;">
          Honey Baked Ham
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Hoss’'; vendorMenu = false;">
          Hoss’
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Hy-Vee Catering'; vendorMenu = false;">
          Hy-Vee Catering
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Italian Village'; vendorMenu = false;">
          Italian Village
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Jimmy John’s'; vendorMenu = false;">
          Jimmy John’s
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Lee’s'; vendorMenu = false;">
          Lee’s
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Mrs. Tammie'; vendorMenu = false;">
          Mrs. Tammie
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Panera'; vendorMenu = false;">
          Panera
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Papa John’s'; vendorMenu = false;">
          Papa John’s
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Picklemans'; vendorMenu = false;">
          Pickleman's
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Pizza Tree'; vendorMenu = false;">
          Pizza Tree
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Popeye’s'; vendorMenu = false;">
          Popeye’s
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Shakespeare’s'; vendorMenu = false;">
          Shakespeare’s
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Sophia’s'; vendorMenu = false;">
          Sophia’s
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Tropical Smoothie Cafe'; vendorMenu = false;">
          Tropical Smoothie Cafe
        </a-menu-item>
      </a-menu>
    </template>
  </a-dropdown>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.PreferredVendor, e.attributes.PreferredVendor.name)]" ref="validator_vendor">
        <rck-field
          v-model="e.attributeValues.PreferredVendor"
          :attribute="e.attributes.PreferredVendor"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.FoodBudgetLine, e.attributes.FoodBudgetLine.name)]" ref="validator_budget">
        <rck-field
          v-model="e.attributeValues.FoodBudgetLine"
          :attribute="e.attributes.FoodBudgetLine"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row mb-2">
    <div class="col col-xs-12">
      <tcc-validator :rules="[rules.required(e.attributeValues.PreferredMenu, e.attributes.PreferredMenu.name)]" ref="validator_menu">
        <rck-field
          v-model="e.attributeValues.PreferredMenu"
          :attribute="e.attributes.PreferredMenu"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <tcc-switch
        v-model="e.attributeValues.NeedsDelivery"
        :label="e.attributes.NeedsDelivery.name"
      ></tcc-switch>
    </div>
  </div>
  <div class="row" v-if="e.attributeValues.NeedsDelivery == 'True'">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.FoodTime, e.attributes.FoodTime.name)]" ref="validator_foodtime">
        <tcc-time 
          :label="e.attributes.FoodTime.name"
          v-model="e.attributeValues.FoodTime"
        ></tcc-time>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.FoodSetupLocation, e.attributes.FoodSetupLocation.name)]" ref="validator_foodloc">
        <rck-field
          v-model="e.attributeValues.FoodSetupLocation"
          :attribute="e.attributes.FoodSetupLocation"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row" v-else>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.FoodTime, e.attributes.FoodTime.name)]" ref="validator_foodtime">
        <tcc-time 
          :label="e.attributes.FoodTime.name"
          v-model="e.attributeValues.FoodTime"
        ></tcc-time>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="e.attributeValues.Drinks"
        :attribute="e.attributes.Drinks"
        :is-edit-mode="true"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.drinkTimeRequired(e.attributeValues.DrinkTime, e.attributeValues.Drinks, e.attributes.DrinkTime.name)]" ref="validator_drinktime">
        <tcc-time 
          :label="e.attributes.DrinkTime.name"
          v-model="e.attributeValues.DrinkTime"
        ></tcc-time>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.NeedsDelivery == 'True'">
      <rck-field
        v-model="e.attributeValues.SetupFoodandDrinkTogether"
        :attribute="e.attributes.SetupFoodandDrinkTogether"
        :is-edit-mode="true"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="e.attributeValues.DrinkSetupLocation"
        :attribute="e.attributes.DrinkSetupLocation"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
</rck-form>
`
});
