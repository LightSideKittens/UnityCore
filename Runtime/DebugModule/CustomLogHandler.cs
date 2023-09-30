using System.Collections.Generic;
using UnityEngine;

public class CustomLogHandler : ILogHandler
{
    public static bool Enable
    {
        set => Debug.unityLogger.logHandler = value ? LogHandler : LogHandler.defaultLogHandler;
    }
    public static CustomLogHandler LogHandler { get; }
    private readonly ILogHandler defaultLogHandler;
    private readonly HashSet<string> filters = new();

    static CustomLogHandler()
    {
        LogHandler = new CustomLogHandler();
    }

    private CustomLogHandler()
    {
        defaultLogHandler = Debug.unityLogger.logHandler;
    }

    public CustomLogHandler AddFilter(string filter)
    {
        filters.Add(filter);
        return this;
    }
    
    public CustomLogHandler ClearFilters()
    {
        filters.Clear();
        return this;
    }

    public void LogFormat(LogType logType, Object context, string format, params object[] args)
    {
        string logMessage = string.Format(format, args);
        
        foreach (var filter in filters)
        {
            if (logMessage.StartsWith(filter))
            {
                return; 
            }
        }
        
        defaultLogHandler.LogFormat(logType, context, format, args);
    }

    public void LogException(System.Exception exception, Object context)
    {
        defaultLogHandler.LogException(exception, context);
    }
}