import { defineComponent, provide } from "vue"
import { useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block"
import { useStore } from "@Obsidian/PageState"
import { DragNDropHtmlViewModel } from './dragNDropHtmlViewModel'
// import { VueDraggableNext } from "vue-draggable-next"
import editorElement from "./Components/editorElement"

const store = useStore()
export default defineComponent({
  name: "CMS.DragNDropHtml",
  setup() {
      const invokeBlockAction = useInvokeBlockAction();
      const viewModel = useConfigurationValues<DragNDropHtmlViewModel | null>();
      return {
        viewModel
      }
  },
  components: {
    // "draggable": VueDraggableNext,
    "nested-draggable": editorElement
  },
  data() {
    return {
      html: [ { name:'Container', tag:'div', innerHtml:'', classes:'', items: [] } ] as any[],
      tools: [
        { name:'Heading', icon:'fa fa-heading', tag:'h2', tagOptions: ['h1', 'h2', 'h3', 'h4', 'h5', 'h6'], innerHtml:'Heading', classes:'', items: [] },
        { name:'Paragraph', icon:'fa fa-paragraph', tag:'p', tagOptions: [], innerHtml:'Lorem Ipsum Dolar Sit', classes:'', items: [] },
        { name:'Image', icon:'fa fa-image', tag:'img', tagOptions: [], innerHtml:'', classes:'', items: [] },
        { name:'Container', icon:'fa fa-image', tag:'div', tagOptions: [], innerHtml:'', classes:'', items: [] },
      ] as any[]
    }
  },
  computed: {

  },
  methods: {
    editElement(element){
      console.log('editing...')
      console.log(element)
    },
    addElement() {
      this.html.push({ id: 123, items: [] })
    }
  },
  mounted() {
  },
  watch: {
    
  },
  template: `
  <div id="htmlEditor">
    <div class="row">
      <div class="col col-xs-3">
        <div class="panel" id="tools">
          <div class="panel-heading">
            Tools
          </div>
          <div class="panel-body">
            <draggable
              :list="tools"
              :group="{ name: 'html', pull: 'clone', put: false }"
              :sort="false"
              class="d-flex"
            >
              <div v-for="t in tools" class="tool-icon">
                <i :class="t.icon + ' fa-2x'"></i>
                {{t.name}}
              </div>
            </draggable>
          </div>
        </div>
      </div>
      <div class="col col-xs-9">
        <div id="preview">
          <!-- <nested-draggable :items="html" v-on:editElement="editElement" /> -->
        </div>
      </div>
    </div>
  </div>
  <v-style>
    .dragArea {
      border: 1px solid lightgrey;
      min-height: 50px;
      padding: 4px;
    }
    #previw:hover {
      cursor: pointer;
    }
    .tool-icon {
      width: 75px;
      height: 75px;
      border-radius: 6px;
      box-shadow: 0 0 1px 0 rgba(0, 0, 0, 0.3), 0 1px 3px 0 rgba(0, 0, 0, 0.3);
      background-color: #e0e0e0;
      margin: 6px;
      padding 6px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-direction: column;
    }
    .editor-elem:hover {
      border: 2px solid lightblue;
    }
  </v-style>
  `
})