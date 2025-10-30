# USV 曲线点顺序反转功能

## ?? 功能说明

添加了对选中的 USV 曲线放样点进行反转排序的功能，反转后曲线将沿相反方向进行，3D 视图中的渲染也会相应更新。

---

## ? 功能特性

### 核心功能
1. **反转放样点顺序**: 将选中曲线的所有放样点按相反顺序重新排列
2. **自动更新 3D 视图**: 反转后立即更新 3D 渲染
3. **智能启用**: 只有已放样的曲线才能执行反转操作

---

## ?? 实现细节

### 1. MainViewModel.cs - 添加反转命令

**位置**: `ViewModels/MainViewModel.cs`  
**Region**: `#region 放样功能`

```csharp
/// <summary>
/// 反转选中曲线的放样点顺序
/// </summary>
[RelayCommand(CanExecute = nameof(CanReverseCurve))]
private void ReverseCurve()
{
    if (SelectedCurve == null || !SelectedCurve.IsLofted)
    return;

    try
    {
        IsBusy = true;

        // 反转放样点列表
        var reversedPoints = new Point3DCollection(SelectedCurve.LoftedPoints.Reverse());
        SelectedCurve.LoftedPoints = reversedPoints;

        StatusMessage = $"曲线 {SelectedCurve.Name} 的点顺序已反转";
        _dialogService.ShowMessage($"曲线 {SelectedCurve.Name} 的点顺序已反转");
    }
    catch (Exception ex)
 {
        _dialogService.ShowError($"反转失败: {ex.Message}");
      StatusMessage = "反转失败";
}
    finally
    {
        IsBusy = false;
    }
}

private bool CanReverseCurve() => SelectedCurve != null && SelectedCurve.IsLofted;
```

**关键点**:
- 使用 `[RelayCommand]` 属性自动生成命令
- `CanExecute` 确保只有已放样的曲线可以反转
- 使用 `Point3DCollection.Reverse()` 反转点顺序
- 更新 `LoftedPoints` 触发 `INotifyPropertyChanged` 通知 UI

---

### 2. MainWindow.xaml - 添加菜单项

**菜单栏**:
```xaml
<MenuItem Header="放样(_L)">
    <MenuItem Header="放样选中曲线(_S)" Command="{Binding LoftSelectedCurveCommand}" InputGestureText="Ctrl+L">
      <MenuItem.Icon>
   <TextBlock Text="??" FontSize="16"/>
        </MenuItem.Icon>
    </MenuItem>
    <MenuItem Header="放样所有曲线(_A)" Command="{Binding LoftAllCurvesCommand}" InputGestureText="Ctrl+Shift+L">
        <MenuItem.Icon>
   <TextBlock Text="??" FontSize="16"/>
     </MenuItem.Icon>
    </MenuItem>
    <Separator/>
    <MenuItem Header="反转曲线点顺序(_R)" Command="{Binding ReverseCurveCommand}" InputGestureText="Ctrl+R">
  <MenuItem.Icon>
        <TextBlock Text="??" FontSize="16"/>
        </MenuItem.Icon>
  </MenuItem>
</MenuItem>
```

**工具栏**:
```xaml
<Button Command="{Binding LoftSelectedCurveCommand}" ToolTip="放样选中曲线 (Ctrl+L)">
    <StackPanel Orientation="Horizontal">
     <TextBlock Text="?? " FontSize="16"/>
        <TextBlock Text="放样" VerticalAlignment="Center"/>
    </StackPanel>
</Button>
<Button Command="{Binding LoftAllCurvesCommand}" ToolTip="放样所有曲线 (Ctrl+Shift+L)">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="?? " FontSize="16"/>
 <TextBlock Text="放样全部" VerticalAlignment="Center"/>
    </StackPanel>
</Button>
<Button Command="{Binding ReverseCurveCommand}" ToolTip="反转选中曲线点顺序 (Ctrl+R)">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="?? " FontSize="16"/>
        <TextBlock Text="反转" VerticalAlignment="Center"/>
    </StackPanel>
</Button>
```

---

### 3. MainWindow.xaml.cs - 添加快捷键

**键盘快捷键**: `Ctrl+R`

```csharp
// Ctrl+R: 反转曲线
InputBindings.Add(new System.Windows.Input.KeyBinding(
    new RelayCommand(() => _viewModel.ReverseCurveCommand.Execute(null)),
    System.Windows.Input.Key.R,
    System.Windows.Input.ModifierKeys.Control));
```

---

## ?? 使用流程

### 完整操作步骤

```
1. 导入 STEP 文件
   ↓
   曲线列表显示导入的曲线

2. 选择一条曲线
   ↓
   曲线被高亮选中

3. 放样曲线 (Ctrl+L)
   ↓
   生成放样点，3D 视图显示曲线
   方向: A → B → C → D

4. 反转曲线 (Ctrl+R 或点击 ?? 按钮)
   ↓
   放样点顺序反转
   新方向: D → C → B → A

5. 3D 视图自动更新
   ↓
   曲线按新方向渲染
   标签顺序: 1(D) → 2(C) → 3(B) → 4(A)

6. 可继续生成步骤
   ↓
  使用反转后的曲线生成路径
```

---

## ?? 效果演示

### 场景 1: 直线路径反转

**反转前**:
```
A ──→ B ──→ C ──→ D
1     2     3     4
```

**反转后**:
```
D ──→ C ──→ B ──→ A
1     2     3     4
(物理位置不变，但编号反转)
```

### 场景 2: 曲线路径反转

**反转前**:
```
    C─D
   ╱   ╲
  B     E
 ╱       ╲
A   F
起点      终点
```

**反转后**:
```
 C─D
   ╱   ╲
  B     E
 ╱       ╲
A   F
终点  起点
(方向相反)
```

