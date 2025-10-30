using System.Windows.Media.Media3D;

namespace Path.Services.Step214
{
    /// <summary>
    /// STEP 214 曲线提取器
    /// </summary>
    public class Step214CurveExtractor
    {
      private readonly Step214Parser _parser;
 private Dictionary<int, StepEntity> _entities = new();
    
        // STEP 实体名称映射表（缩写 -> 完整名称）
        private readonly Dictionary<string, string> _entityNameMap = new()
   {
            // 点类型
  { "CRTPNT", "CARTESIAN_POINT" },
            { "CRTPT", "CARTESIAN_POINT" },
    { "CRPNT", "CARTESIAN_POINT" },
   
        // 曲线类型
            { "BSPCRV", "B_SPLINE_CURVE" },
            { "BSCRV", "B_SPLINE_CURVE" },
        { "BZCRV", "BEZIER_CURVE" },
            { "RBSCRV", "RATIONAL_B_SPLINE_CURVE" },
       { "TRMCRV", "TRIMMED_CURVE" },
            { "CMPCRV", "COMPOSITE_CURVE" },
     { "EDGCRV", "EDGE_CURVE" },
            { "SEAMCRV", "SEAM_CURVE" },
        { "SRFCRV", "SURFACE_CURVE" },
            { "PYRCRV", "POLYLINE_CURVE" },
    { "PRLYAS", "POLYLINE" },
      
  // 方向和向量
    { "DRCTN", "DIRECTION" },
    { "DIR", "DIRECTION" },
            { "VCT", "VECTOR" },
      { "VCTR", "VECTOR" },
    
            // 坐标系
        { "AX2PL3", "AXIS2_PLACEMENT_3D" },
            { "AXIS2", "AXIS2_PLACEMENT_3D" },
            
 // 其他
            { "LNMSR", "LENGTH_MEASURE" },
      { "LNMES", "LENGTH_MEASURE" }
        };

        public Step214CurveExtractor(Step214Parser parser)
  {
  _parser = parser;
   }
      
        /// <summary>
        /// 标准化实体类型名称（处理缩写）
      /// </summary>
        private string NormalizeEntityType(string type)
        {
            if (string.IsNullOrEmpty(type))
     return type;
           
            var upperType = type.ToUpperInvariant();
 
         // 如果是缩写，返回完整名称
            if (_entityNameMap.TryGetValue(upperType, out var fullName))
            return fullName;
   
    // 否则返回原名称
         return upperType;
      }

