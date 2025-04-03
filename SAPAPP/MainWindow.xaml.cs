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
using SAPAPP.Scripts;
using System.IO;
using System.ComponentModel;



namespace SAPAPP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private GangScripts gs;
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
            TestScript = new TestScript(StatusMessageDisplay);
            FetScript = new FetScript(StatusMessageDisplay);
            MegaScript = new MegaScript(StatusMessageDisplay);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            progbar.IsIndeterminate = true;
            StatusMessageDisplay.Text = "Starting Download";

            if (ProductPicker.SelectedIndex == 0)
            {
                TestScript.Download();

            }
            else if (ProductPicker.SelectedIndex == 1)
            {
                FetScript.Download();
            }
            else if (ProductPicker.SelectedIndex == 2)
            {
                MegaScript.Download();
            }
            else if (ProductPicker.SelectedIndex == 3)
            {
                
            }


        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

        }
    }
}