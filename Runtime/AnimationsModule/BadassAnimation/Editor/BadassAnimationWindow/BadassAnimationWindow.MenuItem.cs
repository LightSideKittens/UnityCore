#if UNITY_EDITOR
using System;
using DG.DemiEditor;
using LSCore.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

public partial class BadassAnimationWindow
{
    public class MenuItem : OdinMenuItem
    {
        private bool isLockedSelf;
        private bool isVisibleSelf = true;

        public new Rect Rect { get; protected set; }
        public Color Color { get; protected set; }
        
        public bool IsLockedSelf
        {
            get { return isLockedSelf; }
            set
            {
                if (value != isLockedSelf) LockedSelfChanged?.Invoke(value);
                isLockedSelf = value;
            }
        }

        public bool IsVisibleSelf
        {
            get { return isVisibleSelf; }
            set
            {
                if (value != isVisibleSelf) VisibleSelfChanged?.Invoke(value);
                isVisibleSelf = value;
            }
        }

        public event Action<bool> VisibleSelfChanged;
        public event Action<bool> LockedSelfChanged;

        public bool IsLocked
        {
            set
            {
                IsLockedSelf = value;
                
                foreach (var item in ChildMenuItems)
                {
                    if (item is MenuItem menuItem)
                    {
                        menuItem.IsLocked = value;
                    }
                }
                
                if (!value)
                {
                    var cur = Parent;
                
                    while (cur != null)
                    {
                        if (cur is MenuItem menuItem)
                        {
                            menuItem.IsLockedSelf = false;
                        }
                    
                        cur = cur.Parent;    
                    }
                }
            }
        }
        
        public new bool IsVisible
        {
            set
            {
                IsVisibleSelf = value;
                
                foreach (var item in ChildMenuItems)
                {
                    if (item is MenuItem menuItem)
                    {
                        menuItem.IsVisible = value;
                    }
                }

                if (value)
                {
                    var cur = Parent;
                
                    while (cur != null)
                    {
                        if (cur is MenuItem menuItem)
                        {
                            menuItem.IsVisibleSelf = true;
                        }
                    
                        cur = cur.Parent;
                    }
                }
            }
        }
        
        public MenuItem(OdinMenuTree tree, string name, object value) : base(tree, name, value)
        {
            Style.IndentAmount = 10;
            Style.Height = 20;
            Style.AlignTriangleLeft = true;
            Style.TrianglePadding = 0;
            Color = LSHandles.Styles.selectionColor.SetAlpha(0.5f);
            Style.SelectedColorLightSkin = Color;
            Style.SelectedColorDarkSkin = Color;
        }

        protected void BaseOnDrawMenuItem(Rect rect, Rect labelRect)
        {
            base.OnDrawMenuItem(rect, labelRect);
        }

        public bool IsShowing
        {
            get
            {
                var parent = Parent;

                while (parent != null)
                {
                    if (!parent.Toggled)
                    {
                        return false;
                    }
                    
                    parent = parent.Parent;
                }
                
                return MenuItemIsBeingRendered;
            }
        }

        protected Rect ConvertRect(Rect rect)
        {
            Vector2 screenPosTL = GUIUtility.GUIToScreenPoint(rect.position);
            
            Vector2 screenPosBR = GUIUtility.GUIToScreenPoint(
                new Vector2(rect.xMax, rect.yMax)
            );
            
            Rect screenRect = new Rect(
                screenPosTL,
                screenPosBR - screenPosTL
            );
            
            screenRect.x -= window.position.x;
            screenRect.y -= window.position.y;
            screenRect.y -= 25;
            return screenRect;
        }
        
        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            Rect = ConvertRect(rect);
            rect.TakeFromRight(10);
            
            if (SirenixEditorGUI.SDFIconButton(rect.TakeFromRight(15), isVisibleSelf ? SdfIconType.EyeFill : SdfIconType.EyeSlashFill, isVisibleSelf ? EnabledStyle : DisabledStyle))
            {
                IsVisible = !isVisibleSelf;
            }
            
            if (SirenixEditorGUI.SDFIconButton(rect.TakeFromRight(15), isLockedSelf ? SdfIconType.LockFill : SdfIconType.UnlockFill, isLockedSelf ? EnabledStyle : DisabledStyle))
            {
                IsLocked = !isLockedSelf;
            }
        }
    }
}
#endif