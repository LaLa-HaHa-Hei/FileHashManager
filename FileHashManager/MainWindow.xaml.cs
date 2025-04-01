using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace FileHashManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;
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

            // 点击表头事件
            ProcessedFilesListView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
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
            var stopwatch = Stopwatch.StartNew(); // 开始计时
            fileItem.Md5Hash = BitConverter.ToString(await Task.Run(() => HashCalculator.ComputeFileMd5Async(filePath))).Replace("-", "").ToLower();
            stopwatch.Stop(); // 停止计时
            fileItem.ProcessingTime = stopwatch.Elapsed; // 记录花费的时间
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

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink link)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = link.NavigateUri.AbsoluteUri,
                    UseShellExecute = true
                });
            }
        }

        private void RemoveCurrentFileFromList_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessedFilesListView.SelectedItem is FileItem selectedItem)
            {
                _processedFileList.Remove(selectedItem);
            }
        }

        private void RemoveCheckedFilesFromList_Click(object sender, RoutedEventArgs e)
        {
            var checkedItems = _processedFileList.Where(item => item.IsChecked).ToList();
            foreach (var item in checkedItems)
            {
                _processedFileList.Remove(item);
            }
        }

        private void DeleteCurrentFileFromLocal_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessedFilesListView.SelectedItem is FileItem selectedItem)
            {
                var result = MessageBox.Show($"确定把 {selectedItem.FileName} 放入回收站？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                    return;
                try
                {
                    SendFileToRecycleBin(selectedItem.FilePath);
                    _processedFileList.Remove(selectedItem);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"删除文件失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteCheckedFilesFromLocal_Click(object sender, RoutedEventArgs e)
        {
            var checkedItems = _processedFileList.Where(item => item.IsChecked).ToList();
            if (checkedItems.Count > 0)
            {
                var result = MessageBox.Show($"确定把 {checkedItems.Count} 个文件放入回收站？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                    return;
                foreach (var item in checkedItems)
                {
                    try
                    {
                        SendFileToRecycleBin(item.FilePath);
                        _processedFileList.Remove(item);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show($"删除文件失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("没有选中任何文件", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RemoveDuplicateFiles_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"对于相同哈希的文件将只保留一个，其他将被移入回收站，确认去重吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
            var groupedFiles = _processedFileList.GroupBy(f => f.Md5Hash).Where(g => g.Count() > 1).ToList();
            foreach (var group in groupedFiles)
            {
                var filesToRemove = group.Skip(1).ToList();
                foreach (var file in filesToRemove)
                {
                    try
                    {
                        SendFileToRecycleBin(file.FilePath);
                        _processedFileList.Remove(file);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show($"删除文件失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private static void SendFileToRecycleBin(string filePath) =>
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(filePath, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader headerClicked)
            {
                var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                var sortBy = columnBinding?.Path.Path;
                if (string.IsNullOrEmpty(sortBy))
                    return;
                var direction = _lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                _lastDirection = direction;
                var collectionView = CollectionViewSource.GetDefaultView(_processedFileList);
                collectionView.SortDescriptions.Clear();
                collectionView.SortDescriptions.Add(new SortDescription(sortBy, direction));
            }
        }
    }
}