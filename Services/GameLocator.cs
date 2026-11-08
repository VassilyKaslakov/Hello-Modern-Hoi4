using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Hoi4Launcher.Services;

public sealed class GameInfo
{
    public required string GameDir { get; init; }
    public required string ExePath { get; init; }
    public required string UserDir { get; init; }
    public string Version { get; init; } = "";
    public string RawVersion { get; init; } = "";
    public string[] ExeArgs { get; init; } = Array.Empty<string>();
    public string? SteamExe { get; init; }

    public string ModDir => Path.Combine(UserDir, "mod");
    public string SaveDir => Path.Combine(UserDir, "save games");
    public string LogDir => Path.Combine(UserDir, "logs");
}

public static class GameLocator
{
    private const string GameFolderName = "Hearts of Iron IV";
    private static readonly Regex VdfPathRegex = new("\"path\"\\s+\"([^\"]+)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RawVersionRegex = new(@"\d+\.\d+(?:\.\d+)*", RegexOptions.Compiled);

    public static GameInfo? Find(string? configuredGameDir)
    {
        var steamRoot = ReadSteamRegistry("SteamPath");

        if (!string.IsNullOrWhiteSpace(configuredGameDir))
        {
            var exe = Path.Combine(configuredGameDir, "hoi4.exe");
            if (File.Exists(exe)) return Build(configuredGameDir, exe, steamRoot);
        }

        var libraries = new List<string>();
        if (!string.IsNullOrEmpty(steamRoot))
        {
            libraries.Add(steamRoot);
            var vdf = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            if (File.Exists(vdf))
            {
                try
                {
                    foreach (Match match in VdfPathRegex.Matches(File.ReadAllText(vdf)))
                        libraries.Add(match.Groups[1].Value.Replace("\\\\", "\\"));
                }
                catch
                {
                    // 库列表损坏就算了，后面还有兜底路径
                }
            }
        }

        libraries.AddRange(new[]
        {
            @"C:\Program Files (x86)\Steam",
            @"C:\Program Files\Steam",
            @"D:\SteamLibrary", @"E:\SteamLibrary", @"F:\SteamLibrary", @"G:\SteamLibrary"
        });

        foreach (var library in libraries.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var gameDir = Path.Combine(library, "steamapps", "common", GameFolderName);
            var exe = Path.Combine(gameDir, "hoi4.exe");
            if (File.Exists(exe)) return Build(gameDir, exe, steamRoot);
        }

        return null;
    }

    private static GameInfo Build(string gameDir, string exe, string? steamRoot)
    {
        var userDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Paradox Interactive", GameFolderName);

        var version = "";
        var rawVersion = "";
        var args = new List<string> { "-gdpr-compliant" };

        var settingsPath = Path.Combine(gameDir, "launcher-settings.json");
        if (File.Exists(settingsPath))
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(settingsPath));
                var root = doc.RootElement;

                if (root.TryGetProperty("version", out var v) && v.ValueKind == JsonValueKind.String)
                    version = v.GetString() ?? "";
                if (root.TryGetProperty("rawVersion", out var rv) && rv.ValueKind == JsonValueKind.String)
                    rawVersion = rv.GetString() ?? "";

                if (root.TryGetProperty("exeArgs", out var ea) && ea.ValueKind == JsonValueKind.Array)
                {
                    var parsed = new List<string>();
                    foreach (var item in ea.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.String) continue;
                        var value = item.GetString();
                        if (!string.IsNullOrEmpty(value)) parsed.Add(value);
                    }
                    if (parsed.Count > 0) args = parsed;
                }
            }
            catch
            {
                // 官方 launcher-settings.json 读不了就退回默认参数
            }
        }

        if (string.IsNullOrEmpty(rawVersion))
        {
            var match = RawVersionRegex.Match(version);
            if (match.Success) rawVersion = match.Value;
        }

        return new GameInfo
        {
            GameDir = gameDir,
            ExePath = exe,
            UserDir = userDir,
            Version = version,
            RawVersion = rawVersion,
            ExeArgs = args.ToArray(),
            SteamExe = string.IsNullOrEmpty(steamRoot) ? null : Path.Combine(steamRoot, "steam.exe")
        };
    }

    private static string? ReadSteamRegistry(string name)
    {
        try
        {
            var value = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", name, null) as string;
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
        catch
        {
            return null;
        }
    }
}
