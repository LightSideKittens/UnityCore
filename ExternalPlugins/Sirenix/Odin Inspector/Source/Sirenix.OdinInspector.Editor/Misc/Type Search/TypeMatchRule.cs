//-----------------------------------------------------------------------
// <copyright file="TypeMatchRule.cs" company="Sirenix ApS">
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

    public class TypeMatchRule
    {
        public delegate Type TypeMatchRuleDelegate1(TypeSearchInfo info, Type[] targets);

        public delegate Type TypeMatchRuleDelegate2(TypeSearchInfo info, Type[] targets, ref bool stopMatchingForInfo);

        public readonly string Name;

        private TypeMatchRuleDelegate1 rule1;
        private TypeMatchRuleDelegate2 rule2;

        public TypeMatchRule(string name, TypeMatchRuleDelegate1 rule)
        {
            this.Name = name;
            this.rule1 = rule;
        }

        public TypeMatchRule(string name, TypeMatchRuleDelegate2 rule)
        {
            this.Name = name;
            this.rule2 = rule;
        }

        public Type Match(TypeSearchInfo matchInfo, Type[] targets, ref bool stopMatchingForInfo)
        {
            if (this.rule1 != null)
            {
                return this.rule1(matchInfo, targets);
            }
            else
            {
                return this.rule2(matchInfo, targets, ref stopMatchingForInfo);
            }
        }

        public override string ToString()
        {
            return "TypeMatchRule: " + this.Name;
        }
    }
}
#endif