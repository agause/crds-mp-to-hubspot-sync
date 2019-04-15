﻿using Crossroads.Service.HubSpot.Sync.Core.Serialization;
using Crossroads.Service.HubSpot.Sync.Core.Time;
using Crossroads.Service.HubSpot.Sync.Core.Utilities;
using Crossroads.Service.HubSpot.Sync.Data.HubSpot.Models.Request;
using Crossroads.Service.HubSpot.Sync.Data.HubSpot.Models.Response;
using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing;
using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Dto;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Crossroads.Service.HubSpot.Sync.ApplicationServices.Services.Impl
{
    public class CreateOrUpdateContactsInHubSpot : ICreateOrUpdateContactsInHubSpot
    {
        private const ushort MinBatchSize = 10;

        /// <summary>per HubSpot documentation: https://developers.hubspot.com/docs/methods/contacts/batch_create_or_update </summary>
        private const ushort DefaultBatchSize = 100;

        /// <summary>per HubSpot documentation: https://developers.hubspot.com/docs/methods/contacts/batch_create_or_update </summary>
        private const ushort MaxBatchSize = 1000;

        private readonly IHttpClientFacade _http;
        private readonly IClock _clock;
        private readonly IJsonSerializer _serializer;
        private readonly ISleep _sleeper;
        private readonly string _hubSpotApiKey;
        private readonly ILogger<CreateOrUpdateContactsInHubSpot> _logger;

        public CreateOrUpdateContactsInHubSpot(IHttpClientFacade http, IClock clock, IJsonSerializer serializer, ISleep sleeper, string hubSpotApiKey, ILogger<CreateOrUpdateContactsInHubSpot> logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _sleeper = sleeper ?? throw new ArgumentNullException(nameof(sleeper));
            _hubSpotApiKey = hubSpotApiKey ?? throw new ArgumentNullException(nameof(hubSpotApiKey));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<BulkSyncResult> BulkSync(BulkHubSpotContact[] hubSpotContacts, ushort batchSize = DefaultBatchSize)
        {
            batchSize = RegulateBatchSize(batchSize);
            var run = new BulkSyncResult(_clock.UtcNow)
            {
                TotalContacts = hubSpotContacts.Length,
                BatchCount = CalculateNumberOfBatches(hubSpotContacts.Length, batchSize)
            };

            try
            {
                for (int currentBatchNumber = 0; currentBatchNumber < run.BatchCount; currentBatchNumber++)
                {
                    var contactBatch = hubSpotContacts.Skip(currentBatchNumber * batchSize).Take(batchSize).ToArray(); // extract the relevant group of contacts
                    var response = await _http.PostAsync($"contacts/v1/contact/batch?hapikey={_hubSpotApiKey}", contactBatch);

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Accepted: // 202; deemed successful by HubSpot -- all in the batch were accepted
                            run.SuccessCount += contactBatch.Length;
                            _logger.LogInformation($"ACCEPTED: contact batch {currentBatchNumber + 1} of {run.BatchCount}");
                            break;
                        default: // 400, 429, etc; something went awry and NONE of the contacts were accepted
                            run.FailureCount += contactBatch.Length;
                            run.FailedBatches.Add(new BulkSyncFailure
                            {
                                Count = contactBatch.Length,
                                BatchNumber = currentBatchNumber + 1,
                                HttpStatusCode = response.StatusCode,
                                Exception = await _http.GetResponseContentAsync<HubSpotException>(response),
                                HubSpotContacts = contactBatch
                            });

                            // cast to print out the HTTP status code, just in case what's returned isn't
                            // defined in the https://stackoverflow.com/a/22645395
                            _logger.LogWarning($@"REJECTED: contact batch {currentBatchNumber + 1} of {run.BatchCount}
httpstatuscode: {(int) response.StatusCode}
More details will be available in the serial processing logs.");
                            break;
                    }

                    PumpTheBreaksEveryNRequestsToAvoid429Exceptions(currentBatchNumber + 1);
                }

                return run;
            }
            finally
            {
                run.Execution.FinishUtc = _clock.UtcNow;
            }
        }

        private static ushort RegulateBatchSize(ushort proposedBatchSize)
        {
            if(proposedBatchSize > MaxBatchSize)
                return MaxBatchSize;

            return proposedBatchSize < MinBatchSize ? MinBatchSize : proposedBatchSize;
        }

        private int CalculateNumberOfBatches(int numberOfContacts, int batchSize) => (numberOfContacts / batchSize) + (numberOfContacts % batchSize > 0 ? 1 : 0);

        /// <inheritdoc />
        public async Task<SerialSyncResult> SerialCreateAsync(SerialHubSpotContact[] hubSpotContacts)
        {
            var run = new SerialSyncResult(_clock.UtcNow) { TotalContacts = hubSpotContacts.Length };
            try
            {
                for (int currentContactIndex = 0; currentContactIndex < hubSpotContacts.Length; currentContactIndex++)
                {
                    var contact = hubSpotContacts[currentContactIndex];
                    var response = await _http.PostAsync($"contacts/v1/contact?hapikey={_hubSpotApiKey}", contact);

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK: // 200; create endpoint
                            _logger.LogInformation($"Created: contact {currentContactIndex + 1} of {hubSpotContacts.Length}");
                            run.SuccessCount++;
                            run.InsertCount++;
                            break;
                        case HttpStatusCode.Conflict: // 409; create endpoint; already exists -- when we attempt to create a contact with an email address that has already been claimed
                            run.EmailAddressesAlreadyExist.Add(contact);
                            run.EmailAddressAlreadyExistsCount++;
                            break;
                        default: // contact was rejected for create
                            await SetFailureData(run, response, contact, hubSpotContacts.Length, currentContactIndex);
                            break;
                    }

                    PumpTheBreaksEveryNRequestsToAvoid429Exceptions(currentContactIndex + 1);
                }

                return run;
            }
            finally
            {
                run.Execution.FinishUtc = _clock.UtcNow;
            }
        }

        /// <inheritdoc />
        public async Task<SerialSyncResult> SerialUpdateAsync(SerialHubSpotContact[] hubSpotContacts)
        {
            var run = new SerialSyncResult(_clock.UtcNow) { TotalContacts = hubSpotContacts.Length };
            try
            {
                for (int currentContactIndex = 0; currentContactIndex < hubSpotContacts.Length; currentContactIndex++)
                {
                    var contact = hubSpotContacts[currentContactIndex];
                    await UpdateAsync(currentContactIndex, contact, hubSpotContacts.Length, run);
                    PumpTheBreaksEveryNRequestsToAvoid429Exceptions(currentContactIndex + 1);
                }

                return run;
            }
            finally
            {
                run.Execution.FinishUtc = _clock.UtcNow;
            }
        }

        /// <inheritdoc />
        public async Task<SerialSyncResult> ReconcileConflicts(SerialHubSpotContact[] hubSpotContacts)
        {
            const int requestsPerReconciliation = 3, requestsPerSecond = 9;
            int reconciliationIteration = 1;

            var run = new SerialSyncResult(_clock.UtcNow) { TotalContacts = hubSpotContacts.Length };
            try
            {
                for (int currentContactIndex = 0; currentContactIndex < hubSpotContacts.Length; currentContactIndex++)
                {
                    var contact = hubSpotContacts[currentContactIndex];
                    var hubSpotContact = await SerialGetAsync<HubSpotVidResult>(contact, run); // HubSpot request #1
                    run = await SerialDeleteAsync(currentContactIndex, contact, hubSpotContact, hubSpotContacts.Length, run); // HubSpot request #2
                    contact.Email = contact.Properties.First(p => p.Name == "email").Value; // reset email to the existing one and re-run it
                    run = await UpdateAsync(currentContactIndex, contact, hubSpotContacts.Length, run); // HubSpot request #3

                    PumpTheBreaksEveryNRequestsToAvoid429Exceptions(reconciliationIteration * requestsPerReconciliation, requestsPerSecond);

                    if(reconciliationIteration == requestsPerReconciliation)
                        reconciliationIteration = 0; // reset for the next sleep 3 reconciliations later

                    reconciliationIteration++;
                }

                return run;
            }
            finally
            {
                run.Execution.FinishUtc = _clock.UtcNow;
            }
        }

        private async Task<SerialSyncResult> UpdateAsync(int currentContactIndex, SerialHubSpotContact hubSpotContact, int contactCount, SerialSyncResult run)
        {
            var response = await _http.PostAsync($"contacts/v1/contact/email/{hubSpotContact.Email}/profile?hapikey={_hubSpotApiKey}", hubSpotContact);

            switch (response.StatusCode)
            {
                case HttpStatusCode.NoContent: // 204; update only endpoint
                    _logger.LogInformation($"Updated: contact {currentContactIndex + 1} of {contactCount}");
                    run.SuccessCount++;
                    run.UpdateCount++;
                    break;
                case HttpStatusCode.NotFound: // 404; update only endpoint; contact does not exist
                    run.EmailAddressesDoNotExist.Add(hubSpotContact);
                    run.EmailAddressDoesNotExistCount++;
                    break;
                case HttpStatusCode.Conflict: // 409; update endpoint; already exists -- when a contact attempts to update their email address to one already claimed
                    run.EmailAddressesAlreadyExist.Add(hubSpotContact);
                    run.EmailAddressAlreadyExistsCount++;
                    break;
                default: // contact was rejected for update (application exception)
                    await SetFailureData(run, response, hubSpotContact, contactCount, currentContactIndex);
                    break;
            }

            return run;
        }

        private async Task<TDto> SerialGetAsync<TDto>(SerialHubSpotContact hubSpotContact, SerialSyncResult run)
        {
            var response = await _http.GetAsync($"contacts/v1/contact/email/{hubSpotContact.Email}/profile?hapikey={_hubSpotApiKey}&property=vid");
            run.GetCount++;
            var dto = default(TDto);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK: // 200; update only endpoint
                    _logger.LogInformation($"Retrieved: contact {hubSpotContact.Email}.\r\njson: {dto}");
                    dto = await _http.GetResponseContentAsync<TDto>(response);
                    break;
                case HttpStatusCode.NotFound: // 404; contact with supplied email address does not exist
                    _logger.LogWarning($"Not Found. Contact does not exist\r\njson: {_serializer.Serialize(hubSpotContact)}");
                    break;
                default: // could not get contact
                    _logger.LogWarning($"Exception occurred while GETting contact [{hubSpotContact.Email}]");
                    break;
            }

            return dto;
        }

        private async Task<SerialSyncResult> SerialDeleteAsync(int currentContactIndex, SerialHubSpotContact hubSpotContact, HubSpotVidResult hubSpotVidResult, int contactCount, SerialSyncResult run)
        {
            if (hubSpotVidResult == null)
                return run;

            var response = await _http.DeleteAsync($"contacts/v1/contact/vid/{hubSpotVidResult.ContactVid}?hapikey={_hubSpotApiKey}");
            run.DeleteCount++;

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK: // 200; when contact is deleted successfully
                    _logger.LogInformation($"Deleted: contact {currentContactIndex + 1} of {contactCount}");
                    break;
                case HttpStatusCode.NotFound: // 404; when the contact vid does not exist
                    _logger.LogWarning($"No contact in HubSpot to delete.\r\njson: {_serializer.Serialize(hubSpotContact)}");
                    break;
                default: // contact was rejected for update (application exception)
                    await SetFailureData(run, response, hubSpotContact, contactCount, currentContactIndex);
                    break;
            }

            return run;
        }

        private async Task SetFailureData(SerialSyncResult run, HttpResponseMessage response, IHubSpotContact hubSpotContact, int contactLength, int currentContactIndex)
        {
            run.FailureCount++;
            var failure = new SerialSyncFailure
            {
                HttpStatusCode = response.StatusCode,
                Exception = await _http.GetResponseContentAsync<HubSpotException>(response),
                HubSpotContact = hubSpotContact
            };
            run.Failures.Add(failure);
            LogContactFailure(failure, hubSpotContact, currentContactIndex, contactLength);
        }

        /// <summary>
        /// Should NEVER exceed 10 requests/sec
        /// </summary>
        private void PumpTheBreaksEveryNRequestsToAvoid429Exceptions(int requestCount, int requestThresholdInterval = 7) // spread requests out a bit to 7/s (not critical that this process be lightning fast)
        {
            if (requestThresholdInterval > 10)
                requestThresholdInterval = 10;

            if (requestCount % requestThresholdInterval == 0) 
            {
                _logger.LogInformation("Avoiding HTTP 429 start...");
                _sleeper.Sleep(1000);
                _logger.LogInformation("Avoiding HTTP 429 end.");
            }
        }

        private void LogContactFailure(IFailureDetails failure, IHubSpotContact hubSpotContact, int currentContactIndex, int contactCount)
        {
            // cast to print out the HTTP status code, just in case what's returned isn't
            // defined in the enum https://stackoverflow.com/a/22645395

            var hubSpotException = failure.Exception;
            _logger.LogWarning($@"REJECTED: contact {currentContactIndex + 1} of {contactCount}
httpstatuscode: {(int)failure.HttpStatusCode}
issue: {hubSpotException?.Message} for ({hubSpotException?.ValidationResults?.FirstOrDefault()?.Name})
error: {hubSpotException?.ValidationResults?.FirstOrDefault()?.Error}
contact: {_serializer.Serialize(hubSpotContact)}");
        }
    }
}