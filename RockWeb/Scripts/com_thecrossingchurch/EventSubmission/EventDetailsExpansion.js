export default {
  template: `
<div>
  <v-row v-if="e.StartTime || e.EndTime || ( selected.Changes && (selected.Changes.Events[idx].StartTime || selected.Changes.Events[idx].EndTime) )">
    <v-col v-if="e.StartTime || (selected.Changes && selected.Changes.Events[idx].StartTime)">
      <div class="floating-title">Start Time</div>
      <template v-if="selected.Changes != null && e.StartTime != selected.Changes.Events[idx].StartTime">
        <span class='red--text'>{{(e.StartTime ? e.StartTime : 'Empty')}}: </span>
        <span class='primary--text'>{{(selected.Changes.Events[idx].StartTime ? selected.Changes.Events[idx].StartTime : 'Empty')}}</span>
      </template>
      <template v-else>
        {{e.StartTime}}
      </template>
    </v-col>
    <v-col v-if="e.EndTime || (selected.Changes && selected.Changes.Events[idx].EndTime)">
      <div class="floating-title">End Time</div>
      <template v-if="selected.Changes != null && e.EndTime != selected.Changes.Events[idx].EndTime">
        <span class='red--text'>{{(e.EndTime ? e.EndTime : 'Empty')}}: </span>
        <span class='primary--text'>{{(selected.Changes.Events[idx].EndTime ? selected.Changes.Events[idx].EndTime : 'Empty')}}</span>
      </template>
      <template v-else>
        {{e.EndTime}}
      </template>
    </v-col>
  </v-row>
  <v-row v-if="e.MinsStartBuffer || e.MinsEndBuffer">
    <v-col v-if="e.MinsStartBuffer">
      <div class="floating-title">Set-up Buffer</div>
      {{e.MinsStartBuffer}} minutes
    </v-col>
    <v-col v-if="e.MinsEndBuffer">
      <div class="floating-title">Tear-down Buffer</div>
      {{e.MinsEndBuffer}} minutes
    </v-col>
  </v-row>
  <template v-if="selected.needsSpace || (selected.Changes && selected.Changes.needsSpace)">
    <h6 class='text--accent text-uppercase'>Space Information</h6>
    <v-row>
      <v-col>
        <div class="floating-title">Expected Number of Attendees</div>
        <template v-if="selected.Changes != null && e.ExpectedAttendance != selected.Changes.Events[idx].ExpectedAttendance">
          <span class='red--text'>{{(e.ExpectedAttendance ? e.ExpectedAttendance : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].ExpectedAttendance ? selected.Changes.Events[idx].ExpectedAttendance : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.ExpectedAttendance}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Desired Rooms/Spaces</div>
        <template v-if="selected.Changes != null && formatRooms(e.Rooms) != formatRooms(selected.Changes.Events[idx].Rooms)">
          <span class='red--text'>{{(e.Rooms ? formatRooms(e.Rooms) : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].Rooms ? formatRooms(selected.Changes.Events[idx].Rooms) : 'Empty')}}</span>
        </template>
        <template v-else>
          {{formatRooms(e.Rooms)}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="e.TableType && e.TableType.length > 0 || (selected.Changes && selected.Changes.Events[idx].TableType && selected.Changes.Events[idx].TableType.length > 0)">
      <v-col>
        <div class="floating-title">Requested Tables</div>
        <template v-if="selected.Changes != null && e.TableType.toString() != selected.Changes.Events[idx].TableType.toString()">
          <span class='red--text'>{{(e.TableType ? e.TableType.join(', ')  : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].TableType  ? selected.Changes.Events[idx].TableType.join(', ') : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.TableType.join(', ')}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="e.TableType && e.TableType.includes('Round') || (selected.Changes && selected.Changes.Events[idx].TableType && selected.Changes.Events[idx].TableType.includes('Round'))">
      <v-col>
        <div class="floating-title">Number of Round Tables</div>
        <template v-if="selected.Changes != null && e.NumTablesRound != selected.Changes.Events[idx].NumTablesRound">
          <span class='red--text'>{{(e.NumTablesRound ? e.NumTablesRound  : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].NumTablesRound ? selected.Changes.Events[idx].NumTablesRound : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.NumTablesRound}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Number of Chairs per Round Table</div>
        <template v-if="selected.Changes != null && e.NumChairsRound != selected.Changes.Events[idx].NumChairsRound">
          <span class='red--text'>{{(e.NumChairsRound ? e.NumChairsRound  : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].NumChairsRound ? selected.Changes.Events[idx].NumChairsRound : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.NumChairsRound}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="e.TableType && e.TableType.includes('Rectangular') || (selected.Changes && selected.Changes.Events[idx].TableType && selected.Changes.Events[idx].TableType.includes('Rectangular'))">
      <v-col>
        <div class="floating-title">Number of Rectangular Tables</div>
        <template v-if="selected.Changes != null && e.NumTablesRect != selected.Changes.Events[idx].NumTablesRect">
          <span class='red--text'>{{(e.NumTablesRect ? e.NumTablesRect  : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].NumTablesRect ? selected.Changes.Events[idx].NumTablesRect : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.NumTablesRect}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Number of Chairs per Rectangular Table</div>
        <template v-if="selected.Changes != null && e.NumChairsRect != selected.Changes.Events[idx].NumChairsRect">
          <span class='red--text'>{{(e.NumChairsRect ? e.NumChairsRect  : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].NumChairsRect ? selected.Changes.Events[idx].NumChairsRect : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.NumChairsRect}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="selected.needsReg">
      <v-col>
        <div class="floating-title">Check-in Requested</div>
        <template v-if="selected.Changes != null && e.Checkin != selected.Changes.Events[idx].Checkin">
          <span class='red--text'>{{(e.Checkin != null ? boolToYesNo(e.Checkin) : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].Checkin != null ? boolToYesNo(selected.Changes.Events[idx].Checkin) : 'Empty')}}</span>
        </template>
        <template v-else>
          {{boolToYesNo(e.Checkin)}}
        </template>
      </v-col>
      <v-col v-if="(e.Checkin || (selected.Changes && selected.Changes.Events[idx].Checkin)) && (e.ExpectedAttendance >= 100 || (selected.Changes && selected.Changes.Events[idx].ExpectedAttendance >= 100))">
        <div class="floating-title">Database Team Support Requested</div>
        <template v-if="selected.Changes != null && e.SupportTeam != selected.Changes.Events[idx].SupportTeam">
          <span class='red--text'>{{(e.SupportTeam != null ? boolToYesNo(e.SupportTeam) : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].SupportTeam != null ? boolToYesNo(selected.Changes.Events[idx].SupportTeam) : 'Empty')}}</span>
        </template>
        <template v-else>
          {{boolToYesNo(e.SupportTeam)}}
        </template>
      </v-col>
    </v-row>
  </template>
  <template v-if="selected.needsOnline || (selected.Changes && selected.Changes.needsOnline)">
    <h6 class='text--accent text-uppercase'>Online Information</h6>
    <v-row>
      <v-col>
        <div class="floating-title">Event Link</div>
        <template v-if="selected.Changes != null && e.EventURL != selected.Changes.Events[idx].EventURL">
          <span class='red--text'>{{(e.EventURL ? e.EventURL : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].EventURL ? selected.Changes.Events[idx].EventURL : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.EventURL}}
        </template>
      </v-col>
      <v-col v-if="e.ZoomPassword || (selected.Changes && selected.Changes.Events[idx].ZoomPassword)">
        <div class="floating-title">Password</div>
        <template v-if="selected.Changes != null && e.ZoomPassword != selected.Changes.Events[idx].ZoomPassword">
          <span class='red--text'>{{(e.ZoomPassword ? e.ZoomPassword : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].ZoomPassword ? selected.Changes.Events[idx].ZoomPassword : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.ZoomPassword}}
        </template>
      </v-col>
    </v-row>
  </template>
  <template v-if="selected.needsChildCare || (selected.Changes && selected.Changes.needsChildCare)">
    <h6 class='text--accent text-uppercase'>Childcare Information</h6>
    <v-row>
      <v-col>
        <div class="floating-title">Childcare Age Groups</div>
        <template v-if="selected.Changes != null && e.ChildCareOptions.join(', ') != selected.Changes.Events[idx].ChildCareOptions.join(', ')">
          <span class='red--text'>{{(e.ChildCareOptions ? e.ChildCareOptions.join(', ') : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].ChildCareOptions ? selected.Changes.Events[idx].ChildCareOptions.join(', ') : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.ChildCareOptions.join(', ')}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Expected Number of Children</div>
        <template v-if="selected.Changes != null && e.EstimatedKids != selected.Changes.Events[idx].EstimatedKids">
          <span class='red--text'>{{(e.EstimatedKids ? e.EstimatedKids : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].EstimatedKids ? selected.Changes.Events[idx].EstimatedKids : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.EstimatedKids}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col>
        <div class="floating-title">Childcare Start Time</div>
        <template v-if="selected.Changes != null && e.CCStartTime != selected.Changes.Events[idx].CCStartTime">
          <span class='red--text'>{{(e.CCStartTime ? e.CCStartTime : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].CCStartTime ? selected.Changes.Events[idx].CCStartTime : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.CCStartTime}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Childcare End Time</div>
        <template v-if="selected.Changes != null && e.CCEndTime != selected.Changes.Events[idx].CCEndTime">
          <span class='red--text'>{{(e.CCEndTime ? e.CCEndTime : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].CCEndTime ? selected.Changes.Events[idx].CCEndTime : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.CCEndTime}}
        </template>
      </v-col>
    </v-row>
  </template>
  <template v-if="selected.needsCatering || (selected.Changes && selected.Changes.needsCatering)">
    <h6 class='text--accent text-uppercase'>Catering Information</h6>
    <v-row>
      <v-col>
        <div class="floating-title">Preferred Vendor</div>
        <template v-if="selected.Changes != null && e.Vendor != selected.Changes.Events[idx].Vendor">
          <span class='red--text'>{{(e.Vendor ? e.Vendor : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].Vendor ? selected.Changes.Events[idx].Vendor : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.Vendor}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Budget Line</div>
        <template v-if="selected.Changes != null && e.BudgetLine != selected.Changes.Events[idx].BudgetLine">
          <span class='red--text'>{{(e.BudgetLine ? e.BudgetLine : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].BudgetLine ? selected.Changes.Events[idx].BudgetLine : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.BudgetLine}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col>
        <div class="floating-title">Preferred Menu</div>
        <template v-if="selected.Changes != null && e.Menu != selected.Changes.Events[idx].Menu">
          <span class='red--text'>{{(e.Menu ? e.Menu : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].Menu ? selected.Changes.Events[idx].Menu : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.Menu}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col>
        <div class="floating-title">Food should be delivered</div>
        <template v-if="selected.Changes != null && e.FoodDelivery != selected.Changes.Events[idx].FoodDelivery">
          <span class='red--text'>{{(e.FoodDelivery ? e.FoodDelivery : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].FoodDelivery ? selected.Changes.Events[idx].FoodDelivery : 'Empty')}}</span>
        </template>
        <template v-else>
          {{boolToYesNo(e.FoodDelivery)}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col>
        <div class="floating-title">{{foodTimeTitle(e)}}</div>
        <template v-if="selected.Changes != null && e.FoodTime != selected.Changes.Events[idx].FoodTime">
          <span class='red--text'>{{(e.FoodTime ? e.FoodTime : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].FoodTime ? selected.Changes.Events[idx].FoodTime : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.FoodTime}}
        </template>
      </v-col>
      <v-col v-if="e.FoodDelivery || (selected.Changes && selected.Changes.Events[idx].FoodDelivery)">
        <div class="floating-title">Food Drop off Location</div>
        <template v-if="selected.Changes != null && e.FoodDropOff != selected.Changes.Events[idx].FoodDropOff">
          <span class='red--text'>{{(e.FoodDropOff ? e.FoodDropOff : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].FoodDropOff ? selected.Changes.Events[idx].FoodDropOff : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.FoodDropOff}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="(e.Drinks && e.Drinks.length > 0) || (selected.Changes && selected.Changes.Events[idx].Drinks && selected.Changes.Events[idx].Drinks.length > 0)">
      <v-col>
        <div class="floating-title">Desired Drinks</div>
        <template v-if="selected.Changes != null && e.Drinks && e.Drinks.join(', ') != selected.Changes.Events[idx].Drinks.join(', ')">
          <span class='red--text'>{{(e.Drinks ? e.Drinks.join(', ') : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].Drinks? selected.Changes.Events[idx].Drinks.join(', ') : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.Drinks.join(', ')}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="e.DrinkTime || (selected.Changes && selected.Changes.Events[idx].DrinkTime)">
      <v-col>
        <div class="floating-title">Drink Set-up Time</div>
        <template v-if="selected.Changes != null && e.DrinkTime != selected.Changes.Events[idx].DrinkTime">
          <span class='red--text'>{{(e.DrinkTime ? e.DrinkTime : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].DrinkTime ? selected.Changes.Events[idx].DrinkTime : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.DrinkTime}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Drink Drop off Location</div>
        <template v-if="selected.Changes != null && e.DrinkDropOff != selected.Changes.Events[idx].DrinkDropOff">
          <span class='red--text'>{{(e.DrinkDropOff ? e.DrinkDropOff : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].DrinkDropOff ? selected.Changes.Events[idx].DrinkDropOff : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.DrinkDropOff}}
        </template>
      </v-col>
    </v-row>
    <template v-if="selected.needsChildCare || (selected.Changes && selected.Changes.needsChildCare)">
      <h6 class='text--accent text-uppercase'>Childcare Catering Information</h6>
      <v-row>
        <v-col>
          <div class="floating-title">
            Preferred Vendor for Childcare
          </div>
          <template v-if="selected.Changes != null && e.CCVendor != selected.Changes.Events[idx].CCVendor">
            <span class='red--text'>{{(e.CCVendor ? e.CCVendor : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].CCVendor ? selected.Changes.Events[idx].CCVendor : 'Empty')}}</span>
          </template>
          <template v-else>
            {{e.CCVendor}}
          </template>
        </v-col>
        <v-col>
          <div class="floating-title">Budget Line for Childcare</div>
          <template v-if="selected.Changes != null && e.CCBudgetLine != selected.Changes.Events[idx].CCBudgetLine">
            <span class='red--text'>{{(e.CCBudgetLine ? e.CCBudgetLine : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].CCBudgetLine ? selected.Changes.Events[idx].CCBudgetLine : 'Empty')}}</span>
          </template>
          <template v-else>
            {{e.CCBudgetLine}}
          </template>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <div class="floating-title">
            Preferred Menu for Childcare
          </div>
          <template v-if="selected.Changes != null && e.CCMenu != selected.Changes.Events[idx].CCMenu">
            <span class='red--text'>{{(e.CCMenu ? e.CCMenu : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].CCMenu ? selected.Changes.Events[idx].CCMenu : 'Empty')}}</span>
          </template>
          <template v-else>
            {{e.CCMenu}}
          </template>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <div class="floating-title">ChildCare Food Set-up time</div>
          <template v-if="selected.Changes != null && e.CCFoodTime != selected.Changes.Events[idx].CCFoodTime">
            <span class='red--text'>{{(e.CCFoodTime ? e.CCFoodTime : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].CCFoodTime ? selected.Changes.Events[idx].CCFoodTime : 'Empty')}}</span>
          </template>
          <template v-else>
            {{e.CCFoodTime}}
          </template>
        </v-col>
      </v-row>
    </template>
  </template>
  <template v-if="selected.needsReg || (selected.Changes && selected.Changes.needsReg)">
    <h6 class='text--accent text-uppercase'>Registration Information</h6>
    <v-row v-if="e.RegistrationDate || (selected.Changes && selected.Changes.Events[idx].RegistrationDate)">
      <v-col>
        <div class="floating-title">Registration Date</div>
        <template v-if="selected.Changes != null && e.RegistrationDate != selected.Changes.Events[idx].RegistrationDate">
          <span class='red--text' v-if="e.RegistrationDate">{{e.RegistrationDate | formatDate}}: </span>
          <span class='red--text' v-else>Empty: </span>
          <span class='primary--text' v-if="selected.Changes.Events[idx].RegistrationDate">{{selected.Changes.Events[idx].RegistrationDate | formatDate}}</span>
          <span class='primary--text' v-else>Empty</span>
        </template>
        <template v-else>
          {{e.RegistrationDate | formatDate}}
        </template>
      </v-col>
      <v-col v-if="e.FeeType || (selected.Changes && selected.Changes.Events[idx].FeeType)">
        <div class="floating-title">Registration Fee Types</div>
        <template v-if="selected.Changes != null && e.FeeType.toString() != selected.Changes.Events[idx].FeeType.toString()">
          <span class='red--text' v-if="e.FeeType">{{e.FeeType.join(', ')}}: </span>
          <span class='red--text' v-else>Empty: </span>
          <span class='primary--text' v-if="selected.Changes.Events[idx].FeeType">{{selected.Changes.Events[idx].FeeType.join(', ')}}</span>
          <span class='primary--text' v-else>Empty</span>
        </template>
        <template v-else>
          {{e.FeeType.join(', ')}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="6" v-if="e.FeeBudgetLine || (selected.Changes && selected.Changes.Events[idx].FeeBudgetLine)">
        <div class="floating-title">Registration Fee Budget Line</div>
        <template v-if="selected.Changes != null && e.FeeBudgetLine != selected.Changes.Events[idx].FeeBudgetLine">
          <span class='red--text' v-if="e.FeeBudgetLine">{{e.FeeBudgetLine}}: </span>
          <span class='red--text' v-else>Empty: </span>
          <span class='primary--text' v-if="selected.Changes.Events[idx].FeeBudgetLine">{{selected.Changes.Events[idx].FeeBudgetLine}}</span>
          <span class='primary--text' v-else>Empty</span>
        </template>
        <template v-else>
          {{e.FeeBudgetLine}}
        </template>
      </v-col>
      <v-col cols="12" md="6" v-if="e.Fee || (selected.Changes && selected.Changes.Events[idx].Fee)">
        <div class="floating-title">Individual Registration Fee</div>
        <template v-if="selected.Changes != null && e.Fee != selected.Changes.Events[idx].Fee">
          <span class='red--text' v-if="e.Fee">{{e.Fee | formatCurrency}}: </span>
          <span class='red--text' v-else>Empty: </span>
          <span class='primary--text' v-if="selected.Changes.Events[idx].Fee">{{selected.Changes.Events[idx].Fee | formatCurrency}}</span>
          <span class='primary--text' v-else>Empty</span>
        </template>
        <template v-else>
          {{e.Fee | formatCurrency}}
        </template>
      </v-col>
      <v-col cols="12" md="6" v-if="e.CoupleFee || (selected.Changes && selected.Changes.Events[idx].CoupleFee)">
        <div class="floating-title">Couple Registration Fee</div>
        <template v-if="selected.Changes != null && e.CoupleFee != selected.Changes.Events[idx].CoupleFee">
          <span class='red--text' v-if="e.CoupleFee">{{e.CoupleFee | formatCurrency}}: </span>
          <span class='red--text' v-else>Empty: </span>
          <span class='primary--text' v-if="selected.Changes.Events[idx].CoupleFee">{{selected.Changes.Events[idx].CoupleFee | formatCurrency}}</span>
          <span class='primary--text' v-else>Empty</span>
        </template>
        <template v-else>
          {{e.CoupleFee | formatCurrency}}
        </template>
      </v-col>
      <v-col cols="12" md="6" v-if="e.OnlineFee || (selected.Changes && selected.Changes.Events[idx].OnlineFee)">
        <div class="floating-title">Online Registration Fee</div>
        <template v-if="selected.Changes != null && e.OnlineFee != selected.Changes.Events[idx].OnlineFee">
          <span class='red--text' v-if="e.OnlineFee">{{e.OnlineFee | formatCurrency}}: </span>
          <span class='red--text' v-else>Empty: </span>
          <span class='primary--text' v-if="selected.Changes.Events[idx].OnlineFee">{{selected.Changes.Events[idx].OnlineFee | formatCurrency}}</span>
          <span class='primary--text' v-else>Empty</span>
        </template>
        <template v-else>
          {{e.OnlineFee | formatCurrency}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="e.RegistrationEndDate || (selected.Changes && selected.Changes.Events[idx].RegistrationEndDate)">
      <v-col>
        <div class="floating-title">Registration Close Date</div>
        <template v-if="selected.Changes != null && e.RegistrationEndDate != selected.Changes.Events[idx].RegistrationEndDate">
          <span class='red--text' v-if="e.RegistrationEndDate">{{e.RegistrationEndDate | formatDate}}: </span>
          <span class='red--text' v-else>Empty: </span>
          <span class='primary--text' v-if="selected.Changes.Events[idx].RegistrationEndDate">{{selected.Changes.Events[idx].RegistrationEndDate | formatDate}}</span>
          <span class='primary--text' v-else>Empty</span>
        </template>
        <template v-else>
          {{e.RegistrationEndDate | formatDate}}
        </template>
      </v-col>
      <v-col v-if="e.RegistrationEndTime || (selected.Changes && selected.Changes.Events[idx].RegistrationEndTime)">
        <div class="floating-title">Registration Close Time</div>
        <template v-if="selected.Changes != null && e.RegistrationEndTime != selected.Changes.Events[idx].RegistrationEndTime">
          <span class='red--text'>{{(e.RegistrationEndTime ? e.RegistrationEndTime : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].RegistrationEndTime ? selected.Changes.Events[idx].RegistrationEndTime : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.RegistrationEndTime}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col v-if="e.Sender || (selected.Changes && selected.Changes.Events[idx].Sender)">
        <div class="floating-title">Registration Confirmation Email Sender</div>
        <template v-if="selected.Changes != null && e.Sender != selected.Changes.Events[idx].Sender">
          <span class='red--text'>{{(e.Sender ? e.Sender : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].Sender ? selected.Changes.Events[idx].Sender : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.Sender}}
        </template>
      </v-col>
      <v-col v-if="e.SenderEmail || (selected.Changes && selected.Changes.Events[idx].SenderEmail)">
        <div class="floating-title">Registration Confirmation Sender Email</div>
        <template v-if="selected.Changes != null && e.SenderEmail != selected.Changes.Events[idx].SenderEmail">
          <span class='red--text'>{{(e.SenderEmail ? e.SenderEmail : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].SenderEmail ? selected.Changes.Events[idx].SenderEmail : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.SenderEmail}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col v-if="e.ThankYou || (selected.Changes && selected.Changes.Events[idx].ThankYou)">
        <div class="floating-title">Confirmation Email Thank You</div>
        <template v-if="selected.Changes != null && e.ThankYou != selected.Changes.Events[idx].ThankYou">
          <span class='red--text'>{{(e.ThankYou ? e.ThankYou : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].ThankYou ? selected.Changes.Events[idx].ThankYou : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.ThankYou}}
        </template>
      </v-col>
      <v-col v-if="e.TimeLocation || (selected.Changes && selected.Changes.Events[idx].TimeLocation)">
        <div class="floating-title">Confirmation Email Time and Location</div>
        <template v-if="selected.Changes != null && e.TimeLocation != selected.Changes.Events[idx].TimeLocation">
          <span class='red--text'>{{(e.TimeLocation ? e.TimeLocation : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].TimeLocation ? selected.Changes.Events[idx].TimeLocation : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.TimeLocation}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="e.AdditionalDetails || (selected.Changes && selected.Changes.Events[idx].AdditionalDetails)">
      <v-col>
        <div class="floating-title">Confirmation Email Additional Details</div>
        <template v-if="selected.Changes != null && e.AdditionalDetails != selected.Changes.Events[idx].AdditionalDetails">
          <span class='red--text'>{{(e.AdditionalDetails ? e.AdditionalDetails : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].AdditionalDetails ? selected.Changes.Events[idx].AdditionalDetails : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.AdditionalDetails}}
        </template>
      </v-col>
    </v-row>
  </template>
  <template v-if="selected.needsAccom || (selected.Changes && selected.Changes.needsAccom)">
    <h6 class='text--accent text-uppercase'>Additional Information</h6>
    <v-row v-if="e.TechNeeds || e.TechDescription || (selected.Changes && (selected.Changes.Events[idx].TechNeeds || selected.Changes.Events[idx].TechDescription))">
      <v-col v-if="e.TechNeeds && e.TechNeeds.length > 0">
        <div class="floating-title">Tech Needs</div>
        <template v-if="selected.Changes != null && e.TechNeeds.join(', ') != selected.Changes.Events[idx].TechNeeds.join(', ')">
          <span class='red--text'>{{(e.TechNeeds ? e.TechNeeds.join(', ') : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].TechNeeds ? selected.Changes.Events[idx].TechNeeds.join(', ') : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.TechNeeds.join(', ')}}
        </template>
      </v-col>
      <v-col v-if="e.TechDescription || (selected.Changes && selected.Changes.Events[idx].TechDescription)">
        <div class="floating-title">Tech Description</div>
        <template v-if="selected.Changes != null && e.TechDescription != selected.Changes.Events[idx].TechDescription">
          <span class='red--text'>{{(e.TechDescription ? e.TechDescription : 'Empty')}}: </span>
          <span class='primary--text'>{{selected.Changes.Events[idx].TechDescription}}</span>
        </template>
        <template v-else>
          {{e.TechDescription}}
        </template>
      </v-col>
    </v-row>
    <template v-if="!selected.needsCatering">
      <v-row v-if="(e.Drinks && e.Drinks.length > 0) || (selected.Changes && selected.Changes.Events[idx].Drinks && selected.Changes.Events[idx].Drinks.length > 0)">
        <v-col>
          <div class="floating-title">Desired Drinks</div>
          <template v-if="selected.Changes != null && e.Drinks.join(', ') != selected.Changes.Events[idx].Drinks.join(', ')">
            <span class='red--text'>{{(e.Drinks ? e.Drinks.join(', ') : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].Drinks ? selected.Changes.Events[idx].Drinks.join(', ') : 'Empty')}}</span>
          </template>
          <template v-else>
            {{e.Drinks.join(', ')}}
          </template>
        </v-col>
      </v-row>
      <v-row v-if="e.DrinkTime || (selected.Changes && selected.Changes.Events[idx].DrinkTime)">
        <v-col>
          <div class="floating-title">Drink Set-up Time</div>
          <template v-if="selected.Changes != null && e.DrinkTime != selected.Changes.Events[idx].DrinkTime">
            <span class='red--text'>{{(e.DrinkTime ? e.DrinkTime : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].DrinkTime ? selected.Changes.Events[idx].DrinkTime : 'Empty')}}</span>
          </template>
          <template v-else>
            {{e.DrinkTime}}
          </template>
        </v-col>
        <v-col>
          <div class="floating-title">Drink Drop off Location</div>
          <template v-if="selected.Changes != null && e.DrinkDropOff != selected.Changes.Events[idx].DrinkDropOff">
            <span class='red--text'>{{(e.DrinkDropOff ? e.DrinkDropOff : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].DrinkDropOff ? selected.Changes.Events[idx].DrinkDropOff : 'Empty')}}</span>
          </template>
          <template v-else>
            {{e.DrinkDropOff}}
          </template>
        </v-col>
      </v-row>
    </template>
    <v-row>
      <v-col>
        <div class="floating-title">Needs doors unlocked</div>
        <template v-if="selected.Changes != null && e.NeedsDoorsUnlocked != selected.Changes.Events[idx].NeedsDoorsUnlocked">
          <span class='red--text'>{{(e.NeedsDoorsUnlocked != null ? boolToYesNo(e.NeedsDoorsUnlocked) : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].NeedsDoorsUnlocked != null ? boolToYesNo(selected.Changes.Events[idx].NeedsDoorsUnlocked) : 'Empty')}}</span>
        </template>
        <template v-else>
          {{boolToYesNo(e.NeedsDoorsUnlocked)}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Add to public calendar</div>
        <template v-if="selected.Changes != null && e.ShowOnCalendar != selected.Changes.Events[idx].ShowOnCalendar">
          <span class='red--text'>{{(e.ShowOnCalendar != null ? boolToYesNo(e.ShowOnCalendar) : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].ShowOnCalendar != null ? boolToYesNo(selected.Changes.Events[idx].ShowOnCalendar) : 'Empty')}}</span>
        </template>
        <template v-else>
          {{boolToYesNo(e.ShowOnCalendar)}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="(e.ShowOnCalendar || (selected.Changes && selected.Changes.Events[idx].ShowOnCalendar)) && (e.PublicityBlurb || (selected.Changes && selected.Changes.Events[idx].PublicityBlurb))">
      <v-col>
        <div class="floating-title">Publicity Blurb</div>
        <template v-if="selected.Changes != null && e.PublicityBlurb != selected.Changes.Events[idx].PublicityBlurb">
          <span class='red--text'>{{(e.PublicityBlurb ? e.PublicityBlurb : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].PublicityBlurb ? selected.Changes.Events[idx].PublicityBlurb : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.PublicityBlurb}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="e.SetUp || (selected.Changes && selected.Changes.Events[idx].SetUp)">
      <v-col>
        <div class="floating-title">Requested Set-up</div>
        <template v-if="selected.Changes != null && e.SetUp != selected.Changes.Events[idx].SetUp">
          <span class='red--text'>{{(e.SetUp ? e.SetUp : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].SetUp ? selected.Changes.Events[idx].SetUp : 'Empty')}}</span>
        </template>
        <template v-else>
          {{e.SetUp}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="e.SetUpImage || (selected.Changes && selected.Changes.Events[idx].SetUpImage)">
      <v-col>
        <div class="floating-title">Set-up Image</div>
        {{e.SetUpImage.name}}
        <v-btn icon color="accent" @click="saveFile(idx, 'existing')">
          <v-icon color="accent">mdi-download</v-icon>
        </v-btn>
      </v-col>
      <v-col v-if="selected.Changes != null && e.SetUpImage != selected.Changes.Events[idx].SetUpImage">
        <div class="floating-title">Set-up Image</div>
        {{selected.Changes.Events[idx].SetUpImage.name}}
        <v-btn icon color="accent" @click="saveFile(idx, 'new')">
          <v-icon color="accent">mdi-download</v-icon>
        </v-btn>
      </v-col>
    </v-row>
  </template>
</div>
`,
  props: ["e", "idx", "selected"],
  data: function () {
    return {
      rooms: [],
      ministries: [],
    }
  },
  created: function () {
    this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
    this.ministries = JSON.parse($('[id$="hfMinistries"]')[0].value)
    window['moment-range'].extendMoment(moment)
  },
  filters: {
    formatDate(val) {
      return moment(val).format("MM/DD/yyyy");
    },
    formatCurrency(val) {
      var formatter = new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD",
      });
      return formatter.format(val);
    },
  },
  computed: {
    
  },
  watch: {
    
  },
  methods: {
    boolToYesNo(val) {
      if (val) {
        return "Yes";
      }
      return "No";
    },
    formatDates(val) {
      if (val) {
        let dates = [];
        val.forEach((i) => {
          dates.push(moment(i).format("MM/DD/yyyy"));
        });
        return dates.join(", ");
      }
      return "";
    },
    formatRooms(val) {
      if (val) {
        let rms = [];
        val.forEach((i) => {
          this.rooms.forEach((r) => {
            if (i == r.Id) {
              rms.push(r.Value);
            }
          });
        });
        return rms.join(", ");
      }
      return "";
    },
    formatMinistry(val) {
      if (val) {
        let formattedVal = this.ministries.filter(m => {
          return m.Id == val
        })
        return formattedVal[0].Value
      }
      return "";
    },
    foodTimeTitle(e) {
      if (e.FoodDelivery) {
        return "Food Set-up time";
      } else {
        return "Desired Pick-up time from Vendor";
      }
    },
  }
}