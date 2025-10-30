# Path 项目结构文档

## 项目概述
这是一个基于 .NET 8 和 WPF 的 3D 路径编辑器和可视化应用程序。

## 技术栈
- **框架**: .NET 8 (net8.0-windows)
- **UI 框架**: WPF (Windows Presentation Foundation)
- **3D 渲染**: HelixToolkit.Wpf 2.22.0
- **MVVM 框架**: CommunityToolkit.Mvvm 8.2.2
- **依赖注入**: Microsoft.Extensions.DependencyInjection 8.0.0
- **架构模式**: MVVM (Model-View-ViewModel)

## 项目配置
- **可空引用类型**: 已启用 (`<Nullable>enable</Nullable>`)
- **隐式 Usings**: 已启用 (`<ImplicitUsings>enable</ImplicitUsings>`)

## 目录结构

```
Path/
├── Models/# 数据模型（纯数据，实现 INotifyPropertyChanged）
│   ├── StepModel.cs            # Step 数据模型
│   └── UsvModel.cs       # USV（无人水面艇）数据模型
│
├── ViewModels/    # 视图模型（业务逻辑和命令）
│   └── MainViewModel.cs        # 主窗口视图模型，使用 CommunityToolkit.Mvvm
│
├── Views/         # 视图（XAML 和代码后置）
│   ├── PathEditor3D.xaml  # 3D 路径编辑器用户控件
│   ├── PathEditor3D.xaml.cs    # 3D 编辑器代码后置
│   └── PathEditor3D.Snap.cs    # 3D 编辑器智能捕捉扩展（partial class）
│
├── Services/  # 服务层（业务逻辑和 I/O）
│   ├── IPathDataService.cs     # 路径数据服务接口
│   ├── XmlPathDataService.cs   # XML 数据导入/导出服务实现
│   ├── IDialogService.cs       # 对话框服务接口
│   └── WpfDialogService.cs     # WPF 对话框服务实现
│
├── Helpers/        # 辅助类和工具
│   └── RelayCommand.cs         # ICommand 实现（未使用，项目使用 CommunityToolkit.Mvvm）
│
├── .github/
│   └── copilot-instructions.md # GitHub Copilot 指令说明（中文）
│
├── App.xaml      # 应用程序资源和启动配置
├── App.xaml.cs                 # 应用程序入口，配置依赖注入
├── MainWindow.xaml             # 主窗口 XAML
├── MainWindow.xaml.cs # 主窗口代码后置
├── AssemblyInfo.cs # 程序集信息
└── Path.csproj       # 项目文件

```

## 主要组件说明

### Models（模型层）
- **StepModel**: 表示路径中的一个步骤，包含多个 USV
- **UsvModel**: 表示一个 USV 的位置和属性（X, Y, Z, Yaw, Speed）
- 所有模型都使用 `CallerMemberName` 优化属性变更通知

### ViewModels（视图模型层）
- **MainViewModel**: 主视图模型
  - 使用 `CommunityToolkit.Mvvm` 的 `[ObservableProperty]` 和 `[RelayCommand]`
  - 管理 Steps 集合和当前选中的 Step
  - 提供命令：ImportXml, ExportXml, AddStep, RemoveStep, MoveStepUp, MoveStepDown, AddUsv, RemoveUsv
  - 通过依赖注入获取服务

### Views（视图层）
- **PathEditor3D**: 3D 路径编辑器控件
  - 使用 HelixToolkit 进行 3D 渲染
  - 支持鼠标交互：拖拽、缩放、平移
  - 智能捕捉功能（网格、端点、中点、投影）
  - 视觉坐标和逻辑坐标转换
  - 支持弧线和直线段渲染

### Services（服务层）
- **IPathDataService / XmlPathDataService**: 
  - XML 数据导入/导出
  - 数据验证
  
- **IDialogService / WpfDialogService**:
  - 文件对话框
  - 消息对话框
  - 确认对话框

### 依赖注入配置
在 `App.xaml.cs` 中配置：
```csharp
services.AddSingleton<IPathDataService, XmlPathDataService>();
services.AddSingleton<IDialogService, WpfDialogService>();
services.AddTransient<MainViewModel>();
services.AddTransient<MainWindow>();
```

## 设计模式和最佳实践

### MVVM 模式
- **Model**: 纯数据类，实现 `INotifyPropertyChanged`
- **View**: XAML 文件，仅包含 UI 定义
- **ViewModel**: 业务逻辑、命令和状态管理

### 命令模式
- 使用 `CommunityToolkit.Mvvm` 的 `[RelayCommand]` 特性
- 支持 `CanExecute` 逻辑

### 服务定位
- 通过依赖注入容器管理服务生命周期
- 接口和实现分离，便于测试和替换

### 可空引用类型
- 项目启用了可空引用类型检查
- 使用 `?` 标记可空类型
- 使用 `??` 和 `?.` 进行空值处理

## 编码规范

### 命名约定
- **类名**: PascalCase（如 `MainViewModel`, `PathEditor3D`）
- **方法名**: PascalCase（如 `LoadPath()`, `SaveChanges()`）
- **私有字段**: _camelCase（如 `_currentStep`, `_pathData`）
- **属性**: PascalCase（如 `CurrentPosition`, `IsEnabled`）
- **本地变量**: camelCase

### MVVM 特定规范
- ViewModel 位于 `ViewModels/` 目录
- 使用 `CommunityToolkit.Mvvm` 的源生成器特性
- 数据绑定优先于直接操作 UI

### WPF 特定规范
- XAML 控件使用 `x:Name` 属性命名
- 优先使用布局容器（Grid, StackPanel, DockPanel）
- 样式和模板应该可复用

## 待改进项
1. ~~删除未使用的 `Helpers/RelayCommand.cs`~~（保留以便向后兼容）
2. 添加单元测试
3. 添加错误日志记录
4. 考虑添加撤销/重做功能
5. 优化 3D 渲染性能

## 维护建议
- 遵循项目中的编码规范
- 新功能应该通过服务和接口实现，便于测试
- 保持 MVVM 分层清晰
- 使用依赖注入管理对象生命周期
- 注意内存泄漏，特别是事件订阅和 3D 资源

## 构建和运行
```bash
# 构建项目
dotnet build

# 运行项目
dotnet run
```

## 版本信息
- .NET SDK: 8.0
- 目标框架: net8.0-windows
- C# 语言版本: 默认（C# 12）
