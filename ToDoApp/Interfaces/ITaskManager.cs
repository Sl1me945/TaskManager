using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoApp.Models;
using ToDoApp.Models.Tasks;
using ToDoApp.Interfaces;

namespace ToDoApp.Interfaces
{
    public interface ITaskManager
    {
        Task AddTaskAsync(ITask task);
        Task RemoveTaskAsync(Guid taskId);
        Task MarkAsCompletedAsync(Guid taskId);
        IEnumerable<ITask> Search(string keyword);
        IEnumerable<ITask> SortByDate(bool ascending = true);
        IEnumerable<ITask> FilterByCompletion(bool isCompleted);
    }
}
