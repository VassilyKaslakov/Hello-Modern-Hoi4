using System.IO;
using System.Text;
using System.Text.Json;
using Hoi4Launcher.Models;

namespace Hoi4Launcher.Services;

public static class ModConfigWriter
{
    public static string DlcLoadPath(GameInfo game) => Path.Combine(game.UserDir, "dlc_load.json");

    public static HashSet<string> ReadEnabled(GameInfo game)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var path = DlcLoadPath(game);
        if (!File.Exists(path)) return result;

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            if (!doc.RootElement.TryGetProperty("enabled_mods", out var mods) || mods.ValueKind != JsonValueKind.Array)
                return result;

            foreach (var item in mods.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String) continue;
                var value = item.GetString();
                if (!string.IsNullOrEmpty(value)) result.Add(value);
            }
        }
        catch
        {
            // dlc_load.json 损坏就当没有启用任何 MOD
        }

        return result;
    }

    public static void Save(GameInfo game, IEnumerable<ModEntry> selected)
    {
        Directory.CreateDirectory(game.UserDir);
        var path = DlcLoadPath(game);

        var disabledDlcs = new List<string>();
        if (File.Exists(path))
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                if (doc.RootElement.TryGetProperty("disabled_dlcs", out var dlcs) && dlcs.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in dlcs.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.String) continue;
                        var value = item.GetString();
                        if (!string.IsNullOrEmpty(value)) disabledDlcs.Add(value);
                    }
                }
            }
            catch
            {
                // 原文件坏了也继续写新的
            }

            var backup = path + ".launcher-backup";
            if (!File.Exists(backup))
            {
                try { File.Copy(path, backup); } catch { }
            }
        }

        var enabled = selected
            .Select(m => m.RelativePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var payload = new
        {
            enabled_mods = enabled,
            disabled_dlcs = disabledDlcs
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }
}
