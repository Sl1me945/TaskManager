using ToDoApp.Domain.Enums;

namespace ToDoApp.Domain.Entities.Tasks
{
    public class SimpleTask : BaseTask
    {
        private SimpleTask() { }
        public SimpleTask(string title, string description, DateTime dueDate, Priority priority)
            : base(title, description, dueDate, priority)
        { }

        internal SimpleTask(
            Guid id,
            DateTime createdAt,
            string title,
            string description,
            DateTime dueDate,
            Priority priority,
            bool isCompleted,
            Guid userId)
            : base(id, createdAt, title, description, dueDate, priority, isCompleted, userId)
        { }
    }
}
