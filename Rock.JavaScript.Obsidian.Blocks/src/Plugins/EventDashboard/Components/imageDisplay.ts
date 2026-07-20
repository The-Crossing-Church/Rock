import { defineComponent } from "vue";

export default defineComponent({
    name: "EventDashboard.Components.Modal.OpsAccomInfo.Img",
    props: {
      value: String,
    },
    setup() {

    },
    data() {
        return {
          
        };
    },
    computed: {
        imgUrl () {
            if(this.value){
                let img = JSON.parse(this.value)
                if(img?.value) {
                    return '/GetImage.ashx?guid=' + img.value
                }
            }
            return ""
        },
        imgAlt () {
            if(this.value){
                let img = JSON.parse(this.value)
                if(img?.text) {
                    return img.text
                }
            }
            return ""
        }
    },
    methods: {

    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
    <template v-if="imgAlt != ''">
        <img :src="imgUrl" :alt="imgAlt" class="img-responsive" style="max-width: 250px;" />
    </template>
    <template v-else>
        <div>Empty</div>
    </template>
`
});
