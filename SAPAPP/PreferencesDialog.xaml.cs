using Microsoft.Win32;
using System.Windows;

namespace SAPAPP
{

    /// <summary>
    /// Window used for maintaining and configuring settings related to the products and scripts
    /// </summary>
    public partial class PreferencesDialog : Window
    {
        MainWindow parentWindow;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentWindow"></param>
        public PreferencesDialog(MainWindow parentWindow)
        {
            InitializeComponent();
            this.parentWindow = parentWindow;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            if (ConfigTextBox.Text != "")
            {
                parentWindow.Load_Product_Configurations(ConfigTextBox.Text);
            }

            if (ATmegaPathTextBox.Text != "")
            {
                parentWindow.AVRDUDE_CLI = ATmegaPathTextBox.Text;
            }

            if (STM32PathTextBox.Text != "")
            {
                parentWindow.STM32_Programmer_CLI = STM32PathTextBox.Text;
            }


            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}