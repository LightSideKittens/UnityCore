using Sirenix.Serialization;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace LSCore.Attributes
{
    /// <summary>
    /// Draws properties marked with <see cref="T:Sirenix.OdinInspector.TypeFilterAttribute" />.
    /// </summary>
    [DrawerPriority(0.0, 0.0, 2002.0)]
    public abstract class BaseTypeFilterDrawer<T> : OdinAttributeDrawer<T> where T : BaseTypeFilterAttribute
    {
        protected static List<Type> allTypes;
        private static Type lastItemType;
        private string error;
        protected bool useSpecialListBehaviour;
        private Func<IEnumerable<ValueDropdownItem>> getValues;
        private Func<IEnumerable<object>> getSelection;
        private IEnumerable<object> result;
        private Dictionary<object, string> nameLookup;
        private IEnumerable<object> rawGetter;

        protected override bool CanDrawAttributeProperty(InspectorProperty property) => property.ValueEntry != null;

        /// <summary>Initializes this instance.</summary>
        protected override void Initialize()
        {
            if (Property.ChildResolver is ICollectionResolver childResolver)
            {
                useSpecialListBehaviour = true;
                
                if(lastItemType != childResolver.ElementType)
                {
                    lastItemType = childResolver.ElementType;
                    InitAllTypes(lastItemType);
                }
            }
            else
            {
                var type = Property.BaseValueEntry.BaseValueType;
                if(lastItemType != type)
                {
                    lastItemType = type;
                    InitAllTypes(type);
                }
            }

            rawGetter = GetRawGetter();
            
            getSelection = () => Property.ValueEntry.WeakValues.Cast<object>();
            getValues = () => rawGetter.Where(x => x != null).Select(x => new ValueDropdownItem(null, x));
            ReloadDropdownCollections();
        }

        
        private void InitAllTypes(Type type)
        {
            var assembly = Property.SerializationRoot.Info.TypeOfValue.Assembly;
            var setTypes = assembly.GetVisibleTypes();
            var nestedTypes = new HashSet<Type>();
            
            allTypes = setTypes
                .Where(t =>
                {
                    if (type.IsAssignableFrom(t) && !t.IsAbstract)
                    {
                        if (t.IsGenericType)
                        {
                            nestedTypes.AddRange(FindAllConcreteNestedTypes(setTypes, t));
                        }

                        return true;
                    }

                    return false;
                })
                .ToList();
            allTypes.AddRange(nestedTypes);
        }
        
        public static HashSet<Type> FindAllConcreteNestedTypes(IEnumerable<Type> types, Type nestedGenericType)
        {
            HashSet<Type> nestedTypes = new HashSet<Type>();

            foreach (var type in types)
            {
                if (type.IsClass && !type.IsAbstract && type.BaseType != null && type.BaseType.IsGenericType)
                {
                    Type baseType = type.BaseType.GetGenericTypeDefinition();
                    if (baseType == nestedGenericType.DeclaringType.GetGenericTypeDefinition())
                    {
                        var target = nestedGenericType.MakeGenericType(type.BaseType.GetGenericArguments());
                        Debug.Log(target);
                        nestedTypes.Add(target);
                    }
                }
            }

            return nestedTypes;
        }
        
        
        protected abstract IEnumerable<object> GetRawGetter();

        private void ReloadDropdownCollections()
        {
            if (error != null)
                return;
            object obj = null;
            if (rawGetter != null)
                obj = rawGetter.FirstOrDefault();
            if (obj is IValueDropdownItem)
            {
                IEnumerable<ValueDropdownItem> valueDropdownItems = getValues();
                nameLookup =
                    new Dictionary<object, string>(new IValueDropdownEqualityComparer(true));
                foreach (ValueDropdownItem key in valueDropdownItems)
                    nameLookup[key] = key.Text;
            }
            else
                nameLookup = null;
        }

        private static IEnumerable<ValueDropdownItem> ToValueDropdowns(IEnumerable<object> query) =>
            query.Select(x =>
            {
                switch (x)
                {
                    case ValueDropdownItem valueDropdowns2:
                        return valueDropdowns2;
                    case IValueDropdownItem _:
                        IValueDropdownItem valueDropdownItem = x as IValueDropdownItem;
                        return new ValueDropdownItem(valueDropdownItem.GetText(), valueDropdownItem.GetValue());
                    default:
                        return new ValueDropdownItem(null, x);
                }
            });

        /// <summary>
        /// Draws the property with GUILayout support. This method is called by DrawPropertyImplementation if the GUICallType is set to GUILayout, which is the default.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (Property.ValueEntry == null)
                CallNextDrawer(label);
            else if (error != null)
            {
                SirenixEditorGUI.ErrorMessageBox(error);
                CallNextDrawer(label);
            }
            else if (useSpecialListBehaviour)
            {
                CollectionDrawerStaticInfo.NextCustomAddFunction = new Action(OpenSelector);
                CallNextDrawer(label);
                if (result != null)
                {
                    AddResult(result);
                    result = null;
                }

                CollectionDrawerStaticInfo.NextCustomAddFunction = null;
            }
            else
                DrawDropdown(label);
        }

        private void AddResult(IEnumerable<object> query)
        {
            if (!query.Any())
                return;
            if (useSpecialListBehaviour)
            {
                ICollectionResolver childResolver = Property.ChildResolver as ICollectionResolver;
                foreach (object obj in query)
                {
                    object[] values = new object[Property.ParentValues.Count];
                    for (int index = 0; index < values.Length; ++index)
                    {
                        Type type = obj as Type;
                        if (type != null)
                            values[index] = CreateInstance(type);
                    }

                    childResolver.QueueAdd(values);
                }
            }
            else
            {
                Type type = query.FirstOrDefault() as Type;
                for (int index = 0; index < Property.ValueEntry.WeakValues.Count; ++index)
                {
                    if (type != null)
                        Property.ValueEntry.WeakValues[index] = CreateInstance(type);
                }
            }
        }

        private object CreateInstance(Type type)
        {
            if (Property.ValueEntry.SerializationBackend == SerializationBackend.Unity)
            {
                object initializedObject = UnitySerializationUtility.CreateDefaultUnityInitializedObject(type);
                if (initializedObject != null)
                    return initializedObject;
            }

            if (type == typeof(string))
                return "";
            if (type.IsAbstract || type.IsInterface)
            {
                Debug.LogError("TypeFilter was asked to instantiate a value of type '" + type.GetNiceFullName() +
                                             "', but it is abstract or an interface and cannot be instantiated.");
                return null;
            }

            return type.IsValueType ||
                         type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                             Type.EmptyTypes, null) != null
                ? Activator.CreateInstance(type)
                : FormatterServices.GetUninitializedObject(type);
        }

        private void DrawDropdown(GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            string currentValueName = GetCurrentValueName();
            IEnumerable<object> query;
            if (Property.Children.Count > 0)
            {
                Rect valueRect;
                Property.State.Expanded = SirenixEditorGUI.Foldout(Property.State.Expanded, label, out valueRect);
                query = OdinSelector<object>.DrawSelectorDropdown(valueRect, currentValueName,
                    new Func<Rect, OdinSelector<object>>(ShowSelector));
                if (SirenixEditorGUI.BeginFadeGroup(this, Property.State.Expanded))
                {
                    ++EditorGUI.indentLevel;
                    for (int index = 0; index < Property.Children.Count; ++index)
                    {
                        InspectorProperty child = Property.Children[index];
                        child.Draw(child.Label);
                    }

                    --EditorGUI.indentLevel;
                }

                SirenixEditorGUI.EndFadeGroup();
            }
            else
                query = OdinSelector<object>.DrawSelectorDropdown(label, currentValueName,
                    new Func<Rect, OdinSelector<object>>(ShowSelector), null);

            if (!EditorGUI.EndChangeCheck() || query == null)
                return;
            AddResult(query);
        }

        private void OpenSelector()
        {
            ReloadDropdownCollections();
            ShowSelector(new Rect(Event.current.mousePosition, Vector2.zero)).SelectionConfirmed +=
                x => result = x;
        }

        private OdinSelector<object> ShowSelector(Rect rect)
        {
            GenericSelector<object> selector = CreateSelector();
            rect.x = (int)rect.x;
            rect.y = (int)rect.y;
            rect.width = (int)rect.width;
            rect.height = (int)rect.height;
            selector.ShowInPopup(rect, new Vector2(0.0f, 0.0f));
            return selector;
        }

        private GenericSelector<object> CreateSelector()
        {
            IEnumerable<ValueDropdownItem> source1 = getValues() ?? Enumerable.Empty<ValueDropdownItem>();
            bool flag = source1.Take(10).Count() == 10;
            GenericSelector<object> selector = new GenericSelector<object>(Attribute.DropdownTitle, false,
                source1.Select(
                    x =>
                        new GenericSelectorItem<object>(x.Text, x.Value)));
            selector.CheckboxToggle = false;
            selector.EnableSingleClickToSelect();
            selector.SelectionTree.Config.DrawSearchToolbar = flag;
            IEnumerable<object> source2 = Enumerable.Empty<object>();
            if (!useSpecialListBehaviour)
                source2 = getSelection();
            IEnumerable<object> selection =
                source2.Select(x => x != null ? x.GetType() : (object)null);
            selector.SetSelection(selection);
            selector.SelectionTree.EnumerateTree().AddThumbnailIcons(true);
            return selector;
        }

        private string GetCurrentValueName()
        {
            if (EditorGUI.showMixedValue)
                return "—";
            object key = Property.ValueEntry.WeakSmartValue;
            string name = null;
            if (nameLookup != null && key != null)
                nameLookup.TryGetValue(key, out name);
            if (key != null)
                key = key.GetType();
            return new GenericSelectorItem<object>(name, key).GetNiceName();
        }
    }
}