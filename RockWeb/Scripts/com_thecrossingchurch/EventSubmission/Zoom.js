import utils from '/Scripts/com_thecrossingchurch/EventSubmission/Utilities.js';
export default {
  template: `
    <v-form ref="zoomForm" v-model="valid">
      <v-row>
        <v-col>
          <h3 class="primary--text" v-if="request.Events.length == 1">Zoom Information</h3>
          <h3 class="primary--text" v-else>
            Zoom Information 
            <v-btn rounded outlined color="accent" @click="prefillDate = ''; dialog = true; ">
              Prefill
            </v-btn>
          </h3>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-text-field
            label="If there is a link your attendees will need to access this event, list it here"
            v-model="e.EventURL"
            :rules="[rules.required(e.EventURL, 'Link')]"
          ></v-text-field>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-text-field
            label="If there is a password for the link, list it here"
            v-model="e.ZoomPassword"
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
  props: ["e", "request", "existing"],
  data: function () {
    return {
      valid: true,
      dialog: false,
      prefillDate: '',
      rules: {
        required(val, field) {
          return !!val || `${field} is required`;
        },
      }
    }
  },
  created: function () {
    
  },
  filters: {
    ...utils.filters, 
  },
  computed: {
    prefillOptions() {
      return this.request.EventDates.filter(i => i != this.e.EventDate)
    },
    
  },
  watch: {
    
  },
  methods: {
    ...utils.methods, 
    prefillSection() {
      this.dialog = false
      let idx = this.request.EventDates.indexOf(this.prefillDate)
      let currIdx = this.request.EventDates.indexOf(this.e.EventDate)
      this.$emit('updatezoom', { targetIdx: idx, currIdx: currIdx })
    },
    
  },
}
