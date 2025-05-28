using System.Diagnostics;
using System.Windows;

namespace SAPAPP
{
    public partial class WikiDialog : Window
    {
        public WikiDialog()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenWiki_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/SensitTechnologies/TestSuite/wiki",
                UseShellExecute = true
            });
        }
    }
}