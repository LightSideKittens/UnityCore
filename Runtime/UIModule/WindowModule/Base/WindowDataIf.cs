using System;
using LSCore.Attributes;
using LSCore.ConditionModule;
using Sirenix.OdinInspector;

namespace LSCore
{
    [Serializable]
    [HideReferenceObjectPicker]
    [TypeFrom]
    public abstract class WindowDataIf : If
    {
        public bool value;
        protected abstract bool Ref { get; }
        protected override bool Check() => Ref == value;
    }
    
    public class IsGoBack : WindowDataIf { protected override bool Ref => UIViewBoss.IsGoBack; }
    public class IsHideAllPrevious : WindowDataIf { protected override bool Ref => UIViewBoss.IsHideAllPrevious; }
    public class IsHidePrevious : WindowDataIf { protected override bool Ref => UIViewBoss.IsHidePrevious; }
    [Serializable]
    public class WindowDataIfs : Ifs<WindowDataIf> { }
}