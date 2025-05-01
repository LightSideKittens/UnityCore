//-----------------------------------------------------------------------
// <copyright file="GUIScopeStack.cs" company="Sirenix ApS">
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

    using System.Collections.Generic;
    using UnityEngine;

    public class GUIScopeStack<T>
    {
        public Stack<T> InnerStack = new Stack<T>();
        private GUIFrameCounter guiState = new GUIFrameCounter();

        public int Count
        {
            get
            {
                if (this.guiState.Update().IsNewFrame)
                {
                    this.InnerStack.Clear();
                }

                return this.InnerStack.Count;
            }
        }

        public void Push(T t)
        {
            if (this.guiState.Update().IsNewFrame)
            {
                this.InnerStack.Clear();
            }

            this.InnerStack.Push(t);
        }

        public T Pop()
        {
            if (this.Count == 0)
            {
                Debug.LogError("Pop call mismatch; no corresponding push call! Each call to Pop must always correspond to one - and only one - call to Push.");
                return default(T);
            }
            else if (this.guiState.Update().IsNewFrame)
            {
                Debug.LogError("Pop call mismatch; no corresponding push call! Each call to Pop must always correspond to one - and only one - call to Push.");
                this.InnerStack.Clear();
                return default(T);
            }
            else
            {
                return this.InnerStack.Pop();
            }
        }

        public T Peek()
        {
            if (this.guiState.Update().IsNewFrame)
            {
                this.InnerStack.Clear();
            }

            return this.InnerStack.Peek();
        }
    }
}
#endif