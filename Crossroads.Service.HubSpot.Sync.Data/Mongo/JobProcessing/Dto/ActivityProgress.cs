using Crossroads.Service.HubSpot.Sync.Core.Formatters;
using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Dto
{
    /// <summary>
    /// Commenting mostly for visibility into the fact that this class is a bit busier than its contemporaries.
    /// Newing this object up primes the <see cref="Operations"/> property with all OperationName values and a <see cref="OperationState"/>
    /// of "<see cref="OperationState.Pending"/>".
    /// </summary>
    public class ActivityProgress : IEmitHtml, IEmitPlainText
    {
        private static readonly string Crlf = Environment.NewLine;
        public ActivityState ActivityState { get; set; }

        public string Duration { get; set; }

        public Dictionary<OperationName, OperationDetail> Operations { get; } =
            System.Enum.GetValues(typeof(OperationName)) // primes the Operations dictionary
                .Cast<OperationName>()
                .ToDictionary(k => k, v => new OperationDetail { OperationState = OperationState.Pending });

        public string ToPlainText() =>
             $@"Activity State: {ActivityState}
Duration: {Duration}

{string.Join($"{Crlf}{Crlf}", Operations.Select(PlainTextOperationSelector))}";

        public string ToHtml() =>
            $@"Activity State: <strong>{ActivityState}</strong><br/>
            Duration: {Duration}<br/><br/>

            {string.Join("<br/><br/>", Operations.Select(HtmlOperationSelector))}";

        private static Func<KeyValuePair<OperationName, OperationDetail>, string> HtmlOperationSelector =>
            k => $"<u>{k.Key.ToString().SpaceDelimitTitleCaseText()}</u><br/>{k.Value.ToHtml()}";

        private static Func<KeyValuePair<OperationName, OperationDetail>, string> PlainTextOperationSelector =>
            k => $"{k.Key.ToString().SpaceDelimitTitleCaseText()}{Crlf}{k.Value.ToPlainText()}";
    }
}