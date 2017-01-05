using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
            // NLog for logging (nuget package)

        protected string PDbConnectInfo;
        private string _pNsApp;
        private string _pWfsUrl;
        private string _pNsPrefixTargetNamespace;
        protected string PDbSchema;
        private string _pSchemaFileUri;
        private OrderChangelog _currentOrderChangeLog = null;
        private string destFileName;
        private string destPath;
        private string zipFile;
        private string streamFileLocation;
        private string tmpzipFile;

        public void Intitalize(int datasetId)
        {
            PDbConnectInfo = DatasetsData.DatasetConnection(datasetId);
            _pWfsUrl = DatasetsData.TransformationConnection(datasetId);
            _pNsApp = DatasetsData.TargetNamespace(datasetId);
            _pNsPrefixTargetNamespace = DatasetsData.TargetNamespacePrefix(datasetId);
            PDbSchema = DatasetsData.DbSchema(datasetId);
            _pSchemaFileUri = DatasetsData.SchemaFileUri(datasetId);
            destFileName = Guid.NewGuid().ToString();
            destPath = Path.Combine(Path.GetTempPath(), destFileName) + "\\";
            zipFile = destFileName + ".zip";
            streamFileLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Changelogfiles\\" + zipFile;
            tmpzipFile = Path.Combine(Path.GetTempPath(), zipFile);
        }

        public abstract string GetLastIndex(int datasetId);

        public OrderChangelog GenerateInitialChangelog(int datasetId)
        {
            string downloadUriBase = ServerConfigData.DownloadUriBase().TrimEnd('/');

            using (geosyncEntities db = new geosyncEntities())
            {
                var initialChangelog = (from d in db.StoredChangelogs
                    where d.DatasetId == datasetId && d.StartIndex == 1 && d.Stored == true && d.Status == "finished"
                    orderby d.DateCreated descending
                    select d).FirstOrDefault();
                if (initialChangelog != null && initialChangelog.DownloadUri != null)
                {
                    Uri uri = new Uri(initialChangelog.DownloadUri);
                    ChangelogManager.DeleteFileOnServer(uri);
                    db.StoredChangelogs.DeleteObject(initialChangelog);
                    db.SaveChanges();
                }
            }
            int startIndex = 1; // StartIndex always 1 on initial changelog
            int endIndex = Convert.ToInt32(GetLastIndex(datasetId));
            int count = 1000; // TODO: Get from dataset table
            Logger.Info("GenerateInitialChangelog START");
            StoredChangelog ldbo = new StoredChangelog();
            ldbo.Stored = true;
            ldbo.Status = "queued";
            ldbo.StartIndex = startIndex;

            ldbo.DatasetId = datasetId;
            ldbo.DateCreated = DateTime.Now;

            //TODO make filter 
            //TODO check if similar stored changelog is already done
            using (geosyncEntities db = new geosyncEntities())
            {
                // Store changelog info in database
                db.StoredChangelogs.AddObject(ldbo);

                OrderChangelog resp = new OrderChangelog();
                resp.changelogId = ldbo.ChangelogId.ToString();

                //New thread and do the work....
                // We're coming back to the thread handling later...
                //string sourceFileName = "Changelogfiles/41_changelog.xml";


                Directory.CreateDirectory(destPath);
                // Loop and create xml files
                int i = 1;
                while (i++ <= Convert.ToInt32(Math.Ceiling((double) endIndex/count)))
                {
                    string partFileName = DateTime.Now.Ticks + ".xml";

                    string fullPathWithFile = Path.Combine(destPath, partFileName);
                    MakeChangeLog(startIndex, count, PDbConnectInfo, _pWfsUrl, fullPathWithFile, datasetId);
                    startIndex += count;
                    endIndex = Convert.ToInt32(GetLastIndex(datasetId));
                }

                // Save endIndex to database
                ldbo.EndIndex = endIndex;

                // New code to handle FTP download
                ChangeLogHandler chgLogHandler = new ChangeLogHandler(ldbo, Logger);
                string inFile = "";
                try
                {
                    inFile = destPath;
                    chgLogHandler.CreateZipFileFromFolder(inFile, zipFile, destFileName);
                    ldbo.Status = "queued"; 
                    File.Copy(tmpzipFile, streamFileLocation);
                    File.Delete(tmpzipFile);
                    ldbo.Status = "finished"; 
                }
                catch (Exception ex)
                {
                    Logger.ErrorException(string.Format("Failed to create or upload file {0}", zipFile), ex);
                    throw ex;
                }

                try
                {
                    string downLoadUri = string.Format(@"{0}/{1}", downloadUriBase, zipFile);

                    ldbo.DownloadUri = downLoadUri;
                }
                catch (Exception ex)
                {
                    Logger.ErrorException(string.Format("Failed to create or upload file {0}", zipFile), ex);
                    throw ex;
                }


                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.ErrorException(
                        string.Format(
                            "Failed on SaveChanges, Kartverket.Geosynkronisering.ChangelogProviders.PostGISChangelog.OrderChangelog startIndex:{0} count:{1} changelogId:{2}",
                            startIndex, count, ldbo.ChangelogId), ex);
                    throw ex;
                }
                Logger.Info(
                    "Kartverket.Geosynkronisering.ChangelogProviders.PostGISChangelog.OrderChangelog" +
                    " startIndex:{0}" + " count:{1}" + " changelogId:{2}", startIndex, count, ldbo.ChangelogId);

                Logger.Info("GenerateInitialChangelog END");
                return resp;
            }
        }

        public OrderChangelog CreateChangelog(int startIndex, int count, string todoFilter, int datasetId)
        {
            using (geosyncEntities db = new geosyncEntities())
            {
                if (startIndex < 2)
                {
                    
                    var initialChangelog = (from d in db.StoredChangelogs
                        where
                            d.DatasetId == datasetId && d.StartIndex == 1 && d.Stored == true && d.Status == "finished"
                        orderby d.DateCreated descending
                        select d).FirstOrDefault();
                    if (initialChangelog != null)
                    {
                        OrderChangelog resp = new OrderChangelog();
                        resp.changelogId = initialChangelog.ChangelogId.ToString();
                        _currentOrderChangeLog = resp;
                        return resp;
                    }

                }
                Logger.Info("CreateChangelog START");
                ChangelogManager chlmng = new ChangelogManager(db);
                _currentOrderChangeLog = chlmng.CreateChangeLog(startIndex, count, datasetId);
                chlmng.SetStatus(_currentOrderChangeLog.changelogId, ChangelogStatusType.queued);
            }
            return _currentOrderChangeLog;
        }

        public OrderChangelog _OrderChangelog(int startIndex, int count, string todoFilter, int datasetId)
        {
            string downloadUriBase = ServerConfigData.DownloadUriBase().TrimEnd('/');
            using (geosyncEntities db = new geosyncEntities())
            {
                ChangelogManager chlmng = new ChangelogManager(db);
                
                chlmng.SetStatus(_currentOrderChangeLog.changelogId, ChangelogStatusType.working);
                //System.IO.File.Copy(Utils.BaseVirtualAppPath + sourceFileName, Utils.BaseVirtualAppPath + destFileName);
                try
                {
                    if (!Directory.Exists(destPath)) Directory.CreateDirectory(destPath);
                    MakeChangeLog(startIndex, count, PDbConnectInfo, _pWfsUrl, destPath + destFileName + ".xml",
                        datasetId);
                }
                catch (Exception ex)
                {
                    chlmng.SetStatus(_currentOrderChangeLog.changelogId, ChangelogStatusType.cancelled);
                    Logger.ErrorException(
                        string.Format("Failed to make Change Log {0}", destPath + destFileName + ".xml"), ex);
                    throw ex;
                }

                // New code to handle FTP download
                ChangeLogHandler chgLogHandler = new ChangeLogHandler(Logger);
                string inFile = "";
                try
                {
                    inFile = destPath;
                    chgLogHandler.CreateZipFileFromFolder(inFile, zipFile, destFileName);
                    File.Copy(tmpzipFile, streamFileLocation);
                    File.Delete(tmpzipFile);
                }
                catch (Exception ex)
                {
                    chlmng.SetStatus(_currentOrderChangeLog.changelogId, ChangelogStatusType.cancelled);
                    Logger.ErrorException(string.Format("Failed to create or upload file {0}", zipFile), ex);
                    throw ex;
                }

                try
                {
                    string downLoadUri = string.Format(@"{0}/{1}", downloadUriBase, zipFile);
                    chlmng.SetStatus(_currentOrderChangeLog.changelogId, ChangelogStatusType.finished);
                        chlmng.SetDownloadURI(_currentOrderChangeLog.changelogId, downLoadUri);
                }
                catch (Exception ex)
                {
                    Logger.ErrorException(string.Format("Failed to create or upload file {0}", zipFile), ex);
                    throw ex;
                }

                Logger.Info(
                    "Kartverket.Geosynkronisering.ChangelogProviders.PostGISChangelog.OrderChangelog" +
                    " startIndex:{0}" + " count:{1}" + " changelogId:{2}", startIndex, count,
                    _currentOrderChangeLog.changelogId);

                Logger.Info("OrderChangelog END");
                return _currentOrderChangeLog;
            }
        }

        public OrderChangelog OrderChangelog(int startIndex, int count, string todoFilter, int datasetId)
        {
            // If startIndex == 1: Check if initital changelog exists
            if (startIndex == 1)
            {
                using (geosyncEntities db = new geosyncEntities())
                {
                    var initialChangelog = (from d in db.StoredChangelogs
                        where
                            d.DatasetId == datasetId && d.StartIndex == 1 && d.Stored == true && d.Status == "finished"
                        orderby d.DateCreated descending
                        select d).FirstOrDefault();

                    if (initialChangelog != null)
                    {
                        OrderChangelog resp = new OrderChangelog();
                        resp.changelogId = initialChangelog.ChangelogId.ToString();
                        return resp;
                    }
                }
            }

            // If initial changelog don't exists or startIndex != 1
            return _OrderChangelog(startIndex, count, todoFilter, datasetId);
        }

        public abstract void MakeChangeLog(int startChangeId, int count, string dbConnectInfo, string wfsUrl,
            string changeLogFileName, int datasetId);

        public void BuildChangeLogFile(int count, List<OptimizedChangeLogElement> optimizedChangeLog, string wfsUrl,
            int startChangeId, Int64 endChangeId,
            string changeLogFileName, int datasetId)
        {
            Logger.Info("BuildChangeLogFile START");
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
                    string gmlId = current.GmlId;
                    string transType = current.TransType;
                    long changeId = current.ChangeId;
                    long handle = 0;

                    //If next element == lastelement
                    if ((i + 1) == optimizedChangeLog.Count)
                    {
                        handle = endChangeId;
                    }
                    else
                    {
                        OptimizedChangeLogElement next = optimizedChangeLog.ElementAt(i + 1);
                        handle = next.ChangeId - 1;
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
                        current.Handle = handle;
                        inserts.Add(current);
                        var lastElement = (i + 1 == optimizedChangeLog.Count) || (counter == count);
                        if (!lastElement)
                        {
                            OptimizedChangeLogElement next = optimizedChangeLog.ElementAt(i + 1);
                            string nextChange = next.TransType;
                            if (nextChange != "I") //if next is not insert
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
                    Exception exp = new Exception("CheckChangelogHasFeatures found 0 features");
                    Logger.ErrorException("CheckChangelogHasFeatures found 0 features", exp);
                    throw exp;
                }

                //store changelog to file
                changeLog.Save(changeLogFileName);
            }
            catch (Exception exp)
            {
                Logger.ErrorException("BuildChangeLogFile function failed:", exp);
                throw new Exception("BuildChangeLogFile function failed", exp);
            }
            Logger.Info("BuildChangeLogFile END");
        }

        private void AddReferencedFeatureToChangelog(XElement changeLog, XElement parentElement, XElement getFeatureResponse)
        {
            XNamespace xlink = "http://www.w3.org/1999/xlink";

            foreach (var childElement in parentElement.Elements())
            {
                var hrefAttribute = childElement.Attribute(xlink + "href");
                if (hrefAttribute != null)
                {
                    var lokalid = hrefAttribute.Value.Split('.')[hrefAttribute.Value.Split('.').Length - 1];

                    XElement referencedElement = FetchFeatureByLokalid(lokalid, getFeatureResponse);
                    changeLog.Add(referencedElement);
                    AddReferencedFeatureToChangelog(changeLog, referencedElement, getFeatureResponse);
                }
                AddReferencedFeatureToChangelog(changeLog, childElement, getFeatureResponse);
            }
        }

        private XElement FetchFeatureByLokalid(string lokalid, XElement getFeatureResponse)
        {
            XNamespace nsChlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.1/endringslogg";
            XNamespace nsApp = _pNsApp;
            string nsPrefixApp = "app";
            XmlNamespaceManager mgr = new XmlNamespaceManager(new NameTable());
            mgr.AddNamespace(nsPrefixApp, nsApp.NamespaceName);
            string nsPrefixAppComplete = nsPrefixApp + ":";
            string xpathExpressionLokalid = "//" + nsPrefixAppComplete + "identifikasjon/" + nsPrefixAppComplete +
                                            "Identifikasjon[" + nsPrefixAppComplete + "lokalId='" + lokalid +
                                            "']/../..";

            return getFeatureResponse.XPathSelectElement(xpathExpressionLokalid, mgr);
        }

        private void AddInsertPortionsToChangeLog(List<OptimizedChangeLogElement> insertList, string wfsUrl,
            XElement changeLog, int datasetId)
        {
            int portionSize = 100;

            List<OptimizedChangeLogElement> insertsListPortion = new List<OptimizedChangeLogElement>();
            int portionCounter = 0;
            foreach (OptimizedChangeLogElement insert in insertList)
            {
                portionCounter++;
                insertsListPortion.Add(insert);
                if (portionCounter%portionSize == 0)
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

        private void AddInsertsToChangeLog(List<OptimizedChangeLogElement> insertsGmlIds, string wfsUrl,
            XElement changeLog, int datasetId)
        {
            List<string> typeNames = new List<string>();

            ChangelogWFS wfs = new ChangelogWFS();

            Dictionary<string, string> typeIdDict = new Dictionary<string, string>();

            List<string> gmlIds = new List<string>();

            long handle = 0;
            foreach (OptimizedChangeLogElement insert in insertsGmlIds)
            {
                gmlIds.Add(insert.GmlId);

                if (insert.Handle > handle)
                    handle = insert.Handle;
            }

            XElement getFeatureResponse = wfs.GetFeatureCollectionFromWFS(_pNsPrefixTargetNamespace, _pNsApp, wfsUrl,
                ref typeIdDict, gmlIds, datasetId);

            //Build inserts for each typename
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";
            XNamespace nsChlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.1/endringslogg";

            XNamespace nsApp = _pNsApp;
            // 20130917-Leg: Fix
            string nsPrefixApp = changeLog.GetPrefixOfNamespace(nsApp);
            XmlNamespaceManager mgr = new XmlNamespaceManager(new NameTable());
            mgr.AddNamespace(nsPrefixApp, nsApp.NamespaceName);
            string nsPrefixAppComplete = nsPrefixApp + ":";

            XElement insertElement = new XElement(nsWfs + "Insert", new XAttribute("handle", handle));

            foreach (KeyValuePair<string, string> dictElement in typeIdDict)
            {
                XElement feature = FetchFeatureByLokalid(dictElement.Key, getFeatureResponse);
                insertElement.Add(feature);
                AddReferencedFeatureToChangelog(insertElement, feature, getFeatureResponse);
            }
            
            changeLog.Element(nsChlogf + "transactions").Add(insertElement);    
        }

        private void AddDeleteToChangeLog(string gmlId, long handle, XElement changeLog, int datasetId)
        {
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";
            XNamespace nsChlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.1/endringslogg";
            XNamespace nsFes = "http://www.opengis.net/fes/2.0";
            XNamespace nsApp = _pNsApp;
            string nsPrefixApp = changeLog.GetPrefixOfNamespace(nsApp);
            string nsPrefixAppComplete = nsPrefixApp + ":";
            string xpathExpressionLokalid = nsPrefixAppComplete + "identifikasjon/" + nsPrefixAppComplete +
                                            "Identifikasjon/" + nsPrefixAppComplete + "lokalId";

            int pos = gmlId.IndexOf(".");
            string typename = gmlId.Substring(0, pos);
            string lokalId = gmlId.Substring(pos + 1);
            //new XAttribute("inputFormat", "application/gml+xml; version=3.2"), 
            XElement deleteElement = new XElement(nsWfs + "Delete", new XAttribute("handle", handle),
                new XAttribute("typeName", nsPrefixApp + ":" + typename),
                //new XAttribute("typeName", "app:" + typename),
                new XAttribute(XNamespace.Xmlns + nsPrefixApp, nsApp));
            //XElement deleteElement = new XElement(nsWfs + "Delete", new XAttribute("handle", transCounter), new XAttribute("typeName", typename),
            //    new XAttribute("inputFormat", "application/gml+xml; version=3.2"));

            //deleteElement.Add(getFeatureResponse.Element(nsWfs + "member").Nodes());
            //Add filter
            // 20121031-Leg: "lokal_id" replaced by "lokalId"
            // 20131015-Leg: Filter ValueReference content with namespace prefix
            deleteElement.Add(new XElement(nsFes + "Filter",
                new XElement(nsFes + "PropertyIsEqualTo",
                    new XElement(nsFes + "ValueReference", xpathExpressionLokalid),
                    new XElement(nsFes + "Literal", lokalId)
                    )
                ));

            changeLog.Element(nsChlogf + "transactions").Add(deleteElement);
        }

        private void AddReplaceToChangeLog(string gmlId, long handle, string wfsUrl, XElement changeLog, int datasetId)
        {
            Dictionary<string, string> typeIdDict = new Dictionary<string, string>();
            ChangelogWFS wfs = new ChangelogWFS();
            XElement getFeatureResponse = wfs.GetFeatureCollectionFromWFS(_pNsPrefixTargetNamespace, _pNsApp, wfsUrl,
                ref typeIdDict, new List<string>() {gmlId}, datasetId);
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";
            XNamespace nsChlogf = "http://skjema.geonorge.no/standard/geosynkronisering/1.1/endringslogg";
            XNamespace nsFes = "http://www.opengis.net/fes/2.0";

            XNamespace nsApp = _pNsApp;
            // 20130917-Leg: Fix
            string nsPrefixApp = changeLog.GetPrefixOfNamespace(nsApp);
            string nsPrefixAppComplete = nsPrefixApp + ":";
            string xpathExpressionLokalidFilter = nsPrefixAppComplete + "identifikasjon/" + nsPrefixAppComplete +
                                                  "Identifikasjon/" + nsPrefixAppComplete + "lokalId";

            foreach (KeyValuePair<string, string> dictElement in typeIdDict)
            {
                XElement feature = FetchFeatureByLokalid(dictElement.Key, getFeatureResponse);
                XElement replaceElement = new XElement(nsWfs + "Replace", new XAttribute("handle", handle));
                replaceElement.Add(feature);
                
                //TODO: Check if this can be neccessary
                //AddReferencedFeatureToChangelog(replaceElement, feature, getFeatureResponse);
                replaceElement.Add(new XElement(nsFes + "Filter",
                    new XElement(nsFes + "PropertyIsEqualTo",
                        new XElement(nsFes + "ValueReference", xpathExpressionLokalidFilter),
                        new XElement(nsFes + "Literal", dictElement.Key)
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
            XNamespace nsApp = _pNsApp;
            XNamespace nsWfs = "http://www.opengis.net/wfs/2.0";
            XNamespace nsXsi = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace nsGml = "http://www.opengis.net/gml/3.2";

            // 20150407-Leg: Correct xsd location
            // TODO: Should not be hardcoded
            string schemaLocation = nsChlogf.NamespaceName + " " + ServiceData.SchemaLocation();
            schemaLocation += " " + _pNsApp + " " + _pSchemaFileUri;

            //"2001-12-17T09:30:47Z"
            XElement changelogRoot =
                new XElement(nsChlogf + "TransactionCollection",
                    new XAttribute("timeStamp", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffzzz")),
                    new XAttribute("numberMatched", ""), new XAttribute("numberReturned", ""),
                    new XAttribute("startIndex", ""), new XAttribute("endIndex", ""),
                    new XAttribute(XNamespace.Xmlns + "xsi", nsXsi),
                    new XAttribute(nsXsi + "schemaLocation", schemaLocation),
                    new XAttribute(XNamespace.Xmlns + "chlogf", nsChlogf),
                    new XAttribute(XNamespace.Xmlns + "app", nsApp),
                    new XAttribute(XNamespace.Xmlns + "wfs", nsWfs),
                    new XAttribute(XNamespace.Xmlns + "gml", nsGml)
                    );
            changelogRoot.Add(new XElement(nsChlogf + "transactions", new XAttribute("service", "WFS"),
                new XAttribute("version", "2.0.0")));
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
        public string GmlId;
        public string TransType;
        public long ChangeId;
        public long Handle;

        public OptimizedChangeLogElement(string gmlId, string transType, long changeId)
        {
            GmlId = gmlId;
            TransType = transType;
            ChangeId = changeId;
        }
    }

    public class ChangeLogHandler
    {


        private string _mZipFile;
        private string _mWorkingDirectory;
        //string m_changeLog;
        //StoredChangelog m_storedChangelog;
        //geosyncEntities m_db;
        private Logger _mLogger;

        public ChangeLogHandler(StoredChangelog sclog, Logger logger)
        {
            //m_storedChangelog = sclog;
            //m_db = db;
            _mLogger = logger;
            _mWorkingDirectory = Path.GetTempPath();

        }

        public ChangeLogHandler(Logger logger)
        {
            _mLogger = logger;
            _mWorkingDirectory = Path.GetTempPath();

        }

        public bool CreateZipFile(string infile, string zipFile)
        {
            using (ZipFile zip = new ZipFile())
            {
                _mZipFile = Path.Combine(_mWorkingDirectory, zipFile);
                zip.AddFile(infile, @"\");

                zip.Comment = "Changelog created " + DateTime.Now.ToString("G");
                zip.Save(_mZipFile);
                zip.Dispose();
            }
            return true;
        }

        public bool CreateZipFileFromFolder(string infolder, string zipFile, string toFolder)
        {
            using (ZipFile zip = new ZipFile())
            {
                _mZipFile = Path.Combine(_mWorkingDirectory, zipFile);
                zip.AddDirectory(infolder, @"\" + toFolder + @"\");

                zip.Comment = "Changelog created " + DateTime.Now.ToString("G");
                zip.Save(_mZipFile);
                zip.Dispose();
            }
            return true;
        }
    }
}