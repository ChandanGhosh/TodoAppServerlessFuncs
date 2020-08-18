using System;
using System.Collections.Generic;
using System.Text;

namespace TodoAppServerlessFuncs.Models
{
    public class Todo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
    }
}
