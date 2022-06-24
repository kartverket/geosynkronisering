﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ChangelogManager;
using Kartverket.Geosynkronisering;
using Test_Subscriber_NetCore;
using Serilog;
using Serilog.Events;

namespace Provider_NetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Geosync Provider_NetCore (.NET 5.0)!");

            // Log with Serilog 
            // So long as you've initialised Log.Logger at application start-up, everything should just work that way.
            var currentExecutable = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            var logFile = @"logs/" + currentExecutable + "_log" + ".log";
            var errorFile = @"logs/" + currentExecutable + "_error" + ".log";
            var debugFile = @"logs/" + currentExecutable + "_debug" + ".log";
            var warningFile = @"logs/" + currentExecutable + "_warning" + ".log";
            var fatalFile = @"logs/" + currentExecutable + "_fatal" + ".log";
            

            // Split Log Data to seperate files
            // See https://stackoverflow.com/questions/28292601/serilog-multiple-log-files
            // https://vmsdurano.com/serilog-and-asp-net-core-split-log-data-using-filterexpression/
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information).WriteTo.File(logFile, rollingInterval: RollingInterval.Day, shared: true))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug).WriteTo.File(debugFile, rollingInterval: RollingInterval.Day, shared: true))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error).WriteTo.File(errorFile, rollingInterval: RollingInterval.Day, shared: true))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning).WriteTo.File(warningFile, rollingInterval: RollingInterval.Day, shared: true))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal).WriteTo.File(fatalFile, rollingInterval: RollingInterval.Day, shared: true))
                .CreateLogger();


            Log.Information("Starting Geosync Provider_NetCore (.NET 5.0)!");
            if (false)
            {
                // Testing
                Log.Debug("test debug level ");
                Log.Warning("test warning level");

                try
                {
                    var d = 0;
                    var n = 10 / d;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "test error logging");
                    Log.Fatal(ex, "test fatal logging"  );
                    
                }
            }

            // Make App_Data available
            var contentRootPath = AppDomain.CurrentDomain.BaseDirectory;
            var appData = "App_Data" + Path.DirectorySeparatorChar.ToString();
            //AppDomain.CurrentDomain.SetData("DataDirectory", appData);
            //AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(contentRootPath, "App_Data", Path.DirectorySeparatorChar.ToString()));
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(contentRootPath, appData));


            RunAsConsole(args);
        }

        private static bool InDocker
        {
            get { return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"; }
        }

        private static void RunAsConsole(string[] args)
        {
            if (args.Length < 1)
            {
                WriteHelp();
                return;
            }

            _ = new geosyncEntities();

            switch (args[0].ToLower())
            {
                case Operations.list:
                    if (args.Length == 1) ListDatasets();
                    else WriteHelp(Operations.list);
                    break;
                case Operations.initial:
                    if (args.Length == 2) CreateInitialData(args);
                    else WriteHelp(Operations.initial);
                    break;
                case Operations.push:
                    var datasets = GetDatasetsFromArgs(args);
                    if (!datasets.Any()) WriteHelp(Operations.push);
                    Pusher.Synchronize(datasets);
                    break;
                case Operations.pushAll:
                    Pusher.Synchronize(GetAllDatasets());
                    break;
                default:
                    WriteHelp();
                    break;

            }

        }

        private static void CreateInitialData(string[] args = null)
        {
            var datasets = GetDatasetsFromArgs(args);
            Console.WriteLine($"Create initial datasets for {string.Join(',', datasets.Select(d => d.Name))}?");
            if (!GetYorN()) return;

            Console.WriteLine("Please wait...");
            foreach (var dataset in datasets) CreateInitialData(dataset);

        }

        private static void CreateInitialData(Dataset dataset)
        {
            try
            {
                var resp = Utils.GetChangelogProvider(dataset).GenerateInitialChangelog(dataset.DatasetId);
                Console.WriteLine($"Created initial dataset for dataset:" + dataset.Name + " with datasetId:" + dataset.DatasetId);
            }
            catch (Exception ex)
            {

                string innerExMsg = "";
                Exception innerExp = ex.InnerException;
                while (innerExp != null)
                {
                    innerExMsg += string.Format("{0}. \n", innerExp.Message);
                    innerExp = innerExp.InnerException;
                }

                string errorMsg = string.Format("Klarte ikke å lage initiell endringslogg. {0} \n {1}", ex.Message,
                    innerExMsg);
                Console.WriteLine("ERROR creating initial changelog: " + errorMsg);
                //throw;
            }


        }

        private static List<Dataset> GetDatasetsFromArgs(string[] args)
        {
            var allDatasets = GetAllDatasets();

            var candidateIds = args.ToList().GetRange(1, args.Length - 1);

            return candidateIds.SelectMany(a =>
            {
                if (int.TryParse(a, out int value)) return allDatasets.Where(d => d.DatasetId == int.Parse(a)).ToList();
                return allDatasets.Where(d => d.Name.ToLower() == a.ToLower()).ToList();
            }).ToList();
        }

        private static List<Dataset> GetAllDatasets()
        {
            return geosyncEntities.ReadAll<Dataset>("Datasets");
        }

        private static void ListDatasets()
        {
            Console.WriteLine("Listing local datasets:");

            var datasets = geosyncEntities.ReadAll<Dataset>("Datasets");
            datasets.ForEach(d =>
            {
                Console.WriteLine("-----------------------------------------------------------");
                Console.WriteLine(NormalizeText("DatasetId", d.DatasetId.ToString()));
                Console.WriteLine(NormalizeText("Name", d.Name));

                Console.WriteLine(NormalizeText("DatasetProvider", d.DatasetProvider));
                Console.WriteLine(NormalizeText("SchemaFileUri", d.SchemaFileUri));
                Console.WriteLine(NormalizeText("TargetNameSpace", d.TargetNamespace));

                Console.WriteLine(NormalizeText("TransformationConnection", d.TransformationConnection));
                Console.WriteLine(NormalizeText("DBSchema", d.DBSchema));

                Console.WriteLine(NormalizeText("DefaultCrs", d.DefaultCrs));
                Console.WriteLine(NormalizeText("Version ", d.Version));
            });

        }


        private static void WriteHelp()
        {
            Console.WriteLine($"Args: {string.Join('|', Operations.all)}");
        }

        private static void WriteHelp(string operation)
        {
            switch (operation)
            {

                case Operations.list:
                    Console.WriteLine($"Usage: {Operations.list} || {Operations.list}");
                    break;
                case Operations.push:
                    Console.WriteLine($"Usage: {Operations.push} datasetId1 datasetId2 ...");
                    break;
                default:
                    WriteHelp();
                    break;
            }

        }
        private static string NormalizeText(string key, string value)
        {
            if (string.IsNullOrEmpty(value)) value = "";
            var linelength = 30;
            key = key.Trim() + ":";
            key = key.PadRight(linelength);
            return key + value.Trim();
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

