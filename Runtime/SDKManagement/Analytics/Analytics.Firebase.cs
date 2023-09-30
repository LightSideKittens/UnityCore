#if FIREBASE_ANALYTICS
using System.Collections.Generic;
using System.Text;
using Firebase.Analytics;

namespace LSCore.SDKManagement
{
    public static partial class Analytics
    {
        public class Firebase
        {
            private readonly string eventName;
            private static readonly List<Parameter> parameters = new();
            private static readonly StringBuilder log = new();

            private Firebase(string eventName)
            {
                log.Clear();
                log.Append(eventName);
                parameters.Clear();
                this.eventName = eventName;
            }

            public static Firebase CreateLog(string eventName) => new(eventName);

            public static void LogEvent(string eventName)
            {
                Burger.Log($"[FirebaseAnalytics] {eventName}");
                FirebaseAnalytics.LogEvent(eventName);
            }
            
            public static void LogEvent(string eventName, string paramName, long value)
            {
                Burger.Log($"[FirebaseAnalytics] {eventName} {paramName}: {value}");
                FirebaseAnalytics.LogEvent(eventName, paramName, value);
            }
            
            public static void LogEvent(string eventName, string paramName, string value)
            {
                Burger.Log($"[FirebaseAnalytics] {eventName} {paramName}: {value}");
                FirebaseAnalytics.LogEvent(eventName, paramName, value);
            }
            
            public static void LogEvent(string eventName, string paramName, double value)
            {
                Burger.Log($"[FirebaseAnalytics] {eventName} {paramName}: {value}");
                FirebaseAnalytics.LogEvent(eventName, paramName, value);
            }

            public Firebase Long(string name, long value)
            {
                log.Append($"\n{name}: {value}");
                parameters.Add(new Parameter(name, value));
                return this;
            }
            
            public Firebase String(string name, string value)
            {
                log.Append($"\n{name}: {value}");
                parameters.Add(new Parameter(name, value));
                return this;
            }
            
            public Firebase Double(string name, double value)
            {
                log.Append($"\n{name}: {value}");
                parameters.Add(new Parameter(name, value));
                return this;
            }

            public void Send()
            {
                Burger.Log($"[FirebaseAnalytics] {log}");
                FirebaseAnalytics.LogEvent(eventName, parameters.ToArray());
            }
        }
    }
}
#endif