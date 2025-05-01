//-----------------------------------------------------------------------
// <copyright file="IOverridesSerializationPolicy.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Serialization
{
#pragma warning disable

    /// <summary>
    /// Indicates that an Odin-serialized Unity object provides its own serialization policy rather than using the default policy.
    /// <para/>
    /// Note that THE VALUES RETURNED BY THIS INTERFACE WILL OVERRIDE THE PARAMETERS PASSED TO <see cref="UnitySerializationUtility.SerializeUnityObject(UnityEngine.Object, ref SerializationData, bool, SerializationContext)"/> and <see cref="UnitySerializationUtility.DeserializeUnityObject(UnityEngine.Object, ref SerializationData, DeserializationContext)"/>.
    /// </summary>
    public interface IOverridesSerializationPolicy
    {
        ISerializationPolicy SerializationPolicy { get; }
        bool OdinSerializesUnityFields { get; }
    }
}