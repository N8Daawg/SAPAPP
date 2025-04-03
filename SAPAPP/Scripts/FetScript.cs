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
    internal class FetScript(TextBlock fd, TextBlock pp, ProgressBar pb) : Script(fd, pp, pb)
    {
        private const string localBatInstallDir = @"\firmware\FetPrograms\Sensit_G2";
        private const string loadfile = "dslite.bat";


        protected override void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            string strCmdText = loadfile;
            string firmwareDir = workingDirectory + localBatInstallDir;


            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "cmd.exe";
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
