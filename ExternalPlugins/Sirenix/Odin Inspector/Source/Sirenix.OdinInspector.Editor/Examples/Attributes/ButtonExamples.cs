//-----------------------------------------------------------------------
// <copyright file="ButtonExamples.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(ButtonAttribute))]
    internal class ButtonExamples
    {
        public string ButtonName = "Dynamic button name";

        public bool Toggle;

        [Button("$ButtonName")]
        private void DefaultSizedButton()
        {
            this.Toggle = !this.Toggle;
        }

        [Button("@\"Expression label: \" + DateTime.Now.ToString(\"HH:mm:ss\")")]
        public void ExpressionLabel()
        {
            this.Toggle = !this.Toggle;
        }

        [Button("Name of button")]
        private void NamedButton()
        {
            this.Toggle = !this.Toggle;
        }

        [Button(ButtonSizes.Small)]
        private void SmallButton()
        {
            this.Toggle = !this.Toggle;
        }

        [Button(ButtonSizes.Medium)]
        private void MediumSizedButton()
        {
            this.Toggle = !this.Toggle;
        }

        [DisableIf("Toggle")]
        [HorizontalGroup("Split", 0.5f)]
        [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
        private void FanzyButton1()
        {
            this.Toggle = !this.Toggle;
        }

        [HideIf("Toggle")]
        [VerticalGroup("Split/right")]
        [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
        private void FanzyButton2()
        {
            this.Toggle = !this.Toggle;
        }

        [ShowIf("Toggle")]
        [VerticalGroup("Split/right")]
        [Button(ButtonSizes.Large), GUIColor(1, 0.2f, 0)]
        private void FanzyButton3()
        {
            this.Toggle = !this.Toggle;
        }

        [Button(ButtonSizes.Gigantic)]
        private void GiganticButton()
        {
            this.Toggle = !this.Toggle;
        }

        [Button(90)]
        private void CustomSizedButton()
        {
            this.Toggle = !this.Toggle;
        }

        [Button(Icon = SdfIconType.Dice1Fill, IconAlignment = IconAlignment.LeftOfText)]
        private void IconButton01()
        {
            this.Toggle = !this.Toggle;
        }

        [Button(Icon = SdfIconType.Dice2Fill, IconAlignment = IconAlignment.LeftOfText)]
        private void IconButton02()
        {
            this.Toggle = !this.Toggle;
        }
    }
}
#endif