//-----------------------------------------------------------------------
// <copyright file="MethodPropertyValueResolverCreator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Utilities;
using System.Reflection;

[assembly: Sirenix.OdinInspector.Editor.ValueResolvers.RegisterDefaultValueResolverCreator(typeof(Sirenix.OdinInspector.Editor.ValueResolvers.MethodPropertyValueResolverCreator), 20)]

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
#pragma warning disable

    public class MethodPropertyValueResolverCreator : BaseMemberValueResolverCreator
    {
        public override string GetPossibleMatchesString(ref ValueResolverContext context)
        {
            return null;
        }

        public override ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context)
        {
            var prop = context.Property;

            if (string.IsNullOrEmpty(context.ResolvedString) && prop.Info.PropertyType == PropertyType.Method)
            {
                MethodInfo method = (prop.Info.GetMemberInfo() as MethodInfo) ?? prop.Info.GetMethodDelegate().Method;

                if (method.IsGenericMethodDefinition)
                {
                    context.ErrorMessage = "Cannot invoke a generic method definition such as '" + method.GetNiceName() + "'.";
                    return GetFailedResolverFunc<TResult>();
                }

                var containedType = method.ReturnType;

                if (containedType == typeof(void) || !ConvertUtility.CanConvert(containedType, typeof(TResult)))
                {
                    return null;
                }

                NamedValues argSetup = default(NamedValues);

                if (IsCompatibleMethod(method, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out context.ErrorMessage))
                {
                    if (prop.Info.GetMethodDelegate() != null)
                    {
                        return GetDelegateGetter<TResult>(prop.Info.GetMethodDelegate(), argSetup);
                    }
                    else
                    {
                        return GetMethodGetter<TResult>(method, argSetup, prop.ParentType.IsValueType);
                    }
                }
                else if (context.ErrorMessage != null)
                {
                    return GetFailedResolverFunc<TResult>();
                }
            }

            return null;
        }
    }
}
#endif