# STEP LINE �����޸����ٲο�

## ?? ����

����� LINE �߶��� 3D ��ͼ����ʾ����ȷ

---

## ? ���޸�

�޸��� VECTOR ʵ��Ľ�������ȷ��ȡ����ͳ���

---

## ?? ԭ��

**�޸�ǰ**:
```csharp
// ? Ӳ���볤��Ϊ 10
var p2 = p1 + dir * 10;
```

**�޸���**:
```csharp
// ? ʹ��ʵ�ʳ���
var vector = ExtractVector(vectorRef);  // �������� * ����
var p2 = p1 + vector;
```

---

## ?? STEP LINE ��ʽ

```step
#49=LINE('',#62,#52);      �� LINE ʵ��
#62=CRTPNT('',(0.,0.,0.));    �� ���
#52=VECTOR('',#58,82.356);     �� ���� + ����
#58=DRCTN('',(-0.99,0.14,0.)); �� ��λ����
```

### ���㹫ʽ

```
P2 = P1 + (Direction * Magnitude)

ʾ��:
  P1 = (0, 0, 0)
  Direction = (-0.99, 0.14, 0)
  Magnitude = 82.356
  
  P2 = (0, 0, 0) + (-0.99 * 82.356, 0.14 * 82.356, 0)
     = (-81.51, 11.79, 0)  ?
```

---

## ?? �޸�����

### 1. ExtractVector ����

```csharp
private Vector3D? ExtractVector(int entityId)
{
    // ? ��ȡ��������
    var direction = ExtractDirection(dirRef.Id);
    
    // ? ��ȡ���ȣ��ؼ��޸���
    double magnitude = 1.0;
    if (entity.Parameters.Count > 2)
 {
     magnitude = Convert.ToDouble(entity.Parameters[2]);
    }
    
    // ? ���ط��� * ����
    return direction * magnitude;
}
```

### 2. ExtractDirection ������������

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

### 3. ExtractLinePoints ����

```csharp
private Point3DCollection ExtractLinePoints(StepEntity entity)
{
    var p1 = ExtractCartesianPoint(point1Ref.Id);
    var vector = ExtractVector(vectorRef.Id);  // ? ��������
    
    var p2 = new Point3D(
        p1.X + vector.X,  // ? ����Ӳ����
        p1.Y + vector.Y,
    p1.Z + vector.Z);
  
    points.Add(p1);
    points.Add(p2);
}
```

---

## ? ��֤

### ����

```
���ɳɹ� ?
```

### �����ļ�

**model13_5.stp**:
- 3 �� LINE
- ÿ�� LINE ���Ȳ�ͬ

### Ԥ�ڽ��

```
LINE 1: ���� �� 82.36  ?
LINE 2: ���� �� 78.00  ?
LINE 3: ���� �� 60.01  ?
```

---

## ?? �޸ĵ��ļ�

| �ļ� | �޸� |
|------|------|
| `Services/Step214/Step214CurveExtractor.cs` | �޸� VECTOR/LINE ���� |

---

## ?? ʹ��

```
1. ?? ���� STEP �ļ�
2. ?? �鿴 3D ��ͼ
3. ? �߶γ�����ȷ
```

---

## ?? �������

```
LINE: P1=(0.00, 0.00, 0.00)
LINE: Vector=(-81.51, 11.79, 0.00)  �� ��������
LINE: P2=(-81.51, 11.79, 0.00)  ?
```

---

**��ϸ˵��**: `STEP_LINE�����޸�˵��.md`  
**״̬**: ? ���޸�
