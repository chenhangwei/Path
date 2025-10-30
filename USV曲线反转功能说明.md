# USV ���ߵ�˳��ת����

## ?? ����˵��

����˶�ѡ�е� USV ���߷�������з�ת����Ĺ��ܣ���ת�����߽����෴������У�3D ��ͼ�е���ȾҲ����Ӧ���¡�

---

## ? ��������

### ���Ĺ���
1. **��ת������˳��**: ��ѡ�����ߵ����з����㰴�෴˳����������
2. **�Զ����� 3D ��ͼ**: ��ת���������� 3D ��Ⱦ
3. **��������**: ֻ���ѷ��������߲���ִ�з�ת����

---

## ?? ʵ��ϸ��

### 1. MainViewModel.cs - ��ӷ�ת����

**λ��**: `ViewModels/MainViewModel.cs`  
**Region**: `#region ��������`

```csharp
/// <summary>
/// ��תѡ�����ߵķ�����˳��
/// </summary>
[RelayCommand(CanExecute = nameof(CanReverseCurve))]
private void ReverseCurve()
{
    if (SelectedCurve == null || !SelectedCurve.IsLofted)
    return;

    try
    {
        IsBusy = true;

        // ��ת�������б�
        var reversedPoints = new Point3DCollection(SelectedCurve.LoftedPoints.Reverse());
        SelectedCurve.LoftedPoints = reversedPoints;

        StatusMessage = $"���� {SelectedCurve.Name} �ĵ�˳���ѷ�ת";
        _dialogService.ShowMessage($"���� {SelectedCurve.Name} �ĵ�˳���ѷ�ת");
    }
    catch (Exception ex)
 {
        _dialogService.ShowError($"��תʧ��: {ex.Message}");
      StatusMessage = "��תʧ��";
}
    finally
    {
        IsBusy = false;
    }
}

private bool CanReverseCurve() => SelectedCurve != null && SelectedCurve.IsLofted;
```

**�ؼ���**:
- ʹ�� `[RelayCommand]` �����Զ���������
- `CanExecute` ȷ��ֻ���ѷ��������߿��Է�ת
- ʹ�� `Point3DCollection.Reverse()` ��ת��˳��
- ���� `LoftedPoints` ���� `INotifyPropertyChanged` ֪ͨ UI

---

### 2. MainWindow.xaml - ��Ӳ˵���

**�˵���**:
```xaml
<MenuItem Header="����(_L)">
    <MenuItem Header="����ѡ������(_S)" Command="{Binding LoftSelectedCurveCommand}" InputGestureText="Ctrl+L">
      <MenuItem.Icon>
   <TextBlock Text="??" FontSize="16"/>
        </MenuItem.Icon>
    </MenuItem>
    <MenuItem Header="������������(_A)" Command="{Binding LoftAllCurvesCommand}" InputGestureText="Ctrl+Shift+L">
        <MenuItem.Icon>
   <TextBlock Text="??" FontSize="16"/>
     </MenuItem.Icon>
    </MenuItem>
    <Separator/>
    <MenuItem Header="��ת���ߵ�˳��(_R)" Command="{Binding ReverseCurveCommand}" InputGestureText="Ctrl+R">
  <MenuItem.Icon>
        <TextBlock Text="??" FontSize="16"/>
        </MenuItem.Icon>
  </MenuItem>
</MenuItem>
```

**������**:
```xaml
<Button Command="{Binding LoftSelectedCurveCommand}" ToolTip="����ѡ������ (Ctrl+L)">
    <StackPanel Orientation="Horizontal">
     <TextBlock Text="?? " FontSize="16"/>
        <TextBlock Text="����" VerticalAlignment="Center"/>
    </StackPanel>
</Button>
<Button Command="{Binding LoftAllCurvesCommand}" ToolTip="������������ (Ctrl+Shift+L)">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="?? " FontSize="16"/>
 <TextBlock Text="����ȫ��" VerticalAlignment="Center"/>
    </StackPanel>
</Button>
<Button Command="{Binding ReverseCurveCommand}" ToolTip="��תѡ�����ߵ�˳�� (Ctrl+R)">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="?? " FontSize="16"/>
        <TextBlock Text="��ת" VerticalAlignment="Center"/>
    </StackPanel>
</Button>
```

