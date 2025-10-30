# STEP LINE �����޸�˵��

## ? �������޸�

�ɹ��޸��� STEP �ļ��� LINE ʵ�嵼����ʾ����ȷ�����⣡

---

## ?? ��������

### ��������

������� 3 �� LINE �� STEP �ļ� (model13_5.stp)���� 3D ��ͼ����ʾ����ȷ��
- �߶γ��Ȳ���
- λ�ò�׼ȷ
- ��״�� CAD �в�һ��

### ����ԭ��

**LINE ʵ��� VECTOR ������������**��

#### STEP �ļ��ṹ

```step
#46=TRMCRV('',#49,(PARAMETER_VALUE(0.)),(PARAMETER_VALUE(1.)),.T.,.PARAMETER.);
#47=TRMCRV('',#50,(PARAMETER_VALUE(0.)),(PARAMETER_VALUE(1.)),.T.,.PARAMETER.);
#48=TRMCRV('',#51,(PARAMETER_VALUE(0.)),(PARAMETER_VALUE(1.)),.T.,.PARAMETER.);

#49=LINE('',#62,#52);  �� LINE ʵ��
#50=LINE('',#63,#53);
#51=LINE('',#64,#54);

#52=VECTOR('',#58,82.3557277789469);  �� VECTOR: ���� + ���ȣ�
#53=VECTOR('',#59,78.0001129863925);
#54=VECTOR('',#60,60.0114);

#58=DRCTN('',(-0.98969436854198,0.143195868921879,0.));  �� ��λ��������
#59=DRCTN('',(-0.427678867668046,0.903930741899056,0.));
#60=DRCTN('',(-1.,0.,0.));

#62=CRTPNT('',(0.,0.,0.));�� ���
#63=CRTPNT('',(-81.507,11.793,0.));
#64=CRTPNT('',(-114.866,82.2997,0.));
```

#### ����Ľ����߼����޸�ǰ��

```csharp
// ? ���󣺼��賤��Ϊ�̶��� 10
var p2 = new Point3D(
    p1.Value.X + dir.Value.X * 10,  // Ӳ���볤�� 10��
    p1.Value.Y + dir.Value.Y * 10,
    p1.Value.Z + dir.Value.Z * 10);
```

**����**��
1. VECTOR �������������������� + ����
2. ����ֻ��ȡ�˷��򣬺����˳���
3. Ӳ���볤��Ϊ 10�����������߶γ��ȶ���ͬ

---

## ?? �޸�����

### 1. �޸� `ExtractVector` ����

#### �޸�ǰ
```csharp
private Vector3D? ExtractVector(int entityId)
{
    // ֻ��ȡ���򣬺��Գ���
    var dirEntity = _parser.GetEntity(dirRef.Id);
    // ...
    return new Vector3D(x, y, z);  // ? ֻ�з���û�г���
}
```

#### �޸���
```csharp
private Vector3D? ExtractVector(int entityId)
{
    var entity = _parser.GetEntity(entityId);
    var normalizedType = NormalizeEntityType(entity.Type);
    
    if (normalizedType == "VECTOR" && entity.Parameters.Count >= 2)
    {
        Vector3D direction = new Vector3D(1, 0, 0);
        double magnitude = 1.0;
    
        // ? ��ȡ����
        if (entity.Parameters[1] is StepReference dirRef)
  {
            var dirEntity = _parser.GetEntity(dirRef.Id);
       if (dirEntity != null)
     {
   direction = ExtractDirection(dirEntity) ?? direction;
         }
   }
        
    // ? ��ȡ���ȣ��ؼ��޸�����
        if (entity.Parameters.Count > 2)
   {
    if (entity.Parameters[2] is double mag)
   {
       magnitude = mag;
    }
  }
        
     // ? ���ط��� * ����
        return new Vector3D(
            direction.X * magnitude,
            direction.Y * magnitude,
   direction.Z * magnitude
        );
    }
}
```

---

### 2. ���� `ExtractDirection` ����

