export default {
  template: `
<v-form ref="accomForm" v-model="valid">
  <v-row>
    <v-col>
      <h3 class="primary--text" v-if="request.Events.length == 1">Other Accommodations</h3>
      <h3 class="primary--text" v-else>
        Other Accommodations 
        <v-btn rounded outlined color="accent" @click="prefillDate = ''; dialog = true; ">
          Prefill
        </v-btn>
      </h3>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="12">
      <h4 class="accent--text">Tech</h4>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="12">
      <v-autocomplete
        label="What tech needs do you have?"
        :items="['Handheld Mic', 'Wrap Around Mic', 'Special Lighting', 'Graphics/Video/Powerpoint', 'Worship Team', 'Stage Set-Up', 'Basic Live Stream ($)', 'Advanced Live Stream ($)', 'Pipe and Drape', 'BOSE System']"
        v-model="e.TechNeeds"
        :hint="techHint"
        persistent-hint
        multiple
        chips
        attach
      ></v-autocomplete>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="12">
      <v-textarea
        label='Please describe what you are envisioning regarding your tech needs. For example, "We would like to play videos in the gym."'
        v-model="e.TechDescription"
      ></v-textarea>
    </v-col>
  </v-row>
  <template v-if="!request.needsCatering">
    <v-row>
      <v-col cols="12">
        <h4 class="accent--text">Drinks</h4>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="6">
        <br />
        <v-row>
          <v-col>
            <v-autocomplete
              label="What drinks would you like to have?"
              :items="['Coffee', 'Soda', 'Water']"
              v-model="e.Drinks"
              multiple
              chips
              attach
            ></v-autocomplete>
          </v-col>
        </v-row>
      </v-col>
      <v-col cols="12" md="6" v-if="e.Drinks.length > 0">
        <time-picker
          label="What time would you like your drinks to be delivered?"
          v-model="e.DrinkTime"
          :value="e.DrinkTime"
          :default="defaultFoodTime"
          :rules="[rules.required(e.DrinkTime, 'Time')]"
        ></time-picker>
      </v-col>
    </v-row>
    <v-row v-if="e.Drinks.length > 0">
      <!--<v-col cols="12" md="6" v-if="e.Drinks.includes('Coffee')">
        <v-checkbox
          label="I agree to provide a coffee serving team in compliance with COVID-19 policy."
          :rules="[rules.required(e.ServingTeamAgree, 'Agreement to provide a serving team')]"
          v-model="e.ServingTeamAgree"
        ></v-checkbox>
      </v-col> -->
      <v-col cols="12" md="6">
        <v-text-field
          label="Where would you like your drinks delivered?"
          v-model="e.DrinkDropOff"
          :rules="[rules.required(e.DrinkDropOff, 'Location')]"
        ></v-text-field>
      </v-col>
    </v-row>
  </template>
  <v-row>
    <v-col cols="12">
      <h4 class="accent--text">Web Calendar</h4>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="12" md="6">
      <v-switch
        :label="calLabel"
        v-model="e.ShowOnCalendar"
      ></v-switch>
    </v-col>
  </v-row>
  <v-row v-if="e.ShowOnCalendar">
    <v-col>
      <v-textarea
        label="Please type out your blurb for the web calendar"
        v-model="e.PublicityBlurb"
        :rules="[rules.blurbValidation(e.PublicityBlurb, request.PublicityStartDate)]"
        validate-on-blur
      ></v-textarea>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="12">
      <h4 class="accent--text">Set-Up</h4>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="12" md="6">
      <v-switch
        :label="doorLabel"
        v-model="e.NeedsDoorsUnlocked"
        :hint="doorHint"
        persistent-hint
      ></v-switch>
    </v-col>
    <v-col cols="12" md="6" v-if="e.NeedsDoorsUnlocked">
      <v-autocomplete
        label="What doors would you like unlocked?"
        :items="groupedDoors"
        v-model="e.Doors"
        item-text="Value"
        item-value="Id"
        item-disabled="IsHeader"
        prepend-inner-icon="mdi-map"
        @click:prepend-inner="openMap"
        hint="You may choose specific doors, or leave this blank and the ops team will open doors that make sense for your event"
        persistent-hint
        multiple
        clearable
      >
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
                <v-list-item-title>{{data.item.Value}}</v-list-item-title>
              </v-list-item-content>
            </v-list-item>
          </template>
        </template>
      </v-autocomplete>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="12">
      <v-textarea
        label="Please describe the extensive set-up you require for your event"
        v-model="e.SetUp"
      ></v-textarea>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="12" md="6">
      <v-file-input
        accept="image/*"
        label="If you have an image of the set-up layout you would like upload it here"
        prepend-inner-icon="mdi-camera"
        prepend-icon=""
        v-model="setupImage"
        @change="handleSetUpFile"
      ></v-file-input>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="12">
      <h4 class="accent--text">Personnel</h4>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="12" md="6">
      <v-switch
        :label="medicalLabel"
        v-model="e.NeedsMedical"
      ></v-switch>
    </v-col>
    <v-col cols="12" md="6">
      <v-switch
        :label="securityLabel"
        v-model="e.NeedsSecurity"
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
  props: ["e", "request"],
  data: function () {
    return {
      dialog: false,
      valid: true,
      prefillDate: '',
      setupImage: {},
      map: false,
      rules: {
        required(val, field) {
          return !!val || `${field} is required`;
        },
        requiredArr(val, field) {
          return (val && val.length > 0) || `${field} is required`;
        },
        blurbValidation(value, pubDate) {
          let daysUntil = moment(pubDate).diff(moment(), "days");
          if (daysUntil <= 30) {
            return (
              value.length >= 150 ||
              "It doesn't look like you've entered a complete blurb, please enter the full blurb you wish to appear in publicity"
            );
          } else {
            return true;
          }
        },
      },
      doors: []
    }
  },
  created: function () {
    if (this.e.SetUpImage) {
      this.setupImage = this.e.SetUpImage;
    }
    this.doors = JSON.parse($('[id$="hfDoors"]')[0].value)
  },
  filters: {
    formatDate(val) {
      return moment(val).format("MM/DD/yyyy");
    },
  },
  computed: {
    prefillOptions() {
      return this.request.EventDates.filter(i => i != this.e.EventDate)
    },
    techHint() {
      return `${this.e.TechNeeds.toString().includes('Live Stream') ? 'Keep in mind that all live stream requests will come at an additional charge to the ministry, which will be verified with you in your follow-up email with the Events Director.' : ''}`
    },
    calLabel() {
      return `I would like this event to be listed on the public web calendar (${this.boolToYesNo(this.e.ShowOnCalendar)})`
    },
    doorLabel() {
      return `Will you need doors unlocked for this event? (${this.boolToYesNo(this.e.NeedsDoorsUnlocked)})`
    },
    doorHint(){
      if(this.e.NeedsDoorsUnlocked) {
        return ''
      } else {
        return 'You and your guests will need to enter through the Main Office Doors and/or use your staff fob for building access'
      }
    },
    drinkHint() {
      return ''
      // return `${this.e.Drinks.toString().includes('Coffee') ? 'Due to COVID-19, all drip coffee must be served by a designated person or team from the hosting ministry. This person must wear a mask and gloves and be the only person to touch the cups, sleeves, lids, and coffee carafe before the coffee is served to attendees. If you are not willing to provide this for your own event, please deselect the coffee option and opt for an individually packaged item like bottled water or soda.' : ''}`
    },
    medicalLabel() {
      return `Do you need medical personnel present at your event? (${this.boolToYesNo(this.e.NeedsMedical)})`
    },
    securityLabel() {
      return `Do you need security personnel present at your event? (${this.boolToYesNo(this.e.NeedsSecurity)})`
    },
    defaultFoodTime() {
      if (this.e.StartTime && !this.e.StartTime.includes('null')) {
        let time = moment(this.e.StartTime, "hh:mm A");
        return time.subtract(30, "minutes").format("hh:mm A");
      }
      return null;
    },
    groupedDoors() {
      let loc = []
      this.doors.forEach(l => {
        let idx = -1
        loc.forEach((i, x) => {
          if (i.Type == l.Type) {
            idx = x
          }
        })
        l.IsHeader = false
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
        arr.push({ Value: l.Type, IsHeader: true})
        l.locations.forEach(i => {
          arr.push((i))
        })
      })
      return arr
    },
  },
  methods: {
    prefillSection() {
      this.dialog = false
      let idx = this.request.EventDates.indexOf(this.prefillDate)
      let currIdx = this.request.EventDates.indexOf(this.e.EventDate)
      this.$emit('updateaccom', { targetIdx: idx, currIdx: currIdx })
    },
    handleSetUpFile(e) {
      if(e) {
        let file = { name: e.name, type: e.type };
        var reader = new FileReader();
        const self = this;
        reader.onload = function (e) {
          file.data = e.target.result;
          self.e.SetUpImage = file;
        };
        reader.readAsDataURL(e);
      } else {
        this.e.SetUpImage = null
      }
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