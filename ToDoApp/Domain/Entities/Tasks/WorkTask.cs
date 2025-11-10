using ToDoApp.Domain.Enums;

namespace ToDoApp.Domain.Entities.Tasks
{
    public class WorkTask : BaseTask
    {
        public string ProjectName { get; set; } = "";
        
        public WorkTask(string title, string description, DateTime dueDate, Priority priority, string projectName)
            : base(title, description, dueDate, priority)
        {
            ProjectName = projectName;
        }

        public WorkTask(Guid id, DateTime createdAt)
            : base(id, createdAt) { }

        public WorkTask(WorkTask other) : base(other)
        {
            ProjectName = other.ProjectName;
        }

        public override string ToString()
        {
            return base.ToString() + "\n" +
                $"Project: {ProjectName}";
        }
    }
}
