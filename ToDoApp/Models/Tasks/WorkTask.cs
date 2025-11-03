using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoApp.Models.Enums;

namespace ToDoApp.Models.Tasks
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
