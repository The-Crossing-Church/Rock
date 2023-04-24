import { defineComponent, provide } from "vue"
import { useConfigurationValues, useInvokeBlockAction } from "../../../Util/block"
import { Person } from "../../../ViewModels"
import { SubmissionFormBlockViewModel } from "./submissionFormBlockViewModel"
import { useStore } from "../../../Store/index"
import { Steps, Button, Modal, Select } from "ant-design-vue"
import { ListItem, DefinedValue, ContentChannelItem } from "../../../ViewModels"
import ResourceSwitches from "./Components/resourceSwitches"
import Space from "./Components/space"
import Online from "./Components/online"
import Catering from "./Components/catering"
import Childcare from "./Components/childcare"
import Ops from "./Components/opsAccommodations"
import Registration from "./Components/registration"
import Publicity from "./Components/publicity"
import WebCal from "./Components/webCal"
import ProdTech from "./Components/productionTech"
import BasicInfo from "./Components/basicInfo"
import CCCatering from "./Components/childcareCatering"
import EventTime from "./Components/eventTime"
import DatePicker from "./Components/datePicker"
import EventBuffer from "./Components/eventBuffer"
import { DateTime, Duration } from "luxon"
import RockLabel from "../../../Elements/rockLabel"
import RockField from "../../../Controls/rockField"
import rules from "./Rules/rules"

const store = useStore()
const { Step } = Steps
const { Option } = Select 
const categoryAttrs = [
  { attr: "NeedsSpace", cat: "Event Space" },
  { attr: "NeedsOnline", cat: "Event Online" },
  { attr: "NeedsCatering", cat: "Event Catering" },
  { attr: "NeedsChildCare", cat: "Event Childcare" },
  { attr: "NeedsChildCareCatering", cat: "Event Childcare Catering" },
  { attr: "NeedsOpsAccommodations", cat: "Event Ops Requests" },
  { attr: "NeedsPublicity", cat: "Event Production" },
  { attr: "NeedsProductionAccommodations", cat: "Event Publicity" },
  { attr: "NeedsRegistration", cat: "Event Registration" }
]

