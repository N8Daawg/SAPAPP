using SAPAPP.Configs;
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
        protected const bool testing = false;
        protected const int delay = 100; // delay time in milliseconds

        protected ProductConfig currentDownload = new();

        // Feedback Devices
        protected TextBlock FeedbackDisplay;
        protected TextBlock progressPercentage;
        protected ProgressBar progbar;

        public Script(TextBlock fd, TextBlock pp, ProgressBar pb)
        {
            FeedbackDisplay = fd;
            progressPercentage = pp;
            progbar = pb;
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

        public abstract void Download(ProductConfig product);

        public void Cancel()
        {
            if (backgroundWorker.IsBusy)
            {
                backgroundWorker.CancelAsync();
            }
        }


        // This event handler is where the time-consuming work is done.
        protected abstract void backgroundWorker_DoWork(object sender, DoWorkEventArgs e);


        // This event handler updates the progress.
        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //FeedbackDisplay.Text = (e.ProgressPercentage.ToString() + "%");
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


        protected async Task UpdateProgressBar()
        {
            for (int i = 0; i <= 100; i++)
            {
                progbar.Value = i;
                progressPercentage.Text = $"{i}%";
                await Task.Delay(50);
            }
        }
    }
}
