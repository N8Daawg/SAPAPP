using System;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace SAPAPP
{
    // Configuration Class
    public class ProductConfig
    {
        public string LoadFile { get; set; } = "default.bat";
        public string FirmwareFolderPath { get; set; } = @"C:\DefaultPath";
    }

    // Fuel Gauge Programming Script
    class TIScripts
    {
        private SerialPort serialPort;
        private const int BaudRate = 115200;
        private BackgroundWorker backgroundWorker;
        private bool testing;
        private int delay = 500;
        private string logFilePath = "fuel_gauge_log.txt";
        private ProductConfig currentDownload;

        public TIScripts(string portName, ProductConfig product, bool testMode)
        {
            serialPort = new SerialPort(portName, BaudRate);
            testing = testMode;
            currentDownload = product;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_Completed;

            serialPort.DataReceived += SerialPort_DataReceived;
        }

        public void Initialize()
        {
            try
            {
                serialPort.Open();
                LogMessage("Serial port opened successfully.");
                Console.WriteLine("Press 'f' to program fuel gauge...");
            }
            catch (Exception ex)
            {
                LogMessage($"Error opening serial port: {ex.Message}");
                Console.WriteLine($"Failed to open serial port: {ex.Message}");
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            char inByte = (char)serialPort.ReadByte();
            if (inByte == 'f')
            {
                LogMessage("Firmware programming initiated.");
                backgroundWorker.RunWorkerAsync();
            }
        }

        protected void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            string firmwareDir = currentDownload.FirmwareFolderPath;
            string strCmdText = currentDownload.LoadFile;

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                Arguments = testing ? "/k " + strCmdText : "/c " + strCmdText,
                RedirectStandardOutput = !testing,
                RedirectStandardError = !testing,
                CreateNoWindow = !testing,
                WorkingDirectory = firmwareDir
            };

            try
            {
                Process cmd = new Process { StartInfo = processStartInfo };
                cmd.Start();
                cmd.WaitForExit();

                if (!testing)
                {
                    string line;
                    while (!cmd.StandardOutput.EndOfStream)
                    {
                        if (worker.CancellationPending)
                        {
                            e.Cancel = true;
                            break;
                        }

                        line = cmd.StandardError.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            HandleError(worker, line);
                            break;
                        }

                        line = cmd.StandardOutput.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            Application.Current.Dispatcher.Invoke(() => Console.WriteLine(line));
                            Thread.Sleep(delay);
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

        private void BackgroundWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                LogMessage("Firmware programming cancelled.");
                Console.WriteLine("Firmware programming cancelled.");
            }
            else if (e.Error != null)
            {
                LogMessage($"Error during programming: {e.Error.Message}");
                Console.WriteLine($"Error during programming: {e.Error.Message}");
            }
            else
            {
                LogMessage("Programming PASSED.");
                Console.WriteLine("Programming PASSED.");
            }
        }

        private void HandleError(BackgroundWorker worker, string line)
        {
            string message, header;
            line = line.Trim();

            if (line.ToLower().Contains("system cannot find the path specified"))
            {
                message = line + "\nFirmware file missing or incorrectly named.";
                header = "Firmware File Error";
            }
            else
            {
                message = line;
                header = "Error";
            }

            MessageBox.Show(message, header, MessageBoxButton.OK, MessageBoxImage.Error);
            worker.CancelAsync();
        }

        private void LogMessage(string message)
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}