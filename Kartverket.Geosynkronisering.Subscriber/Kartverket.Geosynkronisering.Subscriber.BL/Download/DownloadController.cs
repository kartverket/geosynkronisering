using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel;
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
            var webClient = new WebClient {Credentials =  GetCredentials(downloadUri)};
            webClient.DownloadFile(downloadUri, ChangelogFilename);
            if (File.Exists(ChangelogFilename))
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

        private static NetworkCredential GetCredentials(string downloadUri)
        {
            Dictionary<string, string> logonDictionary= new Dictionary<string, string>();
            
            string d = downloadUri;
            d = d.Substring(d.IndexOf('/') + 2);
            string[] par1 = d.Split('@');

            logonDictionary["username"] = par1[0].Split(':')[0];
            logonDictionary["password"] = par1[0].Split(':')[1];
            logonDictionary["domain"] = par1[1].Split('/')[0];
            logonDictionary["filename"] = par1[1].Split('/')[1] + ".zip";

            return new NetworkCredential(logonDictionary["username"], logonDictionary["password"]);
        }

        public bool UnpackZipFile(string zipfile, string utpath)
        {
            try
            {
                using (var zip = ZipFile.Read(zipfile))
                {
                    foreach (var fil in zip)
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