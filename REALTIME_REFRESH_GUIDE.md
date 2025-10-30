# USV 数据实时刷新和控制点大小调整实现指南

## 概述

本指南说明如何在 3D 路径编辑器中实现：
1. **USV 数据实时刷新**（无需重新加载整个场景）
2. **控制点大小自适应**（根据相机距离或窗口大小）
3. **用户可调节控制点大小**（通过滑块）

## 一、PathEditor3D.xaml.cs 需要添加的代码

### 1. 添加控制点大小属性（在类字段区域）

```csharp
// 控制点大小设置
private double _pointSizeScale = 1.0; // 控制点大小缩放因子 (0.5 到 3.0)
private bool _autoScalePoints = true; // 是否自动根据视图距离缩放控制点

public double PointSizeScale 
{ 
    get => _pointSizeScale; 
    set 
    { 
        _pointSizeScale = Math.Clamp(value, 0.5, 3.0);
        RefreshPointSizes(); // 大小改变时刷新所有控制点
    } 
}

public bool AutoScalePoints 
{ 
    get => _autoScalePoints; 
    set 
    { 
        _autoScalePoints = value;
        RefreshPointSizes();
    } 
}
```

### 2. 添加实时刷新方法

```csharp
// 实时刷新 USV 数据（不清空场景，只更新）
public void RefreshUsvData(List<UsvDto> usvs)
{
    // 保存当前相机状态
    var savedCamera = HelixView.Camera;

    // 只清理控制点和线段，保留网格和平面
    foreach (var pv in _pointVisuals) SceneRoot.Children.Remove(pv);
    foreach (var lv in _lineVisuals) SceneRoot.Children.Remove(lv);
    foreach (var sv in _sampleVisuals) SceneRoot.Children.Remove(sv);

    _pointVisuals.Clear();
    _lineVisuals.Clear();
    _sampleVisuals.Clear();
    _modelMap.Clear();
    _indexToSphere.Clear();
    _indexToLabel.Clear();
    _logicalPositions.Clear();

    _segToLines.Clear(); 
    _segOriginalAppearance.Clear();
    _lastUsvs = usvs;

    // 重新渲染数据
 RenderUsvData(usvs);

    // 恢复相机状态
    HelixView.Camera = savedCamera;
}
```

### 3. 提取渲染逻辑为独立方法

将 `LoadFrom2D` 中的USV渲染代码提取到 `RenderUsvData` 方法中。在现有的循环中，修改控制点创建部分：

```csharp
// 在创建控制点时使用自适应大小
for (int pi = 0; pi < seg.ControlPoints.Count; pi++)
{
    var p = seg.ControlPoints[pi];
    var visualCenter = new Point3D(
     p.X * _visualScale, 
        (_invertY ? -1.0 : 1.0) * p.Y * _visualScale, 
      p.Z * _visualScale
    );

    // 计算自适应大小
    var radius = CalculatePointRadius(visualCenter, pi == 0);

    var sphere = new SphereVisual3D
    {
        Center = visualCenter,
    Radius = radius,
        Fill = new SolidColorBrush(u.Color)
    };
    // ... 其余代码
}
```

### 4. 添加计算控制点半径的方法

```csharp
// 计算控制点半径（支持自适应和手动缩放）
private double CalculatePointRadius(Point3D position, bool isFirstPoint)
{
    double baseRadius = (isFirstPoint ? 5 : 4) * _visualScale;

    if (_autoScalePoints && HelixView.Camera is PerspectiveCamera cam)
    {
  // 根据相机距离自动缩放
    var distance = (cam.Position - position).Length;
        var scaleFactor = Math.Max(0.5, Math.Min(2.0, distance / 100.0));
        baseRadius *= scaleFactor;
    }

    // 应用用户设置的缩放因子
    return baseRadius * _pointSizeScale;
}
```

### 5. 添加刷新所有控制点大小的方法

```csharp
// 刷新所有控制点大小
private void RefreshPointSizes()
{
    foreach (var kvp in _indexToSphere)
    {
        var key = kvp.Key;
   var sphere = kvp.Value;

    if (_logicalPositions.TryGetValue(key, out var logical))
        {
            var visualCenter = new Point3D(
  logical.X * _visualScale, 
          (_invertY ? -1.0 : 1.0) * logical.Y * _visualScale, 
       logical.Z * _visualScale
          );

     // 判断是否是第一个点
      bool isFirstPoint = (key.pi == 0);
            sphere.Radius = CalculatePointRadius(visualCenter, isFirstPoint);
      }
    }

    // 刷新采样点大小（如果需要）
    // ...
}
```

