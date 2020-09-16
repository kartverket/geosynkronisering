using System;
using System.Collections.Generic;
using Kartverket.Geosynkronisering.Subscriber.BL;
using Kartverket.Geosynkronisering.Subscriber.DL;

namespace Kartverket.Geosynkronisering.Subscriber
{
    /// <summary>
    /// Public methods added for WCF in Sentral Lagring (used by NOIS)
    /// </summary>
    public static partial class Program
    {
        #region WCF SubscriberService

        public static bool IsSubscriberActive()
        {
            return true;
        }

        public static string ResetSubscriberIndex(int datasetId)
        {
            var synchController = new SynchController();
            try
            {
                synchController.ResetSubscriberLastIndex(datasetId);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(
                    " Exception ResetSubscriberIndex datasetId: " + datasetId + ", Desc: " + e.Message);
                return "ResetSubscriberIndex failed for dataset: " + datasetId;
                //throw;
            }

            return "ResetSubscriberIndex succeded for dataset: " + datasetId;
        }

        public static IList<int> GetSupportedDatasets()
        {
            return SubscriberDatasetManager.GetListOfDatasetIDs();
        }

        public static Dataset GetDataset(int datasetId)
        {
            return SubscriberDatasetManager.GetDataset(datasetId);
        }

        public static List<Dataset> GetSupportedDatasetsAll()
        {
            // TODO: Check that username and password is NOT returned
            return SubscriberDatasetManager.GetAllDataset();
        }

        public static string GetLastIndex(int datasetId)
        {
            return SubscriberDatasetManager.GetLastIndex(datasetId);
        }

        public static string SynchronizeSingle(int datasetId)
        {
            var synchController = new SynchController();
            try
            {
                Console.Out.WriteLine(" Start SynchronizeSingle datasetId: " + datasetId);
                synchController.InitTransactionsSummary();
                synchController.DoSynchronization(datasetId);
                WriteTransactionSummary(synchController);
                var sTransactionSummary = GetTransactionSummary(synchController);
                Console.Out.WriteLine(" Finished SynchronizeSingle of datasetId " + datasetId);
                return sTransactionSummary;
            }
            catch (Exception e)
            {
                var sErr = "Exception SynchronizeSingle datasetId: " + datasetId + ", Error desc: " + e.Message;
                Console.Out.WriteLine(sErr);
                return sErr;
                //throw;
            }
        }

        public static bool IsProviderActive(string url, string userName, string password)
        {
            var synchController = new SynchController();
            try
            {
                Console.Out.WriteLine(" Start IsProviderActive: " + url);
                var sTransactionSummary = GetTransactionSummary(synchController);
                // 
                var responseDistributor = synchController.GetCapabilitiesProviderDataset(url, userName, password);
                Console.Out.WriteLine(" Finished IsProviderActive: " + url);
                return responseDistributor.Count > 0;
            }
            catch (Exception e)
            {
                var sErr = "Exception IsProviderActive: " + url + ", Error desc: " + e.Message;
                Console.Out.WriteLine(sErr);
                return false;
                //throw;
            }
        }

        private static string GetTransactionSummary(SynchController synchController)
        {
            var logMessage = "WARNING: No TransactionSummary available";
            if (synchController.TransactionsSummary != null)
            {
                logMessage = "Summary: Total inserted: " + synchController.TransactionsSummary.TotalInserted +
                             ", total updated: " + synchController.TransactionsSummary.TotalUpdated +
                             ", totalDeleted: " +
                             synchController.TransactionsSummary.TotalDeleted + " and total replaced: " +
                             synchController.TransactionsSummary.TotalReplaced;
            }

            return logMessage;
        }

        #endregion         // End - Public methods added for WCF in Sentral Lagring
    }
}