using System.Threading.Tasks;
using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Dto;

namespace Crossroads.Service.HubSpot.Sync.ApplicationServices.Configuration
{
    public interface IConfigurationService
    {
        Task<OperationDates> GetLastSuccessfulOperationDatesAsync();

        /// <summary>
        /// On the initial sync run, it returns a new instance of SyncProgress with a JobState of Idle. All
        /// subsequent syncs will pull from the data store.
        /// </summary>
        Task<ActivityProgress> GetCurrentActivityProgressAsync();

        string GetEnvironmentName();

        bool PersistActivity();
    }
}