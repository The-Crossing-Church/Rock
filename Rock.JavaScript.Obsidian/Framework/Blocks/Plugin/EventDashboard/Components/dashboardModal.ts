import { defineComponent, PropType } from "vue";
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

export default defineComponent({
    name: "EventDashboard.Components.Modal",
    components: {
      "tcc-space": SpaceInfo,
      "tcc-online": OnlineInfo,
      "tcc-catering": CateringInfo,
      "tcc-childcare": ChildcareInfo,
      "tcc-ops": OpsAccomInfo,
      "tcc-registrations": RegistrationInfo,
      "tcc-web-cal": WebCalInfo,
      "tcc-production": ProdAccomInfo,
      "tcc-publicity": PublicityInfo,
      "tcc-date": DatePicker,
      "rck-field": RockField,
      "rck-lbl": RockLabel,
      "a-btn": Button,
      "a-modal": Modal,
    },
    props: {
      request: Object,
      rooms: Array,
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
        return "status-box text-" + this.request?.attributeValues.RequestStatus.replace(" ", "").toLowerCase() + " border-" + this.request?.attributeValues.RequestStatus.replace(" ", "").toLowerCase()
      },
      eventDates() {
        if(this.request?.attributeValues.EventDates) {
          return this.request?.attributeValues.EventDates.split(",").map((d: string) => { 
            return DateTime.fromFormat(d.trim(), "yyyy-MM-dd").toFormat("DDDD")
          })
        }
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
        {{request.title}}
        <i v-if="request.attributeValues.RequestIsValid == 'True'" class="fa fa-check-circle text-accent"></i>
        <i v-else class="fa fa-exclamation-circle text-inprogress"></i>
      </h4>
      <div :class="statusClass">{{request.attributeValues.RequestStatus}}</div>
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
      <rck-field
        v-model="request.attributeValues.Ministry"
        :attribute="request.attributes.Ministry"
      ></rck-field>
    </div>
    <div class="col col-xs-6">
      <rck-field
        v-model="request.attributeValues.Contact"
        :attribute="request.attributes.Contact"
      ></rck-field>
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
          <tcc-space v-if="request.attributeValues.NeedsSpace == 'True'" :details="ci" :rooms="rooms"></tcc-space>
          <tcc-online v-if="request.attributeValues.NeedsOnline == 'True'" :details="ci"></tcc-online>
          <tcc-catering v-if="request.attributeValues.NeedsCatering == 'True'" :details="ci"></tcc-catering>
          <tcc-childcare v-if="request.attributeValues.NeedsChildCare == 'True'" :details="ci"></tcc-childcare>
          <tcc-ops v-if="request.attributeValues.NeedsOpsAccommodations == 'True'" :details="ci"></tcc-ops>
          <tcc-registration v-if="request.attributeValues.NeedsRegistration == 'True'" :details="ci"></tcc-registration>
        </div>
      </div>
    </template>
    <tcc-web-cal v-if="request.attributeValues.NeedsWebCalendar == 'True'" :request="request"></tcc-web-cal>
    <tcc-production v-if="request.attributeValues.NeedsProductionAccommodations == 'True'" :request="request"></tcc-production>
    <tcc-publicity v-if="request.attributeValues.NeedsPublicity == 'True'" :request="request"></tcc-publicity>
    <div class="row" v-if="request.attributeValues.Notes != ''">
      <div class="col col-xs-12">
        <rck-field
          v-model="request.attributeValues.Notes"
          :attribute="request.attributes.Notes"
        ></rck-field>
      </div>
    </div>
  </div>
</div>
<a-modal v-model:visible="modal" width="50%">
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
