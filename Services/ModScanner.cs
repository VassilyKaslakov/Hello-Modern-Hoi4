using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Hoi4Launcher.Models;

namespace Hoi4Launcher.Services;

public static class ModScanner
{
    private static readonly Regex KvRegex = new(@"^\s*([A-Za-z0-9_]+)\s*=\s*""([^""]*)""", RegexOptions.Compiled);
    private static readonly Regex QuotedRegex = new("\"([^\"]*)\"", RegexOptions.Compiled);

    public static List<ModEntry> Scan(string userDir, string gameDir, string gameVersion, ISet<string> enabledRelativePaths)
    {
        var modDir = Path.Combine(userDir, "mod");
        var result = new List<ModEntry>();
        if (!Directory.Exists(modDir)) return result;

        foreach (var file in Directory.EnumerateFiles(modDir, "*.mod", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var entry = Parse(file, gameDir, gameVersion, enabledRelativePaths);
                if (entry != null) result.Add(entry);
            }
            catch
            {
                // 单个 .mod 文件损坏不影响其它 MOD
            }
        }

        return result
            .OrderByDescending(m => m.IsSelected)
            .ThenBy(m => m.Name, StringComparer.CurrentCulture)
            .ToList();
    }

    private static ModEntry? Parse(string file, string gameDir, string gameVersion, ISet<string> enabled)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var tags = new List<string>();
        var inTags = false;

        foreach (var raw in File.ReadAllLines(file))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;

            if (inTags)
            {
                if (line.Contains('}')) inTags = false;
                else CollectQuoted(line, tags);
                continue;
            }

            if (line.StartsWith("tags", StringComparison.OrdinalIgnoreCase) && line.Contains('{'))
            {
                var rest = line[(line.IndexOf('{') + 1)..];
                CollectQuoted(rest, tags);
                if (!rest.Contains('}')) inTags = true;
                continue;
            }

            if (line.StartsWith("dependencies", StringComparison.OrdinalIgnoreCase)) continue;
            if (line.StartsWith("replace_path", StringComparison.OrdinalIgnoreCase)) continue;

            var match = KvRegex.Match(line);
            if (!match.Success) continue;

            var key = match.Groups[1].Value;
            if (!values.ContainsKey(key)) values[key] = match.Groups[2].Value;
        }

        var fileName = Path.GetFileName(file);
        var name = values.GetValueOrDefault("name");
        if (string.IsNullOrWhiteSpace(name)) name = Path.GetFileNameWithoutExtension(file);

        var supported = values.GetValueOrDefault("supported_version") ?? "";
        var remoteId = values.GetValueOrDefault("remote_file_id");
        var picture = values.GetValueOrDefault("picture") ?? "thumbnail.png";

        string? modPath = null;
        if (values.TryGetValue("path", out var rawPath) && !string.IsNullOrWhiteSpace(rawPath))
        {
            var normalized = rawPath.Replace('/', '\\').Trim();
            if (!Path.IsPathRooted(normalized))
            {
                var fromGame = Path.GetFullPath(Path.Combine(gameDir, normalized));
                var fromModDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, normalized));
                normalized = Directory.Exists(fromGame) ? fromGame
                           : Directory.Exists(fromModDir) ? fromModDir
                           : fromGame;
            }
            modPath = normalized;
        }

        var exists = modPath != null && Directory.Exists(modPath);
        BitmapImage? thumbnail = null;
        if (exists)
        {
            var thumbPath = Path.Combine(modPath!, picture);
            if (!Path.IsPathRooted(thumbPath)) thumbPath = Path.Combine(modPath!, thumbPath);
            thumbnail = LoadThumbnail(thumbPath);
        }

        string level;
        string badge;
        string detail;

        if (exists)
        {
            var result = VersionCompat.Classify(supported, gameVersion);
            level = result.Level;
            badge = Loc.T(result.BadgeKey);
            detail = Loc.F(result.DetailKey, result.Args);
        }
        else
        {
            level = "missing";
            badge = Loc.T("compat.missing");
            detail = Loc.T("compat.missing.detail");
        }

        return new ModEntry
        {
            FileName = fileName,
            Name = name!,
            SupportedVersion = supported,
            ModPath = modPath,
            RemoteId = remoteId,
            Tags = tags,
            Thumbnail = thumbnail,
            CompatLevel = level,
            CompatBadge = badge,
            CompatDetail = detail,
            IsSelected = enabled.Contains("mod/" + fileName)
        };
    }

    private static void CollectQuoted(string line, List<string> target)
    {
        foreach (Match match in QuotedRegex.Matches(line))
        {
            var value = match.Groups[1].Value;
            if (value.Length > 0) target.Add(value);
        }
    }

    private static BitmapImage? LoadThumbnail(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;

            using var stream = File.OpenRead(path);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 160;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }
}
