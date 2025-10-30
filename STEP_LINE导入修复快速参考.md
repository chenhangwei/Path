# STEP LINE 导入修复快速参考

## ?? 问题

导入的 LINE 线段在 3D 视图中显示不正确

---

## ? 已修复

修复了 VECTOR 实体的解析，正确提取方向和长度

---

## ?? 原因

**修复前**:
```csharp
// ? 硬编码长度为 10
var p2 = p1 + dir * 10;
```

**修复后**:
```csharp
// ? 使用实际长度
var vector = ExtractVector(vectorRef);  // 包含方向 * 长度
var p2 = p1 + vector;
```

---

## ?? STEP LINE 格式

```step
#49=LINE('',#62,#52);      ← LINE 实体
#62=CRTPNT('',(0.,0.,0.));    ← 起点
#52=VECTOR('',#58,82.356);     ← 方向 + 长度
#58=DRCTN('',(-0.99,0.14,0.)); ← 单位方向
```

### 计算公式

```
P2 = P1 + (Direction * Magnitude)

示例:
  P1 = (0, 0, 0)
  Direction = (-0.99, 0.14, 0)
  Magnitude = 82.356
  
  P2 = (0, 0, 0) + (-0.99 * 82.356, 0.14 * 82.356, 0)
     = (-81.51, 11.79, 0)  ?
```

---

## ?? 修复内容

### 1. ExtractVector 方法

```csharp
private Vector3D? ExtractVector(int entityId)
{
    // ? 提取方向引用
    var direction = ExtractDirection(dirRef.Id);
    
    // ? 提取长度（关键修复）
    double magnitude = 1.0;
    if (entity.Parameters.Count > 2)
 {
     magnitude = Convert.ToDouble(entity.Parameters[2]);
    }
    
    // ? 返回方向 * 长度
    return direction * magnitude;
}
```

### 2. ExtractDirection 方法（新增）

```csharp
private Vector3D? ExtractDirection(StepEntity entity)
{
    // DIRECTION('', (x, y, z))
    if (normalizedType == "DIRECTION" && entity.Parameters.Count > 1)
    {
        var coords = ParseCoordinates(entity.Parameters[1]);
        return new Vector3D(coords[0], coords[1], coords[2]);
    }
}
```

### 3. ExtractLinePoints 方法

```csharp
private Point3DCollection ExtractLinePoints(StepEntity entity)
{
    var p1 = ExtractCartesianPoint(point1Ref.Id);
    var vector = ExtractVector(vectorRef.Id);  // ? 完整向量
    
    var p2 = new Point3D(
        p1.X + vector.X,  // ? 不再硬编码
        p1.Y + vector.Y,
    p1.Z + vector.Z);
  
    points.Add(p1);
    points.Add(p2);
}
```

---

## ? 验证

### 编译

```
生成成功 ?
```

### 测试文件

**model13_5.stp**:
- 3 条 LINE
- 每条 LINE 长度不同

### 预期结果

```
LINE 1: 长度 ≈ 82.36  ?
LINE 2: 长度 ≈ 78.00  ?
LINE 3: 长度 ≈ 60.01  ?
```

---

## ?? 修改的文件

| 文件 | 修改 |
|------|------|
| `Services/Step214/Step214CurveExtractor.cs` | 修复 VECTOR/LINE 解析 |

---

## ?? 使用

```
1. ?? 导入 STEP 文件
2. ?? 查看 3D 视图
3. ? 线段长度正确
```

---

## ?? 调试输出

```
LINE: P1=(0.00, 0.00, 0.00)
LINE: Vector=(-81.51, 11.79, 0.00)  ← 完整向量
LINE: P2=(-81.51, 11.79, 0.00)  ?
```

---

**详细说明**: `STEP_LINE导入修复说明.md`  
**状态**: ? 已修复
