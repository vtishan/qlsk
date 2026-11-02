using System.Text.Json;

namespace QLSK.Setup;

public static class DevelopmentSettings
{
    // Only called for an interactive terminal; automation must specify its target explicitly.
    public static string? FindConnectionString(string startDirectory)
    {
        for (var directory = new DirectoryInfo(startDirectory); directory is not null; directory = directory.Parent)
        {
            var settingsPath = Path.Combine(directory.FullName, "appsettings.Development.json");
            if (!File.Exists(Path.Combine(directory.FullName, "QLSK.csproj")) || !File.Exists(settingsPath)) continue;
            using var settings = JsonDocument.Parse(File.ReadAllText(settingsPath));
            if (settings.RootElement.TryGetProperty("ConnectionStrings", out var connections) &&
                connections.TryGetProperty("Default", out var connection) && connection.ValueKind == JsonValueKind.String)
            {
                var value = connection.GetString();
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }
            return null;
        }
        return null;
    }
}
