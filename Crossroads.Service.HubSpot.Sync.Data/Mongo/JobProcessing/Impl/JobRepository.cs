using Crossroads.Service.HubSpot.Sync.Core.Serialization;
using Crossroads.Service.HubSpot.Sync.Core.Time;
using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Dto;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Impl
{
    public class JobRepository : IJobRepository
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IClock _clock;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger<JobRepository> _logger;

        public JobRepository(
            IMongoDatabase mongoDatabase,
            IClock clock,
            IJsonSerializer jsonSerializer,
            ILogger<JobRepository> logger)
        {
            _mongoDatabase = mongoDatabase ?? throw new ArgumentNullException(nameof(mongoDatabase));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OperationDates> PersistLastSuccessfulOperationDatesAsync(OperationDates operationDates)
        {
            await _mongoDatabase
                .GetCollection<OperationDatesKeyValue>(nameof(OperationDatesKeyValue))
                .ReplaceOneAsync(
                    filter: Builders<OperationDatesKeyValue>.Filter.Eq("_id", nameof(OperationDatesKeyValue)),
                    replacement: new OperationDatesKeyValue { LastUpdatedUtc = _clock.UtcNow, Value = operationDates },
                    options: new UpdateOptions {IsUpsert = true});

            return operationDates;
        }

        public async Task PersistActivityProgressAsync(ActivityProgress activityProgress) =>
            await _mongoDatabase
                .GetCollection<ActivityProgressKeyValue>(nameof(ActivityProgressKeyValue))
                .ReplaceOneAsync(
                    filter: Builders<ActivityProgressKeyValue>.Filter.Eq("_id", nameof(ActivityProgressKeyValue)),
                    replacement: new ActivityProgressKeyValue { LastUpdatedUtc = _clock.UtcNow, Value = activityProgress },
                    options: new UpdateOptions { IsUpsert = true });

        public async Task PersistActivityAsync(Activity activity)
        {
            activity.LastUpdatedUtc = _clock.UtcNow;
            _logger.LogInformation("Storing activity...");
            await _mongoDatabase.GetCollection<Activity>(nameof(Activity)).InsertOneAsync(activity);
        }

        public async Task PersistHubSpotApiDailyRequestCountAsync(int mostRecentRequestCount, DateTime activityDateTime)
        {
            var previousRequestStats =
                await _mongoDatabase
                    .GetCollection<HubSpotApiDailyRequestCountKeyValue>(nameof(HubSpotApiDailyRequestCountKeyValue))
                    .Find(kv => kv.Date == activityDateTime.Date)
                    .FirstOrDefaultAsync() ?? new HubSpotApiDailyRequestCountKeyValue();
            _logger.LogInformation($"Previous request count: {previousRequestStats.Value}");

            var toPersist = new HubSpotApiDailyRequestCountKeyValue
            {
                Value = previousRequestStats.Value + mostRecentRequestCount,
                Date = activityDateTime.Date,
                LastUpdatedUtc = _clock.UtcNow,
                TimesUpdated = ++previousRequestStats.TimesUpdated
            };

            _logger.LogInformation($"Current request count: {toPersist.Value}");

            await _mongoDatabase
                .GetCollection<HubSpotApiDailyRequestCountKeyValue>(nameof(HubSpotApiDailyRequestCountKeyValue))
                .ReplaceOneAsync(
                    filter: Builders<HubSpotApiDailyRequestCountKeyValue>.Filter.Eq(nameof(HubSpotApiDailyRequestCountKeyValue.Date), activityDateTime.Date),
                    replacement: toPersist,
                    options: new UpdateOptions { IsUpsert = true });
        }

        public async Task<List<HubSpotApiDailyRequestCountKeyValue>> GetHubSpotApiDailyRequestCountAsync() =>
            await _mongoDatabase
                .GetCollection<HubSpotApiDailyRequestCountKeyValue>(nameof(HubSpotApiDailyRequestCountKeyValue))
                .Find(getEverySingleDailyCount => true) // hack to get everything
                .ToListAsync();

        public async Task<string> GetActivityAsync(string activityId) =>
            _jsonSerializer.Serialize(
                await _mongoDatabase
                    .GetCollection<Activity>(nameof(Activity))
                    .Find(kv => kv.Id == activityId)
                    .FirstOrDefaultAsync());

        public async Task<string> GetMostRecentActivity()
        {
            var mostRecentActivity = await _mongoDatabase.GetCollection<Activity>(nameof(Activity)).Aggregate().Sort("{_id: -1}").FirstAsync();
            return await GetActivityAsync(mostRecentActivity.Id);
        }

        public async Task<List<string>> GetActivityIdsAsync(int limit) =>
            await _mongoDatabase
                .GetCollection<Activity>(nameof(Activity))
                .Find(all => true)
                .Sort("{_id: -1}")
                .Limit(limit)
                .Project(activity => activity.Id)
                .ToListAsync();
    }
}