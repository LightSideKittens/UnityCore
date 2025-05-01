//-----------------------------------------------------------------------
// <copyright file="ExpressionUtility.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities.Editor.Expressions
{
#pragma warning disable

    using System;
#if !ODIN_LIMITED_VERSION
    using System.Collections.Generic;
    using System.Threading;
#endif

    /// <summary>
    /// Utility for parsing and emitting expression delegates.
    /// </summary>
    public static class ExpressionUtility
    {
#if !ODIN_LIMITED_VERSION
        private struct CachedExpressionKey : IEquatable<CachedExpressionKey>
        {
            public string Expression;
            public bool RichTextError;
            public Type DelegateType;
            public bool IsStatic;
            public Type Context;
            public Type ReturnType;
            public Type[] Parameters;
            public string[] ParameterNames;
            public int? Hash;

            public CachedExpressionKey(string expression, bool richTextError, Type delegateType, EmitContext context)
            {
                this.Expression = expression;
                this.RichTextError = richTextError;
                this.DelegateType = delegateType;
                this.IsStatic = context.IsStatic;
                this.Context = context.Type;
                this.ReturnType = context.ReturnType;
                this.Parameters = context.Parameters;
                this.ParameterNames = context.ParameterNames;
                this.Hash = null;
                this.Hash = GetHashCode();
            }

            public CachedExpressionKey(string expression, bool richTextError, Type delegateType, bool isStatic, Type context, Type returnType, Type[] parameters, string[] parameterNames)
            {
                this.Expression = expression;
                this.RichTextError = richTextError;
                this.DelegateType = delegateType;
                this.IsStatic = isStatic;
                this.Context = context;
                this.ReturnType = returnType;
                this.Parameters = parameters;
                this.ParameterNames = parameterNames;
                this.Hash = null;
                this.Hash = GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return obj is CachedExpressionKey key &&
                       this.Equals(key);
            }

            public bool Equals(CachedExpressionKey other)
            {
                if (this.Hash != other.Hash
                    || this.Expression != other.Expression
                    || this.RichTextError != other.RichTextError
                    || this.DelegateType != other.DelegateType
                    || this.IsStatic != other.IsStatic
                    || this.Context != other.Context
                    || this.ReturnType != other.ReturnType
                    || (this.Parameters == null) != (other.Parameters == null)
                    || (this.Parameters != null && this.Parameters.Length != other.Parameters.Length)
                    || (this.ParameterNames == null) != (other.ParameterNames == null)
                    || (this.ParameterNames != null && this.ParameterNames.Length != other.ParameterNames.Length))
                {
                    return false;
                }

                if (this.Parameters != null)
                {
                    for (int i = 0; i < this.Parameters.Length; i++)
                    {
                        if (this.Parameters[i] != other.Parameters[i]) return false;
                    }
                }

                if (this.ParameterNames != null)
                {
                    for (int i = 0; i < this.ParameterNames.Length; i++)
                    {
                        if (this.ParameterNames[i] != other.ParameterNames[i]) return false;
                    }
                }

                return true;
            }

            public override int GetHashCode()
            {
                if (this.Hash.HasValue) return this.Hash.Value;

                unchecked
                {
                    int hash = 367074651;

                    hash = hash * -1521134295 + this.RichTextError.GetHashCode();
                    hash = hash * -1521134295 + this.Expression.GetHashCode();
                    hash = hash * -1521134295 + this.IsStatic.GetHashCode();
                    hash = hash * -1521134295 + this.Context.GetHashCode();

                    if (this.DelegateType != null)
                    {
                        hash = hash * -1521134295 + this.DelegateType.GetHashCode();
                    }

                    if (this.ReturnType != null)
                    {
                        hash = hash * -1521134295 + this.ReturnType.GetHashCode();
                    }

                    if (this.Parameters != null)
                    {
                        for (int i = 0; i < this.Parameters.Length; i++)
                        {
                            hash = hash * -1521134295 + this.Parameters[i].GetHashCode();
                        }
                    }

                    if (this.ParameterNames != null)
                    {
                        for (int i = 0; i < this.ParameterNames.Length; i++)
                        {
                            if (this.ParameterNames[i] == null)
                            {
                                hash = hash * -1521134295;
                            }
                            else
                            {
                                hash = hash * -1521134295 + this.ParameterNames[i].GetHashCode();
                            }
                        }
                    }

                    this.Hash = hash;
                    return hash;
                }
            }

            public class Comparer : IEqualityComparer<CachedExpressionKey>
            {
                public static readonly Comparer Instance = new Comparer();

                public bool Equals(CachedExpressionKey x, CachedExpressionKey y)
                {
                    return x.Equals(y);
                }

                public int GetHashCode(CachedExpressionKey obj)
                {
                    return obj.GetHashCode();
                }
            }
        }

        public class CachedExpression
        {
            public string Error;
            public Delegate Delegate;
            public DateTime LastAccessedTime;
        }

        static ExpressionUtility()
        {
            var thread = new Thread(ExpressionCleanupThread);
            thread.IsBackground = true;
            thread.Name = "Expression Cache Cleanup Thread";
            thread.Priority = System.Threading.ThreadPriority.Lowest;
            thread.Start();
        }

        private static void ExpressionCleanupThread()
        {
            List<CachedExpressionKey> toClear = new List<CachedExpressionKey>();

            while (true)
            {
                Thread.Sleep(5000);

                lock (ExpressionCache_LOCK)
                {
                    var now = DateTime.Now;

                    foreach (var entry in ExpressionCache)
                    {
                        if ((now - entry.Value.LastAccessedTime).TotalSeconds >= ExpressionCacheClearTimeSeconds)
                        {
                            toClear.Add(entry.Key);
                        }
                    }

                    foreach (var key in toClear)
                    {
                        ExpressionCache.Remove(key);
                    }
                }

                toClear.Clear();
            }
        }

        /// <summary>
        /// The time that the expression cache waits to clear expressions
        /// since the last time they have been used.
        /// </summary>
        public static float ExpressionCacheClearTimeSeconds = 30;

        private static readonly object ExpressionCache_LOCK = new object();
        private static readonly Dictionary<CachedExpressionKey, CachedExpression> ExpressionCache = new Dictionary<CachedExpressionKey, CachedExpression>(CachedExpressionKey.Comparer.Instance);

        private readonly static Tokenizer Tokenizer = new Tokenizer();
        private readonly static ASTParser Parser = new ASTParser(Tokenizer);
        private readonly static ASTEmitter Emitter = new ASTEmitter();
        private readonly static EmitContext Context = new EmitContext();
#endif

        public static string GetASTPrettyPrint(string expression)
        {
#if !ODIN_LIMITED_VERSION
            Tokenizer.SetExpressionString(expression);
            var ast = Parser.Parse();
            return ast.ToPrettyPrint();
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }

        public static bool TryParseTypeNameAsCSharpIdentifier(string typeString, out Type type)
        {
#if !ODIN_LIMITED_VERSION
            try
            {
                Tokenizer.SetExpressionString(typeString);
                var ast = Parser.Parse();

                Context.IsStatic = true;
                Context.Type = typeof(object);
                Context.ReturnType = null;
                Context.Parameters = null;
                Context.ParameterNames = null;

                Emitter.Context = Context;
                Emitter.Visit(ast);

                type = ast.NodeValue as Type;
                return type != null;
            }
            catch (Exception)
            {
                type = null;
                return false;
            }
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }

        /// <summary>Parses an expression and tries to emit a delegate method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
        public static Delegate ParseExpression(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.IsStatic = isStatic;
            Context.Type = contextType;
            Context.ReturnType = null;
            Context.Parameters = null;
            Context.ParameterNames = null;
            return ParseExpression(expression, Context, null, out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }

        /// <summary>Parses an expression and tries to emit a delegate method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="parameters">The parameters of the expression delegate.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
        public static Delegate ParseExpression(string expression, bool isStatic, Type contextType, Type[] parameters, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.IsStatic = isStatic;
            Context.Type = contextType;
            Context.ReturnType = null;
            Context.Parameters = parameters;
            Context.ParameterNames = null;
            return ParseExpression(expression, Context, null, out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }

        /// <summary>Parses an expression and tries to emit a delegate method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="parameters">The parameters of the expression delegate.</param>
        /// <param name="parameterNames">The names of the expression's parameters, for use with the named parameter syntax.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
        public static Delegate ParseExpression(string expression, bool isStatic, Type contextType, Type[] parameters, string[] parameterNames, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.IsStatic = isStatic;
            Context.Type = contextType;
            Context.ReturnType = null;
            Context.Parameters = parameters;
            Context.ParameterNames = parameterNames;
            return ParseExpression(expression, Context, null, out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }

        /// <summary>Parses an expression and tries to emit a delegate method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="context">The emit context.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
        public static Delegate ParseExpression(string expression, EmitContext context, out string errorMessage, bool richTextError = true)
        {
            return ParseExpression(expression, context, null, out errorMessage, richTextError);
        }

        /// <summary>Parses an expression and tries to emit a delegate of the specified type.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="context">The emit context.</param>
        /// <param name="delegateType">The type of the delegate to emit.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
        public static Delegate ParseExpression(string expression, EmitContext context, Type delegateType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            errorMessage = null;

            CachedExpression cachedExpression;
            var cachingKey = new CachedExpressionKey(expression, richTextError, delegateType, context);

            lock (ExpressionCache_LOCK)
            {
                if (!ExpressionCache.TryGetValue(cachingKey, out cachedExpression))
                {
                    cachedExpression = new CachedExpression();

                    try
                    {
                        Tokenizer.SetExpressionString(expression);
                        cachedExpression.Delegate = Emitter.EmitMethod("$Expression(" + expression + ")_" + Guid.NewGuid().ToString(), Parser.Parse(), context, delegateType);
                    }
                    catch (SyntaxException ex)
                    {
                        cachedExpression.Error = ex.GetNiceErrorMessage(expression, richTextError);
                        cachedExpression.Delegate = null;
                    }

                    // Take backups of arrays in case the arrays that were passed in are cached elsewhere and mutated later

                    if (cachingKey.Parameters != null)
                    {
                        var newParamsArr = new Type[cachingKey.Parameters.Length];

                        for (int i = 0; i < newParamsArr.Length; i++)
                        {
                            newParamsArr[i] = cachingKey.Parameters[i];
                        }

                        cachingKey.Parameters = newParamsArr;
                    }

                    if (cachingKey.ParameterNames != null)
                    {
                        var newParamNamesArr = new string[cachingKey.ParameterNames.Length];

                        for (int i = 0; i < newParamNamesArr.Length; i++)
                        {
                            newParamNamesArr[i] = cachingKey.ParameterNames[i];
                        }

                        cachingKey.ParameterNames = newParamNamesArr;
                    }

                    ExpressionCache.Add(cachingKey, cachedExpression);
                }

                cachedExpression.LastAccessedTime = DateTime.Now;
            }

            errorMessage = cachedExpression.Error;
            return cachedExpression.Delegate;
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }

        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<TResult> ParseFunc<TResult>(string expression, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = true;
            Context.Parameters = Type.EmptyTypes;
            Context.ParameterNames = null;
            return (ExpressionFunc<TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, TResult> ParseFunc<T1, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1) }
                : Type.EmptyTypes;
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, TResult> ParseFunc<T1, T2, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2) }
                : new Type[] { typeof(T2) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, T3, TResult> ParseFunc<T1, T2, T3, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3) }
                : new Type[] { typeof(T2), typeof(T3) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, T3, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, T3, T4, TResult> ParseFunc<T1, T2, T3, T4, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, T3, T4, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, T3, T4, T5, TResult> ParseFunc<T1, T2, T3, T4, T5, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, T3, T4, T5, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, T3, T4, T5, T6, TResult> ParseFunc<T1, T2, T3, T4, T5, T6, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, T3, T4, T5, T6, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, T6, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, TResult> ParseFunc<T1, T2, T3, T4, T5, T6, T7, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> ParseFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> ParseFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(TResult);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) };
            Context.ParameterNames = null;
            return (ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }

        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction ParseAction(string expression, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = true;
            Context.Parameters = Type.EmptyTypes;
            Context.ParameterNames = null;
            return (ExpressionAction)ParseExpression(expression, Context, typeof(ExpressionAction), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1> ParseAction<T1>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1) }
                : Type.EmptyTypes;
            Context.ParameterNames = null;
            return (ExpressionAction<T1>)ParseExpression(expression, Context, typeof(ExpressionAction<T1>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2> ParseAction<T1, T2>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2) }
                : new Type[] { typeof(T2) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2, T3> ParseAction<T1, T2, T3>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3) }
                : new Type[] { typeof(T2), typeof(T3) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2, T3>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2, T3, T4> ParseAction<T1, T2, T3, T4>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2, T3, T4>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2, T3, T4, T5> ParseAction<T1, T2, T3, T4, T5>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2, T3, T4, T5>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2, T3, T4, T5, T6> ParseAction<T1, T2, T3, T4, T5, T6>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2, T3, T4, T5, T6>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5, T6>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2, T3, T4, T5, T6, T7> ParseAction<T1, T2, T3, T4, T5, T6, T7>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2, T3, T4, T5, T6, T7>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5, T6, T7>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8> ParseAction<T1, T2, T3, T4, T5, T6, T7, T8>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
        /// <summary>Parses an expression and emits an ExpressionAction method.</summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
        /// <param name="contextType">The context type for the execution of the expression.</param>
        /// <param name="errorMessage">Output for any errors that may occur.</param>
        /// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
        /// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
        public static ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> ParseAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
        {
#if !ODIN_LIMITED_VERSION
            Context.Type = contextType;
            Context.ReturnType = typeof(void);
            Context.IsStatic = isStatic || contextType.IsStatic();
            Context.Parameters = Context.IsStatic
                ? new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) }
                : new Type[] { typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) };
            Context.ParameterNames = null;
            return (ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>), out errorMessage, richTextError);
#else
            throw new InvalidOperationException("Expressions are only available in Odin Pro.");
#endif
        }
    }
}
#endif