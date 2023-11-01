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
using Newtonsoft.Json;

namespace IntegrationGatewayProcessor.Orchestrator
{
    public static class IntegrationGatewayFileSendOrchestrator
    {


        /// <summary>
        /// The main orchestrator function that handles the file transfer process.
        /// </summary>
        /// <param name="context">The orchestration context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Function(nameof(IntegrationGatewayFileSenderOrchestrator))]
        public static async Task<bool> IntegrationGatewayFileSenderOrchestrator(
         [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(IntegrationGatewayFileSenderOrchestrator));

            InputRequestDTO inputRequestDTO = context.GetInput<InputRequestDTO>() ?? throw new ArgumentNullException(nameof(inputRequestDTO));
            string inputRequestJson = JsonConvert.SerializeObject(inputRequestDTO);



            try
            {
               
                string blobContainerName = inputRequestDTO.BlobContainerName?? throw new ArgumentNullException(nameof(blobContainerName));
                string blobName = inputRequestDTO.BlobName ?? throw new ArgumentNullException(nameof(blobName));
                int chunkSize = 8192; // Chunk size in bytes

                int totalChunks = await context.CallActivityAsync<int>("GetTotalChunksActivity", new BlobDTO { BlobContainerName=blobContainerName,BlobName=blobName,ChunkSize=chunkSize});

                var tasks = Enumerable.Range(0, totalChunks)
                                    .Select(currentChunkSequence => 
                                            ProcessAndSendChunkAsync(context, blobContainerName, blobName, currentChunkSequence ,chunkSize,inputRequestDTO.TransactionId,inputRequestJson))
                                    .ToList();


                // Wait for all tasks to complete before continuing
                await Task.WhenAll(tasks);

                // If you need additional orchestration logic, add it here.

                // After the main logic, add the callback URL and payload

                var successPayload = new SuccessCallbackDTO
                {
                    Status = "success",
                    Message = "File transfer completed successfully",
                    ProcessedChunks = totalChunks,
                    TotalBytesProcessed = totalChunks*chunkSize,
                    TransactionId = inputRequestDTO.TransactionId,
                    DocumentId = $"{inputRequestDTO.BlobContainerName}\\{inputRequestDTO.BlobName}"
                };

                string callbackPayload = JsonConvert.SerializeObject(successPayload); // You may need to use a JSON library, like Newtonsoft.Json

                string callbackUrl = inputRequestDTO.CallbackURL;
                

                // Make the HTTP callback at the end of execution
                var callbackstatus=(inputRequestDTO.DoCallBack)?await MakeHttpCallbackAsync(callbackUrl, callbackPayload):true;

                // If you reach this point, it means the execution was successful
                logger.LogInformation("File transfer completed successfully.");
                return callbackstatus;



            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred: {ex.Message}");
                // Make the HTTP callback in case of error
                var failurePayload = new FailureCallbackDTO
                {
                    Status = "failure",
                    ErrorMessage = "An error occurred during file transfer",
                    ErrorDetails = ex.ToString(),
                    TransactionId = inputRequestDTO.TransactionId,
                    DocumentId = $"{inputRequestDTO.BlobContainerName}\\{inputRequestDTO.BlobName}"
                };

                string payloadJson = JsonConvert.SerializeObject(failurePayload); // You may need to use a JSON library, like Newtonsoft.Json

                if (inputRequestDTO.DoCallBack) 
                    await MakeHttpCallbackAsync(inputRequestDTO.CallbackURL, payloadJson);

                return false; // Indicate that this task failed.

            }
        }

        // Implement other functions and interfaces as needed

        private static async Task<bool> ProcessAndSendChunkAsync(
                                  TaskOrchestrationContext context,
                                  string blobContainerName,
                                  string blobName,
                                  int currentChunkSequence,
                                  int chunkSize,
                                  string transactionid,
                                  string inputRequestJson)
        {
            try
            {
                BlobDTO blobDTO = new BlobDTO() { 
                    BlobContainerName = blobContainerName,
                    BlobName = blobName,
                    ChunkSize = chunkSize,
                    CurrentChunkSequence = currentChunkSequence,
                    TransactionId = transactionid,
                    InputRequest=inputRequestJson
                
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

       
       

    public static async Task<bool> MakeHttpCallbackAsync(string callbackUrl, string payload)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(callbackUrl, content);
            return response.IsSuccessStatusCode;
        }
    }



    //private static async Task<bool> SendChunkToRelayAsync(HttpClient httpClient, byte[] chunkData, long sequence)
    //{
    //    // Implement logic to send the chunk to Azure Relay using HttpClient
    //}


}

}
