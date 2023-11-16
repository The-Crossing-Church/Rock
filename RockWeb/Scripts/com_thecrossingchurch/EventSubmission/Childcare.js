export default {
  template: `
<v-form ref="childForm" v-model="valid">
  <v-row>
    <v-col>
      <h3 class="primary--text" v-if="request.Events.length == 1">Childcare Information</h3>
      <h3 class="primary--text" v-else>
        Childcare Information 
        <v-btn rounded outlined color="accent" @click="prefillDate = ''; dialog = true; ">
          Prefill
        </v-btn>
      </h3>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="12" md="6">
      <time-picker 
        label="What time do you need childcare to start?" 
        v-model="e.CCStartTime"
        :value="e.CCStartTime"
        :default="defaultChildcareTime"
        :rules="[rules.required(e.CCStartTime, 'Time')]"
      ></time-picker>
    </v-col> 
    <v-col cols="12" md="6">
      <time-picker 
        label="What time will childcare end?" 
        v-model="e.CCEndTime"
        :value="e.CCEndTime"
        :rules="[rules.required(e.CCEndTime, 'Time')]"
      ></time-picker>
    </v-col> 
  </v-row>
  <v-row>
    <v-col cols="12" md="6">
      <v-autocomplete
        label="What ages of childcare do you want to offer?"
        :items="['Infant/Toddler', 'Preschool', 'K-2nd', '3-5th']"
        chips
        multiple
        attach
        v-model="e.ChildCareOptions"
        ref="childcareOptRef"
      >
        <template v-slot:item="data">
          <div style="padding: 12px 0px; width: 100%">
            <v-icon
              v-if="e.ChildCareOptions.includes(data.item)"
              color="primary"
              style="margin-right: 32px"
            >
              mdi-checkbox-marked
            </v-icon>
            <v-icon
              v-else
              color="primary"
              style="margin-right: 32px"
            >
              mdi-checkbox-blank-outline
            </v-icon>
            {{data.item}}
          </div>
        </template>
        <template v-slot:append-item>
          <v-list-item>
            <div
              class="hover"
              style="padding: 12px 0px; width: 100%"
              @click="toggleChildCareOptions"
            >
              <v-icon
                v-if="childCareSelectAll"
                color="primary"
                style="margin-right: 32px"
              >
                mdi-checkbox-marked
              </v-icon>
              <v-icon
                v-else
                color="primary"
                style="margin-right: 32px"
              >
                mdi-checkbox-blank-outline
              </v-icon>
              Select All
            </div>
          </v-list-item>
        </template>
      </v-autocomplete>
    </v-col>
    <v-col cols="12" md="6">
      <v-text-field
        label="Estimated number of kids"
        type="number"
        v-model="e.EstimatedKids"
        :rules="[rules.required(e.EstimatedKids, 'Number')]"
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
      childCareSelectAll: false,
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
    defaultChildcareTime() {
      if (this.e.StartTime && !this.e.StartTime.includes('null')) {
        let time = moment(this.e.StartTime, "hh:mm A");
        return time.subtract(15, "minutes").format("hh:mm A");
      }
      return null;
    },
  },
  methods: {
    toggleChildCareOptions() {
      this.childCareSelectAll = !this.childCareSelectAll;
      if (this.childCareSelectAll) {
        this.e.ChildCareOptions = [
          "Infant/Toddler",
          "Preschool",
          "K-2nd",
          "3-5th",
        ]
        this.$refs.childcareOptRef.blur()
      } else {
        this.e.ChildCareOptions = []
      }
    },
    prefillSection() {
      this.dialog = false
      let idx = this.request.EventDates.indexOf(this.prefillDate)
      let currIdx = this.request.EventDates.indexOf(this.e.EventDate)
      this.$emit('updatechildcare', { targetIdx: idx, currIdx: currIdx })
    }
  }
}