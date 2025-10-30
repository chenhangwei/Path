# USV ����ʵʱˢ�ºͿ��Ƶ��С����ʵ��ָ��

## ����

��ָ��˵������� 3D ·���༭����ʵ�֣�
1. **USV ����ʵʱˢ��**���������¼�������������
2. **���Ƶ��С����Ӧ**�������������򴰿ڴ�С��
3. **�û��ɵ��ڿ��Ƶ��С**��ͨ�����飩

## һ��PathEditor3D.xaml.cs ��Ҫ��ӵĴ���

### 1. ��ӿ��Ƶ��С���ԣ������ֶ�����

```csharp
// ���Ƶ��С����
private double _pointSizeScale = 1.0; // ���Ƶ��С�������� (0.5 �� 3.0)
private bool _autoScalePoints = true; // �Ƿ��Զ�������ͼ�������ſ��Ƶ�

public double PointSizeScale 
{ 
    get => _pointSizeScale; 
    set 
    { 
        _pointSizeScale = Math.Clamp(value, 0.5, 3.0);
        RefreshPointSizes(); // ��С�ı�ʱˢ�����п��Ƶ�
    } 
}

public bool AutoScalePoints 
{ 
    get => _autoScalePoints; 
    set 
    { 
        _autoScalePoints = value;
        RefreshPointSizes();
    } 
}
```

### 2. ���ʵʱˢ�·���

```csharp
// ʵʱˢ�� USV ���ݣ�����ճ�����ֻ���£�
public void RefreshUsvData(List<UsvDto> usvs)
{
    // ���浱ǰ���״̬
    var savedCamera = HelixView.Camera;

    // ֻ������Ƶ���߶Σ����������ƽ��
    foreach (var pv in _pointVisuals) SceneRoot.Children.Remove(pv);
    foreach (var lv in _lineVisuals) SceneRoot.Children.Remove(lv);
    foreach (var sv in _sampleVisuals) SceneRoot.Children.Remove(sv);

    _pointVisuals.Clear();
    _lineVisuals.Clear();
    _sampleVisuals.Clear();
    _modelMap.Clear();
    _indexToSphere.Clear();
    _indexToLabel.Clear();
    _logicalPositions.Clear();

    _segToLines.Clear(); 
    _segOriginalAppearance.Clear();
    _lastUsvs = usvs;

    // ������Ⱦ����
 RenderUsvData(usvs);

    // �ָ����״̬
    HelixView.Camera = savedCamera;
}
```

### 3. ��ȡ��Ⱦ�߼�Ϊ��������

�� `LoadFrom2D` �е�USV��Ⱦ������ȡ�� `RenderUsvData` �����С������е�ѭ���У��޸Ŀ��Ƶ㴴�����֣�

```csharp
// �ڴ������Ƶ�ʱʹ������Ӧ��С
for (int pi = 0; pi < seg.ControlPoints.Count; pi++)
{
    var p = seg.ControlPoints[pi];
    var visualCenter = new Point3D(
     p.X * _visualScale, 
        (_invertY ? -1.0 : 1.0) * p.Y * _visualScale, 
      p.Z * _visualScale
    );

    // ��������Ӧ��С
    var radius = CalculatePointRadius(visualCenter, pi == 0);

    var sphere = new SphereVisual3D
    {
        Center = visualCenter,
    Radius = radius,
        Fill = new SolidColorBrush(u.Color)
    };
    // ... �������
}
```

### 4. ��Ӽ�����Ƶ�뾶�ķ���

```csharp
// ������Ƶ�뾶��֧������Ӧ���ֶ����ţ�
private double CalculatePointRadius(Point3D position, bool isFirstPoint)
{
    double baseRadius = (isFirstPoint ? 5 : 4) * _visualScale;

    if (_autoScalePoints && HelixView.Camera is PerspectiveCamera cam)
    {
  // ������������Զ�����
    var distance = (cam.Position - position).Length;
        var scaleFactor = Math.Max(0.5, Math.Min(2.0, distance / 100.0));
        baseRadius *= scaleFactor;
    }

    // Ӧ���û����õ���������
    return baseRadius * _pointSizeScale;
}
```

### 5. ���ˢ�����п��Ƶ��С�ķ���

```csharp
// ˢ�����п��Ƶ��С
private void RefreshPointSizes()
{
    foreach (var kvp in _indexToSphere)
    {
        var key = kvp.Key;
   var sphere = kvp.Value;

    if (_logicalPositions.TryGetValue(key, out var logical))
        {
            var visualCenter = new Point3D(
  logical.X * _visualScale, 
          (_invertY ? -1.0 : 1.0) * logical.Y * _visualScale, 
       logical.Z * _visualScale
          );

     // �ж��Ƿ��ǵ�һ����
      bool isFirstPoint = (key.pi == 0);
            sphere.Radius = CalculatePointRadius(visualCenter, isFirstPoint);
      }
    }

    // ˢ�²������С�������Ҫ��
    // ...
}
```

