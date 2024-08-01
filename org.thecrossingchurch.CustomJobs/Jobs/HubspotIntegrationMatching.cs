// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Reflection;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;
using RestSharp;
using Rock.Security;
using System.Diagnostics;
using static Quartz.Plugin.Xml.XMLSchedulingDataProcessorPlugin;
using RestSharp.Extensions;
using Rock.Workflow.Action;
using System.ComponentModel;
using System.Threading;

namespace org.crossingchurch.HubspotIntegration.Jobs
{
    /// <summary>
    /// Job to supply hubspot contacts with a rock_id to the pull related information.
    /// </summary>
    [DisplayName( "Hubspot Integration: Match Records" )]
    [Description( "This job only supplies Hubspot contacts with a Rock ID and adds potential matches to an excel for further investigation." )]
    [DisallowConcurrentExecution]

    [TextField( "AttributeKey", "", true, "HubspotAPIKeyGlobal" )]
    [TextField( "Business Unit", "Hubspot Business Unit value", true, "0" )]
    [TextField( "Potential Matches File Name", "Name of the file for this job to list potential matches for cleaning", true, "Potential_Matches" )]
    public class HubspotIntegrationMatching : IJob
    {
        private string key { get; set; }
        private List<HSContactResult> contacts { get; set; }
        private List<HSContactResult> all_contacts { get; set; }
        private int request_count { get; set; }
        private string businessUnit { get; set; }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public HubspotIntegrationMatching()
        {
        }

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            //Bearer Token, but I didn't change the Attribute Key
            string attrKey = dataMap.GetString( "AttributeKey" );
            key = Encryption.DecryptString( GlobalAttributesCache.Get().GetValue( attrKey ) );
            businessUnit = dataMap.GetString( "BusinessUnit" );

            var current_id = 0;

            PersonService personService = new PersonService( new RockContext() );

            //Set up Static Report of Potential Matches
            ExcelPackage excel = new ExcelPackage();
            excel.Workbook.Properties.Title = "Potential Matches";
            excel.Workbook.Properties.Author = "Rock";
            ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add( "Potential Matches" );
            worksheet.PrinterSettings.LeftMargin = .5m;
            worksheet.PrinterSettings.RightMargin = .5m;
            worksheet.PrinterSettings.TopMargin = .5m;
            worksheet.PrinterSettings.BottomMargin = .5m;
            var headers = new List<string> { "HubSpot FirstName", "Rock FirstName", "HubSpot LastName", "Rock LastName", "HubSpot Email", "Rock Email", "HubSpot Phone", "Rock Phone", "HubSpot Connection Status", "Rock Connection Status", "HubSpot Link", "Rock Link", "HubSpot CreatedDate", "Rock Created Date", "HubSpot Modified Date", "Rock Modified Date", "Rock ID" };
            var h = 1;
            var row = 2;
            foreach ( var header in headers )
            {
                worksheet.Cells[1, h].Value = header;
                h++;
            }

            //Get Hubspot Properties in Rock Information Group
            //This will allow us to add properties temporarily to the sync and then not continue to have them forever
            var propClient = new RestClient( "https://api.hubapi.com/crm/v3/properties/contacts?properties=name,label,createdUserId,groupName,options,fieldType" );
            propClient.Timeout = -1;
            var propRequest = new RestRequest( Method.GET );
            propRequest.AddHeader( "Authorization", $"Bearer {key}" );
            IRestResponse propResponse = propClient.Execute( propRequest );
            var props = new List<HubspotProperty>();
            var propsQry = JsonConvert.DeserializeObject<HSPropertyQueryResult>( propResponse.Content );
            props = propsQry.results;
            props = props.Where( p => p.groupName == "rock_information" ).ToList();
            RockContext _context = new RockContext();
            HistoryService history_svc = new HistoryService( _context );

            //Get List of all contacts from Hubspot
            contacts = new List<HSContactResult>();
            all_contacts = new List<HSContactResult>();
            request_count = 0;
            GetContacts( "https://api.hubapi.com/crm/v3/objects/contacts/searche", "0" );

