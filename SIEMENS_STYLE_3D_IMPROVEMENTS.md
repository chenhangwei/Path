# ?? 3D�����༭�� - ������NX���Ľ�����

## ?? ����

���ĵ��ܽ��˽�3D·���༭���Ľ�Ϊ������NX/Solid Edgeרҵ��ģ�����������������

---

## ? ����ɵĸĽ�

### 1. **�Ҳ������������** ?
- ? ��Ƭʽ��ƣ���ɫ������
- ? ��Ϣ���飨������Ϣ ??���������� ??��ʹ����ʾ ??��
- ? Emojiͼ����ǿ�Ӿ�Ч��
- ? ����֧��

### 2. **3D��ͼ�����Ż�** ???
- ? �����������λ��Ϊ��������
- ? �м��϶�ƽ����ͼ������������Ϊ�ο���
- ? �����Զ���Ӧ��Ļ���û���ȫ�����ӽǣ�

---

## ?? �Ƽ��Ľ�һ���Ľ����ο�������NX��

### 1. **רҵ������ϵͳ** ???

#### ����Ĺ��������֣�

```
������������������������������������������������������������������������������������������������������������������������������
�� �������� ���༭�� ����׽�� ��Լ���� �������� ����ͼ��    ��
������������������������������������������������������������������������������������������������������������������������������
```

#### **����������**
- ??? ѡ��ģʽ (��ݼ�: S)
- ?? ���� (��ݼ�: L)
- �� Բ�� (��ݼ�: A)  
- ?? �������� (��ݼ�: P)
- ? Բ��
- ? ����

#### **�༭������**
- ? ���� (Ctrl+Z)
- ? ���� (Ctrl+Y)
- ?? �޼�
- ? ����
- ?? ����
- ?? ����
- ?? ���� (Ctrl+C)
- ??? ɾ�� (Del)

#### **��׽������**
- ? ����׽ (F9) - ��׽�������
- �� �㲶׽ (F10) - ��׽�����Ƶ�
- ? �е㲶׽ (F11) - ��׽���߶��е�
- �� ���㲶׽ - ��׽����ֱͶӰ��
- �� ���㲶׽ - ��׽���߶ν���

#### **Լ��������**
- ? ˮƽԼ�� (H) - ǿ��ˮƽ����
- ? ��ֱԼ�� (V) - ǿ�ƴ�ֱ����
- �� �Ƕ�Լ�� - ָ���ǶȻ���
- �T ƽ��Լ��
- �� ��ֱ��ϵ

#### **����������**
- ?? ��������
- �� �����Ƕ�
- ?? ��ʾ����
- ?? ��ʾ����

---

### 2. **���ܲ�׽ϵͳ** ??

```csharp
/// <summary>
/// ���ܲ�׽����
/// </summary>
public class SmartSnapSettings
{
    public bool GridSnap { get; set; } = true;     // ����׽
    public bool PointSnap { get; set; } = true;          // �㲶׽
    public bool MidpointSnap { get; set; } = false;      // �е㲶׽
    public bool EndpointSnap { get; set; } = true;  // �˵㲶׽
    public bool IntersectionSnap { get; set; } = false;  // ���㲶׽
    public bool PerpendicularSnap { get; set; } = false; // ���㲶׽
    
    public double SnapDistance { get; set; } = 10.0;      // ��׽���루���أ�
}

/// <summary>
/// Ӧ�����ܲ�׽����
/// </summary>
private Point3D ApplySmartSnapping(Point3D rawPoint)
{
    var result = rawPoint;
  
    // 1. ����׽�����ȼ���ͣ�
    if (_snapSettings.GridSnap)
    {
    double gridSize = 5.0; // ������
 result = new Point3D(
  Math.Round(result.X / gridSize) * gridSize,
        Math.Round(result.Y / gridSize) * gridSize,
    result.Z
 );
    }
    
    // 2. ���ε㲶׽�������ȼ���
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
    
    // 3. �е㲶׽
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
    
    // 4. ���㲶׽
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

**��׽�Ӿ�������**
- ?? ��ɫԲȦ���˵㲶׽
- ?? ��ɫ���ǣ��е㲶׽
- ?? ��ɫ���Σ����㲶׽
- ? ��ɫ��������׽

---

### 3. **��̬Ԥ��ϵͳ** ???

```csharp
/// <summary>
/// ��̬Ԥ����ǰ���ڻ��Ƶļ�����
/// </summary>
private void UpdateDynamicPreview(Point3D currentPoint)
{
    // �����Ԥ��
    ClearPreviewGeometry();
    
    if (_currentMode == DrawMode.Polyline && _tempPoints.Count > 0)
    {
      // Ԥ�����߶�
        var lastPoint = _tempPoints.Last();
        DrawDashedLine(lastPoint, currentPoint, Brushes.Gray);
    }
    else if (_currentMode == DrawMode.Arc && _tempPoints.Count == 2)
    {
 // Ԥ��Բ�������㶨�壩
        DrawPreviewArc(_tempPoints[0], _tempPoints[1], currentPoint);
    }
    else if (_currentMode == DrawMode.Circle && _tempPoints.Count == 1)
    {
        // Ԥ��Բ������+�뾶��
        var radius = Distance(_tempPoints[0], currentPoint);
        DrawPreviewCircle(_tempPoints[0], radius);
    }
    
    // ��ʾ����;�����Ϣ
    ShowPreviewInfo(currentPoint);
}

