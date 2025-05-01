//-----------------------------------------------------------------------
// <copyright file="ArraySlice.cs" company="Sirenix ApS">
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

    using System.Runtime.CompilerServices;

    public struct ArraySlice<T>
    {
        public T[] OriginalArray;
        public int Offset;
        public int Length;

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | (MethodImplOptions)512)]
            get
            {
                return ref this.OriginalArray[index + this.Offset];
            }
        }

        public ArraySlice(T[] array, int offset, int length)
        {
            this.OriginalArray = array;
            this.Offset = offset;
            this.Length = length;
        }

        public Iterator GetEnumerator()
        {
            return new Iterator(this);
        }

        public struct Iterator
        {
            private int index;
            private ArraySlice<T> arr;

            public Iterator(ArraySlice<T> arr)
            {
                this.index = -1;
                this.arr = arr;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | (MethodImplOptions)512)]
            public bool MoveNext()
            {
                this.index++;
                return this.index < this.arr.Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | (MethodImplOptions)512)]
            public void Reset()
            {
                this.index = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | (MethodImplOptions)512)]
            public void Dispose() { }

            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining | (MethodImplOptions)512)]
                get { return ref arr[this.index]; }
            }
        }
    }
}
#endif