### 6. 在相机事件中触发刷新

在以下事件处理器末尾添加刷新调用：

```csharp
// 在 HelixView_PreviewMouseMove 中（旋转和平移后）
if (_autoScalePoints) RefreshPointSizes();

// 在 HelixView_MouseWheel 中（缩放后）
if (_autoScalePoints) RefreshPointSizes();
```

## 二、MainWindow.xaml 添加设置面板

在 3D 编辑器的容器中添加：

```xaml
<Border Grid.Column="4" BorderBrush="#CCCCCC" BorderThickness="1" CornerRadius="4" Background="White">
    <DockPanel>
        <!-- 3D 视图设置面板 -->
        <Expander DockPanel.Dock="Top" Header="?? 3D 视图设置" IsExpanded="False" 
          Background="#F5F5F5" BorderBrush="#CCCCCC" BorderThickness="0,0,0,1">
       <StackPanel Margin="8" Background="White">
      <!-- 控制点大小设置 -->
       <TextBlock Text="控制点大小" FontWeight="SemiBold" Margin="0,0,0,4"/>
      <Grid Margin="0,0,0,8">
       <Grid.ColumnDefinitions>
     <ColumnDefinition Width="Auto"/>
      <ColumnDefinition Width="*"/>
 <ColumnDefinition Width="Auto"/>
              </Grid.ColumnDefinitions>
        <TextBlock Grid.Column="0" Text="0.5x" VerticalAlignment="Center" Margin="0,0,8,0"/>
   <Slider Grid.Column="1" x:Name="PointSizeSlider" 
    Minimum="0.5" Maximum="3.0" Value="1.0" 
      TickFrequency="0.1" IsSnapToTickEnabled="True"
         VerticalAlignment="Center"
  ValueChanged="PointSizeSlider_ValueChanged"/>
<TextBlock Grid.Column="2" 
             Text="{Binding Value, ElementName=PointSizeSlider, StringFormat={}{0:F1}x}" 
                VerticalAlignment="Center" Margin="8,0,0,0" FontWeight="Bold" MinWidth="40"/>
                </Grid>

      <!-- 自动缩放选项 -->
 <CheckBox x:Name="AutoScaleCheckBox" Content="自动根据视图距离缩放" 
    IsChecked="True" Margin="0,0,0,8"
         Checked="AutoScaleCheckBox_Changed" 
   Unchecked="AutoScaleCheckBox_Changed"/>

<!-- 快捷键提示 -->
   <Border Background="#E3F2FD" BorderBrush="#2196F3" BorderThickness="1" 
         CornerRadius="4" Padding="8" Margin="0,8,0,0">
       <StackPanel>
  <TextBlock Text="?? 快捷键" FontWeight="Bold" Foreground="#1976D2" Margin="0,0,0,4"/>
    <TextBlock TextWrapping="Wrap" FontSize="11">
        <Run Text="F"/> <Run Text=" - 适应窗口" Foreground="#666666"/><LineBreak/>
        <Run Text="Ctrl+1/2/3/4"/> <Run Text=" - 切换视图" Foreground="#666666"/><LineBreak/>
       <Run Text="Home"/> <Run Text=" - 重置视图" Foreground="#666666"/>
                </TextBlock>
           </StackPanel>
  </Border>
            </StackPanel>
     </Expander>

    <!-- 3D 编辑器 -->
        <views:PathEditor3D x:Name="PathEditor3DControl"/>
    </DockPanel>
</Border>
```

##三、MainWindow.xaml.cs 添加事件处理

### 1. 添加事件处理方法

