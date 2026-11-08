using System.IO;
using System.Text.Json;
using Hoi4Launcher.Models;

namespace Hoi4Launcher.Services;

public static class SaveScanner
{
    public static List<SaveEntry> Scan(GameInfo game)
    {
        var dir = game.SaveDir;
        if (!Directory.Exists(dir)) return new List<SaveEntry>();

        var titles = ReadContinueTitles(game);
        var result = new List<SaveEntry>();

        foreach (var file in Directory.EnumerateFiles(dir, "*.hoi4"))
        {
            try
            {
                var info = new FileInfo(file);
                var baseName = Path.GetFileNameWithoutExtension(info.Name);
                var title = titles.GetValueOrDefault(info.Name);
                if (string.IsNullOrWhiteSpace(title)) title = FormatName(baseName);

                result.Add(new SaveEntry
                {
                    FileName = info.Name,
                    FilePath = info.FullName,
                    DisplayName = title!,
                    Modified = info.LastWriteTime,
                    Size = info.Length
                });
            }
            catch
            {
                // 单个存档读不了不影响其它存档
            }
        }

        return result
            .OrderByDescending(save => save.Modified)
            .ToList();
    }

    private static Dictionary<string, string> ReadContinueTitles(GameInfo game)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var path = Path.Combine(game.UserDir, "continue_game.json");
            if (!File.Exists(path)) return map;

            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var root = doc.RootElement;

            if (root.TryGetProperty("filename", out var file) && file.ValueKind == JsonValueKind.String &&
                root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
            {
                var fileName = file.GetString();
                var titleText = title.GetString();
                if (!string.IsNullOrWhiteSpace(fileName) && !string.IsNullOrWhiteSpace(titleText))
                    map[fileName] = titleText;
            }
        }
        catch
        {
            // continue_game.json 不存在或损坏都无所谓
        }

        return map;
    }

    private static string FormatName(string baseName)
    {
        // RUS_1910_05_01_12 -> RUS · 1910.05.01
        var parts = baseName.Split('_');
        if (parts.Length >= 4 &&
            parts[1].Length == 4 && int.TryParse(parts[1], out _) &&
            int.TryParse(parts[2], out var month) &&
            int.TryParse(parts[3], out var day))
        {
            return $"{parts[0]} · {parts[1]}.{month:00}.{day:00}";
        }

        return baseName;
    }
}
