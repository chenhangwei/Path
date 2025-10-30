using Path.Services.Step214;

namespace Path.Services
{
    /// <summary>
    /// STEP 文件诊断工具
    /// </summary>
    public class StepFileDiagnostics
    {
     // STEP 实体名称映射表
private static readonly Dictionary<string, string> EntityNameMap = new()
        {
         { "CRTPNT", "CARTESIAN_POINT" },
   { "CRTPT", "CARTESIAN_POINT" },
            { "CRPNT", "CARTESIAN_POINT" },
            { "BSPCRV", "B_SPLINE_CURVE" },
  { "BSCRV", "B_SPLINE_CURVE" },
      { "BZCRV", "BEZIER_CURVE" },
         { "RBSCRV", "RATIONAL_B_SPLINE_CURVE" },
            { "TRMCRV", "TRIMMED_CURVE" },
       { "CMPCRV", "COMPOSITE_CURVE" },
            { "EDGCRV", "EDGE_CURVE" },
      { "SEAMCRV", "SEAM_CURVE" },
    { "SRFCRV", "SURFACE_CURVE" },
 { "PRLYAS", "POLYLINE" },
         { "PYRCRV", "POLYLINE_CURVE" },
            { "DRCTN", "DIRECTION" },
            { "DIR", "DIRECTION" },
       { "VCT", "VECTOR" },
       { "VCTR", "VECTOR" },
     { "AX2PL3", "AXIS2_PLACEMENT_3D" },
       { "AXIS2", "AXIS2_PLACEMENT_3D" },
            { "LNMSR", "LENGTH_MEASURE" },
       { "LNMES", "LENGTH_MEASURE" }
        };
        
        private static string NormalizeEntityType(string type)
        {
       if (string.IsNullOrEmpty(type))
    return type;
        
       var upperType = type.ToUpperInvariant();
         if (EntityNameMap.TryGetValue(upperType, out var fullName))
    return fullName;
      return upperType;
   }
 
