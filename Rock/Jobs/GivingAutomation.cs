﻿// <copyright>
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Quartz;

using Rock.Attribute;
using Rock.Bus.Message;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Tasks;
using Rock.Utility.Enums;
using Rock.Utility.Settings.Giving;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job that serves three purposes:
    ///   1) Update Classification Attributes. This will be done no more than once a day and only on the days of week
    ///       configured in the automation settings.
    ///   2) Update Giving Journey Stage Attributes.    
    ///   3) Send Alerts - Sends alerts for gifts since the last run date and determines ‘Follow-up Alerts’ (alerts
    ///       triggered from gifts expected but not given) once a day.
    /// </summary>
    [DisplayName( "Giving Automation" )]
    [Description( "Job that updates giving classifications and journey stages, and send any giving alerts." )]
    [DisallowConcurrentExecution]

    [IntegerField( "Max Days Since Last Gift for Alerts",
        Description = "The maximum number of days since a giving group last gave where alerts can be made. If the last gift was earlier than this maximum, then alerts are not relevant.",
        DefaultIntegerValue = AttributeDefaultValue.MaxDaysSinceLastGift,
        Key = AttributeKey.MaxDaysSinceLastGift,
        Order = 1 )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (180).",
        IsRequired = false,
        DefaultIntegerValue = AttributeDefaultValue.CommandTimeout,
        Category = "General",
        Order = 7 )]

    public class GivingAutomation : IJob
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            public const string MaxDaysSinceLastGift = "MaxDaysSinceLastGift";
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Default Values for Attributes
        /// </summary>
        private static class AttributeDefaultValue
        {
            public const int MaxDaysSinceLastGift = 548;
            public const int CommandTimeout = 180;
        }

        /// <summary>
        /// The lower percentile for the giver bin
        /// </summary>
        private static class GiverBinLowerPercentile
        {
            public const decimal First = 0.95m;
            public const decimal Second = 0.80m;
            public const decimal Third = 0.60m;
        }

        #endregion Keys

        #region Constructors

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public GivingAutomation()
        {
        }

        #endregion Constructors

        #region Execute

        /// <summary>
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext jobContext )
        {
            var settings = GivingAutomationSettings.LoadGivingAutomationSettings();
            if ( !settings.GivingAutomationJobSettings.IsEnabled )
            {
                jobContext.UpdateLastStatusMessage( $"Giving Automation is not enabled." );
                return;
            }

            // Create a context object that will help transport state and helper information so as to not rely on the
            // job class itself being a single use instance
            var context = new GivingAutomationContext( jobContext );

            // First determine the ranges for each of the 4 giving bins by looking at all contribution transactions in the last 12 months.
            // These ranges will be updated in the Giving Automation system settings.
            UpdateGiverBinRanges( context );

            // Load the alert types once since they will be needed for each giving id
            HydrateAlertTypes( context );

            // we only want to update classifications on people that have had new transactions since they last time they were classified
            DateTime? lastClassificationRunDateTime = GetLastClassificationsRunDateTime();
            var classificationStartDateTime = RockDateTime.Now;

            var classificationsDaysToRun = settings.GivingClassificationSettings.RunDays ?? DayOfWeekFlag.All.AsDayOfWeekList().ToArray();
            context.IsGivingClassificationRunDay = classificationsDaysToRun.Contains( context.Now.DayOfWeek );

            // Get a list of all giving units (distinct giver ids) that have given since the last classification
            HydrateGivingIdsToClassify( context, lastClassificationRunDateTime );

            // For each giving id, classify and run analysis and create alerts
            var totalStopWatch = new Stopwatch();
            var elapsedTimesMs = new ConcurrentBag<long>();

            if ( _debugModeEnabled )
            {
                totalStopWatch.Start();
            }

            long progressCount = 0;
            long totalCount = context.GivingIdsToClassify.Count();

            jobContext.UpdateLastStatusMessage( $"Processing classifications and alerts for {totalCount} Giving Units..." );

            DateTime lastGivingClassificationProgressUpdate = DateTime.MinValue;

            Parallel.ForEach(
                context.GivingIdsToClassify,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                givingId =>
                {
                    var perStopWatch = new Stopwatch();

                    if ( _debugModeEnabled )
                    {
                        perStopWatch.Start();
                    }

                    ProcessGivingIdClassificationsAndAlerts( givingId, context );

                    if ( _debugModeEnabled )
                    {
                        perStopWatch.Stop();
                        elapsedTimesMs.Add( perStopWatch.ElapsedMilliseconds );
                        progressCount++;
                        if ( ( RockDateTime.Now - lastGivingClassificationProgressUpdate ).TotalSeconds >= 3 )
                        {
                            var ms = perStopWatch.ElapsedMilliseconds.ToString( "G" );

                            WriteToDebugOutput( $"Progress: {progressCount}/{totalCount}, progressCount/minute:{progressCount / totalStopWatch.Elapsed.TotalMinutes}" );
                            perStopWatch.Reset();
                            lastGivingClassificationProgressUpdate = RockDateTime.Now;
                        }
                    }
                } );

            if ( context.IsGivingClassificationRunDay )
            {
                // If GivingClassifications where run and we've identified the GivingIds and are done processing them, set lastClassificationRunDateTime to when we starting hydrating. 
                SaveLastClassificationsRunDateTime( classificationStartDateTime );
            }

            if ( _debugModeEnabled && elapsedTimesMs.Any() )
            {
                totalStopWatch.Stop();
                var ms = totalStopWatch.ElapsedMilliseconds.ToString( "G" );
                WriteToDebugOutput( $"Finished {elapsedTimesMs.Count} giving ids in average of {elapsedTimesMs.Average()}ms per giving unit and total {ms}ms" );
            }

            jobContext.UpdateLastStatusMessage( "Processing Alerts..." );

            // Create alerts for "late" gifts
            ProcessLateAlerts( context );

            // Process the Giving Journeys
            var givingJourneyHelper = new GivingJourneyHelper
            {
                SqlCommandTimeout = context.SqlCommandTimeoutSeconds
            };

            givingJourneyHelper.OnProgress += ( object sender, GivingJourneyHelper.ProgressEventArgs e ) =>
            {
                jobContext.UpdateLastStatusMessage( e.ProgressMessage );
            };

            var daysToUpdateGivingJourneys = GivingAutomationSettings.LoadGivingAutomationSettings().GivingJourneySettings.DaysToUpdateGivingJourneys;

            bool updateGivingJourneyStages = daysToUpdateGivingJourneys?.Contains( context.Now.DayOfWeek ) == true;

            string journeyStageStatusMessage;
            if ( updateGivingJourneyStages )
            {
                givingJourneyHelper.UpdateGivingJourneyStages();
                if ( givingJourneyHelper.UpdatedJourneyStageCount == 1 )
                {
                    journeyStageStatusMessage = $"Updated {givingJourneyHelper.UpdatedJourneyStageCount } journey stage.";
                }
                else
                {
                    journeyStageStatusMessage = $"Updated {givingJourneyHelper.UpdatedJourneyStageCount } journey stages.";
                }
            }
            else
            {
                journeyStageStatusMessage = "Journey Stage updates not configured to run today.";
            }

            string classificationStatusMessage;
            if ( context.IsGivingClassificationRunDay )
            {
                classificationStatusMessage = $@"
Classified {context.GivingIdsSuccessful} giving {"group".PluralizeIf( context.GivingIdsSuccessful != 1 )}.
There were {context.GivingIdsFailed} {"failure".PluralizeIf( context.GivingIdsFailed != 1 )}.".Trim();
            }
            else
            {
                classificationStatusMessage = "Classification updates not configured to run today.";
            }

            // Format the result message
            jobContext.Result = $@"
{classificationStatusMessage}
Created {context.AlertsCreated} {"alert".PluralizeIf( context.AlertsCreated != 1 )}.
{journeyStageStatusMessage}".Trim();

            if ( context.Errors.Any() )
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine( "Errors: " );

                foreach ( var error in context.Errors )
                {
                    sb.AppendLine( error );
                }

                var errorMessage = sb.ToString();
                jobContext.Result += errorMessage;
            }
        }

        #endregion Execute

        #region Settings and Attribute Helpers

        /// <summary>
        /// Creates the person identifier query for DataView.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="alertType">Type of the alert.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static IQueryable<int> CreatePersonIdQueryForDataView( RockContext rockContext, FinancialTransactionAlertType alertType, GivingAutomationContext context )
        {
            if ( alertType.DataViewId is null )
            {
                return null;
            }

            var dataViewService = new DataViewService( rockContext );
            var dataview = dataViewService.Get( alertType.DataViewId.Value );

            if ( dataview == null )
            {
                context.Errors.Add( $"The dataview {alertType.DataViewId} for giving alert type {alertType.Id} did not resolve" );
                return null;
            }

            // We can use the dataview to get the person query
            var dataViewGetQueryArgs = new DataViewGetQueryArgs
            {
                DbContext = rockContext
            };

            IQueryable<IEntity> dataviewQuery;
            try
            {
                dataviewQuery = dataview.GetQuery( dataViewGetQueryArgs );
            }
            catch ( Exception ex )
            {
                context.Errors.Add( ex.Message );
                ExceptionLogService.LogException( ex );
                return null;
            }

            if ( dataviewQuery == null )
            {
                context.Errors.Add( $"Generating a query for dataview {alertType.DataViewId} for giving alert type {alertType.Id} was not successful" );
                return null;
            }

            // This query contains person ids in the dataview
            var personIdQuery = dataviewQuery.AsNoTracking().Select( e => e.Id );
            return personIdQuery;
        }

        /// <summary>
        /// Gets the attribute key.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="guidString">The unique identifier string.</param>
        /// <returns></returns>
        private static string GetAttributeKey( GivingAutomationContext context, string guidString )
        {
            var key = AttributeCache.Get( guidString )?.Key;

            if ( key.IsNullOrWhiteSpace() )
            {
                context.Errors.Add( $"An attribute was expected using the guid '{guidString}', but failed to resolve" );
            }

            return key;
        }

        /// <summary>
        /// Gets the giving unit attribute value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="people">The people.</param>
        /// <param name="guidString">The guid string.</param>
        /// <returns></returns>
        private static string GetGivingUnitAttributeValue( GivingAutomationContext context, List<Person> people, string guidString )
        {
            if ( !people.Any() )
            {
                return string.Empty;
            }

            var key = GetAttributeKey( context, guidString );

            if ( key.IsNullOrWhiteSpace() )
            {
                // GetAttributeKey logs an error in the context
                return string.Empty;
            }

            var unitValue = people.First().GetAttributeValue( key );

            for ( var i = 1; i < people.Count; i++ )
            {
                var person = people[i];
                var personValue = person.GetAttributeValue( key );

                if ( unitValue != personValue )
                {
                    // The people in this giving unit have different values for this. We don't know which is actually correct, so assume no value.
                    return string.Empty;
                }
            }

            return unitValue;
        }

        /// <summary>
        /// Sets the giving unit attribute value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="people">The people.</param>
        /// <param name="guidString">The unique identifier string.</param>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void SetGivingUnitAttributeValue( GivingAutomationContext context, List<Person> people, string guidString, double? value, RockContext rockContext = null )
        {
            SetGivingUnitAttributeValue( context, people, guidString, value.ToStringSafe(), rockContext );
        }

        /// <summary>
        /// Sets the giving unit attribute value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="people">The people.</param>
        /// <param name="guidString">The unique identifier string.</param>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void SetGivingUnitAttributeValue( GivingAutomationContext context, List<Person> people, string guidString, decimal? value, RockContext rockContext = null )
        {
            SetGivingUnitAttributeValue( context, people, guidString, value.ToStringSafe(), rockContext );
        }

        /// <summary>
        /// Sets the giving unit attribute value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="people">The people.</param>
        /// <param name="guidString">The unique identifier string.</param>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void SetGivingUnitAttributeValue( GivingAutomationContext context, List<Person> people, string guidString, int? value, RockContext rockContext = null )
        {
            SetGivingUnitAttributeValue( context, people, guidString, value.ToStringSafe(), rockContext );
        }

        /// <summary>
        /// Sets the giving unit attribute value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="people">The people.</param>
        /// <param name="guidString">The unique identifier string.</param>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void SetGivingUnitAttributeValue( GivingAutomationContext context, List<Person> people, string guidString, DateTime? value, RockContext rockContext = null )
        {
            SetGivingUnitAttributeValue( context, people, guidString, value.ToISO8601DateString(), rockContext );
        }

        /// <summary>
        /// Sets the giving unit attribute value.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="people">The people.</param>
        /// <param name="guidString">The unique identifier string.</param>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void SetGivingUnitAttributeValue( GivingAutomationContext context, List<Person> people, string guidString, string value, RockContext rockContext = null )
        {
            var key = GetAttributeKey( context, guidString );

            if ( key.IsNullOrWhiteSpace() )
            {
                // GetAttributeKey logs an error in the context
                return;
            }

            foreach ( var person in people )
            {
                person.SetAttributeValue( key, value );
            }
        }

        /// <summary>
        /// Gets the percent int. Ex: 50 and 200 => 25. This is safeguarded from 0 as a
        /// denominator by returning 0 in that case.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        /// <returns></returns>
        private static int GetPercentInt( int numerator, int denominator )
        {
            if ( denominator == 0 )
            {
                return 0;
            }

            var asDecimal = decimal.Divide( numerator, denominator );
            return GetPercentInt( asDecimal );
        }

        /// <summary>
        /// Gets the percent int. Ex: .976 => 98
        /// </summary>
        /// <param name="percentDecimal">The percent decimal.</param>
        /// <returns></returns>
        private static int GetPercentInt( decimal percentDecimal )
        {
            return ( int ) decimal.Round( percentDecimal * 100 );
        }

        /// <summary>
        /// Gets the global repeat prevention days.
        /// </summary>
        /// <value>
        /// The global repeat prevention days.
        /// </value>
        private static int? GlobalRepeatPreventionDays
        {
            get
            {
                var settings = GivingAutomationSettings.LoadGivingAutomationSettings();
                return settings.GivingAlertingSettings.GlobalRepeatPreventionDurationDays;
            }
        }

        /// <summary>
        /// Gets the gratitude repeat prevention days.
        /// </summary>
        /// <value>
        /// The global repeat prevention days.
        /// </value>
        private static int? GratitudeRepeatPreventionDays
        {
            get
            {
                var settings = GivingAutomationSettings.LoadGivingAutomationSettings();
                return settings?.GivingAlertingSettings?.GratitudeRepeatPreventionDurationDays;
            }
        }

        /// <summary>
        /// Gets the follow-up repeat prevention days.
        /// </summary>
        /// <value>
        /// The global repeat prevention days.
        /// </value>
        private static int? FollowUpRepeatPreventionDays
        {
            get
            {
                var settings = GivingAutomationSettings.LoadGivingAutomationSettings();
                return settings?.GivingAlertingSettings?.FollowupRepeatPreventionDurationDays;
            }
        }

        /// <summary>
        /// Gets the last run date time.
        /// </summary>
        /// <returns></returns>
        private static DateTime? GetLastClassificationsRunDateTime()
        {
            var settings = GivingAutomationSettings.LoadGivingAutomationSettings();
            return settings.GivingClassificationSettings.LastRunDateTime;
        }

        private static void SaveLastClassificationsRunDateTime( DateTime lastRunDateTime )
        {
            var settings = GivingAutomationSettings.LoadGivingAutomationSettings();
            settings.GivingClassificationSettings.LastRunDateTime = lastRunDateTime;
            GivingAutomationSettings.SaveGivingAutomationSettings( settings );
        }

        /// <summary>
        /// Gets the last run date time.
        /// </summary>
        /// <returns></returns>
        private static decimal? GetGivingBinLowerLimit( int binIndex )
        {
            var classificationSettings = GivingAutomationSettings.LoadGivingAutomationSettings().GivingClassificationSettings;
            var giverBin = classificationSettings.GiverBins?.Count > binIndex
                    ? classificationSettings.GiverBins[binIndex]
                    : null;

            return giverBin?.LowerLimit;
        }

        /// <summary>
        /// Gets the last run date time.
        /// </summary>
        /// <returns></returns>
        private static void SetGivingBinLowerLimit( int binIndex, decimal? lowerLimit )
        {
            var settings = GivingAutomationSettings.LoadGivingAutomationSettings();

            while ( settings.GivingClassificationSettings.GiverBins.Count <= binIndex )
            {
                settings.GivingClassificationSettings.GiverBins.Add( new GiverBin() );
            }

            var giverBin = settings.GivingClassificationSettings.GiverBins[binIndex];
            giverBin.LowerLimit = lowerLimit;
            GivingAutomationSettings.SaveGivingAutomationSettings( settings );
        }

        /// <summary>
        /// Gets the earliest last gift date time.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static DateTime GetEarliestLastGiftDateTime( GivingAutomationContext context )
        {
            var days = context.GetAttributeValue( AttributeKey.MaxDaysSinceLastGift ).AsIntegerOrNull() ??
                AttributeDefaultValue.MaxDaysSinceLastGift;
            return context.Now.AddDays( 0 - days );
        }

        /// <summary>
        /// Splits Interquartile Ranges.
        /// Ex: 1,2,3,4,5,6 => (1,2), (3,4), (5,6)
        /// </summary>
        /// <param name="orderedValues"></param>
        /// <returns></returns>
        internal static Tuple<List<decimal>, List<decimal>, List<decimal>> SplitQuartileRanges( List<decimal> orderedValues )
        {
            var count = orderedValues.Count;

            if ( count <= 2 )
            {
                return Tuple.Create( new List<decimal>(), orderedValues, new List<decimal>() );
            }

            var lastMidIndex = count / 2;
            var isSingleMidIndex = count % 2 != 0;
            var firstMidIndex = isSingleMidIndex ? lastMidIndex : lastMidIndex - 1;

            var medianValues = isSingleMidIndex ?
                orderedValues.GetRange( firstMidIndex, 1 ) :
                orderedValues.GetRange( firstMidIndex, 2 );

            var q1 = orderedValues.GetRange( 0, firstMidIndex );
            var q3 = orderedValues.GetRange( lastMidIndex + 1, count - lastMidIndex - 1 );

            return Tuple.Create( q1, medianValues, q3 );
        }

        /// <summary>
        /// Gets the median range.
        /// Ex: 1,2,3,4,5,6 => 3,4
        /// </summary>
        /// <param name="orderedValues">The ordered values.</param>
        /// <returns></returns>
        private static List<decimal> GetMedianRange( List<decimal> orderedValues )
        {
            var ranges = SplitQuartileRanges( orderedValues );
            return ranges.Item2;
        }

        /// <summary>
        /// Gets the median.
        /// Ex: 1,2,3,4,5,6 => 3.5
        /// </summary>
        /// <param name="orderedValues">The ordered values.</param>
        /// <returns></returns>
        private static decimal GetMedian( List<decimal> orderedValues )
        {
            if ( orderedValues.Count == 0 )
            {
                return 0;
            }

            var medianRange = GetMedianRange( orderedValues );
            return medianRange.Average();
        }

        /// <summary>
        /// Gets the amount IQR count.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="people">The people.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        internal static decimal GetAmountIqrCount( GivingAutomationContext context, List<Person> people, decimal amount )
        {
            // For the purpose of having a high number that is reasonable and also does not overflow any c# type, I am choosing
            // a constant to represent infinity other than max value of any particular type
            var infinity = 1000;
            var negativeInfinity = 0 - infinity;

            // Check the number of IQRs that the amount varies
            var medianGiftAmount = GetGivingUnitAttributeValue( context, people, Rock.SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN ).AsDecimal();
            var amountIqr = GetGivingUnitAttributeValue( context, people, Rock.SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR ).AsDecimal();
            var amountDeviation = amount - medianGiftAmount;
            decimal numberOfAmountIqrs;

            if ( amountDeviation == 0 )
            {
                numberOfAmountIqrs = 0;
            }
            else if ( amountIqr != 0 )
            {
                numberOfAmountIqrs = amountDeviation / amountIqr;
            }
            else
            {
                // If the amount IQR is 0, then this giving group gives the same amount every time and even a $1 increase would be an infinite
                // number of IQRs since the formula is dividing by zero. Since we don't want alerts for scenarios like an increase of $1, we use
                // a fallback formula for IQR.
                // Use 15% of the median amount or $100 if the median amount is somehow $0.
                amountIqr = 0.15m * medianGiftAmount;

                if ( amountIqr == 0 )
                {
                    amountIqr = 100m;
                }

                numberOfAmountIqrs = amountDeviation / amountIqr;
            }

            // Make sure the calculation doesn't exceed "infinity"
            if ( numberOfAmountIqrs > infinity )
            {
                numberOfAmountIqrs = infinity;
            }
            else if ( numberOfAmountIqrs < negativeInfinity )
            {
                numberOfAmountIqrs = negativeInfinity;
            }

            return numberOfAmountIqrs;
        }

        /// <summary>
        /// Gets the frequency deviation count.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="people">The people.</param>
        /// <param name="daysSinceLastTransaction">The days since last transaction.</param>
        /// <returns></returns>
        internal static decimal GetFrequencyDeviationCount( GivingAutomationContext context, List<Person> people, double daysSinceLastTransaction )
        {
            // For the purpose of having a high number that is reasonable and also does not overflow any c# type, I am choosing
            // a constant to represent infinity other than max value of any particular type
            const int FrequencyStdDevsInfinity = 1000;
            const int FrequencyStdDevsNegativeInfinity = 0 - FrequencyStdDevsInfinity;

            var frequencyStdDev = GetGivingUnitAttributeValue( context, people, Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS ).AsDecimal();
            var frequencyMean = GetGivingUnitAttributeValue( context, people, Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS ).AsDecimal();
            var frequencyDeviation = frequencyMean - Convert.ToDecimal( daysSinceLastTransaction );
            decimal numberOfFrequencyStdDevs;

            if ( frequencyDeviation == 0 )
            {
                numberOfFrequencyStdDevs = 0;
            }
            else if ( frequencyStdDev >= 1 )
            {
                numberOfFrequencyStdDevs = frequencyDeviation / frequencyStdDev;
            }
            else
            {
                // If the frequency std dev is less than 1, then this giving group gives the same interval and even a 1.1 day change would be a large
                // number of std devs since the formula is dividing by zero. Since we don't want alerts for scenarios like being 1 day early, we use
                // a fallback formula for std dev.
                // Use 15% of the mean or 3 days if the mean is still 0.
                frequencyStdDev = 0.15m * frequencyMean;

                if ( frequencyStdDev < 1 )
                {
                    frequencyStdDev = 3m;
                }

                numberOfFrequencyStdDevs = frequencyDeviation / frequencyStdDev;
            }

            // Make sure the calculation doesn't exceed "infinity"
            if ( numberOfFrequencyStdDevs > FrequencyStdDevsInfinity )
            {
                numberOfFrequencyStdDevs = FrequencyStdDevsInfinity;
            }
            else if ( numberOfFrequencyStdDevs < FrequencyStdDevsNegativeInfinity )
            {
                numberOfFrequencyStdDevs = FrequencyStdDevsNegativeInfinity;
            }

            return numberOfFrequencyStdDevs;
        }

        /// <summary>
        /// Are follow-up alerts allowed given the recent alerts.
        /// </summary>
        /// <param name="recentAlerts">The recent alerts.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static bool AllowFollowUpAlerts( List<AlertView> recentAlerts, GivingAutomationContext context )
        {
            if ( !recentAlerts.Any() )
            {
                return true;
            }

            if ( GlobalRepeatPreventionDays.HasValue )
            {
                var lastAlertDate = recentAlerts.Last().AlertDateTime;
                var daysSinceLastAlert = ( context.Now - lastAlertDate ).TotalDays;

                if ( daysSinceLastAlert <= GlobalRepeatPreventionDays.Value )
                {
                    // This group has alerts within the global repeat duration. Don't create any new alerts.
                    return false;
                }
            }

            var lastFollowUpAlertDate = recentAlerts.LastOrDefault( a => a.AlertType == AlertType.FollowUp )?.AlertDateTime;

            if ( FollowUpRepeatPreventionDays.HasValue && lastFollowUpAlertDate.HasValue )
            {
                var daysSinceLastFollowUpAlert = ( context.Now - lastFollowUpAlertDate.Value ).TotalDays;

                if ( daysSinceLastFollowUpAlert <= FollowUpRepeatPreventionDays.Value )
                {
                    // This group has follow-up alerts within the repeat duration. Don't create any new follow-up alerts.
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Are gratitude alerts allowed given the recent alerts.
        /// </summary>
        /// <param name="recentAlerts">The recent alerts.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static bool AllowGratitudeAlerts( List<AlertView> recentAlerts, GivingAutomationContext context )
        {
            if ( !recentAlerts.Any() )
            {
                return true;
            }

            if ( GlobalRepeatPreventionDays.HasValue )
            {
                var lastAlertDate = recentAlerts.Last().AlertDateTime;
                var daysSinceLastAlert = ( context.Now - lastAlertDate ).TotalDays;

                if ( daysSinceLastAlert <= GlobalRepeatPreventionDays.Value )
                {
                    // This group has alerts within the global repeat duration. Don't create any new alerts.
                    return false;
                }
            }

            var lastGratitudeAlertDate = recentAlerts.LastOrDefault( a => a.AlertType == AlertType.Gratitude )?.AlertDateTime;

            if ( GratitudeRepeatPreventionDays.HasValue && lastGratitudeAlertDate.HasValue )
            {
                var daysSinceLastGratitudeAlert = ( context.Now - lastGratitudeAlertDate.Value ).TotalDays;

                if ( daysSinceLastGratitudeAlert <= GratitudeRepeatPreventionDays.Value )
                {
                    // This group has gratitude alerts within the repeat duration. Don't create any new gratitude alerts.
                    return false;
                }
            }

            return true;
        }

        #endregion Settings and Attribute Helpers

        #region Execute Logic

        /// <summary>
        /// Hydrates the alert types that should be considered today (according to the alert type RunDays).
        /// </summary>
        /// <param name="context">The context.</param>
        private static void HydrateAlertTypes( GivingAutomationContext context )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = context.SqlCommandTimeoutSeconds;

                // Get the alert types
                var alertTypeService = new FinancialTransactionAlertTypeService( rockContext );
                var alertTypes = alertTypeService.Queryable()
                    .AsNoTracking()
                    .OrderBy( at => at.Order )
                    .ToList();

                // Filter out alert types that are not supposed to run today
                var currentDayOfWeekFlag = context.Now.DayOfWeek.AsFlag();

                alertTypes = alertTypes
                    .Where( at =>
                        !at.RunDays.HasValue ||
                        ( at.RunDays.Value & currentDayOfWeekFlag ) == currentDayOfWeekFlag )
                    .ToList();

                context.AlertTypes = alertTypes;
            }
        }

        /// <summary>
        /// Hydrates the giving ids to classify.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="lastClassificationRunDateTime">The last classification run date time.</param>
        private static void HydrateGivingIdsToClassify( GivingAutomationContext context, DateTime? lastClassificationRunDateTime )
        {
            // Classification attributes need to be written for all adults with the same giver id in Rock. So Ted &
            // Cindy should have the same attribute values if they are set to contribute as a family even if Cindy
            // is always the one giving the gift.

            // We will reclassify anyone who has given since the last run of this job. This covers all alerts except
            // the "late gift" alert, which needs to find people based on the absence of a gift.
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = context.SqlCommandTimeoutSeconds;
                var financialTransactionService = new FinancialTransactionService( rockContext );

                // This is the people that have given since the last run date or the configured old gift date point.
                // Just in case transactions were modified since the last run date, also include givingids that have
                // transaction records that have been modified since the last run date.
                var minTransactionDate = lastClassificationRunDateTime ?? GetEarliestLastGiftDateTime( context );
                var givingIds = financialTransactionService.GetGivingAutomationSourceTransactionQuery()
                    .Where( t =>
                        ( t.TransactionDateTime >= minTransactionDate )
                        || ( t.ModifiedDateTime.HasValue && t.ModifiedDateTime.Value >= minTransactionDate ) )
                    .Select( t => t.AuthorizedPersonAlias.Person.GivingId )
                    .Distinct()
                    .ToList();

                // This transforms the set of people to classify into distinct giving ids.
                context.GivingIdsToClassify = givingIds.OrderBy( a => a ).ToList();
            }
        }

        /// <summary>
        /// Updates the giver bins ranges.
        /// </summary>
        /// <param name="context">The context.</param>
        private static void UpdateGiverBinRanges( GivingAutomationContext context )
        {
            // First determine the ranges for each of the 4 giving bins by looking at all contribution transactions in the last 12 months.
            // These ranges will be updated in the Giving Automation system settings.
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = context.SqlCommandTimeoutSeconds;
                var minDate = context.Now.AddMonths( -12 );

                var financialTransactionService = new FinancialTransactionService( rockContext );
                var givingGroups = financialTransactionService.GetGivingAutomationSourceTransactionQuery()
                    .Where( t =>
                        t.TransactionDateTime.HasValue &&
                        t.TransactionDateTime > minDate &&
                        t.AuthorizedPersonAliasId.HasValue &&
                        t.AuthorizedPersonAlias.Person.GivingId != null &&
                        t.AuthorizedPersonAlias.Person.GivingId.Length > 0 )
                    .GroupBy( t => t.AuthorizedPersonAlias.Person.GivingId )
                    .Select( g => new
                    {
                        GivingId = g.Key,
                        Last12MonthsTotalGift = g.Sum( t => t.TransactionDetails.Sum( d => d.Amount ) )
                    } )
                    .ToList();

                givingGroups = givingGroups.OrderBy( g => g.Last12MonthsTotalGift ).ToList();
                var givingGroupCount = givingGroups.Count;

                // Calculate the current giving percentile lower amounts. This means the lower range of the 48th percentile will
                // be at index 48 of the array.
                context.PercentileLowerRange = new List<decimal>();

                for ( var i = 0; i < 100; i++ )
                {
                    var percentDecimal = decimal.Divide( i, 100 );
                    var firstIndex = ( int ) decimal.Round( givingGroupCount * percentDecimal );

                    if ( firstIndex >= givingGroupCount )
                    {
                        firstIndex = givingGroupCount - 1;
                    }

                    if ( firstIndex >= 0 && firstIndex < givingGroups.Count )
                    {
                        context.PercentileLowerRange.Add( givingGroups[firstIndex].Last12MonthsTotalGift );
                    }
                    else
                    {
                        context.PercentileLowerRange.Add( 0m );
                    }
                }

                // These should be static, but just in case the count changes for some reason
                var percentileCount = context.PercentileLowerRange.Count;
                var firstBinStartIndex = ( int ) decimal.Round( percentileCount * GiverBinLowerPercentile.First );
                var secondBinStartIndex = ( int ) decimal.Round( percentileCount * GiverBinLowerPercentile.Second );
                var thirdBinStartIndex = ( int ) decimal.Round( percentileCount * GiverBinLowerPercentile.Third );

                SetGivingBinLowerLimit( 0, context.PercentileLowerRange[firstBinStartIndex] );
                SetGivingBinLowerLimit( 1, context.PercentileLowerRange[secondBinStartIndex] );
                SetGivingBinLowerLimit( 2, context.PercentileLowerRange[thirdBinStartIndex] );
                SetGivingBinLowerLimit( 3, context.PercentileLowerRange[0] );
            }
        }

        /// <summary>
        /// Processes the giving identifier.
        /// </summary>
        /// <param name="givingId">The giving identifier.</param>
        /// <param name="context">The context.</param>
        private static void ProcessGivingIdClassificationsAndAlerts( string givingId, GivingAutomationContext context )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = context.SqlCommandTimeoutSeconds;

                // Load the people that are in this giving group so their attribute values can be set
                var personService = new PersonService( rockContext );
                var people = personService.Queryable()
                    .Include( p => p.Aliases )
                    .Where( p => p.GivingId == givingId )
                    .ToList();

                var personAliasIds = people.SelectMany( p => p.Aliases.Select( a => a.Id ) ).ToList();
                people.LoadAttributes( rockContext );

                // Get the gifts from the past 12 months for the giving group
                var financialTransactionService = new FinancialTransactionService( rockContext );

                // Classifications for: % Scheduled, Gives As ___, Preferred Source, Preferred Currency will be based
                // off of all giving in the last 12 months.In the case of a tie in values( e.g. 50% credit card, 50%
                // cash ) use the most recent value as the tie breaker. This could be calculated with only one gift.
                var oneYearAgo = context.Now.AddMonths( -12 );
                var twelveMonthsTransactionsQry = financialTransactionService.GetGivingAutomationSourceTransactionQuery()
                    .Where( t =>
                        t.AuthorizedPersonAliasId.HasValue &&
                        t.TransactionDateTime >= oneYearAgo );

                if ( personAliasIds.Count == 1 )
                {
                    var personAliasId = personAliasIds[0];
                    twelveMonthsTransactionsQry = twelveMonthsTransactionsQry.Where( t => t.AuthorizedPersonAliasId == personAliasId );
                }
                else
                {
                    twelveMonthsTransactionsQry = twelveMonthsTransactionsQry.Where( t => personAliasIds.Contains( t.AuthorizedPersonAliasId.Value ) );
                }

                var twelveMonthsTransactions = twelveMonthsTransactionsQry
                    .Select( t => new TransactionView
                    {
                        Id = t.Id,
                        AuthorizedPersonAliasId = t.AuthorizedPersonAliasId.Value,
                        TransactionDateTime = t.TransactionDateTime.Value,
                        TotalAmount = t.TransactionDetails.Sum( d => d.Amount ),
                        CurrencyTypeValueId = t.FinancialPaymentDetail.CurrencyTypeValueId,
                        SourceTypeValueId = t.SourceTypeValueId,
                        IsScheduled = t.ScheduledTransactionId.HasValue
                    } )
                    .ToList()
                    .OrderBy( t => t.TransactionDateTime )
                    .ToList();

                // If there are no transactions, then there is no basis to classify or alert
                if ( !twelveMonthsTransactions.Any() )
                {
                    return;
                }

                // Load the defined value guids into the transaction views from cache rather than using a table join in the SQL above
                foreach ( var transaction in twelveMonthsTransactions )
                {
                    if ( transaction.CurrencyTypeValueId.HasValue )
                    {
                        transaction.CurrencyTypeValueGuid = DefinedValueCache.Get( transaction.CurrencyTypeValueId.Value )?.Guid;
                    }

                    if ( transaction.SourceTypeValueId.HasValue )
                    {
                        transaction.SourceTypeValueGuid = DefinedValueCache.Get( transaction.SourceTypeValueId.Value )?.Guid;
                    }
                }

                // If the group doesn't have FirstGiftDate attribute, set it by querying for the value
                var firstGiftDate = GetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_ERA_FIRST_GAVE ).AsDateTime();
                var updatedFirstGiftDate = false;

                if ( !firstGiftDate.HasValue )
                {
                    var firstGiftDateQry = financialTransactionService.GetGivingAutomationSourceTransactionQuery()
                        .Where( t =>
                            t.AuthorizedPersonAliasId.HasValue &&
                            t.TransactionDateTime.HasValue );

                    if ( personAliasIds.Count == 1 )
                    {
                        var personAliasId = personAliasIds[0];
                        firstGiftDateQry = firstGiftDateQry.Where( t => t.AuthorizedPersonAliasId == personAliasId );
                    }
                    else
                    {
                        firstGiftDateQry = firstGiftDateQry.Where( t => personAliasIds.Contains( t.AuthorizedPersonAliasId.Value ) );
                    }

                    firstGiftDate = firstGiftDateQry.Min( t => t.TransactionDateTime );

                    if ( firstGiftDate.HasValue )
                    {
                        SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_ERA_FIRST_GAVE, firstGiftDate );
                        updatedFirstGiftDate = true;
                    }
                }

                // If the first gift date was less than 12 months ago and there are less than 5 gifts, do not process this giving group
                // because there is not enough data to develop any meaningful insights
                if ( ( !firstGiftDate.HasValue || firstGiftDate.Value > oneYearAgo ) && twelveMonthsTransactions.Count < 5 )
                {
                    return;
                }

                // Only run classifications on the days specified by the settings
                if ( context.IsGivingClassificationRunDay )
                {
                    // We need to know if this giving group has other transactions. If they do then we do not need to
                    // extrapolate because we have the complete 12 month data picture.
                    var mostRecentOldTransactionDateQuery = financialTransactionService.GetGivingAutomationSourceTransactionQuery()
                        .Where( t =>
                            t.AuthorizedPersonAliasId.HasValue &&
                            t.TransactionDateTime < oneYearAgo );

                    if ( personAliasIds.Count == 1 )
                    {
                        var personAliasId = personAliasIds[0];
                        mostRecentOldTransactionDateQuery = mostRecentOldTransactionDateQuery.Where( t => t.AuthorizedPersonAliasId == personAliasId );
                    }
                    else
                    {
                        mostRecentOldTransactionDateQuery = mostRecentOldTransactionDateQuery.Where( t => personAliasIds.Contains( t.AuthorizedPersonAliasId.Value ) );
                    }

                    var mostRecentOldTransactionDate = mostRecentOldTransactionDateQuery.Select( t => t.TransactionDateTime ).Max();

                    // Update the attributes using the logic function
                    var classificationSuccess = UpdateGivingUnitClassifications( givingId, people, twelveMonthsTransactions, mostRecentOldTransactionDate, context, oneYearAgo );

                    if ( !classificationSuccess )
                    {
                        context.GivingIdsFailed++;

                        // Do not continue with alerts if classification failed
                        return;
                    }

                    // Update the giving history attribute. This is done outside of the UpdateGivingUnitClassifications method because it
                    // requires a unique query
                    List<MonthlyAccountGivingHistory> monthlyAccountGivingHistoryList = financialTransactionService.GetGivingAutomationMonthlyAccountGivingHistory( givingId );
                    var givingHistoryJson = monthlyAccountGivingHistoryList.ToJson();
                    SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_HISTORY_JSON, givingHistoryJson );

                    // Save all the attribute value changes
                    people.ForEach( p => p.SaveAttributeValues( rockContext ) );
                    rockContext.SaveChanges();
                    context.GivingIdsSuccessful++;

                    // Fire the bus event to notify that these people have been classified
                    GivingUnitWasClassifiedMessage.Publish( people.Select( p => p.Id ) );
                }
                else if ( updatedFirstGiftDate )
                {
                    // Save attribute value change for first gift date
                    // First gift date isn't technically part of classification since this attribute predates giving automation
                    people.ForEach( p => p.SaveAttributeValues( rockContext ) );
                    rockContext.SaveChanges();
                }

                // Next we will generate alerts if there are any to process to today. The HydrateAlerts method already executed
                // and provided with alerts types that are scheduled to be run today
                if ( !context.AlertTypes.Any() )
                {
                    return;
                }

                // If this giving group has not been classified in the last week, then don't generate any alerts
                // because those alerts will be based on outdated statistics. One week was chosen since classifications
                // can occur at a minimum of 1 day per week since the control is a day of the week picker without being
                // all the way turned off.
                var lastClassifiedDate = GetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE ).AsDateTime() ?? DateTime.MinValue;
                var oneWeekAgo = context.Now.AddDays( -7 );
                var hasBeenClassifiedRecently = lastClassifiedDate >= oneWeekAgo;

                if ( !hasBeenClassifiedRecently )
                {
                    return;
                }

                // Alerts can be generated for transactions given in the last week. One week was chosen since alert types
                // can run at a minimum of 1 day per week since the control is a day of the week picker without being
                // all the way turned off.
                var transactionsToCheckAlerts = twelveMonthsTransactions.Where( t => t.TransactionDateTime >= oneWeekAgo ).ToList();

                if ( !transactionsToCheckAlerts.Any() )
                {
                    return;
                }

                // Get any recent alerts for these people. These will be used in the "repeat alert prevention" logic
                var financialTransactionAlertService = new FinancialTransactionAlertService( rockContext );
                var twelveMonthsAlertsQry = financialTransactionAlertService.Queryable()
                    .AsNoTracking()
                    .Where( a =>
                        a.AlertDateTime > oneYearAgo );

                if ( personAliasIds.Count == 1 )
                {
                    var personAliasId = personAliasIds[0];
                    twelveMonthsAlertsQry = twelveMonthsAlertsQry.Where( t => t.PersonAliasId == personAliasId );
                }
                else
                {
                    twelveMonthsAlertsQry = twelveMonthsAlertsQry.Where( t => personAliasIds.Contains( t.PersonAliasId ) );
                }

                var twelveMonthsAlerts = twelveMonthsAlertsQry.Select( a => new AlertView
                {
                    AlertDateTime = a.AlertDateTime,
                    AlertTypeId = a.AlertTypeId,
                    AlertType = a.FinancialTransactionAlertType.AlertType,
                    TransactionId = a.TransactionId
                } )
                    .OrderBy( a => a.AlertDateTime )
                    .ToList();

                // Check the repeat prevention durations. These prevent multiple alert types from being generated
                // for these people in a short time period (if configured)
                var allowFollowUp = AllowFollowUpAlerts( twelveMonthsAlerts, context );
                var allowGratitude = AllowGratitudeAlerts( twelveMonthsAlerts, context );
                var alertsToAddToDb = new List<FinancialTransactionAlert>();

                if ( allowFollowUp || allowGratitude )
                {
                    foreach ( var transaction in transactionsToCheckAlerts )
                    {
                        var lastTransactionDate = twelveMonthsTransactions
                            .LastOrDefault( t => t.TransactionDateTime < transaction.TransactionDateTime )
                            ?.TransactionDateTime;

                        var alertsForTransaction = lastTransactionDate.HasValue ?
                            CreateAlertsForTransaction(
                                rockContext,
                                people,
                                twelveMonthsAlerts,
                                transaction,
                                lastTransactionDate.Value,
                                context,
                                allowGratitude,
                                allowFollowUp ) :
                            new List<FinancialTransactionAlert>();

                        alertsToAddToDb.AddRange( alertsForTransaction );

                        if ( alertsForTransaction.Any() )
                        {
                            twelveMonthsAlerts.AddRange( alertsForTransaction.Select( a => new AlertView
                            {
                                AlertDateTime = a.AlertDateTime,
                                AlertType = context.AlertTypes.First( at => at.Id == a.AlertTypeId ).AlertType,
                                AlertTypeId = a.AlertTypeId,
                                TransactionId = a.TransactionId
                            } ) );

                            // Recheck if more alerts can be created now that there are more alerts created
                            allowFollowUp = AllowFollowUpAlerts( twelveMonthsAlerts, context );
                            allowGratitude = AllowGratitudeAlerts( twelveMonthsAlerts, context );

                            if ( !allowFollowUp && !allowGratitude )
                            {
                                // Break out of the for loop since no more alerts can be generated
                                break;
                            }
                        }
                    }
                }

                // Add any alerts to the database
                if ( alertsToAddToDb.Any() )
                {
                    var service = new FinancialTransactionAlertService( rockContext );
                    service.AddRange( alertsToAddToDb );
                }

                rockContext.SaveChanges();
                context.AlertsCreated += alertsToAddToDb.Count;
                HandlePostAlertsAddedLogic( alertsToAddToDb );
            }
        }

        /// <summary>
        /// Creates the alerts for transaction.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="people">The people.</param>
        /// <param name="recentAlerts">The recent alerts.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="lastGiftDate">The last gift date.</param>
        /// <param name="context">The context.</param>
        /// <param name="allowGratitude">if set to <c>true</c> [allow gratitude].</param>
        /// <param name="allowFollowUp">if set to <c>true</c> [allow follow up].</param>
        internal static List<FinancialTransactionAlert> CreateAlertsForTransaction(
            RockContext rockContext,
            List<Person> people,
            List<AlertView> recentAlerts,
            TransactionView transaction,
            DateTime lastGiftDate,
            GivingAutomationContext context,
            bool allowGratitude,
            bool allowFollowUp )
        {
            var alerts = new List<FinancialTransactionAlert>();

            if ( !people.Any() )
            {
                return alerts;
            }

            var daysSinceLastTransaction = ( transaction.TransactionDateTime - lastGiftDate ).TotalDays;

            // The people all have the same attribute values, so this method will use the first person
            var person = people.First();

            // Go through the ordered alert types (ordered in the hydrate method)
            foreach ( var alertType in context.AlertTypes )
            {
                // Make sure this transaction / alert type combo doesn't already exist
                var alreadyAlerted = recentAlerts.Any( a =>
                    a.AlertTypeId == alertType.Id &&
                    a.TransactionId == transaction.Id );

                if ( alreadyAlerted )
                {
                    continue;
                }

                // Ensure that this alert type is allowed (might be disallowed because of global repeat prevention durations)
                if ( !allowFollowUp && alertType.AlertType == AlertType.FollowUp )
                {
                    continue;
                }

                if ( !allowGratitude && alertType.AlertType == AlertType.Gratitude )
                {
                    continue;
                }

                // Check the days since the last transaction are within allowed range
                if ( alertType.MaximumDaysSinceLastGift.HasValue && daysSinceLastTransaction > alertType.MaximumDaysSinceLastGift.Value )
                {
                    continue;
                }

                // Check the min gift amount
                if ( alertType.MinimumGiftAmount.HasValue && transaction.TotalAmount < alertType.MinimumGiftAmount )
                {
                    // Gift is less than this rule allows
                    continue;
                }

                // Check the max gift amount
                if ( alertType.MaximumGiftAmount.HasValue && transaction.TotalAmount > alertType.MaximumGiftAmount )
                {
                    // Gift is more than this rule allows
                    continue;
                }

                // Check if this alert type has already been alerted too recently
                if ( alertType.RepeatPreventionDuration.HasValue && recentAlerts?.Any( a => a.AlertTypeId == alertType.Id ) == true )
                {
                    var lastAlertOfTypeDate = recentAlerts.Last( a => a.AlertTypeId == alertType.Id ).AlertDateTime;
                    var daysSinceLastAlert = ( context.Now - lastAlertOfTypeDate ).TotalDays;

                    if ( daysSinceLastAlert <= alertType.RepeatPreventionDuration.Value )
                    {
                        // Alert would be too soon after the last alert was generated
                        continue;
                    }
                }

                // Check if the campus is a match
                if ( alertType.CampusId.HasValue )
                {
                    var campusId = person.GetCampus()?.Id;

                    if ( alertType.CampusId != campusId )
                    {
                        // Campus does not match
                        continue;
                    }
                }

                // Check the median gift amount
                var medianGiftAmount = GetGivingUnitAttributeValue( context, people, Rock.SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN ).AsDecimal();

                if ( alertType.MinimumMedianGiftAmount.HasValue && medianGiftAmount < alertType.MinimumMedianGiftAmount )
                {
                    // Median gift amount is too small for this rule
                    continue;
                }

                if ( alertType.MaximumMedianGiftAmount.HasValue && medianGiftAmount > alertType.MaximumMedianGiftAmount )
                {
                    // Median gift amount is too large for this rule
                    continue;
                }

                // Check the number of IQRs that the amount varies
                var amountIqr = GetGivingUnitAttributeValue( context, people, Rock.SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR ).AsDecimal();
                var numberOfAmountIqrs = GetAmountIqrCount( context, people, transaction.TotalAmount );

                // Check the frequency sensitivity scale
                var frequencyStdDev = GetGivingUnitAttributeValue( context, people, Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS ).AsDecimal();
                var frequencyMean = GetGivingUnitAttributeValue( context, people, Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS ).AsDecimal();
                var frequencyDeviation = frequencyMean - Convert.ToDecimal( daysSinceLastTransaction );
                var numberOfFrequencyStdDevs = GetFrequencyDeviationCount( context, people, daysSinceLastTransaction );

                // Detect which thing, amount or frequency, is exceeding the rule's sensitivity scale
                var reasons = new List<string>();

                if ( alertType.AlertType == AlertType.Gratitude )
                {
                    if ( alertType.AmountSensitivityScale.HasValue && numberOfAmountIqrs >= alertType.AmountSensitivityScale.Value )
                    {
                        // Gift is larger than the sensitivity
                        reasons.Add( nameof( alertType.AmountSensitivityScale ) );
                    }

                    if ( alertType.FrequencySensitivityScale.HasValue && numberOfFrequencyStdDevs >= alertType.FrequencySensitivityScale )
                    {
                        // Gift is earlier than the sensitivity
                        reasons.Add( nameof( alertType.FrequencySensitivityScale ) );
                    }
                }
                else if ( alertType.AlertType == AlertType.FollowUp )
                {
                    if ( alertType.AmountSensitivityScale.HasValue && numberOfAmountIqrs <= ( alertType.AmountSensitivityScale * -1 ) )
                    {
                        // Gift is outside the amount sensitivity scale
                        reasons.Add( nameof( alertType.AmountSensitivityScale ) );
                    }

                    if ( alertType.FrequencySensitivityScale.HasValue && numberOfFrequencyStdDevs <= ( alertType.FrequencySensitivityScale * -1 ) )
                    {
                        // Gift is outside the frequency sensitivity scale
                        reasons.Add( nameof( alertType.FrequencySensitivityScale ) );
                    }
                }

                // Sensitivity scales are optional. If all other filters are met, then allow the alert to be created
                if ( !reasons.Any() && ( alertType.AmountSensitivityScale.HasValue || alertType.FrequencySensitivityScale.HasValue ) )
                {
                    // If amount and frequency are within "normal" sensitivity levels for this rule, then continue on without an alert.
                    continue;
                }

                // Check at least one of the people are within the dataview
                if ( alertType.DataViewId.HasValue )
                {
                    var personIdQuery = context.DataViewPersonQueries.GetValueOrNull( alertType.DataViewId.Value );

                    // If the query hasn't already been created, generate the person id query for this dataview
                    if ( personIdQuery is null )
                    {
                        personIdQuery = CreatePersonIdQueryForDataView( rockContext, alertType, context );

                        if ( personIdQuery is null )
                        {
                            // Errors are logged within the creation method so just return
                            return alerts;
                        }

                        context.DataViewPersonQueries[alertType.DataViewId.Value] = personIdQuery;
                    }

                    // Check at least one of the people are within the dataview
                    if ( !personIdQuery.Any( id => people.Any( p => p.Id == id ) ) )
                    {
                        // None of the people are in the dataview
                        continue;
                    }
                }

                // Create the alert because this gift is a match for this rule.
                var financialTransactionAlert = new FinancialTransactionAlert
                {
                    TransactionId = transaction.Id,
                    PersonAliasId = transaction.AuthorizedPersonAliasId,
                    GivingId = person.GivingId,
                    AlertTypeId = alertType.Id,
                    Amount = transaction.TotalAmount,
                    AmountCurrentMedian = medianGiftAmount,
                    AmountCurrentIqr = amountIqr,
                    AmountIqrMultiplier = numberOfAmountIqrs,
                    FrequencyCurrentMean = frequencyMean,
                    FrequencyCurrentStandardDeviation = frequencyStdDev,
                    FrequencyDifferenceFromMean = frequencyDeviation,
                    FrequencyZScore = numberOfFrequencyStdDevs,
                    ReasonsKey = reasons.ToJson(),
                    AlertDateTime = context.Now,
                    AlertDateKey = context.Now.ToDateKey()
                };

                alerts.Add( financialTransactionAlert );

                // Return if not set to continue after a match has been made. Otherwise, let the loop continue to the next rule.
                if ( !alertType.ContinueIfMatched )
                {
                    return alerts;
                }
            }

            return alerts;
        }

        /// <summary>
        /// Processes the giving identifier. This logic was isolated for automated testing.
        /// </summary>
        /// <param name="givingId">The giving identifier.</param>
        /// <param name="people">The people.</param>
        /// <param name="transactions">The past year transactions.</param>
        /// <param name="mostRecentOldTransactionDate">The most recent old transaction date.</param>
        /// <param name="context">The context.</param>
        /// <param name="minDate">The minimum date that the transactions were queried with.</param>
        /// <returns>
        /// True if success
        /// </returns>
        internal static bool UpdateGivingUnitClassifications(
            string givingId,
            List<Person> people,
            List<TransactionView> transactions,
            DateTime? mostRecentOldTransactionDate,
            GivingAutomationContext context,
            DateTime minDate )
        {
            if ( transactions == null )
            {
                context.Errors.Add( $"the list of transactions was null for giving id {givingId}" );
                return false;
            }

            if ( people?.Any() != true )
            {
                context.Errors.Add( $"There were no people passed in the giving group {givingId}" );
                return false;
            }

            if ( people.Any( p => p.GivingId != givingId ) )
            {
                context.Errors.Add( $"The people (IDs: {people.Select( p => p.Id.ToString() ).JoinStringsWithCommaAnd()}) are not within the same giving group {givingId}" );
                return false;
            }

            if ( people.Any( p => p.Attributes == null ) )
            {
                context.Errors.Add( $"The people (IDs: {people.Select( p => p.Id.ToString() ).JoinStringsWithCommaAnd()}) did not have attributes loaded for giving group {givingId}" );
                return false;
            }

            // Update the group's gift count attributes
            var transactionTwelveMonthCount = transactions.Count;
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_12_MONTHS_COUNT, transactionTwelveMonthCount );

            var ninetyDaysAgo = context.Now.AddDays( -90 );
            var transactionNinetyDayCount = transactions.Count( t => t.TransactionDateTime >= ninetyDaysAgo );
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_90_DAYS_COUNT, transactionNinetyDayCount );

            // Update the group's gift total attributes
            var transactionTwelveMonthTotal = transactions.Sum( t => t.TotalAmount );
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_12_MONTHS, transactionTwelveMonthTotal );

            var transactionNinetyDayTotal = transactions.Where( t => t.TransactionDateTime >= ninetyDaysAgo ).Sum( t => t.TotalAmount );
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_90_DAYS, transactionNinetyDayTotal );

            var oneEightyDaysAgo = context.Now.AddDays( -180 );
            var transactionPriorNinetyDayTotal = transactions
                .Where( t =>
                    t.TransactionDateTime >= oneEightyDaysAgo &&
                    t.TransactionDateTime < ninetyDaysAgo )
                .Sum( t => t.TotalAmount );
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_PRIOR_90_DAYS, transactionPriorNinetyDayTotal );

            // Update the groups "last gift date" attribute
            var lastGiftDate = transactions.LastOrDefault().TransactionDateTime;
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_ERA_LAST_GAVE, lastGiftDate );

            // Store percent scheduled
            var scheduledTransactionsCount = transactions.Count( t => t.IsScheduled );
            var percentScheduled = GetPercentInt( scheduledTransactionsCount, transactionTwelveMonthCount );
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_PERCENT_SCHEDULED, percentScheduled );

            // Store preferred source
            var sourceGroups = transactions.GroupBy( t => t.SourceTypeValueGuid ).OrderByDescending( g => g.Count() );
            var maxSourceCount = sourceGroups.FirstOrDefault()?.Count() ?? 0;
            var preferredSourceTransactions = sourceGroups
                .Where( g => g.Count() == maxSourceCount )
                .SelectMany( g => g.ToList() )
                .OrderByDescending( t => t.TransactionDateTime );

            var preferredSourceGuid = preferredSourceTransactions.FirstOrDefault()?.SourceTypeValueGuid;
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_SOURCE, preferredSourceGuid.ToStringSafe() );

            // Store preferred currency
            var currencyGroups = transactions.GroupBy( t => t.CurrencyTypeValueGuid ).OrderByDescending( g => g.Count() );
            var maxCurrencyCount = currencyGroups.FirstOrDefault()?.Count() ?? 0;
            var preferredCurrencyTransactions = currencyGroups
                .Where( g => g.Count() == maxCurrencyCount )
                .SelectMany( g => g.ToList() )
                .OrderByDescending( t => t.TransactionDateTime );

            var preferredCurrencyGuid = preferredCurrencyTransactions.FirstOrDefault()?.CurrencyTypeValueGuid;
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_PREFERRED_CURRENCY, preferredCurrencyGuid.ToStringSafe() );

            // ii.) Classifications for: Bin, Percentile
            //      a.) If there is 12 months of giving use that.
            //      b.) If not then use the current number of days of gifts to extrapolate a full year. So if you have 60
            //          days of giving, multiply the giving amount by 6.08( 356 / 60 ). But there must be at least 3 gifts.
            var extrapolationFactor = 1m;
            var hasMoreTransactions = mostRecentOldTransactionDate.HasValue;

            if ( !hasMoreTransactions )
            {
                var oldestGiftDate = transactions.FirstOrDefault()?.TransactionDateTime;
                var daysSinceOldestGift = oldestGiftDate == null ? 0d : ( context.Now - oldestGiftDate.Value ).TotalDays;
                var daysSinceMinDate = ( context.Now - minDate ).TotalDays;
                extrapolationFactor = Convert.ToDecimal( daysSinceOldestGift > 0d ? ( daysSinceMinDate / daysSinceOldestGift ) : 0d );

                if ( extrapolationFactor > 1m )
                {
                    extrapolationFactor = 1m;
                }
            }

            // Store bin
            var yearGiftAmount = transactions.Sum( t => t.TotalAmount ) * extrapolationFactor;
            var binIndex = 3;

            while ( binIndex >= 1 )
            {
                var lowerLimitForNextBin = GetGivingBinLowerLimit( binIndex - 1 );

                if ( !lowerLimitForNextBin.HasValue || yearGiftAmount >= lowerLimitForNextBin )
                {
                    binIndex--;
                }
                else
                {
                    break;
                }
            }

            var bin = binIndex + 1;
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_BIN, bin );

            // Store percentile
            var percentileInt = 0;

            while ( percentileInt < ( context.PercentileLowerRange.Count - 1 ) )
            {
                var nextPercentileInt = percentileInt + 1;
                var nextPercentileLowerRange = context.PercentileLowerRange[nextPercentileInt];

                if ( yearGiftAmount >= nextPercentileLowerRange )
                {
                    percentileInt++;
                }
                else
                {
                    break;
                }
            }

            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_PERCENTILE, percentileInt );

            // Update the last classification run date to now
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE, context.Now );

            /*
            // iii.) Classification for: Median Amount, IQR Amount, Mean Frequency, Frequency Standard Deviation
            //      a.) If there is 12 months of giving use all of those
            //      b.) If not use the previous gifts that are within 12 months but there must be at least 5 gifts.
            //      c.) For Amount: we will calculate the median and interquartile range
            //      d.) For Frequency: we will calculate the trimmed mean and standard deviation. The trimmed mean will
            //          exclude the top 10 % largest and smallest gifts with in the dataset. If the number of gifts
            //          available is < 10 then we’ll remove the top largest and smallest gift.
            */

            if ( transactionTwelveMonthCount < 5 )
            {
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, string.Empty );
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, string.Empty );
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, string.Empty );
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, string.Empty );
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL, string.Empty );
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE, string.Empty );
                return true;
            }

            /*
            // Interquartile range deals with finding the median. Then we say the numbers before the median numbers
            // are q1 and the numbers after are q1.
            // Ex: 50, 100, 101, 103, 103, 5000
            // Q1, Median, and then Q3: (50, 100), (101, 103), (103, 5000)
            // IQR is the median(Q3) - median(Q1)
            */

            // Store median amount
            var orderedAmounts = transactions.Select( t => t.TotalAmount ).OrderBy( a => a ).ToList();
            var quartileRanges = SplitQuartileRanges( orderedAmounts );
            var medianAmount = quartileRanges.Item2.Average();
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN, medianAmount );

            // Store IQR amount
            var q1Median = GetMedian( quartileRanges.Item1 );
            var q3Median = GetMedian( quartileRanges.Item3 );
            var iqrAmount = q3Median - q1Median;
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR, iqrAmount );

            // Create a parallel array that stores the days since the last transaction for the transaction at that index
            var daysSinceLastTransaction = new List<double?>();
            var lastTransactionDate = mostRecentOldTransactionDate;

            foreach ( var transaction in transactions )
            {
                var currentTransactionDate = transaction.TransactionDateTime;

                if ( lastTransactionDate.HasValue )
                {
                    var daysSince = ( currentTransactionDate - lastTransactionDate.Value ).TotalDays;
                    daysSinceLastTransaction.Add( daysSince );
                }
                else
                {
                    daysSinceLastTransaction.Add( null );
                }

                lastTransactionDate = currentTransactionDate;
            }

            // Store Mean Frequency
            var daysSinceLastTransactionWithValue = daysSinceLastTransaction.Where( d => d.HasValue ).Select( d => d.Value ).ToList();
            var meanFrequencyDays = daysSinceLastTransactionWithValue.Count > 0 ? daysSinceLastTransactionWithValue.Average() : 0;
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS, meanFrequencyDays );

            // Store Frequency Std Dev
            var frequencyStdDevDays = Math.Sqrt( daysSinceLastTransactionWithValue.Average( d => Math.Pow( d - meanFrequencyDays, 2 ) ) );
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS, frequencyStdDevDays );

            // Frequency Labels:
            //      Weekly = Avg days between 4.5 - 8.5; Std Dev< 7;
            //      2 Weeks = Avg days between 9 - 17; Std Dev< 10;
            //      Monthly = Avg days between 25 - 35; Std Dev< 10;
            //      Quarterly = Avg days between 80 - 110; Std Dev< 15;
            //      Erratic = Freq Avg / 2 < Std Dev;
            //      Undetermined = Everything else

            // Attribute value values: 1^Weekly, 2^Bi-Weekly, 3^Monthly, 4^Quarterly, 5^Erratic, 6^Undetermined
            if ( meanFrequencyDays >= 4.5d && meanFrequencyDays <= 8.5d && frequencyStdDevDays < 7d )
            {
                // Weekly
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL, 1 );
            }
            else if ( meanFrequencyDays >= 9d && meanFrequencyDays <= 17d && frequencyStdDevDays < 10d )
            {
                // BiWeekly
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL, 2 );
            }
            else if ( meanFrequencyDays >= 25d && meanFrequencyDays <= 35d && frequencyStdDevDays < 10d )
            {
                // Monthly
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL, 3 );
            }
            else if ( meanFrequencyDays >= 80d && meanFrequencyDays <= 110d && frequencyStdDevDays < 15d )
            {
                // Quarterly
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL, 4 );
            }
            else if ( ( meanFrequencyDays / 2 ) < frequencyStdDevDays )
            {
                // Erratic
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL, 5 );
            }
            else
            {
                // Undetermined
                SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL, 6 );
            }

            // Update the next expected gift date
            var nextExpectedGiftDate = lastTransactionDate.HasValue ? lastTransactionDate.Value.AddDays( meanFrequencyDays ) : ( DateTime? ) null;
            SetGivingUnitAttributeValue( context, people, SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE, nextExpectedGiftDate );

            return true;
        }

        /// <summary>
        /// Processes the late alerts. This method finds giving groups that have an expected gift date that has now passed
        /// </summary>
        /// <param name="context">The context.</param>
        private static void ProcessLateAlerts( GivingAutomationContext context )
        {
            // Find the late gift alert types. There are already filtered to the alert types that should run today
            var lateGiftAlertTypes = context.AlertTypes
                .Where( at =>
                     at.AlertType == AlertType.FollowUp &&
                     at.FrequencySensitivityScale.HasValue )
                .ToList();

            if ( !lateGiftAlertTypes.Any() )
            {
                return;
            }

            // When querying for attribute values, we will use the most aggressive rule, which is the one with the
            // smallest sensitivity value.
            var minExpectedDate = context.Now.AddDays( -100 );
            var minFrequencySensitivity = lateGiftAlertTypes.Select( at => at.FrequencySensitivityScale.Value ).Min();

            var nextExpectedGiftAttributeId = AttributeCache.Get( SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE ).Id;
            var stdDevFrequencyDaysAttributeId = AttributeCache.Get( SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS ).Id;
            var fallbackStdDevDays = 7;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = context.SqlCommandTimeoutSeconds;

                var stopWatch = new Stopwatch();
                var elapsedTimesMs = new List<long>();

                if ( _debugModeEnabled )
                {
                    WriteToDebugOutput( "Starting late transaction query" );
                    stopWatch.Start();
                }

                var attributeValueService = new AttributeValueService( rockContext );

                // Query for the min sensitivity multiplied by the mean days to get each giving groups specific "late"
                // number of days
                var stdDevQuery = attributeValueService.Queryable()
                    .AsNoTracking()
                    .Where( av =>
                        av.AttributeId == stdDevFrequencyDaysAttributeId &&
                        av.ValueAsNumeric.HasValue &&
                        av.EntityId.HasValue )
                    .Select( av => new
                    {
                        PersonId = av.EntityId.Value,
                        StdDevDays = av.ValueAsNumeric.Value == 0 ? fallbackStdDevDays : av.ValueAsNumeric.Value
                    } );

                // Query for expected dates that have passed
                var pastExpectedDatesQuery = attributeValueService.Queryable()
                    .AsNoTracking()
                    .Where( av =>
                        av.AttributeId == nextExpectedGiftAttributeId &&
                        av.ValueAsDateTime.HasValue &&
                        av.ValueAsDateTime.Value < context.Now &&
                        av.ValueAsDateTime.Value > minExpectedDate )
                    .Select( av => new
                    {
                        PersonId = av.EntityId.Value,
                        ExpectedDateTime = av.ValueAsDateTime.Value
                    } );

                // Join the two queries to get the intersection
                var pastExpectedGivers = pastExpectedDatesQuery.Join(
                    stdDevQuery,
                    ed => ed.PersonId,
                    sd => sd.PersonId,
                    ( ed, sd ) => new
                    {
                        ed.PersonId,
                        ed.ExpectedDateTime,
                        sd.StdDevDays
                    } )
                    .ToList();

                // Filter the query for the giving groups that have exceeded the sensitivity level
                var personIds = pastExpectedGivers
                    .Where( p => p.ExpectedDateTime.AddDays( decimal.ToDouble( minFrequencySensitivity * p.StdDevDays ) ) < context.Now )
                    .Select( p => p.PersonId )
                    .ToList();

                if ( _debugModeEnabled )
                {
                    stopWatch.Stop();
                    var ms = stopWatch.ElapsedMilliseconds.ToString( "G" );
                    WriteToDebugOutput( $"Found {personIds.Count} people with potential late transactions in {ms}ms" );
                    stopWatch.Reset();
                }

                // Create a single late alert for each giving group
                var givingIds = new HashSet<string>();
                var personAliasService = new PersonAliasService( rockContext );
                var personService = new PersonService( rockContext );
                var financialTransactionService = new FinancialTransactionService( rockContext );
                var financialTransactionAlertService = new FinancialTransactionAlertService( rockContext );
                var oneYearAgo = context.Now.AddMonths( -12 );
                var alertsToAdd = new List<FinancialTransactionAlert>();

                foreach ( var personId in personIds )
                {
                    // Get the giving id
                    var givingId = personService.Queryable()
                        .AsNoTracking()
                        .Where( p => p.Id == personId )
                        .Select( p => p.GivingId )
                        .FirstOrDefault();

                    if ( givingId.IsNullOrWhiteSpace() || givingIds.Contains( givingId ) )
                    {
                        continue;
                    }

                    // Add to the hashset so this giving id is not processed again
                    givingIds.Add( givingId );

                    // Load the people that are in this giving group so their attribute values can be set
                    var people = personService.Queryable()
                        .Include( p => p.Aliases )
                        .Where( p => p.GivingId == givingId )
                        .ToList();
                    people.LoadAttributes();

                    // Get all aliases in the giving id
                    var aliasIds = people.SelectMany( p => p.Aliases.Select( a => a.Id ) ).ToList();

                    // Get the last giver, who the alert will be tied to
                    var lastTransactionAliasId = financialTransactionService.GetGivingAutomationSourceTransactionQuery()
                        .Where( ft =>
                            ft.AuthorizedPersonAliasId.HasValue &&
                            aliasIds.Contains( ft.AuthorizedPersonAliasId.Value ) )
                        .OrderByDescending( ft => ft.TransactionDateTime )
                        .Select( ft => ft.AuthorizedPersonAliasId )
                        .FirstOrDefault();

                    if ( lastTransactionAliasId is null )
                    {
                        continue;
                    }

                    if ( _debugModeEnabled )
                    {
                        stopWatch.Start();
                    }

                    // Get any recent alerts for these people
                    var recentAlerts = financialTransactionAlertService.Queryable()
                        .AsNoTracking()
                        .Where( a =>
                            aliasIds.Contains( a.PersonAliasId ) &&
                            a.AlertDateTime > oneYearAgo )
                        .Select( a => new AlertView
                        {
                            AlertDateTime = a.AlertDateTime,
                            AlertTypeId = a.AlertTypeId,
                            AlertType = a.FinancialTransactionAlertType.AlertType,
                            TransactionId = a.TransactionId
                        } )
                        .OrderBy( a => a.AlertDateTime )
                        .ToList();

                    // Create the alerts
                    var alerts = CreateAlertsForLateTransaction( rockContext, lateGiftAlertTypes, lastTransactionAliasId.Value, people, recentAlerts, context );
                    alertsToAdd.AddRange( alerts );

                    if ( _debugModeEnabled )
                    {
                        stopWatch.Stop();
                        elapsedTimesMs.Add( stopWatch.ElapsedMilliseconds );
                        var ms = stopWatch.ElapsedMilliseconds.ToString( "G" );
                        WriteToDebugOutput( $"Giving Id {givingId} late alerts done in {ms}ms" );
                        stopWatch.Reset();
                    }
                }

                if ( _debugModeEnabled && elapsedTimesMs.Any() )
                {
                    WriteToDebugOutput( $"Finished {elapsedTimesMs.Count} giving ids for late alerts in average of {elapsedTimesMs.Average()}ms per giving unit" );
                }

                // Save the new alerts to the database
                if ( alertsToAdd.Any() )
                {
                    financialTransactionAlertService.AddRange( alertsToAdd );
                    rockContext.SaveChanges();

                    context.AlertsCreated += alertsToAdd.Count;
                    HandlePostAlertsAddedLogic( alertsToAdd );
                }
            }
        }

        /// <summary>
        /// Creates the alerts for a late transaction.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="lateGiftAlertTypes">The late gift alert types.</param>
        /// <param name="lastTransactionAuthorizedAliasId">The last transaction authorized alias identifier.</param>
        /// <param name="people">The people.</param>
        /// <param name="recentAlerts">The recent alerts.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        internal static List<FinancialTransactionAlert> CreateAlertsForLateTransaction(
            RockContext rockContext,
            List<FinancialTransactionAlertType> lateGiftAlertTypes,
            int lastTransactionAuthorizedAliasId,
            List<Person> people,
            List<AlertView> recentAlerts,
            GivingAutomationContext context )
        {
            var alerts = new List<FinancialTransactionAlert>();

            if ( !people.Any() )
            {
                return alerts;
            }

            // The people all have the same attribute values, so this method will use the first person
            var person = people.First();

            // Check the repeat prevention durations
            var allowFollowUp = AllowFollowUpAlerts( recentAlerts, context );

            if ( !allowFollowUp )
            {
                return alerts;
            }

            // Load giving overview attributes
            var amountIqr = GetGivingUnitAttributeValue( context, people, Rock.SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR ).AsDecimal();
            var medianGiftAmount = GetGivingUnitAttributeValue( context, people, Rock.SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN ).AsDecimal();
            var frequencyStdDev = GetGivingUnitAttributeValue( context, people, Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS ).AsDecimal();
            var frequencyMean = GetGivingUnitAttributeValue( context, people, Rock.SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS ).AsDecimal();
            var lastGiftDate = GetGivingUnitAttributeValue( context, people, Rock.SystemGuid.Attribute.PERSON_ERA_LAST_GAVE ).AsDateTime();

            if ( !lastGiftDate.HasValue )
            {
                return alerts;
            }

            var daysSinceLastTransaction = ( context.Now - lastGiftDate.Value ).TotalDays;
            var frequencyDeviation = frequencyMean - Convert.ToDecimal( daysSinceLastTransaction );

            // Check the frequency sensitivity scale
            var numberOfFrequencyStdDevs = GetFrequencyDeviationCount( context, people, daysSinceLastTransaction );

            // Find the correct alert type to tie the new alert with
            foreach ( var alertType in lateGiftAlertTypes )
            {
                // Make sure that we don't generate late alerts more than once for a giving group since the last time they gave
                var mostRecentAlertOfThisType = recentAlerts.LastOrDefault( a => a.AlertTypeId == alertType.Id )?.AlertDateTime;

                if ( mostRecentAlertOfThisType.HasValue && mostRecentAlertOfThisType > lastGiftDate.Value )
                {
                    continue;
                }

                // Check the maximum days since the last alert
                if ( alertType.MaximumDaysSinceLastGift.HasValue && daysSinceLastTransaction > alertType.MaximumDaysSinceLastGift )
                {
                    continue;
                }

                // Check if this alert type has already been alerted too recently
                if ( alertType.RepeatPreventionDuration.HasValue && recentAlerts?.Any( a => a.AlertTypeId == alertType.Id ) == true )
                {
                    var lastAlertOfTypeDate = recentAlerts.Last( a => a.AlertTypeId == alertType.Id ).AlertDateTime;
                    var daysSinceLastAlert = ( context.Now - lastAlertOfTypeDate ).TotalDays;

                    if ( daysSinceLastAlert <= alertType.RepeatPreventionDuration.Value )
                    {
                        // Alert would be too soon after the last alert was generated
                        continue;
                    }
                }

                // Check if the campus is a match
                if ( alertType.CampusId.HasValue )
                {
                    var campusId = person.GetCampus()?.Id;

                    if ( alertType.CampusId != campusId )
                    {
                        // Campus does not match
                        continue;
                    }
                }

                // Check the median gift amount
                if ( alertType.MinimumMedianGiftAmount.HasValue && medianGiftAmount < alertType.MinimumMedianGiftAmount )
                {
                    // Median gift amount is too small for this rule
                    continue;
                }

                if ( alertType.MaximumMedianGiftAmount.HasValue && medianGiftAmount > alertType.MaximumMedianGiftAmount )
                {
                    // Median gift amount is too large for this rule
                    continue;
                }

                // Detect which thing, amount or frequency, is exceeding the rule's sensitivity scale
                var reasons = new List<string>();

                if ( numberOfFrequencyStdDevs <= ( alertType.FrequencySensitivityScale * -1 ) )
                {
                    // The current date is outside the frequency sensitivity scale
                    reasons.Add( nameof( alertType.FrequencySensitivityScale ) );
                }

                if ( !reasons.Any() )
                {
                    // If frequency is within "normal" sensitivity levels for this rule, then continue on without an alert.
                    continue;
                }

                // How many std deviations does this transaction exceed the sensitivity level by?
                // FrequencySensitivityScale is going to be positive, and numberOfFrequencyStdDevs is negative
                var exceedingFrequencyStdDevs = Math.Abs( numberOfFrequencyStdDevs ) - alertType.FrequencySensitivityScale.Value;
                var exceedingFrequencyStdDevsAsDays = exceedingFrequencyStdDevs * frequencyStdDev;

                // Check at least one of the people are within the dataview
                if ( alertType.DataViewId.HasValue )
                {
                    var personIdQuery = context.DataViewPersonQueries.GetValueOrNull( alertType.DataViewId.Value );

                    // If the query hasn't already been created, generate the person id query for this dataview
                    if ( personIdQuery is null )
                    {
                        personIdQuery = CreatePersonIdQueryForDataView( rockContext, alertType, context );

                        if ( personIdQuery is null )
                        {
                            // Errors are logged within the creation method so just return
                            break;
                        }

                        context.DataViewPersonQueries[alertType.DataViewId.Value] = personIdQuery;
                    }

                    // Check at least one of the people are within the dataview
                    if ( !personIdQuery.Any( id => people.Any( p => p.Id == id ) ) )
                    {
                        // None of the people are in the dataview
                        continue;
                    }
                }

                // Create the alert
                var financialTransactionAlert = new FinancialTransactionAlert
                {
                    TransactionId = null,
                    PersonAliasId = lastTransactionAuthorizedAliasId,
                    GivingId = person.GivingId,
                    AlertTypeId = alertType.Id,
                    Amount = null,
                    AmountCurrentMedian = medianGiftAmount,
                    AmountCurrentIqr = amountIqr,
                    AmountIqrMultiplier = null,
                    FrequencyCurrentMean = frequencyMean,
                    FrequencyCurrentStandardDeviation = frequencyStdDev,
                    FrequencyDifferenceFromMean = null,
                    FrequencyZScore = null,
                    ReasonsKey = reasons.ToJson(),
                    AlertDateTime = context.Now,
                    AlertDateKey = context.Now.ToDateKey()
                };

                alerts.Add( financialTransactionAlert );

                // Break if not set to continue after a match has been made. Otherwise, let the loop continue to the next rule.
                if ( !alertType.ContinueIfMatched )
                {
                    break;
                }
            }

            return alerts;
        }

        /// <summary>
        /// Handles the post alerts-added logic.
        /// </summary>
        /// <param name="alertsAdded">The alerts added.</param>
        private static void HandlePostAlertsAddedLogic( List<FinancialTransactionAlert> alertsAdded )
        {
            foreach ( var alert in alertsAdded )
            {
                if ( alert.Id == 0 )
                {
                    continue;
                }

                new ProcessTransactionAlertActions.Message
                {
                    FinancialTransactionAlertId = alert.Id
                }.Send();
            }
        }

        #endregion Execute Logic

        #region Debug

        /// <summary>
        /// Log debug output?
        /// </summary>
        private static bool _debugModeEnabled = false;

        /// <summary>
        /// Debugs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void WriteToDebugOutput( string message )
        {
            if ( _debugModeEnabled && System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
            {
                System.Diagnostics.Debug.WriteLine( $"\tGiving Automation {RockDateTime.Now:mm.ss.f} {message}" );
            }
        }

        #endregion Debug

        #region Classes

        /// <summary>
        /// Giving Automation Context
        /// </summary>
        public sealed class GivingAutomationContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GivingAutomationContext"/> class.
            /// </summary>
            /// <param name="jobExecutionContext">The job execution context.</param>
            public GivingAutomationContext( IJobExecutionContext jobExecutionContext )
            {
                JobExecutionContext = jobExecutionContext;
                JobDataMap = jobExecutionContext.JobDetail.JobDataMap;
                DataViewPersonQueries = new Dictionary<int, IQueryable<int>>();
                SqlCommandTimeoutSeconds = JobDataMap.GetString( GivingAutomation.AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? AttributeDefaultValue.CommandTimeout;
            }

            #region Fields

            /// <summary>
            /// Returns true based on <see cref="GivingClassificationSettings.RunDays"/>
            /// </summary>
            /// <value><c>true</c> if this instance is giving classification run day; otherwise, <c>false</c>.</value>
            public bool IsGivingClassificationRunDay { get; set; }

            /// <summary>
            /// Gets the SQL command timeout seconds.
            /// </summary>
            /// <value>The SQL command timeout seconds.</value>
            public int SqlCommandTimeoutSeconds { get; private set; }

            #endregion Fields

            /// <summary>
            /// The date time to consider as current time. The time when this processing instance began
            /// </summary>
            public DateTime Now { get; set; } = RockDateTime.Now;

            /// <summary>
            /// The errors
            /// </summary>
            public readonly HashSet<string> Errors = new HashSet<string>();

            /// <summary>
            /// Gets the job execution context.
            /// </summary>
            /// <value>
            /// The job execution context.
            /// </value>
            public IJobExecutionContext JobExecutionContext { get; }

            /// <summary>
            /// Gets the job data map.
            /// </summary>
            /// <value>
            /// The job data map.
            /// </value>
            public JobDataMap JobDataMap { get; }

            /// <summary>
            /// Gets or sets the giving ids to classify.
            /// </summary>
            /// <value>
            /// The giving ids to classify.
            /// </value>
            public List<string> GivingIdsToClassify { get; set; }

            /// <summary>
            /// Gets or sets the giving ids classified.
            /// </summary>
            /// <value>
            /// The giving ids classified.
            /// </value>
            public int GivingIdsSuccessful { get; set; }

            /// <summary>
            /// Gets or sets the alerts created.
            /// </summary>
            /// <value>
            /// The alerts created.
            /// </value>
            public int AlertsCreated { get; set; }

            /// <summary>
            /// Gets or sets the giving ids failed.
            /// </summary>
            /// <value>
            /// The giving ids failed.
            /// </value>
            public int GivingIdsFailed { get; set; }

            /// <summary>
            /// Gets or sets the percentile lower range.
            /// Ex. Index 50 holds the lower range for being in the 50th percentile of the givers within the church
            /// </summary>
            /// <value>
            /// The percentile lower range.
            /// </value>
            public List<decimal> PercentileLowerRange { get; set; }

            /// <summary>
            /// Gets or sets the alert types.
            /// </summary>
            /// <value>
            /// The alert types.
            /// </value>
            public List<FinancialTransactionAlertType> AlertTypes { get; set; }

            /// <summary>
            /// Gets the data view person queries.
            /// </summary>
            /// <value>
            /// The data view person queries.
            /// </value>
            public Dictionary<int, IQueryable<int>> DataViewPersonQueries { get; }

            /// <summary>
            /// Gets the attribute value.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <returns></returns>
            public string GetAttributeValue( string key )
            {
                return JobDataMap.GetString( key );
            }
        }

        #endregion
    }

    /// <summary>
    /// Alert View
    /// </summary>
    public sealed class AlertView
    {
        /// <summary>
        /// Gets or sets the alert type identifier.
        /// </summary>
        /// <value>
        /// The alert type identifier.
        /// </value>
        public int AlertTypeId { get; set; }

        /// <summary>
        /// Gets or sets the alert date time.
        /// </summary>
        /// <value>
        /// The alert date time.
        /// </value>
        public DateTime AlertDateTime { get; set; }

        /// <summary>
        /// Gets the type of the alert.
        /// </summary>
        /// <value>
        /// The type of the alert.
        /// </value>
        public AlertType AlertType { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        public int? TransactionId { get; set; }
    }

    /// <summary>
    /// Transaction View
    /// </summary>
    public sealed class TransactionView
    {
        /// <summary>
        /// Gets or sets the transaction date time.
        /// </summary>
        /// <value>
        /// The transaction date time.
        /// </value>
        public DateTime TransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the total amount.
        /// </summary>
        /// <value>
        /// The total amount.
        /// </value>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the currency type value identifier.
        /// </summary>
        /// <value>
        /// The currency type value identifier.
        /// </value>
        public int? CurrencyTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the source type value identifier.
        /// </summary>
        /// <value>
        /// The source type value identifier.
        /// </value>
        public int? SourceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the currency type value identifier.
        /// </summary>
        /// <value>
        /// The currency type value identifier.
        /// </value>
        public Guid? CurrencyTypeValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the source type value identifier.
        /// </summary>
        /// <value>
        /// The source type value identifier.
        /// </value>
        public Guid? SourceTypeValueGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is scheduled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is scheduled; otherwise, <c>false</c>.
        /// </value>
        public bool IsScheduled { get; set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the authorized person alias identifier.
        /// </summary>
        /// <value>
        /// The authorized person alias identifier.
        /// </value>
        public int AuthorizedPersonAliasId { get; set; }
    }
}