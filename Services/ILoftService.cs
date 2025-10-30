using System.Windows.Media.Media3D;

namespace Path.Services
{
    /// <summary>
    /// ���߷����������ɵȾ������
 /// </summary>
    public interface ILoftService
 {
      /// <summary>
        /// �����߽��з��������ɵȾ�ֲ��Ŀ��Ƶ�
        /// </summary>
     /// <param name="originalPoints">ԭʼ�㼯</param>
   /// <param name="pointCount">Ŀ�������</param>
        /// <returns>�Ⱦ�ֲ��ĵ㼯</returns>
    Point3DCollection LoftCurve(Point3DCollection originalPoints, int pointCount);

 /// <summary>
        /// ���������ܳ���
   /// </summary>
        double CalculateCurveLength(Point3DCollection points);

 /// <summary>
     /// �������ϰ�ָ�������ȡ��
        /// </summary>
   Point3D GetPointAtDistance(Point3DCollection points, double distance);
    }
}
