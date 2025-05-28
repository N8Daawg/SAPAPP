using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace SAPAPP
{
    public class SelectionViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> ProductsList { get; set; }
        public ObservableCollection<string> PCBList { get; set; }
        public Dictionary<string, List<string>> ProductPCBMap { get; set; }
        private const string SaveFilePath = "selection.json";

        private string _selectedProduct;
        public string SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
                SaveSelection();
            }
        }

        private string _selectedPCB;
        public string SelectedPCB
        {
            get => _selectedPCB;
            set
            {
                _selectedPCB = value;
                OnPropertyChanged(nameof(SelectedPCB));
                UpdateProductOptions();
                SaveSelection();
            }
        }

        public SelectionViewModel()
        {
            // Initialize lists
            PCBList = new ObservableCollection<string>
            {
                "---", "Texas Instruments MSP430", "Microchip ATmega", "STMicroelectronics STM32",
                "Ezurio BL654 Bluetooth/NFC Module", "Texas Instruments Battery Fuel Gauges"
            };

            ProductsList = new ObservableCollection<string>();

            // Define relationships with multiple products per PCB
            ProductPCBMap = new Dictionary<string, List<string>>
            {
                { "Texas Instruments MSP430", new List<string> { "MSP-FET", "MSP-GANG" } },
                { "Microchip Atmega", new List<string> { "MPLAB PICkit 5", "MPLAB ICE 4", "MPLAB ICD 5" } },
                { "STMicroelectronics STM32", new List<string> { "STLINK-V3MINIE", "STLINK-V3SET" } },
                { "Ezurio BL654 Bluetooth/NFC Module", new List<string> { "UwTerminalX utility" } },
                { "Texas Instruments Battery Fuel Gauges", new List<string> { "Arduino UART programming" } }
            };

            LoadSelection();
        }

        private void UpdateProductOptions()
        {
            ProductsList.Clear();
            ProductsList.Add("---");

            if (!string.IsNullOrEmpty(SelectedPCB) && ProductPCBMap.ContainsKey(SelectedPCB))
            {
                foreach (var product in ProductPCBMap[SelectedPCB])
                {
                    ProductsList.Add(product);
                }
            }

            SelectedProduct = "---"; // Reset PCB selection when product changes
        }

        private void SaveSelection()
        {
            var selection = new { PCB = SelectedPCB, Product = SelectedProduct };
            File.WriteAllText(SaveFilePath, JsonSerializer.Serialize(selection));
        }

        private void LoadSelection()
        {
            if (File.Exists(SaveFilePath))
            {
                var selection = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(SaveFilePath));
                if (selection != null)
                {
                    SelectedProduct = selection.ContainsKey("Product") ? selection["Product"] : "---";
                    SelectedPCB = selection.ContainsKey("PCB") ? selection["PCB"] : "---";
                    UpdateProductOptions();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}