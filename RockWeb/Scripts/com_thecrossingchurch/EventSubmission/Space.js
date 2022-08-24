import utils from '/Scripts/com_thecrossingchurch/EventSubmission/Utilities.js';
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
            <v-list-item @click="selectAll(data.item.Value, $event)">
              <div style="display: flex; flex-direction: column;">
                <div>
                  <v-list-item-content class="accent--text text-subtitle-2">
                    <v-list-item-title>
                      {{data.item.Value}}
                    </v-list-item-title>
                  </v-list-item-content>
                </div>
                <div style="display: flex; width: 100%;">
                  <v-list-item-action style="margin: 0px; margin-right: 32px;">
                    <v-checkbox :value="allAreChecked(data.item.Value)"></v-checkbox>
                  </v-list-item-action>
                  <v-list-item-content>
                    <v-list-item-title>
                      All {{data.item.Value}}
                    </v-list-item-title>
                  </v-list-item-content>
                </div>
              </div>
            </v-list-item>
          </template>
          <template v-else>
            <v-list-item v-bind="data.attrs" v-on="data.on">
              <v-list-item-action style="margin: 0px; margin-right: 32px;">
                <v-checkbox :value="data.attrs.inputValue" readonly :disabled="data.item.IsDisabled" v-model="data.attrs.inputValue"></v-checkbox>
              </v-list-item-action>
              <v-list-item-content>
                <v-list-item-title>{{data.item.Value}} ({{data.item.Capacity}})</v-list-item-title>
                <v-list-item-subtitle v-if="data.item.SetUp">{{data.item.SetUp}}</v-list-item-subtitle>
              </v-list-item-content>
            </v-list-item>
          </template>
        </template>
      </v-autocomplete>
    </v-col>
  </v-row>
  <v-row v-if="isInfrastructureRequest">
    <v-col>
      <v-textarea
        label="Other Spaces"
        v-model="e.InfrastructureSpace"
      ></v-textarea>
    </v-col>
  </v-row>
  <v-row v-if="canRequestTables">
    <v-col cols="12" md="6">
      <v-select
        label="What kinds of tables would you like?"
        :items="['Round', 'Rectangular']"
        multiple
        attach
        v-model="e.TableType"
      ></v-select>
    </v-col>
    <v-col cols="12" md="6" v-if="!request.needsAccom && isSuperUser && canRequestSpecialAccom" style="display: flex; align-items: center;">
      <v-btn icon @click="openSetUp">
        <v-icon>mdi-seat</v-icon>
      </v-btn>
      <v-switch
        label="I need extensive set up (anything beyond the standard set up for the space)"
        hint="Did you forget to toggle Special Accommodations? Click here to add it. Click the icon to view the set up for your spaces."
        persistent-hint
        v-model="request.needsAccom"
      ></v-switch>
    </v-col>
  </v-row>
  <v-row v-if="canRequestTables && e.TableType.includes('Round')">
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
  <v-row v-if="canRequestTables && e.TableType.includes('Rectangular')">
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
  <v-row v-if="canRequestTables && e.TableType && e.TableType.length > 0">
    <v-col>
      <v-switch
        :label="tableClothLabel"
        v-model="e.NeedsTableCloths"
      ></v-switch>
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
  <v-dialog
    v-if="setUp"
    v-model="setUp"
    max-width="85%"
  >
    <v-card>
      <template v-if="e.Rooms && e.Rooms.length > 0">
        <v-card-title>Standard Set-up for {{formatRooms(e.Rooms)}}</v-card-title>
        <v-card-text>
          <v-list>
            <v-list-item v-for="r in e.Rooms" :key="r">
              <v-list-item-content v-html="formatRoomAndSetUp(r)"></v-list-item-content>
            </v-list-item>
          </v-list>
        </v-card-text>  
      </template>
      <template v-else>
        <v-card-title></v-card-title>
        <v-card-text>
          You have not selected any spaces.
        </v-card-text>  
      </template>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="secondary" @click="setUp = false;">Close</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</v-form>
