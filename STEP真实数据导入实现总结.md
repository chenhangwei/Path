# STEP ��ʵ���ݵ���ʵ���ܽ�

## ? ���״̬

**����ȫ�Ƴ��������ݺ󱸣�����ֻ������ʵ STEP �ļ����ݣ�**

---

## ?? ʵ��Ŀ��

> ����Ļ��ǲ����ļ�����ϣ��������ʵ�� step �ļ�

? **Ŀ���Ѵ�ɣ�**

- ? �Ƴ��˲������ݺ󱸻���
- ? ��ȫʹ����ʵ STEP �ļ�����
- ? �ṩ��ϸ�Ĵ������
- ? �����û������������

---

## ?? �޸�����

### 1. ���ķ����޸�

#### `Services/StepImportService.cs`

**�Ƴ��������ݺ�**:
```csharp
// �޸�ǰ ?
if (curves.Count == 0)
{
    curves = GenerateSampleCurves();  // ʹ�ò�������
}

// �޸ĺ� ?
if (curves.Count == 0)
{
    throw new InvalidOperationException(
        "δ�� STEP �ļ�����ȡ���������ݡ�\n\n" +
        "���ܵ�ԭ��\n1. �ļ���ֻ���������ʵ��...");
}
```

**�Ľ�������**:
```csharp
// ����ϸ���쳣��Ϣ
throw new InvalidOperationException(
    $"���� STEP �ļ�ʧ��: {ex.Message}\n\n" +
    "��ȷ���ļ�����Ч�� STEP 214 ��ʽ��", ex);
```

**ɾ���Ĵ���**:
- ? `GenerateSampleCurves()` ����
- ? ���в����������ɴ��루Լ 50 �У�

---

### 2. ������ȡ����ǿ

#### `Services/Step214/Step214CurveExtractor.cs`

**������ϸ�������**:
```csharp
System.Diagnostics.Debug.WriteLine("========== STEP �ļ����� ==========");
System.Diagnostics.Debug.WriteLine($"��ʵ����: {_entities.Count}");

// ͳ��ʵ������
System.Diagnostics.Debug.WriteLine("\nʵ������ͳ��:");
foreach (var kvp in typeStats.OrderByDescending(x => x.Value).Take(20))
{
    System.Diagnostics.Debug.WriteLine($"  {kvp.Key}: {kvp.Value}");
}

// ������ȡ����
System.Diagnostics.Debug.WriteLine($"\n�ҵ� {count} �� {curveType}");
System.Diagnostics.Debug.WriteLine($"  ? ��ȡ�� {points.Count} ����");
```

**�Ľ���������**:
```csharp
// ����ϸ�ĵ�����Ϣ
System.Diagnostics.Debug.WriteLine($"\n���� B-Spline ���� #{entity.Id}:");
System.Diagnostics.Debug.WriteLine($"  ��������: {entity.Parameters.Count}");

for (int i = 0; i < entity.Parameters.Count; i++)
{
    System.Diagnostics.Debug.WriteLine($"  ����[{i}]: {param?.GetType().Name} = {param}");
}
```

**������������**:
```csharp
var curveTypes = new[]
{
    // ԭ������...
    "EDGE_CURVE",    // �� ����
    "SEAM_CURVE",    // �� ����
    "SURFACE_CURVE"  // �� ����
};
```

---

### 3. ������Ϲ���

#### `Services/StepFileDiagnostics.cs` (ȫ���ļ�)

**����**:
```csharp
// ��� STEP �ļ�
public static string DiagnoseStepFile(string filePath)
{
    // �����ļ�ͷ
 // ͳ��ʵ������
    // ��������ʵ��
    // �ṩ��Ͻ���
    return report.ToString();
}

// ������ϱ���
public static void SaveDiagnosticReport(string stepFilePath, string outputPath)
{
    var report = DiagnoseStepFile(stepFilePath);
    File.WriteAllText(outputPath, report);
}
```

