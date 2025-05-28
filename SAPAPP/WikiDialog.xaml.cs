using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP
{
    public partial class WikiDialog : Window
    {
        public WikiDialog()
        {
            InitializeComponent();
        }

        // Closes the wiki dialog window
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Opens the official Wiki page
        private void OpenWiki_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/SensitTechnologies/SAPAPP/wiki",
                UseShellExecute = true
            });
        }

        // Handles search queries and navigates directly to relevant sections
        private void SearchWiki_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = SearchBox.Text.Trim().ToLower();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                // Ensure TabControl reference is correct
                if (MainTabControl != null)
                {
                    if (searchQuery.Contains("firmware") || searchQuery.Contains("upload") || searchQuery.Contains("flash"))
                    {
                        MessageBox.Show("Navigating to Firmware Upload Section...", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                        MainTabControl.SelectedIndex = 1; // Technician Guide Tab
                    }
                    else if (searchQuery.Contains("hardware") || searchQuery.Contains("microcontroller") || searchQuery.Contains("pcb"))
                    {
                        MessageBox.Show("Navigating to Hardware Requirements Section...", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                        MainTabControl.SelectedIndex = 2; // Hardware Requirements Tab
                    }
                    else if (searchQuery.Contains("troubleshooting") || searchQuery.Contains("faq") || searchQuery.Contains("help"))
                    {
                        MessageBox.Show("Navigating to FAQ Section...", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                        MainTabControl.SelectedIndex = 3; // FAQ Tab
                    }
                    else if (searchQuery.Contains("wiki") || searchQuery.Contains("documentation") || searchQuery.Contains("manual"))
                    {
                        MessageBox.Show("Navigating to Resources Section...", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                        MainTabControl.SelectedIndex = 4; // Resources Tab
                    }
                    else
                    {
                        MessageBox.Show("No direct match found. Try searching for 'firmware', 'hardware', 'troubleshooting', or 'wiki'.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Error: TabControl reference is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please enter a search term.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}