     /// <summary>
        /// 从 STEP 文件提取所有曲线
        /// </summary>
      public List<Point3DCollection> ExtractCurves(string filePath)
        {
       _entities = _parser.Parse(filePath);
   var curves = new List<Point3DCollection>();

System.Diagnostics.Debug.WriteLine($"========== STEP 文件解析 ==========");
       System.Diagnostics.Debug.WriteLine($"总实体数: {_entities.Count}");

     // 查找所有曲线类型（包括缩写）
 var curveTypes = new[]
     {
    "B_SPLINE_CURVE", "BSPCRV", "BSCRV",
    "B_SPLINE_CURVE_WITH_KNOTS",
     "BEZIER_CURVE", "BZCRV",
      "RATIONAL_B_SPLINE_CURVE", "RBSCRV",
  "POLYLINE", "PRLYAS", "PYRCRV",
  "LINE",
  "CIRCLE",
    "ELLIPSE",
    "TRIMMED_CURVE", "TRMCRV",
  "COMPOSITE_CURVE", "CMPCRV",
  "BOUNDED_CURVE",
       "EDGE_CURVE", "EDGCRV",
     "SEAM_CURVE", "SEAMCRV",
       "SURFACE_CURVE", "SRFCRV"
     };

  // 统计每种类型的数量
       var typeStats = new Dictionary<string, int>();
   foreach (var entity in _entities.Values)
    {
        var normalizedType = NormalizeEntityType(entity.Type);
if (!typeStats.ContainsKey(normalizedType))
        typeStats[normalizedType] = 0;
 typeStats[normalizedType]++;
}

System.Diagnostics.Debug.WriteLine("\n实体类型统计 (标准化后):");
   foreach (var kvp in typeStats.OrderByDescending(x => x.Value).Take(20))
       {
           System.Diagnostics.Debug.WriteLine($"  {kvp.Key}: {kvp.Value}");
    }

   foreach (var curveType in curveTypes)
   {
                var normalizedCurveType = NormalizeEntityType(curveType);
         var curveEntities = _entities.Values
         .Where(e => NormalizeEntityType(e.Type) == normalizedCurveType)
       .ToList();
        
    if (curveEntities.Count > 0)
         {
  System.Diagnostics.Debug.WriteLine($"\n找到 {curveEntities.Count} 个 {normalizedCurveType} ({curveType})");
 }
       
     foreach (var entity in curveEntities)
      {
    var points = ExtractCurvePoints(entity);
   if (points != null && points.Count > 0)
        {
    curves.Add(points);
  System.Diagnostics.Debug.WriteLine($"  ? 提取到 {points.Count} 个点");
  }
    else
  {
    System.Diagnostics.Debug.WriteLine($"  ? 无法提取点");
     }
     }
     }

    System.Diagnostics.Debug.WriteLine($"\n========== 提取结果 ==========");
       System.Diagnostics.Debug.WriteLine($"成功提取 {curves.Count} 条曲线");

return curves;
   }

        /// <summary>
 /// 从曲线实体提取点
        /// </summary>
        private Point3DCollection? ExtractCurvePoints(StepEntity entity)
  {
      return entity.Type.ToUpperInvariant() switch
   {
           "B_SPLINE_CURVE" or "B_SPLINE_CURVE_WITH_KNOTS" => ExtractBSplinePoints(entity),
         "BEZIER_CURVE" => ExtractBezierPoints(entity),
       "RATIONAL_B_SPLINE_CURVE" => ExtractRationalBSplinePoints(entity),
       "POLYLINE" => ExtractPolylinePoints(entity),
    "LINE" => ExtractLinePoints(entity),
     "CIRCLE" => ExtractCirclePoints(entity),
      "ELLIPSE" => ExtractEllipsePoints(entity),
    "TRIMMED_CURVE" => ExtractTrimmedCurvePoints(entity),
"COMPOSITE_CURVE" => ExtractCompositeCurvePoints(entity),
     _ => null
     };
        }

 /// <summary>
        /// 提取 B-Spline 曲线点
    /// </summary>
      private Point3DCollection ExtractBSplinePoints(StepEntity entity)
  {
       var points = new Point3DCollection();

       try
   {
        // B_SPLINE_CURVE 格式:
  // B_SPLINE_CURVE('name', degree, control_points_list, curve_form, closed_curve, self_intersect)
   
 System.Diagnostics.Debug.WriteLine($"\n解析 B-Spline 曲线 #{entity.Id}:");
  System.Diagnostics.Debug.WriteLine($"  参数数量: {entity.Parameters.Count}");
       
   // 参数索引可能因格式而异，尝试多种可能性
   for (int i = 0; i < entity.Parameters.Count; i++)
     {
      var param = entity.Parameters[i];
   System.Diagnostics.Debug.WriteLine($"  参数[{i}]: {param?.GetType().Name ?? "null"} = {param}");
         
    // 查找控制点列表（通常是引用或直接的点列表）
     if (param is StepReference controlPointsRef)
{
    System.Diagnostics.Debug.WriteLine($"    尝试解析引用 #{controlPointsRef.Id}");
      var controlPointsList = _parser.GetEntity(controlPointsRef.Id);
     if (controlPointsList != null)
{
    System.Diagnostics.Debug.WriteLine($"    引用实体类型: {controlPointsList.Type}");
    System.Diagnostics.Debug.WriteLine($"    引用实体参数数: {controlPointsList.Parameters.Count}");
           
 // 处理控制点列表
    foreach (var subParam in controlPointsList.Parameters)
         {
    if (subParam is StepReference pointRef)
   {
          var point = ExtractCartesianPoint(pointRef.Id);
             if (point.HasValue)
       {
    points.Add(point.Value);
   System.Diagnostics.Debug.WriteLine($"   提取点: ({point.Value.X:F2}, {point.Value.Y:F2}, {point.Value.Z:F2})");
     }
     }
    }
   }
 }
  }

       // 如果没有足够的点，尝试采样
 if (points.Count < 2)
  {
   System.Diagnostics.Debug.WriteLine("  ? 控制点不足，尝试其他方法");
       }
    }
     catch (Exception ex)
  {
      System.Diagnostics.Debug.WriteLine($"  ? 解析失败: {ex.Message}");
       }

      return points;
  }

