# ?? 3D智能捕捉系统 - 集成指南

## ? 已完成

### 1. **PathEditor3D.Snap.cs** - 智能捕捉核心模块
? 已创建完整的捕捉功能模块

### 功能特性

#### 捕捉类型（5种）
1. **网格捕捉** - 对齐到3D网格 (默认开启)
2. **端点捕捉** - 捕捉到路径端点 (最高优先级)
3. **控制点捕捉** - 捕捉到所有控制点
4. **中点捕捉** - 捕捉到线段中点
5. **投影捕捉** - 捕捉到垂足（点到线段的投影）

#### 视觉反馈
- ?? 半透明球体指示器（带颜色编码）
- ? 十字线标记
- ?? 文字标签显示捕捉类型和坐标

---

## ?? 集成步骤

由于PathEditor3D.xaml.cs文件很大，需要**手动**添加以下代码片段：

### 步骤1：在MouseDown事件中应用捕捉

找到`HelixView_MouseDown`方法中的"clicked empty plane"部分（约第460行），修改为：

```csharp
else
{
    // clicked empty plane — report logical coords to host
    var planePt = HitPlaneAtPoint(pt);
    if (planePt.HasValue)
    {
        // ?? 应用智能捕捉
        var visualSnapped = ApplySmartSnapping3D(planePt.Value, pt);
     var logical3 = VisualToLogical(visualSnapped);

        // If Shift+LeftClick over hovered segment -> insert control point
        if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && _hoveredSeg.HasValue)
        {
 var (ui, si) = _hoveredSeg.Value;
 if (OnPointInserted != null)
        {
         OnPointInserted?.Invoke(ui, si, logical3);
     e.Handled = true; return;
            }
            var insertIndex = ComputeInsertIndexForSegment(ui, si, LogicalToVisual(logical3));
 InternalInsertAt(ui, si, insertIndex, logical3);
       _internalUndo.Push(() => InternalDeletePoint(ui, si, insertIndex));
      e.Handled = true; return;
        }

        // check hovered segment first
      if (e.ChangedButton == MouseButton.Left && _hoveredSeg.HasValue)
        {
   OnSegmentClicked?.Invoke(_hoveredSeg.Value.Item1, _hoveredSeg.Value.Item2);
        }
        else
        {
     if (e.ChangedButton == MouseButton.Right) 
     OnPlaneRightClicked?.Invoke(logical3); 
         else 
  OnPlaneClicked?.Invoke(logical3);
        }
    }
}
```

### 步骤2：在MouseMove事件中显示捕捉预览

找到`HelixView_MouseMove`方法，在处理控制点拖动的部分（约第530行）添加捕捉：

```csharp
// dragging existing control point
if (_isDragging && _dragRef3D != null && _dragModel != null)
{
    if (e.LeftButton != MouseButtonState.Pressed) return;
    var pt = (HelixView.Viewport != null) ? e.GetPosition(HelixView.Viewport) : e.GetPosition(HelixView);
    var planeHit = HitPlaneAtPoint(pt);
    if (!planeHit.HasValue) return;
    
    // ?? 应用智能捕捉到拖动点
    var snappedHit = ApplySmartSnapping3D(planeHit.Value, pt);
    var now = snappedHit;
    
    // ...existing drag logic...
}
```

在Gizmo拖动部分添加捕捉（约第580行）：

```csharp
if (_isGizmoDragging && _gizmoSelectedPoint.HasValue)
{
    var pos = e.GetPosition(HelixView);
    if (!GetRayFromScreen(pos, out var rayOrigin, out var rayDir)) return;
   
    // ...计算newCenter的逻辑...
    
    // ?? 应用捕捉到Gizmo拖动的结果
    newCenter = ApplySmartSnapping3D(newCenter, pos);
    
    // ...更新sphere center的逻辑...
}
```

在非拖动状态下显示预览（约第620行，在segment hover处理之前）：

