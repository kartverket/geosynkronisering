using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
        }

        public void InitTransactionsSummary()
        {
            TransactionsSummary = new TransactionSummary
            {
                TotalDeleted = 0,
                TotalInserted = 0,
                TotalReplaced = 0,
                TotalUpdated = 0
            };
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)

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
            var dataset = SubscriberDatasetManager.GetDataset(datasetId);
            dataset.LastIndex = 0;
            SubscriberDatasetManager.UpdateDataset(dataset);
        }

        /// <summary>
        /// Get OrderChangelog Response and changelogid
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public string OrderChangelog(int datasetId, long startIndex)
        {
            try
            {
                var dataset = SubscriberDatasetManager.GetDataset(datasetId);

                var client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SynchronizationUrl);
                var order = new ChangelogOrderType();
                order.datasetId = dataset.ProviderDatasetId;
                order.count = dataset.MaxCount.ToString();
                order.startIndex = startIndex.ToString();

                ChangelogIdentificationType resp = client.OrderChangelog(order);

                return resp.changelogId;
            }
            catch (Exception ex)
            {
                Logger.ErrorException("OrderChangelog failed:", ex);
                throw;
            }
        }


        /// <summary>
        /// Get ChangelogStatus Response
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="changelogId"></param>
        /// <returns></returns>
        public ChangelogStatusType GetChangelogStatusResponse(int datasetId, string changelogId)
        {
            try
            {
                var dataset = SubscriberDatasetManager.GetDataset(datasetId);

                var client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SynchronizationUrl);

                var id = new ChangelogIdentificationType {changelogId = changelogId};

                ChangelogStatusType resp = client.GetChangelogStatus(id);

                return resp;
            }
            catch (Exception ex)
            {
                Logger.ErrorException("GetChangelogStatusResponse failed:", ex);
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
        public bool GetChangelog(int datasetId, string changelogId, out DownloadController downloadController)
        {
            try
            {
                var dataset = SubscriberDatasetManager.GetDataset(datasetId);

                var client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SynchronizationUrl);

                var id = new ChangelogIdentificationType {changelogId = changelogId};

                ChangelogType resp = client.GetChangelog(id);

                string downloaduri = resp.downloadUri;

                // 20151215-Leg: Norkart downloaduri may contain .zip
                Logger.Info("GetChangelog downloaduri: " + downloaduri);
                string fileExtension = Path.GetExtension(downloaduri);
                if (fileExtension != String.Empty)
                {
                    Logger.Info("GetChangelog downloaduri  contains fileextension:" + fileExtension);
                    // Hack to remove eventual .zip from filename
                    downloaduri = downloaduri.Replace(Path.GetExtension(downloaduri), "");
                }


                string tempDir = Environment.GetEnvironmentVariable("TEMP");
#if (NOT_FTP)
                string fileName = tempDir + @"\" + changelogid + "_Changelog.xml";
#else
                const string ftpPath = "abonnent";
                Utils.Misc.CreateFolderIfMissing(tempDir + @"\" + ftpPath);
                // Create the abonnent folder if missing               

                string fileName = tempDir + @"\" + ftpPath + @"\" + Path.GetFileName(downloaduri) + ".zip";
#endif

                downloadController = new DownloadController {ChangelogFilename = fileName};
                downloadController.DownloadChangelog2(downloaduri);
            }
            catch (WebException webEx)
            {
                Logger.ErrorException("GetChangelog failed:", webEx);
                throw;
            }
            catch (Exception ex)
            {
                Logger.ErrorException("GetChangelog failed:", ex);
                throw;
            }
            return true;
        }


        /// <summary>
        /// Bekrefte at endringslogg er lastet ned
        /// </summary>
        /// <returns></returns>
        public bool AcknowledgeChangelogDownloaded(int datasetId, string changelogId)
        {
            try
            {
                var dataset = SubscriberDatasetManager.GetDataset(datasetId);

                var client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SynchronizationUrl);

                var id = new ChangelogIdentificationType {changelogId = changelogId};
                client.AcknowlegeChangelogDownloaded(id);

                return true;
            }
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Success)
                {
                    return false;
                }

                Logger.ErrorException("AcknowledgeChangelogDownloaded WebException:", webEx);
                throw;
            }
            catch (Exception ex)
            {
                Logger.ErrorException("AcknowledgeChangelogDownloaded failed:", ex);
                throw;
            }
        }


        /// <summary>
        /// Henter siste endringsnr fra tilbyder. Brukes for at klient enkelt kan sjekke om det er noe nytt siden siste synkronisering
        /// </summary>
        /// <returns></returns>
        public long GetLastIndexFromProvider(int datasetId)
        {
            try
            {
                var dataset = SubscriberDatasetManager.GetDataset(datasetId);

                var client = new WebFeatureServiceReplicationPortClient();
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SynchronizationUrl);

                var lastIndexString = client.GetLastIndex(dataset.ProviderDatasetId);
                long lastIndex = Convert.ToInt64(lastIndexString);

                return lastIndex;
            }

            catch (WebException webEx)
            {
                Logger.ErrorException("GetLastIndexFromProvider WebException:", webEx);
                throw;
            }
            catch (Exception ex)
            {
                Logger.ErrorException("GetLastIndexFromProvider failed:", ex);
                throw;
            }
        }

        public long GetLastIndexFromSubscriber(int datasetId)
        {
            var dataset = SubscriberDatasetManager.GetDataset(datasetId);

            if (dataset == null)
                return -1;

            return dataset.LastIndex;
        }

        /// <summary>
        /// Synchronizing of a given dataset
        /// </summary>
        /// <param name="datasetId"></param>
        public void DoSynchronization(int datasetId)
        {
            try
            {
                // Create new stopwatch
                var stopwatch = Stopwatch.StartNew();

                OnNewSynchMilestoneReached("GetLastIndexFromProvider");

                long lastChangeIndexProvider = GetLastIndexFromProvider(datasetId);

                var logMessage = "GetLastIndexFromProvider lastIndex: " + lastChangeIndexProvider;
                Logger.Info(logMessage);
                OnUpdateLogList(logMessage);
                OnNewSynchMilestoneReached("GetLastIndexFromProvider OK");

                OnNewSynchMilestoneReached("GetLastIndexFromSubscriber");
                long lastChangeIndexSubscriber = GetLastIndexFromSubscriber(datasetId);

                logMessage = "GetLastChangeIndexSubscriber lastIndex: " + lastChangeIndexSubscriber;
                Logger.Info(logMessage);
                OnUpdateLogList(logMessage);
                OnNewSynchMilestoneReached("GetLastIndexFromSubscriber OK");

                if (lastChangeIndexSubscriber >= lastChangeIndexProvider)
                {
                    logMessage = "Changelog has already been downloaded and handled:";
                    //this.OnUpdateLogList(logMessage); // Raise event to UI
                    //this.OnNewSynchMilestoneReached(logMessage);
                    logMessage += " Provider lastIndex:" + lastChangeIndexProvider + " Subscriber lastChangeIndex: " +
                                  lastChangeIndexSubscriber;
                    Logger.Info(logMessage);
                    OnUpdateLogList(logMessage); // Raise event to UI
                    OnNewSynchMilestoneReached(logMessage);
                    return;
                }

                OnNewSynchMilestoneReached("Order Changelog. Wait...");

                int maxCount = SubscriberDatasetManager.GetMaxCount(datasetId);

                long numberOfFeatures = lastChangeIndexProvider - lastChangeIndexSubscriber;
                long numberOfOrders = (numberOfFeatures/maxCount);

                if (lastChangeIndexProvider > int.MaxValue)
                {
                    // TODO: Fix for Norkart QMS Provider, TotalNumberOfOrders is not available here
                    // Assume Norkart QMS Provider, not a sequential number
                    Logger.Info(
                        "DoSyncronization: Probably QMS Provider,  Provider lastIndex is not sequential, just a transaction number!");
                    numberOfOrders = 10; // Just a guess
                }


                if (numberOfFeatures%maxCount > 0)
                    ++numberOfOrders;

                Logger.Info("DoSyncronization: numberOfFeatures:{0} numberOfOrders:{1} maxCount:{2}", numberOfFeatures,
                    numberOfOrders, maxCount);
                OnUpdateLogList("MaxCount: " + maxCount);

                #region Håvard

                #region Lars

                OnOrderProcessingStart(numberOfOrders*100);
                int progressCounter = 0;

                #endregion

                int i = 0;
                while (lastChangeIndexSubscriber < lastChangeIndexProvider)
                {
                    #region Lars

                    OnOrderProcessingChange((progressCounter + 1)*100/2);

                    #endregion

                    // Do lots of stuff
                    long startIndex = lastChangeIndexSubscriber + 1;
                    string changeLogId = OrderChangelog(datasetId, startIndex);

                    if (!CheckStatusForChangelogOnProvider(datasetId, changeLogId)) return;

                    DownloadController downloadController;
                    var responseOk = GetChangelog(datasetId, changeLogId, out downloadController);

                    if (!responseOk)
                    {
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

                    SubscriberDataset dataset = SubscriberDatasetManager.GetDataset(datasetId);

                    //Rewrite files according to mappingfile if given
                    if (!String.IsNullOrEmpty(dataset.MappingFile))
                    {
                        fileList = changeLogMapper(fileList, datasetId);
                    }

                    XElement changeLog;
                    if (fileList.Count > 1 && lastChangeIndexSubscriber > 0)
                    {
                        changeLog = mergeChangelogs(fileList);
                        PerformWfsTransaction(changeLog, datasetId, dataset, 1);
                    }
                    else
                        foreach (string fileName in fileList)
                        {
                            changeLog = XElement.Load(fileName);
                            if (!PerformWfsTransaction(changeLog, datasetId, dataset, i + 1))
                                throw new Exception("WfsTransaction failed");

                            if (!downloadController.isFolder)
                            {
                                AcknowledgeChangelogDownloaded(datasetId, changeLogId);
                            }

                            OnOrderProcessingChange((progressCounter + 1)*100);
                            ++progressCounter;
                            i++;
                        }

                    lastChangeIndexProvider = GetLastIndexFromProvider(datasetId);
                    lastChangeIndexSubscriber = GetLastIndexFromSubscriber(datasetId);
                }

                #endregion

                // Stop timing
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);

                OnUpdateLogList("Syncronization Completed. Elapsed time: " + elapsedTime);
                Logger.Info("Syncronization Completed. Elapsed time: {0}", elapsedTime);

                // To set the progressbar to complete / finished
                OnOrderProcessingChange(int.MaxValue);

                OnNewSynchMilestoneReached("Synch completed");
            }
            catch (WebException webEx)
            {
                Logger.ErrorException("DoSynchronization WebException:", webEx);
                throw new Exception(webEx.Message);
            }
            catch (Exception ex)
            {
                Logger.ErrorException("DoSynchronization Exception:", ex);
                OnUpdateLogList(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Waiting until a specific changelog is available
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="changeLogId"></param>
        /// <returns></returns>
        private bool CheckStatusForChangelogOnProvider(int datasetId, string changeLogId)
        {
            try
            {
                var changeLogStatus = GetChangelogStatusResponse(datasetId, changeLogId);

                DateTime starttid = DateTime.Now;
                long elapsedTicks = DateTime.Now.Ticks - starttid.Ticks;

                var elapsedSpan = new TimeSpan(elapsedTicks);
                int timeout = 15;
                //timeout = 50; // TODO: Fix for Norkart Provider,

                while ((changeLogStatus == ChangelogStatusType.queued || changeLogStatus == ChangelogStatusType.working) &&
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
                        Logger.Info("Cancelled by Server! Call provider.");
                        OnNewSynchMilestoneReached("Cancelled ChangeLog from Provider. Contact the proivider.");
                    }
                    else if (changeLogStatus == ChangelogStatusType.failed)
                    {
                        Logger.Info("ChangelogStatusType.failed waiting for ChangeLog from Provider");
                        OnNewSynchMilestoneReached(
                            "Failed waiting for ChangeLog from Provider. Contact the proivider.");
                    }
                    else
                    {
                        Logger.Info("Timeout");
                        OnNewSynchMilestoneReached("Timeout waiting for ChangeLog from Provider.");
                    }
                    return false;
                }
                OnNewSynchMilestoneReached("ChangeLog from Provider ready for download.");
            }
            catch (Exception ex)
            {
                Logger.ErrorException(
                    string.Format("Failed to get ChangeLog Status for changelog {0} from provider {1}", changeLogId,
                        "TEST"), ex);
                return false;
            }
            return true;
        }

        private List<string> changeLogMapper(List<string> fileList, int datasetId)
        {
            List<string> newFileList = new List<string>();
            foreach (string file in fileList)
            {
                string fileName = file;

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
                newFileList.Add(fileName);
            }
            return newFileList;
        }

        private XElement mergeChangelogs(List<string> fileList)
        {
            string transactionsElementPath =
                "{http://skjema.geonorge.no/standard/geosynkronisering/1.1/endringslogg}transactions";
            bool firstFile = true;
            XDocument mergedChangelog = new XDocument();
            XElement originalChangelog;

            foreach (string fileName in fileList)
            {
                if (firstFile)
                {
                    mergedChangelog.AddFirst(XElement.Load(fileName));
                    firstFile = false;
                }
                else
                {
                    originalChangelog = XElement.Load(fileName);
                    var originalTransactions = originalChangelog.Element(transactionsElementPath);
                    if (originalTransactions != null)
                        foreach (XElement wfsOperation in originalTransactions.Elements())
                            if (mergedChangelog.Root != null)
                            {
                                var mergedTransactions = mergedChangelog.Root.Element(transactionsElementPath);
                                if (mergedTransactions != null)
                                    mergedTransactions.LastNode.AddAfterSelf(wfsOperation);
                            }
                }
            }
            return mergedChangelog.Root;
        }

        private bool PerformWfsTransaction(XElement changeLog, int datasetId, SubscriberDataset dataset, int passNr)
        {
            try
            {
                bool status = false;

                long endIndex = (long) changeLog.Attribute("endIndex"); //now correct

                // Build wfs-t transaction from changelog, and do the transaction   
                var wfsController = new WfsController();

                // TODO: This is a little dirty, but we can reuse the events of the SynchController parent for UI feedback
                wfsController.ParentSynchController = this;

                OnNewSynchMilestoneReached("DoWfsTransactions starting...");

                if (wfsController.DoWfsTransactions(changeLog, datasetId))
                {
                    Logger.Info("DoWfsTransactions OK, pass {0}", passNr);
                    OnUpdateLogList(String.Format("DoWfsTransactions OK, pass {0}", passNr));
                    status = true;
                    dataset.LastIndex = endIndex;
                    SubscriberDatasetManager.UpdateDataset(dataset);
                }
                return status;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

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

                int lastChangeIndexSubscriber = (int) dataset.LastIndex;

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

                XElement changeLog;
                int passNr = 1;
                if (fileList.Count > 1 && lastChangeIndexSubscriber > 0)
                {
                    changeLog = mergeChangelogs(fileList);
                    status = PerformWfsTransaction(changeLog, datasetId, dataset, passNr);
                }
                else
                    foreach (string fileName in fileList)
                    {
                        changeLog = XElement.Load(fileName);
                        status = PerformWfsTransaction(changeLog, datasetId, dataset, passNr);
                        passNr += 1;
                    }


                return status;
            }

            catch (Exception ex)
            {
                Logger.ErrorException("TestOfflineSyncronizationComplete:", ex);
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