//-----------------------------------------------------------------------
// <copyright file="BigTitleGroupExample.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(BoxGroupAttribute), Order = 10)]
    [AttributeExample(typeof(ButtonGroupAttribute), Order = 10)]
    [AttributeExample(typeof(TitleGroupAttribute), Order = 10)]
    internal class BigTitleGroupExample
    {
        [BoxGroup("Titles", ShowLabel = false)]
        [TitleGroup("Titles/First Title")]
        public int A;

        [BoxGroup("Titles/Boxed")]
        [TitleGroup("Titles/Boxed/Second Title")]
        public int B;

        [TitleGroup("Titles/Boxed/Second Title")]
        public int C;

        [TitleGroup("Titles/Horizontal Buttons")]
        [ButtonGroup("Titles/Horizontal Buttons/Buttons")]
        public void FirstButton() { }

        [ButtonGroup("Titles/Horizontal Buttons/Buttons")]
        public void SecondButton() { }
    }
}
#endif