---

### 3. MainWindow.xaml.cs - ��ӿ�ݼ�

**���̿�ݼ�**: `Ctrl+R`

```csharp
// Ctrl+R: ��ת����
InputBindings.Add(new System.Windows.Input.KeyBinding(
    new RelayCommand(() => _viewModel.ReverseCurveCommand.Execute(null)),
    System.Windows.Input.Key.R,
    System.Windows.Input.ModifierKeys.Control));
```

---

## ?? ʹ������

### ������������

```
1. ���� STEP �ļ�
   ��
   �����б���ʾ���������

2. ѡ��һ������
   ��
   ���߱�����ѡ��

3. �������� (Ctrl+L)
   ��
   ���ɷ����㣬3D ��ͼ��ʾ����
   ����: A �� B �� C �� D

4. ��ת���� (Ctrl+R ���� ?? ��ť)
   ��
   ������˳��ת
   �·���: D �� C �� B �� A

5. 3D ��ͼ�Զ�����
   ��
   ���߰��·�����Ⱦ
   ��ǩ˳��: 1(D) �� 2(C) �� 3(B) �� 4(A)

6. �ɼ������ɲ���
   ��
  ʹ�÷�ת�����������·��
```

---

## ?? Ч����ʾ

### ���� 1: ֱ��·����ת

**��תǰ**:
```
A ������ B ������ C ������ D
1     2     3     4
```

**��ת��**:
```
D ������ C ������ B ������ A
1     2     3     4
(����λ�ò��䣬����ŷ�ת)
```

### ���� 2: ����·����ת

**��תǰ**:
```
    C��D
   �u   �v
  B     E
 �u       �v
A   F
���      �յ�
```

**��ת��**:
```
 C��D
   �u   �v
  B     E
 �u       �v
A   F
�յ�  ���
(�����෴)
```

### ���� 3: ����·����ת

**��תǰ**: ˳ʱ������  
**��ת��**: ��ʱ������

---

## ?? ʹ�ó���

### ���� 1: ·���������

**����**: ��������߷�����Ԥ���෴  
**���**: ʹ�÷�ת���ܵ�������

### ���� 2: �Գ�·�����

**����**: ��Ҫ��������ԳƵ�·��  
**���**: 
1. ������һ������
2. ��ת�����෴�����·��
3. ���ڶԳƱ��

### ���� 3: ѭ��·��

**����**: ��Ҫ����������·��  
**���**:
1. ����·��: A �� B �� C
2. ��ת·��: C �� B �� A
3. ��ϳ�ѭ��

---

## ?? ����ʵ��

### ������

```
�û����"��ת"��ť
    ��
Command.Execute() ����
    ��
ReverseCurve() ����
    ��
SelectedCurve.LoftedPoints.Reverse()
    ��
new Point3DCollection(reversed)
��
SelectedCurve.LoftedPoints = reversedPoints
    ��
���� INotifyPropertyChanged
  ��
UI ���ݰ󶨸���
    ��
RefreshCurveVisualization() ������
    ��
PathEditor3D.RenderImportedCurves()
    ��
3D ��ͼ������Ⱦ
    ��
���߰���˳����ʾ
```

### �ؼ�����

**��ת�߼�**:
```csharp
// ʹ�� LINQ Reverse() ����
var reversedPoints = new Point3DCollection(
    SelectedCurve.LoftedPoints.Reverse()
);

// ��ֵ�������Ա仯֪ͨ
SelectedCurve.LoftedPoints = reversedPoints;
```

