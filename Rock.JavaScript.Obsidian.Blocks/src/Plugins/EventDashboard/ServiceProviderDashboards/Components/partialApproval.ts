import { defineComponent } from "vue"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"
import RockHtml from "@Obsidian/Controls/htmlEditor.obs"
import PAValues from "../../Components/partialApprovalValues"


export default defineComponent({
  name: "EventDashboard.Components.SSP.PartialApproval",
  components: {
    "rck-field": RockField,
    "rck-lbl": RockLabel,
    "rck-btn": RockButton,
    "rck-html": RockHtml,
    "tcc-pa-val": PAValues,
  },
  props: {
    request: Object,
    editCategories: Array,
    rooms: Array,
    inventory: Array
  },
  setup() {

  },
  data() {
    return {
      approvedAttributes: [] as string[],
      deniedAttributes: [] as string[],
      eventChanges: [] as any[],
      titleApproved: null,
      message: ""
    };
  },
  computed: {
    productionTechChanges() {
      let attrs = [] as any[]
      if(this.request?.attributes && this.request?.attributeValues && this.request?.changes?.attributeValues) {
        for(let key in this.request.attributes) {
          let attr = this.request.attributes[key]
          let item = { attr: attr, value: "", changeValue: null }
          let categories = attr.categories.map((c: any) => c.name)
          if(categories.includes("Event Production")) {
            item.value = this.request.attributeValues[key]
            if(this.request.changes && this.request.changes?.attributeValues[key] != this.request.attributeValues[key]) {
              item.changeValue = this.request.changes.attributeValues[key]
            }
            if(item.changeValue != null && item.value != item.changeValue) {
              attrs.push(item)
            }
          }
        }
      }
      return attrs
    },
    publicityChanges() {
      let attrs = [] as any[]
      if(this.request?.attributes && this.request?.attributeValues && this.request?.changes?.attributeValues) {
        for(let key in this.request.attributes) {
          let attr = this.request.attributes[key]
          let item = { attr: attr, value: "", changeValue: null }
          let categories = attr.categories.map((c: any) => c.name)
          if(categories.includes("Event Publicity")) {
            item.value = this.request.attributeValues[key]
            if(this.request.changes && this.request.changes.attributeValues[key] != this.request.attributeValues[key]) {
              item.changeValue = this.request.changes.attributeValues[key]
            }
            if(item.changeValue != null && item.value != item.changeValue) {
              attrs.push(item)
            }
          }
        }
      }
      return attrs
    },
  },
  methods: {
    eventChangesConfig() {
      let events = [] as any[]
      if(this.request?.childItems) {
        for(let i=0; i<this.request.childItems.length; i++) {
          let item = {"eventIdKey": this.request.childItems[i].idKey, "date": this.request.childItems[i].attributeValues.EventDate, "approvedAttrs": [] as string[], "deniedAttrs": [] as string[], sections: [] as any[]}
          for(let key in this.request.childItems[i].attributes) {
            let attrItem = { attr: this.request.childItems[i].attributes[key], value: "", changeValue: null }
            if(attrItem.attr.categories.length == 0) {
              continue
            }
            let categories = attrItem.attr.categories.map((c: any) => c.name).filter(cat => {
              return this.editCategories?.includes(cat)
            })
            if(categories.length > 0) {
              attrItem.value = this.request.childItems[i].attributeValues[key]
              if(this.request.childItems[i].changes && this.request.childItems[i].changes.attributeValues[key] != this.request.childItems[i].attributeValues[key]) {
                attrItem.changeValue = this.request.childItems[i].changes.attributeValues[key]
              }
              if(attrItem.changeValue != null && attrItem.value != attrItem.changeValue) {
                let existing = item.sections.map((s: any) => { return s.category }).indexOf(categories[0])
                if(existing >= 0) {
                  item.sections[existing].items.push(attrItem)
                } else {
                  item.sections.push({"category": categories[0], items: [attrItem]})
                }
              }
            }
          }
          if(item.sections.length > 0) {
            events.push(item)
          }
        }
      }
      this.eventChanges = events
    },
    approveAttribute(idKey: string, key: string) {
      for(let i = 0; i < this.eventChanges.length; i++) {
        if(this.eventChanges[i].eventIdKey == idKey) {
          let idx = this.eventChanges[i].approvedAttrs.indexOf(key)
          if(idx < 0) {
            this.eventChanges[i].approvedAttrs.push(key)
          }
          idx = this.eventChanges[i].deniedAttrs.indexOf(key)
          if(idx >= 0) {
            this.eventChanges[i].deniedAttrs.splice(idx, 1)
          }
        }
      }
    },
    denyAttribute(idKey: string, key: string) {
      for(let i = 0; i < this.eventChanges.length; i++) {
        if(this.eventChanges[i].eventIdKey == idKey) {
          let idx = this.eventChanges[i].approvedAttrs.indexOf(key)
          if(idx >= 0) {
            this.eventChanges[i].approvedAttrs.splice(idx, 1)
          }
          idx = this.eventChanges[i].deniedAttrs.indexOf(key)
          if(idx < 0) {
            this.eventChanges[i].deniedAttrs.push(key)
          }
        }
      }
    },
    approveRequestAttribute(key: string) {
      let idx = this.approvedAttributes.indexOf(key)
      if(idx < 0) {
        this.approvedAttributes.push(key)
      }
      idx = this.deniedAttributes.indexOf(key)
      if(idx >= 0) {
        this.deniedAttributes.splice(idx, 1)
      }
    },
    denyRequestAttribute(key: string) {
      let idx = this.approvedAttributes.indexOf(key)
      if(idx >= 0) {
        this.approvedAttributes.splice(idx, 1)
      }
      idx = this.deniedAttributes.indexOf(key)
      if(idx < 0) {
        this.deniedAttributes.push(key)
      }
    },
  },
  watch: {
    
  },
  mounted() {
    if(this.request) {
      this.eventChangesConfig()
    }
  },
  template: `
<div>
  <template v-for="e in eventChanges" :key="e.eventIdKey">
    <h4>
      <template v-if="e.date != ''">Changes on {{e.date}}</template>
      <template v-else>Event Changes</template>
    </h4>
    <hr/>
    <template v-for="s in e.sections" :key="s.category">
      <h4 class="text-accent">{{s.category}}</h4>
      <tcc-pa-val 
        v-for="(i, idx) in s.items"
        :key="idx"
        :attribute="i.attr"
        :originalValue="i.value"
        :newValue="i.changeValue"
        :rooms="rooms"
        :inventory="inventory"
        v-on:approved="approveAttribute(e.eventIdKey, i.attr.key)"
        v-on:denied="denyAttribute(e.eventIdKey, i.attr.key)"
      ></tcc-pa-val>
    </template>
  </template>
  <h4 v-if="(publicityChanges.length > 0) || (productionTechChanges.length > 0) || (request.attributeValues.WebCalendarDescription != request.changes.attributeValues.WebCalendarDescription) || (request.attributeValues.Notes != request.changes.attributeValues.Notes)">Additional Changes</h4>
  <template v-if="publicityChanges.length > 0">
    <h4 class="text-accent">Publicity</h4>
    <tcc-pa-val 
      v-for="(i, idx) in publicityChanges"
      :key="idx"
      :attribute="i.attr"
      :originalValue="i.value"
      :newValue="i.changeValue"
      v-on:approved="approveRequestAttribute(i.attr.key)"
      v-on:denied="denyRequestAttribute(i.attr.key)"
    ></tcc-pa-val>
  </template>
  <template v-if="productionTechChanges.length > 0">
    <h4 class="text-accent">Production Accommodations</h4>
    <tcc-pa-val 
      v-for="(i, idx) in productionTechChanges"
      :key="idx"
      :attribute="i.attr"
      :originalValue="i.value"
      :newValue="i.changeValue"
      v-on:approved="approveRequestAttribute(i.attr.key)"
      v-on:denied="denyRequestAttribute(i.attr.key)"
    ></tcc-pa-val>
  </template>
  <template v-if="request.attributeValues.WebCalendarDescription != request.changes.attributeValues.WebCalendarDescription">
    <h4 class="text-accent">Web Calendar</h4>
    <tcc-pa-val 
      :attribute="request.attributes.WebCalendarDescription"
      :originalValue="request.attributeValues.WebCalendarDescription"
      :newValue="request.changes.attributeValues.WebCalendarDescription"
      v-on:approved="approveRequestAttribute(request.attributes.WebCalendarDescription.key)"
      v-on:denied="denyRequestAttribute(request.attributes.WebCalendarDescription.key)"
    ></tcc-pa-val>
  </template>
  
  <h4>Custom Message for Ministry</h4>
  <p class="text-italic text-muted">
    This message will be included in the notification to the ministry that you have resolved their requested changes.
  </p>
  <rck-html
    v-model="message"
    :editorHeight="300"
  ></rck-html>
</div>

<v-style>
  .text-red:empty::before {
    content: "Empty";
  }
  .form-control-static div {
    word-break: break-word;
    white-space: pre-wrap;
  }
</v-style>
`
});
