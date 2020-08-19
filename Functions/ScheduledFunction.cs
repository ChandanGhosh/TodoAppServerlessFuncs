using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using TodoAppServerlessFuncs.Models;

namespace TodoAppServerlessFuncs.Functions
{
    // This function checks the table storage every min and delete the todos whose IsCompleted flag is set to true.
    public static class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async Task Run(
            [TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, 
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation($"Deleting the Todos which are already completed");
            // fetch the todos from the Table
            var query = todoTable.CreateQuery<TodoTableEntity>().Where(t => t.IsCompleted == true).AsTableQuery();
            var completedResults = todoTable.ExecuteQuery(query);

            var deletedCount = 0;
            foreach (var completedTodos in completedResults)
            {
                await todoTable.ExecuteAsync(TableOperation.Delete(completedTodos));
                deletedCount++;
            }
            
            log.LogInformation($"Deleted {deletedCount} items at {DateTime.Now}");
        }
    }
}
