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
            label="In 450 characters or less, describe why someone should attend your event and what they will learn/receive."
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
          <v-menu
            v-model="pubStartMenu"
            :close-on-content-click="false"
            :nudge-right="40"
            transition="scale-transition"
            offset-y
            min-width="290px"
            attach
          >
            <template v-slot:activator="{ on, attrs }">
              <v-text-field
                v-model="request.PublicityStartDate"
                label="What is the earliest date you are comfortable advertising your event?"
                prepend-inner-icon="mdi-calendar"
                readonly
                v-bind="attrs"
                v-on="on"
                clearable
                :rules="[rules.required(request.PublicityStartDate, 'Date')]"
              ></v-text-field>
            </template>
            <v-date-picker
              v-model="request.PublicityStartDate"
              @input="pubStartMenu = false"
              :min="earliestPubDate"
              :show-current="earliestPubDate"
              :from-date="earliestPubDate"
            ></v-date-picker>
          </v-menu>
        </v-col>
        <v-col>
          <v-menu
            v-model="pubEndMenu"
            :close-on-content-click="false"
            :nudge-right="40"
            transition="scale-transition"
            offset-y
            min-width="290px"
            attach
          >
            <template v-slot:activator="{ on, attrs }">
              <v-text-field
                v-model="request.PublicityEndDate"
                label="What is the latest date you are comfortable advertising your event?"
                prepend-inner-icon="mdi-calendar"
                readonly
                v-bind="attrs"
                v-on="on"
                :rules="[rules.required(request.PublicityEndDate, 'Date'), rules.publicityEndDate(request.EventDates, request.PublicityEndDate, request.PublicityStartDate)]"
                clearable
              ></v-text-field>
            </template>
            <v-date-picker
              v-model="request.PublicityEndDate"
              @input="pubEndMenu = false"
              :min="earliestEndPubDate"
              :show-current="earliestEndPubDate"
              :max="latestPubDate"
            >
              <span style="width: 290px; text-align: center; font-size: 12px;" v-if="!request.EventDates || request.EventDates.length == 0">
                Please select dates for your event to calculate the possible end dates
              </span>
            </v-date-picker>
          </v-menu>
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
      <template v-if="request.PublicityStrategies.includes('Social Media/Google Ads')">
        <v-row>
          <v-col>
            <i><strong style="font-size: 16px;">As a reminder the information you are filling out below is a request for Social Media/Google Ads. The Communication Manager will provide further direction and strategy.</strong></i>
          </v-col>
        </v-row>
        <v-row>
          <v-col>
            <v-textarea
              label="In 90 characters or less, describe why someone should attend your event."
              v-model="request.WhyAttendNinety"
              :rules="[rules.required(request.WhyAttendNinety, 'This field'), rules.publicityCharacterLimit(request.WhyAttendNinety, 90)]"
              :hint="WhyAttendNinetyHint"
            ></v-textarea>
          </v-col>
        </v-row>
        <v-row>
          <v-col>
            <label class="v-label theme--light">Which words, phrases, and questions would you like your event to trigger when someone searches on Google?</label>
            <v-chip-group>
              <v-chip
                v-for="(key, idx) in request.GoogleKeys"
                :key="GoogleKey(idx)"
                close
                @click:close="removeGoogleKey(idx)"
                close-icon="mdi-delete"
              >
                {{key}}
              </v-chip>
            </v-chip-group>
            <v-text-field
              label="Type a word or phrase here, then hit the 'Enter' key to add it to your list"
              v-model="googleCurrentKey"
              @keydown.enter="addGoogleKey"
              :disabled="request.GoogleKeys.length >= 50"
              :hint="GoogleKeyHint"
              persistent-hint
            ></v-text-field>
          </v-col>
        </v-row>
      </template>
      <template v-if="request.PublicityStrategies.includes('Mobile Worship Folder')">
        <v-row>
          <v-col>
            <i><strong style="font-size: 16px;">As a reminder the information you are filling out below is a request for Mobile Worship Folder. The Communication Manager will provide further direction and strategy.</strong></i>
          </v-col>
        </v-row>
        <v-row>
          <v-col>
            <v-text-field
              label="In 65 characters or less, describe why someone should attend your event."
              v-model="request.WhyAttendTen"
              :hint="WhyAttendTenHint"
              :rules="[rules.required(request.WhyAttendTen, 'This field'), rules.publicityCharacterLimit(request.WhyAttendTen, 65)]"
            ></v-text-field>
          </v-col>
        </v-row>
        <v-row>
          <v-col>
            <v-textarea
              label="In terms of graphic design, do you have any specific ideas regarding imagery, symbols, or any other visual elements to help guide our graphic designer?"
              v-model="request.VisualIdeas"
              :hint="VisualIdeasHint"
              :rules="[rules.publicityCharacterLimit(request.VisualIdeas, 300)]"
            ></v-textarea>
          </v-col>
        </v-row>
      </template>
      <template v-if="request.PublicityStrategies.includes('Announcement')">
        <v-row>
          <v-col>
            <strong style="font-size: 16px;">Please give the name and email of 1-3 people who have benefited from this in the past. Write a 1 paragraph description of their involvement and experience.</strong>
          </v-col>
        </v-row>
        <v-row v-for="(s, idx) in request.Stories" :key="StoryKey(idx)">
          <v-col>
            <v-text-field
              label="Name"
              v-model="s.Name"
            ></v-text-field>
          </v-col>
          <v-col>
            <v-text-field
              label="Email"
              v-model="s.Email"
            ></v-text-field>
          </v-col>
          <v-col cols="12">
            <v-textarea
              label="Description of their involvement and experience."
              v-model="s.Description"
            ></v-textarea>
          </v-col>
        </v-row>
        <v-row>
          <v-col>
            <v-btn color="accent" :disabled="request.Stories.length == 3" @click="request.Stories.push({Name:'', Email: '', Description: ''})">Add Person</v-btn>
          </v-col>
        </v-row>
        <v-row>
          <v-col>
            <v-textarea
              label="In 175 characters or less, describe why someone should attend your event."
              v-model="request.WhyAttendTwenty"
              :rules="[rules.publicityCharacterLimit(request.WhyAttendTwenty, 175)]"
              :hint="WhyAttendTwentyHint"
            ></v-textarea>
          </v-col>
        </v-row>
      </template>
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
          publicityEndDate(eventDates, endDate, startDate) {
              let dates = eventDates.map(d => moment(d))
              let minDate = moment.max(dates).subtract(1, 'days')
              if (moment(endDate).isAfter(minDate)) {
                  return 'Publicity cannot end after event.'
              }
              let span = moment(endDate).diff(moment(startDate), 'days')
              if (span < 21) {
                  return 'Publicity end date must be at least 21 days after publicity start.'
              }
              return true
          },
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
      return `Be sure to write in the second person using rhetorical questions that elicit interest or touch a felt need. For example, “Have you ever wondered how Jesus would have dealt with depression and anxiety?” (${this.request.WhyAttendSixtyFive.length}/450)`
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
  }
}