using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaskManager.Interfaces;

namespace TaskManager.Services
{
    public class FileStorageService : IFileStorageService
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
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public async Task SaveAsync<T>(string path, T data, FileFormat format = FileFormat.Auto)
        {
            format = ResolveFormat(path, format);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");

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
            format = ResolveFormat(path, format);
            if (!File.Exists(path)) throw new FileNotFoundException(path);

            switch (format)
            {
                case FileFormat.Json:
                    await using (var fs = File.OpenRead(path))
                    {
                        return await JsonSerializer.DeserializeAsync<T>(fs, JsonOptions)
                               ?? throw new InvalidOperationException("Deserialized JSON was null.");
                    }

                case FileFormat.Xml:
                case FileFormat.Csv:
                default:
                    throw new NotSupportedException($"Unsupported format: {format}");
            }
        }
    }
}
