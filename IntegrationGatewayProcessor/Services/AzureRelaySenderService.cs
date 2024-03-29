﻿using IntegrationGatewayProcessor.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IntegrationGatewayProcessor.Models;
using Microsoft.Extensions.Configuration;
using Polly;

namespace IntegrationGatewayProcessor.Services
{
    public class AzureRelaySenderService : IAzureRelaySenderService
    {
        private readonly ILogger<AzureRelaySenderService> _logger;
        private readonly IAzureRelayServiceHelper _relayServiceHelper;
        private readonly HttpClient _httpclient;
        private readonly IConfiguration _configuration;

        public AzureRelaySenderService(
            ILogger<AzureRelaySenderService> logger,
            IAzureRelayServiceHelper relayServiceHelper,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _logger             = logger;
            _relayServiceHelper = relayServiceHelper;
            _httpclient         = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        public async Task<bool> SendFileAsync(BlobDTO input)
        {
            
            try
            {
                
                byte[] buffer = new byte[input.ChunkSize]; // Chunk size in bytes

                // Create a chunk from the buffer
                byte[] chunkData = input.Data;
                Array.Copy(chunkData, buffer, chunkData.Length);

                // Compress the chunk (implement your compression logic)
                byte[] compressedChunk = Compress(buffer);

                // Send the chunk to the Azure Relay service
                if (!await SendChunkToRelayAsync(compressedChunk, input))
                {
                    _logger.LogError($"Failed to send chunk {input.CurrentChunkSequence}.");
                    return false;
                }

                _logger.LogInformation($"Chunk {input.CurrentChunkSequence} sent successfully.");
                        
                

                _logger.LogInformation("File transfer completed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return false;
            }
        }


        private async Task<bool> SendChunkToRelayAsync( byte[] compressedChunk, BlobDTO input)
        {
            try
            {
                // Create a request to send the compressed chunk to the Azure Relay service
                var requestContent = new ByteArrayContent(compressedChunk);

                var retryPolicy = Policy
                        .Handle<HttpRequestException>()
                        .Or<TimeoutException>()
                        .Retry(3, (exception, retryCount) =>
                        {
                            // Log the exception and retry count
                            _logger.LogWarning($"Retry {retryCount} due to {exception}");
                        });

                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                string sasToken =  _relayServiceHelper.GetSasTokenAsync(); // Get the SAS token from the helper

                _httpclient.DefaultRequestHeaders.Remove("Authorization");
                _httpclient.DefaultRequestHeaders.Add("Authorization", sasToken);
                requestContent.Headers.Add("X-Filename", input.BlobName);
                requestContent.Headers.Add("X-ChunkSize", input.ChunkSize.ToString());
                requestContent.Headers.Add("X-ChunkSequence", input.CurrentChunkSequence.ToString());
                requestContent.Headers.Add("X-TotalChunks", input.TotalChunks.ToString());
                requestContent.Headers.Add("X-TotalSize", input.TotalSize.ToString());
                requestContent.Headers.Add("X-TransactionID", input.TransactionId);

                //These Two are Default Heders which need to be added for all Future flows as well
                #region Default Headers
                requestContent.Headers.Add("X-InputRequest", input.InputRequest);
                requestContent.Headers.Add("X-RequestType", RequestType.UploadFile.ToString());
                #endregion


                var response = await retryPolicy.Execute(() =>  _httpclient.PostAsync($"{_configuration["RelayURL"]}", requestContent));


                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while sending chunk {input.CurrentChunkSequence}: {ex.Message}");
                return false;
            }
        }

        // Implement your compression logic here
        private static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    gzipStream.Write(data, 0, data.Length);
                }
                return compressedStream.ToArray();
            }
        }
    }
}
