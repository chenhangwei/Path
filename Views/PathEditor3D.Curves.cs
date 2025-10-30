using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Path.Models;

namespace Path.Views
{
  /// <summary>
  /// PathEditor3D ��������Ⱦ��չ
    /// </summary>
    public partial class PathEditor3D
    {
    private readonly List<LinesVisual3D> _curveLineVisuals = new();
        private readonly List<SphereVisual3D> _curvePointVisuals = new();
      private readonly Dictionary<PathCurveModel, (LinesVisual3D line, List<SphereVisual3D> points)> _curveVisuals = new();
      // ������ߵ��ģ��ӳ�䣬���ڵ�����
        private readonly Dictionary<GeometryModel3D, (PathCurveModel curve, int pointIndex)> _curvePointModelMap = new();
      // ������ߵ�ı�ǩ
private readonly Dictionary<(PathCurveModel curve, int pointIndex), BillboardTextVisual3D> _curvePointLabels = new();
        
     /// <summary>
/// ��Ⱦ����������б�
        /// </summary>
    /// <param name="curves">����ģ���б�</param>
        public void RenderImportedCurves(IEnumerable<PathCurveModel> curves)
  {
 // ���֮ǰ�����߿��ӻ�Ԫ��
            ClearImportedCurves();
 
        foreach (var curve in curves)
         {
     RenderSingleCurve(curve);
            }
  }
        
 /// <summary>
        /// ��Ⱦ��������
  /// </summary>
      /// <param name="curve">����ģ��</param>
     public void RenderSingleCurve(PathCurveModel curve)
        {
       if (curve == null) return;
  
      // ����Ѿ���Ⱦ�������Ƴ�
 if (_curveVisuals.ContainsKey(curve))
  {
 RemoveCurve(curve);
   }
       
   // ����ʹ�ø�ϸ�Ĵ�ϸ���������������������߶�
   var lineVisual = new LinesVisual3D { Thickness = 1.5, Color = Colors.Blue };
    var pointVisuals = new List<SphereVisual3D>();
   
    // ѡ��Ҫ��Ⱦ�ĵ㼯������ѷ������÷����㣬������ԭʼ��
    var pointsToRender = curve.IsLofted && curve.LoftedPoints.Count > 0 
? curve.LoftedPoints 
       : curve.OriginalPoints;
  
  if (pointsToRender.Count == 0) return;
       
  // ��Ⱦ����
      for (int i = 0; i < pointsToRender.Count - 1; i++)
{
         var p1 = LogicalToVisual(pointsToRender[i]);
    var p2 = LogicalToVisual(pointsToRender[i + 1]);
       lineVisual.Points.Add(p1);
   lineVisual.Points.Add(p2);
    }
     
   SceneRoot.Children.Add(lineVisual);
      _curveLineVisuals.Add(lineVisual);
   
   // ��Ⱦ���Ƶ� - ʹ�ø�С�ĵ�
        for (int i = 0; i < pointsToRender.Count; i++)
            {
var point = pointsToRender[i];
   var visualCenter = LogicalToVisual(point);
  // ʹ��ר�ŵ����ߵ�뾶���㷽��
  var radius = CalculateCurvePointRadius(visualCenter, curve.IsSelected);
  
      var sphere = new SphereVisual3D
           {
      Center = visualCenter,
     Radius = radius,
      Fill = curve.IsSelected 
         ? new SolidColorBrush(Colors.Orange) 
         : new SolidColorBrush(Colors.Blue)
  };
     
    SceneRoot.Children.Add(sphere);
   pointVisuals.Add(sphere);
        _curvePointVisuals.Add(sphere);
   
     // ��ӵ�����ӳ��
  if (sphere.Content is GeometryModel3D sphereModel)
       {
     _curvePointModelMap[sphereModel] = (curve, i);
    }
       
 // ��ӱ�ǩ - �޸���ǩλ�ã�ʹ�ý�С��ƫ����
    var labelOffset = new Vector3D(
        0.3 * _visualScale,  // X ƫ�ƣ�ԼΪ��뾶�� 2 ��
  0.3 * _visualScale,  // Y ƫ��
            0.4 * _visualScale   // Z ƫ�ƣ��Ը�һ��
        );
    var labelPos = new Point3D(
    visualCenter.X + labelOffset.X,
     visualCenter.Y + labelOffset.Y,
       visualCenter.Z + labelOffset.Z
     );
      
      var label = new BillboardTextVisual3D
     {
   Text = (i + 1).ToString(),
      Position = labelPos,
       Foreground = Brushes.Black,
       Background = Brushes.White,
     FontSize = 10
            };
      SceneRoot.Children.Add(label);
     _curvePointLabels[(curve, i)] = label;
      }
    
  // ��������
_curveVisuals[curve] = (lineVisual, pointVisuals);
  
     // �����ѡ�е����ߣ�������ʾ
  if (curve.IsSelected)
       {
      HighlightCurve(curve);
  }
        }
 
