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
    /// ����ͼģ��
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
  private readonly IPathDataService _pathDataService;
  private readonly IDialogService _dialogService;
   private readonly IStepImportService _stepImportService;
  private readonly ILoftService _loftService;
        private readonly ICurveMergeService _curveMergeService;

        [ObservableProperty]
 private string _statusMessage = "׼������";

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
 StatusMessage = $"��ѡ������: {value.Name}";
 }
 // ��������� CanExecute ��������
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
        /// �����б�
     /// </summary>
      public ObservableCollection<StepModel> Steps { get; } = new();

  /// <summary>
        /// ����������б�
 /// </summary>
      public ObservableCollection<PathCurveModel> Curves { get; } = new();

    // ? ����������ջ
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
     /// �޲ι��캯�������ʱ�ã�
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
  StatusMessage = value == null ? "δѡ��" : $"��ѡ��: {value.DisplayName}";
     }

      // ? ���� DefaultLoftPointCount �仯��������������
      partial void OnDefaultLoftPointCountChanged(int value)
        {
     foreach (var curve in Curves)
            {
       // ? �޸��������������ߵ� LoftPointCount�������Ƿ��ѷ���
          curve.LoftPointCount = value;
    }
       StatusMessage = $"��������������Ϊ: {value}";
        }

      #region STEP �ļ�����

 [RelayCommand]
    private void ImportStepFile()
     {
   try
 {
      IsBusy = true;
   var filePath = _dialogService.ShowOpenFileDialog(
         "STEP �ļ� (*.step;*.stp)|*.step;*.stp|�����ļ� (*.*)|*.*");

     if (filePath == null)
 return;

   StatusMessage = "���ڽ��� STEP �ļ�...";

 var curveCollections = _stepImportService.ImportStepFile(filePath);

 // Ϊÿ�����ߴ���ģ��
 var curveIndex = Curves.Count + 1;
     foreach (var points in curveCollections)
      {
       var curve = new PathCurveModel
   {
Name = $"usv_{curveIndex:00}",
  OriginalPoints = points,
                // ? ʹ���û����õķ�������
LoftPointCount = DefaultLoftPointCount
    };
       Curves.Add(curve);
     curveIndex++;
 }

       StatusMessage = $"�ɹ����� {curveCollections.Count} ������";
    _dialogService.ShowMessage(
       $"? �ɹ����� {curveCollections.Count} ������\n\n" +
     $"�ļ�: {System.IO.Path.GetFileName(filePath)}");
     }
    catch (InvalidOperationException ex) when (ex.Message.Contains("δ�� STEP �ļ�����ȡ������"))
    {
        // STEP �ļ���Ч��û����������
      StatusMessage = "STEP �ļ���û����������";
      
  // �ṩ���ѡ��
     var result = System.Windows.MessageBox.Show(
           ex.Message + "\n\n�Ƿ�������ϱ����������ļ����ݣ�",
   "δ�ҵ�����",
      System.Windows.MessageBoxButton.YesNo,
      System.Windows.MessageBoxImage.Warning);
       
    if (result == System.Windows.MessageBoxResult.Yes)
{
       try
         {
    var stepFilePath = _dialogService.ShowOpenFileDialog(
   "ѡ��Ҫ��ϵ� STEP �ļ� (*.step;*.stp)|*.step;*.stp");
     
 if (stepFilePath != null)
     {
  var reportPath = System.IO.Path.ChangeExtension(stepFilePath, ".diagnostic.txt");
    Services.StepFileDiagnostics.SaveDiagnosticReport(stepFilePath, reportPath);
        
    _dialogService.ShowMessage(
     $"��ϱ����ѱ��浽:\n{reportPath}\n\n��鿴�����˽���ϸ��Ϣ��");
        
 // �򿪱����ļ�
      System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
 {
  FileName = reportPath,
    UseShellExecute = true
   });
}
          }
        catch (Exception diagEx)
    {
        _dialogService.ShowError($"������ϱ���ʧ��: {diagEx.Message}");
   }
     }
   }
 catch (Exception ex)
  {
     _dialogService.ShowError($"����ʧ��: {ex.Message}");
      StatusMessage = "����ʧ��";
  }
 finally
  {
           IsBusy = false;
    }
  }

 #endregion

        #region ���߹���

 [RelayCommand(CanExecute = nameof(CanRenameCurve))]
        private void RenameCurve()
        {
  if (SelectedCurve == null)
     return;

  // TODO: ��ʾ�������Ի���
        // �����ʵ�֣����������
        var newName = Microsoft.VisualBasic.Interaction.InputBox(
     $"�����������ƣ���ǰ��{SelectedCurve.Name}��",
    "����������",
      SelectedCurve.Name);

      if (!string.IsNullOrWhiteSpace(newName))
 {
    SelectedCurve.Name = newName;
       StatusMessage = $"������������Ϊ: {newName}";
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
 StatusMessage = $"���Զ����� {Curves.Count} ������";
}

  [RelayCommand]
    private void MergeConnectedCurves()
   {
 if (Curves.Count == 0)
   {
 _dialogService.ShowMessage("û�пɺϲ�������");
  return;
        }

       try
     {
     IsBusy = true;
 StatusMessage = "���ڼ��ɺϲ�������...";

     // ���ɺϲ�����
  var curvePoints = Curves.Select(c => c.OriginalPoints).ToList();
  var mergeableGroups = _curveMergeService.DetectMergeableGroups(curvePoints, MergeTolerance);

      if (mergeableGroups.Count == 0)
   {
   _dialogService.ShowMessage(
         "δ��⵽���Ժϲ����������ߡ�\n\n" +
  $"��ǰ�ݲ�: {MergeTolerance:F4}\n\n" +
    "��ʾ��������߶˵���볬���ݲ���޷��Զ��ϲ���");
StatusMessage = "δ��⵽�ɺϲ�������";
    return;
      }

        // ��ʾ�ϲ�Ԥ��
       var groupsInfo = string.Join("\n", mergeableGroups.Select((g, i) =>
  $"  �� {i + 1}: {string.Join(", ", g.Select(idx => Curves[idx].Name))} ({g.Count} ��)"));
      
   var message = $"��⵽ {mergeableGroups.Count} ��ɺϲ�������:\n\n" +
        groupsInfo +
   $"\n\n�ݲ�: {MergeTolerance:F4}\n\n" +
    "�Ƿ�ִ�кϲ���\n" +
       "(ԭʼ���߽���ɾ��)";

 if (!_dialogService.ShowConfirmation(message))
       {
  StatusMessage = "��ȡ���ϲ�";
   return;
 }

       // ִ�кϲ�
     StatusMessage = "���ںϲ�����...";
  var mergedCurves = _curveMergeService.MergeConnectedCurves(curvePoints, MergeTolerance, false);

      // ���������б�
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
      StatusMessage = $"�ϲ���ɣ�ԭ {curvePoints.Count} ������ �� �� {mergedCurves.Count} ������";
               
 _dialogService.ShowMessage(
      $"? �ϲ��ɹ���\n\n" +
              $"ԭʼ������: {curvePoints.Count}\n" +
 $"�ϲ���������: {mergedCurves.Count}\n" +
   $"�ϲ��� {curvePoints.Count - mergedCurves.Count} ������");
  }
  catch (Exception ex)
     {
   _dialogService.ShowError($"�ϲ�ʧ��: {ex.Message}");
StatusMessage = "�ϲ�ʧ��";
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

  if (_dialogService.ShowConfirmation($"ȷ��Ҫɾ������ {SelectedCurve.Name} ��"))
 {
     // ? ���浽����ջ
                _undoStack.Push(("ɾ������", SelectedCurve));
           
  Curves.Remove(SelectedCurve);
   SelectedCurve = null;
 StatusMessage = "������ɾ�� (Ctrl+Z �ɳ���)";
     
      // ֪ͨ��������״̬����
    UndoCommand.NotifyCanExecuteChanged();
   }
      }

private bool CanRemoveCurve() => SelectedCurve != null;

      [RelayCommand]
     private void ClearAllCurves()
      {
    if (Curves.Count == 0)
return;

    if (_dialogService.ShowConfirmation($"ȷ��Ҫ������� {Curves.Count} ��������\n\n�⽫ͬʱ������в���� USV ���ݡ�"))
  {
// �������
Curves.Clear();
      SelectedCurve = null;
    
        // �������
         Steps.Clear();
  SelectedStep = null;
    
                // ? �������ջ
    _undoStack.Clear();
        UndoCommand.NotifyCanExecuteChanged();
   
       StatusMessage = "������������ߺͲ���";
        }
     }

        // ? ��������������
        [RelayCommand(CanExecute = nameof(CanUndo))]
        private void Undo()
        {
 if (_undoStack.Count == 0)
            return;

            var (action, curve) = _undoStack.Pop();
   
      if (action == "ɾ������")
            {
  // �ָ�ɾ��������
        Curves.Add(curve);
    SelectedCurve = curve;
       StatusMessage = $"�ѳ���ɾ������: {curve.Name}";
}
            
      UndoCommand.NotifyCanExecuteChanged();
  }

    private bool CanUndo() => _undoStack.Count > 0;

      #endregion

  #region ��������

 [RelayCommand(CanExecute = nameof(CanLoftCurve))]
 private void LoftSelectedCurve()
  {
   if (SelectedCurve == null)
 return;

      try
       {
  IsBusy = true;
   
     // ? ����������ѷ�������ʾ�û�
   if (SelectedCurve.IsLofted)
   {
          var result = _dialogService.ShowConfirmation(
        $"���� {SelectedCurve.Name} �Ѿ���������\n\n" +
       $"�Ƿ����·�����\n\n" +
            $"��ǰ��������: {SelectedCurve.LoftPointCount}");
      
         if (!result)
    {
           StatusMessage = "��ȡ�����·���";
     return;
  }
 }
         
   // ? ʹ�������Լ��� LoftPointCount ����
SelectedCurve.LoftedPoints = _loftService.LoftCurve(
   SelectedCurve.OriginalPoints,
   SelectedCurve.LoftPointCount);
    
   SelectedCurve.IsLofted = true;
 StatusMessage = $"���� {SelectedCurve.Name} �ѷ��������� {SelectedCurve.LoftPointCount} ����";
    }
   catch (Exception ex)
      {
   _dialogService.ShowError($"����ʧ��: {ex.Message}");
     StatusMessage = "����ʧ��";
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
    _dialogService.ShowMessage("û�пɷ���������");
 return;
   }

  try
 {
     IsBusy = true;
     
   // ? ����������Ƿ����ѷ���������
       var alreadyLoftedCurves = Curves.Where(c => c.IsLofted).ToList();
   bool reloftAll = false;
   
      if (alreadyLoftedCurves.Count > 0)
{
            var message = $"��⵽ {alreadyLoftedCurves.Count} �������Ѿ���������\n\n";
    message += string.Join("\n", alreadyLoftedCurves.Take(5).Select(c => $"  ? {c.Name}"));
     if (alreadyLoftedCurves.Count > 5)
       {
       message += $"\n  ... ���� {alreadyLoftedCurves.Count - 5} ��";
     }
 message += "\n\n�Ƿ����·�����Щ���ߣ�\n\n";
message += "ѡ��\n";
   message += "? �� - ���·����������ߣ������ѷ����ģ�\n";
message += "? �� - ֻ����δ����������";
    
     var result = _dialogService.ShowConfirmation(message);
      reloftAll = result;
  
         // ? ����û�ѡ��"��"���������߶��ѷ�����ֱ�ӷ���
  if (!reloftAll && Curves.All(c => c.IsLofted))
         {
    StatusMessage = "�������߾��ѷ���";
    return;
         }
         }
 
        int loftedCount = 0;

     foreach (var curve in Curves)
           {
     // ? �����û�ѡ������Ƿ������ѷ���������
  if (!curve.IsLofted || reloftAll)
 {
     try
       {
     // ? ʹ��ÿ�������Լ��� LoftPointCount ����
   curve.LoftedPoints = _loftService.LoftCurve(
     curve.OriginalPoints,
curve.LoftPointCount);
      curve.IsLofted = true;
  loftedCount++;
        }
   catch (Exception ex)
         {
        StatusMessage = $"�������� {curve.Name} ʧ��: {ex.Message}";
       System.Diagnostics.Debug.WriteLine($"�������� {curve.Name} ʧ��: {ex.Message}");
        // ����������������
}
 }
  }

         var statusMsg = reloftAll && alreadyLoftedCurves.Count > 0
      ? $"�����·��� {loftedCount} ������"
    : $"�ѷ��� {loftedCount} ������";
       
    StatusMessage = statusMsg;
      
    // ? ֻ��ʵ�ʷ��������߲���ʾ�ɹ���Ϣ
   if (loftedCount > 0)
        {
  _dialogService.ShowMessage($"�ɹ�{(reloftAll && alreadyLoftedCurves.Count > 0 ? "����" : "")}���� {loftedCount} ������");
 }
      else
   {
_dialogService.ShowMessage("δ�����κ�����");
 }
      }
   catch (Exception ex)
  {
   _dialogService.ShowError($"��������ʧ��: {ex.Message}");
    StatusMessage = "��������ʧ��";
    }
 finally
   {
IsBusy = false;
  }
}

 /// <summary>
        /// ��תѡ�����ߵĵ�˳��
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanReverseCurve))]
   private void ReverseCurve()
   {
    if (SelectedCurve == null)
 return;

try
      {
  IsBusy = true;

            // ? �޸���֧�ַ�תԭʼ��ͷ�����
            if (SelectedCurve.IsLofted)
       {
             // ��ת�������б�
 var reversedPoints = new Point3DCollection(SelectedCurve.LoftedPoints.Reverse());
  SelectedCurve.LoftedPoints = reversedPoints;
                StatusMessage = $"���� {SelectedCurve.Name} �ķ�����˳���ѷ�ת";
       }
            else
            {
        // ��תԭʼ���б�
              var reversedPoints = new Point3DCollection(SelectedCurve.OriginalPoints.Reverse());
          SelectedCurve.OriginalPoints = reversedPoints;
 StatusMessage = $"���� {SelectedCurve.Name} ��ԭʼ��˳���ѷ�ת";
  }

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

 // ? �޸���ֻҪ��ѡ�е����߾Ϳ��Է�ת
 private bool CanReverseCurve() => SelectedCurve != null;

        #endregion

   #region ���ɲ���

[RelayCommand]
        private void GenerateStepsFromCurves()
        {
    var loftedCurves = Curves.Where(c => c.IsLofted).ToList();

     if (loftedCurves.Count == 0)
      {
     _dialogService.ShowMessage("���ȶ����߽��з���");
 return;
    }

    if (!_dialogService.ShowConfirmation(
   $"���� {loftedCurves.Count} ���ѷ����������ɲ��裬�Ƿ������"))
            {
     return;
  }

         try
 {
   IsBusy = true;
    Steps.Clear();

   // �ҳ����������е�������
         var maxPoints = loftedCurves.Max(c => c.LoftedPoints.Count);

      // Ϊÿ������������һ������
  for (int i = 0; i < maxPoints; i++)
   {
     var step = new StepModel
  {
    Number = i + 1,
     DisplayName = $"Step {i + 1}"
  };

  // Ϊÿ�������ڸ����������� USV
    foreach (var curve in loftedCurves)
   {
      if (i < curve.LoftedPoints.Count)
 {
       var point = curve.LoftedPoints[i];
          
    // ���� Yaw �Ƕȣ��������߷���
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
    Speed = 0.5 // Ĭ���ٶ�
   };
    step.Usvs.Add(usv);
      }
      }

        Steps.Add(step);
  }

      SelectedStep = Steps.FirstOrDefault();
       StatusMessage = $"������ {Steps.Count} ������";
     _dialogService.ShowMessage($"�ɹ����� {Steps.Count} ������");
  }
    catch (Exception ex)
     {
    _dialogService.ShowError($"���ɲ���ʧ��: {ex.Message}");
      StatusMessage = "���ɲ���ʧ��";
    }
  finally
         {
     IsBusy = false;
}
        }

      #endregion

        #region XML ���뵼��

        [RelayCommand]
      private void ImportXml()
        {
  try
          {
     IsBusy = true;
   var filePath = _dialogService.ShowOpenFileDialog(
      "XML �ļ� (*.xml)|*.xml|�����ļ� (*.*)|*.*");

      if (filePath == null)
 return;

         var importedSteps = _pathDataService.ImportFromXml(filePath);

    Steps.Clear();
      foreach (var step in importedSteps)
  {
 Steps.Add(step);
   }

  SelectedStep = Steps.FirstOrDefault();
      StatusMessage = $"�ѵ���: {System.IO.Path.GetFileName(filePath)}";
    _dialogService.ShowMessage($"�ɹ����� {Steps.Count} ������");
  }
   catch (Exception ex)
{
    _dialogService.ShowError($"����ʧ��: {ex.Message}");
   StatusMessage = "����ʧ��";
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
       _dialogService.ShowError($"������֤ʧ��: {errorMessage}");
  return;
    }

var filePath = _dialogService.ShowSaveFileDialog(
        "XML �ļ� (*.xml)|*.xml|�����ļ� (*.*)|*.*",
  defaultFileName: "cluster.xml");

      if (filePath == null)
    return;

    _pathDataService.ExportToXml(filePath, Steps);
   StatusMessage = $"�ѵ���: {System.IO.Path.GetFileName(filePath)}";
       _dialogService.ShowMessage($"�ɹ�������: {filePath}");
   }
       catch (Exception ex)
    {
     _dialogService.ShowError($"����ʧ��: {ex.Message}");
       StatusMessage = "����ʧ��";
 }
     finally
   {
     IsBusy = false;
         }
  }

   #endregion

