using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Path.Models;
using Path.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Path.ViewModels
{
    /// <summary>
    /// 主视图模型
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
  private readonly IPathDataService _pathDataService;
  private readonly IDialogService _dialogService;
   private readonly IStepImportService _stepImportService;
  private readonly ILoftService _loftService;
        private readonly ICurveMergeService _curveMergeService;

        [ObservableProperty]
 private string _statusMessage = "准备就绪";

     [ObservableProperty]
 private StepModel? _selectedStep;

    private PathCurveModel? _selectedCurve;
     public PathCurveModel? SelectedCurve
     {
 get => _selectedCurve;
  set
  {
   if (SetProperty(ref _selectedCurve, value))
    {
 if (value != null)
  {
 StatusMessage = $"已选择曲线: {value.Name}";
 }
 // 触发命令的 CanExecute 重新评估
  LoftSelectedCurveCommand.NotifyCanExecuteChanged();
ReverseCurveCommand.NotifyCanExecuteChanged();
  RenameCurveCommand.NotifyCanExecuteChanged();
     RemoveCurveCommand.NotifyCanExecuteChanged();
      }
    }
  }

        [ObservableProperty]
   private bool _isBusy;

      [ObservableProperty]
private int _defaultLoftPointCount = 10;

        [ObservableProperty]
        private double _mergeTolerance = 0.01;

      /// <summary>
        /// 步骤列表
     /// </summary>
      public ObservableCollection<StepModel> Steps { get; } = new();

  /// <summary>
        /// 导入的曲线列表
 /// </summary>
      public ObservableCollection<PathCurveModel> Curves { get; } = new();

    // ? 新增：撤销栈
        private readonly Stack<(string action, PathCurveModel curve)> _undoStack = new();

public MainViewModel(
  IPathDataService pathDataService, 
IDialogService dialogService,
      IStepImportService stepImportService,
   ILoftService loftService,
ICurveMergeService curveMergeService)
      {
  _pathDataService = pathDataService;
    _dialogService = dialogService;
        _stepImportService = stepImportService;
     _loftService = loftService;
    _curveMergeService = curveMergeService;
     }

     /// <summary>
     /// 无参构造函数（设计时用）
 /// </summary>
   public MainViewModel() : this(
 new XmlPathDataService(), 
   new WpfDialogService(),
new StepImportService(),
    new LoftService(),
     new CurveMergeService())
    {
   }

partial void OnSelectedStepChanged(StepModel? value)
     {
  StatusMessage = value == null ? "未选择" : $"已选择: {value.DisplayName}";
     }

      // ? 监听 DefaultLoftPointCount 变化，更新所有曲线
      partial void OnDefaultLoftPointCountChanged(int value)
        {
     foreach (var curve in Curves)
            {
       // ? 修复：更新所有曲线的 LoftPointCount，不管是否已放样
          curve.LoftPointCount = value;
    }
       StatusMessage = $"放样点数已设置为: {value}";
        }

      #region STEP 文件导入

 [RelayCommand]
    private void ImportStepFile()
     {
   try
 {
      IsBusy = true;
   var filePath = _dialogService.ShowOpenFileDialog(
         "STEP 文件 (*.step;*.stp)|*.step;*.stp|所有文件 (*.*)|*.*");

     if (filePath == null)
 return;

   StatusMessage = "正在解析 STEP 文件...";

 var curveCollections = _stepImportService.ImportStepFile(filePath);

 // 为每条曲线创建模型
 var curveIndex = Curves.Count + 1;
     foreach (var points in curveCollections)
      {
       var curve = new PathCurveModel
   {
Name = $"usv_{curveIndex:00}",
  OriginalPoints = points,
                // ? 使用用户设置的放样点数
LoftPointCount = DefaultLoftPointCount
    };
       Curves.Add(curve);
     curveIndex++;
 }

       StatusMessage = $"成功导入 {curveCollections.Count} 条曲线";
    _dialogService.ShowMessage(
       $"? 成功导入 {curveCollections.Count} 条曲线\n\n" +
     $"文件: {System.IO.Path.GetFileName(filePath)}");
     }
    catch (InvalidOperationException ex) when (ex.Message.Contains("未从 STEP 文件中提取到曲线"))
    {
        // STEP 文件有效但没有曲线数据
      StatusMessage = "STEP 文件中没有曲线数据";
      
  // 提供诊断选项
     var result = System.Windows.MessageBox.Show(
           ex.Message + "\n\n是否生成诊断报告来分析文件内容？",
   "未找到曲线",
      System.Windows.MessageBoxButton.YesNo,
      System.Windows.MessageBoxImage.Warning);
       
    if (result == System.Windows.MessageBoxResult.Yes)
{
       try
         {
    var stepFilePath = _dialogService.ShowOpenFileDialog(
   "选择要诊断的 STEP 文件 (*.step;*.stp)|*.step;*.stp");
     
 if (stepFilePath != null)
     {
  var reportPath = System.IO.Path.ChangeExtension(stepFilePath, ".diagnostic.txt");
    Services.StepFileDiagnostics.SaveDiagnosticReport(stepFilePath, reportPath);
        
    _dialogService.ShowMessage(
     $"诊断报告已保存到:\n{reportPath}\n\n请查看报告了解详细信息。");
        
 // 打开报告文件
      System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
 {
  FileName = reportPath,
    UseShellExecute = true
   });
}
          }
        catch (Exception diagEx)
    {
        _dialogService.ShowError($"生成诊断报告失败: {diagEx.Message}");
   }
     }
   }
 catch (Exception ex)
  {
     _dialogService.ShowError($"导入失败: {ex.Message}");
      StatusMessage = "导入失败";
  }
 finally
  {
           IsBusy = false;
    }
  }

 #endregion

        #region 曲线管理

 [RelayCommand(CanExecute = nameof(CanRenameCurve))]
        private void RenameCurve()
        {
  if (SelectedCurve == null)
     return;

  // TODO: 显示重命名对话框
        // 这里简单实现：弹出输入框
        var newName = Microsoft.VisualBasic.Interaction.InputBox(
     $"请输入新名称（当前：{SelectedCurve.Name}）",
    "重命名曲线",
      SelectedCurve.Name);

      if (!string.IsNullOrWhiteSpace(newName))
 {
    SelectedCurve.Name = newName;
       StatusMessage = $"曲线已重命名为: {newName}";
      }
  }

      private bool CanRenameCurve() => SelectedCurve != null;

     [RelayCommand]
      private void AutoNameCurves()
    {
for (int i = 0; i < Curves.Count; i++)
    {
Curves[i].Name = $"usv_{i + 1:00}";
    }
 StatusMessage = $"已自动命名 {Curves.Count} 条曲线";
}

  [RelayCommand]
    private void MergeConnectedCurves()
   {
 if (Curves.Count == 0)
   {
 _dialogService.ShowMessage("没有可合并的曲线");
  return;
        }

       try
     {
     IsBusy = true;
 StatusMessage = "正在检测可合并的曲线...";

     // 检测可合并的组
  var curvePoints = Curves.Select(c => c.OriginalPoints).ToList();
  var mergeableGroups = _curveMergeService.DetectMergeableGroups(curvePoints, MergeTolerance);

      if (mergeableGroups.Count == 0)
   {
   _dialogService.ShowMessage(
         "未检测到可以合并的相连曲线。\n\n" +
  $"当前容差: {MergeTolerance:F4}\n\n" +
    "提示：如果曲线端点距离超过容差，将无法自动合并。");
StatusMessage = "未检测到可合并的曲线";
    return;
      }

        // 显示合并预览
       var groupsInfo = string.Join("\n", mergeableGroups.Select((g, i) =>
  $"  组 {i + 1}: {string.Join(", ", g.Select(idx => Curves[idx].Name))} ({g.Count} 条)"));
      
   var message = $"检测到 {mergeableGroups.Count} 组可合并的曲线:\n\n" +
        groupsInfo +
   $"\n\n容差: {MergeTolerance:F4}\n\n" +
    "是否执行合并？\n" +
       "(原始曲线将被删除)";

 if (!_dialogService.ShowConfirmation(message))
       {
  StatusMessage = "已取消合并";
   return;
 }

       // 执行合并
     StatusMessage = "正在合并曲线...";
  var mergedCurves = _curveMergeService.MergeConnectedCurves(curvePoints, MergeTolerance, false);

      // 更新曲线列表
     Curves.Clear();
   int index = 1;
   foreach (var points in mergedCurves)
  {
    var curve = new PathCurveModel
      {
    Name = $"merged_{index:00}",
OriginalPoints = points,
     LoftPointCount = DefaultLoftPointCount
  };
         Curves.Add(curve);
   index++;
    }

     SelectedCurve = Curves.FirstOrDefault();
      StatusMessage = $"合并完成！原 {curvePoints.Count} 条曲线 → 现 {mergedCurves.Count} 条曲线";
               
 _dialogService.ShowMessage(
      $"? 合并成功！\n\n" +
              $"原始曲线数: {curvePoints.Count}\n" +
 $"合并后曲线数: {mergedCurves.Count}\n" +
   $"合并了 {curvePoints.Count - mergedCurves.Count} 条曲线");
  }
  catch (Exception ex)
     {
   _dialogService.ShowError($"合并失败: {ex.Message}");
StatusMessage = "合并失败";
  }
   finally
     {
     IsBusy = false;
    }
 }

 [RelayCommand(CanExecute = nameof(CanRemoveCurve))]
   private void RemoveCurve()
        {
     if (SelectedCurve == null)
   return;

  if (_dialogService.ShowConfirmation($"确定要删除曲线 {SelectedCurve.Name} 吗？"))
 {
     // ? 保存到撤销栈
                _undoStack.Push(("删除曲线", SelectedCurve));
           
  Curves.Remove(SelectedCurve);
   SelectedCurve = null;
 StatusMessage = "曲线已删除 (Ctrl+Z 可撤销)";
     
      // 通知撤销命令状态更新
    UndoCommand.NotifyCanExecuteChanged();
   }
      }

