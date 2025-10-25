using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskManager.Interfaces 
{
    public enum FileFormat
    {
        Auto,
        Json,
        Xml,
        Csv
    }

    public interface IFileStorageService
    {
        Task SaveAsync<T>(string path, T data, FileFormat format = FileFormat.Auto);
        Task<T> LoadAsync<T>(string path, FileFormat format = FileFormat.Auto);
    }
}
