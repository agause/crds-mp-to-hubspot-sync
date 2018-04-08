﻿using System;
using Crossroads.Service.HubSpot.Sync.LiteDb;
using LiteDB;

namespace Crossroads.Service.HubSpot.Sync.Data.LiteDb.JobProcessing.Dto
{
    public struct LastSuccessfulSyncDate : IKeyValuePair<string, DateTime>
    {
        [BsonField("_id")]
        public string Key => "MpNewContactRegistration_LastSuccessfulSyncDate";

        public DateTime Value { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}