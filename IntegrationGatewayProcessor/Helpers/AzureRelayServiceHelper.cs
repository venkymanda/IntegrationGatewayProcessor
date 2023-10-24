using IntegrationGatewayProcessor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationGatewayProcessor.Helpers
{
    public class AzureRelayServiceHelper : IAzureRelayServiceHelper
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public AzureRelayServiceHelper(
            ILogger<AzureRelayServiceHelper> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        public Task<Stream> GetBlobStreamAsync(string blobContainerName, string blobName)
        {
            // Implement your code to retrieve the blob stream
            // You can use Azure Blob Storage SDK or any other method to fetch the stream
            return Task.FromResult<Stream>(new MemoryStream()); // Replace with your implementation
        }

        public void ConfigureHttpClient(HttpClient httpClient)
        {
            // Configure the HttpClient for Azure Relay communication
            // Set the necessary headers, timeouts, and other settings here
        }

        public  string GetSasTokenAsync()
        {
            // Replace with your Azure Relay-specific values
            string relayUrl = _configuration["relayUrl"];
            string sasKeyName = _configuration["sasKeyName"];
            string sasKey = _configuration["sasKey"];

            if (string.IsNullOrEmpty(relayUrl) || string.IsNullOrEmpty(sasKeyName) || string.IsNullOrEmpty(sasKey))
            {
                _logger.LogError("Missing required configuration values for SAS token generation.");
                throw new InvalidOperationException("Missing required configuration values.");
            }

            // Generate the SAS token
            return CreateSaSToken(relayUrl, sasKeyName, sasKey);
        }

        private string CreateSaSToken(string resourceUri, string keyName, string key, int expireInSeconds = 3600)
        {
            TimeSpan sinceEpoch = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1));
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + expireInSeconds);
            string stringToSign = WebUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            return String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", WebUtility.UrlEncode(resourceUri), WebUtility.UrlEncode(signature), expiry, keyName);
        }
    }
}
