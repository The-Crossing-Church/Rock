import { DateTime, Interval } from "luxon"
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag"
import { ContentChannelItemBag } from "src/Plugins/ViewModels/contentChannelItemBag"
import { DefinedValueBag } from "src/Plugins/ViewModels/definedValueBag"

const rules = {
  required: (value: any, key: string) => {
    if(typeof value === 'string') {
      if(value.includes("{")) {
        let obj = JSON.parse(value)
        return obj.value != '' || `${key} is required`
      } 
    } 
    return !!value || `${key} is required`
  },
  timeIsValid:(startTime: string, endTime: string, isStart: boolean) => {
    if(startTime && endTime) {
      let start = DateTime.fromFormat(startTime, 'HH:mm:ss')
      let end = DateTime.fromFormat(endTime, 'HH:mm:ss')
      let span = end.plus({ minutes: 1 })
      let interval = Interval.fromDateTimes(end, span)
      if(interval.isAfter(start)) {
        return true
      }
      if(isStart) {
        return `Start Time must be before ${end.toFormat('hh:mm a')}`
      } else {
        return `End Time must be after ${start.toFormat('hh:mm a')}`
      }
    }
  },
  attendance: (value: number, rooms: string, locs: Array<any>, key: string) => {
    if(rooms) {
      let selectedRooms = JSON.parse(rooms)
      if(selectedRooms && selectedRooms.value) {
          let roomGuids = selectedRooms.value.split(',')
          let locations = locs?.filter((l: any) => {
              return roomGuids.includes(l.guid)
          })
          if(locations && locations.length > 0) {
              let capacity = locations.map((l: any) => {
                  if(l.attributeValues?.Capacity.value) {
                      return parseInt(l.attributeValues.Capacity.value)
                  }
                  return 0
              }).reduce((partialSum: any, a: any) => partialSum + a, 0)
              return value <= capacity || `${key} cannot exceed ${capacity}`
          } else {
              return true
          }
      }
    }
    return true
  },
  largeEventSecurity: (attendance: number, needsOps: string, needsSecurity: string, isSuperUser: boolean) => {
    if(attendance >= 200) {
      if(!isSuperUser) {
        return `Events expecting 200 or more people are required to have security personnel, please ask your ministry's event admin to submit your request`
      }
      if(needsOps == 'False' || needsSecurity == 'False') {
        return `Events expecting 200 or more people are required to have security personnel`
      }
    }
    return true
  },
  maxRegistration: (value: number, rooms: string, locs: Array<any>, key: string, hasOnline: boolean) => {
    if(rooms) {
      let selectedRooms = JSON.parse(rooms)
      if(selectedRooms && selectedRooms.value) {
          let roomGuids = selectedRooms.value.split(',')
          let locations = locs?.filter((l: any) => {
              return roomGuids.includes(l.guid)
          })
          if(locations && locations.length > 0 && !hasOnline) {
              let capacity = locations.map((l: any) => {
                  if(l.attributeValues?.Capacity.value) {
                      return parseInt(l.attributeValues.Capacity.value)
                  }
                  return 0
              }).reduce((partialSum: any, a: any) => partialSum + a, 0)
              return value <= capacity || `${key} cannot exceed ${capacity}`
          } else {
              return true
          }
      }
    }
    return true
  },
  drinkTimeRequired: (value: string, drinkStr: string, key: string) => {
    if(drinkStr != '') {
      let drinks = JSON.parse(drinkStr)
      if(drinks && drinks.value) {
        let selected = drinks.value.split(',')
        if(selected.length > 0) {
          //Required
          return !!value || `${key} is required`
        }
      }
    }
    return true
  },
  timeCannotBeAfterEvent: (value: string, endTime: string, key: string) => {
    if(value && endTime) {
      let time = DateTime.fromFormat(value, "HH:mm:ss")
      let end = DateTime.fromFormat(endTime, "HH:mm:ss")
      let span = end.minus({ minutes: 1 })
      let interval = Interval.fromDateTimes(span, end)
      if(interval.isBefore(time)) {
        return `${key} must be before ${end.toFormat("hh:mm a")}`
      }
    }
    return true
  },
  securityMinimumHours:(start: string, end: string) => {
    if(start && end) {
      let startTime = DateTime.fromFormat(start, "HH:mm:ss")
      let endTime = DateTime.fromFormat(end, "HH:mm:ss")
      let interval = Interval.fromDateTimes(startTime, endTime)
      let hours = interval.length('hours')
      if(hours < 3) {
        return 'Security personnel must be hired for a minimum of 3 hours'
      }
    }
    return true
  },
  dateCannotBeAfterEvent: (value: string, endDate: string, key: string) =>  {
    if(value && endDate) {
      let date = DateTime.fromFormat(`${value} 00:00:00`, "yyyy-MM-dd HH:mm:ss")
      let end = DateTime.fromFormat(`${endDate} 23:58:59`, "yyyy-MM-dd HH:mm:ss")
      let span = end.minus({ minutes: 1 })
      let interval = Interval.fromDateTimes(span, end)
      if(interval.isBefore(date)) {
        return `${key} must be before ${end.toFormat("MM/dd/yyyy")}`
      }
    }
    return true
  },
  pubStartIsValid(value: string, end: string, minPubStartDate: string, maxPubStartDate: string) {
    if(value && end) {
      let startDt = DateTime.fromFormat(value, "yyyy-MM-dd")
      let endDt = DateTime.fromFormat(end, "yyyy-MM-dd")
      let duration = Interval.fromDateTimes(startDt, endDt)
      let days = duration.count('days')
      if(days < 21) {
        return 'Publicity must run for a minimum of 3 weeks'
      }
      if(minPubStartDate) {
        let minStartDt = DateTime.fromFormat(minPubStartDate, "yyyy-MM-dd")
        if(startDt < minStartDt) {
          return `Publicity cannot start before ${minStartDt.toFormat("MM/dd/yyyy")}`
        }
      }
      if(maxPubStartDate) {
        let maxStartDt = DateTime.fromFormat(maxPubStartDate, "yyyy-MM-dd")
        if(startDt > maxStartDt) {
          return `Publicity cannot start after ${maxStartDt.toFormat("MM/dd/yyyy")}`
        }
      }
    }
    return true
  },
  pubEndIsValid(value: string, start: string, eventDates: string, minPubEndDate: string, maxPubEndDate: string) {
    if(value && start) {
      let startDt = DateTime.fromFormat(start, "yyyy-MM-dd")
      let endDt = DateTime.fromFormat(value, "yyyy-MM-dd")
      let duration = Interval.fromDateTimes(startDt, endDt)
      let days = duration.count('days')
      if(days < 21) {
        return 'Publicity must run for a minimum of 3 weeks'
      }
      if(minPubEndDate) {
        let minEndDt = DateTime.fromFormat(minPubEndDate, "yyyy-MM-dd")
        if(endDt < minEndDt) {
          return `Publicity cannot end before ${minEndDt.toFormat("MM/dd/yyyy")}`
        }
      }
      if(maxPubEndDate) {
        let maxEndDt = DateTime.fromFormat(maxPubEndDate, "yyyy-MM-dd")
        if(endDt > maxEndDt) {
          return `Publicity cannot end after ${maxEndDt.toFormat("MM/dd/yyyy")}`
        }
      }
      if(eventDates) {
        let dates = eventDates.split(",").map(d => DateTime.fromFormat(d.trim(), "yyyy-MM-dd")).sort()
        if(endDt > dates[dates.length - 1]) {
          return 'Publicity cannot end after event'
        }
      }
    }
    return true
  },

  findTense( request: ContentChannelItemBag, ministries: DefinedValueBag[] | undefined, numDays: any, specificDate?: any): String {
    if (request.attributeValues) {
      let av = request?.attributeValues.EventDates
      if (av) {
        let dates = av?.split(",").map(d => d.trim())
        if (dates && dates.length > 0) {
          let today = DateTime.now()
          let first = dates.map((i) => {
            return DateTime.fromFormat(i, 'yyyy-MM-dd')
          })?.sort().shift()?.minus({ days: numDays })
          if(specificDate) {
            if(specificDate.includes('T')) {
              specificDate = specificDate.split('T')[0]
            }
            first = DateTime.fromFormat(specificDate, 'yyyy-MM-dd').minus({ days: numDays })
          }
          let isFuneralRequest = false
          let val = request.attributeValues.Ministry
          let ministry = {} as DefinedValueBag
          if(val != '' && ministries) {
            let min = JSON.parse(val) as ListItemBag
            ministry = ministries?.filter((dv: any) => {
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
  sectionInfo: [
    { cat: "Event", section: "Time" },
    { attr: "NeedsSpace", cat: "Event Space", section: "Space" },
    { attr: "NeedsOnline", cat: "Event Online", section: "Online" },
    { attr: "NeedsCatering", cat: "Event Catering", section: "Catering" },
    { attr: "NeedsChildCare", cat: "Event Childcare", section: "Childcare" },
    { attr: "NeedsChildCareCatering", cat: "Event Childcare Catering", section: "Childcare Catering" },
    { attr: "NeedsOpsAccommodations", cat: "Event Ops Requests", section: "Ops" },
    { attr: "NeedsRegistration", cat: "Event Registration", section: "Registration" },
    { attr: "NeedsPublicity", cat: "Event Production", section: "Production" },
    { attr: "NeedsProductionAccommodations", cat: "Event Publicity", section: "Publicity" },
    { attr: "NeedsWebCalendar", cat: "Event Calendar", section: "Calendar" }
  ],
  validate(request: ContentChannelItemBag | undefined, events: ContentChannelItemBag[], locations: DefinedValueBag[] | undefined, ministries: DefinedValueBag[] | undefined, isSuperUser: boolean | undefined) {
      let requestIsValid = true
      let invalidSections = [] as string[]
      let readonlySections = [] as string[]
      if(request && request.attributeValues) {
        let dates = request.attributeValues.EventDates.split(',').map((d) => DateTime.fromFormat(d.trim(), 'yyyy-MM-dd')).sort()
        let firstDate = dates[0]
        let lastDate = dates[dates.length - 1]

        if(this.required(request.title, '') != true ||
          this.required(request.attributeValues.Contact, '') != true ||
          this.required(request.attributeValues.Ministry, '') != true ||
          this.required(request.attributeValues.EventDates, '') != true
        ) {
          requestIsValid = false
        }

        let twoWeeksTense = this.findTense(request, ministries, 14)
        let thirtyDaysTense = this.findTense(request, ministries,30)
        let pubDateCutOff = lastDate.minus({weeks: 6})
        if(request.attributeValues.PublicityStartDate) {
          pubDateCutOff = DateTime.fromISO(request.attributeValues.PublicityStartDate).minus({days: 21})
        }
        let sixWeeksTense = DateTime.now() > pubDateCutOff ? 'was' : 'is'
        let registrationFirstGoLive = ''
        if(request.attributeValues.IsSame == 'True') {
          if(events && events.length > 0 ) {
            let event = events[0]
            registrationFirstGoLive = event?.attributeValues?.RegistrationStartDate as string
          }
        } else {
          let registrationDates = events.map((e: any) => { 
            let str = e?.attributeValues?.RegistrationStartDate.trim() 
            if(str) {
              return DateTime.fromFormat(str, 'yyyy-MM-dd')
            }
          }).sort((a: any, b: any) => {
            if(a < b) {
              return -1
            } else if(a > b) {
              return 1
            }
            return 0
          })
          if(registrationDates && registrationDates.length > 0) {
            let regDate = registrationDates[0]
            registrationFirstGoLive = regDate?.toFormat('yyyy-MM-dd') as string
          }
        }
        let registrationTense = this.findTense(request, ministries,14, registrationFirstGoLive)
        let webCalTense = this.findTense(request, ministries,14, request.attributeValues.WebCalendarGoLive)

        //Drafts, cut anything that is past-deadline
        //Do this before validation so we don't have errors display for sections not on the request anymore
        if(request.attributeValues.RequestStatus == 'Draft') {
          if(twoWeeksTense == 'was') {
            request.attributeValues.NeedsOnline = 'False'
            request.attributeValues.NeedsCatering = 'False'
            request.attributeValues.NeedsOpsAccommodations = 'False'
            request.attributeValues.NeedsWebCalendar = 'False'
            request.attributeValues.NeedsProductionAccommodations = 'False'
            request.attributeValues.NeedsRegistration = 'False'
          }
          if(thirtyDaysTense == 'was') {
            request.attributeValues.NeedsChildCare = 'False'
          }
          if(sixWeeksTense == 'was') {
            request.attributeValues.NeedsPublicity = 'False'
          }
        }

        //Fields on Event 
        let submittedDate = DateTime.now()
        if(request?.attributeValues?.RequestStatus != 'Draft') {
          if(request?.startDateTime) {
            submittedDate = DateTime.fromISO(request?.startDateTime)
          }
        }
        if(request.attributeValues.NeedsPublicity == 'True') {
          let minStart = submittedDate.plus({weeks: 3})
          let minPubStartDate = minStart.toFormat("yyyy-MM-dd")
          let maxPubStartDate = firstDate.minus({weeks: 3}).toFormat("yyyy-MM-dd")
          let minPubEndDate = DateTime.fromFormat(request.attributeValues.PublicityStartDate, 'yyyy-MM-dd').plus({weeks: 3}).toFormat("yyyy-MM-dd")
          let maxPubEndDate = lastDate.toFormat("yyyy-MM-dd")
          if(this.required(request.attributeValues?.WhyAttend, '') != true ||
            this.required(request.attributeValues?.TargetAudience, '') != true ||
            this.required(request.attributeValues?.PublicityStartDate, '') != true ||
            this.pubStartIsValid(request.attributeValues?.PublicityStartDate, request.attributeValues?.PublicityEndDate, minPubStartDate, maxPubStartDate) != true ||
            this.required(request.attributeValues?.PublicityEndDate, '') != true ||
            this.pubEndIsValid(request.attributeValues?.PublicityEndDate, request.attributeValues?.PublicityStartDate, request.attributeValues.EventDates, minPubEndDate, maxPubEndDate) != true ||
            this.required(request.attributeValues?.PublicityStrategies, '') != true
          ) {
            requestIsValid = false
            let idx = invalidSections.indexOf('Publicity')
            if(idx < 0) {
              invalidSections.push('Publicity')
            }
          }
        }
        if(request.attributeValues.NeedsWebCalendar == 'True') {
          if(this.required(request.attributeValues?.WebCalendarDescription, '') != true || 
            this.required(request.attributeValues?.WebCalendarGoLive, '') != true || 
            this.dateCannotBeAfterEvent(request.attributeValues?.WebCalendarGoLive, lastDate.toFormat("yyyy-MM-dd"), '') != true
          ) {
            requestIsValid = false
            let idx = invalidSections.indexOf('Calendar')
            if(idx < 0) {
              invalidSections.push('Calendar')
            }
          }
        }
        if(request.attributeValues.NeedsProductionAccommodations == 'True') {
          if(this.required(request.attributeValues?.ProductionTech, '') != true ||
            this.required(request.attributeValues?.ProductionSetup, '') != true
          ) {
            requestIsValid = false
            let idx = invalidSections.indexOf('Production')
            if(idx < 0) {
              invalidSections.push('Production')
            }
          }
        }

        //Fields on Event Details
        if(events && events.length > 0) {
          for(let i=0; i < events.length; i++) {
            let eventIsValid = true
            if(this.required(events[i].attributeValues?.StartTime, '') != true ||
              this.required(events[i].attributeValues?.EndTime, '') != true ||
              this.timeIsValid(events[i].attributeValues?.StartTime as string, events[i].attributeValues?.EndTime as string, true) != true 
            ) {
              requestIsValid = false
              eventIsValid = false
            }
            if(request.attributeValues.NeedsSpace == 'True') {
              let attendance = events[i].attributeValues?.ExpectedAttendance as string
              let numAttendance = parseInt(attendance)
              let rooms = events[i].attributeValues?.Rooms as string
              if(this.required(events[i].attributeValues?.Rooms, '') != true ||
                this.required(events[i].attributeValues?.ExpectedAttendance, '') != true ||
                ( locations && this.attendance(numAttendance, rooms, locations, '') != true ) || 
                this.largeEventSecurity(numAttendance, request.attributeValues.NeedsOpsAccommodations, `${events[i].attributeValues?.NeedsSecurity}`, isSuperUser ) != true
              ) {
                requestIsValid = false
                eventIsValid = false
                let idx = invalidSections.indexOf('Space')
                if(idx < 0) {
                  invalidSections.push('Space')
                }
              }
            }
            if(request.attributeValues.NeedsOnline == 'True') {
              if(this.required(events[i].attributeValues?.EventURL, '') != true) {
                requestIsValid = false
                eventIsValid = false
                let idx = invalidSections.indexOf('Online')
                if(idx < 0) {
                  invalidSections.push('Online')
                }
              }
            }
            if(request.attributeValues.NeedsCatering == 'True') {
              let drinkTime = events[i].attributeValues?.DrinkTime as string
              let foodTime = events[i].attributeValues?.FoodTime as string
              let endTime = events[i].attributeValues?.EndTime as string
              let drinks = events[i].attributeValues?.Drinks as string
              if(this.required(events[i].attributeValues?.PreferredVendor, '') != true ||
                this.required(events[i].attributeValues?.FoodBudgetLine, '') != true ||
                this.required(events[i].attributeValues?.PreferredMenu, '') != true ||
                this.required(events[i].attributeValues?.FoodTime, '') != true ||
                this.timeCannotBeAfterEvent(foodTime, endTime, '') != true ||
                this.drinkTimeRequired(drinkTime, drinks, '') != true ||
                (events[i].attributeValues?.NeedsDelivery == 'True' && 
                  this.required(events[i].attributeValues?.FoodSetupLocation, '') != true) ||
                (events[i].attributeValues?.NeedsDietaryAccommodations == 'True' && 
                  this.required(events[i].attributeValues?.DietaryAccommodationInfo, '') != true
                ) 
              ) {
                requestIsValid = false
                eventIsValid = false
                let idx = invalidSections.indexOf('Catering')
                if(idx < 0) {
                  invalidSections.push('Catering')
                }
              }
            }
            if(request.attributeValues.NeedsChildCare == 'True') {
              let ccStartTime = events[i].attributeValues?.ChildcareStartTime as string
              let endTime = events[i].attributeValues?.EndTime as string
              if(this.required(events[i].attributeValues?.ChildcareStartTime, '') != true ||
                this.timeCannotBeAfterEvent(ccStartTime, endTime, '') != true ||
                this.required(events[i].attributeValues?.ChildcareEndTime, '') != true ||
                this.required(events[i].attributeValues?.ChildcareOptions, '') != true ||
                this.required(events[i].attributeValues?.EstimatedNumberofKids, '') != true
              ) {
                requestIsValid = false
                eventIsValid = false
                let idx = invalidSections.indexOf('Childcare')
                if(idx < 0) {
                  invalidSections.push('Childcare')
                }
              }
            }
            if(request.attributeValues.NeedsChildCareCatering == 'True') {
              let ccFoodTime = events[i].attributeValues?.ChildcareFoodTime as string
              let endTime = events[i].attributeValues?.EndTime as string
              if(this.required(events[i].attributeValues?.ChildcareVendor, '') != true ||
                this.required(events[i].attributeValues?.ChildcareCateringBudgetLine, '') != true ||
                this.required(events[i].attributeValues?.ChildcarePreferredMenu, '') != true ||
                this.required(events[i].attributeValues?.ChildcareFoodTime, '') != true ||
                this.timeCannotBeAfterEvent(ccFoodTime, endTime, '') != true
              ) {
                requestIsValid = false
                eventIsValid = false
                let idx = invalidSections.indexOf('Childcare Catering')
                if(idx < 0) {
                  invalidSections.push('Childcare Catering')
                }
              }
            }
            if(request.attributeValues.NeedsRegistration == 'True') {
              let regStartDate = events[i].attributeValues?.RegistrationStartDate as string
              let regEndDate = events[i].attributeValues?.RegistrationEndDate as string
              let lastDate = events[i].attributeValues?.EventDate as string
              if(lastDate == '') {
                let dates = request.attributeValues.EventDates.split(",").map((d: string) => d.trim())
                if(dates && dates.length > 0) {
                  lastDate == dates[dates.length - 1]
                }
              }
              if(this.required(events[i].attributeValues?.RegistrationStartDate, '') != true ||
                this.dateCannotBeAfterEvent(regStartDate, lastDate, '') != true ||
                this.required(events[i].attributeValues?.RegistrationFeeType, '') != true ||
                (events[i].attributeValues?.RegistrationFeeType.split(",").includes('Online Fee') && this.required(events[i].attributeValues?.OnlineRegistrationFee, '') != true) ||
                (events[i].attributeValues?.RegistrationFeeType.split(",").includes('Fee per Individual') && this.required(events[i].attributeValues?.IndividualRegistrationFee, '') != true) ||
                (events[i].attributeValues?.RegistrationFeeType.split(",").includes('Fee per Couple') && this.required(events[i].attributeValues?.CoupleRegistrationFee, '') != true) ||
                this.required(events[i].attributeValues?.RegistrationEndDate, '') != true ||
                this.dateCannotBeAfterEvent(regEndDate, lastDate, '') != true ||
                this.required(events[i].attributeValues?.RegistrationEndTime, '') != true ||
                this.required(events[i].attributeValues?.RegistrationConfirmationEmailSender, '') != true ||
                this.required(events[i].attributeValues?.RegistrationConfirmationEmailAdditionalDetails, '') != true ||
                (events[i].attributeValues?.NeedsReminderEmail == 'True' && this.required(events[i].attributeValues?.RegistrationReminderEmailAdditionalDetails, '') != true)
              ) {
                requestIsValid = false
                eventIsValid = false
                let idx = invalidSections.indexOf('Registration')
                if(idx < 0) {
                  invalidSections.push('Registration')
                }
              }
            }
            let opsAttrs = [] as string[]
            let attrs = events[i].attributes
            for(let attr in attrs) {
              let categories = attrs[attr].categories as any[]
              if(categories.map((c: any) => c.name).includes('Event Ops Requests')) {
                opsAttrs.push(attr)
              }
            }
            let opsIsValid = false
            for(let attr in opsAttrs) {
              let event = events[i]
              if(event.attributeValues && (event.attributeValues[attr] != '' && event.attributeValues[attr] != 'False')) {
                opsIsValid = true
              }
            }
            if(request.attributeValues.NeedsOpsAccommodations == 'True') {
              if(events[i].attributeValues?.NeedsSecurity == 'True') {
                if(this.required(events[i].attributeValues?.SecurityBudgetMinistry, '') != true ||
                  this.required(events[i].attributeValues?.SecurityBudgetLine, '') != true ||
                  this.required(events[i].attributeValues?.SecurityStartTime, '') != true ||
                  this.required(events[i].attributeValues?.SecurityEndTime, '') != true ||
                  this.securityMinimumHours(`${events[i].attributeValues?.SecurityStartTime}`, `${events[i].attributeValues?.SecurityEndTime}`) != true
                ) {
                  opsIsValid = false
                }
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

            let event = events[i]
            if(event.attributeValues) {
              event.attributeValues.EventIsValid = eventIsValid ? 'True' : 'False'
            }
          }
        }

        //Remove/Readonly Sections
        if (request.attributeValues.RequestStatus == 'Submitted' || request.attributeValues.RequestStatus == 'In Progress'){
          //If the request is Submitted or In Progress, only remove if the section is invalid
          if(twoWeeksTense == 'was') {
            if(invalidSections.includes('Online')) {
              request.attributeValues.NeedsOnline = 'False'
            }
            if(invalidSections.includes('Catering')) {
              request.attributeValues.NeedsCatering = 'False'
            }
            if(invalidSections.includes('Childcare Catering')) {
              request.attributeValues.NeedsChildCareCatering = 'False'
            }
            if(invalidSections.includes('Production')) {
              request.attributeValues.NeedsProductionAccommodations = 'False'
            }
            if(invalidSections.includes('Ops')) {
              request.attributeValues.NeedsOpsAccommodations = 'False'
            }
          }
          if(registrationTense == 'was') {
            if(invalidSections.includes('Registration')) {
              request.attributeValues.NeedsRegistration = 'False'
            }
          }
          if(webCalTense == 'was') {
            if(invalidSections.includes('Calendar')) {
              request.attributeValues.NeedsWebCalendar = 'False'
            }
          }
          if(thirtyDaysTense == 'was' && invalidSections.includes('Childcare')) {
            request.attributeValues.NeedsChildCare = 'False'
          }
          if(sixWeeksTense == 'was' && invalidSections.includes('Publicity')) {
            request.attributeValues.NeedsPublicity = 'False'
          }
        } else {
          if(twoWeeksTense == 'was') {
            readonlySections.push('Online')
            readonlySections.push('Catering')
            readonlySections.push('Childcare Catering')
            readonlySections.push('Ops')
            readonlySections.push('Production')
          }
          if(registrationTense == 'was') {
            readonlySections.push('Registration')
          }
          if(webCalTense == 'was') {
            readonlySections.push('Calendar')
          }
          if(thirtyDaysTense == 'was') {
            readonlySections.push('Childcare')
          }
          if(sixWeeksTense == 'was') {
            readonlySections.push('Publicity')
          }
        }
        console.log('Invalid Sections')
        console.log(invalidSections)
        console.log('ReadOnly Sections')
        console.log(readonlySections)
        
        // let sectionInfo = [
        //   { attr: "NeedsSpace", cat: "Event Space", section: "Space" },
        //   { attr: "NeedsOnline", cat: "Event Online", section: "Online" },
        //   { attr: "NeedsCatering", cat: "Event Catering", section: "Catering" },
        //   { attr: "NeedsChildCare", cat: "Event Childcare", section: "Childcare" },
        //   { attr: "NeedsChildCareCatering", cat: "Event Childcare Catering", section: "Childcare Catering" },
        //   { attr: "NeedsOpsAccommodations", cat: "Event Ops Requests", section: "Ops" },
        //   { attr: "NeedsPublicity", cat: "Event Production", section: "Production" },
        //   { attr: "NeedsProductionAccommodations", cat: "Event Publicity", section: "Publicity" },
        //   { attr: "NeedsRegistration", cat: "Event Registration", section: "Calendar" }
        // ]
        let invalidCategories = [] as string[]
        let readOnlyCategories = [] as string[]
        if(invalidSections.length > 0) {
          invalidCategories = this.sectionInfo.filter((si: any) => {
            return invalidSections.includes(si.section)
          }).map(si => si.section)
        }
        if(readOnlyCategories.length > 0) {
          readOnlyCategories = this.sectionInfo.filter((si: any) => {
            return readOnlyCategories.includes(si.section)
          }).map(si => si.section)
        }

        request.attributeValues.RequestIsValid = requestIsValid ? 'True' : 'False'
        return { isValid: requestIsValid, invalidSections: invalidCategories, readonlySections: readOnlyCategories }
      }
  }
}
export default rules