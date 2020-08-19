using Microsoft.Azure.Cosmos.Table;
using System;

namespace TodoAppServerlessFuncs.Models
{
    public class TodoTableEntity : TableEntity
    {
        public string Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool IsCompleted { get; set; }
    }

    public static class Mappings
    {
        public static Todo MapToTodo(this TodoTableEntity todoTableEntity)
        {
            return new Todo
            {
                Id = todoTableEntity.RowKey,
                CreatedAt = todoTableEntity.CreatedAt,
                Description = todoTableEntity.Description,
                IsCompleted = todoTableEntity.IsCompleted
            };
        }

        public static TodoTableEntity MapToTableEntity(this Todo todo)
        {
            return new TodoTableEntity
            {
                PartitionKey = "TODO",
                RowKey = todo.Id,
                CreatedAt = todo.CreatedAt,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted
            };
        }
    }
}