import { defineComponent, PropType } from "vue";
import { DefinedValueBag } from "@Obsidian/ViewModels/Entities/definedValueBag";
import { SubmissionFormBlockViewModel } from "../submissionFormBlockViewModel";
import Toggle from "./toggle";
import { DateTime, Duration } from "luxon";


export default defineComponent({
    name: "EventForm.Components.ResourceSwitches",
    components: {
        "tcc-switch": Toggle
    },
    props: {
        viewModel: {
            type: Object as PropType<SubmissionFormBlockViewModel>,
            required: false
        }
    },
    setup() {
    },
    data() {
        return {

        };
    },
    methods: {
        findTense(numDays: any): String {
            if (this.viewModel?.request.attributeValues) {
                let av = this.viewModel?.request?.attributeValues.EventDates
                if (av) {
                    let dates = av?.split(",").map(d => d.trim())
                    if (dates && dates.length > 0) {
                        let today = DateTime.now()
                        let first = dates.map((i) => {
                            return DateTime.fromFormat(i, 'yyyy-MM-dd')
                        })?.sort().shift()?.minus({ days: numDays })
                        if (this.isFuneralRequest || (first && first.startOf("day") >= today.startOf("day"))) {
                            return 'is'
                        }
                        return 'was'
                    } 
                }
            }
            return 'is'
        },
        findDate(numDays: any): String {
          if (this.viewModel?.request.attributeValues) {
            let av = this.viewModel?.request?.attributeValues.EventDates
            let dates = av?.split(",").map(d => d.trim())
            if (dates && dates.length > 0) {
              let span = Duration.fromObject({ days: numDays })
              let first = dates.map((i) => {
                return DateTime.fromFormat(i, 'yyyy-MM-dd')
              })?.sort().shift()
              return first != null ? first.minus(span).toFormat("EEEE, MMMM d") : ''
            }
          }
          return ''
        },
        switchIsDisabled(tense: string, key: string) {
          if(!(this.viewModel?.isEventAdmin || this.viewModel?.isSuperUser || this.viewModel?.isRoomAdmin)) {
            return true
          }
          if(tense == 'was') {
            //We are past the date you can request new resources
            //Allow toggle if the original request has the resource
            if(this.viewModel?.originalRequest?.attributeValues) {
              let val = this.viewModel.originalRequest.attributeValues[key]
              return val == 'False'
            } 
            if(this.viewModel?.request?.attributeValues) {
              return this.viewModel?.request?.attributeValues[key] == 'False'
            }
            return true
          }
          return false
        }
    },
    computed: {
        isFuneralRequest() {
            if (this.viewModel?.request.attributeValues) {
                var av = (this.viewModel.request.attributeValues['Ministry'])
                if(av != '') {
                  let val = JSON.parse(av)
                  var min = this.viewModel.ministries.filter(m => {
                      // return m.guid == val.value
                  })[0] as DefinedValueBag
                  if (min?.value?.toLowerCase().includes("funeral")) {
                      return true
                  }
                }
            }
            return false
        },
        twoWeeksTense(): any {
            return this.findTense(14)
        },
        thirtyDaysTense(): any {
            return this.findTense(30)
        },
        sixWeeksTense(): any {
            return this.findTense(42)
        },
        twoWeeksBeforeEventStart(): any {
            return this.findDate(14)
        },
        thirtyDaysBeforeEventStart(): any {
            return this.findDate(30)
        },
        sixWeeksBeforeEventStart(): any {
            return this.findDate(42)
        },
        earliestPubDate() {
            let eDate = DateTime.now()
            if (this.viewModel && this.viewModel.request?.attributeValues) {
                let av = (this.viewModel.request.attributeValues['Status'])
                // if (this.viewModel.request?.id > 0 && av != 'Draft') {
                //     if (this.viewModel.request.startDateTime) {
                //         eDate = DateTime.fromFormat(this.viewModel.request.startDateTime, 'yyyy-MM-DD')
                //     }
                // }
                let span = Duration.fromObject({ days: 21 })
                eDate = eDate.plus(span)
                //Override for Funerals
                if (this.isFuneralRequest) {
                    eDate = DateTime.now()
                }
            }
            return eDate.toFormat('yyyy-MM-DD')
        },
    },
    watch: {
      'viewModel.request.attributeValues.NeedsOpsAccommodations': {
        handler(val) {
          if(val == 'True' && this.viewModel?.request?.attributeValues) {
            this.viewModel.request.attributeValues.NeedsSpace = "True"
          }
        }
      },
      'viewModel.request.attributeValues.NeedsSpace': {
        handler(val) {
          if(val == 'False' && this.viewModel?.request?.attributeValues) {
            this.viewModel.request.attributeValues.NeedsOpsAccommodations = "False"
          }
        }
      },
    },
    mounted() {

    },
    template: `
<h3>Let's Design Your Event</h3>
<i><b>Select all that apply</b></i>
<br />
<br />
<div class="row" style="padding-bottom:16px;">
  <div class="col col-xs-12">
    <tcc-switch
      v-model="viewModel.request.attributeValues.NeedsSpace"
      :label="viewModel.request.attributes.NeedsSpace.name"
      hint="If you need any doors unlocked for this event, please be sure to include Operations accommodations below. Selecting a physical space does not assume unlocked doors."
      :persistent-hint="viewModel.request.attributeValues.NeedsSpace == 'True'"
    ></tcc-switch>
  </div>
</div>
<div class="row" style="padding-bottom:16px;">
  <div class="col col-xs-12">
    <tcc-switch
      v-model="viewModel.request.attributeValues.NeedsCatering"
      :label="viewModel.request.attributes.NeedsCatering.name"
      :disabled="switchIsDisabled(twoWeeksTense,'NeedsCatering')"
      hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
      :persistent-hint="viewModel.request.attributeValues.NeedsCatering == 'True'"
    ></tcc-switch>
    <div class="date-warning" v-if="!isFuneralRequest && viewModel.request.attributeValues.EventDates && viewModel.request.attributeValues.EventDates?.split(',').length > 0 && viewModel.request.attributeValues.NeedsCatering == 'False'">
      The last possible date to request catering {{twoWeeksTense}} {{twoWeeksBeforeEventStart}}
    </div>
  </div>
</div>
<div class="row" style="padding-bottom:16px;">
  <div class="col col-xs-12">
    <tcc-switch
      v-model="viewModel.request.attributeValues.NeedsOpsAccommodations"
      :label="viewModel.request.attributes.NeedsOpsAccommodations.name"
      :disabled="switchIsDisabled(twoWeeksTense,'NeedsOpsAccommodations')"
      hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
      :persistent-hint="viewModel.request.attributeValues.NeedsOpsAccommodations == 'True'"
    ></tcc-switch>
    <div class="date-warning" v-if="!isFuneralRequest && viewModel.request.attributeValues.EventDates && viewModel.request.attributeValues.EventDates?.split(',').length > 0 && viewModel.request.attributeValues.NeedsOpsAccommodations == 'False'">
      The last possible date to request ops accommodations {{twoWeeksTense}} {{twoWeeksBeforeEventStart}}
    </div>
  </div>
</div>
<div class="row" style="padding-bottom:16px;">
  <div class="col col-xs-12 col-md-6">
    <tcc-switch
      v-model="viewModel.request.attributeValues.NeedsChildCare"
      :label="viewModel.request.attributes.NeedsChildCare.name"
      :disabled="switchIsDisabled(thirtyDaysTense,'NeedsChildCare')"
      hint="Requests involving childcare must be made at least 30 days in advance."
      :persistent-hint="viewModel.request.attributeValues.NeedsChildCare == 'True'"
    ></tcc-switch>
    <div class="date-warning" v-if="!isFuneralRequest && viewModel.request.attributeValues.EventDates && viewModel.request.attributeValues.EventDates?.split(',').length > 0 && viewModel.request.attributeValues.NeedsChildCare == 'False'">
      The last possible date to request childcare {{thirtyDaysTense}} {{thirtyDaysBeforeEventStart}}
    </div>
  </div>
  <div class="col col-xs-12 col-md-6" v-if="viewModel.request.attributeValues.NeedsChildCare == 'True'">
    <tcc-switch
      v-model="viewModel.request.attributeValues.NeedsChildCareCatering"
      :label="viewModel.request.attributes.NeedsChildCareCatering.name"
      :disabled="switchIsDisabled(twoWeeksTense,'NeedsChildCareCatering')"
      hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
      :persistent-hint="viewModel.request.attributeValues.NeedsChildCareCatering == 'True'"
    ></tcc-switch>
    <div class="date-warning" v-if="!isFuneralRequest && viewModel.request.attributeValues.EventDates && viewModel.request.attributeValues.EventDates?.split(',').length > 0 && viewModel.request.attributeValues.NeedsChildCareCatering == 'False'">
      The last possible date to request catering for childcare {{twoWeeksTense}} {{twoWeeksBeforeEventStart}}
    </div>
  </div>
</div>
<div class="row" style="padding-bottom:16px;">
  <div class="col col-xs-12">
    <tcc-switch
      v-model="viewModel.request.attributeValues.NeedsRegistration"
      :label="viewModel.request.attributes.NeedsRegistration.name"
      :disabled="switchIsDisabled(twoWeeksTense,'NeedsRegistration')"
      hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
      :persistent-hint="viewModel.request.attributeValues.NeedsRegistration == 'True'"
    ></tcc-switch>
    <div class="date-warning" v-if="!isFuneralRequest && viewModel.request.attributeValues.EventDates && viewModel.request.attributeValues.EventDates?.split(',').length > 0 && viewModel.request.attributeValues.NeedsRegistration == 'False'">
      The last possible date to request registration {{twoWeeksTense}} {{twoWeeksBeforeEventStart}}
    </div>
  </div>
</div>
<div class="row" style="padding-bottom:16px;">
  <div class="col col-xs-12">
    <tcc-switch
      v-model="viewModel.request.attributeValues.NeedsWebCalendar"
      :label="viewModel.request.attributes.NeedsWebCalendar.name"
      :disabled="switchIsDisabled(twoWeeksTense,'NeedsWebCalendar')"
      hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
      :persistent-hint="viewModel.request.attributeValues.NeedsWebCalendar == 'True'"
    ></tcc-switch>
    <div class="date-warning" v-if="!isFuneralRequest && viewModel.request.attributeValues.EventDates && viewModel.request.attributeValues.EventDates?.split(',').length > 0 && viewModel.request.attributeValues.NeedsWebCalendar == 'False'">
      The last possible date to request web calendar {{twoWeeksTense}} {{twoWeeksBeforeEventStart}}
    </div>
  </div>
</div>
<div class="row" style="padding-bottom:16px;">
  <div class="col col-xs-12">
    <tcc-switch
      v-model="viewModel.request.attributeValues.NeedsPublicity"
      :label="viewModel.request.attributes.NeedsPublicity.name"
      :disabled="switchIsDisabled(sixWeeksTense,'NeedsPublicity')"
      hint="Requests involving publicity must be made at least 6 weeks in advance."
      :persistent-hint="viewModel.request.attributeValues.NeedsPublicity == 'True'"
    ></tcc-switch>
    <div class="date-warning" v-if="!isFuneralRequest && viewModel.request.attributeValues.EventDates && viewModel.request.attributeValues.EventDates?.split(',').length > 0 && viewModel.request.attributeValues.NeedsPublicity == 'False'">
      The last possible date to request publicity {{sixWeeksTense}} {{sixWeeksBeforeEventStart}}
    </div>
  </div>
</div>
<div class="row" style="padding-bottom:16px;">
  <div class="col col-xs-12">
    <tcc-switch
      v-model="viewModel.request.attributeValues.NeedsProductionAccommodations"
      :label="viewModel.request.attributes.NeedsProductionAccommodations.name"
      :disabled="switchIsDisabled(twoWeeksTense,'NeedsProductionAccommodations')"
      hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
      :persistent-hint="viewModel.request.attributeValues.NeedsProductionAccommodations == 'True'"
    ></tcc-switch>
    <div class="date-warning" v-if="!isFuneralRequest && viewModel.request.attributeValues.EventDates && viewModel.request.attributeValues.EventDates?.split(',').length > 0 && viewModel.request.attributeValues.NeedsProductionAccommodations == 'False'">
      The last possible date to request production accommodations {{twoWeeksTense}} {{twoWeeksBeforeEventStart}}
    </div>
  </div>
</div>
<div class="row" style="padding-bottom:16px;">
  <div class="col col-xs-12">
    <tcc-switch
      v-model="viewModel.request.attributeValues.NeedsOnline"
      :disabled="switchIsDisabled(twoWeeksTense, 'NeedsOnline')"
      :label="viewModel.request.attributes.NeedsOnline.name"
      hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
      :persistent-hint="viewModel.request.attributeValues.NeedsOnline == 'True'"
    ></tcc-switch>
    <div class="date-warning" v-if="!isFuneralRequest && viewModel.request.attributeValues.EventDates && viewModel.request.attributeValues.EventDates?.split(',').length > 0 && viewModel.request.attributeValues.NeedsOnline == 'False'">
      The last possible date to request zoom {{twoWeeksTense}} {{twoWeeksBeforeEventStart}}
    </div>
  </div>
</div>
<v-style>
  .date-warning {
    color: #CC3F0C !important;
    font-weight: bold !important;
    font-size: .85rem!important;
    letter-spacing: .13em!important;
    line-height: 2rem;
    text-transform: uppercase;
    padding-top: 8px;
  }
</v-style>
`
});
