using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.IO;
using NLog;
using Npgsql;
using NpgsqlTypes;
using System.Net;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Collections;
using System.Threading;
using Ionic.Zip;
using Kartverket.GeosyncWCF;

namespace Kartverket.Geosynkronisering.ChangelogProviders
{
    public class MockChangelog:IChangelogProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)
        private geosyncEntities p_db;   
        #region IChangelogSource Members

        //public MockChangelog(geosyncEntities _db)
        //{
        //    p_db = _db;
        //}

        public void Intitalize(int datasetId)
        {

        }

        public void SetDb(geosyncEntities db)
        {
            p_db = db;
        }

        public string GetLastIndex(int datasetid)
        {
            

            var dataset = from d in p_db.Datasets select d;
            
            string resp = "20";
                       
            return resp;
        }


        private OrderChangelog CurrentOrderChangeLog = null;
        private string BaseVirtualPath;
        public OrderChangelog CreateChangelog(int startIndex, int count, string todo_filter, int datasetId)
        {
            logger.Info("CreateChangelog START");
            BaseVirtualPath = Utils.BaseVirtualAppPath;
            ChangelogManager chlmng = new ChangelogManager(p_db);
            CurrentOrderChangeLog = chlmng.CreateChangeLog(startIndex, count, datasetId);
            chlmng.SetStatus(CurrentOrderChangeLog.changelogId, ChangelogStatusType.queued);
            return CurrentOrderChangeLog;
        }

        public OrderChangelog GenerateInitialChangelog(int datasetId)
        {
            return null;
        }
        public OrderChangelog OrderChangelog(int startIndex, int count, string todo_filter, int datasetId)
        {
            


            //TODO check if similar stored changelog is already done
            ChangelogManager chlmng = new ChangelogManager(p_db);


           

            //New thread and do the work....
            string sourceFileName = "Changelogfiles/changelog_flytebryggestart.xml";
            string destFileName = "Changelogfiles/" + CurrentOrderChangeLog.changelogId + "_changelog.xml";
            System.IO.File.Copy(BaseVirtualPath + sourceFileName, BaseVirtualPath + destFileName);

            chlmng.SetStatus(CurrentOrderChangeLog.changelogId, ChangelogStatusType.finished);
            chlmng.SetDownloadURI(CurrentOrderChangeLog.changelogId, destFileName);

            p_db.SaveChanges();

            return CurrentOrderChangeLog;

        }

        public OrderChangelog OrderChangelog2(int startIndex, int count, string todo_filter, int datasetId)
        {
            StoredChangelog ldbo = new StoredChangelog();
            ldbo.Stored = false;
            ldbo.Status = "queued";
            ldbo.StartIndex = startIndex;
            ldbo.EndIndex = startIndex + count; //TODO fix
            //TODO tester make filter
            //QueryType q = new QueryType();
            //q.srsName = "urn:ogc:def:crs:EPSG::32633";
            //q.typeNames = new StringCollection();
            //q.typeNames.Add("app:Kystkontur");

            //FilterType f = new FilterType();
            //BinarySpatialOpType op = new BinarySpatialOpType();
            //BBOXType b = new BBOXType();
            //op.Item = b;
            //op.ValueReference = "";
            //f.spatialOps = op;
            //q.AbstractSelectionClause = f;

            //TODO ?
            OrderChangelog r = new OrderChangelog();
           


            //TODO check if similar stored changelog is already done
            p_db.StoredChangelogs.AddObject(ldbo);
            p_db.SaveChanges();

           
            r.changelogId = ldbo.ChangelogId.ToString();

            //New thread and do the work....
            string sourceFileName = "Changelogfiles/changelog_flytebryggestart.xml";
                    string destFileName = "Changelogfiles/"+ ldbo.ChangelogId + "_changelog.xml";
                    System.IO.File.Copy(Utils.BaseVirtualAppPath + sourceFileName, Utils.BaseVirtualAppPath + destFileName);
                    
                    ldbo.DownloadUri = destFileName;
                    ldbo.Status = "finished";

                    p_db.SaveChanges();

            return r;
            
        }

        

        #endregion



        
    }
}