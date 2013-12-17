using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Kartverket.GeosyncWCF;
using Kartverket.Geosynkronisering.Subscriber.BL.SchemaMapping;
using Kartverket.Geosynkronisering.Subscriber.DL;
using NLog;

namespace Kartverket.Geosynkronisering.Subscriber.BL
{
    /// <summary>
    /// 
    /// </summary>
    public class SynchController : FeedbackController.Progress
    {
        public SynchController()
        {
            InitTransactionsSummary();

            //var wfsController = new WfsController();
        }

        public void InitTransactionsSummary()
        {
            TransactionsSummary = new TransactionSummary();
            TransactionsSummary.TotalDeleted = 0;
            TransactionsSummary.TotalInserted = 0;
            TransactionsSummary.TotalReplaced = 0;
            TransactionsSummary.TotalUpdated = 0;
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

        public TransactionSummary TransactionsSummary;


        public IBindingList GetCapabilitiesProviderDataset(string url)
        {
            var cdb = new CapabilitiesDataBuilder(url);
            return cdb.ProviderDatasets;
        }

        /// <summary>
        /// Resets the subscribers last index for a given dataset
        /// </summary>
        /// <param name="datasetId"></param>
        public void ResetSubscriberLastIndex(int datasetId)
        {
            var dataset = DL.SubscriberDatasetManager.GetDataset(datasetId);
            dataset.LastIndex = 0;
            DL.SubscriberDatasetManager.UpdateDataset(dataset);
        }

        /// <summary>
        /// Get OrderChangelog Response and changelogid
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public int OrderChangelog(int datasetId, int startIndex)
        {
            try
            {
                var dataset = DL.SubscriberDatasetManager.GetDataset(datasetId);

                var client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SynchronizationUrl);
                var order = new ChangelogOrderType();
                order.datasetId = dataset.ProviderDatasetId.ToString();
                order.count = dataset.MaxCount.ToString();
                order.startIndex = startIndex.ToString();

                ChangelogIdentificationType resp = client.OrderChangelog(order);

                int changeLogId;
                Int32.TryParse(resp.changelogId, out changeLogId);
                return changeLogId;
            }
            catch (Exception ex)
            {
                logger.ErrorException("OrderChangelog failed:", ex);
                throw;
            }
        }


        /// <summary>
        /// Get ChangelogStatus Response
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="changelogId"></param>
        /// <returns></returns>
        public ChangelogStatusType GetChangelogStatusResponse(int datasetId, int changelogId)
        {
            try
            {
                var dataset = SubscriberDatasetManager.GetDataset(datasetId);

                var client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SynchronizationUrl);

                var id = new ChangelogIdentificationType { changelogId = changelogId.ToString() };

                ChangelogStatusType resp = client.GetChangelogStatus(id);

                return resp;
            }
            catch (Exception ex)
            {
                logger.ErrorException("GetChangelogStatusResponse failed:", ex);
                throw;
            }
        }

        /// <summary>
        /// Get and download changelog
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="changelogId"></param>
        /// <param name="downloadController"></param>
        /// <returns></returns>
        public bool GetChangelog(int datasetId, int changelogId, out DownloadController downloadController)
        {
            try
            {
                var dataset = DL.SubscriberDatasetManager.GetDataset(datasetId);

                var client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SynchronizationUrl);

                var id = new ChangelogIdentificationType { changelogId = changelogId.ToString() };

                ChangelogType resp = client.GetChangelog(id);

                string downloaduri = resp.downloadUri;
                string tempDir = System.Environment.GetEnvironmentVariable("TEMP");
#if (NOT_FTP)
                string fileName = tempDir + @"\" + changelogid + "_Changelog.xml";
#else
                const string ftpPath = "abonnent";
                BL.Utils.Misc.CreateFolderIfMissing(tempDir + @"\" + ftpPath); // Create the abonnent folder if missing               

                string fileName = tempDir + @"\" + ftpPath + @"\" + Path.GetFileName(downloaduri) + ".zip";
#endif

                downloadController = new DownloadController { ChangelogFilename = fileName };
                downloadController.DownloadChangelog2(downloaduri);
            }
            catch (WebException webEx)
            {
                logger.ErrorException("GetChangelog failed:", webEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.ErrorException("GetChangelog failed:", ex);
                throw;
            }
            return true;
        }

        /// <summary>
        /// ListStoredChangelogs
        /// </summary>
        /// <returns></returns>
        public string ListStoredChangelogs(int datasetId)
        {
            try
            {
                var dataset = DL.SubscriberDatasetManager.GetDataset(datasetId);

                var client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SynchronizationUrl);

                StoredChangelogType[] resp = client.ListStoredChangelogs(dataset.ProviderDatasetId.ToString());
                return "Antall lagrede endringslogger:" + resp.Count();
            }
            catch (WebException webEx)
            {
                logger.ErrorException("ListStoredChangelogs WebException:", webEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.ErrorException("ListStoredChangelogs failed:", ex);
                throw;
            }
        }


        /// <summary>
        /// Bekrefte at endringslogg er lastet ned
        /// </summary>
        /// <returns></returns>
        public bool AcknowledgeChangelogDownloaded(int datasetId, int changelogId)
        {
            try
            {
                var dataset = DL.SubscriberDatasetManager.GetDataset(datasetId);

                var client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SynchronizationUrl);

                var id = new ChangelogIdentificationType { changelogId = changelogId.ToString() };
                client.AcknowlegeChangelogDownloaded(id);

                return true;
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Success)
                {
                    return false;
                }

                logger.ErrorException("AcknowledgeChangelogDownloaded WebException:", webEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.ErrorException("AcknowledgeChangelogDownloaded failed:", ex);
                throw;
            }
        }

