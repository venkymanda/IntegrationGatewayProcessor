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

namespace IntegrationGatewayProcessor.ActivityFunctions
{
    [DurableTask(nameof(GetChunkDataActivity))]
    public class GetChunkDataActivity : TaskActivity<string, byte[]>
    {
        private readonly ILogger logger;
        private static SemaphoreSlim semaphore = new SemaphoreSlim(10); // Adjust the limit as per your needs for Conttrolling Parallel execution Limit

        public GetChunkDataActivity(ILogger<GetChunkDataActivity> logger) // activites have access to DI.
        {
            this.logger = logger;
        }

        public async override Task<byte[]> RunAsync(TaskActivityContext context, string input)
        {
            // implementation here TaskActivity<string,string > is what we have to change to relevant input type object and use it later inside here

           

            await semaphore.WaitAsync();

            try
            {
                string blobContainerName = input;//Change
                string connectionstring = input;//change
                string blobName = input;//Change
                long currentChunkSequence = 1;// Change
                int chunkSize = 1;// change

                var blobClient = new BlobClient(connectionstring, blobContainerName, blobName);
                
                // Calculate the range for this chunk
                long startOffset = currentChunkSequence * chunkSize;
                long endOffset = startOffset + chunkSize - 1;

                if (endOffset >= blobClient.GetProperties().Value.ContentLength)
                {
                    // Adjust the endOffset to avoid reading beyond the end of the file.
                    endOffset = blobClient.GetProperties().Value.ContentLength - 1;
                }

                if (startOffset > endOffset)
                {
                    // No more data to read
                    return new byte[0];
                }
                BlobOpenReadOptions blobOpenReadOptions = new BlobOpenReadOptions(true){ BufferSize = chunkSize ,Position=startOffset};
                using (Stream blobStream = await blobClient.OpenReadAsync(blobOpenReadOptions))
                {
                    byte[] chunkData = new byte[chunkSize];
                    int bytesRead = await blobStream.ReadAsync(chunkData, 0, chunkSize);

                    if (bytesRead > 0)
                    {
                        // Return the actual bytes read as a chunk
                        return chunkData.Take(bytesRead).ToArray();
                    }
                    else
                    {
                        return new byte[0]; // No more data to read
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
