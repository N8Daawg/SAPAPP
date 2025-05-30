using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class TestScript(TextBlock fd, TextBlock pp, ProgressBar pb) : Script(fd, pp, pb)
    {
        public override void Download(ProductConfig product)
        {
            if (!backgroundWorker.IsBusy)
            {
                currentDownload = product;
                backgroundWorker.RunWorkerAsync();
            }
        }

        // This event handler is where the time-consuming work is done.
        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
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

            string line = "";
            while (!cmd.StandardOutput.EndOfStream)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        FeedbackDisplay.Text = "Canceled!";
                    }));
                    System.Threading.Thread.Sleep(delay);
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

        protected override void HandleError(BackgroundWorker worker, string line)
        {
            throw new NotImplementedException();
        }
    }
}
