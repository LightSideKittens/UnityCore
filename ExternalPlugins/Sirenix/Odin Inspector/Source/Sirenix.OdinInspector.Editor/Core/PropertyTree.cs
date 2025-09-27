//-----------------------------------------------------------------------
// <copyright file="PropertyTree.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Validation;
    using Internal;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// <para>Represents a set of values of the same type as a tree of properties that can be drawn in the inspector, and provides an array of utilities for querying the tree of properties.</para>
    /// </summary>
    public abstract class PropertyTree : IDisposable
    {
        private static GUIFrameCounter frameCounter = new();
        private static int drawnInspectorDepthCount = 0;
        private static ValueGetter<SerializedObject, IntPtr> SerializedObject_nativeObjectPtrGetter;

        /// <summary>
        /// Delegate for on property value changed callback.
        /// </summary>
        public delegate void OnPropertyValueChangedDelegate(InspectorProperty property, int selectionIndex);

        private MethodInfo onValidateMethod;
        private OdinAttributeProcessorLocator attributeProcessorLocator;
        private OdinPropertyResolverLocator propertyResolverLocator;
        private DrawerChainResolver drawerChainResolver;
        private StateUpdaterLocator stateUpdaterLocator;
        private SerializationBackend serializationBackend;

        internal float ContextWidth;
        public bool RecordUndoForChanges;

        /// <summary>
        /// This will be replaced by an IMGUIDrawingComponent in patch 3.2.
        /// </summary>
        internal bool TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL = true;

        protected SerializedProperty monoScriptProperty;
        protected bool monoScriptPropertyHasBeenGotten;

        private PropertySearchFilter searchFilter;
        public bool AllowSearchFiltering = true;

        private static WeakReferenceEventListener<PropertyTree> UndoEventListener;

        public static readonly EditorPrefBool EnableLeakDetection = new("OdinPropertyTree_EnableLeakDetection", true);

        static PropertyTree()
        {
            string nativeObjectPtrName = UnityVersion.IsVersionOrGreater(2018, 3) ? "m_NativeObjectPtr" : "m_Property";

            var nativeObjectPtrField = typeof(SerializedObject).GetField(nativeObjectPtrName, Flags.InstanceAnyVisibility);

            if (nativeObjectPtrField != null)
            {
                SerializedObject_nativeObjectPtrGetter = EmitUtilities.CreateInstanceFieldGetter<SerializedObject, IntPtr>(nativeObjectPtrField);
            }
            else
            {
                Debug.LogWarning("The internal Unity field SerializedObject.m_Property (< 2018.3)/SerializedObject.m_NativeObjectPtr (>= 2018.3) has been renamed in this version of Unity!");
            }

            UndoEventListener = new WeakReferenceEventListener<PropertyTree>((tree, args) =>
            {
                tree.InvokeOnUndoRedoPerformed();
            });

            Undo.undoRedoPerformed += GlobalUndoEventSubscription;
        }

        private static void GlobalUndoEventSubscription()
        {
            UndoEventListener.InvokeEvent(null);
        }

        /// <summary>
        /// The component providers that create components for each property in the tree. If you change this list after the tree has been used, you should call tree.RootProperty.RefreshSetup() to make the changes update properly throughout the tree.
        /// </summary>
        public readonly List<ComponentProvider> ComponentProviders = new();

        /// <summary>
        /// The <see cref="SerializedObject"/> that this tree represents, if the tree was created for a <see cref="SerializedObject"/>.
        /// </summary>
        public abstract SerializedObject UnitySerializedObject { get; }

        /// <summary>
        /// The current update ID of the tree. This is incremented once, each update, and is used by <see cref="InspectorProperty.Update(bool)"/> to avoid updating multiple times in the same update round.
        /// </summary>
        public abstract int UpdateID { get; }

        /// <summary>
        /// The type of the values that the property tree represents.
        /// </summary>
        public abstract Type TargetType { get; }

        /// <summary>
        /// The actual values that the property tree represents.
        /// </summary>
        public abstract ImmutableList<object> WeakTargets { get; }

        /// <summary>
        /// The number of root properties in the tree.
        /// </summary>
        public abstract int RootPropertyCount { get; }

        /// <summary>
        /// The prefab modification handler of the tree.
        /// </summary>
        public abstract PrefabModificationHandler PrefabModificationHandler { get; }

        /// <summary>
        /// Whether this property tree also represents members that are specially serialized by Odin.
        /// </summary>
        [Obsolete("This value is no longer guaranteed to be correct, as it may have different answers for different properties in the tree. Instead look at InspectorProperty.SerializationRoot to determine whether specially serialized members might be included.", true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public abstract bool IncludesSpeciallySerializedMembers { get; }

        /// <summary>
        /// Gets a value indicating whether or not to draw the mono script object field at the top of the property tree.
        /// </summary>
        public bool DrawMonoScriptObjectField { get; set; }

        /// <summary>
        /// Gets a value indicating whether or not the PropertyTree is inspecting a static type.
        /// </summary>
        public bool IsStatic { get; protected set; }

        /// <summary>
        /// The serialization backend used to determine how to draw this property tree. Set this to control.
        /// </summary>
        public SerializationBackend SerializationBackend
        {
            get
            {
                if (serializationBackend == null)
                {
                    var odinSerialized = InspectorPropertyInfoUtility.TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cached(TargetType);
                    serializationBackend = odinSerialized ? SerializationBackend.Odin : SerializationBackend.Unity;
                }

                return serializationBackend;
            }

            set
            {
                if (serializationBackend != value)
                {
                    serializationBackend = value;
                    DisposeAndResetRootProperty();
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="OdinAttributeProcessorLocator"/> for the PropertyTree.
        /// </summary>
        public OdinAttributeProcessorLocator AttributeProcessorLocator
        {
            get
            {
                if (attributeProcessorLocator == null)
                {
                    attributeProcessorLocator = DefaultOdinAttributeProcessorLocator.Instance;
                }

                return attributeProcessorLocator;
            }
            set
            {
                if (!ReferenceEquals(attributeProcessorLocator, value))
                {
                    attributeProcessorLocator = value;
                    RootProperty.RefreshSetup();
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="OdinPropertyResolverLocator"/> for the PropertyTree.
        /// </summary>
        public OdinPropertyResolverLocator PropertyResolverLocator
        {
            get
            {
                if (propertyResolverLocator == null)
                {
                    propertyResolverLocator = DefaultOdinPropertyResolverLocator.Instance;
                }

                return propertyResolverLocator;
            }
            set
            {
                if (!ReferenceEquals(propertyResolverLocator, value))
                {
                    propertyResolverLocator = value;
                    RootProperty.RefreshSetup();
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Sirenix.OdinInspector.Editor.DrawerChainResolver"/> for the PropertyTree.
        /// </summary>
        public DrawerChainResolver DrawerChainResolver
        {
            get
            {
                if (drawerChainResolver == null)
                {
                    drawerChainResolver = DefaultDrawerChainResolver.Instance;
                }

                return drawerChainResolver;
            }
            set
            {
                if (!ReferenceEquals(drawerChainResolver, value))
                {
                    drawerChainResolver = value;
                    RootProperty.RefreshSetup();
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Sirenix.OdinInspector.Editor.StateUpdaterLocator"/> for the PropertyTree.
        /// </summary>
        public StateUpdaterLocator StateUpdaterLocator
        {
            get
            {
                if (stateUpdaterLocator == null)
                {
                    stateUpdaterLocator = DefaultStateUpdaterLocator.Instance;
                }

                return stateUpdaterLocator;
            }
            set
            {
                if (!ReferenceEquals(stateUpdaterLocator, value))
                {
                    stateUpdaterLocator = value;
                    RootProperty.RefreshSetup();
                }
            }
        }

        /// <summary>
        /// An event that is invoked whenever an undo or a redo is performed in the inspector.
        /// The advantage of using this event on a property tree instance instead of
        /// <see cref="Undo.undoRedoPerformed"/> is that this event will be desubscribed from
        /// <see cref="Undo.undoRedoPerformed"/> when the selection changes and the property
        /// tree is no longer being used, allowing the GC to collect the property tree.
        /// </summary>
        public event Action OnUndoRedoPerformed;

        /// <summary>
        /// This event is invoked whenever the value of any property in the entire property tree is changed through the property system.
        /// </summary>
        public event OnPropertyValueChangedDelegate OnPropertyValueChanged;

        private System.Diagnostics.StackTrace allocationTrace;

        /// <summary>
        /// Creates a new <see cref="PropertyTree" /> for all target values of a <see cref="SerializedObject" />.
        /// </summary>
        public PropertyTree()
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(TargetType))
            {
                onValidateMethod = GetOnValidateMethod(TargetType);
                //Undo.undoRedoPerformed += this.InvokeOnUndoRedoPerformed;
                UndoEventListener.SubscribeListener(this);
            }

            if (EnableLeakDetection.Value)
            {
                allocationTrace = new System.Diagnostics.StackTrace(true);
            }
        }

        private static MethodInfo GetOnValidateMethod(Type type)
        {
            var method = type.GetMethod("OnValidate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (method == null)
            {
                type = type.BaseType;

                while (method == null && type != null)
                {
                    method = type.GetMethod("OnValidate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    type = type.BaseType;
                }
            }

            return method;
        }


        internal void InvokeOnPropertyValueChanged(InspectorProperty property, int selectionIndex)
        {
            if (OnPropertyValueChanged != null)
            {
                try
                {
                    OnPropertyValueChanged(property, selectionIndex);
                }
                catch (ExitGUIException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    if (ex.IsExitGUIException())
                    {
                        throw ex.AsExitGUIException();
                    }

                    Debug.LogException(ex);
                }
            }
        }

        protected abstract bool HasRootPropertyYet { get; }

        /// <summary>
        /// Gets the root property of the tree.
        /// </summary>
        public abstract InspectorProperty RootProperty { get; }

        /// <summary>
        /// Gets the secret root property of the tree, which hosts the property resolver used to resolve the "actual" root properties of the tree.
        /// </summary>
        [Obsolete("Use RootProperty instead; the root is no longer considered 'secret'.", false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public abstract InspectorProperty SecretRootProperty { get; }

        public abstract void CleanForCachedReuse();
        public abstract void SetTargets(params object[] newTargets);
        public abstract void SetSerializedObject(SerializedObject serializedObject);

        /// <summary>
        /// Registers that a given property is dirty and needs its changes to be applied at the end of the current frame.
        /// </summary>
        public abstract void RegisterPropertyDirty(InspectorProperty property);

        /// <summary>
        /// Schedules a delegate to be invoked at the end of the current GUI frame.
        /// </summary>
        /// <param name="action">The action delegate to be delayed.</param>
        public abstract void DelayAction(Action action);

        /// <summary>
        /// Schedules a delegate to be invoked at the end of the next Repaint GUI frame.
        /// </summary>
        /// <param name="action">The action to be delayed.</param>
        public abstract void DelayActionUntilRepaint(Action action);

        /// <summary>
        /// Enumerates over the properties of the tree.
        /// </summary>
        /// <param name="includeChildren">Whether to include children of the root properties or not. If set to true, every property in the entire tree will be enumerated.</param>
        /// <param name="onlyVisible">Whether to only include visible properties. Properties whose parents are invisible are considered invisible.</param>
        public abstract IEnumerable<InspectorProperty> EnumerateTree(bool includeChildren = true, bool onlyVisible = false);

        /// <summary>
        /// Gets the property at the given path. Note that this is the path found in <see cref="InspectorProperty.Path" />, not the Unity path.
        /// </summary>
        /// <param name="path">The path of the property to get.</param>
        public abstract InspectorProperty GetPropertyAtPath(string path);

        /// <summary>
        /// Gets the property at the given path. Note that this is the path found in <see cref="InspectorProperty.Path" />, not the Unity path.
        /// </summary>
        /// <param name="path">The path of the property to get.</param>
        /// <param name="closestProperty"></param>
        public abstract InspectorProperty GetPropertyAtPath(string path, out InspectorProperty closestProperty);

        /// <summary>
        /// Gets the property at the given Unity path.
        /// </summary>
        /// <param name="path">The Unity path of the property to get.</param>
        public abstract InspectorProperty GetPropertyAtUnityPath(string path);

        /// <summary>
        /// Gets the property at the given Unity path.
        /// </summary>
        /// <param name="path">The Unity path of the property to get.</param>
        /// <param name="closestProperty"></param>
        public abstract InspectorProperty GetPropertyAtUnityPath(string path, out InspectorProperty closestProperty);

        /// <summary>
        /// Gets the property at the given deep reflection path.
        /// </summary>
        /// <param name="path">The deep reflection path of the property to get.</param>
        [Obsolete("Use GetPropertyAtPrefabModificationPath instead.", false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public InspectorProperty GetPropertyAtDeepReflectionPath(string path)
        {
            return GetPropertyAtPrefabModificationPath(path);
        }

        /// <summary>
        /// Gets the property at the given Odin prefab modification path.
        /// </summary>
        /// <param name="path">The prefab modification path of the property to get.</param>
        public abstract InspectorProperty GetPropertyAtPrefabModificationPath(string path);

        /// <summary>
        /// Gets the property at the given Odin prefab modification path.
        /// </summary>
        /// <param name="path">The prefab modification path of the property to get.</param>
        /// <param name="closestProperty"></param>
        public abstract InspectorProperty GetPropertyAtPrefabModificationPath(string path, out InspectorProperty closestProperty);

        /// <summary>
        /// <para>Draw the property tree, and handles management of undo, as well as marking scenes and drawn assets dirty.</para>
        /// <para>
        /// This is a shorthand for calling
        /// <see cref="InspectorUtilities.BeginDrawPropertyTree(PropertyTree, bool)"/>,
        /// <see cref="InspectorUtilities.DrawPropertiesInTree(PropertyTree)"/> and .
        /// <see cref="InspectorUtilities.EndDrawPropertyTree(PropertyTree)"/>.
        /// </para>
        /// </summary>
        public void Draw(bool applyUndo = true)
        {
            BeginDraw(applyUndo);
            DrawProperties();
            EndDraw();

            //InspectorUtilities.BeginDrawPropertyTree(this, applyUndo);
            //InspectorUtilities.DrawPropertiesInTree(this);
            //InspectorUtilities.EndDrawPropertyTree(this);
        }

        public void BeginDraw(bool withUndo)
        {
            // This provides GUIHelper with a more reliable context-width, so that Unity
            // can better figure out what the label width is non-repaint events.
            // - Bjarke
            if (Event.current.type == EventType.Repaint)
            {
                ContextWidth = GUIHelper.ContextWidth;
            }
            GUIHelper.BetterContextWidth = ContextWidth;

            if (frameCounter.Update().IsNewFrame)
            {
                drawnInspectorDepthCount = 0;
            }
            drawnInspectorDepthCount++;

            if (this == null)
            {
                throw new ArgumentNullException("tree");
            }

            if (!IsStatic)
            {
                for (int i = 0; i < WeakTargets.Count; i++)
                {
                    if (WeakTargets[i] == null)
                    {
                        GUILayout.Label("An inspected object has been destroyed; please refresh the inspector.");
                        return;
                    }
                }
            }

            UpdateTree();

            RecordUndoForChanges = false;

            if (withUndo)
            {
                if (TargetType.ImplementsOrInherits(typeof(UnityEngine.Object)) == false)
                {
                    Debug.LogError("Automatic inspector undo only works when you're inspecting a type derived from UnityEngine.Object, and you are inspecting '" + TargetType.GetNiceName() + "'.");
                }
                else
                {
                    RecordUndoForChanges = true;
                }
            }

            RootProperty.OnStateUpdate(UpdateID);

            if (PrefabModificationHandler.HasNestedOdinPrefabData)
            {
                SirenixEditorGUI.ErrorMessageBox("A selected object is serialized by Odin, is a prefab, and contains nested prefab data (IE, more than one possible layer of prefab modifications). This is NOT CURRENTLY SUPPORTED by Odin - therefore, modification of all Odin-serialized values has been disabled for this object.\n\nThere is a strong likelihood that Odin-serialized values will be corrupt and/or wrong in other ways, as well as a very real risk that your computer may spontaneously combust and turn into a flaming wheel of cheese.");
            }


            if (DrawMonoScriptObjectField)
            {
                if (!monoScriptPropertyHasBeenGotten)
                {
                    if (UnitySerializedObject != null)
                    {
                        monoScriptProperty = GetUnitySerializedObjectNoUpdate().FindProperty("m_Script");
                    }

                    monoScriptPropertyHasBeenGotten = true;
                }

                if (monoScriptProperty != null)
                {
                    GUIHelper.PushGUIEnabled(false);
                    EditorGUILayout.PropertyField(monoScriptProperty);
                    GUIHelper.PopGUIEnabled();
                }
            }
        }

        public void DrawProperties()
        {
            if (AllowSearchFiltering && searchFilter != null)
            {
                if (DrawSearch()) return;
            }

            RootProperty.Draw(null);
        }

        /// <summary>
        /// <para>Draws a search bar for the property tree, and draws the search results if the search bar is used.</para>
        /// <para>If this method returns true, the property tree should generally not be drawn normally afterwards.</para>
        /// <para>Note that this method will throw exceptions if the property tree is not set up to be searchable; for that, see <see cref="SetSearchable(bool, SearchableAttribute)"/>.</para>
        /// </summary>
        /// <returns>True if the property tree is being searched and is currently drawing its search results, otherwise false.</returns>
        public bool DrawSearch()
        {
            if (AllowSearchFiltering && searchFilter != null)
            {
                searchFilter.DrawDefaultSearchFieldLayout(null);

                if (searchFilter.HasSearchResults)
                {
                    searchFilter.DrawSearchResults();
                    return true;
                }

                return false;
            }
            else throw new InvalidOperationException("Search is not currently enabled on this PropertyTree. Call SetSearchable(true) first.");
        }

        public void EndDraw()
        {
            InvokeDelayedActions();

            var so = GetUnitySerializedObjectNoUpdate();

            if (so != null)
            {
                if (SerializedObject_nativeObjectPtrGetter != null)
                {
                    IntPtr ptr = SerializedObject_nativeObjectPtrGetter(ref so);

                    if (ptr == IntPtr.Zero)
                    {
                        // SerializedObject has been disposed, likely due to a scene change invoked from GUI code.
                        // BAIL OUT! :D Crashes will happen.
                        return;
                    }
                }

                if (RecordUndoForChanges)
                {
                    so.ApplyModifiedProperties();
                }
                else
                {
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            bool appliedOdinChanges = false;

            if (ApplyChanges())
            {
                appliedOdinChanges = true;
                GUIHelper.RequestRepaint();
            }

            // This is very important, as applying changes may cause more actions to be delayed
            InvokeDelayedActions();

            if (appliedOdinChanges)
            {
                InvokeOnValidate();

                if (PrefabModificationHandler.HasPrefabs)
                {
                    var targets = WeakTargets;

                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (PrefabModificationHandler.TargetPrefabs[i] == null) continue;

                        var target = (UnityEngine.Object)targets[i];
                        PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                    }
                }
            }

            if (RecordUndoForChanges)
            {
                if (appliedOdinChanges && Application.platform == RuntimePlatform.OSXEditor)
                {
                    // If we don't do this on Mac, the Undo system will for some reason revert
                    // changes we just made in some cases by invoking deserialization with the
                    // old data, most prominently in the case of drag-and-dropping list elements.

                    // This doesn't *actually* become a new Undo record, all it does is make
                    // sure that the correct object state is registered in the Undo system,
                    // such that deserialization calls triggered by Unity's Undo system do 
                    // not wipe the data changes we just made.

                    Undo.IncrementCurrentGroup();

                    foreach (var target in WeakTargets)
                    {
                        if (target is UnityEngine.Object)
                        {
                            var obj = target as UnityEngine.Object;
                            Undo.RecordObject(obj, "Odin change to " + obj.name);
                        }
                    }
                }

                Undo.FlushUndoRecordObjects();
            }

            drawnInspectorDepthCount--;
        }

        /// <summary>
        /// Gets a Unity property for the given Odin or Unity path. If there is no <see cref="SerializedObject" /> for this property tree, or no such property is found in the <see cref="SerializedObject" />, a property will be emitted using <see cref="UnityPropertyEmitter" />.
        /// </summary>
        /// <param name="path">The Odin or Unity path to the property to get.</param>
        public SerializedProperty GetUnityPropertyForPath(string path)
        {
            FieldInfo fieldInfo;
            return GetUnityPropertyForPath(path, out fieldInfo);
        }

        /// <summary>
        /// Gets a Unity property for the given Odin or Unity path. If there is no <see cref="SerializedObject" /> for this property tree, or no such property is found in the <see cref="SerializedObject" />, a property will be emitted using <see cref="UnityPropertyEmitter" />.
        /// </summary>
        /// <param name="path">The Odin or Unity path to the property to get.</param>
        /// <param name="backingField">The backing field of the Unity property.</param>
        public abstract SerializedProperty GetUnityPropertyForPath(string path, out FieldInfo backingField);

        /// <summary>
        /// Checks whether a given object instance is referenced anywhere in the tree, and if it is, gives the path of the first time the object reference was encountered as an out parameter.
        /// </summary>
        /// <param name="value">The reference value to check.</param>
        /// <param name="referencePath">The first found path of the object.</param>
        public abstract bool ObjectIsReferenced(object value, out string referencePath);

        /// <summary>
        /// Gets the number of references to a given object instance in this tree.
        /// </summary>
        public abstract int GetReferenceCount(object reference);

        /// <summary>
        /// Updates all properties in the entire tree, and validates the prefab state of the tree, if applicable.
        /// </summary>
        public abstract void UpdateTree();

        /// <summary>
        /// Replaces all occurrences of a value with another value, in the entire tree.
        /// </summary>
        /// <param name="from">The value to find all instances of.</param>
        /// <param name="to">The value to replace the found values with.</param>
        public abstract void ReplaceAllReferences(object from, object to);

        /// <summary>
        /// Gets the root tree property at a given index.
        /// </summary>
        /// <param name="index">The index of the property to get.</param>
        public abstract InspectorProperty GetRootProperty(int index);

        /// <summary>
        /// Invokes the actions that have been delayed using <see cref="DelayAction(Action)"/> and <see cref="DelayActionUntilRepaint(Action)"/>.
        /// </summary>
        public abstract void InvokeDelayedActions();

        /// <summary>
        /// Applies all changes made with properties to the inspected target tree values, and marks all changed Unity objects dirty.
        /// </summary>
        /// <returns>true if any values were changed, otherwise false</returns>
        public abstract bool ApplyChanges();

        internal abstract SerializedObject GetUnitySerializedObjectNoUpdate();

        /// <summary>
        /// Invokes the OnValidate method on the property tree's targets if they are derived from <see cref="UnityEngine.Object"/> and have the method defined.
        /// </summary>
        public void InvokeOnValidate()
        {
            if (onValidateMethod != null)
            {
                for (int i = 0; i < WeakTargets.Count; i++)
                {
                    try
                    {
                        onValidateMethod.Invoke(WeakTargets[i], null);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Registers an object reference to a given path; this is used to ensure that objects are always registered after having been encountered once.
        /// </summary>
        /// <param name="reference">The referenced object.</param>
        /// <param name="property">The property that contains the reference.</param>
        internal abstract void ForceRegisterObjectReference(object reference, InspectorProperty property);

        /// <summary>
        /// Creates a PropertyTree to inspect the static values of the given type.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <returns>A PropertyTree instance for inspecting the type.</returns>
        public static PropertyTree CreateStatic(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return ((PropertyTree)Activator.CreateInstance(typeof(PropertyTree<>).MakeGenericType(type))).SetUpForIMGUIDrawing();
        }

        /// <summary>
        /// Creates a new <see cref="PropertyTree" /> for a given target value.
        /// </summary>
        /// <param name="target">The target to create a tree for.</param>
        /// <exception cref="System.ArgumentNullException">target is null</exception>
        public static PropertyTree Create(object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return Create((IList)new object[] { target }, null, (SerializationBackend)null);
        }

        /// <summary>
        /// Creates a new <see cref="PropertyTree" /> for a given target value.
        /// </summary>
        /// <param name="target">The target to create a tree for.</param>
        /// <param name="backend">The serialization backend to use for the tree root.</param>
        /// <exception cref="System.ArgumentNullException">target is null</exception>
        public static PropertyTree Create(object target, SerializationBackend backend)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return Create((IList)new object[] { target }, null, backend);
        }

        /// <summary>
        /// <para>Creates a new <see cref="PropertyTree" /> for a set of given target values.</para>
        /// <para>Note that the targets all need to be of the same type.</para>
        /// </summary>
        /// <param name="targets">The targets to create a tree for.</param>
        /// <exception cref="System.ArgumentNullException">targets is null</exception>
        public static PropertyTree Create(params object[] targets)
        {
            if (targets == null)
            {
                throw new ArgumentNullException("targets");
            }

            return Create((IList)targets);
        }

        /// <summary>
        /// Creates a new <see cref="PropertyTree" /> for all target values of a <see cref="SerializedObject" />.
        /// </summary>
        /// <param name="serializedObject">The serialized object to create a tree for.</param>
        /// <exception cref="System.ArgumentNullException">serializedObject is null</exception>
        public static PropertyTree Create(SerializedObject serializedObject)
        {
            if (serializedObject == null)
            {
                throw new ArgumentNullException("serializedObject");
            }

            return Create(serializedObject.targetObjects, serializedObject);
        }

        /// <summary>
        /// Creates a new <see cref="PropertyTree" /> for all target values of a <see cref="SerializedObject" />.
        /// </summary>
        /// <param name="serializedObject">The serialized object to create a tree for.</param>
        /// <exception cref="System.ArgumentNullException">serializedObject is null</exception>
        /// <param name="backend">The serialization backend to use for the tree root.</param>
        public static PropertyTree Create(SerializedObject serializedObject, SerializationBackend backend)
        {
            if (serializedObject == null)
            {
                throw new ArgumentNullException("serializedObject");
            }

            return Create(serializedObject.targetObjects, serializedObject, backend);
        }

        /// <summary>
        /// <para>Creates a new <see cref="PropertyTree"/> for a set of given target values.</para>
        /// <para>Note that the targets all need to be of the same type.</para>
        /// </summary>
        /// <param name="targets">The targets to create a tree for.</param>
        public static PropertyTree Create(IList targets)
        {
            return Create(targets, null, (SerializationBackend)null);
        }

        /// <summary>
        /// <para>Creates a new <see cref="PropertyTree"/> for a set of given target values.</para>
        /// <para>Note that the targets all need to be of the same type.</para>
        /// </summary>
        /// <param name="targets">The targets to create a tree for.</param>
        /// <param name="backend">The serialization backend to use for the tree root.</param>
        public static PropertyTree Create(IList targets, SerializationBackend backend)
        {
            return Create(targets, null, backend);
        }

        /// <summary>
        /// <para>Creates a new <see cref="PropertyTree"/> for a set of given target values, represented by a given <see cref="SerializedObject"/>.</para>
        /// <para>Note that the targets all need to be of the same type.</para>
        /// </summary>
        /// <param name="targets">The targets to create a tree for.</param>
        /// <param name="serializedObject">The serialized object to create a tree for. Note that the target values of the given <see cref="SerializedObject"/> must be the same values given in the targets parameter.</param>
        public static PropertyTree Create(IList targets, SerializedObject serializedObject)
        {
            return Create(targets, serializedObject, (SerializationBackend)null);
        }

        /// <summary>
        /// <para>Creates a new <see cref="PropertyTree"/> for a set of given target values, represented by a given <see cref="SerializedObject"/>.</para>
        /// <para>Note that the targets all need to be of the same type.</para>
        /// </summary>
        /// <param name="targets">The targets to create a tree for.</param>
        /// <param name="serializedObject">The serialized object to create a tree for. Note that the target values of the given <see cref="SerializedObject"/> must be the same values given in the targets parameter.</param>
        /// <param name="backend">The serialization backend to use for the tree root.</param>
        public static PropertyTree Create(IList targets, SerializedObject serializedObject, SerializationBackend backend)
        {
            if (targets == null)
            {
                throw new ArgumentNullException("targets");
            }

            if (targets.Count == 0)
            {
                throw new ArgumentException("There must be at least one target.");
            }

            if (serializedObject != null)
            {
                bool valid = true;
                var targetObjects = serializedObject.targetObjects;

                if (targets.Count != targetObjects.Length)
                {
                    valid = false;
                }
                else
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (!ReferenceEquals(targets[i], targetObjects[i]))
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                if (!valid)
                {
                    throw new ArgumentException("Given target array must be identical in length and content to the target objects array in the given serializedObject.");
                }
            }

            Type targetType = null;

            for (int i = 0; i < targets.Count; i++)
            {
                Type otherType;
                object target = targets[i];

                if (ReferenceEquals(target, null))
                {
                    throw new ArgumentException("Target at index " + i + " was null.");
                }

                if (i == 0)
                {
                    targetType = target.GetType();
                }
                else if (targetType != (otherType = target.GetType()))
                {
                    if (targetType.IsAssignableFrom(otherType))
                    {
                        continue;
                    }
                    else if (otherType.IsAssignableFrom(targetType))
                    {
                        targetType = otherType;
                        continue;
                    }

                    throw new ArgumentException("Expected targets of type " + targetType.Name + ", but got an incompatible target of type " + otherType.Name + " at index " + i + ".");
                }
            }

            Type treeType = typeof(PropertyTree<>).MakeGenericType(targetType);
            Array targetArray;

            if (targets.GetType().IsArray && targets.GetType().GetElementType() == targetType)
            {
                targetArray = (Array)targets;
            }
            else
            {
                targetArray = Array.CreateInstance(targetType, targets.Count);
                targets.CopyTo(targetArray, 0);
            }

            if (serializedObject == null && typeof(UnityEngine.Object).IsAssignableFrom(targetType))
            {
                UnityEngine.Object[] objs = new UnityEngine.Object[targets.Count];
                targets.CopyTo(objs, 0);
                serializedObject = new SerializedObject(objs);
            }

            return ((PropertyTree)Activator.CreateInstance(treeType, targetArray, serializedObject, backend)).SetUpForIMGUIDrawing();
        }

        private void InvokeOnUndoRedoPerformed()
        {
            if (OnUndoRedoPerformed != null)
            {
                OnUndoRedoPerformed();
            }
        }

        protected void InitSearchFilter()
        {
            var searchAttr = RootProperty.GetAttribute<SearchableAttribute>();

            if (searchAttr != null)
            {
                searchFilter = new PropertySearchFilter(RootProperty, searchAttr);
            }
        }

        protected abstract void DisposeAndResetRootProperty();

        /// <summary>
        /// <para>Sets whether the property tree should be searchable or not, and allows the passing in of a custom SearchableAttribute instance to configure the search.</para>
        /// </summary>
        /// <param name="searchable">Whether the tree should be set to be searchable or not.</param>
        /// <param name="config">If the tree is set to be searchable, then if this parameter is not null, it will be used to configure the property tree search. If the parameter is null, the SearchableAttribute on the tree's <see cref="RootProperty"/> will be used. If that property has no such attribute, then default search settings will be applied.</param>
        public void SetSearchable(bool searchable, SearchableAttribute config = null)
        {
            AllowSearchFiltering = searchable;
            if (searchable)
            {
                searchFilter = new PropertySearchFilter(RootProperty, config ?? RootProperty.GetAttribute<SearchableAttribute>() ?? new SearchableAttribute());
            }
            else
            {
                searchFilter = null;
            }
        }

        #region IDisposable Support
        private volatile bool disposedValue = false;

        protected virtual void Dispose(bool finalizer)
        {
            if (!disposedValue)
            {
                if (finalizer)
                {
                    if (allocationTrace != null)
                    {
#if SIRENIX_INTERNAL
                        Debug.LogError(
#else
                        Debug.LogWarning(
#endif
                            "An Odin PropertyTree instance is being garbage collected without first having been disposed. PropertyTree instances must be disposed once they are no longer needed. This instance was allocated at the following location: \n\n" + allocationTrace.ToString());
                    }

                    // The tree is being garbage collected, but has not yet been disposed.
                    // 
                    // This will "resurrect" the property tree once and put it back on the reachable heap
                    // while it is waiting to be disposed, through the subscribed action, which will be
                    // cleared after it runs. The second time the property tree is collected by the GC,
                    // it will have been disposed properly already, and will not be resurrected again.
                    UnityEditorEventUtility.DelayActionThreadSafe(ActuallyDispose);
                }
                else
                {
                    ActuallyDispose();
                }
            }
        }

        ~PropertyTree()
        {
            Dispose(finalizer: true);
        }

        public void Dispose()
        {
            Dispose(finalizer: false);
        }

        private void ActuallyDispose()
        {
            ApplyChanges();

            if (HasRootPropertyYet)
            {
                RootProperty.Dispose();
            }
            
            UndoEventListener.DesubscribeListener(this);
            
            OnUndoRedoPerformed = null;
            OnPropertyValueChanged = null;
            DisposeInheritedStuff();
            disposedValue = true;
        }

        protected abstract void DisposeInheritedStuff();
        #endregion

        public PropertyTree SetUpForIMGUIDrawing()
        {
            TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL = true;
            ComponentProviders.Clear();

            ComponentProviders.Add(new ValidationComponentProvider(new DefaultValidatorLocator()
            {
                CustomValidatorFilter = (type) =>
                {
                    if (type.IsDefined<NoValidationInInspectorAttribute>(true))
                        return false;

                    return true;
                }
            }));

            RootProperty.RefreshSetup();
            return this;
        }

        public PropertyTree SetUpForValidation()
        {
            TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL = false;
            ComponentProviders.Clear();
            ComponentProviders.Add(new ValidationComponentProvider());
            RootProperty.RefreshSetup();
            return this;
        }

        //protected class SerializedObjectData
        //{
        //    public SerializedObject SerializedObject;
        //    public int LastUpdatedId = -1;
        //}
    }

    /// <summary>
    /// <para>Represents a set of strongly typed values as a tree of properties that can be drawn in the inspector, and provides an array of utilities for querying the tree of properties.</para>
    /// <para>This class also handles management of prefab modifications.</para>
    /// </summary>
    public sealed class PropertyTree<T> : PropertyTree
    {
        private struct PropertyPathResult
        {
            public InspectorProperty Property;
            public InspectorProperty ClosestProperty;
        }

        private static readonly bool TargetIsValueType = typeof(T).IsValueType;
        private static readonly bool TargetIsUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));

        private Dictionary<object, int> objectReferenceCounts = new(ReferenceEqualityComparer<object>.Default);
        private Dictionary<object, string> objectReferences = new(ReferenceEqualityComparer<object>.Default);

        private Dictionary<string, Dictionary<Type, SerializedProperty>> emittedUnityPropertyCache = new();
        //private Dictionary<UnityEngine.Object, SerializedObjectData> serializedObjects = new Dictionary<UnityEngine.Object, SerializedObjectData>(ReferenceEqualityComparer<UnityEngine.Object>.Default);
        //private Dictionary<string, Dictionary<Type, UnityPropertyEmitter.Handle>> emittedUnityGameObjectPropertyCache = new Dictionary<string, Dictionary<Type, UnityPropertyEmitter.Handle>>();

        private List<Action> delayedActions = new();
        private List<Action> delayedRepaintActions = new();

        private List<InspectorProperty> dirtyProperties = new();

        private T[] targets;

        private InspectorProperty rootProperty;

        private SerializedObject serializedObject;
        private int serializedObjectUpdateID;

        private int updateID = 1;
        private object[] weakTargets;
        private ImmutableList<T> immutableTargets;
        private ImmutableList<object> immutableWeakTargets;
        private bool includesSpeciallySerializedMembers;
        private PrefabModificationHandler prefabModificationHandler;
        private int prefabModificationHandler_lastUpdateID;

        private static readonly bool includesSpeciallySerializedMembers_StaticCache = InspectorPropertyInfoUtility.TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cached(typeof(T));

        protected override bool HasRootPropertyYet { get { return rootProperty != null; } }

        /// <summary>
        /// Gets the root property of the tree.
        /// </summary>
        public override InspectorProperty RootProperty
        {
            get
            {
                if (rootProperty == null)
                {
                    rootProperty = InspectorProperty.Create(
                        this,
                        null,
                        InspectorPropertyInfo.CreateValue(
                            name: "$ROOT",
                            order: 0,
                            serializationBackend: SerializationBackend,
                            getterSetter: new GetterSetter<int, T>(
                                getter: (ref int index) => targets[index],
                                setter: (ref int index, T value) => targets[index] = value),
                            attributes: null),
                        0,
                        true);

                    rootProperty.Update(true);
                }

                return rootProperty;
            }
        }

        /// <summary>
        /// Gets the secret root property of the PropertyTree.
        /// </summary>
        [Obsolete("Use RootProperty instead; the root is no longer considered 'secret'.", false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override InspectorProperty SecretRootProperty { get { return RootProperty; } }

        /// <summary>
        /// Gets the <see cref="prefabModificationHandler"/> for the PropertyTree.
        /// </summary>
        public override PrefabModificationHandler PrefabModificationHandler
        {
            get
            {
                if (prefabModificationHandler == null)
                {
                    prefabModificationHandler = new PrefabModificationHandler(this);
                }

                if (TargetIsUnityObject && prefabModificationHandler_lastUpdateID != updateID)
                {
                    prefabModificationHandler.Update();
                    prefabModificationHandler_lastUpdateID = updateID;
                }

                return prefabModificationHandler;
            }
        }

        /// <summary>
        /// The current update ID of the tree. This is incremented once, each update, and is used by <see cref="InspectorProperty.Update(bool)" /> to avoid updating multiple times in the same update round.
        /// </summary>
        public override int UpdateID { get { return updateID; } }

        /// <summary>
        /// The <see cref="SerializedObject" /> that this tree represents, if the tree was created for a <see cref="SerializedObject" />.
        /// </summary>
        public override SerializedObject UnitySerializedObject
        {
            get
            {
                if (serializedObject != null && serializedObjectUpdateID != updateID)
                {
                    serializedObjectUpdateID = updateID;
                    serializedObject.Update();
                }

                return serializedObject;
            }
        }

        /// <summary>
        /// The type of the values that the property tree represents.
        /// </summary>
        public override Type TargetType { get { return typeof(T); } }

        /// <summary>
        /// The strongly types actual values that the property tree represents.
        /// </summary>
        public ImmutableList<T> Targets
        {
            get
            {
                if (immutableTargets == null)
                {
                    immutableTargets = new ImmutableList<T>(targets);
                }

                return immutableTargets;
            }
        }

        /// <summary>
        /// The weakly types actual values that the property tree represents.
        /// </summary>
        public override ImmutableList<object> WeakTargets
        {
            get
            {
                if (immutableWeakTargets == null)
                {
                    if (weakTargets == null)
                    {
                        weakTargets = new object[targets.Length];
                        targets.CopyTo(weakTargets, 0);
                    }

                    immutableWeakTargets = new ImmutableList<object>(weakTargets);
                }
                else if (TargetIsValueType)
                {
                    targets.CopyTo(weakTargets, 0);
                }

                return immutableWeakTargets;
            }
        }

        internal override SerializedObject GetUnitySerializedObjectNoUpdate()
        {
            return serializedObject;
        }

        //// Preliminary work for adding compatibility for getting serialized objects for UnityEngine.Objects that exist further down in the tree than the root
        //private SerializedObject GetSerializedObjectFor(UnityEngine.Object obj, bool allowAutoCreate)
        //{
        //    SerializedObjectData data;
        //    if (!this.serializedObjects.TryGetValue(obj, out data))
        //    {
        //        if (!allowAutoCreate) return null;

        //        data = new SerializedObjectData()
        //        {
        //            SerializedObject = new SerializedObject(obj)
        //        };

        //        this.serializedObjects.Add(obj, data);
        //    }

        //    if (data.LastUpdatedId != this.updateID)
        //    {
        //        data.LastUpdatedId = this.updateID;
        //        data.SerializedObject.Update();
        //    }

        //    return data.SerializedObject;
        //}

        /// <summary>
        /// The number of root properties in the tree.
        /// </summary>
        public override int RootPropertyCount { get { return RootProperty.Children.Count; } }

        /// <summary>
        /// Whether this property tree also represents members that are specially serialized by Odin.
        /// </summary>
        [Obsolete("This value is no longer guaranteed to be correct, as it may have different answers for different properties in the tree. Instead look at InspectorProperty.SerializationRoot to determine whether specially serialized members might be included.", true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override bool IncludesSpeciallySerializedMembers { get { throw new NotSupportedException(); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTree{T}"/> class, inspecting only the target (<see cref="T"/>) type's static members.
        /// </summary>
        public PropertyTree()
        {
            IsStatic = true;
            targets = new T[1];

            InitSearchFilter();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTree{T}"/> class.
        /// </summary>
        /// <param name="serializedObject">The serialized object to represent.</param>
        public PropertyTree(SerializedObject serializedObject)
            : this(serializedObject.targetObjects.Cast<T>().ToArray(), serializedObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTree{T}"/> class.
        /// </summary>
        /// <param name="targets">The targets to represent.</param>
        public PropertyTree(T[] targets)
            : this(targets, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTree{T}"/> class.
        /// </summary>
        /// <param name="targets">The targets to represent.</param>
        /// <param name="serializedObject">The serialized object to represent. Note that the target values of the given <see cref="SerializedObject"/> must be the same values given in the targets parameter.</param>
        /// <exception cref="System.ArgumentNullException">targets is null</exception>
        /// <exception cref="System.ArgumentException">
        /// There must be at least one target.
        /// or
        /// A given target is a null value.
        /// </exception>
        public PropertyTree(T[] targets, SerializedObject serializedObject)
            : this(targets, serializedObject, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTree{T}"/> class.
        /// </summary>
        /// <param name="targets">The targets to represent.</param>
        /// <param name="serializedObject">The serialized object to represent. Note that the target values of the given <see cref="SerializedObject"/> must be the same values given in the targets parameter.</param>
        /// <param name="backend">The serialization backend to use for the tree root.</param>
        /// <exception cref="System.ArgumentNullException">targets is null</exception>
        /// <exception cref="System.ArgumentException">
        /// There must be at least one target.
        /// or
        /// A given target is a null value.
        /// </exception>
        public PropertyTree(T[] targets, SerializedObject serializedObject, SerializationBackend backend = null)
        {
            try
            {
                if (targets == null)
                {
                    throw new ArgumentNullException("targets");
                }

                if (targets.Length == 0)
                {
                    throw new ArgumentException("There must be at least one target.");
                }

                for (int i = 0; i < targets.Length; i++)
                {
                    if (ReferenceEquals(targets[i], null))
                    {
                        throw new ArgumentException("A target at index '" + i + "' is a null value.");
                    }
                }

                includesSpeciallySerializedMembers = includesSpeciallySerializedMembers_StaticCache;
                this.serializedObject = serializedObject;
                this.targets = targets;

                if (backend != null)
                {
                    SerializationBackend = backend;
                }

                InitSearchFilter();

                /// Preliminary work for adding compatibility for getting serialized objects for UnityEngine.Objects that exist further down in the tree than the root
                //if (serializedObject != null)
                //{
                //    var objData = new SerializedObjectData() { SerializedObject = serializedObject };

                //    for (int i = 0; i < serializedObject.targetObjects.Length; i++)
                //    {
                //        this.serializedObjects[serializedObject.targetObjects[i]] = objData;
                //    }
                //}
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Applies all changes made with properties to the inspected target tree values.
        /// </summary>
        /// <returns>
        /// true if any values were changed, otherwise false
        /// </returns>
        public override bool ApplyChanges()
        {
            bool changed = false;

            // Apply changes for dirty properties
            {
                for (int i = 0; i < dirtyProperties.Count; i++)
                {
                    var property = dirtyProperties[i];

                    IApplyableResolver resolver = property.ChildResolver as IApplyableResolver;

                    if (resolver != null && resolver.ApplyChanges())
                    {
                        changed = true;

                        if (property.BaseValueEntry != null)
                        {
                            for (int j = 0; j < property.BaseValueEntry.ValueCount; j++)
                            {
                                property.BaseValueEntry.TriggerOnValueChanged(j);
                            }

                            if (property.BaseValueEntry.ValueChangedFromPrefab)
                            {
                                for (int k = 0; k < Targets.Count; k++)
                                {
                                    PrefabModificationHandler.RegisterPrefabValueModification(property, k);
                                }
                            }
                        }
                    }

                    if (property.ValueEntry != null)
                    {
                        if (property.ValueEntry.ApplyChanges())
                        {
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        var serializationRoot = property.SerializationRoot;

                        for (int j = 0; j < serializationRoot.ValueEntry.ValueCount; j++)
                        {
                            UnityEngine.Object unityObj = serializationRoot.ValueEntry.WeakValues[j] as UnityEngine.Object;

                            if (unityObj != null)
                            {
                                InspectorUtilities.RegisterUnityObjectDirty(unityObj);
                            }
                        }
                    }
                }

                dirtyProperties.Clear();
            }

            if (changed && PrefabModificationHandler != null && PrefabModificationHandler.HasPrefabs && UnitySerializedObject != null)
            {
                DelayActionUntilRepaint(() =>
                {
                    // We make ABSOLUTELY SURE that this code runs at the *very end* of Repaint, after *all* other delayed Repaint invokes.

                    DelayActionUntilRepaint(() =>
                    {
                        for (int i = 0; i < WeakTargets.Count; i++)
                        {
                            // Before we ever call PrefabUtility.RecordPrefabInstancePropertyModifications, we MUST
                            // make sure that prefab modifications are registered and applied on the object.
                            //
                            // If we don't, there is a chance that Unity will crash, for unknown reasons.

                            var receiver = WeakTargets[i] as ISerializationCallbackReceiver;

                            if (receiver != null)
                            {
                                receiver.OnBeforeSerialize();
                            }

                            PrefabUtility.RecordPrefabInstancePropertyModifications((UnityEngine.Object)WeakTargets[i]);
                        }
                    });
                });
            }

            return changed;
        }

        /// <summary>
        /// Registers that a given property is dirty and needs its changes to be applied at the end of the current frame.
        /// </summary>
        /// <param name="property"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void RegisterPropertyDirty(InspectorProperty property)
        {
            dirtyProperties.Add(property);
        }

        /// <summary>
        /// Updates all properties in the entire tree, and validates the prefab state of the tree, if applicable.
        /// </summary>
        public override void UpdateTree()
        {
            // Changes might have been set since the last frame, during event calls that occurred outside of IMGUI.
            // Those changes should be applied before we do anything else, so they don't get lost.
            ApplyChanges();

            unchecked
            {
                updateID++;
            }

            objectReferences.Clear();
            objectReferenceCounts.Clear();

            //if (TargetIsUnityObject)
            //{
            //    this.PrefabModificationHandler.Update();
            //}

            RootProperty.Update();
        }

        /// <summary>
        /// Checks whether a given object instance is referenced anywhere in the tree, and if it is, gives the path of the first time the object reference was encountered as an out parameter.
        /// </summary>
        /// <param name="value">The reference value to check.</param>
        /// <param name="referencePath">The first found path of the object.</param>
        public override bool ObjectIsReferenced(object value, out string referencePath)
        {
            if (value is UnityEngine.Object)
            {
                referencePath = null;
                return false;
            }
            
            return objectReferences.TryGetValue(value, out referencePath);
        }

        /// <summary>
        /// Gets the number of references to a given object instance in this tree.
        /// </summary>
        /// <param name="reference"></param>
        public override int GetReferenceCount(object reference)
        {
            int count;
            objectReferenceCounts.TryGetValue(reference, out count);
            return count;
        }

        /// <summary>
        /// Gets the property at the given path. Note that this is the path found in <see cref="InspectorProperty.Path" />, not the Unity path.
        /// </summary>
        /// <param name="path">The path of the property to get.</param>
        public override InspectorProperty GetPropertyAtPath(string path)
        {
            InspectorProperty closest;
            return GetPropertyAtPath(path, out closest);
        }

        /// <summary>
        /// Gets the property at the given path. Note that this is the path found in <see cref="InspectorProperty.Path" />, not the Unity path.
        /// </summary>
        /// <param name="path">The path of the property to get.</param>
        /// <param name="closestProperty"></param>
        public override InspectorProperty GetPropertyAtPath(string path, out InspectorProperty closestProperty)
        {
            if (path == "$ROOT")
            {
                closestProperty = RootProperty;
                return RootProperty;
            }

            closestProperty = null;
            PropertyPathResult result;

            var currentPathIndex = 0;
            var nextSeparator = StringIndexOf(path, '.', currentPathIndex);
            var step = nextSeparator == -1 ? new StringSlice(path) : path.Slice(currentPathIndex, nextSeparator - currentPathIndex);
            var current = RootProperty;

            while (true)
            {
                result.ClosestProperty = current;
                current = current.Children.Get(ref step);

                if (current == null || nextSeparator == -1) break;

                currentPathIndex = nextSeparator + 1;
                nextSeparator = StringIndexOf(path, '.', currentPathIndex);
                step = nextSeparator == -1 ? path.Slice(currentPathIndex) : path.Slice(currentPathIndex, nextSeparator - currentPathIndex);
            }

            result.Property = current;

            if (result.Property == null && result.ClosestProperty != null)
            {
                int lastDot = path.LastIndexOf('.');

                if (lastDot > 0)
                {
                    var lastPathStep = path.Slice(lastDot + 1);
                    result.Property = result.ClosestProperty.Children.Get(ref lastPathStep);
                }
            }

            closestProperty = result.ClosestProperty;
            return result.Property;
        }

        [MethodImpl((MethodImplOptions)0x100)]  // Set aggressive inlining flag, for the runtimes that understand that
        private static int StringIndexOf(string str, char c, int start)
        {
            for (int i = start; i < str.Length; i++)
            {
                if (str[i] == c) return i;
            }

            return -1;
        }

        /// <summary>
        /// Finds the property at the specified unity path.
        /// </summary>
        /// <param name="path">The unity path for the property.</param>
        /// <returns>The property found at the path.</returns>
        public override InspectorProperty GetPropertyAtUnityPath(string path)
        {
            InspectorProperty closest;
            return GetPropertyAtUnityPath(path, out closest);
        }

        /// <summary>
        /// Finds the property at the specified unity path.
        /// </summary>
        /// <param name="path">The unity path for the property.</param>
        /// <param name="closestProperty"></param>
        /// <returns>The property found at the path.</returns>
        public override InspectorProperty GetPropertyAtUnityPath(string path, out InspectorProperty closestProperty)
        {
            closestProperty = null;
            PropertyPathResult result;
            result.ClosestProperty = null;

            var currentPathIndex = 0;
            var nextSeparator = StringIndexOf(path, '.', currentPathIndex);
            var step = nextSeparator == -1 ? new StringSlice(path) : path.Slice(currentPathIndex, nextSeparator - currentPathIndex);
            var current = RootProperty;

            while (true)
            {
                var next = current.Children.Get(ref step);

                // Cope with Unity's array syntax
                if (next == null && step == "Array" && nextSeparator != -1)
                {
                    var tempNextPathIndex = nextSeparator + 1;
                    var tempNextSeparator = StringIndexOf(path, '.', tempNextPathIndex);
                    var tempNextStep = tempNextSeparator == -1 ? path.Slice(tempNextPathIndex) : path.Slice(tempNextPathIndex, tempNextSeparator - tempNextPathIndex);

                    if (tempNextStep.StartsWith("data["))
                    {
                        var indexStr = tempNextStep.Slice(5, tempNextStep.Length - 6);

                        int index;
                        if (int.TryParse(indexStr.ToString(), out index))
                        {
                            // The standard for prefab collection supporting collections is "$index" for naming elements.
                            var indexName = CollectionResolverUtilities.DefaultIndexToChildName(index);

                            next = current.Children.Get(indexName);

                            if (next != null)
                            {
                                currentPathIndex = tempNextPathIndex;
                                nextSeparator = tempNextSeparator;
                                step = tempNextStep;
                            }
                        }
                    }
                }

                if (next == null && !(current.ChildResolver is ICollectionResolver))
                {
                    // If the above lookup failed, perhaps due to the concrete member being hidden in a group somewhere,
                    // recursively look through all groups in this property for concrete members with a matching name.

                    next = TryFindChildMemberPropertyWithNameFromGroups(step, current);
                }

                current = next;
                if (next == null || nextSeparator == -1) break;
                result.ClosestProperty = current;

                currentPathIndex = nextSeparator + 1;
                nextSeparator = StringIndexOf(path, '.', currentPathIndex);
                step = nextSeparator == -1 ? path.Slice(currentPathIndex) : path.Slice(currentPathIndex, nextSeparator - currentPathIndex);
            }

            result.Property = current;
            closestProperty = result.ClosestProperty;

            return result.Property;
        }

        /// <summary>
        /// Finds the property at the specified modification path.
        /// </summary>
        /// <param name="path">The prefab modification path for the property.</param>
        /// <returns>The property found at the path.</returns>
        public override InspectorProperty GetPropertyAtPrefabModificationPath(string path)
        {
            InspectorProperty closest;
            return GetPropertyAtPrefabModificationPath(path, out closest);
        }

        /// <summary>
        /// Finds the property at the specified modification path.
        /// </summary>
        /// <param name="path">The prefab modification path for the property.</param>
        /// <param name="closestProperty"></param>
        /// <returns>The property found at the path.</returns>
        public override InspectorProperty GetPropertyAtPrefabModificationPath(string path, out InspectorProperty closestProperty)
        {
            closestProperty = null;
            PropertyPathResult result;
            result.ClosestProperty = null;

            var currentPathIndex = 0;
            var nextSeparator = StringIndexOf(path, '.', currentPathIndex);
            var step = nextSeparator == -1 ? new StringSlice(path) : path.Slice(currentPathIndex, nextSeparator - currentPathIndex);
            var current = RootProperty;

            while (true)
            {
                var next = current.Children.Get(ref step);

                if (next == null && !(current.ChildResolver is ICollectionResolver))
                {
                    // If the above lookup failed, perhaps due to the concrete member being hidden in a group somewhere,
                    // recursively look through all groups in this property for concrete members with a matching name.

                    next = TryFindChildMemberPropertyWithNameFromGroups(step, current);
                }

                current = next;
                if (next == null || nextSeparator == -1) break;

                result.ClosestProperty = current;
                currentPathIndex = nextSeparator + 1;
                nextSeparator = StringIndexOf(path, '.', currentPathIndex);
                step = nextSeparator == -1 ? path.Slice(currentPathIndex) : path.Slice(currentPathIndex, nextSeparator - currentPathIndex);
            }

            result.Property = current;
            closestProperty = result.ClosestProperty;
            return result.Property;
        }

        //private static InspectorProperty CalculateLookupPropertyReachability(ref InspectorProperty closestProperty, PropertyPathResult result)
        //{
        //    if (closestProperty != null)
        //    {
        //        if (closestProperty.IsReachableFromRoot())
        //        {
        //            closestProperty.Update();
        //        }
        //        else
        //        {
        //            closestProperty = null;
        //            result.Property = null;
        //        }
        //    }

        //    if (result.Property != null)
        //    {
        //        if (!result.Property.IsReachableFromRoot())
        //            return null;

        //        result.Property.Update();
        //    }

        //    return result.Property;
        //}

        private InspectorProperty TryFindChildMemberPropertyWithNameFromGroups(StringSlice name, InspectorProperty property)
        {
            // Optimization - this shouldn't ever happen for collections, and will just be a performance hog
            if (property.ChildResolver is ICollectionResolver) return null;

            for (int i = 0; i < property.Children.Count; i++)
            {
                var child = property.Children.Get(i);

                switch (child.Info.PropertyType)
                {
                    case PropertyType.Value:
                    {
                        if (child.Info.HasSingleBackingMember && child.Name == name) return child;
                    }
                    break;

                    case PropertyType.Method:
                    continue;

                    case PropertyType.Group:
                    {
                        var found = TryFindChildMemberPropertyWithNameFromGroups(name, child);
                        if (found != null) return found;
                    }
                    break;

                    default:
                    throw new NotImplementedException(child.Info.PropertyType.ToString());
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a Unity property for the given Odin or Unity path. If there is no <see cref="SerializedObject" /> for this property tree, or no such property is found in the <see cref="SerializedObject" />, a property will be emitted using <see cref="UnityPropertyEmitter" />.
        /// </summary>
        /// <param name="path">The Odin or Unity path to the property to get.</param>
        /// <param name="backingField">The backing field of the Unity property.</param>
        public override SerializedProperty GetUnityPropertyForPath(string path, out FieldInfo backingField)
        {
            backingField = null;
            string unityPath;
            InspectorProperty prop = GetPropertyAtPath(path); 

            if (prop == null)
            {
                unityPath = InspectorUtilities.ConvertToUnityPropertyPath(path);
            }
            else
            {
                unityPath = prop.UnityPropertyPath;
            }

            SerializedProperty result = null;

            var so = UnitySerializedObject;

            if (so != null)
            {
                result = so.FindProperty(unityPath);

                if (result != null && prop != null)
                {
                    // There is both a Unity property and a Sirenix property and the backing member is a field
                    // We can assign the FieldInfo now, as it's stored in the Sirenix property (or its parent, in the case of a collection).
                    backingField = prop.Info.GetMemberInfo() as FieldInfo;

                    if (backingField == null && prop.Parent != null && prop.Parent.ChildResolver is ICollectionResolver)
                    {
                        backingField = prop.Parent.Info.GetMemberInfo() as FieldInfo;
                    }
                }
            }

            if (result == null && prop != null && prop.Info.PropertyType == PropertyType.Value)
            {
                Dictionary<Type, SerializedProperty> innerDict;

                if (!emittedUnityPropertyCache.TryGetValue(path, out innerDict))
                {
                    innerDict = new Dictionary<Type, SerializedProperty>(FastTypeComparer.Instance);
                    emittedUnityPropertyCache.Add(path, innerDict);
                }

                if (!innerDict.TryGetValue(prop.ValueEntry.TypeOfValue, out result))
                {
                    result = UnityPropertyEmitter.CreateEmittedScriptableObjectProperty(prop.Info.PropertyName, prop.ValueEntry.TypeOfValue, targets.Length);
                    innerDict.Add(prop.ValueEntry.TypeOfValue, result);
                }
                // TargetObject is sometimes destroyed or the serialized object is disposed, often when the profiler is toggled. Not sure why, but we need to handle the case.
                else if (result != null && result.serializedObject.targetObject == null)
                {
                    result = UnityPropertyEmitter.CreateEmittedScriptableObjectProperty(prop.Info.PropertyName, prop.ValueEntry.TypeOfValue, targets.Length);
                    innerDict[prop.ValueEntry.TypeOfValue] = result;
                }
                //else if (result == null)
                //{
                //    Dictionary<Type, UnityPropertyEmitter.Handle> innerGoDict;
                //    GameObject go = null;

                //    if (!this.emittedUnityGameObjectPropertyCache.TryGetValue(path, out innerGoDict))
                //    {
                //        innerGoDict = new Dictionary<Type, UnityPropertyEmitter.Handle>(FastTypeComparer.Instance);
                //        this.emittedUnityGameObjectPropertyCache.Add(path, innerGoDict);
                //    }

                //    UnityPropertyEmitter.Handle handle;

                //    if (!innerGoDict.TryGetValue(prop.ValueEntry.TypeOfValue, out handle))
                //    {
                //        handle = UnityPropertyEmitter.CreateEmittedMonoBehaviourProperty(prop.Info.PropertyName, prop.ValueEntry.TypeOfValue, this.targets.Length, ref go);
                //        innerGoDict.Add(prop.ValueEntry.TypeOfValue, handle);
                //    }
                //    else if (handle != null && handle.UnityProperty.serializedObject.targetObject == null)
                //    {
                //        handle.Dispose();
                //        handle = UnityPropertyEmitter.CreateEmittedMonoBehaviourProperty(prop.Info.PropertyName, prop.ValueEntry.TypeOfValue, this.targets.Length, ref go);
                //        innerGoDict[prop.ValueEntry.TypeOfValue] = handle;
                //    }

                //    if (handle != null && handle.UnityProperty != null)
                //        result = handle.UnityProperty;
                //}

                if (result != null)
                {
                    result.serializedObject.Update();
                }
            }

            return result;
        }

        /// <summary>
        /// Enumerates over the properties of the tree. WARNING: For tree that have large targets with lots of data, this may involve massive amounts of work as the full tree structure is resolved. USE THIS METHOD SPARINGLY AND ONLY WHEN ABSOLUTELY NECESSARY!
        /// </summary>
        /// <param name="includeChildren">Whether to include children of the root properties or not. If set to true, every property in the entire tree will be enumerated.</param>
        /// /// <param name="onlyVisible">Whether to only include visible properties. Properties whose parents are invisible are considered invisible.</param>
        public override IEnumerable<InspectorProperty> EnumerateTree(bool includeChildren = true, bool onlyVisible = false)
        {
            if (includeChildren)
            {
                if (RootProperty.Children.Count == 0)
                {
                    yield break;
                }

                var current = RootProperty.Children.Get(0);

                while (current != null)
                {
                    if (!onlyVisible || current.State.Visible)
                    {
                        yield return current;
                    }

                    current = current.NextProperty(true, onlyVisible);
                }
            }
            else
            {
                for (int i = 0; i < RootProperty.Children.Count; i++)
                {
                    var child = RootProperty.Children.Get(i);

                    if (onlyVisible && !child.State.Visible)
                        continue;

                    yield return RootProperty.Children.Get(i);
                }
            }
        }

        /// <summary>
        /// Replaces all occurrences of a value with another value, in the entire tree.
        /// </summary>
        /// <param name="from">The value to find all instances of.</param>
        /// <param name="to">The value to replace the found values with.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">The value to replace with must either be null or be the same type as the value to replace (" + from.GetType().Name + ").</exception>
        public override void ReplaceAllReferences(object from, object to)
        {
            if (from == null)
            {
                throw new ArgumentNullException();
            }

            if (to != null)
            {
                if (from.GetType() != to.GetType())
                {
                    throw new ArgumentException("The value to replace with must either be null or be the same type as the value to replace (" + from.GetType().Name + ").");
                }
            }

            foreach (var prop in EnumerateTree(true))
            {
                if (prop.Info.PropertyType == PropertyType.Value && !prop.Info.TypeOfValue.IsValueType)
                {
                    var valueEntry = prop.ValueEntry;

                    for (int i = 0; i < valueEntry.ValueCount; i++)
                    {
                        object obj = valueEntry.WeakValues[i];

                        if (ReferenceEquals(from, obj))
                        {
                            valueEntry.WeakValues[i] = to;
                        }
                    }
                }
            }
        }

        internal override void ForceRegisterObjectReference(object reference, InspectorProperty property)
        {
            objectReferences[reference] = property.Path;
        }

        /// <summary>
        /// Gets the root tree property at a given index.
        /// </summary>
        /// <param name="index">The index of the property to get.</param>
        public override InspectorProperty GetRootProperty(int index)
        {
            return RootProperty.Children.Get(index);
        }

        /// <summary>
        /// Schedules a delegate to be invoked at the end of the current GUI frame.
        /// </summary>
        /// <param name="action">The action delegate to be delayed.</param>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public override void DelayAction(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            delayedActions.Add(action);
        }

        /// <summary>
        /// Schedules a delegate to be invoked at the end of the next Repaint GUI frame.
        /// </summary>
        /// <param name="action">The action to be delayed.</param>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public override void DelayActionUntilRepaint(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            delayedRepaintActions.Add(action);
            GUIHelper.RequestRepaint();
        }

        /// <summary>
        /// Invokes the actions that have been delayed using <see cref="DelayAction(Action)" /> and <see cref="DelayActionUntilRepaint(Action)" />.
        /// </summary>
        public override void InvokeDelayedActions()
        {
            for (int i = 0; i < delayedActions.Count; i++)
            {
                try
                {
                    delayedActions[i]();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            delayedActions.Clear();

            if ((Event.current != null && Event.current.type == EventType.Repaint) || !TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL)
            {
                for (int i = 0; i < delayedRepaintActions.Count; i++)
                {
                    try
                    {
                        delayedRepaintActions[i]();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                delayedRepaintActions.Clear();
            }
        }

        public override void CleanForCachedReuse()
        {
            var rootChild = RootProperty.Children;

            foreach (var child in rootChild.GetExistingChildren())
            {
                child.CleanForCachedReuse();
            }

            delayedActions.Clear();
            delayedRepaintActions.Clear();

            if (prefabModificationHandler != null)
            {
                prefabModificationHandler.CleanForCachedReuse();
            }

            unchecked
            {
                updateID++;
            }
        }

        public override void SetTargets(params object[] newTargets)
        {
            serializedObject = null;
            //this.prefabModificationHandler = null;
            monoScriptProperty = null;
            monoScriptPropertyHasBeenGotten = false;

            if (targets.Length != newTargets.Length)
            {
                throw new ArgumentException("Target count of tree cannot be changed");
            }

            for (int i = 0; i < targets.Length; i++)
            {
                var target = (T)newTargets[i];

                if (target == null)
                {
                    throw new NullReferenceException("Tree target cannot be null");
                }

                targets[i] = target;
            }

            targets.CopyTo(weakTargets, 0);

            UpdateTree();
        }

        public override void SetSerializedObject(SerializedObject serializedObject)
        {
            this.serializedObject = serializedObject;
            //this.prefabModificationHandler = null;
            monoScriptProperty = null;
            monoScriptPropertyHasBeenGotten = false;

            var newTargets = serializedObject.targetObjects;

            if (targets.Length != newTargets.Length)
            {
                throw new ArgumentException("Target count of tree cannot be changed");
            }

            for (int i = 0; i < targets.Length; i++)
            {
                var target = (T)(object)newTargets[i];

                if (target == null)
                {
                    throw new NullReferenceException("Tree target cannot be null");
                }

                targets[i] = target;
            }

            targets.CopyTo(weakTargets, 0);

            UpdateTree();
        }

        protected override void DisposeInheritedStuff()
        {
            if (prefabModificationHandler != null)
            {
                prefabModificationHandler.Dispose();
                prefabModificationHandler = null;
            }
        }

        protected override void DisposeAndResetRootProperty()
        {
            if (rootProperty != null)
            {
                rootProperty.Dispose();
                rootProperty = null;
            }
        }
    }
}
#endif