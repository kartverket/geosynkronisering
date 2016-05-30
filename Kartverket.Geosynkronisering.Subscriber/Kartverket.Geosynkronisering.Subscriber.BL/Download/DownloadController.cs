using System;
using System.IO;
using Ionic.Zip;
using NLog;

namespace Kartverket.Geosynkronisering.Subscriber.BL
{
    public class DownloadController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

        public string ChangelogFilename { get; set; }
        public bool IsFolder = false;

        public DownloadController()
        {
            ChangelogFilename = string.Empty;
        }

        public DownloadController(string changelogFilename)
        {
            ChangelogFilename = changelogFilename;
        }

        private bool _downloadDone = false;

        /// <summary>
        /// Download changelog
        /// </summary>
        /// <param name="downloadUri"></param>
        public bool DownloadChangelog(string downloadUri)
        {
            string d = downloadUri;
            d = d.Substring(d.IndexOf('/') + 2);
            string[] par1 = d.Split('@');
            string ftpUser = par1[0].Split(':')[0];
            string ftpPasswd = par1[0].Split(':')[1];
            string ftpServer = par1[1].Split('/')[0];
            string ftpFileName = par1[1].Split('/')[1] + ".zip";
            var ftpHandler = new FileTransferHandler();
            ftpHandler.ProcessDone += ftpHandler_ProcessDone;
            if (ftpHandler.DownloadFileFromFtp(ChangelogFilename, ftpFileName, ftpServer, ftpUser, ftpPasswd))
            {
                if (Path.GetExtension(ChangelogFilename) != ".zip")
                {
                    Logger.ErrorException("File " + ChangelogFilename + " is not a zip file", null);
                    return false;
                }

                string outPath = Path.GetDirectoryName(ChangelogFilename);
                UnpackZipFile(ChangelogFilename, outPath);

                // TODO: HS: Check if zip contains folder or file
                string baseFilename = ChangelogFilename.Replace(".zip", "");

                if (Directory.Exists(baseFilename))
                {
                    ChangelogFilename = baseFilename;
                    IsFolder = true;
                }
                else
                {
                    string xmlFile = Path.ChangeExtension(ChangelogFilename, ".xml");
                    ChangelogFilename = xmlFile;
                }

                System.Diagnostics.Debug.WriteLine("client_DownloadFileCompleted: File downloaded");
                return true;
            }
            return false;
        }

        void ftpHandler_ProcessDone(object sender, FileTransferHandler.ProgressEventArgs e)
        {
            if (!e.error)
            {
                _downloadDone = e.status == FileTransferHandler.ftpStatus.done;
            }
        }

        public bool UnpackZipFile(string zipfile, string utpath)
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
                Logger.ErrorException("UnpackZipFile failed for file :" + zipfile, ex);
                return false;
            }

            return true;
        }
    }
}