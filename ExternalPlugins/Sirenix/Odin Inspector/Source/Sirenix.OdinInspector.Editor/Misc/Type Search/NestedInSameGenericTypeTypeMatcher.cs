//-----------------------------------------------------------------------
// <copyright file="NestedInSameGenericTypeTypeMatcher.cs" company="Sirenix ApS">
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

    public class NestedInSameGenericTypeTypeMatcher : TypeMatcher
    {
        private TypeSearchInfo info;
        private Type matchTypeGenericDefinition;
        private Type infoTargetGenericDefinition;

        public override string Name { get { return "Nested In Same Generic Type ---> Type<T1, [, T2]>.NestedType : Type<T1, [, T2]>.Match<Target>"; } }

        public override Type Match(Type[] targets, ref bool stopMatching)
        {
            if (targets.Length != 1) return null;

            var target = targets[0];

            if (!target.IsNested) return null;

            if (!target.DeclaringType.IsGenericType)
            {
                return null;
            }

            if (matchTypeGenericDefinition != target.DeclaringType.GetGenericTypeDefinition()) return null;
            if (infoTargetGenericDefinition != target.GetGenericTypeDefinition()) return null;

            var args = target.GetGenericArguments();

            if (info.MatchType.AreGenericConstraintsSatisfiedBy(args))
            {
                return info.MatchType.MakeGenericType(args);
            }

            return null;
        }

        public class Creator : TypeMatcherCreator
        {
            public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
            {
                matcher = null;

                if (info.Targets.Length == 0 || !info.MatchType.IsNested || !info.Targets[0].IsNested) return false;

                if (!info.MatchType.DeclaringType.IsGenericType ||
                    !info.Targets[0].DeclaringType.IsGenericType)
                {
                    return false;
                }

                var matchTypeGenericDefinition = info.MatchType.DeclaringType.GetGenericTypeDefinition();

                if (matchTypeGenericDefinition != info.Targets[0].DeclaringType.GetGenericTypeDefinition()) return false;

                matcher = new NestedInSameGenericTypeTypeMatcher()
                {
                    info = info,
                    matchTypeGenericDefinition = matchTypeGenericDefinition,
                    infoTargetGenericDefinition = info.Targets[0].GetGenericTypeDefinition(),
                };

                return true;
            }
        }
    }
}
#endif