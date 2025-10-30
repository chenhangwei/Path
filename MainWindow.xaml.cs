using Path.ViewModels;
using Path.Views;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Path
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
          InitializeComponent();
    _viewModel = viewModel;
     DataContext = _viewModel;

// 注册键盘快捷键
        RegisterKeyBindings();
     
        // 初始化3D编辑器
Loaded += OnWindowLoaded;
   
 // 监听数据更改以实时刷新 3D 视图
     _viewModel.PropertyChanged += ViewModel_PropertyChanged;
   
         // 监听 DataGrid 的变化
   this.Loaded += (s, e) =>
  {
      var usvGrid = FindName("UsvGrid") as DataGrid;
        if (usvGrid != null)
       {
     // 监听单元格编辑结束事件
    usvGrid.CellEditEnding += UsvGrid_CellEditEnding;
  }
   };
   }

   // ViewModel 属性改变处理
  private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
 {
   // 当选中的步骤改变时，刷新 3D 视图并高亮
   if (e.PropertyName == nameof(MainViewModel.SelectedStep))
  {
   RefreshPathEditor3D();
      
  // 高亮显示选中的步骤
 try
       {
 var pathEditor3D = FindName("PathEditor3DControl") as PathEditor3D;
    if (pathEditor3D != null && _viewModel.SelectedStep != null)
  {
   var usvIds = _viewModel.SelectedStep.Usvs.Select(u => u.Id).ToList();
  var usvPositions = _viewModel.SelectedStep.Usvs.ToDictionary(
  u => u.Id,
 u => new Point3D(u.X, u.Y, u.Z));
          
  pathEditor3D.HighlightStep(_viewModel.SelectedStep.Number, usvIds, usvPositions);
     }
  else if (pathEditor3D != null)
 {
   pathEditor3D.ClearStepHighlight();
     }
 }
    catch { }
}
    
   // 当曲线集合改变或选中曲线改变时，刷新曲线显示
   if (e.PropertyName == nameof(MainViewModel.Curves) ||
   e.PropertyName == nameof(MainViewModel.SelectedCurve))
  {
    RefreshCurveVisualization();
}
        
   // 当步骤集合改变时（生成步骤），刷新3D视图
   if (e.PropertyName == nameof(MainViewModel.Steps))
{
       RefreshPathEditor3D();
 var editor3D = FindName("PathEditor3DControl") as PathEditor3D;
  if (editor3D != null && _viewModel.Steps.Count > 0)
  {
// 生成步骤后，隐藏曲线点的标签
       editor3D.HideCurveLabels();
 _viewModel.StatusMessage = "已生成步骤，曲线标签已隐藏";
      }
   }
      
      // 当状态繁忙改变时，可能有数据更新
      if (e.PropertyName == nameof(MainViewModel.IsBusy))
      {
          if (!_viewModel.IsBusy)
{
           // 操作完成后刷新3D视图
        RefreshCurveVisualization();
          RefreshPathEditor3D();
        }
      }
  }

   // USV 数据编辑结束时实时刷新
        private void UsvGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
 {
 // 使用 Dispatcher 延迟刷新，确保数据已经更新
    Dispatcher.BeginInvoke(new Action(() =>
        {
   RefreshPathEditor3D();
   _viewModel.StatusMessage = "USV 数据已更新";
   }), System.Windows.Threading.DispatcherPriority.Background);
   }

   // 刷新 3D 编辑器的辅助方法
  private void RefreshPathEditor3D()
 {
   try
 {
  var pathEditor3D = FindName("PathEditor3DControl") as PathEditor3D;
if (pathEditor3D == null) return;
    
    // 如果步骤集合为空，清除所有步骤高亮和标签
    if (_viewModel.Steps.Count == 0)
   {
    pathEditor3D.ClearStepHighlight();
    return;
      }
      
      // ✅ 注意：这里不需要渲染所有放样点
    // 因为高亮功能会根据需要在 HighlightStep 中创建高亮球体
      // RefreshUsvData 方法已经不存在了
    
     // 如果有选中的步骤，高亮显示
        if (_viewModel.SelectedStep != null)
        {
  var usvIds = _viewModel.SelectedStep.Usvs.Select(u => u.Id).ToList();
            var usvPositions = _viewModel.SelectedStep.Usvs.ToDictionary(
    u => u.Id,
         u => new Point3D(u.X, u.Y, u.Z));
            
      pathEditor3D.HighlightStep(_viewModel.SelectedStep.Number, usvIds, usvPositions);
        }
   }
 catch { }
   }

   private void OnWindowLoaded(object sender, RoutedEventArgs e)
  {
 // 初始化PathEditor3D的捕捉功能
  try
{
     var editor3D = FindName("PathEditor3DControl") as PathEditor3D;
   if (editor3D != null)
   {
   // 启用智能捕捉
 editor3D.SetSnapOptions(
     gridSnap: true,
pointSnap: true,
      midpointSnap: true,
  endpointSnap: true,
     projectionSnap: true,
          snapDistance: 15.0
  );

    // 设置回调
editor3D.OnPlaneClicked = (point) =>
    {
    // 处理平面点击事件
  _viewModel.StatusMessage = $"点击坐标: ({point.X:F2}, {point.Y:F2}, {point.Z:F2})";
     };
        
     // 初始化设置值
  var pointSizeSlider = FindName("PointSizeSlider") as Slider;
      var autoScaleCheckBox = FindName("AutoScaleCheckBox") as CheckBox;
   var gridSpacingSlider = FindName("GridSpacingSlider") as Slider;
   
      if (pointSizeSlider != null)
     pointSizeSlider.Value = editor3D.PointSizeScale;
  if (autoScaleCheckBox != null)
    autoScaleCheckBox.IsChecked = editor3D.AutoScalePoints;
    if (gridSpacingSlider != null)
   gridSpacingSlider.Value = editor3D.GridSpacing;
      
    // 监听曲线集合的变化
     _viewModel.Curves.CollectionChanged += (s, e) => 
         {
      RefreshCurveVisualization();
_viewModel.StatusMessage = "曲线集合已更新";
         };
                
     // 监听每个曲线的属性变化（例如 IsLofted）
   foreach (var curve in _viewModel.Curves)
        {
            curve.PropertyChanged += Curve_PropertyChanged;
          }
          
       // 监听步骤集合的变化
          _viewModel.Steps.CollectionChanged += (s, e) =>
          {
   RefreshPathEditor3D();
          _viewModel.StatusMessage = "步骤集合已更新";
        };
   
       // 监听每个步骤的 USV 集合变化
          foreach (var step in _viewModel.Steps)
       {
 step.Usvs.CollectionChanged += (s, e) =>
        {
        RefreshPathEditor3D();
        _viewModel.StatusMessage = "USV 数据已更新";
   };
      }
 }
      }
catch { }
  }
 
      // 曲线属性变化处理
      private void Curve_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
       if (e.PropertyName == nameof(Models.PathCurveModel.IsLofted) ||
        e.PropertyName == nameof(Models.PathCurveModel.LoftedPoints) ||
           e.PropertyName == nameof(Models.PathCurveModel.IsSelected) ||
     e.PropertyName == nameof(Models.PathCurveModel.Name))
          {
        RefreshCurveVisualization();
              _viewModel.StatusMessage = $"曲线已更新: {e.PropertyName}";
      }
  }

        // 控制点大小滑块改变事件
   private void PointSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
   {
var editor3D = FindName("PathEditor3DControl") as PathEditor3D;
      if (editor3D != null)
  {
  editor3D.PointSizeScale = e.NewValue;
     _viewModel.StatusMessage = $"控制点大小: {e.NewValue:F1}x";
      }
        }

        // 自动缩放复选框改变事件
  private void AutoScaleCheckBox_Changed(object sender, RoutedEventArgs e)
  {
        var editor3D = FindName("PathEditor3DControl") as PathEditor3D;
            var checkBox = sender as CheckBox;
   if (editor3D != null && checkBox != null)
    {
       editor3D.AutoScalePoints = checkBox.IsChecked == true;
    _viewModel.StatusMessage = checkBox.IsChecked == true 
       ? "已启用自动缩放" 
   : "已禁用自动缩放";
   }
        }

        // 网格间距滑块改变事件
  private void GridSpacingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
   {
     var editor3D = FindName("PathEditor3DControl") as PathEditor3D;
      if (editor3D != null)
      {
    editor3D.GridSpacing = e.NewValue;
    _viewModel.StatusMessage = $"网格间距: {e.NewValue:F1}";
  }
        }

     private void RegisterKeyBindings()
    {
 // Ctrl+O: 导入
        CommandBindings.Add(new System.Windows.Input.CommandBinding(
      System.Windows.Input.ApplicationCommands.Open,
    (s, e) => _viewModel.ImportXmlCommand.Execute(null)));

   // Ctrl+S: 导出
    CommandBindings.Add(new System.Windows.Input.CommandBinding(
    System.Windows.Input.ApplicationCommands.Save,
   (s, e) => _viewModel.ExportXmlCommand.Execute(null)));

          // Ctrl+N: 添加步骤
            CommandBindings.Add(new System.Windows.Input.CommandBinding(
   System.Windows.Input.ApplicationCommands.New,
    (s, e) => _viewModel.AddStepCommand.Execute(null)));

    // Delete: 删除步骤
 CommandBindings.Add(new System.Windows.Input.CommandBinding(
   System.Windows.Input.ApplicationCommands.Delete,
     (s, e) => _viewModel.RemoveStepCommand.Execute(null)));

        // ✅ Ctrl+Z: 撤销
        CommandBindings.Add(new System.Windows.Input.CommandBinding(
            System.Windows.Input.ApplicationCommands.Undo,
       (s, e) => _viewModel.UndoCommand.Execute(null)));

       // F9: 切换网格捕捉
    InputBindings.Add(new System.Windows.Input.KeyBinding(
    new RelayCommand(() => ToggleGridSnap()),
     System.Windows.Input.Key.F9,
     System.Windows.Input.ModifierKeys.None));

     // F10: 切换点捕捉
   InputBindings.Add(new System.Windows.Input.KeyBinding(
  new RelayCommand(() => TogglePointSnap()),
             System.Windows.Input.Key.F10,
      System.Windows.Input.ModifierKeys.None));

 // F11: 切换中点捕捉
       InputBindings.Add(new System.Windows.Input.KeyBinding(
   new RelayCommand(() => ToggleMidpointSnap()),
  System.Windows.Input.Key.F11,
   System.Windows.Input.ModifierKeys.None));

   // 绑定输入手势
  InputBindings.Add(new System.Windows.Input.KeyBinding(
      System.Windows.Input.ApplicationCommands.Open,
     System.Windows.Input.Key.O,
   System.Windows.Input.ModifierKeys.Control));

     InputBindings.Add(new System.Windows.Input.KeyBinding(
System.Windows.Input.ApplicationCommands.Save,
     System.Windows.Input.Key.S,
     System.Windows.Input.ModifierKeys.Control));

       InputBindings.Add(new System.Windows.Input.KeyBinding(
      System.Windows.Input.ApplicationCommands.New,
    System.Windows.Input.Key.N,
       System.Windows.Input.ModifierKeys.Control));

       InputBindings.Add(new System.Windows.Input.KeyBinding(
     System.Windows.Input.ApplicationCommands.Delete,
  System.Windows.Input.Key.Delete,
  System.Windows.Input.ModifierKeys.None));

     // ✅ Ctrl+Z 撤销绑定
    InputBindings.Add(new System.Windows.Input.KeyBinding(
          System.Windows.Input.ApplicationCommands.Undo,
System.Windows.Input.Key.Z,
            System.Windows.Input.ModifierKeys.Control));
       
    // Ctrl+R: 反转曲线
 InputBindings.Add(new System.Windows.Input.KeyBinding(
         new RelayCommand(() => _viewModel.ReverseCurveCommand.Execute(null)),
   System.Windows.Input.Key.R,
     System.Windows.Input.ModifierKeys.Control));
  }

        // 捕捉快捷键处理
        private void ToggleGridSnap()
        {
            var editor3D = FindName("PathEditor3DControl") as PathEditor3D;
    if (editor3D != null)
      {
     editor3D.ToggleGridSnap();
     _viewModel.StatusMessage = "网格捕捉已切换";
     }
        }

        private void TogglePointSnap()
        {
         var editor3D = FindName("PathEditor3DControl") as PathEditor3D;
   if (editor3D != null)
            {
       editor3D.TogglePointSnap();
     _viewModel.StatusMessage = "点捕捉已切换";
    }
        }

        private void ToggleMidpointSnap()
        {
 var editor3D = FindName("PathEditor3DControl") as PathEditor3D;
       if (editor3D != null)
       {
         editor3D.ToggleMidpointSnap();
_viewModel.StatusMessage = "中点捕捉已切换";
       }
        }

        // TreeView选中项更改的事件处理程序
        private void OnStepSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
         // 检查选中的是 StepModel 还是 UsvModel
  if (e.NewValue is Models.StepModel step)
{
      _viewModel.SelectedStep = step;
  }
  else if (e.NewValue is Models.UsvModel usv)
    {
  // 如果选中的是 USV，找到它所属的 Step
    foreach (var s in _viewModel.Steps)
  {
   if (s.Usvs.Contains(usv))
    {
 _viewModel.SelectedStep = s;
break;
 }
   }
 }

 // 高亮显示选中的步骤
   try
   {
 var pathEditor3D = FindName("PathEditor3DControl") as PathEditor3D;
 if (pathEditor3D != null && _viewModel.SelectedStep != null)
 {
   var usvIds = _viewModel.SelectedStep.Usvs.Select(u => u.Id).ToList();
     var usvPositions = _viewModel.SelectedStep.Usvs.ToDictionary(
     u => u.Id,
  u => new Point3D(u.X, u.Y, u.Z));
          
   pathEditor3D.HighlightStep(_viewModel.SelectedStep.Number, usvIds, usvPositions);
  }
        }
   catch { }
    }

        // 转换 StepModel 到 PathEditor3D.UsvDto
        private List<PathEditor3D.UsvDto> ConvertToUsvDtos(IEnumerable<Models.StepModel> steps)
        {
    var result = new List<PathEditor3D.UsvDto>();
  
     // 按USV ID分组，每个USV作为一个路径
   var usvGroups = new Dictionary<string, List<Point3D>>();
         
    foreach (var step in steps.OrderBy(s => s.Number))
{
     foreach (var usv in step.Usvs)
{
        if (!usvGroups.ContainsKey(usv.Id))
        {
       usvGroups[usv.Id] = new List<Point3D>();
   }
  
  usvGroups[usv.Id].Add(new Point3D(usv.X, usv.Y, usv.Z));
 }
  }

       // 转换为UsvDto
        int index = 0;
   foreach (var kvp in usvGroups)
      {
      var segments = new List<PathEditor3D.SegmentDto>
    {
            new PathEditor3D.SegmentDto(0, kvp.Value, IsArc: false)
  };

         result.Add(new PathEditor3D.UsvDto(
        Index: index++,
    Segments: segments,
   Color: GetUsvColor(index),
       Name: kvp.Key
 ));
         }

      return result;
        }

        // 获取USV颜色
   private System.Windows.Media.Color GetUsvColor(int index)
        {
     var colors = new[]
            {
            System.Windows.Media.Colors.SteelBlue,
   System.Windows.Media.Colors.Orange,
  System.Windows.Media.Colors.Green,
          System.Windows.Media.Colors.Purple,
            System.Windows.Media.Colors.Brown,
                System.Windows.Media.Colors.Teal,
       System.Windows.Media.Colors.Magenta,
        System.Windows.Media.Colors.Gold
    };

    return colors[index % colors.Length];
   }

        private void OnExit(object sender, RoutedEventArgs e)
    {
          Application.Current.Shutdown();
        }

