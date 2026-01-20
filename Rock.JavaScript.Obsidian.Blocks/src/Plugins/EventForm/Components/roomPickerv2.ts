import { defineComponent } from "vue"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import Modal from "@Obsidian/Controls/modal.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"
import TreeItemPicker from "./treeItemPicker.obs"

export default defineComponent({
    name: "EventForm.Components.RoomPicker",
    components: {
      "rck-btn": RockButton,
      "rck-modal": Modal,
      "rck-lbl": RockLabel,
      "tcc-tree-pkr": TreeItemPicker,
      VNodes: (_, { attrs }) => {
          return attrs.vnodes;
      }
    },
    props: {
      modelValue: String,
      label: {
          type: String,
          required: false
      },
      disabled: {
          type: Boolean,
          required: false
      },
      hint: {
          type: String,
          required: false
      },
      persistentHint: {
          type: Boolean,
          required: false
      },
      multiple: {
          type: Boolean,
          required: false
      },
      items: {
          type: Array,
          required: true
      },
      icon: {
          type: String,
          required: false
      },
      checkBoxes: {
          type: Boolean,
          required: false
      },
      id: String
    },
    setup() {
    },
    data() {
      return {
        selectedValue: [] as any[],
        search: '',
        menuOpen: false,
        map: false
      }
    },
    computed: {
      options() {
        let selectedValues = this.selectedValue.map((i: any) => i.value)
        let data = [] 
        data.push(...this.items.filter((i: any) => i.isHeader)
          .map((grp: any) => ({ 
            text: grp.value, 
            value: grp.value, 
            type: 'header',
            isActive: true,
            // iconCssClass: 'ti ti-square',
            children: this.items.filter((i:any) => i.type == grp.value)
              .map((i: any) => ({ 
                text: i.text, 
                value: i.value, 
                type: 'item',
                isActive: !i.isDisabled, 
                // iconCssClass: 'ti ti-square',
                description: i.description
              })),
            hasChildren: this.items.filter((i:any) => i.type == grp.value).length > 0
          }))
        )
        data.forEach((grp: any) => {
          if(grp.hasChildren) {
            let disabledChildren = grp.children.filter((i: any) => !i.isActive)
            if(disabledChildren.length == grp.children.length) {
              grp.isActive = false
            }
          } 
        })
        return data
      },
      headings() {
        return this.options.map((i: any) => i.value)
      }
    },
    methods: {
      
    },
    watch: {
      selectedValue: { 
        handler (val, oval) {
          if (val) {
            console.log('Selected Val Watch')
            console.log(val)
            let rockVal = {
              value: val.map((i: any) => i.value).join(","),
              text: val.map((i: any) => i.text.split(" (")[0]).join(", ")
            }
            this.$emit('update:modelValue', JSON.stringify(rockVal))
          }
        },
        deep: true
      },
      modelValue(val) {
        let parsed = JSON.parse(val)
        let roomGuids = parsed.value.split(',')
        if(parsed.value != this.selectedValue.map((i: any) => i.value).join(",")) {
          this.selectedValue = this.items.filter((i: any) =>  roomGuids.includes(i.value)).map((i: any) => { return { text: i.text, value: i.value }})
        }
      },
      items: {
        handler(val) {
          //When list of rooms is updated make sure any now disabled rooms are removed from selection
          let selectedGuids = this.selectedValue.map((i: any) => i.value)
          let selectedRooms = val.filter((i: any) => {
            if(!i.isDisabled) {
              return selectedGuids.includes(i.value)
            }
          }).sort((i: any) => i.order)
          this.selectedValue = selectedRooms
        },
        deep: true
      }
    },
    mounted() {
      if (this.modelValue) {
        let parsed = JSON.parse(this.modelValue)
        if(parsed.value) {
          let roomGuids = parsed.value.split(',')
          this.selectedValue = this.items.filter((i: any) => !i.isDisabled && roomGuids.includes(i.value)).map((i: any) => { return { text: i.text, value: i.value }})
        } else {
          this.selectedValue = []
        }
      }
      let els = document.querySelectorAll(".tcc-text-display")
      els.forEach((el: any) => {
        el.setAttribute("readonly", "")
      })
    },
    template: `
<div class="form-group">
  <rck-lbl>{{label}}</rck-lbl>
  <div style="display: flex; align-items: center;">
    <rck-btn btnType="accent" @click="map = true" class="mr-1 btn-circle" :id="'btn' + id">
      <i class="fas fa-map-marked-alt"></i>
    </rck-btn>
    <div style="width: -webkit-fill-available;">
      <tcc-tree-pkr
        v-model="selectedValue"
        :items="options"
        :initiallyExpanded="headings"
        enhanceForLongLists
        multiple
        autoExpand
        fullWidth
        iconCssClass="ti ti-map2"
        :disabled="disabled"
      >
        <template #pickerContentSuperHeader>
          <b>Room (Capacity)</b>
        </template>
        <template #primaryButtonLabel>
          Ok
        </template>
      </tcc-tree-pkr>
    </div>
  </div>
</div>
<rck-modal v-model="map" style="min-width: 75%;" :isCloseButtonHidden="true" cancelText="" :clickBackdropToClose="true" modalWrapperClasses="modal-no-header">
  <img src="https://rock.thecrossingchurch.com/Content/Operations/Campus%20Map.png" style="width: 100%;"/>
  <template #footer>
    <rck-btn btnType="grey" @click="map = false">Close</rck-btn>
  </template>
</rck-modal>
`
});
