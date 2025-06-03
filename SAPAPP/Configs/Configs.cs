using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPAPP.Configs
{
    [Serializable]
    public class ProductConfig
    {
        public string ProductName { get; set; } = "---";
        public string Executable { get; set; } = "---";
        public string FirmwarePath { get; set; } = "---";

        public new string ToString
        {
            get
            {
                StringBuilder sb = new();
                sb.Append(ProductName);
                sb.Append(" at ");
                sb.Append(FirmwarePath);
                return sb.ToString();
            }
        }
    }

    [Serializable]
    public class PCB
    {
        public string PCBName { get; set; } = "---";
        public List<ProductConfig> Products { get; } = new List<ProductConfig>();

        public new string ToString()
        {
            StringBuilder sb = new();
            sb.Append(PCBName);
            sb.Append(": ");
            foreach (ProductConfig p in Products)
            {
                sb.Append(p.ToString);
                sb.Append(",  ");
            }

            return sb.ToString();
        }
    }

    [Serializable]
    public class FirmwareConfigs
    {
        public List<PCB> PCBs { get; } = [];

        public void Sort()
        {
            foreach (PCB board in PCBs)
            {
                board.Products.Sort(delegate(ProductConfig x, ProductConfig y)
                {
                    if (x.ProductName == null && y.ProductName == null) return 0;
                    else if (x.ProductName == null) return -1;
                    else if (y.ProductName == null) return 1;
                    else return x.ProductName.CompareTo(y.ProductName);
                });
            }

            PCBs.Sort(delegate (PCB x, PCB y)
            {
                if (x.PCBName == null && y.PCBName == null) return 0;
                else if (x.PCBName == null) return -1;
                else if (y.PCBName == null) return 1;
                else return x.PCBName.CompareTo(y.PCBName);
            });
        }

        public new string ToString()
        {
            StringBuilder sb = new();
            foreach (var p in PCBs)
            {
                sb.Append(p.ToString());
                sb.Append('\n');
            }
            return sb.ToString();
        }
    }
}
