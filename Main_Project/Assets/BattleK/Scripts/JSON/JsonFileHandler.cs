using System;
using System.IO;
using Newtonsoft.Json;

namespace BattleK.Scripts.JSON
{
    public static class JsonFileHandler
    {
        private static readonly JsonSerializerSettings DefaultSettings = new()
        {
            Formatting = Formatting.Indented,
            MissingMemberHandling  = MissingMemberHandling.Ignore,
            NullValueHandling      = NullValueHandling.Include,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static bool TryLoadJsonFile<T>(string filePath, out T data, out string message)
        {
            data = default;
            message = string.Empty;
            
            if (!File.Exists(filePath))
            {
                message = "File does not exist.";
                return false;
            }
        
            try
            {
                var json = File.ReadAllText(filePath);
                data = JsonConvert.DeserializeObject<T>(json, DefaultSettings);

                if (data == null)
                {
                    message = "data is null/empty.";
                    return false;
                }

                message = "OK";
                return true;
            }
            catch (Exception ex)
            {
                message = $"Exception: {ex.Message}";
                return false;
            }
        }

        public static bool TrySaveJsonFile<T>(string filePath, T data, out string message)
        {
            message = string.Empty;

            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if(!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
                return true;
            }
            catch (Exception ex)
            {
                message = $"Exception: {ex.Message}";
                return false;
            }
        }
        
    }
}