```csharp
// 控制点大小滑块改变事件
private void PointSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
{
 var editor3D = FindName("PathEditor3DControl") as PathEditor3D;
    if (editor3D != null)
    {
        editor3D.PointSizeScale = e.NewValue;
    _viewModel.StatusMessage = $"控制点大小: {e.NewValue:F1}x";
    }
}

// 自动缩放复选框改变事件
private void AutoScaleCheckBox_Changed(object sender, RoutedEventArgs e)
{
    var editor3D = FindName("PathEditor3DControl") as PathEditor3D;
 var checkBox = sender as CheckBox;
    if (editor3D != null && checkBox != null)
    {
editor3D.AutoScalePoints = checkBox.IsChecked == true;
  _viewModel.StatusMessage = checkBox.IsChecked == true 
        ? "已启用自动缩放" 
     : "已禁用自动缩放";
    }
}
```

### 2. 添加数据变化监听

```csharp
public MainWindow(MainViewModel viewModel)
{
  InitializeComponent();
    _viewModel = viewModel;
    DataContext = _viewModel;

    // 监听数据更改以实时刷新 3D 视图
    _viewModel.PropertyChanged += ViewModel_PropertyChanged;

    // 监听 DataGrid 的变化
    this.Loaded += (s, e) =>
    {
   var usvGrid = FindName("UsvGrid") as DataGrid;
  if (usvGrid != null)
{
            // 监听单元格编辑结束事件
     usvGrid.CellEditEnding += UsvGrid_CellEditEnding;
        }
    };
}

// ViewModel 属性改变处理
private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
{
    if (e.PropertyName == nameof(MainViewModel.SelectedStep))
    {
        RefreshPathEditor3D();
    }
}

// USV 数据编辑结束时实时刷新
private void UsvGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
{
    // 使用 Dispatcher 延迟刷新，确保数据已经更新
    Dispatcher.BeginInvoke(new Action(() =>
    {
        RefreshPathEditor3D();
    _viewModel.StatusMessage = "USV 数据已更新";
    }), System.Windows.Threading.DispatcherPriority.Background);
}

// 刷新 3D 编辑器的辅助方法
private void RefreshPathEditor3D()
{
    try
    {
     var pathEditor3D = FindName("PathEditor3DControl") as PathEditor3D;
      if (pathEditor3D != null && _viewModel.Steps.Count > 0)
        {
         var usvDtos = ConvertToUsvDtos(_viewModel.Steps);
            pathEditor3D.RefreshUsvData(usvDtos); // 使用实时刷新
        }
    }
 catch { }
}
```

### 3. 添加必需的 using 引用

```csharp
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
```

## 四、使用方法

### 1. 实时刷新
- 在 USV 数据网格中输入/修改数据后，3D 视图会自动刷新
- 刷新不会改变当前的相机位置和角度
- 刷新只更新控制点和线段，网格保持不变

### 2. 调整控制点大小
- **手动调整**：拖动"控制点大小"滑块（0.5x 到 3.0x）
- **自动缩放**：勾选"自动根据视图距离缩放"
- 当相机靠近时，控制点会变小
  - 当相机远离时，控制点会变大
  - 保持控制点在屏幕上的视觉大小相对恒定

### 3. 组合使用
- 可以同时启用自动缩放和手动缩放
- 手动缩放系数会应用在自动计算的大小之上
- 例如：设置2.0x，所有控制点会是默认大小的2倍

## 五、注意事项

1. **性能优化**：
   - `RefreshUsvData` 只更新必要的视觉元素
   - 避免在每次鼠标移动时都调用 `RefreshPointSizes`
   - 仅在相机停止移动或缩放结束时刷新

2. **线程安全**：
   - DataGrid 的编辑事件使用 `Dispatcher.BeginInvoke` 延迟执行
 - 确保数据已提交后再刷新 3D 视图

3. **用户体验**：
   - 提供即时反馈（状态栏消息）
   - 保持相机状态不变
 - 快捷键提示显著可见

## 六、扩展功能

可以进一步添加：
1. **点样式**：不同类型的控制点使用不同形状/颜色
2. **标签大小**：随控制点一起缩放
3. **持久化设置**：保存用户的大小偏好
4. **性能模式**：大数据集时禁用自动缩放

## 七、测试检查清单

- [ ] 修改 USV 数据后 3D 视图立即更新
- [ ] 相机位置和角度保持不变
- [ ] 滑块能正常调整控制点大小
- [ ] 自动缩放功能正常工作
- [ ] 状态栏显示相关消息
- [ ] 没有性能问题或卡顿
- [ ] 大数据集（>100 个点）表现正常

---

**版本**: 1.0  
**日期**: 2024-12-22  
**作者**: GitHub Copilot
