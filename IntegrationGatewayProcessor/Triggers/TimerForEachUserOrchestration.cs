using System;
using Confluent.Kafka;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.DurableTask.Client;
using IntegrationGatewayProcessor.Models;
using IntegrationGatewayProcessor.Orchestrator;

namespace IntegrationGatewayProcessor.Triggers
{
   

    public static class TimerForEachUserOrchestration
    {
        private static readonly ConsumerConfig kafkaConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "your-consumer-group-id",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        private static readonly string kafkaTopic = "your-kafka-topic";

        [Function("TimerForEachUserOrchestration")]
        public static async Task Run(
            [TimerTrigger("0 */5 * * * *")] TimerInfo myTimer,
             [DurableClient] DurableTaskClient client,
                FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("TimerForEachUserOrchestration");
            // Check active events from Kafka topic
            List<string> activeEvents = await CheckActiveEventsFromKafka(logger);

            if (activeEvents.Count > 0)
            {
                foreach (var eventId in activeEvents)
                {
                    // Deserialize the JSON string into a Input request object
                    var inputrequest = JsonConvert.DeserializeObject<FileRequestDTO>(eventId);


                    // Function input comes from the request content.
                    string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(IntegrationGatewayFileSendOrchestrator.IntegrationGatewayFileSenderOrchestrator), inputrequest);
                    // Start orchestration for each active event
                    logger.LogInformation($"Started orchestration with ID = '{instanceId}' for event ID '{eventId}'.");
                }
            }
            else
            {
                logger.LogInformation("No active events found.");
            }
        }

        private static async Task<List<string>> CheckActiveEventsFromKafka(ILogger log)
        {
            List<string> activeEvents = new List<string>();

            using (var consumer = new ConsumerBuilder<Ignore, string>(kafkaConfig).Build())
            {
                consumer.Subscribe(kafkaTopic);

                try
                {
                    while (true)
                    {
                        var kafkaMessage = consumer.Consume();
                        string message = kafkaMessage.Message.Value;
                        // Deserialize message if needed
                        // var event = JsonConvert.DeserializeObject<Event>(message);

                        // Check if event is active and add its ID to the list
                        bool isActive = CheckEventIsActive(message); // Implement this method
                        if (isActive)
                        {
                            activeEvents.Add(message); // Add event ID to the list
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    consumer.Close();
                }
                catch (Exception ex)
                {
                    log.LogError($"An error occurred while consuming messages from Kafka: {ex.Message}");
                }
            }

            return activeEvents;
        }

        private static bool CheckEventIsActive(string message)
        {
            // Implement logic to check if the event is active
            // Return true if the event is active, false otherwise
            // Example:
            // var event = JsonConvert.DeserializeObject<Event>(message);
            // return event.IsActive;
            return true;
        }
    }
}




