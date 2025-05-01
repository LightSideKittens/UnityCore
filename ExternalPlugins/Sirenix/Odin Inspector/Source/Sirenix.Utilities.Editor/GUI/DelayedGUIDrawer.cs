//-----------------------------------------------------------------------
// <copyright file="DelayedGUIDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using UnityEngine;

    public class DelayedGUIDrawer
    {
        private Vector2 screenPos;
        private Material material;
        private RenderTexture prev;
        private RenderTexture target;

        public void Begin(float width, float height, bool drawGUI = false)
        {
            this.Begin(new Vector2(width, height), drawGUI);
        }

        public void Begin(Vector2 size, bool drawGUI = false)
        {
            var areaRect = new Rect(this.screenPos, size);

            GUIHelper.BeginIgnoreInput();
            GUILayout.BeginArea(areaRect, SirenixGUIStyles.None);

            if (Event.current.type == EventType.Repaint)
            {
                this.prev = RenderTexture.active;
                if (this.target != null)
                {
                    RenderTexture.ReleaseTemporary(this.target);
                }
                this.target = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
                RenderTexture.active = this.target;
                GL.Clear(false, true, new Color(0, 0, 0, 0));
            }
        }

        public void End()
        {
            if (Event.current.type == EventType.Repaint)
            {
                RenderTexture.active = this.prev;
            }

            GUILayout.EndArea();
            GUIHelper.EndIgnoreInput();
        }

        public void Draw(Vector2 position)
        {
            if (Event.current.type != EventType.Layout)
            {
                this.screenPos = position;
            }

            if (Event.current.type == EventType.Repaint)
            {
                if (this.material == null)
                {
                    this.material = new Material(Shader.Find("Unlit/Transparent"));
                }

                if (this.target != null)
                {
                    Graphics.Blit(this.target, RenderTexture.active, material);


                    RenderTexture.ReleaseTemporary(this.target);
                    this.target = null;
                }
            }
        }
    }
}
#endif