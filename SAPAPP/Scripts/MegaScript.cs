using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class MegaScript(TextBlock fd) : Script(fd)
    {

        private const string localInstallDir = @"\firmware\STMPrograms";
        private const string testprogram = @"\STM32BIG\build\arduino.avr.megaADK";
        private const string cliPath = "\"C:\\Program Files (x86)\\Atmel\\Studio\\7.0\\atbackend\\atprogram.exe\"";

        private const string path = "\"C:\\Program Files\\STMicroelectronics\\STM32Cube\\STM32CubeProgrammer\\bin\\STM32_Programmer_CLI.exe\"";


        protected override void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            string strCmdText = cliPath + " -t avrispmk2 -i ISP -d atmega2560 program -f megaADK.ino.elf";
            string firmwareDir = workingDirectory + localInstallDir + testprogram;

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.UseShellExecute = false;
            processStartInfo.Arguments = "/c" + strCmdText;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WorkingDirectory = firmwareDir;


            Process cmd = new Process();
            cmd.StartInfo = processStartInfo;
            cmd.Start();
            cmd.WaitForExit();

            string line = "";
            while (!cmd.StandardOutput.EndOfStream)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    line = cmd.StandardOutput.ReadLine();
                    if (line != null)
                    {
                        line.Trim();
                        if (line != "")
                        {

                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                FeedbackDisplay.Text = line;
                            }));
                            System.Threading.Thread.Sleep(delay);
                        }
                    }
                }
            }
            cmd.Close();
        }
    }
}
