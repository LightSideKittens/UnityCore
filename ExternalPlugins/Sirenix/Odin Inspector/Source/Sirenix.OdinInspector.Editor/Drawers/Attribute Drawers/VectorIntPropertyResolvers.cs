//-----------------------------------------------------------------------
// <copyright file="VectorIntPropertyResolvers.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
#if UNITY_EDITOR

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using UnityEngine;

    public sealed class Vector2IntResolver : BaseMemberPropertyResolver<Vector2Int>
    {
        protected override InspectorPropertyInfo[] GetPropertyInfos()
        {
            return new InspectorPropertyInfo[]
            {
                InspectorPropertyInfo.CreateValue("x", 0, this.Property.ValueEntry.SerializationBackend,
                    new GetterSetter<Vector2Int, int>(
                        getter: (ref Vector2Int vec) => vec.x,
                        setter: (ref Vector2Int vec, int value) => vec.x = value)),
                InspectorPropertyInfo.CreateValue("y", 0, this.Property.ValueEntry.SerializationBackend,
                    new GetterSetter<Vector2Int, int>(
                        getter: (ref Vector2Int vec) => vec.y,
                        setter: (ref Vector2Int vec, int value) => vec.y = value)),
            };
        }
    }

    public sealed class Vector3IntResolver : BaseMemberPropertyResolver<Vector3Int>
    {
        protected override InspectorPropertyInfo[] GetPropertyInfos()
        {
            return new InspectorPropertyInfo[]
            {
                InspectorPropertyInfo.CreateValue("x", 0, this.Property.ValueEntry.SerializationBackend,
                    new GetterSetter<Vector3Int, int>(
                        getter: (ref Vector3Int vec) => vec.x,
                        setter: (ref Vector3Int vec, int value) => vec.x = value)),
                InspectorPropertyInfo.CreateValue("y", 0, this.Property.ValueEntry.SerializationBackend,
                    new GetterSetter<Vector3Int, int>(
                        getter: (ref Vector3Int vec) => vec.y,
                        setter: (ref Vector3Int vec, int value) => vec.y = value)),
                InspectorPropertyInfo.CreateValue("z", 0, this.Property.ValueEntry.SerializationBackend,
                    new GetterSetter<Vector3Int, int>(
                        getter: (ref Vector3Int vec) => vec.z,
                        setter: (ref Vector3Int vec, int value) => vec.z = value)),
            };
        }
    }
}

#endif // UNITY_EDITOR
#endif