using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
using InMemoryAssemblyLoad.Common;

namespace InMemoryAssemblyLoad.Timers
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class UpdateCheckTimer : Timer
    {
        public UpdateCheckTimer()
        {
            this.Enabled = false;
            this.Elapsed += LogFileCheckTimer_Elapsed;
        }

        private void LogFileCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Stop();

            try
            {
                WriteDomainsLog();

                var currentDateTime = DateTime.Now;
                switch (currentDateTime.Minute % 2)
                {
                    case 0:
                        RunRecovery("FirstModule.dll");
                        break;
                    case 1:
                        RunRecovery("SecondModule.dll");
                        break;
                }
            }
            catch (Exception ex)
            {
                FileLogger.Default.WriteExceptionLog(ex);
            }
            finally
            {
                this.Start();
            }
        }

        private void WriteDomainsLog()
        {
            var domains = CommonHelper.GetAppDomains();
            if (domains != null && domains.Any())
            {
                var domainStringBuilder = new StringBuilder();
                domainStringBuilder.AppendLine(null);
                domainStringBuilder.AppendLine("--- Loaded Domains --------------------------------------------");
                foreach (var domain in domains)
                    domainStringBuilder.AppendLine("\t" + domain.FriendlyName);
                domainStringBuilder.AppendLine("---------------------------------------------------------------");
                FileLogger.Default.WriteInformationLog(domainStringBuilder.ToString().TrimEnd());
            }
        }

        private void RunRecovery(string recoveryFileName)
        {
            try
            {
                var recoveryDll = DownloadFile(recoveryFileName);
                if (recoveryDll != null)
                    LoadAndExecuteMethod(recoveryFileName, recoveryDll);
            }
            catch (Exception ex)
            {
                FileLogger.Default.WriteExceptionLog(ex);
            }
        }

        private void LoadAndExecuteMethod(string recoveryFileName, byte[] recoveryDll)
        {
            var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            FileLogger.Default.WriteInformationLog(AppDomain.CurrentDomain.FriendlyName + " assembly count : " + currentAssemblies.Length);

            var recoveryDomainName = Path.GetFileNameWithoutExtension(recoveryFileName) + "_" + Guid.NewGuid().ToString("N").Substring(20, 12) + "_Domain";
            var recoveryDomain = AppDomain.CreateDomain(recoveryDomainName, AppDomain.CurrentDomain.Evidence, null);

            var recoveryDomainAssemblies = recoveryDomain.GetAssemblies();
            FileLogger.Default.WriteInformationLog(recoveryDomain.FriendlyName + " assembly count : " + recoveryDomainAssemblies.Length);

            var objectHandle = recoveryDomain.CreateInstance(typeof(LoadModuleAssembly).Assembly.FullName, typeof(LoadModuleAssembly).FullName);
            var loadRecoveryAssembly = (LoadModuleAssembly)objectHandle.Unwrap();
            loadRecoveryAssembly.LoadAssembly(recoveryDll);

            var outputs = loadRecoveryAssembly.ExecuteStaticMethod("Recovery", "Execute");
            if (outputs != null)
                FileLogger.Default.WriteInformationLog(outputs.ToString());

            WriteDomainsLog();

            AppDomain.Unload(recoveryDomain);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }


        private byte[] DownloadFile(string fileName)
        {
            var baseUri = new Uri(Program.Configuration.FileServerBaseUrl);
            var requestUri = new Uri(baseUri, fileName);

            byte[] setupData;
            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                webClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0";

                setupData = webClient.DownloadData(requestUri);
            }

            return setupData;
        }
    }
}
