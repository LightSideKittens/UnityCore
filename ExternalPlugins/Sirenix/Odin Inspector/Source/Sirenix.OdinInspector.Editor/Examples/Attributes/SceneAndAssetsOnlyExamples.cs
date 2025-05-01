//-----------------------------------------------------------------------
// <copyright file="SceneAndAssetsOnlyExamples.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;
    using System.Collections.Generic;
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [AttributeExample(typeof(AssetsOnlyAttribute))]
    [AttributeExample(typeof(SceneObjectsOnlyAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic" })]
    internal class SceneAndAssetsOnlyExamples
    {
        [Title("Assets only")]
        [AssetsOnly]
        public List<GameObject> OnlyPrefabs;

        [AssetsOnly]
        public GameObject SomePrefab;

        [AssetsOnly]
        public Material MaterialAsset;

        [AssetsOnly]
        public MeshRenderer SomeMeshRendererOnPrefab;

        [Title("Scene Objects only")]
        [SceneObjectsOnly]
        public List<GameObject> OnlySceneObjects;

        [SceneObjectsOnly]
        public GameObject SomeSceneObject;

        [SceneObjectsOnly]
        public MeshRenderer SomeMeshRenderer;
    }
}
#endif