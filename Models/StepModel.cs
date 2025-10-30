using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Path.Models
{
    /// <summary>
    /// Step 数据模型（领域模型）
    /// </summary>
    public class StepModel : INotifyPropertyChanged
  {
        private int _number;
        private string _displayName = string.Empty;

  public int Number
      {
            get => _number;
    set => SetField(ref _number, value);
        }

        public string DisplayName
        {
            get => _displayName;
set => SetField(ref _displayName, value);
        }

        public ObservableCollection<UsvModel> Usvs { get; } = new();

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
