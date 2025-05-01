//-----------------------------------------------------------------------
// <copyright file="ImGUITimeHelper.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Diagnostics;
    using UnityEngine;

    //public struct ImGUITimeHelper
    //{
    //    private static Stopwatch stopWatch = Stopwatch.StartNew();

    //    private float deltaTime;
    //    public double LastTime;
    //    public EventType TrackingEvent;
    //    public bool ThrowExceptionOnBadUsage;

    //    public float DeltaTime
    //    {
    //        get
    //        {
    //            if (this.ThrowExceptionOnBadUsage && (Event.current == null || Event.current.type != this.TrackingEvent))
    //            {
    //                throw new Exception($"DeltaTime may only be used during {this.TrackingEvent} events.");
    //            }

    //            return this.deltaTime;
    //        }
    //    }

    //    public static ImGUITimeHelper Create(EventType trackingEvent, bool throwExceptionOnBadUsage = true)
    //    {
    //        return new ImGUITimeHelper()
    //        {
    //            deltaTime = 0.02f,
    //            LastTime = stopWatch.Elapsed.TotalSeconds,
    //            TrackingEvent = trackingEvent,
    //            ThrowExceptionOnBadUsage = throwExceptionOnBadUsage,
    //        };
    //    }

    //    public void Update()
    //    {
    //        if (this.TrackingEvent == default(EventType) || Event.current.type == this.TrackingEvent)
    //        {
    //            var time = stopWatch.Elapsed.TotalSeconds;
    //            var newDeltaTime = (float)(time - this.LastTime);

    //            if (newDeltaTime <= 0.2f)
    //            {
    //                this.deltaTime = newDeltaTime;
    //            }

    //            this.LastTime = time;
    //        }
    //    }
    //}
}
#endif