            List<String> emails = new List<string>() { "00.balmier_chosen@icloud.com", "13ad1322@gmail.com", "1414haileywhite@gmail.com", "14coatsj@gmail.com", "14lb1428@gmail.com", "19dpost@gmail.com", "19dpost@gmail.com", "246alyssa@gmail.com", "28wtb01@stu.cpsk12.org", "2jaxandlexi2004@gmail.com", "2meriel1512@gmail.com", "3khenry@gmail.com", "42aramirez@gmail.com", "47carolines@gmail.com", "802grantave@gmail.com", "a.fizer99@yahoo.com", "aacarie529@gmail.com", "aaron.b.smith229@gmail.com", "aaroncampbell@missouri.edu", "abbeyjensen110@gmail.com", "abbieschutjer2@gmail.com", "abbos12@yahoo.com", "abbycrelman@gmail.com", "abc9905@hotmail.com", "abfarris411@icloud.com", "abfarris411@icloud.com", "abh91c@yahoo.com", "ablockett@gmail.com", "ablockett@gmail.com", "abstruttmann@gmail.com", "acamp168@hotmail.com", "acanobi1@gmail.com", "adamstoll21@gmail.com", "addison.ins@gmail.com", "addison7707@icloud.com", "addisonamber@gmail.com", "addisonstuever3@gmail.com", "ae93xy@yahoo.com", "aejstar98@gmail.com", "aepapke@gmail.com", "aesearcy18@gmail.com", "agphillips9@icloud.com", "aimeekweber@gmail.com", "airjordan2320@icloud.com", "akdp8r@umsystem.edu", "alana.gilman13@gmail.com", "alana.smith05@yahoo.com", "alana.smith05@yahoo.com", "alanahsegomez2@icloud.com", "alee112811@yahoo.com", "alex.eskens@gmail.com", "alexarapier@gmail.com", "alexbuwa@gmail.com", "alexembree14@gmail.com", "alexisfrose@gmail.com", "alice.bcampos2011@gmail.com", "aliciacasady@gmail.com", "aliciacasady@gmail.com", "allanachitwood@me.com", "allenrachel4817@gmail.com", "alliedunn14@yahoo.com", "allimiller102@gmail.com", "allisonmabe32@gmail.com", "alyssa.ann1112@gmail.com", "alyssaandrievk6@gmail.com", "alyssarcain03@gmail.com", "amaereynolds11@gmail.com", "amaereynolds11@gmail.com", "amandalynberry@gmail.com", "ambsplatt@yahoo.com", "amcginty139@gmail.ck", "amgann12@gmail.com", "ams72b@protonmail.com", "amy.susan@hotmail.com", "amycato012219@gmail.com", "amycvincent@gmail.com", "amygunn101@gmail.com", "anan.senthil@gmail.com", "anbesaabraham97@gmail.com", "ancurry95@gmail.com", "andavenport1@gmail.com", "andimariebowman@gmail.com", "andrea1andrews@yahoo.com", "andrew.culver@gmail.com", "andrewbconnell@hotmail.com", "andrewcmay@outlook.com", "andrewcmay@outlook.com", "andrewhinojosa48@gmail.com", "andrewhinojosa48@gmail.com", "andy.gilmore@gmail.com", "anedmonson@gmail.com", "angela.marie.mccullough@gmail.com", "angeleyes121037@gmail.com", "angelinaehein@gmail.com", "anguskittelman@missouri.edu", "angwrfam@mchsi.com", "animarisa20@yahoo.com", "anm21320@gmail.com", "annabroyles0525@gmail.com", "anniedoisy@gmail.com", "aprilskywilliams@yahoo.com", "aramis0907@icloud.com", "aren.koenig@gmail.com", "aren.koenig@gmail.com", "ariannaprince2@gmail.com", "arorvig@gmail.com", "arthurb123@myyahoo.com", "asapalmeri1996@gmail.com", "ashababy1028@yahoo.com", "ashepherd908@gmail.com", "asherickson0@gmail.com", "ashley.m.boehm@gmail.com", "ashley_n41@yahoo.com", "ashleyabehrends@gmail.com", "ashleygalvin27@gmail.com", "ashleymyra.c@gmail.com", "ashleystewy100@gmail.com", "ashleyvinsonteacher@gmail.com", "ashly.conner@yahoo.com", "ashton.huey@gmail.com", "atc210@yahoo.com", "atolmachoff1@hotmail.com", "atr2490@gmail.com", "atraxler62@gmail.com", "aubreywillmeth@icloud.com", "audrabarklage@gmail.com", "audreylaurane2011@gmail.com", "audreypreul@gmail.com", "auxioneer@me.com", "averystauffacher@gmail.com", "b.e.schiller@wustl.edu", "b.tobean@gmail.com", "baeplere@gmail.com", "bagilmore2008@gmail.com", "baileeulery@gmail.com", "bailey.29@hotmail.com", "ballet.101@live.com", "ballet.101@live.com", "barbaraellenhodges23@gmail.com", "barbaraellenhodges23@gmail.com", "bbjohnson79@gmail.com", "bbravo@me.com", "bcunningham@harvestkc.com", "bdonmoyer@ymail.com", "be12p@hotmail.com", "beard9107@hotmail.com", "beard9107@hotmail.com", "beasleyrenae00@gmail.com", "beccazeiger@gmail.com", "begley@hotmail.com", "bennirenelascuna@gmail.com", "benreed307@gmail.com", "beth_randall@yahoo.com", "betjokc@gmail.com", "betsey.kimes@gmail.com", "beverly_pr@hotmail.com", "beversdorfd@health.missouri.edu", "beversdorfd@health.missouri.edu", "bfranklin88@icloud.com", "bglide005@gmail.com", "bh777georgiapeach@yahoo.com", "bigmanisaacp@icloud.com", "billieseverns52@gmail.com", "bjhill2@cougars.ccis.edu", "bjhill2@cougars.ccis.edu", "bjutting22@gmail.com", "blairbarber88@yahoo.com", "blakesykes@icloud.com", "blcastona@gmail.com", "blessed.fred.chen@gmail.com", "blueandmccool@yahoo.com", "bnsutebu@gmail.com", "boadu203@gmail.com", "bobbiepauley@icloud.com", "bonniebrockman2@gmail.com", "boyce5024@gmail.com", "bp655274@gmail.com", "bpsteele4@gmail.com", "bradbuchanan@bellsouth.net", "bradbuchanan@bellsouth.net", "braden.daugherty05@gmail.com", "bradhaz13@aol.com", "bradylichtenberg@yahoo.com", "brandinsummers@icloud.com", "brandonjk94@gmail.com", "brandontodd5051@gmail.com", "brandonvogel1@gmail.com", "brandtparker6@gmail.com", "breckenhummel@gmail.com", "breckenhummel@gmail.com", "breedkarr@gmail.com", "brennend@gmail.com", "brentclark624@gmail.com", "brett.montag@oracle.com", "brian@thesummitstl.com", "briannawatson1663@gmail.com", "britanniak342@gmail.com", "britt.gerau@gmail.com", "brittanyann017@aol.com", "brittanynrollins@gmail.com", "brockrohler@gmail.com", "brooke05mckenzie@gmail.com", "brooklynirvin2018@gmail.com", "brooklynirvin2018@gmail.com", "bryan.watanabe@yahoo.com", "btreyton@gmail.com", "btsdrs@earthlink.net", "bulldog220@gmail.com", "butchered277@gmail.com", "buttercup13579@icloud.com", "bwilson@socket.net", "c658901@aol.com", "c_erdel@hotmail.com", "c_franklin93@hotmail.com", "c_tang@hotmail.com", "caby7y@umsystem.edu", "cacox@cpsk12.org", "cait.meyer21@gmail.com", "caitlinbond22@gmail.com", "calebcrowe1@gmail.com", "callielofton@gmail.com", "camillewalker47@gmail.com", "camire90@hotmail.com", "camperry254@gmail.com", "candice.rotter@gmail.com", "canger@bssd.net", "caramc53@yahoo.com", "carriesmithpeters@gmail.com", "carsonamiller@gmail.com", "carterjordan282@gmail.com", "caspercharlie1@gmail.com", "castlefan1942@gmail.com", "catelyn.weise@toltonstudent.org", "catherinefalliss@gmail.com", "catiehinton@gmail.com", "ccosta885@gmail.com", "ccosta885@gmail.com", "cdickin840@aol.com", "cdostaler2@yahoo.com", "celeste.durr16@hotmail.com", "cfarris65203@gmail.com", "cfarris65203@gmail.com", "cfstewart0821@gmail.com", "cgmadss@gmail.com", "chad.scherr@aol.com", "chammer297@gmail.com", "chandraku975@gmail.com", "channingcoslet6@icloud.com", "charishcollins@gmail.com", "chase.hannah.co@gmail.com", "chelcieproctor1345@gmail.com", "cherie.null@gmail.com", "cherie.null@gmail.com", "chievous32@gmail.com", "chikaya07en@gmail.com", "chilenaallen@icloud.com", "china.i.hill@gmail.com", "chloejruble@gmail.com", "chloewhite14@gmail.com", "chris.mckinney@thecrossingchurch.com", "christian.r.rodriguez28@gmail.com", "christiangirl5074@gmail.com", "christiesueritter@gmail.com", "chyannelumbert@gmail.com", "cjd4490@gmail.com", "ckjs1293@gmail.com", "claire.schoene@gmail.com", "clairebillington11@gmail.com", "clairemhawkins55@gmail.com", "clarkelijah18@gmail.com", "claudiaparmijo@gmail.com", "claytonklee2000@gmail.com", "cleigh22@outlook.com", "cntaganzwa@gmail.com", "codymschwartz@yahoo.com", "colby.corsaut@yahoo.com", "coleman.johnson3@gmail.com", "colemanwarren2014@gmail.com", "colton.downing@yahoo.com", "coltonmccauley@yahoo.com", "connerbarraco@gmail.com", "conyersjd@gmail.com", "cooper.shayla06@gmail.com", "cooperbrady1017@gmail.com", "cooperjp2006@gmail.com", "cora.slama@gmail.com", "corbinatterberry@gmail.com", "corey.benke@veteransunited.com", "coreylbond@outlook.com", "courtney0022@yahoo.com", "courtneybrand2021@gmail.com", "courtneykoetting@yahoo.com", "crahayden@gmail.com", "crich2712@gmail.com", "crucat07@icloud.com", "crystal2025@yahoo.com", "crystalcirrincione@gmail.com", "cschmitt126@gmail.com", "cstar47@yahoo.com", "cup@gmail.com", "curt.r.canine@gmail.com", "curtis.sieve@gmail.com", "curtiss_bunch@hotmail.com", "cwinder37@gmail.com", "cwoods23@cccb.edu", "cwoods5151@gmail.com", "cynthiamckee72@yahoo.com", "cyzr77@yahoo.com", "d.j.wulf87@gmail.com", "d49bradley@gmail.com", "dagmawitsolomon5710@gmail.com", "damila.oduolowu@gmail.com", "danacox993@gmail.com", "dangerer76@hotmail.com", "daniel.gyampo@gmail.com", "danielle_oyler@yahoo.com", "danielleann92386@gmail.com", "danielletyree@outlook.com", "danna.tracy@gmail.com", "danniereed@gmail.com", "darabethsharp@gmail.com", "darcyishimwe@gmail.com", "daren@setos.net", "darla.wilt@live.com", "darrellclippard13@gmail.com", "darrellclippard13@gmail.com", "darrinwalker1966@gmail.com", "david.reinhardt.woodard@gmail.com", "davidmiller2052@gmail.com", "davisg473@gmail.com", "dblbck@yahoo.com", "dbrud0113@gmail.com", "ddbacker2017@outlook.com", "ddometrorch@gmail.com", "ddometrorch@gmail.com", "debbie.highmark@gmail.com", "debirvin65@gmail.com", "debora.agahari@gmail.com", "debordblair@gmail.com", "debpierce@centurylink.net", "delrosarioloraine1@gmail.com", "denifalco@gmail.com", "denise@coyotehill.org", "dennis_forgy@yahoo.com", "derian.dodson@outlook.com", "desiraeholsman@gmail.com", "dflorres2@gmail.com", "dkeza123@icloud.com", "dkidwell6491@gmail.com", "dlfalconllc@gmail.com", "dlfalconllc@gmail.com", "dmlyons15@gmail.com", "dmmatn82@gmail.com", "dnitcher68@gmail.com", "dnovosel53@gmail.com", "donnakelley_1952@yahoo.com", "dorcas.nichols@yahoo.com", "dorothy_anderson@icloud.com", "dputter4@yahoo.com", "dr.ashleyemel@gmail.com", "droper11911@gmail.com", "drumbburton@gmail.com", "drumbburton@gmail.com", "druthmoore@hotmail.com", "dsabel1719@gmail.com", "duffer.8967.6@gmail.com", "dulrichhi@gmail.com", "duranearleywine@gmail.com", "dwightmc@gmail.com", "dwiswall@icloud.com", "dwp73@outlook.com", "dyanabeshay@gmail.com", "dye.holly@gmail.com", "e.catherineross@gmail.com", "ebddd@umsystem.edu", "ebuwaosaze@gmail.com", "egramke@icloud.com", "ehughton09@gmail.com", "ejayherradura@gmail.com", "elisiamc@gmail.com", "elizabeth.mckinney@thecrossingchurch.com", "elizabeth9915@icloud.com", "elizabethsteimel@gmail.com", "ellacate89@gmail.com", "ellacatherinecoe@gmail.com", "ellamaddox@socket.net", "ellasebek@icloud.com", "ellenmroberson@gmail.com", "elleseck10@gmail.com", "ellie.m.sjogren@gmail.com", "elliecrede@gmail.com", "embannister28@icloud.com", "emgasa30@gmail.om", "emily.ann.brownfield@gmail.com", "emily.ann.brownfield@gmail.com", "emily.young2020adl@gmail.com", "emily_custer2000@yahoo.com", "emilyg.solter@gmail.com", "emilyjjohnson27@yahoo.com", "emilyjurgensmeyer@gmail.com", "emitchell1519@gmail.com", "emma2527@yahoo.com", "emmadavis0220@icloud.com", "emmalinhill19@gmail.com", "emmiemgraves0326@icloud.com", "emmster2020@yahoo.com", "emmylizabeth12@yahoo.com", "erhc55@icloud.com", "eric@ericcox.us", "erika.j.mueller@gmail.com", "erinhiggins2014@gmail.com", "erinsieli@gmail.com", "eshadow2424@gmail.com", "esmcneal2013@gmail.com", "ettinger4127@gmail.com", "evan_boehm77@me.com", "evansc97@gmail.com", "ezra.kimball01@icloud.com", "fallen11911@gmail.com", "fbrockman03@gmail.com", "fbrockman03@gmail.com", "fernandagoeij@gmail.com", "fivenuns@gmail.com", "fivenuns@gmail.com", "flore518@yahoo.com", "flowerstom19@gmail.com", "foxjuliana24@gmail.com", "frazier86@yahoo.com", "freerobso@gmail.com", "froeschnerj@gmail.com", "funkygreendog73@yahoo.com", "gabegreis@gmail.com", "gabriellecaldwell000@gmail.com", "gabriellencarmo@gmail.com", "gallowayke@gmail.com", "garney92@gmail.com", "garrettantc@gmail.com", "garrykc84@yahoo.com", "gary.anderson313@aol.com", "gary.anderson313@aol.com", "garym0910@gmail.com", "geekgy@umsystem.edu", "genielian0718@gmail.com", "georgelb333@gmail.com", "ghampton@ctc.net", "ghansmeier12@icloud.com", "ghsantschi@gmail.com", "gigibearmo@icloud.com", "gina@mdbelt.com", "gina@mdbelt.com", "gingerplayz7000@gmail.com", "gjudd19@gmail.com", "gkkraske@gmail.com", "glkpr4lf@yahoo.com", "gloria-miller57@hotmail.com", "gnak2009@live.com", "gracegenne@gmail.com", "gracie.anderson10@icloud.com", "gracyhendershott13@gmail.com", "gracyhendershott13@gmail.com", "grananao3@icloud.com", "granren@swbell.net", "grantrgaines@icloud.com", "grdrewing2@gmail.com", "greencurry28@gmail.com", "greenedebbie20@gmail.com", "greenjo@missouri.edu", "gretchen.shults@thecrossingchurch.com", "greynolds01@gmail.com", "gustavobep@gmail.com", "gvictor2017@yahoo.com", "h-grenz@hotmail.com", "haileymaegregory@gmail.com", "halobelle1@gmail.com", "haltermanm14@gmail.com", "hannacweberr@gmail.com", "hannacweberr@gmail.com", "hannahlauren19@icloud.com", "hannahpkstf9705@icloud.com", "hawkeye3186@gmail.com", "hawkinsbl917@yahoo.com", "hayden.jokerst@icloud.com", "hayhaysleeth@gmail.com", "hayleeancell@yahoo.com", "hayleeancell@yahoo.com", "hayleec13@hotmail.com", "hayleelumbert@gmail.com", "hayleycape@gmail.com", "hcollins101985@hotmail.com", "heatherdavis2022@icloud.com", "heatherrae@mchsi.com", "heinj2010@gmail.com", "henryp123@aol.com", "hhytrek@yahoo.com", "highmanl@umsystem.edu", "hillaryhyde484@gmail.com", "hmsiegel01@gmail.com", "hmwxbb@gmail.com", "holli@hollishindler.com", "housekinser@gmail.com", "huangqingnancy23@hotmail.com", "hughes.grant14@gmail.com", "hughes.grant14@gmail.com", "hunner919@gmail.com", "hyewoonie09@yonsei.ac.kr", "iamaudreyc@gmail.com", "iftrost26@gmail.com", "innhawaii@msn.com", "iowa_bam@yahoo.com", "ire74nd@gmail.com", "irelandchap@gmail.com", "irontiger396@icloud.com", "isaac.cox@neighborsbank.com", "isaac.harrington27@gmail.com", "isaac.harrington27@gmail.com", "isaacmaclyn@gmail.com", "ivu010705@gmail.com", "izzylissone@gmail.com", "jackiebnichols@gmail.com", "jackstauss47@gmail.com", "jacobcolethomas@gmail.com", "jacobdodds@yahoo.com", "jacobthoenen@gmail.com", "jacobwolfey100@gmail.com", "jacobyoro@gmail.com", "jacquelynsheard@icloud.com", "jaegil911@gmail.com", "jafoulk@hotmail.com", "jakedixie23@gmail.com", "jalbright623@gmail.com", "jaleene_94@yahoo.com", "james@thetuningspot.com", "jamieemartin333@gmail.com", "janel_gillespie@yahoo.com", "janenelmt@gmail.com", "janiyahcarothers4@icloud.com", "janmcever@att.net", "jaredbrabant@hotmail.com", "jarodmellman@gmail.com", "jarrett.cathy@gmail.com", "jasonmboat@gmail.com", "jasonmboat@gmail.com", "jayceonlw@gmail.com", "jayoutl450@gmail.com", "jaysteacy1234@gmail.com", "jblok11911@gmail.com", "jchayden2@gmail.com", "jcvickie@aol.com", "jdent042@gmail.com", "jdwilsonsooner1@gmail.com", "jeanafavaro@gmail.com", "jeanie777@me.com", "jeepher1028@yahoo.com", "jeffbleijerveld@mac.com", "jeffghuebner@gmail.com", "jeffreygliksman7@gmail.com", "jeffrywinfree@gmail.com", "jeggemeyer@centralmethodist.edu", "jemrenaeb@gmail.com", "jenn-g@hotmail.com", "jenna.donnelly17@yahoo.com", "jennifer.hegerfeld@outlook.com", "jennifer.orford@gmail.com", "jeremy1202@gmail.com", "jerryesty@yahoo.com", "jerrykempf5@gmail.com", "jerrykempf5@gmail.com", "jess@hannibaltractor.com", "jessicaobuchowski@gmail.com", "jessicaross0923@gmail.com", "jessidmartin@gmail.com", "jessreschly@gmail.com", "jill_borg@chs.net", "jillratliff1995@gmail.com", "jillybriscoe@gmail.com", "jjz20376@gmail.com", "jjz20376@gmail.com", "jllnmoore@gmail.com", "jmaland@icloud.com", "jmancuso@cpsk12.org", "jmcruvinel19@gmail.com", "jmpreston01@gmail.com", "jo.lillig@me.com", "joannpizel@msn.com", "jococola@gmail.com", "joellefitz11@icloud.com", "john.richeson@outlook.com", "johnabritt@gmail.com", "johnpatonwheeler@icloud.com", "jon.levin@hotmail.com", "jordanalpha77@gmail.com", "jordongooden@hotmail.com", "josephturnerw@gmail.com", "joseyhulen@gmail.com", "joshpierre0520@gmail.com", "joylc74@yahoo.com", "joyray1225@gmail.com", "jrichardson86@att.net", "jroseyo@gmail.com", "jseiler349@gmail.com", "jstein79@hotmail.com", "jsuebranson@gmail.com", "jtkotzker@icloud.com", "jtoenges123@yahoo.com", "juanubaldo954@gmail.com", "jules13122@aol.com", "juliahelms05@gmail.com", "julieclarkwalters@gmail.com", "julieddot0213@gmail.com", "julietsung01@gmail.com", "jwdye00@gmail.com", "k.blecha@sbcglobal.net", "kailand.keith2013@outlook.com", "kaitg2008@gmail.com", "kaitlyn.schlup0@gmail.com", "kaitlynabrooks@gmail.com", "kaitlyngatewood@gmail.com", "kaitlyngatewood@gmail.com", "karamcalton@gmail.com", "karlie.mast@gmail.com", "kateemig@icloud.com", "kateirving22@gmail.com", "katewollmering@gmail.com", "katherinemscherry@icloud.com", "kathyroderick79@gmail.com", "katie.linn.miller@outlook.com", "katierhoads396@gmail.com", "kaylalamberson20@gmail.com", "kaylinnmae1995@gmail.com", "kbredehoeft@gmail.com", "kchiphop02@icloud.com", "kcolbert1015@gmail.com", "kdjames831@gmail.com", "keeleydiblasi@gmail.com", "keeleyl.splinter@gmail.com", "kelley@thecrossingchurch.com", "kelleylogan401@gmail.com", "kelleywilliams97@gmail.com", "kelliedysart2023@icloud.com", "kellyleavitt7@gmail.com", "kellyrlove21@yahoo.com", "kellysara1986@gmail.com", "kempkerl@health.missouri.edu", "kenata35@icloud.com", "kendorlious@gmail.com", "kendrickenterprises@gmail.com", "kent@mandhsolar.com", "keturner1996@gmail.com", "kevin.g.frazer@gmail.com", "kevin.m.martin180@gmail.com", "kfine115@gmail.com", "kgeiger949@gmail.com", "kgunn934@gmail.com", "khaylapittman@gmail.com", "ki_ji_ji@live.ca", "ki_ji_ji@live.ca", "kierst2002@hotmail.com", "kimbee1986@gmail.com", "kimfindlay00@gmail.com", "kimmiehartland@yahoo.com", "kingstoncashius@yahoo.com", "kingyeti65@gmail.com", "kinkead804@gmail.com", "kinzey.johnston23@icloud.com", "kjhasselbacher@icloud.com", "kjlic14@gmail.com", "kk.ware4@gmail.com", "kkweber13@gmail.com", "klapchenko@yahoo.com", "klapchenko@yahoo.com", "kleanng31@gmail.com", "klinekim821@ymail.com", "kmbutler_@outlook.com", "kmgermany04@gmail.com", "kmkkies@gmail.com", "knudsen.stefan@gmail.com", "koubamelissa@gmail.com", "kristachinnis@gmail.com", "kristacowan@mac.com", "kristendgholson@gmail.com", "kristengray2023@gmail.com", "kristina.foley621@gmail.com", "kristinheckman7@gmail.com", "krose@vu.com", "krystalkelly86@outlook.com", "krystalortiz111820@gmail.com", "ksuhakirina@yahoo.com", "ksully318@outlook.com", "kterry8306@gmail.com", "kuropk17@gmail.com", "kurtis.c.jensen@gmail.com", "kurtis.c.jensen@gmail.com", "kva_4@hotmail.com", "kwbab3@gmail.com", "kwinarskidc@gmail.com", "kyleemsimmons29@gmail.com", "kylemostat@outlook.com", "kylesmithpeters@gmail.com", "kylieprice2410@gmail.com", "kylieshoot@gmail.com", "ladaejapoole11@icloud.com", "ladymagsma3@gmail.com", "laf1013@gmail.com", "lainey.bealmer7@gmail.com", "lakechick@aol.com", "lakechick@aol.com", "lalagc0329@icloud.com", "lance@themowingmagician.com", "landenrscott@gmail.com", "landonroux23@gmail.com", "laneywhite10@gmail.com", "lannelamberta@gmail.com", "laraekostal@gmail.com", "larisaphillips6@gmail.com", "larryryanjr6@gmail.com", "larsonh13@gmail.com", "las3n7@umsystem.edu", "laura.e.wilson00@gmail.com", "laura.schemel@hotmail.com", "laurelbartling@hotmail.com", "lauren11ahrens@gmail.com", "laurenlmort@gmail.com", "laurenrhuff@gmail.com", "lbalwanz@gmail.com", "lbrocky@gmail.com", "ldfeltentruckingllc@gmail.com", "leahmckenziern@gmail.com", "leannakb@gmail.com", "leaton978@gmail.com", "lecox77@yahoo.com", "leeae23@hotmail.com", "leemurray2280@gmail.com", "leerosesenior@outlook.com", "lesaxe5@gmail.com", "levi9703@gmail.com", "lewisharleigh04@gmail.com", "lewms1@gmail.com", "lexihead12@gmail.com", "lgcassidy4@gmail.com", "lily.m.henderson@gmail.com", "lilyediti@gmail.com", "lilypat50@gmail.com", "lilypat50@gmail.com", "lindagrayrobertson@gmail.com", "lindapoe59@gmail.com", "lindsayducharme3@gmail.com", "lindscham11@gmail.com", "linley_stockman@yahoo.com", "lisa@neuendorfandassociates.com", "litwillerle@gmail.com", "liulangjohn@gmail.com", "ljmatson1005@gmail.com", "llmolczyk@gmail.com", "lml8781@yahoo.com", "logancornelius41@gmail.com", "lolapemberton26@icloud.com", "londonheinzler@gmail.com", "loribenton@live.com", "lorie.towe@gmail.com", "lowejustin87@yahoo.com", "lr4717161@gmail.com", "lr4717161@gmail.com", "lslstewa2@aol.com", "ltmiller1994@gmail.com", "lukemaas84@gmail.com", "lumland1@murraystate.edu", "lvanfarowe@gmail.com", "lyndsayrossman@gmail.com", "lynn@thecrossingchurch.com", "lyssrose02@gmail.com", "m3lody696969@gmail.com", "macknierim@gmail.com", "macway0911@icloud.com", "madalyn.gronewold@countryfinancial.com", "maddie.graeve@gmail.com", "maddoxmilbach@icloud.com", "madinoko@gmail.com", "makennagortmaker@gmail.com", "makennagortmaker@gmail.com", "malana.pence2004@gmail.com", "maliahanson2000@icloud.com", "mandylucky2003@yahoo.com.tw", "maocheng@ualberta.ca", "marcus@theboulderinggarden.com", "mariah.schmeling@icloud.com", "marilabau13093@gmail.com", "markkarr2@gmail.com", "marsha.wulf71@gmail.com", "maryandkent@comcast.net", "maryc6583@gmail.com", "maryc@lovecolumbia.org", "maryeherts@gmail.com", "marystrada@live.com", "marystrada@live.com", "masonperkins05@gmail.com", "mattdroach@gmail.com", "matthew.lx.levine@gmail.com", "matthew.t.mcconnell@gmail.com", "matthewbarton23@icloud.com", "mattnivens26@gmail.com", "maty04@live.com", "maureenharris740@gmail.com", "maxwell101mang@gmail.com", "maxwell101mang@gmail.com", "mbarbuzze@yahoo.com", "mbjames2017@gmail.com", "mcchase2015@gmail.com", "mccoy1206@yahoo.com", "mccoyrosanne@yahoo.com", "mccoyrosanne@yahoo.com", "mclellanmark@gmail.com", "mdavidcanada@gmail.com", "measton13@gmail.com", "mecca.chance@gmail.com", "megand381@gmail.com", "megandues@gmail.com", "megang1321@gmail.com", "mejcomp@gmail.com", "melanascharfen@gmail.com", "melissa.white9@live.com", "melissa.white9@live.com", "melissawelch47@gmail.com", "merchantbrandi2@gmail.com", "meredithwalker64@gmail.com", "mescal101@gmail.com", "mfelten.fnp@gmail.com", "mharrisonrn51@outlook.com", "mhartley1031@gmail.com", "micahkbaker@gmail.com", "micahkbaker@gmail.com", "michael.f.boffa@gmail.com", "michelle.cooper1893@gmail.com", "michelle@marketizeit.com", "michellefburke@gmail.com", "michellefburke@gmail.com", "midgyett58@gmail.com", "mikehalliburton07@gmail.com", "mindy.brabec2@gmail.com", "minnickmikayla@gmail.com", "mirandajstegeman@gmail.com", "miriamtye@gmail.com", "missjohnsonjay@gmail.com", "mizacook@gmail.com", "mizzougomizzou2@gmail.com", "mizzouoz@ymail.com", "mjminnix@icloud.com", "mkthy3@yahoo.com", "mlroeleveld@gmail.com", "mlwipfler@gmail.com", "mmason6@msn.com", "mmhcn66@icloud.com", "mmiller2201@gmail.com", "mmiller2201@gmail.com", "mmisirova@yahoo.com", "mmolex22@gmail.com", "mnstorzer1@cougars.ccis.edu", "mom2wyattnaaron@gmail.com", "monagreening57@gmail.com", "monet539@gmail.com", "monica.pennewitt@gmail.com", "monicakatnik@gmail.com", "monicathornburg10@gmail.com", "mordestjembere@gmail.com", "morgan.g.miller17@gmail.com", "morgancreel01@gmail.com", "morganschaefer2023@gmail.com", "motherbrooks@gmail.com", "moussaffi.uri@gmail.com", "mrs.halik@gmail.com", "mrs.terirose@gmail.com", "mrsdejesus@hotmail.com", "mrslumbert2016@gmail.com", "mrsozzyb@gmail.com", "msfaran@gmail.com", "mshp797@hotmail.com", "msotiker@gmail.com", "mta424@yahoo.com", "mtague00@gmail.com", "mtague00@gmail.com", "mutonisonia1@gmail.com", "mwamp10@icloud.com", "mwier1219@gmail.com", "mya_garcia@ymail.com", "mybony118@yahoo.com", "nancyandmikeyb@gmail.com", "nancyforgy@gmail.com", "naomikaranja457@gmail.com", "nashalliecortes@gmail.com", "nashstowing@gmail.com", "natashayoung83@gmail.com", "nathanshainmedia@gmail.com", "natloaiza@gmail.com", "nattyrooroo04@gmail.com", "nblume23@gmail.com", "nerfgunfun1@gmail.com", "nexusltg@icloud.com", "nfreeman5972@gmail.com", "ngarner4668@gmail.com", "nibowkins5@gmail.com", "nick.d.lay@outlook.com", "nick@turpin.com", "nicoleriherter@gmail.com", "nicolesjostrand03@gmail.com", "nikkialeto@gmail.com", "njhess12@gmail.com", "nkardh@yahoo.com", "nlin@gmail.com", "nmoreau1870@gmail.com", "nolls20@hotmail.com", "nreynoldsc21@gmail.com", "nrobinson1948@gmail.com", "nsharp_nls@yahoo.com", "nubertha.jackson4@gmail.com", "nursegrlc@aol.com", "nykhalac@gmail.com", "officialrosaria@gmail.com", "olivetree0711@yahoo.com", "oliviag.bannister@icloud.com", "oluwat2003@gmail.com", "oncloud_09@icloud.com", "padenkleinhesselink@gmail.com", "padraic.ge@hotmail.com", "paigebrenfrow@gmail.com", "paigequilty@gmail.com", "paris.fang2@gmail.com", "pastor.ruffin@gmail.com", "patrick.harkleroad@gmail.com", "patrickcraighead@gmail.com", "pattycornell59@gmail.com", "paytonburton60@gmail.com", "paytonshamp345@gmail.com", "peachesandcreammm4@gmail.com", "perrybrummett77@gmail.com", "philwthornton@gmail.com", "plottersworld@yahoo.com", "plovebocomo@gmail.com", "pmarr348@gmail.com", "pmless5442@gmail.com", "pnoftsinger@yahoo.com", "preciousivy732@gmail.com", "prem_gv@hotmail.com", "pritchardjmichelle@gmail.com", "pwatts@hot.rr.com", "quarlesjamarya@yahoo.com", "ra.ribelin@gmail.com", "rabernskoetter@gmail.com", "rachaelnpowell@gmail.com", "rachaelwolf45@gmail.com", "rachelbcross@gmail.com", "rachelsuewestfall@yahoo.com", "raechel.ford@yahoo.com", "rakrasik@gmail.com", "rariggs@gmail.com", "rasberrymel@yahoo.com", "rasheedg.rg@gmail.com", "rayleeemma@icloud.com", "rays4277@gmail.com", "rbiggerstaff2015@gmail.com", "rcheno8@mac.com", "rcheno8@mac.com", "rcleuthauser@yahoo.com", "rcmueller28@gmail.com", "rcox@myyahoo.com", "readykt@aim.com", "reamsheather16@gmail.com", "reb2004@hotmail.com", "rebeccaharris.bills@gmail.com", "rebeccaharris.phone@gmail.com", "rebekahannprasuhn@gmail.com", "redman6434@gmail.com", "redman6434@gmail.com", "reedpryor@yahoo.com", "reesemackey@gmail.com", "reesemackey@gmail.com", "regan.muth@gmail.com", "rehagenmary@gmail.com", "rehmajgs@gmail.com", "remuenks@gmail.com", "renee.kesler1997@yahoo.com", "reneelgard@gmail.com", "renellachouest@gmail.com", "resource@resourcetreeandland.com", "resource@resourcetreeandland.com", "revee.white@gmail.com", "rgc002@yahoo.com", "rgresham@mediacombb.net", "riceg347@gmail.com", "richardhg71@gmail.com", "richardson1222@gmail.com", "richardson1222@gmail.com", "rikerhale@hotmail.com", "ritah3307@yahoo.com", "riva@enterlife.net", "rjsunna@gmail.com", "rkbailey13@gmail.com", "rlb0739@yahoo.com", "rldingley@gmail.com", "robertthorndyke934@yahoo.com", "robertvelasco672@gmail.com", "rochara.l.knight@gmail.com", "rojobryant@gmail.com", "romes08@yahoo.com", "rooks.christa@gmail.com", "rooks.christa@gmail.com", "rorscheln@orscheln.com", "rpeterson8482@gmail.com", "rrmcbrid33@gmail.com", "rschlotz@centurylink.net", "rustylolo4@yahoo.com", "ruthmead11@icloud.com", "rutledge2259@hotmail.com", "ryan.doppelt@gmail.com", "ryandsonya@yahoo.com", "ryanhfox99@gmail.com", "ryanwd43@gmail.com", "ryliemckee022@gmail.com", "s.namizima479@gmail.com", "sakerstherapy@outlook.com", "samanthamyers327@gmail.com", "samcambron@gmail.com", "samschupp1@gmail.com", "samual.seiders@thehartford.com", "sandradene@mchsi.com", "sandrawald4@gmail.com", "sandsdent@gmail.com", "sandsdent@gmail.com", "sarahbmiller15@gmail.com", "sarahgstair@gmail.com", "sarahjanehendry2004@gmail.com", "savannahs.umland@gmail.com", "savannatrivers21@gmail.com", "sbaileym12@gmail.com", "schelp.breanna@gmail.com", "sclesliewilson@gmail.com", "scotsdeasley@gmail.com", "scottbelden@me.com", "sdenisewebb@gmail.com", "sedlmayrlisa@yahoo.com", "sedonabeth@outlook.com", "sem3909o@gmail.com", "sfrancis@ashland.k12.mo.us", "sgolan@missouri.edu", "sh8gr@missouri.edu", "sharonangelemail@gmail.com", "sharonef@hotmail.com", "sharonef@hotmail.com", "shauna@srshelp.org", "shawnakcook@gmail.com", "shawnta573@gmail.com", "shawnta573@gmail.com", "shayelh@icloud.com", "she.fw.r3@outlook.com", "shelbybutterworth@gmail.com", "shelbyhuey03@gmail.com", "shepard_stephanie@icloud.com", "sheritwaddle@hotmail.com", "sheritwaddle@hotmail.com", "shortsteinman1987@gmail.com", "shortymom7755186@aol.com", "shovey18@gmail.com", "sierradudley01@gmail.com", "sihu1049@gmail.com", "silverbattalion14@gmail.com", "silverbattalion14@gmail.com", "simonsmeredith@gmail.com", "sirbobthewise@gmail.com", "sjeanylle@gmail.com", "sjshack@gmail.com", "skottwitz28@yahoo.com", "skrypniukmykola@gmail.com", "skylarsullins2023@gmail.com", "sldrewing@gmail.com", "slkudrna@yahoo.com", "smcroberts1@gmail.com", "smitebn@aol.com", "smith.brayden@yahoo.com", "smithchrista@missouri.edu", "smyc0905@gmail.com", "sophiaedyer@gmail.com", "sophiarklimek@yahoo.com", "sophieschupp20@gmail.com", "sophieschupp20@gmail.com", "sparjant90@gmail.com", "spiritensh1@gmail.com", "sragan86@gmail.com", "srallen0210@gmail.com", "ssturgess.emily@gmail.com", "staci_hurst@aol.com", "stackhouseshelby@gmail.com", "stacymariehirt@gmail.com", "stambaughkristen71@gmail.com", "stambaughkristen71@gmail.com", "stellajadam@icloud.com", "stephhanneken@yahoo.com", "steve.pankey@gmail.com", "stevemdean@gmail.com", "stevenjamieson1021@gmail.com", "stevenray1227@gmail.com", "stew614460@gmail.com", "stewjg@gmail.com", "stewjg@gmail.com", "stirlingsarahgrace@gmail.com", "stwehous7@gmail.com", "subarooster@icloud.com", "suebluebird2013@gmail.com", "suebluebird2013@gmail.com", "sunflo0818@gmail.com", "sunflowersgrace0408@gmail.com", "suzanne.weimer@gmail.com", "sveasman0305@gmail.com", "syd123alter@gmail.com", "sydneeplayter@icloud.com", "sydney_klimek1@baylor.edu", "sydneygraef@icloud.com", "sydneymarieprice27@gmail.com", "szoellner56@icloud.com", "t_shep@icloud.com", "talktovoss@gmail.com", "taltwies@gmail.com", "tameracollet@gmail.com", "tammifitch@yahoo.com", "tammybise@sbcglobal.net", "tanya.alm@mspd.mo.gov", "tanya.r.woodard@gmail.com", "tarrayoung4@gmail.com", "tayhquint24@icloud.com", "taylor@simpledonation.com", "taylorscott109@gmail.com", "tbr@bikerider.com", "tcbohm@gmail.com", "tcbohm@gmail.com", "tdmerritt28@gmail.com", "teddibear65@hotmail.com", "teneleven1011@gmail.com", "teresasutton@ymail.com", "terri.march@vu.com", "terryosilvfox@hotmail.co.uk", "tesme@juno.com", "tessaspires@gmail.com", "thearmorofgod82@gmail.com", "thekidatco7@gmail.com", "thelundholms@gmail.com", "thelundholms@gmail.com", "therichardholt@gmail.com", "thewaltersworld5shop@gmail.com", "thewaltersworld5shop@gmail.com", "thighland52@aol.com", "thighland52@aol.com", "thomasnatalierose@gmail.com", "tianastrawn@gmail.com", "tiarapatrick3@gmail.com", "tiffanyjfeldman@gmail.com", "tigistat@gmail.com", "timothy.cohee@comcast.net", "timsoliver@gmail.com", "tinabrazas@aol.com", "tionna_jackson@yahoo.com", "tjoehl31@gmail.com", "tjritter@live.com", "tlhmb6@gmail.com", "tmmehlberg@yahoo.com", "tmpbb7@aol.com", "tnnolan15@gmail.com", "toppinsamber@icloud.com", "tracy.jiang7@gmail.com", "tracysieve@gmail.com", "travisoden13@gmail.com", "trentontoigo@icloud.com", "trinity8_reiss@hotmail.com", "trserrin@gmail.com", "tstoked40@gmail.com", "tteena0523@gmail.com", "tundeblu@hotmail.com", "tydubick@gmail.com", "tyleeschnakefive@gmail.com", "tyleeschnakefive@gmail.com", "tyler.warren20@yahoo.com", "tyler_7_93@yahoo.com", "tyler_schrag_20@hotmail.com", "tylerbracht15@gmail.com", "uarebuwa@gmail.com", "vanmarlek@gmail.com", "vaughtjammie@gmail.com", "vbkapila@gmail.com", "victoria-97@live.com", "vivian3allen33@gmail.com", "vkhollingsworth1@gmail.com", "volleyballsoftball@outlook.com", "vrorvig@gmail.com", "w.wipfler@gmail.com", "waknowles98@gmail.com", "wallen4442@gmail.com", "waydeparman21@gmail.com", "wcockma@clemson.edu", "whitancheta@gmail.com", "white.debi@gmail.com", "white.debi@gmail.com", "whitedebbie08@gmail.com", "whitworthk@missouri.edu", "willgmor@aol.com", "williamlopez9027@gmail.com", "willomerrylopez@yahoo.com", "willpriest50@gmail.com", "wilson115mx@gmail.com", "wilsonfarmhike@gmail.com", "winterb0518@gmail.com", "wkb82292@gmail.com", "woodc7659@gmail.com", "yangminseok1011@gmail.com", "yardakay@aol.com", "yvonnemugisha51@yahoo.com", "zaiyanc1@gmail.com", "zane.torreyson@como.gov", "zaneabbott83@gmail.com", "zbwright20@gmail.com", "zcaylor23@gmail.com", "zclanton@msn.com", "zjdydxw1984@gmail.com", "zjenna@live.com", "ztriga79@gmail.com" };

