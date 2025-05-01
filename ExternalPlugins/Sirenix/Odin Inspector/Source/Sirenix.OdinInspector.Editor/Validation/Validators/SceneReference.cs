//-----------------------------------------------------------------------
// <copyright file="SceneReference.cs" company="Sirenix ApS">
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

    using System;
    using System.IO;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public struct SceneReference : IEquatable<SceneReference>
    {
        public bool IsValid;
        public string GUID;

        public static readonly SceneReference Invalid;

        public SceneReference(Scene scene)
        {
            if (!scene.IsValid())
            {
                this.IsValid = false;
                this.GUID = null;
            }
            else
            {
                var guid = AssetDatabase.AssetPathToGUID(scene.path);

                if (string.IsNullOrEmpty(guid))
                {
                    this.IsValid = false;
                    this.GUID = null;
                }
                else
                {
                    this.IsValid = true;
                    this.GUID = guid;
                }
            }
        }

        public SceneReference(string guid)
        {
            this.GUID = guid;
            this.IsValid = true;
        }

        public string Name
        {
            get
            {
                if (!this.IsValid) return "";
                var path = this.Path;
                if (string.IsNullOrEmpty(path)) return "";
                return System.IO.Path.GetFileNameWithoutExtension(path);
            }
        }

        public string Path { get { return this.IsValid ? AssetDatabase.GUIDToAssetPath(this.GUID) : ""; } }

        public bool IsLoaded
        {
            get
            {
                if (!this.IsValid) return false;

                //if (Application.isPlaying)
                {
                    return SceneManager.GetSceneByPath(this.Path).isLoaded;
                }

                //SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();

                //for (int i = 0; i < setup.Length; i++)
                //{
                //    if (setup[i].isLoaded && setup[i].path == this.Path) return true;
                //}

                //return false;
            }
        }

        public bool IsActive
        {
            get
            {
                if (!this.IsValid) return false;
                SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();

                for (int i = 0; i < setup.Length; i++)
                {
                    if (setup[i].isActive && setup[i].path == this.Path) return true;
                }

                return false;
            }
        }

        public bool TryOpenScene(OpenSceneMode mode, out Scene scene)
        {
            if (!File.Exists(this.Path))
            {
                scene = default(Scene);
                return false;
            }

            try
            {
                scene = EditorSceneManager.OpenScene(this.Path, mode);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                scene = default(Scene);
                return false;
            }
        }

        public bool IsInBuildSettings(bool mustBeEnabled = false)
        {
            var buildScenes = EditorBuildSettings.scenes;

            for (int i = 0; i < buildScenes.Length; i++)
            {
                var scene = buildScenes[i];

                if (scene.path == this.Path)
                {
                    if (mustBeEnabled && !scene.enabled) continue;
                    return true;
                }
            }

            return false;
        }

        public SceneAsset GetSceneAsset()
        {
            if (!this.IsValid) return null;
            return AssetDatabase.LoadMainAssetAtPath(this.Path) as SceneAsset;
        }

        public override bool Equals(object obj)
        {
            if (obj is SceneReference)
            {
                return this.Equals((SceneReference)obj);
            }

            return false;
        }

        public bool Equals(SceneReference other)
        {
            return this.IsValid == other.IsValid &&
                   this.GUID == other.GUID;
        }

        public override int GetHashCode()
        {
            int hashCode = -552061963;
            hashCode = hashCode * -1521134295 + this.IsValid.GetHashCode();
            hashCode = hashCode * -1521134295 + (this.GUID ?? "").GetHashCode();
            return hashCode;
        }

        public static bool operator ==(SceneReference a, SceneReference b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SceneReference a, SceneReference b)
        {
            return !(a == b);
        }

        public bool TryGetScene(out Scene scene)
        {
            if (!this.IsValid)
            {
                scene = default(Scene);
                return false;
            }

            scene = EditorSceneManager.GetSceneByPath(this.Path);

            if (scene.IsValid()) return true;

            if (this.GUID != null)
            {
                var path = AssetDatabase.GUIDToAssetPath(this.GUID);

                if (path != null)
                {
                    scene = EditorSceneManager.GetSceneByPath(path);
                    if (scene.IsValid()) return true;
                }
            }

            scene = default(Scene);
            return false;
        }

        public static SceneReference FromPath(string path)
        {
            var guid = AssetDatabase.AssetPathToGUID(path);

            if (string.IsNullOrEmpty(guid))
            {
                return new SceneReference()
                {
                    IsValid = false,
                };
            }

            return new SceneReference(guid);
        }

        public static SceneReference FromAsset(SceneAsset asset)
        {
            if (asset == null) return Invalid;

            var path = AssetDatabase.GetAssetPath(asset);
            var guid = AssetDatabase.AssetPathToGUID(path);

            if (string.IsNullOrEmpty(guid))
            {
                return new SceneReference()
                {
                    IsValid = false,
                };
            }

            return new SceneReference(guid);
        }
    }
}
#endif