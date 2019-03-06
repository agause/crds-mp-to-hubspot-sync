using System;
using System.Collections.Generic;
using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Dto;

namespace Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing
{
    /// <summary>
    /// Holds sync job related settings and operational results.
    /// </summary>
    public interface IJobRepository
    {
        OperationDates PersistLastSuccessfulOperationDates(OperationDates operationDates);

        void PersistActivityProgress(ActivityProgress activityProgress);

        void PersistActivity(Activity activity);

        void PersistHubSpotApiDailyRequestCount(int mostRecentRequestCount, DateTime activityDateTime);

        List<HubSpotApiDailyRequestCountKeyValue> GetHubSpotApiDailyRequestCount();

        string GetActivity(string syncJobActivityId);

        string GetMostRecentActivity();

        List<string> GetActivityIds(int limit);
    }
}
