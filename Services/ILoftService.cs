using System.Windows.Media.Media3D;

namespace Path.Services
{
    /// <summary>
    /// 曲线放样服务，生成等距采样点
 /// </summary>
    public interface ILoftService
 {
      /// <summary>
        /// 对曲线进行放样，生成等距分布的控制点
        /// </summary>
     /// <param name="originalPoints">原始点集</param>
   /// <param name="pointCount">目标点数量</param>
        /// <returns>等距分布的点集</returns>
    Point3DCollection LoftCurve(Point3DCollection originalPoints, int pointCount);

 /// <summary>
        /// 计算曲线总长度
   /// </summary>
        double CalculateCurveLength(Point3DCollection points);

 /// <summary>
     /// 在曲线上按指定距离获取点
        /// </summary>
   Point3D GetPointAtDistance(Point3DCollection points, double distance);
    }
}
