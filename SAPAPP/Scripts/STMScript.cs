﻿using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class STMScript : Script
    {
        private string _stm32_prog_cli;
        public string STM32_Programmer_CLI
        {
            get { return string.Format("\"{0}\"", _stm32_prog_cli); }
            set { _stm32_prog_cli = value; }
        }


        public STMScript(TextBlock fd, TextBlock pp, ProgressBar pb, string cli) : base(fd, pp, pb)
        {
            STM32_Programmer_CLI = cli;
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

            //extra data needed
            string writeHead = "0x08000000";

            // basic commands
            string connect = "-c port=SWD";                                                 // connect to board command
            string write = "-w " + currentDownload.FullFirmwarePath() + " " + writeHead;    // write command

            string strCmdText = STM32_Programmer_CLI + " " + connect + " " + write;
            string firmwareDir = currentDownload.FirmwareFolder;

            
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
            line = line.Trim();
            string message, header;

            message = line;
            header = "Error";

            MessageBox.Show(message, header, MessageBoxButton.OK, MessageBoxImage.Error);
            Cancel();
        }

        protected override void UpdateProgress(string line)
        {
            int? progress = null;
            string? DisplayMessage = null;

            line = line.Trim();
            string[] words = line.Split(' ');

            if (line.Contains("Error:"))
            {
                HandleError(line);
                return;
            }
            else if (!line.Contains("--"))
            {
                DisplayMessage = line;
            }


            if (line.Contains("download complete"))
            {
                progress = 100;
            }
            else if (line.Contains('%'))
            {
                progress = int.Parse(words[^1].Trim('%'));
                DisplayMessage = null;
            }

            UpdateProgressFeedback(progress, DisplayMessage);
        }
    }
}
