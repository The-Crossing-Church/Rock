<%@ Control Language="C#" AutoEventWireup="true"
CodeFile="VolunteerAppreciationDashboard.ascx.cs"
Inherits="RockWeb.Plugins.com_thecrossingchurch.CreativeArts.VolunteerAppreciationDashboard"
%> <%-- Add Vue and Vuetify CDN --%>
<!-- <script src="https://cdn.jsdelivr.net/npm/vue@2.6.12"></script> -->
<script src="https://cdn.jsdelivr.net/npm/vue@2.6.12/dist/vue.js"></script>
<script src="https://cdn.jsdelivr.net/npm/vuetify@2.x/dist/vuetify.js"></script>
<script src="https://cdn.jsdelivr.net/npm/chart.js@2.9.4/dist/Chart.min.js"></script>
<link
  href="https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900"
  rel="stylesheet"
/>
<link
  href="https://cdn.jsdelivr.net/npm/@mdi/font@4.x/css/materialdesignicons.min.css"
  rel="stylesheet"
/>
<link
  href="https://cdn.jsdelivr.net/npm/vuetify@2.x/dist/vuetify.min.css"
  rel="stylesheet"
/>
<script
  src="https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.29.1/moment.min.js"
  integrity="sha512-qTXRIMyZIFb8iQcfjXWCO8+M5Tbc38Qi5WzdPOYZHIlZpzBHG3L3by84BBBOiRGiEb7KKtAOAs5qYdUiZiQNNQ=="
  crossorigin="anonymous"
></script>

<asp:HiddenField ID="hfSurveys" runat="server" />

