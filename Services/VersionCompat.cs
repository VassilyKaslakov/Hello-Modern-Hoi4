namespace Hoi4Launcher.Services;

public readonly record struct CompatResult(string Level, string BadgeKey, string DetailKey, object[] Args);

public static class VersionCompat
{
    /// <summary>把 mod 的 supported_version 和游戏版本对比，返回（等级, 徽章 key, 说明 key, 参数）。</summary>
    public static CompatResult Classify(string supported, string gameVersion)
    {
        var s = (supported ?? "").Trim();
        var g = (gameVersion ?? "").Trim();

        if (s.Length == 0)
            return new CompatResult("unknown", "compat.unknown", "compat.unknown.detail", new object[] { g });
        if (g.Length == 0)
            return new CompatResult("unknown", "compat.unknown", "compat.nogame.detail", new object[] { s });

        if (s == "*" || s.EndsWith(".*"))
        {
            var prefix = s == "*" ? "" : s[..^1]; // 保留结尾的点，例如 "1.19."
            if (g.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return new CompatResult("ok", "compat.ok", "compat.wildcard.ok", new object[] { s, g });

            return new CompatResult("old", "compat.old", "compat.wildcard.bad", new object[] { s, g });
        }

        var sp = s.Split('.');
        var gp = g.Split('.');
        var n = Math.Min(sp.Length, gp.Length);

        var allMatch = true;
        for (var i = 0; i < n; i++)
        {
            if (string.Equals(sp[i], gp[i], StringComparison.OrdinalIgnoreCase)) continue;
            allMatch = false;
            break;
        }

        var exactLength = sp.Length == gp.Length;
        var sameMajorMinor = sp.Length >= 2 && gp.Length >= 2 &&
                             sp[0] == gp[0] && sp[1] == gp[1];

        if (allMatch && exactLength)
            return new CompatResult("ok", "compat.ok", "compat.exact.ok", new object[] { s, g });
        if (allMatch)
            return new CompatResult("patch", "compat.patch", "compat.prefix.detail", new object[] { s, g });
        if (sameMajorMinor)
            return new CompatResult("patch", "compat.patch", "compat.patch.detail", new object[] { s, g });

        return new CompatResult("old", "compat.old", "compat.old.detail", new object[] { s, g });
    }
}
