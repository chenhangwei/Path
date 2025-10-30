using System.Windows.Media.Media3D;

namespace Path.Services
{
    /// <summary>
    /// ���ߺϲ�����ʵ��
    /// </summary>
    public class CurveMergeService : ICurveMergeService
    {
        /// <summary>
        /// �ϲ�����������
 /// </summary>
        public List<Point3DCollection> MergeConnectedCurves(
   List<Point3DCollection> curves,
  double tolerance = 0.001,
            bool keepOriginal = false)
        {
            if (curves == null || curves.Count == 0)
                return new List<Point3DCollection>();

            var result = new List<Point3DCollection>();
   var used = new HashSet<int>();

System.Diagnostics.Debug.WriteLine("========== ��ʼ�ϲ����� ==========");
  System.Diagnostics.Debug.WriteLine($"����������: {curves.Count}");
    System.Diagnostics.Debug.WriteLine($"�����ݲ�: {tolerance}");

 for (int i = 0; i < curves.Count; i++)
      {
           if (used.Contains(i))
       continue;

 // ��ʼ�µĺϲ���
  var current = new Point3DCollection(curves[i]);
             used.Add(i);
         var group = new List<int> { i };

 System.Diagnostics.Debug.WriteLine($"\n��ʼ�ϲ��飬��ʼ����: {i}");

  // �������ҿ������ӵ�����
    bool found;
   do
            {
   found = false;
          for (int j = 0; j < curves.Count; j++)
  {
     if (used.Contains(j))
        continue;

    var candidate = curves[j];

            // ��� 4 �����ӿ�����
       ConnectionType connection = CheckConnection(current, candidate, tolerance);

   if (connection != ConnectionType.None)
   {
          // ��������
   ConnectCurves(current, candidate, connection);
         used.Add(j);
   group.Add(j);
          found = true;

 System.Diagnostics.Debug.WriteLine($"  �������� {j}����������: {connection}����ǰ����: {current.Count}");
        }
            }
       } while (found);

      System.Diagnostics.Debug.WriteLine($"�ϲ�����ɣ���������: [{string.Join(", ", group)}]���ܵ���: {current.Count}");
      result.Add(current);
 }

         System.Diagnostics.Debug.WriteLine($"\n========== �ϲ���� ==========");
   System.Diagnostics.Debug.WriteLine($"ԭʼ������: {curves.Count}");
    System.Diagnostics.Debug.WriteLine($"�ϲ���������: {result.Count}");
        System.Diagnostics.Debug.WriteLine($"�ϲ��� {curves.Count - result.Count} ������");

    // �����Ҫ����ԭʼ����
   if (keepOriginal && result.Count < curves.Count)
        {
         // ���δ���ϲ���ԭʼ����
      for (int i = 0; i < curves.Count; i++)
      {
              if (!used.Contains(i))
        {
result.Add(curves[i]);
  }
                }
            }

            return result;
    }

        /// <summary>
    /// ���ɺϲ���������
        /// </summary>
   public List<List<int>> DetectMergeableGroups(
            List<Point3DCollection> curves,
    double tolerance = 0.001)
   {
        var groups = new List<List<int>>();
       var used = new HashSet<int>();

      for (int i = 0; i < curves.Count; i++)
            {
                if (used.Contains(i))
        continue;

                var group = new List<int> { i };
   used.Add(i);

       // ���������뵱ǰ������������
      bool found;
        do
                {
          found = false;
        for (int j = 0; j < curves.Count; j++)
       {
        if (used.Contains(j))
           continue;

          // ����Ƿ��������κ���������
  bool connected = false;
          foreach (var groupIndex in group)
            {
     if (IsConnected(curves[groupIndex], curves[j], tolerance))
 {
     connected = true;
    break;
   }
  }

   if (connected)
            {
    group.Add(j);
              used.Add(j);
        found = true;
  }
       }
       } while (found);

    groups.Add(group);
     }

  return groups.Where(g => g.Count > 1).ToList(); // ֻ�����ж������ߵ���
        }