        /// <summary>
      /// �������ߵ�İ뾶���ȿ��Ƶ�С�öࣩ
      /// </summary>
        private double CalculateCurvePointRadius(Point3D position, bool isSelected)
        {
   // �����뾶���Դ��������ߣ������ߴ�ϸΪ 1-2��
     double baseRadius = 0.15 * _visualScale; // ԼΪ�����ߵ� 1.5 ����ϸ
            
       if (_autoScalePoints && HelixView.Camera is PerspectiveCamera cam)
 {
        // ������������Զ�����
        var distance = (cam.Position - position).Length;
  // ����ԽԶ����Խ�󣨱����Ӿ���Сһ�£�
      var scaleFactor = System.Math.Max(0.3, System.Math.Min(3.0, distance / 50.0));
     baseRadius *= scaleFactor;
 }
       
   // ѡ��ʱ��΢��һ��
       if (isSelected)
  {
    baseRadius *= 1.3;
      }
 
     // Ӧ���û����õ���������
       return baseRadius * _pointSizeScale;
    }
        
   /// <summary>
   /// �������ߵĿ��ӻ������������
      /// </summary>
        /// <param name="curve">����ģ��</param>
      public void UpdateCurve(PathCurveModel curve)
        {
  if (curve == null) return;
       
 // ������Ⱦ����
         RenderSingleCurve(curve);
        }
  
        /// <summary>
 /// �Ƴ��������ߵĿ��ӻ�
     /// </summary>
      /// <param name="curve">����ģ��</param>
      public void RemoveCurve(PathCurveModel curve)
  {
   if (!_curveVisuals.TryGetValue(curve, out var visuals)) return;
   
    // �Ƴ�����
      SceneRoot.Children.Remove(visuals.line);
 _curveLineVisuals.Remove(visuals.line);
   
   // �Ƴ���
   foreach (var point in visuals.points)
      {
    SceneRoot.Children.Remove(point);
  _curvePointVisuals.Remove(point);

         // �Ƴ�������ӳ��
       if (point.Content is GeometryModel3D model && _curvePointModelMap.ContainsKey(model))
       {
      _curvePointModelMap.Remove(model);
       }
     }
   
 // �Ƴ���ǩ
   var labelsToRemove = _curvePointLabels.Where(kv => kv.Key.curve == curve).ToList();
        foreach (var kvp in labelsToRemove)
    {
    SceneRoot.Children.Remove(kvp.Value);
_curvePointLabels.Remove(kvp.Key);
        }
     
   _curveVisuals.Remove(curve);
    }
  
