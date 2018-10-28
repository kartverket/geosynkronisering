using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Ionic.Zip;
using Kartverket.Geosynkronisering.Subscriber.DL;
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
        /// <param name="dataset"></param>
        public bool DownloadChangelog(string downloadUri, Dataset dataset)
        {
            var webClient = new WebClient { Credentials = new NetworkCredential(dataset.UserName, dataset.Password) };
            webClient.DownloadFile(downloadUri, ChangelogFilename);
            if (File.Exists(ChangelogFilename))
            {
                UnpackZipFile(ChangelogFilename);

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

        public bool UnpackZipFile(string zipfile)
        {
            try
            {
                using (var zip = ZipFile.Read(zipfile, new ReadOptions { Encoding = Encoding.UTF8 }))
                {
                    zip.ToList().ForEach(entry =>
                    {
                        var fileName = Path.GetFileName(entry.FileName);
                        if (fileName != string.Empty) entry.FileName = fileName;
                        entry.Extract(zipfile.Replace(".zip",""), ExtractExistingFileAction.OverwriteSilently);
                    });
                }
            }

            catch (Exception ex)
            {
                Logger.Error(ex, "UnpackZipFile failed for file :" + zipfile);
                return false;
            }

            return true;
        }
    }
}