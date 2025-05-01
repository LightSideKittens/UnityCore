//-----------------------------------------------------------------------
// <copyright file="OdinPrefabUtility.cs" company="Sirenix ApS">
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

    public static class OdinPrefabUtility
    {
        private static readonly ExpressionFunc<UnityEngine.GameObject, bool> isPartOfPrefabStage;
        private static readonly ExpressionFunc<UnityEngine.GameObject, string> assetPathOfPrefabStage;
        private static readonly ExpressionFunc<UnityEngine.GameObject, UnityEngine.GameObject> prefabContentsRoot;
        private static readonly ExpressionFunc<string, GameObject, GameObject> openPrefabStage;

        static OdinPrefabUtility()
        {
            var prefabStageUtilityType = Sirenix.Serialization.TwoWaySerializationBinder.Default.BindToType("UnityEditor.SceneManagement.PrefabStageUtility")
                                         ?? Sirenix.Serialization.TwoWaySerializationBinder.Default.BindToType("UnityEditor.Experimental.SceneManagement.PrefabStageUtility");

            var prefabStageType = Sirenix.Serialization.TwoWaySerializationBinder.Default.BindToType("UnityEditor.SceneManagement.PrefabStage")
                                  ?? Sirenix.Serialization.TwoWaySerializationBinder.Default.BindToType("UnityEditor.Experimental.SceneManagement.PrefabStage");

            string e;

            isPartOfPrefabStage = ExpressionUtility.ParseFunc<UnityEngine.GameObject, bool>("GetPrefabStage($0) != null", true, prefabStageUtilityType, out e);
            if (e != null) Debug.LogError(e);

            if (prefabStageType.GetProperty("assetPath") != null)
            {
                assetPathOfPrefabStage = ExpressionUtility.ParseFunc<UnityEngine.GameObject, string>("GetPrefabStage($0).assetPath", true, prefabStageUtilityType, out e);
                if (e != null) Debug.LogError(e);
            }
            else
            {
                assetPathOfPrefabStage = ExpressionUtility.ParseFunc<UnityEngine.GameObject, string>("GetPrefabStage($0).prefabAssetPath", true, prefabStageUtilityType, out e);
                if (e != null) Debug.LogError(e);
            }

            prefabContentsRoot = ExpressionUtility.ParseFunc<UnityEngine.GameObject, UnityEngine.GameObject>("GetPrefabStage($0).prefabContentsRoot", true, prefabStageUtilityType, out e);
            if (e != null) Debug.LogError(e);

            openPrefabStage = ExpressionUtility.ParseFunc<string, UnityEngine.GameObject, GameObject>("OpenPrefab($0, $1).prefabContentsRoot", true, prefabStageUtilityType, out e);
            if (e != null) Debug.LogError(e);
        }

        public static GameObject OpenPrefabStage(string prefabAssetPath, GameObject openedFromInstance)
        {
            return openPrefabStage.Invoke(prefabAssetPath, openedFromInstance);
        }

        public static PrefabKind GetPrefabKind(InspectorProperty prop)
        {
            var obj = prop.Tree.WeakTargets[0] as UnityEngine.Object;
            return GetPrefabKind(obj);
        }

        internal static bool IsPartOfPrefabStage(GameObject obj) => isPartOfPrefabStage(obj);

        public static GameObject GetNearestPrefabAsset(UnityEngine.Object obj)
        {
            if (obj && (obj is Component || obj is GameObject))
            {
                var go = (obj is Component cmp) ? cmp.gameObject : (GameObject)obj;

                if (PrefabUtility.IsPartOfAnyPrefab(obj) || isPartOfPrefabStage(go))
                {
                    var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);

                    if (string.IsNullOrEmpty(path) && isPartOfPrefabStage(go))
                    {
                        path = assetPathOfPrefabStage(go);
                    }

                    if (!string.IsNullOrEmpty(path))
                    {
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        return prefab;
                    }
                }
            }

            return null;
        }

        public static PrefabKind GetPrefabKind(UnityEngine.Object obj)
        {
            if (obj && (obj is Component || obj is GameObject))
            {
                var go = (obj is Component cmp) ? cmp.gameObject : (GameObject)obj;
                if (isPartOfPrefabStage(go))
                {
                    if (PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.NotAPrefab)
                    {
                        var status = PrefabUtility.GetPrefabInstanceStatus(obj);
                        if (status == PrefabInstanceStatus.NotAPrefab)
                        {
                            return PrefabKind.Regular;
                        }
                    }

                    var nearest = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);

                    if (nearest == null || nearest == prefabContentsRoot(go))
                    {
                        var path = assetPathOfPrefabStage(go);
                        var stagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        var stagePrefabType = PrefabUtility.GetPrefabAssetType(stagePrefab);
                        switch (stagePrefabType)
                        {
                            case PrefabAssetType.Model:
                            case PrefabAssetType.Regular:
                            return PrefabKind.Regular;
                            case PrefabAssetType.Variant:
                            return PrefabKind.Variant;
                        }
                    }
                    else
                    {
                        return PrefabKind.InstanceInPrefab;
                    }
                }
                else
                {
                    var kind = PrefabUtility.GetPrefabInstanceStatus(obj);

                    if (kind == PrefabInstanceStatus.NotAPrefab)
                    {
                        var type = PrefabUtility.GetPrefabAssetType(obj);
                        switch (type)
                        {
                            case PrefabAssetType.Model:
                            case PrefabAssetType.Regular:
                            return PrefabKind.Regular;
                            case PrefabAssetType.Variant:
                            if (PrefabUtility.IsPartOfPrefabInstance(obj))
                                return PrefabKind.Variant;

                            return PrefabKind.Regular;
                        }

                        return PrefabKind.NonPrefabInstance;
                    }
                    else if (kind == PrefabInstanceStatus.Connected)
                    {
                        var nearest = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
                        var inPrefab = nearest != go ? PrefabKind.InstanceInPrefab : PrefabKind.None;
                        var type = PrefabUtility.GetPrefabAssetType(obj);
                        switch (type)
                        {
                            case PrefabAssetType.Model:
                            case PrefabAssetType.Regular:
                            case PrefabAssetType.Variant:
                            return PrefabKind.InstanceInScene | inPrefab;
                        }

#if SIRENIX_INTERNAL
                        throw new NotImplementedException(type + "");
#endif
                    }
                }
            }

            return PrefabKind.None;
        }

        public static void UpdatePrefabInstancePropertyModifications(UnityEngine.Object prefabInstance, bool withUndo)
        {
            if (prefabInstance == null) throw new ArgumentNullException("prefabInstance");
            if (!(prefabInstance is ISupportsPrefabSerialization)) throw new ArgumentException("Type must implement ISupportsPrefabSerialization");
            if (!(prefabInstance is ISerializationCallbackReceiver)) throw new ArgumentException("Type must implement ISerializationCallbackReceiver");
            if (!OdinPrefabSerializationEditorUtility.ObjectIsPrefabInstance(prefabInstance)) throw new ArgumentException("Value must be a prefab instance");

            Action action = null;

            EditorApplication.HierarchyWindowItemCallback hierarchyCallback = (arg1, arg2) => action();
            EditorApplication.ProjectWindowItemCallback projectCallback = (arg1, arg2) => action();
            Action<SceneView> sceneCallback = (arg) => action();

            EditorApplication.hierarchyWindowItemOnGUI += hierarchyCallback;
            EditorApplication.projectWindowItemOnGUI += projectCallback;
            SceneView.duringSceneGui += sceneCallback;

            action = () =>
            {
                EditorApplication.hierarchyWindowItemOnGUI -= hierarchyCallback;
                EditorApplication.projectWindowItemOnGUI -= projectCallback;
                SceneView.duringSceneGui -= sceneCallback;

                // Clear out pre-existing modifications, as they can actually mess this up
                {
                    ISupportsPrefabSerialization supporter = (ISupportsPrefabSerialization)prefabInstance;

                    if (supporter.SerializationData.PrefabModifications != null)
                    {
                        supporter.SerializationData.PrefabModifications.Clear();
                    }

                    if (supporter.SerializationData.PrefabModificationsReferencedUnityObjects != null)
                    {
                        supporter.SerializationData.PrefabModificationsReferencedUnityObjects.Clear();
                    }

                    UnitySerializationUtility.PrefabModificationCache.CachePrefabModifications(prefabInstance, new List<PrefabModification>());
                }

                try
                {
                    if (prefabInstance == null)
                    {
                        // Ignore - the object has been destroyed since the method was invoked.
                        return;
                    }

                    if (Event.current == null) throw new InvalidOperationException("Delayed property modification delegate can only be called during the GUI event loop; Event.current must be accessible.");

                    try
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(prefabInstance);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Exception occurred while calling Unity's PrefabUtility.RecordPrefabInstancePropertyModifications:");
                        Debug.LogException(ex);
                    }

                    var tree = PropertyTree.Create(prefabInstance);

                    tree.DrawMonoScriptObjectField = false;

                    bool isRepaint = Event.current.type == EventType.Repaint;

                    if (!isRepaint)
                    {
                        GUIHelper.PushEventType(EventType.Repaint);
                    }

                    tree.BeginDraw(withUndo);

                    foreach (var property in tree.EnumerateTree(true))
                    {
                        if (property.ValueEntry == null) continue;
                        if (!property.SupportsPrefabModifications) continue;

                        property.Update(true);

                        if (!(property.ChildResolver is IKeyValueMapResolver)) continue;

                        if (property.ValueEntry.DictionaryChangedFromPrefab)
                        {
                            tree.PrefabModificationHandler.RegisterPrefabDictionaryDeltaModification(property, 0);
                        }
                        else
                        {
                            var prefabProperty = tree.PrefabModificationHandler.PrefabPropertyTree.GetPropertyAtPath(property.Path);
                            if (prefabProperty == null) continue;
                            if (prefabProperty.ValueEntry == null) continue;
                            if (!property.SupportsPrefabModifications) continue;
                            if (!(property.ChildResolver is IKeyValueMapResolver)) continue;

                            tree.PrefabModificationHandler.RegisterPrefabDictionaryDeltaModification(property, 0);
                        }
                    }

                    tree.EndDraw();

                    if (!isRepaint)
                    {
                        GUIHelper.PopEventType();
                    }

                    ISerializationCallbackReceiver receiver = (ISerializationCallbackReceiver)prefabInstance;
                    receiver.OnBeforeSerialize();
                    receiver.OnAfterDeserialize();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            };

            foreach (SceneView scene in SceneView.sceneViews)
            {
                scene.Repaint();
            }
        }
    }
}
#endif