using Crossroads.Service.HubSpot.Sync.Core.Formatters;
using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Enum;

namespace Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Dto
{
    public class OperationDetail : IEmitPlainText, IEmitHtml
    {
        public OperationState OperationState { get; set; }

        public int ContactCount { get; set; }

        public string Duration { get; set; }

        public string ToPlainText()
        {
            return $@"Operation State: {OperationState}
Contact count: {ContactCount}
Duration: {Duration}";
        }

        public string ToHtml()
        {
            return $"Operation State: <strong>{OperationState}</strong><br/>Contact count: {ContactCount}<br/>Duration: {Duration}";
        }
    }
}