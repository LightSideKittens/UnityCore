using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace LSCore
{
    public class LSNumber : LSText
    {
        [SerializeField] private string textFormat = "{0}";
        [SerializeField] private string numberFormat;
        [SerializeField] private float number;
        
        [ShowInInspector]
        public string TextFormat
        {
            get => textFormat;
            set { textFormat = value; UpdateText(); }
        }
        
        [ShowInInspector]
        public string NumberFormat
        {
            get => numberFormat;
            set { numberFormat = value; UpdateText(); }
        }
        
        [ShowInInspector]
        public float Number
        {
            get => number;
            set { number = value; UpdateText(); }
        }

        private void UpdateText()
        {
            base.text = string.Format(textFormat, number.ToString(numberFormat));
        }
        
        public static implicit operator float(LSNumber number) => number.Number;

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
            tree = PropertyTree.Create(serializedObject);
            Number = tree.RootProperty.Children["Number"];
            TextFormat = tree.RootProperty.Children["TextFormat"];
            NumberFormat = tree.RootProperty.Children["NumberFormat"];
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            tree?.Dispose();
        }

        public override void OnInspectorGUI()
        {
            if (IsMixSelectionTypes()) return;

            serializedObject.Update();

            Draw();

            if (serializedObject.ApplyModifiedProperties() || m_HavePropertiesChanged)
            {
                m_TextComponent.havePropertiesChanged = true;
                m_HavePropertiesChanged = false;
                EditorUtility.SetDirty(target);
            }
        }

        protected virtual void Draw()
        {
            tree.BeginDraw(true);
            Number.Draw();
            TextFormat.Draw();
            NumberFormat.Draw();
            tree.EndDraw();

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