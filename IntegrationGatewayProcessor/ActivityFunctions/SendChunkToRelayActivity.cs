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
    [DurableTask(nameof(SendChunkToRelayActivity))]
    public class SendChunkToRelayActivity : TaskActivity<string, bool>
    {
        private readonly ILogger logger;
        private readonly IAzureRelaySenderService _azureRelaySenderService;
        private readonly IAzureRelayServiceHelper _azureRelayServiceHelper;

        private static SemaphoreSlim semaphore = new SemaphoreSlim(10); // Adjust the limit as per your needs for Conttrolling Parallel execution Limit


        public SendChunkToRelayActivity(ILogger<SendChunkToRelayActivity> logger,IAzureRelayServiceHelper azureRelayServiceHelper,IAzureRelaySenderService azureRelaySenderService) // activites have access to DI.
        {
            this.logger = logger;
            _azureRelaySenderService = azureRelaySenderService;
            _azureRelayServiceHelper = azureRelayServiceHelper;
        }


        public async override Task<bool> RunAsync(TaskActivityContext context, string input)
        {
            await semaphore.WaitAsync();

            try
            {

                logger.LogInformation("Saying hello to {name}.", input);

                await _azureRelaySenderService.SendFileAsync(input);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {
                semaphore.Release();
            }
        }
    }



}