/// <summary>
/// ��ʾʵʱ��Ϣ����Ļ���Ͻǣ�
/// </summary>
private void ShowPreviewInfo(Point3D point)
{
    var info = new StringBuilder();
 info.AppendLine($"����: X={point.X:F2}, Y={point.Y:F2}, Z={point.Z:F2}");
    
    if (_tempPoints.Count > 0)
    {
        var dist = Distance(_tempPoints.Last(), point);
        info.AppendLine($"����: {dist:F2}");
        
        if (_tempPoints.Count > 1)
    {
       var angle = CalculateAngle(_tempPoints[^2], _tempPoints.Last(), point);
     info.AppendLine($"�Ƕ�: {angle:F1}��");
        }
    }
    
    // ��ʾԼ��״̬
    if (_constraints.HorizontalConstraint)
   info.AppendLine("Լ��: ˮƽ (H)");
    if (_constraints.VerticalConstraint)
        info.AppendLine("Լ��: ��ֱ (V)");
    
    _infoTextBlock.Text = info.ToString();
    _infoBorder.Visibility = Visibility.Visible;
}
```

---

### 4. **����Լ��ϵͳ** ??

```csharp
/// <summary>
/// ����Լ������
/// </summary>
public class GeometricConstraints
{
    public bool HorizontalConstraint { get; set; }  // ˮƽԼ��
    public bool VerticalConstraint { get; set; }    // ��ֱԼ��
    public double? AngleConstraint { get; set; }    // �Ƕ�Լ�����ȣ�
    public double? LengthConstraint { get; set; }   // ����Լ��
    public bool ParallelConstraint { get; set; }    // ƽ��Լ��
    public bool PerpendicularConstraint { get; set; } // ��ֱ��ϵԼ��
}

