import { DateTime, Interval } from "luxon"

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
  }
}
export default rules