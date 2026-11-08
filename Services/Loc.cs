using System.Collections.Generic;
using System.Windows;

namespace Hoi4Launcher.Services;

public static class Loc
{
    public const string Zh = "zh-CN";
    public const string En = "en-US";

    public static string Current { get; private set; } = Zh;

    private static readonly Dictionary<string, Dictionary<string, string>> Tables = new()
    {
        [Zh] = new Dictionary<string, string>
        {
            ["app.subtitle"] = "钢铁雄心 IV 启动器 · hmhoi4",
            ["btn.workshop"] = "创意工坊",
            ["tip.workshop"] = "打开 Steam 创意工坊，订阅 / 下载 MOD",
            ["btn.rescan"] = "重新扫描",

            ["nav.launch"] = "启动游戏",
            ["nav.settings"] = "设置",
            ["nav.personalize"] = "个性化",
            ["sidebar.info"] = "游戏信息",

            ["mods.title"] = "MOD 列表",
            ["search.placeholder"] = "搜索 MOD…",
            ["btn.selectall"] = "全选",
            ["btn.selectnone"] = "全不选",
            ["btn.invert"] = "反选",
            ["details.placeholder"] = "点击左侧的 MOD 查看详情",
            ["details.placeholder2"] = "双击卡片可快速启用 / 禁用",
            ["launch.enabled"] = "当前启用",
            ["btn.launch"] = "启动游戏",
            ["chk.debug"] = "调试模式（-debug_mode）",
            ["chk.close"] = "启动后关闭启动器",
            ["detail.tags"] = "标签",
            ["detail.workshop"] = "工坊",
            ["detail.path"] = "路径",
            ["btn.opensteamworkshop"] = "在 Steam 工坊打开",
            ["btn.openfolder"] = "在资源管理器中打开",

            ["settings.language.title"] = "语言",
            ["settings.language.desc"] = "切换界面语言后会重新扫描一次 MOD。",
            ["settings.game.title"] = "游戏位置",
            ["settings.game.desc"] = "启动器会读取游戏目录下的 launcher-settings.json 来获取版本号和启动参数。",
            ["label.gamedir"] = "游戏目录",
            ["label.userdir"] = "数据目录",
            ["label.moddir"] = "MOD 目录",
            ["label.savedir"] = "存档目录",
            ["btn.choose"] = "选择…",
            ["btn.open"] = "打开",
            ["btn.autodetect"] = "自动检测游戏",
            ["hint.autodetect"] = "清除手动指定的目录，重新扫描 Steam 库。",
            ["settings.launch.title"] = "启动选项",
            ["settings.launch.desc"] = "启动前会把勾选的 MOD 写入 dlc_load.json（原文件首次启动时备份为 dlc_load.json.launcher-backup），随后直接运行 hoi4.exe，不经过 Paradox Launcher。",
            ["settings.steam.title"] = "Steam",
            ["steam.running"] = "Steam 正在运行",
            ["steam.notrunning"] = "Steam 未运行，启动游戏时会自动拉起",
            ["btn.refresh"] = "刷新状态",
            ["hint.steam"] = "Steam 没运行时会先自动拉起 Steam，等它初始化完再启动游戏。",
            ["settings.shortcuts.title"] = "快捷操作",
            ["btn.opengame"] = "打开游戏目录",
            ["btn.openmods"] = "打开 MOD 目录",
            ["btn.opensaves"] = "打开存档目录",
            ["btn.openlogs"] = "打开日志目录",
            ["btn.openworkshop"] = "打开创意工坊",
            ["settings.about.title"] = "关于",
            ["about.line"] = "Hello Modern Hoi4 v1.0 · 钢铁雄心 IV 独立启动器",
            ["about.version"] = "游戏版本：",
            ["btn.reset"] = "恢复默认设置",

            ["perso.color.title"] = "主题色",
            ["perso.color.desc"] = "应用到启动按钮、勾选框和选中状态，点一下立即生效。",
            ["btn.customcolor"] = "自定义颜色…",
            ["perso.bg.title"] = "窗口背景",
            ["perso.bg.desc"] = "选一张图片作为窗口底图，内容区会盖一层半透明白色保证可读。",
            ["btn.chooseimage"] = "选择图片…",
            ["btn.usebg"] = "使用官方启动器背景",
            ["btn.clearbg"] = "清除背景",
            ["bg.none"] = "未设置背景图片",

            ["nav.saves"] = "存档",
            ["saves.title"] = "存档",
            ["saves.refresh"] = "刷新",
            ["saves.empty"] = "没有找到存档文件。",
            ["saves.hint"] = "载入会先把所选存档写入 continue_game.json，再用 -continuelastsave 启动游戏，直接读档进入。",
            ["saves.none"] = "未选择存档",
            ["saves.count"] = "{0} 个存档",
            ["btn.loadsave"] = "载入所选存档",
            ["btn.resume"] = "继续最近存档",
            ["msg.noSaves"] = "没有可载入的存档。",
            ["status.loadingSave"] = "正在载入存档：{0}",

            ["modcount.enabled"] = "{0} / {1} 已启用",
            ["selected.count"] = "{0} / {1}",
            ["launch.subtitle"] = "已启用 {0} 个 MOD",
            ["game.notfound.short"] = "未找到游戏",
            ["footer"] = "游戏 {0} · 共 {1} 个 MOD · Hello Modern Hoi4 v1.0",
            ["footer.nogame"] = "Hello Modern Hoi4 v1.0",

            ["status.scanning"] = "正在扫描游戏与 MOD…",
            ["status.notfound"] = "未找到《钢铁雄心 IV》。到「设置」页选择 hoi4.exe 所在的游戏目录。",
            ["status.nomods"] = "没有扫描到任何 MOD。确认订阅了工坊 MOD，并用官方启动器进过一次游戏。",
            ["status.scanned"] = "扫描完成：{0} 个 MOD，已启用 {1} 个。",
            ["status.scanfailed"] = "扫描失败：{0}",
            ["status.writing"] = "正在写入 MOD 配置…",
            ["status.startingsteam"] = "Steam 未运行，正在启动 Steam…",
            ["status.launching"] = "正在启动游戏…",
            ["status.launched"] = "游戏已启动，祝你好运。",
            ["status.launchfailed"] = "启动失败",
            ["status.reset"] = "已恢复默认设置。",
            ["status.autodetect"] = "已恢复自动检测，正在扫描 Steam 库…",
            ["status.language"] = "语言已切换。",

            ["msg.gameRunning"] = "游戏已经在运行了。",
            ["msg.earlyExit"] = "hoi4.exe 启动后立刻退出了（退出码 {0}）。\n\n常见原因：MOD 与游戏版本不兼容、MOD 冲突，或者缺少运行库。\n日志位置：文档\\Paradox Interactive\\Hearts of Iron IV\\logs\\",
            ["msg.dirMissing"] = "目录不存在：\n{0}",
            ["msg.linkFailed"] = "打不开链接：\n{0}\n\n{1}",
            ["msg.resetConfirm"] = "把语言、主题色、背景、启动选项和手动指定的游戏目录恢复为默认？",
            ["msg.noGame"] = "还没有找到游戏目录。",
            ["msg.noOfficialBg"] = "游戏目录里没有找到官方启动器背景：\n{0}",
            ["msg.steamMissing"] = "Steam 没有运行，而且找不到 steam.exe。请先手动打开 Steam 再试。",

            ["dialog.pickGameTitle"] = "选择《钢铁雄心 IV》的 hoi4.exe",
            ["dialog.exeFilter"] = "hoi4.exe|hoi4.exe|可执行文件|*.exe",
            ["dialog.pickBgTitle"] = "选择窗口背景图片",
            ["dialog.imageFilter"] = "图片|*.png;*.jpg;*.jpeg;*.bmp;*.gif|所有文件|*.*",

            ["compat.ok"] = "匹配",
            ["compat.patch"] = "版本微差",
            ["compat.old"] = "不兼容",
            ["compat.unknown"] = "未标注",
            ["compat.missing"] = "路径缺失",
            ["compat.unknown.detail"] = "该 MOD 没有声明 supported_version。当前游戏 {0}，能不能用只能实测。",
            ["compat.nogame.detail"] = "读不到游戏版本，MOD 声明适配 {0}。",
            ["compat.wildcard.ok"] = "MOD 适配 {0}，当前游戏 {1} —— 版本匹配。",
            ["compat.wildcard.bad"] = "MOD 适配 {0}，当前游戏 {1} —— 跨版本，加载后容易崩溃或报错。",
            ["compat.exact.ok"] = "MOD 适配 {0}，当前游戏 {1} —— 完全一致。",
            ["compat.prefix.detail"] = "MOD 适配 {0}，当前游戏 {1} —— 主版本相同，通常能用。",
            ["compat.patch.detail"] = "MOD 适配 {0}，当前游戏 {1} —— 补丁号不同，多半可用；如果崩溃先换掉它。",
            ["compat.old.detail"] = "MOD 适配 {0}，当前游戏 {1} —— 跨大版本，很可能崩溃或跳出。",
            ["compat.missing.detail"] = "MOD 目录不存在：可能已取消订阅，或者 path 指向的位置被移动了。"
        },

        [En] = new Dictionary<string, string>
        {
            ["app.subtitle"] = "Hearts of Iron IV launcher · hmhoi4",
            ["btn.workshop"] = "Workshop",
            ["tip.workshop"] = "Open the Steam Workshop to subscribe / download mods",
            ["btn.rescan"] = "Rescan",

            ["nav.launch"] = "Launch",
            ["nav.settings"] = "Settings",
            ["nav.personalize"] = "Appearance",
            ["sidebar.info"] = "Game info",

            ["mods.title"] = "Mods",
            ["search.placeholder"] = "Search mods…",
            ["btn.selectall"] = "Select all",
            ["btn.selectnone"] = "Clear",
            ["btn.invert"] = "Invert",
            ["details.placeholder"] = "Click a mod to see its details",
            ["details.placeholder2"] = "Double-click a card to toggle it",
            ["launch.enabled"] = "Enabled",
            ["btn.launch"] = "Launch Game",
            ["chk.debug"] = "Debug mode (-debug_mode)",
            ["chk.close"] = "Close launcher after launch",
            ["detail.tags"] = "Tags",
            ["detail.workshop"] = "Workshop",
            ["detail.path"] = "Path",
            ["btn.opensteamworkshop"] = "Open in Steam Workshop",
            ["btn.openfolder"] = "Open in Explorer",

            ["settings.language.title"] = "Language",
            ["settings.language.desc"] = "Switching the interface language rescans your mods.",
            ["settings.game.title"] = "Game location",
            ["settings.game.desc"] = "The launcher reads launcher-settings.json from the game folder for the version and launch arguments.",
            ["label.gamedir"] = "Game folder",
            ["label.userdir"] = "Data folder",
            ["label.moddir"] = "Mod folder",
            ["label.savedir"] = "Save folder",
            ["btn.choose"] = "Choose…",
            ["btn.open"] = "Open",
            ["btn.autodetect"] = "Auto-detect",
            ["hint.autodetect"] = "Clears the manual folder and rescans Steam libraries.",
            ["settings.launch.title"] = "Launch options",
            ["settings.launch.desc"] = "Selected mods are written to dlc_load.json before launch (the original is backed up as dlc_load.json.launcher-backup the first time), then hoi4.exe is started directly, bypassing the Paradox Launcher.",
            ["settings.steam.title"] = "Steam",
            ["steam.running"] = "Steam is running",
            ["steam.notrunning"] = "Steam isn't running — it will be started automatically",
            ["btn.refresh"] = "Refresh",
            ["hint.steam"] = "If Steam isn't running, it will be started first and given time to initialize.",
            ["settings.shortcuts.title"] = "Shortcuts",
            ["btn.opengame"] = "Open game folder",
            ["btn.openmods"] = "Open mod folder",
            ["btn.opensaves"] = "Open saves folder",
            ["btn.openlogs"] = "Open logs folder",
            ["btn.openworkshop"] = "Open Workshop",
            ["settings.about.title"] = "About",
            ["about.line"] = "Hello Modern Hoi4 v1.0 · Standalone launcher for Hearts of Iron IV",
            ["about.version"] = "Game version: ",
            ["btn.reset"] = "Restore defaults",

            ["perso.color.title"] = "Accent color",
            ["perso.color.desc"] = "Applies to the launch button, checkboxes and selection states — instantly.",
            ["btn.customcolor"] = "Custom color…",
            ["perso.bg.title"] = "Window background",
            ["perso.bg.desc"] = "Choose an image as the window backdrop; content areas get a translucent white wash for readability.",
            ["btn.chooseimage"] = "Choose image…",
            ["btn.usebg"] = "Use official background",
            ["btn.clearbg"] = "Clear background",
            ["bg.none"] = "No background image set",

            ["nav.saves"] = "Saves",
            ["saves.title"] = "Saves",
            ["saves.refresh"] = "Refresh",
            ["saves.empty"] = "No save files found.",
            ["saves.hint"] = "Loading writes the selected save to continue_game.json and starts the game with -continuelastsave to load straight in.",
            ["saves.none"] = "No save selected",
            ["saves.count"] = "{0} saves",
            ["btn.loadsave"] = "Load selected save",
            ["btn.resume"] = "Resume latest save",
            ["msg.noSaves"] = "No saves are available to load.",
            ["status.loadingSave"] = "Loading save: {0}",

            ["modcount.enabled"] = "{0} / {1} enabled",
            ["selected.count"] = "{0} / {1}",
            ["launch.subtitle"] = "{0} mods enabled",
            ["game.notfound.short"] = "Game not found",
            ["footer"] = "Game {0} · {1} mods · Hello Modern Hoi4 v1.0",
            ["footer.nogame"] = "Hello Modern Hoi4 v1.0",

            ["status.scanning"] = "Scanning game and mods…",
            ["status.notfound"] = "Hearts of Iron IV not found. Choose the folder containing hoi4.exe on the Settings page.",
            ["status.nomods"] = "No mods found. Make sure you're subscribed to Workshop mods and have launched the game once with the official launcher.",
            ["status.scanned"] = "Scan complete: {0} mods, {1} enabled.",
            ["status.scanfailed"] = "Scan failed: {0}",
            ["status.writing"] = "Writing mod configuration…",
            ["status.startingsteam"] = "Steam isn't running — starting Steam…",
            ["status.launching"] = "Launching game…",
            ["status.launched"] = "Game launched. Good luck.",
            ["status.launchfailed"] = "Launch failed",
            ["status.reset"] = "Default settings restored.",
            ["status.autodetect"] = "Auto-detect restored; scanning Steam libraries…",
            ["status.language"] = "Language switched.",

            ["msg.gameRunning"] = "The game is already running.",
            ["msg.earlyExit"] = "hoi4.exe exited immediately after launch (exit code {0}).\n\nCommon causes: mods incompatible with the game version, mod conflicts, or missing runtimes.\nLogs: Documents\\Paradox Interactive\\Hearts of Iron IV\\logs\\",
            ["msg.dirMissing"] = "Folder not found:\n{0}",
            ["msg.linkFailed"] = "Couldn't open the link:\n{0}\n\n{1}",
            ["msg.resetConfirm"] = "Restore language, accent color, background, launch options and the manual game folder to defaults?",
            ["msg.noGame"] = "Game folder hasn't been found yet.",
            ["msg.noOfficialBg"] = "The official launcher background wasn't found in the game folder:\n{0}",
            ["msg.steamMissing"] = "Steam isn't running and steam.exe couldn't be found. Start Steam manually and try again.",

            ["dialog.pickGameTitle"] = "Select hoi4.exe for Hearts of Iron IV",
            ["dialog.exeFilter"] = "hoi4.exe|hoi4.exe|Executables|*.exe",
            ["dialog.pickBgTitle"] = "Choose a background image",
            ["dialog.imageFilter"] = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*",

            ["compat.ok"] = "Compatible",
            ["compat.patch"] = "Minor mismatch",
            ["compat.old"] = "Incompatible",
            ["compat.unknown"] = "Unrated",
            ["compat.missing"] = "Missing files",
            ["compat.unknown.detail"] = "This mod doesn't declare supported_version. Current game is {0}; compatibility must be tested.",
            ["compat.nogame.detail"] = "Couldn't read the game version. The mod targets {0}.",
            ["compat.wildcard.ok"] = "Mod targets {0}, current game {1} — version matches.",
            ["compat.wildcard.bad"] = "Mod targets {0}, current game {1} — different version; may crash or throw errors.",
            ["compat.exact.ok"] = "Mod targets {0}, current game {1} — exact match.",
            ["compat.prefix.detail"] = "Mod targets {0}, current game {1} — same major version; usually fine.",
            ["compat.patch.detail"] = "Mod targets {0}, current game {1} — patch version differs; usually works. Swap it out first if you crash.",
            ["compat.old.detail"] = "Mod targets {0}, current game {1} — different major version; likely to crash or break.",
            ["compat.missing.detail"] = "Mod folder is missing — it may have been unsubscribed or moved."
        }
    };

    public static void Apply(string language)
    {
        Current = Tables.ContainsKey(language) ? language : Zh;

        var resources = Application.Current.Resources;
        foreach (var pair in Tables[Current])
            resources["L." + pair.Key] = pair.Value;
    }

    public static string T(string key)
    {
        if (Tables[Current].TryGetValue(key, out var value)) return value;
        return Tables[Zh].TryGetValue(key, out var fallback) ? fallback : key;
    }

    public static string F(string key, params object[] args) => string.Format(T(key), args);
}
