using System.Windows.Media.Media3D;

namespace Path.Services
{
    /// <summary>
    /// 曲线合并服务接口
    /// </summary>
    public interface ICurveMergeService
    {
        /// <summary>
     /// 合并相连的曲线
        /// </summary>
        /// <param name="curves">要合并的曲线集合</param>
        /// <param name="tolerance">连接点容差（默认 0.001）</param>
        /// <param name="keepOriginal">是否保留原始曲线</param>
        /// <returns>合并后的曲线集合</returns>
List<Point3DCollection> MergeConnectedCurves(
 List<Point3DCollection> curves,
    double tolerance = 0.001,
    bool keepOriginal = false);

        /// <summary>
        /// 检测哪些曲线可以合并
 /// </summary>
     /// <param name="curves">曲线集合</param>
        /// <param name="tolerance">连接点容差</param>
    /// <returns>可合并的曲线组（每组是相连的曲线索引列表）</returns>
        List<List<int>> DetectMergeableGroups(
            List<Point3DCollection> curves,
            double tolerance = 0.001);
    }
}
