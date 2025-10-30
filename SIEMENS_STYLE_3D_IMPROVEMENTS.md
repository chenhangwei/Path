# ?? 3D线条编辑器 - 西门子NX风格改进建议

## ?? 概述

本文档总结了将3D路径编辑器改进为西门子NX/Solid Edge专业建模软件风格的完整方案。

---

## ? 已完成的改进

### 1. **右侧属性面板美化** ?
- ? 卡片式设计，蓝色标题栏
- ? 信息分组（基本信息 ??、几何属性 ??、使用提示 ??）
- ? Emoji图标增强视觉效果
- ? 滚动支持

### 2. **3D视图交互优化** ???
- ? 鼠标滚轮以鼠标位置为中心缩放
- ? 中键拖动平移视图（以网格中心为参考）
- ? 禁用自动适应屏幕（用户完全控制视角）

---

## ?? 推荐的进一步改进（参考西门子NX）

### 1. **专业工具栏系统** ???

#### 建议的工具栏布局：

```
┌─────────────────────────────────────────────────────────────┐
│ 【创建】 【编辑】 【捕捉】 【约束】 【测量】 【视图】    │
└─────────────────────────────────────────────────────────────┘
```

#### **创建工具栏**
- ??? 选择模式 (快捷键: S)
- ?? 折线 (快捷键: L)
- ⌒ 圆弧 (快捷键: A)  
- ?? 样条曲线 (快捷键: P)
- ? 圆形
- ? 矩形

#### **编辑工具栏**
- ? 撤销 (Ctrl+Z)
- ? 重做 (Ctrl+Y)
- ?? 修剪
- ? 延伸
- ?? 镜像
- ?? 阵列
- ?? 复制 (Ctrl+C)
- ??? 删除 (Del)

#### **捕捉工具栏**
- ? 网格捕捉 (F9) - 捕捉到网格点
- ● 点捕捉 (F10) - 捕捉到控制点
- ? 中点捕捉 (F11) - 捕捉到线段中点
- ⊥ 垂足捕捉 - 捕捉到垂直投影点
- ∩ 交点捕捉 - 捕捉到线段交点

#### **约束工具栏**
- ? 水平约束 (H) - 强制水平绘制
- ? 垂直约束 (V) - 强制垂直绘制
- ∠ 角度约束 - 指定角度绘制
- ═ 平行约束
- ⊥ 垂直关系

#### **测量工具栏**
- ?? 测量距离
- ∠ 测量角度
- ?? 显示坐标
- ?? 显示长度

---

### 2. **智能捕捉系统** ??

```csharp
/// <summary>
/// 智能捕捉设置
/// </summary>
public class SmartSnapSettings
{
    public bool GridSnap { get; set; } = true;     // 网格捕捉
    public bool PointSnap { get; set; } = true;          // 点捕捉
    public bool MidpointSnap { get; set; } = false;      // 中点捕捉
    public bool EndpointSnap { get; set; } = true;  // 端点捕捉
    public bool IntersectionSnap { get; set; } = false;  // 交点捕捉
    public bool PerpendicularSnap { get; set; } = false; // 垂足捕捉
    
    public double SnapDistance { get; set; } = 10.0;      // 捕捉距离（像素）
}

/// <summary>
/// 应用智能捕捉到点
/// </summary>
private Point3D ApplySmartSnapping(Point3D rawPoint)
{
    var result = rawPoint;
  
    // 1. 网格捕捉（优先级最低）
    if (_snapSettings.GridSnap)
    {
    double gridSize = 5.0; // 可配置
 result = new Point3D(
  Math.Round(result.X / gridSize) * gridSize,
        Math.Round(result.Y / gridSize) * gridSize,
    result.Z
 );
    }
    
    // 2. 几何点捕捉（高优先级）
  if (_snapSettings.EndpointSnap || _snapSettings.PointSnap)
    {
        var nearestPoint = FindNearestControlPoint(result);
        if (nearestPoint != null && Distance(result, nearestPoint.Value) < _snapSettings.SnapDistance)
        {
            result = nearestPoint.Value;
   ShowSnapIndicator(result, SnapType.Endpoint);
            return result;
        }
    }
    
    // 3. 中点捕捉
    if (_snapSettings.MidpointSnap)
    {
      var nearestMidpoint = FindNearestMidpoint(result);
        if (nearestMidpoint != null && Distance(result, nearestMidpoint.Value) < _snapSettings.SnapDistance)
      {
       result = nearestMidpoint.Value;
    ShowSnapIndicator(result, SnapType.Midpoint);
            return result;
        }
    }
    
    // 4. 交点捕捉
    if (_snapSettings.IntersectionSnap)
    {
        var intersection = FindNearestIntersection(result);
        if (intersection != null)
  {
            result = intersection.Value;
            ShowSnapIndicator(result, SnapType.Intersection);
            return result;
        }
    }
    
    return result;
}
```

