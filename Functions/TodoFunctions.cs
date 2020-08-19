using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TodoAppServerlessFuncs.Models;
using System.Text.Json;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;

namespace TodoAppServerlessFuncs.Functions
{
    public static class TodoFunctions
    {
        
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [Table("todos", "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Getting all todos.");
            var query = new TableQuery<TodoTableEntity>();
            var segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(Mappings.MapToTodo));
        }

        [FunctionName("GetTodoById")]
        public static async Task<IActionResult> GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoTableEntity todoTableEntity,
            ILogger logger, string id)
        {
            logger.LogInformation("Get a todo by id");

            if (todoTableEntity == null) return new NotFoundResult();

            return new OkObjectResult(await Task.FromResult(todoTableEntity.MapToTodo()));

        }

        [FunctionName("AddTodo")]
        public static async Task<IActionResult> AddTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route ="todo")]HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] IAsyncCollector<TodoTableEntity> todoTable,
            [Queue("todos", Connection ="AzureWebJobsStorage")] IAsyncCollector<Todo> todoQueue,
            ILogger logger)
        {
            logger.LogInformation("Creating a new todo");
   
            var todoCreateModel = await JsonSerializer.DeserializeAsync<TodoCreateModel>(req.Body, options);
            var todo = new Todo()
            {
                Description = todoCreateModel.Description
            };

            await todoTable.AddAsync(todo.MapToTableEntity());
            logger.LogInformation("Added todo to the table storage");
            await todoQueue.AddAsync(todo);
            logger.LogInformation("Added todo to the storage queue");
            return new OkObjectResult(todo);
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route ="todo/{id}")]HttpRequest req,
            [Table("todos", "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger logger, string id)
        {
            logger.LogInformation("Updating todo");
      
            var updateModel = await JsonSerializer.DeserializeAsync<TodoUpdateModel>(req.Body, options);

            var findOperation = TableOperation.Retrieve<TodoTableEntity>("TODO", id);
            var findResult = await todoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null) return new NotFoundResult();

            TodoTableEntity existingTodo = (TodoTableEntity)findResult.Result;
            existingTodo.IsCompleted = updateModel.IsCompleted;

            var replaceOperation = TableOperation.Replace(existingTodo);
            await todoTable.ExecuteAsync(replaceOperation);

            return new OkObjectResult(existingTodo.MapToTodo());
        }      

        [FunctionName("DeleteTodo")]
        public static async Task<IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route ="todo/{id}")]HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger logger, string id)
        {
            logger.LogInformation("Delete a todo by id");
            //ETag helps in optimistic concurrency checks. Here we are simply deleting not checking for data consistency though for simplicity.
            var deleteOperation = TableOperation.Delete(new TableEntity() { PartitionKey = "TODO", RowKey = id, ETag = "*" }); 

            try
            {
                var deleteResult = await todoTable.ExecuteAsync(deleteOperation);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {

                return new NotFoundResult();
            }
           
            return new OkResult();
        }
    }
}

