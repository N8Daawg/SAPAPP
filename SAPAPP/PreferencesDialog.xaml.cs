using System;
using System.Windows;
using Microsoft.Win32;
using SAPAPP.Configs;
using SAPAPP.Settings;

namespace SAPAPP
{
    public partial class PreferencesDialog : Window
    {
        Window parentWindow;
        public PreferencesDialog(Window parentWindow)
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

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            FirmwareConfigs configs = Settings.Settings.openConfigs(ConfigTextBox.Text);
            parentWindow.DataContext = new SelectionViewModel(configs);
            
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}