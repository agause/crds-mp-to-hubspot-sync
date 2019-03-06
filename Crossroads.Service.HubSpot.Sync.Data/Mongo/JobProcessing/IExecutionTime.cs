using System;

namespace Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing
{
    public interface IExecutionTime
    {
        DateTime StartUtc { get; }

        DateTime FinishUtc { get; set; }

        string Duration { get; }
    }
}