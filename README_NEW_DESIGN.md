# 3D 路径编辑器 - STEP 导入与放样

## 项目概述

这是一个基于 WPF 和 .NET 8 的 3D 路径编辑器，专门用于从 STEP 文件导入曲线、进行放样处理，并生成 USV 路径规划数据。

## 核心功能

### 1. STEP 文件导入
- 支持导入 `.step` 和 `.stp` 格式的 3D CAD 文件
- 自动提取文件中的线条（曲线）数据
- 将曲线转换为 3D 点集合

### 2. 曲线管理
- **自动命名**：按 `usv_01`, `usv_02` ... 格式自动命名所有曲线
- **手动重命名**：选择曲线后可以单独重命名
- **曲线属性设置**：可以为每条曲线设置不同的放样点数
- **删除曲线**：可以删除单条曲线或清除所有曲线

### 3. 放样功能
- **单曲线放样**：对选中的曲线进行放样处理
- **批量放样**：一次性对所有曲线进行放样
- **等距采样**：按设定的点数量，在曲线上生成等距分布的控制点
- **可视化标记**：已放样的曲线会显示 ? 标记

### 4. 步骤生成
- 从已放样的曲线自动生成步骤列表
- 每个步骤包含所有曲线在该位置的 USV 数据
- 自动计算坐标（X, Y, Z）
- 自动计算航向角（Yaw），基于曲线切线方向

### 5. XML 导出
- 将生成的步骤列表导出为 XML 格式
- 兼容现有的路径规划系统

## 工作流程

```
1. 导入 STEP 文件
   ↓
2. 查看导入的曲线列表
   ↓
3. 自动或手动命名曲线（usv_01, usv_02...）
   ↓
4. 设置放样点数量
   ↓
5. 选择曲线 → 单击"放样"
   ↓
6. 对所有曲线执行放样
   ↓
7. 点击"生成步骤"
   ↓
8. 查看生成的步骤列表和 USV 数据
   ↓
9. 导出 XML 文件
```

## 快捷键

- `Ctrl+I` - 导入 STEP 文件
- `Ctrl+L` - 放样选中曲线
- `Ctrl+Shift+L` - 放样所有曲线
- `Ctrl+G` - 生成步骤列表
- `Ctrl+S` - 导出 XML
- `Ctrl+O` - 导入 XML

## 界面布局

```
┌─────────────────────────────────────────────────────────┐
│  菜单栏 / 工具栏        │
├──────────┬──────────────┬──────────────────────────────┤
│ 曲线列表 │  步骤列表    │         │
│          │    │           │
│ - usv_01 │  ┌─ Step 1   │      3D 视图窗口         │
│ - usv_02 │  └─ Step 2   │     │
│ - usv_03 │    │       │
│   │──────────────│        │
│ [放样点数]│  USV 数据表  │      │
│ [10    ] │   │        │
│          │  Id X Y Z... │        │
└──────────┴──────────────┴──────────────────────────────┘
│  状态栏：曲线数 | 步骤数    │
└─────────────────────────────────────────────────────────┘
```

## 技术架构

### 核心类

- **PathCurveModel**: 路径曲线模型
  - 存储原始点集和放样后的点集
  - 管理曲线名称和属性

- **StepModel**: 步骤模型
  - 包含该步骤的所有 USV 数据

- **UsvModel**: USV 数据模型
  - 存储 ID、坐标、航向角、速度等

### 服务接口

- **IStepImportService**: STEP 文件导入服务
- **ILoftService**: 曲线放样服务
- **IPathDataService**: XML 数据导入导出服务
- **IDialogService**: 对话框服务

## 注意事项

### STEP 文件解析

当前版本使用**模拟数据**进行测试。完整的 STEP 文件解析需要专业库支持，推荐：

- **Open CASCADE Technology (OCCT)** - 开源 CAD 内核
- **StepCode** - ISO 10303 (STEP) 文件解析库

### 实现真实 STEP 解析的方法

要实现真正的 STEP 文件解析，需要：

1. 安装 OCCT 或 StepCode 库
2. 在 `StepImportService.cs` 中实现 `ImportStepFile` 方法
3. 提取 STEP 文件中的 B-Spline 曲线或多段线
4. 转换为 `Point3DCollection`

示例代码框架（伪代码）：

```csharp
public List<Point3DCollection> ImportStepFile(string filePath)
{
 // 使用 OCCT 读取 STEP 文件
    var stepReader = new STEPControl_Reader();
    stepReader.ReadFile(filePath);
    stepReader.TransferRoots();
    
    var shape = stepReader.OneShape();
    
    // 提取所有边（曲线）
    var curves = ExtractCurvesFromShape(shape);
    
    // 转换为 Point3DCollection
    return ConvertToPoint3DCollections(curves);
}
```

## 依赖项

- **.NET 8** (net8.0-windows)
- **WPF** (Windows Presentation Foundation)
- **HelixToolkit.Wpf** 2.22.0 - 3D 可视化
- **CommunityToolkit.Mvvm** 8.2.2 - MVVM 框架
- **Microsoft.VisualBasic** - InputBox 支持

## 开发计划

- [ ] 集成真实的 STEP 文件解析库
- [ ] 在 3D 视图中实时显示曲线
- [ ] 支持曲线选择和交互操作
- [ ] 支持手动调整放样点
- [ ] 添加更多曲线属性（颜色、线型等）
- [ ] 支持撤销/重做操作
- [ ] 添加预览功能
- [ ] 支持更多 CAD 格式（IGES, DXF 等）

## 许可证

本项目遵循 MIT 许可证。
