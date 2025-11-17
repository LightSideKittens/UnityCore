#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

namespace LSCore
{
    public class ReferencedFundText : FundText
    {
        public Get<FundText> reference;
        public float multiplier = 2;
        
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (reference == null) return;
            var value = reference.Data;
            
            if (value == this)
            {
                reference = null;
                return;
            }

            OnValidate();
            value.Validated -= OnValidate;
            value.Validated += OnValidate;

            void OnValidate()
            {
                Id = value.Id;
                Number = value.Number * multiplier;
            }
        }

        protected override void OnDestroy()
        {
            reference.Data.Validated -= OnValidate;
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