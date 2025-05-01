//-----------------------------------------------------------------------
// <copyright file="DropZoneHandle.cs" company="Sirenix ApS">
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

    using System;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// This class is due to undergo refactoring.
    /// </summary>
    public sealed class DropZoneHandle
    {
        private static DropZoneHandle hoveringDropZone = null;
        private bool isBeingHovered;

        public bool IsAccepted { get; private set; }

        public bool IsBeingHovered
        {
            get
            {
                return this.isBeingHovered && DragAndDropManager.IsDragInProgress;
            }
        }

        internal EditorWindow SourceWindow;

        public Rect ScreenRect;
        public Rect Rect;

        public Type Type { get; internal set; }

        public int LayoutDepth { get; set; }

        public bool CanAcceptMove { get; internal set; }

        public bool IsCrossWindowDrag { get; private set; }

        public bool IsReadyToClaim
        {
            get
            {
                return
                    this.IsAccepted &&
                    hoveringDropZone == this &&
                    DragAndDropManager.IsDragInProgress &&
                    DragAndDropManager.CurrentDraggingHandle.IsReadyToBeClaimed &&
                    DragAndDropManager.CurrentDraggingHandle.IsBeingClaimed == false &&
                    Event.current.type == EventType.Repaint;
            }
        }

        public bool Enabled = true;

        public object ClaimObject()
        {
            if (this.IsReadyToClaim == false)
            {
                throw new Exception("Check IsReadyToClaim before claiming the object.");
            }

            return DragAndDropManager.CurrentDraggingHandle.DropObject();
        }

        internal void Update()
        {
            if (hoveringDropZone == this)
            {
                hoveringDropZone = null;
                DragAndDropManager.CurrentHoveringDropZone = null;
            }

            this.isBeingHovered = false;
            this.IsAccepted = false;
            this.IsCrossWindowDrag = false;

            if (DragAndDropManager.IsDragInProgress && this.Enabled && DragAndDropManager.AllowDrop && GUI.enabled)
            {
                this.IsAccepted = DragAndDropManager.IsDragInProgress &&
                                 (DragAndDropManager.CurrentDraggingHandle.Object != null &&
                                  DragAndDropManager.CurrentDraggingHandle.Object.GetType().InheritsFrom(this.Type) ||
                                  this.Type.IsNullableType() && object.ReferenceEquals(null, DragAndDropManager.CurrentDraggingHandle.Object));

                if (this.ScreenRect.Contains(GUIHelper.MouseScreenPosition) && GUIHelper.CurrentWindowHasFocus)
                {
                    bool setHoveringDropZone =
                            hoveringDropZone == null ||
                            this.LayoutDepth >= hoveringDropZone.LayoutDepth ||
                            hoveringDropZone.ScreenRect.Contains(GUIHelper.MouseScreenPosition) == false;

                    if (setHoveringDropZone && this.IsAccepted)
                    {
                        hoveringDropZone = this;
                    }

                    if (this.IsAccepted && hoveringDropZone == this)
                    {
                        DragAndDropManager.CurrentHoveringDropZone = this;
                    }
                    else
                    {
                        this.isBeingHovered = false;
                    }
                }
                else if (DragAndDropManager.CurrentHoveringDropZone == this)
                {
                    this.isBeingHovered = false;
                    DragAndDropManager.CurrentHoveringDropZone = null;
                }

                this.isBeingHovered = DragAndDropManager.CurrentHoveringDropZone == this;

                if (this.isBeingHovered)
                {
                    this.IsCrossWindowDrag = DragAndDropManager.CurrentDraggingHandle.SourceWindow != this.SourceWindow;
                    DragAndDropManager.CurrentDraggingHandle.IsCrossWindowDrag = this.IsCrossWindowDrag;
                }
            }
        }
    }
}
#endif