using System.Windows.Media.Media3D;

namespace Path.Services
{
    /// <summary>
    /// STEP 文件导入服务接口
    /// </summary>
  public interface IStepImportService
    {
        /// <summary>
        /// 导入 STEP 文件并提取曲线
 /// </summary>
        /// <param name="filePath">STEP 文件路径</param>
        /// <returns>曲线点集合的列表</returns>
      List<Point3DCollection> ImportStepFile(string filePath);

        /// <summary>
        /// 验证文件是否为有效的 STEP 格式
    /// </summary>
 bool ValidateStepFile(string filePath);
    }
}
