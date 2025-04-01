using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace FileHashManager
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
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
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
