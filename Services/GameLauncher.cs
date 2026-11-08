using System.Diagnostics;
using System.IO;

namespace Hoi4Launcher.Services;

public static class GameLauncher
{
    public static bool IsGameRunning() => Process.GetProcessesByName("hoi4").Length > 0;

    public static bool IsSteamRunning() => Process.GetProcessesByName("steam").Length > 0;

    public static async Task EnsureSteamAsync(GameInfo game, Action<string> status)
    {
        if (IsSteamRunning()) return;

        if (string.IsNullOrEmpty(game.SteamExe) || !File.Exists(game.SteamExe))
            throw new InvalidOperationException(Loc.T("msg.steamMissing"));

        status(Loc.T("status.startingsteam"));
        Process.Start(new ProcessStartInfo(game.SteamExe) { UseShellExecute = true });

        for (var i = 0; i < 90; i++)
        {
            await Task.Delay(500);
            if (IsSteamRunning()) break;
        }

        // Steam 刚起来时还在初始化，给它一点时间
        await Task.Delay(4000);
    }

    public static Process? Launch(GameInfo game, bool debugMode, bool continueLastSave = false)
    {
        var args = new List<string>(game.ExeArgs);
        if (debugMode && !args.Contains("-debug_mode")) args.Add("-debug_mode");
        if (continueLastSave && !args.Contains("-continuelastsave")) args.Add("-continuelastsave");

        var psi = new ProcessStartInfo(game.ExePath)
        {
            WorkingDirectory = game.GameDir,
            UseShellExecute = false,
            Arguments = string.Join(" ", args.Select(Quote))
        };

        return Process.Start(psi);
    }

    private static string Quote(string arg) => arg.Contains(' ') ? $"\"{arg}\"" : arg;
}
