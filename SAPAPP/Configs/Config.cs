using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace SAPAPP.Configs
{
    [Serializable]
    public class ProductSettings
    {
        public string ProductName { get; set; }
        public string FirmwareFolderPath { get; set; }
    }

    [Serializable]
    public class TiProductSettings : ProductSettings
    {
        public string ProductName { get; set; }
        public string FirmwareFolderPath { get; set; }
    }

    [Serializable]
    public class ATMegaProductSettings : ProductSettings 
    {
        public string FirmwareFilePath { get; set; }
        public string Processor { get; set; }
    }

    [Serializable]
    public class PCB
    {
        public string PCBName { get; set; }
        public List<ProductSettings> Products { get; } = new List<ProductSettings>();
    }

    [Serializable]
    public class FirmwareConfigs
    {
        public List<PCB> PCBs { get; } = new List<PCB>();
    }
}
