using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class TestScript(TextBlock fd, TextBlock pp, ProgressBar pb) : Script(fd, pp, pb)
    {
        public override void Download(Part download)
        {
            if (!backgroundWorker.IsBusy)
            {
                currentDownload = download;
                backgroundWorker.RunWorkerAsync();
            }
        }

        // This event handler is where the time-consuming work is done.
        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            string strCmdText = "dir";
            string firmwareDir = workingDirectory;


            ProcessStartInfo processStartInfo = new()
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                Arguments = "/c" + strCmdText,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = firmwareDir,
            };


            try
            {
                Process cmd = new()
                {
                    StartInfo = processStartInfo
                };
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
                            line = line.Trim();
                            if (line != "")
                            {
                                UpdateProgress(line);
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
            throw new NotImplementedException();
        }

        protected override void UpdateProgress(string line)
        {
            string[] words = line.Split(' ');

            string news = "";
            int i = 0;
            foreach (string word in words)
            {
                news += i + " " + word + "\n";
                i++;
            }
            //MessageBox.Show(news);
            Application.Current.Dispatcher.Invoke(() => { FeedbackDisplay.Text = line; });
            System.Threading.Thread.Sleep(delay);
        }
    }
}