/// <summary>
/// Ӧ��Լ�����µ�
/// </summary>
private Point3D ApplyConstraints(Point3D rawPoint, Point3D? referencePoint)
{
    if (referencePoint == null)
        return rawPoint;
    
    var result = rawPoint;
    var refPt = referencePoint.Value;
    
// 1. ˮƽԼ��
    if (_constraints.HorizontalConstraint)
    {
   result = new Point3D(result.X, refPt.Y, result.Z);
     ShowConstraintLine(refPt, result, Colors.Yellow);
    }
    
    // 2. ��ֱԼ��
    if (_constraints.VerticalConstraint)
    {
        result = new Point3D(refPt.X, result.Y, result.Z);
        ShowConstraintLine(refPt, result, Colors.Yellow);
}
    
    // 3. �Ƕ�Լ��
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
    
    // 4. ����Լ��
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

### 5. **��ѡ����������** ??

```csharp
/// <summary>
/// ѡ�񼯹���
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
/// ��������������
/// </summary>
private void MirrorSelection(MirrorAxis axis)
{
    if (_selectionSet.IsEmpty)
    {
    MessageBox.Show("����ѡ��Ҫ����Ķ���");
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
/// ��������������
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
         if (r == 0 && c == 0) continue; // ����ԭʼλ��
    
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

### 6. **��ǿ�ĳ���/����ϵͳ** ??

```csharp
/// <summary>
/// ����ģʽ����/����
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
 _redoStack.Clear(); // �²����������ջ
   
        // ������ʷ��С
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
/// ��ӵ�����
/// </summary>
public class AddPointCommand : ICommand
{
    private readonly int _usvIndex;
    private readonly int _segIndex;
    private readonly Point3D _point;
    private readonly PathEditor _editor;
    
    public string Description => "��ӵ�";
    
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

### 7. **��������** ??

```csharp
/// <summary>
/// ����ģʽ
/// </summary>
public enum MeasureMode
{
    None,
    Distance,      // ������������
  Angle,         // ���������Ƕ�
    Area,        // ��������������
    Length         // ���������ܳ���
}

/// <summary>
/// ��������
/// </summary>
private void MeasureDistance()
{
    _measureMode = MeasureMode.Distance;
    _measurePoints.Clear();
    ShowMeasureHint("����������������");
}

/// <summary>
/// ��ʾ�������
/// </summary>
private void DisplayMeasurementResult()
{
    if (_measureMode == MeasureMode.Distance && _measurePoints.Count == 2)
    {
     var dist = Distance(_measurePoints[0], _measurePoints[1]);
        var displayDist = dist * _displayUnitScale;
        
        // ������֮����ʾ�����ߺͱ�ע
        DrawMeasurementLine(_measurePoints[0], _measurePoints[1]);
        DrawMeasurementText(
          Midpoint(_measurePoints[0], _measurePoints[1]),
$"{displayDist:F2} ��λ",
            Brushes.Blue
        );
        
   ShowMeasurementPanel($"����: {displayDist:F3} ��λ");
        _measureMode = MeasureMode.None;
    }
    else if (_measureMode == MeasureMode.Angle && _measurePoints.Count == 3)
    {
        var angle = CalculateAngle(_measurePoints[0], _measurePoints[1], _measurePoints[2]);
        
      // ��ʾ�Ƕȱ�ע
        DrawAngleArc(_measurePoints[0], _measurePoints[1], _measurePoints[2]);
   DrawMeasurementText(_measurePoints[1], $"{angle:F1}��", Brushes.Green);
      
        ShowMeasurementPanel($"�Ƕ�: {angle:F2}��");
  _measureMode = MeasureMode.None;
    }
}

/// <summary>
/// ����Ƕȣ��ȣ�
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

### 8. **��ݼ�ϵͳ** ??

```csharp
/// <summary>
/// ��ݼ�ӳ��
/// </summary>
private Dictionary<Key, Action> _shortcutKeys = new()
{
    // �����л�
    { Key.S, () => SetDrawMode(DrawMode.Select) },
    { Key.L, () => SetDrawMode(DrawMode.Polyline) },
    { Key.A, () => SetDrawMode(DrawMode.Arc) },
    { Key.P, () => SetDrawMode(DrawMode.Spline) },
 { Key.C, () => SetDrawMode(DrawMode.Circle) },
    { Key.R, () => SetDrawMode(DrawMode.Rectangle) },
    
  // ��׽ģʽ
    { Key.F9, () => ToggleGridSnap() },
    { Key.F10, () => TogglePointSnap() },
    { Key.F11, () => ToggleMidpointSnap() },
    
    // Լ��
    { Key.H, () => ToggleHorizontalConstraint() },
    { Key.V, () => ToggleVerticalConstraint() },
    
    // ��ͼ
    { Key.F, () => FitToView() },
    { Key.Home, () => ResetView() },
  
    // �༭
    { Key.Delete, () => DeleteSelected() },
    { Key.Escape, () => CancelCurrentOperation() },
};

protected override void OnKeyDown(KeyEventArgs e)
{
 base.OnKeyDown(e);
    
    // Ctrl+Z: ����
    if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
    {
        _commandManager.Undo();
        e.Handled = true;
        return;
    }
    
    // Ctrl+Y: ����
    if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
    {
        _commandManager.Redo();
        e.Handled = true;
        return;
    }
 
    // Ctrl+C: ����
    if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
    {
   CopySelection();
        e.Handled = true;
        return;
    }
    
    // Ctrl+V: ճ��
    if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
    {
    PasteSelection();
        e.Handled = true;
        return;
    }
    
    // ��ͨ��ݼ�
    if (_shortcutKeys.TryGetValue(e.Key, out var action))
    {
        action();
        e.Handled = true;
    }
}
```

---

## ?? ������ݼ��б�

| ��ݼ� | ���� | ���� |
|--------|------|------|
| **S** | ѡ��ģʽ | ���� |
| **L** | ���߹��� | ���� |
| **A** | Բ������ | ���� |
| **P** | �������� | ���� |
| **C** | Բ�ι��� | ���� |
| **R** | ���ι��� | ���� |
| **F9** | ����׽��/�� | ��׽ |
| **F10** | �㲶׽��/�� | ��׽ |
| **F11** | �е㲶׽��/�� | ��׽ |
| **H** | ˮƽԼ����/�� | Լ�� |
| **V** | ��ֱԼ����/�� | Լ�� |
| **Ctrl+Z** | ���� | �༭ |
| **Ctrl+Y** | ���� | �༭ |
| **Ctrl+C** | ���� | �༭ |
| **Ctrl+V** | ճ�� | �༭ |
| **Del** | ɾ��ѡ�� | �༭ |
| **Esc** | ȡ������ | ͨ�� |
| **F** | ��Ӧȫ�� | ��ͼ |
| **Home** | ������ͼ | ��ͼ |
| **�м�** | ƽ����ͼ | ��ͼ |
| **����** | ���� | ��ͼ |

---

## ?? UI/UX�Ľ�����

### ״̬����Ϣ��ʾ

```
����������������������������������������������������������������������������������������������������������������������
�� ģʽ: ���� | ��׽: ����? ��? | Լ��: ˮƽ | ѡ��: 3��  ��
����������������������������������������������������������������������������������������������������������������������
```

### ����������ʾ

```
 ������������������������������������
 �� ?? ��׽���˵�  ��
 �� X: 125.50      ��
 �� Y: 80.25     ��
 �� Z: 0.00 ��
 ������������������������������������
```

### �Ҽ������Ĳ˵�

```
����������������������������������������
�� �༭            ��
�� �� �޼�          ��
�� �� ����          ��
�� �� �ָ�          ��
�� �� �ϲ�          ��
�� �任    ��
�� �� �ƶ�  ��
�� �� ��ת       ��
�� �� ����       ��
�� �� ����   ��
�� �� ����          ��
�� ����...    ��
�� ɾ��    (Del)   ��
����������������������������������������
```

---

## ?? ʵ�����ȼ�

### ?? �����ȼ������Ĺ��ܣ�
1. ? �����������λ������
2. ? �м��϶�ƽ��
3. ?? ���ܲ�׽ϵͳ�����񡢵㡢�е㣩
4. ?? ��̬Ԥ��
5. ?? ����Լ����ˮƽ����ֱ��

### ?? �����ȼ�����ǿ���飩
6. ?? ����/������ǿ
7. ?? ��ѡ����������
8. ?? ��������
9. ?? ��ݼ�ϵͳ

### ?? �����ȼ�����������
10. ?? �߼�Լ�����Ƕȡ����ȡ�ƽ�С���ֱ��ϵ��
11. ?? �޼�/���칤��
12. ?? ����/����߼�ѡ��
13. ?? �������߱༭

---

## ?? �ܽ�

ͨ�����ϸĽ���3D·���༭�����߱���

- ? רҵ�Ĺ���������
- ? ���ܲ�׽��Լ��ϵͳ
- ? ʵʱ��̬Ԥ��
- ? ǿ��ı༭����
- ? ֱ�۵��Ӿ�����
- ? �����Ŀ�ݼ�֧��
- ? ����������NX��רҵ����

�⽫ʹ�༭���Ӽ򵥵Ļ�ͼ��������Ϊרҵ��3D��ģ�������ߡ�

---

**��ǰ״̬:**
- ? 3����ĸĽ������
- ?? 10+��߼����ܴ�ʵ��

**��һ���ж�:**
1. ʵ�����ܲ�׽ϵͳ
2. ��Ӷ�̬Ԥ��
3. ��ǿԼ��ϵͳ