**�Զ����»���**:
```csharp
// PathCurveModel ʵ���� INotifyPropertyChanged
public Point3DCollection LoftedPoints
{
    get => _loftedPoints;
    set => SetField(ref _loftedPoints, value);  // ���� PropertyChanged
}

// MainWindow �������Ա仯
_viewModel.Curves.CollectionChanged += (s, e) => RefreshCurveVisualization();
foreach (var curve in _viewModel.Curves)
{
    curve.PropertyChanged += (s, e) =>
    {
  if (e.PropertyName == nameof(PathCurveModel.LoftedPoints))
      {
            RefreshCurveVisualization();  // �Զ�ˢ�� 3D ��ͼ
        }
    };
}
```

---

## ? ��֤�嵥

### ���ܲ���
- [x] ���� STEP �ļ�
- [x] ��������
- [x] ��ת���ߵ�˳��
- [x] 3D ��ͼ�Զ�����
- [x] ��ת�����ɲ���

### UI ����
- [x] �˵���������ʾ
- [x] ��������ť������ʾ
- [x] δ����ʱ��ť����
- [x] �ѷ���ʱ��ť����

### ��ݼ�����
- [x] `Ctrl+R` ������ת
- [x] ״̬����ʾȷ����Ϣ
- [x] ������ʾ�Ի���

### �߽����
- [x] δѡ������ʱ����
- [x] ѡ��δ��������ʱ����
- [x] ѡ���ѷ�������ʱ����
- [x] ��η�ת��������

---

## ?? �޸��ļ��嵥

### 1. ViewModels/MainViewModel.cs
- ? ��� `ReverseCurve()` ����
- ? ��� `CanReverseCurve()` ����
- ? ʹ�� `[RelayCommand]` ����

### 2. MainWindow.xaml
- ? ��"����"�˵����"��ת���ߵ�˳��"�˵���
- ? �ڹ�������ӷ�ת��ť��?? ͼ�꣩

### 3. MainWindow.xaml.cs
- ? ��� `Ctrl+R` ��ݼ���

---

## ?? ����״̬

### ��������

| ���� | ��� |
|------|------|
| δѡ������ | ? ���� |
| ѡ��δ�������� | ? ���� |
| ѡ���ѷ������� | ? ���� |

### �����

```csharp
[RelayCommand(CanExecute = nameof(CanReverseCurve))]
private void ReverseCurve() { ... }

private bool CanReverseCurve() => 
    SelectedCurve != null && SelectedCurve.IsLofted;
```

---

## ?? ���������ܵļ���

### 1. ��������

**��ϵ**: �����ȷ������ܷ�ת

```
���� (Ctrl+L)
    ��
IsLofted = true
    ��
��ת��ť����
    ��
���Է�ת (Ctrl+R)
```

### 2. ���ɲ��蹦��

**��ϵ**: ��ת��Ӱ�����ɲ���

```
��������
    ��
��ת����ѡ��
    ��
���ɲ��� (Ctrl+G)
    ��
ʹ�õ�ǰ���߷�������
```

### 3. 3D ��ͼ

**��ϵ**: �Զ�ͬ������

```
��ת����
��
LoftedPoints ���Ա仯
��
PropertyChanged �¼�
    ��
RefreshCurveVisualization()
    ��
3D ��ͼ������Ⱦ
```

---

## ?? ����ĵ�

- [3D������Ⱦ�����ܽ�.md](3D������Ⱦ�����ܽ�.md) - ������Ⱦ����
- [������Ⱦʹ��ָ��.md](������Ⱦʹ��ָ��.md) - ��Ⱦʹ��˵��
- [REALTIME_REFRESH_GUIDE.md](REALTIME_REFRESH_GUIDE.md) - ʵʱˢ�»���

---

## ?? �ܽ�

### ��������
- ? USV ���ߵ�˳��ת
- ? �˵���͹�������ť
- ? `Ctrl+R` ��ݼ�
- ? �Զ����� 3D ��ͼ

### ����״̬
? **����ɹ�**  
? **��������**  
? **UI ����**

### �û�����
- ?? ֱ�۵� ?? ͼ��
- ?? ��ݵ� `Ctrl+R` ��ݼ�
- ?? ʵʱ�� 3D ��ͼ����
- ?? ������״̬��ʾ

---

**�������������ɵط�ת USV ���ߵĵ�˳�򣬵���·������** ??
