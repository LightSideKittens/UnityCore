using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace LSCore
{
    public class FundText : LSNumber
    {
        [field: SerializeField, Id(typeof(CurrencyIdGroup))] public Id Id { get; private set; }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(FundText), true), CanEditMultipleObjects]
    public class FundTextEditor : LSNumberEditor
    {
        private PropertyTree propertyTree;
        private InspectorProperty id;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            propertyTree = PropertyTree.Create(serializedObject);
            id = propertyTree.RootProperty.Children["<Id>k__BackingField"];
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            propertyTree.Dispose();
        }

        protected override void Draw()
        {
            id.Draw();
            id.Tree.UpdateTree();
            base.Draw();
        }
    }
#endif
}