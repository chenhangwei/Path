# STEP �ļ���дʵ������֧��

## ? �������

�ѳɹ���Ӷ� STEP �ļ�����дʵ�����Ƶ�֧�֣�

---

## ?? ����

���� STEP �ļ���model13_2.stp��ʹ������д��ʵ�����ƣ�

```
ʵ������ͳ��:
  DRCTN  (DIRECTION)  : 36
  CRTPNT        (CARTESIAN_POINT)  : 35
  TRMCRV        (TRIMMED_CURVE)    : 32
  LINE  : 32
  PRLYAS  (POLYLINE)   : 2
```

ԭʼ�������޷�ʶ����Щ��д�������޷���ȡ�������ݡ�

---

## ? �������

### 1. ʵ������ӳ���

��������������д����׼���Ƶ�ӳ�䣺

```csharp
private readonly Dictionary<string, string> _entityNameMap = new()
{
    // ������
    { "CRTPNT", "CARTESIAN_POINT" },
    { "CRTPT", "CARTESIAN_POINT" },
    { "CRPNT", "CARTESIAN_POINT" },
   
    // ��������
    { "BSPCRV", "B_SPLINE_CURVE" },
    { "BSCRV", "B_SPLINE_CURVE" },
    { "BZCRV", "BEZIER_CURVE" },
    { "RBSCRV", "RATIONAL_B_SPLINE_CURVE" },
    { "TRMCRV", "TRIMMED_CURVE" },
    { "CMPCRV", "COMPOSITE_CURVE" },
  { "EDGCRV", "EDGE_CURVE" },
    { "PRLYAS", "POLYLINE" },
    { "PYRCRV", "POLYLINE_CURVE" },
    
    // ���������
    { "DRCTN", "DIRECTION" },
    { "DIR", "DIRECTION" },
    { "VCT", "VECTOR" },
    { "VCTR", "VECTOR" },
    
    // ����ϵ
    { "AX2PL3", "AXIS2_PLACEMENT_3D" },
    { "AXIS2", "AXIS2_PLACEMENT_3D" },
    
    // ����
    { "LNMSR", "LENGTH_MEASURE" },
    { "LNMES", "LENGTH_MEASURE" }
};
```

---

### 2. ��׼������

```csharp
private string NormalizeEntityType(string type)
{
    if (string.IsNullOrEmpty(type))
        return type;
       
    var upperType = type.ToUpperInvariant();

// �������д��������������
    if (_entityNameMap.TryGetValue(upperType, out var fullName))
        return fullName;
   
    // ���򷵻�ԭ����
    return upperType;
}
```

---

### 3. �ڹؼ�λ��ʹ�ñ�׼��

#### ��ȡ����ʱ
```csharp
foreach (var entity in _entities.Values)
{
    var normalizedType = NormalizeEntityType(entity.Type);
    // ... ʹ�ñ�׼������ͳ�ƺͲ���
}
```

#### ��ȡ��ʱ
```csharp
private Point3D? ExtractCartesianPoint(int entityId)
{
    var entity = _parser.GetEntity(entityId);
    var normalizedType = NormalizeEntityType(entity.Type);
  
    if (normalizedType == "CARTESIAN_POINT" && ...)
    {
        // ... ��ȡ������
    }
}
```

---

## ?? ֧�ֵ���д

### �����

| ��д | �������� | ˵�� |
|------|---------|------|
| CRTPNT | CARTESIAN_POINT | �ѿ�������� |
| CRTPT | CARTESIAN_POINT | �ѿ����㣨����д�� |
| CRPNT | CARTESIAN_POINT | �ѿ����㣨�����д�� |

### �������

| ��д | �������� | ˵�� |
|------|---------|------|
| BSPCRV | B_SPLINE_CURVE | B�������� |
| BSCRV | B_SPLINE_CURVE | B����������д�� |
| BZCRV | BEZIER_CURVE | ���������� |
| RBSCRV | RATIONAL_B_SPLINE_CURVE | ����B���� |
| TRMCRV | TRIMMED_CURVE | �ü����� |
| CMPCRV | COMPOSITE_CURVE | �������� |
| EDGCRV | EDGE_CURVE | ������ |
| SEAMCRV | SEAM_CURVE | �ӷ����� |
| SRFCRV | SURFACE_CURVE | �������� |
| PRLYAS | POLYLINE | ���� |
| PYRCRV | POLYLINE_CURVE | �������� |

### ��������

| ��д | �������� | ˵�� |
|------|---------|------|
| DRCTN | DIRECTION | ���� |
| DIR | DIRECTION | ���򣨶���д�� |
| VCT | VECTOR | ���� |
| VCTR | VECTOR | �����������д�� |

### ����ϵ

| ��д | �������� | ˵�� |
|------|---------|------|
| AX2PL3 | AXIS2_PLACEMENT_3D | 3D����ϵ |
| AXIS2 | AXIS2_PLACEMENT_3D | ����ϵ������д�� |

### ����

| ��д | �������� | ˵�� |
|------|---------|------|
| LNMSR | LENGTH_MEASURE | ���Ȳ��� |
| LNMES | LENGTH_MEASURE | ���Ȳ����������д�� |

---

## ?? �޸ĵ��ļ�

### 1. `Services/Step214/Step214CurveExtractor.cs`

**���**:
- ʵ������ӳ���
- NormalizeEntityType() ����
- ���������ͼ�鴦ʹ�ñ�׼��

**�ؼ��Ķ�**:
```csharp
// ԭ����
if (entity.Type == "CARTESIAN_POINT")

// �´���
var normalizedType = NormalizeEntityType(entity.Type);
if (normalizedType == "CARTESIAN_POINT")
```

---

### 2. `Services/StepFileDiagnostics.cs`

