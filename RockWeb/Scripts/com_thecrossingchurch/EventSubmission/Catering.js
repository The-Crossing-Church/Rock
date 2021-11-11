export default {
  template: `
    <v-form ref="cateringForm" v-model="valid">
      <v-row>
        <v-col>
          <h3 class="primary--text" v-if="request.Events.length == 1">Catering Information</h3>
          <h3 class="primary--text" v-else>
            Catering Information 
            <v-btn rounded outlined color="accent" @click="prefillDate = ''; dialog = true; ">
              Prefill
            </v-btn>
          </h3>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" style='font-weight: bold;'>
          Although buffets are now permissible, keep in mind that some of your attendees may still appreciate the precautionary measures of an individually packaged meal. For a list of vendors providing boxed options, 
          <v-menu attach>
            <template v-slot:activator="{ on, attrs }">
              <span v-bind="attrs" v-on="on" class='accent-text'>
                please click here. 
              </span>
            </template>
            <v-list>
              <v-list-item @click="e.Vendor = 'Chick-fil-A'">
                Chick-fil-A
              </v-list-item>
              <v-list-item @click="e.Vendor = 'Como Smoke and Fire'">
                Como Smoke and Fire
              </v-list-item>
              <v-list-item @click="e.Vendor = 'Honey Baked Ham'">
                Honey Baked Ham
              </v-list-item>
              <v-list-item @click="e.Vendor = 'Panera'">
                Panera
              </v-list-item>
              <v-list-item @click="e.Vendor = 'Picklemans'">
                Pickleman's
              </v-list-item>
              <v-list-item @click="e.Vendor = 'Tropical Smoothie Cafe'">
                Tropical Smoothie Cafe
              </v-list-item>
              <v-list-item @click="e.Vendor = 'Word of Mouth Catering'">
                Word of Mouth Catering
              </v-list-item>
            </v-list>
          </v-menu>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-text-field
            label="Preferred Vendor"
            v-model="e.Vendor"
            :rules="[rules.required(e.Vendor, 'Vendor')]"
          ></v-text-field>
        </v-col>
        <v-col>
          <v-text-field
            label="Food Budget Line"
            v-model="e.BudgetLine"
            :rules="[rules.required(e.BudgetLine, 'Budget Line')]"
          ></v-text-field>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-textarea
            label="Preferred Menu"
            v-model="e.Menu"
            :rules="[rules.required(e.Menu, 'Menu')]"
          ></v-textarea>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-switch
            v-model="e.FoodDelivery"
            :label="deliveryLabel"
          ></v-switch>
        </v-col>
      </v-row>
      <v-row v-if="e.FoodDelivery">
        <v-col cols="12" md="6">
          <strong>What time would you like food to be set up and ready?</strong>
          <time-picker
            v-model="e.FoodTime"
            :value="e.FoodTime"
            :default="defaultFoodTime"
            :rules="[rules.required(e.FoodTime, 'Time')]"
          ></time-picker>
        </v-col>
        <v-col cols="12" md="6">
          <br />
          <v-row>
            <v-col>
              <v-text-field
                label="Where should the food be set up?"
                v-model="e.FoodDropOff"
                :rules="[rules.required(e.FoodDropOff, 'Location')]"
              ></v-text-field>
            </v-col>
          </v-row>
        </v-col>
      </v-row>
      <v-row v-else>
        <v-col cols="12" md="6">
          <strong>What time would you like to pick up your food?</strong>
          <time-picker
            v-model="e.FoodTime"
            :value="e.FoodTime"
            :default="defaultFoodTime"
            :rules="[rules.required(e.FoodTime, 'Time')]"
          ></time-picker>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" md="6">
          <br />
          <v-row>
            <v-col>
              <v-autocomplete
                label="What drinks would you like to have?"
                :items="['Coffee', 'Soda', 'Water']"
                v-model="e.Drinks"
                multiple
                chips
                attach
              ></v-autocomplete>
            </v-col>
          </v-row>
        </v-col>
        <v-col cols="12" md="6">
          <strong>What time would you like your drinks to be delivered?</strong>
          <time-picker
            v-model="e.DrinkTime"
            :value="e.DrinkTime"
            :default="defaultFoodTime"
          ></time-picker>
        </v-col>
      </v-row>
      <!--<v-row v-if="e.Drinks.includes('Coffee')">
        <v-col cols="12" md="6">
          <v-checkbox
            label="I agree to provide a coffee serving team in compliance with COVID-19 policy."
            :rules="[rules.required(e.ServingTeamAgree, 'Agreement to provide a serving team')]"
            v-model="e.ServingTeamAgree"
          ></v-checkbox>
        </v-col>
      </v-row> -->
      <v-row>
        <v-col cols="12" md="6" v-if="e.FoodDropOff != ''">
          <v-checkbox
            label="Set up my drinks in the same location as my food please!"
            v-model="sameFoodDrinkDropOff"
            dense
          ></v-checkbox>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field
            label="Where would you like your drinks delivered?"
            v-model="e.DrinkDropOff"
          ></v-text-field>
        </v-col>
      </v-row>
      <!-- Childcare Catering -->
      <template v-if="request.needsChildCare">
        <v-row>
          <v-col>
            <v-select
              label="Preferred Vendor for Childcare"
              v-model="e.CCVendor"
              :rules="[rules.required(e.CCVendor, 'Vendor')]"
              :items="['Pizza', 'Other']"
              attach
            ></v-select>
          </v-col>
          <v-col>
            <v-text-field
              label="Food Budget Line for Childcare"
              v-model="e.CCBudgetLine"
              :rules="[rules.required(e.CCBudgetLine, 'Budget Line')]"
            ></v-text-field>
          </v-col>
        </v-row>
        <v-row>
          <v-col>
            <v-textarea
              label="Preferred Menu for Childcare"
              v-model="e.CCMenu"
              :rules="[rules.required(e.CCMenu, 'Menu')]"
            ></v-textarea>
          </v-col>
        </v-row>
        <v-row>
          <v-col cols="12" md="6">
            <strong>
              What time would you like your childcare food delivered?
            </strong>
            <time-picker
              v-model="e.CCFoodTime"
              :value="e.CCFoodTime"
              :default="defaultFoodTime"
              :rules="[rules.required(e.CCFoodTime, 'Time')]"
            ></time-picker>
          </v-col>
        </v-row>
      </template>
      <v-dialog
        v-if="dialog"
        v-model="dialog"
        max-width="850px"
      >
        <v-card>
          <v-card-title>
            Pre-fill this section with information from another date
          </v-card-title>
          <v-card-text>
            <v-select
              :items="prefillOptions"
              v-model="prefillDate"
            >
              <template v-slot:selection="data">
                {{data.item | formatDate}}
              </template>
              <template v-slot:item="data">
                {{data.item | formatDate}}
              </template>
            </v-select>
          </v-card-text>
          <v-card-actions>
            <v-btn color="secondary" @click="dialog = false; prefillDate = '';">Cancel</v-btn>
            <v-spacer></v-spacer>
            <v-btn color="primary" @click="prefillSection">Pre-fill Section</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </v-form>
  `,
  props: ["e", "request"],
  data: function () {
      return {
          dialog: false,
          valid: true,
          rooms: [],
          prefillDate: '',
          sameFoodDrinkDropOff: false,
          rules: {
              required(val, field) {
                  return !!val || `${field} is required`;
              },
              requiredArr(val, field) {
                  return val.length > 0 || `${field} is required`;
              },
          }
      }
  },
  created: function () {
      this.allEvents = [];
      this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
  },
  filters: {
      formatDate(val) {
          return moment(val).format("MM/DD/yyyy");
      },
  },
  computed: {
      prefillOptions() {
          return this.request.EventDates.filter(i => i != this.e.EventDate)
      },
      deliveryLabel() {
          return `Would you like your food to be delivered? ${this.e.FoodDelivery ? 'Yes!' : 'No, someone from my team will pick it up'}`
      },
      drinkHint() {
          return ''
          // return `${this.e.Drinks.toString().includes('Coffee') ? 'Due to COVID-19, all drip coffee must be served by a designated person or team from the hosting ministry. This person must wear a mask and gloves and be the only person to touch the cups, sleeves, lids, and coffee carafe before the coffee is served to attendees. If you are not willing to provide this for your own event, please deselect the coffee option and opt for an individually packaged item like bottled water or soda.' : ''}`
      },
      defaultFoodTime() {
          if (this.e.StartTime && !this.e.StartTime.includes('null')) {
              let time = moment(this.e.StartTime, "hh:mm A");
              return time.subtract(30, "minutes").format("hh:mm A");
          }
          return null;
      },
  },
  methods: {
      prefillSection() {
          this.dialog = false
          let idx = this.request.EventDates.indexOf(this.prefillDate)
          let currIdx = this.request.EventDates.indexOf(this.e.EventDate)
          this.$emit('updatecatering', { targetIdx: idx, currIdx: currIdx })
      }
  },
  watch: {
      sameFoodDrinkDropOff(val) {
          if (val) {
              this.e.DrinkDropOff = this.e.FoodDropOff
          }
      },
  }
}