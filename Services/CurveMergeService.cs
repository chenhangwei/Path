using System.Windows.Media.Media3D;

namespace Path.Services
{
    /// <summary>
    /// 曲线合并服务实现
    /// </summary>
    public class CurveMergeService : ICurveMergeService
    {
        /// <summary>
        /// 合并相连的曲线
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

System.Diagnostics.Debug.WriteLine("========== 开始合并曲线 ==========");
  System.Diagnostics.Debug.WriteLine($"输入曲线数: {curves.Count}");
    System.Diagnostics.Debug.WriteLine($"连接容差: {tolerance}");

 for (int i = 0; i < curves.Count; i++)
      {
           if (used.Contains(i))
       continue;

 // 开始新的合并组
  var current = new Point3DCollection(curves[i]);
             used.Add(i);
         var group = new List<int> { i };

 System.Diagnostics.Debug.WriteLine($"\n开始合并组，起始曲线: {i}");

  // 持续查找可以连接的曲线
    bool found;
   do
            {
   found = false;
          for (int j = 0; j < curves.Count; j++)
  {
     if (used.Contains(j))
        continue;

    var candidate = curves[j];

            // 检查 4 种连接可能性
       ConnectionType connection = CheckConnection(current, candidate, tolerance);

   if (connection != ConnectionType.None)
   {
          // 连接曲线
   ConnectCurves(current, candidate, connection);
         used.Add(j);
   group.Add(j);
          found = true;

 System.Diagnostics.Debug.WriteLine($"  连接曲线 {j}，连接类型: {connection}，当前点数: {current.Count}");
        }
            }
       } while (found);

      System.Diagnostics.Debug.WriteLine($"合并组完成，包含曲线: [{string.Join(", ", group)}]，总点数: {current.Count}");
      result.Add(current);
 }

         System.Diagnostics.Debug.WriteLine($"\n========== 合并完成 ==========");
   System.Diagnostics.Debug.WriteLine($"原始曲线数: {curves.Count}");
    System.Diagnostics.Debug.WriteLine($"合并后曲线数: {result.Count}");
        System.Diagnostics.Debug.WriteLine($"合并了 {curves.Count - result.Count} 条曲线");

    // 如果需要保留原始曲线
   if (keepOriginal && result.Count < curves.Count)
        {
         // 添加未被合并的原始曲线
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
    /// 检测可合并的曲线组
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

       // 查找所有与当前组相连的曲线
      bool found;
        do
                {
          found = false;
        for (int j = 0; j < curves.Count; j++)
       {
        if (used.Contains(j))
           continue;

          // 检查是否与组中任何曲线相连
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

  return groups.Where(g => g.Count > 1).ToList(); // 只返回有多条曲线的组
        }

        /// <summary>
        /// 连接类型
   /// </summary>
 private enum ConnectionType
  {
            None,// 不连接
     EndToStart,     // current 的末尾连接 candidate 的开头
  EndToEnd,     // current 的末尾连接 candidate 的末尾
            StartToStart,   // current 的开头连接 candidate 的开头
  StartToEnd      // current 的开头连接 candidate 的末尾
        }

   /// <summary>
        /// 检查两条曲线的连接类型
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

   // 优先检查末尾到开头（最常见）
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
    /// 连接两条曲线
   /// </summary>
        private void ConnectCurves(
     Point3DCollection current,
            Point3DCollection candidate,
  ConnectionType connection)
        {
         switch (connection)
            {
            case ConnectionType.EndToStart:
       // current 末尾连接 candidate 开头
     // 添加 candidate 的所有点（跳过第一个点，因为与 current 末尾重合）
        foreach (var pt in candidate.Skip(1))
            {
      current.Add(pt);
   }
   break;

     case ConnectionType.EndToEnd:
         // current 末尾连接 candidate 末尾
         // 添加 candidate 的反向点（跳过最后一个点）
         for (int i = candidate.Count - 2; i >= 0; i--)
   {
          current.Add(candidate[i]);
     }
         break;

                case ConnectionType.StartToStart:
    // current 开头连接 candidate 开头
      // 在 current 前面插入 candidate 的反向点（跳过第一个点）
   for (int i = 1; i < candidate.Count; i++)
            {
  current.Insert(0, candidate[i]);
             }
    break;

   case ConnectionType.StartToEnd:
         // current 开头连接 candidate 末尾
           // 在 current 前面插入 candidate 的所有点（跳过最后一个点）
           for (int i = candidate.Count - 2; i >= 0; i--)
      {
    current.Insert(0, candidate[i]);
            }
               break;
        }
   }

 /// <summary>
     /// 检查两条曲线是否相连
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
   /// 检查两个点是否接近
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