### 6. ������¼��д���ˢ��

�������¼�������ĩβ���ˢ�µ��ã�

```csharp
// �� HelixView_PreviewMouseMove �У���ת��ƽ�ƺ�
if (_autoScalePoints) RefreshPointSizes();

// �� HelixView_MouseWheel �У����ź�
if (_autoScalePoints) RefreshPointSizes();
```

## ����MainWindow.xaml ����������

�� 3D �༭������������ӣ�

```xaml
<Border Grid.Column="4" BorderBrush="#CCCCCC" BorderThickness="1" CornerRadius="4" Background="White">
    <DockPanel>
        <!-- 3D ��ͼ������� -->
        <Expander DockPanel.Dock="Top" Header="?? 3D ��ͼ����" IsExpanded="False" 
          Background="#F5F5F5" BorderBrush="#CCCCCC" BorderThickness="0,0,0,1">
       <StackPanel Margin="8" Background="White">
      <!-- ���Ƶ��С���� -->
       <TextBlock Text="���Ƶ��С" FontWeight="SemiBold" Margin="0,0,0,4"/>
      <Grid Margin="0,0,0,8">
       <Grid.ColumnDefinitions>
     <ColumnDefinition Width="Auto"/>
      <ColumnDefinition Width="*"/>
 <ColumnDefinition Width="Auto"/>
              </Grid.ColumnDefinitions>
        <TextBlock Grid.Column="0" Text="0.5x" VerticalAlignment="Center" Margin="0,0,8,0"/>
   <Slider Grid.Column="1" x:Name="PointSizeSlider" 
    Minimum="0.5" Maximum="3.0" Value="1.0" 
      TickFrequency="0.1" IsSnapToTickEnabled="True"
         VerticalAlignment="Center"
  ValueChanged="PointSizeSlider_ValueChanged"/>
<TextBlock Grid.Column="2" 
             Text="{Binding Value, ElementName=PointSizeSlider, StringFormat={}{0:F1}x}" 
                VerticalAlignment="Center" Margin="8,0,0,0" FontWeight="Bold" MinWidth="40"/>
                </Grid>

      <!-- �Զ�����ѡ�� -->
 <CheckBox x:Name="AutoScaleCheckBox" Content="�Զ�������ͼ��������" 
    IsChecked="True" Margin="0,0,0,8"
         Checked="AutoScaleCheckBox_Changed" 
   Unchecked="AutoScaleCheckBox_Changed"/>

<!-- ��ݼ���ʾ -->
   <Border Background="#E3F2FD" BorderBrush="#2196F3" BorderThickness="1" 
         CornerRadius="4" Padding="8" Margin="0,8,0,0">
       <StackPanel>
  <TextBlock Text="?? ��ݼ�" FontWeight="Bold" Foreground="#1976D2" Margin="0,0,0,4"/>
    <TextBlock TextWrapping="Wrap" FontSize="11">
        <Run Text="F"/> <Run Text=" - ��Ӧ����" Foreground="#666666"/><LineBreak/>
        <Run Text="Ctrl+1/2/3/4"/> <Run Text=" - �л���ͼ" Foreground="#666666"/><LineBreak/>
       <Run Text="Home"/> <Run Text=" - ������ͼ" Foreground="#666666"/>
                </TextBlock>
           </StackPanel>
  </Border>
            </StackPanel>
     </Expander>

    <!-- 3D �༭�� -->
        <views:PathEditor3D x:Name="PathEditor3DControl"/>
    </DockPanel>
</Border>
```

##����MainWindow.xaml.cs ����¼�����

### 1. ����¼�������

```csharp
// ���Ƶ��С����ı��¼�
private void PointSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
{
 var editor3D = FindName("PathEditor3DControl") as PathEditor3D;
    if (editor3D != null)
    {
        editor3D.PointSizeScale = e.NewValue;
    _viewModel.StatusMessage = $"���Ƶ��С: {e.NewValue:F1}x";
    }
}

// �Զ����Ÿ�ѡ��ı��¼�
private void AutoScaleCheckBox_Changed(object sender, RoutedEventArgs e)
{
    var editor3D = FindName("PathEditor3DControl") as PathEditor3D;
 var checkBox = sender as CheckBox;
    if (editor3D != null && checkBox != null)
    {
editor3D.AutoScalePoints = checkBox.IsChecked == true;
  _viewModel.StatusMessage = checkBox.IsChecked == true 
        ? "�������Զ�����" 
     : "�ѽ����Զ�����";
    }
}
```