        /// <summary>
   /// 提取 Bezier 曲线点
   /// </summary>
     private Point3DCollection ExtractBezierPoints(StepEntity entity)
  {
 // Bezier 曲线与 B-Spline 类似
        return ExtractBSplinePoints(entity);
   }

     /// <summary>
  /// 提取有理 B-Spline 曲线点
     /// </summary>
  private Point3DCollection ExtractRationalBSplinePoints(StepEntity entity)
    {
     // 有理 B-Spline 包含权重，这里简化处理
            return ExtractBSplinePoints(entity);
    }

   /// <summary>
        /// 提取 Polyline 点
        /// </summary>
   private Point3DCollection ExtractPolylinePoints(StepEntity entity)
        {
      var points = new Point3DCollection();

       try
       {
       // POLYLINE 格式: POLYLINE('name', points_list)
       if (entity.Parameters.Count > 1 && entity.Parameters[1] is StepReference pointsRef)
   {
   var pointsList = _parser.GetEntity(pointsRef.Id);
        if (pointsList != null)
       {
    foreach (var param in pointsList.Parameters)
     {
       if (param is StepReference pointRef)
    {
   var point = ExtractCartesianPoint(pointRef.Id);
           if (point.HasValue)
              {
      points.Add(point.Value);
      }
    }
    }
            }
}
     }
       catch { }

         return points;
   }

        /// <summary>
    /// 提取 Line 点
     /// </summary>
      private Point3DCollection ExtractLinePoints(StepEntity entity)
   {
    var points = new Point3DCollection();

  try
        {
       // LINE 格式: LINE('name', point, vector)
          // vector 包含方向和长度
    if (entity.Parameters.Count > 1)
   {
   if (entity.Parameters[1] is StepReference point1Ref)
      {
  var p1 = ExtractCartesianPoint(point1Ref.Id);
   if (p1.HasValue)
{
    points.Add(p1.Value);
          
     // 添加线段终点（起点 + 方向向量）
   if (entity.Parameters.Count > 2 && entity.Parameters[2] is StepReference vectorRef)
 {
         // ExtractVector 现在正确返回 方向 * 长度
                var vector = ExtractVector(vectorRef.Id);
          if (vector.HasValue)
    {
                var p2 = new Point3D(
    p1.Value.X + vector.Value.X,
    p1.Value.Y + vector.Value.Y,
       p1.Value.Z + vector.Value.Z);
       points.Add(p2);
          
     System.Diagnostics.Debug.WriteLine($"    LINE: P1=({p1.Value.X:F2}, {p1.Value.Y:F2}, {p1.Value.Z:F2})");
        System.Diagnostics.Debug.WriteLine($"    LINE: Vector=({vector.Value.X:F2}, {vector.Value.Y:F2}, {vector.Value.Z:F2})");
   System.Diagnostics.Debug.WriteLine($"    LINE: P2=({p2.X:F2}, {p2.Y:F2}, {p2.Z:F2})");
      }
          }
   }
          }
        }
     }
     catch (Exception ex)
   {
  System.Diagnostics.Debug.WriteLine($"  ? 提取 LINE 失败: {ex.Message}");
   }

      return points;
        }

