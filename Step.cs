using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Path
{
    public class Step : INotifyPropertyChanged
    {
        private int _number;
        public int Number
        {
            get => _number;
            set
            {
                if (_number != value)
                {
                    _number = value;
                    OnPropertyChanged(nameof(Number));
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public string DisplayName { get; }

        public ObservableCollection<USV> USVs { get; set; }

        public Step(string displayName)
        {
            DisplayName = displayName;
            USVs = new ObservableCollection<USV>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
