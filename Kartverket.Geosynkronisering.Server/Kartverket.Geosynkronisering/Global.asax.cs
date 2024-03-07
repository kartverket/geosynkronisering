using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Routing;
using System.ServiceModel.Activation;
using System.Configuration;
using System.IO;
using Serilog;
using Serilog.Events;

namespace Kartverket.Geosynkronisering
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            //RegisterRoutes(RouteTable.Routes);
            //RouteTable.Routes.Add(new ServiceRoute("", new WebServiceHostFactory(), typeof(Syncronization)));

            // Serilog
            var appSettings = ConfigurationManager.AppSettings;
            var serilogPath = appSettings["serilog-filepath"];
            if (serilogPath.ToUpper() == "%TEMP%")
            {
                serilogPath = Path.GetTempPath();
            }

            var logFile = Path.Combine(serilogPath, appSettings["serilog-logfile"]);
            var errorFile = Path.Combine(serilogPath, appSettings["serilog-errorfile"]);
            var warningFile = Path.Combine(serilogPath, appSettings["serilog-warningfile"]);
            var debugFile = Path.Combine(serilogPath, appSettings["serilog-debugfile"]);
            var fatalFile = Path.Combine(serilogPath, appSettings["serilog-fatalfile"]);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(ev => ev.Level == LogEventLevel.Information).WriteTo.File(logFile, rollingInterval: RollingInterval.Day, shared: true))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(ev => ev.Level == LogEventLevel.Debug).WriteTo.File(debugFile, rollingInterval: RollingInterval.Day, shared: true))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(ev => ev.Level == LogEventLevel.Error).WriteTo.File(errorFile, rollingInterval: RollingInterval.Day, shared: true))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(ev => ev.Level == LogEventLevel.Warning).WriteTo.File(warningFile, shared: true))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(ev => ev.Level == LogEventLevel.Fatal).WriteTo.File(fatalFile, rollingInterval: RollingInterval.Day, shared: true))
                .CreateLogger();
            
            
            Log.Information("Application_Start!");
            if (false)
            {
                Log.Warning("Starting Test Serilog Warning!");
                Log.Debug("test debug level ");
                try
                {
                    var d = 0;
                    var n = 10 / d;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "test error logging");
                    Log.Fatal(ex, "test fatal logging");
                }
            }


        }
        public static void RegisterRoutes(RouteCollection routes)
        {
            //routes.MapPageRoute("Sync",
            //    "ds/{dataset}",
            //    "~/Syncronization.svc");
            

        }


        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}