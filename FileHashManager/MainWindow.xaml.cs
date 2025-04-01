using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace FileHashManager
{
    // 数据模型类
    public class FileItem : INotifyPropertyChanged
    {
        private Brush _backgroundColor = Brushes.Transparent;
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public string Md5Hash { get; set; } = string.Empty;
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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<FileItem> _pendingFileList = [];
        private readonly ObservableCollection<FileItem> _processedFileList = [];
        private readonly BackgroundColor _backgroundColor = new();

        public MainWindow()
        {
            InitializeComponent();

            // 初始化数据绑定
            PendingFilesListView.ItemsSource = _pendingFileList;
            ProcessedFilesListView.ItemsSource = _processedFileList;

            // 订阅集合变化事件，更新状态栏显示
            _pendingFileList.CollectionChanged += (sender, e) => PendingStatusTextBlock.Text = $"处理中：{_pendingFileList.Count} 个";
            _processedFileList.CollectionChanged += (sender, e) => ProcessedStatusTextBlock.Text = $"已处理：{_processedFileList.Count} 个";
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var items = (string[])e.Data.GetData(DataFormats.FileDrop);
                var allFiles = GetAllFilePaths(items);
                foreach (var file in allFiles)
                {
                    ProcessFile(file);
                }
            }
        }

        private async void ProcessFile(string filePath)
        {
            if (_pendingFileList.Any(f => f.FilePath == filePath) || _processedFileList.Any(f => f.FilePath == filePath)) return;
            FileItem fileItem = new()
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath
            };

            _pendingFileList.Add(fileItem);
            fileItem.Md5Hash = BitConverter.ToString(await Task.Run(() => HashCalculator.ComputeFileMd5Async(filePath))).Replace("-", "");
            _pendingFileList.Remove(fileItem);

            var existingItem = _processedFileList.FirstOrDefault(f => f.Md5Hash == fileItem.Md5Hash);
            if (existingItem != null)
            {
                if (existingItem.BackgroundColor == Brushes.Transparent)
                {
                    var newColor = _backgroundColor.GetNextColor();
                    existingItem.BackgroundColor = newColor;
                    fileItem.BackgroundColor = newColor;
                }
                else
                {
                    fileItem.BackgroundColor = existingItem.BackgroundColor;
                }
            }

            _processedFileList.Add(fileItem);
        }

        /// <summary>
        /// 获取文件和文件夹混合的路径列表中所有的文件路径
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        private static List<string> GetAllFilePaths(IEnumerable<string> paths)
        {
            var filePaths = new List<string>();
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    filePaths.Add(path);
                }
                else if (Directory.Exists(path))
                {
                    // 递归获取所有文件（包括子目录）
                    filePaths.AddRange(Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories));
                }
            }
            return filePaths;
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // 创建并显示关于窗口
            AboutWindow aboutWindow = new()
            {
                Owner = this
            };
            aboutWindow.ShowDialog();
        }
    }
}