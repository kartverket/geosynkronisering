using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Xml;
using ChangelogManager;
using Kartverket.GeosyncWCF;

namespace Kartverket.Geosynkronisering
{
    public class ChangelogManager
    {
        private StoredChangelogsEntities db;
        // private geosyncEntities db;

        public ChangelogManager(StoredChangelogsEntities _db) // public ChangelogManager(geosyncEntities _db)
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

                    case "queued":
                        return ChangelogStatusType.queued;
                    case "working":
                        return ChangelogStatusType.working;
                    case "cancelled":
                        return ChangelogStatusType.cancelled;
                    case "finished":
                        return ChangelogStatusType.finished;
                }
            }

            return ChangelogStatusType.cancelled;
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
                    if (changelog.DownloadUri == null)
                    {
                        db.DeleteObject(changelog);

                        //db.StoredChangelogs.DeleteObject(changelog);
                        //db.SaveChanges();
                        
                        throw new ArgumentNullException("DownloadURI is null.");
                    }
                    resp.id = new Kartverket.GeosyncWCF.ChangelogIdentificationType();
                    resp.id.changelogId = changelog.ChangelogId.ToString();
                    resp.downloadUri = changelog.DownloadUri;
                    resp.endIndex = changelog.EndIndex.Value.ToString();
                }
            }
            return resp;
         }

        public void AcknowledgeChangelogDownloaded( string changelogid)
        {                   
            DateTime deleteDate = DateTime.Now.AddDays(-10); // TODO: Add to dataset settings

            var obsoleteFiles = (from c in db.StoredChangelogs where (c.Status == "started" || (c.Status == "Finished" && c.Stored == false)) && c.DateCreated < deleteDate select c);

            foreach (var of in obsoleteFiles)
            {
                if (of.DownloadUri != null) //TODO: Quick fix of permanent?
                {
                    var downloadUri = new Uri(of.DownloadUri);
                    DeleteFileOnServer(downloadUri);
                    db.DeleteObject(of);
                    //db.StoredChangelogs.DeleteObject(of);
                }
            }

            //db.SaveChanges();


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
                    //db.StoredChangelogs.DeleteObject(changelog);
                    //db.SaveChanges();
                    db.DeleteObject(changelog);

                }
            }
        }

        public static bool DeleteFileOnServer(Uri serverUri)
        {
            if (serverUri.Scheme != Uri.UriSchemeHttps)            
                return false;

            int length = serverUri.AbsoluteUri.Split('/').Count();
            string zipFile = serverUri.AbsoluteUri.Split('/')[length - 1];
            string filLocation=AppDomain.CurrentDomain.BaseDirectory + "\\Changelogfiles\\" + zipFile;
           
            try
            {
                File.Delete(filLocation);
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
            var ldbo = new StoredChangelog();
            ldbo.Stored = false;            
            
            ldbo.Status = ((string)System.Enum.GetName(typeof(Kartverket.GeosyncWCF.ChangelogStatusType),Kartverket.GeosyncWCF.ChangelogStatusType.queued));
            ldbo.StartIndex = startIndex;
            ldbo.EndIndex = startIndex + count; //TODO fix

            ldbo.DatasetId = datasetId;
            ldbo.DateCreated = DateTime.Now;

            //TODO make filter 
            //TODO check if similar stored changelog is already done
            db.StoredChangelogs.Add(ldbo);
            //db.AddObject(ldbo);
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