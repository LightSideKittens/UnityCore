//-----------------------------------------------------------------------
// <copyright file="WeakReferenceEventListener.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

    using System;
    using System.Collections.Generic;

    public sealed class WeakReferenceEventListener<TListener> where TListener : class
    {
        public Action<TListener, object[]> InvokeEventOnListener;
        public List<WeakReference<TListener>> Listeners;

        public WeakReferenceEventListener(Action<TListener, object[]> invokeEventOnListener)
        {
            this.InvokeEventOnListener = invokeEventOnListener;
            this.Listeners = new List<WeakReference<TListener>>();
        }

        public void InvokeEvent(object[] args)
        {
            for (int i = 0; i < this.Listeners.Count; i++)
            {
                var listenerRef = this.Listeners[i];

                TListener listener;

                if (!listenerRef.TryGetTarget(out listener) || object.ReferenceEquals(listener, null))
                {
                    this.Listeners.RemoveAt(i--);
                    continue;
                }

                this.InvokeEventOnListener(listener, args);
            }
        }

        public void SubscribeListener(TListener listener)
        {
            this.Listeners.Add(new WeakReference<TListener>(listener, false));
        }

        public void DesubscribeListener(TListener listener)
        {
            for (int i = 0; i < this.Listeners.Count; i++)
            {
                var listenerRef = this.Listeners[i];

                TListener l;

                if (!listenerRef.TryGetTarget(out l) || object.ReferenceEquals(l, null))
                {
                    this.Listeners.RemoveAt(i--);
                    continue;
                }

                if (object.ReferenceEquals(listener, l))
                {
                    this.Listeners.RemoveAt(i--);
                    continue;
                }
            }
        }
    }
}
#endif