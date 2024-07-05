using System;
using System.IO;
using InMemoryAssemblyLoad.Common;
using InMemoryAssemblyLoad.Timers;

namespace InMemoryAssemblyLoad
{
    internal class Program
    {
        internal static ApplicationConfiguration Configuration = ApplicationConfiguration.Create();
        static void Main(string[] args)
        {
            try
            {
                var logFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                FileLogger.Default.CreateLogSource(logFolderPath, Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName), true, true, null, 0);
                FileLogger.Default.IsVerbose = true;
                FileLogger.Default.IsDebug = true;
                FileLogger.Default.IsTrace = true;
                FileLogger.Default.IsDevelopmentEnvironment = true;

                FileLogger.Default.WriteInformationLog("Log file created or connected. FilePath : " + FileLogger.Default.CurrentLogPath + ", IsDailyLog : " + FileLogger.Default.IsDailyLog + ", IsVerbose : " + FileLogger.Default.IsVerbose + ", IsDebug : " + FileLogger.Default.IsDebug + ", IsTrace : " + FileLogger.Default.IsTrace);

                var updateCheckTimer = new UpdateCheckTimer();
                updateCheckTimer.Interval = 3000;
                updateCheckTimer.Enabled = true;
                updateCheckTimer.Start();
                if (FileLogger.Default.IsVerbose)
                    FileLogger.Default.WriteInformationLog("Log File Check timer started.");

            }
            catch (Exception ex)
            {
                FileLogger.Default.WriteExceptionLog(ex);
            }

            Console.ReadKey();
        }
    }
}
