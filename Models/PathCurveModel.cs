using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Media3D;

namespace Path.Models
{
    /// <summary>
    /// ·������ģ�ͣ������ STEP �ļ����������
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
        /// �������ƣ��� usv_01, usv_02��
   /// </summary>
        public string Name
        {
   get => _name;
            set => SetField(ref _name, value);
        }

        /// <summary>
        /// �Ƿ�ѡ��
        /// </summary>
      public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

 /// <summary>
        /// ����������
        /// </summary>
        public int LoftPointCount
        {
   get => _loftPointCount;
         set => SetField(ref _loftPointCount, value);
        }

        /// <summary>
        /// �Ƿ��Ѿ�����
        /// </summary>
   public bool IsLofted
  {
     get => _isLofted;
            set => SetField(ref _isLofted, value);
        }

  /// <summary>
        /// ԭʼ����ĵ㼯��
        /// </summary>
        public Point3DCollection OriginalPoints
   {
        get => _originalPoints;
            set => SetField(ref _originalPoints, value);
        }

    /// <summary>
        /// ������ĵ㼯�ϣ��Ⱦ������
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
