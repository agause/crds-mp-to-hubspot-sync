﻿using System;
using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Dto;

namespace Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing
{
    public interface IActivitySyncOperation
    {
        ExecutionTime Execution { get; }

        DateTime PreviousSyncDate { get; set; }

        int TotalContacts { get; }

        int SuccessCount { get; }

        int FailureCount { get; }

        int UpdateSuccessCount { get; }

        int CreateSuccessCount { get; }

        int ReconciliationSuccessCount { get; }

        int UpdateFailureCount { get; }

        int CreateFailureCount { get; }

        int ReconciliationFailureCount { get; }

        int EmailAddressAlreadyExistsCount { get; }

        int EmailAddressDoesNotExistCount { get; }

        int HubSpotApiRequestCount { get; }

        SerialSyncResult SerialUpdateResult { get; set; }

        SerialSyncResult SerialCreateResult { get; set; }

        SerialSyncResult SerialReconciliationResult { get; set; }
    }
}
