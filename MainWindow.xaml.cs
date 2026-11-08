using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Hoi4Launcher.Models;
using Hoi4Launcher.Services;
using Microsoft.Win32;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;

namespace Hoi4Launcher;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private static readonly Color DefaultAccent = Color.FromRgb(0xC4, 0x70, 0x2F);

    private readonly ObservableCollection<ModEntry> _mods = new();
    private readonly ObservableCollection<SaveEntry> _saves = new();
    private readonly LauncherSettings _settings;

    private GameInfo? _game;
    private ModEntry? _selectedMod;
    private SaveEntry? _selectedSave;
    private string _language = Loc.Zh;
    private string _status = "";
    private bool _launching;
    private bool _syncingUi;

    public ICollectionView Mods { get; }
    public IReadOnlyList<ThemeSwatch> Swatches { get; }

    public MainWindow()
    {
        _settings = SettingsStore.Load();
        _language = ResolveLanguage(_settings.Language);
        Loc.Apply(_language);

        InitializeComponent();

        Mods = CollectionViewSource.GetDefaultView(_mods);
        Mods.Filter = FilterMod;

        Swatches = BuildSwatches();
        SwatchList.ItemsSource = Swatches;
        SaveList.ItemsSource = _saves;

        DataContext = this;

        SyncSwatchSelection();

        _syncingUi = true;
        LanguageCombo.SelectedIndex = _language == Loc.En ? 1 : 0;
        _syncingUi = false;

        ApplyAccent(ParseColor(_settings.Accent, DefaultAccent), persist: false);
        ApplyBackground(_settings.BackgroundImage, persist: false);

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateLaunchGlow(ParseColor(_settings.Accent, DefaultAccent));
        LoadAll();
        SelectInitialPage();
    }

    // ================= 绑定属性 =================

    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public ModEntry? SelectedMod
    {
        get => _selectedMod;
        set
        {
            _selectedMod = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelection));
        }
    }

    public bool HasSelection => _selectedMod != null;

    public SaveEntry? SelectedSave
    {
        get => _selectedSave;
        set
        {
            _selectedSave = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanLoadSave));
            OnPropertyChanged(nameof(SelectedSaveText));
        }
    }

    public bool CanLoadSave => _game != null && _selectedSave != null && !_launching;

    public bool CanLaunch => _game != null && !_launching;

    public bool DebugMode
    {
        get => _settings.DebugMode;
        set
        {
            if (_settings.DebugMode == value) return;
            _settings.DebugMode = value;
            OnPropertyChanged();
            SaveSettings();
        }
    }

    public bool CloseAfterLaunch
    {
        get => _settings.CloseAfterLaunch;
        set
        {
            if (_settings.CloseAfterLaunch == value) return;
            _settings.CloseAfterLaunch = value;
            OnPropertyChanged();
            SaveSettings();
        }
    }

    public string GameVersionShort => _game?.RawVersion ?? Loc.T("game.notfound.short");
    public string GameVersionFull => _game?.Version ?? "—";
    public string GameDirShort => _game?.GameDir ?? "—";
    public string UserDirShort => _game?.UserDir ?? "—";
    public string ModDirShort => _game?.ModDir ?? "—";
    public string SaveDirShort => _game?.SaveDir ?? "—";
    public string LogDirShort => _game?.LogDir ?? "—";

    public bool SteamRunning => GameLauncher.IsSteamRunning();

    public string SteamStatus => Loc.T(SteamRunning ? "steam.running" : "steam.notrunning");

    public string ModCountText => Loc.F("modcount.enabled", CountSelected(), _mods.Count);
    public string SelectedCountText => Loc.F("selected.count", CountSelected(), _mods.Count);

    public string SaveCountText => Loc.F("saves.count", _saves.Count);
    public string SelectedSaveText => _selectedSave?.DisplayName ?? Loc.T("saves.none");

    public string LaunchSubtitle => _game == null
        ? Loc.T("game.notfound.short")
        : Loc.F("launch.subtitle", CountSelected());

    public string FooterText => _game == null
        ? Loc.T("footer.nogame")
        : Loc.F("footer", _game.RawVersion, _mods.Count);

    public string BackgroundPathText => string.IsNullOrEmpty(_settings.BackgroundImage)
        ? Loc.T("bg.none")
        : _settings.BackgroundImage!;

    // ================= 加载 =================

    private void LoadAll(ISet<string>? keepSelected = null)
    {
        Status = Loc.T("status.scanning");

        try
        {
            _game = GameLocator.Find(_settings.GameDir);
            _mods.Clear();

            ModList.SelectedItem = null;
            SelectedMod = null;

            if (_game == null)
            {
                Status = Loc.T("status.notfound");
                RefreshAll();
                return;
            }

            var enabled = ModConfigWriter.ReadEnabled(_game);
            var entries = ModScanner.Scan(_game.UserDir, _game.GameDir, _game.RawVersion, enabled);

            foreach (var entry in entries)
            {
                if (keepSelected != null)
                    entry.IsSelected = keepSelected.Contains(entry.FileName);

                entry.PropertyChanged += OnModPropertyChanged;
                _mods.Add(entry);
            }

            Status = _mods.Count == 0
                ? Loc.T("status.nomods")
                : Loc.F("status.scanned", _mods.Count, CountSelected());

            RefreshAll();
        }
        catch (Exception ex)
        {
            Status = Loc.F("status.scanfailed", ex.Message);
            RefreshAll();
        }
        finally
        {
            RefreshSaves();
        }
    }

    private void OnModPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ModEntry.IsSelected)) return;

        OnPropertyChanged(nameof(ModCountText));
        OnPropertyChanged(nameof(SelectedCountText));
        OnPropertyChanged(nameof(LaunchSubtitle));
        OnPropertyChanged(nameof(FooterText));
    }

    private void RefreshAll()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(GameVersionShort));
        OnPropertyChanged(nameof(GameVersionFull));
        OnPropertyChanged(nameof(GameDirShort));
        OnPropertyChanged(nameof(UserDirShort));
        OnPropertyChanged(nameof(ModDirShort));
        OnPropertyChanged(nameof(SaveDirShort));
        OnPropertyChanged(nameof(LogDirShort));
        OnPropertyChanged(nameof(SteamRunning));
        OnPropertyChanged(nameof(SteamStatus));
        OnPropertyChanged(nameof(ModCountText));
        OnPropertyChanged(nameof(SelectedCountText));
        OnPropertyChanged(nameof(LaunchSubtitle));
        OnPropertyChanged(nameof(FooterText));
        OnPropertyChanged(nameof(CanLaunch));
        OnPropertyChanged(nameof(CanLoadSave));
        OnPropertyChanged(nameof(SaveCountText));
        OnPropertyChanged(nameof(SelectedSaveText));
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(BackgroundPathText));
    }

    private int CountSelected() => _mods.Count(mod => mod.IsSelected);

    private void SelectInitialPage()
    {
        var args = Environment.GetCommandLineArgs();
        if (args.Any(a => a.Equals("--settings", StringComparison.OrdinalIgnoreCase)))
            NavSettings.IsChecked = true;
        else if (args.Any(a => a.Equals("--personalize", StringComparison.OrdinalIgnoreCase) ||
                               a.Equals("--personalization", StringComparison.OrdinalIgnoreCase)))
            NavPersonalize.IsChecked = true;
    }

    // ================= 语言 =================

    private static string ResolveLanguage(string? value)
    {
        if (value == Loc.Zh || value == Loc.En) return value;

        var culture = System.Globalization.CultureInfo.CurrentUICulture;
        return culture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase)
            ? Loc.Zh
            : Loc.En;
    }

    private void Language_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingUi) return;
        if (LanguageCombo.SelectedItem is not ComboBoxItem item) return;
        if (item.Tag is not string language || language == _language) return;

        SwitchLanguage(language);
    }

    private void SwitchLanguage(string language)
    {
        var keep = new HashSet<string>(
            _mods.Where(mod => mod.IsSelected).Select(mod => mod.FileName),
            StringComparer.OrdinalIgnoreCase);

        _language = language;
        _settings.Language = language;
        SaveSettings();
        Loc.Apply(language);

        LoadAll(keep);
        Status = Loc.T("status.language");
    }

    // ================= 导航 =================

    private void Nav_Checked(object sender, RoutedEventArgs e)
    {
        if (PageLaunch == null || PageSaves == null || PageSettings == null || PagePersonalize == null) return;

        var tag = (sender as FrameworkElement)?.Tag as string ?? "launch";
        PageLaunch.Visibility = tag == "launch" ? Visibility.Visible : Visibility.Collapsed;
        PageSaves.Visibility = tag == "saves" ? Visibility.Visible : Visibility.Collapsed;
        PageSettings.Visibility = tag == "settings" ? Visibility.Visible : Visibility.Collapsed;
        PagePersonalize.Visibility = tag == "personalize" ? Visibility.Visible : Visibility.Collapsed;

        if (tag == "saves") RefreshSaves();

        if (tag == "settings")
        {
            OnPropertyChanged(nameof(SteamRunning));
            OnPropertyChanged(nameof(SteamStatus));
        }
    }

    // ================= 搜索与批量选择 =================

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Mods == null) return;

        SearchHint.Visibility = string.IsNullOrEmpty(SearchBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;

        Mods.Refresh();
    }

    private bool FilterMod(object item)
    {
        if (item is not ModEntry mod) return false;

        var query = SearchBox?.Text?.Trim();
        if (string.IsNullOrEmpty(query)) return true;

        return mod.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
            || mod.SupportedVersion.Contains(query, StringComparison.OrdinalIgnoreCase)
            || mod.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase))
            || (mod.RemoteId?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private void SelectAll_Click(object sender, RoutedEventArgs e) => SetVisible(_ => true);

    private void SelectNone_Click(object sender, RoutedEventArgs e) => SetVisible(_ => false);

    private void Invert_Click(object sender, RoutedEventArgs e) => SetVisible(mod => !mod.IsSelected);

    private void SetVisible(Func<ModEntry, bool> predicate)
    {
        foreach (var mod in Mods.Cast<ModEntry>().ToList())
            mod.IsSelected = predicate(mod);
    }

    // ================= 列表交互 =================

    private void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => SelectedMod = ModList.SelectedItem as ModEntry;

    private void ModList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FindAncestor<CheckBox>(e.OriginalSource as DependencyObject) != null) return;
        if (SelectedMod == null) return;

        SelectedMod.IsSelected = !SelectedMod.IsSelected;
    }

    private static T? FindAncestor<T>(DependencyObject? source) where T : DependencyObject
    {
        while (source != null)
        {
            if (source is T match) return match;
            source = source is Visual || source is System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(source)
                : LogicalTreeHelper.GetParent(source);
        }
        return null;
    }

    // ================= 启动 =================

    private async void Launch_Click(object sender, RoutedEventArgs e) => await StartGameAsync(null, false);

    private async void ResumeLatest_Click(object sender, RoutedEventArgs e)
    {
        var latest = _game == null ? null : SaveScanner.Scan(_game).FirstOrDefault();
        if (latest == null)
        {
            MessageBox.Show(this, Loc.T("msg.noSaves"), "Hello Modern Hoi4",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        await StartGameAsync(latest, true);
    }

    private async Task StartGameAsync(SaveEntry? save, bool continueSave)
    {
        if (_game == null || _launching) return;

        if (GameLauncher.IsGameRunning())
        {
            MessageBox.Show(this, Loc.T("msg.gameRunning"), "Hello Modern Hoi4",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _launching = true;
        OnPropertyChanged(nameof(CanLaunch));
        OnPropertyChanged(nameof(CanLoadSave));

        try
        {
            Status = Loc.T("status.writing");
            ModConfigWriter.Save(_game, _mods.Where(mod => mod.IsSelected));

            if (save != null)
            {
                SaveLauncher.WriteContinueGame(_game, save);
                Status = Loc.F("status.loadingSave", save.DisplayName);
            }

            await GameLauncher.EnsureSteamAsync(_game, message => Status = message);

            if (save == null) Status = Loc.T("status.launching");
            var process = GameLauncher.Launch(_game, DebugMode, continueSave);

            await Task.Delay(4000);

            if (process != null && process.HasExited)
                throw new InvalidOperationException(Loc.F("msg.earlyExit", process.ExitCode));

            Status = Loc.T("status.launched");
            if (CloseAfterLaunch) Close();
        }
        catch (Exception ex)
        {
            Status = Loc.T("status.launchfailed");
            MessageBox.Show(this, ex.Message, Loc.T("status.launchfailed"),
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _launching = false;
            OnPropertyChanged(nameof(CanLaunch));
            OnPropertyChanged(nameof(CanLoadSave));
        }
    }

    // ================= 存档 =================

    private void RefreshSaves()
    {
        _saves.Clear();

        if (_game != null)
        {
            foreach (var save in SaveScanner.Scan(_game))
                _saves.Add(save);
        }

        SavesEmpty.Visibility = _saves.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        if (_saves.Count > 0) SaveList.SelectedIndex = 0;
        else SelectedSave = null;

        OnPropertyChanged(nameof(SaveCountText));
        OnPropertyChanged(nameof(CanLoadSave));
        OnPropertyChanged(nameof(SelectedSaveText));
    }

    private void RefreshSaves_Click(object sender, RoutedEventArgs e) => RefreshSaves();

    private void SaveList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => SelectedSave = SaveList.SelectedItem as SaveEntry;

    private async void LoadSelectedSave_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedSave == null)
        {
            MessageBox.Show(this, Loc.T("msg.noSaves"), "Hello Modern Hoi4",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        await StartGameAsync(SelectedSave, true);
    }

    // ================= 个性化：主题色 =================

    private static IReadOnlyList<ThemeSwatch> BuildSwatches() => new[]
    {
        new ThemeSwatch { Name = "橙棕 / Amber", Color = Color.FromRgb(0xC4, 0x70, 0x2F) },
        new ThemeSwatch { Name = "琥珀 / Gold", Color = Color.FromRgb(0xD0, 0x9A, 0x3A) },
        new ThemeSwatch { Name = "砖红 / Brick", Color = Color.FromRgb(0xB8, 0x45, 0x3C) },
        new ThemeSwatch { Name = "苔绿 / Moss", Color = Color.FromRgb(0x4F, 0x8A, 0x5B) },
        new ThemeSwatch { Name = "钢蓝 / Steel", Color = Color.FromRgb(0x3F, 0x72, 0xAF) },
        new ThemeSwatch { Name = "紫棠 / Violet", Color = Color.FromRgb(0x7A, 0x5A, 0xA6) }
    };

    private void Swatch_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingUi || SwatchList.SelectedItem is not ThemeSwatch swatch) return;
        ApplyAccent(swatch.Color);
    }

    private void CustomColor_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.ColorDialog { FullOpen = true };
        var current = ParseColor(_settings.Accent, DefaultAccent);
        dialog.Color = System.Drawing.Color.FromArgb(current.R, current.G, current.B);

        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

        var color = Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B);

        _syncingUi = true;
        SwatchList.SelectedItem = null;
        _syncingUi = false;

        ApplyAccent(color);
    }

    private void SyncSwatchSelection()
    {
        _syncingUi = true;
        SwatchList.SelectedItem = Swatches.FirstOrDefault(
            swatch => ToHex(swatch.Color).Equals(_settings.Accent, StringComparison.OrdinalIgnoreCase));
        _syncingUi = false;
    }

    private void ApplyAccent(Color color, bool persist = true)
    {
        var resources = Application.Current.Resources;
        resources["AccentBrush"] = Frozen(new SolidColorBrush(color));
        resources["AccentHoverBrush"] = Frozen(new SolidColorBrush(Lighten(color, 22)));
        resources["AccentPressedBrush"] = Frozen(new SolidColorBrush(Darken(color, 26)));
        resources["AccentSoftBrush"] = Frozen(new SolidColorBrush(Blend(color, Colors.White, 0.88)));

        if (persist)
        {
            _settings.Accent = ToHex(color);
            SaveSettings();
        }

        UpdateLaunchGlow(color);
    }

    private void UpdateLaunchGlow(Color color)
    {
        if (LaunchButton?.Template?.FindName("LaunchBd", LaunchButton) is not Border border) return;

        try
        {
            if (border.Effect is DropShadowEffect shadow)
            {
                shadow.Color = color;
                return;
            }
        }
        catch
        {
            // 模板里的效果被冻结了就换成新的
        }

        border.Effect = new DropShadowEffect
        {
            BlurRadius = 16,
            ShadowDepth = 2,
            Opacity = 0.30,
            Color = color
        };
    }

    // ================= 个性化：背景 =================

    private void ChooseBackground_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = Loc.T("dialog.pickBgTitle"),
            Filter = Loc.T("dialog.imageFilter"),
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == true) ApplyBackground(dialog.FileName);
    }

    private void UseOfficialBackground_Click(object sender, RoutedEventArgs e)
    {
        if (_game == null)
        {
            MessageBox.Show(this, Loc.T("msg.noGame"), "Hello Modern Hoi4",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var path = Path.Combine(_game.GameDir, "assets", "app-background.png");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, Loc.F("msg.noOfficialBg", path), "Hello Modern Hoi4",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        ApplyBackground(path);
    }

    private void ClearBackground_Click(object sender, RoutedEventArgs e) => ApplyBackground(null);

    private void ApplyBackground(string? path, bool persist = true)
    {
        path = string.IsNullOrWhiteSpace(path) ? null : path;
        BitmapImage? bitmap = null;

        if (path != null)
        {
            if (File.Exists(path)) bitmap = LoadBitmap(path);
            if (bitmap == null) path = null;
        }

        if (path != null && bitmap != null)
        {
            Background = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill };
            BackdropWash.Visibility = Visibility.Visible;
            BgPreviewBorder.Background = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill };
        }
        else
        {
            Background = new SolidColorBrush(Color.FromRgb(0xF2, 0xF4, 0xF8));
            BackdropWash.Visibility = Visibility.Collapsed;
            BgPreviewBorder.Background = new SolidColorBrush(Color.FromRgb(0xEE, 0xF2, 0xF7));
        }

        _settings.BackgroundImage = path;
        OnPropertyChanged(nameof(BackgroundPathText));

        if (persist) SaveSettings();
    }

    private static BitmapImage? LoadBitmap(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 1920;
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

    // ================= 设置 =================

    private void SetGameDir_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = Loc.T("dialog.pickGameTitle"),
            Filter = Loc.T("dialog.exeFilter"),
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != true) return;

        var dir = Path.GetDirectoryName(dialog.FileName);
        if (string.IsNullOrEmpty(dir)) return;

        _settings.GameDir = dir;
        SaveSettings();
        LoadAll();
    }

    private void AutoDetect_Click(object sender, RoutedEventArgs e)
    {
        _settings.GameDir = null;
        SaveSettings();
        LoadAll();
        Status = Loc.T("status.autodetect");
    }

    private void RefreshSteam_Click(object sender, RoutedEventArgs e)
    {
        OnPropertyChanged(nameof(SteamRunning));
        OnPropertyChanged(nameof(SteamStatus));
        Status = SteamStatus;
    }

    private void ResetSettings_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(this, Loc.T("msg.resetConfirm"),
            "Hello Modern Hoi4", MessageBoxButton.OKCancel, MessageBoxImage.Question);

        if (result != MessageBoxResult.OK) return;

        var language = ResolveLanguage(null);

        _settings.Accent = ToHex(DefaultAccent);
        _settings.BackgroundImage = null;
        _settings.DebugMode = false;
        _settings.CloseAfterLaunch = false;
        _settings.GameDir = null;
        _settings.Language = language;
        SaveSettings();

        _language = language;
        Loc.Apply(language);

        _syncingUi = true;
        LanguageCombo.SelectedIndex = language == Loc.En ? 1 : 0;
        _syncingUi = false;

        ApplyAccent(DefaultAccent, persist: false);
        ApplyBackground(null, persist: false);
        SyncSwatchSelection();

        OnPropertyChanged(nameof(DebugMode));
        OnPropertyChanged(nameof(CloseAfterLaunch));

        LoadAll();
        Status = Loc.T("status.reset");
    }

    private void SaveSettings() => SettingsStore.Save(_settings);

    // ================= 创意工坊 =================

    private const string WorkshopAppUrl = "https://steamcommunity.com/app/394360/workshop/";

    private void OpenWorkshop_Click(object sender, RoutedEventArgs e) => OpenUrl(WorkshopAppUrl);

    private void OpenSelectedWorkshop_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedMod is not { HasRemoteId: true } mod || string.IsNullOrEmpty(mod.RemoteId)) return;

        var id = mod.RemoteId;

        // 优先让 Steam 客户端打开工坊条目，Steam 没装再退回浏览器
        try
        {
            Process.Start(new ProcessStartInfo($"steam://url/CommunityFilePage/{id}") { UseShellExecute = true });
            return;
        }
        catch
        {
            // 走浏览器兜底
        }

        OpenUrl($"https://steamcommunity.com/sharedfiles/filedetails/?id={id}");
    }

    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, Loc.F("msg.linkFailed", url, ex.Message), "Hello Modern Hoi4",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ================= 目录 =================

    private void OpenModDir_Click(object sender, RoutedEventArgs e) => OpenFolder(_game?.ModDir);

    private void OpenSaveDir_Click(object sender, RoutedEventArgs e) => OpenFolder(_game?.SaveDir);

    private void OpenGameDir_Click(object sender, RoutedEventArgs e) => OpenFolder(_game?.GameDir);

    private void OpenUserDir_Click(object sender, RoutedEventArgs e) => OpenFolder(_game?.UserDir);

    private void OpenLogDir_Click(object sender, RoutedEventArgs e) => OpenFolder(_game?.LogDir);

    private void OpenSelectedModDir_Click(object sender, RoutedEventArgs e) => OpenFolder(SelectedMod?.ModPath);

    private void OpenFolder(string? path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            MessageBox.Show(this, Loc.F("msg.dirMissing", path ?? "—"), "Hello Modern Hoi4",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
    }

    private void Rescan_Click(object sender, RoutedEventArgs e) => LoadAll();

    // ================= 颜色工具 =================

    private static SolidColorBrush Frozen(SolidColorBrush brush)
    {
        brush.Freeze();
        return brush;
    }

    private static Color Lighten(Color color, int amount) => Color.FromRgb(
        (byte)Math.Min(255, color.R + amount),
        (byte)Math.Min(255, color.G + amount),
        (byte)Math.Min(255, color.B + amount));

    private static Color Darken(Color color, int amount) => Color.FromRgb(
        (byte)Math.Max(0, color.R - amount),
        (byte)Math.Max(0, color.G - amount),
        (byte)Math.Max(0, color.B - amount));

    private static Color Blend(Color color, Color towards, double weight)
        => Color.FromRgb(
            (byte)(color.R * (1 - weight) + towards.R * weight),
            (byte)(color.G * (1 - weight) + towards.G * weight),
            (byte)(color.B * (1 - weight) + towards.B * weight));

    private static string ToHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    private static Color ParseColor(string? hex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hex)) return fallback;

        try
        {
            return ColorConverter.ConvertFromString(hex) is Color color ? color : fallback;
        }
        catch
        {
            return fallback;
        }
    }

    // ================= INotifyPropertyChanged =================

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