            var z = contacts.Where( c => emails.Contains( c.properties.email ) ).ToList();
            var y = all_contacts.Where( c => emails.Contains( c.properties.email ) ).ToList();
            var x = 7;
            //Foreach contact with an email, look for a 1:1 match in Rock by email and schedule it's update 
            //WriteToLog( string.Format( "Total Contacts to Match: {0}", contacts.Count() ) );
            for ( var i = 0; i < contacts.Count(); i++ )
            {
                //Stopwatch watch = new Stopwatch();
                //watch.Start();
                //First Check if they have a rock Id in their hubspot data to use
                Person person = null;
                bool hasMultiEmail = false;
                List<int> matchingIds = FindPersonIds( contacts[i] );
                if ( matchingIds.Count > 1 )
                {
                    hasMultiEmail = true;
                }
                if ( matchingIds.Count == 1 )
                {
                    person = personService.Get( matchingIds.First() );
                }

                //For Testing
                //WriteToLog( string.Format( "{1}i: {0}", i, Environment.NewLine ) );
                //WriteToLog( string.Format( "    After SQL: {0}", watch.ElapsedMilliseconds ) );

                //Atempt to match 1:1 based on email history making sure we exclude emails with multiple matches in the person table
                if ( person == null && !hasMultiEmail )
                {
                    string email = contacts[i].properties.email.ToLower();
                    var matches = history_svc.Queryable().Where( hist => hist.EntityTypeId == 15 && hist.ValueName == "Email" && hist.NewValue.ToLower() == email ).Select( hist => hist.EntityId ).Distinct();
                    if ( matches.Count() == 1 )
                    {
                        //If 1:1 Email match and Hubspot has no other info, make it a match
                        if ( String.IsNullOrEmpty( contacts[i].properties.firstname ) && String.IsNullOrEmpty( contacts[i].properties.lastname ) )
                        {
                            person = personService.Get( matches.First() );
                        }
                    }
                }
                //WriteToLog( string.Format( "    After History: {0}", watch.ElapsedMilliseconds ) );

                bool inBucket = false;
                //Try to mark people that are potential matches, only people who can at least match email or phone and one other thing
                if ( person == null )
                {
                    var contact = contacts[i];
                    //Matches phone number and one other piece of info
                    if ( !String.IsNullOrEmpty( contact.properties.phone ) )
                    {
                        var phone_matches = personService.Queryable().Where( p => p.PhoneNumbers.Select( pn => pn.Number ).Contains( contact.properties.phone ) ).ToList();
                        if ( phone_matches.Count() > 0 )
                        {
                            phone_matches = phone_matches.Where( p => CustomEquals( p.FirstName, contact.properties.firstname ) || CustomEquals( p.NickName, contact.properties.firstname ) || CustomEquals( p.Email, contact.properties.email ) || CustomEquals( p.LastName, contact.properties.lastname ) ).ToList();
                            for ( var j = 0; j < phone_matches.Count(); j++ )
                            {
                                //Save this information in the excel sheet....
                                SaveData( worksheet, row, phone_matches[j], contact );
                                inBucket = true;
                                row++;
                            }
                        }
                    }
                    //Matches email and one other piece of info
                    var email_matches = personService.Queryable().ToList().Where( p =>
                    {
                        return CustomEquals( p.Email, contact.properties.email );
                    } ).ToList();
                    if ( email_matches.Count() > 0 )
                    {
                        email_matches = email_matches.Where( p => CustomEquals( p.FirstName, contact.properties.firstname ) || CustomEquals( p.NickName, contact.properties.firstname ) || ( !String.IsNullOrEmpty( contact.properties.phone ) && p.PhoneNumbers.Select( pn => pn.Number ).Contains( contact.properties.phone ) ) || CustomEquals( p.LastName, contact.properties.lastname ) ).ToList();
                        for ( var j = 0; j < email_matches.Count(); j++ )
                        {
                            //Save this information in the excel sheet....
                            SaveData( worksheet, row, email_matches[j], contact );
                            inBucket = true;
                            row++;
                        }
                    }
                    //if (inBucket)
                    //{
                    //    WriteToLog( string.Format( "    Added data to bucket!" ) );
                    //}
                    //WriteToLog( string.Format( "    After Bucket: {0}", watch.ElapsedMilliseconds ) );
                }

                //For Testing
                //if (contacts[i].properties.email != "coolrobot@hubspot.com")
                //{
                //    person = null;
                //}
                //else
                //{
                //    person = personService.Get( 1 );
                //}

                //Schedule HubSpot update if 1:1 match
                if ( person != null )
                {
                    var properties = new List<HubspotPropertyUpdate>() { new HubspotPropertyUpdate() { property = "rock_id", value = person.Id.ToString() } };

                    //If the Hubspot Contact does not have FirstName, LastName, or Phone Number we want to update those...
                    if ( String.IsNullOrEmpty( contacts[i].properties.firstname ) )
                    {
                        properties.Add( new HubspotPropertyUpdate() { property = "firstname", value = person.NickName } );
                    }
                    if ( String.IsNullOrEmpty( contacts[i].properties.lastname ) )
                    {
                        properties.Add( new HubspotPropertyUpdate() { property = "lastname", value = person.LastName } );
                    }
                    if ( String.IsNullOrEmpty( contacts[i].properties.phone ) )
                    {
                        PhoneNumber mobile = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == 12 );
                        if ( mobile != null && !mobile.IsUnlisted )
                        {
                            properties.Add( new HubspotPropertyUpdate() { property = "phone", value = mobile.NumberFormatted } );
                        }
                    }
                    var url = $"https://api.hubapi.com/crm/v3/objects/contacts/{contacts[i].id}";
                    MakeRequest( current_id, url, properties, 0 );
                    //WriteToLog( string.Format( "    After Request: {0}", watch.ElapsedMilliseconds ) );
                }
                else
                {
                    if ( inBucket )
                    {
                        var alreadyKnown = contacts[i].properties.has_potential_rock_match;
                        if ( alreadyKnown != "True" )
                        {
                            //We don't have an exact match but we have guesses, so update Hubspot to reflect that.
                            var bucket_prop = props.FirstOrDefault( p => p.label == "Rock Custom Has Potential Rock Match" );
                            var properties = new List<HubspotPropertyUpdate>() { new HubspotPropertyUpdate() { property = bucket_prop.name, value = "True" } };
                            var url = $"https://api.hubapi.com/crm/v3/objects/contacts/{contacts[i].id}";
                            MakeRequest( current_id, url, properties, 0 );
                            //WriteToLog( string.Format( "    After Request: {0}", watch.ElapsedMilliseconds ) );
                        }
                        //If it is already known, do nothing
                    }
                }
                //WriteToLog( string.Format( "    End of Iteration: {0}", watch.ElapsedMilliseconds ) );
                //watch.Stop();
            }

