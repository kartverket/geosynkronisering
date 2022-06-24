using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace ChangelogManager
{
    public class JsonConfig
    {

        public static IConfiguration SetupJsonConfig(object bindTo = null)
        {
            //Assembly.GetEntryAssembly()
            var strPath = Assembly.GetEntryAssembly().Location; //AppDomain.CurrentDomain.BaseDirectory;
            ////AppDomain.CurrentDomain.FriendlyName
            //var strPath = Assembly.GetExecutingAssembly().Location;
            var basePath = Path.GetDirectoryName(strPath);

            var strJsonFile = Path.Combine(basePath, "appsettings.json");
            // var strJsonFile = Path.GetFileNameWithoutExtension(strPath) + ".json";
            //var strJsonFile = strPath.Replace(".dll", ".json");

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
            

            return configuration;
        }
    }
}
