import { defineComponent } from "vue";
import { Modal, Button, TreeSelect } from "ant-design-vue";
import RockLabel from "@Obsidian/Controls/rockLabel";

const { TreeNode } = TreeSelect

type ListItem = {
  label: string,
  value: string,
  description: string,
  isDisabled: boolean,
  isHeader: boolean,
  order: number
}


export default defineComponent({
    name: "EventForm.Components.RoomPicker",
    components: {
      "a-btn": Button,
      "a-modal": Modal,
      "a-tree-select": TreeSelect,
      "a-tree-select-item": TreeNode,
      "rck-lbl": RockLabel,
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
        selectedValue: [] as string[],
        search: '',
        menuOpen: false,
        map: false
      }
    },
    computed: {
      options() {
        let data = [{
          label: 'Room (Capacity)',
          value: 'Room (Capacity)',
          isHeader: true,
          disabled: true
        }] 
        data.push(...this.items.filter((i: any) => i.isHeader)
          .map((grp: any) => ({ 
            label: grp.value, 
            value: grp.value, 
            isHeader: true,
            disabled: false,
            children: this.items.filter((i:any) => i.type == grp.value)
              .map((i: any) => ({ 
                label: i.text, 
                value: i.value, 
                isHeader: false,
                disabled: i.isDisabled, 
                description: i.description
              })) })))
        return data
      }
    },
    methods: {
      
    },
    watch: {
      selectedValue: { 
        handler (val) {
          if (val) {
            let selectedGuids = val
            let selectedRooms = this.items.filter((i: any) => {
              if(!i.isDisabled) {
                return selectedGuids.includes(i.value)
              }
            }).sort((i: any) => i.order)
            let rockVal = {
              value: selectedRooms.map((i: any) => i.value).join(","),
              text: selectedRooms.map((i: any) => i.text.split(" (")[0]).join(", ")
            }
            this.$emit('update:modelValue', JSON.stringify(rockVal))
          }
        },
        deep: true
      },
      modelValue(val) {
        let parsed = JSON.parse(val)
        if(parsed.value != this.selectedValue.join(",")) {
          this.selectedValue = parsed.value.split(",")
        }
      },
      items: {
        handler(val) {
          //When list of rooms is updated make sure any now disabled rooms are removed from selection
          let selectedGuids = this.selectedValue
          let selectedRooms = val.filter((i: any) => {
            if(!i.isDisabled) {
              return selectedGuids.includes(i.value)
            }
          }).sort((i: any) => i.order)
          this.selectedValue = selectedRooms.map((i: any) => i.value)
        },
        deep: true
      }
    },
    mounted() {
      if (this.modelValue) {
        let parsed = JSON.parse(this.modelValue)
        if(parsed.value) {
          this.selectedValue = parsed.value.split(",")
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
<rck-lbl>{{label}}</rck-lbl>
<div style="display: flex; align-items: center;">
  <a-btn shape="circle" type="accent" @click="map = true" class="mr-1" :id="'btn' + id">
    <i class="fas fa-map-marked-alt"></i>
  </a-btn>
  <div style="width: -webkit-fill-available;">
    <a-tree-select
      v-model:value="selectedValue"
      style="width: 100%;"
      :tree-data="options"
      tree-checkable
      allow-clear
      multiple
      treeDefaultExpandAll
      tree-node-filter-prop="label"
    >
      <template #switcherIcon>
        <i class="fa fa-angle-down"></i>
        <i class="fa fa-angle-right"></i>
      </template>
      <template #title="{ value, label, isHeader, description, disabled }">
        <div v-if="isHeader && disabled" class="tcc-menu-header tcc-menu-banner">
          Room (Capacity)
        </div>
        <span v-else-if="isHeader" class="tcc-dropdown-header hover">
          {{label}}
        </span>
        <template v-else>
          <span class="tcc-dropdown-item-content hover">
            {{label}}
          </span> 
          <br v-if="description" />
          <span v-if="description" class="tcc-dropdown-item-description hover">
            {{description}}
          </span>
        </template>
      </template>
    </a-tree-select>
  </div>
</div>
<a-modal v-model:visible="map" style="min-width: 75%;">
  <img src="https://rock.thecrossingchurch.com/Content/Operations/Campus%20Map.png" style="width: 100%;"/>
  <template #footer>
    <a-btn type="grey" @click="map = false">Close</a-btn>
  </template>
</a-modal>
<v-style>
  .tcc-menu-header {
    position: sticky;
    top: 0;
    z-index: 10;
    margin: -4px;
    margin-top: -16px;
    margin-bottom: -16px;
    background-color: #fff;
  }
  .tcc-menu-search {
    display: flex;
    align-items: center;
  }
  .tcc-menu-search .control-wrapper {
    width: 95%;
  }
  .tcc-menu-banner {
    font-weight: 500;
    background-color: #347689 !important;
    border-color: #347689 !important;
    color: #fff;
    padding: 4px 12px;
    font-size: 16px;
  }
  .tcc-dropdown-item {
    padding: 4px;
    cursor: pointer;
    display: flex;
    align-items: center;
  }
  .tcc-dropdown-item .ant-dropdown-menu-title-content {
    display: flex;
    align-items: center;
  }
  .tcc-dropdown-header {
    color: #347689;
    font-weight: bold;
    font-size: 1.2em;
    line-height: 1.2;
  }
  .tcc-dropdown-item-content {
    font-weight: 500;
    font-size: 16px;
  }
  .tcc-dropdown-item-description {
    font-size: .875rem;
    line-height: 1.2;
    font-weight: normal;
    display: inline-block;
    padding-left: 16px;
  }
  .ant-select-selection-item .tcc-dropdown-item-description {
    display: none;
  }
  .tcc-dropdown-item:hover {
    background-color: #EEEFEF;
  }
  .tcc-dropdown-item.disabled, .tcc-dropdown-item.disabled .tcc-checkbox .far, .tcc-dropdown-item.disabled .tcc-checkbox .fa {
    color: rgba(0,0,0,.26)!important;
    cursor: not-allowed;
  }
  .tcc-dropdown-item.active {
    color: #347689;
    background-color: #E8EEF1;
  }
  .fa-angle-right, .fa-angle-down {
    margin-right: 5px;
  }
  .ant-select-tree-switcher_open .fa-angle-right {
    display: none;
  }
  .ant-select-tree-switcher_close .fa-angle-down {
    display: none;
  }

  /* Ant Design Overrides */
  .ant-checkbox-indeterminate .ant-checkbox-inner::after, .ant-checkbox-indeterminate .ant-checkbox-inner span {
    background-color: #8ED2C9;
  }
  .ant-checkbox-input:focus + .ant-checkbox-inner, .ant-checkbox-checked::after, .ant-select:not(.ant-select-disabled):hover .ant-select-selector {
    border-color: #8ED2C9;
  }
  .ant-select-focused {
    border: 1px solid #748b92 !important;
    outline: none !important;
    box-shadow: 0 0 0 2px #cfd7da !important;
  }
  .ant-select-tree-checkbox-checked .ant-select-tree-checkbox-inner {
    background-color: #347689;
    border-color: #347689;
  }
  .ant-select-focused:not(.ant-select-disabled).ant-select:not(.ant-select-customize-input) .ant-select-selector, .ant-select-focused:not(.ant-select-disabled).ant-select-multiple .ant-select-selector {
    border: none !important;
    outline: none !important;
  }
  .ant-select:not(.ant-select-customize-input) .ant-select-selector, .ant-select {
    border-radius: 4px;
  }
  .ant-select-multiple .ant-select-selector, .ant-select-selector {
    padding: 5px 12px;
  }

  /* Scrollbar */
  ul.tcc-dropdown::-webkit-scrollbar {
    width: 5px;
    border-radius: 3px;
  }
  ul.tcc-dropdown::-webkit-scrollbar-track {
    background: #bfbfbf;
    -webkit-box-shadow: inset 1px 1px 2px rgba(0,0,0,0.1);
  }
  ul.tcc-dropdown::-webkit-scrollbar-thumb {
    background: rgb(224, 224, 224);
    -webkit-box-shadow: inset 1px 1px 2px rgba(0,0,0,0.2);
  }
  ul.tcc-dropdown::-webkit-scrollbar-thumb:hover {
    background: #AAA;
  }
  ul.tcc-dropdown::-webkit-scrollbar-thumb:active {
    background: #888;
    -webkit-box-shadow: inset 1px 1px 2px rgba(0,0,0,0.3);
  }
  .ant-select-tree-indent-unit {
    padding-left: 14px;
  }
</v-style>
`
});
