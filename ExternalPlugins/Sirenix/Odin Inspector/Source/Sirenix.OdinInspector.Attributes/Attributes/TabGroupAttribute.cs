//-----------------------------------------------------------------------
// <copyright file="TabGroupAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Sirenix.OdinInspector.Internal;

    /// <summary>
    /// <para>TabGroup is used on any property, and organizes properties into different tabs.</para>
    /// <para>Use this to organize different value to make a clean and easy to use inspector.</para>
    /// </summary>
	/// <remarks>
    /// <para>Use groups to create multiple tab groups, each with multiple tabs and even sub tabs.</para>
    /// </remarks>
	/// <example>
	/// <para>The following example shows how to create a tab group with two tabs.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
	///	{
	///		[TabGroup("First")]
	///		public int MyFirstInt;
	///
	///		[TabGroup("First")]
	///		public int AnotherInt;
	///
	///		[TabGroup("Second")]
	///		public int MySecondInt;
	///	}
    /// </code>
    /// </example>
	/// <example>
	/// <para>The following example shows how multiple groups of tabs can be created.</para>
    /// <code>
	///	public class MyComponent : MonoBehaviour
	///	{
	///		[TabGroup("A", "FirstGroup")]
	///		public int FirstGroupA;
	///
	///		[TabGroup("B", "FirstGroup")]
	///		public int FirstGroupB;
	///
	///		// The second tab group has been configured to have constant height across all tabs.
	///		[TabGroup("A", "SecondGroup", true)]
	///		public int SecondgroupA;
	///
	///		[TabGroup("B", "SecondGroup")]
	///		public int SecondGroupB;
	///
	///		[TabGroup("B", "SecondGroup")]
	///		public int AnotherInt;
	///	}
	/// </code>
    /// </example>
    /// <example>
    /// <para>This example demonstrates how multiple tabs groups can be combined to create tabs in tabs.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     [TabGroup("ParentGroup", "First Tab")]
    ///     public int A;
    ///     
    ///     [TabGroup("ParentGroup", "Second Tab")]
    ///     public int B;
    ///     
    ///     // Specify 'First Tab' as a group, and another child group to the 'First Tab' group.
    ///     [TabGroup("ParentGroup/First Tab/InnerGroup", "Inside First Tab A")]
    ///     public int C;
    ///     
    ///     [TabGroup("ParentGroup/First Tab/InnerGroup", "Inside First Tab B")]
    ///     public int D;
    ///     
    ///     [TabGroup("ParentGroup/Second Tab/InnerGroup", "Inside Second Tab")]
    ///     public int E;
    /// }
    /// </code>
    /// </example>
	/// <seealso cref="TabListAttribute"/>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class TabGroupAttribute : PropertyGroupAttribute, ISubGroupProviderAttribute
    {
        /// <summary>
        /// The default tab group name which is used when the single-parameter constructor is called.
        /// </summary>
        public const string DEFAULT_NAME = "_DefaultTabGroup";

        /// <summary>
        /// Name of the tab.
        /// </summary>
        public string TabName;

        public string TabId;

        /// <summary>
        /// Should this tab be the same height as the rest of the tab group.
        /// </summary>
        public bool UseFixedHeight;

        /// <summary>
        /// If true, the content of each page will not be contained in any box.
        /// </summary>
        public bool Paddingless;

        /// <summary>
        /// If true, the tab group will be hidden if it only contains one tab.
        /// </summary>
        public bool HideTabGroupIfTabGroupOnlyHasOneTab;

        /// <summary> Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow. </summary>
        public string TextColor;

        public SdfIconType Icon;

        /// <summary>
        /// Specify how tabs should be layouted.
        /// </summary>
        public TabLayouting TabLayouting;

        /// <summary>
        /// Organizes the property into the specified tab in the default group.
        /// Default group name is '_DefaultTabGroup'
        /// </summary>
        /// <param name="tab">The tab.</param>
        /// <param name="useFixedHeight">if set to <c>true</c> [use fixed height].</param>
        /// <param name="order">The order.</param>
        public TabGroupAttribute(string tab, bool useFixedHeight = false, float order = 0)
            : this(DEFAULT_NAME, tab, useFixedHeight, order)
        { }

        /// <summary>
        /// Organizes the property into the specified tab in the specified group.
        /// </summary>
        /// <param name="group">The group to attach the tab to.</param>
        /// <param name="tab">The name of the tab.</param>
        /// <param name="useFixedHeight">Set to true to have a constant height across the entire tab group.</param>
        /// <param name="order">The order of the group.</param>
        public TabGroupAttribute(string group, string tab, bool useFixedHeight = false, float order = 0)
            : base(group, order)
        {
            this.TabId = tab;
            this.UseFixedHeight = useFixedHeight;
            this.Tabs = new List<TabGroupAttribute>()
            {
                this,
            };
        }


        /// <summary>
        /// Organizes the property into the specified tab in the specified group.
        /// </summary>
        /// <param name="group">The group to attach the tab to.</param>
        /// <param name="tab">The name of the tab.</param>
        /// <param name="useFixedHeight">Set to true to have a constant height across the entire tab group.</param>
        /// <param name="order">The order of the group.</param>
        public TabGroupAttribute(string group, string tab, SdfIconType icon, bool useFixedHeight = false, float order = 0)
            : this(group, tab, useFixedHeight, order)
        {
            this.Icon = icon;
        }

        /// <summary>
        /// Name of all tabs in this group.
        /// </summary>
        public List<TabGroupAttribute> Tabs;

        protected override void CombineValuesWith(PropertyGroupAttribute other)
        {
            var otherTab = other as TabGroupAttribute;
            if (otherTab.TabId != null)
            {
                if (otherTab.TabLayouting != default)
                {
                    this.TabLayouting = otherTab.TabLayouting;
                }

                this.UseFixedHeight = this.UseFixedHeight || otherTab.UseFixedHeight;
                this.Paddingless = this.Paddingless || otherTab.Paddingless;
                this.HideTabGroupIfTabGroupOnlyHasOneTab = this.HideTabGroupIfTabGroupOnlyHasOneTab || otherTab.HideTabGroupIfTabGroupOnlyHasOneTab;

                bool hasTabIDAlready = false;

                for (int i = 0; i < this.Tabs.Count; i++)
                {
                    var tab = this.Tabs[i];

                    if (tab.TabId == otherTab.TabId)
                    {
                        if (tab.TextColor == null)
                            tab.TextColor = otherTab.TextColor;

                        if (tab.Icon == SdfIconType.None)
                            tab.Icon = otherTab.Icon;

                        if (tab.TabName == null)
                            tab.TabName = otherTab.TabName;

                        hasTabIDAlready = true;
						break;
					}
                }

                if (!hasTabIDAlready)
                {
                    this.Tabs.Add(otherTab);
                }
            }
        }

        IList<PropertyGroupAttribute> ISubGroupProviderAttribute.GetSubGroupAttributes()
        {
            int count = 0;

            var result = new List<PropertyGroupAttribute>(this.Tabs.Count)
            {
                new TabSubGroupAttribute(this, this.GroupID + "/" + TabId, count++)
            };

            foreach (var tab in this.Tabs)
            {
                if (tab.TabId != this.TabId)
                {
                    result.Add(new TabSubGroupAttribute(tab, this.GroupID + "/" + tab.TabId, count++));
                }
            }

            return result;
        }

        string ISubGroupProviderAttribute.RepathMemberAttribute(PropertyGroupAttribute attr)
        {
            var tabAttr = (TabGroupAttribute)attr;
            return this.GroupID + "/" + tabAttr.TabId;
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public class TabSubGroupAttribute : PropertyGroupAttribute
        {
            public TabGroupAttribute Tab;

            public TabSubGroupAttribute(TabGroupAttribute tab, string groupId, float order) : base(groupId, order)
            {
                this.Tab = tab;
            }

            protected override void CombineValuesWith(PropertyGroupAttribute other)
            {
                if (other is TabSubGroupAttribute otherTab)
                {
                    if (this.Tab.TextColor == null)
                        this.Tab.TextColor = otherTab.Tab.TextColor;

                    if (this.Tab.Icon == SdfIconType.None)
                        this.Tab.Icon = otherTab.Tab.Icon;

                    if (this.Tab.TabName == null)
                        this.Tab.TabName = otherTab.Tab.TabName;
                }
            }
        }
    }

    public enum TabLayouting
    {
        MultiRow,
        Shrink,
    }
}