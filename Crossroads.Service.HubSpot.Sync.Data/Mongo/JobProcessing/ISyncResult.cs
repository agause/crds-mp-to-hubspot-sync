using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Dto;

namespace Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing
{
    public interface ISyncResult
    {
        ExecutionTime Execution { get; }
        int FailureCount { get; }
        int SuccessCount { get; }
        int TotalContacts { get; }
    }
}