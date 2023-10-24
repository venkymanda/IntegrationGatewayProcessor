using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DurableTask;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Threading;
using IntegrationGatewayProcessor.Models;
using Microsoft.Extensions.Configuration;

namespace IntegrationGatewayProcessor.ActivityFunctions
{
    [DurableTask(nameof(GetChunkDataActivity))]
    public class GetChunkDataActivity : TaskActivity<BlobDTO, BlobDTO>
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private static SemaphoreSlim semaphore = new SemaphoreSlim(10); // Adjust the limit as per your needs for Conttrolling Parallel execution Limit

        public GetChunkDataActivity(
            ILogger<GetChunkDataActivity> logger,
            IConfiguration configuration) // activites have access to DI.
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async override Task<BlobDTO> RunAsync(TaskActivityContext context, BlobDTO input)
        {
            // implementation here TaskActivity<string,string > is what we have to change to relevant input type object and use it later inside here

           

            await semaphore.WaitAsync();

            try
            {
                string blobContainerName = input.BlobContainerName;
                string connectionstring = _configuration["StorageConnectionString"];
                string blobName = input.BlobName;
                long currentChunkSequence = input.CurrentChunkSequence;
                int chunkSize = input.ChunkSize;
                

                var blobClient = new BlobClient(connectionstring, blobContainerName, blobName);

                long fileSize = blobClient.GetProperties().Value.ContentLength; // Get the file size from the blob properties

                // Calculate the total number of chunks
                long totalChunks = (fileSize + chunkSize - 1) / chunkSize;

                // Ensure the totalChunks is at least 1
                totalChunks = Math.Max(totalChunks, 1);

                // You can then assign the totalChunks to your input object
                input.TotalChunks = totalChunks;

                // Calculate the range for this chunk
                long startOffset = currentChunkSequence * chunkSize;
                long endOffset = startOffset + chunkSize - 1;

                if (endOffset >= blobClient.GetProperties().Value.ContentLength)
                {
                    // Adjust the endOffset to avoid reading beyond the end of the file.
                    endOffset = blobClient.GetProperties().Value.ContentLength - 1;
                    input.TotalSize = blobClient.GetProperties().Value.ContentLength;
                }

                if (startOffset > endOffset)
                {
                    // No more data to read
                    input.Data = new byte[0];
                    return input;
                }
                BlobOpenReadOptions blobOpenReadOptions = new BlobOpenReadOptions(true){ BufferSize = chunkSize ,Position=startOffset};
                using (Stream blobStream = await blobClient.OpenReadAsync(blobOpenReadOptions))
                {
                    byte[] chunkData = new byte[chunkSize];
                    int bytesRead = await blobStream.ReadAsync(chunkData, 0, chunkSize);

                    if (bytesRead > 0)
                    {
                        // Return the actual bytes read as a chunk
                        input.Data= chunkData.Take(bytesRead).ToArray();
                        return input;
                    }
                    else
                    {
                        input.Data = new byte[0];
                        return input; // No more data to read
                    }
                }
                
            }
            catch (Exception ex)
            {
                // Handle the exception, log it, and possibly retry if needed.
                // You can use context.SignalEntity to handle retries or failure handling.

                // Log the error
                // Handle the error appropriately
                throw;
            }

            finally
            {
                semaphore.Release();
            }
        }

      
    }
}
