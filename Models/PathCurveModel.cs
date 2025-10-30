using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Media3D;

namespace Path.Models
{
    /// <summary>
    /// 路径曲线模型，代表从 STEP 文件导入的线条
    /// </summary>
    public class PathCurveModel : INotifyPropertyChanged
  {
      private string _name = string.Empty;
      private bool _isSelected;
        private int _loftPointCount = 10;
        private bool _isLofted;
        private Point3DCollection _originalPoints = new();
  private Point3DCollection _loftedPoints = new();

        /// <summary>
        /// 曲线名称（如 usv_01, usv_02）
   /// </summary>
        public string Name
        {
   get => _name;
            set => SetField(ref _name, value);
        }

        /// <summary>
        /// 是否被选中
        /// </summary>
      public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

 /// <summary>
        /// 放样点数量
        /// </summary>
        public int LoftPointCount
        {
   get => _loftPointCount;
         set => SetField(ref _loftPointCount, value);
        }

        /// <summary>
        /// 是否已经放样
        /// </summary>
   public bool IsLofted
  {
     get => _isLofted;
            set => SetField(ref _isLofted, value);
        }

  /// <summary>
        /// 原始导入的点集合
        /// </summary>
        public Point3DCollection OriginalPoints
   {
        get => _originalPoints;
            set => SetField(ref _originalPoints, value);
        }

    /// <summary>
        /// 放样后的点集合（等距采样）
 /// </summary>
        public Point3DCollection LoftedPoints
        {
      get => _loftedPoints;
     set => SetField(ref _loftedPoints, value);
        }

      public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

     protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
     OnPropertyChanged(propertyName);
      return true;
        }
    }
}
