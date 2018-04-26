﻿using Crossroads.Service.HubSpot.Sync.Data.HubSpot.Models.Response;
using System.Net;

namespace Crossroads.Service.HubSpot.Sync.Data.LiteDb.JobProcessing.Dto
{
    public interface IFailureDetails
    {
        HttpStatusCode HttpStatusCode { get; set; }

        HubSpotException Exception { get; set; }
    }
}
