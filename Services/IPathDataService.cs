using Path.Models;
using System.Collections.Generic;

namespace Path.Services
{
    /// <summary>
    /// 路径数据服务接口
    /// </summary>
    public interface IPathDataService
    {
        /// <summary>
   /// 从 XML 文件导入数据
        /// </summary>
        List<StepModel> ImportFromXml(string filePath);

    /// <summary>
  /// 导出数据到 XML 文件
        /// </summary>
        void ExportToXml(string filePath, IEnumerable<StepModel> steps);

        /// <summary>
        /// 验证数据有效性
     /// </summary>
bool ValidateData(IEnumerable<StepModel> steps, out string? errorMessage);
    }
}
