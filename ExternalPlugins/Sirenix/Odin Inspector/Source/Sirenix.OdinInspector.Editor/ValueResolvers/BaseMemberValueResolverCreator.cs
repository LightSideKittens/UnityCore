//-----------------------------------------------------------------------
// <copyright file="BaseMemberValueResolverCreator.cs" company="Sirenix ApS">
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
using System;
using System.Reflection;

[assembly: Sirenix.OdinInspector.Editor.ValueResolvers.RegisterDefaultValueResolverCreator(typeof(Sirenix.OdinInspector.Editor.ValueResolvers.MethodPropertyValueResolverCreator), 20)]

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
#pragma warning disable

    public abstract class BaseMemberValueResolverCreator : ValueResolverCreator
    {
        protected static ValueResolverFunc<TResult> GetFieldGetter<TResult>(FieldInfo field)
        {
            if (field.IsStatic)
            {
                return (ref ValueResolverContext context, int selectionIndex) => ConvertUtility.Convert<TResult>(field.GetValue(null));
            }
            else
            {
                return (ref ValueResolverContext context, int selectionIndex) => ConvertUtility.Convert<TResult>(field.GetValue(context.GetParentValue(selectionIndex)));
            }
        }

        protected static ValueResolverFunc<TResult> GetPropertyGetter<TResult>(PropertyInfo property, bool parentIsValueType)
        {
            if (property.IsStatic())
            {
                return (ref ValueResolverContext context, int selectionIndex) => ConvertUtility.Convert<TResult>(property.GetValue(null, null));
            }
            else
            {
                return (ref ValueResolverContext context, int selectionIndex) =>
                {
                    var instance = context.GetParentValue(selectionIndex);
                    var result = ConvertUtility.Convert<TResult>(property.GetValue(instance, null));

                    if (parentIsValueType)
                    {
                        context.SetParentValue(selectionIndex, instance);
                    }

                    return result;
                };
            }
        }

        protected static ValueResolverFunc<TResult> GetMethodGetter<TResult>(MethodInfo method, NamedValues argSetup, bool parentIsValueType)
        {
            object[] parameterValues = new object[argSetup.Count];
            bool isStatic = method.IsStatic;

            ParameterInfo[] parameters = method.GetParameters();
            bool[] byRefParameters = new bool[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                byRefParameters[i] = parameters[i].ParameterType.IsByRef;
            }

            return (ref ValueResolverContext context, int selectionIndex) =>
            {
                for (int i = 0; i < parameterValues.Length; i++)
                {
                    object value = context.NamedValues.GetValue(argSetup[i].Name);
                    parameterValues[i] = ConvertUtility.WeakConvert(value, argSetup[i].Type);
                }

                object instance = isStatic ? null : context.GetParentValue(selectionIndex);
                var result = ConvertUtility.Convert<TResult>(method.Invoke(instance, parameterValues));

                if (!isStatic && parentIsValueType)
                {
                    context.SetParentValue(selectionIndex, instance);
                }

                if (context.SyncRefParametersWithNamedValues)
                {
                    for (int i = 0; i < parameterValues.Length; i++)
                    {
                        if (!byRefParameters[i]) continue;

                        NamedValue value;
                        var argInfo = argSetup[i];

                        if (!context.NamedValues.TryGetValue(argInfo.Name, out value))
                        {
                            throw new Exception("Expected named value '" + argInfo.Name + "' was not present!");
                        }

                        context.NamedValues.Set(argInfo.Name, ConvertUtility.WeakConvert(parameterValues[i], value.Type));
                    }
                }

                return result;
            };
        }

        protected static ValueResolverFunc<TResult> GetDelegateGetter<TResult>(Delegate @delegate, NamedValues argSetup)
        {
            object[] parameterValues = new object[argSetup.Count];

            ParameterInfo[] parameters = @delegate.Method.GetParameters();
            bool[] byRefParameters = new bool[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                byRefParameters[i] = parameters[i].ParameterType.IsByRef;
            }

            return (ref ValueResolverContext context, int selectionIndex) =>
            {
                for (int i = 0; i < parameterValues.Length; i++)
                {
                    object value = context.NamedValues.GetValue(argSetup[i].Name);
                    parameterValues[i] = ConvertUtility.WeakConvert(value, argSetup[i].Type);
                }

                TResult result = ConvertUtility.Convert<TResult>(@delegate.DynamicInvoke(parameterValues));

                if (context.SyncRefParametersWithNamedValues)
                {
                    for (int i = 0; i < parameterValues.Length; i++)
                    {
                        if (!byRefParameters[i]) continue;

                        NamedValue value;
                        var argInfo = argSetup[i];

                        if (!context.NamedValues.TryGetValue(argInfo.Name, out value))
                        {
                            throw new Exception("Expected named value '" + argInfo.Name + "' was not present!");
                        }

                        context.NamedValues.Set(argInfo.Name, ConvertUtility.WeakConvert(parameterValues[i], value.Type));
                    }
                }

                return result;
            };
        }

        protected static MethodInfo GetCompatibleMethod(Type type, string methodName, BindingFlags flags, ref NamedValues namedValues, ref NamedValues argSetup, bool requiresBackcasting, out string errorMessage)
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

            if (IsCompatibleMethod(method, ref namedValues, ref argSetup, requiresBackcasting, out errorMessage))
            {
                return method;
            }
            else
            {
                return null;
            }
        }

        protected static unsafe bool IsCompatibleMethod(MethodInfo method, ref NamedValues namedValues, ref NamedValues argSetup, bool requiresBackcasting, out string errorMessage)
        {
            var parameters = method.GetParameters();
            var namedValueCount = namedValues.Count;

            if (parameters.Length > namedValueCount)
            {
                errorMessage = "Method '" + method.GetNiceName() + "' has too many parameters (" + parameters.Length + "). The following '" + namedValueCount + "' parameters are available: \n\n" + namedValues.GetValueOverviewString();
                return false;
            }

            bool* claimedNamedValues = stackalloc bool[namedValueCount];

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var parameterName = parameter.Name;
                var parameterType = parameter.ParameterType;

                bool isByRef = false;

                if (parameterType.IsByRef)
                {
                    isByRef = true;
                    parameterType = parameterType.GetElementType();
                }

                bool foundMatch = false;

                // First look for named args
                for (int j = 0; j < namedValueCount; j++)
                {
                    if (*(claimedNamedValues + j)) continue; // Value has been claimed by another parameter

                    var value = namedValues[j];

                    if (value.Name == parameterName)
                    {
                        if (!ConvertUtility.CanConvert(value.Type, parameterType))
                        {
                            errorMessage = "Method '" + method.Name + "' has an invalid signature; the parameter '" + parameterName + "' of type '" + parameter.ParameterType.GetNiceName() + "' cannot be assigned from the available type '" + value.Type.GetNiceName() + "'. The following parameters are available: \n\n" + namedValues.GetValueOverviewString();
                            return false;
                        }

                        if (requiresBackcasting && isByRef && !ConvertUtility.CanConvert(parameterType, value.Type))
                        {
                            errorMessage = "Method '" + method.Name + "' has an invalid signature; the ref/in/out parameter '" + parameterName + "' of type '" + parameter.ParameterType.GetNiceName() + "' can be assigned from the available type '" + value.Type.GetNiceName() + "', but type '" + value.Type.GetNiceName() + "' cannot be reassigned assigned back to type '" + parameter.ParameterType.GetNiceName() + "'. The following parameters are available: \n\n" + namedValues.GetValueOverviewString();
                            return false;
                        }

                        argSetup.Add(value.Name, parameterType, null);
                        *(claimedNamedValues + j) = true;
                        foundMatch = true;
                        break;
                    }
                }

                if (!foundMatch)
                {
                    // Then try to match by exact type
                    for (int j = 0; j < namedValueCount; j++)
                    {
                        if (*(claimedNamedValues + j)) continue; // Value has been claimed by another parameter

                        var value = namedValues[j];

                        if (value.Type == parameterType)
                        {
                            foundMatch = true;
                            argSetup.Add(value.Name, parameterType, null);
                            *(claimedNamedValues + j) = true;
                            break;
                        }
                    }
                }

                if (!foundMatch && parameterType != typeof(string))
                {
                    // Then try to match by convertable type
                    // All things convert to strings (via ToString()), so we don't allow that; otherwise, every value matches on string parameters, and that's no good
                    for (int j = 0; j < namedValueCount; j++)
                    {
                        if (*(claimedNamedValues + j)) continue; // Value has been claimed by another parameter

                        var value = namedValues[j];

                        if (ConvertUtility.CanConvert(value.Type, parameterType))
                        {
                            if (!requiresBackcasting || ConvertUtility.CanConvert(parameterType, value.Type))
                            {
                                foundMatch = true;
                                argSetup.Add(value.Name, parameterType, null);
                                *(claimedNamedValues + j) = true;
                                break;
                            }
                        }
                    }
                }

                if (!foundMatch)
                {
                    errorMessage = "Method '" + method.Name + "' has an invalid signature; no values could be assigned to the parameter '" + parameterName + "' of type '" + parameter.ParameterType.GetNiceName() + "'. The following parameter values are available: \n\n" + namedValues.GetValueOverviewString();
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }
    }
}
#endif