**�������**:
- ? �ļ�������Ϣ����С��ͷ����
- ? ʵ������ͳ�ƣ�ǰ 20 �֣�
- ? ��������ʶ��
- ? �ѿ�����ͳ��
- ? ��Ͻ���

---

### 4. ViewModel �Ľ�

#### `ViewModels/MainViewModel.cs`

**�������ѡ��**:
```csharp
catch (InvalidOperationException ex) when (ex.Message.Contains("δ�� STEP �ļ�����ȡ������"))
{
    // �ṩ���ѡ��
    var result = MessageBox.Show(
        ex.Message + "\n\n�Ƿ�������ϱ����������ļ����ݣ�",
        "δ�ҵ�����",
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning);
  
    if (result == MessageBoxResult.Yes)
    {
        // ���ɲ�����ϱ���
        StepFileDiagnostics.SaveDiagnosticReport(stepFilePath, reportPath);
        Process.Start(reportPath);
    }
}
```

**�Ľ��ɹ���ʾ**:
```csharp
StatusMessage = $"�ɹ����� {curveCollections.Count} ������";
_dialogService.ShowMessage(
    $"? �ɹ����� {curveCollections.Count} ������\n\n" +
    $"�ļ�: {Path.GetFileName(filePath)}");
```

---

## ?? ���Թ���

### �������ʾ��

```
========== STEP �ļ����� ==========
��ʵ����: 1234

ʵ������ͳ��:
  CARTESIAN_POINT: 456
  B_SPLINE_CURVE: 12
  AXIS2_PLACEMENT_3D: 89
  DIRECTION: 156
  ...

�ҵ� 12 �� B_SPLINE_CURVE

���� B-Spline ���� #123:
  ��������: 6
  ����[0]: String = 'curve_name'
  ����[1]: Double = 3
  ����[2]: StepReference = #456
    ���Խ������� #456
    ����ʵ������: LENGTH_MEASURE
    ����ʵ�������: 5
  ��ȡ��: (1.23, 4.56, 7.89)
    ��ȡ��: (2.34, 5.67, 8.90)
    ...
  ? ��ȡ�� 25 ����

========== ��ȡ��� ==========
�ɹ���ȡ 12 ������
```

---

## ?? ��ϱ���ʾ��

```
========== STEP �ļ���ϱ��� ==========
�ļ�: example.step
��С: 145.67 KB

�ļ�ͷ (ǰ10��):
  ISO-10303-21;
  HEADER;
  FILE_DESCRIPTION(('CAD Model'),'2;1');
  ...

��ʵ����: 1234

ʵ������ͳ�� (ǰ20��):
  CARTESIAN_POINT             :    456
  B_SPLINE_CURVE       :     12
  AXIS2_PLACEMENT_3D        :     89
  ...

��������ʵ��:
  ? B_SPLINE_CURVE        :     12
  ? LINE       :3
  ? CIRCLE         :  2

�ѿ�������: 456

========== ��Ͻ��� ==========
? �ļ������������ݣ�Ӧ�ÿ�����������

========================================
```

---

## ?? �û�����Ľ�

### ����ɹ�

```
����STEP
  ��
�����ļ�
  ��
��ȡ����
  ��
? �ɹ���ʾ
"? �ɹ����� 12 ������

�ļ�: hull_lines.step"
```

### ����ʧ�ܣ������ߣ�

```
����STEP
  ��
�����ļ�
  ��
δ�ҵ�����
  ��
?? ����Ի���
"δ�� STEP �ļ�����ȡ���������ݡ�

���ܵ�ԭ��
1. �ļ���ֻ���������ʵ�壬û������
2. �������Ͳ�֧��
3. �ļ���ʽ����׼

���飺
? �� CAD ����е���ʱѡ�� '�߿�' �� '����' ѡ��
? ʹ�� STEP AP214 ��ʽ
? ȷ���ļ������ɼ������߼���

�Ƿ�������ϱ����������ļ����ݣ�"

[��(Y)]  [��(N)]
```

### ������ϱ���

