import AntSelect from "ant-design-vue/lib/select"
import AntAutoComplete from "ant-design-vue/lib/auto-complete"
import AntSteps from "ant-design-vue/lib/steps"
import AntButton from "ant-design-vue/lib/button"
import AntModal from "ant-design-vue/lib/modal"
import AntSwitch from "ant-design-vue/lib/switch"
import AntDropDown from "ant-design-vue/lib/dropdown"
import AntMenu from "ant-design-vue/lib/menu"
import AntInput from "ant-design-vue/lib/input"
import AntTable from "ant-design-vue/lib/table"
import AntPopover from "ant-design-vue/lib/popover"
import AntBadge from "ant-design-vue/lib/badge"
import * as Axios from "axios"
import { DateTime, Duration, Interval, FixedOffsetZone } from "luxon/src/luxon"
import * as Mitt from "mitt"
import * as Vue from "vue"
import * as TSLib from "tslib"

// This shrinks the bundle by 11KB over just importing all of luxon.
const Luxon = {
    DateTime,
    Duration,
    Interval,
    FixedOffsetZone
};

// Only include the components we are actually going to use.
const AntDesignVue = {
    Select: AntSelect,
    AutoComplete: AntAutoComplete,
    Steps: AntSteps,
    Button: AntButton,
    Modal: AntModal,
    Switch: AntSwitch,
    Dropdown: AntDropDown,
    Menu: AntMenu,
    Input: AntInput,
    Table: AntTable,
    Popover: AntPopover,
    Badge: AntBadge
};

export {
    AntDesignVue, // 284KB
    Axios, // 13.7KB
    Luxon, // 60KB
    Mitt, // 374b
    Vue, // 127.2KB
    TSLib, // 6.8KB
};
