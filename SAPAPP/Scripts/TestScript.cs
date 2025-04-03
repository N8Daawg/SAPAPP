using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace SAPAPP.Scripts
{
    internal class TestScript(TextBlock fd) : Script(fd)
    {

        // This event handler is where the time-consuming work is done.
        protected override void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            string strCmdText = "dir";
            string firmwareDir = workingDirectory;


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

            //List<string> list = new List<string>();
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
