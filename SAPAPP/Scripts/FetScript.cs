using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SAPAPP.Scripts
{
    internal class FetScript : Script
    {
        private const string localBatInstallDir = @"\firmware\Sensit_G2";
        private const string loadfile = "dslite.bat";
        private const string configFile = "one_time_setup.bat";

        public FetScript() 
            : base(){}


        public override void Download()
        {
            string strCmdText = loadfile;
            string firmwareDir = workingDirectory + localBatInstallDir;


            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.UseShellExecute = false;
            if (testing)
            {
                processStartInfo.Arguments = "/k" + strCmdText;
                processStartInfo.RedirectStandardOutput = false;
                processStartInfo.RedirectStandardError = false;
                processStartInfo.CreateNoWindow = false;
            }
            else
            {
                processStartInfo.Arguments = "/c" + strCmdText;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.CreateNoWindow = true;
            }
            processStartInfo.WorkingDirectory = firmwareDir;


            Process cmd = new Process();
            cmd.StartInfo = processStartInfo;
            cmd.Start();
            cmd.WaitForExit();

            string results = "";
            if (!testing)
            {
                results = cmd.StandardOutput.ReadToEnd();
            }
            
            cmd.Close();

            MessageBox.Show(results);
        }
    }
}
