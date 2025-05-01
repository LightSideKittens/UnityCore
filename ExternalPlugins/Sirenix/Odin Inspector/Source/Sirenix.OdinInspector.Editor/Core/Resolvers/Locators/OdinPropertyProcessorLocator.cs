//-----------------------------------------------------------------------
// <copyright file="OdinPropertyProcessorLocator.cs" company="Sirenix ApS">
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

    using Sirenix.OdinInspector.Editor.TypeSearch;
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using UnityEditor;

    public static class OdinPropertyProcessorLocator
    {
        private static readonly Dictionary<Type, OdinPropertyProcessor> EmptyInstances = new Dictionary<Type, OdinPropertyProcessor>(FastTypeComparer.Instance);
        public static readonly TypeSearchIndex SearchIndex = new TypeSearchIndex() { MatchedTypeLogName = "member property processor" };
        private static readonly List<TypeSearchResult[]> CachedQueryList = new List<TypeSearchResult[]>();

        static OdinPropertyProcessorLocator()
        {
            using (SimpleProfiler.Section("OdinPropertyProcessorLocator Type Cache"))
            {
                var types = TypeCache.GetTypesDerivedFrom(typeof(OdinPropertyProcessor));

                foreach (var type in types)
                {
                    if (type.IsAbstract || type.IsDefined<OdinDontRegisterAttribute>(false))
                        continue;

                    IndexType(type);
                }
            }
        }

        private static void IndexType(Type type)
        {
            if (type.ImplementsOpenGenericClass(typeof(OdinPropertyProcessor<>)))
            {
                if (type.ImplementsOpenGenericClass(typeof(OdinPropertyProcessor<,>)))
                {
                    // Value/attribute targeted resolver
                    SearchIndex.AddIndexedType(new TypeSearchInfo()
                    {
                        MatchType = type,
                        Targets = type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinPropertyProcessor<,>)),
                        TargetCategories = TypeSearchIndex.ValueAttributeMatchCategoryArray,
                        Priority = ResolverUtilities.GetResolverPriority(type)
                    });
                }
                else
                {
                    // Value targeted resolver
                    SearchIndex.AddIndexedType(new TypeSearchInfo()
                    {
                        MatchType = type,
                        Targets = type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinPropertyProcessor<>)),
                        TargetCategories = TypeSearchIndex.ValueMatchCategoryArray,
                        Priority = ResolverUtilities.GetResolverPriority(type)
                    });
                }
            }
            else
            {
                // No target constraints resolver (only CanResolveForProperty)
                SearchIndex.AddIndexedType(new TypeSearchInfo()
                {
                    MatchType = type,
                    Targets = Type.EmptyTypes,
                    TargetCategories = TypeSearchIndex.EmptyCategoryArray,
                    Priority = ResolverUtilities.GetResolverPriority(type)
                });
            }
        }

        public static List<OdinPropertyProcessor> GetMemberProcessors(InspectorProperty property)
        {
            var queries = CachedQueryList;
            queries.Clear();

            //var results = CachedResultList;
            //results.Clear();

            queries.Add(SearchIndex.GetMatches(Type.EmptyTypes, TypeSearchIndex.EmptyCategoryArray));

            if (property.ValueEntry != null)
            {
                var valueType = property.ValueEntry.TypeOfValue;

                queries.Add(SearchIndex.GetMatches(valueType, TargetMatchCategory.Value));

                for (int i = 0; i < property.Attributes.Count; i++)
                {
                    queries.Add(SearchIndex.GetMatches(valueType, property.Attributes[i].GetType(), TargetMatchCategory.Value, TargetMatchCategory.Attribute));
                }
            }

            var results = TypeSearchIndex.GetCachedMergedQueryResults(queries);

            List<OdinPropertyProcessor> processors = new List<OdinPropertyProcessor>();

            for (int i = 0; i < results.Length; i++)
            {
                var result = results[i];
                if (GetEmptyInstance(result.MatchedType).CanProcessForProperty(property))
                {
                    processors.Add(OdinPropertyProcessor.Create(result.MatchedType, property));
                }
            }

            return processors;
        }

        private static OdinPropertyProcessor GetEmptyInstance(Type type)
        {
            OdinPropertyProcessor result;
            if (!EmptyInstances.TryGetValue(type, out result))
            {
                result = (OdinPropertyProcessor)FormatterServices.GetUninitializedObject(type);
                EmptyInstances[type] = result;
            }
            return result;
        }
    }
}
#endif