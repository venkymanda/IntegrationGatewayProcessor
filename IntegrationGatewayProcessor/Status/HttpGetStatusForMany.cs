
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using Azure;

namespace IntegrationGatewayProcessor.Status
{
    public class HttpGetStatusForMany
    {
        /// <summary>
        /// This function retrives the status for several Orchestrators in the same Function App.
        /// </summary>
        /// <param name="req">The HttpRequestMessage which contains the GetStatusRequest data.</param>
        /// <param name="OrchestratorClient">An instance of the DurableOrchestrationClient used to start a new Orchestrator.</param>
        /// <param name="log">ILogger implementation.</param>
        /// <returns>A collection of DurableOrchestrationStatus messages.</returns>
        [Function("HttpGetStatusForMany")]
        public static async Task<IList<OrchestrationMetadata>> Run(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status")] HttpRequestData req,
          [DurableClient] DurableTaskClient client,
          FunctionContext executionContext)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var getStatusRequest =  JsonConvert.DeserializeObject<GetStatusRequest>(requestBody);


            IList<OrchestrationMetadata> resultslist = new List<OrchestrationMetadata>();

            OrchestrationQuery query =new OrchestrationQuery() 
            { 
                CreatedFrom= getStatusRequest.CreatedFrom.Value,
                CreatedTo= getStatusRequest.CreatedTo.Value,
                Statuses=getStatusRequest.StatussesToMatch

            };
            if (getStatusRequest.CreatedFrom.HasValue && getStatusRequest.StatussesToMatch.Any())
            {
               var results = client.GetAllInstancesAsync(query);
                await foreach (var page in results.AsPages())
                {
                    // enumerate through page items
                    foreach (var data in page.Values)
                    {
                        resultslist.Add(data);
                    }
                }
               
            }
            else
            {
                var results =  client.GetAllInstancesAsync();
                await foreach (var page in results.AsPages())
                {
                    // enumerate through page items
                    foreach (var data in page.Values)
                    {
                        resultslist.Add(data);
                    }
                }
            }

            return resultslist;
        }
    }
}