**捕捉视觉反馈：**
- ?? 绿色圆圈：端点捕捉
- ?? 蓝色三角：中点捕捉
- ?? 黄色菱形：交点捕捉
- ? 白色方框：网格捕捉

---

### 3. **动态预览系统** ???

```csharp
/// <summary>
/// 动态预览当前正在绘制的几何体
/// </summary>
private void UpdateDynamicPreview(Point3D currentPoint)
{
    // 清除旧预览
    ClearPreviewGeometry();
    
    if (_currentMode == DrawMode.Polyline && _tempPoints.Count > 0)
    {
      // 预览折线段
        var lastPoint = _tempPoints.Last();
        DrawDashedLine(lastPoint, currentPoint, Brushes.Gray);
    }
    else if (_currentMode == DrawMode.Arc && _tempPoints.Count == 2)
    {
 // 预览圆弧（三点定义）
        DrawPreviewArc(_tempPoints[0], _tempPoints[1], currentPoint);
    }
    else if (_currentMode == DrawMode.Circle && _tempPoints.Count == 1)
    {
        // 预览圆（中心+半径）
        var radius = Distance(_tempPoints[0], currentPoint);
        DrawPreviewCircle(_tempPoints[0], radius);
    }
    
    // 显示坐标和距离信息
    ShowPreviewInfo(currentPoint);
}

/// <summary>
/// 显示实时信息（屏幕左上角）
/// </summary>
private void ShowPreviewInfo(Point3D point)
{
    var info = new StringBuilder();
 info.AppendLine($"坐标: X={point.X:F2}, Y={point.Y:F2}, Z={point.Z:F2}");
    
    if (_tempPoints.Count > 0)
    {
        var dist = Distance(_tempPoints.Last(), point);
        info.AppendLine($"距离: {dist:F2}");
        
        if (_tempPoints.Count > 1)
    {
       var angle = CalculateAngle(_tempPoints[^2], _tempPoints.Last(), point);
     info.AppendLine($"角度: {angle:F1}°");
        }
    }
    
    // 显示约束状态
    if (_constraints.HorizontalConstraint)
   info.AppendLine("约束: 水平 (H)");
    if (_constraints.VerticalConstraint)
        info.AppendLine("约束: 垂直 (V)");
    
    _infoTextBlock.Text = info.ToString();
    _infoBorder.Visibility = Visibility.Visible;
}
```

---

### 4. **几何约束系统** ??

```csharp
/// <summary>
/// 几何约束管理
/// </summary>
public class GeometricConstraints
{
    public bool HorizontalConstraint { get; set; }  // 水平约束
    public bool VerticalConstraint { get; set; }    // 垂直约束
    public double? AngleConstraint { get; set; }    // 角度约束（度）
    public double? LengthConstraint { get; set; }   // 长度约束
    public bool ParallelConstraint { get; set; }    // 平行约束
    public bool PerpendicularConstraint { get; set; } // 垂直关系约束
}

/// <summary>
/// 应用约束到新点
/// </summary>
private Point3D ApplyConstraints(Point3D rawPoint, Point3D? referencePoint)
{
    if (referencePoint == null)
        return rawPoint;
    
    var result = rawPoint;
    var refPt = referencePoint.Value;
    
// 1. 水平约束
    if (_constraints.HorizontalConstraint)
    {
   result = new Point3D(result.X, refPt.Y, result.Z);
     ShowConstraintLine(refPt, result, Colors.Yellow);
    }
    
    // 2. 垂直约束
    if (_constraints.VerticalConstraint)
    {
        result = new Point3D(refPt.X, result.Y, result.Z);
        ShowConstraintLine(refPt, result, Colors.Yellow);
}
    
    // 3. 角度约束
    if (_constraints.AngleConstraint.HasValue)
    {
      var angle = _constraints.AngleConstraint.Value * Math.PI / 180.0;
        var dx = result.X - refPt.X;
        var dy = result.Y - refPt.Y;
        var dist = Math.Sqrt(dx * dx + dy * dy);
    
     result = new Point3D(
    refPt.X + dist * Math.Cos(angle),
      refPt.Y + dist * Math.Sin(angle),
            result.Z
        );
     
        ShowAngleIndicator(refPt, result, _constraints.AngleConstraint.Value);
    }
    
    // 4. 长度约束
    if (_constraints.LengthConstraint.HasValue)
    {
        var dx = result.X - refPt.X;
  var dy = result.Y - refPt.Y;
        var currentDist = Math.Sqrt(dx * dx + dy * dy);
        
   if (currentDist > 0)
    {
  var scale = _constraints.LengthConstraint.Value / currentDist;
      result = new Point3D(
                refPt.X + dx * scale,
      refPt.Y + dy * scale,
          result.Z
    );
        }
    }
    
    return result;
}
```

