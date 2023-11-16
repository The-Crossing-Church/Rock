export default {
  template: `
<v-form ref="accomForm" v-model="valid" v-if="canRequestSpecialAccom">
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
    <v-col cols="12" md="6">
      <v-switch
        v-model="needsDrinks"
        label="Would you like drinks?"
      ></v-switch>
    </v-col>
    <v-col cols="12" md="6" v-if="needsDrinks">
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
  <v-row v-if="needsDrinks">
    <v-col cols="12" md="6">
      <time-picker 
        label="What time would you like your drinks to be delivered?" 
        v-model="e.DrinkTime"
        :value="e.DrinkTime"
        :rules="[rules.required(e.DrinkTime, 'Time')]"
      ></time-picker>
    </v-col>
    <v-col cols="12" md="6">
      <v-text-field
        label="Where would you like your drinks delivered?"
        v-model="e.DrinkDropOff"
        :rules="[rules.required(e.DrinkDropOff, 'Location')]"
      ></v-text-field>
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
      needsDrinks: false,
      rules: {
        required(val, field) {
          return !!val || `${field} is required`;
        },
        requiredArr(val, field) {
          return val.length > 0 || `${field} is required`;
        },
      }
    }
  },
  created: function () {
    if(this.e.Drinks && this.e.Drinks.length > 0 || this.e.DrinkTime || this.e.DrinkDropOff) {
      this.needsDrinks = true
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
    defaultFoodTime() {
      if (this.e.StartTime && !this.e.StartTime.includes('null')) {
        let time = moment(this.e.StartTime, "hh:mm A");
        return time.subtract(30, "minutes").format("hh:mm A");
      }
      return null;
    },
    canRequestSpecialAccom() {
      if(this.request.Id > 0 && this.request.Status != "Draft" && this.e.Drinks && this.e.Drinks.length > 0) {
        //This date already requested drinks
        return true
      }
      let twoWeeks = moment(new Date()).add(14, 'days')
      if(this.request.IsSame) {
        let dates = this.request.EventDates.map(d => moment(d))
        let minDate = moment.min(dates)
        if (twoWeeks.isAfter(moment(minDate))) {
          return false
        }
      } else {
        if (twoWeeks.isAfter(moment(this.e.EventDate))) {
          return false
        }
      }
      return true
    },
  },
  methods: {
    prefillSection() {
      this.dialog = false
      let idx = this.request.EventDates.indexOf(this.prefillDate)
      let currIdx = this.request.EventDates.indexOf(this.e.EventDate)
      this.$emit('updateaccom', { targetIdx: idx, currIdx: currIdx })
      if((this.e.Drinks && this.e.Drinks.length > 0) && this.e.DrinkTime && this.e.DrinkDropOff) {
        this.needsDrinks = true
      }
    },
    boolToYesNo(val) {
      if (val) {
          return "Yes";
      }
      return "No";
    },
  },
  watch: {
    e(val) {
      if((!val.Drinks || val.Drinks.length == 0) && !val.DrinkTime && !val.DrinkDropOff) {
        this.needsDrinks = false
      }
      if(val.Drinks && val.Drinks.length > 0 || val.DrinkTime || val.DrinkDropOff) {
        this.needsDrinks = true
      }
    },
    needsDrinks(val) {
      if(!val) {
        this.e.Drinks = []
        this.e.DrinkTime = ''
        this.e.DrinkDropOff = ''
      }
    }
  }
}