`,
  props: ["e", "request", "existing"],
  data: function () {
      return {
          cnsole: console,
          dialog: false,
          map: false,
          setUp: false,
          valid: true,
          rooms: [],
          ministries: [],
          prefillDate: '',
          searchInput: '',
          isSuperUser: false,
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
    this.allEvents = []
    this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value)
    this.ministries = JSON.parse($('[id$="hfMinistries"]')[0].value)
    let isSU = $('[id$="hfIsSuperUser"]')[0].value
    if(isSU == 'True') {
      this.isSuperUser = true
    }
  },
  filters: {
    formatDate(val) {
      return moment(val).format("MM/DD/yyyy")
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
      let existingOnDate = []
      this.existing.forEach((e, evIdx) => {
        if(e.Id != this.request.Id) {
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
            events.forEach((event, idx) => {
              let overlaps = false
              let date = event.EventDate
              if(e.IsSame) {
                date = intersect[idx]
              }
              let cdStart = moment(`${date} ${event.StartTime}`, `yyyy-MM-DD hh:mm A`)
              if (event.MinsStartBuffer) {
                  cdStart = cdStart.subtract(event.MinsStartBuffer, "minute");
              }
              let cdEnd = moment(`${date} ${event.EndTime}`, `yyyy-MM-DD hh:mm A`).subtract(1, 'minute')
              if (event.MinsEndBuffer) {
                  cdEnd = cdEnd.add(event.MinsEndBuffer, "minute");
              }
              let cRange = moment.range(cdStart, cdEnd);
              for(let i=0; i<dates.length; i++) {
                let current = moment.range(
                    moment(`${dates[i]} ${this.e.StartTime}`, `yyyy-MM-DD hh:mm A`),
                    moment(`${dates[i]} ${this.e.EndTime}`, `yyyy-MM-DD hh:mm A`).subtract(1, 'minute')
                );
                if (cRange.overlaps(current)) {
                  overlaps = true
                }
              }
              if (overlaps) {
                existingOnDate.push(event)
              }
            })
          }
        }
      })
      let existingRooms = []
      existingOnDate.forEach(e => {
        existingRooms.push(...e.Rooms)
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
        if(!l.IsActive) {
          l.IsDisabled = true
        }
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
        arr.push({ Value: l.Type, IsHeader: true, IsDisabled: false})
        l.locations.forEach(i => {
          arr.push((i))
        })
      })
      return arr
    },
    CheckinLabel() {
      return `Do you need in-person check-in on the day of the event? (${this.boolToYesNo(this.e.Checkin)})`
    },
    canRequestTables() {
      if(this.request.Id > 0 && this.request.Status != 'Draft' && this.e.TableType ) {
        return true
      }
      let dates = this.request.EventDates.map(d => moment(d))
      let minDate = moment.min(dates)
      let oneWeek = moment(new Date()).add(14, 'days')
      if (!this.request.IsSame || this.request.Events.length > 1) {
        minDate = moment(this.e.EventDate)
      }
      if (oneWeek.isAfter(minDate)) {
        return false
      }
      return true
    },
    canRequestSpecialAccom() {
      let dates = this.request.EventDates.map(d => moment(d))
      let minDate = moment.min(dates)
      let twoWeeks = moment(new Date()).add(14, 'days')
      if (!this.request.IsSame || this.request.Events.length > 1) {
        minDate = moment(this.e.EventDate)
      }
      if (twoWeeks.isAfter(minDate)) {
        return false
      }
      return true
    },
    tableClothLabel() {
      return `Would you like tablecloths? (${this.boolToYesNo(this.e.NeedsTableCloths)})`
    },
    isInfrastructureRequest() {
      let ministryName = this.ministries.filter(m => { return m.Id == this.request.Ministry })[0]?.Value
      if(ministryName?.toLowerCase().includes("infrastructure")) {
        return true
      }
      return false
    },
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
    ...utils.methods,
    prefillSection() {
      this.dialog = false
      let idx = this.request.EventDates.indexOf(this.prefillDate)
      let currIdx = this.request.EventDates.indexOf(this.e.EventDate)
      this.$emit('updatespace', { targetIdx: idx, currIdx: currIdx })
    },
    openMap() {
      this.map = true
    },
    openSetUp() {
      this.setUp = true
    },
    formatRoomAndSetUp(roomId) {
      let room = this.rooms.filter(r => r.Id.toString() == roomId)[0]
      if(room) {
        return `<strong>${room.Value}</strong><span>${room.SetUp != '' ? room.SetUp : 'There is no standard set up for this space.'}</span>`
      }
      return ''
    },
    selectAll(category, event) {
      //Don't add header to room list
      event.preventDefault()
      //Select all/none of the rooms in the section
      let roomsInCategory = this.groupedRooms.filter(r => {
        return r.Type == category && !r.IsDisabled
      }).map(r => r.Id).sort()
      let selectedRoomsInCategory = this.e.Rooms.filter(r => {
        return roomsInCategory.includes(r)
      }).sort()
      if(roomsInCategory.toString() == selectedRoomsInCategory.toString()) {
        this.e.Rooms = this.e.Rooms.filter(r => {
          return !roomsInCategory.includes(r)
        })
      } else {
        for(let i = 0; i < roomsInCategory.length; i++) {
          let idx = this.e.Rooms.indexOf(roomsInCategory[i])
          if(idx < 0) {
            this.e.Rooms.push(roomsInCategory[i])
          }
        }
        this.e.Rooms = this.e.Rooms.sort()
      }
    },
    allAreChecked(category) {
      //Select all/none of the rooms in the section
      let roomsInCategory = this.groupedRooms.filter(r => {
        return r.Type == category && !r.IsDisabled
      }).map(r => r.Id).sort()
      let selectedRoomsInCategory = this.e.Rooms.filter(r => {
        return roomsInCategory.includes(r)
      }).sort()
      if(roomsInCategory.toString() == selectedRoomsInCategory.toString()) {
        return true
      } else {
        return false
      }
    }
  }
}
