using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class MegaScript(TextBlock fd, TextBlock pp, ProgressBar pb) : Script(fd, pp, pb)
    {

        private const string localInstallDir = @"\firmware\STMPrograms";
        private const string testprogram = @"\STM32BIG\build\arduino.avr.megaADK";
        private const string cliPath = @"C:\Program Files (x86)\Atmel\Studio\7.0\atbackend\atprogram.exe";

        public override void Download(ProductConfig product)
        {
            if (!backgroundWorker.IsBusy)
            {
                currentDownload = product;
                backgroundWorker.RunWorkerAsync();
            }
            //await UpdateProgressBar();
        }

        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            string strCmdText = cliPath + " -t avrispmk2 -i ISP -d atmega2560 program -f megaADK.ino.elf";
            string firmwareDir = workingDirectory;

            ProcessStartInfo processStartInfo = new()
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                Arguments = testing ? "/k" + strCmdText : "/c" + strCmdText,
                RedirectStandardOutput = !testing,
                RedirectStandardError = !testing,
                CreateNoWindow = !testing,
                WorkingDirectory = firmwareDir
            };


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
                            line = line.Trim();
                            HandleError(worker, line);
                            break;
                        }


                        line = cmd.StandardOutput.ReadLine();
                        if (line != null)
                        {
                            line = line.Trim();
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

        protected override void HandleError(BackgroundWorker worker, string line)
        {
            string message, header;

            message = line;
            header = "Error";

            MessageBox.Show(message, header, MessageBoxButton.OK, MessageBoxImage.Error);
            worker.CancelAsync();
        }

        protected override void UpdateProgress(string line)
        {
            throw new NotImplementedException();
        }
    }
}
