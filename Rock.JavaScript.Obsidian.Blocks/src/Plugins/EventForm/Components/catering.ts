import { defineComponent, PropType } from "vue"
import { ContentChannelItemBag } from "@Obsidian/ViewModels/Entities/contentChannelItemBag"
import { Menu, Dropdown } from "ant-design-vue"
const { MenuItem } = Menu
import RockField from "@Obsidian/Controls/rockField"
import RockForm from "@Obsidian/Controls/rockForm"
import Toggle from "./toggle"
import TimePicker from "./timePicker"
import Validator from "./validator"
import { DateTime } from "luxon"
import rules from "../Rules/rules"


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
        type: Object as PropType<ContentChannelItemBag>,
        required: false
      },
      request: {
        type: Object as PropType<ContentChannelItemBag>,
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
      'e.attributeValues.DrinkSetupLocation'(val) {
        if(this.e?.attributeValues) {
          if(this.e.attributeValues.FoodSetupLocation != val) { 
            this.e.attributeValues.SetupFoodandDrinkTogether = "False"
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
      if(this.readonly) {
        document.querySelectorAll('.catering-form input').forEach((el: any) => {
          el.setAttribute("readonly", "")
        })
        document.querySelectorAll('.catering-form textarea').forEach((el: any) => {
          el.setAttribute("readonly", "")
        })
      }
    },
    template: `
<rck-form ref="form" @validationChanged="validationChange" class="catering-form">
  <a-dropdown :trigger="['click']" v-model:visible="vendorMenu" v-if="!readonly">
    <div class="hover font-weight-bold" id="ddlPreferredVendor">For a list of our preferred vendors - <span class="text-accent">please click here.</span></div>
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
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Hy-Vee Catering'; vendorMenu = false;">
          Hy-Vee Catering
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Italian Village'; vendorMenu = false;">
          Italian Village
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Jimmy John’s'; vendorMenu = false;">
          Jimmy John’s
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Mrs. Tammie'; vendorMenu = false;">
          Mrs. Tammie
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Panera'; vendorMenu = false;">
          Panera
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Pancheros'; vendorMenu = false;">
          Pancheros
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
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Shakespeare’s'; vendorMenu = false;">
          Shakespeare’s
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Sophia’s'; vendorMenu = false;">
          Sophia’s
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Tropical Smoothie Cafe'; vendorMenu = false;">
          Tropical Smoothie Cafe
        </a-menu-item>
        <a-menu-item class="hover" @click="e.attributeValues.PreferredVendor = 'Your Pie'; vendorMenu = false;">
          Your Pie
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
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="txtPreferredVendor"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="request.attributeValues.NeedsSpace == 'False'">
      <tcc-validator :rules="[rules.required(e.attributeValues.ExpectedAttendance, e.attributes.ExpectedAttendance.name), rules.attendance(e.attributeValues.ExpectedAttendance, e.attributeValues.Rooms, [], e.attributes.ExpectedAttendance.name)]" ref="validator_att">
        <rck-field
          v-model="e.attributeValues.ExpectedAttendance"
          :attribute="e.attributes.ExpectedAttendance"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          :id="txtAttendance"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.FoodBudgetMinistry, e.attributes.FoodBudgetMinistry.name)]" ref="validator_budgetmin">
        <rck-field
          v-model="e.attributeValues.FoodBudgetMinistry"
          :attribute="e.attributes.FoodBudgetMinistry"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="ddlFoodBudgetMinistry"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.FoodBudgetLine, e.attributes.FoodBudgetLine.name)]" ref="validator_budget">
        <rck-field
          v-model="e.attributeValues.FoodBudgetLine"
          :attribute="e.attributes.FoodBudgetLine"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="ddlFoodBudgetLine"
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
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="txtMenu"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.NeedsDelivery"
        :label="e.attributes.NeedsDelivery.name"
        v-if="!readonly"
        id="boolNeedsDelivery"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="e.attributeValues.NeedsDelivery"
        :attribute="e.attributes.NeedsDelivery"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="boolNeedsDelivery"
      ></rck-field>
    </div>
  </div>
  <div class="row" v-if="e.attributeValues.NeedsDelivery == 'True'">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.FoodTime, e.attributes.FoodTime.name)]" ref="validator_foodtime" v-if="!readonly">
        <tcc-time 
          :label="e.attributes.FoodTime.name"
          v-model="e.attributeValues.FoodTime"
          id="TimeFoodTime"
        ></tcc-time>
      </tcc-validator>
      <rck-field
        v-else
        v-model="e.attributeValues.FoodTime"
        :attribute="e.attributes.FoodTime"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="timeFoodTime"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.FoodSetupLocation, e.attributes.FoodSetupLocation.name)]" ref="validator_foodloc">
        <rck-field
          v-model="e.attributeValues.FoodSetupLocation"
          :attribute="e.attributes.FoodSetupLocation"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="txtFoodSetupLocation"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row" v-else>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.FoodTime, e.attributes.FoodTime.name)]" ref="validator_foodtime" v-if="!readonly">
        <tcc-time 
          :label="e.attributes.FoodTime.name"
          v-model="e.attributeValues.FoodTime"
          id="TimeFoodTime"
        ></tcc-time>
      </tcc-validator>
      <rck-field
        v-else
        v-model="e.attributeValues.FoodTime"
        :attribute="e.attributes.FoodTime"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="timeFoodTime"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="e.attributeValues.Drinks"
        :attribute="e.attributes.Drinks"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
        id="ddlDrinks"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.drinkTimeRequired(e.attributeValues.DrinkTime, e.attributeValues.Drinks, e.attributes.DrinkTime.name)]" ref="validator_drinktime" v-if="!readonly">
        <tcc-time 
          :label="e.attributes.DrinkTime.name"
          v-model="e.attributeValues.DrinkTime"
          id="TimeDrinkTime"
        ></tcc-time>
      </tcc-validator>
      <rck-field
        v-else
        v-model="e.attributeValues.DrinkTime"
        :attribute="e.attributes.DrinkTime"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="timeDrinkTime"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.NeedsDelivery == 'True'">
      <rck-field
        v-model="e.attributeValues.SetupFoodandDrinkTogether"
        :attribute="e.attributes.SetupFoodandDrinkTogether"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
        id="boolSetupFoodandDrinkTogether"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="e.attributeValues.DrinkSetupLocation"
        :attribute="e.attributes.DrinkSetupLocation"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
        id="txtDrinkSetupLocation"
      ></rck-field>
    </div>
  </div>
</rck-form>
`
});
