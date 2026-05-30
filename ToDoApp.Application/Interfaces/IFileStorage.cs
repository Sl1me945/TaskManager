using ToDoApp.Application.Enums;

namespace ToDoApp.Application.Interfaces 
{
    public interface IFileStorage
    {
        Task SaveAsync<T>(string path, T data, FileFormat format = FileFormat.Auto);
        Task<T> LoadAsync<T>(string path, FileFormat format = FileFormat.Auto);
    }
}