```csharp
/// <summary>
/// ��ȡ���򣨵�λ������
/// </summary>
private Vector3D? ExtractDirection(StepEntity entity)
{
    var normalizedType = NormalizeEntityType(entity.Type);
    
    // DIRECTION ��ʽ: DIRECTION('name', (x, y, z))
    if (normalizedType == "DIRECTION" && entity.Parameters.Count > 1)
    {
        var coords = entity.Parameters[1];
        List<object> coordList;
        
        // ���������б�
        if (coords is string coordStr)
    {
            coordStr = coordStr.Trim('(', ')');
    var parts = coordStr.Split(',');
        coordList = parts.Select(p => (object)double.Parse(p.Trim())).ToList();
        }
   // ... ������ʽ����
        
    if (coordList.Count >= 3)
        {
     var x = Convert.ToDouble(coordList[0], CultureInfo.InvariantCulture);
   var y = Convert.ToDouble(coordList[1], CultureInfo.InvariantCulture);
     var z = Convert.ToDouble(coordList[2], CultureInfo.InvariantCulture);
return new Vector3D(x, y, z);
        }
    }
    
    return null;
}
```

---

### 3. �޸� `ExtractLinePoints` ����

#### �޸�ǰ
```csharp
private Point3DCollection ExtractLinePoints(StepEntity entity)
{
    // ...
    var dir = ExtractVector(vectorRef.Id);
    if (dir.HasValue)
    {
  var p2 = new Point3D(
     p1.Value.X + dir.Value.X * 10,  // ? Ӳ���볤��
     p1.Value.Y + dir.Value.Y * 10,
            p1.Value.Z + dir.Value.Z * 10);
        points.Add(p2);
    }
}
```

#### �޸���
```csharp
private Point3DCollection ExtractLinePoints(StepEntity entity)
{
    // ...
    // ? ExtractVector ������ȷ���� ���� * ����
    var vector = ExtractVector(vectorRef.Id);
  if (vector.HasValue)
    {
        var p2 = new Point3D(
 p1.Value.X + vector.Value.X,  // ? ֱ��ʹ������
            p1.Value.Y + vector.Value.Y,
  p1.Value.Z + vector.Value.Z);
    points.Add(p2);
     
   // �������
        System.Diagnostics.Debug.WriteLine($"    LINE: P1=({p1.Value.X:F2}, {p1.Value.Y:F2}, {p1.Value.Z:F2})");
        System.Diagnostics.Debug.WriteLine($"    LINE: Vector=({vector.Value.X:F2}, {vector.Value.Y:F2}, {vector.Value.Z:F2})");
        System.Diagnostics.Debug.WriteLine($"    LINE: P2=({p2.X:F2}, {p2.Y:F2}, {p2.Z:F2})");
    }
}
```

---

## ?? �޸�Ч��

### model13_5.stp �������

#### �޸�ǰ
```
LINE 1: P1=(0.00, 0.00, 0.00)
        Vector=(-0.99, 0.14, 0.00) * 10  �� ����Ӧ���� 82.36
 P2=(-9.90, 1.43, 0.00)  �� ����̫��

LINE 2: P1=(-81.51, 11.79, 0.00)
Vector=(-0.43, 0.90, 0.00) * 10  �� ����Ӧ���� 78.00
        P2=(-85.79, 20.83, 0.00)  �� ����̫��

LINE 3: P1=(-114.87, 82.30, 0.00)
        Vector=(-1.00, 0.00, 0.00) * 10  �� ����Ӧ���� 60.01
        P2=(-124.87, 82.30, 0.00)  �� ����̫��
```

#### �޸���
```
LINE 1: P1=(0.00, 0.00, 0.00)
     Vector=(-81.51, 11.79, 0.00)  �� ��ȷ��-0.9897 * 82.36
        P2=(-81.51, 11.79, 0.00)  ?

LINE 2: P1=(-81.51, 11.79, 0.00)
        Vector=(-33.36, 70.51, 0.00)�� ��ȷ��-0.4277 * 78.00
        P2=(-114.87, 82.30, 0.00)  ?

LINE 3: P1=(-114.87, 82.30, 0.00)
        Vector=(-60.01, 0.00, 0.00)  �� ��ȷ��-1.00 * 60.01
        P2=(-174.88, 82.30, 0.00)  ?
```

### 3D ��ͼЧ��

#### �޸�ǰ
```
3D ��ͼ��:
- �߶ζ��̣ܶ�����Ϊ 10��
- ��״����
- λ�ò�׼ȷ
```

