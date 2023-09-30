using System.Text.RegularExpressions;
using LSCore.Extensions;
using GameCore.Attributes;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Attributes.Editor.Drawers
{
    public partial class ColoredFieldPropertyDrawer
    {
        private string prefabPath;
        private int lastModifiedPropCount;
        
        protected override void Initialize()
        {
            base.Initialize();

            var parent = Property.Parent;
            heartTexture = Resources.Load<Texture2D>($"ColoredField/heart");
            blueHeartTexture = Resources.Load<Texture2D>($"ColoredField/blue_heart");

            style = new GUIStyle(SirenixGUIStyles.BoxContainer);
            
            if (parent != null)
            {
                var name = Property.Info.GetMemberInfo().Name;
                prefabPath = $"{Property.PrefabModificationPath}.{name}";
                style.margin = new RectOffset(10, 10, 10, 10);
            }

            OnPropPrefabUpdated();
            var colorData = Attribute.colorData;
            normalColor = colorData.isDark ? Color.white.CloneAndChangeBrightness(0.8f) : Color.white.CloneAndChangeBrightness(0.2f);
            hoverColor = colorData.isDark ? Color.white : Color.black;
            labelName = Property.Info.GetMemberInfo().Name.SplitPascalCase();
        }

        private void OnPropPrefabUpdated()
        {
            var propertyModifications = PrefabUtility.GetPropertyModifications(Selection.activeGameObject);
            
            if (propertyModifications == null || propertyModifications.Length == lastModifiedPropCount) return;

            for (int i = 0; i < propertyModifications.Length; i++)
            {
                var prop = propertyModifications[i];
                var isMatch = Regex.Match(prop.propertyPath, @$"^({prefabPath})\.(\w+)$");
            
                if (isMatch.Captures.Count != 0)
                {
                    Attribute.isModified = true;
                    break;
                }
            }
            
            lastModifiedPropCount = propertyModifications.Length;
            
        }
    }
}