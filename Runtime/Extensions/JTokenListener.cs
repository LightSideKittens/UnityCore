using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.Extensions
{
    public sealed class JTokenListener : IDisposable
    {
        [ResetStatic(true)]
        private static Dictionary<JContainer, JTokenListener> listeners = new();
        public JContainer token;
        private Dictionary<object, JTokenListener> nestedListeners = new();
        public event Action Changed;
        private Action dispose;

        public JContainer Token
        {
            get => token;
            set
            {
                if (token != value)
                {
                    if (token != null)
                    {
                        UnListen();
                    }
                    token = value;
                    Listen();
                    Changed?.Invoke();
                }
            }
        }

        public JTokenListener this[object key]
        {
            get
            {
                if (!nestedListeners.TryGetValue(key, out var value))
                {
                    if (token != null)
                    {
                        var valToken = token[key];
                        value = valToken != null ? Get(valToken) : new JTokenListener();
                    }
                    else
                    {
                        value = new JTokenListener();
                    }
                    
                    nestedListeners[key] = value;
                }
                
                return value;
            }
        }

        private JTokenListener(){}
        
        internal static JTokenListener Get(JToken token)
        {
            if (token == null) throw new NullReferenceException();
            var container = GetContainer(token);
            if (!listeners.TryGetValue(container, out var value))
            {
                value = new JTokenListener();
                value.Token = container;
            }
            
            return value;
        }

        private static JContainer GetContainer(JToken token)
        {
            if(token == null) return null;
            
            if (token is JContainer container)
            {
                return container;
            }
            
            return token.Parent;
        }

        public void ListenAndCall(Action onChanged)
        {
            Changed += onChanged;
            if (token != null)
            {
                onChanged();
            }
        }

        public void UnListen(Action onChanged)
        {
            Changed -= onChanged;
            if (Changed == null)
            {
                Dispose();
            }
        }
        
        private void Listen()
        {
            if(token == null) return;
            listeners[token] = this;
            token.ListChanged += OnListChanged;
        }

        private void UnListen()
        {
            if(token == null) return;
            listeners.Remove(token);
            token.ListChanged -= OnListChanged;
        }

        private void OnListChanged(object sender, ListChangedEventArgs e)
        {
            bool needFireChanged = true;
            
            if (token is JProperty prop)
            {
                var lastToken = token;
                Token = GetContainer(prop.Value);
                needFireChanged = lastToken == token;
            }
            
            OnChanged(needFireChanged);
        }

        private void OnChanged(bool needFireChanged = true)
        {
            if (token is not JProperty)
            {
                foreach (var (key, listener) in nestedListeners)
                {
                    listener.Token = GetContainer(token[key]);
                }
            }

            if (needFireChanged)
            { 
                Changed?.Invoke();
            }
        }
        
        public void Dispose()
        {
            UnListen();
            foreach (var listener in nestedListeners.Values)
            {
                listener.Dispose();
            }
        }
    }
}