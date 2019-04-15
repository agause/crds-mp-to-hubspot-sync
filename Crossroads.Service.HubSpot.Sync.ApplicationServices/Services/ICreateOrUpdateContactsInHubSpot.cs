using System.Threading.Tasks;
using Crossroads.Service.HubSpot.Sync.Data.HubSpot.Models.Request;
using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Dto;

namespace Crossroads.Service.HubSpot.Sync.ApplicationServices.Services
{
    public interface ICreateOrUpdateContactsInHubSpot
    {
        /// <summary>
        /// Creates and/or updates HubSpot contacts in bulk.
        /// https://developers.hubspot.com/docs/methods/contacts/batch_create_or_update
        /// </summary>
        /// <param name="hubSpotContacts">List of Ministry Platform contacts to sync to HubSpot.</param>
        /// <param name="batchSize">Number of contacts to send to HubSpot per request.</param>
        Task<BulkSyncResult> BulkSync(BulkHubSpotContact[] hubSpotContacts, ushort batchSize = 100);

        /// <summary>
        /// Creates contacts serially.
        /// https://developers.hubspot.com/docs/methods/contacts/create_contact
        /// </summary>
        Task<SerialSyncResult> SerialCreateAsync(SerialHubSpotContact[] hubSpotContacts);

        /// <summary>
        /// Updates contacts serially. Can also update email addresses.
        /// https://developers.hubspot.com/docs/methods/contacts/update_contact-by-email
        /// </summary>
        Task<SerialSyncResult> SerialUpdateAsync(SerialHubSpotContact[] hubSpotContacts);

        /// <summary>
        /// Responsible for deleting the contact record of the old email address that is not able to be updated
        /// to the "new" email address due to the fact that the email address we wish to switch to already exists.
        /// We're ok deleting the existing account b/c Ministry Platform's dp_Users.User_Name field is our source
        /// of truth, is not nullable and has a unique constraint; so the contact attempting to update to a given
        /// email address is the true owner of the account. An edge case, but I've seen this happen and would like to
        /// minimize the cruft created by this app in HubSpot.
        /// 
        /// 1) Get contact by old email address
        /// 2) Delete contact by VID (acquired by old email address)
        /// 3) Update contact in HubSpot with new email address in both the url and the post body
        /// </summary>
        Task<SerialSyncResult> ReconcileConflicts(SerialHubSpotContact[] hubSpotContacts);
    }
}