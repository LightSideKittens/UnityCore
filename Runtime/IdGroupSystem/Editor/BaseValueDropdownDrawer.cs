using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using UnityEditor;
using UnityEngine;



[DrawerPriority(0.0, 0.0, 2002.0)]
public abstract class BaseValueDropdownDrawer<T> : OdinAttributeDrawer<T> where T : ValueDropdownAttribute
{
    private string error;
    private GUIContent label;
    private bool isList;
    private bool isListElement;
    private Func<IEnumerable<ValueDropdownItem>> getValues;
    private Func<IEnumerable<object>> getSelection;
    private IEnumerable<object> result;
    private bool enableMultiSelect;
    private Dictionary<object, string> nameLookup;
    private LocalPersistentContext<bool> isToggled;

    protected abstract object GetValue();

    /// <summary>Initializes this instance.</summary>
    protected override void Initialize()
    {
        isToggled = this.GetPersistentValue("Toggled", SirenixEditorGUI.ExpandFoldoutByDefault);
        isList = Property.ChildResolver is ICollectionResolver;
        isListElement = Property.Parent != null && Property.Parent.ChildResolver is ICollectionResolver;
        getSelection = () => Property.ValueEntry.WeakValues.Cast<object>();
        getValues = () =>
        {
            object source = GetValue();

            return source != null
                ? (source as IEnumerable).Cast<object>().Where(x => x != null).Select(x =>
                {
                    switch (x)
                    {
                        case ValueDropdownItem valueDropdownItem3:
                            return valueDropdownItem3;
                        case IValueDropdownItem _:
                            IValueDropdownItem valueDropdownItem4 = x as IValueDropdownItem;
                            return new ValueDropdownItem(valueDropdownItem4.GetText(), valueDropdownItem4.GetValue());
                        default:
                            return new ValueDropdownItem(null, x);
                    }
                })
                : null;
        };
        ReloadDropdownCollections();
    }

    private void ReloadDropdownCollections()
    {
        if (error != null)
            return;
        object obj = null;
        object source = GetValue();
        if (source != null)
            obj = (source as IEnumerable).Cast<object>().FirstOrDefault();
        if (obj is IValueDropdownItem)
        {
            IEnumerable<ValueDropdownItem> valueDropdownItems = getValues();
            nameLookup = new Dictionary<object, string>(new ValueDropdownEqualityComparer(false));
            foreach (ValueDropdownItem key in valueDropdownItems)
                nameLookup[key] = key.Text;
        }
        else
            nameLookup = null;
    }

    private static IEnumerable<ValueDropdownItem> ToValueDropdowns(IEnumerable<object> query) => query.Select(x =>
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
        this.label = label;
        if (Property.ValueEntry == null)
            CallNextDrawer(label);
        else if (error != null)
        {
            SirenixEditorGUI.ErrorMessageBox(error);
            CallNextDrawer(label);
        }
        else if (isList)
        {
            if (Attribute.DisableListAddButtonBehaviour)
            {
                CallNextDrawer(label);
            }
            else
            {
                Action customAddFunction = CollectionDrawerStaticInfo.NextCustomAddFunction;
                CollectionDrawerStaticInfo.NextCustomAddFunction = new Action(OpenSelector);
                CallNextDrawer(label);
                if (result != null)
                {
                    AddResult(result);
                    result = null;
                }

                CollectionDrawerStaticInfo.NextCustomAddFunction = customAddFunction;
            }
        }
        else if (Attribute.DrawDropdownForListElements || !isListElement)
            DrawDropdown();
        else
            CallNextDrawer(label);
    }

    private void AddResult(IEnumerable<object> query)
    {
        if (isList)
        {
            ICollectionResolver childResolver = Property.ChildResolver as ICollectionResolver;
            if (enableMultiSelect)
                childResolver.QueueClear();
            foreach (object obj in query)
            {
                object[] values = new object[Property.ParentValues.Count];
                for (int index = 0; index < values.Length; ++index)
                    values[index] = !Attribute.CopyValues
                        ? obj
                        : Sirenix.Serialization.SerializationUtility.CreateCopy(obj);
                childResolver.QueueAdd(values);
            }
        }
        else
        {
            object obj = query.FirstOrDefault();
            for (int index = 0; index < Property.ValueEntry.WeakValues.Count; ++index)
            {
                if (Attribute.CopyValues)
                    Property.ValueEntry.WeakValues[index] = Sirenix.Serialization.SerializationUtility.CreateCopy(obj);
                else
                    Property.ValueEntry.WeakValues[index] = obj;
            }
        }
    }

