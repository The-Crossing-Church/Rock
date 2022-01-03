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
        <strong>What time would you like your drinks to be delivered?</strong>
        <time-picker
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
    <v-col cols="12" md="6">
      <v-switch
        :label="doorLabel"
        v-model="e.NeedsDoorsUnlocked"
      ></v-switch>
    </v-col>
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
</v-form>
`,
  props: ["e", "request"],
  data: function () {
    return {
      dialog: false,
      valid: true,
      prefillDate: '',
      setupImage: {},
      rules: {
        required(val, field) {
          return !!val || `${field} is required`;
        },
        requiredArr(val, field) {
          return val.length > 0 || `${field} is required`;
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
      }
    }
  },
  created: function () {
    if (this.e.SetUpImage) {
      this.setupImage = this.e.SetUpImage;
    }
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
    drinkHint() {
      return ''
      // return `${this.e.Drinks.toString().includes('Coffee') ? 'Due to COVID-19, all drip coffee must be served by a designated person or team from the hosting ministry. This person must wear a mask and gloves and be the only person to touch the cups, sleeves, lids, and coffee carafe before the coffee is served to attendees. If you are not willing to provide this for your own event, please deselect the coffee option and opt for an individually packaged item like bottled water or soda.' : ''}`
    },
    defaultFoodTime() {
      if (this.e.StartTime && !this.e.StartTime.includes('null')) {
        let time = moment(this.e.StartTime, "hh:mm A");
        return time.subtract(30, "minutes").format("hh:mm A");
      }
      return null;
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
      let file = { name: e.name, type: e.type };
      var reader = new FileReader();
      const self = this;
      reader.onload = function (e) {
        console.log(e)
        file.data = e.target.result;
        self.e.SetUpImage = file;
      };
      reader.readAsDataURL(e);
    },
    boolToYesNo(val) {
      if (val) {
        return "Yes";
      }
      return "No";
    },
  }
}