private bool CanRemoveCurve() => SelectedCurve != null;

      [RelayCommand]
     private void ClearAllCurves()
      {
    if (Curves.Count == 0)
return;

    if (_dialogService.ShowConfirmation($"确定要清除所有 {Curves.Count} 条曲线吗？\n\n这将同时清除所有步骤和 USV 数据。"))
  {
// 清除曲线
Curves.Clear();
      SelectedCurve = null;
    
        // 清除步骤
         Steps.Clear();
  SelectedStep = null;
    
                // ? 清除撤销栈
    _undoStack.Clear();
        UndoCommand.NotifyCanExecuteChanged();
   
       StatusMessage = "已清除所有曲线和步骤";
        }
     }

        // ? 新增：撤销命令
        [RelayCommand(CanExecute = nameof(CanUndo))]
        private void Undo()
        {
 if (_undoStack.Count == 0)
            return;

            var (action, curve) = _undoStack.Pop();
   
      if (action == "删除曲线")
            {
  // 恢复删除的曲线
        Curves.Add(curve);
    SelectedCurve = curve;
       StatusMessage = $"已撤销删除曲线: {curve.Name}";
}
            
      UndoCommand.NotifyCanExecuteChanged();
  }

    private bool CanUndo() => _undoStack.Count > 0;

      #endregion

  #region 放样功能

 [RelayCommand(CanExecute = nameof(CanLoftCurve))]
 private void LoftSelectedCurve()
  {
   if (SelectedCurve == null)
 return;

      try
       {
  IsBusy = true;
   
     // ? 新增：如果已放样，提示用户
   if (SelectedCurve.IsLofted)
   {
          var result = _dialogService.ShowConfirmation(
        $"曲线 {SelectedCurve.Name} 已经放样过。\n\n" +
       $"是否重新放样？\n\n" +
            $"当前放样点数: {SelectedCurve.LoftPointCount}");
      
         if (!result)
    {
           StatusMessage = "已取消重新放样";
     return;
  }
 }
         
   // ? 使用曲线自己的 LoftPointCount 设置
SelectedCurve.LoftedPoints = _loftService.LoftCurve(
   SelectedCurve.OriginalPoints,
   SelectedCurve.LoftPointCount);
    
   SelectedCurve.IsLofted = true;
 StatusMessage = $"曲线 {SelectedCurve.Name} 已放样，生成 {SelectedCurve.LoftPointCount} 个点";
    }
   catch (Exception ex)
      {
   _dialogService.ShowError($"放样失败: {ex.Message}");
     StatusMessage = "放样失败";
   }
 finally
  {
  IsBusy = false;
       }
   }

    private bool CanLoftCurve() => SelectedCurve != null;

  [RelayCommand]
  private void LoftAllCurves()
 {
   if (Curves.Count == 0)
     {
    _dialogService.ShowMessage("没有可放样的曲线");
 return;
   }

  try
 {
     IsBusy = true;
     
   // ? 新增：检查是否有已放样的曲线
       var alreadyLoftedCurves = Curves.Where(c => c.IsLofted).ToList();
   bool reloftAll = false;
   
      if (alreadyLoftedCurves.Count > 0)
{
            var message = $"检测到 {alreadyLoftedCurves.Count} 条曲线已经放样过：\n\n";
    message += string.Join("\n", alreadyLoftedCurves.Take(5).Select(c => $"  ? {c.Name}"));
     if (alreadyLoftedCurves.Count > 5)
       {
       message += $"\n  ... 还有 {alreadyLoftedCurves.Count - 5} 条";
     }
 message += "\n\n是否重新放样这些曲线？\n\n";
message += "选择：\n";
   message += "? 是 - 重新放样所有曲线（包括已放样的）\n";
message += "? 否 - 只放样未放样的曲线";
    
     var result = _dialogService.ShowConfirmation(message);
      reloftAll = result;
  
         // ? 如果用户选择"否"且所有曲线都已放样，直接返回
  if (!reloftAll && Curves.All(c => c.IsLofted))
         {
    StatusMessage = "所有曲线均已放样";
    return;
         }
         }
 
        int loftedCount = 0;

     foreach (var curve in Curves)
           {
     // ? 根据用户选择决定是否跳过已放样的曲线
  if (!curve.IsLofted || reloftAll)
 {
     try
       {
     // ? 使用每条曲线自己的 LoftPointCount 设置
   curve.LoftedPoints = _loftService.LoftCurve(
     curve.OriginalPoints,
curve.LoftPointCount);
      curve.IsLofted = true;
  loftedCount++;
        }
   catch (Exception ex)
         {
        StatusMessage = $"放样曲线 {curve.Name} 失败: {ex.Message}";
       System.Diagnostics.Debug.WriteLine($"放样曲线 {curve.Name} 失败: {ex.Message}");
        // 继续处理其他曲线
}
 }
  }

         var statusMsg = reloftAll && alreadyLoftedCurves.Count > 0
      ? $"已重新放样 {loftedCount} 条曲线"
    : $"已放样 {loftedCount} 条曲线";
       
    StatusMessage = statusMsg;
      
    // ? 只有实际放样了曲线才显示成功消息
   if (loftedCount > 0)
        {
  _dialogService.ShowMessage($"成功{(reloftAll && alreadyLoftedCurves.Count > 0 ? "重新" : "")}放样 {loftedCount} 条曲线");
 }
      else
   {
_dialogService.ShowMessage("未放样任何曲线");
 }
      }
   catch (Exception ex)
  {
   _dialogService.ShowError($"批量放样失败: {ex.Message}");
    StatusMessage = "批量放样失败";
    }
 finally
   {
IsBusy = false;
  }
}

 /// <summary>
        /// 反转选中曲线的点顺序
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanReverseCurve))]
   private void ReverseCurve()
   {
    if (SelectedCurve == null)
 return;

try
      {
  IsBusy = true;

            // ? 修复：支持反转原始点和放样点
            if (SelectedCurve.IsLofted)
       {
             // 反转放样点列表
 var reversedPoints = new Point3DCollection(SelectedCurve.LoftedPoints.Reverse());
  SelectedCurve.LoftedPoints = reversedPoints;
                StatusMessage = $"曲线 {SelectedCurve.Name} 的放样点顺序已反转";
       }
            else
            {
        // 反转原始点列表
              var reversedPoints = new Point3DCollection(SelectedCurve.OriginalPoints.Reverse());
          SelectedCurve.OriginalPoints = reversedPoints;
 StatusMessage = $"曲线 {SelectedCurve.Name} 的原始点顺序已反转";
  }

  _dialogService.ShowMessage($"曲线 {SelectedCurve.Name} 的点顺序已反转");
  }
  catch (Exception ex)
 {
   _dialogService.ShowError($"反转失败: {ex.Message}");
   StatusMessage = "反转失败";
    }
     finally
      {
  IsBusy = false;
 }
    }

 // ? 修复：只要有选中的曲线就可以反转
 private bool CanReverseCurve() => SelectedCurve != null;

        #endregion

   #region 生成步骤

