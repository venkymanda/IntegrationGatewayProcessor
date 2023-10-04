using IntegrationGatewayProcessor.Helpers;
using IntegrationGatewayProcessor.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationGatewayProcessor.ActivityFunctions
{
    [DurableTask(nameof(SendFileActivity))]
    public class SendFileActivity 
    {
        private readonly ILogger logger;
        private readonly IAzureRelaySenderService _azureRelaySenderService;
        private readonly IAzureRelayServiceHelper _azureRelayServiceHelper;

        public SendFileActivity(ILogger<SendFileActivity> logger,IAzureRelayServiceHelper azureRelayServiceHelper,IAzureRelaySenderService azureRelaySenderService) // activites have access to DI.
        {
            this.logger = logger;
            _azureRelaySenderService = azureRelaySenderService;
            _azureRelayServiceHelper = azureRelayServiceHelper;
        }


        [Function(nameof(SendFileActivity))]
        public  string SendFileActivityFunction([ActivityTrigger] string name, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("SendFileActivity");
            logger.LogInformation("Saying hello to {name}.", name);
            return $"Hello {name}!";
           
            _azureRelaySenderService.SendFileAsync(name);
            return null;
        }
    }



}
