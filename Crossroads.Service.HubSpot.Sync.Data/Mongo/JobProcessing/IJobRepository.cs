using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Dto;

namespace Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing
{
    /// <summary>
    /// Holds sync job related settings and operational results.
    /// </summary>
    public interface IJobRepository
    {
        Task<OperationDates> PersistLastSuccessfulOperationDatesAsync(OperationDates operationDates);

        Task PersistActivityProgressAsync(ActivityProgress activityProgress);

        Task PersistActivityAsync(Activity activity);

        Task PersistHubSpotApiDailyRequestCountAsync(int mostRecentRequestCount, DateTime activityDateTime);

        Task<List<HubSpotApiDailyRequestCountKeyValue>> GetHubSpotApiDailyRequestCountAsync();

        Task<string> GetActivityAsync(string syncJobActivityId);

        Task<string> GetMostRecentActivity();

        Task<List<string>> GetActivityIdsAsync(int limit);
    }
}