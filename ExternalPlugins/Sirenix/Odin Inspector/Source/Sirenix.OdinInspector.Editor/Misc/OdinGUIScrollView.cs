//-----------------------------------------------------------------------
// <copyright file="OdinGUIScrollView.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

	public class OdinGUIScrollView
	{
		public const float SCROLL_BAR_SIZE = 12;
	 
		internal const int NONE_REFERENCE_ID = -1;

		internal struct RectInfo
		{
			public int ReferenceId;
			public float Indentation;
			public float Height;
		}

		internal struct VisibleItem
		{
			public Rect Position;
			public object Reference;
			public float Indentation;

			public VisibleItem(float x, float y, float width, float height, object reference, float indentation)
			{
				this.Position = new Rect(x, y, width, height);
				this.Reference = reference;
				this.Indentation = indentation;
			}
		}

		public readonly struct VisibleItems
		{
			public static VisibleItems None => new VisibleItems(0, 0, Array.Empty<VisibleItem>());

			public readonly int Length;
			
			public readonly int Offset;

			internal readonly VisibleItem[] VisibleItemsBuffer;

			internal VisibleItems(int offset, int length, VisibleItem[] visibleItemsBuffer)
			{
				this.Offset = offset;
				this.Length = length;
				this.VisibleItemsBuffer = visibleItemsBuffer;
			}

			public Rect this[int index] => this.GetRect(index);

			public Rect GetRect(int index) => this.VisibleItemsBuffer[index].Position;

			/// <summary>
			/// Checks whether the allocated <see cref="Rect"/> has data associated with it.
			/// </summary>
			/// <param name="index">The index of the <see cref="Rect"/> to check.</param>
			/// <returns><c>true</c> if the <see cref="Rect"/> has data associated with it; otherwise <c>false</c>.</returns>
			public bool HasAssociatedData(int index) => this.VisibleItemsBuffer[index].Reference != null;

			/// <summary>
			/// Gets the data associated with the <see cref="Rect"/> at the given <paramref name="index"/>; this is the second parameter assigned in the <see cref="OdinGUIScrollView.AllocateRect"/> method.
			/// </summary>
			/// <param name="index">The index of the <see cref="Rect"/> to retrieve the associated data from.</param>
			/// <returns>The associated data.</returns>
			public object GetAssociatedData(int index) => this.VisibleItemsBuffer[index].Reference;

			/// <summary>
			/// Gets the data associated with the <see cref="Rect"/> at the given <see cref="index"/>; this is the second parameter assigned in the <see cref="OdinGUIScrollView.AllocateRect"/> method.
			/// </summary>
			/// <param name="index">The index of the <see cref="Rect"/> to retrieve the associated data from.</param>
			/// <typeparam name="T">The expected associated data type.</typeparam>
			/// <returns>The associated data.</returns>
			public T GetAssociatedData<T>(int index) => (T) this.VisibleItemsBuffer[index].Reference;

			/// <summary>
			/// Gets the indentation set for the <see cref="Rect"/> at the given <paramref name="index"/>.
			/// </summary>
			/// <param name="index">The <paramref name="index"/> of the <see cref="Rect"/> to retrieve the indentation for.</param>
			/// <returns>The indentation for the <see cref="Rect"/>.</returns>
			/// <remarks>The indentation is set using <see cref="OdinGUIScrollView.Indentation"/> during <see cref="OdinGUIScrollView.BeginAllocations"/> and <see cref="OdinGUIScrollView.EndAllocations"/>.</remarks>
			public float GetIndentation(int index) => this.VisibleItemsBuffer[index].Indentation;

			/// <summary>
			/// Creates a <see cref="Rect"/> representing all the visible <see cref="Rect"/>'s combined.
			/// </summary>
			/// <returns>The created <see cref="Rect"/>.</returns>
			public Rect GetCombinedRects()
			{
				if (this.Length < 1)
				{
					return Rect.zero;
				}

				Rect result = this[0];

				for (var i = 1; i < this.Length; i++)
				{
					result.height += this.VisibleItemsBuffer[i].Position.height;
				}

				return result;
			}
		}

		public int Length => this.NextRectInfoIndex;
		public int ReferencedAmount => this.NextReferenceId;

		public bool IsBeyondVerticalBounds
		{
			get
			{
				if (this.AdjustViewForVerticalScrollBar)
				{
					return this.ViewRect.height > this.Bounds.height;
				}

				return this.ViewRect.height > this.InteractRect.height;
			}
		}

		public bool IsBeyondHorizontalBounds
		{
			get
			{
				if (this.AdjustViewForVerticalScrollBar)
				{
					return this.ViewRect.width > this.Bounds.width;
				}

				return this.ViewRect.width > this.InteractRect.width;
			}
		}

		public bool IsBeyondBounds => this.IsBeyondVerticalBounds && this.IsBeyondHorizontalBounds;
		public bool IsBeyondAnyBounds => this.IsBeyondVerticalBounds || this.IsBeyondHorizontalBounds;

		public bool IsDraggingMouse => SharedUniqueControlId.IsActive && this.isDraggingMouse;
		public bool IsDraggingVerticalScrollBar => SharedUniqueControlId.IsActive && this.isDraggingVertical;
		public bool IsDraggingHorizontalScrollBar => SharedUniqueControlId.IsActive && this.isDraggingHorizontal;

		public Vector2 Position
		{
			get => this.CurrentPosition;

			set
			{
				this.CurrentPosition = value;
				this.NextPosition = value;
				this.AnimatedPosition = value;
			}
		}

		public float PositionX
		{
			get => this.Position.x;

			set
			{
				this.CurrentPosition.x = value;
				this.NextPosition.x = value;
				this.AnimatedPosition.Start.x = value;
				this.AnimatedPosition.Destination.x = value;
			}
		}

		public float PositionY
		{
			get => this.Position.y;

			set
			{
				this.CurrentPosition.y = value;
				this.NextPosition.y = value;
				this.AnimatedPosition.Start.y = value;
				this.AnimatedPosition.Destination.y = value;
			}
		}
		
		public Rect Bounds;
		public Rect ViewRect;
		public Rect InteractRect;
		public float Indentation;

		internal int NextReferenceId;
		internal int NextRectInfoIndex;
		internal object[] ReferencedObjects;
		internal RectInfo[] RectInfos;
		internal VisibleItem[] VisibleItemsBuffer = new VisibleItem[64];
		internal Stack<int> FreedReferenceIds = new Stack<int>();

		internal bool AdjustViewForVerticalScrollBar;
		internal Rect VerticalScrollBarRect;
		internal Rect HorizontalScrollBarRect;
		internal bool isDraggingVertical;
		internal bool isDraggingHorizontal;
		internal bool isDraggingMouse;

		internal bool IsScrollWaitingUntilDone = false;
		internal Vector2 CurrentPosition = Vector2.zero;
		internal Vector2 NextPosition = Vector2.zero;
		internal SirenixAnimationUtility.InterpolatedVector2 AnimatedPosition = Vector2.zero;
		internal float ScrollSpeed;
		internal Easing ScrollEasing;

		public OdinGUIScrollView(int capacity, int? referenceCapacity = null, bool adjustViewForVerticalScrollBar = true)
		{
			if (capacity < 1)
			{
				throw new ArgumentException($"{nameof(capacity)} can't be less than 1.");
			}

			if (referenceCapacity.HasValue)
			{
				if (referenceCapacity.Value < 1)
				{
					throw new ArgumentException($"{nameof(referenceCapacity)} can't be less than 1.");
				}
			}
			else
			{
				referenceCapacity = capacity;
			}

			this.AnimatedPosition.Time = 1.0f;

			this.RectInfos = new RectInfo[capacity];
			this.ReferencedObjects = new object[referenceCapacity.Value];

			this.AdjustViewForVerticalScrollBar = adjustViewForVerticalScrollBar;
		}

		public void SetBounds(Rect bounds, float viewWidth = float.NaN)
		{
			this.Bounds = bounds;
			this.ViewRect = new Rect(0, 0, float.IsNaN(viewWidth) ? bounds.width : viewWidth, 0);
			this.InteractRect = bounds;
		}

		// TODO: this name is a mess, it's meant to be called when you want to update the bounds, but don't want to redo any allocations
		public void SetBoundsForCurrentAllocations(Rect bounds, float viewWidth = float.NaN)
		{
			this.Bounds = bounds;
			this.ViewRect.width = float.IsNaN(viewWidth) ? bounds.width : viewWidth;
			this.InteractRect = bounds;

			this.VerticalScrollBarRect = Rect.zero;
			this.HorizontalScrollBarRect = Rect.zero;

			this.EndAllocations();
		}

		public void BeginAllocations()
		{
			this.ViewRect.height = 0;
			this.NextReferenceId = 0;
			this.NextRectInfoIndex = 0;

			this.VerticalScrollBarRect = Rect.zero;
			this.HorizontalScrollBarRect = Rect.zero;
		}

		public void EndAllocations()
		{
			if (GUIUtility.hotControl == 0)
			{
				this.isDraggingHorizontal = false;
				this.isDraggingVertical = false;
			}

			if (this.IsBeyondHorizontalBounds)
			{
				this.HorizontalScrollBarRect = this.Bounds.TakeFromBottom(SCROLL_BAR_SIZE);
			}
			else
			{
				this.PositionX = 0.0f;
			}

			if (this.IsBeyondVerticalBounds)
			{
				this.VerticalScrollBarRect = this.Bounds.TakeFromRight(SCROLL_BAR_SIZE);

				if (this.HorizontalScrollBarRect != Rect.zero)
				{
					this.VerticalScrollBarRect.height += SCROLL_BAR_SIZE;
					this.HorizontalScrollBarRect.width -= this.VerticalScrollBarRect.width;
				}

				if (this.AdjustViewForVerticalScrollBar)
				{
					this.ViewRect.width -= SCROLL_BAR_SIZE;
				}
			}
			else
			{
				this.PositionY = 0.0f;
			}
		}

		public void Space(float amount) => this.AllocateRect(amount);

		/// <summary>
		/// Allocates an <see cref="Rect"/> in the view, with the option to associate a given <see cref="object"/> with it.
		/// </summary>
		/// <param name="height"></param>
		/// <param name="reference"></param>
		/// <remarks>Ensure <see cref="BeginAllocations"/> is called before calling this, and ensure <see cref="EndAllocations"/> is called after you're done with <see cref="AllocateRect"/></remarks>
		public void AllocateRect(float height, object reference = null)
		{
			if (this.NextRectInfoIndex >= this.RectInfos.Length)
			{
				Array.Resize(ref this.RectInfos, this.RectInfos.Length * 2);
			}

			ref RectInfo rectInfo = ref this.RectInfos[this.NextRectInfoIndex++];

			rectInfo.Indentation = this.Indentation;
			rectInfo.Height = height;

			if (reference != null)
			{
				if (this.NextReferenceId >= this.ReferencedObjects.Length)
				{
					Array.Resize(ref this.ReferencedObjects, this.ReferencedObjects.Length * 2);
				}

				this.ReferencedObjects[this.NextReferenceId] = reference;
				rectInfo.ReferenceId = this.NextReferenceId;
				this.NextReferenceId++;
			}
			else
			{
				rectInfo.ReferenceId = NONE_REFERENCE_ID;
			}

			this.ViewRect.height += height;
		}

		public void ReallocateRect(int index, float height, object reference = null)
		{
			ref RectInfo currentRectInfo = ref this.RectInfos[index];

			float difference = currentRectInfo.Height - height;

			currentRectInfo.Height = height;

			this.ViewRect.height -= difference;

			if (reference != null)
			{
				if (currentRectInfo.ReferenceId != NONE_REFERENCE_ID)
				{
					this.ReferencedObjects[currentRectInfo.ReferenceId] = reference;
				}
				else
				{
					if (this.FreedReferenceIds.Count > 0)
					{
						int referenceId = this.FreedReferenceIds.Pop();

						currentRectInfo.ReferenceId = referenceId;
						this.ReferencedObjects[referenceId] = reference;
					}
					else
					{
						if (this.NextReferenceId >= this.ReferencedObjects.Length)
						{
							Array.Resize(ref this.ReferencedObjects, this.ReferencedObjects.Length * 2);
						}

						this.ReferencedObjects[this.NextReferenceId] = reference;
						currentRectInfo.ReferenceId = this.NextReferenceId;
						this.NextReferenceId++;
					}
				}
			}
			else
			{
				if (currentRectInfo.ReferenceId != NONE_REFERENCE_ID)
				{
					this.FreedReferenceIds.Push(currentRectInfo.ReferenceId);
				}

				currentRectInfo.ReferenceId = NONE_REFERENCE_ID;
			}
		}

		public void BeginScrollView(Vector2? offset = null, Vector2? addViewSize = null, float scrollSpeed = 36.0f)
		{
			Vector2 offsetValue = offset ?? Vector2.zero;

			Rect clipRect = this.Bounds;

			clipRect.position += offsetValue;
			clipRect.size -= offsetValue;

			Rect clipViewRect = this.ViewRect;

			if (this.VerticalScrollBarRect != Rect.zero) { }

			if (addViewSize.HasValue)
			{
				clipViewRect.size += addViewSize.Value;
			}

			if (Event.current.IsMouseOver(this.InteractRect))
			{
#if true
				if (Event.current.type == EventType.ScrollWheel)
				{
					if (Event.current.modifiers == EventModifiers.Shift)
					{
						// NOTE: later Unity versions utilize the x-axis for this use case, so we just want the one with the non-zero value.
						if (Event.current.delta.x != 0.0f)
						{
							this.NextPosition.x += Event.current.delta.x * scrollSpeed;
						}
						else
						{
							this.NextPosition.x += Event.current.delta.y * scrollSpeed;
						}
					}
					else
					{
						this.NextPosition.y += Event.current.delta.y * scrollSpeed;
					}

					if (this.NextPosition.y < 0.0f)
					{
						this.NextPosition.y = 0.0f;
					}

					this.ScrollTo(this.NextPosition, 1.0f / 0.35f, Easing.OutCubic, false);
				}

#else
				if (Event.current.type == EventType.ScrollWheel)
				{
					if (Event.current.modifiers == EventModifiers.Shift)
					{
						this.Position.x += Event.current.delta.y * scrollSpeed;
					}
					else
					{
						this.Position.y += Event.current.delta.y * scrollSpeed;
					}
				}
#endif
			}

			this.HandleSmoothScrolling();


			if (this.HorizontalScrollBarRect != Rect.zero && clipRect.width >= 0.0f)
			{
#if true
				float scaleFactor = clipRect.width / clipViewRect.width;

				if (Event.current.type == EventType.MouseDrag && this.IsDraggingHorizontalScrollBar)
				{
					this.PositionX += Event.current.delta.x / scaleFactor;
				}

				if (this.CurrentPosition.x + clipRect.width > clipViewRect.width)
				{
					float diff = this.CurrentPosition.x + clipRect.width - clipViewRect.width;

					this.PositionX -= diff;
				}

				if (this.CurrentPosition.x < 0.0f)
				{
					this.PositionX = 0.0f;
				}

				ScrollBackground(this.HorizontalScrollBarRect, false);

				Rect buttonRect = this.HorizontalScrollBarRect;

				buttonRect.x += this.CurrentPosition.x * (this.HorizontalScrollBarRect.width / clipViewRect.width);
				buttonRect.width = this.HorizontalScrollBarRect.width * scaleFactor;

				if (ScrollButton(buttonRect, this.IsDraggingHorizontalScrollBar))
				{
					SharedUniqueControlId.SetActive();
					this.isDraggingHorizontal = true;
				}

				if (this.IsDraggingHorizontalScrollBar && Event.current.OnMouseUp(0))
				{
					SharedUniqueControlId.SetInactive();
					this.isDraggingHorizontal = false;
				}
#else
				this.CurrentPosition.x = GUI.HorizontalScrollbar(this.HorizontalScrollBarRect,
																				 this.Position.x,
																				 clipRect.width,
																				 clipViewRect.x,
																				 clipViewRect.width,
																				 this.HorizontalScrollBarStyle ?? GUI.skin.horizontalScrollbar);
#endif
			}
			else
			{
				this.PositionX = 0.0f;
			}

			if (this.VerticalScrollBarRect.width > 0.0f && clipRect.height >= 0.0f)
			{
				float scaleFactor = clipRect.height / clipViewRect.height;

				if (Event.current.type == EventType.MouseDrag && this.IsDraggingVerticalScrollBar)
				{
					this.PositionY += Event.current.delta.y / scaleFactor;
				}

				if (this.CurrentPosition.y + clipRect.height > clipViewRect.height)
				{
					float diff = this.CurrentPosition.y + clipRect.height - clipViewRect.height;

					this.PositionY -= diff;
				}

				if (this.CurrentPosition.y < 0.0f)
				{
					this.PositionY = 0.0f;
				}

				ScrollBackground(this.VerticalScrollBarRect, true);

				Rect buttonRect = this.VerticalScrollBarRect;

				buttonRect.y += this.CurrentPosition.y * (this.VerticalScrollBarRect.height / clipViewRect.height);
				buttonRect.height = this.VerticalScrollBarRect.height * scaleFactor;

				if (ScrollButton(buttonRect, this.IsDraggingVerticalScrollBar))
				{
					SharedUniqueControlId.SetActive();
					this.isDraggingVertical = true;
				}

				if (this.IsDraggingVerticalScrollBar && Event.current.OnMouseUp(0))
				{
					SharedUniqueControlId.SetInactive();
					this.isDraggingVertical = false;
				}
			}
			else if (!this.IsBeyondVerticalBounds)
			{
				this.PositionY = 0.0f;
			}

			GUI.BeginClip(clipRect, -this.Position, Vector2.zero, false);
		}

		public void EndScrollView()
		{
			GUI.EndClip();

			if (this.IsBeyondAnyBounds)
			{
				GUIHelper.RequestRepaint();
			}
		}

		public Rect GetClipRect() => this.Bounds;

		public Rect GetViewClipRect() => new Rect(this.Bounds.position, this.ViewRect.size);

		public void BeginClip(Rect? clipRect = null, Vector2? offset = null, bool ignoreScrollX = false, bool ignoreScrollY = false)
		{
			Rect clipRectValue = clipRect ?? this.GetClipRect();

			Vector2 offsetValue = offset ?? Vector2.zero;

			clipRectValue.position += offsetValue;
			clipRectValue.size -= offsetValue;

			var scrollPosition = new Vector2(ignoreScrollX ? 0.0f : this.Position.x, ignoreScrollY ? 0.0f : this.Position.y);

			GUI.BeginClip(clipRectValue, -scrollPosition, Vector2.zero, false);
		}

		public void EndClip() => GUI.EndClip();

		public void ScrollTo(Vector2 position, float speed, Easing easing = Easing.Linear, bool waitUntilDone = true)
		{
			this.NextPosition = position;
			this.ScrollEasing = easing;
			this.ScrollSpeed = speed;
			this.IsScrollWaitingUntilDone = waitUntilDone;

			if (waitUntilDone)
			{
				this.AnimatedPosition.ChangeDestination(this.NextPosition);
			}
		}

		public void ScrollTo(float speed, float xPosition = float.NaN, float yPosition = float.NaN, Easing easing = Easing.Linear, bool waitUntilDone = true)
		{
			this.NextPosition = new Vector2(float.IsNaN(xPosition) ? this.NextPosition.x : xPosition,
													  float.IsNaN(yPosition) ? this.NextPosition.y : yPosition);

			this.ScrollEasing = easing;
			this.ScrollSpeed = speed;
			this.IsScrollWaitingUntilDone = waitUntilDone;

			if (waitUntilDone)
			{
				this.AnimatedPosition.ChangeDestination(this.NextPosition);
			}
		}

		public bool HandleMiddleMouseDrag(bool inverted, bool useEvents = false, float speed = 1.0f)
		{
			if (GUIUtility.hotControl == 0)
			{
				this.isDraggingMouse = false;
			}

			if (this.IsDraggingMouse && Event.current.OnMouseUp(2))
			{
				this.isDraggingMouse = false;
				SharedUniqueControlId.SetInactive();
			}

			if (Event.current.OnMouseDown(this.InteractRect, 2))
			{
				SharedUniqueControlId.SetActive();
				this.isDraggingMouse = true;
			}

			if (!this.IsDraggingMouse)
			{
				return false;
			}

			if (Event.current.type != EventType.MouseDrag)
			{
				return false;
			}

			if (inverted)
			{
				this.NextPosition -= Event.current.delta * speed;
			}
			else
			{
				this.NextPosition += Event.current.delta * speed;
			}

			this.ScrollTo(this.NextPosition, 1.0f / 0.35f, Easing.OutCubic, false);

			if (useEvents)
			{
				Event.current.Use();
			}

			return true;
		}

		public object GetReferencedObject(int index) => this.ReferencedObjects[index];

		public void Resize(int capacity, int? referenceCapacity = null)
		{
			if (capacity < 1)
			{
				throw new ArgumentException($"{nameof(capacity)} can't be less than 1.");
			}

			if (referenceCapacity.HasValue)
			{
				if (referenceCapacity.Value < 1)
				{
					throw new ArgumentException($"{nameof(referenceCapacity)} can't be less than 1.");
				}
			}
			else
			{
				referenceCapacity = capacity;
			}

			Array.Resize(ref this.RectInfos, capacity);
			Array.Resize(ref this.ReferencedObjects, referenceCapacity.Value);
		}

		public void ResizeToFit()
		{
			Array.Resize(ref this.RectInfos, this.NextRectInfoIndex);
			Array.Resize(ref this.ReferencedObjects, this.NextReferenceId);
		}

		public VisibleItems GetVisibleItems()
		{
			const int LENGTH_NOT_SET = -1;

			var offset = 0;
			int length = LENGTH_NOT_SET;

			var currentVisibleHeight = 0.0f;

			var yMin = 0.0f;
			var yMax = 0.0f;

			for (var i = 0; i < this.Length; i++)
			{
				ref RectInfo rectInfo = ref this.RectInfos[i];

				yMax += rectInfo.Height;

				if (!(this.Position.y >= yMin && this.Position.y <= yMax))
				{
					yMin = yMax;
					continue;
				}

				offset = i;

				currentVisibleHeight = yMax - this.Position.y;
				break;
			}

			for (int i = offset + 1; i < this.Length; i++)
			{
				if (currentVisibleHeight >= this.Bounds.height)
				{
					length = i - offset + 1;
					break;
				}

				currentVisibleHeight += this.RectInfos[i].Height;
			}

			if (length == LENGTH_NOT_SET)
			{
				length = this.Length - offset;
			}

			if (length < 1)
			{
				return VisibleItems.None;
			}

			if (this.VisibleItemsBuffer.Length < length)
			{
				Array.Resize(ref this.VisibleItemsBuffer, length + 16);
			}

			for (var i = 0; i < length; i++)
			{
				ref RectInfo rectInfo = ref this.RectInfos[offset + i];

				if (rectInfo.ReferenceId == NONE_REFERENCE_ID)
				{
					this.VisibleItemsBuffer[i] = new VisibleItem(this.ViewRect.x,
																				yMin,
																				this.ViewRect.width,
																				rectInfo.Height,
																				null,
																				rectInfo.Indentation);
				}
				else
				{
					this.VisibleItemsBuffer[i] = new VisibleItem(this.ViewRect.x,
																				yMin,
																				this.ViewRect.width,
																				rectInfo.Height,
																				this.ReferencedObjects[rectInfo.ReferenceId],
																				rectInfo.Indentation);
				}

				yMin += rectInfo.Height;
			}

			return new VisibleItems(offset, length, this.VisibleItemsBuffer);
		}

		private void HandleSmoothScrolling()
		{
			if (!this.IsScrollWaitingUntilDone)
			{
				this.AnimatedPosition.ChangeDestination(this.NextPosition);
			}

			if (this.AnimatedPosition.IsDone)
			{
				this.IsScrollWaitingUntilDone = false;
				
				return;
			}

			this.AnimatedPosition.Move(this.ScrollSpeed, this.ScrollEasing);

			this.CurrentPosition = this.AnimatedPosition;
		}

		public static bool ScrollButton(Rect position, bool isDragging)
		{
			if (position.height < 10.0f)
			{
				position.height = 10.0f;
			}

			Rect contentPosition = position.Padding(3);

			bool isMouseOver = Event.current.IsMouseOver(position);

			if (EditorGUIUtility.isProSkin)
			{
				SirenixEditorGUI.DrawRoundRect(contentPosition, new FancyColor(isMouseOver || isDragging ? 0.4f : 0.3f), 5.0f);
			}
			else
			{
				SirenixEditorGUI.DrawRoundRect(contentPosition, new FancyColor(isMouseOver || isDragging ? 0.48f : 0.54f), 5.0f);
			}

			return Event.current.OnMouseDown(position, 0);
		}

		public static void ScrollBackground(Rect position, bool isVertical)
		{
			if (EditorGUIUtility.isProSkin)
			{
				EditorGUI.DrawRect(position, new FancyColor(0.1f));
			}
			else
			{
				EditorGUI.DrawRect(position, new FancyColor(0.66f));
			}

			EditorGUI.DrawRect(isVertical ? position.AlignLeft(1) : position.AlignTop(1), new FancyColor(0, 0.4f));
		}
	}
}
#endif