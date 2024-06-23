using System;
using Sirenix.Utilities;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace LSCore.Attributes
{
    public class DefaultValueAttribute : Attribute
    {
        public Type type;

        public DefaultValueAttribute(Type type)
        {
            this.type = type;
        }
    }
    
#if UNITY_EDITOR
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public class DefaultValueAttributeDrawer : OdinValueDrawer<object>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var type = Property.Info.TypeOfValue;
            DefaultValueAttribute attribute = null;

            var baseType = type;
            while (baseType != null)
            {
                attribute = baseType.GetAttribute<DefaultValueAttribute>();
                if (attribute != null)
                {
                    break;
                }
                baseType = baseType.BaseType;
            }
            
            if (attribute != null)
            {
                if (baseType == type)
                {
                    ValueEntry.SmartValue ??= Activator.CreateInstance(attribute.type);
                }
                else
                {
                    ValueEntry.SmartValue ??= Activator.CreateInstance(type);
                }
            }
        
            CallNextDrawer(label);
        }
    }
#endif
}