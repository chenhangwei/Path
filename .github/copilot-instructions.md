# GitHub Copilot 项目指导说明

## 项目概述
这是一个基于 .NET 8 的 WPF 应用程序，用于 3D 路径编辑和可视化。

## 技术栈
- **框架**: .NET 8 (net8.0-windows)
- **UI 框架**: WPF (Windows Presentation Foundation)
- **3D 渲染**: HelixToolkit.Wpf 2.22.0
- **架构模式**: MVVM (Model-View-ViewModel)

## 编码规范和最佳实践

### C# 代码规范
1. **空值处理**: 项目启用了 Nullable 引用类型 (`<Nullable>enable</Nullable>`)
   - 始终明确标注可空类型 (`string?` vs `string`)
   - 使用 null 合并运算符 `??` 和 null 条件运算符 `?.`
   - 避免使用 null 抑制运算符 `!` 除非绝对必要

2. **Using 声明**: 项目启用了隐式 usings (`<ImplicitUsings>enable</ImplicitUsings>`)
   - 不需要添加常见的 System 命名空间
   - 只导入项目特定的命名空间

3. **命名约定**:
   - 类名使用 PascalCase: `MainViewModel`, `PathEditor`
   - 方法名使用 PascalCase: `LoadPath()`, `SaveChanges()`
   - 私有字段使用 camelCase 或 _camelCase: `_currentStep`, `pathData`
   - 属性使用 PascalCase: `CurrentPosition`, `IsEnabled`

### MVVM 架构要求

1. **ViewModels 组织**:
   - 所有 ViewModel 放在 `ViewModels/` 目录
   - ViewModel 应继承 `INotifyPropertyChanged` 或使用 MVVM 框架基类
   - 使用 `RelayCommand` (位于 `Helpers/RelayCommand.cs`) 处理命令

2. **Views 组织**:
   - 所有 View 放在 `Views/` 目录
   - XAML 文件和代码隐藏文件成对出现
   - 代码隐藏应尽量简洁，业务逻辑放在 ViewModel

3. **数据绑定**:
   - 优先使用数据绑定而非代码直接操作 UI
   - 使用 `{Binding}` 语法连接 View 和 ViewModel
   - 命令绑定使用 `ICommand` 接口

### WPF 特定指导

1. **XAML 代码风格**:
- 使用有意义的 `x:Name` 属性
   - 合理使用 Grid, StackPanel, DockPanel 等布局容器
   - 样式和模板应考虑复用性

2. **3D 渲染 (HelixToolkit)**:
   - 使用 HelixToolkit 的 HelixViewport3D 控件
   - 3D 模型操作应在 ViewModel 中处理
   - 注意 3D 对象的性能优化

### 项目特定规则

1. **Step 类**:
   - 有两个 Step 类文件 (`Step.cs` 和 `ViewModels/Step.cs`)
   - 明确区分领域模型和视图模型
   - 避免循环依赖

2. **Helper 类**:
   - 辅助类放在 `Helpers/` 目录
   - 保持辅助类的单一职责
   - `RelayCommand` 用于实现 ICommand 模式

3. **性能考虑**:
   - 3D 渲染可能消耗资源，注意内存管理
 - 正确实现 `IDisposable` 释放非托管资源
   - 大量数据绑定时使用虚拟化技术

### 代码质量要求

1. **异步编程**:
   - 使用 `async/await` 处理耗时操作
   - 避免阻塞 UI 线程
   - 异步方法名以 `Async` 结尾

2. **异常处理**:
   - 使用 try-catch 处理预期异常
   - 记录异常信息便于调试
   - 向用户显示友好的错误消息

3. **代码注释**:
   - 公共 API 使用 XML 文档注释 (`///`)
   - 复杂算法添加解释性注释
   - 避免显而易见的注释

### 测试建议
- 为 ViewModel 编写单元测试
- 测试数据绑定和命令执行逻辑
- 使用 Mock 对象隔离依赖

## 文件组织结构
```
Path/
├── Views/    # XAML 视图文件
├── ViewModels/    # 视图模型
├── Helpers/   # 辅助类和工具
├── Models/         # (建议) 领域模型
└── Services/         # (建议) 业务服务层
```

## 依赖注入建议
考虑引入依赖注入容器 (如 Microsoft.Extensions.DependencyInjection) 以提高代码的可测试性和可维护性。

## 版本控制
- 遵循 Git 最佳实践
- 提交信息应清晰描述更改内容
- 不要提交 `bin/` 和 `obj/` 目录