        /// <summary>
        /// 提取 Circle 点
  /// </summary>
   private Point3DCollection ExtractCirclePoints(StepEntity entity)
  {
      var points = new Point3DCollection();

            try
 {
     // CIRCLE 格式: CIRCLE('name', position, radius)
        if (entity.Parameters.Count > 2)
       {
      Point3D? center = null;
     double radius = 0;

          if (entity.Parameters[1] is StepReference posRef)
       {
var pos = ExtractAxis2Placement3D(posRef.Id);
 center = pos.Origin;
     }

   if (entity.Parameters[2] is double r)
        {
   radius = r;
         }

        if (center.HasValue && radius > 0)
    {
   // 采样圆形
    for (int i = 0; i <= 36; i++)
         {
   double angle = i * Math.PI * 2 / 36;
     double x = center.Value.X + radius * Math.Cos(angle);
          double y = center.Value.Y + radius * Math.Sin(angle);
     points.Add(new Point3D(x, y, center.Value.Z));
 }
    }
      }
     }
       catch { }

    return points;
        }

        /// <summary>
     /// 提取 Ellipse 点
   /// </summary>
    private Point3DCollection ExtractEllipsePoints(StepEntity entity)
     {
      var points = new Point3DCollection();

      try
       {
     // ELLIPSE 格式: ELLIPSE('name', position, semi_axis1, semi_axis2)
    if (entity.Parameters.Count > 3)
     {
        Point3D? center = null;
         double semiAxis1 = 0, semiAxis2 = 0;

    if (entity.Parameters[1] is StepReference posRef)
          {
         var pos = ExtractAxis2Placement3D(posRef.Id);
            center = pos.Origin;
          }

     if (entity.Parameters[2] is double a1) semiAxis1 = a1;
       if (entity.Parameters[3] is double a2) semiAxis2 = a2;

   if (center.HasValue && semiAxis1 > 0 && semiAxis2 > 0)
        {
      // 采样椭圆
          for (int i = 0; i <= 36; i++)
     {
    double angle = i * Math.PI * 2 / 36;
  double x = center.Value.X + semiAxis1 * Math.Cos(angle);
           double y = center.Value.Y + semiAxis2 * Math.Sin(angle);
          points.Add(new Point3D(x, y, center.Value.Z));
    }
    }
   }
       }
            catch { }

 return points;
     }

  /// <summary>
   /// 提取 Trimmed Curve 点
        /// </summary>
   private Point3DCollection ExtractTrimmedCurvePoints(StepEntity entity)
  {
    // TRIMMED_CURVE 格式: TRIMMED_CURVE('name', basis_curve, trim1, trim2, sense, master_representation)
     if (entity.Parameters.Count > 1 && entity.Parameters[1] is StepReference basisCurveRef)
{
       var basisCurve = _parser.GetEntity(basisCurveRef.Id);
      if (basisCurve != null)
       {
          return ExtractCurvePoints(basisCurve) ?? new Point3DCollection();
       }
       }
 return new Point3DCollection();
        }

/// <summary>
   /// 提取 Composite Curve 点
        /// </summary>
   private Point3DCollection ExtractCompositeCurvePoints(StepEntity entity)
{
     var points = new Point3DCollection();

   // COMPOSITE_CURVE 格式: COMPOSITE_CURVE('name', segments, self_intersect)
     if (entity.Parameters.Count > 1 && entity.Parameters[1] is StepReference segmentsRef)
  {
        var segments = _parser.GetEntity(segmentsRef.Id);
        if (segments != null)
      {
  foreach (var param in segments.Parameters)
        {
     if (param is StepReference segmentRef)
   {
         var segment = _parser.GetEntity(segmentRef.Id);
       if (segment != null && segment.Parameters.Count > 1 && segment.Parameters[1] is StepReference curveRef)
      {
                var curve = _parser.GetEntity(curveRef.Id);
      if (curve != null)
        {
 var curvePoints = ExtractCurvePoints(curve);
   if (curvePoints != null)
        {
       foreach (var pt in curvePoints)
 {
              points.Add(pt);
           }
    }
      }
                }
          }
         }
    }
   }

return points;
        }

