//-----------------------------------------------------------------------
// <copyright file="ProjectSettingPropertyProcessor.cs" company="Sirenix ApS">
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

    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class ProjectSettingAttributeProcessor<TSetting, TValue> : OdinAttributeProcessor<TSetting>
        where TSetting : ProjectSetting<TValue>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            attributes.Add(new CustomContextMenuAttribute("Reset Config Value", "Reset"));

            if (member.Name != "Value") return;

            var parentAttrs = parentProperty.Attributes;

            foreach (var attr in parentAttrs)
            {
                if (attr is InlinePropertyAttribute
                    || attr is PropertyGroupAttribute
                    || attr is CustomContextMenuAttribute
                    || attr is ProjectSettingKeyAttribute
                    || attr is SuffixLabelAttribute) continue;

                attributes.Add(attr);
            }
        }

        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new SuffixLabelAttribute("@$value.LocalOverride ? \"Local\" : \"Global\""));
            attributes.Add(new CustomContextMenuAttribute("Reset Config Value", "@$value.Reset()"));
            attributes.Add(new SuppressInvalidAttributeErrorAttribute());
        }
    }
}
#endif