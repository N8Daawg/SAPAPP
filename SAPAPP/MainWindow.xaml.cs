using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using SAPAPP.Scripts;

namespace SAPAPP
{
    public partial class MainWindow : Window
    {
        private FetScript fs;

        public MainWindow()
        {
            InitializeComponent();
            fs = new FetScript();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                StatusMessageDisplay.Text = $"Opened: {openFileDialog.FileName}";
            }
        }

        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            StatusMessageDisplay.Text = "File Closed";
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, "Sample content"); // Replace with actual file-saving logic
                StatusMessageDisplay.Text = $"Saved: {saveFileDialog.FileName}";
            }
        }

        private void ConfigurePaths_Click(object sender, RoutedEventArgs e)
        {
            StatusMessageDisplay.Text = "Configure File Paths option selected";
        }

        // Wiki Click event handler
        private void Wiki_Click(object sender, RoutedEventArgs e)
        {
            // Open the wiki URL in the default web browser
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/SensitTechnologies/TestSuite/wiki",
                UseShellExecute = true
            });
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.Background = Brushes.White;
            StopButton.Foreground = Brushes.Black;

            if (sender is Button button)
            {
                button.Background = Brushes.Green;
                button.Foreground = Brushes.White;
            }

            progbar.IsIndeterminate = false;
            progbar.Value = 0;
            StatusMessageDisplay.Text = "Starting Download";

            if (PCBPicker.SelectedIndex == 1 && ProductPicker.SelectedIndex == 1)
            {
                fs.Download();
            }

            await UpdateProgressBar();
            StatusMessageDisplay.Text = "Download Complete!";
            StartButton.IsEnabled = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Background = Brushes.White;
            StartButton.Foreground = Brushes.Black;

            if (sender is Button button)
            {
                button.Background = Brushes.Red;
                button.Foreground = Brushes.White;
            }

            progbar.IsIndeterminate = false;
            StatusMessageDisplay.Text = "Download Canceled";
        }

        private async Task UpdateProgressBar()
        {
            for (int i = 0; i <= 100; i++)
            {
                progbar.Value = i;
                progressPercentage.Text = $"{i}%";
                await Task.Delay(50);
            }
        }

        private void CLI_test()
        {
            string strCmdText = "echo hello world";

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + strCmdText,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Process cmd = new Process { StartInfo = processStartInfo };
            cmd.Start();
            cmd.WaitForExit();

            Process_Feedback(cmd.StandardOutput.ReadToEnd());
        }

        private void Process_Feedback(string feedback)
        {
            feedback = feedback.Trim();
            StatusMessageDisplay.Text = feedback;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // Additional initialization logic if needed
        }
    }
}


