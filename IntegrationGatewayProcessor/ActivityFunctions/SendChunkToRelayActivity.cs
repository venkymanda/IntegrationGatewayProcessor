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
using IntegrationGatewayProcessor.Models;
using Microsoft.Extensions.Configuration;

namespace IntegrationGatewayProcessor.ActivityFunctions
{
    [DurableTask(nameof(SendChunkToRelayActivity))]
    public class SendChunkToRelayActivity : TaskActivity<BlobDTO, bool>
    {
        private readonly ILogger<SendChunkToRelayActivity> _logger;
        private readonly IAzureRelaySenderService _azureRelaySenderService;
        private readonly IAzureRelayServiceHelper _azureRelayServiceHelper;
        private readonly IConfiguration _configuration;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(10); // Adjust the limit as per your needs for Conttrolling Parallel execution Limit

        public SendChunkToRelayActivity(
            ILogger<SendChunkToRelayActivity> logger,
            IAzureRelayServiceHelper azureRelayServiceHelper,
            IAzureRelaySenderService azureRelaySenderService,
            IConfiguration configuration)
        {
            _logger = logger;
            _azureRelaySenderService = azureRelaySenderService;
            _azureRelayServiceHelper = azureRelayServiceHelper;
            _configuration = configuration;
        }

        public override async Task<bool> RunAsync(TaskActivityContext context, BlobDTO input)
        {
            await _semaphore.WaitAsync();

            try
            {
                _logger.LogInformation("Saying hello to {name}.", input);

                await _azureRelaySenderService.SendFileAsync(input);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }




}
