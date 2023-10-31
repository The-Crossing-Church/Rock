// import { DateTime, Interval } from "luxon"

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
}
export default rules