---

### 5. **多选和批量操作** ??

```csharp
/// <summary>
/// 选择集管理
/// </summary>
public class SelectionSet
{
public List<(int usvIndex, int segIndex)> SelectedSegments { get; } = new();
    public List<(int usvIndex, int segIndex, int pointIndex)> SelectedPoints { get; } = new();
    
    public bool IsEmpty => SelectedSegments.Count == 0 && SelectedPoints.Count == 0;
    
    public void Clear()
    {
 SelectedSegments.Clear();
     SelectedPoints.Clear();
    }
    
    public void AddSegment(int usvIndex, int segIndex)
  {
        var tuple = (usvIndex, segIndex);
        if (!SelectedSegments.Contains(tuple))
     SelectedSegments.Add(tuple);
    }
    
    public void ToggleSegment(int usvIndex, int segIndex)
    {
     var tuple = (usvIndex, segIndex);
        if (SelectedSegments.Contains(tuple))
            SelectedSegments.Remove(tuple);
        else
    SelectedSegments.Add(tuple);
    }
}

/// <summary>
/// 批量操作：镜像
/// </summary>
private void MirrorSelection(MirrorAxis axis)
{
    if (_selectionSet.IsEmpty)
    {
    MessageBox.Show("请先选择要镜像的对象");
        return;
    }
    
    var centerPoint = CalculateSelectionCenter();
    
    foreach (var (ui, si) in _selectionSet.SelectedSegments)
    {
        var segment = _usvs[ui].Segments[si];
        for (int i = 0; i < segment.ControlPoints.Count; i++)
        {
    var pt = segment.ControlPoints[i];
Point3D mirrored;
     
        if (axis == MirrorAxis.X)
   mirrored = new Point3D(2 * centerPoint.X - pt.X, pt.Y, pt.Z);
            else // MirrorAxis.Y
         mirrored = new Point3D(pt.X, 2 * centerPoint.Y - pt.Y, pt.Z);
        
   segment.ControlPoints[i] = mirrored;
   }
 }
    
  Redraw();
}

/// <summary>
/// 批量操作：阵列
/// </summary>
private void ArraySelection(int rows, int columns, double spacingX, double spacingY)
{
    if (_selectionSet.IsEmpty)
        return;
    
    var originalSegments = new List<Segment>();
    foreach (var (ui, si) in _selectionSet.SelectedSegments)
 {
        originalSegments.Add(_usvs[ui].Segments[si].Clone());
    }
    
    for (int r = 0; r < rows; r++)
    {
 for (int c = 0; c < columns; c++)
        {
         if (r == 0 && c == 0) continue; // 跳过原始位置
    
         foreach (var originalSeg in originalSegments)
        {
         var newSeg = originalSeg.Clone();
 var offset = new Vector3D(c * spacingX, r * spacingY, 0);
     
     for (int i = 0; i < newSeg.ControlPoints.Count; i++)
      {
       newSeg.ControlPoints[i] += offset;
}
    
                _usvs[_selectedUsvIndex].Segments.Add(newSeg);
  }
        }
    }
    
    Redraw();
}
```

---

### 6. **增强的撤销/重做系统** ??

