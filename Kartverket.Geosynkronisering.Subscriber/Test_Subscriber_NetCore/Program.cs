using System;
using System.Collections.Generic;
using System.Linq;
using Kartverket.Geosynkronisering.Subscriber.BL;
using Kartverket.Geosynkronisering.Subscriber.DL;

namespace Test_Subscriber_NetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsConsole(args);
            Console.WriteLine();
        }

        private static bool InDocker { get { return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"; } }

        private static void RunAsConsole(string[] args)
        {
            var datasetIds = SubscriberDatasetManager.GetListOfDatasetIDs();

            if (args.Length < 1) { WriteHelp(); return; }

            switch (args[0].ToLower())
            {
                case Operations.auto:
                    SynchronizeDatasets(datasetIds);
                    break;
                case Operations.help:
                    if (args.Length > 1) WriteHelp(args[1]);
                    else WriteHelp();
                    break;
                case Operations.add:
                    if(args.Length < 4) WriteHelp(Operations.add);
                    else if (args.Length == 5) AddAllDatasets(args);
                    else AddSpecifiedDatasets(args);
                    break;
                case Operations.reset:
                    ResetDatasets(args);
                    break;
                case Operations.remove:
                    RemoveDatasets(args);
                    break;
                case Operations.list:
                    if (args.Length == 1) ListDatasets();
                    else if (args.Length == 4) ListProviderDatasets(args);
                    else WriteHelp(Operations.list);
                    break;
                case Operations.sync:
                    SynchronizeDatasets(args, datasetIds);
                    break;
                default:
                    WriteHelp();
                    break;
            }
        }



        private static void SynchronizeDatasets(string[] args, IList<int> datasetIds)
        {
            Console.WriteLine($"Syncronize datasets {string.Join(',', args)}?");

            if (!GetYorN()) return;

            foreach (var datasetId in GetDatasetIdsFromArgs(args))
            {
                if (datasetIds.Contains(datasetId)) Synchronize(datasetId);
                else Console.WriteLine("ERROR: DatasetId " + datasetId + " does not exist");
            };
        }

        private static void ListProviderDatasets(string[] args)
        {
            var capabilitiesDataBuilder = new CapabilitiesDataBuilder(args[1], args[2], args[3]);
            var datasets = capabilitiesDataBuilder.ProviderDatasetsList;
            Console.WriteLine($"Listing datasets from provider {args[1]}:");

            datasets.ForEach(d => {
                Console.WriteLine("-----------------------------------------------------------");
                Console.WriteLine(NormalizeText("ProviderDatasetId", d.ProviderDatasetId));
                Console.WriteLine(NormalizeText("TargetNameSpace", d.TargetNamespace));
            });
        }

        private static void ListDatasets()
        {
            Console.WriteLine("Listing local datasets:");

            var datasets = GeosyncDbEntities.ReadAll<Dataset>("Dataset");
            datasets.ForEach(d => {
                Console.WriteLine("-----------------------------------------------------------");
                Console.WriteLine(NormalizeText("DatasetId", d.DatasetId.ToString()));
                Console.WriteLine(NormalizeText("ProviderDatasetId",d.ProviderDatasetId));
                Console.WriteLine(NormalizeText("TargetNameSpace",d.TargetNamespace));
                Console.WriteLine(NormalizeText("SyncronizationUrl", d.SyncronizationUrl));
                Console.WriteLine(NormalizeText("ClientWfsUrl", d.ClientWfsUrl));
                Console.WriteLine(NormalizeText("AbortedChangelogPath", d.AbortedChangelogPath));
            });

        }

        private static string NormalizeText(string key, string value)
        {
            if (string.IsNullOrEmpty(value)) value = "";
            var linelength = 30;
            key = key.Trim() + ":";
            key = key.PadRight(linelength);
            return key + value.Trim();
        }

        private static void RemoveDatasets(string[] args)
        {
            var datasetIds = GetDatasetIdsFromArgs(args);

            Console.WriteLine($"Remove datasets {string.Join(',', datasetIds)}?");

            if (!GetYorN()) return;

            var datasets = GeosyncDbEntities.ReadAll<Dataset>("Dataset").Where(d => datasetIds.Contains(d.DatasetId));
            
            foreach (var dataset in datasets)
            {
                Console.WriteLine($"Removing dataset {dataset.DatasetId}");
                GeosyncDbEntities.DeleteDataset(dataset);
            }

        }

        private static void ResetDatasets(string[] args)
        {
            var datasetIds = GetDatasetIdsFromArgs(args);

            Console.WriteLine($"Reset datasets {string.Join(',', datasetIds)}?");

            if (!GetYorN()) return;

            var synchController = new SynchController();

            datasetIds.ForEach(d => synchController.ResetSubscriberLastIndex(d));

        }

        private static List<int> GetDatasetIdsFromArgs(string[] args)
        {
            return args.ToList().GetRange(1, args.Length - 1).Select(a => int.Parse(a)).ToList();
        }

        private static void AddSpecifiedDatasets(string[] args)
        {
            var capabilitiesDataBuilder = new CapabilitiesDataBuilder(args[1], args[2], args[3]);

            List<string> requiredDatasets = new List<string>();

            for (var i = 4; i < args.Length; i++) requiredDatasets.Add(args[i]);

            var datasets = capabilitiesDataBuilder.ProviderDatasetsList.Where(d => requiredDatasets.Contains(d.ProviderDatasetId));

            AddDatasets(args[1], args[2], args[3], datasets, args[4]);
        }

        private static void AddAllDatasets(string[] args)
        {
            var capabilitiesDataBuilder = new CapabilitiesDataBuilder(args[1], args[2], args[3]);

            AddDatasets(args[1], args[2], args[3], capabilitiesDataBuilder.ProviderDatasetsList, args[4]);
        }

        private static void WriteHelp()
        {
            Console.WriteLine($"Args: {string.Join('|', Operations.all)}");
        }

        private static void WriteHelp(string operation)
        {
            switch (operation)
            {
                case Operations.add:
                    Console.WriteLine($"Usage: {Operations.add} $serviceUrl $username $password $wfsUrl $datasetid");
                    break;
                case Operations.auto:
                    Console.WriteLine($"Usage: {Operations.auto}");
                    break;
                case Operations.reset:
                    Console.WriteLine($"Usage: {Operations.reset} $datasetId1 $datasetId2 ...");
                    break;
                case Operations.remove:
                    Console.WriteLine($"Usage: {Operations.remove} $datasetId1 $datasetId2 ...");
                    break;
                case Operations.list:
                    Console.WriteLine($"Usage: list || list $serviceUrl $username $password");
                    break;
                default:
                    WriteHelp();
                    break;

            }
        }

        private static void AddDatasets(string url, string user, string password, IEnumerable<Dataset> datasets, string clientWfsUrl)
        {
            foreach (var dataset in datasets)
            {
                dataset.SyncronizationUrl = url;
                dataset.ClientWfsUrl = clientWfsUrl;
                dataset.UserName = user;
                dataset.Password = password;
                GeosyncDbEntities.InsertDataset(dataset);
            }
        }

        private static void SynchronizeDatasets(IEnumerable<int> datasetIds)
        {
            Console.WriteLine("INFO: Fetching list of datasetIds");

            foreach (var datasetId in datasetIds) Synchronize(datasetId);
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

        static bool GetYorN()
        {
            ConsoleKey response; // Creates a variable to hold the user's response.

            do
            {
                while (Console.KeyAvailable) // Flushes the input queue.
                    Console.ReadKey();

                Console.Write("Y or N? "); // Asks the user to answer with 'Y' or 'N'.
                response = Console.ReadKey().Key; // Gets the user's response.
                Console.WriteLine(); // Breaks the line.
            } while (response != ConsoleKey.Y && response != ConsoleKey.N); // If the user did not respond with a 'Y' or an 'N', repeat the loop.

            /* 
             * Return true if the user responded with 'Y', otherwise false.
             * 
             * We know the response was either 'Y' or 'N', so we can assume 
             * the response is 'N' if it is not 'Y'.
             */
            return response == ConsoleKey.Y;
        }
    }
}