export default defineComponent({
  name: "EventForm.SubmissionForm",
  components: {
      "a-steps": Steps,
      "a-step": Step,
      "a-btn": Button,
      "a-modal": Modal,
      "a-select": Select,
      "a-select-option": Option,
      "tcc-resources": ResourceSwitches,
      "tcc-basic": BasicInfo,
      "tcc-space": Space,
      "tcc-online": Online,
      "tcc-catering": Catering,
      "tcc-childcare": Childcare,
      "tcc-childcare-catering": CCCatering,
      "tcc-ops": Ops,
      "tcc-registration": Registration,
      "tcc-publicity": Publicity,
      "tcc-web-cal": WebCal,
      "tcc-prod-tech": ProdTech,
      "tcc-event-time": EventTime,
      "tcc-buffer": EventBuffer,
      "tcc-date-pkr": DatePicker,
      "rck-lbl": RockLabel,
      "rck-field": RockField
  },
  setup() {
      const viewModel = useConfigurationValues<SubmissionFormBlockViewModel | null>();
      viewModel?.existing.forEach((e: any) => {
        e.childItems = viewModel.existingDetails.filter((d: any) => { return d.contentChannelItemId == e.id })
      })
      viewModel?.locationSetupMatrix?.forEach((l: any) => {
        l.matrixItems = viewModel.locationSetupMatrixItem.filter((mi: any) => { return mi.attributeMatrixId == l.id })
      })
      const invokeBlockAction = useInvokeBlockAction();
      
      /** A method to save a submission draft */
      const save: (viewModel: SubmissionFormBlockViewModel) => Promise<any> = async (viewModel) => {
          const response = await invokeBlockAction<{ expirationDateTime: string }>("Save", {
              viewModel: viewModel.request, events: viewModel.events
          });
          if (response) {
            return response
          }
      };
      provide("save", save);

      /** A method to submit a submission draft */
      const submit: (viewModel: SubmissionFormBlockViewModel) => Promise<any> = async (viewModel) => {
          const response = await invokeBlockAction<{ expirationDateTime: string }>("Submit", {
              viewModel: viewModel.request, events: viewModel.events
          });
          if (response) {
            return response
          }
      };
      provide("submit", submit);

      /** A method to reload the current request */
      const reload: (id: number) => Promise<any> = async (id) => {
          const response = await invokeBlockAction<{ expirationDateTime: string }>("ReloadRequest", {
              id: id
          });
          if (response) {
            return response
          }
      };
      provide("reload", reload);

      const addComment: (id: number, message: string) => Promise<any> = async (id, message) => {
        const response = await invokeBlockAction("AddComment", {
          id: id, message: message
        });
        return response
      }
      provide("addComment", addComment);

      return {
          viewModel,
          save,
          submit,
          addComment,
          reload
      }
  },
  data() {
      return {
          step: 0,
          modal: false,
          id: 0,
          response: {} as any,
          isSave: false,
          isSubmitting: false,
          resources: [] as string[],
          pagesViewed: [] as number[],
          requestErrors: [] as any[],
          changeDateModal: false,
          changeDateOriginal: "",
          changeDateReplacement: "",
          preFillModal: false,
          preFillModalOption: "",
          preFillTarget: "",
          preFillSource: "",
          rules: rules,
          readonlySections: [] as string[],
          toastMessage: "",
          toastIsError: true
      };
  },
  computed: {
      /** The person currently authenticated */
      currentPerson(): Person | null {
          return store.state.currentPerson;
      },
      selectedDates(): any {
        if(this.viewModel?.request.attributeValues) {
          return this.viewModel.request.attributeValues.EventDates.split(',')
        }
      },
      hasEventData(): boolean {
        if(this.viewModel?.request.attributeValues) {
          return this.viewModel.request.attributeValues.NeedsSpace == 'True' || this.viewModel.request.attributeValues.NeedsOnline == 'True' || this.viewModel.request.attributeValues.NeedsCatering == 'True' || this.viewModel.request.attributeValues.NeedsChildCare == 'True' || this.viewModel.request.attributeValues.NeedsOpsAccommodations == 'True' || this.viewModel.request.attributeValues.NeedsRegistration == 'True'
        }
        return false
      },
      publicityStep(): any {
        let step = 2
        if(this.viewModel?.request.attributeValues) {
          if(this.viewModel.request.attributeValues.IsSame == 'True') {
            if(this.hasEventData) {
              step++
            }
          } else {
            if(this.hasEventData) {
              step += this.viewModel.events.length
            }
          }
        }
        return step
      },
      lastStep(): any {
        let step = 1
        if(this.viewModel?.request.attributeValues?.NeedsSpace == "True" ||
          this.viewModel?.request.attributeValues?.NeedsOnline == "True" ||
          this.viewModel?.request.attributeValues?.NeedsCatering == "True" ||
          this.viewModel?.request.attributeValues?.NeedsChildCare == "True" ||
          this.viewModel?.request.attributeValues?.NeedsChildCareCatering == "True" ||
          this.viewModel?.request.attributeValues?.NeedsRegistration == "True" ||
          this.viewModel?.request.attributeValues?.NeedsOpsAccommodations == "True"
        ) {
          if(this.viewModel?.request.attributeValues?.IsSame == "True") {
            step++
          } else {
            step += this.viewModel?.events ? this.viewModel.events.length : 0
          }
        }
        if(this.viewModel?.request.attributeValues?.NeedsPublicity == "True" || this.viewModel?.request.attributeValues?.NeedsProductionAccommodations == "True" || this.viewModel?.request.attributeValues?.NeedsWebCalendar == "True") {
          step++
        }
        return step
      },
      canSubmit(): boolean {
        //actually opposite, since we are using this for the disabled prop. 
        let canSubmit = true
        if(this.viewModel?.request.title) {
          if(this.viewModel.request?.attributeValues?.EventDates) {
            if(this.viewModel.request?.attributeValues?.Ministry!= '{"value":"","text":"","description":""}') {
              canSubmit = false
            }
          }
        }
        return canSubmit
      },
      noTitle(): boolean {
        let hasTitle = false
        if(this.viewModel?.request?.title) {
          hasTitle = this.viewModel.request.title.trim() != ''
        }
        return !hasTitle
      },
      errorList(): string[] {
        let errors = [] as string[]
        this.requestErrors.forEach((error: any) => {
          error.errors.forEach((e: any) => {
            let idx = errors.indexOf(e.text)
            if(idx < 0) {
              errors.push(e.text)
            }
          })
        })
        return errors
      },
      minEventDate() {
        let date = DateTime.now()
        if(this.viewModel?.request?.attributeValues) {
          if(this.viewModel.request?.attributeValues.RequestStatus != "Draft" && this.viewModel.request?.attributeValues.RequestStatus != "Submitted" && this.viewModel.request?.attributeValues.RequestStatus != " In Progress") {
            let val = this.viewModel.request.startDateTime as string
            date = DateTime.fromISO(val)
          }
        }
        let span = Duration.fromObject({days: 0})
        if(this.viewModel) {
          if( this.viewModel.request?.attributeValues?.NeedsOnline == "True"
            || this.viewModel.request?.attributeValues?.NeedsRegistration == "True"
            || this.viewModel.request?.attributeValues?.NeedsWebCalendar == "True"
            || this.viewModel.request?.attributeValues?.NeedsCatering == "True"
            || this.viewModel.request?.attributeValues?.NeedsOpsAccommodations == "True"
            || this.viewModel.request?.attributeValues?.NeedsProductionAccommodations == "True"
          ) {
            span = Duration.fromObject({days: 14})
          }
          if(this.viewModel.request?.attributeValues?.NeedsChildCare == "True") {
            span = Duration.fromObject({days: 30})
          }
          if(this.viewModel.request?.attributeValues?.NeedsPublicity == "True") {
            span = Duration.fromObject({weeks: 6})
          }
          //Override restrictions for Funerals
          if(this.viewModel.request?.attributeValues?.Ministry) {
            let ministry = JSON.parse(this.viewModel.request?.attributeValues?.Ministry)
            if(ministry.text.toLowerCase().includes("funeral")) {
              span = Duration.fromObject({days: 0})
            }
          }
        }
        date = date.plus(span)
        return date.toFormat('yyyy-MM-dd')
      },
      toastClass() {
        if(this.toastIsError) {
          return 'toast-error'
        }
        return 'toast-success'
      }
  },
  methods: {
    matchMultiEvent() {
      if(this.viewModel && this.viewModel?.request.attributeValues && this.viewModel?.events && this.viewModel?.events.length > 0 && this.selectedDates) {
        let numDays = this.selectedDates.length
        //Remove Events not in Selected dates
        this.viewModel.events.forEach((e: any, idx: number) => {
          if(e.attributeValues.EventDate == "") {
            e.attributeValues.EventDate = this.selectedDates[0]
          } 
          if(!this.selectedDates.includes(e.attributeValues.EventDate)) {
            this.viewModel?.events.splice(idx, 1)
          }
        })
        this.selectedDates?.forEach((e: string, idx: number) => {
            let exists = this.viewModel?.events.filter(event => { return event.attributeValues?.EventDate == e })
            if (exists && exists.length > 0) {
              //We don't need to do anything because it already has a matching item
            } else {
              let t = JSON.parse(JSON.stringify(this.viewModel?.events[0]))
              t.attributeValues.EventDate = e
              t.id = 0
              this.viewModel?.events.push(t)
            }
        })
        this.viewModel.events = this.viewModel.events.sort((a: any, b: any) => {
          if(a.attributeValues && b.attributeValues) {
            let obj = DateTime.fromFormat(a.attributeValues.EventDate, "yyyy-MM-dd").diff(DateTime.fromFormat(b.attributeValues.EventDate, "yyyy-MM-dd"), 'days').toObject()
            if(obj.days) {
              if(obj.days < 0) {
                return -1
              } else if(obj.days > 0) {
                return 1
              }
            }
          }
          return 0
        })
      }
    },
    formatDate(date: string) {
      let dt = DateTime.fromFormat(date, 'yyyy-MM-dd')
      if(!dt.isValid) {
        dt = DateTime.fromISO(date)
      }
      return dt.toFormat("MM/dd/yyyy")
    },
    saveDraft() {
      if(this.viewModel) {
        this.validate()
        this.save(this.viewModel).then((res: any) => {
          if(res.isSuccess){
            this.id = res?.data?.id
            this.response = res.data
            this.isSave = true
            this.modal = true
            if(this.viewModel?.request) {
              this.viewModel.request.id = res.data.id
            }
          } else if (res.isError) {

          }
        })
      }
    },
    submitRequest() {
      let el = document.getElementById('updateProgress')
      if(el) {
        el.style.display = 'block'
      }
      this.isSubmitting = true
      if(this.viewModel) {
        this.validate()
        this.submit(this.viewModel).then((res: any) => {
          if(res) {
            if(res.isSuccess){
              this.id = res?.data?.id
              this.response = res.data
              this.isSave = false
              this.modal = true
              if(this.viewModel?.request) {
                this.viewModel.request.id = res?.data?.id
              }
            } else if (res.isError || res.Message) {
              this.toastIsError = true
              this.toastMessage = res.errorMessage ? res.errorMessage : res.Message
              let el = document.getElementById('toast')
              el?.classList.add("show")
            }
          }
          this.isSubmitting = false
        }).catch((err: any) => {
          console.log('catch error')
          if(err.Message) {
            this.toastIsError = true
            this.toastMessage = err.Message
            let el = document.getElementById('toast')
            el?.classList.add("show")
          }
        }).finally(() => {
          if(el) {
            el.style.display = 'none'
          }
        })
      }
    },
    continueEdit() {
      let url = window.location.href
      if(!url.includes("?Id")) {
        url += "?Id=" + this.id
      }
      window.location.assign(url)
    },
    openInDashboard() {
      let url = ""
      if(this.viewModel?.isEventAdmin) {
        if(this.viewModel.adminDashboardURL) {
          url = "https://" + window.location.host + (this.viewModel.adminDashboardURL.includes("/") ? "" : "/") + this.viewModel.adminDashboardURL + "?Id=" + this.id
        }
      } else {
        if(this.viewModel?.userDashboardURL) {
          url = "https://" + window.location.host + (this.viewModel.userDashboardURL.includes("/") ? "" : "/") + this.viewModel.userDashboardURL + "?Id=" + this.id
        }
      }
      if(url) {
        window.location.assign(url)
      }
    },
    clearEventDetailsData(category: string) {
      if(this.viewModel?.events) {
        this.viewModel?.events.forEach((event: any) => {
          for(let key in event.attributes) {
            let attr = event.attributes[key]
            let categories = attr.categories.map((c: any) => { return c.name })
            if(categories.length == 1 && categories.includes(category)) {
              event.attributeValues[key] = ""
            } else if(categories.length > 1 && categories.includes(category)) {
              let otherCategories = categories.filter((c: string) => { return c != category })
              let catAttr = categoryAttrs.filter((ca: any) => { return otherCategories.includes(ca.cat) })
              let allOtherFalse = true
              catAttr.forEach((ca: any) => {
                if(this.viewModel?.request?.attributeValues && this.viewModel?.request?.attributeValues[ca.attr] == 'True') {
                  allOtherFalse = false
                }
              })
              if(allOtherFalse) {
                event.attributeValues[key] = ""
              }
            }
          }
        })
      }
    },
    clearEventData(category: string) {
      if(this.viewModel?.request?.attributes && this.viewModel?.request?.attributeValues) {
        for(let key in this.viewModel.request.attributes) {
          let attr = this.viewModel.request.attributes[key]
          if(attr.categories.map((c: any) => { return c.name }).includes(category)) {
            this.viewModel.request.attributeValues[key] = ""
          }
        }
      }
    },
    next() {
      this.pagesViewed.push(this.step)
      this.step++
      window.scrollTo(0, 0)
    },
    prev() {
      this.pagesViewed.push(this.step)
      this.step--
      window.scrollTo(0, 0)
    },
    jumpTo(s: number) {
      this.pagesViewed.push(this.step)
      this.step = s
      window.scrollTo(0, 0)
    },
    getRefName(name: string, idx: number) {
      return `${name}_${idx}`
    },
    hideToast() {
      let el = document.getElementById('toast')
      el?.classList.remove("show")
      el = document.getElementById('toastSuccess')
      el?.classList.remove("show")
    },
    validate() {
      let requestIsValid = true
      let invalidSections = [] as string[]
      if(this.viewModel?.request && this.viewModel.request.attributeValues) {
        if(this.rules.required(this.viewModel.request.title, '') != true ||
          this.rules.required(this.viewModel.request.attributeValues.Contact, '') != true ||
          this.rules.required(this.viewModel.request.attributeValues.Ministry, '') != true ||
          this.rules.required(this.viewModel.request.attributeValues.EventDates, '') != true
        ) {
          requestIsValid = false
        }

        //Fields on Event 
        let submittedDate = DateTime.now()
        if(this.viewModel.request?.attributeValues?.RequestStatus != 'Draft') {
          if(this.viewModel.request?.startDateTime) {
            submittedDate = DateTime.fromISO(this.viewModel.request?.startDateTime)
          }
        }
        let dates = this.viewModel.request.attributeValues.EventDates.split(',').map((d) => DateTime.fromFormat(d.trim(), 'yyyy-MM-dd')).sort()
        let firstDate = dates[0]
        let lastDate = dates[dates.length - 1]
        if(this.viewModel.request.attributeValues.NeedsPublicity == 'True') {
          let minStart = submittedDate.plus({weeks: 3})
          let minPubStartDate = minStart.toFormat("yyyy-MM-dd")
          let maxPubStartDate = firstDate.minus({weeks: 3}).toFormat("yyyy-MM-dd")
          let minPubEndDate = DateTime.fromFormat(this.viewModel.request.attributeValues.PublicityStartDate, 'yyyy-MM-dd').plus({weeks: 3}).toFormat("yyyy-MM-dd")
          let maxPubEndDate = lastDate.toFormat("yyyy-MM-dd")
          if(this.rules.required(this.viewModel.request.attributeValues?.WhyAttend, '') != true ||
            this.rules.required(this.viewModel.request.attributeValues?.TargetAudience, '') != true ||
            this.rules.required(this.viewModel.request.attributeValues?.PublicityStartDate, '') != true ||
            this.rules.pubStartIsValid(this.viewModel.request.attributeValues?.PublicityStartDate, this.viewModel.request.attributeValues?.PublicityEndDate, minPubStartDate, maxPubStartDate) != true ||
            this.rules.required(this.viewModel.request.attributeValues?.PublicityEndDate, '') != true ||
            this.rules.pubEndIsValid(this.viewModel.request.attributeValues?.PublicityEndDate, this.viewModel.request.attributeValues?.PublicityStartDate, this.viewModel.request.attributeValues.EventDates, minPubEndDate, maxPubEndDate) != true ||
            this.rules.required(this.viewModel.request.attributeValues?.PublicityStrategies, '') != true
          ) {
            requestIsValid = false
            let idx = invalidSections.indexOf('Publicity')
            if(idx < 0) {
              invalidSections.push('Publicity')
            }
          }
        }
        if(this.viewModel.request.attributeValues.NeedsWebCalendar == 'True') {
          if(this.rules.required(this.viewModel.request.attributeValues?.WebCalendarDescription, '') != true ) {
            requestIsValid = false
            let idx = invalidSections.indexOf('Calendar')
            if(idx < 0) {
              invalidSections.push('Calendar')
            }
          }
        }
        if(this.viewModel.request.attributeValues.NeedsProductionAccommodations == 'True') {
          if(this.rules.required(this.viewModel.request.attributeValues?.ProductionTech, '') != true ||
            this.rules.required(this.viewModel.request.attributeValues?.ProductionSetup, '') != true
          ) {
            requestIsValid = false
            let idx = invalidSections.indexOf('Production')
            if(idx < 0) {
              invalidSections.push('Production')
            }
          }
        }

        //Fields on Event Details
        if(this.viewModel?.events && this.viewModel.events.length > 0) {
          for(let i=0; i < this.viewModel.events.length; i++) {
            let eventIsValid = true
            if(this.rules.required(this.viewModel.events[i].attributeValues?.StartTime, '') != true ||
              this.rules.required(this.viewModel.events[i].attributeValues?.EndTime, '') != true ||
              this.rules.timeIsValid(this.viewModel.events[i].attributeValues?.StartTime as string, this.viewModel.events[i].attributeValues?.EndTime as string, true) != true 
            ) {
              requestIsValid = false
              eventIsValid = false
            }
            if(this.viewModel.request.attributeValues.NeedsSpace == 'True') {
              let attendance = this.viewModel.events[i].attributeValues?.ExpectedAttendance as string
              let numAttendance = parseInt(attendance)
              let rooms = this.viewModel.events[i].attributeValues?.Rooms as string
              if(this.rules.required(this.viewModel.events[i].attributeValues?.Rooms, '') != true ||
                this.rules.required(this.viewModel.events[i].attributeValues?.ExpectedAttendance, '') != true ||
                this.rules.attendance(numAttendance, rooms, this.viewModel.locations, '') != true
              ) {
                requestIsValid = false
                eventIsValid = false
                let idx = invalidSections.indexOf('Space')
                if(idx < 0) {
                  invalidSections.push('Space')
                }
              }
            }
            if(this.viewModel.request.attributeValues.NeedsOnline == 'True') {
              if(this.rules.required(this.viewModel.events[i].attributeValues?.EventURL, '') != true) {
                requestIsValid = false
                eventIsValid = false
                let idx = invalidSections.indexOf('Online')
                if(idx < 0) {
                  invalidSections.push('Online')
                }
              }
            }
            if(this.viewModel.request.attributeValues.NeedsCatering == 'True') {
              let drinkTime = this.viewModel.events[i].attributeValues?.DrinkTime as string
              let foodTime = this.viewModel.events[i].attributeValues?.FoodTime as string
              let endTime = this.viewModel.events[i].attributeValues?.EndTime as string
              let drinks = this.viewModel.events[i].attributeValues?.Drinks as string
              if(this.rules.required(this.viewModel.events[i].attributeValues?.PreferredVendor, '') != true ||
                this.rules.required(this.viewModel.events[i].attributeValues?.FoodBudgetLine, '') != true ||
                this.rules.required(this.viewModel.events[i].attributeValues?.PreferredMenu, '') != true ||
                this.rules.required(this.viewModel.events[i].attributeValues?.FoodTime, '') != true ||
                this.rules.timeCannotBeAfterEvent(foodTime, endTime, '') != true ||
                this.rules.drinkTimeRequired(drinkTime, drinks, '') != true ||
                (this.viewModel.events[i].attributeValues?.NeedsDelivery == 'True' && this.rules.required(this.viewModel.events[i].attributeValues?.FoodSetupLocation, '') != true)
              ) {
                requestIsValid = false
                eventIsValid = false
                let idx = invalidSections.indexOf('Catering')
                if(idx < 0) {
                  invalidSections.push('Catering')
                }
              }
            }
            if(this.viewModel.request.attributeValues.NeedsChildCare == 'True') {
              let ccStartTime = this.viewModel.events[i].attributeValues?.ChildcareStartTime as string
              let endTime = this.viewModel.events[i].attributeValues?.EndTime as string
              if(this.rules.required(this.viewModel.events[i].attributeValues?.ChildcareStartTime, '') != true ||
                this.rules.timeCannotBeAfterEvent(ccStartTime, endTime, '') != true ||
                this.rules.required(this.viewModel.events[i].attributeValues?.ChildcareEndTime, '') != true ||
                this.rules.required(this.viewModel.events[i].attributeValues?.ChildcareOptions, '') != true ||
                this.rules.required(this.viewModel.events[i].attributeValues?.EstimatedNumberofKids, '') != true
              ) {
                requestIsValid = false
                eventIsValid = false
                let idx = invalidSections.indexOf('Childcare')
                if(idx < 0) {
                  invalidSections.push('Childcare')
                }
              }
            }
            if(this.viewModel.request.attributeValues.NeedsChildCareCatering == 'True') {
              let ccFoodTime = this.viewModel.events[i].attributeValues?.ChildcareFoodTime as string
              let endTime = this.viewModel.events[i].attributeValues?.EndTime as string
              if(this.rules.required(this.viewModel.events[i].attributeValues?.ChildcareVendor, '') != true ||
                this.rules.required(this.viewModel.events[i].attributeValues?.ChildcareCateringBudgetLine, '') != true ||
                this.rules.required(this.viewModel.events[i].attributeValues?.ChildcarePreferredMenu, '') != true ||
                this.rules.required(this.viewModel.events[i].attributeValues?.ChildcareFoodTime, '') != true ||
                this.rules.timeCannotBeAfterEvent(ccFoodTime, endTime, '') != true
              ) {
                requestIsValid = false
                eventIsValid = false
                let idx = invalidSections.indexOf('Childcare Catering')
                if(idx < 0) {
                  invalidSections.push('Childcare Catering')
                }
              }
            }
            if(this.viewModel.request.attributeValues.NeedsRegistration == 'True') {
              let regStartDate = this.viewModel.events[i].attributeValues?.RegistrationStartDate as string
              let regEndDate = this.viewModel.events[i].attributeValues?.RegistrationEndDate as string
              let lastDate = this.viewModel.events[i].attributeValues?.EventDate as string
              if(lastDate == '') {
                let dates = this.viewModel.request.attributeValues.EventDates.split(",").map((d: string) => d.trim())
                if(dates && dates.length > 0) {
                  lastDate == dates[dates.length - 1]
                }
              }
              if(this.rules.required(this.viewModel.events[i].attributeValues?.RegistrationStartDate, '') != true ||
                this.rules.dateCannotBeAfterEvent(regStartDate, lastDate, '') != true ||
                this.rules.required(this.viewModel.events[i].attributeValues?.RegistrationFeeType, '') != true ||
                (this.viewModel.events[i].attributeValues?.RegistrationFeeType.split(",").includes('Online Fee') && this.rules.required(this.viewModel.events[i].attributeValues?.OnlineRegistrationFee, '') != true) ||
                (this.viewModel.events[i].attributeValues?.RegistrationFeeType.split(",").includes('Fee per Individual') && this.rules.required(this.viewModel.events[i].attributeValues?.IndividualRegistrationFee, '') != true) ||
                (this.viewModel.events[i].attributeValues?.RegistrationFeeType.split(",").includes('Fee per Couple') && this.rules.required(this.viewModel.events[i].attributeValues?.CoupleRegistrationFee, '') != true) ||
                this.rules.required(this.viewModel.events[i].attributeValues?.RegistrationEndDate, '') != true ||
                this.rules.dateCannotBeAfterEvent(regEndDate, lastDate, '') != true ||
                this.rules.required(this.viewModel.events[i].attributeValues?.RegistrationEndTime, '') != true ||
                this.rules.required(this.viewModel.events[i].attributeValues?.RegistrationConfirmationEmailSender, '') != true ||
                this.rules.required(this.viewModel.events[i].attributeValues?.RegistrationConfirmationEmailAdditionalDetails, '') != true ||
                (this.viewModel.events[i].attributeValues?.NeedsReminderEmail == 'True' && this.rules.required(this.viewModel.events[i].attributeValues?.RegistrationReminderEmailAdditionalDetails, '') != true)
              ) {
                requestIsValid = false
                eventIsValid = false
                let idx = invalidSections.indexOf('Registration')
                if(idx < 0) {
                  invalidSections.push('Registration')
                }
              }
              let opsAttrs = [] 
              let attrs = this.viewModel.events[i].attributes
              for(let attr in attrs) {
                if(attrs[attr].categories.map((c: any) => c.name).includes('Event Ops Requests')) {
                  opsAttrs.push(attr)
                }
              }
              let opsIsValid = false
              for(let attr in opsAttrs) {
                let event = this.viewModel.events[i]
                if(event.attributeValues && (event.attributeValues[attr] != '' && event.attributeValues[attr] != 'False')) {
                  opsIsValid = true
                }
              }
              if(!opsIsValid) {
                requestIsValid = false
                eventIsValid = false
                let idx = invalidSections.indexOf('Ops')
                if(idx < 0) {
                  invalidSections.push('Ops')
                }
              }
            }
            let event = this.viewModel.events[i]
            if(event.attributeValues) {
              event.attributeValues.EventIsValid = eventIsValid ? 'True' : 'False'
            }
          }
        }
        this.viewModel.request.attributeValues.RequestIsValid = requestIsValid ? 'True' : 'False'

        //Remove/Readonly Sections
        let twoWeeksTense = this.findTense(14)
        let thirtyDaysTense = this.findTense(30)
        let pubDateCutOff = lastDate.minus({days: 42})
        if(this.viewModel.request.attributeValues.PublicityStartDate) {
          pubDateCutOff = DateTime.fromISO(this.viewModel.request.attributeValues.PublicityStartDate).minus({days: 21})
        }
        let regDateCutOff = firstDate.minus({days: 14})
        if(this.viewModel.events[0].attributeValues && this.viewModel.events[0].attributeValues.RegistrationStartDate) {
          regDateCutOff = DateTime.fromISO(this.viewModel.events[0].attributeValues.RegistrationStartDate).minus({days: 14})
        }
        let sixWeeksTense = DateTime.now() > pubDateCutOff ? 'was' : 'is'
        console.log(sixWeeksTense)
        console.log("Reg Cut Off:", regDateCutOff)
        console.log("Pub Cut Off:", pubDateCutOff)
        //Drafts, cut anything that is past-deadline
        if(this.viewModel.request.attributeValues.RequestStatus == 'Draft') {
          if(twoWeeksTense == 'was') {
            this.viewModel.request.attributeValues.NeedsOnline = 'False'
            this.viewModel.request.attributeValues.NeedsCatering = 'False'
            this.viewModel.request.attributeValues.NeedsOpsAccommodations = 'False'
            this.viewModel.request.attributeValues.NeedsWebCalendar = 'False'
            this.viewModel.request.attributeValues.NeedsProductionAccommodations = 'False'
          }
          if(DateTime.now() > regDateCutOff) {
            this.viewModel.request.attributeValues.NeedsRegistration = 'False'
          }
          if(thirtyDaysTense == 'was') {
            this.viewModel.request.attributeValues.NeedsChildCare = 'False'
          }
          if(sixWeeksTense == 'was') {
            this.viewModel.request.attributeValues.NeedsPublicity = 'False'
          }
        } else if (this.viewModel.request.attributeValues.RequestStatus == 'Submitted' || this.viewModel.request.attributeValues.RequestStatus == 'In Progress'){
          //If the request is Submitted or In Progress, only remove if the section is invalid
          if(twoWeeksTense == 'was') {
            if(invalidSections.includes('Online')) {
              this.viewModel.request.attributeValues.NeedsOnline = 'False'
            }
            if(invalidSections.includes('Catering')) {
              this.viewModel.request.attributeValues.NeedsCatering = 'False'
            }
            if(invalidSections.includes('Childcare Catering')) {
              this.viewModel.request.attributeValues.NeedsChildCareCatering = 'False'
            }
            if(invalidSections.includes('Registration')) {
              this.viewModel.request.attributeValues.NeedsRegistration = 'False'
            }
            if(invalidSections.includes('Calendar')) {
              this.viewModel.request.attributeValues.NeedsWebCalendar = 'False'
            }
            if(invalidSections.includes('Production')) {
              this.viewModel.request.attributeValues.NeedsProductionAccommodations = 'False'
            }
            if(invalidSections.includes('Ops')) {
              this.viewModel.request.attributeValues.NeedsOpsAccommodations = 'False'
            }
          }
          if(thirtyDaysTense == 'was' && invalidSections.includes('Childcare')) {
            this.viewModel.request.attributeValues.NeedsChildCare = 'False'
          }
          if(sixWeeksTense == 'was' && invalidSections.includes('Publicity')) {
            this.viewModel.request.attributeValues.NeedsPublicity = 'False'
          }
        } else {
          if(twoWeeksTense == 'was') {
            this.readonlySections.push('Online')
            this.readonlySections.push('Catering')
            this.readonlySections.push('Childcare Catering')
            this.readonlySections.push('Ops')
            this.readonlySections.push('Registration')
            this.readonlySections.push('Calendar')
            this.readonlySections.push('Production')
          }
          if(thirtyDaysTense == 'was') {
            this.readonlySections.push('Childcare')
          }
          if(sixWeeksTense == 'was') {
            this.readonlySections.push('Publicity')
          }
        }
        console.log('Invalid Sections')
        console.log(invalidSections)
        console.log('ReadOnly Sections')
        console.log(this.readonlySections)
      }
    },
    validationChange(errs: any) {
      let exists = false
      this.requestErrors.forEach((e: any) => {
        if(e.ref == errs.ref) {
          e.errors = errs.errors
          exists = true
        }
      })
      if(!exists) {
        this.requestErrors.push(errs)
      }
    },
    preFill() {
      let source = this.viewModel?.events.filter((e: any) => {
        return e.attributeValues.EventDate == this.preFillSource
      })[0]
      let target = this.viewModel?.events.filter((e: any) => {
        return e.attributeValues.EventDate == this.preFillTarget
      })[0] 
      if(source?.attributeValues && target?.attributeValues) {
        for(let attr in target?.attributes) {
          if(attr != 'EventDate') {
            if(this.preFillModalOption != '') {
              let categories = target.attributes[attr].categories.map((c: any) => c.name)
              if(categories.includes(this.preFillModalOption)) {
                target.attributeValues[attr] = source.attributeValues[attr]
              }
            } else {
              target.attributeValues[attr] = source.attributeValues[attr]
            }
          }
        }
      }
      this.preFillModal = false
      this.preFillSource = ""
    }, 
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
            let isFuneralRequest = false
            let val = this.viewModel.request.attributeValues.Ministry
            let ministry = {} as DefinedValue
            if(val != '') {
              let min = JSON.parse(val) as ListItem
              ministry = this.viewModel.ministries.filter((dv: any) => {
                return dv.guid == min.value
              })[0]
            }
            if(ministry?.value?.toLowerCase().includes("funeral")) {
              isFuneralRequest = true
            }
            if (isFuneralRequest || (first && first.startOf("day") >= today.startOf("day"))) {
              return 'is'
            }
            return 'was'
          } 
        }
      }
      return 'is'
    },
    previewStartBuffer(time: string, buffer: any) {
      if(time && buffer) {
        return DateTime.fromFormat(time, 'HH:mm:ss').minus({minutes: buffer}).toFormat('hh:mm a')
      } else if (time) {
        return DateTime.fromFormat(time, 'HH:mm:ss').toFormat('hh:mm a')
      }
    },
    previewEndBuffer(time: string, buffer: any) {
      if(time && buffer) {
        return DateTime.fromFormat(time, 'HH:mm:ss').plus({minutes: buffer}).toFormat('hh:mm a')
      } else if (time) {
        return DateTime.fromFormat(time, 'HH:mm:ss').toFormat('hh:mm a')
      }
    },
    replaceDate() {
      if(this.viewModel?.request.attributeValues && this.changeDateOriginal && this.changeDateReplacement) {
        this.viewModel.request.attributeValues.EventDates = this.viewModel.request.attributeValues.EventDates.replace(this.changeDateOriginal, this.changeDateReplacement)
        this.viewModel.events.forEach((e: any) => {
          if(e.attributeValues.EventDate == this.changeDateOriginal) {
            e.attributeValues.EventDate = this.changeDateReplacement
          }
        })
      }
      this.matchMultiEvent()
      let selectedIdx = -1 
      this.viewModel?.events.forEach((e: any, idx: number) => {
        if(e.attributeValues.EventDate == this.changeDateReplacement) {
          selectedIdx = idx
        }
      })
      this.jumpTo(selectedIdx + 2)
      this.changeDateModal = false
    },
    createComment(comment: string) {
      let el = document.getElementById('updateProgress')
      if(el) {
        el.style.display = 'block'
      }
      let id = this.viewModel?.request?.id as number
      this.addComment(id, comment).then((res: any) => {
        if (res.isError) {
          this.toastMessage = res.errorMessage
          let toast = document.getElementById('toast')
          toast?.classList.add("show")
        } else {
          this.toastMessage = "Comment Added"
          let toast = document.getElementById('toastSuccess')
          toast?.classList.add("show")
        }
      }).finally(() => {
        if(el) {
          el.style.display = 'none'
        }
      })
    },
  },
  watch: {
    'viewModel.request.attributeValues.IsSame'(val) {
      if(this.viewModel?.events) {
        if(val == 'True' && this.viewModel.events.length > 1) {
          this.viewModel.events = this.viewModel.events.slice(0, 1)
        } else {
          this.matchMultiEvent()
        }
      }
    },
    'viewModel.request.attributeValues.NeedsSpace'(val) {
      let idx = this.resources.indexOf('Room')
      if(val == 'False') {
        //Remove all Space data
        this.clearEventDetailsData("Event Space")
        //Remove Space from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Space to Requested Resources List
        if(idx < 0) {
          this.resources.push('Room')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsOnline'(val) {
      let idx = this.resources.indexOf('Online Event')
      if(val == 'False') {
        //Remove all Online data
        this.clearEventDetailsData("Event Online")
        //Remove Online from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Online to Requested Resources List
        if(idx < 0) {
          this.resources.push('Online Event')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsCatering'(val) {
      let idx = this.resources.indexOf('Catering')
      if(val == 'False') {
        //Remove all Catering data
        this.clearEventDetailsData("Event Catering")
        //Remove Catering from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Auto-true for CC Catering
        if(this.viewModel?.request.attributeValues?.NeedsChildCare == 'True') {
          this.viewModel.request.attributeValues.NeedsChildCareCatering = 'True'
        }
        //Add Catering to Requested Resources List
        if(idx < 0) {
          this.resources.push('Catering')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsChildCare'(val) {
      let idx = this.resources.indexOf('Childcare')
      if(val == 'False') {
        //Remove all Childcare data
        this.clearEventDetailsData("Event Childcare")
        //Remove Childcare from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
        //Remove CC Catering 
        if(this.viewModel?.request.attributeValues) {
          this.viewModel.request.attributeValues.NeedsChildCareCatering = 'False'
        }
      } else {
        //Auto-true for CC Catering
        if(this.viewModel?.request.attributeValues?.NeedsCatering == 'True') {
          this.viewModel.request.attributeValues.NeedsChildCareCatering = 'True'
        }
        //Add Childcare to Requested Resources List
        if(idx < 0) {
          this.resources.push('Childcare')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsChildCareCatering'(val) {
      let idx = this.resources.indexOf('Childcare Catering')
      if(val == 'False') {
        //Remove all Childcare Catering data
        this.clearEventDetailsData("Event Childcare Catering")
        //Remove Childcare from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Childcare to Requested Resources List
        if(idx < 0) {
          this.resources.push('Childcare Catering')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsOpsAccommodations'(val) {
      let idx = this.resources.indexOf('Extra Resources')
      if(val == 'False') {
        //Remove all Ops Accom data
        this.clearEventDetailsData("Event Ops Requests")
        //Remove Ops from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Ops to Requested Resources List
        if(idx < 0) {
          this.resources.push('Extra Resources')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsRegistration'(val) {
      let idx = this.resources.indexOf('Registration')
      if(val == 'True' && this.viewModel?.events) {
        if(this.viewModel.events[0].attributeValues?.RegistrationConfirmationEmailSender == "" && this.currentPerson?.fullName) {
          this.viewModel.events[0].attributeValues.RegistrationConfirmationEmailSender = this.currentPerson.fullName
        }
      }
      if(val == 'False') {
        //Remove all Registration data
        this.clearEventDetailsData("Event Registration")
        //Remove Registration from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Registration to Requested Resources List
        if(idx < 0) {
          this.resources.push('Registration')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsPublicity'(val) {
      let idx = this.resources.indexOf('Publicity')
      if(val == 'False') {
        //Remove all Publicity data
        this.clearEventData("Event Publicity")
        //Remove Publicity from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Publicity to Requested Resources List
        if(idx < 0) {
          this.resources.push('Publicity')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsProductionAccommodations'(val) {
      let idx = this.resources.indexOf('Production')
      if(val == 'False') {
        //Remove all Production data
        this.clearEventData("Event Production")
        //Remove Production from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Production to Requested Resources List
        if(idx < 0) {
          this.resources.push('Production')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsWebCalendar'(val) {
      let idx = this.resources.indexOf('Web Calendar')
      if(val == 'False' && this.viewModel?.request?.attributeValues) {
        //Remove all Web Calendar data
        this.viewModel.request.attributeValues.WebCalendarDescription = ""
        //Remove Web Calendar from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Web Calendar to Requested Resources List
        if(idx < 0) {
          this.resources.push('Web Calendar')
        }
      }
    },
    resources: {
      handler(val) {
        val = val.filter((r: string) => {
          return r != ""
        })
        if(val && this.viewModel?.request?.attributeValues) {
          this.viewModel.request.attributeValues.RequestType = val.join(',')
        }
      },
      deep: true
    },
    selectedDates: {
      handler(val) {
        if(this.viewModel?.request?.attributeValues?.IsSame == 'False') {
          //Need to fix the events items to match the new dates
          this.matchMultiEvent()
        }
      },
      deep: true
    },
    modal: {
      handler (val) {
        if(!val) {
          if(this.viewModel?.request && this.viewModel.request.id > 0) {
            let el = document.getElementById('updateProgress')
            if(el) {
              el.style.display = 'block'
            }
            this.reload(this.viewModel.request.id).then((res) => {
              if(this.viewModel?.request && res.data.request ) {
                this.viewModel.request = res.data.request
                this.viewModel.events = res.data.events
              } else if (res.isError || res.Message) {
                this.toastIsError = true
                this.toastMessage = res.errorMessage ? res.errorMessage : res.Message
                let el = document.getElementById('toast')
                el?.classList.add("show")
              }
            }).catch((err) => {
              console.log(err)
              if(err.Message) {
                this.toastIsError = true
                this.toastMessage = err.Message
                let el = document.getElementById('toast')
                el?.classList.add("show")
              }
            }).finally(() => {
              if(el) {
                el.style.display = 'none'
              }
            })
          }
        }
      },
      deep: true
    }
  },
  mounted() {
    if (!this.viewModel?.isSuperUser) {
      this.step = 1
      if(this.viewModel?.request?.attributeValues) {
        this.viewModel.request.attributeValues.NeedsSpace = 'True'
      }
    }
    if(this.viewModel?.request.id == 0) {
      //New Request set some defaults
      if(this.viewModel?.request?.attributeValues) {
        this.viewModel.request.attributeValues.Contact = `${this.currentPerson?.nickName} ${this.currentPerson?.lastName}`
        this.viewModel.request.attributeValues.RequestStatus = "Draft"
      }
    } else {
      if(this.viewModel?.request.attributeValues) {
        this.resources = this.viewModel.request.attributeValues.RequestType.split(",").map((t: string) => t.trim())
      }
      //Show Validation
      for(let i=0; i<= this.lastStep; i++) {
        this.pagesViewed.push(i)
      }
      this.validate()
    }
  },
  template: `
<div class="card">
  <a-steps :current="step">
    <a-step class="hover" :key="0" @click="jumpTo(0)" title="Resources" />
    <a-step class="hover" :key="1" @click="jumpTo(1)" title="Basic Info" />
    <template v-if="hasEventData">
      <template v-if="viewModel.request.attributeValues.IsSame == 'True'">
        <a-step class="hover" v-if="hasEventData" :key="2" @click="jumpTo(2)" title="Event Info" />
      </template>
      <template v-else>
          <a-step class="hover" v-for="(e, idx) in viewModel.events" :key="(idx + 2)" @click="jumpTo((idx + 2))" :title="formatDate(e.attributeValues.EventDate)" />
      </template>
    </template>
    <a-step class="hover" v-if="viewModel.request.attributeValues.NeedsPublicity == 'True' || viewModel.request.attributeValues.NeedsWebCalendar == 'True' || viewModel.request.attributeValues.NeedsProductionAccommodations == 'True'" :key="publicityStep" @click="jumpTo(publicityStep)" title="Additional Requests" />
  </a-steps>
  <div class="steps-content">
    <br/>
    <tcc-resources v-if="step == 0" :view-model="viewModel"></tcc-resources>
    <tcc-basic v-if="step == 1" :view-model="viewModel" :minEventDate="minEventDate" :showValidation="pagesViewed.includes(1)" refName="basic" @validation-change="validationChange" ref="basic" v-on:createComment="createComment"></tcc-basic>
    <template v-for="(e, idx) in viewModel.events" :key="idx">
      <template v-if="step == (idx + 2)">
        <template v-if="viewModel.request.attributeValues.IsSame == 'False'">
          <h2 class="text-accent" style="display: flex; align-items: center;">
            Information for {{formatDate(e.attributeValues.EventDate)}}
            <a-btn class="ml-1" type="accent-outlined" shape="round" @click="preFillSource = ''; preFillTarget = e.attributeValues.EventDate; preFillModalOption = ''; preFillModal = true;">Prefill Event</a-btn>
            <a-btn class="ml-1" type="accent-outlined" shape="round" @click="changeDateOriginal = e.attributeValues.EventDate; changeDateReplacement = ''; changeDateModal = true;">Change Event Date</a-btn>
          </h2>
        </template>
        <template v-if="viewModel.request.attributeValues.IsSame == 'False'">
          <strong>What time will your event begin and end on {{formatDate(e.attributeValues.EventDate)}}?</strong>
          <tcc-event-time :request="viewModel.request" :e="e" :showValidation="pagesViewed.includes(idx + 2)" :refName="getRefName('time', idx)" @validation-change="validationChange" :ref="getRefName('time', idx)"></tcc-event-time>
          <tcc-buffer v-if="(viewModel.isEventAdmin || viewModel.isSuperUser)" :e="e"></tcc-buffer>
          <br/>
        </template>
        <template v-if="viewModel.request.attributeValues.NeedsSpace == 'True'">
          <h3 class="text-primary">
            Space Information
            <a-btn v-if="viewModel.request.attributeValues.IsSame == 'False'" type="accent-outlined" shape="round" @click="preFillSource = ''; preFillTarget = e.attributeValues.EventDate; preFillModalOption = 'Event Space'; preFillModal = true;">Prefill Section</a-btn>
          </h3>
          <tcc-space :e="e" :request="viewModel.request" :originalRequest="viewModel.originalRequest" :locations="viewModel.locations" :existing="viewModel.existing" :showValidation="pagesViewed.includes(idx + 2)" :refName="getRefName('space', idx)" @validation-change="validationChange" :ref="getRefName('space', idx)"></tcc-space>
          <br/>
        </template>
        <template v-if="viewModel.request.attributeValues.NeedsCatering == 'True'">
          <h3 class="text-primary">
            Catering Information
            <a-btn v-if="viewModel.request.attributeValues.IsSame == 'False'" type="accent-outlined" shape="round" @click="preFillSource = ''; preFillTarget = e.attributeValues.EventDate; preFillModalOption = 'Event Catering'; preFillModal = true;">Prefill Section</a-btn>
          </h3>
          <tcc-catering :e="e" :request="viewModel.request" :showValidation="pagesViewed.includes(idx + 2)" :refName="getRefName('catering', idx)" @validation-change="validationChange" :ref="getRefName('catering', idx)"></tcc-catering>
          <br/>
        </template>
        <template v-if="viewModel.request.attributeValues.NeedsOpsAccommodations == 'True'">
          <h3 class="text-primary">
            Other Accomodations
            <a-btn v-if="viewModel.request.attributeValues.IsSame == 'False'" type="accent-outlined" shape="round" @click="preFillSource = ''; preFillTarget = e.attributeValues.EventDate; preFillModalOption = 'Event Ops Requests'; preFillModal = true;">Prefill Section</a-btn>
          </h3>
          <tcc-ops :e="e" :request="viewModel.request" :showValidation="pagesViewed.includes(idx + 2)" :locations="viewModel.locations" :locationSetUp="viewModel.locationSetupMatrix" :inventoryList="viewModel.inventoryList" :existing="viewModel.existing" :refName="getRefName('ops', idx)" @validation-change="validationChange" :ref="getRefName('ops', idx)"></tcc-ops>
          <br/>
        </template>
        <template v-if="viewModel.request.attributeValues.NeedsChildCare == 'True'">
          <h3 class="text-primary">
            Childcare Information
            <a-btn v-if="viewModel.request.attributeValues.IsSame == 'False'" type="accent-outlined" shape="round" @click="preFillSource = ''; preFillTarget = e.attributeValues.EventDate; preFillModalOption = 'Event Childcare'; preFillModal = true;">Prefill Section</a-btn>
          </h3>
          <tcc-childcare :e="e" :showValidation="pagesViewed.includes(idx + 2)" :refName="getRefName('childcare', idx)" @validation-change="validationChange" :ref="getRefName('childcare', idx)"></tcc-childcare>
          <br v-if="viewModel.request.attributeValues.NeedsChildCareCatering == 'True'" />
          <h4 class="text-accent" v-if="viewModel.request.attributeValues.NeedsChildCareCatering == 'True'">Childcare Catering Information</h4>
          <tcc-childcare-catering v-if="viewModel.request.attributeValues.NeedsChildCareCatering == 'True'" :e="e" :showValidation="pagesViewed.includes(idx + 2)" :refName="getRefName('cccatering', idx)" @validation-change="validationChange" :ref="getRefName('cccatering', idx)"></tcc-childcare-catering>
          <br/>
        </template>
        <template v-if="viewModel.request.attributeValues.NeedsRegistration == 'True'">
          <h3 class="text-primary">
            Registration Information
            <a-btn v-if="viewModel.request.attributeValues.IsSame == 'False'" type="accent-outlined" shape="round" @click="preFillSource = ''; preFillTarget = e.attributeValues.EventDate; preFillModalOption = 'Event Registration'; preFillModal = true;">Prefill Section</a-btn>
          </h3>
          <tcc-registration :e="e" :request="viewModel.request" :original="viewModel.originalRequest" :ministries="viewModel.ministries" :discountAttrs="viewModel.discountCodeAttrs" :showValidation="pagesViewed.includes(idx + 2)" :refName="getRefName('reg', idx)" @validation-change="validationChange" :ref="getRefName('reg', idx)"></tcc-registration>
          <br/>
        </template>
        <template v-if="viewModel.request.attributeValues.NeedsOnline == 'True'">
          <h3 class="text-primary">
            Zoom Information
            <a-btn v-if="viewModel.request.attributeValues.IsSame == 'False'" type="accent-outlined" shape="round" @click="preFillSource = ''; preFillTarget = e.attributeValues.EventDate; preFillModalOption = 'Event Online'; preFillModal = true;">Prefill Section</a-btn>
          </h3>
          <tcc-online :e="e" :showValidation="pagesViewed.includes(idx + 2)" :refName="getRefName('online', idx)" @validation-change="validationChange" :ref="getRefName('online', idx)"></tcc-online>
          <br/>
        </template>
      </template>
    </template>
    <template v-if="step == publicityStep">
      <template v-if="viewModel.request.attributeValues.NeedsWebCalendar == 'True'">
        <h3 class="text-primary">Web Calendar Information</h3>
        <tcc-web-cal :request="viewModel.request" :showValidation="pagesViewed.includes(publicityStep)" refName="webcal" @validation-change="validationChange" ref="webcal"></tcc-web-cal>
      </template>
      <template v-if="viewModel.request.attributeValues.NeedsPublicity == 'True'">
        <h3 class="text-primary">Publicity Information</h3>
        <tcc-publicity :request="viewModel.request" :showValidation="pagesViewed.includes(publicityStep)" refName="publicity" @validation-change="validationChange" ref="publicity"></tcc-publicity>
      </template>
      <template v-if="viewModel.request.attributeValues.NeedsProductionAccommodations == 'True'">
        <h3 class="text-primary">Production Tech Information</h3>
        <tcc-prod-tech :request="viewModel.request" :showValidation="pagesViewed.includes(publicityStep)" refName="prodtech" @validation-change="validationChange" ref="prodtech"></tcc-prod-tech>
      </template>
    </template>
    <template v-if="step == lastStep">
      <h3 class="text-primary">Notes</h3>
      <rck-field
        v-model="viewModel.request.attributeValues.Notes"
        :attribute="viewModel.request.attributes.Notes"
        :is-edit-mode="true"
      ></rck-field>
    </template>
  </div>
  <div class="row steps-action pt-2">
    <div class="col">
      <a-btn v-if="step == lastStep" class="pull-right" type="primary" @click="submitRequest" :disabled="canSubmit || isSubmitting">
        <template v-if="viewModel.request.attributeValues.RequestStatus == 'Approved'">
          Request Changes
        </template>  
        <template v-else-if="viewModel.request.attributeValues.RequestStatus == 'Submitted' || viewModel.request.attributeValues.RequestStatus == 'In Progress' || viewModel.request.attributeValues.RequestStatus == 'Pending Changes'">
          Update
        </template>
        <template v-else>
          Submit
        </template>  
      </a-btn> 
      <a-btn v-else class="pull-right" type="primary" @click="next">Next</a-btn>
      <a-btn v-if="viewModel.request.attributeValues.RequestStatus == 'Draft'" style="margin: 0px 4px;" class="pull-right" type="accent" @click="saveDraft" :disabled="noTitle">Save</a-btn>
    </div>
  </div>
  <div class="row" v-if="canSubmit && step == lastStep">
    <div class="col text-red" style="text-align: right;">
      You cannot submit a request unless you have: a title, ministry, and at least one event date
    </div>
  </div>
  <!-- Confirmation Modal -->
  <a-modal v-model:visible="modal" width="80%">
    <div class="pt-2">
      <div class="mb-2 text-primary">
        <strong>
          <i class="fa fa-check" style="font-size: 25px"></i>
          {{response.message}}
        </strong>
      </div>
      <div v-if="response.isPreApproved">
        Due to the nature of your request, it has been pre-approved. 
      </div>
      <div v-else>
        This request is not eligible for pre-approval based on the following criteria: 
        <ul>
          <li v-for="(r, idx) in response.notValidForPreApprovalReasons" :key="idx">{{r}}</li>
        </ul>
      </div>
      <div class="text-red" v-if="errorList && errorList.length > 0">
        Before your request can be approved, you will need to correct the following:
        <ul>
          <li v-for="(e, idx) in errorList" :key="idx">{{e}}</li>
        </ul>
      </div>
    </div>
    <template #footer>
      <a-btn type="accent" @click="continueEdit">Continue Editing</a-btn>
      <a-btn type="primary" @click="openInDashboard">Open Dashboard</a-btn>
    </template>
  </a-modal>
  <!-- Prefill Modal --> 
  <a-modal v-model:visible="preFillModal">
    <div class="pt-2">
      <rck-lbl v-if="preFillModalOption">
        Select the date of the event you wish to use to prefill the {{preFillModalOption}} information for your event on {{formatDate(preFillTarget)}}
      </rck-lbl>
      <rck-lbl v-else>
        Select the date of the event you wish to use to prefill all information for your event on {{formatDate(preFillTarget)}}
      </rck-lbl>
      <div class="row">
        <div class="col col-xs-12">
          <a-select
            v-model:value="preFillSource"
            style="width: 100%;"
          >
            <template v-for="(e, idx) in viewModel.events">
              <a-select-option v-if="e.attributeValues.EventDate != preFillTarget" :key="idx" :value="e.attributeValues.EventDate">
                {{formatDate(e.attributeValues.EventDate)}}
              </a-select-option>
            </template>
          </a-select>
        </div>
      </div>
    </div>
    <template #footer>
      <a-btn type="primary" @click="preFill">Prefill</a-btn>
      <a-btn type="grey" @click="preFillModal = false;">Cancel</a-btn>
    </template>
  </a-modal>
  <a-modal v-model:visible="changeDateModal">
    <div class="pt-2">
      <div class="row">
        <div class="col col-xs-12">
          <div class="mb-2">
            Please note that event information tabs are displayed in chronological order, if your replacement date changes this order you might need to look for the tab with your new date.
          </div>
          <tcc-date-pkr
            label="Select the new date of this event"
            v-model="changeDateReplacement"
            :min="minEventDate"
            :disabledDates="viewModel.request.attributeValues.EventDates"
          ></tcc-date-pkr>
        </div>
      </div>
    </div>
    <template #footer>
      <a-btn type="primary" @click="replaceDate">Replace Date</a-btn>
      <a-btn type="grey" @click="changeDateModal = false;">Cancel</a-btn>
    </template>
  </a-modal>
</div>
<div id="toast" role="alert">
  <div class="toast-header text-red">
    <i class="fas fa-exclamation-triangle mr-1"></i>
    <strong>System Exception</strong>
    <i class="fa fa-times pull-right hover" @click="hideToast"></i>
  </div>
  <div class="toast-body">
    {{toastMessage}}
  </div>
</div>
<div id="toastSuccess" role="alert">
  <div class="toast-header text-primary">
    <i class="fas fa-check-circle mr-1"></i>
    <strong>System Notification</strong>
    <i class="fa fa-times pull-right hover" @click="hideToast"></i>
  </div>
  <div class="toast-body">
    {{toastMessage}}
  </div>
</div>
<v-style>
label, .control-label {
  color: rgba(0,0,0,.6);
  line-height: 18px;
  letter-spacing: normal;
  font-size: 14px;
}
.ant-form-item-label {
  text-align: left;
  line-height: 1em;
}
.ant-form-item-label>label:after {
  content: none;
}
.ant-col {
  padding: 4px 12px !important;
}
.ant-form-item .ant-col {
  padding: 0px !important;
}
.input-hint {
  color: rgba(0,0,0,.6);
  font-size: 12px;
  line-height: 12px;
  word-break: break-word;
  overflow-wrap: break-word;
  word-wrap: break-word;
  -webkit-hyphens: auto;
  -ms-hyphens: auto;
  hyphens: auto;
  padding-top: 8px;
}
.input-hint.rock-hint {
  padding-top: 0px;
  padding-bottom: 8px;
}
.input-label {
  font-weight: bold;
}
.has-error input, .has-error .chosen-single, .has-error .chosen-choices, .has-error textarea, .has-error select {
  border-color: #cc3f0c;
}
.card {
  box-shadow: 0 0 1px 0 rgb(0 0 0 / 8%), 0 1px 3px 0 rgb(0 0 0 / 15%);
  padding: 32px;
  border-radius: 4px;
}
.ant-steps-item-process .ant-steps-item-icon, .ant-steps-item-process .ant-steps-item-icon, .ant-switch-checked, .ant-btn-primary, .ant-steps-item-finish>.ant-steps-item-container>.ant-steps-item-content>.ant-steps-item-title:after, .dp__range_end, .dp__range_start, .dp__active_date {
  background-color: #347689;
  border-color: #347689;
}
.ant-btn-accent {
  background-color: #8ED2C9;
  border-color: #8ED2C9;
}
.ant-btn-accent-outlined {
  color: #8ED2C9;
  border-color: #8ED2C9;
}
.ant-steps-item-finish .ant-steps-item-icon, .dp__today {
  border-color: #347689;
}
.ant-steps-item-finish .ant-steps-item-icon>.ant-steps-icon {
  color: #347689;
}
.ant-btn-primary:focus, .ant-btn-primary:hover {
  background-color: rgba(52, 118, 137, .85);
  border-color: rgba(52, 118, 137, .85);
}
.ant-btn-accent:focus, .ant-btn-accent:hover {
  background-color: hsl(172deg 30% 55%);
  border-color: hsl(172deg 30% 55%);
  color: black;
}
.ant-btn-accent-outlined:focus, .ant-btn-accent-outlined:hover {
  border-color: hsl(172deg 30% 55%);
  color: hsl(172deg 30% 55%);
}
.ant-btn-cancelled, .ant-btn-cancelledbyuser, .ant-btn-grey {
  background-color: #9e9e9e;
  border-color: #9e9e9e;
}
.ant-btn-grey:focus, .ant-btn-grey:hover, .ant-btn-cancelled:focus, .ant-btn-cancelled:hover, .ant-btn-cancelledbyuser:focus, .ant-btn-cancelledbyuser:hover {
  background-color: #929392;
  border-color: #929392;
  color: black;
}
.ant-btn-red {
  background-color: rgb(204 63 12);
  border-color: rgb(204 63 12);
}
.ant-btn-red:focus, .ant-btn-red:hover {
  background-color: rgb(184 56 9);
  border-color: rgb(184 56 9);
  color: #fff;
}
.hover {
  cursor: pointer;
}
.text-primary {
  color: #347689;
}
.text-accent {
  color: #8ED2C9;
}
.text-red, .text-errors, .has-error label {
  color: #cc3f0c;
}
.text-errors {
  font-size: .85em;
}
.bg-red {
  background-color: #dec7c7;
}
.tcc-dropdown {
  display: flex;
  flex-direction: column;
  padding: 0px 4px;
  max-height: 300px;
  overflow-y: scroll;
}
.row-equal-height {
  display: flex;
  flex-wrap: wrap;
}
.row-equal-height > .col {
  display: flex;
  flex-direction: column;
  align-items: start;
  justify-content: center;
}
.form-group {
  margin-bottom: 0px;
}
/* Toast Message */
#toast, #toastSuccess {
  display: none; 
  opacity: 0;
  min-width: 250px; 
  border-radius: 2px; 
  padding: 8px; 
  position: fixed; 
  z-index: 5000; 
  left: 30px; 
  bottom: 30px; 
  transition: opacity 1s ease;
  box-shadow: 0 0 1px 0 rgb(0 0 0 / 8%), 0 1px 3px 0 rgb(0 0 0 / 15%);
}
#toast {
  border-top: 3px solid #f4bec2;
  background-color: #f8d7da; 
  color: #333;
}
#toastSuccess {
  border-top: 3px solid #16c98d;
  color: #108043;
  background-color: #eaf6ef;
}
#toast.show, #toastSuccess.show {
  display: block;
  opacity: 1;
  transition: opacity 1s ease;
}
</v-style>
`
})
