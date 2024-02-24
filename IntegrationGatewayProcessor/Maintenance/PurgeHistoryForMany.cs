using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DurableTask.Core;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask;
using IntegrationGatewayProcessor.ActivityFunctions;
using Microsoft.DurableTask;
using Microsoft.Azure.Functions.Worker;


namespace IntegrationGatewayProcessor.Maintenance
{
    public class PurgeHistoryForMany
    {
        // https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer#ncrontab-expressions
        // https://cronexpressiondescriptor.azurewebsites.net
        // {second} {minute} {hour} {day} {month} {day-of-week}
        const string EveryDayAt730AM = "0 30 7 * * *";

        [Function(nameof(PurgeHistoryForMany))]
        public async Task Run(
            [TimerTrigger(EveryDayAt730AM)] TimerInfo myTimer,
            [DurableClient] DurableTaskClient client,
            ILogger log)
        {
            try
            {
                var instancesCreatedFromDate = DateTime.Today.Subtract(TimeSpan.FromDays(9));
                var instancesCreatedToDate = DateTime.Today.Subtract(TimeSpan.FromDays(0));
                var statussesToPurge = new List<OrchestrationRuntimeStatus>();
                statussesToPurge.Add(OrchestrationRuntimeStatus.Completed);
                statussesToPurge.Add(OrchestrationRuntimeStatus.Terminated);
            
                var purgefilter = new PurgeInstancesFilter()
                {
                    CreatedFrom = instancesCreatedFromDate,
                    CreatedTo = instancesCreatedToDate,
                    Statuses = statussesToPurge
                };

                var purgeResult = await client.PurgeAllInstancesAsync(purgefilter);

                log.LogInformation($"Purged {purgeResult.PurgedInstanceCount} instances.");
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    // Handle each inner exception appropriately
                    Console.WriteLine($"Inner exception: {ex.Message}");
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}
