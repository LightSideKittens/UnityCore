//-----------------------------------------------------------------------
// <copyright file="ModuleData.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Modules
{
#pragma warning disable

    using Sirenix.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ModuleData
    {
        public string ID;
        public Version Version;
        public List<ModuleFile> Files;

        public class ModuleFile
        {
            public string Path;
            public byte[] Data;
        }

        public ModuleManifest ToManifest()
        {
            return new ModuleManifest()
            {
                ID = this.ID,
                Version = this.Version,
                Files = this.Files.Select(n => n.Path).ToList()
            };
        }

        public static byte[] Serialize(ModuleData data)
        {
            return SerializationUtility.SerializeValue(data, DataFormat.Binary);
        }

        public static ModuleData Deserialize(byte[] bytes)
        {
            return SerializationUtility.DeserializeValue<ModuleData>(bytes, DataFormat.Binary);
        }
    }
}
#endif