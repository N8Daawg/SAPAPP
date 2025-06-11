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
        private TIScript TIScript;

        private FirmwareConfigs configs;
        private const string CLI_config_path = "CLI_configs.json";


        private string _AVRDUDE_CLI;
        public string AVRDUDE_CLI
        {
            get => _AVRDUDE_CLI;
            set
            {
                _AVRDUDE_CLI = value;
                MegaScript.AVRDUDE_CLI = value;
                Save_CLIs();
            }
        }

        private string _STM32_Programmer_CLI;
        public string STM32_Programmer_CLI
        {
            get => _STM32_Programmer_CLI;
            set
            {
                _STM32_Programmer_CLI = value;
                STMScript.STM32_Programmer_CLI= value;
                Save_CLIs();
            }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            InitializeScripts();
            Load_CLIs(CLI_config_path);
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
            TIScript = new TIScript(StatusMessageDisplay, progressPercentage, progbar);
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
                case "fuel gauge": TIScript.Download(currentPart); break;
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
        /// Helps serves a click event for closing the overlay. 
        /// Sets the visibility of the overlay container to collapsed
        /// which serves as hiding the overlay altogether. 
        /// </summary>
        /// <param name="sender"></param>
        /// Serves as the source of the event, without the need of a close overlay button
        /// <param name="e"></param>
        /// The event data associated with clicks on the MainWindow.
        /// </summary>
        private void CloseOverlay_Click(object sender, RoutedEventArgs e)
        {
            OverlayContainer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Serves in handling the click event for toggling the window in
        /// being in the state of always on top.
        /// The property of the Topmost is flipped, which helps in keeping the
        /// window visible at all times.
        /// </summary>
        /// <param name="sender"></param>
        /// The source of an event without the need for a toggle button.
        /// <param name="e"></param>
        /// The event data associated with clicks and other functionality.
        /// </summary>
        private void ToggleStayOnTop_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }

        /// <summary>
        /// Helps in handling the click event for setting the window's font to
        /// small based upon a fixed value. 
        /// </summary>
        /// <param name="sender"></param>
        /// The source of an event without the need for a toggle button.
        /// <param name="e"></param>
        /// The event data associated with clicks and other functionality.
        /// </summary>
        private void FontSizeSmall_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 12;
        }

        /// <summary>
        /// Helps in handling the click event for setting the window's font to
        /// medium based upon a fixed value. 
        /// </summary>
        /// <param name="sender"></param>
        /// The source of an event without the need for a toggle button.
        /// <param name="e"></param>
        /// The event data associated with clicks and other functionality.
        /// </summary>
        private void FontSizeMedium_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 16;
        }

        /// <summary>
        /// Helps in handling the click event for setting the window's font to
        /// large based upon a fixed value. 
        /// </summary>
        /// <param name="sender"></param>
        /// The source of an event without the need for a toggle button.
        /// <param name="e"></param>
        /// The event data associated with clicks and other functionality.
        /// </summary>
        private void FontSizeLarge_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 20;
        }

        /// <summary>
        /// Handles the window size change event through the dynamic 
        /// layout of the canvas items and other UI elements. 
        /// Modifies font sizes, button dimensions and other classifications
        /// based around the aspects of a switch in orientation of the application. 
        /// </summary>
        /// <param name="sender"></param>
        /// The source of an event without the need for a toggle button.
        /// <param name="e"></param>
        /// The event data associated with clicks and other functionality.
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