        /// <summary>
   /// ������ʾ���ߣ�ѡ��״̬��
      /// </summary>
        /// <param name="curve">����ģ��</param>
    public void HighlightCurve(PathCurveModel curve)
        {
        if (!_curveVisuals.TryGetValue(curve, out var visuals)) return;
            
      // �������� - ��΢��һ��
    visuals.line.Color = Colors.Orange;
     visuals.line.Thickness = 2.5;
     
     // ����� - ���¼���뾶��ѡ��״̬��
       foreach (var point in visuals.points)
 {
    point.Fill = new SolidColorBrush(Colors.Orange);
  // ʹ��ѡ��״̬�İ뾶
point.Radius = CalculateCurvePointRadius(point.Center, true);
 }
     }
        
        /// <summary>
      /// ȡ���������ָ�����״̬��
   /// </summary>
        /// <param name="curve">����ģ��</param>
     public void UnhighlightCurve(PathCurveModel curve)
     {
 if (!_curveVisuals.TryGetValue(curve, out var visuals)) return;
            
   // �ָ�����
 visuals.line.Color = Colors.Blue;
    visuals.line.Thickness = 1.5;
  
   // �ָ��� - ʹ��δѡ��״̬�İ뾶
            foreach (var point in visuals.points)
    {
                point.Fill = new SolidColorBrush(Colors.Blue);
      point.Radius = CalculateCurvePointRadius(point.Center, false);
          }
        }
        
        /// <summary>
 /// ������е������ߵĿ��ӻ�
    /// </summary>
        public void ClearImportedCurves()
        {
  foreach (var lineVisual in _curveLineVisuals)
    {
      SceneRoot.Children.Remove(lineVisual);
    }
_curveLineVisuals.Clear();
     
      foreach (var pointVisual in _curvePointVisuals)
    {
    SceneRoot.Children.Remove(pointVisual);
}
     _curvePointVisuals.Clear();
 
   // �����ǩ
        foreach (var label in _curvePointLabels.Values)
  {
  SceneRoot.Children.Remove(label);
   }
   _curvePointLabels.Clear();
 
     // ���������ӳ��
 _curvePointModelMap.Clear();
     _curveVisuals.Clear();
    }

     /// <summary>
        /// �����������ߵ�ı�ǩ�����ɲ��������ǩ�ظ���
        /// </summary>
 public void HideCurveLabels()
     {
   foreach (var label in _curvePointLabels.Values)
      {
     SceneRoot.Children.Remove(label);
      }
          _curvePointLabels.Clear();
        }

     /// <summary>
        /// ��ʾ���ߵ�ı�ǩ
        /// </summary>
        public void ShowCurveLabels()
        {
            // ������Ⱦ���������Իָ���ǩ
            var curvesToRender = _curveVisuals.Keys.ToList();
   foreach (var curve in curvesToRender)
            {
     RenderSingleCurve(curve);
            }
        }
        
        /// <summary>
   /// ����Ӧ��ͼ����ʾ��������
      /// </summary>
        public void FitCurvesToView()
        {
    try
    {
   if (_curvePointVisuals.Count == 0) return;
 
       // �������е�ı߽��
    var minX = double.MaxValue;
         var minY = double.MaxValue;
    var minZ = double.MaxValue;
   var maxX = double.MinValue;
    var maxY = double.MinValue;
              var maxZ = double.MinValue;
  
  foreach (var point in _curvePointVisuals)
     {
   var center = point.Center;
      minX = System.Math.Min(minX, center.X);
minY = System.Math.Min(minY, center.Y);
        minZ = System.Math.Min(minZ, center.Z);
   maxX = System.Math.Max(maxX, center.X);
    maxY = System.Math.Max(maxY, center.Y);
        maxZ = System.Math.Max(maxZ, center.Z);
    }
  
    // �������ĵ�ͳߴ�
    var centerX = (minX + maxX) / 2;
var centerY = (minY + maxY) / 2;
        var centerZ = (minZ + maxZ) / 2;
           var sizeX = maxX - minX;
  var sizeY = maxY - minY;
       var sizeZ = maxZ - minZ;
         var maxSize = System.Math.Max(System.Math.Max(sizeX, sizeY), sizeZ);
 
       // �������λ��
    if (HelixView.Camera is PerspectiveCamera cam && maxSize > 0)
        {
    var distance = maxSize * 2.5;
 var center = new Point3D(centerX, centerY, centerZ);
    
       cam.Position = new Point3D(
     center.X + distance * 0.7,
            center.Y + distance * 0.7,
      center.Z + distance * 0.7
    );
     
          cam.LookDirection = new Vector3D(
        center.X - cam.Position.X,
       center.Y - cam.Position.Y,
          center.Z - cam.Position.Z
     );
       
    _cameraManuallyAdjusted = true;
  
   // �Զ�����ģʽ�¸��¿��Ƶ��С
        if (_autoScalePoints) RefreshPointSizes();
   }
      }
    catch { }
 }
        
