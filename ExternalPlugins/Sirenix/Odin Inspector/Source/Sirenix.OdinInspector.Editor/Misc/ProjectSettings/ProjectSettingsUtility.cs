//-----------------------------------------------------------------------
// <copyright file="ProjectSettingsUtility.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Reflection;

    public static class ProjectSettingsUtility
    {
        public static void InitAllProjectSettingFieldsFromAttributes(UnityEngine.Object instance)
        {
            var type = instance.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];

                if (typeof(IProjectSetting).IsAssignableFrom(field.FieldType))
                {
                    var attr = field.GetAttribute<ProjectSettingKeyAttribute>();

                    if (attr != null)
                    {
                        //Debug.LogError("The project settings field " + type.GetNiceName() + "." + field.Name + " has no ProjectSettingKeyAttribute on it. This project setting will be broken!");
                        //}
                        //else
                        //{
                        IProjectSetting settings = field.GetValue(instance) as IProjectSetting;

                        if (settings == null)
                        {
                            settings = Activator.CreateInstance(field.FieldType) as IProjectSetting;
                            field.SetValue(instance, settings);
                        }

                        settings.SetInitData(attr.Key, attr.DefaultValue, instance);
                    }
                }
            }
        }
    }
}
#endif