```csharp
// ?? 不在拖动时，显示捕捉预览
if (!_isDragging && !_isGizmoDragging && !_isMiddleButtonPanning)
{
    var mousePos = (HelixView.Viewport != null) ? e.GetPosition(HelixView.Viewport) : e.GetPosition(HelixView);
    var planePt = HitPlaneAtPoint(mousePos);
    if (planePt.HasValue)
    {
  // 显示捕捉预览（不实际修改任何内容）
        var snappedPreview = ApplySmartSnapping3D(planePt.Value, mousePos);
        // 捕捉指示器已在ApplySmartSnapping3D中显示
    }
    else
    {
        HideSnapIndicator3D();
}
}
```

### 步骤3：在MouseUp事件中隐藏指示器

找到`HelixView_MouseUp`方法，添加：

```csharp
private void HelixView_MouseUp(object? sender, MouseButtonEventArgs e)
{
    // ?? 隐藏捕捉指示器
    HideSnapIndicator3D();
    
    // ...existing code...
}
```

---

## ?? 使用方法

### 方法1：通过代码设置

```csharp
// 在初始化PathEditor3D后
pathEditor3D.SetSnapOptions(
    gridSnap: true,        // 网格捕捉
    pointSnap: true,    // 点捕捉  
    midpointSnap: false,   // 中点捕捉
    endpointSnap: true,    // 端点捕捉
    projectionSnap: false, // 投影捕捉
    snapDistance: 15.0     // 捕捉距离(像素)
);
```

### 方法2：快捷键切换

```csharp
// 在MainWindow或控制器中响应按键
protected override void OnKeyDown(KeyEventArgs e)
{
    base.OnKeyDown(e);
    
    switch (e.Key)
    {
        case Key.F9:
            pathEditor3D.ToggleGridSnap();
            break;
        case Key.F10:
pathEditor3D.TogglePointSnap();
            break;
        case Key.F11:
            pathEditor3D.ToggleMidpointSnap();
            break;
        case Key.F12:
       pathEditor3D.ToggleEndpointSnap();
    break;
    }
}
```

---

## ?? 视觉效果

### 捕捉指示器

每种捕捉类型都有独特的视觉反馈：

| 捕捉类型 | 颜色 | 半径 | 标签 |
|---------|------|------|------|
| 端点 | ?? 红色 | 8.0 | "?? 端点" |
| 控制点 | ?? 绿色 | 8.0 | "?? 控制点" |
| 中点 | ?? 青色 | 8.0 | "?? 中点" |
| 投影 | ?? 洋红色 | 7.0 | "?? 投影" |
| 网格 | ? 灰色 | 5.0 | "?? 网格" |

### 示例效果
```
      球体 (半透明)
         ●
   /|\
       / | \
      十字线
      
   ?? 端点
   (125.5, 80.2, 0.0)
```

---

## ? 性能优化

### 动态像素转换
```csharp
// 根据相机距离动态计算世界单位到屏幕像素的转换
var worldPerPixel = EstimateWorldPerPixel(screenPos);
var snapDistanceWorld = _snapSettings3D.SnapDistance * worldPerPixel;
```

### 捕捉优先级
系统按优先级顺序检测，找到第一个匹配即返回：
1. 端点（最常用）?????
2. 控制点 ????
3. 中点 ???
4. 投影 ??
5. 网格（兜底）?

---

## ?? 故障排查

### 问题1：捕捉不生效
**解决方案**：
- 检查是否调用了`SetSnapOptions`
- 确保`_snapSettings3D`不为null
- 验证`HitPlaneAtPoint`返回值不为null

### 问题2：指示器不显示
**解决方案**：
- 确保`SceneRoot`正确引用
- 检查`SnapIndicatorSphere`是否成功添加到场景
- 验证视觉坐标转换是否正确

### 问题3：捕捉距离不合适
**解决方案**：
```csharp
// 调整捕捉距离（屏幕像素）
pathEditor3D.SetSnapOptions(..., snapDistance: 20.0); // 增大捕捉范围

// 或减小
pathEditor3D.SetSnapOptions(..., snapDistance: 10.0); // 更精确的捕捉
```

---

## ?? 完整示例

### 在MainViewModel中集成

