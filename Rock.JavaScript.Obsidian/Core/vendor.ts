import * as Axios from "axios";
import { DateTime, Duration, Interval } from "luxon/src/luxon";
import * as Mitt from "mitt";
import * as Vue from "vue";
import * as AntDesignVue from "ant-design-vue";
import * as VueDatePicker from "@vuepic/vue-datepicker";

// This shrinks the bundle by 11KB over just importing all of luxon.
const Luxon = {
    DateTime,
    Duration,
    Interval
};

export {
    AntDesignVue,
    Axios, // 13.7KB
    Luxon, // 60KB
    Mitt, // 374b
    Vue, // 127.2KB
    VueDatePicker
};
