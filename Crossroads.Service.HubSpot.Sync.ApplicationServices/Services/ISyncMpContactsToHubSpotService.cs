﻿using System.Threading.Tasks;
using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Dto;

namespace Crossroads.Service.HubSpot.Sync.ApplicationServices.Services
{
    /// <summary>
    /// Service class for one-way syncing (creating and updating) Ministry Platform contacts
    /// to HubSpot.
    /// </summary>
    public interface ISyncMpContactsToHubSpotService
    {
        /// <summary>
        /// Syncs newly registered Ministry Platform contacts to HubSpot.
        /// </summary>
        Task<Activity> Sync();
    }
}