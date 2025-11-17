using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json.Linq;

namespace LSCore.Extensions
{
    public static partial class JTokenExtensions
    {
        [ResetStatic(true)] 
        private static Dictionary<JContainer, Dictionary<Action, Action>> unlistens = new();
        
        public static void ListenAndCall(this JToken token, Action action) => token.Parent.ListenAndCall(action);
        public static void UnListen(this JToken token, Action action) => token.Parent.UnListen(action);

        public static void ListenAndCall(this JContainer token, Action action)
        {
            if (!unlistens.TryGetValue(token, out var unlistenDict))
            {
                unlistenDict = new();
                unlistens.Add(token, unlistenDict);
            }
            
            unlistenDict.TryGetValue(action, out var unlisten);
            unlisten += UnListen;
            unlistenDict[action] = unlisten;
            
            token.ListChanged += OnListChanged;
            action();
            
            void OnListChanged(object sender, ListChangedEventArgs e)
            {
                action();
            }

            void UnListen()
            {
                token.ListChanged -= OnListChanged;
                unlistenDict.TryGetValue(action, out var nunlisten);
                nunlisten -= UnListen;
                unlistenDict[action] = nunlisten;
            }
        }
        
        public static void UnListen(this JContainer token, Action action)
        {
            if (!unlistens.TryGetValue(token, out var unlistenDict))
            {
                unlistenDict = new();
                unlistens.Add(token, unlistenDict);
            }
            
            unlistenDict.TryGetValue(action, out var unlisten);
            unlisten?.Invoke();
        } }
}