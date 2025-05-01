//-----------------------------------------------------------------------
// <copyright file="DefaultStateUpdaterLocator.cs" company="Sirenix ApS">
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
    using System.Reflection.Emit;
    using System.Runtime.Serialization;

    public class DefaultStateUpdaterLocator : StateUpdaterLocator
    {
        public static readonly DefaultStateUpdaterLocator Instance = new DefaultStateUpdaterLocator();
        public static readonly TypeSearchIndex SearchIndex = new TypeSearchIndex() { MatchedTypeLogName = "state updater" };

        private static readonly Dictionary<Type, Func<StateUpdater>> FastCreators = new Dictionary<Type, Func<StateUpdater>>(FastTypeComparer.Instance);
        private static readonly Dictionary<Type, StateUpdater> EmptyInstances = new Dictionary<Type, StateUpdater>(FastTypeComparer.Instance);

        private static readonly StateUpdater[] EmptyResult = new StateUpdater[0];

        private static TypeSearchResult[][] CachedQueryResultArray = new TypeSearchResult[32][];
        private static StateUpdater[] CachedResultBuilderArray = new StateUpdater[16];

        static DefaultStateUpdaterLocator()
        {
            var assemblies = ResolverUtilities.GetResolverAssemblies();

            for (int i = 0; i < assemblies.Count; i++)
            {
                object[] attributes;

                try
                {
                    attributes = assemblies[i].SafeGetCustomAttributes(typeof(RegisterStateUpdaterAttribute), false);
                }
                catch { continue; }

                for (int j = 0; j < attributes.Length; j++)
                {
                    var attribute = (RegisterStateUpdaterAttribute)attributes[j];

                    if (!attribute.Type.IsAbstract && typeof(StateUpdater).IsAssignableFrom(attribute.Type))
                    {
                        IndexType(attribute.Type, attribute.Priority);
                    }
                }
            }
        }

        private static void IndexType(Type type, double priority)
        {
            var result = new TypeSearchInfo()
            {
                MatchType = type,
                Priority = priority,
            };

            if (type.ImplementsOpenGenericType(typeof(AttributeStateUpdater<>)))
            {
                if (type.ImplementsOpenGenericType(typeof(AttributeStateUpdater<,>)))
                {
                    result.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(AttributeStateUpdater<,>));
                    result.TargetCategories = TypeSearchIndex.AttributeValueMatchCategoryArray;
                }
                else
                {
                    result.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(AttributeStateUpdater<>));
                    result.TargetCategories = TypeSearchIndex.AttributeMatchCategoryArray;
                }
            }
            else if (type.ImplementsOpenGenericType(typeof(ValueStateUpdater<>)))
            {
                result.Targets = type.GetArgumentsOfInheritedOpenGenericType(typeof(ValueStateUpdater<>));
                result.TargetCategories = TypeSearchIndex.ValueMatchCategoryArray;
            }
            else
            {
                result.Targets = Type.EmptyTypes;
                result.TargetCategories = TypeSearchIndex.EmptyCategoryArray;
            }

            SearchIndex.AddIndexedType(result);
        }

        public override StateUpdater[] GetStateUpdaters(InspectorProperty property)
        {
            int queryCount = 0;
            CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(Type.EmptyTypes, TypeSearchIndex.EmptyCategoryArray);

            var valueEntry = property.ValueEntry;

            // Value query
            if (valueEntry != null)
            {
                CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(valueEntry.TypeOfValue, TargetMatchCategory.Value);
            }

            int maxNeededSize = 2 + property.Attributes.Count * 2;
            while (CachedQueryResultArray.Length <= maxNeededSize) ExpandArray(ref CachedQueryResultArray);

            // Attribute queries
            for (int i = 0; i < property.Attributes.Count; i++)
            {
                var attr = property.Attributes[i].GetType();
                CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(attr, TargetMatchCategory.Attribute);

                // Attribute and value query
                if (valueEntry != null)
                {
                    CachedQueryResultArray[queryCount++] = SearchIndex.GetMatches(attr, valueEntry.TypeOfValue, TargetMatchCategory.Attribute, TargetMatchCategory.Value);
                }
            }

            var finalResults = TypeSearchIndex.GetCachedMergedQueryResults(CachedQueryResultArray, queryCount);
            int resultCount = 0;
            
            while (CachedResultBuilderArray.Length < finalResults.Length) ExpandArray(ref CachedResultBuilderArray);

            // Build up the final result list, filtering invalid types away as we go.
            for (int i = 0; i < finalResults.Length; i++)
            {
                var result = finalResults[i];

                if (GetEmptyUpdaterInstance(result.MatchedType).CanUpdateProperty(property))
                {
                    CachedResultBuilderArray[resultCount++] = CreateStateUpdater(result.MatchedType);
                }
            }

            if (resultCount == 0) return EmptyResult;

            // Populate final result array
            var finalResult = new StateUpdater[resultCount];

            for (int i = 0; i < resultCount; i++)
            {
                finalResult[i] = CachedResultBuilderArray[i];
                CachedResultBuilderArray[i] = null;
            }

            return finalResult;
        }

        public StateUpdater GetEmptyUpdaterInstance(Type type)
        {
            StateUpdater result;
            if (!EmptyInstances.TryGetValue(type, out result))
            {
                result = (StateUpdater)FormatterServices.GetUninitializedObject(type);
                EmptyInstances[type] = result;
            }
            return result;
        }

        private static void ExpandArray<T>(ref T[] array)
        {
            var newArray = new T[array.Length * 2];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }
            array = newArray;
        }

        private static StateUpdater CreateStateUpdater(Type type)
        {
            Func<StateUpdater> fastCreator;

            if (!FastCreators.TryGetValue(type, out fastCreator))
            {
                var constructor = type.GetConstructor(Type.EmptyTypes);
                var method = new DynamicMethod("FastCreator", typeof(StateUpdater), Type.EmptyTypes);

                var il = method.GetILGenerator();

                il.Emit(OpCodes.Newobj, constructor);
                il.Emit(OpCodes.Ret);

                fastCreator = (Func<StateUpdater>)method.CreateDelegate(typeof(Func<StateUpdater>));
                FastCreators.Add(type, fastCreator);
            }

            return fastCreator();
        }
    }

}
#endif