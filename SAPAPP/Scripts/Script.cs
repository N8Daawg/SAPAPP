﻿using SAPAPP.Configs;
using System.ComponentModel;
using System.IO;
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

        protected Part currentDownload = new();

        // Feedback Devices
        protected TextBlock FeedbackDisplay;
        protected TextBlock progressPercentage;
        protected ProgressBar progbar;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fd"></param>
        /// <param name="pp"></param>
        /// <param name="pb"></param>
        public Script(TextBlock fd, TextBlock pp, ProgressBar pb)
        {
            FeedbackDisplay = fd;
            progressPercentage = pp;
            progbar = pb;
            workingDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

            backgroundWorker = InitializeBackgroundWorker();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private BackgroundWorker InitializeBackgroundWorker()
        {
            BackgroundWorker worker = new()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };

            worker.DoWork +=
                new DoWorkEventHandler(BackgroundWorker_DoWork);
            worker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
            worker.ProgressChanged +=
                new ProgressChangedEventHandler(BackgroundWorker_ProgressChanged);

            return worker;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="download"></param>
        public abstract void Download(Part download);

        /// <summary>
        /// 
        /// </summary>
        public void Cancel()
        {
            if (backgroundWorker.IsBusy)
            {
                backgroundWorker.CancelAsync();
            }
        }


        // This event handler is where the time-consuming work is done.
        protected abstract void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e);


        // This event handler updates the progress.
        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //FeedbackDisplay.Text = (e.ProgressPercentage.ToString() + "%");
        }

        // This event handler deals with the results of the background operation.
        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="line"></param>
        protected abstract void HandleError(BackgroundWorker worker, string line);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        protected abstract void UpdateProgress(string line);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="progress"></param>
        protected void UpdateProgressBar(int progress)
        {

            Application.Current.Dispatcher.Invoke(() => { progbar.Value = progress; });

        }
    }
}
