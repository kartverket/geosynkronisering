using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Globalization;
// using System.Xml;
// using System.Xml.Serialization;
// using System.Xml.Linq;


namespace Kartverket.Geosynkronisering
{
    public class ChangelogManager
    {
        private geosyncEntities db;

        public ChangelogManager(geosyncEntities _db)
        {
            db = _db;
        }

        public System.Xml.XmlDocument GetCapabilities()
        {
            //TODO FIX this mockup
            
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            string filePath;
            filePath = string.Format(CultureInfo.InvariantCulture, "{0}\\GetCapabilities.xml", Utils.App_DataPath);
            xmlDoc.Load(filePath);
            return xmlDoc;
        }

        public System.Xml.XmlDocument DescribeFeatureType(int datasetId)
        {
            var dataset = from d in db.Datasets where d.DatasetId == datasetId select d;

            //get xsd file from dataset table
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            string filePath = Utils.BaseSiteUrl + dataset.First().SchemaFileUri;
            //xmlDoc.Load(filePath);
            //XElement retElement = XElement.Load(new XmlNodeReader(xmlDoc));
            //return retElement;
            return xmlDoc;
        }

        public Kartverket.GeosyncWCF.ListStoredChangelogsResponse ListStoredChangelogs(int datasetId)
        {
            Kartverket.GeosyncWCF.ListStoredChangelogsResponse resp = new Kartverket.GeosyncWCF.ListStoredChangelogsResponse();

            var changelogs = from c in db.StoredChangelogs where c.Stored == true && c.DatasetId == datasetId select c;

            List<Kartverket.GeosyncWCF.StoredChangelogType> ret = new List<Kartverket.GeosyncWCF.StoredChangelogType>();

            foreach (var ch in changelogs)
            {
                Kartverket.GeosyncWCF.StoredChangelogType stch = new Kartverket.GeosyncWCF.StoredChangelogType();
                stch.id = new Kartverket.GeosyncWCF.ChangelogIdentificationType();
                stch.id.changelogId = ch.ChangelogId.ToString();
                stch.downloadUri = Utils.BaseSiteUrl + ch.DownloadUri;
                stch.endIndex = ch.EndIndex.ToString();
                stch.name = ch.Name;
                stch.order = new Kartverket.GeosyncWCF.ChangelogOrderType();
                //TODO set filter
                
                stch.startIndex = ch.StartIndex.ToString();

                ret.Add(stch);

            }

            resp.@return = ret.ToArray();

            return resp;
        }

        public Kartverket.GeosyncWCF.ChangelogStatusType GetChangelogStatus(int changelogid)
        {
            var changelog = (from c in db.StoredChangelogs where c.ChangelogId == changelogid select c).First();

            Kartverket.GeosyncWCF.ChangelogStatusType resp = new GeosyncWCF.ChangelogStatusType();
            switch (changelog.Status)
            {

                case "started": resp = Kartverket.GeosyncWCF.ChangelogStatusType.started;break;
                case "working": resp = Kartverket.GeosyncWCF.ChangelogStatusType.working; break;
                case "cancelled": resp = Kartverket.GeosyncWCF.ChangelogStatusType.cancelled; break;
                case "finished": resp = Kartverket.GeosyncWCF.ChangelogStatusType.finished; break;
            }

            return resp;
        }

        public Kartverket.GeosyncWCF.ChangelogType GetChangelog(int changelogid)
        {
            var changelog = (from c in db.StoredChangelogs where c.ChangelogId == changelogid select c).First();

            Kartverket.GeosyncWCF.ChangelogType resp = new Kartverket.GeosyncWCF.ChangelogType();

            if (changelog != null)
            {


                resp.id = new Kartverket.GeosyncWCF.ChangelogIdentificationType();
                resp.id.changelogId = changelog.ChangelogId.ToString();
               // resp.@return.downloadUri = Utils.BaseSiteUrl + changelog.DownloadUri;
                resp.downloadUri = changelog.DownloadUri; // OKA changed 20121030
                resp.endIndex = changelog.EndIndex.Value.ToString();
            }

            return resp;
        }

        public void AcknowledgeChangelogDownloaded(int changelogid)
        {                   
            DateTime deleteDate = DateTime.Now.AddDays(-10); // TODO: Add to dataset settings

            // Find and delete obsolete files from database and server
            var obsoleteFiles = (from c in db.StoredChangelogs where c.Status == "started" && c.DateCreated < deleteDate select c);
            // TODO: Should be replaced by: var obsoleteFiles = (from c in db.StoredChangelogs where (c.Status == "started" || (c.Status == "Finished" && c.Stored == false)) && c.DateCreated < deleteDate select c);

            foreach (var of in obsoleteFiles)
            {                                
                if (of.DownloadUri != null) //TODO: Quick fix of permanent?
                {
                    var downloadUri = new Uri(of.DownloadUri);
                    DeleteFileOnServer(downloadUri);
                    db.StoredChangelogs.DeleteObject(of);                   
                }
            }

            db.SaveChanges();


            //Delete files and db row if not a stored one...
            var changelog = (from c in db.StoredChangelogs where c.ChangelogId == changelogid select c).First();
            
            if (changelog != null)
            {
                if (changelog.Stored == false)
                {
                    var downloadUri = new Uri(changelog.DownloadUri);
                    DeleteFileOnServer(downloadUri);
                    db.StoredChangelogs.DeleteObject(changelog);
                    db.SaveChanges();
                }
            }
        }

        public static bool DeleteFileOnServer(Uri serverUri)
        {
            if (serverUri.Scheme != Uri.UriSchemeFtp)            
                return false;

            string zipFilename = serverUri + ".zip";
           
            try
            {
                // Get the object used to communicate with the server.
                var request = (FtpWebRequest) WebRequest.Create(zipFilename);
                request.Method = WebRequestMethods.Ftp.DeleteFile;

                var response = (FtpWebResponse) request.GetResponse();
                Console.WriteLine("Delete status: {0}", response.StatusDescription);
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void CancelChangelog(int changelogid)
        {
            this.AcknowledgeChangelogDownloaded(changelogid);
        }

        
    }
}