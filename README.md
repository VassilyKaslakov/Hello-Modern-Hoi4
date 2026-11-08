# Hello Modern Hoi4（hmhoi4）

《钢铁雄心 IV》独立启动器。绕过 Paradox Launcher，直接把勾选的 MOD 写进 `dlc_load.json`，然后启动 `hoi4.exe`。

界面参考 PCL2：浅色卡片 + 圆角 + 左侧导航，主色默认橙棕（`#C4702F`），可在「个性化」里改。

## 运行

- 直接运行 `publish\hmhoi4.exe`（单文件，约 0.9 MB）
- 依赖 .NET 9 **Desktop Runtime**（本机已安装 9.0.22）
- 想做成自包含（换电脑不用装运行时）：
  ```bash
  dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish-full
  ```

## 功能

- 扫描 `文档\Paradox Interactive\Hearts of Iron IV\mod\*.mod`，读取名称、版本、标签、工坊 ID、缩略图
- 按 `supported_version` 和游戏版本比对，标出「匹配 / 版本微差 / 不兼容 / 路径缺失」
- 勾选后写回 `dlc_load.json`；首次启动前自动备份为 `dlc_load.json.launcher-backup`
- 直接运行 `hoi4.exe`（参数取自官方 `launcher-settings.json`，即 `-gdpr-compliant`），不经过 Paradox Launcher
- Steam 没开时自动拉起并等它初始化
- 中英双语：设置页「语言」切换，默认跟随系统；MOD 兼容性提示、状态栏、对话框全部本地化
- 存档页：列出存档（标题 / 修改时间 / 体积），「载入所选存档」会写 `continue_game.json` 并用 `-continuelastsave` 启动，直接读档进入；启动页有「继续最近存档」快捷按钮
- 「创意工坊」跳转：顶栏进入游戏工坊下载页；MOD 详情里「在 Steam 工坊打开」直达该 MOD 的订阅页
- 「设置」页：游戏目录、数据/MOD/存档/日志目录、调试模式、启动后关闭、Steam 状态
- 「个性化」页：6 套主题色 + 自定义颜色、窗口背景图片（可用游戏自带的官方启动器背景 `assets\app-background.png`）
- 配置持久化在 `%APPDATA%\HelloModernHoi4\settings.json`

## 文件结构

```
hoi4_launcher/
├── Assets/                 # 图标（由游戏 assets/game-icon.png 转出的多尺寸 ico + png）
├── Models/                 # ModEntry、SaveEntry、ThemeSwatch
├── Services/               # 游戏定位、mod 扫描、版本判定、dlc_load 读写、存档扫描/读档、本地化、设置存储
├── MainWindow.xaml(.cs)    # 界面与交互
├── App.xaml(.cs)           # 主题资源
├── GlobalUsings.cs
├── Hoi4Launcher.csproj
└── publish/hmhoi4.exe      # 发布产物
```

## 重新编译

```bash
cd C:\Users\plop0\hoi4_launcher
dotnet build
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=none -o publish
```

调试时可直接指定页面：`hmhoi4.exe --settings` / `hmhoi4.exe --personalize`。

## 注意

- 游戏启动与读档尚未在实机上完整跑通（会先备份 `dlc_load.json` 和 `continue_game.json`）；点一次「启动游戏 / 载入所选存档」即可验证。
- 直接读档依赖 `hoi4.exe` 的 `-continuelastsave` 参数（已在二进制里确认存在）。如果某版本游戏不认这个参数，会退化成普通启动、停在主菜单，此时主菜单的「继续」仍指向你选的存档。
- 语言、主题色、背景、启动选项存在 `%APPDATA%\HelloModernHoi4\settings.json`。
- 只会把**勾选**的 MOD 写进配置；没勾的会被关掉。
- 不认识的 MOD 仍可勾选，但版本徽章是红色时建议先换掉。
- 样式改动集中在 `App.xaml`；主题色是运行时替换 `Application.Current.Resources` 里的 `AccentBrush` 系列，所以 XAML 里必须用 `DynamicResource`。
