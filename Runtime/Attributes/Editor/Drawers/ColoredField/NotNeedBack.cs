using Attributes;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Attributes.Editor.Drawers
{
    public partial class ColoredFieldPropertyDrawer
    {
        private void BaseDraw(GUIContent label)
        {
            GUIHelper.PushColor(Attribute.colorData.color);
            
            SirenixEditorGUI.BeginIndentedVertical(style);
            GUIHelper.PushHierarchyMode(false);
            GUIHelper.PushLabelWidth(labelWidth);
            
            var box = BeginBoxHeader();
            var heartRect = box;
            heartRect.width = box.height - 5;
            heartRect.height = box.height - 6;
            heartRect.x = box.width - 10;
            heartRect.y += 3;

            GUIHelper.PopColor();
            SetupHeader(box);
            
            label.text = labelName;
            isOpen = Foldout(isOpen, label);
            SirenixEditorGUI.EndBoxHeader();

            var isChildModified = false;
            
            if (SirenixEditorGUI.BeginFadeGroup(this, isOpen))
            {
                for (int i = 0; i < Property.Children.Count; i++)
                {
                    var childProp = Property.Children[i];

                    for (int j = 0; j < childProp.Children.Count; j++)
                    {
                        var child = childProp.Children[j];
                        child.Draw();
                        
                        var attribute = child.GetAttribute<ColoredFieldAttribute>();
                        
                        if (attribute != null)
                        {
                            isChildModified |= attribute.isModified;
                        }
                    }
                }
            }
            else
            {

                for (int i = 0; i < Property.Children.Count; i++)
                {
                    var childProp = Property.Children[i];

                    for (int j = 0; j < childProp.Children.Count; j++)
                    {
                        var child = childProp.Children[j];
                        var attribute = child.GetAttribute<ColoredFieldAttribute>();
                        
                        if (attribute != null)
                        {
                            Graphics.DrawTexture(heartRect, heartTexture);
                            heartRect.x -= heartRect.width + 2;
                            isChildModified |= attribute.isModified;
                        }
                    }
                }
            }
            
            if (isChildModified)
            {
                Graphics.DrawTexture(heartRect, blueHeartTexture);
                heartRect.x -= heartRect.width + 2;
            }
            
            if (Attribute.isModified)
            {
                Graphics.DrawTexture(heartRect, blueHeartTexture);
            }
            else
            {
                OnPropPrefabUpdated();
            }

            SirenixEditorGUI.EndFadeGroup();
            SirenixEditorGUI.EndBox();
        }
    }
}