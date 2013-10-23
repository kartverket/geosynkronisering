using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class SynchController
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // NLog for logging (nuget package)
       
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

                var id = new ChangelogIdentificationType {changelogId = changelogId.ToString()};
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
        /// 
        /// </summary>
        /// <param name="datasetId"></param>
        /// <returns></returns>
        public bool DoSynchronization(int datasetId)
        {           
            try
            {                
                logger.Info("DoSynchronization start");

                var lastChangeIndexProvider = GetLastIndexFromProvider(datasetId);
                logger.Info("GetLastIndexFromProvider lastIndex :{0}{1}", "\t", lastChangeIndexProvider);

                int lastChangeIndexSubscriber = GetLastIndexFromSubscriber(datasetId);
                logger.Info("GetLastChangeIndexSubscriber lastIndex :{0}{1}", "\t", lastChangeIndexSubscriber);

                if (lastChangeIndexSubscriber >= lastChangeIndexProvider)
                {
                    string message = "Changelog has already been downloaded and handled:";
                    logger.Info(message + " Provider lastIndex:{0} Subscriber lastChangeIndex:{1}", lastChangeIndexProvider, lastChangeIndexSubscriber);
                    return false;
                }

                int maxCount = DL.SubscriberDatasetManager.GetMaxCount(datasetId);

                int numberOfFeatures = lastChangeIndexProvider - lastChangeIndexSubscriber;
                int numberOfOrders = (numberOfFeatures / maxCount);
                if (numberOfFeatures % maxCount > 0)
                    ++numberOfOrders;

                logger.Info("TestSyncronizationComplete: numberOfFeatures:{0} numberOfOrders:{1} maxCount:{2}", numberOfFeatures, numberOfOrders, maxCount);

                if (lastChangeIndexSubscriber < lastChangeIndexProvider)
                {
                    for (int i = 0; i < numberOfOrders; i++)
                    {

                        // 20130822-Leg: Fix for initial/first syncronization: Provider startIndex (GetLastIndex) starts at 1
                        int startIndex = lastChangeIndexSubscriber + 1;

                        if (i > 0)
                        {
                            startIndex = (i * maxCount) + lastChangeIndexSubscriber + 1;
                        }

                        System.Diagnostics.Debug.WriteLine("startIndex:" + startIndex.ToString() + " lastChangeIndex:" +
                                                           lastChangeIndexSubscriber.ToString());
                       
                        int changeLogId = OrderChangelog(datasetId, startIndex);

                        // Check status for changelog production at provider site
                        if (!CheckStatusForChangelogOnProvider(datasetId, changeLogId)) return false;                      

                        DownloadController downloadController;
                        GetChangelog(datasetId, changeLogId, out downloadController);
                        logger.Info("GetChangelog for dataset {0} OK", datasetId);   
                          
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

                        if (wfsController.DoWfsTransactions(changeLog, datasetId))
                        {
                            // sucsess - update subscriber lastChangeIndex
                            int lastIndexSubscriber = lastChangeIndexProvider;
                            if (numberOfOrders > 1 && i < (numberOfOrders - 1))
                            {
                                lastIndexSubscriber = (i * maxCount) + lastChangeIndexSubscriber;
                                logger.Info("DoWfsTransactions OK, pass {0}", (i + 1));
                            }
                            else
                            {
                                logger.Info("DoSynchronization sucsess");
                            }

                            SubscriberDataset subscriberDataset;
                            
                            var dataset = DL.SubscriberDatasetManager.GetDataset(datasetId);
                            dataset.LastIndex = lastIndexSubscriber;
                            DL.SubscriberDatasetManager.UpdateDataset(dataset);

                            AcknowledgeChangelogDownloaded(datasetId, changeLogId);
                        }
                    }
                }             
                return true;
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
                    }
                    else
                    {
                        logger.Info("Timeout");               
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException(string.Format("Failed to get ChangeLog Status for changelog {0} from provider {1}", changeLogId, "TEST"),ex);
                return false;
            }
            return true;
        }

        public bool TestOfflineSyncronizationComplete(string zipFile, int datasetId)
        {
            //TODO: TestOfflineSyncronizationComplete NOT finished
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

                // TODO: Could be more than one file
                string xmlFile = Path.ChangeExtension(zipFile, ".xml");
                //_downLoadedChangelogName = xmlFile;

                //
                // Schema transformation
                // Mapping from the nested structure of one or more simple features to the simple features for GeoServer.
                //
                string fileName = xmlFile; // txbDownloadedFile.Text;

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

                if (wfsController.DoWfsTransactions(changeLog, datasetId))
                {
                    status = true;

                    int numberMatched = (int)changeLog.Attribute("numberMatched");
                    int numberReturned = (int)changeLog.Attribute("numberReturned");
                    int startIndex = (int)changeLog.Attribute("startIndex");
                    int endIndex = (int)changeLog.Attribute("endIndex"); //NOT correct, always the latest!
                    int lastIndexSubscriber = startIndex + numberReturned; //endIndex - startIndex + 1;

                    dataset.LastIndex = lastIndexSubscriber;
                    DL.SubscriberDatasetManager.UpdateDataset(dataset);
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
}
