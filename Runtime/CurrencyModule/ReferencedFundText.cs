#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

namespace LSCore
{
    public class ReferencedFundText : FundText
    {
        public FundText reference;
        public float multiplier = 2;
        
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (reference == null) return;
            if (reference == this)
            {
                reference = null;
                return;
            }

            OnValidate();
            reference.Validated -= OnValidate;
            reference.Validated += OnValidate;

            void OnValidate()
            {
                Id = reference.Id;
                Number = reference.Number * multiplier;
            }
        }
#endif
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(ReferencedFundText), true), CanEditMultipleObjects]
    public class ReferencedFundTextEditor : FundTextEditor
    {
        private InspectorProperty reference;
        private InspectorProperty multiplier;

        protected override void OnEnable()
        {
            base.OnEnable();
            var children = propertyTree.RootProperty.Children;
            reference = children["reference"];
            multiplier = children["multiplier"];
        }

        protected override void DrawNumber()
        {
            reference.Draw();
            multiplier.Draw();
        }
    }
#endif
}