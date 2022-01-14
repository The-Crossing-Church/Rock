export default {
  template: `
  <v-form ref="roomform" v-model="valid">
    <v-row>
      <v-col>
        <br/>
        <v-autocomplete
          label="Select a Room/Space to view availability"
          :items="groupedRooms"
          item-text="Value"
          item-value="Id"
          item-disabled="IsDisabled"
          v-model="selected"
          attach
          :rules="[rules.required(selected, 'Room/Space')]"
          :value="request.Events[0].Rooms"
        >
          <template v-slot:selection="data">
            {{data.item.Value}} ({{data.item.Capacity}})
          </template>
          <template v-slot:item="data">
            <template v-if="typeof data.item !== 'object'">
              <v-list-item-content v-text="data.item"></v-list-item-content>
            </template>
            <template v-else>
              <v-list-item-content>
                <v-list-item-title>{{data.item.Value}} ({{data.item.Capacity}})</v-list-item-title>
              </v-list-item-content>
            </template>
          </template>
        </v-autocomplete>
      </v-col>  
    </v-row>
    <br/>
    <template v-if="page == 0">
      <v-sheet height="600">
        <v-calendar
          ref="calendar"
          :now="today"
          :value="today"
          :events="events"
          color="primary"
          type="week"
          @click:time="calendarClick"
          :weekdays="weekdays"
        ></v-calendar>
      </v-sheet>
    </template>
    <template v-else>
      <h4>{{ formatDate(eventDate) }}</h4>
      <v-row>
        <v-col>
          <strong>What time will your event begin and end?</strong>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" md="6">
          <strong>Start Time</strong>
          <time-picker
            v-model="startTime"
            :value="startTime"
            :rules="[rules.required(startTime, 'Start Time'), rules.validTime(startTime, endTime, true)]"
          ></time-picker>
        </v-col>
        <v-col cols="12" md="6">
          <strong>End Time</strong>
          <time-picker
            v-model="endTime"
            :value="endTime"
            :rules="[rules.required(endTime, 'End Time'), rules.validTime(endTime, startTime, false)]"
          ></time-picker>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-text-field
            label="How many people are you expecting to attend?"
            type="number"
            v-model="att"
            :value="request.Events[0].ExpectedAttendance"
            :rules="[rules.required(att, 'Expected Attendance'), rules.exceedsSelected(att, selected, rooms)]"
          ></v-text-field>
        </v-col>  
      </v-row>
      <v-row>
        <v-col>
          <v-btn color='accent' @click="page=0">back</v-btn>  
        </v-col>  
      </v-row>
    </template>
  </v-form>
`,
  props: ["rules", "request"],
  data: function () {
      return {
          today: moment().format("yyyy-MM-DD"),
          valid: true,
          allEvents: [],
          page: 0,
          selected: this.request.Events[0].Rooms[0],
          rooms: [],
          eventDate: this.request.Events[0].EventDate,
          startTime: this.request.Events[0].StartTime,
          endTime: this.request.Events[0].EndTime,
          att: this.request.Events[0].ExpectedAttendance,
      };
  },
  mounted: function () {
      this.$refs.calendar.scrollToTime("08:00");
  },
  created() {
      this.allEvents = [];
      this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
      let rawEvents = JSON.parse($('[id$="hfThisWeeksRequests"]')[0].value);
      let oneWeek = moment().add(6, 'days')
      for (i = 0; i < rawEvents.length; i++) {
          let event = JSON.parse(rawEvents[i])
          if (event.IsSame || event.Events.length == 1) {
              for (k = 0; k < event.EventDates.length; k++) {
                  let inRange = moment(event.EventDates[k]).isBetween(moment(), oneWeek, 'days', '[]')
                  if (inRange) {
                      this.allEvents.push({
                          name: event.Name,
                          start: moment(`${event.EventDates[k]} ${event.Events[0].StartTime}`).format("yyyy-MM-DD HH:mm"),
                          end: moment(`${event.EventDates[k]} ${event.Events[0].EndTime}`).format("yyyy-MM-DD HH:mm"),
                          loc: event.Events[0].Rooms,
                      });
                  }
              }
          } else {
              event.Events.forEach(e => {
                  let inRange = moment(e.EventDate).isBetween(moment(), oneWeek, 'days', '[]')
                  if (inRange) {
                      this.allEvents.push({
                          name: event.Name,
                          start: moment(`${e.EventDate} ${e.StartTime}`).format("yyyy-MM-DD HH:mm"),
                          end: moment(`${e.EventDate} ${e.EndTime}`).format("yyyy-MM-DD HH:mm"),
                          loc: e.Rooms,
                      });
                  }
              })
          }
      }
  },
  computed: {
      weekdays() {
          let dow = moment().day();
          let arr = [];
          for (i = dow; i < 7; i++) {
              arr.push(i);
          }
          for (i = 0; i < dow; i++) {
              arr.push(i);
          }
          return arr;
      },
      events() {
          if (this.selected) {
              return this.allEvents.filter((i) => {
                  return i.loc.includes(this.selected);
              });
          } else {
              return [];
          }
      },
      groupedRooms() {
          let loc = []
          this.rooms.forEach(l => {
              let idx = -1
              loc.forEach((i, x) => {
                  if (i.Type == l.Type) {
                      idx = x
                  }
              })
              if (idx > -1) {
                  loc[idx].locations.push(l)
              } else {
                  loc.push({ Type: l.Type, locations: [l] })
              }
          })
          loc.forEach(l => {
              l.locations = l.locations.sort((a, b) => {
                  if (a.Value < b.Value) {
                      return -1
                  } else if (a.Value > b.Value) {
                      return 1
                  } else {
                      return 0
                  }
              })
          })
          loc = loc.sort((a, b) => {
              if (a.Type < b.Type) {
                  return -1
              } else if (a.Type > b.Type) {
                  return 1
              } else {
                  return 0
              }
          })
          let arr = []
          loc.forEach(l => {
              arr.push({ header: l.Type })
              l.locations.forEach(i => {
                  arr.push((i))
              })
              arr.push({ divider: true })
          })
          arr.splice(arr.length - 1, 1)
          return arr
      },
  },
  methods: {
      calendarClick(val) {
          this.eventDate = val.date;
          let hour = val.hour;
          let min = val.minute;
          let apm = "AM";
          if (hour >= 12) {
              if (hour > 12) {
                  hour -= 12;
              }
              apm = "PM";
          }
          if (hour.toString().length < 2) {
              hour = "0" + hour;
          }
          if (min < 15) {
              min = "00";
          } else if (min < 30) {
              min = "15";
          } else if (min < 45) {
              min = "30";
          } else {
              min = "45";
          }
          this.startTime = hour + ":" + min + " " + apm;
          this.page = 1;
      },
      emitChanges() {
          this.$emit("update", {
              eventDate: this.eventDate,
              startTime: this.startTime,
              endTime: this.endTime,
              room: this.selected,
              att: this.att,
          });
      },
      formatDate(val) {
          return moment(val).format("dddd, MMMM Do yyyy");
      },
  },
  watch: {
      selected(val) {
          this.emitChanges();
      },
      startTime(val) {
          this.emitChanges();
      },
      endTime(val) {
          this.emitChanges();
      },
      att(val) {
          this.emitChanges();
      },
      request: {
          handler(val) { },
          deep: true,
      },
  },
}