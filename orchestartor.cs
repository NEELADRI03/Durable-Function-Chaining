using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using durableFunction2.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace durableFunction2
{
    public static class orchestartor
    {
        [FunctionName("AzureStorageOrchestrator")]
        public static async Task<string> AzureStorageOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,ILogger log)
        {
            var details = context.GetInput<ImageN>();
            var serviceBusMessage = await context.CallActivityAsync<string>("SendMessageToServiceBusQueue", details);

            log.LogInformation(serviceBusMessage);

            return $"Done with the orchestration with Durable Context Id: {context.InstanceId}";
        }

        [FunctionName("SendMessageToServiceBusQueue")]
        public static async Task<string> SendMessageToServiceBusQueue([ActivityTrigger] ImageN details, ILogger log, ExecutionContext executionContext)
        {
            log.LogInformation($"Received event data with an uploaded cloud blob {details.Name}");

            var azureServiceBusConfig = new ConfigurationBuilder()
                .SetBasePath(executionContext.FunctionAppDirectory)
                .AddJsonFile("local.setings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables().Build();

            var serviceBusConnection = azureServiceBusConfig["AzureServiceBusConnectionString"];
            var serviceBusQueue = azureServiceBusConfig["ServiceBusQueueName"];
            string composedMessage = "";

            try
            {
                if (details != null)
                {
                    log.LogInformation($"Composing message to be sent to the queue");



                    composedMessage = $"Image Name: {details.Name} and Image size: {details.Description} bytes....created by Neeladri";



                    await using (ServiceBusClient client = new ServiceBusClient(serviceBusConnection))
                    {
                        //Create sender
                        ServiceBusSender sender = client.CreateSender(serviceBusQueue);



                        //Create message
                        ServiceBusMessage message = new ServiceBusMessage(composedMessage);



                        //Send Message to ServiceBus Queue
                        await sender.SendMessageAsync(message);
                        log.LogInformation($"Sent a message to Service Bus Queue: {serviceBusQueue}");
                        return composedMessage;
                    }
                }
                else
                    return composedMessage;
            }
            catch (Exception ex)
            {
                log.LogInformation($"Something went wrong sending the message to the queue : {serviceBusQueue}. Exception {ex.InnerException}");
                throw;
            }

        }

    }
}