            byte[] sheetbytes = excel.GetAsByteArray();
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Content\\" + dataMap.GetString( "PotentialMatchesFileName" ) + ".xlsx";
            System.IO.File.WriteAllBytes( path, sheetbytes );
        }

        private void MakeRequest( int current_id, string url, List<HubspotPropertyUpdate> properties, int attempt )
        {
            //Update the Hubspot Contact
            try
            {
                //For Testing Write to Log File
                WriteToLog( string.Format( "{0}     ID: {1}{2}PROPS:{2}{3}", RockDateTime.Now.ToString( "HH:mm:ss.ffffff" ), current_id, Environment.NewLine, JsonConvert.SerializeObject( properties ) ) );

                //var client = new RestClient( url );
                //client.Timeout = -1;
                //var request = new RestRequest( Method.PATCH );
                //request.AddHeader( "accept", "application/json" );
                //request.AddHeader( "content-type", "application/json" );
                //request.AddHeader( "Authorization", $"Bearer {key}" );
                //request.AddParameter( "application/json", $"{{\"properties\": {{ {String.Join( ",", properties.Select( p => $"\"{p.property}\": \"{p.value}\"" ) )} }} }}", ParameterType.RequestBody );
                //IRestResponse response = client.Execute( request );
                //if ( ( int ) response.StatusCode == 429 )
                //{
                //    if ( attempt < 3 )
                //    {
                //        Thread.Sleep( 9000 );
                //        MakeRequest( current_id, url, properties, attempt + 1 );
                //    }
                //}
                //if ( response.StatusCode != HttpStatusCode.OK )
                //{
                //    throw new Exception( response.Content );
                //}
            }
            catch ( Exception e )
            {
                var json = $"{{\"properties\": {JsonConvert.SerializeObject( properties )} }}";
                ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error{Environment.NewLine}{e}{Environment.NewLine}Current Id: {current_id}{Environment.NewLine}Exception from Request:{Environment.NewLine}{e.Message}{Environment.NewLine}Request:{Environment.NewLine}{json}{Environment.NewLine}" ) );
            }
        }

        private void WriteToLog( string message )
        {
            string logFile = System.Web.Hosting.HostingEnvironment.MapPath( "~/App_Data/Logs/HubSpotMatchLog.txt" );
            using ( System.IO.FileStream fs = new System.IO.FileStream( logFile, System.IO.FileMode.Append, System.IO.FileAccess.Write ) )
            {
                using ( System.IO.StreamWriter sw = new System.IO.StreamWriter( fs ) )
                {
                    sw.WriteLine( message );
                }
            }

        }

        private void GetContacts( string url, string after )
        {
            request_count++;
            var contactClient = new RestClient( url );
            contactClient.Timeout = -1;
            var contactRequest = new RestRequest( Method.POST );
            contactRequest.AddHeader( "Authorization", $"Bearer {key}" );
            contactRequest.AddHeader( "accept", "application/json" );
            contactRequest.AddHeader( "content-type", "application/json" );
            var body = @"{" + "\n" +
            @"    ""filterGroups"": [" + "\n" +
            @"        {" + "\n" +
            @"            ""filters"": [" + "\n" +
            @"                {" + "\n" +
            @"                    ""operator"": ""IN""," + "\n" +
            @"                    ""propertyName"": ""hs_all_assigned_business_unit_ids""," + "\n" +
            @"                    ""values"": [" + "\n" +
            @"                        ""0""," + "\n" +
            @"                        ""137869""" + "\n" +
            @"                    ]" + "\n" +
            @"                }," + "\n" +
            @"                {" + "\n" +
            @"                    ""operator"": ""NOT_HAS_PROPERTY""," + "\n" +
            @"                    ""propertyName"": ""rock_id""" + "\n" +
            @"                }" + "\n" +
            @"            ]" + "\n" +
            @"        }" + "\n" +
            @"    ]," + "\n" +
            @"    ""limit"": ""100""," + "\n" +
            @"    ""after"": """ + after + @"""," + "\n" +
            @"    ""properties"": [" + "\n" +
            @"        ""email""," + "\n" +
            @"        ""firstname""," + "\n" +
            @"        ""lastname""," + "\n" +
            @"        ""phone""," + "\n" +
            @"        ""hs_all_assigned_business_unit_ids""," + "\n" +
            @"        ""rock_id""," + "\n" +
            @"        ""which_best_describes_your_involvement_with_the_crossing_""," + "\n" +
            @"        ""has_potential_rock_match""," + "\n" +
            @"        ""createdate""," + "\n" +
            @"        ""lastmodifieddate""" + "\n" +
            @"    ]" + "\n" +
            @"}";
            contactRequest.AddParameter( "application/json", body, ParameterType.RequestBody );
            IRestResponse contactResponse = contactClient.Execute( contactRequest );
            var contactResults = JsonConvert.DeserializeObject<HSContactQueryResult>( contactResponse.Content );
            //Contacts with emails that do not already have Rock IDs in the desired business unit
            contacts.AddRange( contactResults.results.Where( c => c.properties.email != null && c.properties.email != "" && ( c.properties.rock_id == null || c.properties.rock_id == "" || c.properties.rock_id == "0" ) && c.properties.hs_all_assigned_business_unit_ids != null && c.properties.hs_all_assigned_business_unit_ids.Split( ';' ).Contains( businessUnit ) ).ToList() );
            all_contacts.AddRange( contactResults.results );
            //For Testing
            //if ( contacts.Count >= 1000 )
            //{
            //    return;
            //}

            if ( contactResults.paging != null && contactResults.paging.next != null && !String.IsNullOrEmpty( contactResults.paging.next.after ) && request_count < 500 )
            {
                GetContacts( url, contactResults.paging.next.after );
            }
        }

        private List<int> FindPersonIds( HSContactResult contact )
        {
            using ( RockContext context = new RockContext() )
            {
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter( "@rock_id", contact.properties.rock_id.HasValue() ? contact.properties.rock_id : "" ),
                    new SqlParameter( "@first_name", contact.properties.firstname.HasValue() ? contact.properties.firstname : "" ),
                    new SqlParameter( "@last_name", contact.properties.lastname.HasValue() ? contact.properties.lastname : "" ),
                    new SqlParameter( "@email", contact.properties.email.HasValue() ? contact.properties.email : "" ),
                    new SqlParameter( "@mobile_number", contact.properties.phone.HasValue() ? contact.properties.phone : "" ),
                };
                var query = context.Database.SqlQuery<int>( $@"
DECLARE @matches int = (SELECT COUNT(*) FROM Person WHERE Email = @email);

SELECT DISTINCT Person.Id
FROM Person
         LEFT OUTER JOIN PhoneNumber ON Person.Id = PhoneNumber.PersonId
WHERE ((@email IS NOT NULL AND @email != '') AND
       (Email = @email AND
        (((@first_name IS NULL OR @first_name = '') AND (@last_name IS NULL OR @last_name = '') AND @matches = 1) OR
         ((@first_name IS NOT NULL AND @first_name != '' AND
           (FirstName = @first_name OR NickName = @first_name)) OR
          (@last_name IS NOT NULL AND @last_name != '' AND LastName = @last_name) OR
          (@mobile_number IS NOT NULL AND @mobile_number != '' AND
           (Number = @mobile_number OR PhoneNumber.NumberFormatted = @mobile_number))))))
", sqlParams ).ToList();
                return query;
            }
        }

        private ExcelWorksheet ColorCell( ExcelWorksheet worksheet, int row, int col )
        {
            //Color the Matching Data Green 
            Color c = System.Drawing.ColorTranslator.FromHtml( "#9CD8BC" );
            worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor( c );
            return worksheet;
        }

        private ExcelWorksheet SaveData( ExcelWorksheet worksheet, int row, Person person, HSContactResult contact )
        {
            //Add FirstNames
            worksheet.Cells[row, 1].Value = contact.properties.firstname;
            worksheet.Cells[row, 2].Value = person.NickName;
            if ( person.NickName != person.FirstName )
            {
                worksheet.Cells[row, 2].Value += " (" + person.FirstName + ")";
            }
            //Color cells if they match
            if ( CustomEquals( contact.properties.firstname, person.FirstName ) || CustomEquals( contact.properties.firstname, person.NickName ) )
            {
                worksheet = ColorCell( worksheet, row, 1 );
                worksheet = ColorCell( worksheet, row, 2 );
            }

            //Add LastNames
            worksheet.Cells[row, 3].Value = contact.properties.lastname;
            worksheet.Cells[row, 4].Value = person.LastName;
            //Color cells if they match 
            if ( CustomEquals( contact.properties.lastname, person.LastName ) )
            {
                worksheet = ColorCell( worksheet, row, 3 );
                worksheet = ColorCell( worksheet, row, 4 );
            }

            //Add Emails
            worksheet.Cells[row, 5].Value = contact.properties.email;
            worksheet.Cells[row, 6].Value = person.Email;
            //Color cells if they match
            if ( CustomEquals( contact.properties.email, person.Email ) )
            {
                worksheet = ColorCell( worksheet, row, 5 );
                worksheet = ColorCell( worksheet, row, 6 );
            }

            //Add Phone Numbers
            var num = person.PhoneNumbers.FirstOrDefault( pn => pn.Number == contact.properties.phone );
            worksheet.Cells[row, 7].Value = contact.properties.phone;
            worksheet.Cells[row, 8].Value = num != null ? num.Number : "No Matching Number";
            //Color cells if they match
            if ( num != null && CustomEquals( contact.properties.phone, num.Number ) )
            {
                worksheet = ColorCell( worksheet, row, 7 );
                worksheet = ColorCell( worksheet, row, 8 );
            }

            //Add Connection Statuses
            worksheet.Cells[row, 9].Value = contact.properties.which_best_describes_your_involvement_with_the_crossing_;
            worksheet.Cells[row, 10].Value = person.ConnectionStatusValue;

            //Add links
            worksheet.Cells[row, 11].Value = "https://app.hubspot.com/contacts/6480645/contact/" + contact.id;
            worksheet.Cells[row, 12].Value = "https://rock.thecrossingchurch.com/Perosn/" + person.Id;

            //Add Created Dates
            if ( !String.IsNullOrEmpty( contact.properties.createdate ) )
            {
                DateTime hubspotVal;
                if ( DateTime.TryParse( contact.properties.createdate, out hubspotVal ) )
                {
                    worksheet.Cells[row, 13].Value = hubspotVal.ToString( "MM/dd/yyyy" );
                }
            }
            worksheet.Cells[row, 14].Value = person.CreatedDateTime.Value.ToString( "MM/dd/yyyy" );

            //Add Modified Dates
            if ( !String.IsNullOrEmpty( contact.properties.lastmodifieddate ) )
            {
                DateTime hubspotVal;
                if ( DateTime.TryParse( contact.properties.lastmodifieddate, out hubspotVal ) )
                {
                    worksheet.Cells[row, 15].Value = hubspotVal.ToString( "MM/dd/yyyy" );
                }
            }
            worksheet.Cells[row, 16].Value = person.ModifiedDateTime.Value.ToString( "MM/dd/yyyy" );

            //Add Rock Id
            worksheet.Cells[row, 17].Value = person.Id;


            return worksheet;
        }

        private bool CustomEquals( string a, string b )
        {
            if ( !String.IsNullOrEmpty( a ) && !String.IsNullOrEmpty( b ) )
            {
                return a.ToLower() == b.ToLower();
            }
            return false;
        }

    }

    [DebuggerDisplay( "Label: {label}, FieldType: {fieldType}" )]
    public class HubspotProperty
    {
        public string name { get; set; }
        public string label { get; set; }
        public string fieldType { get; set; }
        public string groupName { get; set; }
    }

    public class HubspotPropertyUpdate
    {
        public string property { get; set; }
        public string value { get; set; }
    }

    public class HSContactProperties
    {
        public string createdate { get; set; }
        public string email { get; set; }
        public string firstname { get; set; }
        public string has_potential_rock_match { get; set; }
        public string hs_all_assigned_business_unit_ids { get; set; }
        public string lastname { get; set; }
        public string lastmodifieddate { get; set; }
        private string _phone { get; set; }
        public string phone
        {
            get
            {
                return !String.IsNullOrEmpty( _phone ) ? _phone.Replace( " ", "" ).Replace( "(", "" ).Replace( ")", "" ).Replace( "-", "" ) : "";
            }
            set
            {
                _phone = value;
            }
        }
        public string rock_id { get; set; }
        public string which_best_describes_your_involvement_with_the_crossing_ { get; set; }
    }

    [DebuggerDisplay( "Id: {id}, Email: {properties.email}" )]
    public class HSContactResult
    {
        public string id { get; set; }
        public HSContactProperties properties { get; set; }
        public string archived { get; set; }
        public virtual int rock_id
        {
            get
            {
                int id = 0;
                if ( properties != null && properties.rock_id != null )
                {
                    Int32.TryParse( properties.rock_id, out id );
                }
                return id;
            }
        }
    }
    public class HSResultPaging
    {
        public HSResultPagingNext next { get; set; }
    }
    public class HSResultPagingNext
    {
        public string after { get; set; }
        public string link { get; set; }
    }
    public class HSContactQueryResult
    {
        public int total { get; set; }
        public List<HSContactResult> results { get; set; }
        public HSResultPaging paging { get; set; }
    }
    public class HSPropertyQueryResult
    {
        public List<HubspotProperty> results { get; set; }
    }
}
