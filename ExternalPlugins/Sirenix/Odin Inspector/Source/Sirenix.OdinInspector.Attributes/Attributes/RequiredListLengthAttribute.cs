//-----------------------------------------------------------------------
// <copyright file="RequiredListLengthAttribute.cs" company="Sirenix ApS">
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

    /// <summary>
    /// An attribute that can be applied to lists, arrays and other types of collections to ensure they contain a specified number of elements.
    /// </summary>
    /// <example>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     [RequiredListLength(10)]
    ///     public int[] fixedLength;
    /// 
    ///     [RequiredListLength(1, null)]
    ///     public int[] minLength;
    /// 
    ///     [RequiredListLength(null, 10)]
    ///     public List<int> maxLength;
    /// 
    ///     [RequiredListLength(3, 10)]
    ///     public List<int> minAndMaxLength;
    /// 
    ///     public int SomeNumber;
    /// 
    ///     [RequiredListLength("@this.GetComponent<Vehicle>().NumberOfWheels")]
    ///     public List<GameObject> matchLengthOfOther;
    /// 
    ///     [RequiredListLength("@this.SomeNumber", null)]
    ///     public int[] minLengthExpression;
    /// 
    ///     [RequiredListLength(null, "@this.SomeNumber")]
    ///     public List<int> maxLengthExpression;
    /// }
    /// </code>
    /// </example>
    public sealed class RequiredListLengthAttribute : Attribute
    {
        private PrefabKind prefabKind;
        private bool prefabKindIsSet;

        private int minLength;
        private int maxLength;
        private bool minLengthIsSet;
        private bool maxLengthIsSet;

        /// <summary>
        /// The minimum length of the collection. If not set, there is no minimum length restriction.
        /// </summary>
        public int MinLength
        {
            get { return minLength; }
            set
            {
                minLength = value;
                minLengthIsSet = true;
            }
        }

        /// <summary>
        /// The maximum length of the collection. If not set, there is no maximum length restriction.
        /// </summary>
        public int MaxLength
        {
            get { return maxLength; }
            set
            {
                maxLength = value;
                maxLengthIsSet = true;
            }
        }

        public bool MinLengthIsSet => minLengthIsSet;

        public bool MaxLengthIsSet => maxLengthIsSet;

        public bool PrefabKindIsSet => this.prefabKindIsSet;

        public PrefabKind PrefabKind
        {
            get => this.prefabKind;
            set
            {
                this.prefabKind = value;
                this.prefabKindIsSet = true;
            }
        }

        /// <summary>
        /// A C# expression for getting the minimum length of the collection, for example "@this.otherList.Count".
        /// If set, MinLength will be the fallback in case nothing in case MinLengthGetter returns null.
        /// </summary>
        public string MinLengthGetter;

        /// <summary>
        /// A C# expression for getting the maximum length of the collection, for example "@this.otherList.Count".
        /// If set, MaxLength will be the fallback in case nothing in case MaxLengthGetter returns null.
        /// </summary>
        public string MaxLengthGetter;

        /// <summary>
        /// Limits the collection to be contain the specified number of elements.
        /// </summary>
        public RequiredListLengthAttribute()
        {
        }

        /// <summary>
        /// Limits the collection to be contain the specified number of elements.
        /// </summary>
        /// <param name="fixedLength">The minimum and maximum length of the collection.</param>
        public RequiredListLengthAttribute(int fixedLength)
        {
            this.MinLength = fixedLength;
            this.MaxLength = fixedLength;
        }

        /// <summary>
        /// Limits the collection to be contain the specified number of elements.
        /// </summary>
        /// <param name="minLength">The minimum length of the collection.</param>
        /// <param name="maxLength">The maximum length of the collection.</param>
        public RequiredListLengthAttribute(int minLength, int maxLength)
        {
            this.MinLength = minLength;
            this.MaxLength = maxLength;
        }

        /// <summary>
        /// Limits the collection to be contain the specified number of elements.
        /// </summary>
        /// <param name="minLength">The minimum length of the collection.</param>
        /// <param name="maxLengthGetter">A C# expression for getting the maximum length of the collection, for example "@this.otherList.Count". If set, MaxLength will be the fallback in case nothing in case MaxLengthGetter returns null.</param>
        public RequiredListLengthAttribute(int minLength, string maxLengthGetter)
        {
            this.MinLength = minLength;
            this.MaxLengthGetter = maxLengthGetter;
        }

        /// <summary>
        /// Limits the collection to be contain the specified number of elements.
        /// </summary>
        /// <param name="fixedLengthGetter">The minimum and maximum length of the collection.</param>
        public RequiredListLengthAttribute(string fixedLengthGetter)
        {
            this.MinLengthGetter = fixedLengthGetter;
            this.MaxLengthGetter = fixedLengthGetter;
        }

        /// <summary>
        /// Limits the collection to be contain the specified number of elements.
        /// </summary>
        /// <param name="minLengthGetter">A C# expression for getting the minimum length of the collection, for example "@this.otherList.Count". If set, MinLength will be the fallback in case nothing in case MinLengthGetter returns null.</param>
        /// <param name="maxLengthGetter">A C# expression for getting the maximum length of the collection, for example "@this.otherList.Count". If set, MaxLength will be the fallback in case nothing in case MaxLengthGetter returns null.</param>
        public RequiredListLengthAttribute(string minLengthGetter, string maxLengthGetter)
        {
            this.MinLengthGetter = minLengthGetter;
            this.MaxLengthGetter = maxLengthGetter;
        }

        /// <summary>
        /// Limits the collection to be contain the specified number of elements.
        /// </summary>
        /// <param name="minLengthGetter">A C# expression for getting the minimum length of the collection, for example "@this.otherList.Count". If set, MinLength will be the fallback in case nothing in case MinLengthGetter returns null.</param>
        /// <param name="maxLength">The maximum length of the collection.</param>
        public RequiredListLengthAttribute(string minLengthGetter, int maxLength)
        {
            this.MinLengthGetter = minLengthGetter;
            this.MaxLength = maxLength;
        }
    }
}