private void OnAbout(object sender, RoutedEventArgs e)
   {
   MessageBox.Show(
    "3D 路径编辑器 v2.0\n\n" +
    "用于 USV 路径规划的 3D 可视化编辑工具\n\n" +
    "技术栈:\n" +
    "- .NET 8 / WPF\n" +
    "- HelixToolkit.Wpf 2.22.0\n" +
    "- CommunityToolkit.Mvvm\n\n" +
  "快捷键:\n" +
    "- F9: 网格捕捉\n" +
    "- F10: 点捕捉\n" +
    "- F11: 中点捕捉\n\n" +
 "© 2024",
     "关于",
     MessageBoxButton.OK,
     MessageBoxImage.Information);
        }
        
        // 刷新曲线可视化
        private void RefreshCurveVisualization()
   {
  try
  {
    var editor3D = FindName("PathEditor3DControl") as PathEditor3D;
    if (editor3D == null) return;
  
  // 如果曲线集合为空，清除所有曲线显示
  if (_viewModel.Curves.Count == 0)
     {
      editor3D.ClearImportedCurves();
       return;
    }
       
       // 先取消之前选中的曲线高亮
    foreach (var curve in _viewModel.Curves)
{
      if (!curve.IsSelected)
       {
     editor3D.UnhighlightCurve(curve);
   }
   }
     
    // 渲染所有曲线
     editor3D.RenderImportedCurves(_viewModel.Curves);
        
       // 如果有选中的曲线，自适应视图
   if (_viewModel.Curves.Count > 0)
 {
  editor3D.FitCurvesToView();
         }
 }
   catch { }
        }

     // 曲线列表选择改变事件
        private void CurveList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Models.PathCurveModel selectedCurve)
            {
       // 取消所有曲线的选中状态
       foreach (var curve in _viewModel.Curves)
{
            curve.IsSelected = false;
       }
      
     // 设置当前选中的曲线
             selectedCurve.IsSelected = true;
      _viewModel.SelectedCurve = selectedCurve;
         
           // 刷新 3D 视图以更新高亮
            RefreshCurveVisualization();
     
           _viewModel.StatusMessage = $"已选择曲线: {selectedCurve.Name}";
          }
      }
    }

    // RelayCommand helper class
    public class RelayCommand : System.Windows.Input.ICommand
    {
    private readonly System.Action _execute;
        private readonly System.Func<bool>? _canExecute;

        public RelayCommand(System.Action execute, System.Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new System.ArgumentNullException(nameof(execute));
       _canExecute = canExecute;
        }

        public event System.EventHandler? CanExecuteChanged
        {
     add { System.Windows.Input.CommandManager.RequerySuggested += value; }
            remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
    }
}