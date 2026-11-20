namespace Solution;

public static class Logger
{
    //public static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    //public static readonly log4net.ILog _logPTime = log4net.LogManager.GetLogger("LogRequestAppender");
    //static List<IWebSocketConnection> _allSockets = new List<IWebSocketConnection>();
    //public static cWebSocket _oSocket;
    public static bool IsSocket = false;
    public static bool IsConsole = true;

    public enum TipoLog
    {
        Error = 0,
        Debug = 1,
        Fatal = 2,
        Info = 3,
        Warn = 4

    }

    public static void WriteLine(string sMessage, TipoLog Tipo)
    {

        //switch (Tipo)
        //{
        //    case TipoLog.Debug:
        //        _log.Debug(sMessage);
        //        break;
        //    case TipoLog.Error:
        //        _log.Error(sMessage);
        //        break;
        //    case TipoLog.Fatal:
        //        _log.Fatal(sMessage);
        //        break;
        //    case TipoLog.Info:
        //        _log.Info(sMessage);
        //        break;
        //    case TipoLog.Warn:
        //        _log.Warn(sMessage);
        //        break;
        //}
    }

    public static void WriteLineProcessing(string sMessage)
    {
        //_logPTime.Info(sMessage);
    }

}

public class Log
{
    //public static log4net.ILog _log;
    public enum TipoLog
    {
        Error = 0,
        Debug = 1,
        Fatal = 2,
        Info = 3,
        Warn = 4

    }
    public static bool IsConsole { get; set; } = true;
    public static bool IsFile { get; set; } = false;
    public static string Format { get; set; } = "HH:mm:ss.fff";

    private static readonly object _fileLock = new();

    private static string SanitizeLogMessage(string? message)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;
        return message.Replace("\r", "\\r").Replace("\n", "\\n");
    }

    static public void WriteLine(params string[] sMessages)
    {
        for (int i = 0; i < sMessages.Length; i++)
        {
            string sanitized = SanitizeLogMessage(sMessages[i]);
            if (IsConsole)
                Console.WriteLine(sanitized);
            if (IsFile)
            {
                lock (_fileLock)
                {
                    Directory.CreateDirectory("log");
                    using var stream = new StreamWriter("log/log_" + DateTime.Now.ToString("yyyyMMdd") + ".log", true);
                    stream.WriteLine(sanitized);
                }
            }
        }
    }

    static public void WriteLineWithTime(params string[] sMessages)
    {
        for (int i = 0; i < sMessages.Length; i++)
        {
            WriteLine(DateTime.Now.ToString(Format) + " " + sMessages[i]);
        }
    }

    static public void Write(params string[] sMessages)
    {
        for (int i = 0; i < sMessages.Length; i++)
        {
            string sanitized = SanitizeLogMessage(sMessages[i]);
            if (IsConsole)
                Console.Write(sanitized);
            if (IsFile)
            {
                lock (_fileLock)
                {
                    Directory.CreateDirectory("log");
                    using var stream = new StreamWriter("log/log_" + DateTime.Now.ToString("yyyyMMdd") + ".log", true);
                    stream.Write(sanitized);
                }
            }
        }
    }
}
