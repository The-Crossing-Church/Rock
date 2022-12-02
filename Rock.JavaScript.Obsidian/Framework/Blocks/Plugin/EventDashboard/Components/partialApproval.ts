import { defineComponent } from "vue"
import RockField from "../../../../Controls/rockField"
import RockLabel from "../../../../Elements/rockLabel"
import { Button } from "ant-design-vue"
import PAValues from "./partialApprovalValues"


export default defineComponent({
    name: "EventDashboard.Components.Modal.PartialApproval",
    components: {
      "rck-field": RockField,
      "rck-lbl": RockLabel,
      "a-btn": Button,
      "tcc-pa-val": PAValues
    },
    props: {
      request: Object
    },
    setup() {

    },
    data() {
      return {
        approvedAttributes: [] as string[],
        deniedAttributes: [] as string[],
        eventChanges: [] as any[]
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
            let item = {"eventid": this.request.childItems[i].id, "date": this.request.childItems[i].attributeValues.EventDate, "approvedAttrs": [] as string[], "deniedAttrs": [] as string[], sections: [] as any[]}
            for(let key in this.request.childItems[i].attributes) {
              let attrItem = { attr: this.request.childItems[i].attributes[key], value: "", changeValue: null }
              if(attrItem.attr.categories.length == 0) {
                continue
              }
              let categories = attrItem.attr.categories.map((c: any) => c.name)
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
            if(item.sections.length > 0) {
              events.push(item)
            }
          }
        }
        this.eventChanges = events
      },
      approveAttribute(id: number, key: string) {
        for(let i = 0; i < this.eventChanges.length; i++) {
          if(this.eventChanges[i].eventid == id) {
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
      denyAttribute(id: number, key: string) {
        for(let i = 0; i < this.eventChanges.length; i++) {
          if(this.eventChanges[i].eventid == id) {
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
  <div class="row mb-2">
    <div class="col col-xs-12">
      <h4>Partial Approval for: {{request.title}}</h4>
    </div>
  </div>
  
  <template v-if="request.changes && request.title != request.changes.title.substring(0, (request.changes.title.length - 8))">
    <div class="row" style="display: flex; align-items: center;">
      <div class="col col-xs-10">
        <div class="row">
          <div class="col col-xs-6 text-red">
            <rck-lbl>Request Title</rck-lbl><br/>
            {{request.title}}
          </div>
          <div class="col col-xs-6 text-primary">
            <div style="padding-top: 18px;">
              {{request.changes.title.substring(0, (request.changes.title.length - 8))}}
            </div>
          </div>
        </div>
      </div>
      <div class="col col-xs-2">
        <a-btn shape="circle" type="accent" class="mr-1" @click="approveRequestAttribute('Title')">
          <i class="fa fa-check"></i>
        </a-btn>
        <a-btn shape="circle" type="red" @click="denyRequestAttribute('Title')">
          <i class="fa fa-times"></i>
        </a-btn>
      </div>
    </div>
  </template>
  <template v-if="request.attributeValues.Ministry != request.changes.attributeValues.Ministry">
    <tcc-pa-val 
      :attribute="request.attributes.Ministry"
      :originalValue="request.attributeValues.Ministry"
      :newValue="request.changes.attributeValues.Ministry"
      v-on:approved="approveRequestAttribute(request.attributes.Ministry.key)"
      v-on:denied="denyRequestAttribute(request.attributes.Ministry.key)"
    ></tcc-pa-val>
  </template>
  <template v-if="request.attributeValues.Contact != request.changes.attributeValues.Contact">
    <tcc-pa-val 
      :attribute="request.attributes.Contact"
      :originalValue="request.attributeValues.Contact"
      :newValue="request.changes.attributeValues.Contact"
      v-on:approved="approveRequestAttribute(request.attributes.Contact.key)"
      v-on:denied="denyRequestAttribute(request.attributes.Contact.key)"
    ></tcc-pa-val>
  </template>
  <template v-if="request.attributeValues.EventDates != request.changes.attributeValues.EventDates">
    <tcc-pa-val 
      :attribute="request.attributes.EventDates"
      :originalValue="request.attributeValues.EventDates"
      :newValue="request.changes.attributeValues.EventDates"
      v-on:approved="approveRequestAttribute(request.attributes.EventDates.key)"
      v-on:denied="denyRequestAttribute(request.attributes.EventDates.key)"
    ></tcc-pa-val>
  </template>
  <template v-for="e in eventChanges" :key="e.id">
    <h3>
      <template v-if="e.date != ''">Changes on {{e.date}}</template>
      <template v-else>Event Changes</template>
    </h3>
    <hr/>
    <template v-for="s in e.sections" :key="s.category">
      <h3 class="text-accent">{{s.category}}</h3>
      <tcc-pa-val 
        v-for="(i, idx) in s.items"
        :key="idx"
        :attribute="i.attr"
        :originalValue="i.value"
        :newValue="i.changeValue"
        v-on:approved="approveAttribute(e.eventid, i.attr.key)"
        v-on:denied="denyAttribute(e.eventid, i.attr.key)"
      ></tcc-pa-val>
    </template>
  </template>
  <h3>Additional Changes</h3>
  <template v-if="publicityChanges.length > 0">
    <h3 class="text-accent">Publicity</h3>
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
    <h3 class="text-accent">Production Accommodations</h3>
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
    <h3 class="text-accent">Web Calendar</h3>
    <tcc-pa-val 
      :attribute="request.attributes.WebCalendarDescription"
      :originalValue="request.attributeValues.WebCalendarDescription"
      :newValue="request.changes.attributeValues.WebCalendarDescription"
      v-on:approved="approveRequestAttribute(request.attributes.WebCalendarDescription.key)"
      v-on:denied="denyRequestAttribute(request.attributes.WebCalendarDescription.key)"
    ></tcc-pa-val>
  </template>
</div>
`
});
