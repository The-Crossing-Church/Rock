import { defineComponent, PropType } from "vue"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import TextBox from "@Obsidian/Controls/textBox.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import DDL from "@Obsidian/Controls/dropDownList.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"
import Checkbox from "@Obsidian/Controls/checkBox.obs"
import Modal from "@Obsidian/Controls/modal.obs"
import Validator from "./validator"
import rules from "../Rules/rules"

type RoomSetUp = {
  TypeofTable: string,
  NumberofTables: number,
  NumberofChairs: number,
  NeedsTablecloths: boolean,
  Rotation: number
}

export default defineComponent({
  name: "EventForm.Components.EventBuffer",
  components: {
    "rck-text": TextBox,
    "rck-lbl": RockLabel,
    "rck-ddl": DDL,
    "rck-btn": RockButton,
    "rck-modal": Modal,
    "rck-chk": Checkbox,
    "tcc-validator": Validator,
  },
  props: {
  e: {
    type: Object as PropType<ContentChannelItemBag>,
    required: false
  }
  },
  setup() {

  },
  data() {
    return {
      roomImg: "",
      roomSetUp: {} as RoomSetUp,
      dragItem: {} as any,
      items: [] as any[],
      modal: false,
      rules: rules,
      errors: []
    };
  },
  computed: {
    typeOfTableRules() {
      if(this.roomSetUp.NumberofTables > 0) {
        return [this.rules.required(this.roomSetUp.TypeofTable, 'Type of Table')]
      }
      return []
    }
  
  },
  methods: {
    newTable() {
      this.roomSetUp = { Rotation: 0 } as RoomSetUp
      this.modal = true
    },
    addTable() {
      this.modal = false
      let identifier = this.roomSetUp.TypeofTable + '_' + this.items.length
      let table = { id: identifier, tableType: this.roomSetUp.TypeofTable, offsetX: 0, offsetY: 0, clientX: 0, clientY: 0, rotate: this.roomSetUp.Rotation, clothed: this.roomSetUp.NeedsTablecloths, style: '' }
      if(table.rotate > 0) {
        table.style = 'transform: rotateZ(' + table.rotate + 'deg);'
      } else if(table.rotate < 0) {
        table.rotate = 360 + parseInt(`${table.rotate}`)
        table.style = 'transform: rotateZ(' + table.rotate + 'deg);'
      } else {
        table.rotate = 0
      }
      if(table.clothed) {
        table.style += ` background-color: black; `
      }
      this.items.push(table)
    },
    startDrag(event, item) {
      this.dragItem = this.items[this.items.map(i => i.id).indexOf(item.id)]
      this.dragItem.offsetX = event.offsetX
      this.dragItem.offsetY = event.offsetY
    },
    onDrop(event) {
      let room = document.getElementById("roomBoundary")?.getBoundingClientRect() as any;
      this.dragItem.clientX = event.clientX - room.left - this.dragItem.offsetX
      this.dragItem.clientY = event.clientY - room.top - this.dragItem.offsetY
      this.dragItem.style = `position: absolute; transform: translate(${this.dragItem.clientX}px, ${this.dragItem.clientY}px) rotateZ(${this.dragItem.rotate}deg);`
      if(this.dragItem.clothed) {
        this.dragItem.style += " background-color: black;"
      }
    }
  },
  watch: {

  },
  mounted() {

  },
  template: `
<div class="row">
  <div class="col col-xs-0 col-md-2"></div>
  <div class="col col-xs-8 col-md-5">
    <div class="set-up-container">
      <div 
        id="roomBoundary" 
        class="drop-zone" 
        @drop="onDrop($event)" 
        @dragover.prevent 
        @dragenter.prevent
      >
        <div v-for="item in items" 
          :key="item.id" 
          :class="'table table-' + item.tableType"
          :style="item.style"
          draggable="true" 
          @dragstart="startDrag($event, item)"
        ></div>
      </div>
    </div>
  </div>
  <div class="col col-xs-4 col-md-3">
    <rck-btn @click="newTable">Add Table</rck-btn>
  </div>
  <div class="col col-xs-0 col-md-2"></div>
</div>
<rck-modal 
  v-model="modal" 
  style="min-width: 50%;" 
  :isCloseButtonHidden="true" 
  cancelText="" 
  :clickBackdropToClose="true" 
  modalWrapperClasses="modal-no-header"
>
  <div class="row">
    <div class="col col-xs-10">
      <div class="row">
        <div class="col col-xs-12 col-md-4">
          <tcc-validator :rules="typeOfTableRules" ref="validators_typeoftable">
            <rck-lbl>Type of Table</rck-lbl>
            <rck-ddl
              v-model="roomSetUp.TypeofTable"
              :items="[{value: 'round', text: 'Round'}, {value: 'rect-6', text: '6ft Rectangular'}, {value: 'rect-4', text: '4ft Rectangular'}]"
            ></rck-ddl>
          </tcc-validator>
        </div>
        <div class="col col-xs-12 col-md-4">
          <rck-lbl>Rotation</rck-lbl>
          <rck-text
            v-model="roomSetUp.Rotation"
            type="number"
          ></rck-text>
        </div>
        <div class="col col-xs-12 col-md-4">
          <div class="validator form-group">
            <rck-lbl>Tablecloths</rck-lbl>
            <rck-chk
              v-model="roomSetUp.NeedsTablecloths"
            ></rck-chk>
          </div>
        </div>
      </div>
    </div>
    <div class="col col-xs-2">
      <div :class="'table table-' + roomSetUp.TypeofTable" :style="'transform: rotateZ(' + roomSetUp.Rotation + 'deg);' + (roomSetUp.NeedsTablecloths ? ' background-color: black;' : '')"></div>
    </div>
  </div>

  <template #customButtons>
    <div style="display: flex;">
      <div class="spacer"></div>
      <rck-btn btnType="primary" @click="addTable">Add Table</rck-btn>
    </div>
  </template>
</rck-modal>

<v-style>
  .set-up-container {
    height: 200px;
  }
  #roomBoundary {
    position: absolute;
    border: 2px solid black; 
    cursor: pointer;
    width: 300px;
    height: 200px;
  }
  .table {
    border: 2px solid black;
    border-radius: 4px;
  }
  .table-rect-6 {
    width: 80px;
    height: 30px;
  }
  .table-rect-4 {
    width: 60px;
    height: 30px;
  }
  .table-round {
    width: 40px;
    height: 40px;
    border-radius: 20px;
  }
  .spacer {
    flex-grow: 1!important;
  }
</v-style>
  `
});
