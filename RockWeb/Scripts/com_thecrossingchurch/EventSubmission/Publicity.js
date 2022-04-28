import datePicker from '/Scripts/com_thecrossingchurch/EventSubmission/DatePicker.js';
export default {
  template: 
  `
  <div>
    <v-row>
      <v-col>
        <h3 class="primary--text">Publicity Information</h3>
      </v-col>
    </v-row>
    <v-form ref="pubForm" v-model="valid">
      <v-row>
        <v-col>
          <v-textarea
            label="Describe why someone should attend your event, what they will gain from your event, and any additional information that will help publicize your event."
            v-model="request.WhyAttendSixtyFive"
            :hint="WhyAttendSixtyFiveHint"
            :rules="[rules.required(request.WhyAttendSixtyFive, 'This field'), rules.publicityCharacterLimit(request.WhyAttendSixtyFive, 450)]"
          ></v-textarea>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-select
            label="Who are you targeting with this event/class?"
            v-model="request.TargetAudience"
            :items="[{text: 'Top of the Funnel Event/Class', desc: 'People who have not attended past ministry events'}, {text: 'Middle of the Funnel Event/Class', desc: 'People who are currently attending ministry events'}, {text: 'Bottom of the Funnel Event/Class', desc: 'Leaders/Super fans of your ministry events'}]"
            :rules="[rules.required(request.TargetAudience, 'Target Audience')]"
            item-value="text"
            attach
          >
            <template v-slot:item="data">
              <v-list-item-content>
                <v-list-item-title>{{data.item.text}}</v-list-item-title>
                <v-list-item-subtitle>{{data.item.desc}}</v-list-item-subtitle>
              </v-list-item-content>
            </template>
          </v-select>
        </v-col>
        <v-col>
          <v-switch
            :label="IsEventStickyLabel"
            hint="i.e. NewComers, Small Group Preview, Discovery Class or Serving at Church"
            persistent-hint
            v-model="request.EventIsSticky"
          ></v-switch>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <date-picker
            v-model="request.PublicityStartDate"
            label="What is the earliest date you are comfortable advertising your event?"
            :rules="[rules.needsEventDates(request.EventDates), rules.required(request.PublicityStartDate, 'Date'), rules.publicityStartDate(request.Status, request.SubmittedOn, request.PublicityStartDate, request.PublicityEndDate)]"
            :min="earliestPubDate"
            :max="latestStartPubDate"
            :show-current="earliestPubDate"
            :date="request.PublicityStartDate"
            :clearable="true"
          ></date-picker>
        </v-col>
        <v-col>
          <date-picker
            v-model="request.PublicityEndDate"
            label="What is the latest date you are comfortable advertising your event?"
            :rules="[rules.needsEventDates(request.EventDates), rules.required(request.PublicityEndDate, 'Date'), rules.publicityEndDate(request.EventDates, request.Status, request.SubmittedOn, request.PublicityEndDate, request.PublicityStartDate)]"
            :min="earliestEndPubDate"
            :max="latestPubDate"
            :date="request.PublicityEndDate"
            :clearable="true"
          ></date-picker>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" md="6">
          <v-select
            label="What publicity strategies are you interested in implementing for your event/class?"
            :items="pubStrategyOptions"
            v-model="request.PublicityStrategies"
            attach
            multiple
            :rules="[rules.required(request.PublicityStrategies, 'Publicity Strategy')]"
          ></v-select>
        </v-col>
      </v-row>
    </v-form>
  </div>
  `,
  props: ["request", "earliestPubDate"],
  data: function () {
    return {
      valid: true,
      rules: {
          required(val, field) {
              return !!val || `${field} is required`;
          },
          requiredArr(val, field) {
              return val.length > 0 || `${field} is required`;
          },
          publicityWordLimit(text, limit) {
              if (text) {
                  let arr = text.split(' ')
                  if (arr.length > limit) {
                      return `Please limit yourself to ${limit} words`
                  }
              }
              return true
          },
          publicityCharacterLimit(text, limit) {
              if (text) {
                  if (text.length > limit) {
                      return `Please limit yourself to ${limit} characters`
                  }
              }
              return true
          },
          publicityStartDate(reqStatus, submittedDate, startDate, endDate) {
            let subDate = moment(submittedDate)
            if(reqStatus == 'Draft') {
              subDate = moment()
            }
            if(endDate) {
              let pubSpan = moment(endDate).diff(moment(startDate), 'days')
              if (pubSpan < 21) {
                  return 'Publicity start date must be at least 21 days before publicity end.'
              }
            }
            let subSpan = moment(startDate).diff(moment(subDate), 'days') + 1
            if(subSpan < 21) {
              return `Publicity start date must be at least 21 days after submission date (${subDate.format('MM/DD/yyyy')}).`
            }
            return true
          },
          publicityEndDate(eventDates, reqStatus, submittedDate, endDate, startDate) {
              let dates = eventDates.map(d => moment(d))
              let minDate = moment.max(dates).subtract(1, 'days')
              if (moment(endDate).isAfter(minDate)) {
                  return 'Publicity cannot end after event.'
              }
              let span = moment(endDate).diff(moment(startDate), 'days') + 1
              if (span < 21) {
                  return 'Publicity end date must be at least 21 days after publicity start.'
              }
              let subDate = moment(submittedDate)
              if(reqStatus == 'Draft') {
                subDate = moment()
              }
              let subSpan = moment(endDate).diff(moment(subDate), 'days') + 1
              if(subSpan < 42) {
                return `Publicity end date must be at least 6 weeks after submission date (${subDate.format('MM/DD/yyyy')}).`
              }
              return true
          },
          needsEventDates(eventDates) {
            if(!eventDates || eventDates.length == 0) {
              return "You must select the dates of your event before selecting publicity dates"
            }
            return true
          }
      },
      pubStartMenu: false,
      pubEndMenu: false,
      googleCurrentKey: "",
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
    earliestEndPubDate() {
      let eDate = new moment();
      if (this.request.PublicityStartDate) {
        eDate = moment(this.request.PublicityStartDate).add(21, "days");
      } else {
        eDate = moment(this.earliestPubDate).add(21, "days");
      }
      return moment(eDate).format("yyyy-MM-DD");
    },
    latestStartPubDate() {
      let eDate = new moment();
      if (this.request.PublicityEndDate) {
        eDate = moment(this.request.PublicityEndDate).add(-21, "days");
      } else {
        eDate = moment(this.latestPubDate).add(-21, "days");
      }
      return moment(eDate).format("yyyy-MM-DD");
    },
    latestPubDate() {
      let sortedDates = this.request.EventDates.sort((a, b) => moment(a).diff(moment(b)))
      let eDate = new moment(sortedDates[sortedDates.length - 1]);
      eDate = moment(eDate).subtract(1, "days");
      return moment(eDate).format("yyyy-MM-DD");
    },
    pubStrategyOptions() {
      let ops = ['Social Media/Google Ads', 'Mobile Worship Folder']
      if (this.request.EventIsSticky) {
        ops.push('Announcement')
      }
      return ops
    },
    WhyAttendSixtyFiveHint() {
      return `(${this.request.WhyAttendSixtyFive.length}/450)`
    },
    IsEventStickyLabel() {
      return `Is your event a “sticky” event? (${this.boolToYesNo(this.request.EventIsSticky)})`
    },
    WhyAttendNinetyHint() {
      return `Be sure to write in the second person using rhetorical questions that elicit interest or touch a felt need. For example, “Have you ever wondered how Jesus would have dealt with depression and anxiety?” ${this.request.WhyAttendNinety.length}/90`
    },
    GoogleKeyHint() {
      return `Limited to 50 keys (${this.request.GoogleKeys.length}/50)`
    },
    WhyAttendTenHint() {
      return `Be sure to write in the second person using rhetorical questions that elicit interest or touch a felt need. For example, “Have you ever wondered how Jesus would have dealt with depression and anxiety?” ${this.request.WhyAttendTen.length}/65`
    },
    VisualIdeasHint() {
      return `${this.request.VisualIdeas.length}/300`
    },
    WhyAttendTwentyHint() {
      return `Be sure to write in the second person using rhetorical questions that elicit interest or touch a felt need. For example, “Have you ever wondered how Jesus would have dealt with depression and anxiety?” (${this.request.WhyAttendTwenty.length}/175)`
    }
  },
  methods: {
    boolToYesNo(val) {
      if (val) {
        return "Yes";
      }
      return "No";
    },
    GoogleKey(idx) {
      return `google_${idx}`
    },
    StoryKey(idx) {
      return `story_${idx}`
    },
    addGoogleKey(key) {
      if (this.googleCurrentKey) {
        this.request.GoogleKeys.push(this.googleCurrentKey)
        this.googleCurrentKey = ''
      }
    },
    removeGoogleKey(idx) {
      this.request.GoogleKeys.splice(idx, 1)
    },
  },
  components: {
    'date-picker' : datePicker
  }
}