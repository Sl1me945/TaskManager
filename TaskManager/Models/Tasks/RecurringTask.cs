using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Models.Enums;

namespace TaskManager.Models.Tasks
{
    public class RecurringTask : BaseTask
    {
        public TimeSpan RecurrenceInterval { get; set; }

        public RecurringTask(string title, string description, DateTime dueDate, Priority priority, TimeSpan recurrenceInterval)
            : base(title, description, dueDate, priority)
        { 
            RecurrenceInterval = recurrenceInterval;
        }

        public RecurringTask(RecurringTask other) : base(other)
        {
            RecurrenceInterval = other.RecurrenceInterval;
        }

        public override string ToString()
        {
            return base.ToString() + "\n" +
                $"Repeat every: {RecurrenceInterval.Days}d {RecurrenceInterval.Hours}h {RecurrenceInterval.Minutes}m";
        }
    }
}
