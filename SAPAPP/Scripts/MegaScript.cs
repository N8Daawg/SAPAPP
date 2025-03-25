using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SAPAPP.Scripts
{
    internal class MegaScript : Script
    {

        private const string localInstallDir = @"\firmware\STMPrograms";
        private const string testprogram = @"\STM32BIG\build\arduino.avr.megaADK";
        private const string cliPath = "\"C:\\Program Files (x86)\\Atmel\\Studio\\7.0\\atbackend\\atprogram.exe\"";

        private const string path = "\"C:\\Program Files\\STMicroelectronics\\STM32Cube\\STM32CubeProgrammer\\bin\\STM32_Programmer_CLI.exe\"";
        public MegaScript()
        : base() { }


        public override void Download()
        {
            string strCmdText = cliPath + " -t avrispmk2 -i ISP -d atmega2560 program -f megaADK.ino.elf";
            string firmwareDir = workingDirectory + localInstallDir + testprogram;


            MessageBox.Show(strCmdText);

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.UseShellExecute = false;
            if (testing)
            {
                processStartInfo.Arguments = "/k" + strCmdText;
                processStartInfo.RedirectStandardOutput = false;
                processStartInfo.RedirectStandardError = false;
                processStartInfo.CreateNoWindow = false;
            }
            else
            {
                processStartInfo.Arguments = "/c" + strCmdText;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.CreateNoWindow = true;
            }
            processStartInfo.WorkingDirectory = firmwareDir;


            Process cmd = new Process();
            cmd.StartInfo = processStartInfo;
            cmd.Start();
            cmd.WaitForExit();

            string results = "";
            if (!testing)
            {
                results = cmd.StandardOutput.ReadToEnd();
            }

            cmd.Close();

            MessageBox.Show(results);
        }
    }
}