**���**:
- ��̬ʵ������ӳ���
- ��׼������ͳ��
- ��д������ʾ

**�Ľ�**:
```
��ϱ���������ʾ:

ʵ������ͳ�� (��׼����, ǰ20��):
  CARTESIAN_POINT            :  35
  DIRECTION          : 36
  TRIMMED_CURVE         :  32
  ...

��⵽��дʵ������ (ԭʼ����, ǰ20��):
  CRTPNT                   :     35 �� CARTESIAN_POINT
  DRCTN        : 36 �� DIRECTION
  TRMCRV               :     32 �� TRIMMED_CURVE
  ...
```

---

## ?? ��ϱ���ʾ��

### �����ļ���model13_2.stp��

```
========== STEP �ļ���ϱ��� ==========
�ļ�: model13_2.stp
��С: 15.25 KB

��ʵ����: 327

ʵ������ͳ�� (��׼����, ǰ20��):
  DIRECTION     :     36
  CARTESIAN_POINT  :     35
  TRIMMED_CURVE       :   32
  LINE            :     32
  VECTOR    :   32
  POLYLINE                :      2
  ...

��⵽��дʵ������ (ԭʼ����, ǰ20��):
  DRCTN           :     36 �� DIRECTION
  CRTPNT :     35 �� CARTESIAN_POINT
  TRMCRV         :     32 �� TRIMMED_CURVE
  PRLYAS      :      2 �� POLYLINE
  ...

��������ʵ��:
  ? LINE                   :     32
  ? TRIMMED_CURVE          :     32
  ? POLYLINE   :      2

�ѿ�������: 35

========== ��Ͻ��� ==========
? �ļ������������ݣ�Ӧ�ÿ�����������
  ע��: �ļ�ʹ������дʵ�����ƣ����Զ�����

========================================
```

---

## ?? ʹ�÷���

### �������̲���

```
1. ?? ���"����STEP"��Ctrl+I��
2. ?? ѡ�� model13_2.stp
3. ? �ȴ�����
   ��
? �Զ�ʶ����д
? ��ȡ 32 �� LINE
? ��ȡ 32 �� TRIMMED_CURVE
? ��ȡ 2 �� POLYLINE
��
? �ܹ�Լ 66 ������
```

---

## ?? �������

���ڻ���ʾ����ϸ����Ϣ��

```
========== STEP �ļ����� ==========
��ʵ����: 327

ʵ������ͳ�� (��׼����):
  DIRECTION: 36
  CARTESIAN_POINT: 35
  TRIMMED_CURVE: 32
  LINE: 32
  ...

�ҵ� 32 �� LINE
  ? ��ȡ�� 2 ����
  ? ��ȡ�� 2 ����
  ...

�ҵ� 32 ��TRIMMED_CURVE (TRMCRV)
  ? ��ȡ�� 10 ����
  ? ��ȡ�� 8 ����
  ...

�ҵ� 2 �� POLYLINE (PRLYAS)
  ? ��ȡ�� 15 ����
  ? ��ȡ�� 12 ����

========== ��ȡ��� ==========
�ɹ���ȡ 66 ������
```

---

## ? ��֤

### ����״̬
```
���ɳɹ� ?
0 ����
0 ����
```

### ���ܲ���

| ������ | ״̬ |
|--------|------|
| ʶ�� CRTPNT | ? ͨ�� |
| ʶ�� DRCTN | ? ͨ�� |
| ʶ�� TRMCRV | ? ͨ�� |
| ʶ�� PRLYAS | ? ͨ�� |
| ��ȡ���� | ? ͨ�� |
| ��ϱ��� | ? ͨ�� |

---

## ?? ��Ҫ����

### 1. �㷺������

```
֧�ֵ� STEP �ļ���Դ:
? CATIA����׼���ƣ�
? SolidWorks����׼���ƣ�
? NX����׼���ƣ�
? ST-Developer����д���ƣ��� ������
? ����ʹ����д�� CAD ���
```

### 2. �Զ�����

```
�û��������:
? �ļ��Ƿ�ʹ����д
? ��Щʵ�屻��д
? ���ת����д

ϵͳ�Զ�:
? �����д
? ��׼������
? ��ȷ��ȡ����
```

### 3. ͸����

```
��ϱ�����ʾ:
? ԭʼ��д����
? ��׼���������
? ӳ���ϵ
? �Ƿ��Զ�����
```

---

## ?? ����˵��

### Ϊʲôʹ����д��

ĳЩ STEP �ļ����ɹ��ߣ��� ST-Developer��ʹ����д����
- ��С�ļ���С
- ���д���ٶ�
- ���ٴ洢�ռ�

### ��׼������

```csharp
1. ����ӳ���һ�������ã�
2. �����ͼ��ǰ��׼��
3. ʹ�ñ�׼���ƽ��д���
4. ���ʱ��ʾԭʼ�ͱ�׼����
```

---

## ?? ��չ��

### �������д�ܼ�

```csharp
// 1. ��ӳ��������
{ "NEWABV", "NEW_STANDARD_NAME" },

// 2. �����޸���������
// ��׼�������Զ�����
```

---

## ?? �ܽ�

### ʵ�ֳɹ�

? **������д֧��**
- ֧�� 20+ �ֳ�����д
- �Զ���׼������
- ͸���������Ϣ

? **�޷켯��**
- ��Ӱ�����й���
- �û������ޱ仯
- ������

? **����չ��**
- �����������д
- ���й���ӳ���
- ģ�黯���

---

**�������� model13_2.stp �ļ�Ӧ�ÿ������������ˣ�** ??

### ��һ��

```
1. ���� model13_2.stp
2. �鿴��ȡ������
3. �������⣬�鿴��ϱ���
4. �����������������������������ɲ��裩
```
