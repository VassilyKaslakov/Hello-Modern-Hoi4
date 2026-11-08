namespace Hoi4Launcher.Models;

public sealed class SaveEntry
{
    public required string FileName { get; init; }
    public required string FilePath { get; init; }
    public required string DisplayName { get; init; }
    public DateTime Modified { get; init; }
    public long Size { get; init; }

    public string ModifiedText => Modified.ToString("yyyy-MM-dd HH:mm");
    public string SizeText => $"{Size / 1048576.0:0.0} MB";
    public string SubLine => $"{ModifiedText}  ·  {SizeText}";
}
