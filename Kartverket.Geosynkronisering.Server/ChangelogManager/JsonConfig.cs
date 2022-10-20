using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Serilog;

namespace ChangelogManager
{
    public class JsonConfig
    {

        public static IConfiguration SetupJsonConfig(object bindTo = null)
        {
            Log.Information("start SetupJsonConfig");

            //var testGetCurrentDirectory = System.IO.Directory.GetCurrentDirectory();
            //Log.Information("SetupJsonConfig testGetCurrentDirectory: {0}", testGetCurrentDirectory);


            // issue #157: fix for  self-contained executable, Assembly.GetEntryAssembly().Location failes
            var strPath = Process.GetCurrentProcess().MainModule.FileName;
            Log.Information("SetupJsonConfig strPath: {0}", strPath);
            //var strPath = Assembly.GetEntryAssembly().Location; //AppDomain.CurrentDomain.BaseDirectory;

            var basePath = Path.GetDirectoryName(strPath);
            Log.Information("SetupJsonConfig basePath: {0}", strPath);
            var strJsonFile = Path.Combine(basePath, "appsettings.json");
            Log.Information("SetupJsonConfig strJsonFile: {0}", strPath);

            if (!File.Exists(strJsonFile))
            {
                // When blazor-app is published on IIS, we must use this method to find the correct directory of where the project is stored
                basePath = AppDomain.CurrentDomain.BaseDirectory;
                Log.Information("SetupJsonConfig basePath: {0}", basePath);
                strJsonFile = Path.Combine(basePath, "appsettings.json");
                Log.Information("SetupJsonConfig strJsonFile: {0}", strJsonFile);

            }
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath) // Directory where the json files are located
                .AddJsonFile(strJsonFile, optional: false, reloadOnChange: true)
                .Build();

            //  bind a configuration instance, see https://weblog.west-wind.com/posts/2017/dec/12/easy-configuration-binding-in-aspnet-core-revisited#raw-configuration-value-binding
            if (bindTo != null)
            {
                configuration.Bind(bindTo);
                // we can access with  var setting = configuration.Get<SettingsXXX>();
            }

            // test
            //var connectionstring = configuration.GetValue<string>("connectionStrings:geosyncEntities");

            Log.Information("SetupJsonConfig End ,configuration != null: {0}", configuration != null);
            return configuration;
        }
    }
}
