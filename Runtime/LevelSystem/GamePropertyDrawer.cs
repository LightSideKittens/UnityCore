#if UNITY_EDITOR

using System;
using LSCore.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Battle.Data.GameProperty
{
    [Serializable]
    public abstract class GamePropertyDrawer
    {
        public override bool Equals(object obj)
        {
            if (obj is GamePropertyDrawer drawer)
            {
                return Equals(drawer);
            }

            return false;
        }
        
        public bool Equals(GamePropertyDrawer other)
        {
            return GetType() == other.GetType();
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        [CustomValueDrawer("IconDrawer")]
        [ShowInInspector] private int iconDrawer;
        
        public static bool isInited;
        private static Texture2D evenTexture;
        protected abstract string IconName { get; }
        private Texture2D icon;

        private string Title => GetType().Name.Replace("GP", "Property").SplitPascalCase();

        
        protected GamePropertyDrawer()
        {
            if (isInited)
            {
                CreateData();
            }
        }

        [OnInspectorDispose]
        private void OnInspectorDispose()
        {
            isInited = false;
        }
        
        [OnInspectorInit]
        private void CreateData()
        {
            isInited = true;
            InitIcon();

            if (evenTexture == null)
            {
                evenTexture = EditorUtils.GetTextureByColor(new Color(0.17f, 0.17f, 0.18f));
            }

            OnInit();
        }

        protected virtual void OnInit(){}

        private void InitIcon()
        {
            icon = LSIcons.Get(IconName);
        }

        private int IconDrawer(int value, GUIContent label, Func<GUIContent, bool> callNextDrawer)
        {
            if (icon == null)
            {
                InitIcon();
                return 0;
            }

            var rect = GUIHelper.GetCurrentLayoutRect();

            rect.xMax -= 20;
            rect.xMin += 20;
            rect.yMax = rect.yMin + 30;
            var texRect = rect;
            var center = texRect.center;
            texRect.height -= 8;
            texRect.width -= 8;
            texRect.center = center;
            
            GUI.DrawTexture(texRect, evenTexture, ScaleMode.StretchToFill, false);
            
            var textStyle = new GUIStyle() {alignment = TextAnchor.MiddleCenter};
            textStyle.normal.textColor = Color.white;
            textStyle.richText = true;
            GUI.Label(rect, $"<b>{Title}</b>", textStyle);
            
            var lastPosition = rect.position;
            rect.position = lastPosition;
            GUI.Box(rect, icon, GUIStyle.none);
            rect.position = lastPosition + new Vector2(rect.width - rect.height, 0);
            GUI.Box(rect, icon, GUIStyle.none);
            GUILayout.Space(10);

            return 0;
        }
    }
}
#endif