   /// <summary>
        /// ˢ���������ߵ�Ĵ�С������������ͼ��
   /// </summary>
        public void RefreshCurvePointSizes()
   {
  foreach (var kvp in _curveVisuals)
 {
    var curve = kvp.Key;
       var visuals = kvp.Value;
   
    foreach (var point in visuals.points)
         {
      point.Radius = CalculateCurvePointRadius(point.Center, curve.IsSelected);
    }
}
        }
        
        // ������������������֮��� XY ƽ����루��������ڣ�
      private double DistanceXY(Point3D p1, Point3D p2)
        {
    var dx = p1.X - p2.X;
          var dy = p1.Y - p2.Y;
   return System.Math.Sqrt(dx * dx + dy * dy);
   }
   
        // ������������ȡ��������
   private Point3D GetSceneCenter()
  {
if (_curvePointVisuals.Count > 0)
     {
      double sumX = 0, sumY = 0, sumZ = 0;
      foreach (var point in _curvePointVisuals)
          {
   sumX += point.Center.X;
     sumY += point.Center.Y;
      sumZ += point.Center.Z;
}
     return new Point3D(
   sumX / _curvePointVisuals.Count,
     sumY / _curvePointVisuals.Count,
      sumZ / _curvePointVisuals.Count
);
}
 
   if (_pointVisuals.Count > 0)
{
    double sumX = 0, sumY = 0, sumZ = 0;
      foreach (var point in _pointVisuals)
           {
   sumX += point.Center.X;
      sumY += point.Center.Y;
         sumZ += point.Center.Z;
   }
      return new Point3D(
  sumX / _pointVisuals.Count,
      sumY / _pointVisuals.Count,
       sumZ / _pointVisuals.Count);
      }
     
   return new Point3D(0, 0, 0);
   }
  
    // ������������Բ�����в���
        private List<Point3D> SampleArcPointsForRendering(Point3D p1, Point3D p2, Point3D p3, double spacing)
     {
   var result = new List<Point3D> { p1 };

     // ��ʵ�֣����������߲�ֵ
   int segments = 10;
      for (int i = 1; i < segments; i++)
   {
       double t = (double)i / segments;
     double t1 = (1 - t) * (1 - t);
   double t2 = 2 * (1 - t) * t;
 double t3 = t * t;
     
      var x = t1 * p1.X + t2 * p2.X + t3 * p3.X;
     var y = t1 * p1.Y + t2 * p2.Y + t3 * p3.Y;
      var z = t1 * p1.Z + t2 * p2.Z + t3 * p3.Z;

          result.Add(new Point3D(x, y, z));
   }
    
     result.Add(p3);
   return result;
  }
      
        // �����¼�����
 private void UserControl_PreviewKeyDown(object? sender, System.Windows.Input.KeyEventArgs e)
        {
       // F �� - ����Ӧ��ͼ
      if (e.Key == System.Windows.Input.Key.F)
   {
 FitCurvesToView();
       e.Handled = true;
    }
        }
 }
}
