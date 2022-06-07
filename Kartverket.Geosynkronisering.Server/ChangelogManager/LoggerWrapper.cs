using System;
using System.Collections.Generic;
using System.Text;
using Serilog;
using Serilog.Events;


namespace Kartverket.Geosynkronisering
{
    /// <summary>
    /// Wrapper for replacing nlog with Serilog
    /// </summary>
    public static class Logger
    {
        public static void Info(string messageTemplate) => Log.Write(LogEventLevel.Information, messageTemplate);
        public static void Info(string messageTemplate, params object[] propertyValues) => Log.Logger.Information(messageTemplate, propertyValues);
        //static void Info()
        //{
        //    Log.Information();
        //}
        public static void Error(Exception exception, string messageTemplate) => Log.Write(LogEventLevel.Error, exception, messageTemplate);
        public static void Error(string messageTemplate, params object[] propertyValues) => Log.Logger.Error(messageTemplate, propertyValues);
        public static void Warn(string messageTemplate) => Log.Write(LogEventLevel.Warning, messageTemplate);
        
        public static void Debug(string messageTemplate) => Log.Write(LogEventLevel.Debug, messageTemplate);
    }
}
