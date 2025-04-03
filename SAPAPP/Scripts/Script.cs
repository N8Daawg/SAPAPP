using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal abstract class Script
    {

        protected BackgroundWorker backgroundWorker;
        protected string workingDirectory;
        protected const bool testing = true;
        protected const int delay = 300; // delay time in milliseconds
        protected TextBlock FeedbackDisplay;


        public Script(TextBlock fd)
        {
            FeedbackDisplay = fd;
            workingDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            
            InitializeBackgroundWorker();
        }

        private void InitializeBackgroundWorker()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;

            backgroundWorker.DoWork +=
                new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            backgroundWorker.ProgressChanged +=
                new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);
        }

        public void Download()
        {
            if(!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }


        // This event handler is where the time-consuming work is done.
        protected abstract void backgroundWorker_DoWork(object sender, DoWorkEventArgs e);


        // This event handler updates the progress.
        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            FeedbackDisplay.Text = (e.ProgressPercentage.ToString() + "%");
        }

        // This event handler deals with the results of the background operation.
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                FeedbackDisplay.Text = "Canceled!";
            }
            else if (e.Error != null)
            {
                FeedbackDisplay.Text = "Error: " + e.Error.Message;
            }
            else
            {
                FeedbackDisplay.Text = "Done!";
            }
        }
    }
}
