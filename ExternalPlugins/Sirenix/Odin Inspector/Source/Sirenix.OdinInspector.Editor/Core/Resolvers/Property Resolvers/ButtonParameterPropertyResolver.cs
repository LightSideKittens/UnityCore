//-----------------------------------------------------------------------
// <copyright file="ButtonParameterPropertyResolver.cs" company="Sirenix ApS">
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

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class ButtonParameterPropertyResolver : OdinPropertyResolver
    {
        public const string RETURN_VALUE_NAME = "$Result";

        private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();
        private Dictionary<StringSlice, int> indexNameLookup = new Dictionary<StringSlice, int>(StringSliceEqualityComparer.Instance);

        private MethodInfo methodInfo;
        private ParameterInfo[] parameters;
        private object[] parameterValues;
        private object returnedValue;
        private Type returnType;

        public override bool CanResolveForPropertyFilter(InspectorProperty property)
        {
            if (property.Info.PropertyType != PropertyType.Method) return false;

            var info = property.Info.GetMemberInfo() as MethodInfo;

            if (info == null)
            {
                info = property.Info.GetMethodDelegate().Method;
            }

            if (info.IsGenericMethodDefinition) return false;
            return true;
        }

        protected override void Initialize()
        {
            this.methodInfo = (this.Property.Info.GetMemberInfo() as MethodInfo).DeAliasMethod();

            if (this.methodInfo == null)
            {
                this.methodInfo = this.Property.Info.GetMethodDelegate().Method.DeAliasMethod();
            }

            this.returnType = this.methodInfo.ReturnType;

            if (this.returnType == typeof(void))
                this.returnType = null;

            if (this.returnType == null)
            {
                this.parameters = this.methodInfo.GetParameters();
                this.parameterValues = new object[parameters.Length];
            }
            else
            {
                var temp = this.methodInfo.GetParameters();

                this.parameters = new ParameterInfo[temp.Length + 1];
                this.parameterValues = new object[parameters.Length];

                for (int i = 0; i < temp.Length; i++)
                {
                    this.parameters[i] = temp[i];
                }

                this.parameters[this.parameters.Length - 1] = this.methodInfo.ReturnParameter;
            }

            for (int i = 0; i < this.parameters.Length; i++)
            {
                string name = (this.returnType != null && i == this.parameters.Length - 1) ? RETURN_VALUE_NAME : this.parameters[i].Name;

                this.indexNameLookup[name] = i;

                var val = this.parameters[i].DefaultValue;

                if (val != DBNull.Value)
                {
                    this.parameterValues[i] = val;
                }
            }
        }

        public override int ChildNameToIndex(string name)
        {
            int index;
            if (this.indexNameLookup.TryGetValue(name, out index))
            {
                return index;
            }
            return -1;
        }

        public override int ChildNameToIndex(ref StringSlice name)
        {
            int index;
            if (this.indexNameLookup.TryGetValue(name, out index))
            {
                return index;
            }
            return -1;
        }

        public override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            InspectorPropertyInfo info;
            if (this.childInfos.TryGetValue(childIndex, out info))
            {
                return info;
            }
            
            var parameter = this.parameters[childIndex];

            var type = parameter.ParameterType;

            if (type.IsByRef)
            {
                type = type.GetElementType();
            }

            Type getterSetterType = null;
            IValueGetterSetter getterSetter = null;

            try
            {
                getterSetterType = typeof(GetterSetter<>).MakeGenericType(type);
                getterSetter = Activator.CreateInstance(getterSetterType, new object[] { this.parameterValues, childIndex }) as IValueGetterSetter;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }

            info = InspectorPropertyInfo.CreateValue((this.returnType != null && childIndex == this.parameters.Length - 1) ? RETURN_VALUE_NAME : parameter.Name, childIndex, SerializationBackend.None, getterSetter, parameter.GetAttributes());

            this.childInfos[childIndex] = info;
            return info;
        }

        protected override int CalculateChildCount()
        {
            return this.parameterValues.Length;
        }

        private class GetterSetter<T> : IValueGetterSetter<object, T>
        {
            private readonly object[] parameterValues;
            private readonly int index;

            public bool IsReadonly { get { return false; } }
            public Type OwnerType { get { return typeof(object); } }
            public Type ValueType { get { return typeof(T); } }

            public GetterSetter(object[] parameterValues, int index)
            {
                this.parameterValues = parameterValues;
                this.index = index;
            }

            public T GetValue(ref object owner)
            {
                object value = this.parameterValues[this.index];

                if (value == null) return default(T);

                try
                {
                    return (T)value;
                }
                catch { return default(T); }
            }

            public object GetValue(object owner)
            {
                return this.parameterValues[this.index];
            }

            public void SetValue(ref object owner, T value)
            {
                this.parameterValues[this.index] = value;
            }

            public void SetValue(object owner, object value)
            {
                this.parameterValues[this.index] = value;
            }
        }
    }
}
#endif