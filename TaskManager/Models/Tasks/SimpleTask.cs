using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Models.Enums;

namespace TaskManager.Models.Tasks
{
    public class SimpleTask : BaseTask
    {
        public SimpleTask(string title, string description, DateTime dueDate, Priority priority) 
            : base(title, description, dueDate, priority) { }

        public SimpleTask(Guid id, DateTime createdAt)
            : base(id, createdAt) { }

        public SimpleTask(SimpleTask other) : base(other) { }
    }
}
