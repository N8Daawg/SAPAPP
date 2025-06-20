﻿using SAPAPP.Configs;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal abstract class Script
    {
        protected BackgroundWorker backgroundWorker;
        protected const bool testing = false;
        protected const int delay = 0; // delay time in milliseconds

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
            Application.Current.Dispatcher.Invoke(new Action(() =>
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
                    MessageBox.Show("Download Finished Successfully!", "Download Finished", MessageBoxButton.OK, MessageBoxImage.Information);
                    FeedbackDisplay.Text = "Done!";
                }
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="line"></param>
        protected abstract void HandleError(string line);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        protected abstract void UpdateProgress(string line);

        protected void UpdateProgressFeedback(int? progress, string message)
        {
            if ((progress != null) && (progress > 100))
            {
                progress = 100;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (progress != null)
                {
                    progbar.Value = (double)progress;
                    progressPercentage.Text = progress.ToString() + '%';
                }
                if ((message != null) && (message != ""))
                { 
                    FeedbackDisplay.Text = message; 
                }
            });
            System.Threading.Thread.Sleep(delay);
        }
    }
}
