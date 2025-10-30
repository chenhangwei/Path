# PathEditor3D.xaml.cs �޸��ܽ�

## ? �����ѽ��

�ɹ��޸��� `PathEditor3D.xaml.cs` �ļ������⣬��Ŀ���ڿ����������룡

---

## ?? �޸�����

### �����Դ

��֮ǰ�ı༭�����У�`PathEditor3D.xaml.cs` ��������滻���˾ɰ汾�� `PathEditor` �࣬���£�

1. **��������**: `PathEditor` ������ `PathEditor3D`
2. **ȱ�ٷֲ�������**: ���� `partial class`
3. **ȱ�ٹؼ�����**: �� `HitPlaneAtPoint`
4. **�����ռ����**: ������2D�༭�����߼�

### �޸�����

#### 1. ��д���ļ�

��������ȷ�� `PathEditor3D` �ֲ������ļ���

```csharp
namespace Path.Views
{
    public partial class PathEditor3D : UserControl
    {
        // �����ֶκ�����
        private const double _visualScale = 0.1;
        private double _pointSizeScale = 1.0;
        private bool _autoScalePoints = true;
     
        // ����
  private readonly List<SphereVisual3D> _pointVisuals = new();
        private readonly List<LinesVisual3D> _lineVisuals = new();
        private readonly Dictionary<...> _modelMap = new();
        
        // ���캯��
        public PathEditor3D()
        {
       InitializeComponent();
       // ��ʼ�� UG NX ������
  // ...
 }

        // DTO ����
     public record SegmentDto(...);
     public record UsvDto(...);
    }
}
```

#### 2. ���ȱʧ����

����� `HitPlaneAtPoint` ��������������ƽ��Ľ����⣺

```csharp
private Point3D? HitPlaneAtPoint(Point screenPoint)
{
    try
    {
        if (HelixView.Viewport is Viewport3D viewport)
  {
  var rayHit = Viewport3DHelper.FindNearestPoint(viewport, screenPoint);
  if (rayHit.HasValue)
   {
      return rayHit.Value;
  }
  }
    }
    catch { }
   return null;
}
```

#### 3. �Ƴ���ʱ����

��ʱע�͵��� `MainWindow.xaml.cs` �ж���δʵ�ֹ��ܵĵ��ã�

-RefreshUsvData` (��ʵ��)
- `HighlightStep` (��ʵ��)
- `ClearStepHighlight` (��ʵ��)

---

## ?? �ļ��ṹ

PathEditor3D ������һ��**�ֲ���**����ɢ�ڶ���ļ��У�

```
Views/
������ PathEditor3D.xaml.cs      �� ���ļ������޸���
������ PathEditor3D.xaml     �� XAML ����
������ PathEditor3D.Curves.cs    �� ������Ⱦ��չ
������ PathEditor3D.Size.cs      �� ��С������չ
������ PathEditor3D.Snap.cs      �� ��׽������չ
```

### ���ļ�ְ��

| �ļ� | ְ�� |
|------|------|
| `PathEditor3D.xaml.cs` | ���Ķ��塢����������DTO ���� |
| `PathEditor3D.Curves.cs` | �������ߵ���Ⱦ�͹��� |
| `PathEditor3D.Size.cs` | ���Ƶ��С�����ˢ�� |
| `PathEditor3D.Snap.cs` | ���ܲ�׽ϵͳ |

---

## ? ��֤���

### ����״̬
```
���ɳɹ� ?
0 ����
0 ����
```

### �ؼ�����

| ���� | ״̬ |
|------|------|
| ���ߵ�����Ⱦ | ? ���� |
| ���߷��� | ? ���� |
| ���߷�ת | ? ���� |
| ���ܲ�׽ | ? ���� |
| 3D ��ͼ���� | ? ���� |

---

## ?? ��ʵ�ֹ���

### 1. RefreshUsvData

�Ӳ�������ˢ�� 3D ��ͼ��

```csharp
public void RefreshUsvData(List<UsvDto> usvs)
{
    // ���浱ǰ���״̬
    // ������Ƶ���߶�
    // ������Ⱦ����
    // �ָ����״̬
}
```

### 2. �����������

```csharp
public void HighlightStep(int stepNumber, List<string> usvIds)
{
    // ���֮ǰ�ĸ���
    // �������� USV
  // �ҵ��ò����Ӧ�ĵ�
  // �����������壨��ɫ��1.5����С��
    // ���� USV ID ��ǩ
}

public void ClearStepHighlight()
{
    // �Ƴ����и�����ǩ
    // �Ƴ����и�������
}
```

---

## ?? ��һ���ж�

### �������õĹ���

1. ? **���ߵ���**: `Ctrl+I` ���� STEP �ļ�
2. ? **���߷���**: `Ctrl+L` ����ѡ������
3. ? **���߷�ת**: `Ctrl+R` ��ת��˳��
4. ? **3D ��ͼ**: �м���ת��Shift+�м�ƽ�ơ���������

### ��Ҫʵ�ֵĹ���

1. ? **�������**: ѡ�в���ʱ������ʾ��Ӧ�� USV ��
2. ? **����ˢ��**: �༭�������ݺ�ʵʱ���� 3D ��ͼ

---

## ?? ����ĵ�

- [USV���߷�ת����˵��.md](USV���߷�ת����˵��.md) - ���߷�ת��ϸ˵��
- [3D������Ⱦ�����ܽ�.md](3D������Ⱦ�����ܽ�.md) - ������Ⱦ����
- [�����������ʵ��ָ��.md](�����������ʵ��ָ��.md) - ��ʵ�ֵĸ�������

---

## ?? �ܽ�

### �ɹ��޸�������

1. ? ������ `PathEditor` ��Ϊ `PathEditor3D`
2. ? ��� `partial` �ؼ���
3. ? ʵ�� `HitPlaneAtPoint` ����
4. ? ��ȷ�����������ͼ
5. ? �Ƴ���ʱ���ñ���������

### ��ǰ״̬

- **����**: ? �ɹ�
- **���߹���**: ? ����
- **3D ��ͼ**: ? ����
- **���蹦��**: ? ��ʵ��

---

**��Ŀ���ڿ������������ˣ�** ??

���Կ�ʼʹ�ã�
1. ���� STEP �ļ�
2. ��������
3. ��ת���߷���
4. �鿴 3D Ч��

��ʵ�ֲ���������ܺ󣬿��Խ�һ����ǿ�û����顣
