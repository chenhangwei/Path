using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Path.Models
{
    /// <summary>
    /// USV 数据模型（领域模型）
    /// </summary>
    public class UsvModel : INotifyPropertyChanged
    {
        private string _id = string.Empty;
 private double _x;
 private double _y;
        private double _z;
        private double _yaw;
        private double _speed;

        public string Id
        {
       get => _id;
  set => SetField(ref _id, value);
        }

        public double X
        {
      get => _x;
            set => SetField(ref _x, value);
        }

   public double Y
        {
         get => _y;
            set => SetField(ref _y, value);
    }

        public double Z
      {
            get => _z;
            set => SetField(ref _z, value);
        }

      public double Yaw
     {
            get => _yaw;
    set => SetField(ref _yaw, value);
  }

 public double Speed
        {
            get => _speed;
 set => SetField(ref _speed, value);
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
