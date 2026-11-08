using System.IO;
using System.Text.Json;

namespace Hoi4Launcher.Services;

public sealed class LauncherSettings
{
    public string? GameDir { get; set; }
    public string Language { get; set; } = "";
    public string Accent { get; set; } = "#C4702F";
    public string? BackgroundImage { get; set; }
    public bool DebugMode { get; set; }
    public bool CloseAfterLaunch { get; set; }
}

public static class SettingsStore
{
    private static string ConfigDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HelloModernHoi4");

    public static string ConfigPath => Path.Combine(ConfigDir, "settings.json");

    public static LauncherSettings Load()
    {
        try
        {
            if (!File.Exists(ConfigPath)) return new LauncherSettings();

            var settings = JsonSerializer.Deserialize<LauncherSettings>(File.ReadAllText(ConfigPath));
            return settings ?? new LauncherSettings();
        }
        catch
        {
            return new LauncherSettings();
        }
    }

    public static void Save(LauncherSettings settings)
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);
            File.WriteAllText(ConfigPath,
                JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            // 配置写不进去不影响本次使用
        }
    }
}
