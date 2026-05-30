using ToDoApp.Domain.Enums;

namespace ToDoApp.Domain.Entities.Tasks
{
    public class RecurringTask : BaseTask
    {
        private TimeSpan _repeatInterval;

        public TimeSpan RepeatInterval
        {
            get => _repeatInterval;
            private set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentException("Repeat interval must be positive", nameof(value));

                if (value > TimeSpan.FromDays(365))
                    throw new ArgumentException("Repeat interval cannot exceed 1 year", nameof(value));

                _repeatInterval = value;
            }
        }

        private RecurringTask() { }

        public RecurringTask(string title, string description, DateTime dueDate, Priority priority, TimeSpan repeatInterval)
            : base(title, description, dueDate, priority)
        {
            RepeatInterval = repeatInterval;
        }

        internal RecurringTask(
            Guid id,
            DateTime createdAt,
            string title,
            string description,
            DateTime dueDate,
            Priority priority,
            bool isCompleted,
            Guid userId,
            TimeSpan repeatInterval)
            : base(id, createdAt, title, description, dueDate, priority, isCompleted, userId)
        {
            RepeatInterval = repeatInterval;
        }

        public void UpdateRepeatInterval(TimeSpan repeatInterval)
        {
            RepeatInterval = repeatInterval;
        }
    }
}