#region �����������ԭ�й��ܣ�

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
     StatusMessage = $"����� {newStep.DisplayName}";
    }

 [RelayCommand(CanExecute = nameof(CanRemoveStep))]
        private void RemoveStep()
        {
     if (SelectedStep == null)
    return;

    if (!_dialogService.ShowConfirmation($"ȷ��Ҫɾ�� {SelectedStep.DisplayName} ��"))
    return;

         var index = Steps.IndexOf(SelectedStep);
  Steps.Remove(SelectedStep);

      if (Steps.Count == 0)
      SelectedStep = null;
      else
   SelectedStep = Steps[Math.Max(0, index - 1)];

       StatusMessage = "��ɾ������";
        }

    private bool CanRemoveStep() => SelectedStep != null;

     [RelayCommand(CanExecute = nameof(CanMoveStepUp))]
    private void MoveStepUp()
     {
     if (SelectedStep == null)
  return;

   var index = Steps.IndexOf(SelectedStep);
  Steps.Move(index, index - 1);
  StatusMessage = "�������ƶ�";
     }

        private bool CanMoveStepUp() => SelectedStep != null && Steps.IndexOf(SelectedStep) > 0;

  [RelayCommand(CanExecute = nameof(CanMoveStepDown))]
    private void MoveStepDown()
      {
       if (SelectedStep == null)
   return;

 var index = Steps.IndexOf(SelectedStep);
 Steps.Move(index, index + 1);
    StatusMessage = "�������ƶ�";
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
 StatusMessage = $"����� {id}";
        }

   private bool CanAddUsv() => SelectedStep != null;

        [RelayCommand(CanExecute = nameof(CanRemoveUsv))]
        private void RemoveUsv(UsvModel? usv)
        {
  if (SelectedStep == null || usv == null)
      return;

  SelectedStep.Usvs.Remove(usv);
  StatusMessage = $"��ɾ�� {usv.Id}";
  }

 private bool CanRemoveUsv(UsvModel? usv) => usv != null && SelectedStep != null;

        #endregion
    }
}
