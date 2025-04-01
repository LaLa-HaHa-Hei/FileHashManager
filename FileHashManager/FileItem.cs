using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace FileHashManager
{
    public class FileItem : INotifyPropertyChanged
    {
        private Brush _backgroundColor = Brushes.Transparent;
        private bool _isChecked = false;

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public string Md5Hash { get; set; } = string.Empty;
        public TimeSpan ProcessingTime { get; set; }
        public Brush BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
        {
            if (Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
