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

            var tries = 0;

            var waitMilliseconds = 300;

            while (tries < 10)
            {
                try
                {
                    webClient.DownloadFile(downloadUri, ChangelogFilename);
                    break;
                }
                catch (Exception e)
                {
                    System.Threading.Thread.Sleep(waitMilliseconds);

                    waitMilliseconds *= 2;

                    tries += 1;

                    if (tries == 9) throw;
                }
            }
            
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
                // Check encoding to fix filanames for "ø", e.g. Høydekurve.
                Encoding encoding = Encoding.UTF8;
                if (CheckEncoding(zipfile, encoding))
                {
                    encoding = null; // not UTF-8, use default encoding for unpacking zip-fle
                }

                // using (var zip = ZipFile.Read(zipfile, new ReadOptions { Encoding = Encoding.UTF8 }))
                using (var zip = ZipFile.Read(zipfile, new ReadOptions { Encoding = encoding }))
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

        /// <summary>
        /// Check the filenames in a zip-file for encoding
        /// </summary>
        /// <param name="zipfile"></param>
        /// <param name="encoding"></param>
        /// <returns>true if encoding error, false if OK</returns>
        private static bool CheckEncoding(string zipfile, Encoding encoding)
        {
            var encodingError = false;
            using (var zip = ZipFile.Read(zipfile, new ReadOptions { Encoding = encoding }))
            {
                zip.ToList().ForEach(entry =>
                {
                    if (!encodingError)
                    {
                        var fileName = Path.GetFileName(entry.FileName);
                        if (fileName != string.Empty) entry.FileName = fileName;

                        if (Encoding.UTF8.GetChars(Encoding.UTF8.GetBytes(entry.FileName)).Any(ch => ch == 65533))
                        {
                            encodingError = true;
                        }
                    }
                });

            }
            return encodingError;
        }

    }
}