using System.Text.Json;
using System.IO;
using Microsoft.Maui.Storage;

namespace Drift_Dragon.BusinessLogic
{
    public static class JsonDataService
    {
        public static async Task<T?> LoadJsonAsync<T>(string filename)
        {
            try
            {
                // FIX: Remove "Raw/" - MAUI handles Resources/Raw automatically
                using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON Load Error: {ex.Message}");
                return default;
            }
        }
    }
}