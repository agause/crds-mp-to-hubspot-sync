
using System;

namespace Crossroads.Service.HubSpot.Sync.Data.Mongo
{
    /// <summary>
    /// Construct establishes a contract for a document-data-store-persisted key/value pair.
    /// </summary>
    /// <typeparam name="TKey">Generic parameter to represent the key in the pair.</typeparam>
    /// <typeparam name="TValue">Generic parameter to represent the value in the pair.</typeparam>
    public interface IKeyValuePair<out TKey, out TValue>
    {
        TKey Key { get; }
        TValue Value { get; }
        DateTime LastUpdatedUtc { get; set; }
    }
}