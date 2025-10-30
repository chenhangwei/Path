using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Path.Models;

namespace Path.Views
{
  /// <summary>
  /// PathEditor3D 的曲线渲染扩展
    /// </summary>
    public partial class PathEditor3D
    {
    private readonly List<LinesVisual3D> _curveLineVisuals = new();
        private readonly List<SphereVisual3D> _curvePointVisuals = new();
      private readonly Dictionary<PathCurveModel, (LinesVisual3D line, List<SphereVisual3D> points)> _curveVisuals = new();
      // 添加曲线点的模型映射，用于点击检测
        private readonly Dictionary<GeometryModel3D, (PathCurveModel curve, int pointIndex)> _curvePointModelMap = new();
      // 添加曲线点的标签
private readonly Dictionary<(PathCurveModel curve, int pointIndex), BillboardTextVisual3D> _curvePointLabels = new();
        
     /// <summary>
/// 渲染导入的曲线列表
        /// </summary>
    /// <param name="curves">曲线模型列表</param>
        public void RenderImportedCurves(IEnumerable<PathCurveModel> curves)
  {
 // 清除之前的曲线可视化元素
            ClearImportedCurves();
 
        foreach (var curve in curves)
         {
     RenderSingleCurve(curve);
            }
  }
        
 /// <summary>
        /// 渲染单条曲线
  /// </summary>
      /// <param name="curve">曲线模型</param>
     public void RenderSingleCurve(PathCurveModel curve)
        {
       if (curve == null) return;
  
      // 如果已经渲染过，先移除
 if (_curveVisuals.ContainsKey(curve))
  {
 RemoveCurve(curve);
   }
       
   // 线条使用更细的粗细，让它看起来像正常的线段
   var lineVisual = new LinesVisual3D { Thickness = 1.5, Color = Colors.Blue };
    var pointVisuals = new List<SphereVisual3D>();
   
    // 选择要渲染的点集：如果已放样则用放样点，否则用原始点
    var pointsToRender = curve.IsLofted && curve.LoftedPoints.Count > 0 
? curve.LoftedPoints 
       : curve.OriginalPoints;
  
  if (pointsToRender.Count == 0) return;
       
  // 渲染线条
      for (int i = 0; i < pointsToRender.Count - 1; i++)
{
         var p1 = LogicalToVisual(pointsToRender[i]);
    var p2 = LogicalToVisual(pointsToRender[i + 1]);
       lineVisual.Points.Add(p1);
   lineVisual.Points.Add(p2);
    }
     
   SceneRoot.Children.Add(lineVisual);
      _curveLineVisuals.Add(lineVisual);
   
   // 渲染控制点 - 使用更小的点
        for (int i = 0; i < pointsToRender.Count; i++)
            {
var point = pointsToRender[i];
   var visualCenter = LogicalToVisual(point);
  // 使用专门的曲线点半径计算方法
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
   
     // 添加点击检测映射
  if (sphere.Content is GeometryModel3D sphereModel)
       {
     _curvePointModelMap[sphereModel] = (curve, i);
    }
       
 // 添加标签 - 修复标签位置，使用较小的偏移量
    var labelOffset = new Vector3D(
        0.3 * _visualScale,  // X 偏移：约为点半径的 2 倍
  0.3 * _visualScale,  // Y 偏移
            0.4 * _visualScale   // Z 偏移：稍高一点
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
    
  // 保存引用
_curveVisuals[curve] = (lineVisual, pointVisuals);
  
     // 如果是选中的曲线，高亮显示
  if (curve.IsSelected)
       {
      HighlightCurve(curve);
  }
        }
 
        /// <summary>
      /// 计算曲线点的半径（比控制点小得多）
      /// </summary>
        private double CalculateCurvePointRadius(Point3D position, bool isSelected)
        {
   // 基础半径：稍大于网格线（网格线粗细为 1-2）
     double baseRadius = 0.15 * _visualScale; // 约为网格线的 1.5 倍粗细
            
       if (_autoScalePoints && HelixView.Camera is PerspectiveCamera cam)
 {
        // 根据相机距离自动缩放
        var distance = (cam.Position - position).Length;
  // 距离越远，点越大（保持视觉大小一致）
      var scaleFactor = System.Math.Max(0.3, System.Math.Min(3.0, distance / 50.0));
     baseRadius *= scaleFactor;
 }
       
   // 选中时稍微大一点
       if (isSelected)
  {
    baseRadius *= 1.3;
      }
 
     // 应用用户设置的缩放因子
       return baseRadius * _pointSizeScale;
    }
        
   /// <summary>
   /// 更新曲线的可视化（例如放样后）
      /// </summary>
        /// <param name="curve">曲线模型</param>
      public void UpdateCurve(PathCurveModel curve)
        {
  if (curve == null) return;
       
 // 重新渲染曲线
         RenderSingleCurve(curve);
        }
  
        /// <summary>
 /// 移除单条曲线的可视化
     /// </summary>
      /// <param name="curve">曲线模型</param>
      public void RemoveCurve(PathCurveModel curve)
  {
   if (!_curveVisuals.TryGetValue(curve, out var visuals)) return;
   
    // 移除线条
      SceneRoot.Children.Remove(visuals.line);
 _curveLineVisuals.Remove(visuals.line);
   
   // 移除点
   foreach (var point in visuals.points)
      {
    SceneRoot.Children.Remove(point);
  _curvePointVisuals.Remove(point);

         // 移除点击检测映射
       if (point.Content is GeometryModel3D model && _curvePointModelMap.ContainsKey(model))
       {
      _curvePointModelMap.Remove(model);
       }
     }
   
 // 移除标签
   var labelsToRemove = _curvePointLabels.Where(kv => kv.Key.curve == curve).ToList();
        foreach (var kvp in labelsToRemove)
    {
    SceneRoot.Children.Remove(kvp.Value);
_curvePointLabels.Remove(kvp.Key);
        }
     
   _curveVisuals.Remove(curve);
    }
  
        /// <summary>
   /// 高亮显示曲线（选中状态）
      /// </summary>
        /// <param name="curve">曲线模型</param>
    public void HighlightCurve(PathCurveModel curve)
        {
        if (!_curveVisuals.TryGetValue(curve, out var visuals)) return;
            
      // 线条高亮 - 稍微粗一点
    visuals.line.Color = Colors.Orange;
     visuals.line.Thickness = 2.5;
     
     // 点高亮 - 重新计算半径（选中状态）
       foreach (var point in visuals.points)
 {
    point.Fill = new SolidColorBrush(Colors.Orange);
  // 使用选中状态的半径
point.Radius = CalculateCurvePointRadius(point.Center, true);
 }
     }
        
        /// <summary>
      /// 取消高亮（恢复正常状态）
   /// </summary>
        /// <param name="curve">曲线模型</param>
     public void UnhighlightCurve(PathCurveModel curve)
     {
 if (!_curveVisuals.TryGetValue(curve, out var visuals)) return;
            
   // 恢复线条
 visuals.line.Color = Colors.Blue;
    visuals.line.Thickness = 1.5;
  
   // 恢复点 - 使用未选中状态的半径
            foreach (var point in visuals.points)
    {
                point.Fill = new SolidColorBrush(Colors.Blue);
      point.Radius = CalculateCurvePointRadius(point.Center, false);
          }
        }
        
        /// <summary>
 /// 清除所有导入曲线的可视化
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
 
   // 清理标签
        foreach (var label in _curvePointLabels.Values)
  {
  SceneRoot.Children.Remove(label);
   }
   _curvePointLabels.Clear();
 
     // 清理点击检测映射
 _curvePointModelMap.Clear();
     _curveVisuals.Clear();
    }

     /// <summary>
        /// 隐藏所有曲线点的标签（生成步骤后避免标签重复）
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
        /// 显示曲线点的标签
        /// </summary>
        public void ShowCurveLabels()
        {
            // 重新渲染所有曲线以恢复标签
            var curvesToRender = _curveVisuals.Keys.ToList();
   foreach (var curve in curvesToRender)
            {
     RenderSingleCurve(curve);
            }
        }
        
        /// <summary>
   /// 自适应视图以显示所有曲线
      /// </summary>
        public void FitCurvesToView()
        {
    try
    {
   if (_curvePointVisuals.Count == 0) return;
 
       // 计算所有点的边界框
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
  
    // 计算中心点和尺寸
    var centerX = (minX + maxX) / 2;
var centerY = (minY + maxY) / 2;
        var centerZ = (minZ + maxZ) / 2;
           var sizeX = maxX - minX;
  var sizeY = maxY - minY;
       var sizeZ = maxZ - minZ;
         var maxSize = System.Math.Max(System.Math.Max(sizeX, sizeY), sizeZ);
 
       // 设置相机位置
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
  
   // 自动缩放模式下更新控制点大小
        if (_autoScalePoints) RefreshPointSizes();
   }
      }
    catch { }
 }
        
   /// <summary>
        /// 刷新所有曲线点的大小（用于缩放视图后）
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
        
        // 辅助方法：计算两点之间的 XY 平面距离（如果不存在）
      private double DistanceXY(Point3D p1, Point3D p2)
        {
    var dx = p1.X - p2.X;
          var dy = p1.Y - p2.Y;
   return System.Math.Sqrt(dx * dx + dy * dy);
   }
   
        // 辅助方法：获取场景中心
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
  
    // 辅助方法：对圆弧进行采样
        private List<Point3D> SampleArcPointsForRendering(Point3D p1, Point3D p2, Point3D p3, double spacing)
     {
   var result = new List<Point3D> { p1 };

     // 简化实现：贝塞尔曲线插值
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
      
        // 键盘事件处理
 private void UserControl_PreviewKeyDown(object? sender, System.Windows.Input.KeyEventArgs e)
        {
       // F 键 - 自适应视图
      if (e.Key == System.Windows.Input.Key.F)
   {
 FitCurvesToView();
       e.Handled = true;
    }
        }
 }
}
