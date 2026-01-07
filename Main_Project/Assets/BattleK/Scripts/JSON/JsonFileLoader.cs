using System;
using System.IO;
using Newtonsoft.Json;

namespace BattleK.Scripts.JSON
{
    public static class JsonFileLoader
    {
        private static readonly JsonSerializerSettings DefaultSettings = new()
        {
            MissingMemberHandling  = MissingMemberHandling.Ignore,
            NullValueHandling      = NullValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        public static bool TryLoadJsonFile<T>(string filePath, out T data, out string message,
            JsonSerializerSettings settings = null)
        {
            data = default;
            message = string.Empty;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                message = "Path is empty.";
                return false;
            }

            if (!File.Exists(filePath))
            {
                message = "File does not exist.";
                return false;
            }
        
            try
            {
                var json = File.ReadAllText(filePath);
                data = JsonConvert.DeserializeObject<T>(json, settings ?? DefaultSettings);

                if (data == null)
                {
                    message = "data is null.";
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
    }
}