      /// <summary>
   /// 提取笛卡尔坐标点
     /// </summary>
      private Point3D? ExtractCartesianPoint(int entityId)
{
  var entity = _parser.GetEntity(entityId);
       if (entity == null)
 {
 System.Diagnostics.Debug.WriteLine($"      ? 实体 #{entityId} 不存在");
 return null;
       }

    try
       {
     // 标准化实体类型（处理缩写）
  var normalizedType = NormalizeEntityType(entity.Type);
  
     // CARTESIAN_POINT 格式: CARTESIAN_POINT('name', (x, y, z))
     // 缩写格式: CRTPNT('name', (x, y, z))
 if (normalizedType == "CARTESIAN_POINT" && entity.Parameters.Count > 1)
 {
 var coords = entity.Parameters[1];
       
      // 坐标可能是嵌套实体或直接的参数列表
       List<object> coordList;
 
    if (coords is StepEntity coordEntity)
 {
      coordList = coordEntity.Parameters;
    }
    else if (coords is string coordStr)
{
     // 解析坐标字符串 "(x, y, z)"
    coordStr = coordStr.Trim('(', ')');
   var parts = coordStr.Split(',');
  coordList = parts.Select(p => 
     {
    if (double.TryParse(p.Trim(), System.Globalization.NumberStyles.Any, 
   System.Globalization.CultureInfo.InvariantCulture, out var val))
      return (object)val;
       return (object)p.Trim();
     }).ToList();
 }
     else if (coords is List<object> list)
        {
      coordList = list;
     }
      else
       {
 System.Diagnostics.Debug.WriteLine($"      ? 未知坐标格式: {coords?.GetType().Name}");
     return null;
    }

   if (coordList.Count >= 3)
     {
var x = Convert.ToDouble(coordList[0], System.Globalization.CultureInfo.InvariantCulture);
   var y = Convert.ToDouble(coordList[1], System.Globalization.CultureInfo.InvariantCulture);
  var z = Convert.ToDouble(coordList[2], System.Globalization.CultureInfo.InvariantCulture);
return new Point3D(x, y, z);
     }
      else
     {
 System.Diagnostics.Debug.WriteLine($" ? 坐标数量不足: {coordList.Count}");
   }
       }
    else
 {
     System.Diagnostics.Debug.WriteLine($"      ? 非笛卡尔点类型: {entity.Type} (标准化: {normalizedType})");
  }
  }
    catch (Exception ex)
   {
  System.Diagnostics.Debug.WriteLine($"  ? 解析点失败: {ex.Message}");
  }

      return null;
  }

