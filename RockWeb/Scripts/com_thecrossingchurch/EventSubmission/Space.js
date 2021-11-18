export default {
  template: `
<v-form ref="spaceForm" v-model="valid">
<v-row>
  <v-col>
    <h3 class="primary--text" v-if="request.Events.length == 1">Space Information</h3>
    <h3 class="primary--text" v-else>
      Space Information 
      <v-btn rounded outlined color="accent" @click="prefillDate = ''; dialog = true; ">
        Prefill
      </v-btn>
    </h3>
  </v-col>
</v-row>
<v-row>
  <v-col cols="12" md="6">
    <v-text-field
      label="How many people are you expecting to attend?"
      type="number"
      v-model="e.ExpectedAttendance"
      :rules="[rules.required(e.ExpectedAttendance, 'Expected Attendance'), rules.isInt(e.ExpectedAttendance, 'Expected Attendance')]"
      :hint="attHint"
    ></v-text-field>
  </v-col>
  <v-col cols="12" md="6">
    <v-autocomplete
      label="What room(s) would you like to reserve"
      :items="groupedRooms"
      item-text="Value"
      item-value="Id"
      item-disabled="IsDisabled"
      v-model="e.Rooms"
      prepend-inner-icon="mdi-map"
      @click:prepend-inner="openMap"
      :search-input.sync="searchInput"
      @change="searchInput=''"
      chips
      deletable-chips
      multiple
      attach
      :rules="[rules.requiredArr(e.Rooms, 'Room/Space'), rules.roomCapacity(rooms, e.Rooms, e.ExpectedAttendance)]"
      hint="Click the map icon to view campus map. Rooms that are unselectable are unavailable for your dates and times."
      persistent-hint
    >
      <template v-slot:prepend-item>
        <v-toolbar dense color="primary">Room (Capacity)</v-toolbar>
      </template>
      <template v-slot:item="data">
        <template v-if="data.item.IsHeader">
          <v-list-item-content class="accent--text text-subtitle-2">{{data.item.Value}}</v-list-item-content>
        </template>
        <template v-else>
          <v-list-item v-bind="data.attrs" v-on="data.on">
            <v-list-item-action style="margin: 0px; margin-right: 32px;">
              <v-checkbox :value="data.attrs.inputValue" @change="data.parent.$emit('select')" :disabled="data.item.IsDisabled" v-model="data.attrs.inputValue"></v-checkbox>
            </v-list-item-action>
            <v-list-item-content>
              <v-list-item-title>{{data.item.Value}} ({{data.item.Capacity}})</v-list-item-title>
            </v-list-item-content>
          </v-list-item>
        </template>
      </template>
    </v-autocomplete>
  </v-col>
</v-row>
<v-row v-if="canRequestTables && !request.needsAccom">
  <v-col cols="12" md="6">
    <v-select
      label="What kinds of tables would you like?"
      :items="['Round', 'Rectangular']"
      multiple
      attach
      v-model="e.TableType"
    ></v-select>
  </v-col>
</v-row>
<v-row v-if="canRequestTables && !request.needsAccom && e.TableType.includes('Round')">
  <v-col>
    <v-text-field
      label="How many round tables do you need?"
      type="number"
      v-model="e.NumTablesRound"
      :rules="[rules.isInt(e.NumTablesRound, 'Number of tables')]"
    ></v-text-field>
  </v-col>
  <v-col>
    <v-text-field
      label="How many chairs should each round table have?"
      type="number"
      v-model="e.NumChairsRound"
      :rules="[rules.isInt(e.NumChairsRound, 'Number of chairs')]"
    ></v-text-field>
  </v-col>
</v-row>
<v-row v-if="canRequestTables && !request.needsAccom && e.TableType.includes('Rectangular')">
  <v-col>
    <v-text-field
      label="How many rectangular tables do you need?"
      type="number"
      v-model="e.NumTablesRect"
      :rules="[rules.isInt(e.NumTablesRect, 'Number of tables')]"
    ></v-text-field>
  </v-col>
  <v-col>
    <v-text-field
      label="How many chairs should each rectangular table have?"
      type="number"
      v-model="e.NumChairsRect"
      :rules="[rules.isInt(e.NumChairsRect, 'Number of chairs')]"
    ></v-text-field>
  </v-col>
</v-row>
<v-row v-if="request.needsReg">
  <v-col cols="12" md="6">
    <v-switch
      :label="CheckinLabel"
      v-model="e.Checkin"
    ></v-switch>
  </v-col>
  <v-col cols="12" md="6" v-if="e.Checkin && e.ExpectedAttendance >= 100">
    <v-switch
      label="Since your event is estimated to support more than 100 people, would you like the database team to provide a team to work your event in-person?"
      v-model="e.SupportTeam"
    ></v-switch>
  </v-col>
</v-row>
<v-dialog
  v-if="dialog"
  v-model="dialog"
  max-width="850px"
>
  <v-card>
    <v-card-title>
      Pre-fill this section with information from another date
    </v-card-title>  
    <v-card-text>
      <v-select
        :items="prefillOptions"
        v-model="prefillDate"
      >
        <template v-slot:selection="data">
          {{data.item | formatDate}}
        </template>
        <template v-slot:item="data">
          {{data.item | formatDate}}
        </template>
      </v-select>  
    </v-card-text>  
    <v-card-actions>
      <v-btn color="secondary" @click="dialog = false; prefillDate = '';">Cancel</v-btn> 
      <v-spacer></v-spacer> 
      <v-btn color="primary" @click="prefillSection">Pre-fill Section</v-btn>  
    </v-card-actions>  
  </v-card>
</v-dialog>
<v-dialog
  v-if="map"
  v-model="map"
  max-width="85%"
>
  <v-card>
    <v-card-text>
      <v-img src="https://rock.thecrossingchurch.com/Content/Operations/Campus%20Map.png"/>  
    </v-card-text>  
  </v-card>
</v-dialog>
</v-form>
`,
  props: ["e", "request", "existing"],
  data: function () {
      return {
          dialog: false,
          map: false,
          valid: true,
          rooms: [],
          prefillDate: '',
          searchInput: '',
          rules: {
              required(val, field) {
                  return !!val || `${field} is required`;
              },
              requiredArr(val, field) {
                  return val.length > 0 || `${field} is required`;
              },
              isInt(val, field) {
                  if (val) {
                      return !(val.includes('.') || val.includes('-')) || `${field} must be a whole number`
                  }
                  return true
              },
              exceedsSelected(val, selected, rooms) {
                  if (val && selected) {
                      let room = rooms.filter((i) => {
                          return i.Id == selected;
                      })[0];
                      let cap = room.Capacity;
                      if (val > cap) {
                          return `You cannot have more than ${cap} ${cap == 1 ? "person" : "people"
                              } in the selected space`;
                      }
                  }
                  return true;
              },
              roomCapacity(allRooms, rooms, attendance) {
                  if (attendance) {
                      let selectedRooms = allRooms.filter((r) => {
                          return rooms.includes(r.Id);
                      });
                      let maxCapacity = 0;
                      selectedRooms.forEach((r) => {
                          maxCapacity += r.Capacity;
                      });
                      if (attendance <= maxCapacity) {
                          return true;
                      } else {
                          return `This selection of rooms alone can only support a maximum capacity of ${maxCapacity}. Please select more rooms for increased capacity or lower your expected attendance.`;
                      }
                  }
                  return true;
              },
          }
      }
  },
  created: function () {
      this.allEvents = [];
      this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
  },
  filters: {
      formatDate(val) {
          return moment(val).format("MM/DD/yyyy");
      },
  },
  computed: {
      attHint() {
          return this.e.ExpectedAttendance > 250 ? 'Events with more than 250 attendees must be approved by the city and requests must be submitted at least 30 days in advance' : ''
      },
      prefillOptions() {
          return this.request.EventDates.filter(i => i != this.e.EventDate)
      },
      groupedRooms() {
          let loc = []
          let dates = []
          if(this.request.IsSame) {
            dates = this.request.EventDates
          } else {
            dates.push(this.e.EventDate)
          }
          let existingOnDate = this.existing.filter(e => {
            e = JSON.parse(e)
            if(e.Id == this.request.Id) {
              return false
            }
            let intersect = e.EventDates.filter(val => dates.includes(val))
            if(intersect.length > 0) {
              //Filter to events object for the matching dates
              let events = []
              if(e.IsSame) {
                events = e.Events
              } else {
                events = e.Events.filter(val => dates.includes(val.EventDate))
              }
              //Check if the times overlap
              let overlaps = false
              events.forEach((event, idx) => {
                let date = event.EventDate
                if(e.IsSame) {
                  date = intersect[idx]
                }
                let cdStart = moment(`${date} ${event.StartTime}`, `yyyy-MM-DD hh:mm A`)
                if (event.MinsStartBuffer) {
                    cdStart = cdStart.subtract(event.MinsStartBuffer, "minute");
                }
                let cdEnd = moment(`${date} ${event.EndTime}`, `yyyy-MM-DD hh:mm A`)
                if (event.MinsEndBuffer) {
                    cdEnd = cdEnd.add(event.MinsEndBuffer, "minute");
                }
                let cRange = moment.range(cdStart, cdEnd);
                for(let i=0; i<dates.length; i++) {
                  let current = moment.range(
                      moment(`${dates[i]} ${this.e.StartTime}`, `yyyy-MM-DD hh:mm A`),
                      moment(`${dates[i]} ${this.e.EndTime}`, `yyyy-MM-DD hh:mm A`)
                  );
                  if (cRange.overlaps(current)) {
                    overlaps = true
                  }
                }
              })
              return overlaps
            }
            return false
          }).map(e => JSON.parse(e))
          let existingRooms = []
          existingOnDate.forEach(e => {
            e.Events.forEach(ev => {
              existingRooms.push(...ev.Rooms)
            })
          })
          this.rooms.forEach(l => {
              let idx = -1
              loc.forEach((i, x) => {
                  if (i.Type == l.Type) {
                      idx = x
                  }
              })
              //Disable rooms not available for the date/time
              l.IsDisabled = false
              l.IsHeader = false
              if(existingRooms.includes(l.Id)){
                l.IsDisabled = true
              }
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
              arr.push({ Value: l.Type, IsHeader: true, IsDisabled: true})
              l.locations.forEach(i => {
                  arr.push((i))
              })
              // arr.push({ divider: true })
          })
          arr.splice(arr.length - 1, 1)
          return arr
      },
      CheckinLabel() {
          return `Do you need in-person check-in on the day of the event? (${this.boolToYesNo(this.e.Checkin)})`
      },
      canRequestTables() {
          let dates = this.request.EventDates.map(d => moment(d))
          let minDate = moment.min(dates)
          let oneWeek = moment(new Date()).add(7, 'days')
          if (!this.request.IsSame || this.request.Events.length > 1) {
              minDate = moment(this.e.EventDate)
          }
          if (oneWeek.isAfter(minDate)) {
              return false
          }
          return true
      }
  },
  watch: {
    groupedRooms: {
      handler(val) {
        let self = this
        let selected = val.filter(i => {
          return self.e.Rooms.includes(i.Id)
        })
        selected.forEach(s => {
          if(s.IsDisabled == true) {
            let idx = self.e.Rooms.indexOf(s.Id)
            self.e.Rooms.splice(idx, 1)
          }
        })
      },
      deep: true
    }
  },
  methods: {
      prefillSection() {
          this.dialog = false
          let idx = this.request.EventDates.indexOf(this.prefillDate)
          let currIdx = this.request.EventDates.indexOf(this.e.EventDate)
          this.$emit('updatespace', { targetIdx: idx, currIdx: currIdx })
      },
      boolToYesNo(val) {
          if (val) {
              return "Yes";
          }
          return "No";
      },
      openMap() {
          this.map = true
      }
  }
}