        /// <summary>
        /// ��������
   /// </summary>
 private enum ConnectionType
  {
            None,// ������
     EndToStart,     // current ��ĩβ���� candidate �Ŀ�ͷ
  EndToEnd,     // current ��ĩβ���� candidate ��ĩβ
            StartToStart,   // current �Ŀ�ͷ���� candidate �Ŀ�ͷ
  StartToEnd      // current �Ŀ�ͷ���� candidate ��ĩβ
        }

   /// <summary>
        /// ����������ߵ���������
   /// </summary>
    private ConnectionType CheckConnection(
        Point3DCollection curve1,
Point3DCollection curve2,
   double tolerance)
  {
  if (curve1.Count == 0 || curve2.Count == 0)
     return ConnectionType.None;

  var c1Start = curve1.First();
       var c1End = curve1.Last();
    var c2Start = curve2.First();
     var c2End = curve2.Last();

   // ���ȼ��ĩβ����ͷ�������
 if (IsPointsClose(c1End, c2Start, tolerance))
         return ConnectionType.EndToStart;

            if (IsPointsClose(c1End, c2End, tolerance))
                return ConnectionType.EndToEnd;

       if (IsPointsClose(c1Start, c2Start, tolerance))
    return ConnectionType.StartToStart;

      if (IsPointsClose(c1Start, c2End, tolerance))
 return ConnectionType.StartToEnd;

      return ConnectionType.None;
 }

        /// <summary>
    /// ������������
   /// </summary>
        private void ConnectCurves(
     Point3DCollection current,
            Point3DCollection candidate,
  ConnectionType connection)
        {
         switch (connection)
            {
            case ConnectionType.EndToStart:
       // current ĩβ���� candidate ��ͷ
     // ��� candidate �����е㣨������һ���㣬��Ϊ�� current ĩβ�غϣ�
        foreach (var pt in candidate.Skip(1))
            {
      current.Add(pt);
   }
   break;

     case ConnectionType.EndToEnd:
         // current ĩβ���� candidate ĩβ
         // ��� candidate �ķ���㣨�������һ���㣩
         for (int i = candidate.Count - 2; i >= 0; i--)
   {
          current.Add(candidate[i]);
     }
         break;

                case ConnectionType.StartToStart:
    // current ��ͷ���� candidate ��ͷ
      // �� current ǰ����� candidate �ķ���㣨������һ���㣩
   for (int i = 1; i < candidate.Count; i++)
            {
  current.Insert(0, candidate[i]);
             }
    break;

   case ConnectionType.StartToEnd:
         // current ��ͷ���� candidate ĩβ
           // �� current ǰ����� candidate �����е㣨�������һ���㣩
           for (int i = candidate.Count - 2; i >= 0; i--)
      {
    current.Insert(0, candidate[i]);
            }
               break;
        }
   }

 /// <summary>
     /// ������������Ƿ�����
   /// </summary>
   private bool IsConnected(
            Point3DCollection curve1,
        Point3DCollection curve2,
       double tolerance)
        {
  if (curve1.Count == 0 || curve2.Count == 0)
         return false;

       var c1Start = curve1.First();
            var c1End = curve1.Last();
        var c2Start = curve2.First();
        var c2End = curve2.Last();

 return IsPointsClose(c1End, c2Start, tolerance) ||
          IsPointsClose(c1End, c2End, tolerance) ||
        IsPointsClose(c1Start, c2Start, tolerance) ||
   IsPointsClose(c1Start, c2End, tolerance);
        }

    /// <summary>
   /// ����������Ƿ�ӽ�
     /// </summary>
        private bool IsPointsClose(Point3D p1, Point3D p2, double tolerance)
        {
   var dx = p1.X - p2.X;
  var dy = p1.Y - p2.Y;
        var dz = p1.Z - p2.Z;
   var distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            return distance < tolerance;
     }
    }
}
