import { defineComponent, PropType } from "vue"
import { DefinedValue, DefinedType, Person } from "../../../../ViewModels"
import RockText from "../../../../Elements/textBox"
import RockDDL from "../../../../Elements/dropDownList"
import RockLabel from "../../../../Elements/rockLabel"
import RockField from "../../../../Controls/rockField"
import RockCheckbox from "../../../../Elements/checkBox"
import PhoneNumberBox from "../../../../Elements/phoneNumberBox"

export default defineComponent({
    name: "Checkin.Components.ContactInfo",
    components: {
      "rck-txt": RockText,
      "rck-ddl": RockDDL,
      "rck-lbl": RockLabel,
      "rck-field": RockField,
      "rck-chk": RockCheckbox,
      "rck-phn": PhoneNumberBox
    },
    props: {
      showEmail: Boolean,
      showEmailOptOut: Boolean,
      showCell: Boolean,
      showSMS: Boolean,
      person: {
        type: Object as PropType<Person>,
        required: false
      }
    },
    setup() {

    },
    data() {
      return {
        
      }
    },
    computed: {
      colWidth() {
        if(this.showEmail && this.showCell) {
          return { small: 12, normal: 6 }
        } 
        return { small: 6, normal: 12 }
      }
    },
    methods: {
      checkForDuplicates() {
        this.$emit('checkForDuplicates')
      }
    },
    watch: {
      
    },
    mounted() {

    },
    template: `
<div class="row mt-4">
  <div :class="'col col-xs-12 col-md-' + colWidth.normal" v-if="showEmail">
    <div class="row">
      <div :class="'col col-xs-12 col-md-' + colWidth.small">
        <rck-lbl>Email</rck-lbl>
        <rck-txt
          v-model="person.Email"
          v-on:blur="checkForDuplicates"
          type="email"
        ></rck-txt>
      </div>
      <div :class="'col col-xs-12 col-md-' + colWidth.small" v-if="showEmailOptOut">
        <rck-chk
          label="Opt out of mass email communications"
        ></rck-chk>
      </div>
    </div>
  </div>
  <div :class="'col col-xs-12 col-md-' + colWidth.normal" v-if="showCell">
    <div class="row">
      <div :class="'col col-xs-12 col-md-' + colWidth.small">
        <rck-lbl>Mobile Phone Number</rck-lbl>
        <rck-phn
          v-model="person.phoneNumbers[0].numberFormatted"
          type="phone"
          v-on:blur="checkForDuplicates"
        ></rck-phn>
      </div>
      <div :class="'col col-xs-12 col-md-' + colWidth.small" v-if="(person.id > 0 && !person.phoneNumbers[0].isMessagingEnabled)">
        <rck-chk
          label="SMS Enabled"
          v-model="person.phoneNumbers[0].isMessagingEnabled"
        ></rck-chk>
      </div>
    </div>
  </div>
</div>
<div class="alert alert-danger mt-3" v-if="person.phoneNumberCantBeMessaged || !person.phoneNumbers[0].isMessagingEnabled">
  <div v-if="!person.phoneNumbers[0].isMessagingEnabled">
    {{person.nickName}} has SMS disabled for their mobile phone, they will not be able to receive alerts from the CK Desk if something were to happen to their child. Please confirm with the parent we can enable messaging for their device.
  </div>
  <div v-if="person.phoneNumberCantBeMessaged">
    {{person.nickName}} has replied STOP to CK Desk Alerts, they will not be able to receive alerts from the CK Desk if something were to happen to their child. The parent will need to text the word "START" to 573-397-7375 to resubscribe them to the CK Desk number.
  </div>
</div>
`
});
