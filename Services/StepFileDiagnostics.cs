using Path.Services.Step214;

namespace Path.Services
{
    /// <summary>
    /// STEP �ļ���Ϲ���
    /// </summary>
    public class StepFileDiagnostics
    {
     // STEP ʵ������ӳ���
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
        /// ��� STEP �ļ�����
      /// </summary>
        public static string DiagnoseStepFile(string filePath)
        {
         var report = new System.Text.StringBuilder();
 report.AppendLine("========== STEP �ļ���ϱ��� ==========");
report.AppendLine($"�ļ�: {System.IO.Path.GetFileName(filePath)}");
      report.AppendLine($"��С: {new System.IO.FileInfo(filePath).Length / 1024.0:F2} KB");
        report.AppendLine();

     try
      {
    // ��ȡ�ļ�ͷ
using var reader = new System.IO.StreamReader(filePath);
   var headerLines = new List<string>();
   for (int i = 0; i < 10 && !reader.EndOfStream; i++)
   {
    headerLines.Add(reader.ReadLine() ?? "");
   }

   report.AppendLine("�ļ�ͷ (ǰ10��):");
        foreach (var line in headerLines)
  {
     report.AppendLine($"  {line}");
    }
         report.AppendLine();

       // ���� STEP �ļ�
     var parser = new Step214Parser();
       var entities = parser.Parse(filePath);

report.AppendLine($"��ʵ����: {entities.Count}");
 report.AppendLine();

    // ͳ��ʵ�����ͣ���׼����
    var typeStats = new Dictionary<string, int>();
     var rawTypeStats = new Dictionary<string, int>();
  foreach (var entity in entities.Values)
      {
      // ԭʼ����ͳ��
       if (!rawTypeStats.ContainsKey(entity.Type))
    rawTypeStats[entity.Type] = 0;
  rawTypeStats[entity.Type]++;
    
       // ��׼������ͳ��
   var normalizedType = NormalizeEntityType(entity.Type);
  if (!typeStats.ContainsKey(normalizedType))
       typeStats[normalizedType] = 0;
      typeStats[normalizedType]++;
     }

  report.AppendLine("ʵ������ͳ�� (��׼����, ǰ20��):");
  foreach (var kvp in typeStats.OrderByDescending(x => x.Value).Take(20))
     {
        report.AppendLine($"  {kvp.Key,-40} : {kvp.Value,6}");
  }
     report.AppendLine();
       
       // �������д����ʾԭʼ����
       var hasAbbreviations = rawTypeStats.Keys.Any(k => EntityNameMap.ContainsKey(k.ToUpperInvariant()));
  if (hasAbbreviations)
  {
   report.AppendLine("��⵽��дʵ������ (ԭʼ����, ǰ20��):");
         foreach (var kvp in rawTypeStats.OrderByDescending(x => x.Value).Take(20))
   {
 var normalized = NormalizeEntityType(kvp.Key);
       var abbr = normalized != kvp.Key.ToUpperInvariant() ? $" �� {normalized}" : "";
    report.AppendLine($"  {kvp.Key,-40} : {kvp.Value,6}{abbr}");
       }
       report.AppendLine();
            }

         // �����������ʵ��
         var curveTypes = new[]
  {
   "B_SPLINE_CURVE", "BEZIER_CURVE",
       "RATIONAL_B_SPLINE_CURVE", "POLYLINE", "LINE", "CIRCLE", "ELLIPSE",
   "TRIMMED_CURVE", "COMPOSITE_CURVE", "BOUNDED_CURVE", "EDGE_CURVE",
    "SEAM_CURVE", "SURFACE_CURVE"
      };

   report.AppendLine("��������ʵ��:");
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
   report.AppendLine("  ? δ�ҵ��κ���������ʵ��");
      }
 report.AppendLine();

    // ���ҵ�ʵ�壨��׼����
   var pointCount = typeStats.ContainsKey("CARTESIAN_POINT") ? typeStats["CARTESIAN_POINT"] : 0;
                report.AppendLine($"�ѿ�������: {pointCount}");
      report.AppendLine();

  // ����
 report.AppendLine("========== ��Ͻ��� ==========");
 if (!foundCurves)
    {
 report.AppendLine("? ����: �ļ���û���ҵ�����ʵ��");
report.AppendLine();
     report.AppendLine("���ܵ�ԭ��:");
    report.AppendLine("  1. �ļ�ֻ���������ʵ��ģ��");
    report.AppendLine("  2. �ļ�ʹ���˲�ͬ��ʵ������");
        report.AppendLine("  3. ���߱�Ƕ���������ṹ��");
  report.AppendLine();
    report.AppendLine("�������:");
      report.AppendLine("  ? �� CAD ��������µ�����ѡ�� '�߿�' �� '����' ѡ��");
      report.AppendLine("  ? ʹ�� STEP AP214 ��ʽ");
     report.AppendLine("  ? ���ģ�Ͱ������棬������ȡ�߽����ߺ󵼳�");
            }
     else if (pointCount == 0)
        {
       report.AppendLine("? ����: �ļ���������ʵ�嵫û�еѿ�����");
report.AppendLine("  ����ܵ����޷���ȡ���߼�������");
    }
      else
      {
   report.AppendLine("? �ļ������������ݣ�Ӧ�ÿ�����������");
 if (hasAbbreviations)
     {
       report.AppendLine("  ע��: �ļ�ʹ������дʵ�����ƣ����Զ�����");
    }
     }

 }
     catch (Exception ex)
 {
   report.AppendLine();
  report.AppendLine($"? ��Ϲ��̳���: {ex.Message}");
            }

     report.AppendLine();
         report.AppendLine("========================================");

      return report.ToString();
      }

  /// <summary>
        /// ����ϱ��汣�浽�ļ�
      /// </summary>
  public static void SaveDiagnosticReport(string stepFilePath, string outputPath)
        {
     var report = DiagnoseStepFile(stepFilePath);
      System.IO.File.WriteAllText(outputPath, report);
   }
    }
}
