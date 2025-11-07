using ToDoApp.Models.Enums;

namespace ToDoApp.Interfaces
{
    public interface ITask
    {
        Guid Id { get; init; }
        string Title { get; set; }
        string Description { get; set; }
        DateTime CreatedAt { get; init; }
        DateTime DueDate { get; set; }
        bool IsCompleted { get; }
        Priority Priority { get; set; }

        void MarkAsCompleted ();
    }
}
