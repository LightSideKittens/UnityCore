using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace LSCore
{
    public class LSNumber : LSText
    {
        [SerializeField] private string textFormat = "{0}";
        [SerializeField] private string numberFormat;
        [SerializeField] private float number;
        
        public string TextFormat
        {
            get => textFormat;
            set { textFormat = value; UpdateText(); }
        }
        
        public string NumberFormat
        {
            get => numberFormat;
            set { numberFormat = value; UpdateText(); }
        }
        
        public float Number
        {
            get => number;
            set { number = value; UpdateText(); OnNumberChanged(); }
        }

        private void UpdateText()
        {
            base.text = string.Format(textFormat, number.ToString(numberFormat));
        }

        protected virtual void OnNumberChanged(){}
        
        public static implicit operator float(LSNumber number) => number.Number;
        public static implicit operator int(LSNumber number) => (int)number.Number;
        
        public static LSNumber operator +(LSNumber a, float b)
        { 
            a.Number += b;
            return a;
        }
        
        public static LSNumber operator -(LSNumber a, float b)
        { 
            a.Number -= b;
            return a;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            UpdateText();
        }
#endif

        public override string text
        {
            get => base.text;
            set { }
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(LSNumber), true), CanEditMultipleObjects]
    public class LSNumberEditor : LSTextEditor
    {
        PropertyTree tree;
        InspectorProperty Number;
        InspectorProperty TextFormat;
        InspectorProperty NumberFormat;

        protected override void OnEnable()
        {
            base.OnEnable();
            Number = propertyTree.RootProperty.Children["number"];
            TextFormat = propertyTree.RootProperty.Children["textFormat"];
            NumberFormat = propertyTree.RootProperty.Children["numberFormat"];
        }

        public override void OnInspectorGUI()
        {
            if (IsMixSelectionTypes()) return;

            serializedObject.Update();

            propertyTree.BeginDraw(true);
            Draw();
            propertyTree.EndDraw();
            
            if (serializedObject.ApplyModifiedProperties() || m_HavePropertiesChanged)
            {
                m_TextComponent.havePropertiesChanged = true;
                m_HavePropertiesChanged = false;
                EditorUtility.SetDirty(target);
            }
        }

        protected virtual void DrawNumber() => Number.Draw();

        protected virtual void Draw()
        {
            DrawNumber();
            TextFormat.Draw();
            NumberFormat.Draw();

            DrawMainSettings();

            DrawExtraSettings();

            EditorGUILayout.Space();
        }


        [MenuItem("GameObject/LSCore/Number")]
        private static void CreateButton()
        {
            new GameObject(nameof(LSNumber)).AddComponent<LSNumber>();
        }
    }
#endif
}