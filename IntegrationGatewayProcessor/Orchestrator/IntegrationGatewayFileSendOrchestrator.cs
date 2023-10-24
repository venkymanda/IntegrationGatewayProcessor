using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using IntegrationGatewayProcessor.Models;

namespace IntegrationGatewayProcessor.Orchestrator
{
    public static class IntegrationGatewayFileSendOrchestrator
    {


        [Function(nameof(IntegrationGatewayFileSenderOrchestrator))]
        public static async Task<bool> IntegrationGatewayFileSenderOrchestrator(
         [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(IntegrationGatewayFileSenderOrchestrator));

          

            try
            {
                string blobContainerName = context.GetInput<string>() ?? throw new ArgumentNullException("blobContainerName");
                string blobName = context.GetInput<string>() ?? throw new ArgumentNullException("blobName");
                int chunkSize = 8192; // Chunk size in bytes

                int totalChunks = await context.CallActivityAsync<int>("GetTotalChunksActivity", new BlobDTO { BlobContainerName=blobContainerName,BlobName=blobName,ChunkSize=chunkSize});

                var tasks = new List<Task<bool>>();
                for (int currentChunkSequence = 0; currentChunkSequence < totalChunks; currentChunkSequence++)
                {
                    tasks.Add(ProcessAndSendChunkAsync(context, blobContainerName, blobName, currentChunkSequence ,chunkSize));
                }

                // Wait for all tasks to complete before continuing
                await Task.WhenAll(tasks);

                // If you need additional orchestration logic, add it here.

              

                logger.LogInformation("File transfer completed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred: {ex.Message}");
                return false;
            }
        }

        // Implement other functions and interfaces as needed

        private static async Task<bool> ProcessAndSendChunkAsync(
                                  TaskOrchestrationContext context,
                                  string blobContainerName,
                                  string blobName,
                                  int currentChunkSequence,
                                  int chunkSize)
        {
            try
            {
                BlobDTO blobDTO = new BlobDTO() { 
                    BlobContainerName = blobContainerName,
                    BlobName = blobName,
                    ChunkSize = chunkSize,
                    CurrentChunkSequence = currentChunkSequence
                
                };
                var outputDTO                 = await context.CallActivityAsync<BlobDTO>("GetChunkDataActivity", blobDTO);
                bool sentSuccessfully         = await context.CallActivityAsync<bool>("SendChunkToRelayActivity", outputDTO);
                return sentSuccessfully;
            }
            catch (Exception ex)
            {
                // Handle the exception, log it, and possibly retry if needed.
                // You can use context.SignalEntity to handle retries or failure handling.

                // Log the error
                context.SetCustomStatus($"Error processing chunk {currentChunkSequence}: {ex.Message}");

                return false; // Indicate that this task failed.
            }
        }

       

      

        //private static async Task<bool> SendChunkToRelayAsync(HttpClient httpClient, byte[] chunkData, long sequence)
        //{
        //    // Implement logic to send the chunk to Azure Relay using HttpClient
        //}

       
    }

}
