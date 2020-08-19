using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using TodoAppServerlessFuncs.Models;

namespace TodoAppServerlessFuncs.Functions
{
    public static class QueueListener
    {
        [FunctionName("QueueListener")]
        public static async Task Run(
            [QueueTrigger("todos", Connection = "AzureWebJobsStorage")]Todo todo, 
            [Blob("todos", Connection = "AzureWebJobsStorage")] CloudBlobContainer blobContainer,
            ILogger log)
        {
            await blobContainer.CreateIfNotExistsAsync();
            var blob = blobContainer.GetBlockBlobReference($"{todo.Id}.txt");
            await blob.UploadTextAsync($"Created a new task: {todo.Description}");
            log.LogInformation($"Queue with task: {todo.Description} processed.");
        }
    }
}
