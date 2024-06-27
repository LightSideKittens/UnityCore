using System;
using LSCore.Attributes;
using LSCore.ConditionModule;
using Sirenix.OdinInspector;

namespace LSCore
{
    [Serializable]
    [HideReferenceObjectPicker]
    [TypeFrom]
    public abstract class WindowDataCondition : Condition
    {
        public bool value;
        protected abstract bool Ref { get; }
        protected override bool Check() => Ref == value;
    }
    
    public class IsGoBack : WindowDataCondition { protected override bool Ref => WindowsData.IsGoBack; }
    public class IsHideAllPrevious : WindowDataCondition { protected override bool Ref => WindowsData.IsHideAllPrevious; }
    public class IsHidePrevious : WindowDataCondition { protected override bool Ref => WindowsData.IsHidePrevious; }
    [Serializable]
    public class WindowDataConditions : Conditions<WindowDataCondition> { }
}