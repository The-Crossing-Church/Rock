import { defineComponent, PropType } from "vue"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import { DateTime, Interval } from "luxon"
import RockField from "@Obsidian/Controls/rockField.obs"

export default defineComponent({
    name: "EventForm.Components.EventBuffer",
    components: {
      "rck-field": RockField
    },
    props: {
      e: {
        type: Object as PropType<ContentChannelItemBag>,
        required: false
      }
    },
    setup() {

    },
    data() {
      return {

      };
    },
    computed: {
      
    },
    methods: {
      startChanged() {
        if(this.e && this.e.attributeValues) {
          let buffer = this.e.attributeValues.StartBuffer ? parseInt(this.e.attributeValues.StartBuffer) : 0
          if(this.e.attributeValues.RoomSetUp) {
            let data = JSON.parse(this.e.attributeValues.RoomSetUp)
            if(data && data.length > 0) {
              if(buffer < 30) {
                this.e.attributeValues.StartBuffer = "30"
              }
            }
          }
          if(this.e.attributeValues.StartTime) {
            let start = DateTime.fromFormat(this.e.attributeValues.StartTime, "HH:mm:ss")
            let minStart = start.startOf('day')
            let interval = Interval.fromDateTimes(minStart, start)
            let maxBuffer = interval.length('minutes')
            if(buffer > maxBuffer) {
              this.e.attributeValues.StartBuffer = `${maxBuffer}`
            }
          }
        }
      },
      endChanged() {
        if(this.e && this.e.attributeValues) {
          let buffer = this.e.attributeValues.EndBuffer ? parseInt(this.e.attributeValues.EndBuffer) : 0
          if(this.e.attributeValues.RoomSetUp) {
            let data = JSON.parse(this.e.attributeValues.RoomSetUp)
            if(data && data.length > 0) {
              if(buffer < 30) {
                this.e.attributeValues.EndBuffer = "30"
              }
            }
          }
          if(this.e.attributeValues.EndTime) {
            let end = DateTime.fromFormat(this.e.attributeValues.EndTime, "HH:mm:ss")
            let maxEnd = end.endOf('day')
            let interval = Interval.fromDateTimes(end, maxEnd)
            let maxBuffer = Math.floor(interval.length('minutes'))
            if(buffer > maxBuffer) {
              this.e.attributeValues.EndBuffer = `${maxBuffer}`
            }
          }
        }
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
      }
    },
    watch: {
      'e.attributeValues.StartTime': {
        handler(val) {
          this.startChanged()
        }
      },
      'e.attributeValues.EndTime': {
        handler(val) {
          this.endChanged()
        }
      }
    },
    mounted() {

    },
    template: `
<div class="row">
  <div class="col col-xs-12 col-md-6">
    <rck-field
      v-model="e.attributeValues.StartBuffer"
      :attribute="e.attributes.StartBuffer"
      :is-edit-mode="true"
      v-on:change="startChanged"
      id="txtStartBuffer"
    ></rck-field>
  </div>
  <div class="col col-xs-12 col-md-6">
    <rck-field
      v-model="e.attributeValues.EndBuffer"
      :attribute="e.attributes.EndBuffer"
      :is-edit-mode="true"
      v-on:change="endChanged"
      id="txtEndBuffer"
    ></rck-field>
  </div>
</div>
<br/>
<div class="row" >
  <div class="col col-xs-6" v-if="e.attributeValues.StartBuffer != ''">
    <rck-lbl>Space Reservation Starting At</rck-lbl> <br/>
    {{e.attributeValues.StartBuffer}} minutes: {{previewStartBuffer(e.attributeValues.StartTime, e.attributeValues.StartBuffer)}}
  </div>
  <div class="col col-xs-6" v-if="e.attributeValues.EndBuffer != ''">
    <rck-lbl>Space Reservation Ending At</rck-lbl> <br/>
    {{e.attributeValues.EndBuffer}} minutes: {{previewEndBuffer(e.attributeValues.EndTime, e.attributeValues.EndBuffer)}}
  </div>
</div>
`
});
