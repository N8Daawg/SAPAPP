using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class STMScript : Script
    {
        private string STM32_Programmer_CLI;


        public STMScript(TextBlock fd, TextBlock pp, ProgressBar pb, string cli) : base(fd, pp, pb)
        {
            STM32_Programmer_CLI = cli;
        }

        public override void Download(ProductConfig product)
        {
            if(!backgroundWorker.IsBusy)
            {
                currentDownload = product;
                backgroundWorker.RunWorkerAsync();
            }
        }

        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker? worker = sender as BackgroundWorker;

            //extra data needed
            string writeHead = "0x08000000";

            // basic commands
            string connect = "-c port=SWD";                                         // connect to board command
            string write = "-w " + currentDownload.Executable + " " + writeHead;    // write command

            string strCmdText = STM32_Programmer_CLI + " "+ connect +" "+ write;
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
                    StartInfo = processStartInfo,
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
                                if (line.Length > 0)
                                {
                                    if (line.Contains("Error"))
                                    {
                                        HandleError(worker, line);
                                    }
                                    else
                                    {
                                        UpdateProgress(line);
                                    }
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

            message = line;
            header = "Error";

            MessageBox.Show(message, header, MessageBoxButton.OK, MessageBoxImage.Error);
            Cancel();
        }

        protected override void UpdateProgress(string line)
        {
            Application.Current.Dispatcher.Invoke(() => { FeedbackDisplay.Text = line; });
            System.Threading.Thread.Sleep(delay);
        }
    }
}
