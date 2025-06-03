using Microsoft.Win32;
using System.Windows;

namespace SAPAPP
{
    public partial class PreferencesDialog : Window
    {
        MainWindow parentWindow;
        public PreferencesDialog(MainWindow parentWindow)
        {
            InitializeComponent();
            this.parentWindow = parentWindow;
            
        }

        private void BrowseLog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Log Files (*.txt)|*.txt"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                LogTextBox.Text = openFileDialog.FileName;
            }
        }
        private void BrowseConfig_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Config Files (XML-File)|*.xml",
                Title = "Open Config Settings",
            };
            if (openFileDialog.ShowDialog() == true)
            {
                ConfigTextBox.Text = openFileDialog.FileName;
            }
        }

        private void BrowseATmega_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select ATmega Programmer (atprogram.exe)",
                Filter = "Executable Files (*.exe)|*.exe",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ATmegaPathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void BrowseSTM32_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select STM32 Programmer (STM32_Programmer_CLI.exe)",
                Filter = "Executable Files (*.exe)|*.exe",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                STM32PathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {

            if (ConfigTextBox.Text != "")
            {
                parentWindow.Load_Product_Configurations(ConfigTextBox.Text);
            }

            if (STM32PathTextBox.Text != "")
            {
                parentWindow.STM32_Programmer_CLI = STM32PathTextBox.Text;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}