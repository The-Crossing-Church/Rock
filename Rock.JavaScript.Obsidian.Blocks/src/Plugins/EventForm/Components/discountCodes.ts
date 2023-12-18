import { defineComponent, PropType } from "vue"
import { ContentChannelItemBag } from "@Obsidian/ViewModels/Entities/contentChannelItemBag"
import { AttributeBag } from "@Obsidian/ViewModels/Entities/attributeBag"
import { DateTime, Duration, Interval } from "luxon"
import RockField from "@Obsidian/Controls/rockField"
import RockForm from "@Obsidian/Controls/rockForm"
import RockLabel from "@Obsidian/Controls/rockLabel"
import { Button, Modal, Select } from "ant-design-vue"
import Toggle from "./toggle"
import DiscountCodePicker from "./discountCodePicker"

type DiscountCode = {
  CodeType: string,
  Amount: string,
  Code: string,
  AutoApply: boolean,
  EffectiveDateRange: string,
  MaxUses: number
}

export default defineComponent({
  name: "EventForm.Components.DiscountCodes",
  components: {
    "rck-field": RockField,
    "rck-form": RockForm,
    "rck-lbl": RockLabel,
    "a-btn": Button,
    "a-modal": Modal,
    "a-select": Select,
    "tcc-switch": Toggle,
    "tcc-code": DiscountCodePicker
  },
  props: {
    e: {
      type: Object as PropType<ContentChannelItemBag>,
      required: false
    },
    attrs: Array as PropType<AttributeBag[]>
  },
  setup() {

  },
  data() {
      return {
        discountCodes: [] as DiscountCode[],
        newCode: {} as any,
        modal: false,
        message: "",
        selectedIdx: -1
      };
  },
  computed: {
    codeAttr() {
      if(this.attrs) {
        let attr = this.attrs.filter((a: any) => { return a.key == "Code" })
        if(attr && attr.length > 0) {
          return attr[0]
        }
      }
      return null
    },
    codeTypeAttr() {
      if(this.attrs) {
        let attr = this.attrs.filter((a: any) => { return a.key == "CodeType" })
        if(attr && attr.length > 0) {
          return attr[0]
        }
      }
      return null
    },
    codeTypeOptions() {
      if(this.codeTypeAttr?.qualifierValues?.values) {
        let vals = this.codeTypeAttr.qualifierValues.values as any
        return vals.value.split(",")
      }
    },
    amountAttr() {
      if(this.attrs) {
        let attr = this.attrs.filter((a: any) => { return a.key == "Amount" })
        if(attr && attr.length > 0) {
          return attr[0]
        }
      }
      return null
    },
    autoApplyAttr() {
      if(this.attrs) {
        let attr = this.attrs.filter((a: any) => { return a.key == "AutoApply" })
        if(attr && attr.length > 0) {
          return attr[0]
        }
      }
      return null
    },
    effectiveDateRangeAttr() {
      if(this.attrs) {
        let attr = this.attrs.filter((a: any) => { return a.key == "EffectiveDateRange" })
        if(attr && attr.length > 0) {
          return attr[0]
        }
      }
      return null
    },
    maxUsesAttr() {
      if(this.attrs) {
        let attr = this.attrs.filter((a: any) => { return a.key == "MaxUses" })
        if(attr && attr.length > 0) {
          return attr[0]
        }
      }
      return null
    },
  },
  methods: {
    updateCodeType(val: string) {
      this.newCode.CodeType = val
    },
    updateAmount(val: string) {
      this.newCode.Amount = val
    },
    edit(idx: number) {
      this.newCode = JSON.parse(JSON.stringify(this.discountCodes[idx]))
      this.selectedIdx = idx
      this.modal = true
    },
    save() {
      let amt = parseInt(this.newCode.Amount)
      if(this.newCode.CodeType == "%") {
        if(amt > 100) {
          this.newCode.Amount = "100"
        }
      }
      if(this.newCode.Amount && this.newCode.Code) {
        if(this.selectedIdx >= 0) {
          this.discountCodes[this.selectedIdx] = this.newCode
        } else {
          this.discountCodes.push(this.newCode as DiscountCode)
        }
        this.modal = false
        this.message = ""
      } else {
        let msg = "Discount codes require an amount and code. "
        if(this.newCode.Amount == '' && this.newCode.Code == '') {
          msg += "Please enter an amount and code."
        } else if (this.newCode.Amount == '') {
          msg += "Please enter an amount."
        } else {
          msg += "Please enter a code."
        }
        this.message = msg
      }
    },
    removeCode() {
      if(this.selectedIdx >= 0) {
        this.discountCodes.splice(this.selectedIdx, 1)
      }
      this.modal = false
    },
    formatDateRange(range: string) {
      let dates = range.split(",")
      return DateTime.fromFormat(dates[0], "yyyy-MM-dd").toFormat("MM/dd/yy") + " - " + DateTime.fromFormat(dates[1], "yyyy-MM-dd").toFormat("MM/dd/yy")
    }
  },
  watch: {
    discountCodes: {
      handler(val) {
        if(this.e?.attributeValues) {
          this.e.attributeValues.DiscountCodes = JSON.stringify(val)
        }
      },
      deep: true
    },
    modal(val) {
      if(!val) {
        this.selectedIdx = -1
      }
    }
  },
  mounted() {
    if(this.e?.attributeValues) {
      if(this.e?.attributeValues.DiscountCodes) {
        this.discountCodes = JSON.parse(this.e.attributeValues.DiscountCodes)
      }
    }
  },
  template: `
<rck-lbl class="mt-2">Discount Codes</rck-lbl>
<div class="setup-table mb-2">
  <div class="row">
    <div class="col col-xs-11">
      <template v-if="discountCodes.length > 0">
        <div class="row">
          <div class="col col-xs-6 col-md-3 col-lg-2" v-for="(dc, idx) in discountCodes" :key="idx">
            <div class="p-2">
              <h6 class="text-uppercase">{{dc.Code}}</h6>
              <hr class="my-2"/>
              <template v-if="dc.CodeType == '%'">
                {{dc.Amount}}%
              </template>
              <template v-else>
                {{dc.CodeType}}{{dc.Amount}}
              </template>
              <template v-if="dc.MaxUses != ''">
                <span class="pl-2">{{dc.MaxUses}} Uses</span>
              </template>
              <br/>
              <template v-if="dc.EffectiveDateRange != ''">
                {{formatDateRange(dc.EffectiveDateRange)}} <br/>
              </template>
              <template v-if="dc.AutoApply == 'True'">
                <i class="fas fa-check-square"></i> Auto-Apply 
              </template>
              <a-btn @click="edit(idx)" shape="circle" type="accent" class="pull-right">
                <i class="fa fa-pencil-alt"></i>
              </a-btn>
            </div>
          </div>
        </div>
      </template>
      <template v-else>
        Click the add button to create a discount code for your event.
      </template>
    </div>
    <div class="col col-xs-1">
      <a-btn class="pull-right" type="accent" shape="circle" @click="newCode = { AutoApply: 'False', CodeType: '%', Amount: '', MaxUses: '', EffectiveDateRange: '', Code: '' }; modal = true;">
        <i class="fa fa-plus"></i>
      </a-btn>
    </div>
  </div>
</div>
<a-modal v-model:visible="modal" style="min-width: 50%;">
  <div class="alert alert-danger mt-2" v-if="message != ''">
    {{message}}
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-code
        :label="amountAttr.name"
        :codeType="newCode.CodeType"
        :amount="newCode.Amount"
        :items="codeTypeOptions"
        v-on:updateCodeType="updateCodeType"
        v-on:updateAmount="updateAmount"
      ></tcc-code>
    </div>
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="newCode.Code"
        :attribute="codeAttr"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="newCode.MaxUses"
        :attribute="maxUsesAttr"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row mt-2">
    <div class="col col-xs-12">
      <rck-field
        v-model="newCode.EffectiveDateRange"
        :attribute="effectiveDateRangeAttr"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row mt-2">
    <div class="col col-xs-12">
      <tcc-switch
        v-model="newCode.AutoApply"
        :label="autoApplyAttr.name"
      ></tcc-switch>
    </div>
  </div>
  <template #footer>
    <a-btn type="red" @click="removeCode" v-if="selectedIdx >= 0">Delete</a-btn>
    <a-btn type="primary" @click="save">Save</a-btn>
  </template>
</a-modal>
<v-style>
.setup-table {
  border-radius: 6px;
  border: 1px solid #dfe0e1;
  padding: 8px;
}
.setup-row {
  display: flex;
  align-items: center;
}
.setup-row:not(:last-child) {
  border-bottom: 1px solid #F0F0F0;
}
.spacer {
  flex-grow: 1!important;
}
</v-style>
`
});
