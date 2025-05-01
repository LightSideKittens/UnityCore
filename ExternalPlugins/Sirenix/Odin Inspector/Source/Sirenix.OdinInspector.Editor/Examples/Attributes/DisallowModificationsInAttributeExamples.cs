//-----------------------------------------------------------------------
// <copyright file="DisallowModificationsInAttributeExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(DisallowModificationsInAttribute), "DisallowModificationsIn disables / grays out members, preventing modifications from being made and enables validation, providing error messages in case a modification was made prior to introducing the attribute. Modifications can be prevented for prefab variants and prefab instances in scenes and prefabs.")]
    [TypeInfoBox("Note that this drawn example does not represent a GameObject, so the attribute is not shown correctly in the Attribute Overview window. If you copy it to a script, for example via the Save Component Script button, the example will display correctly when you put it on GameObjects in various kinds of prefabs.")]
    internal class DisallowModificationsInAttributeExamples
    {
        [DisallowModificationsIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
        public string PrefabInstanceAndNonPrefabInstance = "Prefab Instances, as well as non-prefab instances";

        [DisallowModificationsIn(PrefabKind.InstanceInScene)]
        public string InstanceInScene = "Instances of prefabs in scenes";

        [DisallowModificationsIn(PrefabKind.InstanceInPrefab)]
        public string InstanceInPrefab = "Instances of prefabs nested inside other prefabs";

        [DisallowModificationsIn(PrefabKind.Variant)]
        public string Variant = "Prefab variant assets";

        [DisallowModificationsIn(PrefabKind.NonPrefabInstance)]
        public string NonPrefabInstance = "Non-prefab component or gameobject instances in scenes";

        [DisallowModificationsIn(PrefabKind.PrefabInstance)]
        public string PrefabInstance = "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";
    }
#pragma warning restore CS0618 // Type or member is obsolete

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