```csharp
/// <summary>
/// 命令模式撤销/重做
/// </summary>
public interface ICommand
{
    void Execute();
    void Undo();
    string Description { get; }
}

public class CommandManager
{
    private Stack<ICommand> _undoStack = new();
    private Stack<ICommand> _redoStack = new();
    private const int MaxHistorySize = 50;
    
    public void ExecuteCommand(ICommand command)
    {
    command.Execute();
        _undoStack.Push(command);
 _redoStack.Clear(); // 新操作清空重做栈
   
        // 限制历史大小
  if (_undoStack.Count > MaxHistorySize)
        {
            var temp = _undoStack.Reverse().Take(MaxHistorySize).Reverse().ToList();
     _undoStack.Clear();
       foreach (var cmd in temp)
      _undoStack.Push(cmd);
      }
    }

    public void Undo()
    {
        if (_undoStack.Count > 0)
     {
   var command = _undoStack.Pop();
 command.Undo();
   _redoStack.Push(command);
        }
    }
    
    public void Redo()
    {
        if (_redoStack.Count > 0)
        {
        var command = _redoStack.Pop();
            command.Execute();
        _undoStack.Push(command);
        }
    }

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
}

/// <summary>
/// 添加点命令
/// </summary>
public class AddPointCommand : ICommand
{
    private readonly int _usvIndex;
    private readonly int _segIndex;
    private readonly Point3D _point;
    private readonly PathEditor _editor;
    
    public string Description => "添加点";
    
    public AddPointCommand(PathEditor editor, int usvIndex, int segIndex, Point3D point)
    {
        _editor = editor;
      _usvIndex = usvIndex;
   _segIndex = segIndex;
        _point = point;
    }
  
    public void Execute()
    {
        _editor._usvs[_usvIndex].Segments[_segIndex].ControlPoints.Add(_point);
        _editor.Redraw();
    }
    
    public void Undo()
    {
        var segment = _editor._usvs[_usvIndex].Segments[_segIndex];
        segment.ControlPoints.RemoveAt(segment.ControlPoints.Count - 1);
        _editor.Redraw();
    }
}
```

---

### 7. **测量工具** ??

```csharp
/// <summary>
/// 测量模式
/// </summary>
public enum MeasureMode
{
    None,
    Distance,      // 测量两点间距离
  Angle,         // 测量三点间角度
    Area,        // 测量封闭区域面积
    Length         // 测量曲线总长度
}

/// <summary>
/// 测量距离
/// </summary>
private void MeasureDistance()
{
    _measureMode = MeasureMode.Distance;
    _measurePoints.Clear();
    ShowMeasureHint("点击两个点测量距离");
}

/// <summary>
/// 显示测量结果
/// </summary>
private void DisplayMeasurementResult()
{
    if (_measureMode == MeasureMode.Distance && _measurePoints.Count == 2)
    {
     var dist = Distance(_measurePoints[0], _measurePoints[1]);
        var displayDist = dist * _displayUnitScale;
        
        // 在两点之间显示测量线和标注
        DrawMeasurementLine(_measurePoints[0], _measurePoints[1]);
        DrawMeasurementText(
          Midpoint(_measurePoints[0], _measurePoints[1]),
$"{displayDist:F2} 单位",
            Brushes.Blue
        );
        
   ShowMeasurementPanel($"距离: {displayDist:F3} 单位");
        _measureMode = MeasureMode.None;
    }
    else if (_measureMode == MeasureMode.Angle && _measurePoints.Count == 3)
    {
        var angle = CalculateAngle(_measurePoints[0], _measurePoints[1], _measurePoints[2]);
        
      // 显示角度标注
        DrawAngleArc(_measurePoints[0], _measurePoints[1], _measurePoints[2]);
   DrawMeasurementText(_measurePoints[1], $"{angle:F1}°", Brushes.Green);
      
        ShowMeasurementPanel($"角度: {angle:F2}°");
  _measureMode = MeasureMode.None;
    }
}

/// <summary>
/// 计算角度（度）
/// </summary>
private double CalculateAngle(Point3D p1, Point3D vertex, Point3D p3)
{
var v1 = new Vector3D(p1.X - vertex.X, p1.Y - vertex.Y, 0);
    var v2 = new Vector3D(p3.X - vertex.X, p3.Y - vertex.Y, 0);
    
    var dot = v1.X * v2.X + v1.Y * v2.Y;
  var len1 = Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y);
    var len2 = Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y);
  
    var cosAngle = dot / (len1 * len2);
    var angleRad = Math.Acos(Math.Max(-1, Math.Min(1, cosAngle)));
    
    return angleRad * 180.0 / Math.PI;
}
```

---

### 8. **快捷键系统** ??

