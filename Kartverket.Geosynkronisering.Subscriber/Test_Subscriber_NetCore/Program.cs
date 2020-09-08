using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kartverket.Geosynkronisering.Subscriber.BL;
using Kartverket.Geosynkronisering.Subscriber.DL;
using NLog;

namespace Test_Subscriber_NetCore
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            // Support nlog on .net core
            logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            logger.Info("Starting Geosync Subscriber_NetCore");

            try
            {
                //throw new NotImplementedException(); // Test that nlog works

                Console.WriteLine();

                RunAsConsole(args);
            }
            catch (Exception e)
            {
                logger.Error(e);
                Console.WriteLine(e);
                throw;
            }
        }

        private static bool InDocker { get { return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"; } }

        private static void RunAsConsole(string[] args)
        {
            if (args.Length < 1) { WriteHelp(); return; }

            _ = new GeosyncDbEntities();

            switch (args[0].ToLower())
            {
                case Operations.auto:
                    SynchronizeDatasets();
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
                    SynchronizeDatasets(args);
                    break;
                case Operations.set:
                    if (args.Length == 1) Console.WriteLine($"FieldNames: {string.Join(',',GetDatasetProperties().ToList().Select(p => p.Name))}");
                    else SetFields(args);
                    break;
                default:
                    WriteHelp();
                    break;
            }
        }

        private static void SetFields(string[] args)
        {
            GetDatasetsFromArgs(args).ForEach(d => { 
                GeosyncDbEntities.UpdateDataset(SetFieldValues(args, d));
            });            
        }

        private static Dataset SetFieldValues(string[] args, Dataset dataset)
        {
            var list = args.ToList();

            foreach (var property in GetDatasetProperties())
            {
                var key = list.FirstOrDefault(a => a.ToLower() == property.Name.ToLower());

                if (key == null) continue;
                
                var valueIndex = list.IndexOf(key) + 1;

                var value = list[valueIndex];

                property.SetValue(dataset, value);
            }

            return dataset;
        }

        private static PropertyInfo[] GetDatasetProperties()
        {
            return new Dataset().GetType().GetProperties();
        }

        private static bool SkipPrompt(string[] args)
        {
            return args == null || args.ToList().Any(a => a.ToLower() == "--f" || a.ToLower() == "-f");
        }

        private static void SynchronizeDatasets(string[] args = null)
        {
            var datasets = args == null ? SubscriberDatasetManager.GetAllDataset() : GetDatasetsFromArgs(args);

            if (!SkipPrompt(args))
            {
                Console.WriteLine($"Syncronize datasets {string.Join(',', datasets.Select(d => d.Name))}?");

                if (!GetYorN()) return;
            }

            foreach (var dataset in datasets) Synchronize(dataset.DatasetId);
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
                Console.WriteLine(NormalizeText("Name", d.Name));
                Console.WriteLine(NormalizeText("DatasetId", d.DatasetId.ToString()));
                Console.WriteLine(NormalizeText("ProviderDatasetId",d.ProviderDatasetId));
                Console.WriteLine(NormalizeText("TargetNameSpace",d.TargetNamespace));
                Console.WriteLine(NormalizeText("SyncronizationUrl", d.SyncronizationUrl));
                Console.WriteLine(NormalizeText("ClientWfsUrl", d.ClientWfsUrl));
                Console.WriteLine(NormalizeText("AbortedChangelogPath", d.AbortedChangelogPath));
                Console.WriteLine(NormalizeText("ChangelogDirectory ", d.ChangelogDirectory));
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
            var datasets = GetDatasetsFromArgs(args);

            if (datasets.Count == 0) return;

            Console.WriteLine($"Remove datasets {string.Join(',', datasets.Select(d => d.Name.ToString()))}?");

            if (!SkipPrompt(args) && !GetYorN()) return;

            foreach (var dataset in datasets)
            {
                Console.WriteLine($"Removing dataset {dataset.Name}");
                GeosyncDbEntities.DeleteDataset(dataset);
            }

        }

        private static void ResetDatasets(string[] args)
        {
            var datasets = GetDatasetsFromArgs(args);

            if (datasets.Count == 0) return;

            Console.WriteLine($"Reset datasets {string.Join(',', datasets.Select(d => d.Name.ToString()))}?");

            if (!SkipPrompt(args) && !GetYorN()) return;

            var synchController = new SynchController();

            datasets.ForEach(d => synchController.ResetSubscriberLastIndex(d.DatasetId));

        }

        private static List<Dataset> GetDatasetsFromArgs(string[] args)
        {
            var allDatasets = SubscriberDatasetManager.GetAllDataset();

            var candidateIds = args.ToList().GetRange(1, args.Length - 1);

            return candidateIds.SelectMany(a =>
            {
                if (int.TryParse(a, out int value)) return allDatasets.Where(d => d.DatasetId == int.Parse(a)).ToList();
                return allDatasets.Where(d => d.Name.ToLower() == a.ToLower()).ToList();
            }).ToList();
        }

        private static void AddSpecifiedDatasets(string[] args)
        {
            var capabilitiesDataBuilder = new CapabilitiesDataBuilder(args[1], args[2], args[3]);

            var requiredDatasets = new List<string>();

            var aliases = new List<string>();

            for (var i = 5; i < args.Length; i++)
            {
                requiredDatasets.Add(args[i].Split(':')[0]);

                aliases.Add(args[i]);
            }

            var datasets = capabilitiesDataBuilder.ProviderDatasetsList.Where(d => requiredDatasets.Contains(d.ProviderDatasetId)).ToList();

            AddDatasets(args[1], args[2], args[3], datasets, args[4], aliases);
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
                    Console.WriteLine($"Usage: {Operations.add} $serviceUrl $username $password $wfsUrl [ $datasetid:name ]");
                    Console.WriteLine($"\tAdds datasets from provider. If no datasetId is specified, all are added.");
                    break;
                case Operations.auto:
                    Console.WriteLine($"Usage: {Operations.auto}");
                    Console.WriteLine($"\tUsed for batch-running. Syncs all datasets without prompt");
                    break;
                case Operations.reset:
                    Console.WriteLine($"Usage: {Operations.reset} $datasetId1 $datasetId2 ... [--f]");
                    Console.WriteLine($"\tReset dataset(s)");
                    Console.WriteLine($"\t--f\tSkip prompt");
                    break;
                case Operations.remove:
                    Console.WriteLine($"Usage: {Operations.remove} $datasetId1 $datasetId2 ... [--f]");
                    Console.WriteLine($"\tRemove dataset(s)");
                    Console.WriteLine($"\t--f\tSkip prompt");
                    break;
                case Operations.list:
                    Console.WriteLine($"Usage: {Operations.list} || {Operations.list} $serviceUrl $username $password");
                    Console.WriteLine($"\tIf no more arguments are given, lists local datasets. Else lists datasets on specified provider");
                    break;
                case Operations.sync:
                    Console.WriteLine($"Usage: {Operations.sync} $datasetId1 $datasetId2 ...  [--f]");
                    Console.WriteLine($"\tSync dataset(s) using local datasetId (found using list)");
                    Console.WriteLine($"\t--f\tSkip prompt");
                    break;
                case Operations.set:
                    Console.WriteLine($"Usage: {Operations.set} $datasetId1 $datasetId2 ...  $fieldName1 $fieldValue1 $fieldName2 $fieldValue2 ...");
                    Console.WriteLine($"\tSet fields for dataset(s) in sqlite database, e.g. for ChangelogDirectory");
                    break;
                default:
                    WriteHelp();
                    break;

            }
        }

        private static void AddDatasets(string url, string user, string password, List<Dataset> datasets, string clientWfsUrl, List<string> aliases = null)
        {
            for (var i = 0; i < datasets.Count(); i++)
            {
                var dataset = datasets[i];

                dataset.SyncronizationUrl = url;
                
                dataset.ClientWfsUrl = clientWfsUrl;
                
                dataset.UserName = user;
                
                dataset.Password = password;
                
                if (aliases != null && aliases[i].Contains(':')) dataset.Name = aliases[i].Split(':')[1];

                GeosyncDbEntities.InsertDataset(dataset);
            }
        }

        private static void Synchronize(int datasetId)
        {
            var synchController = new SynchController();
            try
            {
                // Added Event handling for CORESubscriber
                synchController.NewSynchMilestoneReached += Progress_OnMilestoneReached;
                synchController.UpdateLogList += Progress_UpdateLogList;
                synchController.OrderProcessingStart += Progress_OrderProcessingStart;
                synchController.OrderProcessingChange += Progress_OrderProcessingChange;


                Console.Out.WriteLine("INFO: Starting synchronization of datasetId " + datasetId);
                synchController.InitTransactionsSummary();
                synchController.DoSynchronization(datasetId);
                WriteTransactionSummary(synchController);
                Console.Out.WriteLine("INFO: Finished synchronization of datasetId " + datasetId);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR Synchronize: " + e.Message + e.StackTrace);
                //throw;
            }
        }

        #region Event handling

        protected static void Progress_OnMilestoneReached(object sender, EventArgs e)
        {
            try
            {
                var prg = (FeedbackController.Progress)sender;

                var newMilestoneDescription = prg.MilestoneDescription;
                Console.Out.WriteLine("INFO: " + newMilestoneDescription);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }
        
        protected static void Progress_UpdateLogList(object sender, EventArgs e)
        {
            try
            {
                var prg = (FeedbackController.Progress)sender;

                var newLogListItem = prg.NewLogListItem;
                Console.Out.WriteLine("LOG: " + newLogListItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }

        protected static void Progress_OrderProcessingStart(object sender, EventArgs e)
        {
            try
            {
                var prg = (FeedbackController.Progress)sender;
                Console.Out.WriteLine("TotalNumberOfOrders: " + prg.TotalNumberOfOrders);
            }
            catch (Exception ex)
            {
            }
        }

        protected static void Progress_OrderProcessingChange(object sender, EventArgs e)
        {
            try
            {
                var prg = (FeedbackController.Progress)sender;
                Console.Out.WriteLine("OrdersProcessedCount: " + prg.OrdersProcessedCount/100 + " of total: "+ prg.TotalNumberOfOrders);
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

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
