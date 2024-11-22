using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class BurgerHandler
{
    private static string fileName;
    private static string logPath => $"{Application.persistentDataPath}/Logs";
    private static StringBuilder builder;
    private static (string message, string stacktrace, LogType logType) prevMessage;
    private static int lastLength;
    private static int repeatCount;
    private static object threadLock = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (!Directory.Exists(logPath))
        {
            Directory.CreateDirectory(logPath);
        }
        
        Application.logMessageReceivedThreaded -= UnityLogCallback;
        Application.logMessageReceivedThreaded += UnityLogCallback;
    }
    
    private static void UnityLogCallback(string condition, string stackTrace, LogType type)
    {                
        lock (threadLock)
        { 
            if (builder == null || builder.Length > 10000)
            {
                builder = new StringBuilder();
                fileName = $"{logPath}/{DateTime.UtcNow.Ticks}.txt";
                prevMessage = default;
                lastLength = 0;
                repeatCount = 1;
            }
            
            if (prevMessage != default && prevMessage.logType == type && prevMessage.message == condition &&
                prevMessage.stacktrace == stackTrace)
            {
                repeatCount++;
                
                if (Burger.logToFile)
                {
                    builder.Remove(lastLength, builder.Length - lastLength);
                    var repeatCountText = $"({repeatCount}) ";
                    builder.Append(repeatCountText);
                    Build();
                    lastLength -= repeatCountText.Length;
                }
                else
                {
                    builder = null;
                }
            }
            else
            {
                repeatCount = 1;
                if (Burger.logToFile)
                {
                    Build();
                }
                else
                {
                    builder = null;
                }
            }

            prevMessage.logType = type;
            prevMessage.message = condition;
            prevMessage.stacktrace = stackTrace;
            
            void Build()
            {
                lastLength = builder.Length;
                builder.Append($"[{type}] ");
                builder.AppendLine(condition);
                builder.AppendLine(stackTrace);
                File.WriteAllText(fileName, builder.ToString());
            }
        }
    }
}