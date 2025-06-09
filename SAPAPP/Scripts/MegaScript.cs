using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class MegaScript : Script
    {

        private string _avrdudeCLI;
        public string AVRDUDE_CLI
        {
            get { return string.Format("\"{0}\"", _avrdudeCLI); }
            set { _avrdudeCLI = value; }
        }

        private string boardType = "m2560";


        public MegaScript(TextBlock fd, TextBlock pp, ProgressBar pb, string cli) : base(fd, pp, pb)
        {
            AVRDUDE_CLI = cli;
        }
        public override void Download(Part download)
        {
            if (!backgroundWorker.IsBusy)
            {
                currentDownload = download;
                backgroundWorker.RunWorkerAsync();
            }
            //await UpdateProgressBar();
        }

        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            //string fullCommand = "avrdude -c avrispmkII -p m2560 -P usb:20:38 -U flash:w:\"C:\\Users\\nbealsla\\Downloads\\Blink\\build\\arduino.avr.megaADK\\Blink.ino.hex\":i";

            string connect = GetConnection();
            string write = "-U flash:w:" + currentDownload.Executable + ":i";
            string strCmdText = AVRDUDE_CLI + " " + connect + " " + write;
            
            ProcessStartInfo processStartInfo = new()
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                Arguments = testing ? "/k" + strCmdText : "/c" + strCmdText,
                RedirectStandardOutput = !testing,
                RedirectStandardError = !testing,
                CreateNoWindow = !testing,
                WorkingDirectory = currentDownload.FirmwarePath
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
                            HandleError(line);
                            break;
                        }


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
            }
            
            cmd.Close();

        }

        private string GetConnection()
        {
            string fullformat = "-c avrispmkII -p {0} -P {1}";
            string usbformat = "usb:{0}";

            string strCmdText = AVRDUDE_CLI + " " + string.Format(fullformat, boardType, string.Format(usbformat, "xxx")) + " -v";
            
            ProcessStartInfo processStartInfo = new()
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                Arguments = "/c" + strCmdText,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = currentDownload.FirmwarePath
            };

            Process cmd = new()
            {
                StartInfo = processStartInfo,
            };

            cmd.Start();
            cmd.WaitForExit();

            string results = cmd.StandardOutput.ReadToEnd();
            results += "\n";
            results = cmd.StandardError.ReadToEnd();
            cmd.Close();


            string lastFour = "";
            string[] lines = results.Split("\n");
            foreach (string line in lines)
            {
                if (line.ToLower().Contains("found"))
                {
                    string[] words = line.Split(' ');
                    string serno = words[words.Length-1];

                    string digitformat = "{0}{1}";
                    string firstTwo = string.Format(digitformat, serno[serno.Length-5], serno[serno.Length - 4]);
                    string lastTwo = string.Format(digitformat, serno[serno.Length - 3], serno[serno.Length - 2]);

                    lastFour = string.Format("{0}:{1}", firstTwo, lastTwo);
                }
            }
            string serial = string.Format(usbformat, lastFour);

            return string.Format(fullformat, boardType, serial);
        }

        protected override void HandleError(string line)
        {
            string message, header;

            message = line;
            header = "Error";

            MessageBox.Show(message, header, MessageBoxButton.OK, MessageBoxImage.Error);
            Cancel();
        }

        protected override void UpdateProgress(string line)
        {
            throw new NotImplementedException();
        }
    }
}
