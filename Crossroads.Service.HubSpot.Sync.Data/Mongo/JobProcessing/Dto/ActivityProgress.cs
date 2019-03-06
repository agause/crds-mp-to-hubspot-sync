using Crossroads.Service.HubSpot.Sync.Core.Formatters;
using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Dto
{
    public class ActivityProgress : IEmitHtml, IEmitPlainText
    {
        private static readonly string Crlf = Environment.NewLine;

        /// <summary>
        /// Commenting mostly for visibility into the fact that this constructor is a bit busier than its contemporaries.
        /// Newing this object up primes the Steps property with all SyncStepName values and a <see cref="OperationState"/>
        /// of "<see cref="OperationState.Pending"/>".
        /// </summary>
        public ActivityProgress() => PrimeOperations();

        public ActivityState ActivityState { get; set; }

        public string Duration { get; set; }

        public Dictionary<string, OperationDetail> Operations { get; } = new Dictionary<string, OperationDetail>();

        private void PrimeOperations()
        {
            foreach (var syncStepName in System.Enum.GetValues(typeof(OperationName)).Cast<OperationName>())
            {
                Operations.Add(syncStepName.ToString(), new OperationDetail {OperationState = OperationState.Pending});
            }
        }

        public string ToPlainText() =>
             $@"Activity State: {ActivityState}
Duration: {Duration}

{string.Join($"{Crlf}{Crlf}", Operations.Select(PlainTextOperationSelector))}";

        public string ToHtml() =>
            $@"Activity State: <strong>{ActivityState}</strong><br/>
            Duration: {Duration}<br/><br/>

            {string.Join("<br/><br/>", Operations.Select(HtmlOperationSelector))}";

        private static Func<KeyValuePair<string, OperationDetail>, string> HtmlOperationSelector =>
            k => $"<u>{k.Key.SpaceDelimitTitleCaseText()}</u><br/>{k.Value.ToHtml()}";

        private static Func<KeyValuePair<string, OperationDetail>, string> PlainTextOperationSelector =>
            k => $"{k.Key.SpaceDelimitTitleCaseText()}{Crlf}{k.Value.ToPlainText()}";
    }
}