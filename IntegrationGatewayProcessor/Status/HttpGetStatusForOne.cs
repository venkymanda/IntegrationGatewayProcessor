using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;

namespace IntegrationGatewayProcessor.Status
{
    public class HttpGetStatusForOne
    {
        /// <summary>
        /// This function retrives the status for one Orchestrator in the same Function App
        /// which matches the OrchestratorName parameter.
        /// </summary>
        /// <param name="req">The HttpRequestMessage which can contain input data for the Orchestrator.</param>
        /// <param name="orchestratorClient">An instance of the DurableOrchestrationClient used to start a new Orchestrator.</param>
        /// <param name="id">Orchestrator instance id to get the status for.</param>
        /// <param name="log">ILogger implementation.</param>
        /// <returns>A DurableOrchestrationStatus message.</returns>
        /// 
        [Function("HttpGetStatusForOne")]
        public static async Task<OrchestrationMetadata> Run(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status/{id}")] HttpRequestData req,
         [DurableClient] DurableTaskClient client,
         string id,
         FunctionContext executionContext)
        {
           
            OrchestrationMetadata status=new OrchestrationMetadata(id,"");

            var parameters = GetQueryStringParameters(req);
            if (parameters.hasParameters)
            {
                status = await client.GetInstancesAsync(id,parameters.showHistory);
                return status;
            }
            else
            {
                status = await client.GetInstancesAsync(id);
                return status;
            }

           
        }

        private static (bool hasParameters, bool showHistory) GetQueryStringParameters(HttpRequestData request)
        {

            bool hasParameters = request.Query.HasKeys();
            bool showHistory = false;
           
            if (hasParameters)
            {
                string showHistoryString = request.Query.Get("showHistory");
                bool.TryParse(showHistoryString, out showHistory);
                
            }

            return (hasParameters, showHistory);
        }
    }
}