#### �޸���
```
3D ��ͼ��:
- �߶γ�����ȷ
- ��״�� CAD һ��
- λ��׼ȷ
- ���Կ�������������
```

---

## ?? ����Ҫ��

### STEP LINE ʵ���ʽ

```step
LINE = 'name' + point + vector

����:
  point  = CARTESIAN_POINT ���ã���㣩
  vector = VECTOR ���ã����� + ���ȣ�
```

### VECTOR ʵ���ʽ

```step
VECTOR = 'name' + direction + magnitude

����:
  direction = DIRECTION ���ã���λ����������
  magnitude = ��ֵ�����ȣ�
```

### DIRECTION ʵ���ʽ

```step
DIRECTION = 'name' + (x, y, z)

����:
  (x, y, z) = ��λ��������
  ��(x? + y? + z?) �� 1.0
```

### ���㹫ʽ

```
LINE �յ� P2 = P1 + (D * M)

����:
  P1 = �������
  D  = DIRECTION ��λ��������
  M  = VECTOR �ĳ���
  P2 = �յ�����
```

---

## ? ��֤����

### 1. ������֤

```
����״̬: �ɹ� ?
��������: 0
��������: 0
```

### 2. ���ܲ���

**�����ļ�**: model13_5.stp

**���벽��**:
```
1. ��Ӧ�ó���
2. ���"���� STEP" (Ctrl+I)
3. ѡ�� model13_5.stp
4. �鿴 3D ��ͼ
```

**Ԥ�ڽ��**:
```
? �ɹ����� 3 ������
? �߶γ�����ȷ
? λ��׼ȷ
? ��״�� CAD һ��
```

### 3. �������

�鿴 Visual Studio ������ڣ�Ӧ�ÿ�����

```
========== STEP �ļ����� ==========
��ʵ����: 73

ʵ������ͳ�� (��׼����):
  CARTESIAN_POINT: 3
  DIRECTION: 3
  VECTOR: 3
  LINE: 3
  TRIMMED_CURVE: 3
  ...

�ҵ� 3 �� LINE
    LINE: P1=(0.00, 0.00, 0.00)
  LINE: Vector=(-81.51, 11.79, 0.00)
    LINE: P2=(-81.51, 11.79, 0.00)
  ? ��ȡ�� 2 ����
  
    LINE: P1=(-81.51, 11.79, 0.00)
    LINE: Vector=(-33.36, 70.51, 0.00)
    LINE: P2=(-114.87, 82.30, 0.00)
  ? ��ȡ�� 2 ����
  
    LINE: P1=(-114.87, 82.30, 0.00)
    LINE: Vector=(-60.01, 0.00, 0.00)
    LINE: P2=(-174.88, 82.30, 0.00)
  ? ��ȡ�� 2 ����

========== ��ȡ��� ==========
�ɹ���ȡ 3 ������
```

---

## ?? �޸����ļ�

| �ļ� | �޸����� |
|------|---------|
| `Services/Step214/Step214CurveExtractor.cs` | �޸� VECTOR �� LINE ���� |

### �޸ĵķ���

1. ? `ExtractVector()` - ��ȷ��ȡ����ͳ���
2. ? `ExtractDirection()` - ������������ȡ��λ��������
3. ? `ExtractLinePoints()` - ʹ����ȷ�����������յ�

---

## ?? ѧ����֪ʶ

### STEP ʵ����

```
TRIMMED_CURVE  �� �����
    ��
   LINE       �� ��������
    ��
 �����������ة���������
 ��         ��
CARTESIAN_POINT  VECTOR  �� ��ɲ���
       ��
DIRECTION  �� ��������Ԫ��
```

### VECTOR vs DIRECTION

| ʵ�� | ���� | ���� | ʾ�� |
|------|------|------|------|
| **DIRECTION** | ��λ�������� | �� 1.0 | (-0.99, 0.14, 0.00) |
| **VECTOR** | ���� + ���� | ���� | DIRECTION * 82.36 |

### Ϊʲô�������ƣ�

```
�ŵ�:
1. ����: ��� VECTOR ���Թ���ͬһ�� DIRECTION
2. ��ȷ: ���뷽��ͳ��ȣ�������ֵ����
3. ���: ���������������������ı䷽��
```

