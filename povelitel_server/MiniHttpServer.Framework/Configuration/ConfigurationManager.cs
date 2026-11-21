using System.Text.Json;

namespace MiniHttpServer.Framework.Configuration;

public sealed class ConfigurationManager
{
    private static readonly Lazy<ConfigurationManager> _instance = new(() => new ConfigurationManager());
    public static ConfigurationManager Instance => _instance.Value;
    public ServerSettings Settings { get; }

    private ConfigurationManager()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "settings.json");
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            Settings = JsonSerializer.Deserialize<ServerSettings>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new ServerSettings();
        }
        else
        {
            Settings = new ServerSettings();
        }
        ApplyEnvironmentOverrides();
    }

    private void ApplyEnvironmentOverrides()
    {
        var connectionString = Environment.GetEnvironmentVariable("PT_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(connectionString))
            Settings.ConnectionString = connectionString;

        var host = Environment.GetEnvironmentVariable("PT_SERVER_HOST");
        if (!string.IsNullOrWhiteSpace(host))
            Settings.Server.Host = host;

        var port = Environment.GetEnvironmentVariable("PT_SERVER_PORT");
        if (int.TryParse(port, out var p) && p > 0)
            Settings.Server.Port = p;
    }
}
