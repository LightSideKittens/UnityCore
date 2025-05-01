//-----------------------------------------------------------------------
// <copyright file="TabGroupAttributeDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System.Collections.Generic;
    using Utilities.Editor;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using System.Linq;

    /// <summary>
    /// Draws all properties grouped together with the <see cref="TabGroupAttribute"/>
    /// </summary>
    /// <seealso cref="TabGroupAttribute"/>
    public class TabGroupAttributeDrawer : OdinGroupDrawer<TabGroupAttribute>, IOnSelfStateChangedNotification
    {
        public const string CurrentTabIndexKey = "CurrentTabIndex";
        public const string CurrentTabNameKey = "CurrentTabName";
        public const string TabCountKey = "TabCount";

        private bool isChangingTabName;
        private GUITabGroup tabGroup;
        private List<TabInfo> tabs;
        private bool initialized;

        private class TabInfo
        {
            public string TabId;
            public SdfIconType Icon;
            public ValueResolver<Color?> TextColor;
            public List<InspectorProperty> InspectorProperties;
            public ValueResolver<string> Title;
            public string NiceName;
        }

        protected override void Initialize()
        {
            this.tabGroup = new GUITabGroup();
            this.tabGroup.AnimationSpeed = 1f / SirenixEditorGUI.TabPageSlideAnimationDuration;
            this.tabGroup.TabLayouting = this.Attribute.TabLayouting;

            this.tabs = new List<TabInfo>();

            for (int j = 0; j < this.Property.Children.Count; j++)
            {
                var child = this.Property.Children[j];
                if (child.Info.PropertyType == PropertyType.Group)
                {
                    var tabAttr = child.GetAttribute<TabGroupAttribute.TabSubGroupAttribute>();
                    if (tabAttr != null)
                    {
                        var tabId = tabAttr.Tab.TabId;
                        var tabName = tabAttr.Tab.TabName ?? child.Name.TrimStart('#');

                        var tab = new TabInfo()
                        {
                            TabId = tabId,
                            Icon = tabAttr.Tab.Icon,
                            Title = ValueResolver.GetForString(this.Property, tabName),
                            NiceName = child.NiceName,
                            InspectorProperties = child.Children.ToList(),
                            TextColor = ValueResolver.Get<Color?>(this.Property, tabAttr.Tab.TextColor),
                        };

                        var tabGroup = this.tabGroup.RegisterTab(tab.TabId);
                        tabGroup.Icon = tabAttr.Tab.Icon;
                        tabGroup.TextColor = tab.TextColor.GetValue();
                        this.tabs.Add(tab);
                    }
                }
            }

            this.Property.State.Create<int>(CurrentTabIndexKey, true, 0);
            this.Property.State.Create<int>(TabCountKey, false, this.tabs.Count);

            var currentIndex = this.GetClampedCurrentIndex();
            var currentTab = this.tabs[currentIndex];

            var selectedTabGroup = this.tabGroup.RegisterTab(currentTab.TabId);
            this.tabGroup.SetCurrentPage(selectedTabGroup);

            this.isChangingTabName = true;
            this.Property.State.Create<string>(CurrentTabNameKey, false, currentTab.TabId);
            this.isChangingTabName = false;

            this.initialized = true;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            foreach (var item in this.tabs)
            {
                item.TextColor.DrawError();
            }

            var property = this.Property;
            var attribute = this.Attribute;

            if (attribute.HideTabGroupIfTabGroupOnlyHasOneTab && this.tabs.Count <= 1)
            {
                for (int i = 0; i < this.tabs.Count; i++)
                {
                    int pageCount = this.tabs[i].InspectorProperties.Count;
                    for (int j = 0; j < pageCount; j++)
                    {
                        var child = this.tabs[i].InspectorProperties[j];
                        child.Update();
                        child.Draw(child.Label);
                    }
                }
                return;
            }

            this.tabGroup.AnimationSpeed = 1 / SirenixEditorGUI.TabPageSlideAnimationDuration;
            this.tabGroup.FixedHeight = attribute.UseFixedHeight;

            this.GetClampedCurrentIndex();

            SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
            tabGroup.BeginGroup(true, attribute.Paddingless ? GUIStyle.none : null);

            property.State.Set(TabCountKey, this.tabs.Count);

            for (int i = 0; i < this.tabs.Count; i++)
            {
                var tab = this.tabs[i];

                var page = tabGroup.RegisterTab(this.tabs[i].TabId);
                page.Title = tab.Title.GetValue();
                page.TextColor = tab.TextColor.GetValue();
                
                if (string.IsNullOrEmpty(page.Title))
                {
                    page.Tooltip = tab.NiceName;
                }
                else
                {
                    page.Tooltip = null;
                }

                if (this.tabGroup.NextPage == null && this.tabGroup.CurrentPage == page)
                {
                    property.State.Set<int>(CurrentTabIndexKey, i);
                }

                if (page.BeginPage())
                {
                    int pageCount = this.tabs[i].InspectorProperties.Count;
                    for (int j = 0; j < pageCount; j++)
                    {
                        var child = this.tabs[i].InspectorProperties[j];
                        child.Update(); // Since the property is not fetched through the property system, ensure it's updated before drawing it.
                        child.Draw(child.Label);
                    }
                }
                page.EndPage();
            }

            tabGroup.EndGroup();
            SirenixEditorGUI.EndIndentedVertical();
        }

        private int GetClampedCurrentIndex()
        {
            var currentIndex = this.Property.State.Get<int>(CurrentTabIndexKey);

            if (currentIndex < 0)
            {
                currentIndex = 0;
                this.Property.State.Set<int>(CurrentTabIndexKey, currentIndex);
            }
            else if (currentIndex >= this.tabs.Count)
            {
                currentIndex = this.tabs.Count - 1;
                this.Property.State.Set<int>(CurrentTabIndexKey, currentIndex);
            }

            return currentIndex;
        }

        public void OnSelfStateChanged(string state)
        {
            if (!this.initialized) return;

            if (state == CurrentTabIndexKey)
            {
                var index = this.GetClampedCurrentIndex();
                var tab = this.tabs[index];

                this.isChangingTabName = true;
                this.Property.State.Set<string>(CurrentTabNameKey, tab.TabId);
                this.isChangingTabName = false;

                this.tabGroup.GoToPage(this.tabs[index].TabId);
            }
            else if (state == CurrentTabNameKey && !this.isChangingTabName)
            {
                var name = this.Property.State.Get<string>(CurrentTabNameKey);
                int index = -1;

                for (int i = 0; i < this.tabs.Count; i++)
                {
                    if (this.tabs[i].TabId == name)
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1)
                {
                    Debug.LogError("There is no tab named '" + name + "' in the tab group '" + this.Property.NiceName + "'!");

                    index = this.Property.State.Get<int>(CurrentTabIndexKey);

                    this.isChangingTabName = true;
                    this.Property.State.Set<string>(CurrentTabNameKey, this.tabs[index].TabId);
                    this.isChangingTabName = false;
                }
                else
                {
                    this.Property.State.Set<int>(CurrentTabIndexKey, index);
                }
            }
        }
    }
}
#endif