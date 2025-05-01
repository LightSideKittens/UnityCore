//-----------------------------------------------------------------------
// <copyright file="ExactTypeMatcher.cs" company="Sirenix ApS">
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

    using System;

    public class ExactTypeMatcher : TypeMatcher
    {
        private TypeSearchInfo info;

        public override string Name { get { return "Exact Match --> Type : Match[<Target>]"; } }

        public override Type Match(Type[] targets, ref bool stopMatching)
        {
            if (targets.Length != this.info.Targets.Length) return null;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != this.info.Targets[i]) return null;
            }

            return this.info.MatchType;
        }

        public class Creator : TypeMatcherCreator
        {
            public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
            {
                matcher = null;

                if (info.MatchType.IsGenericTypeDefinition) return false;

                matcher = new ExactTypeMatcher()
                {
                    info = info
                };

                return true;
            }
        }
    }
}
#endif