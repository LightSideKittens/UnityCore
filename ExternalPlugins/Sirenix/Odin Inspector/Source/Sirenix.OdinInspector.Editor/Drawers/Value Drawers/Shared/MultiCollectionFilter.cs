//-----------------------------------------------------------------------
// <copyright file="MultiCollectionFilter.cs" company="Sirenix ApS">
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

	using System;
	using System.Collections.Generic;
	using Sirenix.Utilities;
	using Sirenix.Utilities.Editor;
	using UnityEditor;
	using UnityEngine;

	public class MultiCollectionFilter<TResolver> : IDisposable where TResolver : ICollectionResolver
	{
		public bool IsFiltered => this.IsUsed && this.FilteredChildren.Count > 0;
		
		public readonly bool IsUsed;
		
		public PropertySearchFilter Filter;
		
		internal SearchField SearchField;
		
		internal string            ControlName;
		internal InspectorProperty Property;
		internal TResolver Resolver;
		
		internal PropertyChildren        Children;
		internal List<InspectorProperty> FilteredChildren;
		internal int                     LastChildrenCount;

		internal readonly Action<CollectionChangeInfo> OnChange;
		internal readonly Action ScheduledUpdateAction;
		
		public MultiCollectionFilter(InspectorProperty property, TResolver resolver) 
		{
			this.ControlName = $"PropertyTreeSearchField_{Guid.NewGuid()}";
			this.Property    = property;
			this.Children    = property.Children;
			this.Resolver = resolver;
			
			this.LastChildrenCount = this.Children.Count;

			var searchable = property.GetAttribute<SearchableAttribute>();

			if (searchable is null) 
			{
				this.IsUsed = false;
				return;
			}

			this.SearchField = new SearchField();

			this.Filter = new PropertySearchFilter(null, searchable);
			this.FilteredChildren = new List<InspectorProperty>(this.Children.Count);

			this.ScheduledUpdateAction = () => 
			{
				this.Update();
				GUIHelper.RequestRepaint();
			};

			this.IsUsed = true;
		}

		public InspectorProperty this[int index] => this.IsFiltered ? this.FilteredChildren[index] : this.Children[index];

		public int GetCount() 
		{
			if (this.Children.Count != this.LastChildrenCount) 
			{
				this.Update();
				this.LastChildrenCount = this.Children.Count;
			}

			return this.IsFiltered ? this.FilteredChildren.Count : this.Children.Count;
		}

		public void Draw() 
		{
			if (!this.IsUsed)
			{
				return;
			}
			
			Rect rect = EditorGUILayout.GetControlRect(false).AddYMin(2);

			if (UnityVersion.IsVersionOrGreater(2019, 3)) 
			{
				rect = rect.AddY(-2);
			}

			this.Draw(rect);
		}

		public void Draw(Rect rect) 
		{
			EditorGUI.BeginChangeCheck();
			string searchTerm = this.SearchField.Draw(rect, this.Filter.SearchTerm, "Find element...");

			if (!EditorGUI.EndChangeCheck())
			{
				return;
			}
			
			if (!string.IsNullOrEmpty(searchTerm)) 
			{
				this.Property.State.Expanded = true;
			}

			this.Filter.SearchTerm = searchTerm;

			this.ScheduleUpdate();
		}

		public void Update() 
		{
			if (!this.IsUsed) 
			{
				return;
			}

			this.FilteredChildren.Clear();
				
			if (string.IsNullOrEmpty(this.Filter.SearchTerm))
			{
				return;
			}
			
			for (var i = 0; i < this.Children.Count; i++) 
			{
				InspectorProperty parentProperty = this.Children[i];

				for (var j = 0; j < parentProperty.Children.Count; j++) 
				{
					InspectorProperty property = parentProperty.Children[j];

					if (this.Filter.IsMatch(property, this.Filter.SearchTerm))
					{
						this.FilteredChildren.Add(parentProperty);
						break;
					}

					if (!this.Filter.Recursive) 
					{
						continue;
					}

					var foundMatch = false;
					
					foreach (InspectorProperty recursiveProperty in property.Children.Recurse()) 
					{
						if (!this.Filter.IsMatch(recursiveProperty, this.Filter.SearchTerm)) 
						{
							continue;
						}

						this.FilteredChildren.Add(parentProperty);
						foundMatch = true;
						break;
					}

					if (foundMatch) 
					{
						break;
					}
				}
			}
		}

		public void ScheduleUpdate() => Property.Tree.DelayActionUntilRepaint(this.ScheduledUpdateAction);

		public void Dispose()
		{
			if (this.OnChange != null)
			{
				this.Resolver.OnAfterChange -= this.OnChange;
			}
		}

		public class IndexNotFoundException : Exception
		{
			public IndexNotFoundException(InspectorProperty expectedProperty, int index) :
				base($"Couldn't find the index of a filtered property: {expectedProperty} @ {index}") { }
		}

		/// <summary>
		/// Retrieves the index of a filtered item; if the collection is not filtered, it just returns the passed index.
		/// </summary>
		/// <param name="index">The index to find.</param>
		/// 
		/// <returns>
		/// The index in the collection of the filtered item,
		/// or the passed index if the collection is not <see cref="IsFiltered">filtered</see>.
		/// </returns>
		/// 
		/// <exception cref="MultiCollectionFilter{TResolver}.IndexNotFoundException">
		/// This is thrown if it's unable to find the index in the original collection,
		/// this indicates a discrepancy between the filtered collection and the original collection.
		/// </exception>
		public int GetCollectionIndex(int index)
		{
			if (!this.IsFiltered)
			{
				return index;
			}

			InspectorProperty expected = this.FilteredChildren[index];

			for (var i = 0; i < this.Children.Count; i++)
			{
				if (this.Children[i] == expected)
				{
					return i;
				}
			}

			throw new IndexNotFoundException(expected, index);
		}
	}
}
#endif