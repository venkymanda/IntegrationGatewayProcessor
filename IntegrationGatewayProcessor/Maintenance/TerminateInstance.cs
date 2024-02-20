
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationGatewayProcessor.Maintenance
{
    public class TerminateInstance
    {
        [Function(nameof(TerminateInstance))]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "POST", Route = "terminate/{instanceId}")] HttpRequestData request,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext,
            string instanceId)
        {
            var reason =  request.ReadAsString();
            await client.TerminateInstanceAsync(instanceId, reason);

            return request.CreateResponse(System.Net.HttpStatusCode.OK);
        }
    }
}