        /// <summary>
        /// Avbryter endringslogg jobb hvis feks noe går galt.
        /// </summary>
        /// <returns></returns>
        public bool CancelChangelog(int datasetId, string changelogId)
        {
            try
            {
                var dataset = DL.SubscriberDatasetManager.GetDataset(datasetId);

                var client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SynchronizationUrl);
                var id = new ChangelogIdentificationType();
                id.changelogId = changelogId;
                client.CancelChangelog(id);

                return true;
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Success)
                {
                    return true;
                }

                logger.ErrorException("CancelChangelog WebException:", webEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.ErrorException("CancelChangelog failed:", ex);
                throw;
            }
        }

        /// <summary>
        /// Henter siste endringsnr fra tilbyder. Brukes for at klient enkelt kan sjekke om det er noe nytt siden siste synkronisering
        /// </summary>
        /// <returns></returns>
        public int GetLastIndexFromProvider(int datasetId)
        {
            try
            {
                var dataset = DL.SubscriberDatasetManager.GetDataset(datasetId);

                var client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SynchronizationUrl);

                var lastIndexString = client.GetLastIndex(dataset.ProviderDatasetId.ToString());
                int lastIndex;
                Int32.TryParse(lastIndexString, out lastIndex);

                return lastIndex;
            }

            catch (WebException webEx)
            {
                logger.ErrorException("GetLastIndexFromProvider WebException:", webEx);
                throw;
            }
            catch (Exception ex)
            {
                logger.ErrorException("GetLastIndexFromProvider failed:", ex);
                throw;
            }
        }

        public int GetLastIndexFromSubscriber(int datasetId)
        {
            var dataset = DL.SubscriberDatasetManager.GetDataset(datasetId);

            if (dataset == null)
                return -1;

            return dataset.LastIndex;
        }

        /// <summary>
        /// Synchronizing of a given dataset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DoSynchronization(int datasetId)
        {
            try
            {
                // Create new stopwatch
                var stopwatch = Stopwatch.StartNew();

                this.OnNewSynchMilestoneReached("GetLastIndexFromProvider");
                var lastChangeIndexProvider = GetLastIndexFromProvider(datasetId);

                var logMessage = "GetLastIndexFromProvider lastIndex: " + lastChangeIndexProvider;
                logger.Info(logMessage);
                this.OnUpdateLogList(logMessage);
                this.OnNewSynchMilestoneReached("GetLastIndexFromProvider OK");

                this.OnNewSynchMilestoneReached("GetLastIndexFromSubscriber");
                int lastChangeIndexSubscriber = GetLastIndexFromSubscriber(datasetId);

                logMessage = "GetLastChangeIndexSubscriber lastIndex: " + lastChangeIndexSubscriber;
                logger.Info(logMessage);
                this.OnUpdateLogList(logMessage);
                this.OnNewSynchMilestoneReached("GetLastIndexFromSubscriber OK");

                if (lastChangeIndexSubscriber >= lastChangeIndexProvider)
                {
                    logMessage = "Changelog has already been downloaded and handled:";
                    //this.OnUpdateLogList(logMessage); // Raise event to UI
                    //this.OnNewSynchMilestoneReached(logMessage);
                    logMessage += " Provider lastIndex:" + lastChangeIndexProvider + " Subscriber lastChangeIndex: " + lastChangeIndexSubscriber;
                    logger.Info(logMessage);
                    this.OnUpdateLogList(logMessage); // Raise event to UI
                    this.OnNewSynchMilestoneReached(logMessage);
                    return;
                }

                this.OnNewSynchMilestoneReached("Order Changelog. Wait...");

                int maxCount = DL.SubscriberDatasetManager.GetMaxCount(datasetId);

                int numberOfFeatures = lastChangeIndexProvider - lastChangeIndexSubscriber;
                int numberOfOrders = (numberOfFeatures / maxCount);
                if (numberOfFeatures % maxCount > 0)
                    ++numberOfOrders;

                logger.Info("DoSyncronization: numberOfFeatures:{0} numberOfOrders:{1} maxCount:{2}", numberOfFeatures, numberOfOrders, maxCount);
                this.OnUpdateLogList("MaxCount: " + maxCount);

                #region Håvard

                #region Lars
                this.OnOrderProcessingStart(numberOfOrders * 100);
                int progressCounter = 0;
                #endregion
                int i = 0;
                while (lastChangeIndexSubscriber < lastChangeIndexProvider)
                {
                    //int i = 0;

                    #region Lars
                    this.OnOrderProcessingChange((progressCounter + 1) * 100 / 2);
                    //++progressCounter;
                    #endregion

                    // Do lots of stuff
                    int startIndex = lastChangeIndexSubscriber + 1;
                    int changeLogId = OrderChangelog(datasetId, startIndex);

                    if (!CheckStatusForChangelogOnProvider(datasetId, changeLogId)) return;

                    DownloadController downloadController;
                    var responseOk = GetChangelog(datasetId, changeLogId, out downloadController);

                    if (!responseOk)
                    {
                        //logger.Info("GetChangelog " + (i + 1) + " failed");
                        return;
                    }

                    List<string> fileList = new List<string>();

                    if (downloadController.isFolder)
                    {
                        string[] fileArray = Directory.GetFiles(downloadController.ChangelogFilename);
                        Array.Sort(fileArray);
                        fileList = fileArray.ToList();
                    }
                    else
                    {
                        fileList.Add(downloadController.ChangelogFilename);
                    }

                    foreach (string file in fileList)
                    {
                        string fileName = file;
                        #region Lars
                        #endregion

                        //
                        // Schema transformation
                        // Mapping from the nested structure of one or more simple features to the simple features for GeoServer.
                        //
                        var schemaTransform = new SchemaTransform();
                        var newFileName = schemaTransform.SchemaTransformSimplify(fileName, datasetId);

                        if (!string.IsNullOrEmpty(newFileName))
                        {
                            fileName = newFileName;
                        }


                        // load an XML document from a file
                        XElement changeLog = XElement.Load(fileName);

                        this.OnNewSynchMilestoneReached("DoWfsTransactions starting...");



                        var wfsController = new WfsController();
                        wfsController.ParentSynchController = this; // TODO: This is a little dirty, but we can reuse the events of the SynchController parent for UI feedback


                        // 20131102-Leg: Must use DoWfsTransactions, DoWfsTransactions2 is to slow on Updae/Delete
                        if (wfsController.DoWfsTransactions(changeLog, datasetId))
                        //if (wfsController.DoWfsTransactions2(changeLog, datasetId))
                        {
                            logger.Info("DoWfsTransactions OK, pass {0}", (i + 1));
                            this.OnUpdateLogList(String.Format("DoWfsTransactions OK, pass {0}", (i + 1)));

                            if (!downloadController.isFolder)
                            {
                                AcknowledgeChangelogDownloaded(datasetId, changeLogId);
                            }

                            // 20131102-Leg
                            var dataset = SubscriberDatasetManager.GetDataset(datasetId);
                            int endChangeId = (int)changeLog.Attribute("endIndex");
                            //
                            int numberReturned = (int)changeLog.Attribute("numberReturned");
                            int startChangeId = (int)changeLog.Attribute("startIndex");
                            //int endIndex = (int)changeLog.Attribute("endIndex"); 
                            //endChangeId = startChangeId + numberReturned-1;

                            dataset.LastIndex = endChangeId;
                            DL.SubscriberDatasetManager.UpdateDataset(dataset);
                        }
                        else
                        {
                            // TODO: What to do here? We must at least break?
                            return;

                        }

                        lastChangeIndexProvider = GetLastIndexFromProvider(datasetId);
                        lastChangeIndexSubscriber = GetLastIndexFromSubscriber(datasetId);
                        i++;

                        #region Lars
                        this.OnOrderProcessingChange((progressCounter + 1) * 100);
                        ++progressCounter;
                        #endregion
                    }
                }

                #endregion

                /*if (lastChangeIndexSubscriber < lastChangeIndexProvider)
                {
                    this.OnOrderProcessingStart(numberOfOrders);
                    for (int i = 0; i < numberOfOrders; i++)
                    {
                        this.OnOrderProcessingChange(i+1);
                        // 20130822-Leg: Fix for initial/first syncronization: Provider startIndex (GetLastIndex) starts at 1
                        int startIndex = lastChangeIndexSubscriber + 1;

                        if (i > 0)
                        {
                            startIndex = (i * maxCount) + lastChangeIndexSubscriber + 1;
                        }

                        this.OnUpdateLogList("Subscriber startIndex:" + startIndex.ToString());

                        int changeLogId = OrderChangelog(datasetId, startIndex);

                        this.OnUpdateLogList("ChangeLogId: "+changeLogId.ToString());

                        // Check status for changelog production at provider site
                        this.OnNewSynchMilestoneReached("Waiting for ChangeLog from Provider...");

                        if (!CheckStatusForChangelogOnProvider(datasetId, changeLogId)) return;

                        this.OnNewSynchMilestoneReached("GetChangelog "+  (i+1) +". Wait...");
                        DownloadController downloadController;
                        var responseOk = GetChangelog(datasetId, changeLogId, out downloadController);
                       
                        if (!responseOk)
                        {
                            logger.Info("GetChangelog " + (i + 1) + " failed");
                            return;
                        }                        

                        var message = "GetChangelog "+ (i+1) +" OK";
                        logger.Info(message);
                        this.OnNewSynchMilestoneReached(message);
                          
                        //
                        // Schema transformation
                        // Mapping from the nested structure of one or more simple features to the simple features for GeoServer.
                        //
                        var schemaTransform = new SchemaTransform();
                        var newFileName = schemaTransform.SchemaTransformSimplify(downloadController.ChangelogFilename, datasetId);
                   
                        if (!string.IsNullOrEmpty(newFileName))
                        {
                            downloadController.ChangelogFilename = newFileName;
                        }

                        // load an XML document from a file
                        XElement changeLog = XElement.Load(downloadController.ChangelogFilename);                       

#if (false)              
                        if (GetWfsInsert(changeLog) < 0)
                        {
                            return false;
                        }                   
#endif
                        var wfsController = new WfsController();

                        this.OnNewSynchMilestoneReached("DoWfsTransactions starting...");

                        if (wfsController.DoWfsTransactions2(changeLog, datasetId))
                        {
                            // sucsess - update subscriber lastChangeIndex
                            int lastIndexSubscriber = lastChangeIndexProvider;
                            if (numberOfOrders > 1 && i < (numberOfOrders - 1))
                            {
                                lastIndexSubscriber = (i * maxCount) + lastChangeIndexSubscriber;
                                logger.Info("DoWfsTransactions OK, pass {0}", (i + 1));

                                this.OnNewSynchMilestoneReached("DoWfsTransactions OK");                                
                                // this.OnUpdateLogList("DoWfsTransactions OK, pass " + (i + 1).ToString());
                            }
                            else
                            {
                                logger.Info("DoSynchronization success");
                            }

                            SubscriberDataset subscriberDataset;
                            
                            var dataset = DL.SubscriberDatasetManager.GetDataset(datasetId);
                            dataset.LastIndex = lastIndexSubscriber;
                            DL.SubscriberDatasetManager.UpdateDataset(dataset);

                            AcknowledgeChangelogDownloaded(datasetId, changeLogId);
                        }
                    }
                }*/

                // Stop timing
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);

                this.OnUpdateLogList("Syncronization Completed. Elapsed time: " + elapsedTime);
                logger.Info("Syncronization Completed. Elapsed time: {0}", elapsedTime);

                this.OnNewSynchMilestoneReached("Synch completed");
            }
            catch (WebException webEx)
            {
                throw new Exception(webEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Waiting until a specific changelog is available
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="changeLogId"></param>
        /// <returns></returns>
        private bool CheckStatusForChangelogOnProvider(int datasetId, int changeLogId)
        {
            try
            {
                var changeLogStatus = GetChangelogStatusResponse(datasetId, changeLogId);

                DateTime starttid = DateTime.Now;
                long elapsedTicks = DateTime.Now.Ticks - starttid.Ticks;

                var elapsedSpan = new TimeSpan(elapsedTicks);
                int timeout = 5;

                while ((changeLogStatus == ChangelogStatusType.started || changeLogStatus == ChangelogStatusType.working) &&
                       elapsedSpan.Minutes < timeout)
                {
                    System.Threading.Thread.Sleep(3000);
                    elapsedTicks = DateTime.Now.Ticks - starttid.Ticks;
                    elapsedSpan = new TimeSpan(elapsedTicks);
                    changeLogStatus = GetChangelogStatusResponse(datasetId, changeLogId);
                }
                if (changeLogStatus != ChangelogStatusType.finished)
                {
                    if (changeLogStatus == ChangelogStatusType.cancelled)
                    {
                        logger.Info("Cancelled by Server! Call provider.");
                        this.OnNewSynchMilestoneReached("Cancelled ChangeLog from Provider. Contact the proivider.");
                    }
                    else
                    {
                        logger.Info("Timeout");
                        this.OnNewSynchMilestoneReached("Timeout waiting for ChangeLog from Provider.");
                    }
                    return false;
                }
                this.OnNewSynchMilestoneReached("ChangeLog from Provider ready for download.");
            }
            catch (Exception ex)
            {
                logger.ErrorException(string.Format("Failed to get ChangeLog Status for changelog {0} from provider {1}", changeLogId, "TEST"), ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests the offline syncronization complete.
        /// </summary>
        /// <param name="zipFile">The zip file.</param>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <returns>True if OK.</returns>
        public bool TestOfflineSyncronizationComplete(string zipFile, int datasetId)
        {
            try
            {
                bool status = false;

                var dataset = SubscriberDatasetManager.GetDataset(datasetId);

                int lastChangeIndexSubscriber = (int)dataset.LastIndex;
                if (lastChangeIndexSubscriber > 0)
                {
                    logger.Info("TestOfflineSyncronizationComplete colud only be run if lastChangeIndexSubscriber = 0");
                    return false;
                }


                string outPath = Path.GetDirectoryName(zipFile);

                var downloadController = new DownloadController();
                downloadController.UnpackZipFile(zipFile, outPath);


                // Check if zip contains folder or file - Could be more than one file
                string baseFilename = zipFile.Replace(".zip", "");
                string xmlFile;
                bool isFolder = false;
                if (Directory.Exists(baseFilename))
                {
                    xmlFile = baseFilename;
                    isFolder = true;
                }
                else
                {
                    xmlFile = Path.ChangeExtension(zipFile, ".xml");
                    //ChangelogFilename = xmlFile;
                }

                List<string> fileList = new List<string>();

                if (isFolder)
                {
                    string[] fileArray = Directory.GetFiles(xmlFile);
                    Array.Sort(fileArray);
                    fileList = fileArray.ToList();
                }
                else
                {
                    fileList.Add(xmlFile);
                }

                foreach (string file in fileList)
                {
                    string fileName = file;


                    //string xmlFile = Path.ChangeExtension(zipFile, ".xml");
                    ////_downLoadedChangelogName = xmlFile;

                    //
                    // Schema transformation
                    // Mapping from the nested structure of one or more simple features to the simple features for GeoServer.
                    //
                    var schemaTransform = new SchemaTransform();
                    var newFileName = schemaTransform.SchemaTransformSimplify(fileName, datasetId);
                    if (!string.IsNullOrEmpty(newFileName))
                    {
                        fileName = newFileName;
                    }



                    // load an XML document from a file
                    XElement changeLog = XElement.Load(fileName);

                    // Build wfs-t transaction from changelog, and do the transaction    
                    var wfsController = new WfsController();
                    wfsController.ParentSynchController = this; // TODO: This is a little dirty, but we can reuse the events of the SynchController parent for UI feedback

                    if (wfsController.DoWfsTransactions(changeLog, datasetId))
                    {
                        status = true;

                        int numberMatched = (int)changeLog.Attribute("numberMatched");
                        int numberReturned = (int)changeLog.Attribute("numberReturned");
                        int startIndex = (int)changeLog.Attribute("startIndex");
                        int endIndex = (int)changeLog.Attribute("endIndex"); //now correct
                        int lastIndexSubscriber = startIndex + numberReturned; //endIndex - startIndex + 1;

                        dataset.LastIndex = endIndex; //lastIndexSubscriber;
                        DL.SubscriberDatasetManager.UpdateDataset(dataset);
                    }
                }


                return status;
            }

            catch (Exception ex)
            {
                logger.ErrorException("TestOfflineSyncronizationComplete:", ex);
                throw;
            }
        }
    }

    /// <summary>
    /// TransactionSummary helper class
    /// </summary>
    public class TransactionSummary
    {
        public int TotalInserted;
        public int TotalUpdated;
        public int TotalDeleted;
        public int TotalReplaced;
    }

}