using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Globalization;
using System.Xml;
using Kartverket.GeosyncWCF;

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

        public XmlDocument DescribeFeatureType(int datasetId, DescribeFeatureTypeType describefeaturetype1)
        {
            var dataset = from d in db.Datasets where d.DatasetId == datasetId select d;

            string serviceUrl = dataset.First().TransformationConnection;
            if (serviceUrl.Contains("?"))
                serviceUrl = serviceUrl.Split('?')[0];

            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                {"service", describefeaturetype1.service},
                {"version", describefeaturetype1.version},
                {"request", "DescribeFeatureType"},
                //{"outputFormat",describefeaturetype1.outputFormat},
            };
            string typeNames = "";

            foreach (var VARIABLE in describefeaturetype1.TypeName)
                if (string.IsNullOrEmpty(VARIABLE.Namespace))
                    typeNames += VARIABLE.Name + ",";
                else
                    typeNames += VARIABLE.Namespace + ":" + VARIABLE.Name + ",";

            parameters["typeNames"] = typeNames.TrimEnd(',');

            string queryUrl = serviceUrl + "?" + DictToString(parameters, "=", "&");

            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(queryUrl);
            }
            catch
            {
                parameters.Remove("typeNames");
                queryUrl = serviceUrl + "?" + DictToString(parameters, "=", "&");
                xmlDoc.Load(queryUrl);
            }

            return xmlDoc;
        }

        private static string DictToString(Dictionary<string, string> source, string keyValueSeparator,
            string sequenceSeparator)
        {
            if (source == null)
                throw new ArgumentException("Parameter source can not be null.");
            var pairs = source.Select(x => x.Key + keyValueSeparator + x.Value).ToArray();
            return string.Join(sequenceSeparator, pairs);
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

        public Kartverket.GeosyncWCF.ChangelogStatusType GetChangelogStatus(string changelogid)
        {
            int nchangelogid = Int32.Parse(changelogid);

            var result = (from c in db.StoredChangelogs where c.ChangelogId == nchangelogid select c);
            
            Kartverket.GeosyncWCF.ChangelogStatusType resp = new GeosyncWCF.ChangelogStatusType();
            if (result.Count() > 0)
            {
                var changelog = result.First();
                switch (changelog.Status)
                {

                    case "queued": resp = Kartverket.GeosyncWCF.ChangelogStatusType.queued; break;
                    case "working": resp = Kartverket.GeosyncWCF.ChangelogStatusType.working; break;
                    case "cancelled": resp = Kartverket.GeosyncWCF.ChangelogStatusType.cancelled; break;
                    case "finished": resp = Kartverket.GeosyncWCF.ChangelogStatusType.finished; break;
                }
            }
            return resp;
        }

        public Kartverket.GeosyncWCF.ChangelogType GetChangelog(string changelogid)
        {
            int nchangelogid = Int32.Parse(changelogid);
            var result = (from c in db.StoredChangelogs where c.ChangelogId == nchangelogid select c);
            
            Kartverket.GeosyncWCF.ChangelogType resp = new Kartverket.GeosyncWCF.ChangelogType();
            if (result.Count() > 0)
            {
                var changelog = result.First();
                if (changelog != null)
                {
                    resp.id = new Kartverket.GeosyncWCF.ChangelogIdentificationType();
                    resp.id.changelogId = changelog.ChangelogId.ToString();
                    // resp.@return.downloadUri = Utils.BaseSiteUrl + changelog.DownloadUri;
                    resp.downloadUri = changelog.DownloadUri + ".zip"; // JJP - added .zip 20151215
                    resp.endIndex = changelog.EndIndex.Value.ToString();
                }
            }
            return resp;
         }

        public void AcknowledgeChangelogDownloaded( string changelogid)
        {                   
            DateTime deleteDate = DateTime.Now.AddDays(-10); // TODO: Add to dataset settings

            // Find and delete obsolete files from database and server
            var obsoleteFiles = (from c in db.StoredChangelogs where c.Status == "queued" && c.DateCreated < deleteDate select c);
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
            //var changelog = (from c in db.StoredChangelogs where c.ChangelogId == changelogid select c).First();
            int nchangelogid = Int32.Parse(changelogid);
            var result = (from c in db.StoredChangelogs where c.ChangelogId == nchangelogid select c);
            if (result != null && result.Count()>0)
            {
                var changelog = result.First();
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

        public void CancelChangelog(string changelogid)
        {
            this.AcknowledgeChangelogDownloaded(changelogid);
        }

        public OrderChangelog CreateChangeLog(int startIndex, int count, int datasetId)
        {            
            StoredChangelog ldbo = new StoredChangelog();
            ldbo.Stored = false;            
            
            ldbo.Status = ((string)System.Enum.GetName(typeof(Kartverket.GeosyncWCF.ChangelogStatusType),Kartverket.GeosyncWCF.ChangelogStatusType.queued));
            ldbo.StartIndex = startIndex;
            ldbo.EndIndex = startIndex + count; //TODO fix

            ldbo.DatasetId = datasetId;
            ldbo.DateCreated = DateTime.Now;

            //TODO make filter 
            //TODO check if similar stored changelog is already done
            db.StoredChangelogs.AddObject(ldbo);
            db.SaveChanges();

            OrderChangelog resp = new OrderChangelog();
            resp.changelogId = ldbo.ChangelogId.ToString();
            return resp;            
        }

        public StoredChangelog GetStoredChangelogRow(string changelogid)
        {
            int nchangelogid = Int32.Parse(changelogid);
            var changelog = (from c in db.StoredChangelogs where c.ChangelogId == nchangelogid select c).First();
            return changelog;
        }

        public bool SetStatus(string changelogid, Kartverket.GeosyncWCF.ChangelogStatusType status)
        {
            int nchangelogid = Int32.Parse(changelogid);
            var changelog = (from c in db.StoredChangelogs where c.ChangelogId == nchangelogid select c).First();

            changelog.Status = ((string)System.Enum.GetName(typeof(Kartverket.GeosyncWCF.ChangelogStatusType), status));
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

        public bool SetDownloadURI(string changelogid, string URI)
        {
            int nchangelogid = Int32.Parse(changelogid);
            var changelog = (from c in db.StoredChangelogs where c.ChangelogId == nchangelogid select c).First();

            changelog.DownloadUri = URI;
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

    }
}