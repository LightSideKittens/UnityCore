//-----------------------------------------------------------------------
// <copyright file="Expressionator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities.Editor.Expressions.Internal
{
#pragma warning disable

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Expressionator
    {
        public Expressionator(object value)
        {
            this.value = value;
        }

        [SerializeField]
        private object value;

        [SerializeField]
        public readonly Dictionary<string, object> Context = new Dictionary<string, object>();

        public object Value
        {
            get { return this.value ?? this; }
            set { this.value = value; }
        }

        public Type ValueType
        {
            get { return this.Value.GetType(); }
        }

        public object this[string expression]
        {
            get
            {
                return this.Expr(expression);
            }
        }

        public Expressionator Expressionate(string expression)
        {
            return new Expressionator(this.Expr(expression));
        }

        public bool Bool(string expression) => this.Expr<bool>(expression);
        public string String(string expression) => this.Expr<string>(expression);
        public int Int(string expression) => this.Expr<int>(expression);

        public object Expr(string expression)
        {
            return this.Expr<object>(expression);
        }

        private bool GetContextParameters(out Type[] parameters, out string[] parameterNames, out object[] parameterValues)
        {
            if (this.Context.Count == 0)
            {
                parameters = null;
                parameterNames = null;
                parameterValues = null;
                return false;
            }
            else
            {
                parameters = new Type[this.Context.Count];
                parameterNames = new string[this.Context.Count];
                parameterValues = new object[this.Context.Count + 1];

                int i = 0;

                foreach (var entry in this.Context)
                {
                    parameterNames[i] = entry.Key;
                    parameterValues[i + 1] = entry.Value;
                    parameters[i] = entry.Value == null ? typeof(object) : entry.Value.GetType();

                    i++;
                }
                return true;
            }
        }

        public T Expr<T>(string expression)
        {
            var hasParameters = this.GetContextParameters(out var parameters, out var parameterNames, out var parameterValues);

            var del = ExpressionUtility.ParseExpression(expression, new EmitContext()
            {
                IsStatic = false,
                Type = this.ValueType,
                Parameters = parameters,
                ParameterNames = parameterNames,
            }, out string error);

            if (error != null)
            {
                throw new Exception(error);
            }

            if (hasParameters)
            {
                parameterValues[0] = this.Value;
                return (T)del.DynamicInvoke(parameterValues);
            }
            else
            {
                return (T)del.DynamicInvoke(this.Value);
            }
        }

        public IEnumerable<Expressionator> ForEachExpressionate(string expression)
        {
            var enumerable = this.Expr<IEnumerable>(expression);

            if (enumerable != null)
            {
                foreach (var result in enumerable)
                {
                    yield return new Expressionator(result);
                }
            }
        }
    }
}
#endif