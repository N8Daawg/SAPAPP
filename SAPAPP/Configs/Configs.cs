﻿using System.Text;


namespace SAPAPP.Configs
{

    [Serializable]
    public class Part
    {
        public string PartName { get; set; } = "---";
        public string Architecture { get; set; } = "---";
        public string Chip { get; set; }
        public string DriveLocation { get; set; }
        public string ProductFolder { get; set; }
        public string FirmwareFolder { get; set; }
        public string FirmwareFile { get; set; }

        private string FullPath()
        {
            string path = string.Empty;
            if (string.IsNullOrEmpty(FirmwareFolder))
            {
                path = string.Format("{0}\\{1}", DriveLocation, ProductFolder);
            }
            else
            {
                path = string.Format("{0}\\{1}\\{2}", DriveLocation, ProductFolder, FirmwareFolder);
            }
            return path;
        }
        public string FullFirmwarePath()
        {
            return string.Format("{0}\\{1}", FullPath(), FirmwareFile);
        }
        public new string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(PartName);
            return sb.ToString();
        }
    }

    [Serializable]
    public class Product
    {
        public string ProductName { get; set; } = "---";
        public List<Part> Parts { get; set; } = [];
        public string ProductFolder { get; set; }
        public string DriveLocation { get; set; }
        public void configureFullPaths()
        {
            foreach (Part part in Parts)
            {
                part.ProductFolder = ProductFolder;
                part.DriveLocation = DriveLocation;
            }
        }
        public void Sort() 
        {
            Parts.Sort(delegate (Part x, Part y)
            {
                if (x.PartName == null && y.PartName == null) return 0;
                else if (x.PartName == null) return -1;
                else if (y.PartName == null) return 1;
                else return x.PartName.CompareTo(y.PartName);
            });
        }
        public new string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(ProductName); sb.Append(": ");
            foreach(Part part in Parts)
            {
                sb.Append(part.ToString());
            }

            return sb.ToString();
        }
    }

    [Serializable]
    public class FirmwareConfigs
    {
        public List<Product> Products { get; set; } = [];
        public string DriveLocation { get; set; }
        public void configureFullPaths()
        {
            foreach (Product product in Products)
            {
                product.DriveLocation = DriveLocation;
                product.configureFullPaths();
            }
        }
        public void Sort() 
        {

            foreach (Product product in Products)
            {
                product.Sort();
            }

            Products.Sort(delegate (Product x, Product y)
            {
                if (x.ProductName == null && y.ProductName == null) return 0;
                else if (x.ProductName == null) return -1;
                else if (y.ProductName == null) return 1;
                else return x.ProductName.CompareTo(y.ProductName);
            });
        }
        public new string ToString() 
        { 
            StringBuilder sb = new StringBuilder();
            foreach(Product product in Products)
            {
                sb.Append(product.ToString());
                sb.Append('\n');
            }
            return sb.ToString(); 
        }
    }
}