```csharp
/// <summary>
/// 快捷键映射
/// </summary>
private Dictionary<Key, Action> _shortcutKeys = new()
{
    // 工具切换
    { Key.S, () => SetDrawMode(DrawMode.Select) },
    { Key.L, () => SetDrawMode(DrawMode.Polyline) },
    { Key.A, () => SetDrawMode(DrawMode.Arc) },
    { Key.P, () => SetDrawMode(DrawMode.Spline) },
 { Key.C, () => SetDrawMode(DrawMode.Circle) },
    { Key.R, () => SetDrawMode(DrawMode.Rectangle) },
    
  // 捕捉模式
    { Key.F9, () => ToggleGridSnap() },
    { Key.F10, () => TogglePointSnap() },
    { Key.F11, () => ToggleMidpointSnap() },
    
    // 约束
    { Key.H, () => ToggleHorizontalConstraint() },
    { Key.V, () => ToggleVerticalConstraint() },
    
    // 视图
    { Key.F, () => FitToView() },
    { Key.Home, () => ResetView() },
  
    // 编辑
    { Key.Delete, () => DeleteSelected() },
    { Key.Escape, () => CancelCurrentOperation() },
};

protected override void OnKeyDown(KeyEventArgs e)
{
 base.OnKeyDown(e);
    
    // Ctrl+Z: 撤销
    if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
    {
        _commandManager.Undo();
        e.Handled = true;
        return;
    }
    
    // Ctrl+Y: 重做
    if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
    {
        _commandManager.Redo();
        e.Handled = true;
        return;
    }
 
    // Ctrl+C: 复制
    if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
    {
   CopySelection();
        e.Handled = true;
        return;
    }
    
    // Ctrl+V: 粘贴
    if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
    {
    PasteSelection();
        e.Handled = true;
        return;
    }
    
    // 普通快捷键
    if (_shortcutKeys.TryGetValue(e.Key, out var action))
    {
        action();
        e.Handled = true;
    }
}
```

---

## ?? 完整快捷键列表

| 快捷键 | 功能 | 分类 |
|--------|------|------|
| **S** | 选择模式 | 工具 |
| **L** | 折线工具 | 工具 |
| **A** | 圆弧工具 | 工具 |
| **P** | 样条曲线 | 工具 |
| **C** | 圆形工具 | 工具 |
| **R** | 矩形工具 | 工具 |
| **F9** | 网格捕捉开/关 | 捕捉 |
| **F10** | 点捕捉开/关 | 捕捉 |
| **F11** | 中点捕捉开/关 | 捕捉 |
| **H** | 水平约束开/关 | 约束 |
| **V** | 垂直约束开/关 | 约束 |
| **Ctrl+Z** | 撤销 | 编辑 |
| **Ctrl+Y** | 重做 | 编辑 |
| **Ctrl+C** | 复制 | 编辑 |
| **Ctrl+V** | 粘贴 | 编辑 |
| **Del** | 删除选中 | 编辑 |
| **Esc** | 取消操作 | 通用 |
| **F** | 适应全部 | 视图 |
| **Home** | 重置视图 | 视图 |
| **中键** | 平移视图 | 视图 |
| **滚轮** | 缩放 | 视图 |

---

## ?? UI/UX改进建议

### 状态栏信息显示

```
┌─────────────────────────────────────────────────────────┐
│ 模式: 折线 | 捕捉: 网格? 点? | 约束: 水平 | 选中: 3段  │
└─────────────────────────────────────────────────────────┘
```

### 浮动工具提示

```
 ┌────────────────┐
 │ ?? 捕捉到端点  │
 │ X: 125.50      │
 │ Y: 80.25     │
 │ Z: 0.00 │
 └────────────────┘
```

### 右键上下文菜单

```
┌──────────────────┐
│ 编辑            │
│ ├ 修剪          │
│ ├ 延伸          │
│ ├ 分割          │
│ └ 合并          │
│ 变换    │
│ ├ 移动  │
│ ├ 旋转       │
│ ├ 缩放       │
│ ├ 镜像   │
│ └ 阵列          │
│ 属性...    │
│ 删除    (Del)   │
└──────────────────┘
```

---

## ?? 实现优先级

### ?? 高优先级（核心功能）
1. ? 鼠标滚轮以鼠标位置缩放
2. ? 中键拖动平移
3. ?? 智能捕捉系统（网格、点、中点）
4. ?? 动态预览
5. ?? 基本约束（水平、垂直）

### ?? 中优先级（增强体验）
6. ?? 撤销/重做增强
7. ?? 多选和批量操作
8. ?? 测量工具
9. ?? 快捷键系统

### ?? 低优先级（锦上添花）
10. ?? 高级约束（角度、长度、平行、垂直关系）
11. ?? 修剪/延伸工具
12. ?? 阵列/镜像高级选项
13. ?? 样条曲线编辑

---

## ?? 总结

通过以上改进，3D路径编辑器将具备：

- ? 专业的工具栏布局
- ? 智能捕捉和约束系统
- ? 实时动态预览
- ? 强大的编辑能力
- ? 直观的视觉反馈
- ? 完整的快捷键支持
- ? 类似西门子NX的专业体验

这将使编辑器从简单的绘图工具提升为专业的3D建模辅助工具。

---

**当前状态:**
- ? 3项核心改进已完成
- ?? 10+项高级功能待实现

**下一步行动:**
1. 实现智能捕捉系统
2. 添加动态预览
3. 增强约束系统
