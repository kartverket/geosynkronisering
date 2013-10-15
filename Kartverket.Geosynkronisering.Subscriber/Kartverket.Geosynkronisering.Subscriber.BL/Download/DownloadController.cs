using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using Ionic.Zip;
using NLog;

namespace Kartverket.Geosynkronisering.Subscriber.BL
{
    public class DownloadController
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

        public string DownloadFilename { get; set; }

        public DownloadController()
        {
            DownloadFilename = string.Empty;
        }

        public DownloadController(string downloadFilename)
        {
            DownloadFilename = downloadFilename;
        }

        private bool DownloadDone = false;

        /// <summary>
        /// Download changelog
        /// </summary>
        /// <param name="downloadUri"></param>
        /// <param name="localFileName"> Filename including path </param>
        private bool DownloadChangelog2(string downloadUri, string localFileName)
        {
            string d = downloadUri;
            d = d.Substring(d.IndexOf('/') + 2);
            string[] par1 = d.Split('@');
            string ftpUser = par1[0].Split(':')[0];
            string ftpPasswd = par1[0].Split(':')[1];
            string ftpServer = par1[1].Split('/')[0];
            string ftpFileName = par1[1].Split('/')[1] + ".zip";
            var ftpHandler = new FileTransferHandler();
            ftpHandler.ProgressChanged += new FileTransferHandler.ProgressHandler(ftpHandler_ProgressChanged);
            ftpHandler.ProcessDone += new FileTransferHandler.ProcessDoneHandler(ftpHandler_ProcessDone);
            if (ftpHandler.DownloadFileFromFtp(localFileName, ftpFileName, ftpServer, ftpUser, ftpPasswd))
            {
                if (Path.GetExtension(localFileName) != ".zip")
                {
                    logger.ErrorException("File " + localFileName + " is not a zip file", null);
                    return false;
                }

                string outPath = Path.GetDirectoryName(DownloadFilename);
                this.unpackZipFile(DownloadFilename, outPath);
                string xmlFile = Path.ChangeExtension(DownloadFilename, ".xml");
                DownloadFilename = xmlFile;

                System.Diagnostics.Debug.WriteLine("client_DownloadFileCompleted: File downloaded");
                return true;
            }
            else
            { return false; }
        }

        void ftpHandler_ProcessDone(object sender, FileTransferHandler.ProgressEventArgs e)
        {
            if (!e.error)
            {
                DownloadDone = e.status == FileTransferHandler.ftpStatus.done;
                //toolStripProgressBar1.Maximum = e.totalFiles;
                //toolStripProgressBar1.Value = e.currentFile;
                //toolStripStatusLabel1.Text = "Downloaded file: " + e.fileName;
            }
        }

        void ftpHandler_ProgressChanged(object sender, FileTransferHandler.ProgressEventArgs e)
        {
            if (!e.error)
            {
                //toolStripProgressBar1.Maximum = e.totalFiles;
                //toolStripProgressBar1.Value = e.currentFile;
                //toolStripStatusLabel1.Text = "Downloading file: " + e.fileName;
            }

        }


        /// <summary>
        /// This event is raised each time an asynchronous file download operation completes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {           
            if (e.Error == null)
            {
                if (Path.GetExtension(DownloadFilename) != ".zip")
                {
                    logger.ErrorException("File " + DownloadFilename + " is not a zip file", null);
                    return;
                }

                var client = (WebClient)sender;

#if !(NOT_FTP)
                string outPath = Path.GetDirectoryName(DownloadFilename);

                FileInfo fileInfo = new FileInfo(DownloadFilename);
                int retry_counter = 0;

                while ((!fileInfo.Exists || fileInfo.Length == 0) && retry_counter < 5)
                {
                    logger.ErrorException("File " + DownloadFilename + " is empty, counter = " + retry_counter, null);
                    Thread.Sleep(2000);
                    fileInfo.Refresh();
                    retry_counter++;
                }

                if (retry_counter == 4)
                {
                    logger.ErrorException("File " + DownloadFilename + " is empty", null);                  
                    System.Diagnostics.Debug.WriteLine("client_DownloadFileCompleted failed");
                }

                this.unpackZipFile(DownloadFilename, outPath);

                string localFileName = Path.ChangeExtension(DownloadFilename, ".xml");
                DownloadFilename = localFileName;
#endif
              
                System.Diagnostics.Debug.WriteLine("client_DownloadFileCompleted: File downloaded");
            }
            else
            {
                logger.ErrorException(e.Error.ToString(), null);            
                System.Diagnostics.Debug.WriteLine("client_DownloadFileCompleted failed");
            }
        }

        private bool unpackZipFile(string zipfile, string utpath)
        {
            try
            {
                using (ZipFile zip = ZipFile.Read(zipfile))
                {
                    foreach (ZipEntry fil in zip)
                    {
                        fil.Extract(utpath, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }

            catch (Exception ex)
            {
                logger.ErrorException("unpackZipFile failed for file :" + zipfile, ex);
                return false;
            }

            return true;
        }
    }
}

