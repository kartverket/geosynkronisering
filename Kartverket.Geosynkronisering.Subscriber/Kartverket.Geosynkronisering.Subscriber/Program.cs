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
    internal static class Program
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
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                AttachConsole();
                Console.WriteLine("INFO: Fetching list of datasetIds");
                try
                {
                    foreach (var datasetId in SubscriberDatasetManager.GetListOfDatasetIDs())
                    {
                        Synchronize(datasetId);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                }
            }
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
