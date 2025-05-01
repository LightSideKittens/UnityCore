//-----------------------------------------------------------------------
// <copyright file="TypeMatchIndexingRule.cs" company="Sirenix ApS">
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

    public class TypeMatchIndexingRule
    {
        public delegate bool TypeMatchIndexingRuleDelegate(ref TypeSearchInfo info, ref string errorMessage);

        public readonly string Name;
        private TypeMatchIndexingRuleDelegate rule;

        public TypeMatchIndexingRule(string name, TypeMatchIndexingRuleDelegate rule)
        {
            this.Name = name;
            this.rule = rule;
        }

        public bool Process(ref TypeSearchInfo info, ref string errorMessage)
        {
            return this.rule(ref info, ref errorMessage);
        }

        public override string ToString()
        {
            return "TypeMatchIndexingRule: " + this.Name;
        }
    }
}
#endif