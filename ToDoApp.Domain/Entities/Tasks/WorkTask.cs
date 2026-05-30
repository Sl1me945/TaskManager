using ToDoApp.Domain.Enums;

namespace ToDoApp.Domain.Entities.Tasks
{
    public class WorkTask : BaseTask
    {
        private const int MaxProjectNameLength = 200;

        private string _projectName = "";

        public string ProjectName
        {
            get => _projectName;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Project name cannot be empty", nameof(value));

                if (value.Length > MaxProjectNameLength)
                    throw new ArgumentException("Project name cannot exceed 200 characters", nameof(value));

                _projectName = value;
            }
        }

        private WorkTask() { }
        public WorkTask(string title, string description, DateTime dueDate, Priority priority, string projectName)
            : base(title, description, dueDate, priority)
        {
            ProjectName = projectName;
        }

        internal WorkTask(
            Guid id,
            DateTime createdAt,
            string title,
            string description,
            DateTime dueDate,
            Priority priority,
            bool isCompleted,
            Guid userId,
            string projectName)
            : base(id, createdAt, title, description, dueDate, priority, isCompleted, userId)
        {
            _projectName = projectName;
        }

        public void UpdateProjectName(string projectName)
        {
            ProjectName = projectName;
        }
    }
}
