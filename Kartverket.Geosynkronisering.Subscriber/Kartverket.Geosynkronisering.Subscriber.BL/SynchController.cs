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

        public IBindingList GetCapabilitiesProviderDataset(string url, string UserName, string Password)
        {
            var cdb = new CapabilitiesDataBuilder(url, UserName, Password);
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
            ResetDataset(dataset, 0);
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

                var client = buildClient(dataset);

                var order = new ChangelogOrderType();
                order.datasetId = dataset.ProviderDatasetId;
                order.count = dataset.MaxCount.ToString();
                order.startIndex = startIndex.ToString();

                var resp = client.OrderChangelog(order);

                dataset.AbortedChangelogId = resp.changelogId;
                SubscriberDatasetManager.UpdateDataset(dataset);
                return dataset.AbortedChangelogId;
            }
            catch (Exception ex)
            {
                Logger.ErrorException("OrderChangelog failed:", ex);
                throw;
            }
        }

        private WebFeatureServiceReplicationPortClient buildClient(SubscriberDataset dataset)
        {
            var client = new WebFeatureServiceReplicationPortClient();
            client.ClientCredentials.UserName.UserName = dataset.UserName;
            client.ClientCredentials.UserName.Password = dataset.Password;
            client.Endpoint.Address = new System.ServiceModel.EndpointAddress(dataset.SynchronizationUrl);
            return client;
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

                var client = buildClient(dataset);

                var id = new ChangelogIdentificationType {changelogId = changelogId};

                var resp = client.GetChangelogStatus(id);

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

                var client = buildClient(dataset);

                var id = new ChangelogIdentificationType {changelogId = changelogId};

                var resp = client.GetChangelog(id);

                var downloaduri = resp.downloadUri;

                // 20151215-Leg: Norkart downloaduri may contain .zip
                Logger.Info("GetChangelog downloaduri: " + downloaduri);

                var changelogDir = string.IsNullOrEmpty(dataset.ChangelogDirectory)
                    ? Environment.GetEnvironmentVariable("TEMP")
                    : dataset.ChangelogDirectory;
#if (NOT_FTP)
                string fileName = changelogDir + @"\" + changelogid + "_Changelog.xml";
#else
                CreateFolderIfMissing(changelogDir);
                const string ftpPath = "abonnent";
                CreateFolderIfMissing(changelogDir + @"\" + ftpPath);
                // Create the abonnent folder if missing               

                var fileName = changelogDir + @"\" + ftpPath + @"\" + Path.GetFileName(downloaduri);
#endif

                downloadController = new DownloadController {ChangelogFilename = fileName};
                downloadController.DownloadChangelog(downloaduri, dataset);
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

                var client = buildClient(dataset);

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

                var client = buildClient(dataset);

                var lastIndexString = client.GetLastIndex(dataset.ProviderDatasetId);
                var lastIndex = Convert.ToInt64(lastIndexString);

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
                var dataset = SubscriberDatasetManager.GetDataset(datasetId);
                var stopwatch = Stopwatch.StartNew();

                var lastIndexProvider = performSynchronization(dataset, datasetId);

                while (dataset.LastIndex <= lastIndexProvider)
                    lastIndexProvider = performSynchronization(dataset, datasetId);

                stopwatch.Stop();
                var ts = stopwatch.Elapsed;
                var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);

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

        private long performSynchronization(SubscriberDataset dataset, int datasetId)
        {

            //Check if previous transactions failed
            if (dataset.AbortedEndIndex != null)
            {
                var transactionStart = dataset.AbortedTransaction ?? 0;
                DoSyncronizationOffline(dataset.AbortedChangelogPath, datasetId, transactionStart);
                return -1;
            }

            const int progressCounter = 0;
            string changeLogId;

            long lastIndexProvider = 0;

            if (string.IsNullOrEmpty(dataset.AbortedChangelogId))
            {
                // Do lots of stuff
                var startIndex = dataset.LastIndex + 1;

                lastIndexProvider = PrepareForOrder(datasetId);

                if (lastIndexProvider == 0) return -1;

                OnOrderProcessingChange((progressCounter + 1)*100/2);

                changeLogId = OrderChangelog(datasetId, startIndex);
            }
            else
            {
                changeLogId = dataset.AbortedChangelogId;
                OnNewSynchMilestoneReached("Found changelogId " + dataset.AbortedChangelogId +
                                           ". Querying for status.");
            }

            GetStatusForChangelogOnProvider(datasetId, changeLogId);

            DownloadController downloadController;
            var responseOk = GetChangelog(datasetId, changeLogId, out downloadController);

            if (!responseOk)
            {
                return -1;
            }

            var fileList = GetChangelogFiles(downloadController.IsFolder,
                downloadController.ChangelogFilename);


            //Rewrite files according to mappingfile if given
            if (!string.IsNullOrEmpty(dataset.MappingFile))
            {
                fileList = ChangeLogMapper(fileList, datasetId);
            }

            LoopChangeLog(fileList, dataset, datasetId, progressCounter, downloadController.ChangelogFilename, 0);

            AcknowledgeChangelogDownloaded(datasetId, changeLogId);

            return lastIndexProvider;
        }

        private static List<string> GetChangelogFiles(bool isFolder, string changelogPath)
        {
            var fileList = new List<string>();

            if (isFolder)
            {
                var fileArray = Directory.GetFiles(changelogPath);
                if (fileArray[0].Contains('_'))
                {
                    var comparison = new Comparison<string>(delegate(string a, string b)
                    {
                        return ExtractNumber(a).CompareTo(ExtractNumber(b));
                    });

                    Array.Sort(fileArray, comparison);

                }
                return fileArray.ToList();
            }

            fileList.Add(changelogPath);
            return fileList;
        }

        private static int ExtractNumber(string a)
        {
            var aFileArray = a.Split('\\');
            var aFile = aFileArray[aFileArray.Length - 1];
            return int.Parse(aFile.Substring(0, aFile.IndexOf('_')));
        }

        private long PrepareForOrder(int datasetId)
        {
            OnNewSynchMilestoneReached("GetLastIndexFromProvider");

            var lastChangeIndexProvider = GetLastIndexFromProvider(datasetId);

            var logMessage = "GetLastIndexFromProvider lastIndex: " + lastChangeIndexProvider;
            Logger.Info(logMessage);
            OnUpdateLogList(logMessage);
            OnNewSynchMilestoneReached("GetLastIndexFromProvider OK");

            OnNewSynchMilestoneReached("GetLastIndexFromSubscriber");
            var lastChangeIndexSubscriber = GetLastIndexFromSubscriber(datasetId);

            logMessage = "GetLastChangeIndexSubscriber lastIndex: " + lastChangeIndexSubscriber;
            Logger.Info(logMessage);
            OnUpdateLogList(logMessage);
            OnNewSynchMilestoneReached("GetLastIndexFromSubscriber OK");

            if (lastChangeIndexSubscriber >= lastChangeIndexProvider)
            {
                logMessage = "Changelog has already been downloaded and handled:";
                logMessage += " Provider lastIndex:" + lastChangeIndexProvider + " Subscriber lastChangeIndex: " +
                              lastChangeIndexSubscriber;
                Logger.Info(logMessage);
                OnUpdateLogList(logMessage); // Raise event to UI
                OnNewSynchMilestoneReached(logMessage);
                return 0;
            }

            OnNewSynchMilestoneReached("Order Changelog. Wait...");

            return lastChangeIndexProvider;
        }

        private bool LoopChangeLog(IEnumerable<string> fileList, SubscriberDataset dataset, int datasetId,
            int progressCounter,
            string changelogFilename, long transactionStart)
        {
            var i = 0;

            var status = false;

            XElement changeLog = null;

            foreach (var fileName in fileList)
            {
                if (transactionStart > i)
                {
                    i++;
                    continue;
                }

                changeLog = XElement.Load(fileName);

                dataset.AbortedEndIndex = GetAbortedEndIndex(changeLog);
                dataset.AbortedTransaction = i;
                if(File.Exists(changelogFilename))
                    dataset.AbortedChangelogPath = changelogFilename;
                else if(File.Exists(changelogFilename + ".zip"))
                    dataset.AbortedChangelogPath = changelogFilename + ".zip";

                SubscriberDatasetManager.UpdateDataset(dataset);
                status = DoWfsTransaction(changeLog, datasetId, i + 1);
                if (!status)
                    throw new Exception("WfsTransaction failed");

                OnOrderProcessingChange((progressCounter + 1)*100);
                ++progressCounter;
                i++;
            }

            if (changeLog != null)
            {
                ResetDataset(dataset, (long) changeLog.Attribute("endIndex"));
            }

            return status;
        }

        private static void ResetDataset(SubscriberDataset dataset, long endIndex)
        {
            if (endIndex > 0)
                dataset.LastIndex = endIndex;
            dataset.AbortedEndIndex = null;
            dataset.AbortedTransaction = null;
            dataset.AbortedChangelogId = null;
            if (!string.IsNullOrEmpty(dataset.AbortedChangelogPath))
            {
                if (File.Exists(dataset.AbortedChangelogPath + ".zip"))
                    File.Delete(dataset.AbortedChangelogPath + ".zip");
                else
                    File.Delete(dataset.AbortedChangelogPath);
                Directory.Delete(dataset.AbortedChangelogPath, true);
                dataset.AbortedChangelogPath = null;
            }
            SubscriberDatasetManager.UpdateDataset(dataset);
        }

        /// <summary>
        /// Waiting until a specific changelog is available
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="changeLogId"></param>
        /// <returns></returns>
        private void GetStatusForChangelogOnProvider(int datasetId, string changeLogId)
        {
            try
            {
                var changeLogStatus = GetChangelogStatusResponse(datasetId, changeLogId);

                var starttid = DateTime.Now;
                var elapsedTicks = DateTime.Now.Ticks - starttid.Ticks;

                var elapsedSpan = new TimeSpan(elapsedTicks);
                var timeout = 15;
                //timeout = 50; // TODO: Fix for Norkart Provider,

                while ((changeLogStatus == ChangelogStatusType.queued || changeLogStatus == ChangelogStatusType.working) &&
                       elapsedSpan.Minutes < timeout)
                {
                    System.Threading.Thread.Sleep(3000);
                    elapsedTicks = DateTime.Now.Ticks - starttid.Ticks;
                    elapsedSpan = new TimeSpan(elapsedTicks);
                    changeLogStatus = GetChangelogStatusResponse(datasetId, changeLogId);
                }
                if (changeLogStatus == ChangelogStatusType.finished)
                    OnNewSynchMilestoneReached("ChangeLog from Provider ready for download.");
                else
                {
                    switch (changeLogStatus)
                    {
                        case (ChangelogStatusType.cancelled):
                            Logger.Info("Cancelled by Server! Call provider.");
                            ResetDataset(SubscriberDatasetManager.GetDataset(datasetId), -1);
                            throw new IOException("Recieved ChangelogStatus == cancelled from provider");
                        case (ChangelogStatusType.failed):
                            Logger.Info("ChangelogStatusType.failed waiting for ChangeLog from Provider");
                            ResetDataset(SubscriberDatasetManager.GetDataset(datasetId), -1);
                            throw new IOException("Recieved ChangelogStatus == failed from provider");
                        default:
                            Logger.Info("Timeout");
                            throw new IOException("Timed out waiting for ChangelogStatus == finished from provider");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException(
                    string.Format("Failed to get ChangeLog Status for changelog {0} from provider {1}", changeLogId,
                        "TEST"), ex);
                throw new IOException(ex.Message);
            }
        }

        private static List<string> ChangeLogMapper(IEnumerable<string> fileList, int datasetId)
        {
            var newFileList = new List<string>();
            foreach (var file in fileList)
            {
                var fileName = file;

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

        private bool DoWfsTransaction(XElement changeLog, int datasetId, int passNr)
        {
            try
            {
                // Build wfs-t transaction from changelog, and do the transaction   
                var wfsController = new WfsController();

                // TODO: This is a little dirty, but we can reuse the events of the SynchController parent for UI feedback
                wfsController.ParentSynchController = this;

                OnNewSynchMilestoneReached("DoWfsTransactions starting...");

                if (!wfsController.DoWfsTransactions(changeLog, datasetId)) return false;
                Logger.Info("DoWfsTransactions OK, pass {0}", passNr);
                OnUpdateLogList(string.Format("DoWfsTransactions OK, pass {0}", passNr));
                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        private static long GetAbortedEndIndex(XElement changeLog)
        {
            return Convert.ToInt64(changeLog.Attribute("endIndex").Value);
        }

        /// <summary>
        /// Tests the offline syncronization complete.
        /// </summary>
        /// <param name="zipFile">The zip file.</param>
        /// <param name="datasetId">The dataset identifier.</param>
        /// <param name="transactionStart"></param>
        /// <returns>True if OK.</returns>
        public bool DoSyncronizationOffline(string zipFile, int datasetId, long transactionStart)
        {
            try
            {
                var dataset = SubscriberDatasetManager.GetDataset(datasetId);

                var downloadController = new DownloadController();
                downloadController.UnpackZipFile(zipFile);


                // Check if zip contains folder or file - Could be more than one file
                var baseFilename = zipFile.Replace(".zip", "");
                string xmlFile;
                var isFolder = false;
                if (Directory.Exists(baseFilename))
                {
                    xmlFile = baseFilename;
                    isFolder = true;
                }
                else
                {
                    xmlFile = Path.ChangeExtension(zipFile, ".xml");
                }

                var fileList = GetChangelogFiles(isFolder, xmlFile);

                return LoopChangeLog(fileList, dataset, datasetId, 0, zipFile, transactionStart);
            }

            catch (Exception ex)
            {
                Logger.ErrorException("TestOfflineSyncronizationComplete:", ex);
                throw;
            }

        }


        public static bool CreateFolderIfMissing(string path)
        {
            try
            {
                bool folderExists = Directory.Exists((path));
                if (!folderExists)
                    Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception ex)
            {
                return false;
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