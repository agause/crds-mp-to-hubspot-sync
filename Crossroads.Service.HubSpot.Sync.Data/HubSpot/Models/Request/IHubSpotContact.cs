using System.Collections.Generic;
using Newtonsoft.Json;

namespace Crossroads.Service.HubSpot.Sync.Data.HubSpot.Models.Request
{
    /// <summary>
    /// Contract for HubSpot contact types.
    /// </summary>
    public interface IHubSpotContact
    {
        string Email { get; set; }

        [JsonProperty(PropertyName = "properties")]
        List<HubSpotContactProperty> Properties { get; set; }
    }
}