[RelayCommand]
        private void GenerateStepsFromCurves()
        {
    var loftedCurves = Curves.Where(c => c.IsLofted).ToList();

     if (loftedCurves.Count == 0)
      {
     _dialogService.ShowMessage("请先对曲线进行放样");
 return;
    }

    if (!_dialogService.ShowConfirmation(
   $"将从 {loftedCurves.Count} 条已放样曲线生成步骤，是否继续？"))
            {
     return;
  }

         try
 {
   IsBusy = true;
    Steps.Clear();

   // 找出所有曲线中的最大点数
         var maxPoints = loftedCurves.Max(c => c.LoftedPoints.Count);

      // 为每个点索引创建一个步骤
  for (int i = 0; i < maxPoints; i++)
   {
     var step = new StepModel
  {
    Number = i + 1,
     DisplayName = $"Step {i + 1}"
  };

  // 为每条曲线在该索引处创建 USV
    foreach (var curve in loftedCurves)
   {
      if (i < curve.LoftedPoints.Count)
 {
       var point = curve.LoftedPoints[i];
          
    // 计算 Yaw 角度（基于切线方向）
      var distance = i * _loftService.CalculateCurveLength(curve.OriginalPoints) / (curve.LoftPointCount - 1);
  var tangent = ((LoftService)_loftService).GetTangentAtDistance(curve.OriginalPoints, distance);
   var yaw = Math.Atan2(tangent.Y, tangent.X) * 180 / Math.PI;

var usv = new UsvModel
  {
  Id = curve.Name,
   X = point.X,
   Y = point.Y,
      Z = point.Z,
Yaw = yaw,
    Speed = 0.5 // 默认速度
   };
    step.Usvs.Add(usv);
      }
      }

        Steps.Add(step);
  }

      SelectedStep = Steps.FirstOrDefault();
       StatusMessage = $"已生成 {Steps.Count} 个步骤";
     _dialogService.ShowMessage($"成功生成 {Steps.Count} 个步骤");
  }
    catch (Exception ex)
     {
    _dialogService.ShowError($"生成步骤失败: {ex.Message}");
      StatusMessage = "生成步骤失败";
    }
  finally
         {
     IsBusy = false;
}
        }

      #endregion

        #region XML 导入导出

        [RelayCommand]
      private void ImportXml()
        {
  try
          {
     IsBusy = true;
   var filePath = _dialogService.ShowOpenFileDialog(
      "XML 文件 (*.xml)|*.xml|所有文件 (*.*)|*.*");

      if (filePath == null)
 return;

         var importedSteps = _pathDataService.ImportFromXml(filePath);

    Steps.Clear();
      foreach (var step in importedSteps)
  {
 Steps.Add(step);
   }

  SelectedStep = Steps.FirstOrDefault();
      StatusMessage = $"已导入: {System.IO.Path.GetFileName(filePath)}";
    _dialogService.ShowMessage($"成功导入 {Steps.Count} 个步骤");
  }
   catch (Exception ex)
{
    _dialogService.ShowError($"导入失败: {ex.Message}");
   StatusMessage = "导入失败";
      }
finally
 {
     IsBusy = false;
  }
}

     [RelayCommand]
      private void ExportXml()
     {
 try
            {
      IsBusy = true;

 if (!_pathDataService.ValidateData(Steps, out var errorMessage))
     {
       _dialogService.ShowError($"数据验证失败: {errorMessage}");
  return;
    }

var filePath = _dialogService.ShowSaveFileDialog(
        "XML 文件 (*.xml)|*.xml|所有文件 (*.*)|*.*",
  defaultFileName: "cluster.xml");

      if (filePath == null)
    return;

    _pathDataService.ExportToXml(filePath, Steps);
   StatusMessage = $"已导出: {System.IO.Path.GetFileName(filePath)}";
       _dialogService.ShowMessage($"成功导出到: {filePath}");
   }
       catch (Exception ex)
    {
     _dialogService.ShowError($"导出失败: {ex.Message}");
       StatusMessage = "导出失败";
 }
     finally
   {
     IsBusy = false;
         }
  }

   #endregion

