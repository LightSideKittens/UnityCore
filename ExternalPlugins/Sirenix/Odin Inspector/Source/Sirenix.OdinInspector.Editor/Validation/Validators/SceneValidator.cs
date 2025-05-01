//-----------------------------------------------------------------------
// <copyright file="SceneValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;


    public abstract class SceneValidator : IValidator
    {
        private static Func<Type, bool, IEnumerable<UnityEngine.Object>> FindComponentsOfType;
        private bool initialized;
        private List<GameObject> sceneRoots;

        static SceneValidator()
        {
            var findObjectsOfType = default(MethodInfo);

            if (UnityVersion.Major > 2019)
            {
                var methods = typeof(UnityEngine.Object).GetMethods();
                foreach (var methodInfo in methods)
                {
                    if (!methodInfo.IsPublic || methodInfo.IsGenericMethod || !methodInfo.IsStatic)
                        continue;

                    if (methodInfo.Name == "FindObjectsOfType") // Non generic version of FindObjectsOfType(typeof(X), includeInactive);
                    {
                        var parms = methodInfo.GetParameters();
                        if (parms.Length == 2 && parms[0].ParameterType == typeof(Type) && parms[1].ParameterType == typeof(bool))
                        {
                            findObjectsOfType = methodInfo;
                            break;
                        }
                    }
                }
            }

            if (findObjectsOfType == null)
            {
                FindComponentsOfType = (t, b) => FindComponentsOfTypeFallback(t, b);
            }
            else
            {
                var methodDel = (Func<Type, bool, UnityEngine.Object[]>)Delegate.CreateDelegate(typeof(Func<Type, bool, UnityEngine.Object[]>), findObjectsOfType);
                FindComponentsOfType = (t, b) => methodDel(t, b);
            }
        }

        static IEnumerable<UnityEngine.Object> FindComponentsOfTypeFallback(Type t, bool includeInactive)
        {
            var objects = Resources.FindObjectsOfTypeAll(t);
            foreach (var o in objects)
            {
                if (o is Component cmp && (o.hideFlags & HideFlags.DontSave) == 0 && cmp.gameObject.scene.IsValid())
                {
                    yield return o;
                }
            }
        }

        public SceneReference ValidatedScene { get; private set; }

        public void Initialize(SceneReference scene)
        {
            if (this.initialized) throw new Exception("Can't initialize a scene validator twice!");
            this.initialized = true;

            this.ValidatedScene = scene;

            this.Initialize();
        }

        public List<GameObject> GetSceneRoots()
        {
            this.LoadSceneIfNotLoaded();

            if (!this.ValidatedScene.IsValid || !this.ValidatedScene.IsLoaded)
            {
                return new List<GameObject>();
            }

            if (this.sceneRoots == null) this.sceneRoots = new List<GameObject>();
            else this.sceneRoots.Clear();

            Scene scene;

            if (this.ValidatedScene.TryGetScene(out scene))
            {
                this.sceneRoots.AddRange(SceneUtilities.GetSceneRoots(scene));
            }

            return this.sceneRoots;
        }

        public GameObject GetSceneRoot(string name)
        {
            foreach (var root in this.GetSceneRoots())
            {
                if (root.name == name) return root;
            }

            return null;
        }

        public IEnumerable<T> FindAllComponentsInSceneOfType<T>(bool includeInactive = true)
            where T : Component
        {
            var objects = FindComponentsOfType(typeof(T), includeInactive);

            foreach (var o in objects)
            {
                if (o is T cmp)
                {
                    if (cmp.gameObject.scene.path == this.ValidatedScene.Path)
                    {
                        yield return cmp;
                    }
                }
            }
        }

        public T FindComponentInSceneOfType<T>(bool includeInactive = true)
            where T : Component
        {
            foreach (var cmp in FindAllComponentsInSceneOfType<T>(includeInactive))
            {
                return cmp;
            }

            return null;
        }

        public IEnumerable<GameObject> GetAllGameObjectsInScene()
        {
            foreach (var root in this.GetSceneRoots())
            {
                yield return root;

                foreach (var child in GetChildren(root.transform))
                {
                    yield return child.gameObject;
                }
            }
        }

        private static IEnumerable<Transform> GetChildren(Transform transform)
        {
            var childCount = transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);

                yield return child;

                foreach (var subChild in GetChildren(child))
                {
                    yield return subChild;
                }
            }
        }

        public GameObject GetGameObjectAtPath(string path, char pathSeparator = '/')
        {
            var steps = path.Split(pathSeparator);
            return GetGameObjectAtPath(steps);
        }

        public GameObject GetGameObjectAtPath(IList<string> path)
        {
            if (path.Count == 0) return null;
            GameObject root = this.GetSceneRoot(path[0]);

            if (root == null) return null;

            var current = root.transform;

            for (int i = 1; i < path.Count; i++)
            {
                var childCount = current.childCount;

                Transform next = null;

                for (int j = 0; j < childCount; j++)
                {
                    var child = current.GetChild(j);

                    if (child.name == path[i])
                    {
                        next = child;
                        break;
                    }
                }

                if (next == null) return null;
                current = next;
            }

            return current.gameObject;
        }

        public T GetComponentAtPath<T>(string path, char pathSeparator = '/') where T : Component
        {
            var go = GetGameObjectAtPath(path, pathSeparator);
            if (go != null) return go.GetComponent<T>();
            return null;
        }

        public T GetComponentAtPath<T>(IList<string> path) where T : Component
        {
            var go = GetGameObjectAtPath(path);
            if (go != null) return go.GetComponent<T>();
            return null;
        }

        public void LoadSceneIfNotLoaded(bool askToSave = true)
        {
            if (this.ValidatedScene.IsLoaded) return;
            if (!this.ValidatedScene.IsValid) return;

            Scene scene;

            if (!this.ValidatedScene.TryGetScene(out scene))
            {
                return;
            }

            if (askToSave)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            // Debug.Log("path: " + scene.path);
            // Debug.Log("name: " + scene.name);
            // Debug.Log("buildIndex: " + scene.buildIndex);
            // Debug.Log("IsValid(): " + scene.IsValid());
            // Debug.Log("rootCount: " + scene.rootCount);

            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
        }

        protected virtual void Initialize()
        {
        }

        RevalidationCriteria IValidator.RevalidationCriteria
        {
            get { return RevalidationCriteria.OnValueChange; }
        }

        public virtual bool CanValidateScene(SceneReference scene)
        {
            Scene unityScene;
            return scene.TryGetScene(out unityScene) && unityScene.IsValid();
        }

        public void RunValidation(ref ValidationResult result)
        {
            if (result == null)
                result = new ValidationResult();

            result.Setup = new ValidationSetup()
            {
                Validator = this,
                Root = this.ValidatedScene,
            };

            result.Path = this.ValidatedScene.Path;
            result.ResultType = ValidationResultType.Valid;
            result.Message = "";

            try
            {
                this.Validate(result);
            }
            catch (Exception ex)
            {
                while (ex is TargetInvocationException)
                {
                    ex = ex.InnerException;
                }

                result.ResultType = ValidationResultType.Error;
                result.Message = "An exception was thrown during validation: " + ex.ToString();
            }
        }

        protected abstract void Validate(ValidationResult result);
    }

    public static class SceneUtilities
    {
        // TODO: Remove reflection and call it directly when we're all on a later version of Unity.
        private static readonly MethodInfo Scene_GetRootGameObjects_Method = typeof(Scene).GetMethod("GetRootGameObjects", Flags.InstancePublic, null, Type.EmptyTypes, null);

        private static PropertyInfo loadedSceneCount;

        static SceneUtilities()
        {
            loadedSceneCount =
                typeof(UnityEngine.SceneManagement.SceneManager).GetProperty("loadedSceneCount", Flags.StaticPublicDeclaredOnly) ??
                typeof(UnityEditor.SceneManagement.EditorSceneManager).GetProperty("loadedSceneCount", Flags.StaticPublicDeclaredOnly);

            if (loadedSceneCount == null)
            {
                Debug.LogError(
                    "Unity's SceneManager.loadedSceneCount and EditorSceneManager.loadedSceneCount properties have both " +
                    "been renamed or removed in this version of Unity (" + Application.unityVersion + "). Odin will not " +
                    "be able to determine how many scenes are loaded, which may cause UI issues at times, such as the " +
                    "Validator being unable to warn you about new, unsaved scenes not being validated. Please update Odin, " +
                    "or inform the developers of this issue if you are on the latest version of Odin. You are currently " +
                    "on Odin version " + OdinInspectorVersion.Version + ".");
            }
        }

        public static int GetLoadedSceneCount()
        {
            if (loadedSceneCount == null)
                return 0;

            return (int)loadedSceneCount.GetValue(null);
        }

        public static IEnumerable<GameObject> GetSceneRoots(Scene scene)
        {
            if (Scene_GetRootGameObjects_Method != null && scene.IsValid())
            {
                var roots = (GameObject[])Scene_GetRootGameObjects_Method.Invoke(scene, null);

                foreach (var root in roots)
                    yield return root;
            }
            else
            {
                // Fallback; only works in Unity versions without multi-scene support
                var prop = new HierarchyProperty(HierarchyType.GameObjects);
                var expanded = new int[0];

                while (prop.Next(expanded))
                {
                    yield return prop.pptrValue as GameObject;
                }
            }
        }


    }
}
#endif