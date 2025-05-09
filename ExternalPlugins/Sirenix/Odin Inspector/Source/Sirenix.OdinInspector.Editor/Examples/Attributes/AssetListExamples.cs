//-----------------------------------------------------------------------
// <copyright file="AssetListExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(AssetListAttribute), "The AssetList attribute works on both lists of UnityEngine.Object types, and directly on UnityEngine.Object types, but has different behaviour for each case.")]
	[ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic" })]
    internal class AssetListExamples
    {
        [AssetList]
        [PreviewField(70, ObjectFieldAlignment.Center)]
        public Texture2D SingleObject;

        [AssetList(Path = "/Plugins/Sirenix/")]
        public List<ScriptableObject> AssetList;

        [FoldoutGroup("Filtered Odin ScriptableObjects", expanded: false)]
        [AssetList(Path = "Plugins/Sirenix/")]
        public ScriptableObject Object;

        [AssetList(AutoPopulate = true, Path = "Plugins/Sirenix/")]
        [FoldoutGroup("Filtered Odin ScriptableObjects", expanded: false)]
        public List<ScriptableObject> AutoPopulatedWhenInspected;

        [AssetList(LayerNames = "MyLayerName")]
        [FoldoutGroup("Filtered AssetLists examples")]
        public GameObject[] AllPrefabsWithLayerName;

        [AssetList(AssetNamePrefix = "Rock")]
        [FoldoutGroup("Filtered AssetLists examples")]
        public List<GameObject> PrefabsStartingWithRock;

        [FoldoutGroup("Filtered AssetLists examples")]
        [AssetList(Tags = "MyTagA, MyTabB", Path = "/Plugins/Sirenix/")]
        public List<GameObject> GameObjectsWithTag;

        [FoldoutGroup("Filtered AssetLists examples")]
        [AssetList(CustomFilterMethod = "HasRigidbodyComponent")]
        public List<GameObject> MyRigidbodyPrefabs;

        private bool HasRigidbodyComponent(GameObject obj)
        {
            return obj.GetComponent<Rigidbody>() != null;
        }
    }
}
#endif