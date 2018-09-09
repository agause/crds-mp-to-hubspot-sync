﻿using Crossroads.Service.HubSpot.Sync.Data.MP.Dto;
using System;

namespace Crossroads.Service.HubSpot.Sync.Data.LiteDb.JobProcessing.Dto
{
    /// <summary>
    /// Captures results (stats, errors, etc) around the operation to synchronize updated
    /// Ministry Platform contact data to HubSpot.
    /// </summary>
    public class SyncActivityChildAgeAndGradeUpdateOperation : ISyncActivityChildAgeAndGradeUpdateOperation
    {
        public SyncActivityChildAgeAndGradeUpdateOperation()
        {
            BulkUpdateSyncResult1000 = new BulkSyncResult();
            BulkUpdateSyncResult100 = new BulkSyncResult();
            BulkUpdateSyncResult10 = new BulkSyncResult();
            RetryBulkUpdateAsSerialUpdateResult = new SerialSyncResult();
            SerialCreateResult = new SerialSyncResult();
        }

        public SyncActivityChildAgeAndGradeUpdateOperation(DateTime executionStartTime)
        {
            Execution = new ExecutionTime(executionStartTime);
            BulkUpdateSyncResult1000 = new BulkSyncResult();
            BulkUpdateSyncResult100 = new BulkSyncResult();
            BulkUpdateSyncResult10 = new BulkSyncResult();
            RetryBulkUpdateAsSerialUpdateResult = new SerialSyncResult();
            SerialCreateResult = new SerialSyncResult();
        }

        public IExecutionTime Execution { get; set; }

        public DateTime PreviousSyncDate { get; set; }

        public int TotalContacts => BulkUpdateSyncResult100.TotalContacts;

        public int SuccessCount => InitialSuccessCount + RetrySuccessCount;

        public int InitialSuccessCount => BulkUpdateSyncResult100.SuccessCount;

        public int RetrySuccessCount => BulkUpdateSyncResult100.SuccessCount +
                                        BulkUpdateSyncResult10.SuccessCount +
                                        RetryBulkUpdateAsSerialUpdateResult.SuccessCount +
                                        SerialCreateResult.SuccessCount;

        public int InitialFailureCount => BulkUpdateSyncResult100.FailureCount;

        public int RetryFailureCount => RetryBulkUpdateAsSerialUpdateResult.FailureCount + SerialCreateResult.FailureCount;

        public int EmailAddressAlreadyExistsCount => RetryBulkUpdateAsSerialUpdateResult.EmailAddressAlreadyExistsCount + SerialCreateResult.EmailAddressAlreadyExistsCount;

        public int HubSpotApiRequestCount => BulkUpdateSyncResult1000.BatchCount +
                                             BulkUpdateSyncResult100.BatchCount +
                                             BulkUpdateSyncResult10.BatchCount +
                                             RetryBulkUpdateAsSerialUpdateResult.TotalContacts +
                                             SerialCreateResult.TotalContacts;

        public ChildAgeAndGradeDeltaLogDto AgeAndGradeDelta { get; set; }

        public BulkSyncResult BulkUpdateSyncResult1000 { get; set; }

        public BulkSyncResult BulkUpdateSyncResult100 { get; set; }

        public BulkSyncResult BulkUpdateSyncResult10 { get; set; }

        public SerialSyncResult RetryBulkUpdateAsSerialUpdateResult { get; set; }

        public SerialSyncResult SerialCreateResult { get; set; }
    }
}
