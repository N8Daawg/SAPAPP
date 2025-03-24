using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

public class FETScripts
{
    private string workingDirectory;
    private const string localBatInstallDir = @"\firmware\productConfig";
    private const string loadfile = "dslite.bat";

    private const bool testing = false;
    


    public FETScripts()
	{
        this.workingDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
	}

	public string MSP430Download()
	{


        string strCmdText = "dslite.bat";
        string firmwareDir = this.workingDirectory + localBatInstallDir;
        

        ProcessStartInfo processStartInfo = new ProcessStartInfo();
        processStartInfo.FileName = "cmd.exe";
        processStartInfo.Arguments = "/k" + strCmdText;
        processStartInfo.UseShellExecute = false;
        if (testing)
        {
            processStartInfo.RedirectStandardOutput = false;
            processStartInfo.RedirectStandardError = false;
            processStartInfo.CreateNoWindow = false;
        }
        else
        {
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.CreateNoWindow = true;
        }
        processStartInfo.WorkingDirectory = firmwareDir;
        

        Process cmd = new Process();
        cmd.StartInfo = processStartInfo;
        cmd.Start();
        cmd.WaitForExit();


        string str = ""; // cmd.StandardOutput.ReadToEnd();
        cmd.Close();

        MessageBox.Show(str);
        return str;

	}
}
