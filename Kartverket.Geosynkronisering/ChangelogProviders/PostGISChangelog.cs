using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.IO;
using System.Xml.XPath;
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
    internal class OptimizedChangeLogElement
    {
        public string _gmlId;
        public string _transType;
        public long _changeId;
        public long _handle;
        public OptimizedChangeLogElement(string gmlId, string transType, long changeId)
        {
            _gmlId = gmlId;
            _transType = transType;
            _changeId = changeId;
        }
    }

    public class PostGISChangelog : IChangelogProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)
        //private geosyncEntities p_db;
        private string p_dbConnectInfo;
        private string p_nsApp;
        private string p_wfsURL;
        private string p_nsPrefixTargetNamespace;
        private string p_dbSchema;
        private string p_SchemaFileUri;

        #region IChangelogSource Members

        public void Intitalize( int datasetId)
        {
            p_dbConnectInfo = Database.DatasetsData.DatasetConnection(datasetId);
            p_wfsURL = Database.DatasetsData.TransformationConnection(datasetId);
            p_nsApp = Database.DatasetsData.TargetNamespace(datasetId);
            p_nsPrefixTargetNamespace = Database.DatasetsData.TargetNamespacePrefix(datasetId);
            p_dbSchema = Database.DatasetsData.DBSchema(datasetId);
            p_SchemaFileUri = Database.DatasetsData.SchemaFileUri(datasetId);
        }

        public string GetLastIndex(int datasetId)
        {
            Int64 endChangeId = 0;
            try
            {
                //Connect to postgres database
                Npgsql.NpgsqlConnection conn = null;
                //logger.Info("PostGISChangelog.GetLastIndex" + " NpgsqlConnection:{0}", p_db.Datasets.);
                conn = new NpgsqlConnection(p_dbConnectInfo);
                conn.Open();

                //Get max changelogid
                endChangeId = GetMaxChangeLogId(conn, datasetId);

                conn.Close();
                logger.Info("GetLastIndexResponse endChangeId :{0}{1}", "\t", endChangeId);
            }
            catch (System.Exception exp)
            {
                logger.ErrorException("GetLastIndex Exception:", exp);
                throw new System.Exception("GetLastIndex function failed", exp);
            }

            return endChangeId.ToString();
        }

        public OrderChangelog GenerateInitialChangelog(int datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                var initialChangelog = (from d in db.StoredChangelogs where d.DatasetId == datasetId && d.StartIndex == 1 && d.Stored == true && d.Status == "finished" orderby d.DateCreated descending select d).FirstOrDefault();
                if (initialChangelog != null)
                {
                    Uri uri = new Uri(initialChangelog.DownloadUri);
                    ChangelogManager.DeleteFileOnServer(uri);
                    db.StoredChangelogs.DeleteObject(initialChangelog);
                    db.SaveChanges();
                }
            }
            int startIndex = 1; // StartIndex always 1 on initial changelog
            int endIndex = Convert.ToInt32(GetLastIndex(datasetId));
            int count = 2000; // TODO: Get from dataset table
            logger.Info("GenerateInitialChangelog START");
            StoredChangelog ldbo = new StoredChangelog();
            ldbo.Stored = true;
            ldbo.Status = "queued";
            ldbo.StartIndex = startIndex;

            ldbo.DatasetId = datasetId;
            ldbo.DateCreated = DateTime.Now;

            //TODO make filter 
            //TODO check if similar stored changelog is already done
            // Generate unique filename
            string destFileName = Guid.NewGuid().ToString();

            //System.IO.File.Copy(Utils.BaseVirtualAppPath + sourceFileName, Utils.BaseVirtualAppPath + destFileName);

            string destPath = System.IO.Path.Combine(Utils.BaseVirtualAppPath, destFileName);
            string ftpURL = Database.ServerConfigData.FTPUrl();
            string ftpUser = Database.ServerConfigData.FTPUser();
            string ftpPwd = Database.ServerConfigData.FTPPwd();

            using (geosyncEntities db = new geosyncEntities())
            {
                // Store changelog info in database
                db.StoredChangelogs.AddObject(ldbo);
                db.SaveChanges();

                OrderChangelog resp = new OrderChangelog();
                resp.changelogId = ldbo.ChangelogId.ToString();

                //New thread and do the work....
                // We're coming back to the thread handling later...
                //string sourceFileName = "Changelogfiles/41_changelog.xml";


                System.IO.Directory.CreateDirectory(destPath);
                // Loop and create xml files
                int i = 1;
                int test = Convert.ToInt32(Math.Ceiling((double)endIndex / count));
                while (i++ <= Convert.ToInt32(Math.Ceiling((double)endIndex / count)))
                {
                    string partFileName = DateTime.Now.Ticks + ".xml";

                    string fullPathWithFile = System.IO.Path.Combine(destPath, partFileName);
                    MakeChangeLog(startIndex, count, p_dbConnectInfo, p_wfsURL, fullPathWithFile, datasetId);
                    startIndex += count;
                    endIndex = Convert.ToInt32(GetLastIndex(datasetId));
                }

                // Save endIndex to database
                ldbo.EndIndex = endIndex;
                db.SaveChanges();

                // New code to handle FTP download
                changeLogHandler chgLogHandler = new changeLogHandler(ldbo, logger);
                string inFile = "";
                string zipFile = "";
                try
                {
                    inFile = destPath;
                    zipFile = destFileName + ".zip";
                    chgLogHandler.createZipFileFromFolder(inFile, zipFile, destFileName);
                    Common.FileTransferHandler ftpTool = new Common.FileTransferHandler();
                    string tmpzipFile = Path.Combine(System.IO.Path.GetTempPath(), zipFile);
                    logger.Info(string.Format("Upload of file {0} started", tmpzipFile));
                    ldbo.Status = "queued";
                    db.SaveChanges();
                    // chgLogHandler.UploadFileToFtp(zipFile, Kartverket.Geosynkronisering.Properties.Settings.Default.ftpServer, Kartverket.Geosynkronisering.Properties.Settings.Default.ftpUser, Kartverket.Geosynkronisering.Properties.Settings.Default.ftpPassword);
                    if (!ftpTool.UploadFileToFtp(tmpzipFile, ftpURL, ftpUser, ftpPwd)) throw new Exception("Could not upload file to FTPServer!");
                    ldbo.Status = "finished";
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.ErrorException(string.Format("Failed to create or upload file {0}", zipFile), ex);
                    throw ex;
                }

                try
                {
                    string downLoadUri = string.Format(@"ftp://{0}:{1}@{2}/{3}", ftpUser, ftpPwd, ftpURL, destFileName);

                    ldbo.DownloadUri = downLoadUri;
                }
                catch (Exception ex)
                {
                    logger.ErrorException(string.Format("Failed to create or upload file {0}", zipFile), ex);
                    throw ex;
                }


                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.ErrorException(string.Format("Failed on SaveChanges, Kartverket.Geosynkronisering.ChangelogProviders.PostGISChangelog.OrderChangelog startIndex:{0} count:{1} changelogId:{2}", startIndex, count, ldbo.ChangelogId), ex);
                    throw ex;
                }
                logger.Info("Kartverket.Geosynkronisering.ChangelogProviders.PostGISChangelog.OrderChangelog" + " startIndex:{0}" + " count:{1}" + " changelogId:{2}", startIndex, count, ldbo.ChangelogId);

                logger.Info("GenerateInitialChangelog END");
                return resp;
            }
        }
        private OrderChangelog CurrentOrderChangeLog = null;
        private string BaseVirtualPath;
        public OrderChangelog CreateChangelog(int startIndex, int count, string todo_filter, int datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                if (startIndex == 1)
                {

                        var initialChangelog = (from d in db.StoredChangelogs where d.DatasetId == datasetId && d.StartIndex == 1 && d.Stored == true && d.Status == "finished" orderby d.DateCreated descending select d).FirstOrDefault();
                        if (initialChangelog != null)
                        {
                            OrderChangelog resp = new OrderChangelog();
                            resp.changelogId = initialChangelog.ChangelogId.ToString();
                            CurrentOrderChangeLog = resp;
                            return resp;
                        }

                }
                logger.Info("CreateChangelog START");
                BaseVirtualPath = Utils.BaseVirtualAppPath;
                ChangelogManager chlmng = new ChangelogManager(db);
                CurrentOrderChangeLog = chlmng.CreateChangeLog(startIndex, count, datasetId);
                chlmng.SetStatus(CurrentOrderChangeLog.changelogId, ChangelogStatusType.queued);
            }
            return CurrentOrderChangeLog;
        }

        public OrderChangelog _OrderChangelog(int startIndex, int count, string todo_filter, int datasetId)
        {
            string ftpURL = Database.ServerConfigData.FTPUrl();
            string ftpUser = Database.ServerConfigData.FTPUser();
            string ftpPwd = Database.ServerConfigData.FTPPwd();
            using (geosyncEntities db = new geosyncEntities())
            {
                ChangelogManager chlmng = new ChangelogManager(db);
                string destFileName = Guid.NewGuid().ToString();

                chlmng.SetStatus(CurrentOrderChangeLog.changelogId, ChangelogStatusType.working);
                //System.IO.File.Copy(Utils.BaseVirtualAppPath + sourceFileName, Utils.BaseVirtualAppPath + destFileName);
                try
                {
                    MakeChangeLog(startIndex, count, p_dbConnectInfo, p_wfsURL, BaseVirtualPath + destFileName + ".xml", datasetId);
                }
                catch (Exception ex)
                {
                    chlmng.SetStatus(CurrentOrderChangeLog.changelogId, ChangelogStatusType.cancelled);
                    logger.ErrorException(string.Format("Failed to make Change Log {0}", BaseVirtualPath + destFileName + ".xml"), ex);
                    throw ex;
                }

                // New code to handle FTP download
                changeLogHandler chgLogHandler = new changeLogHandler(logger);
                string inFile = "";
                string zipFile = "";
                try
                {
                    inFile = BaseVirtualPath + destFileName + ".xml";
                    zipFile = destFileName + ".zip";
                    chgLogHandler.createZipFile(inFile, zipFile);
                    Common.FileTransferHandler ftpTool = new Common.FileTransferHandler();
                    string tmpzipFile = Path.Combine(System.IO.Path.GetTempPath(), zipFile);
                    logger.Info(string.Format("Upload of file {0} started", tmpzipFile));

                    // p_db.SaveChanges();
                    // chgLogHandler.UploadFileToFtp(zipFile, Kartverket.Geosynkronisering.Properties.Settings.Default.ftpServer, Kartverket.Geosynkronisering.Properties.Settings.Default.ftpUser, Kartverket.Geosynkronisering.Properties.Settings.Default.ftpPassword);
                    if (!ftpTool.UploadFileToFtp(tmpzipFile, ftpURL, ftpUser, ftpPwd)) throw new Exception("Could not upload file to FTPServer!");

                    //p_db.SaveChanges();
                }
                catch (Exception ex)
                {
                    chlmng.SetStatus(CurrentOrderChangeLog.changelogId, ChangelogStatusType.cancelled);
                    logger.ErrorException(string.Format("Failed to create or upload file {0}", zipFile), ex);
                    throw ex;
                }

                try
                {
                    string downLoadUri = string.Format(@"ftp://{0}:{1}@{2}/{3}", Database.ServerConfigData.FTPUser(), Database.ServerConfigData.FTPPwd(), Database.ServerConfigData.FTPUrl(), destFileName);
                    chlmng.SetStatus(CurrentOrderChangeLog.changelogId, ChangelogStatusType.finished);
                    chlmng.SetDownloadURI(CurrentOrderChangeLog.changelogId, downLoadUri);
                }
                catch (Exception ex)
                {
                    logger.ErrorException(string.Format("Failed to create or upload file {0}", zipFile), ex);
                    throw ex;
                }


                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.ErrorException(string.Format("Failed on SaveChanges, Kartverket.Geosynkronisering.ChangelogProviders.PostGISChangelog.OrderChangelog startIndex:{0} count:{1} changelogId:{2}", startIndex, count, CurrentOrderChangeLog.changelogId), ex);
                    throw ex;
                }
                logger.Info("Kartverket.Geosynkronisering.ChangelogProviders.PostGISChangelog.OrderChangelog" + " startIndex:{0}" + " count:{1}" + " changelogId:{2}", startIndex, count, CurrentOrderChangeLog.changelogId);

                logger.Info("OrderChangelog END");
                return CurrentOrderChangeLog;
            }
        }

        public bool CreateDataExtraction(int startIndex, int count, string todo_filter, int datasetId, string ChangelogID)
        {
            // Generate unique filename
            string destFileName = Guid.NewGuid().ToString();

            //System.IO.File.Copy(Utils.BaseVirtualAppPath + sourceFileName, Utils.BaseVirtualAppPath + destFileName);

            MakeChangeLog(startIndex, count, p_dbConnectInfo, p_wfsURL, Utils.BaseVirtualAppPath + destFileName + ".xml", datasetId);

            // New code to handle FTP download
            string ftpURL = Database.ServerConfigData.FTPUrl();
            string ftpUser = Database.ServerConfigData.FTPUser();
            string ftpPwd= Database.ServerConfigData.FTPPwd();

            using (geosyncEntities db = new geosyncEntities())
            {
                ChangelogManager chlmng = new ChangelogManager(db);

                //TODO:HLU ChangelogHandler IS FTP- DO not need LDBO. Could be handled here.
                StoredChangelog ldbo = chlmng.GetStoredChangelogRow(ChangelogID);
                changeLogHandler chgLogHandler = new changeLogHandler(ldbo, logger);
                string inFile = "";
                string zipFile = "";
                try
                {
                    inFile = Utils.BaseVirtualAppPath + destFileName + ".xml";
                    zipFile = destFileName + ".zip";
                    chgLogHandler.createZipFile(inFile, zipFile);
                    Common.FileTransferHandler ftpTool = new Common.FileTransferHandler();
                    string tmpzipFile = Path.Combine(System.IO.Path.GetTempPath(), zipFile);
                    logger.Info(string.Format("Upload of file {0} started", tmpzipFile));
                    chlmng.SetStatus(ChangelogID, ChangelogStatusType.working);
                    // chgLogHandler.UploadFileToFtp(zipFile, Kartverket.Geosynkronisering.Properties.Settings.Default.ftpServer, Kartverket.Geosynkronisering.Properties.Settings.Default.ftpUser, Kartverket.Geosynkronisering.Properties.Settings.Default.ftpPassword);
                    if (!ftpTool.UploadFileToFtp(tmpzipFile,ftpURL, ftpUser,ftpPwd )) throw new Exception("Could not upload file to FTPServer!");
                }
                catch (Exception ex)
                {
                    logger.ErrorException(string.Format("Failed to create or upload file {0}", zipFile), ex);
                    throw ex;
                }

                try
                {
                    string downLoadUri = string.Format(@"ftp://{0}:{1}@{2}/{3}", Database.ServerConfigData.FTPUser(), Database.ServerConfigData.FTPPwd(), Database.ServerConfigData.FTPUrl(), destFileName);

                    ldbo.DownloadUri = downLoadUri;
                }
                catch (Exception ex)
                {
                    logger.ErrorException(string.Format("Failed to create or upload file {0}", zipFile), ex);
                    throw ex;
                }


                try
                {
                    chlmng.SetStatus(ChangelogID, ChangelogStatusType.finished);
                }
                catch (Exception ex)
                {
                    logger.ErrorException(string.Format("Failed on SaveChanges, Kartverket.Geosynkronisering.ChangelogProviders.PostGISChangelog.OrderChangelog startIndex:{0} count:{1} changelogId:{2}", startIndex, count, ldbo.ChangelogId), ex);
                    throw ex;
                }
                logger.Info("Kartverket.Geosynkronisering.ChangelogProviders.PostGISChangelog.OrderChangelog" + " startIndex:{0}" + " count:{1}" + " changelogId:{2}", startIndex, count, ldbo.ChangelogId);

                logger.Info("OrderChangelog END");
                return true;
            }
        }

        public OrderChangelog OrderChangelog(int startIndex, int count, string todo_filter, int datasetId)
        {
            // If startIndex == 1: Check if initital changelog exists
            if (startIndex == 1)
            {
                using (geosyncEntities db = new geosyncEntities())
                {
                    var initialChangelog = (from d in db.StoredChangelogs where d.DatasetId == datasetId && d.StartIndex == 1 && d.Stored == true && d.Status == "finished" orderby d.DateCreated descending select d).FirstOrDefault();

                    if (initialChangelog != null)
                    {
                        OrderChangelog resp = new OrderChangelog();
                        resp.changelogId = initialChangelog.ChangelogId.ToString();
                        return resp;
                    }
                }
            }

            // If initial changelog don't exists or startIndex != 1
            return _OrderChangelog(startIndex, count, todo_filter, datasetId);
        }


        //Build changelog responsefile
        private void MakeChangeLog(int startChangeId, int count, string dbConnectInfo, string wfsUrl, string changeLogFileName, int datasetId)
        {
            logger.Info("MakeChangeLog START");
            try
            {
                //Connect to postgres database
                Npgsql.NpgsqlConnection conn = null;
                conn = new NpgsqlConnection(dbConnectInfo);
                conn.Open();

                //Get max changelogid
                Int64 endChangeId = GetMaxChangeLogId(conn, datasetId);

                //Prepare query against the changelog table in postgres
                Npgsql.NpgsqlCommand command = null;
                PrepareChangeLogQuery(conn, ref command, startChangeId, endChangeId, datasetId);

                List<OptimizedChangeLogElement> optimizedChangeLog = new List<OptimizedChangeLogElement>();
                //Execute query against the changelog table and remove unnecessary transactions.
                FillOptimizedChangeLog(ref command, ref optimizedChangeLog, startChangeId);

                //Get features from WFS and add transactions to changelogfile
                BuildChangeLogFile(count, optimizedChangeLog, wfsUrl, startChangeId, endChangeId, changeLogFileName, datasetId);

                conn.Close();
            }
            catch (System.Exception exp)
            {
                logger.ErrorException("MakeChangeLog function failed:", exp);
                throw new System.Exception("MakeChangeLog function failed", exp);
            }
            logger.Info("MakeChangeLog END");
        }

        private void FillOptimizedChangeLog(ref Npgsql.NpgsqlCommand command, ref List<OptimizedChangeLogElement> optimizedChangeLog, int startChangeId)
        {
            logger.Info("FillOptimizedChangeLog START");
            try
            {
                OrderedDictionary tempOptimizedChangeLog = new OrderedDictionary();
                //Fill optimizedChangeLog
                using (NpgsqlDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string gmlId = dr.GetString(0);

                        // TODO: Fix dirty implementation later - 20121006-Leg: Uppercase First Letter
                        //gmlId = char.ToUpper(gmlId[0]) + gmlId.Substring(1);

                        string transType = dr.GetString(1);
                        long changelogId = dr.GetInt64(2);


                        OptimizedChangeLogElement optimizedChangeLogElement;
                        if (transType.Equals("D"))
                        {
                            //Remove if inserted or updated earlier in this sequence of transactions
                            if (tempOptimizedChangeLog.Contains(gmlId))
                            {
                                optimizedChangeLogElement = (OptimizedChangeLogElement)tempOptimizedChangeLog[gmlId];
                                string tempTransType = optimizedChangeLogElement._transType;
                                tempOptimizedChangeLog.Remove(gmlId);
                                if (tempTransType.Equals("U"))
                                {
                                    //Add delete if last operation was update. 
                                    tempOptimizedChangeLog.Add(gmlId, new OptimizedChangeLogElement(gmlId, transType, changelogId));
                                }
                            }
                            else
                            {
                                tempOptimizedChangeLog.Add(gmlId, new OptimizedChangeLogElement(gmlId, transType, changelogId));
                            }
                        }
                        else
                        {
                            if (!tempOptimizedChangeLog.Contains(gmlId))
                            {
                                tempOptimizedChangeLog.Add(gmlId, new OptimizedChangeLogElement(gmlId, transType, changelogId));
                            }
                        }
                    }
                }

                //Fill optimizedChangeLog
                foreach (var item in tempOptimizedChangeLog.Values)
                {
                    optimizedChangeLog.Add((OptimizedChangeLogElement)item);
                }
            }
            catch (System.Exception exp)
            {
                logger.ErrorException("FillOptimizedChangeLog function failed:", exp);
                throw new System.Exception("FillOptimizedChangeLog function failed", exp);
            }
            logger.Info("FillOptimizedChangeLog END");
        }

        private void BuildChangeLogFile(int count, List<OptimizedChangeLogElement> optimizedChangeLog, string wfsUrl, int startChangeId, Int64 endChangeId,
            string changeLogFileName, int datasetId)
        {
            logger.Info("BuildChangeLogFile START");
            try
            {
                //Use changelog_empty.xml as basis for responsefile
                //XElement changeLog = XElement.Load(Utils.BaseVirtualAppPath + @"Changelogfiles\changelog_empty.xml");

                XElement changeLog = BuildChangelogRoot(datasetId);

                int counter = 0;

                List<OptimizedChangeLogElement> inserts = new List<OptimizedChangeLogElement>();
                long portionEndIndex = 0;
                for (int i = 0; i < optimizedChangeLog.Count; i++)
                {
                    OptimizedChangeLogElement current = optimizedChangeLog.ElementAt(i);
                    string gmlId = current._gmlId;
                    string transType = current._transType;
                    long changeId = current._changeId;
                    long handle = 0;

                    //If next element == lastelement
                    if ((i + 1) == optimizedChangeLog.Count)
                    {
                        handle = endChangeId;
                    }
                    else
                    {
                        OptimizedChangeLogElement next = optimizedChangeLog.ElementAt(i + 1);
                        handle = next._changeId - 1;
                    }
                    portionEndIndex = handle;

                    counter++;

                    if (transType == "D")
                    {
                        AddDeleteToChangeLog(gmlId, handle, changeLog, datasetId);
                    }
                    else if (transType == "U")
                    {
                        AddReplaceToChangeLog(gmlId, handle, wfsUrl, changeLog, datasetId);
                    }
                    else if (transType == "I")
                    {
                        current._handle = handle;
                        inserts.Add(current);
                        bool lastElement = false;
                        if ((i + 1 == optimizedChangeLog.Count) || (counter == count))//If last element or portion is finished
                            lastElement = true;
                        if (!lastElement)
                        {
                            OptimizedChangeLogElement next = optimizedChangeLog.ElementAt(i + 1);
                            string nextChange = next._transType;
                            if (nextChange != "I")//if next is not insert
                            {
                                //Add collected inserts to changelog
                                AddInsertPortionsToChangeLog(inserts, wfsUrl, changeLog, datasetId);
                                inserts.Clear();
                            }
                        }
                        else
                        {
                            AddInsertPortionsToChangeLog(inserts, wfsUrl, changeLog, datasetId);
                            inserts.Clear();
                        }
                    }
                    if (counter == count)
                    {
                        break;
                    }
                }

                //Update attributes in chlogf:TransactionCollection
                UpdateRootAttributes(changeLog, counter, startChangeId, portionEndIndex);

                if (!CheckChangelogHasFeatures(changeLog))
                {
                    System.Exception exp = new System.Exception("CheckChangelogHasFeatures found 0 features");
                    logger.ErrorException("CheckChangelogHasFeatures found 0 features", exp);
                    throw exp;
                }

                //store changelog to file
                changeLog.Save(changeLogFileName);
            }
            catch (System.Exception exp)
            {
                logger.ErrorException("BuildChangeLogFile function failed:", exp);
                throw new System.Exception("BuildChangeLogFile function failed", exp);
            }
            logger.Info("BuildChangeLogFile END");
        }

        private void AddInsertPortionsToChangeLog(List<OptimizedChangeLogElement> insertList, string wfsUrl, XElement changeLog, int datasetId)
        {
            int portionSize = 100;

            List<OptimizedChangeLogElement> insertsListPortion = new List<OptimizedChangeLogElement>();
            int portionCounter = 0;
            foreach (OptimizedChangeLogElement insert in insertList)
            {
                portionCounter++;
                insertsListPortion.Add(insert);
                if (portionCounter % portionSize == 0)
                {
                    AddInsertsToChangeLog(insertsListPortion, wfsUrl, changeLog, datasetId);
                    insertsListPortion.Clear();
                }
            }
            if (insertsListPortion.Count() > 0)
            {
                AddInsertsToChangeLog(insertsListPortion, wfsUrl, changeLog, datasetId);
            }

        }

        private void AddInsertsToChangeLog(List<OptimizedChangeLogElement> insertsGmlIds, string wfsUrl, XElement changeLog, int datasetId)
        {
            List<string> typeNames = new List<string>();

            ChangelogWFS wfs = new ChangelogWFS();

            Dictionary<string, string> typeIdDict = new Dictionary<string, string>();

            List<string> GmlIds = new List<string>();

            long handle = 0;
            foreach (OptimizedChangeLogElement insert in insertsGmlIds)
            {
                GmlIds.Add(insert._gmlId);

                if (insert._handle > handle)
                    handle = insert._handle;
            }

            XElement getFeatureResponse = wfs.GetFeatureCollectionFromWFS(p_nsPrefixTargetNamespace, p_nsApp, wfsUrl, ref typeIdDict, GmlIds, datasetId);
           
            //Build inserts for each typename
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";
            XNamespace nsChlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.0/endringslogg";

            XNamespace nsApp = p_nsApp;
            // 20130917-Leg: Fix
            string nsPrefixApp = changeLog.GetPrefixOfNamespace(nsApp);
            XmlNamespaceManager mgr = new XmlNamespaceManager(new NameTable());
            mgr.AddNamespace(nsPrefixApp, nsApp.NamespaceName);
            string nsPrefixAppComplete = nsPrefixApp + ":";

            XElement insertElement = new XElement(nsWfs + "Insert", new XAttribute("handle", handle)); //new XAttribute("inputFormat", "application/gml+xml; version=3.2") TKN:funker ikke på deegree

            foreach (KeyValuePair<string, string> dictElement in typeIdDict)
            {
                //string xpathExpressionLokalid = "//" + nsPrefixAppComplete + dictElement.Value + "[" + nsPrefixAppComplete + "identifikasjon/" + nsPrefixAppComplete + "Identifikasjon/" + nsPrefixAppComplete + "lokalId='"+dictElement.Key+"']";

                // //kyst:identifikasjon/kyst:Identifikasjon[kyst:lokalId='0a11f53a-8a37-5977-b9a2-e65a17d42e05']/../..
                string xpathExpressionLokalid = "//" + nsPrefixAppComplete + "identifikasjon/" + nsPrefixAppComplete + "Identifikasjon[" + nsPrefixAppComplete + "lokalId='" + dictElement.Key + "']/../..";

                XElement feature = getFeatureResponse.XPathSelectElement(xpathExpressionLokalid, mgr);

                insertElement.Add(feature);

            }
            changeLog.Element(nsChlogf + "transactions").Add(insertElement);
        }

        private void AddDeleteToChangeLog(string gmlId, long handle, XElement changeLog, int datasetId)
        {
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";
            XNamespace nsChlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.0/endringslogg";
            XNamespace nsFes = "http://www.opengis.net/fes/2.0";
            XNamespace nsApp = p_nsApp;
            string nsPrefixApp = changeLog.GetPrefixOfNamespace(nsApp);
            string nsPrefixAppComplete = nsPrefixApp + ":";
            string xpathExpressionLokalid = nsPrefixAppComplete + "identifikasjon/" + nsPrefixAppComplete +
                                          "Identifikasjon/" + nsPrefixAppComplete + "lokalId";

            int pos = gmlId.IndexOf(".");
            string typename = gmlId.Substring(0, pos);
            string lokalId = gmlId.Substring(pos + 1);
            //new XAttribute("inputFormat", "application/gml+xml; version=3.2"), 
            XElement deleteElement = new XElement(nsWfs + "Delete", new XAttribute("handle", handle), new XAttribute("typeName", nsPrefixApp + ":" + typename), //new XAttribute("typeName", "app:" + typename),
                 new XAttribute(XNamespace.Xmlns + nsPrefixApp, nsApp));
            //XElement deleteElement = new XElement(nsWfs + "Delete", new XAttribute("handle", transCounter), new XAttribute("typeName", typename),
            //    new XAttribute("inputFormat", "application/gml+xml; version=3.2"));

            //deleteElement.Add(getFeatureResponse.Element(nsWfs + "member").Nodes());
            //Add filter
            // 20121031-Leg: "lokal_id" replaced by "lokalId"
            // 20131015-Leg: Filter ValueReference content with namespace prefix
            deleteElement.Add(new XElement(nsFes + "Filter",
                                    new XElement(nsFes + "PropertyIsEqualTo",
                                        new XElement(nsFes + "ValueReference", xpathExpressionLokalid), //new XElement(nsFes + "ValueReference", "identifikasjon/Identifikasjon/lokalId"),
                                        new XElement(nsFes + "Literal", lokalId)
                                    )
                                ));

            changeLog.Element(nsChlogf + "transactions").Add(deleteElement);
        }

        private void AddReplaceToChangeLog(string gmlId, long handle, string wfsUrl, XElement changeLog, int datasetId)
        {
            Dictionary<string, string> typeIdDict = new Dictionary<string, string>();
            ChangelogWFS wfs = new ChangelogWFS();
            XElement getFeatureResponse = wfs.GetFeatureCollectionFromWFS(p_nsPrefixTargetNamespace, p_nsApp, wfsUrl, ref typeIdDict, new List<string>() { gmlId }, datasetId);
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";
            XNamespace nsChlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.0/endringslogg";
            XNamespace nsFes = "http://www.opengis.net/fes/2.0";

            XNamespace nsApp = p_nsApp;
            // 20130917-Leg: Fix
            string nsPrefixApp = changeLog.GetPrefixOfNamespace(nsApp);
            XmlNamespaceManager mgr = new XmlNamespaceManager(new NameTable());
            mgr.AddNamespace(nsPrefixApp, nsApp.NamespaceName);
            string nsPrefixAppComplete = nsPrefixApp + ":";
            string xpathExpressionLokalidFilter = nsPrefixAppComplete + "identifikasjon/" + nsPrefixAppComplete +
                                "Identifikasjon/" + nsPrefixAppComplete + "lokalId";

            foreach (KeyValuePair<string, string> dictElement in typeIdDict)
            {
                string xpathExpressionLokalid = "//" + nsPrefixAppComplete + "identifikasjon/" + nsPrefixAppComplete + "Identifikasjon[" + nsPrefixAppComplete + "lokalId='" + dictElement.Key + "']/../..";
                XElement feature = getFeatureResponse.XPathSelectElement(xpathExpressionLokalid, mgr);
                XElement replaceElement = new XElement(nsWfs + "Replace", new XAttribute("handle", handle));
                XElement lokalidElement = feature.XPathSelectElement(xpathExpressionLokalidFilter, mgr);
                string lokalId = lokalidElement.Value;

                replaceElement.Add(feature);

                replaceElement.Add(new XElement(nsFes + "Filter",
                                        new XElement(nsFes + "PropertyIsEqualTo",
                                            new XElement(nsFes + "ValueReference", xpathExpressionLokalidFilter), //new XElement(nsFes + "ValueReference", "identifikasjon/Identifikasjon/lokalId"),
                                            new XElement(nsFes + "Literal", lokalId)
                                        )
                                  ));
                changeLog.Element(nsChlogf + "transactions").Add(replaceElement);
            }
        }

        private void AddUpdateToChangeLog(string gmlId, long handle, string wfsUrl, XElement changeLog, int datasetId)
        {
            Dictionary<string, string> typeIdDict = new Dictionary<string, string>();
            //int pos = gmlId.IndexOf(".");
            //string typename = gmlId.Substring(0, pos);
            //string lokalId = gmlId.Substring(pos + 1);
            ChangelogWFS wfs = new ChangelogWFS();
            XElement getFeatureResponse = wfs.GetFeatureCollectionFromWFS(p_nsPrefixTargetNamespace, p_nsApp, wfsUrl, ref typeIdDict, new List<string>() { gmlId }, datasetId);

            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";
            XNamespace nsChlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.0/endringslogg";
            XNamespace nsFes = "http://www.opengis.net/fes/2.0";
            XNamespace nsGml = "http://www.opengis.net/gml/3.2";
            XNamespace nsApp = p_nsApp;

            // 20130917-Leg: Fix
            string nsPrefixApp = changeLog.GetPrefixOfNamespace(nsApp);
            XmlNamespaceManager mgr = new XmlNamespaceManager(new NameTable());
            mgr.AddNamespace(nsPrefixApp, nsApp.NamespaceName);
            string nsPrefixAppComplete = nsPrefixApp + ":";
            // LokalId is of form: "app:identifikasjon/app:Identifikasjon/app:lokalId"
            string xpathExpressionLokalidFilter = nsPrefixAppComplete + "identifikasjon/" + nsPrefixAppComplete +
                                            "Identifikasjon/" + nsPrefixAppComplete + "lokalId";

            //int count = 0;
            foreach (KeyValuePair<string, string> dictElement in typeIdDict) //foreach (string gmlId in updatesGmlIds) //foreach (string typename in typeNames)
            {
                string xpathExpressionLokalid = "//" + nsPrefixAppComplete + "identifikasjon/" + nsPrefixAppComplete + "Identifikasjon[" + nsPrefixAppComplete + "lokalId='" + dictElement.Key + "']/../..";

                XElement feature = getFeatureResponse.XPathSelectElement(xpathExpressionLokalid, mgr);
                //var featuresOfType = getFeatureResponse.Descendants(nsApp + typename);
                //foreach (XElement feature in featuresOfType)
                {
                    //new XAttribute("inputFormat", "application/gml+xml; version=3.2"),
                    XElement updateElement = new XElement(nsWfs + "Update", new XAttribute("typeName", nsPrefixAppComplete + dictElement.Value), //new XAttribute("typeName", "app:" + typename),
                                    new XAttribute("handle", handle),
                                     new XAttribute(XNamespace.Xmlns + nsPrefixApp, nsApp));
                    //XElement updateElement = new XElement(nsWfs + "Update", new XAttribute("typeName", typename), new XAttribute("handle", transCounter),
                    //                                    new XAttribute("inputFormat", "application/gml+xml; version=3.2"), new XAttribute(XNamespace.Xmlns + "App", nsApp));
                    //string lokalId = feature.Element(nsApp + "lokalId").Value;

                    // Get the lokalId with XPath
                    XElement lokalidElement = feature.XPathSelectElement(xpathExpressionLokalidFilter, mgr);
                    string lokalId = lokalidElement.Value;

                    foreach (XElement e in feature.Elements())
                    {
                        if (e.Name.Equals(nsGml + "boundedBy"))
                        {
                            continue;
                        }
                        
                     

                        //
                        // 20140318-Leg: Value without namespace prefix to fix deegree update problem:
                        // We must skip the first node sibling after <wfs:Value>.
                        // Correct:
                        //  <wfs:Property>
                        //     <wfs:ValueReference>app:arealtype</wfs:ValueReference>
                        //     <wfs:Value>50</wfs:Value>
                        //  </wfs:Property>
                        // Incorrect:
                        //  <wfs:Property>
                        //     <wfs:ValueReference>app:arealtype</wfs:ValueReference>
                        //     <wfs:Value>
                        //       <app:arealtype>50</app:arealtype>
                        //     </wfs:Value>
                        //   </wfs:Property
                        //
                        if (e.Descendants().Any()) //Complex attributes or geometry
                        {
                            var localName = e.Name.LocalName;

                            //string nsPrefixTargetNamespace = Database.DatasetsData.TargetNamespacePrefix(datasetId);
                            //XNamespace nsOriginalNamespace = e.GetNamespaceOfPrefix(nsPrefixTargetNamespace);
                            //Console.WriteLine("Namespace: {0}", nsOriginalNamespace);


                            //IEnumerable<XElement> xElems = from item in (e.Descendants())
                            //                               //where item.Name == nsOriginalNamespace + localName
                            //                               select item;
                            //foreach (var el in xElems)
                            //{
                            //    Debug.WriteLine(el.Name);
                            //    Debug.Flush();
                            //    //xRootElement.Add(tran);
                            //}
                            //Debug.WriteLine(e.Descendants().Count());
                            //Debug.Flush();

                            //XElement childElement = new XElement(nsWfs + "Value",
                            //    from item in e.Descendants()
                            //    where item.HasElements
                            //                         select item);

                            XElement childElement = new XElement(nsWfs + "Value", e.Descendants().ElementAt(0));
                            updateElement.Add(new XElement(nsWfs + "Property", new XElement(nsWfs + "ValueReference", nsPrefixAppComplete + localName), childElement));

                            //e.Element(nsOriginalNamespace + localName).SetElementValue(null);
                            //e.DescendantsAndSelf().ToList().ElementAt(0).Remove();


                            //e.Elements().ToList().ElementAt(0).Remove();
                            //e.Elements(e.Name.Namespace + name).ToList().ElementAt(0).Remove();
                            //updateElement.Add(new XElement(nsWfs + "Property", new XElement(nsWfs + "ValueReference", nsPrefixAppComplete + e.Name.LocalName), new XElement(nsWfs + "Value", e.Descendants())));

                            //updateElement.Add(new XElement(nsWfs + "Property", new XElement(nsWfs + "ValueReference", nsPrefixAppComplete + localName), new XElement(nsWfs + "Value", e)));

                            //updateElement.Add(new XElement(nsWfs + "Property", new XElement(nsWfs + "ValueReference", nsPrefixAppComplete + localName), new XElement(nsWfs + "Value", e.Descendants(nsOriginalNamespace + localName))));

                        }
                        else
                        {
                            updateElement.Add(new XElement(nsWfs + "Property", new XElement(nsWfs + "ValueReference", nsPrefixAppComplete + e.Name.LocalName), new XElement(nsWfs + "Value", e.Value)));
                        }
                        // 20131015-Leg: ValueReference content with namespace prefix:
                        //updateElement.Add(new XElement(nsWfs + "Property", new XElement(nsWfs + "ValueReference", nsPrefixAppComplete + e.Name.LocalName), new XElement(nsWfs + "Value", e)));
                        //updateElement.Add(new XElement(nsWfs + "Property", new XElement(nsWfs + "ValueReference", e.Name.LocalName), new XElement(nsWfs + "Value", e)));
                    }

                    //string gmlId = updatesGmlIds[count];
                    //int pos = gmlId.IndexOf(".");
                    ////string typename = gmlId.Substring(0, pos);
                    //string lokalId = gmlId.Substring(pos + 1);

                    // 20131015-Leg: Filter ValueReference content with namespace prefix
                    updateElement.Add(new XElement(nsFes + "Filter",
                                            new XElement(nsFes + "PropertyIsEqualTo",
                                                new XElement(nsFes + "ValueReference", xpathExpressionLokalidFilter), //new XElement(nsFes + "ValueReference", "identifikasjon/Identifikasjon/lokalId"),
                                                new XElement(nsFes + "Literal", lokalId)
                                            )
                                      ));
                    changeLog.Element(nsChlogf + "transactions").Add(updateElement);
                    //transCounter++;
                    //count++;
                }
            }
        }

        private void UpdateRootAttributes(XElement changeLog, int counter, int startChangeId, Int64 endChangeId)
        {
            changeLog.SetAttributeValue("numberMatched", counter);
            changeLog.SetAttributeValue("numberReturned", counter);
            changeLog.SetAttributeValue("startIndex", startChangeId);
            changeLog.SetAttributeValue("endIndex", endChangeId);
        }

        private XElement BuildChangelogRoot(int datasetId)
        {
            XNamespace nsChlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.0/endringslogg";
            XNamespace nsApp = p_nsApp;
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";
            XNamespace nsXsi = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace nsGml = "http://www.opengis.net/gml/3.2";

            string schemaLocation = "http://skjema.geonorge.no/standard/geosynkronisering/1.0/endringslogg http://geosynkronisering.no/files/skjema/1.0/changelogfile.xsd ";
            schemaLocation += p_nsApp + " " + p_SchemaFileUri;

            XElement changelogRoot =
                new XElement(nsChlogf + "TransactionCollection", new XAttribute("timeStamp", "2001-12-17T09:30:47Z"), new XAttribute("numberMatched", ""), new XAttribute("numberReturned", ""), new XAttribute("startIndex", ""), new XAttribute("endIndex", ""),
                    new XAttribute(XNamespace.Xmlns + "xsi", nsXsi),
                    new XAttribute(nsXsi + "schemaLocation", schemaLocation),
                    new XAttribute(XNamespace.Xmlns + "chlogf", nsChlogf),
                    new XAttribute(XNamespace.Xmlns + "app", nsApp),
                    new XAttribute(XNamespace.Xmlns + "wfs", nsWfs),
                    new XAttribute(XNamespace.Xmlns + "gml", nsGml)
                    );
            changelogRoot.Add(new XElement(nsChlogf + "transactions", new XAttribute("service", "WFS"), new XAttribute("version", "2.0.0")));
            return changelogRoot;
        }

        private void PrepareChangeLogQuery(Npgsql.NpgsqlConnection conn, ref Npgsql.NpgsqlCommand command, int startChangeId, Int64 endChangeId, int datasetId)
        {
            logger.Info("PrepareChangeLogQuery START");
            try
            {
                //20121021-Leg: Correction "endringsid >= :startChangeId"
                //20121031-Leg: rad is now lokalId

                string sqlSelectGmlIds = "SELECT tabell || '.' || lokalid, type, endringsid FROM " + p_dbSchema + ".endringslogg WHERE endringsid >= :startChangeId AND endringsid <= :endChangeId ORDER BY endringsid";
                //string sqlSelectGmlIds = "SELECT tabell || '.' || rad, type FROM tilbyder.endringslogg WHERE endringsid >= :startChangeId AND endringsid <= :endChangeId ORDER BY endringsid";
                // string sqlSelectGmlIds = "SELECT tabell || '.' || rad, type FROM tilbyder.endringslogg WHERE endringsid > :startChangeId AND endringsid <= :endChangeId ";
                command = new NpgsqlCommand(sqlSelectGmlIds, conn);

                command.Parameters.Add(new NpgsqlParameter("startChangeId", NpgsqlDbType.Integer));
                command.Parameters.Add(new NpgsqlParameter("endChangeId", NpgsqlDbType.Integer));

                command.Prepare();

                command.Parameters[0].Value = startChangeId;
                command.Parameters[1].Value = endChangeId;
            }
            catch (System.Exception exp)
            {
                logger.ErrorException("PrepareChangeLogQuery function failed:", exp);
                throw new System.Exception("PrepareChangeLogQuery function failed", exp);
            }
            logger.Info("PrepareChangeLogQuery END");
        }

        private Int64 GetMaxChangeLogId(Npgsql.NpgsqlConnection conn, int datasetid)
        {
            logger.Info("GetMaxChangeLogId START");
            try
            {
                Int64 endChangeId = 0;
       
                string sqlSelectMaxChangeLogId = "SELECT COALESCE(MAX(endringsid),0) FROM " + p_dbSchema + ".endringslogg";

                NpgsqlCommand command = new NpgsqlCommand(sqlSelectMaxChangeLogId, conn);
                NpgsqlDataReader dr = command.ExecuteReader();
                dr.Read(); //Only one row
                endChangeId = dr.GetInt64(0);
                dr.Close();
                logger.Info("GetMaxChangeLogId END");
                return endChangeId;
            }
            catch (System.Exception exp)
            {
                logger.ErrorException("GetMaxChangeLogId function failed:", exp);
                throw new System.Exception("GetMaxChangeLogId function failed", exp);
            }
        }


        private bool CheckChangelogHasFeatures(XElement changeLog)
        {
            var reader = changeLog.CreateReader();
            XmlNamespaceManager manager = new XmlNamespaceManager(reader.NameTable);
            manager.AddNamespace("gml", "http://www.opengis.net/gml/3.2");
            //Search recursively for first occurence of attribute gml:id 
            XElement element = changeLog.XPathSelectElement("//*[@gml:id or @typeName][1]", manager);
            if (element == null)
            {
                return false;
            }

            return true;
        }


        #endregion

    }
    #region changeLogHandler
    public class changeLogHandler
    {


        string m_zipFile;
        string m_workingDirectory;
        //string m_changeLog;
        //StoredChangelog m_storedChangelog;
        //geosyncEntities m_db;
        Logger m_logger;

        public changeLogHandler(StoredChangelog sclog, Logger logger)
        {
            //m_storedChangelog = sclog;
            //m_db = db;
            m_logger = logger;
            m_workingDirectory = System.IO.Path.GetTempPath();

        }

        public changeLogHandler(Logger logger)
        {
            m_logger = logger;
            m_workingDirectory = System.IO.Path.GetTempPath();

        }

        protected class FtpState
        {
            private ManualResetEvent wait;
            private FtpWebRequest request;
            private string fileName;
            private Exception operationException = null;
            string status;

            public FtpState()
            {
                wait = new ManualResetEvent(false);
            }

            public ManualResetEvent OperationComplete
            {
                get { return wait; }
            }

            public FtpWebRequest Request
            {
                get { return request; }
                set { request = value; }
            }

            public string FileName
            {
                get { return fileName; }
                set { fileName = value; }
            }
            public Exception OperationException
            {
                get { return operationException; }
                set { operationException = value; }
            }
            public string StatusDescription
            {
                get { return status; }
                set { status = value; }
            }
        }

        protected class AsynchronousFtpUpLoader
        {

            private string m_ftpServer;
            private string m_user;
            private string m_pwd;
            //  private StoredChangelog m_storedChangelog;
            // private geosyncEntities m_db;
            private Logger m_logger;

            public AsynchronousFtpUpLoader(string ftpserver, string user, string password, Logger logger)
            {
                m_ftpServer = ftpserver;
                m_user = user;
                m_pwd = password;
                // m_storedChangelog = sclog;
                // m_db = db;
                m_logger = logger;
            }
            // Command line arguments are two strings:
            // 1. The url that is the name of the file being uploaded to the server.
            // 2. The name of the file on the local machine.
            //
            public void UploadFileToFtpServer(string fileName, string user = null, string password = null)
            {
                // Create a Uri instance with the specified URI string.
                // If the URI is not correctly formed, the Uri constructor
                // will throw an exception.
                ManualResetEvent waitObject;
                string file;
                if (fileName.Contains(@"\")) file = fileName.Substring(fileName.LastIndexOf(@"\") + 1); else file = fileName;

                //Må definere full target, ftp://server/folder/file.txt
                //Feiler dersom ikke file.txt er med.
                Uri target = new Uri(string.Concat("ftp://", m_ftpServer, "/", file));

                FtpState state = new FtpState();
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(target);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                bool passv = true;
                request.UsePassive = passv;
                request.UseBinary = true;
                request.Proxy = null;

                // This example uses anonymous logon.
                // The request is anonymous by default; the credential does not have to be specified. 
                // The example specifies the credential only to
                // control how actions are logged on the server.
                if (user == null || password == null)
                {
                    request.Credentials = new NetworkCredential(m_user, m_pwd);
                }
                else
                {
                    request.Credentials = new NetworkCredential(user, password);
                }

                // Store the request in the object that we pass into the
                // asynchronous operations.
                state.Request = request;
                state.FileName = fileName;

                // Get the event to wait on.
                waitObject = state.OperationComplete;

                // Asynchronously get the stream for the file contents.
                m_logger.Info(string.Format("Upload of file {0} started", fileName));
                //m_storedChangelog.Status = "started";
                request.BeginGetRequestStream(
                    new AsyncCallback(EndGetStreamCallback),
                    state
                );

                // Block the current thread until all operations are complete.
                waitObject.WaitOne(); //20130129-Leg: Must be called to let the upload complete

                // The operations either completed or threw an exception.
                if (state.OperationException != null)
                {
                    throw state.OperationException;
                }
                else
                {
                    //OnFileSendingProgressChanged(new CopyProgressEventArgs(state.FileName, 1, 1, string.Format("The operation completed - {0}", state.StatusDescription)));

                    //m_logger.Info(string.Format("Upload of file {0} started", fileName));
                    // m_storedChangelog.Status="started";
                    // m_db.SaveChanges();
                }
            }

            public void downloadFileFromFTPServer(string SourceFileName, string TargetFilename, string user = null, string password = null)
            {
                // Create a Uri instance with the specified URI string.
                // If the URI is not correctly formed, the Uri constructor
                // will throw an exception.
                ManualResetEvent waitObject;
                string file;
                if (SourceFileName.Contains(@"\")) file = SourceFileName.Substring(SourceFileName.LastIndexOf(@"\") + 1); else file = SourceFileName;

                //Må definere full target, ftp://server/folder/file.txt
                //Feiler dersom ikke file.txt er med.
                Uri target = new Uri(string.Concat("ftp://", m_ftpServer, "/", file));

                FtpState state = new FtpState();
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(target);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                bool passv = true;// Core.Config.GetSettingString(Core.Config.configSetting.SKSDftppassive) == "1";
                request.UsePassive = passv;
                request.UseBinary = true;
                request.Proxy = null;




                // This example uses anonymous logon.
                // The request is anonymous by default; the credential does not have to be specified. 
                // The example specifies the credential only to
                // control how actions are logged on the server.
                if (user == null || password == null)
                {
                    request.Credentials = new NetworkCredential(m_user, m_pwd);
                }
                else
                {
                    request.Credentials = new NetworkCredential(user, password);
                }

                // Store the request in the object that we pass into the
                // asynchronous operations.
                state.Request = request;
                state.FileName = TargetFilename;

                // Get the event to wait on.
                waitObject = state.OperationComplete;

                // Asynchronously get the stream for the file contents.
                request.BeginGetResponse(
                    new AsyncCallback(EndGetStreamCallbackDownload),
                    state
                );

                // Block the current thread until all operations are complete.
                //waitObject.WaitOne();

                // The operations either completed or threw an exception.
                if (state.OperationException != null)
                {
                    throw state.OperationException;
                }
                else
                {
                    m_logger.Info(string.Format("Download of file {0} started", target));
                    //m_storedChangelog.Status = "started";
                    //m_db.SaveChanges();
                }
            }

            private void EndGetStreamCallbackDownload(IAsyncResult ar)
            {
                FtpState state = (FtpState)ar.AsyncState;

                FtpWebResponse fileFtpRepsonse;
                Stream requestStream = null;
                FileStream stream = null;
                // End the asynchronous call to get the request stream.
                try
                {
                    fileFtpRepsonse = (FtpWebResponse)state.Request.EndGetResponse(ar);

                    requestStream = fileFtpRepsonse.GetResponseStream();
                    // Copy the file contents to the request stream.
                    const int bufferLength = 2048;
                    byte[] buffer = new byte[bufferLength];
                    int count = 0;
                    int readBytes = 2048;
                    stream = File.Create(state.FileName);
                    do
                    {
                        readBytes = requestStream.Read(buffer, 0, bufferLength);

                        stream.Write(buffer, 0, readBytes);
                        count += readBytes;
                    }
                    while (readBytes != 0);

                    // IMPORTANT: Close the request stream before sending the request.
                    requestStream.Close();
                    // Asynchronously get the response to the upload request.
                    state.Request.BeginGetResponse(
                        new AsyncCallback(EndGetResponseCallback),
                        state
                    );
                    m_logger.Info(string.Format("Download of file {0} finished", state.FileName));
                    // m_storedChangelog.Status = "finished";
                    //m_db.SaveChanges();
                }
                // Return exceptions to the main application thread.

                catch (Exception e)
                {
                    // Throw exception
                    state.OperationException = e;
                    state.OperationComplete.Set();
                    return;
                }
                finally
                {
                    if (requestStream != null)
                    {
                        requestStream.Dispose();
                        requestStream = null;
                    }
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                        stream = null;
                    }
                }


            }
            private void EndGetStreamCallback(IAsyncResult ar)
            {
                FtpState state = (FtpState)ar.AsyncState;

                Stream requestStream = null;
                FileStream stream = null;
                // End the asynchronous call to get the request stream.
                try
                {
                    requestStream = state.Request.EndGetRequestStream(ar);
                    // Copy the file contents to the request stream.
                    const int bufferLength = 2048;
                    byte[] buffer = new byte[bufferLength];
                    int count = 0;
                    int readBytes = 0;
                    stream = File.OpenRead(state.FileName);
                    do
                    {
                        readBytes = stream.Read(buffer, 0, bufferLength);
                        requestStream.Write(buffer, 0, readBytes);
                        count += readBytes;
                    }
                    while (readBytes != 0);
                    //OnFileSendingProgressChanged(new CopyProgressEventArgs(state.FileName, 1, 1, string.Format("Writing {0} bytes to the stream.", count.ToString())));
                    // IMPORTANT: Close the request stream before sending the request.
                    requestStream.Close();
                    // Asynchronously get the response to the upload request.
                    state.Request.BeginGetResponse(
                        new AsyncCallback(EndGetResponseCallback),
                        state
                    );
                    m_logger.Info(string.Format("Upload of file {0} finished", state.FileName));
                    //m_storedChangelog.Status = "finished";
                    //m_db.SaveChanges();
                }
                // Return exceptions to the main application thread.
                catch (Exception e)
                {
                    //OnFileSendingProgressChanged(new CopyProgressEventArgs(state.FileName, 1, 1, string.Format("Could not get the request stream. \n{0}", e.Message.ToString()), true));
                    state.OperationException = e;
                    state.OperationComplete.Set();
                    return;
                }
                finally
                {
                    if (requestStream != null)
                    {
                        requestStream.Dispose();
                        requestStream = null;
                    }
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                        stream = null;
                    }
                }


            }

            // The EndGetResponseCallback method  
            // completes a call to BeginGetResponse.
            private void EndGetResponseCallback(IAsyncResult ar)
            {
                FtpState state = (FtpState)ar.AsyncState;
                FtpWebResponse response = null;
                try
                {
                    response = (FtpWebResponse)state.Request.EndGetResponse(ar);
                    response.Close();
                    state.StatusDescription = response.StatusDescription;
                    // Signal the main application thread that 
                    // the operation is complete.
                    state.OperationComplete.Set();
                }
                // Return exceptions to the main application thread.
                catch (Exception e)
                {
                    Console.WriteLine("Error getting response.");
                    state.OperationException = e;
                    state.OperationComplete.Set();
                }
            }




            /*public bool DeleteFileOnServer(string fileName, string user = null, string password = null)
            {
                // The serverUri parameter should use the ftp:// scheme.
                // It contains the name of the server file that is to be deleted.
                // Example: ftp://contoso.com/someFile.txt.
                // 

                string file;
                if (fileName.Contains(@"\")) file = fileName.Substring(fileName.LastIndexOf(@"\") + 1); else file = fileName;
                //OnFileSendingProgressChanged(new CopyProgressEventArgs(fileName, 1, 1, string.Format("Delete status: {0}{1}", "Starts deleting file:", file)));

                Uri fullFilePath = new Uri(string.Concat("ftp://", m_ftpServer, "/", file));
                if (fullFilePath.Scheme != Uri.UriSchemeFtp)
                {
                    return false;
                }
                // Get the object used to communicate with the server.               
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fullFilePath);
                request.Method = WebRequestMethods.Ftp.DeleteFile;



                bool passv = true;// Core.Config.GetSettingString(Core.Config.configSetting.SKSDftppassive) == "1";
                request.UsePassive = passv;
                request.UseBinary = true;
                request.Proxy = null;




                // This example uses anonymous logon.
                // The request is anonymous by default; the credential does not have to be specified. 
                // The example specifies the credential only to
                // control how actions are logged on the server.
                if (user == null || password == null)
                {
                    request.Credentials = new NetworkCredential(m_user, m_pwd);
                }
                else
                {
                    request.Credentials = new NetworkCredential(user, password);
                }


                FtpWebResponse response = null;
                try
                {
                    response = (FtpWebResponse)request.GetResponse();
                    //OnFileSendingProgressChanged(new CopyProgressEventArgs(fileName, 1, 1, string.Format("Delete status: {0}", response.StatusDescription)));
                }
                catch (Exception ex)
                {
                    
                    return false;
                }
                finally
                {
                    if (response != null)
                    {
                        try
                        {
                            response.Close();
                        }
                        catch
                        {

                        }

                    }

                }
                return true;
            }
            */


            public static bool GetFilListFromFTP(string ftpServer, string user, string pwd)
            {
                List<string> strFileList = new List<string>();
                FtpWebRequest fwr;
                try
                {
                    fwr = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServer + "//"));
                    fwr.Credentials = new NetworkCredential(user, pwd);
                    fwr.Method = WebRequestMethods.Ftp.ListDirectory;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                StreamReader sr = null;
                try
                {

                    sr = new StreamReader(fwr.GetResponse().GetResponseStream());

                    string str = sr.ReadLine();
                    if (str == null) return false;
                    while (str != null)
                    {
                        strFileList.Add(str);
                        str = sr.ReadLine();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (sr != null) sr.Close();
                    sr = null;
                    fwr = null;
                }


                return true;
            }


            //public bool FindFileinFilList(string partOfFileName, out string zipfile)
            //{

            //    string file;
            //    if (partOfFileName.Contains(@"\")) file = partOfFileName.Substring(partOfFileName.LastIndexOf(@"\") + 1); else file = partOfFileName;
            //    OnFileSendingProgressChanged(new CopyProgressEventArgs(partOfFileName, 1, 1, string.Format("Check if file exists: {0}{1}", "Looking for file:", file)));
            //    zipfile = "";
            //    //if (!strFileList.Contains(partOfFileName)) return false;
            //    zipfile = strFileList.FirstOrDefault(s => s.StartsWith(partOfFileName) && s.EndsWith(".zip"));
            //    if (zipfile == null) return false;
            //    return true;
            //}

            public bool FileExistsOnServer(string fileName)
            {
                // The serverUri parameter should use the ftp:// scheme.
                // It contains the name of the server file that is to be deleted.
                // Example: ftp://contoso.com/someFile.txt.
                // 

                string file;
                if (fileName.Contains(@"\")) file = fileName.Substring(fileName.LastIndexOf(@"\") + 1); else file = fileName;
                //OnFileSendingProgressChanged(new CopyProgressEventArgs(fileName, 1, 1, string.Format("Check if file exists: {0}{1}", "Looking for file:", file)));

                Uri fullFilePath = new Uri(string.Concat("ftp://", m_ftpServer, "/", file));
                if (fullFilePath.Scheme != Uri.UriSchemeFtp)
                {
                    return false;
                }
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fullFilePath);
                request.Credentials = new NetworkCredential(m_user, m_pwd);
                request.Method = WebRequestMethods.Ftp.GetDateTimestamp;

                FtpWebResponse response = null;
                try
                {
                    response = (FtpWebResponse)request.GetResponse();
                    //OnFileSendingProgressChanged(new CopyProgressEventArgs(fileName, 1, 1, string.Format("Check file status: {0}", response.StatusDescription)));
                }
                catch (WebException ex)
                {
                    FtpWebResponse responseRes = (FtpWebResponse)ex.Response;
                    if (responseRes.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally
                {
                    if (response != null) response.Close();
                }
                return true;
            }


        }






        //private bool cleanUpTemp()
        //{
        //    string zipFile = m_zipFile;

        //    try
        //    {

        //        if (File.Exists(zipFile)) File.Delete(zipFile);
        //        if (Directory.Exists(m_workingDirectory)) Directory.Delete(m_workingDirectory, true);
        //    }
        //    catch (Exception exp)
        //    {
        //        m_logger.ErrorException("cleanUpTemp function failed:", exp);
        //        throw new System.Exception("cleanUpTemp function failed", exp);

        //    }
        //    return true;
        //}




        public bool UploadFileToFtp(string FileName, string ftpserver = null, string user = null, string password = null)
        {
            // Todo: get ftpserver from setting
            string server = "";
            if (ftpserver != null) server = ftpserver;

            AsynchronousFtpUpLoader ftp = new AsynchronousFtpUpLoader(server, user, password, m_logger);
            try
            {
                //ftp.FileSendingProgressChanged += new AsynchronousFtpUpLoader.FileSendingProgressHandler(FileSendingProgressChanged);
                ftp.UploadFileToFtpServer(Path.Combine(m_workingDirectory, FileName), user, password);

            }
            catch (Exception exp)
            {
                m_logger.ErrorException("UploadFileToFtp function failed:", exp);
                throw new System.Exception("UploadFileToFtp function failed", exp);

            }
            finally
            {
                ftp = null;
            }



            return true;
        }

        /*
        public bool DownloadFileFromFtp(string SourceFileName, string TargetFileName, string ftpserver = null, string user = null, string password = null)
        {
            // Todo: get ftpserver from setting
            string server = "";
            if (ftpserver != null) server = ftpserver;

            AsynchronousFtpUpLoader ftp = new AsynchronousFtpUpLoader(server, user, password, m_storedChangelog, m_db, m_logger);
            try
            {

                ftp.FileFromFTPServer(SourceFileName, TargetFileName, user, password);

            }
            catch (Exception exp)
            {
                m_logger.ErrorException("DownloadFileFromFtp function failed:", exp);
                throw new System.Exception("DownloadFileFromFtp function failed", exp);

            }
            finally
            {
                ftp = null;
            }



            return true;
        }




        /*
        public bool CleanUpFTP(string packageName, string ftpserver, string user, string pwd, bool formatZipFile = true)
        {
            string zipFile;
            if (formatZipFile)
            {
                if (packageName.Contains(@"\")) zipFile = packageName.Substring(packageName.LastIndexOf(@"\") + 1); else zipFile = packageName;
                if (!zipFile.ToLower().Contains(".ftp")) zipFile = String.Concat(zipFile, ".zip");
            }
            else zipFile = packageName;
            AsynchronousFtpUpLoader ftp = new AsynchronousFtpUpLoader(ftpserver, user, pwd, m_storedChangelog, m_db, m_logger);
            try
            {

                ftp.DeleteFileOnServer(zipFile);
            }
            catch (Exception exp)
            {
                m_logger.ErrorException("CleanUpFTP function failed:", exp);
                throw new System.Exception("CleanUpFTP function failed", exp);

            }
            finally
            {
                ftp = null;
            }
            return true;
        }

        */



        /*public bool unpackZipFile(string zipfile, string utpath)
        {
            using (ZipFile zip = ZipFile.Read(zipfile))
                foreach (ZipEntry fil in zip)
                {
                    fil.Extract(utpath, ExtractExistingFileAction.OverwriteSilently);
                }

            return true;
        }
        */

        public bool createZipFile(string infile, string zipFile)
        {
            using (ZipFile zip = new ZipFile())
            {
                m_zipFile = Path.Combine(m_workingDirectory, zipFile);
                zip.AddFile(infile, @"\");

                zip.Comment = "Changelog created " + DateTime.Now.ToString("G");
                zip.Save(m_zipFile);
                zip.Dispose();
            }
            return true;
        }

        public bool createZipFileFromFolder(string infolder, string zipFile, string toFolder)
        {
            using (ZipFile zip = new ZipFile())
            {
                m_zipFile = Path.Combine(m_workingDirectory, zipFile);
                zip.AddDirectory(infolder, @"\" + toFolder + @"\");

                zip.Comment = "Changelog created " + DateTime.Now.ToString("G");
                zip.Save(m_zipFile);
                zip.Dispose();
            }
            return true;
        }

        /*
        private string createWorkingDirectory(string name)
        {
            string tempPath = string.Concat(System.IO.Path.GetTempPath(), name);
            try
            {
                System.IO.Directory.CreateDirectory(tempPath);
            }
            catch (Exception exp)
            {
                m_logger.ErrorException("createWorkingDirectory function failed:", exp);
                throw new System.Exception("createWorkingDirectory function failed", exp);

            }
            return tempPath;
        }

        private bool CopyFileToDirectory(string fileName, string destination = null)
        {
            string destFileName = null;
            if (destination == null) destFileName = string.Concat(m_workingDirectory, fileName.Substring(fileName.LastIndexOf(@"\"))); else destFileName = destination;
            try
            {

                System.IO.File.Copy(fileName, destFileName);
                return true;
            }
            catch (Exception exp)
            {

                m_logger.ErrorException("CopyFileToDirectory function failed:", exp);
                throw new System.Exception("CopyFileToDirectory function failed", exp);
            }
        } */
    }
    #endregion
}
