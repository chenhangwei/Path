using Path.Models;
using System.Collections.Generic;

namespace Path.Services
{
    /// <summary>
    /// ·�����ݷ���ӿ�
    /// </summary>
    public interface IPathDataService
    {
        /// <summary>
   /// �� XML �ļ���������
        /// </summary>
        List<StepModel> ImportFromXml(string filePath);

    /// <summary>
  /// �������ݵ� XML �ļ�
        /// </summary>
        void ExportToXml(string filePath, IEnumerable<StepModel> steps);

        /// <summary>
        /// ��֤������Ч��
     /// </summary>
bool ValidateData(IEnumerable<StepModel> steps, out string? errorMessage);
    }
}
