using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoApp.Models.Enums;

namespace ToDoApp.Models.Tasks
{
    public class RecurringTask : BaseTask
    {
        public TimeSpan RepeatInterval { get; set; }

        public RecurringTask(string title, string description, DateTime dueDate, Priority priority, TimeSpan repeatInterval)
            : base(title, description, dueDate, priority)
        {
            RepeatInterval = repeatInterval;
        }

        public RecurringTask(Guid id, DateTime createdAt)
            : base(id, createdAt) { }

        public RecurringTask(RecurringTask other) : base(other)
        {
            RepeatInterval = other.RepeatInterval;
        }

        public override string ToString()
        {
            return base.ToString() + "\n" +
                $"Repeat every: {RepeatInterval.Days}d {RepeatInterval.Hours}h {RepeatInterval.Minutes}m";
        }
    }
}
