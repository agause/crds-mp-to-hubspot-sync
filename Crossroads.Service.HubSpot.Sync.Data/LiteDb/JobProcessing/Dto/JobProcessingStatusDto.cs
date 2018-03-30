﻿using Crossroads.Service.HubSpot.Sync.Data.LiteDb.JobProcessing.Enum;
using Crossroads.Service.HubSpot.Sync.LiteDb;
using LiteDB;

namespace Crossroads.Service.HubSpot.Sync.Data.LiteDb.JobProcessing.Dto
{
    public struct JobProcessingStatusDto : IKeyValuePair<string, JobProcessingState>
    {
        [BsonField("_id")]
        public string Key => "JobProcessingStatus";

        public JobProcessingState Value { get; set; }
    }
}
