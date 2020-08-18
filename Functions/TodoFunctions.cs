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

namespace TodoAppServerlessFuncs.Functions
{
    public static class TodoFunctions
    {
        private static readonly IList<Todo> Todos = new List<Todo>();
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting all todos.");
            return new OkObjectResult(await Task.FromResult(Todos));
        }

        [FunctionName("AddTodo")]
        public static async Task<IActionResult> AddTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route ="todo")]HttpRequest req,
            ILogger logger)
        {
            logger.LogInformation("Creating a new todo");
   
            var todoCreateModel = await JsonSerializer.DeserializeAsync<TodoCreateModel>(req.Body, options);
            var todo = new Todo()
            {
                Description = todoCreateModel.Description
            };

            Todos.Add(todo);
            return new OkObjectResult(todo);
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route ="todo/{id}")]HttpRequest req,
            ILogger logger, string id)
        {
            logger.LogInformation("Updating todo");
      
            var updateModel = await JsonSerializer.DeserializeAsync<TodoUpdateModel>(req.Body, options);

            var todo = Todos.FirstOrDefault(t => t.Id == id);
            if (todo == null) return new NotFoundResult();

            todo.IsCompleted = updateModel.IsCompleted;

            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodoById")]
        public static async Task<IActionResult> GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route ="todo/{id}")]HttpRequest req,
            ILogger logger, string id)
        {
            logger.LogInformation("Get a todo by id");
            var todo = Todos.FirstOrDefault(t => t.Id == id);
            if (todo == null) return new NotFoundResult();

            return new OkObjectResult(await Task.FromResult(todo));

        }

        [FunctionName("DeleteTodo")]
        public static IActionResult DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route ="todo/{id}")]HttpRequest req,
            ILogger logger, string id)
        {
            logger.LogInformation("Delete a todo by id");

            var todo = Todos.FirstOrDefault(t => t.Id == id);
            if (todo == null) return new NotFoundResult();


            Todos.Remove(todo);
            return new OkResult();
        }
    }
}

