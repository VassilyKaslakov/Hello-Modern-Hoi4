using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using Hoi4Launcher.Models;

namespace Hoi4Launcher.Services;

public static class SaveLauncher
{
    /// <summary>把选中的存档写成 continue_game.json，随后游戏可以用 -continuelastsave 直接读档。</summary>
    public static void WriteContinueGame(GameInfo game, SaveEntry save)
    {
        Directory.CreateDirectory(game.UserDir);
        var path = Path.Combine(game.UserDir, "continue_game.json");

        if (File.Exists(path))
        {
            var backup = path + ".launcher-backup";
            if (!File.Exists(backup))
            {
                try { File.Copy(path, backup); } catch { }
            }
        }

        // 游戏写的是 ctime 风格的时间串：Mon Aug  3 09:58:06 2026
        var modified = save.Modified;
        var day = modified.Day.ToString().PadLeft(2);
        var dateText = modified.ToString("ddd MMM ", CultureInfo.InvariantCulture) + day +
                       modified.ToString(" HH:mm:ss yyyy", CultureInfo.InvariantCulture) + "\n";

        var payload = new
        {
            title = save.DisplayName,
            desc = "",
            date = dateText,
            filename = save.FileName,
            is_remote = false
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }
}