        /// <summary>
        /// 提取向量
     /// </summary>
        private Vector3D? ExtractVector(int entityId)
   {
       var entity = _parser.GetEntity(entityId);
       if (entity == null)
      return null;

 try
     {
// VECTOR 格式: VECTOR('name', direction_ref, magnitude)
        // direction_ref 指向 DIRECTION 实体，magnitude 是长度
     var normalizedType = NormalizeEntityType(entity.Type);
        
        if (normalizedType == "VECTOR" && entity.Parameters.Count >= 2)
        {
  // 参数 1: 方向引用
     // 参数 2: 长度（可选）
       
            Vector3D direction = new Vector3D(1, 0, 0);  // 默认方向
   double magnitude = 1.0;  // 默认长度
  
    // 提取方向
            if (entity.Parameters[1] is StepReference dirRef)
  {
       var dirEntity = _parser.GetEntity(dirRef.Id);
      if (dirEntity != null)
 {
         direction = ExtractDirection(dirEntity) ?? direction;
        }
  }
        
    // 提取长度（如果有）
   if (entity.Parameters.Count > 2)
  {
    if (entity.Parameters[2] is double mag)
    {
  magnitude = mag;
     }
     else if (entity.Parameters[2] is string magStr && 
      double.TryParse(magStr, System.Globalization.NumberStyles.Any,
     System.Globalization.CultureInfo.InvariantCulture, out var magVal))
         {
  magnitude = magVal;
         }
}
            
   // 返回方向 * 长度
    return new Vector3D(
     direction.X * magnitude,
     direction.Y * magnitude,
       direction.Z * magnitude
    );
        }
        
        // 兼容旧格式：直接处理 DIRECTION
  if (entity.Parameters.Count > 1)
     {
        var direction = entity.Parameters[1];
   if (direction is StepReference dirRef)
  {
      var dirEntity = _parser.GetEntity(dirRef.Id);
        if (dirEntity != null)
   {
     return ExtractDirection(dirEntity);
}
     }
     }
 }
    catch { }

    return null;
  }

        /// <summary>
        /// 提取方向（单位向量）
        /// </summary>
        private Vector3D? ExtractDirection(StepEntity entity)
        {
  try
  {
     var normalizedType = NormalizeEntityType(entity.Type);
       
  // DIRECTION 格式: DIRECTION('name', (x, y, z))
          if (normalizedType == "DIRECTION" && entity.Parameters.Count > 1)
                {
   var coords = entity.Parameters[1];
 List<object> coordList;

     if (coords is StepEntity coordEntity)
 {
        coordList = coordEntity.Parameters;
     }
     else if (coords is string coordStr)
      {
    coordStr = coordStr.Trim('(', ')');
      var parts = coordStr.Split(',');
 coordList = parts.Select(p =>
       {
       if (double.TryParse(p.Trim(), System.Globalization.NumberStyles.Any,
   System.Globalization.CultureInfo.InvariantCulture, out var val))
       return (object)val;
      return (object)p.Trim();
 }).ToList();
  }
 else if (coords is List<object> list)
              {
              coordList = list;
    }
       else
    {
   return null;
}

   if (coordList.Count >= 3)
   {
  var x = Convert.ToDouble(coordList[0], System.Globalization.CultureInfo.InvariantCulture);
    var y = Convert.ToDouble(coordList[1], System.Globalization.CultureInfo.InvariantCulture);
     var z = Convert.ToDouble(coordList[2], System.Globalization.CultureInfo.InvariantCulture);
   return new Vector3D(x, y, z);
 }
    }
     }
      catch { }
     
    return null;
 }

     /// <summary>
        /// 提取坐标系
        /// </summary>
    private (Point3D Origin, Vector3D XAxis, Vector3D YAxis, Vector3D ZAxis) ExtractAxis2Placement3D(int entityId)
   {
  var entity = _parser.GetEntity(entityId);
       var origin = new Point3D(0, 0, 0);
  var xAxis = new Vector3D(1, 0, 0);
   var yAxis = new Vector3D(0, 1, 0);
     var zAxis = new Vector3D(0, 0, 1);

     if (entity != null && entity.Parameters.Count > 0)
      {
       if (entity.Parameters[0] is StepReference originRef)
       {
  var o = ExtractCartesianPoint(originRef.Id);
         if (o.HasValue) origin = o.Value;
   }
    }

      return (origin, xAxis, yAxis, zAxis);
    }

        /// <summary>
  /// 采样 B-Spline 曲线
   /// </summary>
        private Point3DCollection SampleBSplineCurve(StepEntity entity, int sampleCount)
        {
  var points = new Point3DCollection();
  
   // 这里需要实现 B-Spline 求值算法
            // 由于复杂性，这里返回空集合，实际项目中需要完整实现
          
         return points;
   }
    }
}
