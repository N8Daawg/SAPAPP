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
        public string ProductName { get; set; } = "";
        public string FirmwareFolderPath { get; set; } = "";
    }

    [Serializable]
    public class TiProductConfig : ProductConfig
    {
        public string ProductName { get; set; } = "";
        public string FirmwareFolderPath { get; set; } = "";
    }

    [Serializable]
    public class ATMegaProductConfig : ProductConfig
    {
        public string FirmwareFilePath { get; set; } = "";
        public string Processor { get; set; } = "";
    }

    [Serializable]
    public class PCB
    {
        public string PCBName { get; set; }
        public List<ProductConfig> Products { get; } = new List<ProductConfig>();
    }

    [Serializable]
    public class FirmwareConfigs
    {
        public List<PCB> PCBs { get; } = new List<PCB>();
    }
}
