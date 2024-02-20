using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Azure.Functions.Worker.Http;

namespace IntegrationGatewayProcessor.Maintenance
{
    public class TerminateWithCallback
    {
        [Function("TerminateWithCallback")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "POST", Route = "terminatewithcallback/{instanceId}")] HttpRequestData request,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext,
            string instanceId, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(request.Body).ReadToEndAsync();

            TerminateModel terminatemodel = JsonConvert.DeserializeObject<TerminateModel>(requestBody);


            var callrequest = new HttpRequestMessage(HttpMethod.Post, terminatemodel.CallbackUrl);
            HttpResponseMessage response;
            callrequest.Content = new StringContent(JsonConvert.SerializeObject(terminatemodel), Encoding.UTF8, "application/json");
            await client.TerminateInstanceAsync(instanceId, terminatemodel.Reason);
            using (var httpClient = new HttpClient())
            {
                response = await httpClient.SendAsync(callrequest);
                return request.CreateResponse(System.Net.HttpStatusCode.OK);
            }

        }
    }
}
