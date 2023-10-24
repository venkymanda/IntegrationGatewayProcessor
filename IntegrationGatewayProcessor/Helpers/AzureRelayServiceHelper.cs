using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
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
using Azure.Core;

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

        public   int GetTotalChunks(string blobContainerName, string blobName, int chunkSize)
        {
            // Implement the logic to calculate the total number of chunks based on file size and chunk size.
            // You may need to interact with Azure Blob Storage to determine the file size.
            // Return the total number of chunks.
            // You'll need to interact with Azure Blob Storage to determine the file size.
            // Replace "<YourConnectionString>" with your Azure Blob Storage connection string.

            string storageconnectionstring = _configuration["storageconnectionstring"];

            if (string.IsNullOrEmpty(storageconnectionstring))
            {
                _logger.LogError("Missing required configuration values for Storage Connection.");
                throw new InvalidOperationException("Missing required configuration values.");
            }

            // Provide the client configuration options for connecting to Azure Blob Storage
            BlobClientOptions blobOptions = new BlobClientOptions()
            {
                Retry = {
                        Delay = TimeSpan.FromSeconds(2),
                        MaxRetries = 5,
                        Mode = RetryMode.Exponential,
                        MaxDelay = TimeSpan.FromSeconds(10),
                        NetworkTimeout = TimeSpan.FromSeconds(100)
                    },
            };

            var blobServiceClient =  new BlobServiceClient(storageconnectionstring,blobOptions);

            // Get a reference to the container
            var containerClient =  blobServiceClient.GetBlobContainerClient(blobContainerName);

            // Get a reference to the blob
            var blobClient = containerClient.GetBlobClient(blobName);

            // Get the blob's properties to obtain its size
            BlobProperties blobProperties =  blobClient.GetProperties();

            long fileSize = blobProperties.ContentLength;

            // Calculate the total number of chunks
            int totalChunks = (int)Math.Ceiling((double)fileSize / chunkSize);

            // Ensure the totalChunks is at least 1
            totalChunks = Math.Max(totalChunks, 1);

            return totalChunks;
        }
    }
}
