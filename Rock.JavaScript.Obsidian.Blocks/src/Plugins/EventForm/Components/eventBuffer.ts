import { defineComponent, PropType } from "vue"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import { DateTime, Interval } from "luxon"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import TimePicker from "./timePicker"

export default defineComponent({
    name: "EventForm.Components.EventBuffer",
    components: {
      "rck-field": RockField,
      "rck-lbl": RockLabel,
      "tcc-time": TimePicker,
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
        setUpTime: "",
        tearDownTime: ""
      };
    },
    computed: {
      
    },
    methods: {
      startChanged() {
        if(this.e && this.e.attributeValues) {
          let buffer = this.e.attributeValues.StartBuffer ? parseInt(this.e.attributeValues.StartBuffer) : 0
          let newBuffer = buffer
          let start = DateTime.fromFormat(this.e.attributeValues.StartTime, "HH:mm:ss")

          if(start.isValid){
            if(this.e.attributeValues.RoomSetUp) {
              let data = JSON.parse(this.e.attributeValues.RoomSetUp)
              if(data && data.length > 0) {
                if(newBuffer < 30) {
                  newBuffer = 30
                }
              }
            }

            if(this.e.attributeValues.StartTime) {
              let minStart = start.startOf('day')
              let interval = Interval.fromDateTimes(minStart, start)
              let maxBuffer = interval.length('minutes')
              if(buffer > maxBuffer) {
                newBuffer = maxBuffer
              }
            }
            
            if(newBuffer >= 0) {
              this.e.attributeValues.StartBuffer = `${newBuffer}`
              this.setUpTime = start.minus({minutes: newBuffer}).toFormat("HH:mm:ss")
            } 
          }
        }
      },
      endChanged() {
        if(this.e && this.e.attributeValues) {
          let buffer = this.e.attributeValues.EndBuffer ? parseInt(this.e.attributeValues.EndBuffer) : 0
          let newBuffer = buffer
          let end = DateTime.fromFormat(this.e.attributeValues.EndTime, "HH:mm:ss")

          if(end.isValid){
            if(this.e.attributeValues.RoomSetUp) {
              let data = JSON.parse(this.e.attributeValues.RoomSetUp)
              if(data && data.length > 0) {
                if(buffer < 30) {
                  newBuffer = 30
                }
              }
            }
            if(this.e.attributeValues.EndTime) {
              let maxEnd = end.endOf('day')
              let interval = Interval.fromDateTimes(end, maxEnd)
              let maxBuffer = Math.floor(interval.length('minutes'))
              if(buffer > maxBuffer) {
                newBuffer = maxBuffer
              }
            }
            
            if(newBuffer >= 0) {
              this.e.attributeValues.EndBuffer = `${newBuffer}`
              this.tearDownTime = end.plus({minutes: newBuffer}).toFormat("HH:mm:ss")
            }
          }
        }
      },
      formatTime(val) {
        if(val) {
          let dt = DateTime.fromFormat(val, "HH:mm:ss")
          return dt.toFormat("h:mm a")
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
      },
      'e.attributeValues.StartBuffer': {
        handler(val) {
          if(val) {
            this.startChanged()
          }
        }
      },
      'e.attributeValues.EndBuffer': {
        handler(val) {
          if(val) {
            this.endChanged()
          }
        }
      },
      setUpTime(val) {
        if(this.e?.attributeValues?.StartTime) {
          let start = DateTime.fromFormat(this.e.attributeValues.StartTime, "HH:mm:ss")
          if(val) {
            let setUp = DateTime.fromFormat(val, "HH:mm:ss")
            if(start && setUp) {
              let interval = Interval.fromDateTimes(setUp, start)
              let buffer = interval.length('minutes')
              this.e.attributeValues.StartBuffer = `${buffer}`
            }
          }
        }
      },
      tearDownTime(val) {
        if(this.e?.attributeValues?.EndTime) {
          let end = DateTime.fromFormat(this.e.attributeValues.EndTime, "HH:mm:ss")
          if(val) {
            let tearDown = DateTime.fromFormat(val, "HH:mm:ss")
            if(end && tearDown) {
              let interval = Interval.fromDateTimes(end, tearDown)
              let buffer = interval.length('minutes')
              this.e.attributeValues.EndBuffer = `${buffer}`
            }
          }
        }
      }
    },
    mounted() {
      this.startChanged()
      this.endChanged()
    },
    template: `
<div class="row">
  <div class="col col-xs-12 col-md-6">
    <tcc-time
      :label="e.attributes.StartBuffer.name"
      v-model="setUpTime"
    ></tcc-time> 
  </div>
  <div class="col col-xs-12 col-md-6">
    <tcc-time
      :label="e.attributes.EndBuffer.name"
      v-model="tearDownTime"
    ></tcc-time> 
  </div>
</div>
<br/>
<div class="row" >
  <div class="col col-xs-6" v-if="setUpTime != ''">
    <rck-lbl>Set-up</rck-lbl> <br/>
    Begins at {{formatTime(setUpTime)}}, {{e.attributeValues.StartBuffer}} minutes before the start of your event.
  </div>
  <div class="col col-xs-6" v-if="tearDownTime != ''">
    <rck-lbl>Tear-down</rck-lbl> <br/>
    Ends at {{formatTime(tearDownTime)}}, {{e.attributeValues.EndBuffer}} minutes after the end of your event.
  </div>
</div>
`
});
