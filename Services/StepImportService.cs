using System.IO;
using System.Windows.Media.Media3D;
using Path.Services.Step214;

namespace Path.Services
{
    /// <summary>
    /// STEP �ļ��������ʵ��
    /// ֧�� STEP 214 (AP214) ��ʽ
    /// </summary>
    public class StepImportService : IStepImportService
  {
  public List<Point3DCollection> ImportStepFile(string filePath)
        {
 if (!File.Exists(filePath))
        {
   throw new FileNotFoundException($"�ļ�������: {filePath}");
 }

 if (!ValidateStepFile(filePath))
     {
  throw new InvalidDataException("������Ч�� STEP �ļ���ʽ����ȷ���ļ��� STEP 214 (AP214) �� ISO-10303-21 ��ʽ��");
}

     var curves = new List<Point3DCollection>();

   try
      {
    // ʹ�� STEP 214 ������
       var parser = new Step214Parser();
       var extractor = new Step214CurveExtractor(parser);
 
     // ��ȡ��������
          curves = extractor.ExtractCurves(filePath);
      
  // ���û����ȡ�����ߣ��׳��쳣������ʹ�ò�������
      if (curves.Count == 0)
  {
throw new InvalidOperationException(
  "δ�� STEP �ļ�����ȡ���������ݡ�\n\n" +
          "���ܵ�ԭ��\n" +
        "1. �ļ���ֻ���������ʵ�壬û������\n" +
        "2. �������Ͳ�֧��\n" +
          "3. �ļ���ʽ����׼\n\n" +
          "���飺\n" +
       "? �� CAD ����е���ʱѡ�� '�߿�' �� '����' ѡ��\n" +
    "? ʹ�� STEP AP214 ��ʽ\n" +
         "? ȷ���ļ������ɼ������߼���");
         }
     
System.Diagnostics.Debug.WriteLine($"�ɹ��� STEP �ļ���ȡ {curves.Count} ������");
   }
        catch (InvalidOperationException)
        {
            // �����׳������Լ����쳣
  throw;
        }
   catch (Exception ex)
            {
      throw new InvalidOperationException($"���� STEP �ļ�ʧ��: {ex.Message}\n\n��ȷ���ļ�����Ч�� STEP 214 ��ʽ��", ex);
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

  // ��ȡ�ļ�ͷ��֤ STEP ��ʽ
    using var reader = new StreamReader(filePath);
     var firstLine = reader.ReadLine();
       
 // STEP �ļ�ͨ���� ISO-10303 ��ͷ
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
