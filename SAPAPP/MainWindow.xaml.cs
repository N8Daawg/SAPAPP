using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using SAPAPP.Scripts;
using System.ComponentModel;

namespace SAPAPP
{
    public partial class MainWindow : Window
    {
        private TestScript TestScript;
        private FetScript FetScript;
        private MegaScript MegaScript;

        public MainWindow()
        {
            InitializeComponent();
            InitializeScripts();
        }

        private void InitializeScripts()
        {
            TestScript = new TestScript(StatusMessageDisplay, progressPercentage, progbar);
            FetScript = new FetScript(StatusMessageDisplay, progressPercentage, progbar);
            MegaScript = new MegaScript(StatusMessageDisplay, progressPercentage, progbar);
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                StatusMessageDisplay.Text = $"Opened: {openFileDialog.FileName}";
        }

        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to close?", "Confirm", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                StatusMessageDisplay.Text = "File Closed";
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, "Sample content");
                StatusMessageDisplay.Text = $"Saved: {saveFileDialog.FileName}";
            }
        }

        private void ConfigurePaths_Click(object sender, RoutedEventArgs e) => StatusMessageDisplay.Text = "Configure File Paths option selected";

        private void Wiki_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/SensitTechnologies/TestSuite/wiki",
                UseShellExecute = true
            });
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            SetButtonAppearance(StartButton, Brushes.Green, Brushes.White);
            SetButtonAppearance(StopButton, Brushes.White, Brushes.Black);
            ResetProgressBar();

            StatusMessageDisplay.Text = "Starting Download";

            switch (ProductPicker.SelectedIndex)
            {
                case 0: TestScript.Download(); break;
                case 1: FetScript.Download(); break;
                case 2: MegaScript.Download(); break;
            }

            await UpdateProgressBar();
            StatusMessageDisplay.Text = "Download Complete!";
            StartButton.IsEnabled = true;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonAppearance(StartButton, Brushes.White, Brushes.Black);
            SetButtonAppearance(StopButton, Brushes.Red, Brushes.White);

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

        private void SetButtonAppearance(Button button, Brush background, Brush foreground)
        {
            button.Background = background;
            button.Foreground = foreground;
        }

        private void ResetProgressBar()
        {
            progbar.IsIndeterminate = false;
            progbar.Value = 0;
        }

        // Dark Mode Toggle
        private void ToggleDarkMode_Click(object sender, RoutedEventArgs e)
        {
            if (this.Background == Brushes.Black)
            {
                this.Background = Brushes.White;
                SetLightModeColors();
            }
            else
            {
                this.Background = Brushes.Black;
                SetDarkModeColors();
            }
        }

        private void SetLightModeColors()
        {
            // Change UI elements for light mode
            StatusMessageDisplay.Foreground = Brushes.Black;
            progressPercentage.Foreground = Brushes.Black;
            progbar.Foreground = Brushes.Black;
            StartButton.Background = Brushes.LightGray;
            StopButton.Background = Brushes.LightGray;
        }

        private void SetDarkModeColors()
        {
            // Change UI elements for dark mode
            StatusMessageDisplay.Foreground = Brushes.White;
            progressPercentage.Foreground = Brushes.White;
            progbar.Foreground = Brushes.Green;
            StartButton.Background = Brushes.Gray;
            StopButton.Background = Brushes.Gray;
        }

        // Stay On Top Feature
        private void ToggleStayOnTop_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }

        // Font Size Adjustments
        private void FontSizeSmall_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 12; // Adjust as needed
        }

        private void FontSizeMedium_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 16; // Adjust as needed
        }

        private void FontSizeLarge_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 20; // Adjust as needed
        }

        // CLI Test Method
        private void CLI_test()
        {
            string strCmdText = "echo hello world";
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + strCmdText,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using Process cmd = new Process { StartInfo = processStartInfo };
            cmd.Start();
            cmd.WaitForExit();

            Process_Feedback(cmd.StandardOutput.ReadToEnd());
        }

        private void Process_Feedback(string feedback) => StatusMessageDisplay.Text = feedback.Trim();
    }
}