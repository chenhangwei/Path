using System.IO;
using System.Windows.Media.Media3D;
using Path.Services.Step214;

namespace Path.Services
{
    /// <summary>
    /// STEP 文件导入服务实现
    /// 支持 STEP 214 (AP214) 格式
    /// </summary>
    public class StepImportService : IStepImportService
  {
  public List<Point3DCollection> ImportStepFile(string filePath)
        {
 if (!File.Exists(filePath))
        {
   throw new FileNotFoundException($"文件不存在: {filePath}");
 }

 if (!ValidateStepFile(filePath))
     {
  throw new InvalidDataException("不是有效的 STEP 文件格式。请确保文件是 STEP 214 (AP214) 或 ISO-10303-21 格式。");
}

     var curves = new List<Point3DCollection>();

   try
      {
    // 使用 STEP 214 解析器
       var parser = new Step214Parser();
       var extractor = new Step214CurveExtractor(parser);
 
     // 提取所有曲线
          curves = extractor.ExtractCurves(filePath);
      
  // 如果没有提取到曲线，抛出异常而不是使用测试数据
      if (curves.Count == 0)
  {
throw new InvalidOperationException(
  "未从 STEP 文件中提取到曲线数据。\n\n" +
          "可能的原因：\n" +
        "1. 文件中只包含曲面或实体，没有曲线\n" +
        "2. 曲线类型不支持\n" +
          "3. 文件格式不标准\n\n" +
          "建议：\n" +
       "? 在 CAD 软件中导出时选择 '线框' 或 '曲线' 选项\n" +
    "? 使用 STEP AP214 格式\n" +
         "? 确保文件包含可见的曲线几何");
         }
     
System.Diagnostics.Debug.WriteLine($"成功从 STEP 文件提取 {curves.Count} 条曲线");
   }
        catch (InvalidOperationException)
        {
            // 重新抛出我们自己的异常
  throw;
        }
   catch (Exception ex)
            {
      throw new InvalidOperationException($"解析 STEP 文件失败: {ex.Message}\n\n请确保文件是有效的 STEP 214 格式。", ex);
       }

  return curves;
        }

    public bool ValidateStepFile(string filePath)
 {
         try
        {
       if (!File.Exists(filePath))
   {
  return false;
}

  var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
if (extension != ".step" && extension != ".stp")
     {
 return false;
  }

  // 读取文件头验证 STEP 格式
    using var reader = new StreamReader(filePath);
     var firstLine = reader.ReadLine();
       
 // STEP 文件通常以 ISO-10303 开头
     return firstLine?.Contains("ISO-10303") == true || 
    firstLine?.Contains("STEP") == true ||
      firstLine?.StartsWith("ISO-10303-21") == true;
    }
            catch
   {
     return false;
  }
     }
    }
}
