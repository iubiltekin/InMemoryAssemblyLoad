using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FirstModule
{
    public class Recovery
    {

        public string Execute()
        {
            var outputBuilder = new StringBuilder();
            outputBuilder.AppendLine("First Recovery Module output");

            var systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var processPath = Path.Combine(systemFolder, "timeout.exe");
            var arguments = "/t 5";

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = false,
                    LoadUserProfile = true,
                    FileName = processPath,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    ErrorDialog = false,
                    Arguments = arguments
                };


                var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        outputBuilder.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        outputBuilder.AppendLine(e.Data);
                };

                var isStarted = process.Start();
                if (!isStarted)
                {
                    var winErrorCode = Marshal.GetLastWin32Error();
                    if (winErrorCode != 0)
                        throw new Win32Exception(winErrorCode);

                    outputBuilder.AppendLine("Process initialization method failed to start the process without receiving an error. Error Code : " + winErrorCode);
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit(120000);
            }
            catch (Exception ex)
            {
                var winErrorCode = Marshal.GetLastWin32Error();
                if (winErrorCode != 0)
                    throw new Win32Exception(winErrorCode);

                outputBuilder.AppendLine(ex.Message + "|" + ex.GetType() + " Error Code : " + winErrorCode);
            }

            return outputBuilder.Length > 0 ? outputBuilder.ToString().TrimEnd() : null;
        }
    }
}