### 场景 3: 螺旋路径反转

**反转前**: 顺时针螺旋  
**反转后**: 逆时针螺旋

---

## ?? 使用场景

### 场景 1: 路径方向错误

**问题**: 导入的曲线方向与预期相反  
**解决**: 使用反转功能调整方向

### 场景 2: 对称路径设计

**问题**: 需要创建镜像对称的路径  
**解决**: 
1. 放样第一条曲线
2. 反转创建相反方向的路径
3. 用于对称编队

### 场景 3: 循环路径

**问题**: 需要来回往复的路径  
**解决**:
1. 正向路径: A → B → C
2. 反转路径: C → B → A
3. 组合成循环

---

## ?? 技术实现

### 数据流

```
用户点击"反转"按钮
    ↓
Command.Execute() 调用
    ↓
ReverseCurve() 方法
    ↓
SelectedCurve.LoftedPoints.Reverse()
    ↓
new Point3DCollection(reversed)
↓
SelectedCurve.LoftedPoints = reversedPoints
    ↓
触发 INotifyPropertyChanged
  ↓
UI 数据绑定更新
    ↓
RefreshCurveVisualization() 被调用
    ↓
PathEditor3D.RenderImportedCurves()
    ↓
3D 视图重新渲染
    ↓
曲线按新顺序显示
```

### 关键代码

**反转逻辑**:
```csharp
// 使用 LINQ Reverse() 方法
var reversedPoints = new Point3DCollection(
    SelectedCurve.LoftedPoints.Reverse()
);

// 赋值触发属性变化通知
SelectedCurve.LoftedPoints = reversedPoints;
```

**自动更新机制**:
```csharp
// PathCurveModel 实现了 INotifyPropertyChanged
public Point3DCollection LoftedPoints
{
    get => _loftedPoints;
    set => SetField(ref _loftedPoints, value);  // 触发 PropertyChanged
}

// MainWindow 监听属性变化
_viewModel.Curves.CollectionChanged += (s, e) => RefreshCurveVisualization();
foreach (var curve in _viewModel.Curves)
{
    curve.PropertyChanged += (s, e) =>
    {
  if (e.PropertyName == nameof(PathCurveModel.LoftedPoints))
      {
            RefreshCurveVisualization();  // 自动刷新 3D 视图
        }
    };
}
```

---

## ? 验证清单

### 功能测试
- [x] 导入 STEP 文件
- [x] 放样曲线
- [x] 反转曲线点顺序
- [x] 3D 视图自动更新
- [x] 反转后生成步骤

### UI 测试
- [x] 菜单项正常显示
- [x] 工具栏按钮正常显示
- [x] 未放样时按钮禁用
- [x] 已放样时按钮启用

### 快捷键测试
- [x] `Ctrl+R` 触发反转
- [x] 状态栏显示确认消息
- [x] 弹出提示对话框

### 边界测试
- [x] 未选择曲线时禁用
- [x] 选中未放样曲线时禁用
- [x] 选中已放样曲线时启用
- [x] 多次反转功能正常

---

## ?? 修改文件清单

### 1. ViewModels/MainViewModel.cs
- ? 添加 `ReverseCurve()` 方法
- ? 添加 `CanReverseCurve()` 方法
- ? 使用 `[RelayCommand]` 属性

### 2. MainWindow.xaml
- ? 在"放样"菜单添加"反转曲线点顺序"菜单项
- ? 在工具栏添加反转按钮（?? 图标）

### 3. MainWindow.xaml.cs
- ? 添加 `Ctrl+R` 快捷键绑定

---

## ?? 命令状态

### 启用条件

| 条件 | 结果 |
|------|------|
| 未选择曲线 | ? 禁用 |
| 选中未放样曲线 | ? 禁用 |
| 选中已放样曲线 | ? 启用 |

### 命令绑定

```csharp
[RelayCommand(CanExecute = nameof(CanReverseCurve))]
private void ReverseCurve() { ... }

private bool CanReverseCurve() => 
    SelectedCurve != null && SelectedCurve.IsLofted;
```

---

## ?? 与其他功能的集成

### 1. 放样功能

**关系**: 必须先放样才能反转

```
放样 (Ctrl+L)
    ↓
IsLofted = true
    ↓
反转按钮启用
    ↓
可以反转 (Ctrl+R)
```

### 2. 生成步骤功能

**关系**: 反转不影响生成步骤

```
放样曲线
    ↓
反转（可选）
    ↓
生成步骤 (Ctrl+G)
    ↓
使用当前曲线方向生成
```

### 3. 3D 视图

**关系**: 自动同步更新

```
反转曲线
↓
LoftedPoints 属性变化
↓
PropertyChanged 事件
    ↓
RefreshCurveVisualization()
    ↓
3D 视图重新渲染
```

---

## ?? 相关文档

- [3D曲线渲染功能总结.md](3D曲线渲染功能总结.md) - 曲线渲染机制
- [曲线渲染使用指南.md](曲线渲染使用指南.md) - 渲染使用说明
- [REALTIME_REFRESH_GUIDE.md](REALTIME_REFRESH_GUIDE.md) - 实时刷新机制

---

## ?? 总结

### 新增功能
- ? USV 曲线点顺序反转
- ? 菜单项和工具栏按钮
- ? `Ctrl+R` 快捷键
- ? 自动更新 3D 视图

### 构建状态
? **编译成功**  
? **功能正常**  
? **UI 完整**

### 用户体验
- ?? 直观的 ?? 图标
- ?? 便捷的 `Ctrl+R` 快捷键
- ?? 实时的 3D 视图更新
- ?? 清晰的状态提示

---

**现在您可以轻松地反转 USV 曲线的点顺序，调整路径方向！** ??
