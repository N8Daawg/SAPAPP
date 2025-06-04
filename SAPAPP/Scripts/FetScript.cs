using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class FetScript(TextBlock fd, TextBlock pp, ProgressBar pb) : Script(fd, pp, pb)
    {
        private static readonly List<string> value = ["Connecting...", "loading...", "verifying..."];
        private readonly List<string> Milestones = value;

        public override async void Download(Part download)
        {
            if (!backgroundWorker.IsBusy)
            {
                currentDownload = download;
                backgroundWorker.RunWorkerAsync();
            }
        }

        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker? worker = sender as BackgroundWorker;

            string strCmdText = currentDownload.Executable;
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
                                if (line.Length > 0)
                                {
                                    UpdateProgress(line);
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
            if (line.Contains('%'))
            {
                words[^1] = words[^1].Trim('%');
                //MessageBox.Show(words[words.Length-1]);

                UpdateProgressBar(int.Parse(words[^1]));
            }



            string display = "";
            if (line.Contains("Connecting"))
            {
                display = Milestones[0];
            }
            else if (line.Contains("Loading"))
            {
                display = Milestones[1];
            }
            else if (line.Contains("Verifying"))
            {
                display = Milestones[2];
            }

            if (display != "")
            {
                Application.Current.Dispatcher.Invoke(() => { FeedbackDisplay.Text = display; });
                System.Threading.Thread.Sleep(delay);
            }
        }
    }
}
