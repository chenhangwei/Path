using System.Windows.Media.Media3D;

namespace Path.Services
{
    /// <summary>
    /// ���߷�������ʵ��
    /// </summary>
    public class LoftService : ILoftService
    {
        public Point3DCollection LoftCurve(Point3DCollection originalPoints, int pointCount)
        {
            if (originalPoints == null || originalPoints.Count < 2)
       {
        throw new ArgumentException("ԭʼ�㼯������Ҫ 2 ����");
     }

 if (pointCount < 2)
            {
         throw new ArgumentException("��������������Ϊ 2");
   }

            var loftedPoints = new Point3DCollection();
  var totalLength = CalculateCurveLength(originalPoints);

            if (totalLength == 0)
   {
 // ������߳���Ϊ 0�����ص�һ����ĸ���
     for (int i = 0; i < pointCount; i++)
          {
        loftedPoints.Add(originalPoints[0]);
   }
    return loftedPoints;
    }

    // ����Ⱦ���
            var interval = totalLength / (pointCount - 1);

  // ���ɵȾ��
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
     throw new ArgumentException("�㼯����Ϊ��");
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
         // �ڵ�ǰ�߶���
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

   // ������볬�����߳��ȣ��������һ����
       return points[points.Count - 1];
  }

        /// <summary>
   /// ������������ϵ����߷������ڼ��� Yaw �Ƕȣ�
   /// </summary>
  public Vector3D GetTangentAtDistance(Point3DCollection points, double distance)
   {
     if (points == null || points.Count < 2)
    {
     return new Vector3D(1, 0, 0); // Ĭ�Ϸ���
 }

      double accumulatedLength = 0;

 for (int i = 1; i < points.Count; i++)
            {
  var segmentVector = points[i] - points[i - 1];
    var segmentLength = segmentVector.Length;

       if (accumulatedLength + segmentLength >= distance)
            {
     // ���ص�ǰ�߶εķ���
      segmentVector.Normalize();
   return segmentVector;
     }

 accumulatedLength += segmentLength;
   }

 // �������һ�εķ���
            var lastSegment = points[points.Count - 1] - points[points.Count - 2];
          lastSegment.Normalize();
       return lastSegment;
     }
    }
}
