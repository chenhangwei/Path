using System.Windows.Media.Media3D;

namespace Path.Services.Step214
{
    /// <summary>
    /// STEP 214 ������ȡ��
    /// </summary>
    public class Step214CurveExtractor
    {
      private readonly Step214Parser _parser;
 private Dictionary<int, StepEntity> _entities = new();
    
        // STEP ʵ������ӳ�����д -> �������ƣ�
        private readonly Dictionary<string, string> _entityNameMap = new()
   {
            // ������
  { "CRTPNT", "CARTESIAN_POINT" },
            { "CRTPT", "CARTESIAN_POINT" },
    { "CRPNT", "CARTESIAN_POINT" },
   
        // ��������
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
      
  // ���������
    { "DRCTN", "DIRECTION" },
    { "DIR", "DIRECTION" },
            { "VCT", "VECTOR" },
      { "VCTR", "VECTOR" },
    
            // ����ϵ
        { "AX2PL3", "AXIS2_PLACEMENT_3D" },
            { "AXIS2", "AXIS2_PLACEMENT_3D" },
            
 // ����
            { "LNMSR", "LENGTH_MEASURE" },
      { "LNMES", "LENGTH_MEASURE" }
        };

        public Step214CurveExtractor(Step214Parser parser)
  {
  _parser = parser;
   }
      
        /// <summary>
        /// ��׼��ʵ���������ƣ�������д��
      /// </summary>
        private string NormalizeEntityType(string type)
        {
            if (string.IsNullOrEmpty(type))
     return type;
           
            var upperType = type.ToUpperInvariant();
 
         // �������д��������������
            if (_entityNameMap.TryGetValue(upperType, out var fullName))
            return fullName;
   
    // ���򷵻�ԭ����
         return upperType;
      }

     /// <summary>
        /// �� STEP �ļ���ȡ��������
        /// </summary>
      public List<Point3DCollection> ExtractCurves(string filePath)
        {
       _entities = _parser.Parse(filePath);
   var curves = new List<Point3DCollection>();

System.Diagnostics.Debug.WriteLine($"========== STEP �ļ����� ==========");
       System.Diagnostics.Debug.WriteLine($"��ʵ����: {_entities.Count}");

     // ���������������ͣ�������д��
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

  // ͳ��ÿ�����͵�����
       var typeStats = new Dictionary<string, int>();
   foreach (var entity in _entities.Values)
    {
        var normalizedType = NormalizeEntityType(entity.Type);
if (!typeStats.ContainsKey(normalizedType))
        typeStats[normalizedType] = 0;
 typeStats[normalizedType]++;
}

System.Diagnostics.Debug.WriteLine("\nʵ������ͳ�� (��׼����):");
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
  System.Diagnostics.Debug.WriteLine($"\n�ҵ� {curveEntities.Count} �� {normalizedCurveType} ({curveType})");
 }
       
     foreach (var entity in curveEntities)
      {
    var points = ExtractCurvePoints(entity);
   if (points != null && points.Count > 0)
        {
    curves.Add(points);
  System.Diagnostics.Debug.WriteLine($"  ? ��ȡ�� {points.Count} ����");
  }
    else
  {
    System.Diagnostics.Debug.WriteLine($"  ? �޷���ȡ��");
     }
     }
     }

