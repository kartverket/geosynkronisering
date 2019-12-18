using System;
using System.Collections.Generic;
using Kartverket.Geosynkronisering.Subscriber.BL;
using Kartverket.Geosynkronisering.Subscriber.DL;

namespace Test_Subscriber_NetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing .NET Standard from .NET Core!");
            if (InDocker)
            {
                Console.WriteLine("We are in Docker, y'all!");
            }
            RunAsConsole(args);
        }

        private static bool InDocker { get { return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"; } }

        private static void RunAsConsole(string[] args)
        {
            var datasetIds = SubscriberDatasetManager.GetListOfDatasetIDs();
            if (args.Length == 1 && args[0].ToLower() == "auto")
            {
                SynchronizeDatasets(datasetIds);
            }
            else if (args.Length == 1 && args[0].ToLower() == "help")
            {
                Console.WriteLine("Args: auto | datasetId ...");
            }
            else
            {
                foreach (var datasetId in args)
                {
                    if (datasetIds.Contains(int.Parse(datasetId)))
                        Synchronize(int.Parse(datasetId));
                    else
                    {
                        Console.WriteLine("ERROR: DatasetId " + datasetId + " does not exist");
                    }
                }
            }
        }

        private static void SynchronizeDatasets(IEnumerable<int> datasetIds)
        {
            Console.WriteLine("INFO: Fetching list of datasetIds");
            foreach (var datasetId in datasetIds)
            {
                Synchronize(datasetId);
            }
        }

        private static void Synchronize(int datasetId)
        {
            var synchController = new SynchController();
            try
            {
                Console.Out.WriteLine("INFO: Starting synchronization of datasetId " + datasetId);
                synchController.InitTransactionsSummary();
                synchController.DoSynchronization(datasetId);
                WriteTransactionSummary(synchController);
                Console.Out.WriteLine("INFO: Finished synchronization of datasetId " + datasetId);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                //throw;
            }
        }

        private static void WriteTransactionSummary(SynchController synchController)
        {
            if (synchController.TransactionsSummary != null)
            {
                var logMessage = "INFO: Syncronization Transaction summary:";
                Console.WriteLine(logMessage);
                logMessage = "INFO: TotalInserted: " + synchController.TransactionsSummary.TotalInserted;
                Console.WriteLine(logMessage);
                logMessage = "INFO: TotalUpdated: " + synchController.TransactionsSummary.TotalUpdated;
                Console.WriteLine(logMessage);
                logMessage = "INFO: TotalDeleted: " + synchController.TransactionsSummary.TotalDeleted;
                Console.WriteLine(logMessage);
                logMessage = "INFO: TotalReplaced: " + synchController.TransactionsSummary.TotalReplaced;
                Console.WriteLine(logMessage);
            }
            else
            {
                Console.WriteLine("WARNING: No TransactionSummary available");
            }
        }
    }
}
