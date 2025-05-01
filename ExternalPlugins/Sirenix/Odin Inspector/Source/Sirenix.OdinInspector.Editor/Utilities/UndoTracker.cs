//-----------------------------------------------------------------------
// <copyright file="UndoTracker.cs" company="Sirenix ApS">
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

    using Sirenix.OdinInspector.Editor.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    public class UndoTracker : SessionSingletonSO<UndoTracker>
    {
        public int CurrentIndex;

        private static int LastSeenUndoGroup;

        public static event Action<UndoPropertyModificationGroup[]> OnObjectValueModified;
        public static event Action<List<UnityEngine.Object>> OnUndoPerformed;
        public static event Action<List<UnityEngine.Object>> OnRedoPerformed;

        static UndoTracker()
        {
            Undo.postprocessModifications += PostProcessModifications;
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        private static List<UnityEngine.Object> GetUndoGroupObjs(int index)
        {
            List<UnityEngine.Object> objs;

            if (!UndoTrackerStateContainer.Instance.UndoGroups.TryGetValue(index, out objs))
            {
                objs = new List<UnityEngine.Object>();
                UndoTrackerStateContainer.Instance.UndoGroups.Add(index, objs);
            }

            return objs;
        }

        private static UndoPropertyModification[] PostProcessModifications(UndoPropertyModification[] modifications)
        {
            for (int i = 0; i < modifications.Length; i++)
            {
                var mod = modifications[i];

                var value = mod.currentValue ?? mod.previousValue;

                if (value == null) continue;

                if (value.target == Instance ||
                    value.target == UndoTrackerStateContainer.Instance ||
                    (value.target.GetType() == typeof(Transform) && value.propertyPath == "m_RootOrder"))
                {
                    return modifications;
                }
            }

            try
            {
                var group = Undo.GetCurrentGroup();

                if (group != LastSeenUndoGroup)
                {
                    var toRemove = new List<int>();

                    foreach (var key in UndoTrackerStateContainer.Instance.UndoGroups.Keys)
                    {
                        if (key > Instance.CurrentIndex)
                        {
                            toRemove.Add(key);
                        }
                    }

                    foreach (var key in toRemove)
                    {
                        UndoTrackerStateContainer.Instance.UndoGroups.Remove(key);
                    }

                    Undo.RecordObject(Instance, Undo.GetCurrentGroupName());
                    Instance.CurrentIndex++;
                    Undo.FlushUndoRecordObjects();

                    LastSeenUndoGroup = group;

                }

                var groupMods = GetUndoGroupObjs(Instance.CurrentIndex);
                UndoTrackerStateContainer.Instance.LatestIndex = Instance.CurrentIndex;

                for (int i = 0; i < modifications.Length; i++)
                {
                    var mod = modifications[i];

                    var value = mod.currentValue ?? mod.previousValue;

                    if (value == null) continue;

                    if (!groupMods.Contains(value.target))
                    {
                        groupMods.Add(value.target);
                    }
                }

                try
                {
                    if (OnObjectValueModified != null)
                    {
                        var modificationsGrouped = modifications
                            .Where(x => x.currentValue != null || x.previousValue != null)
                            .GroupBy(x => (x.currentValue ?? x.previousValue).target)
                            .Select(x => new UndoPropertyModificationGroup()
                        {
                            Target = x.Key,
                            Modifications = x.ToArray()
                        }).ToArray();

                        OnObjectValueModified(modificationsGrouped);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return modifications;
        }

        private static void UndoRedoPerformed()
        {
            var latestIndex = UndoTrackerStateContainer.Instance.LatestIndex;
            var currentIndex = Instance.CurrentIndex;

            UndoTrackerStateContainer.Instance.LatestIndex = currentIndex;

            if (currentIndex < latestIndex)
            {
                // Undo
                if (OnUndoPerformed != null)
                {
                    foreach (var group in UndoTrackerStateContainer.Instance.UndoGroups.Where(n => n.Key > currentIndex && n.Key <= latestIndex).OrderByDescending(n => n.Key))
                    {
                        try
                        {
                            OnUndoPerformed(group.Value);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }
            else if (currentIndex > latestIndex)
            {
                // Redo
                if (OnRedoPerformed != null)
                {
                    foreach (var group in UndoTrackerStateContainer.Instance.UndoGroups.Where(n => n.Key <= currentIndex && n.Key > latestIndex).OrderBy(n => n.Key))
                    {
                        try
                        {
                            OnRedoPerformed(group.Value);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }
        }
        public struct UndoPropertyModificationGroup
        {
            public UnityEngine.Object Target;
            public UndoPropertyModification[] Modifications;
        }
    }
}
#endif