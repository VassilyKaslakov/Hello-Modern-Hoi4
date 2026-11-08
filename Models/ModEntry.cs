using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace Hoi4Launcher.Models;

public sealed class ModEntry : INotifyPropertyChanged
{
    public required string FileName { get; init; }
    public required string Name { get; init; }
    public string SupportedVersion { get; init; } = "";
    public string? ModPath { get; init; }
    public string? RemoteId { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public BitmapImage? Thumbnail { get; init; }
    public string CompatLevel { get; init; } = "unknown";
    public string CompatBadge { get; init; } = "未标注";
    public string CompatDetail { get; init; } = "";

    public string RelativePath => "mod/" + FileName;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public string SubLine
    {
        get
        {
            var parts = new List<string>
            {
                string.IsNullOrWhiteSpace(SupportedVersion) ? "未标注版本" : SupportedVersion
            };
            if (Tags.Count > 0) parts.Add(string.Join(" / ", Tags.Take(3)));
            if (!string.IsNullOrEmpty(RemoteId)) parts.Add("工坊 " + RemoteId);
            return string.Join("  ·  ", parts);
        }
    }

    public string TagsText => Tags.Count == 0 ? "—" : string.Join(", ", Tags);
    public bool HasRemoteId => !string.IsNullOrEmpty(RemoteId);
    public string RemoteLink => string.IsNullOrEmpty(RemoteId)
        ? "—"
        : "steamcommunity.com/sharedfiles/filedetails/?id=" + RemoteId;
    public string PathText => ModPath ?? "（.mod 文件里没有 path 字段）";

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
