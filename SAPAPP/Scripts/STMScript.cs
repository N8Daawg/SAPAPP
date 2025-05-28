using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{

    internal class STMScript(TextBlock fd, TextBlock pp, ProgressBar pb) : Script(fd, pp, pb)
    {

        private const string path = "\"C:\\Program Files\\STMicroelectronics\\STM32Cube\\STM32CubeProgrammer\\bin\\STM32_Programmer_CLI.exe\"";


        protected override void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
