import { defineComponent, provide } from "vue"
import { useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block"
import { useStore } from "@Obsidian/PageState"
import { EmailCampaignImpactViewModel } from "./emailCampaignImpactViewModel"
import  Panel from "@Obsidian/Controls/panel"
import RockButton from "@Obsidian/Controls/rockButton"
import RockLabel from "@Obsidian/Controls/rockLabel"
import RockDDL from "@Obsidian/Controls/dropDownList"
import RockField from "@Obsidian/Controls/rockField"
import RockForm from "@Obsidian/Controls/rockForm"
import groupPicker from "@Obsidian/Controls/groupPicker"
import treeItemPicker from "@Obsidian/Controls/treeItemPicker.obs"
import definedValuePicker from "@Obsidian/Controls/definedValuePicker.obs"
import datePicker from "@Obsidian/Controls/datePicker.obs"

const store = useStore()

export default defineComponent({
  name: "Reporting.EmailCampaignImpact",
  components: {
    "rck-pnl": Panel,
    "rck-btn": RockButton,
    "rck-ddl": RockDDL,
    "rck-lbl": RockLabel,
    "rck-field": RockField,
    "rck-form": RockForm,
    "rck-grp-pkr": groupPicker,
    "rck-tree-pkr": treeItemPicker,
    "rck-dv-pkr": definedValuePicker,
    "rck-dt-pkr": datePicker
  },
  setup() {
    const invokeBlockAction = useInvokeBlockAction()
    const viewModel = useConfigurationValues<EmailCampaignImpactViewModel | null>()

    /** A method to generate the impact report */
    const generateReport: (commIds: number[], groupGuid: string, accounts: string[], transactionTypes: string[], endDate: string) => Promise<any> = async (commIds, groupGuid, accounts, transactionTypes, endDate) => {
      const response = await invokeBlockAction<{ expirationDateTime: string }>("GenerateReport", {
        commIds: commIds, groupGuid: groupGuid, accountGuids: accounts, transactionTypeGuids: transactionTypes, transactionEndDate: endDate
      })
      if (response) {
        return response
      }
    }
    provide("generateReport", generateReport)

    return {
      viewModel,
      generateReport
    }
  },
  data() {
    return {
      comms: [809785, 799965],
      group: { text: "", value: "" },
      accounts: [
        {value: '465440a7-7843-4315-bfe7-14d9e94553ae', text: 'Mercy Ministry'},
        {value: 'f5838861-f0e6-47c8-89a5-86da6b57e25e', text: 'General Operating Fund'},
      ] as any[],
      transactionTypes: [] as any[],
      endDate: "2024-12-31"
    }
  },
  computed: {

  },
  methods: {
    filter() {
      this.generateReport(this.comms, this.group.value, this.accounts.map(a => a.value), this.transactionTypes.map(dv => dv.value), this.endDate)
    }
  },
  mounted() {
    console.log(this.viewModel)
  },
  template: `
    <rck-pnl
      title="Configuration"
    >
      <div class="row">
        <div class="col-xs-12 col-md-6">
          <rck-ddl 
            label="Communications"
            help="The communications to compile data for"
            :items="viewModel.communications"
            :enhanceForLongLists="true"
            :multiple="true"
            v-model="comms"
          />
        </div>
        <div class="col-xs-12 col-md-3">
          <rck-grp-pkr
            label="Group"
            help="Limit results to people in this group"
            v-model="group"
          ></rck-grp-pkr>
        </div>
      </div>
      <div class="row">
        <div class="col-xs-12 col-md-3">
          <rck-tree-pkr
            label="Financial Accounts"
            iconCssClass="fa fa-building-o fa-fw"
            :multiple="true"
            :items="viewModel.accounts"
            v-model="accounts"
          ></rck-tree-pkr>
        </div>
        <div class="col-xs-12 col-md-3">
          <rck-dv-pkr
            label="Financial Transaction Type"
            hint="Limit financial transactions only to those of a certain type"
            :multiple="true"
            definedTypeGuid="FFF62A4B-5D88-4DEB-AF8F-8E6178E41FE5"
            v-model="transactionTypes"
          ></rck-dv-pkr>
        </div>
        <div class="col-xs-12 col-md-3">
          <rck-dt-pkr
            label="End Date for Transactions"
            hint="Transactinons from the first communication send date until this date will be included"
            v-model="endDate"
          ></rck-dt-pkr>
        </div>
      </div>
      <template #footerActions>
        <rck-btn btnType="primary" @click="filter">Generate</rck-btn>
      </template>
    </rck-pnl>

    <div>
      griiiiid
    </div>
  `
})