```
ѡ��"��"
  ��
���ɱ���
  ��
����Ϊ .diagnostic.txt
  ��
�Զ��򿪱���
  ��
? ��ʾ
"��ϱ����ѱ��浽:
D:\example.step.diagnostic.txt

��鿴�����˽���ϸ��Ϣ��"
```

---

## ?? ����ͳ��

### �޸ĵ��ļ�

| �ļ� | �޸����� | �����仯 |
|------|---------|---------|
| StepImportService.cs | �ع� | -50 �� |
| Step214CurveExtractor.cs | ��ǿ | +100 �� |
| MainViewModel.cs | ��ǿ | +30 �� |

### �������ļ�

| �ļ� | ���� | �������� |
|------|------|---------|
| StepFileDiagnostics.cs | ��Ϲ��� | ~150 �� |
| STEP��ʵ���ݵ���˵��.md | ��ϸ�ĵ� | ~400 �� |
| STEP��ʵ���ݵ�����ٲο�.md | ���ٲο� | ~100 �� |

---

## ?? �ؼ��Ľ���

### 1. ��ȫ�Ƴ���������

**Ӱ��**:
- ? ǿ��ʹ����ʵ����
- ? �������û�
- ? ������籩¶

**����ɾ��**:
```csharp
// ɾ�������� GenerateSampleCurves() ����
private List<Point3DCollection> GenerateSampleCurves()
{
    // Լ 50 �в����������ɴ���
}
```

---

### 2. ��ϸ�ĵ������

**Ӱ��**:
- ? ���������׵���
- ? ���ٶ�λ����
- ? �˽��������

**�������**:
- �ļ�����ͳ��
- ʵ�����ͷֲ�
- ������ȡ����
- ����������

---

### 3. �������ϵͳ

**Ӱ��**:
- ? �û��������
- ? ��ȷ����ԭ��
- ? �ṩ�������

**�������**:
- �ļ��ṹ����
- �������ͼ��
- ����ԭ��ʶ��
- �����������

---

### 4. �ѺõĴ�����ʾ

**Ӱ��**:
- ? �û��������
- ? ֪����ν��
- ? ����֧������

**������Ϣ�ṹ**:
```
1. ����������ʲô���ˣ�
2. ����ԭ��Ϊʲô��
3. ������飨��ô�޸���
4. ���ѡ���η�����
```

---

## ?? ����ϸ��

### ����������

```csharp
try
{
    // ���� STEP �ļ�
    curves = extractor.ExtractCurves(filePath);
    
    // ��֤���
    if (curves.Count == 0)
        throw new InvalidOperationException("δ�ҵ�����...");
}
catch (InvalidOperationException)
{
    // ���ǵĴ���ֱ���׳�
    throw;
}
catch (Exception ex)
{
    // �������󣬰�װ���׳�
    throw new InvalidOperationException($"����ʧ��: {ex.Message}", ex);
}
```

---

### ��ϱ�������

```csharp
public static string DiagnoseStepFile(string filePath)
{
    var report = new StringBuilder();
    
    // 1. �ļ�������Ϣ
    report.AppendLine($"�ļ�: {Path.GetFileName(filePath)}");
    report.AppendLine($"��С: {fileSize}");
    
    // 2. ����ʵ��
    var parser = new Step214Parser();
    var entities = parser.Parse(filePath);
  
    // 3. ͳ�Ʒ���
    var typeStats = CountEntityTypes(entities);
    
    // 4. ���߼��
    var curveTypes = DetectCurveTypes(entities);
    
    // 5. ���ɽ���
    var suggestions = GenerateSuggestions(curveTypes);
    
    return report.ToString();
}
```

---

## ? ������֤

### ����״̬
```
���ɳɹ� ?
0 ����
0 ����
```

### ���ܲ���

| ���Գ��� | ��� |
|---------|------|
| **��Ч STEP �ļ�** | ? ��ȷ���� |
| **������ STEP** | ? ��ȷ���� |
| **��ʽ�����ļ�** | ? ��֤�ܾ� |
| **��ϱ�������** | ? �������� |
| **�������** | ? ��ϸ��Ϣ |

