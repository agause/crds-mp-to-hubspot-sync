using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Crossroads.Service.HubSpot.Sync.Core.Logging;
using Crossroads.Service.HubSpot.Sync.Core.Serialization;
using Microsoft.Extensions.Logging;

namespace Crossroads.Service.HubSpot.Sync.Core.Utilities.Impl
{
    /// <summary>
    /// JSON posts data to the provided endpoint via a persistent HttpClient instance, defined at object construction.
    /// </summary>
    public class HttpClientFacade : IHttpClientFacade
    {
        private readonly HttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger<HttpClientFacade> _logger;

        public HttpClientFacade(HttpClient httpClient, IJsonSerializer jsonSerializer, ILogger<HttpClientFacade> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public async Task<HttpResponseMessage> PostAsync<TDto>(string requestUriPathAndQuery, TDto postBody)
        {
            var fullUrl = $"{_httpClient.BaseAddress}{requestUriPathAndQuery}";
            var jsonPostBody = _jsonSerializer.Serialize(postBody);

            using (_logger.BeginScope(CoreEvent.Http.Post))
            {
                _logger.LogInformation(CoreEvent.Http.Post, $"Begin POST to {fullUrl}...");
                _logger.LogInformation(CoreEvent.Http.Post, $"Post body: {jsonPostBody}");
                try
                {
                    var content = new StringContent(jsonPostBody, Encoding.UTF8, "application/json");
                    return await _httpClient.PostAsync(requestUriPathAndQuery, content);
                }
                catch (Exception exc) // network error
                {
                    Log("Exception occurred trying to reach the API endpoint. Rethrowing to ensure we abort the current operation.", fullUrl, jsonPostBody, exc);
                    throw;
                }
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUriPathAndQuery)
        {
            var fullUrl = $"{_httpClient.BaseAddress}{requestUriPathAndQuery}";
            using (_logger.BeginScope(CoreEvent.Http.Post))
            {
                _logger.LogInformation(CoreEvent.Http.Post, $"Begin GET to {fullUrl}...");
                try
                {
                    return await _httpClient.GetAsync(requestUriPathAndQuery);
                }
                catch (Exception exc) // network error
                {
                    Log("Exception occurred trying to reach the API endpoint. Rethrowing to ensure we abort the current operation.", fullUrl, string.Empty, exc);
                    throw;
                }
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUriPathAndQuery)
        {
            var fullUrl = $"{_httpClient.BaseAddress}{requestUriPathAndQuery}";
            using (_logger.BeginScope(CoreEvent.Http.Post))
            {
                _logger.LogInformation(CoreEvent.Http.Post, $"Begin DELETE to {fullUrl}...");
                try
                {
                    return await _httpClient.DeleteAsync(requestUriPathAndQuery);
                }
                catch (Exception exc) // network error
                {
                    Log("Exception occurred trying to reach the API endpoint. Rethrowing to ensure we abort the current operation.", fullUrl, string.Empty, exc);
                    throw;
                }
            }
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public async Task<TDto> GetResponseContentAsync<TDto>(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage == null) return default(TDto);

            try
            {
                return _jsonSerializer.Deserialize<TDto>(await httpResponseMessage.Content.ReadAsStringAsync());
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Exception occurred while getting content stream.");
                return default(TDto);
            }
        }

        private void Log(string message, string fullUrl, string json, Exception exc)
        {
            _logger.LogError(CoreEvent.Exception, exc, $@"{message}
url: {fullUrl}
json: {json}");
        }
    }
}