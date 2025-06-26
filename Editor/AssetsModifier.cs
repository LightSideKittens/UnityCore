using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace LSCore.Editor
{
    public class AssetsModifier : OdinEditorWindow
    {
        [Serializable]
        public abstract class BaseModifier
        {
            public virtual string Filter { get; }
            public List<Object> objects = new();
            
            protected string[] assetPaths;

            public void Modify()
            {
                var folderPaths = objects.Select(AssetDatabase.GetAssetPath).Where(AssetDatabase.IsValidFolder).ToArray();
                var iter = objects.Select(AssetDatabase.GetAssetPath).Where(AssetDatabaseUtils.IsAssetPath);
                if (folderPaths is { Length: > 0 })
                {
                    iter = iter.Concat(AssetDatabase.FindAssets(Filter, folderPaths).Select(AssetDatabase.GUIDToAssetPath));
                }
                assetPaths = iter.ToArray();
                OnModify();
            }
            
            protected abstract void OnModify();
        }
        
        [Serializable]
        public abstract class BaseGameObjectsModifier : BaseModifier
        {
            public override string Filter => "t:Scene t:Prefab";

            protected virtual bool OnEnter(List<GameObject> roots, out bool needBreak)
            {
                needBreak = false;
                return false;
            }
            
            protected sealed override void OnModify()
            {
                foreach (string path in assetPaths)
                {
                    if (!IsEditableAsset(path))
                    {
                        continue;
                    }

                    List<GameObject> roots = new();
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    Scene scene = default;
                    bool sceneDirty = false;

                    if (prefab != null)
                    {
                        roots.Add(prefab);
                    }
                    else
                    {
                        scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                        roots.AddRange(scene.GetRootGameObjects());
                    }

                    sceneDirty |= OnEnter(roots, out var needBreak);

                    if (!needBreak)
                    {
                        var gos = roots.SelectMany(x => x.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject)).ToList();
                        foreach (var go in gos)
                        {
                            if(go == null) continue;
                            sceneDirty |= Modify(go, out needBreak);
                            if (needBreak)
                            {
                                break;
                            }
                        }
                    }

                    if (sceneDirty)
                    {
                        if (prefab != null)
                        {
                            PrefabUtility.SavePrefabAsset(prefab);
                            Debug.Log($"Updated prefab: {path}");
                        }
                        else
                        {
                            EditorSceneManager.MarkSceneDirty(scene);
                            EditorSceneManager.SaveScene(scene);
                            Debug.Log($"Updated scene: {path}");
                        }
                    }
                }
            }
            
            protected abstract bool Modify(GameObject go, out bool needBreak);
        }
        
        [SerializeReference] public BaseModifier[] modifiers;
        
        [Button]
        public void Modify()
        {
            for (int i = 0; i < modifiers.Length; i++)
            {
                modifiers[i].Modify();
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem(LSPaths.Windows.AssetsModifier)]
        private static void OpenWindow() => GetWindow<AssetsModifier>().Show();

        private static bool IsPartOfAnotherPrefab(GameObject go) => PrefabUtility.IsPartOfPrefabInstance(go);

        private static bool IsEditableAsset(string path) => path.StartsWith("Assets/");
    }
}