using SAPAPP.Configs;
using SAPAPP.Scripts;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace SAPAPP
{
    public partial class MainWindow : Window
    {
        private TestScript TestScript;
        private FetScript FetScript;
        private MegaScript MegaScript;
        private STMScript STMScript;

        private FirmwareConfigs configs;

        private const string CLI_config_path = "CLI_configs.json";


        private string _AVRDUDE_CLI;
        public string AVRDUDE_CLI
        {
            get => _AVRDUDE_CLI;
            set
            {
                _AVRDUDE_CLI = value;
                Save_CLIs();
            }
        }

        //private string STM32_Programmer_CLI = "\"C:\\Program Files\\STMicroelectronics\\STM32Cube\\STM32CubeProgrammer\\bin\\STM32_Programmer_CLI.exe\"";
        private string _STM32_Programmer_CLI;
        public string STM32_Programmer_CLI
        {
            get => _STM32_Programmer_CLI;
            set
            {
                _STM32_Programmer_CLI = value;
                Save_CLIs();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Load_CLIs(CLI_config_path);
            InitializeScripts();
            Load_Product_Configurations(Settings.Settings.configFile);
        }

        private void InitializeScripts()
        {
            TestScript = new TestScript(StatusMessageDisplay, progressPercentage, progbar);
            FetScript = new FetScript(StatusMessageDisplay, progressPercentage, progbar);
            MegaScript = new MegaScript(StatusMessageDisplay, progressPercentage, progbar, AVRDUDE_CLI);
            STMScript = new STMScript(StatusMessageDisplay, progressPercentage, progbar, STM32_Programmer_CLI);
        }

        public void Load_Product_Configurations(string filename)
        {
            configs = Settings.Settings.OpenConfigs(filename);
            DataContext = new SelectionViewModel(configs);
        }

        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to close?",
                                                      "Confirm", MessageBoxButton.YesNo,
                                                      MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown(); // Closes the entire application
            }
        }

        private void Preferences_Click(object sender, RoutedEventArgs e)
        {
            StatusMessageDisplay.Text = "Preferences option selected";

            PreferencesDialog preferencesDialog = new(this);
            preferencesDialog.Owner = this;  // Sets MainWindow as the owner
            preferencesDialog.ShowDialog();  // Opens it as a modal window
        }

        private void Wiki_Click(object sender, RoutedEventArgs e)
        {
            StatusMessageDisplay.Text = "Wiki option selected";

            WikiDialog wikiDialog = new();
            wikiDialog.Owner = this;  // Sets MainWindow as the owner
            wikiDialog.ShowDialog();  // Opens it as a modal window
        }

        public void CloseOverlay_Click(object sender, RoutedEventArgs e)
        {
            OverlayContainer.Visibility = Visibility.Collapsed;
        }

        public void Load_CLIs(string filename)
        {
            if (File.Exists(filename))
            {
                Dictionary<string, string> selection = Settings.Serializer.DeserializeJson<Dictionary<string, string>>(filename);
                if (selection != null)
                {
                    STM32_Programmer_CLI = selection.ContainsKey("STM32") ? selection["STM32"] : "\"C:\\Program Files\\STMicroelectronics\\STM32Cube\\STM32CubeProgrammer\\bin\\STM32_Programmer_CLI.exe\"";
                    AVRDUDE_CLI = selection.ContainsKey("AVRDUDE") ? selection["AVRDUDE"] : "";
                }
            }
            
            Save_CLIs();

        }
        
        public void Save_CLIs()
        {
            var selection = new { STM32=STM32_Programmer_CLI, AVRDUDE=AVRDUDE_CLI };
            Settings.Serializer.SerializeJson(selection, CLI_config_path);
            InitializeScripts();
        }

        private Product Get_Current_Product()
        {
            Product currentProduct = new Product();
            foreach (Product product in configs.Products)
            {
                if (product.ProductName == ProductPicker.Text)
                {
                    currentProduct = product;
                    break;
                }
            }

            return currentProduct;
        }
        private Part Get_Current_Part(Product product)
        {
            Part currentPart = new Part();
            foreach (Part part in product.Parts)
            {
                if (part.PartName == PartPicker.Text)
                {
                    currentPart = part;
                    break;
                }
            }
            return currentPart;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonAppearance(StartButton, Brushes.White, Brushes.Black);
            SetButtonAppearance(StopButton, Brushes.Red, Brushes.White);


            Product currentProduct = Get_Current_Product();
            Part currentPart = Get_Current_Part(currentProduct); 
            switch (currentPart.Architecture)
            {
                case "---": TestScript.Cancel(); break;
                case "MSP430": FetScript.Cancel(); break;
                case "ATmega": MegaScript.Cancel(); break;
                case "STM32": STMScript.Cancel(); break;
                default: break;
            }

            StatusMessageDisplay.Text = "Download Canceled";
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            SetButtonAppearance(StartButton, Brushes.Green, Brushes.White);
            SetButtonAppearance(StopButton, Brushes.White, Brushes.Black);
            ResetProgressBar();

            StatusMessageDisplay.Text = "Starting Download";

            Product currentProduct = Get_Current_Product();
            Part currentPart = Get_Current_Part(currentProduct);

            switch (currentPart.Architecture)
            {
                case "---": TestScript.Download(currentPart); break;
                case "MSP430": FetScript.Download(currentPart); break;
                case "ATmega": MegaScript.Download(currentPart); break;
                case "STM32": STMScript.Download(currentPart); break;
                default: break;

            }

            StartButton.IsEnabled = true;
        }

        private static void SetButtonAppearance(Button button, Brush background, Brush foreground)
        {
            button.Background = background;
            button.Foreground = foreground;
        }

        private void ResetProgressBar()
        {
            progbar.IsIndeterminate = false;
            progbar.Value = 0;
        }

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
            StatusMessageDisplay.Foreground = Brushes.Black;
            progressPercentage.Foreground = Brushes.Black;
            progbar.Foreground = Brushes.Black;
            StartButton.Background = Brushes.LightGray;
            StopButton.Background = Brushes.LightGray;
        }

        private void SetDarkModeColors()
        {
            StatusMessageDisplay.Foreground = Brushes.White;
            progressPercentage.Foreground = Brushes.White;
            progbar.Foreground = Brushes.Green;
            StartButton.Background = Brushes.Gray;
            StopButton.Background = Brushes.Gray;
        }

        private void ToggleStayOnTop_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }

        private void FontSizeSmall_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 12;
        }

        private void FontSizeMedium_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 16;
        }

        private void FontSizeLarge_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 20;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void PCBPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }

    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application app = new();
            app.Run(new MainWindow());
        }
    }
}