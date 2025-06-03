﻿using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SAPAPP
{
    public partial class WikiDialog : Window
    {
        private Stack<string> navigationHistory = new Stack<string>();
        private Stack<string> forwardHistory = new Stack<string>();

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

        private void WikiPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WikiPages.SelectedItem != null)
            {
                string selectedPage = WikiPages.SelectedItem.ToString();

                if (navigationHistory.Count == 0 || navigationHistory.Peek() != selectedPage)
                {
                    navigationHistory.Push(selectedPage);
                    forwardHistory.Clear(); // Clear forward history when selecting new pages
                }

                LoadWikiPage(selectedPage);
            }
        }

        private void LoadWikiPage(string pageName)
        {
            string filePath = $"WikiPages/{pageName}.txt";
            if (File.Exists(filePath))
            {
                WikiContent.Document.Blocks.Clear();
                WikiContent.Document.Blocks.Add(new Paragraph(new Run(File.ReadAllText(filePath))));
            }
            else
            {
                WikiContent.Document.Blocks.Clear();
                WikiContent.Document.Blocks.Add(new Paragraph(new Run("Content unavailable for the selected page.")));
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (navigationHistory.Count > 1)
            {
                forwardHistory.Push(navigationHistory.Pop());

                if (navigationHistory.Count > 0)
                {
                    string previousPage = navigationHistory.Peek();
                    LoadWikiPage(previousPage);
                }
            }
            else
            {
                // Completely reset the screen to initial state
                WikiContent.Document.Blocks.Clear();
                WikiContent.Document.Blocks.Add(new Paragraph(new Run("Welcome to the Swiss-Army Programmer Wiki!")));
                SearchBox.Clear(); // Ensure search box is reset
                MainTabControl.SelectedIndex = 0; // Reset to first tab
                navigationHistory.Clear(); // Clear history so nothing remains
                forwardHistory.Clear(); // Also reset forward stack

                // Restore visibility of all tabs
                foreach (TabItem tab in MainTabControl.Items)
                {
                    tab.Visibility = Visibility.Visible;
                }
            }
        }

        // Handles live filtering based on displayed text
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();

            if (!string.IsNullOrEmpty(searchText))
            {
                foreach (TabItem tab in MainTabControl.Items)
                {
                    var textBlocks = tab.Content as ScrollViewer;
                    if (textBlocks != null)
                    {
                        var stackPanel = textBlocks.Content as StackPanel;
                        if (stackPanel != null)
                        {
                            bool matchFound = stackPanel.Children.OfType<TextBlock>()
                                .Any(tb => tb.Text.ToLower().Contains(searchText));

                            tab.Visibility = matchFound ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void HighlightSearchResults(string searchText)
        {
            if (string.IsNullOrEmpty(searchText)) return;

            TextRange documentRange = new TextRange(WikiContent.Document.ContentStart, WikiContent.Document.ContentEnd);
            documentRange.ClearAllProperties(); // Remove previous highlights

            TextPointer start = WikiContent.Document.ContentStart;

            while (start != null && start.CompareTo(WikiContent.Document.ContentEnd) < 0)
            {
                if (start.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = start.GetTextInRun(LogicalDirection.Forward);
                    int index = textRun.ToLower().IndexOf(searchText.ToLower());

                    if (index >= 0)
                    {
                        TextPointer highlightStart = start.GetPositionAtOffset(index);
                        TextPointer highlightEnd = highlightStart.GetPositionAtOffset(searchText.Length);

                        TextRange highlightRange = new TextRange(highlightStart, highlightEnd);
                        highlightRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
                    }
                }
                start = start.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        private void PlayVideo_Click(object sender, RoutedEventArgs e)
        {
            TutorialVideo.Play();
        }

        private void PauseVideo_Click(object sender, RoutedEventArgs e)
        {
            TutorialVideo.Pause();
        }

        private void StopVideo_Click(object sender, RoutedEventArgs e)
        {
            TutorialVideo.Stop();
        }

        // Handles search queries and navigates to relevant sections
        private void SearchWiki_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = SearchBox.Text.Trim().ToLower();
            HighlightSearchResults(searchQuery); // Keep search highlighting

            if (!string.IsNullOrEmpty(searchQuery))
            {
                if (MainTabControl != null)
                {
                    SearchBox_TextChanged(null, null);

                    foreach (TabItem tab in MainTabControl.Items)
                    {
                        var textBlocks = tab.Content as ScrollViewer;
                        if (textBlocks != null)
                        {
                            var stackPanel = textBlocks.Content as StackPanel;
                            if (stackPanel != null)
                            {
                                if (stackPanel.Children.OfType<TextBlock>().Any(tb => tb.Text.ToLower().Contains(searchQuery)))
                                {
                                    MessageBox.Show($"Navigating to: {tab.Header}", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                                    MainTabControl.SelectedItem = tab;
                                    SearchBox.Clear(); // Reset search field
                                    return;
                                }
                            }
                        }
                    }

                    MessageBox.Show("No direct match found. Try refining your search term.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Warning);
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