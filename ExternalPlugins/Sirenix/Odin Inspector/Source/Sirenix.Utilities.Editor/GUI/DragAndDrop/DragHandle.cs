//-----------------------------------------------------------------------
// <copyright file="DragHandle.cs" company="Sirenix ApS">
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

    using Serialization;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using Utilities;
    using SerializationUtility = Sirenix.Serialization.SerializationUtility;

    /// <summary>
    /// This class is due to undergo refactoring.
    /// </summary>
    public class DragHandle
    {
        internal EditorWindow SourceWindow;

        private DropEvents dropEvent = DropEvents.None;
        private Vector2 mouseDownPostionOffset;
        private bool isMouseDown;
        private bool isDragging;
        private EventType lastSeenEvent;

        public bool IsCrossWindowDrag { get; internal set; }

        public Rect Rect { get; set; }

        public Rect DraggingScreenRect
        {
            get
            {
                var r = this.Rect;
                var mp = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                r.x = mp.x - this.mouseDownPostionOffset.x;
                r.y = mp.y - this.mouseDownPostionOffset.y;

                return r;
            }
        }

        public object Object { get; internal set; }

        public DragAndDropMethods DragAndDropMethod { get; internal set; }

        public bool OnDragStarted { get; private set; }

        public OnDragFinnished OnDragFinnished { get; set; }

        public bool IsDragging { get { return DragAndDropManager.CurrentDraggingHandle == this; } }

        public bool IsHovering { get; private set; }

        public DragAndDropMethods CurrentMethod { get; private set; }

        public Vector2 MouseDownPostionOffset
        {
            get { return this.mouseDownPostionOffset; }
        }

        public Rect? DragHandleRect { get; set; }

        internal Rect TempRect { get; set; }
        internal int LayoutDepth { get; set; }
        internal bool WillDrop { get; private set; }
        internal bool IsReadyToBeClaimed { get; private set; }
        internal bool IsBeingClaimed { get; private set; }

        private static DragHandle currentHoveringDragHandle;

        public bool Enabled = true;

        internal void Update()
        {
            this.lastSeenEvent = Event.current.type;

            this.SetCurrentDragAndDropMethod();

            if (this.lastSeenEvent == EventType.Repaint)
            {
                this.FinalizeDropObject();
            }

            if (Event.current.isMouse || Event.current.type == EventType.DragUpdated)
            {
                Rect screenSpaceRect;

                if (this.DragHandleRect.HasValue)
                {
                    screenSpaceRect = this.DragHandleRect.Value;
                }
                else
                {
                    screenSpaceRect = this.Rect;
                }

                Vector2 screenPos = GUIUtility.GUIToScreenPoint(new Vector2(screenSpaceRect.x, screenSpaceRect.y));
                screenSpaceRect.x = screenPos.x;
                screenSpaceRect.y = screenPos.y;

                this.IsHovering = screenSpaceRect.Contains(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
            }

            this.OnDragStarted = false;

            if (DragAndDropManager.IsDragInProgress == false)
            {
                if (Event.current.isMouse)
                {
                    if (this.IsHovering)
                    {
                        if (this.Enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            this.isMouseDown = true;
                            this.mouseDownPostionOffset = Event.current.mousePosition - new Vector2(this.Rect.x, this.Rect.y);
                            GUIHelper.RemoveFocusControl();
                            Event.current.Use();
                            DragAndDrop.PrepareStartDrag();
                        }

                        if (this.isMouseDown && Event.current.type == EventType.MouseDrag)
                        {
                            this.isDragging = true;
                            this.OnDragStarted = true;

                            DragAndDrop.objectReferences = new UnityEngine.Object[0];
                            DragAndDrop.paths = null;
                            
                            if (this.Object != null)
                            {
                                DragAndDrop.SetGenericData(this.Object.GetType().Name, this.Object);
                            }
                            
                            DragAndDrop.StartDrag("Odin Drag Operation");
                        }
                    }
                }
            }
            else
            {
                GUIHelper.RequestRepaint();
            }

            if (this.isDragging)
            {
                GUIHelper.RequestRepaint();

                DragAndDropManager.CurrentDraggingHandle = this;

                if (EditorWindow.mouseOverWindow != null)
                {
                    EditorWindow.mouseOverWindow.Focus();
                }

                if (DragAndDropManager.WasDragPerformed)
                {
                    this.IsReadyToBeClaimed = true;

                    if (DragAndDropManager.IsHoveringDropZone == false)
                    {
                        this.DropObject(DropEvents.Canceled);
                    }
                }
            }
            else
            {
                if (this.IsHovering)
                {
                    if (currentHoveringDragHandle == null || this.LayoutDepth >= currentHoveringDragHandle.LayoutDepth)
                    {
                        currentHoveringDragHandle = this;
                    }
                }
                else if (currentHoveringDragHandle == this)
                {
                    currentHoveringDragHandle = null;
                }

                this.IsHovering = currentHoveringDragHandle == this;
            }
        }

        private void SetCurrentDragAndDropMethod()
        {
            if (Event.current.type == EventType.Repaint)
            {
                this.CurrentMethod = this.DragAndDropMethod;

                bool ctrl = (Event.current.modifiers & EventModifiers.Control) == EventModifiers.Control;
                bool shift = (Event.current.modifiers & EventModifiers.Shift) == EventModifiers.Shift;

                if (ctrl && (this.CurrentMethod == DragAndDropMethods.Reference || this.CurrentMethod == DragAndDropMethods.Move))
                {
                    this.CurrentMethod = DragAndDropMethods.Copy;
                }
                else if (shift && this.CurrentMethod == DragAndDropMethods.Copy)
                {
                    this.CurrentMethod = DragAndDropMethods.Reference;
                }
            }
        }

        private void FinalizeDropObject()
        {
            if (this.WillDrop)
            {
                this.WillDrop = false;
                this.IsBeingClaimed = false;
                this.IsReadyToBeClaimed = false;
                this.isDragging = false;
                this.isMouseDown = false;
                this.IsHovering = false;
                currentHoveringDragHandle = null;
                DragAndDropManager.WasDragPerformed = false;
                DragAndDropManager.CurrentDraggingHandle = null;
                DragAndDropManager.CurrentHoveringDropZone = null;
                GUIHelper.RequestRepaint();

                if (this.OnDragFinnished != null)
                {
                    this.OnDragFinnished(this.dropEvent);
                }
            }
        }

        internal object DropObject()
        {
            DropEvents e;

            if (this.CurrentMethod == DragAndDropMethods.Move)
            {
                e = DropEvents.Moved;
            }
            else if (this.CurrentMethod == DragAndDropMethods.Reference)
            {
                e = DropEvents.Referenced;
            }
            else if (this.CurrentMethod == DragAndDropMethods.Copy)
            {
                e = DropEvents.Copied;
            }
            else
            {
                throw new NotImplementedException();
            }

            return this.DropObject(e);
        }

        internal object DropObject(DropEvents dropEvent)
        {
            this.dropEvent = dropEvent;
            this.WillDrop = true;
            this.IsBeingClaimed = dropEvent != DropEvents.None && dropEvent != DropEvents.Canceled;

            if (this.lastSeenEvent == EventType.Repaint)
            {
                this.FinalizeDropObject();
            }

            if (dropEvent == DropEvents.Copied && this.Object != null)
            {
                if (this.Object.GetType().InheritsFrom(typeof(UnityEngine.Object)))
                {
                    return this.Object;
                }

                using (var stream = new MemoryStream())
                {
                    List<UnityEngine.Object> unityReferences;
                    SerializationUtility.SerializeValue(this.Object, stream, DataFormat.Binary, out unityReferences);
                    stream.Position = 0;
                    return SerializationUtility.DeserializeValue<object>(stream, DataFormat.Binary, unityReferences);
                }
            }

            return this.Object;
        }
    }
}
#endif