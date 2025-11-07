using ToDoApp.Interfaces;
using ToDoApp.Models.Enums;

namespace ToDoApp.Models.Tasks
{
    public abstract class BaseTask : ITask
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedAt { get; init; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; private set; }
        public Priority Priority { get; set; }

        protected BaseTask(string title, string description, DateTime dueDate, Priority priority) 
        {
            Title = title;
            Description = description;
            DueDate = dueDate;
            Priority = priority;
            CreatedAt = DateTime.Now;
            IsCompleted = false;
        }

        protected BaseTask(Guid id, DateTime createdAt)
        {
            Id = id;
            CreatedAt = createdAt;
        }

        protected BaseTask(BaseTask other)
        {
            Title = other.Title;
            Description = other.Description;
            DueDate = other.DueDate;
            Priority = other.Priority;
            CreatedAt = other.CreatedAt;
            IsCompleted = other.IsCompleted;
        }

        public void MarkAsCompleted()
        {
            IsCompleted = true;
        }

        public override string ToString()
        {
            return
                $"Title: {Title}\n" +
                $"Description: {(string.IsNullOrWhiteSpace(Description) ? "-" : Description)}\n" +
                $"Created: {CreatedAt:yyyy-MM-dd HH:mm}\n" +
                $"Due: {DueDate:yyyy-MM-dd}\n" +
                $"Priority: {Priority}\n" +
                $"Completed: {(IsCompleted ? "✅" : "❌")}";
        }
    }
}
