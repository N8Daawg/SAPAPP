using SAPAPP.Configs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

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

        public SelectionViewModel(FirmwareConfigs config)
        {

            PCBList = new ObservableCollection<string>();
            ProductsList = new ObservableCollection<string>();
            ProductPCBMap = new Dictionary<string, List<string>>();


            PCB defaultView = new();
            defaultView.Products.Add(new ProductConfig());
            List<string> products = [defaultView.Products[0].ProductName];
            PCBList.Add(defaultView.PCBName);
            ProductPCBMap.Add(defaultView.PCBName, products);


            foreach (PCB pcb in config.PCBs)
            {

                PCBList.Add(pcb.PCBName);
                List<string> ProductNames = new List<string>();
                foreach (ProductConfig product in pcb.Products)
                {
                    ProductNames.Add(product.ProductName);
                }

                if (ProductPCBMap.ContainsKey(pcb.PCBName))
                {
                    List<string> existingPCBProducts = ProductPCBMap[pcb.PCBName];
                    foreach (string productName in ProductNames)
                    {
                        existingPCBProducts.Add(productName);
                    }
                } else
                {
                    ProductPCBMap.Add(pcb.PCBName, ProductNames);
                }
            }

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
            Settings.Serializer.SerializeJson(selection, SaveFilePath);
            //File.WriteAllText(SaveFilePath, JsonSerializer.Serialize(selection));
        }

        private void LoadSelection()
        {
            if (File.Exists(SaveFilePath))
            {
                Dictionary<string, string> selection = Settings.Serializer.DeserializeJson<Dictionary<string, string>>(SaveFilePath);
                //var selection = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(SaveFilePath));
                if (selection != null)
                {
                    SelectedPCB = selection.ContainsKey("PCB") ? selection["PCB"] : "---";
                    UpdateProductOptions();
                    SelectedProduct = selection.ContainsKey("Product") ? selection["Product"] : "---";

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