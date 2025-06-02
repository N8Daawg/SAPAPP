using SAPAPP.Configs;
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
        private const string loadfile = "dslite.bat";

        public override async void Download(ProductConfig product) 
        {
            if(!backgroundWorker.IsBusy)
            {
                currentDownload = product;
                backgroundWorker.RunWorkerAsync();
            }
            //await UpdateProgressBar();
        }

        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker? worker = sender as BackgroundWorker;

            string strCmdText = loadfile;
            //string firmwareDir = workingDirectory + localBatInstallDir;
            string firmwareDir = currentDownload.FirmwarePath;


            ProcessStartInfo processStartInfo = new()
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                Arguments = testing ? "/k" + strCmdText : "/c" + strCmdText,
                RedirectStandardOutput = !testing,
                RedirectStandardError = !testing,
                CreateNoWindow = !testing,
                WorkingDirectory = firmwareDir,
            };

            try
            {

                Process cmd = new()
                {
                    StartInfo = processStartInfo
                };
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
                                HandleError(worker, line);
                                break;
                            }


                            line = cmd.StandardOutput.ReadLine();
                            if (line != null)
                            {
                                line = line.Trim();
                                if (line != "")
                                {
                                    Application.Current.Dispatcher.Invoke(() => { FeedbackDisplay.Text = line; });
                                    System.Threading.Thread.Sleep(delay);
                                }
                            }
                        }
                    }
                }
                cmd.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        protected override void HandleError(BackgroundWorker worker, string line)
        {
            line = line.Trim();
            string message, header;
            if (line.Contains("system cannot find the path specified", StringComparison.CurrentCultureIgnoreCase)) // package needs to be recompiled
            {
                message = line + "\nRecompile Package and try again.";
                header = "Package Error";
            }
            else if (line.Contains("no usb fet", StringComparison.CurrentCultureIgnoreCase))
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
            Cancel();
        }
    }
}