### 2. ������ݱ仯����

```csharp
public MainWindow(MainViewModel viewModel)
{
  InitializeComponent();
    _viewModel = viewModel;
    DataContext = _viewModel;

    // �������ݸ�����ʵʱˢ�� 3D ��ͼ
    _viewModel.PropertyChanged += ViewModel_PropertyChanged;

    // ���� DataGrid �ı仯
    this.Loaded += (s, e) =>
    {
   var usvGrid = FindName("UsvGrid") as DataGrid;
  if (usvGrid != null)
{
            // ������Ԫ��༭�����¼�
     usvGrid.CellEditEnding += UsvGrid_CellEditEnding;
        }
    };
}

// ViewModel ���Ըı䴦��
private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
{
    if (e.PropertyName == nameof(MainViewModel.SelectedStep))
    {
        RefreshPathEditor3D();
    }
}

// USV ���ݱ༭����ʱʵʱˢ��
private void UsvGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
{
    // ʹ�� Dispatcher �ӳ�ˢ�£�ȷ�������Ѿ�����
    Dispatcher.BeginInvoke(new Action(() =>
    {
        RefreshPathEditor3D();
    _viewModel.StatusMessage = "USV �����Ѹ���";
    }), System.Windows.Threading.DispatcherPriority.Background);
}

// ˢ�� 3D �༭���ĸ�������
private void RefreshPathEditor3D()
{
    try
    {
     var pathEditor3D = FindName("PathEditor3DControl") as PathEditor3D;
      if (pathEditor3D != null && _viewModel.Steps.Count > 0)
        {
         var usvDtos = ConvertToUsvDtos(_viewModel.Steps);
            pathEditor3D.RefreshUsvData(usvDtos); // ʹ��ʵʱˢ��
        }
    }
 catch { }
}
```

### 3. ��ӱ���� using ����

```csharp
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
```

## �ġ�ʹ�÷���

### 1. ʵʱˢ��
- �� USV ��������������/�޸����ݺ�3D ��ͼ���Զ�ˢ��
- ˢ�²���ı䵱ǰ�����λ�úͽǶ�
- ˢ��ֻ���¿��Ƶ���߶Σ����񱣳ֲ���

### 2. �������Ƶ��С
- **�ֶ�����**���϶�"���Ƶ��С"���飨0.5x �� 3.0x��
- **�Զ�����**����ѡ"�Զ�������ͼ��������"
- ���������ʱ�����Ƶ���С
  - �����Զ��ʱ�����Ƶ����
  - ���ֿ��Ƶ�����Ļ�ϵ��Ӿ���С��Ժ㶨

### 3. ���ʹ��
- ����ͬʱ�����Զ����ź��ֶ�����
- �ֶ�����ϵ����Ӧ�����Զ�����Ĵ�С֮��
- ���磺����2.0x�����п��Ƶ����Ĭ�ϴ�С��2��

## �塢ע������

1. **�����Ż�**��
   - `RefreshUsvData` ֻ���±�Ҫ���Ӿ�Ԫ��
   - ������ÿ������ƶ�ʱ������ `RefreshPointSizes`
   - �������ֹͣ�ƶ������Ž���ʱˢ��

2. **�̰߳�ȫ**��
   - DataGrid �ı༭�¼�ʹ�� `Dispatcher.BeginInvoke` �ӳ�ִ��
 - ȷ���������ύ����ˢ�� 3D ��ͼ

3. **�û�����**��
   - �ṩ��ʱ������״̬����Ϣ��
   - �������״̬����
 - ��ݼ���ʾ�����ɼ�

## ������չ����

���Խ�һ����ӣ�
1. **����ʽ**����ͬ���͵Ŀ��Ƶ�ʹ�ò�ͬ��״/��ɫ
2. **��ǩ��С**������Ƶ�һ������
3. **�־û�����**�������û��Ĵ�Сƫ��
4. **����ģʽ**�������ݼ�ʱ�����Զ�����

## �ߡ����Լ���嵥

- [ ] �޸� USV ���ݺ� 3D ��ͼ��������
- [ ] ���λ�úͽǶȱ��ֲ���
- [ ] �����������������Ƶ��С
- [ ] �Զ����Ź�����������
- [ ] ״̬����ʾ�����Ϣ
- [ ] û����������򿨶�
- [ ] �����ݼ���>100 ���㣩��������

---

**�汾**: 1.0  
**����**: 2024-12-22  
**����**: GitHub Copilot
