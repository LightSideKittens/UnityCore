//-----------------------------------------------------------------------
// <copyright file="DragAndDropUtilities.cs" company="Sirenix ApS">
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

    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities.Editor.Internal;
    using System;
    //using System.Collections;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Drag and drop utilities for both Unity and non-unity objects.
    /// </summary>
    public static class DragAndDropUtilities
    {
        private const int SPECIAL_CONTROL_ID_START = 961999999;

        //private static FieldInfo dragAndDrop_GenericData;
        private readonly static Func<System.Collections.Hashtable> getDragAndDropGenericData;

        private static int mouseDownDragAndDropId = -1;
        private static int mouseClickedId = -1;
        private static bool currentDragIsMove;
        private static int draggingId;
        private static bool isAccepted;
        private static object dropZoneObject;
        private static object[] draggingObjects = new object[] { };
        private static bool isDragging = false;
        private static int hoveringAcceptedDropZone;
        private static int specialDragAndDropControlId = SPECIAL_CONTROL_ID_START;
        private static EventType prevEventType = EventType.Repaint;
        private static Rect preventDropAreaRect;
        private static Rect nextPreventDropAreaRect;

        static DragAndDropUtilities()
        {
            var dragAndDrop_GenericData = typeof(DragAndDrop).GetField("s_GenericData", BindingFlags.Static | BindingFlags.NonPublic);
            getDragAndDropGenericData = EmitUtilities.CreateStaticFieldGetter<System.Collections.Hashtable>(dragAndDrop_GenericData);
        }

        public static int PrevDragAndDropId { get; private set; }

        /// <summary>
        /// Gets the position from where the last drag started from in screen space.
        /// </summary>
        public static Vector2 OnDragStartMouseScreenPos { get; private set; }

        /// <summary>
        /// Gets the delta position between the currrent mouse position and where the last drag originated from.
        /// </summary>
        public static Vector2 MouseDragOffset
        {
            get
            {
                if (Event.current == null)
                {
                    return Vector2.zero;
                }

                return GUIUtility.GUIToScreenPoint(Event.current.mousePosition) - OnDragStartMouseScreenPos;
            }
        }

        /// <summary>
        /// Gets the hovering accepted drop zone ID.
        /// </summary>
        public static int HoveringAcceptedDropZone { get { return hoveringAcceptedDropZone; } }

        /// <summary>
        /// Gets a value indicating whether an instance is currently being dragged.
        /// </summary>
        public static bool IsDragging
        {
            get
            {
                switch (Event.current.rawType)
                {
                    case EventType.MouseDown:
                    case EventType.MouseUp:
                    case EventType.MouseMove:
                    isDragging = false;
                    break;
                    case EventType.MouseDrag:
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                    case EventType.DragExited:
                    isDragging = true;
                    break;
                    default:
                    break;
                }

                return isDragging;
            }
        }

        /// <summary>
        /// Gets the currently dragging identifier.
        /// </summary>
        public static int CurrentDragId
        {
            get
            {
                if (!IsDragging)
                {
                    return 0;
                }

                return draggingId;
            }
        }

        /// <summary>
        /// Gets the current hovering drop zone identifier.
        /// </summary>
        public static int CurrentDropId
        {
            get
            {
                if (!IsDragging)
                {
                    return 0;
                }

                return hoveringAcceptedDropZone != 0 ? hoveringAcceptedDropZone : DragAndDrop.activeControlID;
            }
        }

        /// <summary>
        /// Gets a more percistent id for drag and drop.
        /// </summary>
        public static int GetDragAndDropId(Rect rect)
        {
            int id = GUIUtility.GetControlID(SPECIAL_CONTROL_ID_START, FocusType.Passive, rect);

            PrevDragAndDropId = id;

            return id;
        }

        /// <summary>
        /// Draws a objectpicker button in the given rect. This one is designed to look good on top of DrawDropZone().
        /// </summary>
        public static object ObjectPickerZone(Rect rect, object value, Type type, bool allowSceneObjects, int id)
        {
            return InternalOdinEditorWrapper.ObjectPickerZone(rect, rect, value, type, allowSceneObjects, id);
        }

        /// <summary>
        /// Draws a objectpicker butter, in the given rect. This one is designed to look good on top of DrawDropZone().
        /// </summary>
        public static T ObjectPickerZone<T>(Rect rect, T value, bool allowSceneObjects, int id)
        {
            return (T)ObjectPickerZone(rect, value, typeof(T), allowSceneObjects, id);
        }

        /// <summary>
        /// Draws the graphics for a DropZone.
        /// </summary>
        public static void DrawDropZone(Rect rect, object value, GUIContent label, int id)
        {
            bool isDragging = IsDragging;

            if (Event.current.type == EventType.Repaint)
            {
                var objectToPaint = value as UnityEngine.Object;
                var objectFieldThumb = EditorStyles.objectFieldThumb;
                var on = GUI.enabled && hoveringAcceptedDropZone == id && rect.Contains(Event.current.mousePosition) && isDragging;

                objectFieldThumb.Draw(rect, GUIContent.none, id, on);

                if (EditorGUI.showMixedValue)
                {
                    GUI.Label(rect, SirenixEditorGUI.MixedValueDashChar, SirenixGUIStyles.LabelCentered);
                }
                else if (objectToPaint)
                {
                    var image = GUIHelper.GetPreviewTexture(objectToPaint);

                    rect = rect.Padding(2);
                    float size = Mathf.Min(rect.width, rect.height);

                    EditorGUI.DrawTextureTransparent(rect.AlignCenter(size, size), image, ScaleMode.ScaleToFit);

                    if (label != null)
                    {
                        rect = rect.AlignBottom(16);
                        GUI.Label(rect, label, EditorStyles.label);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the graphics for a DropZone.
        /// </summary>
        public static void DrawDropZone(Rect rect, Texture preview, GUIContent label, int id)
        {
            bool isDragging = IsDragging;

            if (Event.current.type == EventType.Repaint)
            {
                var objectFieldThumb = EditorStyles.objectFieldThumb;
                var on = GUI.enabled && hoveringAcceptedDropZone == id && rect.Contains(Event.current.mousePosition) && isDragging;

                objectFieldThumb.Draw(rect, GUIContent.none, id, on);

                if (EditorGUI.showMixedValue)
                {
                    GUI.Label(rect, SirenixEditorGUI.MixedValueDashChar, SirenixGUIStyles.LabelCentered);
                }
                else
                {
                    rect = rect.Padding(2);
                    float size = Mathf.Min(rect.width, rect.height);

                    if (preview != null)
                    {
                        EditorGUI.DrawTextureTransparent(rect.AlignCenter(size, size), preview, ScaleMode.ScaleToFit);
                    }

                    if (label != null)
                    {
                        rect = rect.AlignBottom(16);
                        GUI.Label(rect, label, EditorStyles.label);
                    }
                }
            }
        }

        /// <summary>
        /// A draggable zone for both Unity and non-unity objects.
        /// </summary>
        public static object DragAndDropZone(Rect rect, object value, Type type, bool allowMove, bool allowSwap)
        {
            return DragAndDropZone(rect, value, type, allowMove, allowSwap, true);
        }
        
        /// <summary>
        /// A draggable zone for both Unity and non-unity objects.
        /// </summary>
        public static object DragAndDropZone(Rect rect, object value, Type type, bool allowMove, bool allowSwap, bool allowSceneObjects)
        {
            var id = GetDragAndDropId(rect);
            value = DropZone(rect, value, type, allowSceneObjects, id);
            value = DragZone(rect, value, type, allowMove, allowSwap, id);
            return value;
        }

        /// <summary>
        /// A drop zone area for both Unity and non-unity objects.
        /// </summary>
        public static object DropZone(Rect rect, object value, Type type, bool allowSceneObjects, int id)
        {
            if (Event.current.type == EventType.Layout)
            {
                return value;
            }

            if (rect.Contains(Event.current.mousePosition))
            {
                var t = Event.current.type;

                if (t == EventType.DragUpdated || t == EventType.DragPerform)
                {
                    // This bit disables all dropzones inside the provided preventDropAreaRect.
                    //
                    // RootNode1
                    //    ChileNode1
                    //    ChileNode2
                    //       ChileNode2.1
                    //       ChileNode2.2
                    //    ChileNode3
                    // RootNode2
                    //
                    // If the RootNode has provided a preventDropAreaRect, then that means that the RootNode won't be able to be dragged into any of its child nodes.

                    if (preventDropAreaRect.Contains(new Vector2(rect.x, rect.y)) && preventDropAreaRect.Contains(new Vector2(rect.xMax, rect.yMax)))
                    {
                        return value;
                    }

                    object obj = null;

                    if (obj == null) obj = draggingObjects.Where(x => x != null && x.GetType().InheritsFrom(type)).FirstOrDefault();
                    if (obj == null) obj = DragAndDrop.objectReferences.Where(x => x != null && x.GetType().InheritsFrom(type)).FirstOrDefault();

                    if (obj == null)
                    {
                        obj = draggingObjects.Concat(DragAndDrop.objectReferences).Where(x => x != null && ConvertUtility.CanConvert(x.GetType(), type)).Select(x =>
                        {
                            if (ConvertUtility.TryWeakConvert(x, type, out var converted))
                            {
                                return converted;
                            }

                            return null;
                        }).Where(x => x != null).FirstOrDefault();
                    }

                    if (obj == null)
                    {
                        var genericData = getDragAndDropGenericData();
                        if (genericData != null)
                        {
                            foreach (var data in genericData.Values)
                            {
                                if (data is System.Collections.ICollection collection)
                                {
                                    foreach (var x in collection)
                                    {
                                        if (x.GetType().InheritsFrom(type))
                                        {
                                            obj = x;
                                            break;
                                        }
                                        else if (ConvertUtility.TryWeakConvert(x, type, out var converted))
                                        {
                                            obj = converted;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //if (type.InheritsFrom<Component>() || type.IsInterface)
                    //{
                    //    if (obj == null) obj = draggingObjects.OfType<GameObject>().Where(x => x != null).Select(x => x.GetComponent(type)).Where(x => x != null).FirstOrDefault();
                    //    if (obj == null) obj = DragAndDrop.objectReferences.OfType<GameObject>().Where(x => x != null).Select(x => x.GetComponent(type)).Where(x => x != null).FirstOrDefault();
                    //}
                    //else if (type == typeof(GameObject))
                    //{
                    //    if (obj == null) obj = draggingObjects.OfType<Component>().Where(x => x != null).Select(x => x.gameObject).FirstOrDefault();
                    //    if (obj == null) obj = DragAndDrop.objectReferences.OfType<Component>().Where(x => x != null).Select(x => x.gameObject).FirstOrDefault();
                    //}

                    //if (obj == null && type == typeof(Sprite))
                    //{
                    //    foreach (var dragObj in draggingObjects.Concat(DragAndDrop.objectReferences))
                    //    {
                    //        if (dragObj is Texture tex && tex != null && AssetDatabase.Contains(tex))
                    //        {
                    //            var assetPath = AssetDatabase.GetAssetPath(tex);
                    //            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

                    //            if (sprite != null)
                    //            {
                    //                obj = sprite;
                    //                break;
                    //            }
                    //        }
                    //    }
                    //}

                    bool acceptsDrag = obj != null;

                    if (acceptsDrag && allowSceneObjects == false)
                    {
                        var uObj = obj as UnityEngine.Object;
                        if (uObj != null)
                        {
                            if (typeof(Component).IsAssignableFrom(uObj.GetType()))
                            {
                                uObj = ((Component)uObj).gameObject;
                            }

                            acceptsDrag = EditorUtility.IsPersistent(uObj);
                        }
                    }

                    if (acceptsDrag)
                    {
                        hoveringAcceptedDropZone = id;
                        bool move = Event.current.modifiers != EventModifiers.Control && draggingId != 0 && currentDragIsMove;
                        if (move)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                        }
                        else
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        }

                        Event.current.Use();
                        if (t == EventType.DragPerform)
                        {
                            if (!move)
                            {
                                draggingId = 0;
                                //preventDropAreaRect = new Rect();
                            }

                            // Calling this here makes Unity crash on MacOS
                            // DragAndDrop.objectReferences = new UnityEngine.Object[] { };
                            DragAndDrop.AcceptDrag();
                            GUI.changed = true;
                            GUIHelper.RemoveFocusControl();
                            draggingObjects = new object[] { };
                            currentDragIsMove = false;
                            isAccepted = true;
                            dropZoneObject = value;
                            preventDropAreaRect = new Rect();
                            DragAndDrop.activeControlID = 0;
                            GUIHelper.RequestRepaint();
                            return obj;
                        }
                        else
                        {
                            DragAndDrop.activeControlID = id;
                        }
                    }
                    else
                    {
                        hoveringAcceptedDropZone = 0;
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                }
            }
            else
            {
                if (hoveringAcceptedDropZone == id)
                {
                    hoveringAcceptedDropZone = 0;
                }
            }

            return value;
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static object DropZone(Rect rect, object value, Type type, int id)
        {
            return DropZone(rect, value, type, true, id);
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static object DropZone(Rect rect, object value, Type type)
        {
            var id = GetDragAndDropId(rect);
            return DropZone(rect, value, type, id);
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static object DropZone(Rect rect, object value, Type type, bool allowSceneObjects)
        {
            var id = GetDragAndDropId(rect);
            return DropZone(rect, value, type, allowSceneObjects, id);
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static T DropZone<T>(Rect rect, T value, bool allowSceneObjects, int id)
        {
            return (T)DropZone(rect, value, typeof(T), allowSceneObjects, id);
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static T DropZone<T>(Rect rect, T value, int id)
        {
            return (T)DropZone(rect, value, typeof(T), id);
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static T DropZone<T>(Rect rect, T value, bool allowSceneObjects)
        {
            var id = GetDragAndDropId(rect);
            return (T)DropZone(rect, value, typeof(T), allowSceneObjects, id);
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static T DropZone<T>(Rect rect, T value)
        {
            var id = GetDragAndDropId(rect);
            return (T)DropZone(rect, value, typeof(T), id);
        }

        /// <summary>
        /// Disalloweds the drop area for next drag zone. Follow this function call by a DragZone.
        /// </summary>
        public static void DisallowedDropAreaForNextDragZone(Rect rect)
        {
            nextPreventDropAreaRect = rect;
        }

        public static bool PrevDragZoneWasClicked() => PrevDragAndDropId == mouseClickedId;

        /// <summary>
        /// A draggable zone for both Unity and non-unity objects.
        /// </summary>
        public static object DragZone(Rect rect, object value, Type type, bool allowMove, bool allowSwap, int id)
        {
            if (ReferenceEquals(value, null))
            {
                return null;
            }

            var unityObject = value as UnityEngine.Object;
            var isUnityObject = (bool) unityObject;

            if (isUnityObject && unityObject == null)
            {
                return value;
            }

            bool isMouseOver = rect.Contains(Event.current.mousePosition);

            if (!IsDragging)
            {
                draggingId = 0;
                mouseDownDragAndDropId = -1;
            }

            switch (Event.current.type)
            {
                case EventType.MouseDrag:
                {
                    if (!isMouseOver || mouseDownDragAndDropId == id)
                    {
                        break;
                    }

                    GUIHelper.RemoveFocusControl();

                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.activeControlID = 0;

                    if (isUnityObject)
                    {
                        DragAndDrop.objectReferences = new[] {unityObject};
                        draggingObjects = new object[] { };
                    }
                    else
                    {
                        DragAndDrop.objectReferences = new UnityEngine.Object[] { };
                        draggingObjects = new[] {value};
                    }

                    DragAndDrop.StartDrag("Dragging");

                    GUIHelper.RequestRepaint();
                    isAccepted = false;
                    dropZoneObject = null;
                    draggingId = id;
                    currentDragIsMove = allowMove;
                    mouseDownDragAndDropId = id;
                    preventDropAreaRect = rect.Expand(1);
                    OnDragStartMouseScreenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

                    Event.current.Use();
                    break;
                }

                case EventType.MouseUp:
                {
                    if (mouseDownDragAndDropId != id)
                    {
                        break;
                    }

                    mouseClickedId = id;
                    mouseDownDragAndDropId = -1;

                    Event.current.Use();
                    break;
                }
            }

            if (draggingId != id)
            {
                return value;
            }

            if (isAccepted)
            {
                GUIHelper.RequestRepaint();
                GUI.changed = true;
                draggingId = 0;
                preventDropAreaRect = new Rect();

                // TODO: Validate drop zone object, and only return that if it's assignable from type.

                if (allowMove)
                {
                    if (allowSwap)
                    {
                        if (dropZoneObject != null)
                        {
                            if (ConvertUtility.TryWeakConvert(dropZoneObject, type, out object newObj))
                            {
                                return newObj;
                            }
                        }
                    }

                    return null;
                }
            }

            return value;
        }

        /// <summary>
        /// A draggable zone for both Unity and non-unity objects.
        /// </summary>
        public static object DragZone(Rect rect, object value, Type type, bool allowMove, bool allowSwap)
        {
            var id = GetDragAndDropId(rect);
            return DragZone(rect, value, type, allowMove, allowSwap, id);
        }

        /// <summary>
        /// A draggable zone for both Unity and non-unity objects.
        /// </summary>
        public static T DragZone<T>(Rect rect, T value, bool allowMove, bool allowSwap, int id)
        {
            return (T)DragZone(rect, value, typeof(T), allowMove, allowSwap, id);
        }

        /// <summary>
        /// A draggable zone for both Unity and non-unity objects.
        /// </summary>
        public static T DragZone<T>(Rect rect, T value, bool allowMove, bool allowSwap)
        {
            var id = GetDragAndDropId(rect);
            return (T)DragZone(rect, value, typeof(T), allowMove, allowSwap, id);
        }
    }
}
#endif