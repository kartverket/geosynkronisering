using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kartverket.Geosynkronisering.Subscriber.BL;
using Kartverket.Geosynkronisering.Subscriber.DL;

namespace Kartverket.Geosynkronisering.Subscriber
{
    public static class Program
    {
        //We want to be able to write to console if the process has been started from one
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length < 1)
                RunAsWinForms();
            else
                RunAsConsole(args);
        }

        public static void Run()
        {
            SynchronizeDatasets(SubscriberDatasetManager.GetListOfDatasetIDs());
        }

        private static void RunAsConsole(string[] args)
        {
            AttachConsole();
            Console.WriteLine();
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

        private static void RunAsWinForms()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void AttachConsole()
        {
            StreamWriter _stdOutWriter;
            var stdout = Console.OpenStandardOutput();
            _stdOutWriter = new StreamWriter(stdout);
            _stdOutWriter.AutoFlush = true;
            AttachConsole(ATTACH_PARENT_PROCESS);
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