    private void DrawDropdown()
    {
        IEnumerable<object> objects;
        if (Attribute.AppendNextDrawer && !isList)
        {
            GUILayout.BeginHorizontal();
            float width = 15f;
            if (label != null)
                width += GUIHelper.BetterLabelWidth;
            GUIContent btnLabel = GUIHelper.TempContent("");
            if (Property.Info.TypeOfValue == typeof(Type))
                btnLabel.image = GUIHelper.GetAssetThumbnail(null, Property.ValueEntry.WeakSmartValue as Type, false);
            objects = OdinSelector<object>.DrawSelectorDropdown(label, btnLabel,
                new Func<Rect, OdinSelector<object>>(ShowSelector), !Attribute.OnlyChangeValueOnConfirm, GUIStyle.none,
                GUILayoutOptions.Width(width));
            if (Event.current.type == EventType.Repaint)
            {
                Rect position = GUILayoutUtility.GetLastRect().AlignRight(15f);
                position.y += 4f;
                SirenixGUIStyles.PaneOptions.Draw(position, GUIContent.none, 0);
            }

            GUILayout.BeginVertical();
            bool inAppendedDrawer = Attribute.DisableGUIInAppendedDrawer;
            if (inAppendedDrawer)
                GUIHelper.PushGUIEnabled(false);
            CallNextDrawer(null);
            if (inAppendedDrawer)
                GUIHelper.PopGUIEnabled();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        else
        {
            GUIContent btnLabel = GUIHelper.TempContent(GetCurrentValueName());
            if (Property.Info.TypeOfValue == typeof(Type))
                btnLabel.image = GUIHelper.GetAssetThumbnail(null, Property.ValueEntry.WeakSmartValue as Type, false);
            if (!Attribute.HideChildProperties && Property.Children.Count > 0)
            {
                Rect valueRect;
                isToggled.Value = SirenixEditorGUI.Foldout(isToggled.Value, label, out valueRect);
                objects = OdinSelector<object>.DrawSelectorDropdown(valueRect, btnLabel,
                    new Func<Rect, OdinSelector<object>>(ShowSelector), !Attribute.OnlyChangeValueOnConfirm);
                if (SirenixEditorGUI.BeginFadeGroup(this, isToggled.Value))
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
                objects = OdinSelector<object>.DrawSelectorDropdown(label, btnLabel,
                    new Func<Rect, OdinSelector<object>>(ShowSelector), !Attribute.OnlyChangeValueOnConfirm, null);
        }

        if (objects == null || !objects.Any())
            return;
        AddResult(objects);
    }

    private void OpenSelector()
    {
        ReloadDropdownCollections();
        ShowSelector(new Rect(Event.current.mousePosition, Vector2.zero)).SelectionConfirmed += x => result = x;
    }

    private OdinSelector<object> ShowSelector(Rect rect)
    {
        GenericSelector<object> selector = CreateSelector();
        rect.x = (int)rect.x;
        rect.y = (int)rect.y;
        rect.width = (int)rect.width;
        rect.height = (int)rect.height;
        if (Attribute.AppendNextDrawer && !isList)
            rect.xMax = GUIHelper.GetCurrentLayoutRect().xMax;
        selector.ShowInPopup(rect, new Vector2(Attribute.DropdownWidth, Attribute.DropdownHeight));
        return selector;
    }

    private GenericSelector<object> CreateSelector()
    {
        bool isUniqueList = Attribute.IsUniqueList;
        IEnumerable<ValueDropdownItem> source = getValues() ?? Enumerable.Empty<ValueDropdownItem>();
        if (source.Any())
        {
            if (isList && Attribute.ExcludeExistingValuesInList || isListElement & isUniqueList)
            {
                List<ValueDropdownItem> list = source.ToList();
                InspectorProperty parent = Property.FindParent(x => x.ChildResolver is ICollectionResolver, true);
                ValueDropdownEqualityComparer comparer = new ValueDropdownEqualityComparer(false);
                parent.ValueEntry.WeakValues.Cast<IEnumerable>().SelectMany(x => x.Cast<object>())
                    .ForEach(x => list.RemoveAll(c => comparer.Equals(c, x)));
                source = list;
            }

            if (nameLookup != null)
            {
                foreach (ValueDropdownItem valueDropdownItem in source)
                {
                    if (valueDropdownItem.Value != null)
                        nameLookup[valueDropdownItem.Value] = valueDropdownItem.Text;
                }
            }
        }

        bool flag = Attribute.NumberOfItemsBeforeEnablingSearch == 0 || source != null &&
            source.Take(Attribute.NumberOfItemsBeforeEnablingSearch).Count() ==
            Attribute.NumberOfItemsBeforeEnablingSearch;
        GenericSelector<object> selector = new GenericSelector<object>(Attribute.DropdownTitle, false,
            source.Select(x => new GenericSelectorItem<object>(x.Text, x.Value)));
        enableMultiSelect = isList & isUniqueList && !Attribute.ExcludeExistingValuesInList;
        if (Attribute.FlattenTreeView)
            selector.FlattenedTree = true;
        if (((!isList ? 0 : (!Attribute.ExcludeExistingValuesInList ? 1 : 0)) & (isUniqueList ? 1 : 0)) != 0)
            selector.CheckboxToggle = true;
        else if (!Attribute.DoubleClickToConfirm && !enableMultiSelect)
            selector.EnableSingleClickToSelect();
        if (isList && enableMultiSelect)
        {
            selector.SelectionTree.Selection.SupportsMultiSelect = true;
            selector.DrawConfirmSelectionButton = true;
        }

        selector.SelectionTree.Config.DrawSearchToolbar = flag;
        IEnumerable<object> selection = Enumerable.Empty<object>();
        if (!isList)
            selection = getSelection();
        else if (enableMultiSelect)
            selection = getSelection().SelectMany(x => (x as IEnumerable).Cast<object>());
        selector.SetSelection(selection);
        selector.SelectionTree.EnumerateTree().AddThumbnailIcons(true);
        if (Attribute.ExpandAllMenuItems)
            selector.SelectionTree.EnumerateTree(x => x.Toggled = true);
        if (Attribute.SortDropdownItems)
            selector.SelectionTree.SortMenuItemsByName();
        return selector;
    }

    private string GetCurrentValueName()
    {
        if (EditorGUI.showMixedValue)
            return "—";
        object weakSmartValue = Property.ValueEntry.WeakSmartValue;
        string name = null;
        if (nameLookup != null && weakSmartValue != null)
            nameLookup.TryGetValue(weakSmartValue, out name);
        return new GenericSelectorItem<object>(name, weakSmartValue).GetNiceName();
    }
}

internal class ValueDropdownEqualityComparer : IEqualityComparer<object>
{
    private bool isTypeLookup;

    public ValueDropdownEqualityComparer(bool isTypeLookup) => this.isTypeLookup = isTypeLookup;

    public bool Equals(object x, object y)
    {
        if (x is ValueDropdownItem)
            x = ((ValueDropdownItem)x).Value;
        if (y is ValueDropdownItem)
            y = ((ValueDropdownItem)y).Value;
        if (EqualityComparer<object>.Default.Equals(x, y))
            return true;
        if (x == null != (y == null) || !isTypeLookup)
            return false;
        Type type1 = x as Type;
        if ((object)type1 == null)
            type1 = x.GetType();
        Type type2 = type1;
        Type type3 = y as Type;
        if ((object)type3 == null)
            type3 = y.GetType();
        Type type4 = type3;
        return type2 == type4;
    }

    public int GetHashCode(object obj)
    {
        if (obj is ValueDropdownItem)
            obj = ((ValueDropdownItem)obj).Value;
        if (obj == null)
            return -1;
        if (!isTypeLookup)
            return obj.GetHashCode();
        Type type = obj as Type;
        if ((object)type == null)
            type = obj.GetType();
        return type.GetHashCode();
    }
}

