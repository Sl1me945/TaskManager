using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoApp.Models.Enums;

namespace ToDoApp.DTOs
{
    public enum TaskType
    {
        Simple,
        Work,
        Recurring
    }

    public class TaskDto
    {
        public TaskType Type { get; set; }
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public Priority Priority { get; set; }

        // optional fields
        public string? ProjectName { get; set; }
        public TimeSpan? RepeatInterval { get; set; }
    }
}