<div id="app">
  <v-app>
    <div>
      <v-card>
        <v-card-title> Responses </v-card-title>
        <v-card-text>
          <v-list>
            <v-list-item>
              <v-row>
                <v-col>
                  <v-text-field
                    label="Search"
                    prepend-inner-icon="mdi-magnify"
                    v-model="search"
                  ></v-text-field>
                </v-col>
              </v-row>
            </v-list-item>
            <v-list-item class="border-bottom">
              <v-row>
                <v-col> Name </v-col>
                <v-col> Email </v-col>
                <v-col> Personality </v-col>
                <v-col> Appreciation Language </v-col>
                <v-col> Presentation Preference </v-col>
              </v-row>
            </v-list-item>
            <v-list-item
              v-for="(s, idx) in filteredSurveys"
              :key="s.Id"
              @click="openSurvey(s)"
              :class="className(idx)"
            >
              <v-row>
                <v-col>
                  {{s.AttributeValues.FirstName.ValueFormatted}}
                  {{s.AttributeValues.MiddleInitial.ValueFormatted}}
                  {{s.AttributeValues.LastName.ValueFormatted}}
                </v-col>
                <v-col>
                  {{s.AttributeValues.WorkEmailAddress.ValueFormatted}}
                </v-col>
                <v-col>
                  {{s.AttributeValues.Personality.ValueFormatted}}
                </v-col>
                <v-col>
                  {{s.AttributeValues.AppreciationLanguage.ValueFormatted}}
                </v-col>
                <v-col>
                  {{s.AttributeValues.PresentationPreference.ValueFormatted}}
                </v-col>
              </v-row>
            </v-list-item>
          </v-list>
        </v-card-text>
      </v-card>
      <v-dialog v-model="dialog" max-width="80%" v-if="dialog">
        <v-card>
          <v-card-title>
            {{selected.AttributeValues.FirstName.ValueFormatted}}
            {{selected.AttributeValues.MiddleInitial.ValueFormatted}}
            {{selected.AttributeValues.LastName.ValueFormatted}}
          </v-card-title>
          <v-card-text>
            <v-row v-if="selected.AttributeValues.Family.ValueFormatted">
              <v-col>
                <div class="floating-title">Family</div>
                {{selected.AttributeValues.Family.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row v-if="selected.AttributeValues.Pets.ValueFormatted">
              <v-col>
                <div class="floating-title">Pets</div>
                {{selected.AttributeValues.Pets.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row
              v-if="selected.AttributeValues.FavoriteColors.ValueFormatted"
            >
              <v-col>
                <div class="floating-title">Favorite Colors</div>
                {{selected.AttributeValues.FavoriteColors.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row
              v-if="selected.AttributeValues.FavoriteSnacks.ValueFormatted"
            >
              <v-col>
                <div class="floating-title">Favorite Snacks</div>
                {{selected.AttributeValues.FavoriteSnacks.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row
              v-if="selected.AttributeValues.FavoriteBeverages.ValueFormatted"
            >
              <v-col>
                <div class="floating-title">Favorite Beverages</div>
                {{selected.AttributeValues.FavoriteBeverages.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row
              v-if="selected.AttributeValues.FavoriteRestaurants.ValueFormatted"
            >
              <v-col>
                <div class="floating-title">Favorite Restaurants</div>
                {{selected.AttributeValues.FavoriteRestaurants.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row v-if="selected.AttributeValues.FavoriteStore.ValueFormatted">
              <v-col>
                <div class="floating-title">Favorite Place to Spend Money</div>
                {{selected.AttributeValues.FavoriteStore.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row v-if="selected.AttributeValues.FavoriteMusic.ValueFormatted">
              <v-col>
                <div class="floating-title">Favorite Music</div>
                {{selected.AttributeValues.FavoriteMusic.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row
              v-if="selected.AttributeValues.FavoriteSports.ValueFormatted"
            >
              <v-col>
                <div class="floating-title">Favorite Sports</div>
                {{selected.AttributeValues.FavoriteSports.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row v-if="selected.AttributeValues.SpareTime.ValueFormatted">
              <v-col>
                <div class="floating-title">
                  In {{selected.AttributeValues.FirstName.ValueFormatted}}'s
                  Spare Time, they...
                </div>
                {{selected.AttributeValues.SpareTime.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row v-if="selected.AttributeValues.WishList.ValueFormatted">
              <v-col>
                <div class="floating-title">
                  Something
                  {{selected.AttributeValues.FirstName.ValueFormatted}} wants
                  but wouldn't buy for themselves
                </div>
                {{selected.AttributeValues.WishList.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row v-if="selected.AttributeValues.BucketList.ValueFormatted">
              <v-col>
                <div class="floating-title">Bucket List</div>
                {{selected.AttributeValues.BucketList.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row
              v-if="selected.AttributeValues.AllergiesDislikes.ValueFormatted"
            >
              <v-col>
                <div class="floating-title">
                  Allergies, Pet Peeves, and other Dislikes
                </div>
                {{selected.AttributeValues.AllergiesDislikes.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row v-if="selected.AttributeValues.Personality.ValueFormatted">
              <v-col>
                <div class="floating-title">Personality</div>
                {{selected.AttributeValues.Personality.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row
              v-if="selected.AttributeValues.AppreciationPreference.ValueFormatted"
            >
              <v-col>
                <div class="floating-title">
                  How {{selected.AttributeValues.FirstName.ValueFormatted}}
                  likes to be appreciated
                </div>
                {{selected.AttributeValues.AppreciationPreference.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row
              v-if="selected.AttributeValues.AppreciationLanguage.ValueFormatted"
            >
              <v-col>
                <div class="floating-title">
                  {{selected.AttributeValues.FirstName.ValueFormatted}}'s
                  Appreciation Language
                </div>
                {{selected.AttributeValues.AppreciationLanguage.ValueFormatted}}
              </v-col>
            </v-row>
            <v-row
              v-if="selected.AttributeValues.PresentationPreference.ValueFormatted"
            >
              <v-col>
                <div class="floating-title">Appreciation Presentation</div>
                {{selected.AttributeValues.PresentationPreference.ValueFormatted}}
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
      </v-dialog>
    </div>
  </v-app>
</div>
<script>
    document.addEventListener("DOMContentLoaded", function () {
        new Vue({
            el: "#app",
            vuetify: new Vuetify({
                theme: {
                    themes: {
                        light: {
                            primary: "#347689",
                            secondary: "#3D3D3D",
                            accent: "#8ED2C9",
                        },
                    },
                },
                iconfont: "mdi",
            }),
            config: {
                devtools: true,
            },
            data: {
                surveys: [],
                selected: {},
                dialog: false,
                search: "",
            },
            created() {
                this.surveys = JSON.parse($('[id$="hfSurveys"]').val());
            },
            filters: {},
            computed: {
                filteredSurveys() {
                    if (this.search) {
                        let items = this.surveys
                        items = items.filter(i => {
                            return i.AttributeValues.FirstName.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.LastName.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.Family.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.Pets.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.FavoriteColors.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.FavoriteSnacks.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.FavoriteBeverages.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.FavoriteRestaurants.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.FavoriteStore.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.FavoriteMusic.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.FavoriteSports.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.SpareTime.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.WishList.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.BucketList.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.AllergiesDislikes.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.Personality.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.AppreciationPreference.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.AppreciationLanguage.ValueFormatted.toLowerCase().includes(this.search.toLowerCase()) ||
                                i.AttributeValues.PresentationPreference.ValueFormatted.toLowerCase().includes(this.search.toLowerCase())
                        })
                        return items
                    } else {
                        return this.surveys
                    }
                }
            },
            methods: {
                openSurvey(item) {
                    this.selected = item;
                    this.dialog = true;
                },
                className(idx) {
                    if (idx < this.surveys.length - 1) {
                        return "hover bottom-border";
                    }
                    return "hover";
                },
            },
        });
    });
</script>

<style>
  .theme--light.v-application {
    background: rgba(0, 0, 0, 0);
  }
  .col {
    padding: 4px 12px !important;
  }
  .no-top-pad {
    padding: 0px 12px !important;
  }
  input[type="text"]:focus,
  textarea:focus {
    border: none !important;
    box-shadow: none !important;
  }
  .v-input__slot {
    min-height: 42px !important;
  }
  .v-window {
    overflow: visible !important;
  }
  .btn-hidden {
    visibility: hidden;
  }
  .list-with-border,
  .border-bottom {
    border-bottom: 1px solid #c7c7c7;
  }
  .hover {
    cursor: pointer;
  }
  .v-dialog {
    margin-top: 150px;
    max-height: 80% !important;
  }
  .floating-title {
    text-transform: uppercase;
    font-size: 0.65rem;
  }
  .event-pill {
    border-radius: 6px;
    border: 2px solid #5f9bad;
  }
  .status-pill {
    border-radius: 6px;
    display: flex;
    min-height: 36px;
    align-items: center;
    justify-content: center;
  }
  .status-pill.submitted {
    border: 2px solid #347689;
  }
  .status-pill.approved {
    border: 2px solid #8ed2c9;
  }
  .status-pill.denied {
    border: 2px solid #f44336;
  }
  .status-pill.cancelled {
    border: 2px solid #9e9e9e;
  }
  ::-webkit-scrollbar {
    width: 5px;
    border-radius: 3px;
  }
  ::-webkit-scrollbar-track {
    background: #bfbfbf;
    -webkit-box-shadow: inset 1px 1px 2px rgba(0, 0, 0, 0.1);
  }
  ::-webkit-scrollbar-thumb {
    background: rgb(224, 224, 224);
    -webkit-box-shadow: inset 1px 1px 2px rgba(0, 0, 0, 0.2);
  }
  ::-webkit-scrollbar-thumb:hover {
    background: #aaa;
  }
  ::-webkit-scrollbar-thumb:active {
    background: #888;
    -webkit-box-shadow: inset 1px 1px 2px rgba(0, 0, 0, 0.3);
  }
</style>
