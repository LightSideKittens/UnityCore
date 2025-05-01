//-----------------------------------------------------------------------
// <copyright file="MethodReferenceActionResolverCreator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.ActionResolvers.RegisterDefaultActionResolver(typeof(Sirenix.OdinInspector.Editor.ActionResolvers.MethodReferenceActionResolverCreator), 10)]

namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Reflection;
    
    public class MethodReferenceActionResolverCreator : ActionResolverCreator
    {
        public override string GetPossibleMatchesString(ref ActionResolverContext context)
        {
            return "Method References: \"MethodName\"";
        }

        public override ResolvedAction TryCreateAction(ref ActionResolverContext context)
        {
            if (string.IsNullOrEmpty(context.ResolvedString)) return null;

            var memberName = context.ResolvedString;

            // Optimization; if it is not a valid identifier there is no need to bother looking for it
            if (!TypeExtensions.IsValidIdentifier(memberName))
            {
                return null;
            }

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            bool isStatic = context.Property == context.Property.Tree.RootProperty && context.Property.Tree.IsStatic;

            if (!isStatic)
            {
                flags |= BindingFlags.Instance;
            }

            var contextType = context.ParentType;
            string errorMessage;
            NamedValues argSetup = default(NamedValues);

            MethodInfo method = GetCompatibleMethod(contextType, memberName, flags, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out errorMessage);

            if (errorMessage != null)
            {
                context.ErrorMessage = errorMessage;
                return FailedResolveAction;
            }

            if (method == null && !isStatic)
            {
                // We can go looking in base classes now
                Type current = contextType.BaseType;
                var newFlags = flags;

                newFlags &= ~BindingFlags.FlattenHierarchy;
                newFlags |= BindingFlags.DeclaredOnly;

                do
                {
                    method = GetCompatibleMethod(current, memberName, flags, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out errorMessage);

                    if (errorMessage != null)
                    {
                        context.ErrorMessage = errorMessage;
                        return FailedResolveAction;
                    }

                    if (method == null) current = current.BaseType;
                    else break;
                }
                while (current != null);
            }

            if (method != null)
            {
                return GetMethodInvoker(method as MethodInfo, argSetup, context.ParentType.IsValueType);
            }

            return null;
        }

        private static unsafe MethodInfo GetCompatibleMethod(Type type, string methodName, BindingFlags flags, ref NamedValues namedValues, ref NamedValues argSetup, bool requiresBackcasting, out string errorMessage)
        {
            MethodInfo method;

            try
            {
                method = type.GetMethod(methodName, flags);
            }
            catch (AmbiguousMatchException)
            {
                errorMessage = "Could not find exact method named '" + methodName + "' because there are several methods with that name defined, and so it is an ambiguous match.";
                return null;
            }

            if (method == null)
            {
                errorMessage = null;
                return null;
            }

            if (!IsCompatibleMethod(method, ref namedValues, ref argSetup, requiresBackcasting, out errorMessage))
            {
                return null;
            }

            return method;
        }
    }
}
#endif