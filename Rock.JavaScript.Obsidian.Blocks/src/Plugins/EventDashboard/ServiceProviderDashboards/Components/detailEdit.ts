import { defineComponent, PropType } from "vue"
import { DateTime } from "luxon"
import helper from "../../Helpers/helper"
import { AttributeBag } from "../../../ViewModels/attributeBag"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import RockField from "@Obsidian/Controls/rockField.obs"
import Panel from "@Obsidian/Controls/panel.obs"
import AdditionalRegQuestions from "../../../EventForm/Components/additionalRegQuestions"
import DiscountCodes from "../../../EventForm/Components/discountCodes"
import RoomSetUp from "../../../EventForm/Components/roomSetUp"
import OpsInv from "../../../EventForm/Components/opsInventory"

export default defineComponent({
  name: "EventDashboard.Components.SSP.EditModal.Details",
  components: {
    "rck-lbl": RockLabel,
    "rck-field": RockField,
    "rck-panel": Panel,
    "tcc-reg-questions": AdditionalRegQuestions,
    "tcc-discount": DiscountCodes,
    "tcc-setup": RoomSetUp,
    "tcc-ops-inv": OpsInv,
  },
  props: {
    request: Object,
    detail: Object,
    editCategories: Array,
    regAttrs: Array as PropType<AttributeBag[]>,
    discountAttrs: Array as PropType<AttributeBag[]>,
  },
  setup() {

  },
  data() {
    return {
      helper: helper,
      panel: false
    };
  },
  created() {
    
  },
  computed: {
    detailAttrs() {
      let sections = [] as any
      let editCategorySet = new Set(this.editCategories)
      if(this.detail?.attributeValues) {
        Object.keys(this.detail.attributeValues).forEach(key => {
          let attrCategories = new Set(this.detail?.attributes[key]?.categories.map(c => c.name))
          let intersection = attrCategories.intersection(editCategorySet)
          if(intersection.size > 0) {
            let section = sections.filter(s => s.cat == [...intersection][0])[0]
            if(!section) {
              sections.push({ cat: [...intersection][0], attrs: [] })
              section = sections[sections.length - 1]
            }
            section.attrs.push({ key: key, attr: this.detail?.attributes[key], value: this.detail?.attributeValues[key] })
          }
        })
      }
      sections.forEach(section => {
        section.attrs = section.attrs.sort((a, b) => {
          return a.attr.order - b.attr.order
        })
      })
      return sections
    }
  },
  methods: {
    getSectionName(category: string) {
      let name = ""
      if(this.helper?.sectionInfo) {
        name = this.helper.sectionInfo.filter(si => si.cat == category)[0].section
      }
      return name ?? category
    },
    getPanelName(date: string, room: string) {
      let title = "" 
      if(date) {
        let dt = DateTime.fromFormat(date, 'yyyy-MM-dd')
        if(!dt.isValid) {
          dt = DateTime.fromISO(date)
        }
        title = dt.toFormat('MM/dd/yyyy')
      } else {
        let dates = this.request?.attributeValues.EventDates.split(",")
        dates = dates.map((d: any) => {
          return DateTime.fromFormat(d.trim(), 'yyyy-MM-dd').toFormat('MM/dd/yyyy')
        })
        if(dates.length > 3) {
          title = dates[0] + " - " + dates[dates.length - 1]
        } else {
          title = dates.join(", ")
        }
      }
      let rooms = JSON.parse(room)
      let roomList = rooms.text.split(", ")
      if(rooms && rooms.text != '' && roomList && roomList.length > 0) {
        title += " ("
        if(roomList.length > 5) {
          title += roomList.slice(0,5).join(", ")
          title += "..."
        } else {
          title += rooms.text
        }
        title +=")"
      }
      return title
    },
  },
  watch: {
    
  },
  mounted() {
    
  },
  template: `
<rck-panel
  v-model="panel"
  hasCollapse
  :title="getPanelName(detail.attributeValues.EventDate, detail.attributeValues.Rooms)"
>
  <div v-for="(section, idx) in detailAttrs" :key="idx">
    <h4 class="text-accent">{{getSectionName(section.cat)}}</h4>
    <div class="row row-eq-height">
      <div v-for="(av, aIdx) in section.attrs" :key="aIdx" class="col col-xs-12 col-md-6">
        <template v-if="av.attr.key == 'DiscountCodes'">
          <tcc-discount 
            :e="detail" 
            :attrs="discountAttrs"
          ></tcc-discount>
        </template>
        <template v-else-if="av.attr.key == 'AdditionalRegistrationQuestions'">
          <tcc-reg-questions
            :e="detail" 
            :attrs="regAttrs" 
          ></tcc-reg-questions>
        </template>
        <template v-else-if="av.attr.key == 'RoomSetUp'">
          <tcc-setup></tcc-setup>
        </template>
        <template v-else-if="av.attr.key == 'OpsInventory'">
        </template>
        <rck-field
          v-else
          :attribute="av.attr"
          v-model="detail.attributeValues[av.attr.key]"
          :isEditMode="true"
        ></rck-field>
      </div>
    </div>
  </div>
</rck-panel>
`
});
