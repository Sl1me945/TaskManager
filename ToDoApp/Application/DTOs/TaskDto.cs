using ToDoApp.Domain.Enums;

namespace ToDoApp.Application.DTOs
{
    public class TaskDto
    {
        public TaskType Type { get; set; }
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
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
