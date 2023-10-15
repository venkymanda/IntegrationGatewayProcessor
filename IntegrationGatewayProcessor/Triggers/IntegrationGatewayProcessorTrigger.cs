using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Threading;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using IntegrationGatewayProcessor.Orchestrator;

namespace IntegrationGatewayProcessor.Triggers
{
    public static class IntegrationGatewayProcessorTrigger
    {



        [Function("IntegrationGatewayProcessorFileSenderTrigger")]
        public static async Task<HttpResponseData> HttpStart(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("IntegrationGatewayProcessorFileSenderTrigger");

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(IntegrationGatewayFileSendOrchestrator.IntegrationGatewayFileSenderOrchestrator));

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
