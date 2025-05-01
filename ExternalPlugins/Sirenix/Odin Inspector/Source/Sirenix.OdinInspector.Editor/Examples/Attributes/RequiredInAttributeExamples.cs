//-----------------------------------------------------------------------
// <copyright file="RequiredInAttributeExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(RequiredInAttribute), "Required members in the inspector when edited from regular prefab assets, prefab variants, nested prefabs, prefab instances or non prefab instances.")]
    [TypeInfoBox("Note that this drawn example does not represent a GameObject, so the attribute is not shown correctly in the Attribute Overview window. If you copy it to a script, for example via the Save Component Script button, the example will display correctly when you put it on GameObjects in various kinds of prefabs.")]
    internal class RequiredInAttributeExamples
    {
        [RequiredIn(PrefabKind.InstanceInScene, ErrorMessage = "Error messages can be customized. Odin expressions is supported.")]
        public string InstanceInScene = "Instances of prefabs in scenes";

        [RequiredIn(PrefabKind.InstanceInPrefab)]
        public string InstanceInPrefab = "Instances of prefabs nested inside other prefabs";

        [RequiredIn(PrefabKind.Regular)]
        public string Regular = "Regular prefab assets";

        [RequiredIn(PrefabKind.Variant)]
        public string Variant = "Prefab variant assets";

        [RequiredIn(PrefabKind.NonPrefabInstance)]
        public string NonPrefabInstance = "Non-prefab component or gameobject instances in scenes";

        [RequiredIn(PrefabKind.PrefabInstance)]
        public string PrefabInstance = "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";

        [RequiredIn(PrefabKind.PrefabAsset)]
        public string PrefabAsset = "Prefab assets and prefab variant assets";

        [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
        public string PrefabInstanceAndNonPrefabInstance = "Prefab Instances, as well as non-prefab instances";
    }
    
    // [AttributeExample(typeof(OptionalInAttributeExamples), "Overrides the 'Reference Required by Default' rule to allow for null values. Has no effect if the rule is disabled. " +
    //                                                        "This attribute does not do anything unless you have Odin Validator and the 'Reference Required by Default' rule is enabled." +
    //                                                        "" +
    //                                                        "Optional members in the inspector when edited from regular prefab assets, prefab variants, nested prefabs, prefab instances or non prefab instances.")]
    // internal class OptionalInAttributeExamples
    // {
    //     [OptionalIn(PrefabKind.InstanceInScene)]        
    //     public string InstanceInScene = "Instances of prefabs in scenes";
    //      
    //     [OptionalIn(PrefabKind.InstanceInPrefab)]       
    //     public string InstanceInPrefab = "Instances of prefabs nested inside other prefabs";
    //     
    //     [OptionalIn(PrefabKind.Regular)]        
    //     public string Regular = "Regular prefab assets";
    //     
    //     [OptionalIn(PrefabKind.Variant)]        
    //     public string Variant = "Prefab variant assets";
    //     
    //     [OptionalIn(PrefabKind.NonPrefabInstance)]      
    //     public string NonPrefabInstance = "Non-prefab component or gameobject instances in scenes";
    //      
    //     [OptionalIn(PrefabKind.PrefabInstance)]         
    //     public string PrefabInstance = "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";
    //     
    //     [OptionalIn(PrefabKind.PrefabAsset)]        
    //     public string PrefabAsset = "Prefab assets and prefab variant assets";
    //     
    //     [OptionalIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]         
    //     public string PrefabInstanceAndNonPrefabInstance = "Prefab Instances, as well as non-prefab instances";
    // }
    
}
#endif