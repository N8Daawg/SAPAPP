using SAPAPP.Configs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace SAPAPP
{
    public class SelectionViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> ProductsList { get; set; }
        public ObservableCollection<string> PartsList { get; set; }
        public Dictionary<string, List<string>> ProductPartMap { get; set; }
        private const string SaveFilePath = "selection.json";

        private string _selectedProduct;
        public string SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
                UpdatePartOptions();
                SaveSelection();
            }
        }

        private string _selectedPart;
        public string SelectedPart
        {
            get => _selectedPart;
            set
            {
                _selectedPart = value;
                OnPropertyChanged(nameof(SelectedPart));
                SaveSelection();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public SelectionViewModel(FirmwareConfigs config)
        {
            ProductsList = new ObservableCollection<string>();
            PartsList = new ObservableCollection<string>();
            ProductPartMap = new Dictionary<string, List<string>>();


            Product defaultView = new();
            defaultView.Parts.Add(new Part());
            List<string> parts = [defaultView.Parts[0].PartName];
            ProductsList.Add(defaultView.ProductName);
            ProductPartMap.Add(defaultView.ProductName, parts);


            foreach (Product product in config.Products)
            {
                ProductsList.Add(product.ProductName);
                List<string> PartNames = new List<string>();
                foreach (Part part in product.Parts)
                {
                    PartNames.Add(part.PartName);
                }

                if (ProductPartMap.ContainsKey(product.ProductName))
                {
                    foreach (string PartName in PartNames)
                    {
                        ProductPartMap[product.ProductName].Add(PartName);
                    }
                } 
                else
                {
                    ProductPartMap.Add(product.ProductName, PartNames);
                }
            }

            LoadSelection();
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdatePartOptions()
        {
            PartsList.Clear();
            PartsList.Add("---");
            if (!string.IsNullOrEmpty(SelectedProduct) && ProductPartMap.ContainsKey(SelectedProduct))
            {
                foreach (var part in ProductPartMap[SelectedProduct])
                {
                    PartsList.Add(part);
                }
            }
            SelectedPart = "---"; // Reset Part selection when product changes
        }

        /// <summary>
        /// 
        /// </summary>
        private void SaveSelection()
        {
            var selection = new { Product = SelectedProduct, Part = SelectedPart };
            Settings.Serializer.SerializeJson(selection, SaveFilePath);
            //File.WriteAllText(SaveFilePath, JsonSerializer.Serialize(selection));
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadSelection()
        {
            if (File.Exists(SaveFilePath))
            {
                Dictionary<string, string> selection = Settings.Serializer.DeserializeJson<Dictionary<string, string>>(SaveFilePath);
                //var selection = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(SaveFilePath));
                if (selection != null)
                {
                    SelectedProduct = selection.ContainsKey("Product") ? selection["Product"] : "---";
                    UpdatePartOptions();
                    SelectedPart = selection.ContainsKey("Part") ? selection["Part"] : "---";

                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}