    System.Diagnostics.Debug.WriteLine($"\n========== ��ȡ��� ==========");
       System.Diagnostics.Debug.WriteLine($"�ɹ���ȡ {curves.Count} ������");

return curves;
   }

        /// <summary>
 /// ������ʵ����ȡ��
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
        /// ��ȡ B-Spline ���ߵ�
    /// </summary>
      private Point3DCollection ExtractBSplinePoints(StepEntity entity)
  {
       var points = new Point3DCollection();

       try
   {
        // B_SPLINE_CURVE ��ʽ:
  // B_SPLINE_CURVE('name', degree, control_points_list, curve_form, closed_curve, self_intersect)
   
 System.Diagnostics.Debug.WriteLine($"\n���� B-Spline ���� #{entity.Id}:");
  System.Diagnostics.Debug.WriteLine($"  ��������: {entity.Parameters.Count}");
       
   // ���������������ʽ���죬���Զ��ֿ�����
   for (int i = 0; i < entity.Parameters.Count; i++)
     {
      var param = entity.Parameters[i];
   System.Diagnostics.Debug.WriteLine($"  ����[{i}]: {param?.GetType().Name ?? "null"} = {param}");
         
    // ���ҿ��Ƶ��б�ͨ�������û�ֱ�ӵĵ��б�
     if (param is StepReference controlPointsRef)
{
    System.Diagnostics.Debug.WriteLine($"    ���Խ������� #{controlPointsRef.Id}");
      var controlPointsList = _parser.GetEntity(controlPointsRef.Id);
     if (controlPointsList != null)
{
    System.Diagnostics.Debug.WriteLine($"    ����ʵ������: {controlPointsList.Type}");
    System.Diagnostics.Debug.WriteLine($"    ����ʵ�������: {controlPointsList.Parameters.Count}");
           
 // ������Ƶ��б�
    foreach (var subParam in controlPointsList.Parameters)
         {
    if (subParam is StepReference pointRef)
   {
          var point = ExtractCartesianPoint(pointRef.Id);
             if (point.HasValue)
       {
    points.Add(point.Value);
   System.Diagnostics.Debug.WriteLine($"   ��ȡ��: ({point.Value.X:F2}, {point.Value.Y:F2}, {point.Value.Z:F2})");
     }
     }
    }
   }
 }
  }

       // ���û���㹻�ĵ㣬���Բ���
 if (points.Count < 2)
  {
   System.Diagnostics.Debug.WriteLine("  ? ���Ƶ㲻�㣬������������");
       }
    }
     catch (Exception ex)
  {
      System.Diagnostics.Debug.WriteLine($"  ? ����ʧ��: {ex.Message}");
       }

      return points;
  }

        /// <summary>
   /// ��ȡ Bezier ���ߵ�
   /// </summary>
     private Point3DCollection ExtractBezierPoints(StepEntity entity)
  {
 // Bezier ������ B-Spline ����
        return ExtractBSplinePoints(entity);
   }

     /// <summary>
  /// ��ȡ���� B-Spline ���ߵ�
     /// </summary>
  private Point3DCollection ExtractRationalBSplinePoints(StepEntity entity)
    {
     // ���� B-Spline ����Ȩ�أ�����򻯴���
            return ExtractBSplinePoints(entity);
    }

   /// <summary>
        /// ��ȡ Polyline ��
        /// </summary>
   private Point3DCollection ExtractPolylinePoints(StepEntity entity)
        {
      var points = new Point3DCollection();

       try
       {
       // POLYLINE ��ʽ: POLYLINE('name', points_list)
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
    /// ��ȡ Line ��
     /// </summary>
      private Point3DCollection ExtractLinePoints(StepEntity entity)
   {
    var points = new Point3DCollection();

  try
        {
       // LINE ��ʽ: LINE('name', point, vector)
          // vector ��������ͳ���
    if (entity.Parameters.Count > 1)
   {
   if (entity.Parameters[1] is StepReference point1Ref)
      {
  var p1 = ExtractCartesianPoint(point1Ref.Id);
   if (p1.HasValue)
{
    points.Add(p1.Value);
          
     // ����߶��յ㣨��� + ����������
   if (entity.Parameters.Count > 2 && entity.Parameters[2] is StepReference vectorRef)
 {
         // ExtractVector ������ȷ���� ���� * ����
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
  System.Diagnostics.Debug.WriteLine($"  ? ��ȡ LINE ʧ��: {ex.Message}");
   }

      return points;
        }

        /// <summary>
        /// ��ȡ Circle ��
  /// </summary>
   private Point3DCollection ExtractCirclePoints(StepEntity entity)
  {
      var points = new Point3DCollection();

            try
 {
     // CIRCLE ��ʽ: CIRCLE('name', position, radius)
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
   // ����Բ��
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
     /// ��ȡ Ellipse ��
   /// </summary>
    private Point3DCollection ExtractEllipsePoints(StepEntity entity)
     {
      var points = new Point3DCollection();

      try
       {
     // ELLIPSE ��ʽ: ELLIPSE('name', position, semi_axis1, semi_axis2)
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
      // ������Բ
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
   /// ��ȡ Trimmed Curve ��
        /// </summary>
   private Point3DCollection ExtractTrimmedCurvePoints(StepEntity entity)
  {
    // TRIMMED_CURVE ��ʽ: TRIMMED_CURVE('name', basis_curve, trim1, trim2, sense, master_representation)
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
   /// ��ȡ Composite Curve ��
        /// </summary>
   private Point3DCollection ExtractCompositeCurvePoints(StepEntity entity)
{
     var points = new Point3DCollection();

   // COMPOSITE_CURVE ��ʽ: COMPOSITE_CURVE('name', segments, self_intersect)
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
   /// ��ȡ�ѿ��������
     /// </summary>
      private Point3D? ExtractCartesianPoint(int entityId)
{
  var entity = _parser.GetEntity(entityId);
       if (entity == null)
 {
 System.Diagnostics.Debug.WriteLine($"      ? ʵ�� #{entityId} ������");
 return null;
       }

    try
       {
     // ��׼��ʵ�����ͣ�������д��
  var normalizedType = NormalizeEntityType(entity.Type);
  
     // CARTESIAN_POINT ��ʽ: CARTESIAN_POINT('name', (x, y, z))
     // ��д��ʽ: CRTPNT('name', (x, y, z))
 if (normalizedType == "CARTESIAN_POINT" && entity.Parameters.Count > 1)
 {
 var coords = entity.Parameters[1];
       
      // ���������Ƕ��ʵ���ֱ�ӵĲ����б�
       List<object> coordList;
 
    if (coords is StepEntity coordEntity)
 {
      coordList = coordEntity.Parameters;
    }
    else if (coords is string coordStr)
{
     // ���������ַ��� "(x, y, z)"
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
 System.Diagnostics.Debug.WriteLine($"      ? δ֪�����ʽ: {coords?.GetType().Name}");
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
 System.Diagnostics.Debug.WriteLine($" ? ������������: {coordList.Count}");
   }
       }
    else
 {
     System.Diagnostics.Debug.WriteLine($"      ? �ǵѿ���������: {entity.Type} (��׼��: {normalizedType})");
  }
  }
    catch (Exception ex)
   {
  System.Diagnostics.Debug.WriteLine($"  ? ������ʧ��: {ex.Message}");
  }

      return null;
  }

        /// <summary>
        /// ��ȡ����
     /// </summary>
        private Vector3D? ExtractVector(int entityId)
   {
       var entity = _parser.GetEntity(entityId);
       if (entity == null)
      return null;

 try
     {
// VECTOR ��ʽ: VECTOR('name', direction_ref, magnitude)
        // direction_ref ָ�� DIRECTION ʵ�壬magnitude �ǳ���
     var normalizedType = NormalizeEntityType(entity.Type);
        
        if (normalizedType == "VECTOR" && entity.Parameters.Count >= 2)
        {
  // ���� 1: ��������
     // ���� 2: ���ȣ���ѡ��
       
            Vector3D direction = new Vector3D(1, 0, 0);  // Ĭ�Ϸ���
   double magnitude = 1.0;  // Ĭ�ϳ���
  
    // ��ȡ����
            if (entity.Parameters[1] is StepReference dirRef)
  {
       var dirEntity = _parser.GetEntity(dirRef.Id);
      if (dirEntity != null)
 {
         direction = ExtractDirection(dirEntity) ?? direction;
        }
  }
        
    // ��ȡ���ȣ�����У�
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
            
   // ���ط��� * ����
    return new Vector3D(
     direction.X * magnitude,
     direction.Y * magnitude,
       direction.Z * magnitude
    );
        }
        
        // ���ݾɸ�ʽ��ֱ�Ӵ��� DIRECTION
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
        /// ��ȡ���򣨵�λ������
        /// </summary>
        private Vector3D? ExtractDirection(StepEntity entity)
        {
  try
  {
     var normalizedType = NormalizeEntityType(entity.Type);
       
  // DIRECTION ��ʽ: DIRECTION('name', (x, y, z))
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
        /// ��ȡ����ϵ
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
  /// ���� B-Spline ����
   /// </summary>
        private Point3DCollection SampleBSplineCurve(StepEntity entity, int sampleCount)
        {
  var points = new Point3DCollection();
  
   // ������Ҫʵ�� B-Spline ��ֵ�㷨
            // ���ڸ����ԣ����ﷵ�ؿռ��ϣ�ʵ����Ŀ����Ҫ����ʵ��
          
         return points;
   }
    }
}
