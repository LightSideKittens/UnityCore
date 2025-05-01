//-----------------------------------------------------------------------
// <copyright file="GenericParameterInferenceTypeMatcher.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;

    public class GenericParameterInferenceTypeMatcher : TypeMatcher
    {
        private static readonly Dictionary<Type, Type[]> GenericParameterConstraintsCache = new Dictionary<Type, Type[]>(FastTypeComparer.Instance);
        private static readonly Dictionary<Type, Type[]> GenericArgumentsCache = new Dictionary<Type, Type[]>(FastTypeComparer.Instance);

        private TypeSearchInfo info;
        private int genericParameterTargetCount;
        private bool[] targetIsGenericParameterCached;
        private Type[] typeArrayOfGenericParameterTargetCountSize;

        private Type[] matchTypeGenericArgs_Backup;
        private Type[] matchTypeGenericDefinitionArgs_Backup;

        private Type[] matchTypeGenericArgs;
        private Type[] matchTypeGenericDefinitionArgs;

        private static readonly object LOCK = new object();

        public override string Name { get { return "Generic Parameter Inference ---> Type<T1 [, T2] : Match<T1> [where T1 : constraints [, T2]]"; } }

        public override Type Match(Type[] targets, ref bool stopMatching)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (!targetIsGenericParameterCached[i] && info.Targets[i] != targets[i])
                {
                    // Everything but generic parameters must match exactly
                    return null;
                }
            }

            lock (LOCK)
            {
                Type[] inferTargets;

                if (genericParameterTargetCount != targets.Length)
                {
                    inferTargets = typeArrayOfGenericParameterTargetCountSize;

                    int count = 0;
                    for (int i = 0; i < info.Targets.Length; i++)
                    {
                        if (targetIsGenericParameterCached[i])
                        {
                            inferTargets[count++] = targets[i];
                        }
                    }
                }
                else
                {
                    inferTargets = targets;
                }

                Type[] inferredArgs;

                if (this.TryInferGenericParameters(info.MatchType, out inferredArgs, inferTargets))
                {
                    return info.MatchType.GetGenericTypeDefinition().MakeGenericType(inferredArgs);
                }

                return null;
            }
        }

        public class Creator : TypeMatcherCreator
        {
            public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
            {
                matcher = null;

                // Make sure we can apply generic parameter inference to the match info
                int genericParameterTargetCount = 0;
                if (!info.MatchType.IsGenericType) return false;

                for (int i = 0; i < info.Targets.Length; i++)
                {
                    if (info.Targets[i].IsGenericParameter)
                    {
                        genericParameterTargetCount++;
                    }
                }

                if (genericParameterTargetCount == 0) return false;

                var targetIsGenericParameterCached = new bool[info.Targets.Length];

                for (int i = 0; i < targetIsGenericParameterCached.Length; i++)
                {
                    targetIsGenericParameterCached[i] = info.Targets[i].IsGenericParameter;
                }

                var matchTypeGenericArgs = info.MatchType.GetGenericArguments();
                var matchTypeGenericDefinitionArgs = info.MatchType.GetGenericTypeDefinition().GetGenericArguments();

                matcher = new GenericParameterInferenceTypeMatcher()
                {
                    info = info,
                    genericParameterTargetCount = genericParameterTargetCount,
                    targetIsGenericParameterCached = targetIsGenericParameterCached,
                    typeArrayOfGenericParameterTargetCountSize = new Type[genericParameterTargetCount],
                    matchTypeGenericArgs = new Type[matchTypeGenericArgs.Length],
                    matchTypeGenericArgs_Backup = matchTypeGenericArgs,
                    matchTypeGenericDefinitionArgs = new Type[matchTypeGenericDefinitionArgs.Length],
                    matchTypeGenericDefinitionArgs_Backup = matchTypeGenericDefinitionArgs,
                };

                return true;
            }
        }

        private static readonly Dictionary<Type, Type> GenericConstraintsSatisfactionInferredParameters = new Dictionary<Type, Type>();
        private static readonly HashSet<Type> GenericConstraintsSatisfactionTypesToCheck = new HashSet<Type>();
        private static readonly List<Type> GenericConstraintsSatisfactionTypesToCheck_ToAdd = new List<Type>();

        private bool TryInferGenericParameters(Type genericTypeDefinition, out Type[] inferredParams, params Type[] knownParameters)
        {
            // NOTE: When modifying this method, also remember to modify Sirenix.Utilities.TypeExtensions.TryInferGenericParameters
            // and Sirenix.Serialization.TypeExtensions.TryInferGenericParameters!

            if (genericTypeDefinition == null)
            {
                throw new ArgumentNullException("genericTypeDefinition");
            }

            if (knownParameters == null)
            {
                throw new ArgumentNullException("knownParameters");
            }

            if (!genericTypeDefinition.IsGenericType)
            {
                throw new ArgumentException("The genericTypeDefinition parameter must be a generic type.");
            }

            for (int i = 0; i < this.matchTypeGenericArgs.Length; i++)
            {
                this.matchTypeGenericArgs[i] = this.matchTypeGenericArgs_Backup[i];
            }

            for (int i = 0; i < this.matchTypeGenericDefinitionArgs.Length; i++)
            {
                this.matchTypeGenericDefinitionArgs[i] = this.matchTypeGenericDefinitionArgs_Backup[i];

            }

            Dictionary<Type, Type> matches = GenericConstraintsSatisfactionInferredParameters;
            matches.Clear();

            HashSet<Type> typesToCheck = GenericConstraintsSatisfactionTypesToCheck;
            typesToCheck.Clear();

            List<Type> typesToCheck_ToAdd = GenericConstraintsSatisfactionTypesToCheck_ToAdd;
            typesToCheck_ToAdd.Clear();

            for (int i = 0; i < knownParameters.Length; i++)
            {
                typesToCheck.Add(knownParameters[i]);
            }

            Type[] definitions = this.matchTypeGenericArgs;

            if (!genericTypeDefinition.IsGenericTypeDefinition)
            {
                Type[] constructedParameters = definitions;
                genericTypeDefinition = genericTypeDefinition.GetGenericTypeDefinition();
                definitions = this.matchTypeGenericDefinitionArgs;

                int unknownCount = 0;

                for (int i = 0; i < constructedParameters.Length; i++)
                {
                    if (!constructedParameters[i].IsGenericParameter && (!constructedParameters[i].IsGenericType || constructedParameters[i].IsFullyConstructedGenericType()))
                    {
                        matches[definitions[i]] = constructedParameters[i];
                    }
                    else
                    {
                        unknownCount++;
                    }
                }

                if (unknownCount == knownParameters.Length)
                {
                    int count = 0;

                    for (int i = 0; i < constructedParameters.Length; i++)
                    {
                        if (constructedParameters[i].IsGenericParameter)
                        {
                            constructedParameters[i] = knownParameters[count++];
                        }
                    }

                    if (TypeExtensions.AreGenericConstraintsSatisfiedBy(this.matchTypeGenericDefinitionArgs_Backup, constructedParameters))
                    {
                        inferredParams = constructedParameters;
                        return true;
                    }
                }
            }

            if (definitions.Length == knownParameters.Length && TypeExtensions.AreGenericConstraintsSatisfiedBy(this.matchTypeGenericDefinitionArgs_Backup, knownParameters))
            {
                inferredParams = knownParameters;
                return true;
            }

            foreach (var typeArg in definitions)
            {
                var constraints = GetGenericParameterConstraintsCached(typeArg);

                foreach (var constraint in constraints)
                {
                    if (!constraint.IsGenericType)
                    {
                        continue;
                    }

                    Type constraintDefinition = constraint.GetGenericTypeDefinition();
                    var constraintParams = GetGenericArgumentsCached(constraint);

                    foreach (var parameter in typesToCheck)
                    {
                        Type[] paramParams;

                        if (parameter.IsGenericType && constraintDefinition == parameter.GetGenericTypeDefinition())
                        {
                            paramParams = GetGenericArgumentsCached(parameter);
                        }
                        else if (constraintDefinition.IsInterface && parameter.ImplementsOpenGenericInterface(constraintDefinition))
                        {
                            paramParams = parameter.GetArgumentsOfInheritedOpenGenericInterface(constraintDefinition);
                        }
                        else if (constraintDefinition.IsClass && parameter.ImplementsOpenGenericClass(constraintDefinition))
                        {
                            paramParams = parameter.GetArgumentsOfInheritedOpenGenericClass(constraintDefinition);
                        }
                        else
                        {
                            continue;
                        }

                        matches[typeArg] = parameter;
                        typesToCheck_ToAdd.Add(parameter);

                        for (int i = 0; i < constraintParams.Length; i++)
                        {
                            if (constraintParams[i].IsGenericParameter)
                            {
                                matches[constraintParams[i]] = paramParams[i];
                                typesToCheck_ToAdd.Add(paramParams[i]);
                            }
                        }
                    }

                    foreach (var type in typesToCheck_ToAdd)
                    {
                        typesToCheck.Add(type);
                    }

                    typesToCheck_ToAdd.Clear();
                }
            }

            if (matches.Count == definitions.Length)
            {
                inferredParams = new Type[matches.Count];

                for (int i = 0; i < definitions.Length; i++)
                {
                    inferredParams[i] = matches[definitions[i]];
                }

                if (TypeExtensions.AreGenericConstraintsSatisfiedBy(this.matchTypeGenericDefinitionArgs_Backup, inferredParams))
                {
                    return true;
                }
            }

            inferredParams = null;
            return false;
            
        }

        private static Type[] GetGenericParameterConstraintsCached(Type type)
        {
            Type[] result;
            if (!GenericParameterConstraintsCache.TryGetValue(type, out result))
            {
                result = type.GetGenericParameterConstraints();
                GenericParameterConstraintsCache.Add(type, result);
            }
            return result;
        }

        private static Type[] GetGenericArgumentsCached(Type type)
        {
            Type[] result;
            if (!GenericArgumentsCache.TryGetValue(type, out result))
            {
                result = type.GetGenericArguments();
                GenericArgumentsCache.Add(type, result);
            }
            return result;
        }
    }
}
#endif