```csharp
public class MainViewModel : INotifyPropertyChanged
{
    private PathEditor3D? _pathEditor3D;
    
    public void InitializePathEditor(PathEditor3D editor)
    {
        _pathEditor3D = editor;
    
        // 启用智能捕捉
        editor.SetSnapOptions(
            gridSnap: true,
          pointSnap: true,
            midpointSnap: true,    // 3D中启用中点捕捉
            endpointSnap: true,
          projectionSnap: true,  // 3D特有：投影捕捉
        snapDistance: 15.0
        );
    }
    
    public void HandleKeyPress(Key key)
    {
        if (_pathEditor3D == null) return;
        
        switch (key)
        {
            case Key.F9:
  _pathEditor3D.ToggleGridSnap();
    StatusMessage = "网格捕捉已" + (/* 状态 */ ? "开启" : "关闭");
                break;
case Key.F10:
     _pathEditor3D.TogglePointSnap();
           StatusMessage = "点捕捉已" + (/* 状态 */ ? "开启" : "关闭");
             break;
     case Key.F11:
      _pathEditor3D.ToggleMidpointSnap();
   StatusMessage = "中点捕捉已" + (/* 状态 */ ? "开启" : "关闭");
       break;
        }
    }
}
```

---

## ?? 自定义样式

### 修改指示器外观

编辑`PathEditor3D.Snap.cs`中的`ShowSnapIndicator3D`方法：

```csharp
// 更改球体大小
_snapIndicatorSphere = new SphereVisual3D
{
    Center = visualPoint,
    Radius = radius * 1.5,  // ?? 放大1.5倍
    Fill = new SolidColorBrush(Color.FromArgb(180, color.R, color.G, color.B))  // ?? 更不透明
};

// 更改十字线大小
double crossSize = 20.0 * _visualScale;  // ?? 更大的十字

// 更改标签样式
_snapIndicatorLabel = new BillboardTextVisual3D
{
    Text = $"?? {typeName}\n({displayX:F1}, {displayY:F1}, {displayZ:F1})",
    Position = ...,
    Foreground = Brushes.White,  // ?? 白色文字
    Background = new SolidColorBrush(Color.FromArgb(230, 0, 0, 0)),  // ?? 黑色背景
    FontSize = 14,  // ?? 更大字体
    FontWeight = FontWeights.Bold
};
```

---

## ?? 下一步增强

### 建议功能
1. ?? **交点捕捉** - 捕捉两条线的交点（需要计算线-线交点）
2. ?? **切点捕捉** - 捕捉圆弧切点
3. ?? **角度捕捉** - 按固定角度（45°、90°等）捕捉
4. ?? **距离约束** - 从某点沿特定方向固定距离
5. ?? **平面捕捉** - 捕捉到XY/XZ/YZ平面

### 实现思路

#### 交点捕捉示例
```csharp
private Point3D? FindNearestIntersection3D(Point3D point, double threshold)
{
    Point3D? nearest = null;
    double minDist = threshold;
    
    // 遍历所有线段对
    for (int i = 0; i < segments.Count; i++)
    {
        for (int j = i + 1; j < segments.Count; j++)
        {
            var intersection = ComputeLineIntersection(
                segments[i].Start, segments[i].End,
  segments[j].Start, segments[j].End
         );
            
     if (intersection.HasValue)
            {
    var dist = DistanceXY(point, intersection.Value);
    if (dist < minDist)
                {
 minDist = dist;
        nearest = intersection.Value;
                }
            }
        }
    }
    
    return nearest;
}
```

---

## ?? 参考资料

- [PathEditor3D.Snap.cs](Views/PathEditor3D.Snap.cs) - 完整实现
- [SNAP_SYSTEM_IMPLEMENTATION.md](SNAP_SYSTEM_IMPLEMENTATION.md) - 2D捕捉系统文档
- [SIEMENS_STYLE_3D_IMPROVEMENTS.md](SIEMENS_STYLE_3D_IMPROVEMENTS.md) - 3D改进计划

---

**实现状态**: ? 核心模块完成，需手动集成  
**测试状态**: ?? 待测试  
**文档更新**: 2024年12月

?? **3D智能捕捉系统实现完成！**
