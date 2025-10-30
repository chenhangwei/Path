using System.Windows.Media.Media3D;

namespace Path.Services
{
    /// <summary>
    /// STEP �ļ��������ӿ�
    /// </summary>
  public interface IStepImportService
    {
        /// <summary>
        /// ���� STEP �ļ�����ȡ����
 /// </summary>
        /// <param name="filePath">STEP �ļ�·��</param>
        /// <returns>���ߵ㼯�ϵ��б�</returns>
      List<Point3DCollection> ImportStepFile(string filePath);

        /// <summary>
        /// ��֤�ļ��Ƿ�Ϊ��Ч�� STEP ��ʽ
    /// </summary>
 bool ValidateStepFile(string filePath);
    }
}
