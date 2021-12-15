import approvalField from '/Scripts/com_thecrossingchurch/EventSubmission/ApprovalField.js';
import utils from '/Scripts/com_thecrossingchurch/EventSubmission/Utilities.js';
export default {
  template: `
    <div>
      <h6 class='text--accent text-uppercase'>Publicity Information</h6>
      <v-row>
        <v-col>
          <div class="floating-title">Describe Why Someone Should Attend Your Event (450)</div>
          <template v-if="request.Changes != null && request.WhyAttendSixtyFive != request.Changes.WhyAttendSixtyFive">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="null" :idx="null" field="WhyAttendSixtyFive" :fieldname="formatFieldName('Describe Why Someone Should Attend Your Event (450)')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(request.WhyAttendSixtyFive ? request.WhyAttendSixtyFive : 'Empty' )}}: </span>
              <span class='primary--text'>{{(request.Changes.WhyAttendSixtyFive ? request.Changes.WhyAttendSixtyFive : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{request.WhyAttendSixtyFive}}
          </template>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <div class="floating-title">Target Audience</div>
          <template v-if="request.Changes != null && request.TargetAudience != request.Changes.TargetAudience">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="null" :idx="null" field="TargetAudience" :fieldname="formatFieldName('Target Audience')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(request.TargetAudience ? request.TargetAudience : 'Empty')}}: </span>
              <span class='primary--text'>{{(request.Changes.TargetAudience ? request.Changes.TargetAudience : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{request.TargetAudience}}
          </template>
        </v-col>
        <v-col>
          <div class="floating-title">Event is Sticky</div>
          <template v-if="request.Changes != null && request.EventIsSticky != request.Changes.EventIsSticky">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="null" :idx="null" field="EventIsSticky" :fieldname="formatFieldName('Event is Sticky')" :formatter="boolToYesNo" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(request.EventIsSticky != null ? boolToYesNo(request.EventIsSticky) : 'Empty')}}: </span>
              <span class='primary--text'>{{(request.Changes.EventIsSticky ? boolToYesNo(request.Changes.EventIsSticky) : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{boolToYesNo(request.EventIsSticky)}}
          </template>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <div class="floating-title">Publicity Start Date</div>
          <template v-if="request.Changes != null && request.PublicityStartDate != request.Changes.PublicityStartDate">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="null" :idx="null" field="PublicityStartDate" :fieldname="formatFieldName('Publicity Start Date')" :formatter="formatDate" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text' v-if="request.PublicityStartDate">{{request.PublicityStartDate | formatDate}}: </span>
              <span class='red--text' v-else>Empty: </span>
              <span class='primary--text' v-if="request.Changes.PublicityStartDate">{{request.Changes.PublicityStartDate | formatDate}}</span>
              <span class='primary--text' v-else>Empty</span>
            </template>
          </template>
          <template v-else>
            {{request.PublicityStartDate | formatDate}}
          </template>
        </v-col>
        <v-col>
          <div class="floating-title">Publicity End Date</div>
          <template v-if="request.Changes != null && request.PublicityEndDate != request.Changes.PublicityEndDate">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="null" :idx="null" field="PublicityEndDate" :fieldname="formatFieldName('Publicity End Date')" :formatter="formatDate" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text' v-if="request.PublicityEndDate">{{request.PublicityEndDate | formatDate}}: </span>
              <span class='red--text' v-else>Empty: </span>
              <span class='primary--text' v-if="request.Changes.PublicityEndDate">{{request.Changes.PublicityEndDate | formatDate}}</span>
              <span class='primary--text' v-else>Empty</span>
            </template>
          </template>
          <template v-else>
            {{request.PublicityEndDate | formatDate}}
          </template>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <div class="floating-title">Publicity Strategies</div>
          <template v-if="request.Changes != null && request.PublicityStrategies.toString() != request.Changes.PublicityStrategies.toString()">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="null" :idx="null" field="PublicityStrategies" :fieldname="formatFieldName('Publicity Strategies')" :formatter="formatList" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(request.PublicityStrategies ? request.PublicityStrategies.join(', ') : 'Empty')}}: </span>
              <span class='primary--text'>{{(request.Changes.PublicityStrategies ? request.Changes.PublicityStrategies.join(', ') : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{request.PublicityStrategies.join(', ')}}
          </template>
        </v-col>
      </v-row>
      <template v-if="request.PublicityStrategies.includes('Social Media/Google Ads')">
        <v-row>
          <v-col>
            <div class="floating-title">Describe Why Someone Should Attend Your Event (90)</div>
            <template v-if="request.Changes != null && request.WhyAttendNinety != request.Changes.WhyAttendNinety">
              <template v-if="approvalmode">
                <approval-field :request="selected" :e="null" :idx="null" field="WhyAttendNinety" :fieldname="formatFieldName('Describe Why Someone Should Attend Your Event (90)')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
              </template>
              <template v-else>
                <span class='red--text'>{{(request.WhyAttendNinety ? request.WhyAttendNinety : 'Empty')}}: </span>
                <span class='primary--text'>{{(request.Changes.WhyAttendNinety ? request.Changes.WhyAttendNinety : 'Empty')}}</span>
              </template>
            </template>
            <template v-else>
              {{request.WhyAttendNinety}}
            </template>
          </v-col>
        </v-row>
        <v-row>
          <template v-if="request.Changes != null && request.GoogleKeys.toString() != request.Changes.GoogleKeys.toString()">
            <v-col class='red--text'>
                <div class="floating-title">Google Keys</div>
                <ul>
                  <li v-for="k in request.GoogleKeys" :key="k">
                    {{k}}
                  </li>
                </ul>
              </v-col>
              <v-col class='primary--text'>
                <ul>
                  <li v-for="k in request.Changes.GoogleKeys" :key="k">
                    {{k}}
                  </li>
                </ul>
              </v-col>
            </template>
            <template v-else>
              <v-col>
                <div class="floating-title">Google Keys</div>
                <ul>
                  <li v-for="k in request.GoogleKeys" :key="k">
                    {{k}}
                  </li>
                </ul>
              </v-col>
          </template>
        </v-row>
      </template>
      <template v-if="request.PublicityStrategies.includes('Mobile Worship Folder')">
        <v-row>
          <v-col>
            <div class="floating-title">Describe Why Someone Should Attend Your Event (65)</div>
            <template v-if="request.Changes != null && request.WhyAttendTen != request.Changes.WhyAttendTen">
              <template v-if="approvalmode">
                <approval-field :request="selected" :e="null" :idx="null" field="WhyAttendTen" :fieldname="formatFieldName('Describe Why Someone Should Attend Your Event (10)')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
              </template>
              <template v-else>
                <span class='red--text'>{{(request.WhyAttendTen ? request.WhyAttendTen : 'Empty')}}: </span>
                <span class='primary--text'>{{(request.Changes.WhyAttendTen ? request.Changes.WhyAttendTen : 'Empty')}}</span>
              </template>
            </template>
            <template v-else>
              {{request.WhyAttendTen}}
            </template>
          </v-col>
          <v-col v-if="request.VisualIdeas != ''">
            <div class="floating-title">Visual Ideas for Graphic</div>
            <template v-if="request.Changes != null && request.VisualIdeas != request.Changes.VisualIdeas">
              <template v-if="approvalmode">
                <approval-field :request="selected" :e="null" :idx="null" field="VisualIdeas" :fieldname="formatFieldName('Visual Ideas for Graphic')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
              </template>
              <template v-else>
                <span class='red--text'>{{(request.VisualIdeas ? request.VisualIdeas : 'Empty')}}: </span>
                <span class='primary--text'>{{(request.Changes.VisualIdeas ? request.Changes.VisualIdeas : 'Empty')}}</span>
              </template>
            </template>
            <template v-else>
              {{request.VisualIdeas}}
            </template>
          </v-col>
        </v-row>
      </template>
      <template v-if="request.PublicityStrategies.includes('Announcement')">
        <v-row v-for="(s, sidx) in request.Stories" :key="storyKey(sidx)">
          <template v-if="request.Changes != null && request.Stories.toString() != request.Changes.Stories.toString()">
            <v-col class='red--text'>
              <div class="floating-title">Story {{sidx+1}}</div>
              {{s.Name}}, {{s.Email}} <br/>
              {{s.Description}}
            </v-col>
            <v-col class='primary--text'>
              <div class="floating-title">Story {{sidx+1}}</div>
              {{request.Changes.Stories[sidx].Name}}, {{request.Changes.Stories[sidx].Email}} <br/>
              {{request.Changes.Stories[sidx].Description}}
            </v-col>
          </template>
          <template v-else>
            <v-col>
              <div class="floating-title">Story {{sidx+1}}</div>
              {{s.Name}}, {{s.Email}} <br/>
              {{s.Description}}
            </v-col>
          </template>
        </v-row>
        <v-row>
          <v-col>
            <div class="floating-title">Describe Why Someone Should Attend Your Event (175)</div>
            <template v-if="request.Changes != null && request.WhyAttendTwenty != request.Changes.WhyAttendTwenty">
              <template v-if="approvalmode">
                <approval-field :request="selected" :e="null" :idx="null" field="WhyAttendTwenty" :fieldname="formatFieldName('Describe Why Someone Should Attend Your Event (175)')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
              </template>
              <template v-else>
                <span class='red--text'>{{(request.WhyAttendTwenty ? request.WhyAttendTwenty : 'Empty')}}: </span>
                <span class='primary--text'>{{(request.Changes.WhyAttendTwenty ? request.Changes.WhyAttendTwenty : 'Empty')}}</span>
              </template>
            </template>
            <template v-else>
              {{request.WhyAttendTwenty}}
            </template>
          </v-col>
        </v-row>
      </template>
    </div>
  `,
  props: ["request", "approvalmode"],
  data: function () {
      return {
        
      }
  },
  created: function () {
    
  },
  filters: {
    ...utils.filters
  },
  computed: {
    
  },
  methods: {
    storyKey(idx) {
      return `Story_${idx}`
    },
    approveChange(field) {
      this.$emit("approvechange", field)
    },
    denyChange(field) {
      this.$emit("denychange", field)
    },
    ...utils.methods
  },
  watch: {
    
  },
  components: {
    'approval-field': approvalField,
  }
}