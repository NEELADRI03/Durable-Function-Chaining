using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using durableFunction2.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;

namespace durableFunction2
{
    public class BlobTriggerClient
    {
        [FunctionName("BlobTrigger")]
        public static async Task BlobTrigger([BlobTrigger("mycontainer/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob, string name, ILogger log,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var newUploadBlob = new ImageN
            {
                Name = name,
                Description = myBlob.Length.ToString()
            };
            var instanceId = await starter.StartNewAsync("AzureStorageOrchestrator", newUploadBlob);
            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            
        }
        
    }
}
