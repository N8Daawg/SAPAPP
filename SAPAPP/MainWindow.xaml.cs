using SAPAPP.Configs;
using SAPAPP.Scripts;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace SAPAPP
{
    public partial class MainWindow : Window
    {
        #region instanceVariables


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

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            Load_CLIs(CLI_config_path);
            InitializeScripts();
            Load_Product_Configurations(Settings.Settings.configFile);
        }


        #region Scripts&Configs

        /// <summary>
        /// Initializes all loader scripts
        /// </summary>
        private void InitializeScripts()
        {
            TestScript = new TestScript(StatusMessageDisplay, progressPercentage, progbar);
            FetScript = new FetScript(StatusMessageDisplay, progressPercentage, progbar);
            MegaScript = new MegaScript(StatusMessageDisplay, progressPercentage, progbar, AVRDUDE_CLI);
            STMScript = new STMScript(StatusMessageDisplay, progressPercentage, progbar, STM32_Programmer_CLI);
        }

        /// <summary>
        /// Method reads dropdown and gets the selected product from the configuration
        /// </summary>
        /// <returns>The selected Product</returns>
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

        /// <summary>
        /// Uses selected product and Drop down menu selection to get the selected part to load
        /// </summary>
        /// <param name="product">The currently selected product</param>
        /// <returns>The currently selected Part</returns>
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

        /// <summary>
        /// Loads a new list of products and their respective parts from a configuration file in the form of a filename 
        /// </summary>
        /// <param name="filename"></param>
        public void Load_Product_Configurations(string filename)
        {
            configs = Settings.Settings.OpenConfigs(filename);

            SelectionViewModel newContext = new SelectionViewModel(configs);
            if (filename != Settings.Settings.configFile)
            {
                newContext.SelectedProduct = "---";
                newContext.SelectedPart = "---";
            }
            DataContext = newContext;
            
        }

        /// <summary>
        /// Loads the configuration file containing the pathways for the required applications for the function to function
        /// </summary>
        /// <param name="filename"></param>
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

        /// <summary>
        /// Saves the configurations for the current pathways of the required applications for this software to function
        /// </summary>
        public void Save_CLIs()
        {
            var selection = new { STM32 = STM32_Programmer_CLI, AVRDUDE = AVRDUDE_CLI };
            Settings.Serializer.SerializeJson(selection, CLI_config_path);
            InitializeScripts();
        }

        #endregion


        #region Buttons

        /// <summary>
        /// Method for when the start button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            SetButtonAppearance(StartButton, Brushes.Green, Brushes.White);
            SetButtonAppearance(StopButton, Brushes.White, Brushes.Black);
            ResetProgressBar();

            StatusMessageDisplay.Text = "Starting Download";

            Product currentProduct = Get_Current_Product();
            Part currentPart = Get_Current_Part(currentProduct);

            switch (currentPart.Architecture.ToLower())
            {
                case "---": TestScript.Download(currentPart); break;
                case "msp430": FetScript.Download(currentPart); break;
                case "atmega": MegaScript.Download(currentPart); break;
                case "stm32": STMScript.Download(currentPart); break;
                case "laird": break;
                case "fuel gauge": break;
                default: break;

            }

            StartButton.IsEnabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="button"></param>
        /// <param name="background"></param>
        /// <param name="foreground"></param>
        private static void SetButtonAppearance(Button button, Brush background, Brush foreground)
        {
            button.Background = background;
            button.Foreground = foreground;
        }

        #endregion


        #region ToolbarMethods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Preferences_Click(object sender, RoutedEventArgs e)
        {
            StatusMessageDisplay.Text = "Preferences option selected";

            PreferencesDialog preferencesDialog = new(this);
            preferencesDialog.Owner = this;  // Sets MainWindow as the owner
            preferencesDialog.ShowDialog();  // Opens it as a modal window
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wiki_Click(object sender, RoutedEventArgs e)
        {
            StatusMessageDisplay.Text = "Wiki option selected";

            WikiDialog wikiDialog = new();
            wikiDialog.Owner = this;  // Sets MainWindow as the owner
            wikiDialog.ShowDialog();  // Opens it as a modal window
        }

        #endregion


        #region Feedback

        /// <summary>
        /// 
        /// </summary>
        private void ResetProgressBar()
        {
            progbar.IsIndeterminate = false;
            progbar.Value = 0;
        }

        #endregion


        #region MiscUIMethods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseOverlay_Click(object sender, RoutedEventArgs e)
        {
            OverlayContainer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 
        /// </summary>
        private void SetLightModeColors()
        {
            StatusMessageDisplay.Foreground = Brushes.Black;
            progressPercentage.Foreground = Brushes.Black;
            progbar.Foreground = Brushes.Black;
            StartButton.Background = Brushes.LightGray;
            StopButton.Background = Brushes.LightGray;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetDarkModeColors()
        {
            StatusMessageDisplay.Foreground = Brushes.White;
            progressPercentage.Foreground = Brushes.White;
            progbar.Foreground = Brushes.Green;
            StartButton.Background = Brushes.Gray;
            StopButton.Background = Brushes.Gray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleStayOnTop_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontSizeSmall_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 12;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontSizeMedium_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 16;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontSizeLarge_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 20;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double scaleFactor = e.NewSize.Width / 1366.0;

            ProductLabel.FontSize = 48 * scaleFactor;
            PartLabel.FontSize = 48 * scaleFactor;
            StartButton.FontSize = 40 * scaleFactor;
            StopButton.FontSize = 40 * scaleFactor;

            ProductPicker.Width = 600 * scaleFactor;
            PartPicker.Width = 600 * scaleFactor;

            // Adjust positioning dynamically
            Canvas.SetLeft(ProductLabel, 100 * scaleFactor);
            Canvas.SetTop(ProductLabel, 80 * scaleFactor);
            Canvas.SetLeft(PartLabel, 100 * scaleFactor);
            Canvas.SetTop(PartLabel, 240 * scaleFactor);
            Canvas.SetLeft(StartButton, 100 * scaleFactor);
            Canvas.SetTop(StartButton, 420 * scaleFactor);
            Canvas.SetLeft(StopButton, 400 * scaleFactor);
            Canvas.SetTop(StopButton, 420 * scaleFactor);

            if (GridBackground != null)
            {
                ImageBrush bg = GridBackground.Background as ImageBrush;
                if (bg != null)
                {
                    // Adjust background stretch based on orientation
                    bg.Stretch = e.NewSize.Width > e.NewSize.Height ? Stretch.UniformToFill : Stretch.Fill;
                }
            }
        }

        #endregion
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