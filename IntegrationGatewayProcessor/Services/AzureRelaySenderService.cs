using IntegrationGatewayProcessor.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using IntegrationGatewayProcessor.Models;

namespace IntegrationGatewayProcessor.Services
{
    public class AzureRelaySenderService : IAzureRelaySenderService
    {
        private readonly ILogger<AzureRelaySenderService> _logger;
        private readonly IAzureRelayServiceHelper _relayServiceHelper;
        private readonly HttpClient _httpclient;

        public AzureRelaySenderService(
            ILogger<AzureRelaySenderService> logger,
            IAzureRelayServiceHelper relayServiceHelper,
            IHttpClientFactory httpClientFactory)
        {
            _logger             = logger;
            _relayServiceHelper = relayServiceHelper;
            _httpclient         = httpClientFactory.CreateClient();
        }

        public async Task<bool> SendFileAsync(BlobDTO input)
        {
            
            try
            {
                
                byte[] buffer = new byte[input.ChunkSize]; // Chunk size in bytes

                // Create a chunk from the buffer
                byte[] chunkData = input.Data;
                Array.Copy(buffer, chunkData, input.ChunkSize);

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

                requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                requestContent.Headers.Add("X-Filename", input.BlobName);
                requestContent.Headers.Add("X-ChunkSize", input.ChunkSize.ToString());
                requestContent.Headers.Add("X-ChunkSequence", input.CurrentChunkSequence.ToString());
                requestContent.Headers.Add("X-TotalChunks", input.TotalChunks.ToString());
                requestContent.Headers.Add("X-TotalSize", input.TotalSize.ToString());
                var response = await _httpclient.PostAsync($"relay-service-url/{input.CurrentChunkSequence}", requestContent);

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
