export default {
  filters: {
    formatDateTime(val) {
      return moment(val).format("MM/DD/yyyy hh:mm A");
    },
    formatDate(val) {
      return moment(val).format("MM/DD/yyyy");
    },
    formatCurrency(val) {
      var formatter = new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD",
      });
      return formatter.format(val);
    },
  },
  methods: {
    boolToYesNo(val) {
      if (val) {
        return "Yes";
      }
      return "No";
    },
    formatDates(val) {
      if (val) {
        let dates = [];
        val.forEach((i) => {
          dates.push(moment(i).format("MM/DD/yyyy"));
        });
        return dates.join(", ");
      }
      return "";
    },
    formatRooms(val) {
      if (val) {
        let rms = [];
        val.forEach((i) => {
          this.rooms.forEach((r) => {
            if (i == r.Id) {
              rms.push(r.Value);
            }
          });
        });
        return rms.join(", ");
      }
      return "";
    },
    formatDoors(val) {
      if (val) {
        let drs = [];
        val.forEach((i) => {
          this.doors.forEach((r) => {
            if (i == r.Id) {
              drs.push(r.Value);
            }
          });
        });
        return drs.join(", ");
      }
      return "";
    },
    formatMinistry(val) {
      if (val) {
        let formattedVal = this.ministries.filter(m => {
          return m.Id == val
        })
        if(formattedVal && formattedVal.length > 0) {
          return formattedVal[0].Value
        } 
      }
      return "";
    },
    formatBudgetLine(val) {
      if (val) {
        let formattedVal = this.budgetLines.filter(m => {
          return m.Id == val
        })
        if(formattedVal && formattedVal.length > 0) {
          return formattedVal[0].Value
        } else {
          return val
        }
      }
      return "";
    },
    formatCurrency(val) {
      var formatter = new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD",
      });
      return formatter.format(val);
    },
    formatDate(val) {
      return moment(val).format("MM/DD/yyyy");
    },
    formatList(val) {
      return val.join(", ")
    },
    requestType(itm) {
      if (itm) {
        let resources = [];
        if (itm.needsSpace) {
          resources.push("Room");
        }
        if (itm.needsOnline) {
          resources.push("Online Event");
        }
        if (itm.needsPub) {
          resources.push("Publicity");
        }
        if (itm.needsReg) {
          resources.push("Registration");
        }
        if (itm.needsChildCare) {
          resources.push("Childcare");
        }
        if (itm.needsCatering) {
          resources.push("Catering");
        }
        if (itm.needsAccom) {
          resources.push("Extra Resources");
        }
        return resources.join(", ");
      }
      return "";
    },
    invalidSections(request) {
      if(request.ValidSections) {
        let requested = this.requestType(request).split(", ")
        return requested.filter(r => !request.ValidSections.includes(r) ).join(", ")
      } else {
        return ""
      }
    }
  }
}