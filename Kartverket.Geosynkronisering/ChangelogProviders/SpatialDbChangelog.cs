using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Ionic.Zip;
using Kartverket.GeosyncWCF;
using Kartverket.Geosynkronisering.Database;
using NLog;

namespace Kartverket.Geosynkronisering.ChangelogProviders
{
    public abstract class SpatialDbChangelog : IChangelogProvider
    {
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)
        protected string p_dbConnectInfo;
        private string p_nsApp;
        private string p_wfsURL;
        private string p_nsPrefixTargetNamespace;
        protected string p_dbSchema;
        private string p_SchemaFileUri;
        private OrderChangelog CurrentOrderChangeLog = null;
        private string BaseVirtualPath;

        public void Intitalize( int datasetId)
        {
            p_dbConnectInfo = Database.DatasetsData.DatasetConnection(datasetId);
            p_wfsURL = Database.DatasetsData.TransformationConnection(datasetId);
            p_nsApp = Database.DatasetsData.TargetNamespace(datasetId);
            p_nsPrefixTargetNamespace = Database.DatasetsData.TargetNamespacePrefix(datasetId);
            p_dbSchema = Database.DatasetsData.DBSchema(datasetId);
            p_SchemaFileUri = Database.DatasetsData.SchemaFileUri(datasetId);
        }

        public abstract string GetLastIndex(int datasetId);

        public OrderChangelog GenerateInitialChangelog(int datasetId)
        {
            string ftpURL = Database.ServerConfigData.FTPUrl();
            string ftpUser = Database.ServerConfigData.FTPUser();
            string ftpPwd = Database.ServerConfigData.FTPPwd();
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

        public OrderChangelog CreateChangelog(int startIndex, int count, string todo_filter, int datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                if (startIndex == -1)
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
                    string downLoadUri = string.Format(@"ftp://{0}:{1}@{2}/{3}",ftpUser, ftpPwd, ftpURL, destFileName);
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
            string ftpURL = Database.ServerConfigData.FTPUrl();
            string ftpUser = Database.ServerConfigData.FTPUser();
            string ftpPwd = Database.ServerConfigData.FTPPwd();
            MakeChangeLog(startIndex, count, p_dbConnectInfo, p_wfsURL, Utils.BaseVirtualAppPath + destFileName + ".xml", datasetId);

            // New code to handle FTP download
           

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

        public abstract void MakeChangeLog(int startChangeId, int count, string dbConnectInfo, string wfsUrl, string changeLogFileName, int datasetId);

        public void BuildChangeLogFile(int count, List<OptimizedChangeLogElement> optimizedChangeLog, string wfsUrl, int startChangeId, Int64 endChangeId,
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
            XNamespace nsChlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.1/endringslogg";

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
            XNamespace nsChlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.1/endringslogg";
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
            XNamespace nsChlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.1/endringslogg";
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

        private void UpdateRootAttributes(XElement changeLog, int counter, int startChangeId, Int64 endChangeId)
        {
            changeLog.SetAttributeValue("numberMatched", counter);
            changeLog.SetAttributeValue("numberReturned", counter);
            changeLog.SetAttributeValue("startIndex", startChangeId);
            changeLog.SetAttributeValue("endIndex", endChangeId);
        }

        private XElement BuildChangelogRoot(int datasetId)
        {
            XNamespace nsChlogf = ServiceData.Namespace();
            XNamespace nsApp = p_nsApp;
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";
            XNamespace nsXsi = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace nsGml = "http://www.opengis.net/gml/3.2";

            // 20150407-Leg: Correct xsd location
            // TODO: Should not be hardcoded
            string schemaLocation = nsChlogf.NamespaceName + " " + ServiceData.SchemaLocation();
            schemaLocation += " " + p_nsApp + " " + p_SchemaFileUri;

            //"2001-12-17T09:30:47Z"
            XElement changelogRoot =
                new XElement(nsChlogf + "TransactionCollection", new XAttribute("timeStamp", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffz")), new XAttribute("numberMatched", ""), new XAttribute("numberReturned", ""), new XAttribute("startIndex", ""), new XAttribute("endIndex", ""),
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
    }

    public class OptimizedChangeLogElement
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
    }
}