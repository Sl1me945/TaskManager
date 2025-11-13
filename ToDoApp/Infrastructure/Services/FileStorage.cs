using System.Text.Json;
using ToDoApp.Application.Enums;
using ToDoApp.Application.Interfaces;

namespace ToDoApp.Infrastructure.Services
{
    public class FileStorage : IFileStorage
    {
        private static FileFormat ResolveFormat(string path, FileFormat format)
        {
            if (format != FileFormat.Auto) return format;
            var ext = Path.GetExtension(path)?.ToLowerInvariant();
            return ext switch
            {
                ".json" => FileFormat.Json,
                ".xml" => FileFormat.Xml,
                ".csv" => FileFormat.Csv,
                _ => FileFormat.Json
            };
        }

        // JSON options shared across calls
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public async Task SaveAsync<T>(string path, T data, FileFormat format = FileFormat.Auto)
        {
            EnsurePath(path);
            format = ResolveFormat(path, format);

            switch (format)
            {
                case FileFormat.Json:
                    await using (var fs = File.Create(path))
                    {
                        await JsonSerializer.SerializeAsync(fs, data, JsonOptions);
                    }
                    break;

                case FileFormat.Xml:
                case FileFormat.Csv:
                default:
                    throw new NotSupportedException($"Unsupported format: {format}");
            }
        }

        public async Task<T> LoadAsync<T>(string path, FileFormat format = FileFormat.Auto)
        {
            EnsurePath(path);
            format = ResolveFormat(path, format);
            if (!File.Exists(path))
            {
                try
                {
                    // create file with valid empty JSON array to avoid empty-file deserialization issues
                    File.WriteAllText(path, "[]");
                }
                catch (Exception)
                {
                    // preserve original stack trace
                    throw;
                }
            }

            switch (format)
            {
                case FileFormat.Json:
                    await using (var fs = File.OpenRead(path))
                    {
                        try
                        {
                            return await JsonSerializer.DeserializeAsync<T>(fs, JsonOptions)
                                   ?? throw new InvalidOperationException("Deserialized JSON was null.");
                        }
                        catch (Exception)
                        {
                            // fallback to deserializing from an empty array representation
                            return JsonSerializer.Deserialize<T>("[]")
                                    ?? throw new InvalidOperationException("Failed to deserialize and fallback returned null.");
                        }
                    }

                case FileFormat.Xml:
                case FileFormat.Csv:
                default:
                    throw new NotSupportedException($"Unsupported format: {format}");
            }
        }

        private static void EnsurePath(string path)
        {
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }
    }
}