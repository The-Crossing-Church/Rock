import { defineComponent, PropType } from "vue"
import { ContentChannelItem } from "../../../../ViewModels"
import { DateTime } from "luxon"
import RockField from "../../../../Controls/rockField"

export default defineComponent({
    name: "EventForm.Components.EventBuffer",
    components: {
      "rck-field": RockField,
    },
    props: {
      e: {
        type: Object as PropType<ContentChannelItem>,
        required: false
      },
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
          if(this.e.attributeValues.RoomSetUp) {
            let data = JSON.parse(this.e.attributeValues.RoomSetUp)
            if(data && data.length > 0) {
              let buffer = this.e.attributeValues.StartBuffer ? parseInt(this.e.attributeValues.StartBuffer) : 0
              if(buffer < 30) {
                this.e.attributeValues.StartBuffer = "30"
              }
            }
          }
        }
      },
      endChanged() {
        if(this.e && this.e.attributeValues) {
          if(this.e.attributeValues.RoomSetUp) {
            let data = JSON.parse(this.e.attributeValues.RoomSetUp)
            if(data && data.length > 0) {
              let buffer = this.e.attributeValues.EndBuffer ? parseInt(this.e.attributeValues.EndBuffer) : 0
              if(buffer < 30) {
                this.e.attributeValues.EndBuffer = "30"
              }
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
    ></rck-field>
  </div>
  <div class="col col-xs-12 col-md-6">
    <rck-field
      v-model="e.attributeValues.EndBuffer"
      :attribute="e.attributes.EndBuffer"
      :is-edit-mode="true"
      v-on:change="endChanged"
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
