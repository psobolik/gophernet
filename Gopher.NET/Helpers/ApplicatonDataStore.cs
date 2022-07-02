using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gopher.NET.Helpers
{
    internal static class ApplicationDataStore<T> where T : new()
    {
        public static T Read()
        {
            var filePath = GetFilePath(typeof(T));
            if (filePath == null || !File.Exists(filePath)) return new T();

            var jsonString = File.ReadAllText(filePath);
            if (jsonString == null) return new T();

            return JsonSerializer.Deserialize<T>(jsonString)!;
        }

        public static void Write(T settings)
        {
            var filePath = GetFilePath(typeof(T));
            if (filePath == null) return;

            var jsonString = JsonSerializer.Serialize(settings);
            if (jsonString == null) return;

            File.WriteAllText(filePath, jsonString);
        }

        private static string? GetFilePath(Type type)
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (root == null) return null;

            var folder = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            if (folder == null) return null;

            var path = Path.Combine(root, folder);
            if (path == null) return null;

            var file = type.Name;
            if (file == null) return null;

            Directory.CreateDirectory(path);
            return Path.Combine(path, Path.ChangeExtension(file, "json"));
        }
    }
}
