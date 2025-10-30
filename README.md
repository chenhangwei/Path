# 3D 路径编辑器

基于 .NET 8 和 WPF 的 3D 路径规划和可视化编辑工具，专为 USV（无人水面艇）路径规划设计。

## ? 特性

- ?? **专业 3D 编辑**: 基于 HelixToolkit.Wpf 的强大 3D 可视化
- ?? **UG NX 风格操作**: 符合专业 CAD 软件习惯的交互方式
- ?? **智能捕捉系统**: 网格/点/中点/端点/投影多种捕捉模式
- ? **实时刷新**: 数据修改后 3D 视图即时更新
- ?? **直观控制**: 
  - 中键旋转视图
  - Shift+中键平移
  - 滚轮智能缩放
  - 快捷键视图切换
- ?? **数据持久化**: XML 格式导入/导出
- ??? **MVVM 架构**: 清晰的代码组织和可维护性

## ?? 截图

*(待添加截图)*

## ?? 快速开始

### 系统要求

- Windows 10/11 (64-bit)
- [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

### 安装

1. 从 [Releases](https://github.com/chenhangwei/Path/releases) 下载最新版本
2. 解压到任意目录
3. 运行 `Path.exe`

或者从源码编译：

```bash
git clone https://github.com/chenhangwei/Path.git
cd Path
dotnet build
dotnet run
```

### 基本使用

1. **添加步骤**: 点击 `? 添加步骤` 或按 `Ctrl+N`
2. **添加 USV**: 点击 `?? 添加 USV` 或按 `Ctrl+U`
3. **编辑数据**: 在中间的数据表格中编辑坐标和参数
4. **查看 3D**: 右侧 3D 视图自动显示路径
5. **保存项目**: 点击 `?? 导出` 或按 `Ctrl+S`

详细使用说明请参阅 [快速开始指南](docs/03-用户手册/快速开始.md)

## ?? 操作指南

### 鼠标操作

| 操作 | 功能 |
|------|------|
| 中键拖动 | 旋转视图 |
| Shift + 中键 | 平移视图 |
| 滚轮 | 缩放视图 |
| 左键单击 | 选择控制点 |
| 左键拖动 | 移动控制点 |

### 键盘快捷键

| 快捷键 | 功能 |
|--------|------|
| `F` | 适应窗口 |
| `Ctrl+1/2/3/4` | 切换视图（顶/前/右/等轴测） |
| `Home` | 重置视图 |
| `F9/F10/F11` | 切换捕捉模式 |
| `Ctrl+O` | 导入文件 |
| `Ctrl+S` | 导出文件 |
| `Ctrl+N` | 添加步骤 |
| `Ctrl+U` | 添加 USV |

完整操作手册: [UG NX 操作指南](docs/03-用户手册/UGNX操作指南.md)

## ?? 文档

完整的文档体系，方便查找和学习：

### ?? 项目概览
- [项目结构](docs/01-项目概览/项目结构.md) - 详细的项目架构说明
- [技术栈](docs/01-项目概览/技术栈.md) - 使用的技术和框架

### ??? 开发指南
- [3D编辑器迁移指南](docs/02-开发指南/功能实现/3D编辑器迁移指南.md)
- [智能捕捉集成指南](docs/02-开发指南/功能实现/智能捕捉集成指南.md)
- [实时刷新指南](docs/02-开发指南/功能实现/实时刷新指南.md)
- [UG NX风格优化](docs/02-开发指南/功能实现/UGNX风格优化.md)
- [Siemens风格改进](docs/02-开发指南/功能实现/Siemens风格改进.md)

### ?? 用户手册
- [快速开始](docs/03-用户手册/快速开始.md) - 新用户入门指南
- [UG NX操作指南](docs/03-用户手册/UGNX操作指南.md) - 完整操作手册
- [智能捕捉功能](docs/03-用户手册/智能捕捉功能.md) - 精确定位技巧

### ?? 维护日志
- [重构日志](docs/04-维护日志/重构日志.md) - 代码重构记录
- [UG NX优化总结](docs/04-维护日志/UGNX优化总结.md) - 优化过程记录
- [变更历史](docs/04-维护日志/变更历史.md) - 版本更新历史

**?? 完整文档导航**: [docs/README.md](docs/README.md)

## ??? 技术栈

- **框架**: .NET 8.0
- **UI**: WPF (Windows Presentation Foundation)
- **3D 渲染**: HelixToolkit.Wpf 2.22.0
- **MVVM**: CommunityToolkit.Mvvm 8.4.0
- **依赖注入**: Microsoft.Extensions.DependencyInjection 8.0.0
- **架构模式**: MVVM

详细说明: [技术栈文档](docs/01-项目概览/技术栈.md)

## ?? 项目结构

```
Path/
├── docs/   # ?? 完整文档
│   ├── 01-项目概览/# 项目介绍和架构
│   ├── 02-开发指南/   # 开发者文档
│├── 03-用户手册/   # 用户操作指南
│   └── 04-维护日志/   # 变更和维护记录
├── Models/            # 数据模型
├── ViewModels/   # 视图模型
├── Views/       # 用户界面
│   └── PathEditor3D/  # 3D 编辑器组件
├── Services/  # 服务层
└── Helpers/           # 辅助工具

```

详细结构: [项目结构文档](docs/01-项目概览/项目结构.md)

## ?? 贡献

欢迎贡献！请遵循以下步骤：

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

### 开发规范

请参阅 [编码规范](.github/copilot-instructions.md) 了解：
- C# 代码规范
- MVVM 模式要求
- WPF 最佳实践
- 命名约定

## ?? 许可证

本项目采用 Apache License 2.0 许可证 - 详见 [LICENSE](LICENSE) 文件

## ?? 致谢

- [HelixToolkit](https://github.com/helix-toolkit/helix-toolkit) - 强大的 WPF 3D 工具包
- [CommunityToolkit](https://github.com/CommunityToolkit/dotnet) - MVVM 工具集
- UG NX - 操作风格灵感来源

## ?? 联系方式

- **问题反馈**: [GitHub Issues](https://github.com/chenhangwei/Path/issues)
- **功能建议**: [GitHub Discussions](https://github.com/chenhangwei/Path/discussions)
- **项目主页**: [https://github.com/chenhangwei/Path](https://github.com/chenhangwei/Path)

---

**? 如果觉得这个项目对您有帮助，请给个 Star！**

---

**版本**: 2.0.0  
**最后更新**: 2024-12-22  
**维护者**: 项目团队