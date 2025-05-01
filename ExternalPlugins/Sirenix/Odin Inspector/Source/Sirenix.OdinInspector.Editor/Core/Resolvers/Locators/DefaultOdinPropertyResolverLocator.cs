//-----------------------------------------------------------------------
// <copyright file="DefaultOdinPropertyResolverLocator.cs" company="Sirenix ApS">
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

    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Sirenix.OdinInspector.Editor.TypeSearch;
    using Sirenix.Utilities;
    using UnityEditor;

    /// <summary>
    /// Default implementation and the version that will be used by <see cref="PropertyTree"/> if no other <see cref="OdinPropertyResolver"/> instance have been specified.
    /// </summary>
    public class DefaultOdinPropertyResolverLocator : OdinPropertyResolverLocator
    {
        /// <summary>
        /// Singleton instance of <see cref="DefaultOdinPropertyResolverLocator"/>.
        /// </summary>
        public static readonly DefaultOdinPropertyResolverLocator Instance = new DefaultOdinPropertyResolverLocator();

        public static readonly TypeSearchIndex SearchIndex = new TypeSearchIndex();

        private static Dictionary<Type, OdinPropertyResolver> resolverEmptyInstanceMap = new Dictionary<Type, OdinPropertyResolver>(FastTypeComparer.Instance);
        private static readonly List<TypeSearchResult[]> QueryResultsList = new List<TypeSearchResult[]>();
        private static readonly List<TypeSearchResult> MergedSearchResultsList = new List<TypeSearchResult>();

        static DefaultOdinPropertyResolverLocator()
        {
            using (SimpleProfiler.Section("DefaultOdinPropertyResolverLocator Type Cache"))
            {
                var types = TypeCache.GetTypesDerivedFrom(typeof(OdinPropertyResolver));

                foreach (var type in types)
                {
                    if (type.IsAbstract || type.IsDefined<OdinDontRegisterAttribute>())
                        continue;

                    IndexType(type);
                }
            }

            //SearchIndex.AddIndexedTypes(ResolverUtilities.GetResolverAssemblies()
            //    .SelectMany(a => a.SafeGetTypes())
            //    .Where(type => type.IsAbstract == false
            //        && typeof(OdinPropertyResolver).IsAssignableFrom(type)
            //        && !type.IsDefined<OdinDontRegisterAttribute>())
            //    .Select(type =>
            //    {
            //        var result = new TypeSearchInfo()
            //        {
            //            MatchType = type,
            //            Priority = ResolverUtilities.GetResolverPriority(type),
            //        };

            //        if (type.ImplementsOpenGenericType(typeof(OdinPropertyResolver<>)))
            //        {
            //            if (type.ImplementsOpenGenericType(typeof(OdinPropertyResolver<,>)))
            //            {
            //                result.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(OdinPropertyResolver<,>));
            //            }
            //            else
            //            {
            //                result.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(OdinPropertyResolver<>));
            //            }
            //        }
            //        else
            //        {
            //            result.Targets = Type.EmptyTypes;
            //        }

            //        return result;
            //    }));
        }

        private static void IndexType(Type type)
        {
            var result = new TypeSearchInfo()
            {
                MatchType = type,
                Priority = ResolverUtilities.GetResolverPriority(type),
            };

            if (type.ImplementsOpenGenericType(typeof(OdinPropertyResolver<>)))
            {
                if (type.ImplementsOpenGenericType(typeof(OdinPropertyResolver<,>)))
                {
                    result.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(OdinPropertyResolver<,>));
                    result.TargetCategories = TypeSearchIndex.ValueAttributeMatchCategoryArray;
                }
                else
                {
                    result.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(OdinPropertyResolver<>));
                    result.TargetCategories = TypeSearchIndex.ValueMatchCategoryArray;
                }
            }
            else
            {
                result.Targets = Type.EmptyTypes;
                result.TargetCategories = TypeSearchIndex.EmptyCategoryArray;
            }

            SearchIndex.AddIndexedType(result);
        }

        /// <summary>
        /// Gets an <see cref="OdinPropertyResolver"/> instance for the specified property.
        /// </summary>
        /// <param name="property">The property to get an <see cref="OdinPropertyResolver"/> instance for.</param>
        /// <returns>An instance of <see cref="OdinPropertyResolver"/> to resolver the specified property.</returns>
        public override OdinPropertyResolver GetResolver(InspectorProperty property)
        {
            if (property.Tree.IsStatic && property == property.Tree.RootProperty)
            {
                return OdinPropertyResolver.Create(typeof(StaticRootPropertyResolver<>).MakeGenericType(property.ValueEntry.TypeOfValue), property);
            }

            var queries = QueryResultsList;
            queries.Clear();

            queries.Add(SearchIndex.GetMatches(Type.EmptyTypes, TypeSearchIndex.EmptyCategoryArray));

            Type typeOfValue = property.ValueEntry != null ? property.ValueEntry.TypeOfValue : null;

            if (typeOfValue != null)
            {
                queries.Add(SearchIndex.GetMatches(typeOfValue, TargetMatchCategory.Value));

                for (int i = 0; i < property.Attributes.Count; i++)
                {
                    queries.Add(SearchIndex.GetMatches(typeOfValue, property.Attributes[i].GetType(), TargetMatchCategory.Value, TargetMatchCategory.Attribute));
                }
            }

            TypeSearchIndex.MergeQueryResultsIntoList(queries, MergedSearchResultsList);

            for (int i = 0; i < MergedSearchResultsList.Count; i++)
            {
                var info = MergedSearchResultsList[i];

                if (GetEmptyResolverInstance(info.MatchedType).CanResolveForPropertyFilter(property))
                {
                    return OdinPropertyResolver.Create(info.MatchedType, property);
                }
            }

            return OdinPropertyResolver.Create<EmptyPropertyResolver>(property);
        }

        public OdinPropertyResolver GetEmptyResolverInstance(Type resolverType)
        {
            OdinPropertyResolver result;
            if (!resolverEmptyInstanceMap.TryGetValue(resolverType, out result))
            {
                result = (OdinPropertyResolver)FormatterServices.GetUninitializedObject(resolverType);
                resolverEmptyInstanceMap[resolverType] = result;
            }
            return result;
        }
    }
}
#endif