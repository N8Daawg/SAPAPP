using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Orientation = System.Windows.Controls.Orientation;
using ProgressBar = System.Windows.Controls.ProgressBar;



namespace SAPAPP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private GangScripts gs;
        private FETScripts fs;

        public MainWindow()
        {
            InitializeComponent();
            //gs = new GangScripts();
            fs = new FETScripts();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            progbar.IsIndeterminate = true;
            StatusMessageDisplay.Text = "Starting Download";

            if (PCBPicker.SelectedIndex == 0)
            {
                if (ProductPicker.SelectedIndex == 0)
                {
                    Process_Feedback(fs.MSP430Download());
                }
            }
            
            //CLI_test();
            //StatusMessageDisplay.Text =  gs.GangInit();

        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            progbar.IsIndeterminate = false;
            StatusMessageDisplay.Text = "Download Canceled";
        }

        private void CLI_test()
        {
            string strCmdText = "echo hello world";
            
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.Arguments = "/c" + strCmdText;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;

            Process cmd = new Process();
            cmd.StartInfo = processStartInfo;
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

        }
    }
}