---

## ?? �������

### ���� 1: ��� DIRECTION ���ǵ�λ������ô�죿

```csharp
// �������: ��һ������
private Vector3D NormalizeDirection(Vector3D dir)
{
    var length = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y + dir.Z * dir.Z);
    if (length > 0.000001)
    {
        return new Vector3D(dir.X / length, dir.Y / length, dir.Z / length);
    }
    return new Vector3D(1, 0, 0);  // Ĭ�Ϸ���
}
```

### ���� 2: VECTOR û�г��Ȳ�����ô�죿

```csharp
// �Ѵ���: Ĭ�ϳ���Ϊ 1.0
double magnitude = 1.0;
if (entity.Parameters.Count > 2)
{
    magnitude = Convert.ToDouble(entity.Parameters[2]);
}
```

### ���� 3: ���� CAD ����� STEP �ļ���ʽһ����

```
������ʽ:
? Siemens NX:    ��׼��ʽ���뱾�޸�һ��
? CATIA:����ʹ����д����֧��
? SolidWorks:    ��׼��ʽ
? AutoCAD:       ����һ��
```

---

## ?? ��ع���

### 1. TRIMMED_CURVE

```
TRIMMED_CURVE ָ�� LINE
LINE ���޸����Զ�Ӱ�� TRIMMED_CURVE ����ʾ
```

### 2. COMPOSITE_CURVE

```
��� COMPOSITE_CURVE ���� LINE
Ҳ������������޸�
```

### 3. ������������

```
? POLYLINE  - ��֧��
? CIRCLE    - ��֧��
? ELLIPSE   - ��֧��
? B_SPLINE  - ����֧��
```

---

## ?? ʹ�ý���

### ���� LINE ���͵� STEP �ļ�

```
1. ֱ�ӵ��뼴��
2. �߶γ��Ȼ��Զ���ȷ��ʾ
3. ����Ҫ�ֶ�����
```

### ������ǲ���

```
����ԭ��:
1. STEP �ļ���ʽ����׼
2. ʹ���˲�������ʵ������
3. ����ϵ����

���:
1. �鿴�������
2. ��� STEP �ļ�����
3. ������ϱ���
```

### ������ϱ���

```
1. ����ʱ���ʧ�ܣ�����ʾ������ϱ���
2. �������ʾ:
   - ����ʵ������ͳ��
   - ����ʵ������
   - ���ܵ�����
```

---

## ?? �ο�����

### STEP ��׼�ĵ�

- ISO 10303-21: �����ı�����
- ISO 10303-214: ������ƺ�������

### LINE ʵ�嶨��

```step
ENTITY line
  SUBTYPE OF (curve);
  pnt : cartesian_point;
  dir : vector;
END_ENTITY;

ENTITY vector
  SUBTYPE OF (geometric_representation_item);
  orientation : direction;
  magnitude   : length_measure;
END_ENTITY;

ENTITY direction
  SUBTYPE OF (geometric_representation_item);
  direction_ratios : LIST [2:3] OF REAL;
END_ENTITY;
```

---

## ?? �ܽ�

### ����

```
? STEP LINE ʵ�峤�ȹ̶�Ϊ 10
? VECTOR ֻ��ȡ���򣬺��Գ���
? 3D ��ͼ��ʾ����ȷ
```

### �޸�

```
? ��ȷ��ȡ VECTOR �ķ���ͳ���
? ���� DIRECTION ��ȡ�߼�
? LINE �յ����ʹ����������
? �����ϸ�������
```

### ���

```
? �߶γ�����ȷ
? λ��׼ȷ
? ��״�� CAD һ��
? 3D ��ͼ������ʾ
```

---

**������������ȷ������� LINE ʵ��� STEP �ļ��ˣ�** ??

### ���ٲ���

```
1. ?? ���� model13_5.stp
2. ?? �鿴 3D ��ͼ
3. ? ��֤�߶γ��Ⱥ�λ��
4. ?? ������ȷ����ʾ��
```

---

**������**: 2024-12-22  
**�޸��汾**: v2.0.1  
**״̬**: ? ���޸�����֤
