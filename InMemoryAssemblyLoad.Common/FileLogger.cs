using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace InMemoryAssemblyLoad.Common
{
    public delegate void LogWriteDelegate(string log);
    public class FileLogger
    {
        public const string LoggerDateFormat = "yyyy-MM-ddTHH:mm:ss.fff";

        private static readonly Lazy<FileLogger> _lazyFileLogger = new Lazy<FileLogger>(() => new FileLogger());

        public static FileLogger Default => _lazyFileLogger.Value;

        public event LogWriteDelegate LogWrite;

        protected virtual void OnLogWrite(string log)
        {
            LogWrite?.Invoke(log);
        }

        public string LogName { get; private set; }

        public string LogFileName { get; private set; }

        public string LogSource { get; private set; }

        public bool IsDevelopmentEnvironment { get; set; }

        public bool IsVerbose { get; set; }

        public bool IsConsoleWrite { get; private set; }

        public long? MaxLogFileSize { get; private set; }

        public ulong ProgramId { get; private set; }

        public bool IsDailyLog { get; private set; }

        public bool IsDebug { get; set; }

        public bool IsTrace { get; set; }

        public string CurrentLogPath { get; private set; }

        private readonly object _lockWriteLogSource = new object();

        private readonly object _lockCreateLogSource = new object();

        private readonly object _lockTrimLogFile = new object();

        private readonly object _lockErrorLogSource = new object();

        private bool _isCreated;
        private bool _isClosed;

        public void CreateLogSource(string logSource, string logName, bool isConsoleWrite, bool isDailyLog, long? maxLogFileSize, ulong programId)
        {
            if (_isCreated == false)
            {
                lock (_lockCreateLogSource)
                {
                    if (_isCreated)
                        return;

                    IsDailyLog = isDailyLog;
                    IsConsoleWrite = isConsoleWrite;
                    MaxLogFileSize = maxLogFileSize;
                    ProgramId = programId;
                    LogSource = string.IsNullOrEmpty(logSource) ? AppDomain.CurrentDomain.BaseDirectory : logSource;
                    LogName = logName;
                    LogFileName = GenerateLogFileName();
                    CurrentLogPath = Path.Combine(LogSource, LogFileName);

                    if (Directory.Exists(LogSource) == false)
                        Directory.CreateDirectory(LogSource);

                    Close();

                    var writerListener = new TextWriterTraceListener(CurrentLogPath) { Name = LogName };
                    Trace.Listeners.Add(writerListener);

                    if (IsConsoleWrite)
                    {
                        var consoleListener = new ConsoleTraceListener(true) { TraceOutputOptions = TraceOptions.DateTime };
                        Trace.Listeners.Add(consoleListener);
                    }

                    Trace.AutoFlush = false;
                    _isCreated = true;
                    _isClosed = false;
                }
            }
        }

        private string GenerateLogFileName()
        {
            string logFileName;

            if (IsDailyLog)
                logFileName = DateTime.Now.ToString("yyyy-MM-dd") + "-" + LogName;
            else
                logFileName = LogName;

            return logFileName + ".log";
        }

        public void Close()
        {
            lock (_lockCreateLogSource)
            {
                if (_isClosed)
                    return;

                if (Trace.Listeners.Count > 0)
                {
                    foreach (TraceListener listener in Trace.Listeners)
                    {
                        listener.Flush();
                        listener.Close();
                    }

                    Trace.Listeners.Clear();
                }

                _isClosed = true;
            }
        }

        public void WriteLog(string log, LogTypes logType)
        {
            WriteLog(log, logType, false);
        }

        public void WriteLog(string log, LogTypes logType, bool useLogWriteEvent)
        {
            try
            {
                NewLogFileIsNecessary();

                if (logType != LogTypes.Unformatted)
                    log = $"{DateTime.Now.ToString(LoggerDateFormat)}|{logType.GetShortDescription()}|{ProgramId}|{Environment.MachineName}|AGT|{log}";

                if (useLogWriteEvent)
                    OnLogWrite(log);

                foreach (TraceListener listener in Trace.Listeners)
                {
                    listener.WriteLine(log);
                    if (listener is TextWriterTraceListener)
                    {
                        lock (_lockWriteLogSource)
                            listener.Flush();
                    }
                }

                if (MaxLogFileSize.HasValue && MaxLogFileSize.Value > 0)
                {
                    var fileInfo = new FileInfo(CurrentLogPath);
                    if (fileInfo.Exists && fileInfo.Length > MaxLogFileSize.Value)
                        TrimLogFile();
                }
            }
            catch (Exception ex)
            {
                WriteLogErrorLog(ex);
            }
        }

        private Exception _lastWriteLogException;

        private void WriteLogErrorLog(Exception exception)
        {
            try
            {
                lock (_lockErrorLogSource)
                {
                    if (_lastWriteLogException != null && _lastWriteLogException.Message == exception.Message)
                        return;
                    _lastWriteLogException = exception;
                }

                var errorLog = $"{DateTime.Now.ToString(LoggerDateFormat)}|ERR|{ProgramId}|{Environment.MachineName}|AGT|{Environment.NewLine + exception + Environment.NewLine}";

                var errorLogPath = CurrentLogPath + ".err";
                if (File.Exists(errorLogPath))
                    File.AppendAllText(errorLogPath, errorLog);
                else
                    File.WriteAllText(errorLogPath, errorLog);
            }
            catch (Exception ex)
            {
                OnLogWrite(ex.ToString());
            }
        }

        private void NewLogFileIsNecessary()
        {
            var isNecessary = false;
            if (IsDailyLog)
            {
                var newLogFileName = GenerateLogFileName();
                isNecessary = LogFileName != newLogFileName;
            }

            if (isNecessary)
            {
                _isCreated = false;
                lock (_lockCreateLogSource)
                {
                    if (_isCreated)
                        return;
                    CreateLogSource(LogSource, LogName, IsConsoleWrite, IsDailyLog, MaxLogFileSize, ProgramId);
                    WriteInformationLog("Log file created or connected. " + CurrentLogPath);
                }
            }
        }

        private void TrimLogFile()
        {
            lock (_lockTrimLogFile)
            {
                var fileInfo = new FileInfo(CurrentLogPath);
                if (fileInfo.Exists && fileInfo.Length > MaxLogFileSize)
                {
                    TextWriterTraceListener textWriterTraceListener = null;
                    foreach (TraceListener listener in Trace.Listeners)
                    {
                        if (listener is TextWriterTraceListener traceListener)
                        {
                            textWriterTraceListener = traceListener;
                            break;
                        }
                    }

                    if (textWriterTraceListener == null)
                        return;

                    textWriterTraceListener.Flush();
                    textWriterTraceListener.Close();

                    var allLineCount = File.ReadAllLines(CurrentLogPath);
                    var skipLineCount = Convert.ToInt32(allLineCount.LongLength / 4);
                    File.WriteAllLines(CurrentLogPath, allLineCount.Skip(skipLineCount).ToArray(), Encoding.UTF8);
                }
            }
        }

        public void WriteUnformattedLog(string log)
        {
            WriteUnformattedLog(log, false);
        }

        public void WriteUnformattedLog(string log, bool useLogWriteEvent)
        {
            WriteLog(log, LogTypes.Unformatted, useLogWriteEvent);
        }

        public void WriteErrorLog(string log)
        {
            WriteErrorLog(log, false);
        }

        public void WriteErrorLog(string log, bool useLogWriteEvent)
        {
            WriteLog(log, LogTypes.Error, useLogWriteEvent);
        }

        public void WriteInformationLog(string log)
        {
            WriteInformationLog(log, false);
        }

        public void WriteInformationLog(string log, bool useLogWriteEvent)
        {
            WriteLog(log, LogTypes.Information, useLogWriteEvent);
        }

        public void WriteWarningLog(string log)
        {
            WriteWarningLog(log, false);
        }

        public void WriteWarningLog(string log, bool useLogWriteEvent)
        {
            WriteLog(log, LogTypes.Warning, useLogWriteEvent);
        }

        public void WriteExceptionLog(Exception exception)
        {
            WriteExceptionLog(exception, false);
        }

        public void WriteExceptionLog(Exception exception, bool useLogWriteEvent)
        {
            WriteLog($"{exception.GetType().FullName}|{Environment.NewLine}{exception}", LogTypes.Error, useLogWriteEvent);
            if (exception.InnerException != null)
                WriteExceptionLog(exception.InnerException, useLogWriteEvent);
        }

        public void WriteExceptionLog(string log, Exception exception)
        {
            WriteExceptionLog(log, exception, false);
        }

        public void WriteExceptionLog(string log, Exception exception, bool useLogWriteEvent)
        {
            WriteLog($"{exception.GetType().FullName}|{log}{Environment.NewLine}{exception}", LogTypes.Error, useLogWriteEvent);
            if (exception.InnerException != null)
                WriteExceptionLog(log, exception.InnerException, useLogWriteEvent);
        }

        public void WriteDebugLog(string log)
        {
            if (IsDebug == false)
                return;

            WriteDebugLog(log, false);
        }

        public void WriteDebugLog(Exception exception)
        {
            if (IsDebug == false)
                return;

            WriteDebugLog(exception, false);
        }

        public void WriteDebugLog(Exception exception, bool useLogWriteEvent)
        {
            if (IsDebug == false)
                return;

            WriteDebugLog($"{exception.GetType().FullName}|{Environment.NewLine}{exception}", useLogWriteEvent);
            if (exception.InnerException != null)
                WriteDebugLog(exception.InnerException, useLogWriteEvent);
        }

        public void WriteDebugLog(string log, bool useLogWriteEvent)
        {
            if (IsDebug == false)
                return;

            WriteLog(log, LogTypes.Debug, useLogWriteEvent);
        }

        public void WriteTraceLog(string log)
        {
            if (IsTrace == false)
                return;

            WriteTraceLog(log, false);
        }

        public void WriteTraceLog(Exception exception)
        {
            if (IsTrace == false)
                return;

            WriteTraceLog(exception, false);
        }

        public void WriteTraceLog(Exception exception, bool useLogWriteEvent)
        {
            if (IsTrace == false)
                return;

            WriteTraceLog($"{exception.GetType().FullName}|{Environment.NewLine}{exception}", useLogWriteEvent);
            if (exception.InnerException != null)
                WriteTraceLog(exception.InnerException, useLogWriteEvent);
        }

        public void WriteTraceLog(string log, bool useLogWriteEvent)
        {
            if (IsTrace == false)
                return;

            WriteLog(log, LogTypes.Trace, useLogWriteEvent);
        }
    }
}