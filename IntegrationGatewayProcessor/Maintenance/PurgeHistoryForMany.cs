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
            var instancesCreatedFromDate = DateTime.Today.Subtract(TimeSpan.FromDays(90));
            var instancesCreatedToDate = DateTime.Today.Subtract(TimeSpan.FromDays(84));
            var statussesToPurge = new List<OrchestrationRuntimeStatus> {
                OrchestrationRuntimeStatus.Completed,
                OrchestrationRuntimeStatus.Terminated,
            };
            var purgefilter = new PurgeInstancesFilter()
            {
                CreatedFrom = instancesCreatedFromDate,
                CreatedTo = instancesCreatedToDate,
                Statuses = statussesToPurge
            };

            var purgeResult = await client.PurgeAllInstancesAsync(purgefilter);

            log.LogInformation($"Purged {purgeResult.PurgedInstanceCount} instances.");
        }
    }
}
