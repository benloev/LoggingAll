using System;
using Serilog;
using Serilog.Events;

namespace LogAll
{
    public static class FullLogger
    {
        private static readonly ILogger _perfLogger;
        private static readonly ILogger _usageLogger;
        private static readonly ILogger _errorLogger;
        private static readonly ILogger _diagnosticLogger;

        static FullLogger()
        {
            //TODO: move Sinks to config file
            //TODO: extract the path and get from constractor 
            _perfLogger = new LoggerConfiguration()
                .WriteTo.File(path: Environment.GetEnvironmentVariable("LOGFILE_PERFORM"))
                .WriteTo.Elasticsearch("http://localhost:9200", indexFormat: "perform-{0:yyyy.MM.dd}", inlineFields: true)
                .CreateLogger();

            _usageLogger = new LoggerConfiguration()
                .WriteTo.File(path: Environment.GetEnvironmentVariable("LOGFILE_USAGE"))
                .WriteTo.Elasticsearch("http://localhost:9200", indexFormat: "usage-{0:yyyy.MM.dd}", inlineFields: true)
                .CreateLogger();

            _errorLogger = new LoggerConfiguration()
                .WriteTo.File(path: Environment.GetEnvironmentVariable("LOGFILE_ERROR"))
                .WriteTo.Elasticsearch("http://localhost:9200", indexFormat: "error-{0:yyyy.MM.dd}", inlineFields: true)
                .CreateLogger();

            _diagnosticLogger = new LoggerConfiguration()
                .WriteTo.File(path: Environment.GetEnvironmentVariable("LOGFILE_DIAGN"))
                .WriteTo.Elasticsearch("http://localhost:9200", indexFormat: "diagnostic-{0:yyyy.MM.dd}", inlineFields: true)
                .CreateLogger();
        }

        public static void WritePerf(LogDetails infoToLog)
        {
            _perfLogger.Write(LogEventLevel.Information, "{@LogDetails}", infoToLog);
        }
        public static void WriteUsage(LogDetails infoToLog)
        {
            _usageLogger.Write(LogEventLevel.Information, "{@LogDetails}", infoToLog);
        }
        public static void WriteError(LogDetails infoToLog)
        {
            if (infoToLog.Exception != null)
            {
                //var procName = FindProcName(infoToLog.Exception);
                //infoToLog.Location = string.IsNullOrEmpty(procName)
                //    ? infoToLog.Location
                //    : procName;
                infoToLog.Message = GetMessageFromException(infoToLog.Exception);
            }
            _errorLogger.Write(LogEventLevel.Information, "{@LogDetails}", infoToLog);
        }
        public static void WriteDiagnostic(LogDetails infoToLog)
        {
            var writeDiagnostics = false;
                Boolean.TryParse(Environment.GetEnvironmentVariable("DIAGNOSTICS_ON"), out writeDiagnostics);
            if (!writeDiagnostics)
                return;

            _diagnosticLogger.Write(LogEventLevel.Information, "{@LogDetails}", infoToLog);
        }

        private static string GetMessageFromException(Exception ex)
        {
            if (ex.InnerException != null)
                return GetMessageFromException(ex.InnerException);

            return ex.Message;
        }

        //private static string FindProcName(Exception ex)
        //{
        //    var sqlEx = ex as SqlException;
        //    if (sqlEx != null)
        //    {
        //        var procName = sqlEx.Procedure;
        //        if (!string.IsNullOrEmpty(procName))
        //            return procName;
        //    }

        //    if (!string.IsNullOrEmpty((string)ex.Data["Procedure"]))
        //    {
        //        return (string)ex.Data["Procedure"];
        //    }

        //    if (ex.InnerException != null)
        //        return FindProcName(ex.InnerException);

        //    return null;
        //}
    }
}