   /// <summary>
        /// 诊断 STEP 文件内容
      /// </summary>
        public static string DiagnoseStepFile(string filePath)
        {
         var report = new System.Text.StringBuilder();
 report.AppendLine("========== STEP 文件诊断报告 ==========");
report.AppendLine($"文件: {System.IO.Path.GetFileName(filePath)}");
      report.AppendLine($"大小: {new System.IO.FileInfo(filePath).Length / 1024.0:F2} KB");
        report.AppendLine();

     try
      {
    // 读取文件头
using var reader = new System.IO.StreamReader(filePath);
   var headerLines = new List<string>();
   for (int i = 0; i < 10 && !reader.EndOfStream; i++)
   {
    headerLines.Add(reader.ReadLine() ?? "");
   }

   report.AppendLine("文件头 (前10行):");
        foreach (var line in headerLines)
  {
     report.AppendLine($"  {line}");
    }
         report.AppendLine();

       // 解析 STEP 文件
     var parser = new Step214Parser();
       var entities = parser.Parse(filePath);

report.AppendLine($"总实体数: {entities.Count}");
 report.AppendLine();

    // 统计实体类型（标准化）
    var typeStats = new Dictionary<string, int>();
     var rawTypeStats = new Dictionary<string, int>();
  foreach (var entity in entities.Values)
      {
      // 原始类型统计
       if (!rawTypeStats.ContainsKey(entity.Type))
    rawTypeStats[entity.Type] = 0;
  rawTypeStats[entity.Type]++;
    
       // 标准化类型统计
   var normalizedType = NormalizeEntityType(entity.Type);
  if (!typeStats.ContainsKey(normalizedType))
       typeStats[normalizedType] = 0;
      typeStats[normalizedType]++;
     }

  report.AppendLine("实体类型统计 (标准化后, 前20种):");
  foreach (var kvp in typeStats.OrderByDescending(x => x.Value).Take(20))
     {
        report.AppendLine($"  {kvp.Key,-40} : {kvp.Value,6}");
  }
     report.AppendLine();
       
       // 如果有缩写，显示原始类型
       var hasAbbreviations = rawTypeStats.Keys.Any(k => EntityNameMap.ContainsKey(k.ToUpperInvariant()));
  if (hasAbbreviations)
  {
   report.AppendLine("检测到缩写实体名称 (原始类型, 前20种):");
         foreach (var kvp in rawTypeStats.OrderByDescending(x => x.Value).Take(20))
   {
 var normalized = NormalizeEntityType(kvp.Key);
       var abbr = normalized != kvp.Key.ToUpperInvariant() ? $" → {normalized}" : "";
    report.AppendLine($"  {kvp.Key,-40} : {kvp.Value,6}{abbr}");
       }
       report.AppendLine();
            }

         // 查找曲线相关实体
         var curveTypes = new[]
  {
   "B_SPLINE_CURVE", "BEZIER_CURVE",
       "RATIONAL_B_SPLINE_CURVE", "POLYLINE", "LINE", "CIRCLE", "ELLIPSE",
   "TRIMMED_CURVE", "COMPOSITE_CURVE", "BOUNDED_CURVE", "EDGE_CURVE",
    "SEAM_CURVE", "SURFACE_CURVE"
      };

   report.AppendLine("曲线类型实体:");
     var foundCurves = false;
       foreach (var curveType in curveTypes)
      {
    var count = typeStats.ContainsKey(curveType) ? typeStats[curveType] : 0;
  if (count > 0)
  {
   report.AppendLine($"  ? {curveType,-40} : {count,6}");
    foundCurves = true;
  }
      }

if (!foundCurves)
          {
   report.AppendLine("  ? 未找到任何曲线类型实体");
      }
 report.AppendLine();

    // 查找点实体（标准化）
   var pointCount = typeStats.ContainsKey("CARTESIAN_POINT") ? typeStats["CARTESIAN_POINT"] : 0;
                report.AppendLine($"笛卡尔点数: {pointCount}");
      report.AppendLine();

  // 建议
 report.AppendLine("========== 诊断建议 ==========");
 if (!foundCurves)
    {
 report.AppendLine("? 警告: 文件中没有找到曲线实体");
report.AppendLine();
     report.AppendLine("可能的原因:");
    report.AppendLine("  1. 文件只包含曲面或实体模型");
    report.AppendLine("  2. 文件使用了不同的实体名称");
        report.AppendLine("  3. 曲线被嵌入在其他结构中");
  report.AppendLine();
    report.AppendLine("解决建议:");
      report.AppendLine("  ? 在 CAD 软件中重新导出，选择 '线框' 或 '曲线' 选项");
      report.AppendLine("  ? 使用 STEP AP214 格式");
     report.AppendLine("  ? 如果模型包含曲面，尝试提取边界曲线后导出");
            }
     else if (pointCount == 0)
        {
       report.AppendLine("? 警告: 文件中有曲线实体但没有笛卡尔点");
report.AppendLine("  这可能导致无法提取曲线几何数据");
    }
      else
      {
   report.AppendLine("? 文件包含曲线数据，应该可以正常导入");
 if (hasAbbreviations)
     {
       report.AppendLine("  注意: 文件使用了缩写实体名称（已自动处理）");
    }
     }

 }
     catch (Exception ex)
 {
   report.AppendLine();
  report.AppendLine($"? 诊断过程出错: {ex.Message}");
            }

     report.AppendLine();
         report.AppendLine("========================================");

      return report.ToString();
      }

  /// <summary>
        /// 将诊断报告保存到文件
      /// </summary>
  public static void SaveDiagnosticReport(string stepFilePath, string outputPath)
        {
     var report = DiagnoseStepFile(stepFilePath);
      System.IO.File.WriteAllText(outputPath, report);
   }
    }
}
