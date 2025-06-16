using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class FetScript: Script
    {
        public string ToolsFolder { get; set; }

        public FetScript(TextBlock fd, TextBlock pp, ProgressBar pb, string folder) : base(fd, pp, pb)
        {
            ToolsFolder = folder;
        }

        public override void Download(Part download)
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

            string strCmdText = string.Format("FetExecutor.bat \"{0}\" \"user_files\\configs\\{1}.ccxml\"", currentDownload.FullFirmwarePath(), @"MSP430F2418");

            Process cmd = new()
            {
                StartInfo = new()
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    Arguments = testing ? "/k" + strCmdText : "/c" + strCmdText,
                    RedirectStandardOutput = !testing,
                    RedirectStandardError = !testing,
                    CreateNoWindow = !testing,
                    WorkingDirectory = ToolsFolder,
                }
            };

            cmd.OutputDataReceived += Cmd_OutputDataReceived;
            cmd.ErrorDataReceived += Cmd_ErrorDataReceived;


            cmd.Start();
            if (!testing)
            {
                cmd.BeginErrorReadLine();
                cmd.BeginOutputReadLine();
                cmd.WaitForExitAsync();

                while ((worker.IsBusy) && (!cmd.HasExited))
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        cmd.CancelErrorRead();
                        cmd.CancelOutputRead();
                        cmd.Kill();
                    }
                }
            }

            if (!cmd.HasExited)
            {
                cmd.Close();
            }
        }

        private void Cmd_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                HandleError(e.Data);
            }
        }

        private void Cmd_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                UpdateProgress(e.Data);
            }
        }

        protected override void HandleError(string line)
        {
            Cancel();

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
        }

        protected override void UpdateProgress(string line)
        {
            int? progress = null;
            string? DisplayMessage = null;


            line = line.Trim();
            string[] words = line.Split(' ');
            if (line.Contains("Finished"))
            {
                progress = 100;
            }
            else if (line.Contains('%'))
            {
                progress = int.Parse(words[^1].Trim('%'));
            }

            if (line.Contains("Configuring"))
            {
                DisplayMessage = line;
            }
            else if (line.Contains("Initializing"))
            {
                DisplayMessage = line + "...";
            }
            else if (line.Contains("Connecting"))
            {
                DisplayMessage = line;
            }
            else if (line.Contains("Loading"))
            {
                DisplayMessage = words[0] + ' ' + words[1] + ' ' + currentDownload.FullFirmwarePath();
            }
            else if (line.Contains("Verifying"))
            {
                DisplayMessage = words[0] + ' ' + words[1] + ' ' + currentDownload.FullFirmwarePath();
            }


            UpdateProgressFeedback(progress, DisplayMessage);
        }
    }
}
