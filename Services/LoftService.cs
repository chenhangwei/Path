using System.Windows.Media.Media3D;

namespace Path.Services
{
    /// <summary>
    /// 曲线放样服务实现
    /// </summary>
    public class LoftService : ILoftService
    {
        public Point3DCollection LoftCurve(Point3DCollection originalPoints, int pointCount)
        {
            if (originalPoints == null || originalPoints.Count < 2)
       {
        throw new ArgumentException("原始点集至少需要 2 个点");
     }

 if (pointCount < 2)
            {
         throw new ArgumentException("放样点数量至少为 2");
   }

            var loftedPoints = new Point3DCollection();
  var totalLength = CalculateCurveLength(originalPoints);

            if (totalLength == 0)
   {
 // 如果曲线长度为 0，返回第一个点的副本
     for (int i = 0; i < pointCount; i++)
          {
        loftedPoints.Add(originalPoints[0]);
   }
    return loftedPoints;
    }

    // 计算等距间隔
            var interval = totalLength / (pointCount - 1);

  // 生成等距点
      for (int i = 0; i < pointCount; i++)
            {
             var distance = i * interval;
         var point = GetPointAtDistance(originalPoints, distance);
       loftedPoints.Add(point);
        }

         return loftedPoints;
        }

        public double CalculateCurveLength(Point3DCollection points)
      {
if (points == null || points.Count < 2)
          {
            return 0;
   }

         double totalLength = 0;
   for (int i = 1; i < points.Count; i++)
    {
       totalLength += (points[i] - points[i - 1]).Length;
            }

    return totalLength;
   }

   public Point3D GetPointAtDistance(Point3DCollection points, double distance)
        {
         if (points == null || points.Count == 0)
{
     throw new ArgumentException("点集不能为空");
   }

    if (distance <= 0)
        {
       return points[0];
   }

    double accumulatedLength = 0;
  
         for (int i = 1; i < points.Count; i++)
   {
         var segmentVector = points[i] - points[i - 1];
         var segmentLength = segmentVector.Length;

          if (accumulatedLength + segmentLength >= distance)
    {
         // 在当前线段上
        var remainingDistance = distance - accumulatedLength;
          var t = remainingDistance / segmentLength;
  
         return new Point3D(
         points[i - 1].X + segmentVector.X * t,
 points[i - 1].Y + segmentVector.Y * t,
 points[i - 1].Z + segmentVector.Z * t
         );
                }

          accumulatedLength += segmentLength;
    }

   // 如果距离超过曲线长度，返回最后一个点
       return points[points.Count - 1];
  }

        /// <summary>
   /// 计算点在曲线上的切线方向（用于计算 Yaw 角度）
   /// </summary>
  public Vector3D GetTangentAtDistance(Point3DCollection points, double distance)
   {
     if (points == null || points.Count < 2)
    {
     return new Vector3D(1, 0, 0); // 默认方向
 }

      double accumulatedLength = 0;

 for (int i = 1; i < points.Count; i++)
            {
  var segmentVector = points[i] - points[i - 1];
    var segmentLength = segmentVector.Length;

       if (accumulatedLength + segmentLength >= distance)
            {
     // 返回当前线段的方向
      segmentVector.Normalize();
   return segmentVector;
     }

 accumulatedLength += segmentLength;
   }

 // 返回最后一段的方向
            var lastSegment = points[points.Count - 1] - points[points.Count - 2];
          lastSegment.Normalize();
       return lastSegment;
     }
    }
}
