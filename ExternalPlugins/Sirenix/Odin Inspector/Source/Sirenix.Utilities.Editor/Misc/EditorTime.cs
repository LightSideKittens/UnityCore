//-----------------------------------------------------------------------
// <copyright file="EditorTime.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Utilities;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// A utility class for getting delta time for the GUI editor.
    /// </summary>
    public class GUITimeHelper
    {
        private const float fallbackDeltaTime = 0.02f;
        private static Dictionary<IMGUIContainer, (GUITimeHelper layout, GUITimeHelper repaint)> handlers = new Dictionary<IMGUIContainer, (GUITimeHelper layout, GUITimeHelper repaint)>();
        private static Stopwatch sw = Stopwatch.StartNew();

        [InitializeOnLoadMethod]
        private static void Init()
        {
            Sirenix.Reflection.Editor.UIElementsUtility_Internals.BeginContainerCallback += OnBeginContainer;

            // Cleanup code if needed.
            // var toRemove = new List<IMGUIContainer>();
            // EditorApplication.update += () =>
            // {
            //     foreach (var item in handlers)
            //     {
            //         var idleTime = item.Value.layout.sw.Elapsed.TotalSeconds - item.Value.layout.lastTime;
            //         if (idleTime > 30)
            //             toRemove.Add(item.Key);
            //     }
            //     foreach (var item in toRemove)
            //     {
            //         handlers.Remove(item);
            //         Debug.Log("removed old timer");
            //     }
            //     toRemove.Clear();
            // };
        }

        private static void OnBeginContainer(IMGUIContainer obj)
        {
            if (handlers.TryGetValue(obj, out var val) == false)
            {
                val = handlers[obj] = (new GUITimeHelper(EventType.Layout), new GUITimeHelper(EventType.Repaint));
                obj.RegisterCallback<DetachFromPanelEvent>((e) =>
                {
                    handlers.Remove(obj);
                });
            }

            val.layout.Update();
            val.repaint.Update();
        }

        public static float RepaintDeltaTime
        {
            get
            {
                var key = Sirenix.Reflection.Editor.UIElementsUtility_Internals.GetCurrentIMGUIContainer();
                if (key != null && handlers.TryGetValue(key, out var val))
                    return val.repaint.deltaTime;
                return fallbackDeltaTime;
            }
        }

        public static int RepaintFPS => (int) (1.0f / RepaintDeltaTime);

        public static float LayoutDeltaTime
        {
            get
            {
                var key = Sirenix.Reflection.Editor.UIElementsUtility_Internals.GetCurrentIMGUIContainer();
                if (key != null && handlers.TryGetValue(key, out var val))
                    return val.layout.deltaTime;
                return fallbackDeltaTime;
            }
        }
        
        public static int LayoutFPS => (int) (1.0f / LayoutDeltaTime);


        private float deltaTime;
        private double lastTime;
        private EventType trackingEvent;

        private GUITimeHelper(EventType trackingEvent)
        {
            this.deltaTime = fallbackDeltaTime;
            this.lastTime = sw.Elapsed.TotalSeconds;
            this.trackingEvent = trackingEvent;
        }

        private void Update()
        {
            if (Event.current.type == this.trackingEvent)
            {
                var time = sw.Elapsed.TotalSeconds;
                var newDeltaTime = (float)(time - this.lastTime);

                if (newDeltaTime <= 0.2f)
                {
                    this.deltaTime = newDeltaTime;
                }

                this.lastTime = time;
            }
        }

        private struct Key : IEquatable<Key>
        {
            public int WindowHash;
            public UnityEngine.UIElements.IMGUIContainer CurrentContainer;

            public override bool Equals(object obj) => obj is Key key && this.Equals(key);
            public bool Equals(Key other) => this.WindowHash == other.WindowHash && EqualityComparer<IMGUIContainer>.Default.Equals(this.CurrentContainer, other.CurrentContainer);
            public override int GetHashCode() => this.WindowHash + this.CurrentContainer.GetHashCode();
        }
    }




    /// <summary>
    /// A utility class for getting delta time for the GUI editor.
    /// </summary>
    [Obsolete("Use GUITimeHelper.LayoutDeltaTime or GUITimeHelper.RepaintDeltaTime instead depending on which event you're tracking delta time in.", Consts.IsSirenixInternal)]
    public class EditorTimeHelper
    {
        public static readonly EditorTimeHelper Time = new EditorTimeHelper();
        public float DeltaTime => GUITimeHelper.LayoutDeltaTime;
        public void Update()
        {
        }
    }
}
#endif