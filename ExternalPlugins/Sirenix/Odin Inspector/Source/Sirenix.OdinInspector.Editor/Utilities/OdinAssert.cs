//-----------------------------------------------------------------------
// <copyright file="OdinAssert.cs" company="Sirenix ApS">
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

    using Sirenix.Utilities.Editor.Expressions;
    using Sirenix.Serialization;
    using Sirenix.Utilities.Editor;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System;

    internal class OdinAssert
    {
        [System.Diagnostics.Conditional("SIRENIX_INTERNAL")]
        public static void Log(object message)
        {
            Debug.Log(message);
        }

        [System.Diagnostics.Conditional("SIRENIX_INTERNAL")]
        public static void LogError(object message)
        {
            Debug.LogError(message);
        }

        [System.Diagnostics.Conditional("SIRENIX_INTERNAL")]
        public static void Assert(bool condition)
        {
            Debug.Assert(condition);
            if (!condition)
                Debug.DebugBreak();
        }

        [System.Diagnostics.Conditional("SIRENIX_INTERNAL")]
        public static void Assert(bool condition, UnityEngine.Object context)
        {
            Debug.Assert(condition, context);
            if (!condition)
                Debug.DebugBreak();
        }

        [System.Diagnostics.Conditional("SIRENIX_INTERNAL")]
        public static void Assert(bool condition, object message)
        {
            Debug.Assert(condition, message);
            if (!condition)
                Debug.DebugBreak();
        }

        [System.Diagnostics.Conditional("SIRENIX_INTERNAL")]
        public static void Assert(bool condition, string message)
        {
            Debug.Assert(condition, message);
            if (!condition)
                Debug.DebugBreak();
        }

        [System.Diagnostics.Conditional("SIRENIX_INTERNAL")]
        public static void Assert(bool condition, object message, UnityEngine.Object context)
        {
            Debug.Assert(condition, message, context);
            if (!condition)
                Debug.DebugBreak();
        }

        [System.Diagnostics.Conditional("SIRENIX_INTERNAL")]
        public static void Assert(bool condition, string message, UnityEngine.Object context)
        {
            Debug.Assert(condition, message, context);
            if (!condition)
                Debug.DebugBreak();
        }
    }
}
#endif