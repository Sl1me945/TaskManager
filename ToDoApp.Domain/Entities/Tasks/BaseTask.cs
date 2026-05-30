using System.Text.Json.Serialization;
using ToDoApp.Domain.Enums;

namespace ToDoApp.Domain.Entities.Tasks
{
    public abstract class BaseTask
    {
        private const int MaxTitleLength = 200;

        private string _title = "";
        private string _description = "";

        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;
        public string Title 
        { 
            get => _title;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Title cannot be empty", nameof(value));

                if (value.Length > MaxTitleLength)
                    throw new ArgumentException("Title cannot exceed 200 characters", nameof(value));

                _title = value;
            }
        }
        public string Description
        {
            get => _description;
            private set => _description = value ?? string.Empty;
        }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime DueDate { get; private set; }
        public bool IsCompleted { get; private set; }
        public Priority Priority { get; private set; }

        protected BaseTask() { }

        protected BaseTask(string title, string description, DateTime dueDate, Priority priority)
        {
            Title = title;
            Description = description;
            SetDueDate(dueDate);
            Priority = priority;
        }

        protected BaseTask(
            Guid id,
            DateTime createdAt,
            string title,
            string description,
            DateTime dueDate,
            Priority priority,
            bool isCompleted,
            Guid userId)
        {
            Id = id;
            CreatedAt = createdAt;
            Title = title;
            Description = description;
            DueDate = dueDate;
            Priority = priority;
            IsCompleted = isCompleted;
            UserId = userId;
        }
        public void UpdateTitle(string newTitle)
        {
            Title = newTitle;
        }

        public void UpdateDescription(string newDescription)
        {
            Description = newDescription;
        }

        public void SetDueDate(DateTime dueDate)
        {
            if (dueDate < CreatedAt)
                throw new ArgumentException("Due date cannot be in the past", nameof(dueDate));

            DueDate = dueDate;
        }

        public void SetPriority(Priority priority)
        {
            Priority = priority;
        }

        public void AssignToUser(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            UserId = userId;
        }

        public void MarkAsCompleted()
        {
            IsCompleted = true;
        }

        public void MarkAsNotCompleted()
        {
            IsCompleted = false;
        }
    }
}
