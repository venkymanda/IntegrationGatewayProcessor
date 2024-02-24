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
    [DurableTask(nameof(GetTotalChunksActivity))]
    public class GetTotalChunksActivity : TaskActivity<BlobDTO, long>
    {
        private readonly ILogger<GetTotalChunksActivity> _logger;
       
        private readonly IAzureRelayServiceHelper _azureRelayServiceHelper;
        private readonly IConfiguration _configuration;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(10); // Adjust the limit as per your needs for Conttrolling Parallel execution Limit

        public GetTotalChunksActivity(
            ILogger<GetTotalChunksActivity> logger,
            IAzureRelayServiceHelper azureRelayServiceHelper,
            IConfiguration configuration)
        {
            _logger = logger;
            _azureRelayServiceHelper = azureRelayServiceHelper;
            _configuration = configuration;
        }

        public override async Task<long> RunAsync(TaskActivityContext context, BlobDTO input)
        {
            await _semaphore.WaitAsync();

            try
            {
                _logger.LogInformation("Saying hello to {name}.", input);

                
                return  _azureRelayServiceHelper.GetTotalChunks(input.BlobContainerName,input.BlobName,input.ChunkSize); ;
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
