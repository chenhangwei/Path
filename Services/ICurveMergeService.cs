using System.Windows.Media.Media3D;

namespace Path.Services
{
    /// <summary>
    /// ���ߺϲ�����ӿ�
    /// </summary>
    public interface ICurveMergeService
    {
        /// <summary>
     /// �ϲ�����������
        /// </summary>
        /// <param name="curves">Ҫ�ϲ������߼���</param>
        /// <param name="tolerance">���ӵ��ݲĬ�� 0.001��</param>
        /// <param name="keepOriginal">�Ƿ���ԭʼ����</param>
        /// <returns>�ϲ�������߼���</returns>
List<Point3DCollection> MergeConnectedCurves(
 List<Point3DCollection> curves,
    double tolerance = 0.001,
    bool keepOriginal = false);

        /// <summary>
        /// �����Щ���߿��Ժϲ�
 /// </summary>
     /// <param name="curves">���߼���</param>
        /// <param name="tolerance">���ӵ��ݲ�</param>
    /// <returns>�ɺϲ��������飨ÿ�������������������б�</returns>
        List<List<int>> DetectMergeableGroups(
            List<Point3DCollection> curves,
            double tolerance = 0.001);
    }
}
