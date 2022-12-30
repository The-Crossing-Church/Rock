import { defineComponent, PropType } from "vue"
import { ContentChannelItem } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField"
import RockLabel from "../../../../Elements/rockLabel"
import SpaceInfo from "./spaceInfo"
import OnlineInfo from "./onlineInfo"
import CateringInfo from "./cateringInfo"
import ChildcareInfo from "./childcareInfo"
import OpsAccomInfo from "./opsAccomInfo"
import RegistrationInfo from "./registrationInfo"
import WebCalInfo from "./calendarInfo"
import ProdAccomInfo from "./productionAccomInfo"
import PublicityInfo from "./publicityInfo"
import { DateTime } from "luxon"
import { Button, Modal } from "ant-design-vue"
import DatePicker from "../../EventForm/Components/calendar"
import Chip from "../../EventForm/Components/chip"

export default defineComponent({
    name: "EventDashboard.Components.Modal",
    components: {
      "tcc-space": SpaceInfo,
      "tcc-online": OnlineInfo,
      "tcc-catering": CateringInfo,
      "tcc-childcare": ChildcareInfo,
      "tcc-ops": OpsAccomInfo,
      "tcc-registration": RegistrationInfo,
      "tcc-web-cal": WebCalInfo,
      "tcc-production": ProdAccomInfo,
      "tcc-publicity": PublicityInfo,
      "tcc-date": DatePicker,
      "tcc-chip": Chip,
      "rck-field": RockField,
      "rck-lbl": RockLabel,
      "a-btn": Button,
      "a-modal": Modal,
    },
    props: {
      request: Object,
      rooms: Array,
      drinks: Array,
      createdBy: Object,
      modifiedBy: Object,
    },
    setup() {

    },
    data() {
        return {
          modal: false
        };
    },
    computed: {
      statusClass() {
        return "status-box text-" + this.request?.attributeValues.RequestStatus.replaceAll(" ", "").toLowerCase() + " border-" + this.request?.attributeValues.RequestStatus.replaceAll(" ", "").toLowerCase()
      },
      eventDates() {
        if(this.request?.attributeValues.EventDates) {
          return this.request?.attributeValues.EventDates.split(",").map((d: string) => { 
            return DateTime.fromFormat(d.trim(), "yyyy-MM-dd").toFormat("DDDD")
          })
        }
      },
      changeDates() {
        if(this.request?.attributeValues.EventDates && this.request?.changes?.attributeValues.EventDates) {
          let dates = this.request.attributeValues.EventDates.split(",").map((date: string) => date.trim())
          let changeDates = this.request.changes.attributeValues.EventDates.split(",").map((date: string) => date.trim())
          let changes = [] as any[]
          if(dates.join(",") != changeDates.join(",")) {
            let combined = [] as string[]
            for(let i = 0; i < dates.length; i++) {
              let idx = combined.indexOf(dates[i])
              if(idx < 0) {
                combined.push(dates[i])
              }
            }
            for(let i = 0; i < changeDates.length; i++) {
              let idx = combined.indexOf(changeDates[i])
              if(idx < 0) {
                combined.push(changeDates[i])
              }
            }
            combined = combined.sort((a: string, b: string) => {
              let aDt = DateTime.fromFormat(a, 'yyyy-MM-dd')
              let bDt = DateTime.fromFormat(b, 'yyyy-MM-dd')
              if(aDt < bDt) {
                return -1
              } else if(aDt > bDt) {
                return 1
              }
              return 0
            })
            for(let i = 0; i < combined.length; i++) {
              if(dates.includes(combined[i])) {
                if(changeDates.includes(combined[i])) {
                  changes.push({date: DateTime.fromFormat(combined[i], 'yyyy-MM-dd').toFormat('DDDD'), class: 'text-black'})
                } else {
                  changes.push({date: DateTime.fromFormat(combined[i], 'yyyy-MM-dd').toFormat('DDDD'), class: 'text-red text-strikethrough'})
                }
              } else {
                changes.push({date: DateTime.fromFormat(combined[i], 'yyyy-MM-dd').toFormat('DDDD'), class: 'text-primary'})
              }
            }
            return changes
          }
        }
        return null
      },
      originalResources() {
        if(this.request?.attributeValues?.RequestType) {
          return this.request.attributeValues.RequestType.split(',')
        }
        return []
      },
      changesResources() {
        if(this.request?.changes?.attributeValues?.RequestType) {
          return this.request.changes.attributeValues.RequestType.split(',')
        }
        return []
      }
    },
    methods: {
      getCollapseName(id: string) {
        return "collapse-"+id
      },
      getCollapseReference(id: string) {
        return "#collapse-"+id
      },
      getPanelName(date: string, room: string) {
        let title = "" 
        if(date) {
          title = DateTime.fromFormat(date, 'yyyy-MM-dd').toFormat('MM/dd/yyyy')
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
        title += " ("
        let rooms = JSON.parse(room)
        let roomList = rooms.text.split(", ")
        if(roomList.length > 5) {
          title += roomList.slice(0,5).join(", ")
          title += "..."
        } else {
          title += rooms.text
        }
        title +=")"
        return title
      },
      formatDateTime(dt: string) {
        let date = DateTime.fromISO(dt)
        if(date) {
          return date.toFormat("MM/dd/yyyy hh:mm a")
        }
      },
      getConflictingDates(conflict: any) {
        if(conflict.attributeValues) {
          if(conflict.attributeValues.EventDate != "") {
            return DateTime.fromFormat(conflict.attributeValues.EventDate.trim(), "yyyy-MM-dd").toFormat("MM/dd/yyyy")
          } else if(conflict.attributeValues.EventDates) {
            let dates = conflict.attributeValues.EventDates.split(",").map((d: string) => d.trim())
            let selectedDates = this.request?.attributeValues.EventDates.split(",").map((d: string) => d.trim())
            return selectedDates.filter((d: string) => { return dates.includes(d) }).map((d: string) => {
              return DateTime.fromFormat(d, "yyyy-MM-dd").toFormat("MM/dd/yyyy")
            }).join(", ")
          }
        }
        return ""
      },
      getConflictingRooms(conflict: any) {
        if(conflict.attributeValues) {
          if(conflict.attributeValues.Rooms) {
            let obj = JSON.parse(conflict.attributeValues.Rooms)
            let conflictRooms = obj.text.split(", ")
            let selectedRooms = [] as string[]
            this.request?.childItems.forEach((ci: any) => {
              if(ci.attributeValues.Rooms) {
                let val = JSON.parse(ci.attributeValues.Rooms)
                let rooms = val.text.split(", ").filter((r: string) => {
                  return conflictRooms.includes(r)
                })
                rooms.forEach((r: string) => {
                  if(selectedRooms.indexOf(r) < 0) {
                    selectedRooms.push(r)
                  }
                })
              }
            })
            return selectedRooms.join(", ")
          }
        }
        return ""
      },
      openRelated(request: any) {
        if(request.attributeValues.ParentId && request.attributeValues.ParentId != "") {
          request.id = request.attributeValues.ParentId
        }
        this.$emit("openrequest", request)
      },
      previewStartBuffer(time: string, buffer: any) {
        if(time && buffer) {
          return DateTime.fromFormat(time, 'HH:mm:ss').minus({minutes: buffer}).toFormat('hh:mm a')
        } else if (time) {
          return DateTime.fromFormat(time, 'HH:mm:ss').toFormat('hh:mm a')
        }
      },
      previewEndBuffer(time: string, buffer: any) {
        if(time && buffer) {
          return DateTime.fromFormat(time, 'HH:mm:ss').plus({minutes: buffer}).toFormat('hh:mm a')
        } else if (time) {
          return DateTime.fromFormat(time, 'HH:mm:ss').toFormat('hh:mm a')
        }
      },
      getChipClassName(resource: string, fromOriginal: Boolean) {
        let className = ""
        if(fromOriginal) {
          if(this.changesResources && this.changesResources.length > 0) {
            let idx = this.changesResources.indexOf(resource)
            if(idx < 0) {
              className += "text-red text-strikethrough "
            }
          }
        } else {
          if(this.originalResources && this.originalResources.length > 0) {
            let idx = this.originalResources.indexOf(resource)
            if(idx >= 0) {
              className += "chip-hidden"
            }
          }
        }
        return className
      }
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<div>
  <div class="row mb-2">
    <div class="col col-xs-12">
      <h4>
        <template v-if="request.changes && request.changes.title.endsWith('Changes') && request.title != request.changes.title.substring(0, (request.changes.title.length - 8))">
          <span class="text-red">{{request.title}}</span>: <span class="text-primary">{{request.changes.title.substring(0, (request.changes.title.length - 8))}}</span>
        </template>
        <template v-else>
          {{request.title}}
        </template>
        <i v-if="request.attributeValues.RequestIsValid == 'True'" class="fa fa-check-circle text-accent ml-2"></i>
        <i v-else class="fa fa-exclamation-circle text-inprogress ml-2"></i>
      </h4>
      <div :class="statusClass">{{request.attributeValues.RequestStatus}}</div>
    </div>
  </div>
  <div class="row mb-4">
    <div class="col col-xs-12">
      <div class="chip-group">
        <tcc-chip v-for="r in originalResources" :class="getChipClassName(r, true)" :disabled="true">{{r}}</tcc-chip>
        <tcc-chip v-for="r in changesResources" :class="getChipClassName(r, false)" :disabled="true">{{r}}</tcc-chip>
      </div>
    </div>
  </div>
  <div class="row mb-4">
    <div class="col col-xs-6">
      <div class="row">
        <div class="col col-xs-6">
          <rck-lbl>Submitted By</rck-lbl><br/>
          {{createdBy.nickName}} {{createdBy.lastName}}
        </div>
        <div class="col col-xs-6" v-if="createdBy.nickName != modifiedBy.nickName">
          <rck-lbl>Modified By</rck-lbl><br/>
          {{modifiedBy.nickName}} {{modifiedBy.lastName}}
        </div>
      </div>
    </div>
    <div class="col col-xs-6 text-right">
      <rck-lbl>Submitted On</rck-lbl> <br/>
      {{formatDateTime(request.startDateTime)}}
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-6">
      <template v-if="request.changes && request.changes.attributeValues.Ministry != request.attributeValues.Ministry">
        <div class="row">
          <div class="col col-xs-6">
            <rck-field
              v-model="request.attributeValues.Ministry"
              :attribute="request.attributes.Ministry"
              class="text-red"
              :showEmptyValue="true"
            ></rck-field>
          </div>
          <div class="col col-xs-6">
            <rck-field
              v-model="request.changes.attributeValues.Ministry"
              :attribute="request.attributes.Ministry"
              class="text-primary"
              :showEmptyValue="true"
              :showLabel="false"
              style="padding-top: 18px;"
            ></rck-field>
          </div>
        </div>
      </template>
      <template v-else>
        <rck-field
          v-model="request.attributeValues.Ministry"
          :attribute="request.attributes.Ministry"
        ></rck-field>
      </template>
    </div>
    <div class="col col-xs-6">
      <template v-if="request.changes && request.changes.attributeValues.Contact != request.attributeValues.Contact">
        <div class="row">
          <div class="col col-xs-6">
            <rck-field
              v-model="request.attributeValues.Contact"
              :attribute="request.attributes.Contact"
              class="text-red"
              :showEmptyValue="true"
            ></rck-field>
          </div>
          <div class="col col-xs-6">
            <rck-field
              v-model="request.changes.attributeValues.Contact"
              :attribute="request.attributes.Contact"
              class="text-primary"
              :showEmptyValue="true"
              :showLabel="false"
              style="padding-top: 18px;"
            ></rck-field>
          </div>
        </div>
      </template>
      <template v-else>
        <rck-field
          v-model="request.attributeValues.Contact"
          :attribute="request.attributes.Contact"
        ></rck-field>
      </template>
    </div>
  </div>
  <div class="row mb-4">
    <div class="col col-xs-6">
      <a-btn type="primary" shape="circle" class="mr-1" @click="modal = true">
        <i class="fa fa-calendar"></i> 
      </a-btn>
      View All Event Dates
    </div>
  </div>
  <div id="accordion" class="accordion">
    <template v-for="ci in request.childItems" :key="ci.id">
      <div class='panel no-border'>
        <a role="button" :href="getCollapseReference(ci.id)" data-parent="#accordion" data-toggle="collapse" aria-expanded="false" :aria-controls="getCollapseName(ci.id)">
          <h5 class='header pb-1'>{{ getPanelName(ci.attributeValues.EventDate, ci.attributeValues.Rooms) }} 
          <i v-if="ci.attributeValues.EventIsValid == 'True'" class="fa fa-check-circle text-accent"></i>
          <i v-else class="fa fa-exclamation-circle text-inprogress"></i>
          <i class='fa expand-icon pull-right'></i></h5>
        </a>
        <div class='collapse collapse body' :id="getCollapseName(ci.id)" aria-expanded="false">
          <div class="row">
            <div class="col col-xs-12 col-md-6">
              <template v-if="ci.changes && ci.changes.attributeValues.StartTime != ci.attributeValues.StartTime">
                <div class="row">
                  <div class="col col-xs-6">
                    <rck-field
                      v-model="ci.attributeValues.StartTime"
                      :attribute="ci.attributes.StartTime"
                      class="text-red"
                      :showEmptyValue="true"
                    ></rck-field>
                  </div>
                  <div class="col col-xs-6">
                    <rck-field
                      v-model="ci.changes.attributeValues.StartTime"
                      :attribute="ci.attributes.StartTime"
                      class="text-primary"
                      :showEmptyValue="true"
                      :showLabel="false"
                      style="padding-top: 18px;"
                    ></rck-field>
                  </div>
                </div>
              </template>
              <template v-else>
                <rck-field
                  v-model="ci.attributeValues.StartTime"
                  :attribute="ci.attributes.StartTime"
                  :showEmptyValue="true"
                ></rck-field>
              </template>
            </div>
            <div class="col col-xs-12 col-md-6">
              <template v-if="ci.changes && ci.changes.attributeValues.EndTime != ci.attributeValues.EndTime">
                <div class="row">
                  <div class="col col-xs-6">
                    <rck-field
                      v-model="ci.attributeValues.EndTime"
                      :attribute="ci.attributes.EndTime"
                      class="text-red"
                      :showEmptyValue="true"
                    ></rck-field>
                  </div>
                  <div class="col col-xs-6">
                    <rck-field
                      v-model="ci.changes.attributeValues.EndTime"
                      :attribute="ci.attributes.EndTime"
                      class="text-primary"
                      :showEmptyValue="true"
                      :showLabel="false"
                      style="padding-top: 18px;"
                    ></rck-field>
                  </div>
                </div>
              </template>
              <template v-else>
                <rck-field
                  v-model="ci.attributeValues.EndTime"
                  :attribute="ci.attributes.EndTime"
                  :showEmptyValue="true"
                ></rck-field>
              </template>
            </div>
          </div>
          <div class="row mb-2">
            <div class="col col-xs-6">
              <rck-lbl>Start Time Set-up Buffer</rck-lbl> <br/>
              <template v-if="ci.attributeValues.StartBuffer != ''">
                {{ci.attributeValues.StartBuffer}} minutes: {{previewStartBuffer(ci.attributeValues.StartTime, ci.attributeValues.StartBuffer)}}
              </template>
            </div>
            <div class="col col-xs-6">
              <rck-lbl>End Time Tear-down Buffer</rck-lbl> <br/>
              <template v-if="ci.attributeValues.EndBuffer != ''">
                {{ci.attributeValues.EndBuffer}} minutes: {{previewEndBuffer(ci.attributeValues.EndTime, ci.attributeValues.EndBuffer)}}
              </template>
            </div>
          </div>
          <tcc-space v-if="request.attributeValues.NeedsSpace == 'True' || ( request.changes && request.changes.attributeValues.NeedsSpace == 'True' )" :details="ci" :rooms="rooms"></tcc-space>
          <tcc-catering v-if="request.attributeValues.NeedsCatering == 'True' || ( request.changes && request.changes.attributeValues.NeedsCatering == 'True' )" :details="ci" :drinks="drinks"></tcc-catering>
          <tcc-ops v-if="request.attributeValues.NeedsOpsAccommodations == 'True' || ( request.changes && request.changes.attributeValues.NeedsOpsAccommodations == 'True' )" :details="ci" :rooms="rooms" :drinks="drinks" :needsCatering="request.attributeValues.NeedsCatering"></tcc-ops>
          <tcc-childcare v-if="request.attributeValues.NeedsChildCare == 'True' || ( request.changes && request.changes.attributeValues.NeedsChildCare == 'True' )" :details="ci"></tcc-childcare>
          <tcc-registration v-if="request.attributeValues.NeedsRegistration == 'True' || ( request.changes && request.changes.attributeValues.NeedsRegistration == 'True' )" :details="ci"></tcc-registration>
          <tcc-online v-if="request.attributeValues.NeedsOnline == 'True' || ( request.changes && request.changes.attributeValues.NeedsOnline == 'True' )" :details="ci"></tcc-online>
        </div>
      </div>
    </template>
    <tcc-web-cal v-if="request.attributeValues.NeedsWebCalendar == 'True' || ( request.changes && request.changes.attributeValues.NeedsWebCalendar == 'True' )" :request="request"></tcc-web-cal>
    <tcc-production v-if="request.attributeValues.NeedsProductionAccommodations == 'True' || ( request.changes && request.changes.attributeValues.NeedsProductionAccommodations == 'True' )" :request="request"></tcc-production>
    <tcc-publicity v-if="request.attributeValues.NeedsPublicity == 'True' || ( request.changes && request.changes.attributeValues.NeedsPublicity == 'True' )" :request="request"></tcc-publicity>
    <div class="row" v-if="request.attributeValues.Notes != ''">
      <div class="col col-xs-12">
        <rck-field
          v-model="request.attributeValues.Notes"
          :attribute="request.attributes.Notes"
          :showEmptyValue="true"
        ></rck-field>
      </div>
    </div>
    <template v-if="request.conflicts && request.conflicts.length > 0">
      <h3 class="text-red">Conflicts</h3>
      <div class="row">
        <div class="col col-xs-4 hover" v-for="c in request.conflicts" :key="c.id" @click="openRelated(c)">
          {{c.title}}<br/> 
          {{getConflictingDates(c)}}: {{getConflictingRooms(c)}}
        </div>
      </div>
    </template>
    <template v-if="request.changesConflicts && request.changesConflicts.length > 0">
      <h3 class="text-red">Conflicts With Requested Changes</h3>
      <div class="row">
        <div class="col col-xs-4 hover" v-for="c in request.changesConflicts" :key="c.id" @click="openRelated(c)">
          {{c.title}}<br/> 
          {{getConflictingDates(c)}}: {{getConflictingRooms(c)}}
        </div>
      </div>
    </template>
  </div>
</div>
<a-modal v-model:visible="modal" width="50%">
  <template v-if="request.changes && request.changes.attributeValues.EventDates.split(',').map(d => d.trim()).join(',') != request.attributeValues.EventDates.split(',').map(d => d.trim()).join(',')">
    <div class="row">
      <div class="col col-xs-6">
        <tcc-date v-model="request.changes.attributeValues.EventDates" :multiple="true" :readonly="true"></tcc-date>
      </div>
      <div class="col col-xs-6">
        <div v-for="d in changeDates">
          <span :class="d.class">{{d.date}}</span>
        </div>
      </div>
    </div>
  </template>
  <template v-else>
    <div class="row">
      <div class="col col-xs-6">
        <tcc-date v-model="request.attributeValues.EventDates" :multiple="true" :readonly="true"></tcc-date>
      </div>
      <div class="col col-xs-6">
        <div v-for="d in eventDates">
          {{d}}
        </div>
      </div>
    </div>
  </template>
</a-modal>
<v-style>
  .panel.no-border {
    box-shadow: none !important;
  }
  .panel.no-border a h5 {
    border-bottom: 1px solid #e2e2e2;
  }
  a[aria-expanded="true"] h5 i.expand-icon:before {
    content: "\\f0d8";
  }
  a[aria-expanded="false"] h5 i.expand-icon:before {
    content: "\\f0d7";
  }
  .status-box {
    padding: 4px 8px;
    border: 2px solid;
    border-radius: 8px;
    position: absolute;
    top: 0px;
    right: 8px;
    font-weight: 500;
    font-size: 1.1em;
  }
</v-style>
`
});
