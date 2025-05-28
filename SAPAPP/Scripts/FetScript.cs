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

            if (!testing)
            {
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
                        // read from standard Error to see if there was a mistake
                        line = cmd.StandardError.ReadLine();
                        if (line != null)
                        {
                            line.Trim();
                            string message, header;
                            if (line.ToLower().Contains("system cannot find the path specified")) // package needs to be recompiled
                            {
                                message = line + "\nRecompile Package and try again.";
                                header = "Package Error";
                            }
                            else if (line.ToLower().Contains("no usb fet"))
                            {
                                message = line;
                                header = "Connection Error";
                            }
                            else
                            {
                                message = line;
                                header = "Error";
                            }

                            MessageBox.Show(message, header, MessageBoxButton.OK, MessageBoxImage.Error);
                            worker.CancelAsync();
                            break;
                        }


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
            }
            cmd.Close();
        }
    }
}