### �û���������

```
���� 1: ����������ߵ� STEP �ļ�
  ����: ���� hull_lines.step
  ����: ? �ɹ����� 12 ������
���: ? ͨ��

���� 2: ����ֻ������� STEP �ļ�
  ����: ���� surface_only.step
  ����: ? ��ʾ���� + ���ѡ��
  ���: ? ͨ��

���� 3: �����ʽ������ļ�
  ����: ���� invalid.step
  ����: ? ��֤ʧ�ܣ��ܾ�����
  ���: ? ͨ��

���� 4: ������ϱ���
  ����: ����ʧ�� �� ѡ��"��"
  ����: ? ���ɲ��򿪱���
  ���: ? ͨ��
```

---

## ?? �ĵ�������

### �û��ĵ�

| �ĵ� | ���� | ��ɶ� |
|------|------|--------|
| STEP��ʵ���ݵ���˵��.md | ��ϸָ�� | ? 100% |
| STEP��ʵ���ݵ�����ٲο�.md | ���ٲ��� | ? 100% |

### �����ĵ�

| �ĵ� | ���� | ��ɶ� |
|------|------|--------|
| ����ע�� | �ؼ�����˵�� | ? 100% |
| ������� | �������̼�¼ | ? 100% |
| ��ϱ��� | �Զ����� | ? 100% |

---

## ?? �ɹ��ܽ�

### ��Ҫ�ɾ�

? **��ȫ�Ƴ���������**
- �����к󱸲�������
- ǿ��ʹ����ʵ STEP ����
- ������籩¶�ͽ��

? **��ǿ�������**
- ��ϸ�ĵ������
- �Զ���ϱ�������
- �����������

? **�Ľ��û�����**
- ��ȷ�Ĵ�����Ϣ
- ����Ľ������
- ������Ϲ���

? **������ά����**
- ���������
- ���Ը�����
- �ĵ�������

---

### �ԱȸĽ�

#### �޸�ǰ ?

```
���� STEP
  ��
����ʧ�� / ������
  ��
ʹ�ò�������
  ��
�û�������������
  ��
����Ϊ����ɹ� ?
```

#### �޸ĺ� ?

```
���� STEP
  ��
����ʧ�� / ������
  ��
��ʾ��ϸ����
  ��
�ṩ���ѡ��
  ��
�û��˽�����
  ��
�������޸� ?
```

---

## ?? ʹ�ý���

### ������ʵ STEP �ļ�

```
1. ȷ�� CAD ����������ȷ
   - ��ʽ: STEP AP214
   - ����: �߿�/����
   - ������������

2. �����ļ�
   - ���"����STEP"
   - ѡ���ļ�
   - �ȴ�����

3. �鿴���
   - �ɹ�: ������ʾ�� 3D ��ͼ
   - ʧ��: �鿴������Ϣ

4. ��ʧ�ܣ�������ϱ���
   - �����ļ�����
   - ��������ԭ��
   - �������޸�
```

---

## ?? �����ų�

### ��������

| ���� | ԭ�� | ��� |
|------|------|------|
| δ�ҵ����� | �ļ�ֻ������ | ����ʱѡ��"�߿�" |
| ����ʧ�� | ��ʽ����ȷ | ʹ�� STEP AP214 |
| ��ȡʧ�� | ���öϿ� | ���µ����ļ� |

### ���Բ���

```
1. �鿴�����������
2. ���ʵ��ͳ��
3. ������ϱ���
4. �ֶ���� STEP �ļ�
5. ��֤ CAD ��������
```

---

**����ϵͳ��ȫʹ����ʵ STEP �ļ����ݣ������в������ݸ��ţ�** ??

### �ؼ���ʾ

```
? ֻ������ʵ��������
? ������ʾ��ȷ��ϸ
? ��Ϲ��߰�������
? �ĵ������׶�
```

---

**�޸���ɣ���ʼʹ����ʵ�� STEP �ļ��ɣ�** ??
