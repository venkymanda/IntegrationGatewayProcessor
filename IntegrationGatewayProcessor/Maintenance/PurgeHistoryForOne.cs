using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;

namespace IntegrationGatewayProcessor.Maintenance
{
    public class PurgeHistoryForOne
    {

        [Function(nameof(PurgeHistoryForOne))]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "DELETE", Route = "purge/{instanceId}")] HttpRequestData request,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext,
            string instanceId)
        {
            PurgeResult result=await client.PurgeInstanceAsync(instanceId);
            HttpResponseData response = request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.WriteString($"Instance is Purged -{result.PurgedInstanceCount}");
            return response;
        }
    }
}