#region 步骤管理（保留原有功能）

     [RelayCommand]
  private void AddStep()
     {
        var newStep = new StepModel
{
Number = Steps.Count + 1,
    DisplayName = $"Step {Steps.Count + 1}"
    };
  Steps.Add(newStep);
       SelectedStep = newStep;
     StatusMessage = $"已添加 {newStep.DisplayName}";
    }

 [RelayCommand(CanExecute = nameof(CanRemoveStep))]
        private void RemoveStep()
        {
     if (SelectedStep == null)
    return;

    if (!_dialogService.ShowConfirmation($"确定要删除 {SelectedStep.DisplayName} 吗？"))
    return;

         var index = Steps.IndexOf(SelectedStep);
  Steps.Remove(SelectedStep);

      if (Steps.Count == 0)
      SelectedStep = null;
      else
   SelectedStep = Steps[Math.Max(0, index - 1)];

       StatusMessage = "已删除步骤";
        }

    private bool CanRemoveStep() => SelectedStep != null;

     [RelayCommand(CanExecute = nameof(CanMoveStepUp))]
    private void MoveStepUp()
     {
     if (SelectedStep == null)
  return;

   var index = Steps.IndexOf(SelectedStep);
  Steps.Move(index, index - 1);
  StatusMessage = "步骤已移动";
     }

        private bool CanMoveStepUp() => SelectedStep != null && Steps.IndexOf(SelectedStep) > 0;

  [RelayCommand(CanExecute = nameof(CanMoveStepDown))]
    private void MoveStepDown()
      {
       if (SelectedStep == null)
   return;

 var index = Steps.IndexOf(SelectedStep);
 Steps.Move(index, index + 1);
    StatusMessage = "步骤已移动";
     }

  private bool CanMoveStepDown() => SelectedStep != null && Steps.IndexOf(SelectedStep) < Steps.Count - 1;

   [RelayCommand(CanExecute = nameof(CanAddUsv))]
        private void AddUsv()
    {
        if (SelectedStep == null)
return;

          var id = $"usv_{SelectedStep.Usvs.Count + 1:00}";
          var newUsv = new UsvModel { Id = id, X = 0, Y = 0, Z = 0, Yaw = 0, Speed = 0 };
     SelectedStep.Usvs.Add(newUsv);
 StatusMessage = $"已添加 {id}";
        }

   private bool CanAddUsv() => SelectedStep != null;

        [RelayCommand(CanExecute = nameof(CanRemoveUsv))]
        private void RemoveUsv(UsvModel? usv)
        {
  if (SelectedStep == null || usv == null)
      return;

  SelectedStep.Usvs.Remove(usv);
  StatusMessage = $"已删除 {usv.Id}";
  }

 private bool CanRemoveUsv(UsvModel? usv) => usv != null && SelectedStep != null;

        #endregion
    }
}
