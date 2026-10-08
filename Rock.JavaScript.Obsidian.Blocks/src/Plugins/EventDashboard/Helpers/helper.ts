import { DateTime } from "luxon"

const helper = {
  sectionInfo: [
    {                                        cat: "Event",                         section: "Time",                   type: "Event"              },
    { attr: "NeedsSpace",                    cat: "Event Space",                   section: "Space",                  type: "Room"               },
    { attr: "NeedsOnline",                   cat: "Event Online",                  section: "Online",                 type: "Online Event"       },
    { attr: "NeedsCatering",                 cat: "Event Catering",                section: "Catering",               type: "Catering"           },
    { attr: "NeedsChildCare",                cat: "Event Childcare",               section: "Childcare",              type: "Childcare"          },
    { attr: "NeedsChildCareCatering",        cat: "Event Childcare Catering",      section: "Childcare Catering",     type: "Childcare Catering" },
    {                                        cat: "Event Childcare Registration",  section: "Childcare Registration", type: "Childcare"          },
    { attr: "NeedsOpsAccommodations",        cat: "Event Ops Requests",            section: "Ops",                    type: "Extra Resources"    },
    { attr: "NeedsRegistration",             cat: "Event Registration",            section: "Registration",           type: "Registration"       },
    { attr: "NeedsPublicity",                cat: "Event Publicity",               section: "Publicity",              type: "Publicity"          },
    { attr: "NeedsProductionAccommodations", cat: "Event Production",              section: "Production",             type: "Production"         },
    { attr: "NeedsWorship",                  cat: "Event Worship",                 section: "Worship",                type: "Worship"            },
    { attr: "NeedsWebCalendar",              cat: "Event Calendar",                section: "Calendar",               type: "Web Calendar"       }
  ],
  requestStatuses: [
    { text: "Draft",                    value: "Draft"                    },
    { text: "Submitted",                value: "Submitted"                },
    { text: "In Progress",              value: "In Progress"              },
    { text: "Pending Confirmation",     value: "Pending Confirmation"     },
    { text: "Approved",                 value: "Approved"                 },
    { text: "Confirmed",                value: "Confirmed"                },
    { text: "Denied",                   value: "Denied"                   },
    { text: "Cancelled",                value: "Cancelled"                },
    { text: "Pending Changes",          value: "Pending Changes"          },
    { text: "Proposed Changes Denied",  value: "Proposed Changes Denied"  },
    { text: "Changes Accepted by User", value: "Changes Accepted by User" },
    { text: "Cancelled by User",        value: "Cancelled by User"        }
  ],
  resources: [
    { text: "Room",               value: "Room"               },
    { text: "Online Event",       value: "Online Event"       },
    { text: "Catering",           value: "Catering"           },
    { text: "Childcare",          value: "Childcare"          },
    { text: "Childcare Catering", value: "Childcare Catering" },
    { text: "Extra Resources",    value: "Extra Resources"    },
    { text: "Registration",       value: "Registration"       },
    { text: "Web Calendar",       value: "Web Calendar"       },
    { text: "Production",         value: "Production"         },
    { text: "Worship",            value: "Worship"            },
    { text: "Publicity",          value: "Publicity"          }
  ],
  //Date Helpers
  dateFromString(value: string | null | undefined) {
    if(value) {
      if(value.includes('T')) {
        value = value.split('T')[0]
      }
      return DateTime.fromFormat(value, 'yyyy-MM-dd')
    } else {
      return null
    }
  },
  formatDateTime: (date: any): string => {
    if(date) {
      return DateTime.fromISO(date).toFormat("MM/dd/yyyy hh:mm a");
    }
    return ""
  },
  formatDate: (date: any): string => {
    if(date) {
      return DateTime.fromISO(date).toFormat("MM/dd/yyyy");
    }
    return ""
  },
  formatLongDate: (date: any): string => {
    return DateTime.fromFormat(date, "yyyy-MM-dd").toFormat("DDDD")
  },
  formatDates: (dates: string): string => {
    if(dates) {
      let dateArray = dates.split(",").map((d: string) => DateTime.fromFormat(d.trim(), "yyyy-MM-dd").toFormat("MM/dd/yyyy"))
      return dateArray.join(", ")
    }
    return ""
  },
  //Grid Helpers
  getDatesFilterValues: (row: any) => {
    if(row?.attributeValues) {
      let dates = row.attributeValues.EventDates.split(",")
      if(dates) {
        return dates.map(d => {
          return { value: d.trim(), rowData: row }
        })
      }
    }
    return []
  },
  getDatesSortValue: (row: any) => {
    if(row?.attributeValues) {
      let datesRaw = row.attributeValues.EventDates.split(",")
      if(datesRaw) {
        let dates = datesRaw.map(d => {
          return DateTime.fromFormat(d.trim(), 'yyyy-MM-dd')
        }).sort()
        if(dates && dates.length > 0) {
          return dates[0]
        }
      }
    }
    return -1
  },
  getRequestTypeFilterValues: (row: any) => {
    if(row?.attributeValues) {
      let requestedResources = row.attributeValues.RequestType.split(",")
      if(requestedResources) {
        return requestedResources.map(d => {
          return { value: d.trim(), rowData: row }
        })
      }
    }
    return []
  },
  getRequestStatusFilterValue: (row: any) => {
    if(row?.attributeValues) {
      return row.attributeValues.RequestStatus
    }
    return ""
  },
  getRequestStatusSortValue: (row: any) => {
    if(row?.attributeValues) {
      return helper.requestStatuses.findIndex(rs => rs.text === row.attributeValues.RequestStatus)
    }
    return -1
  },
}
export default helper